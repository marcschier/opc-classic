//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Tests;

public sealed class NoOpAuthContextTests
{
    [Test]
    public async Task Instance_ExposesUnauthenticatedDefaults()
    {
        IAuthContext context = NoOpAuthContext.Instance;

        await Assert.That(object.ReferenceEquals(NoOpAuthContext.Instance, NoOpAuthContext.Instance)).IsTrue();
        await Assert.That(context.ProtectionLevel).IsEqualTo(OpcProtectionLevel.None);
        await Assert.That(context.AuthenticationServiceCode).IsEqualTo((byte)0);
    }

    [Test]
    public async Task BuildInitialToken_ReturnsEmptyToken()
    {
        byte[] token = NoOpAuthContext.Instance.BuildInitialToken();

        await Assert.That(token.Length).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessChallengeToken_IgnoresServerTokenAndReturnsEmptyToken()
    {
        byte[] token = NoOpAuthContext.Instance.ProcessChallengeToken(new byte[] { 0x01, 0x02, 0x03 });

        await Assert.That(token.Length).IsEqualTo(0);
    }

    [Test]
    public async Task SignAndSeal_LeavesPduUnchangedAndReturnsEmptySignature()
    {
        byte[] pdu = [0x10, 0x20, 0x30];

        NoOpAuthContext.Instance.SignAndSeal(pdu, out byte[] signature);

        await Assert.That(signature.Length).IsEqualTo(0);
        await Assert.That(pdu).IsEquivalentTo(new byte[] { 0x10, 0x20, 0x30 });
    }

    [Test]
    public async Task VerifyAndUnseal_EmptySignatureSucceedsAndLeavesPduUnchanged()
    {
        byte[] pdu = [0x40, 0x50];

        bool verified = NoOpAuthContext.Instance.VerifyAndUnseal(pdu, ReadOnlyMemory<byte>.Empty);

        await Assert.That(verified).IsTrue();
        await Assert.That(pdu).IsEquivalentTo(new byte[] { 0x40, 0x50 });
    }

    [Test]
    public async Task VerifyAndUnseal_NonEmptySignatureFails()
    {
        byte[] pdu = [0x40, 0x50];

        bool verified = NoOpAuthContext.Instance.VerifyAndUnseal(pdu, new byte[] { 0xAA });

        await Assert.That(verified).IsFalse();
        await Assert.That(pdu).IsEquivalentTo(new byte[] { 0x40, 0x50 });
    }
}
