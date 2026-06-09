//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

/// <summary>
/// Round-trip coverage for <see cref="NtlmPassiveUnwrapper"/> proving
/// that the passive sniffer path agrees with the live
/// <see cref="Ntlm1.ProcessOutgoing"/> sign-and-seal path on:
///   - 4 sub-key derivation,
///   - HMAC-MD5 + RC4 + SigningPt2 verifier composition,
///   - per-direction sequence counter increments.
/// </summary>
public sealed class NtlmPassiveUnwrapperTests
{
    private const NtlmFlags Flags = NtlmPassiveUnwrapper.DefaultFlags;
    // 16-byte deterministic test session key (NOT random) so failing
    // tests print stable diagnostics. Not a real Windows key.
    private static readonly byte[] s_testSessionKey = new byte[]
    {
        0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF,
        0xFE, 0xDC, 0xBA, 0x98, 0x76, 0x54, 0x32, 0x10,
    };

    [Test]
    public async Task Ctor_RejectsWrongSizeSessionKey()
    {
        await Assert.That(() => new NtlmPassiveUnwrapper(new byte[15])).Throws<ArgumentException>();
        await Assert.That(() => new NtlmPassiveUnwrapper(new byte[17])).Throws<ArgumentException>();
        await Assert.That(() => new NtlmPassiveUnwrapper(Array.Empty<byte>())).Throws<ArgumentException>();
    }

    [Test]
    public async Task Ctor_RejectsProtectionLevelBelowIntegrity()
    {
        await Assert.That(() =>
            new NtlmPassiveUnwrapper(s_testSessionKey, Flags, ProtectionLevel.PROTECTION_LEVEL_NONE))
            .Throws<ArgumentException>();
        await Assert.That(() =>
            new NtlmPassiveUnwrapper(s_testSessionKey, Flags, ProtectionLevel.PROTECTION_LEVEL_CONNECT))
            .Throws<ArgumentException>();
        await Assert.That(() =>
            new NtlmPassiveUnwrapper(s_testSessionKey, Flags, ProtectionLevel.PROTECTION_LEVEL_PACKET))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Disabled_AlwaysReturnsDisabledStatus_AndLeavesBufferUntouched()
    {
        NtlmPassiveUnwrapper unwrapper = NtlmPassiveUnwrapper.Disabled;
        byte[] stub = [1, 2, 3, 4, 5];
        byte[] copy = (byte[])stub.Clone();
        byte[] trailer = new byte[NtlmPassiveUnwrapper.VerifierLength];

        NtlmUnwrapResult result = unwrapper.TryUnwrap(NtlmDirection.ClientToServer, stub, trailer);

        await Assert.That(result.Status).IsEqualTo(NtlmUnwrapStatus.Disabled);
        await Assert.That(result.Succeeded).IsFalse();
        await Assert.That(stub).IsEquivalentTo(copy);
        await Assert.That(unwrapper.IsDisabled).IsTrue();
    }

    [Test]
    public async Task TryUnwrap_AuthTrailerWrongLength_ReturnsInvalidTrailerLength()
    {
        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey, Flags);
        byte[] stub = new byte[32];
        byte[] tooShort = new byte[15];
        byte[] tooLong = new byte[17];

        NtlmUnwrapResult r1 = unwrapper.TryUnwrap(NtlmDirection.ClientToServer, stub, tooShort);
        NtlmUnwrapResult r2 = unwrapper.TryUnwrap(NtlmDirection.ServerToClient, stub, tooLong);

        await Assert.That(r1.Status).IsEqualTo(NtlmUnwrapStatus.InvalidTrailerLength);
        await Assert.That(r2.Status).IsEqualTo(NtlmUnwrapStatus.InvalidTrailerLength);
    }

    [Test]
    public async Task RoundTrip_ClientToServer_PrivacyMode_DecryptsAndVerifies()
    {
        // Produce a sealed-and-signed PDU body using the production NTLMv2-mode
        // signing path (Ntlm1 with ExtendedSessionSecurity + KeyExch + Seal).
        (byte[] cipherStub, byte[] trailer) = SealOutgoing(
            sessionKey: s_testSessionKey,
            plaintext: BuildPlaintext(64),
            isServer: false);

        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey, Flags);
        NtlmUnwrapResult result = unwrapper.TryUnwrap(NtlmDirection.ClientToServer, cipherStub, trailer);

        await Assert.That(result.Status).IsEqualTo(NtlmUnwrapStatus.Decrypted);
        await Assert.That(result.Succeeded).IsTrue();
        await Assert.That(cipherStub).IsEquivalentTo(BuildPlaintext(64));
        await Assert.That(unwrapper.ClientSequence).IsEqualTo(1);
        await Assert.That(unwrapper.ServerSequence).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_ServerToClient_PrivacyMode_DecryptsAndVerifies()
    {
        (byte[] cipherStub, byte[] trailer) = SealOutgoing(
            sessionKey: s_testSessionKey,
            plaintext: BuildPlaintext(48),
            isServer: true);

        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey, Flags);
        NtlmUnwrapResult result = unwrapper.TryUnwrap(NtlmDirection.ServerToClient, cipherStub, trailer);

        await Assert.That(result.Status).IsEqualTo(NtlmUnwrapStatus.Decrypted);
        await Assert.That(cipherStub).IsEquivalentTo(BuildPlaintext(48));
        await Assert.That(unwrapper.ClientSequence).IsEqualTo(0);
        await Assert.That(unwrapper.ServerSequence).IsEqualTo(1);
    }

    [Test]
    public async Task RoundTrip_TwoPdusInSequence_BothDecryptAndCounterAdvancesPerDirection()
    {
        byte[] plain1 = BuildPlaintext(40);
        byte[] plain2 = BuildPlaintext(72);
        // Use a single Ntlm1 producer so its internal counter advances 0 -> 1 -> 2.
#pragma warning disable CS0618 // Ntlm1 [Obsolete] is intentional in this passive-unwrap test
        var producer = new Ntlm1(Flags, (byte[])s_testSessionKey.Clone(), isServer: false);
#pragma warning restore CS0618
        (byte[] cipher1, byte[] trailer1) = SealWithProducer(producer, plain1);
        (byte[] cipher2, byte[] trailer2) = SealWithProducer(producer, plain2);

        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey, Flags);
        NtlmUnwrapResult r1 = unwrapper.TryUnwrap(NtlmDirection.ClientToServer, cipher1, trailer1);
        NtlmUnwrapResult r2 = unwrapper.TryUnwrap(NtlmDirection.ClientToServer, cipher2, trailer2);

        await Assert.That(r1.Succeeded).IsTrue();
        await Assert.That(r2.Succeeded).IsTrue();
        await Assert.That(cipher1).IsEquivalentTo(plain1);
        await Assert.That(cipher2).IsEquivalentTo(plain2);
        await Assert.That(unwrapper.ClientSequence).IsEqualTo(2);
    }

    [Test]
    public async Task TryUnwrap_WrongSessionKey_ReturnsSignatureMismatch_DoesNotAdvanceCounter()
    {
        (byte[] cipherStub, byte[] trailer) = SealOutgoing(
            sessionKey: s_testSessionKey,
            plaintext: BuildPlaintext(32),
            isServer: false);

        var wrongKey = new byte[16];
        for (int i = 0; i < 16; i++)
        {
            wrongKey[i] = 0xAA;
        }

        using var unwrapper = new NtlmPassiveUnwrapper(wrongKey, Flags);
        NtlmUnwrapResult result = unwrapper.TryUnwrap(NtlmDirection.ClientToServer, cipherStub, trailer);

        await Assert.That(result.Status).IsEqualTo(NtlmUnwrapStatus.SignatureMismatch);
        await Assert.That(result.Reason).IsNotNull();
        await Assert.That(result.Reason!).Contains("Signature mismatch");
        await Assert.That(unwrapper.ClientSequence).IsEqualTo(0);
        await Assert.That(unwrapper.ServerSequence).IsEqualTo(0);
    }

    [Test]
    public async Task TryUnwrap_SkippedPdu_NextUnwrapFailsWithSignatureMismatch_DocumentsCounterDriftLimitation()
    {
        // Producer emits 3 PDUs with counters 0, 1, 2.
#pragma warning disable CS0618
        var producer = new Ntlm1(Flags, (byte[])s_testSessionKey.Clone(), isServer: false);
#pragma warning restore CS0618
        (byte[] _, byte[] _) = SealWithProducer(producer, BuildPlaintext(16));
        (byte[] cipher2, byte[] trailer2) = SealWithProducer(producer, BuildPlaintext(24));
        (byte[] _, byte[] _) = SealWithProducer(producer, BuildPlaintext(32));

        // Passive unwrapper starts at counter=0 but we feed it PDU #2 (counter=1).
        // The HMAC mismatch is detected; counter does NOT advance.
        using var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey, Flags);
        NtlmUnwrapResult result = unwrapper.TryUnwrap(NtlmDirection.ClientToServer, cipher2, trailer2);

        await Assert.That(result.Status).IsEqualTo(NtlmUnwrapStatus.SignatureMismatch);
        await Assert.That(unwrapper.ClientSequence).IsEqualTo(0);
    }

    [Test]
    public async Task TryUnwrap_AfterDispose_ThrowsObjectDisposedException()
    {
        var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey, Flags);
        unwrapper.Dispose();
        await Assert.That(() => unwrapper.TryUnwrap(
            NtlmDirection.ClientToServer,
            new byte[16],
            new byte[16])).Throws<ObjectDisposedException>();
    }

    [Test]
    public async Task Dispose_IsIdempotent()
    {
        var unwrapper = new NtlmPassiveUnwrapper(s_testSessionKey, Flags);
        unwrapper.Dispose();
        unwrapper.Dispose();
        // Second dispose returned normally without throwing.
        await Assert.That(() => unwrapper.TryUnwrap(
            NtlmDirection.ClientToServer,
            new byte[16],
            new byte[16])).Throws<ObjectDisposedException>();
    }

    [Test]
    public async Task Disabled_DisposeIsNoOp()
    {
        NtlmPassiveUnwrapper.Disabled.Dispose();
        // The singleton must remain usable after dispose (we never actually
        // dispose its keys — they are all-zero stubs anyway).
        NtlmUnwrapResult result = NtlmPassiveUnwrapper.Disabled.TryUnwrap(
            NtlmDirection.ClientToServer, new byte[4], new byte[16]);
        await Assert.That(result.Status).IsEqualTo(NtlmUnwrapStatus.Disabled);
    }

    private static byte[] BuildPlaintext(int length)
    {
        var data = new byte[length];
        for (int i = 0; i < length; i++)
        {
            data[i] = (byte)(i * 7 + 3);
        }
        return data;
    }

    /// <summary>
    /// Encrypts and signs a buffer using a fresh production Ntlm1 instance
    /// (starting at counter 0) and returns (ciphertext, auth-trailer).
    /// </summary>
    private static (byte[] CipherStub, byte[] Trailer) SealOutgoing(
        byte[] sessionKey, byte[] plaintext, bool isServer)
    {
#pragma warning disable CS0618 // Ntlm1 [Obsolete] is intentional in this passive-unwrap test
        var producer = new Ntlm1(Flags, (byte[])sessionKey.Clone(), isServer);
#pragma warning restore CS0618
        return SealWithProducer(producer, plaintext);
    }

    /// <summary>
    /// Same as <see cref="SealOutgoing"/> but reuses an existing producer so
    /// the test can advance counters across multiple PDUs.
    /// </summary>
#pragma warning disable CS0618
    private static (byte[] CipherStub, byte[] Trailer) SealWithProducer(Ntlm1 producer, byte[] plaintext)
#pragma warning restore CS0618
    {
        const int verifierLength = NtlmPassiveUnwrapper.VerifierLength;
        var buffer = new byte[plaintext.Length + verifierLength];
        Array.Copy(plaintext, 0, buffer, 0, plaintext.Length);

        var ndrBuffer = new NdrBuffer(buffer, 0);
        ndrBuffer.SetLength(buffer.Length);
        var ndr = new NdrCodec { Buffer = ndrBuffer, Format = NdrFormat.DEFAULT_FORMAT };

        producer.ProcessOutgoing(ndr, index: 0, length: plaintext.Length, verifierIndex: plaintext.Length, isFragmented: false);

        var stub = new byte[plaintext.Length];
        Array.Copy(buffer, 0, stub, 0, plaintext.Length);
        var trailer = new byte[verifierLength];
        Array.Copy(buffer, plaintext.Length, trailer, 0, verifierLength);
        return (stub, trailer);
    }
}
