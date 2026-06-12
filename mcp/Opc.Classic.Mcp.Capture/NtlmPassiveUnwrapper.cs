//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Security.Cryptography;
using System.Text;
using Opc.Classic.Dcom.Crypto;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Direction of a captured PDU relative to the established DCOM
/// connection. Used by <see cref="NtlmPassiveUnwrapper.TryUnwrap"/>
/// to pick the correct NTLM sub-key + sequence counter.
/// </summary>
public enum NtlmDirection
{
    /// <summary>Frame sent by the DCOM client to the server (request side).</summary>
    ClientToServer,

    /// <summary>Frame sent by the DCOM server to the client (response / notification side).</summary>
    ServerToClient,
}

/// <summary>
/// Outcome of a single <see cref="NtlmPassiveUnwrapper.TryUnwrap"/>
/// call. <see cref="NtlmUnwrapStatus.Decrypted"/> and
/// <see cref="NtlmUnwrapStatus.IntegrityVerified"/> both indicate a
/// healthy unwrap (the stub buffer now contains plaintext when
/// privacy was enabled); other statuses indicate a failure mode the
/// caller should surface to the operator.
/// </summary>
public enum NtlmUnwrapStatus
{
    /// <summary>Body decrypted + signature verified (privacy mode).</summary>
    Decrypted,

    /// <summary>Body left as-is (already plaintext) + signature verified (integrity-only mode).</summary>
    IntegrityVerified,

    /// <summary>
    /// Signature did not match the expected verifier. Likely causes,
    /// in order of probability: wrong session key supplied; capture
    /// started after the Type3 handshake so per-direction counters
    /// drifted; legitimate corruption / replay attack on the wire.
    /// </summary>
    SignatureMismatch,

    /// <summary>The auth trailer length did not match the NTLM verifier length (16 bytes); not an NTLMSSP-signed PDU.</summary>
    InvalidTrailerLength,

    /// <summary>The unwrapper was constructed with the disabled-/no-key sentinel and silently passes through.</summary>
    Disabled,
}

/// <summary>
/// Snapshot result returned by <see cref="NtlmPassiveUnwrapper.TryUnwrap"/>.
/// </summary>
/// <param name="Status">Discriminator for the outcome.</param>
/// <param name="Reason">
/// Optional human-readable reason populated on failure statuses; null
/// for successful unwrap. Operator-friendly; safe to surface in MCP
/// tool output / capture summaries.
/// </param>
public readonly record struct NtlmUnwrapResult(NtlmUnwrapStatus Status, string? Reason)
{
    /// <summary>Convenience: true when the stub buffer contains plaintext after the call.</summary>
    public bool Succeeded => Status is NtlmUnwrapStatus.Decrypted or NtlmUnwrapStatus.IntegrityVerified;
}

/// <summary>
/// Developer-only passive NTLMSSP auth-trailer unwrapper for offline
/// pcap analysis of captured DCOM traffic. Given the 16-byte session
/// key established by a captured NTLM Type3 handshake, decrypts +
/// verifies sign-and-seal-protected stub bodies on Request /
/// Response PDUs.
/// </summary>
/// <remarks>
/// <para><strong>Security model.</strong> This class is intended ONLY
/// for the developer's own test traffic OR for traffic the developer
/// is explicitly authorised to inspect. The NTLM session key is
/// equivalent to the wire-level secrets that protect the
/// authenticated connection — leaking it elsewhere is equivalent to
/// leaking those secrets. The class zeroes its derived sub-keys on
/// dispose; callers must zero the input session key themselves and
/// MUST NOT log / persist the key.</para>
///
/// <para><strong>Algorithm.</strong> Implements the NTLMSSP wire-level
/// sign-and-seal scheme per MS-NLMP §3.4 (Extended Session Security +
/// Key Exchange) directly on top of BCL <see cref="HMACMD5"/> and the
/// public <see cref="RC4Engine"/>. Independently maintains two RC4
/// stream-cipher instances (one per direction) and two independent
/// sequence counters because passive sniffing sees BOTH halves of a
/// single bidirectional connection (vs. the live peer's view where
/// only one direction is "outgoing" off its own counter pair).
/// Cross-validated against the production
/// <see cref="Opc.Classic.Dcom.Rpc.Auth.ntlm.Ntlm1"/> sign-and-seal
/// pipeline in <c>NtlmPassiveUnwrapperTests</c>.</para>
///
/// <para><strong>Counter recovery.</strong> The NTLM sign/seal
/// counters start at 0 immediately after the Type3 handshake. If the
/// pcap capture started AFTER the handshake, both directions' counters
/// are unrecoverable from passive observation alone and every unwrap
/// will fail with <see cref="NtlmUnwrapStatus.SignatureMismatch"/>.
/// This is a documented limitation; the operator should always start
/// the capture before the DCOM connection bind.</para>
///
/// <para><strong>Flag defaults.</strong> Constructed flags default
/// to the standard NTLMv2 wire-level mode used by all modern Windows
/// peers (<c>NtlmsspNegotiateUnicode | NtlmsspNegotiateExtendedSessionSecurity |
/// NtlmsspNegotiateSign | NtlmsspNegotiateAlwaysSign | NtlmsspNegotiateSeal |
/// NtlmsspNegotiateKeyExch | NtlmsspNegotiate128</c>). Override only
/// when working with non-default DCOM peers (e.g. lab traffic with
/// downgraded negotiation).</para>
/// </remarks>
public sealed class NtlmPassiveUnwrapper : IDisposable
{
    /// <summary>
    /// Standard NTLMv2 wire-level flag set used by every modern
    /// Windows DCOM peer. Reuse with the parameterless / single-arg
    /// constructor unless you know your traffic negotiates a
    /// different subset.
    /// </summary>
    public const NtlmFlags DefaultFlags =
        NtlmFlags.NtlmsspNegotiateUnicode |
        NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity |
        NtlmFlags.NtlmsspNegotiateSign |
        NtlmFlags.NtlmsspNegotiateAlwaysSign |
        NtlmFlags.NtlmsspNegotiateSeal |
        NtlmFlags.NtlmsspNegotiateKeyExch |
        NtlmFlags.NtlmsspNegotiate128;

    /// <summary>NTLM verifier length (auth trailer length on RPC PDUs).</summary>
    public const int VerifierLength = 16;

    // MS-NLMP §3.4.5.3 magic constants used to derive 4 sub-keys from
    // the exported NTLMv2 session key via HMAC-MD5. Stored as ASCII
    // bytes including the terminating NUL byte that the spec mandates.
    private static readonly byte[] s_clientSigningMagic =
        Encoding.ASCII.GetBytes("session key to client-to-server signing key magic constant\0");
    private static readonly byte[] s_serverSigningMagic =
        Encoding.ASCII.GetBytes("session key to server-to-client signing key magic constant\0");
    private static readonly byte[] s_clientSealingMagic =
        Encoding.ASCII.GetBytes("session key to client-to-server sealing key magic constant\0");
    private static readonly byte[] s_serverSealingMagic =
        Encoding.ASCII.GetBytes("session key to server-to-client sealing key magic constant\0");

    private readonly NtlmFlags _flags;
    private readonly ProtectionLevel _protection;
    private readonly bool _encryptMessageSignature;

    private byte[] _clientSigningKey;
    private byte[] _serverSigningKey;
    private readonly RC4Engine _clientCipher;
    private readonly RC4Engine _serverCipher;

    private int _clientSequence;
    private int _serverSequence;
    private bool _disposed;

    /// <summary>
    /// Creates a passive unwrapper bound to the given 16-byte NTLMv2
    /// session key plus the negotiated flag set and protection level.
    /// </summary>
    /// <param name="sessionKey">
    /// The 16-byte exported NTLMv2 session key established by the
    /// captured Type3 handshake. Caller owns + zeroes the buffer.
    /// </param>
    /// <param name="flags">
    /// NTLMSSP-negotiated flags. Default = <see cref="DefaultFlags"/>
    /// (standard modern Windows NTLMv2 wire mode).
    /// </param>
    /// <param name="protection">
    /// RPC protection level. <see cref="ProtectionLevel.PROTECTION_LEVEL_PRIVACY"/>
    /// (default) decrypts the stub body via RC4 + verifies the
    /// signature; <see cref="ProtectionLevel.PROTECTION_LEVEL_INTEGRITY"/>
    /// only verifies the signature without touching the body.
    /// Anything below integrity (<c>CONNECT</c>, <c>CALL</c>,
    /// <c>PACKET</c>, <c>NONE</c>) is a contract violation — those
    /// modes do not produce an auth trailer at all and this class
    /// will not be invoked.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="sessionKey"/> is not exactly 16
    /// bytes or <paramref name="protection"/> is below INTEGRITY.
    /// </exception>
    public NtlmPassiveUnwrapper(
        ReadOnlySpan<byte> sessionKey,
        NtlmFlags flags = DefaultFlags,
        ProtectionLevel protection = ProtectionLevel.PROTECTION_LEVEL_PRIVACY)
    {
        if (sessionKey.Length != VerifierLength)
        {
            throw new ArgumentException(
                $"NTLMv2 session key must be exactly {VerifierLength} bytes; got {sessionKey.Length}.",
                nameof(sessionKey));
        }

        if (protection < ProtectionLevel.PROTECTION_LEVEL_INTEGRITY)
        {
            throw new ArgumentException(
                $"NtlmPassiveUnwrapper requires INTEGRITY or PRIVACY; got {protection}. Unsigned/unsealed PDUs have no auth trailer.",
                nameof(protection));
        }

        _flags = flags;
        _protection = protection;
        _encryptMessageSignature =
            (flags & (NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity | NtlmFlags.NtlmsspNegotiateKeyExch))
            == (NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity | NtlmFlags.NtlmsspNegotiateKeyExch);

        byte[] sk = sessionKey.ToArray();
        byte[]? clientSealingKey = null;
        byte[]? serverSealingKey = null;
        try
        {
            _clientSigningKey = DeriveExtendedSessionKey(sk, s_clientSigningMagic);
            _serverSigningKey = DeriveExtendedSessionKey(sk, s_serverSigningMagic);
            clientSealingKey = DeriveExtendedSessionKey(sk, s_clientSealingMagic);
            serverSealingKey = DeriveExtendedSessionKey(sk, s_serverSealingMagic);
            _clientCipher = CreateRc4(clientSealingKey);
            _serverCipher = CreateRc4(serverSealingKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(sk);
            CryptographicOperations.ZeroMemory(clientSealingKey);
            CryptographicOperations.ZeroMemory(serverSealingKey);
        }
    }

    /// <summary>
    /// Bypass-mode constructor: returns a disabled unwrapper that
    /// always returns <see cref="NtlmUnwrapStatus.Disabled"/> with
    /// the stub buffer untouched. Convenient for the
    /// <c>--ntlmSessionKeyHex unset</c> code path so the decoder
    /// integration can carry a non-nullable unwrapper without a
    /// special-case branch.
    /// </summary>
    private NtlmPassiveUnwrapper()
    {
        _flags = NtlmFlags.None;
        _protection = ProtectionLevel.PROTECTION_LEVEL_NONE;
        _clientSigningKey = Array.Empty<byte>();
        _serverSigningKey = Array.Empty<byte>();
        _clientCipher = CreateRc4(new byte[16]);
        _serverCipher = CreateRc4(new byte[16]);
        IsDisabled = true;
    }

    /// <summary>Singleton no-op unwrapper used when no session key is configured.</summary>
    public static NtlmPassiveUnwrapper Disabled { get; } = new();

    /// <summary>True for the disabled sentinel; false for a real keyed unwrapper.</summary>
    public bool IsDisabled { get; }

    /// <summary>Current direction-specific sequence counter (test-visible).</summary>
    public int ClientSequence => _clientSequence;

    /// <summary>Current direction-specific sequence counter (test-visible).</summary>
    public int ServerSequence => _serverSequence;

    /// <summary>
    /// Decrypts (when protection level is privacy) + verifies a
    /// single captured PDU's stub body against its auth trailer,
    /// advancing the per-direction sequence counter on success. The
    /// <paramref name="stubBuffer"/> contents are mutated in-place
    /// from ciphertext to plaintext when the protection level is
    /// privacy AND the call returns
    /// <see cref="NtlmUnwrapStatus.Decrypted"/>; left as-is otherwise.
    /// </summary>
    /// <param name="dir">Captured PDU direction (use port-pair heuristic on the caller's flow).</param>
    /// <param name="stubBuffer">
    /// The encrypted (or signed-only) stub body. On
    /// <see cref="NtlmUnwrapStatus.Decrypted"/> the span is overwritten
    /// with the decrypted bytes; do NOT supply a view into immutable
    /// captured-packet bytes — copy them first if you need both.
    /// </param>
    /// <param name="authTrailer">
    /// The 16-byte verifier that follows the stub on the wire.
    /// Returns <see cref="NtlmUnwrapStatus.InvalidTrailerLength"/>
    /// for any other length.
    /// </param>
    public NtlmUnwrapResult TryUnwrap(
        NtlmDirection dir,
        Span<byte> stubBuffer,
        ReadOnlySpan<byte> authTrailer)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (IsDisabled)
        {
            return new NtlmUnwrapResult(NtlmUnwrapStatus.Disabled, "No NTLM session key configured.");
        }

        if (authTrailer.Length != VerifierLength)
        {
            return new NtlmUnwrapResult(
                NtlmUnwrapStatus.InvalidTrailerLength,
                $"Auth trailer length {authTrailer.Length} != expected {VerifierLength}.");
        }

        bool isClient2Server = dir == NtlmDirection.ClientToServer;
        byte[] signingKey = isClient2Server ? _clientSigningKey : _serverSigningKey;
        RC4Engine cipher = isClient2Server ? _clientCipher : _serverCipher;
        int sequenceNumber = isClient2Server ? _clientSequence : _serverSequence;

        // PRIVACY: decrypt body FIRST (so HMAC is computed over plaintext).
        // INTEGRITY: leave body alone.
        if (_protection == ProtectionLevel.PROTECTION_LEVEL_PRIVACY && stubBuffer.Length > 0)
        {
            byte[] cipherBuf = stubBuffer.ToArray();
            byte[] plaintext = new byte[cipherBuf.Length];
            cipher.ProcessBytes(cipherBuf, 0, cipherBuf.Length, plaintext, 0);
            plaintext.AsSpan().CopyTo(stubBuffer);
        }

        // HMAC over plaintext + sequence number → 16-byte verifier
        // (with the 8 middle bytes XOR-encrypted via SigningPt2 when
        // ExtendedSessionSecurity + KeyExch is negotiated).
        byte[] expected = ComputeVerifier(sequenceNumber, signingKey, stubBuffer, cipher);

        if (!authTrailer.SequenceEqual(expected))
        {
            // Signature mismatch — DON'T advance the counter. The caller
            // can decide to retry, give up, or surface to the operator
            // ("likely wrong session key; or capture started after Type3").
            return new NtlmUnwrapResult(
                NtlmUnwrapStatus.SignatureMismatch,
                $"Signature mismatch on {dir} at counter={sequenceNumber}. " +
                "Verify the supplied session key matches the captured Type3 handshake AND that the capture starts BEFORE the bind/handshake.");
        }

        // Successful unwrap — advance the counter for the next PDU on this direction.
        if (isClient2Server)
        {
            _clientSequence++;
        }
        else
        {
            _serverSequence++;
        }

        return new NtlmUnwrapResult(
            _protection == ProtectionLevel.PROTECTION_LEVEL_PRIVACY
                ? NtlmUnwrapStatus.Decrypted
                : NtlmUnwrapStatus.IntegrityVerified,
            null);
    }

    /// <summary>
    /// Zeroes the derived sub-keys and resets the RC4 streams. The
    /// disabled sentinel is a no-op so the shared singleton stays
    /// usable across the entire process lifetime.
    /// </summary>
    public void Dispose()
    {
        if (_disposed || IsDisabled)
        {
            return;
        }
        _disposed = true;

        CryptographicOperations.ZeroMemory(_clientSigningKey);
        CryptographicOperations.ZeroMemory(_serverSigningKey);
        _clientSigningKey = Array.Empty<byte>();
        _serverSigningKey = Array.Empty<byte>();
        try { _clientCipher.Reset(); } catch (NotSupportedException) { /* tolerate cipher impls that don't support Reset */ }
        try { _serverCipher.Reset(); } catch (NotSupportedException) { /* tolerate */ }
    }

    /// <summary>
    /// MS-NLMP §3.4.5.3 extended-session-security sub-key derivation:
    /// <c>MD5(sessionKey || magicConstant)</c>. Matches the production
    /// <c>NTLMKeyFactory.GenerateExtendedSessionSecurityKey</c> which
    /// uses plain MD5 (NOT HMAC-MD5) for the 4 sign/seal sub-keys
    /// when ExtendedSessionSecurity is negotiated.
    /// </summary>
    private static byte[] DeriveExtendedSessionKey(byte[] sessionKey, byte[] magicConstant)
    {
        byte[] dataforhash = new byte[sessionKey.Length + magicConstant.Length];
        try
        {
            sessionKey.CopyTo(dataforhash, 0);
            magicConstant.CopyTo(dataforhash, sessionKey.Length);
#pragma warning disable CA5351 // NTLM requires MD5 per [MS-NLMP] §3.4.5.3.
            return MD5.HashData(dataforhash);
#pragma warning restore CA5351
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataforhash);
        }
    }

    private static RC4Engine CreateRc4(byte[] key)
    {
        var engine = new RC4Engine();
        engine.Init(forEncryption: true, new KeyParameter(key));
        return engine;
    }

    /// <summary>
    /// Composes the 16-byte NTLMv2 verifier:
    /// <c>0x01 0x00 0x00 0x00 || HMAC-MD5(signingKey, seqNum_le || plaintext)[0..7]
    ///    || seqNum_le</c>, with the 8 middle HMAC bytes XOR-encrypted
    /// via the RC4 stream when <see cref="_encryptMessageSignature"/>
    /// is true (always true for Extended Session Security + KeyExch,
    /// which is the standard NTLMv2 wire mode).
    /// </summary>
    private byte[] ComputeVerifier(
        int sequenceNumber,
        byte[] signingKey,
        ReadOnlySpan<byte> plaintext,
        RC4Engine cipher)
    {
        // SigningPt1: HMAC(seqNum_le || plaintext)
        byte[] seqNumPlusData = new byte[4 + plaintext.Length];
        BinaryPrimitives_WriteInt32LE(seqNumPlusData.AsSpan(0, 4), sequenceNumber);
        plaintext.CopyTo(seqNumPlusData.AsSpan(4));

        byte[] hmac;
        try
        {
            using var h = new HMACMD5(signingKey);
            hmac = h.ComputeHash(seqNumPlusData);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(seqNumPlusData);
        }

        byte[] verifier = new byte[VerifierLength];
        verifier[0] = 0x01; // NTLMSSP signature version (little-endian 1).
        // verifier[1..3] stay 0.
        Array.Copy(hmac, 0, verifier, 4, 8);
        BinaryPrimitives_WriteInt32LE(verifier.AsSpan(12, 4), sequenceNumber);

        // SigningPt2: when ExtendedSessionSecurity + KeyExch, XOR-encrypt
        // the 8 HMAC bytes (verifier[4..11]) with the RC4 stream.
        if (_encryptMessageSignature)
        {
            for (int i = 0; i < 8; i++)
            {
                verifier[i + 4] = cipher.ReturnByte(verifier[i + 4]);
            }
        }

        return verifier;
    }

    private static void BinaryPrimitives_WriteInt32LE(Span<byte> dest, int value)
    {
        dest[0] = unchecked((byte)(value & 0xFF));
        dest[1] = unchecked((byte)((value >> 8) & 0xFF));
        dest[2] = unchecked((byte)((value >> 16) & 0xFF));
        dest[3] = unchecked((byte)((value >> 24) & 0xFF));
    }
}
