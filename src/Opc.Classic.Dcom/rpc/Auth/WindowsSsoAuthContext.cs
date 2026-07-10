// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers;
using System.Net;
using System.Net.Security;
using System.Runtime.Versioning;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Authentication context that uses <see cref="NegotiateAuthentication"/> with
/// the current Windows logon's <see cref="CredentialCache.DefaultNetworkCredentials"/>
/// to perform SPNEGO (NTLM or Kerberos) authentication transparently.
/// </summary>
/// <remarks>
/// Windows-only. The constructor throws <see cref="PlatformNotSupportedException"/>
/// on non-Windows operating systems because <see cref="CredentialCache.DefaultNetworkCredentials"/>
/// has no usable value outside Windows for SSPI/Negotiate.
/// <para>
/// The DCE/RPC verifier auth_type is reported as <c>0x09</c> (SPNEGO) per
/// MS-RPCE §2.2.1.1.7 — this is what Windows DCOM uses for Negotiate-based
/// binds and is the value the SCM expects from .NET's NegotiateAuthentication
/// when the underlying package is "Negotiate".
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class WindowsSsoAuthContext : IAuthContext, IDisposable
{
    private const byte AuthenticationServiceNtlm = 0x0A;

    private readonly NegotiateAuthentication _negotiate;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="connectData">The OPC connection data with auth mode <see cref="OpcAuthMode.WindowsSso"/>.</param>
    public WindowsSsoAuthContext(OpcConnectData connectData)
    {
        ArgumentNullException.ThrowIfNull(connectData);
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "OpcAuthMode.WindowsSso is Windows-only because it relies on SSPI/Negotiate via " +
                "System.Net.Security.NegotiateAuthentication and the current Windows logon.");
        }

        ProtectionLevel = connectData.ProtectionLevel;

        string host = string.IsNullOrWhiteSpace(connectData.Url.Host) ? "localhost" : connectData.Url.Host;
        string targetName = "RPCSS/" + host;

        // Use raw NTLM (SSPI package "NTLM") rather than SPNEGO ("Negotiate").
        // SPNEGO requires a 4-leg handshake (NegTokenInit -> NegTokenResp ->
        // NegTokenResp -> final ack) but the DCE/RPC connection-oriented
        // protocol allocates only 3 message slots: bind, bind_ack, auth3.
        // Raw NTLMSSP fits the 3-leg model: NEGOTIATE in bind, CHALLENGE in
        // bind_ack, AUTHENTICATE in auth3. The DCE/RPC verifier auth_type is
        // set to 0x0A (RPC_C_AUTHN_WINNT) per MS-RPCE §2.2.1.1.7.
        var options = new NegotiateAuthenticationClientOptions
        {
            Package = "NTLM",
            Credential = CredentialCache.DefaultNetworkCredentials,
            TargetName = targetName,
            AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Impersonation,
            RequiredProtectionLevel = ToFrameworkProtectionLevel(connectData.ProtectionLevel),
            RequireMutualAuthentication = false,
        };

        _negotiate = new NegotiateAuthentication(options);
    }

    /// <summary>
    /// The DCE/RPC packet-protection level negotiated for this context.
    /// </summary>
    public OpcProtectionLevel ProtectionLevel { get; }

    /// <summary>
    /// NTLM auth-service code per MS-RPCE §2.2.1.1.7.
    /// </summary>
    public byte AuthenticationServiceCode => AuthenticationServiceNtlm;

    /// <inheritdoc />
    public byte[] BuildInitialToken()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        byte[] token = _negotiate.GetOutgoingBlob(ReadOnlySpan<byte>.Empty, out NegotiateAuthenticationStatusCode status);
        if (status != NegotiateAuthenticationStatusCode.Completed && status != NegotiateAuthenticationStatusCode.ContinueNeeded)
        {
            throw new InvalidOperationException(
                "NegotiateAuthentication failed to produce the initial token: " + status);
        }
        return token ?? Array.Empty<byte>();
    }

    /// <inheritdoc />
    public byte[] ProcessChallengeToken(ReadOnlyMemory<byte> serverToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (serverToken.IsEmpty)
        {
            return Array.Empty<byte>();
        }

        byte[] outgoing = _negotiate.GetOutgoingBlob(serverToken.Span, out NegotiateAuthenticationStatusCode status);
        if (status == NegotiateAuthenticationStatusCode.Completed
            || status == NegotiateAuthenticationStatusCode.ContinueNeeded)
        {
            return outgoing ?? Array.Empty<byte>();
        }
        throw new InvalidOperationException(
            "NegotiateAuthentication failed to process the server challenge: " + status);
    }

    /// <inheritdoc />
    public void SignAndSeal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, out byte[] signature)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (ProtectionLevel < OpcProtectionLevel.Integrity)
        {
            // At PROTECTION_LEVEL_NONE/CONNECT/CALL/PACKET the DCE/RPC layer does
            // NOT attach a per-PDU signature; the bind handshake is enough to satisfy
            // server-side AuthenticationLevel checks. Return an empty signature so
            // DcomCallChannel.ApplyPacketProtection skips the verifier slot.
            signature = Array.Empty<byte>();
            return;
        }

        if (ProtectionLevel == OpcProtectionLevel.Integrity)
        {
            // NTLMSSP per-message signature (16 bytes for NTLMv2 with extended session
            // security), computed over the ENTIRE signed region: the common header, body,
            // auth padding, and the sec_trailer header, excluding the auth_value itself
            // (MS-RPCE §3.3.1.5.2.2). SSPI MakeSignature tracks the outgoing sequence number
            // internally, starting at 0 for the first protected PDU on the connection.
            var sigWriter = new ArrayBufferWriter<byte>(16);
            _negotiate.ComputeIntegrityCheck(signedRegion, sigWriter);
            signature = sigWriter.WrittenSpan.ToArray();
            return;
        }

        // Privacy: seal only the confidential stub sub-range in place; the 16-byte signature
        // follows the sealed body per the NTLMSSP wire format (MS-NLMP §2.2.2.9).
        Span<byte> confidential = signedRegion.Slice(confidentialOffset, confidentialLength);
        var writer = new ArrayBufferWriter<byte>(confidential.Length + 32);
        NegotiateAuthenticationStatusCode wrapStatus = _negotiate.Wrap(
            confidential,
            writer,
            requestEncryption: true,
            out _);
        if (wrapStatus != NegotiateAuthenticationStatusCode.Completed)
        {
            throw new InvalidOperationException(
                "NegotiateAuthentication.Wrap failed with status: " + wrapStatus);
        }
        ReadOnlySpan<byte> wrapped = writer.WrittenSpan;
        if (wrapped.Length < confidential.Length + 16)
        {
            throw new InvalidOperationException(
                "NegotiateAuthentication.Wrap returned fewer bytes than expected for NTLMSSP Privacy.");
        }
        wrapped.Slice(0, confidential.Length).CopyTo(confidential);
        signature = wrapped.Slice(confidential.Length).ToArray();
    }

    /// <inheritdoc />
    public bool VerifyAndUnseal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, ReadOnlyMemory<byte> signature)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (ProtectionLevel < OpcProtectionLevel.Integrity)
        {
            return true;
        }

        if (ProtectionLevel == OpcProtectionLevel.Integrity)
        {
            // Verify the peer's NTLMSSP signature over the full signed region (MS-RPCE
            // §3.3.1.5.2.2). SSPI VerifySignature tracks the incoming sequence number
            // internally, starting at 0 for the first protected response on the connection.
            return _negotiate.VerifyIntegrityCheck(signedRegion, signature.Span);
        }

        // Privacy: reassemble sealed_body || signature and unseal in place.
        Span<byte> confidential = signedRegion.Slice(confidentialOffset, confidentialLength);
        byte[] sealedBytes = new byte[confidential.Length + signature.Length];
        confidential.CopyTo(sealedBytes);
        signature.Span.CopyTo(sealedBytes.AsSpan(confidential.Length));
        var writer = new ArrayBufferWriter<byte>(confidential.Length);
        NegotiateAuthenticationStatusCode status = _negotiate.Unwrap(sealedBytes, writer, out _);
        if (status != NegotiateAuthenticationStatusCode.Completed)
        {
            return false;
        }
        writer.WrittenSpan.CopyTo(confidential);
        return true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _negotiate.Dispose();
    }

    private static System.Net.Security.ProtectionLevel ToFrameworkProtectionLevel(OpcProtectionLevel level) => level switch
    {
        OpcProtectionLevel.None => System.Net.Security.ProtectionLevel.None,
        OpcProtectionLevel.Connect => System.Net.Security.ProtectionLevel.None,
        OpcProtectionLevel.Call => System.Net.Security.ProtectionLevel.None,
        OpcProtectionLevel.Packet => System.Net.Security.ProtectionLevel.None,
        OpcProtectionLevel.Integrity => System.Net.Security.ProtectionLevel.Sign,
        OpcProtectionLevel.Privacy => System.Net.Security.ProtectionLevel.EncryptAndSign,
        _ => System.Net.Security.ProtectionLevel.None,
    };
}
