//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosReplayProtectionTests
{
    private static readonly byte[] Key = KerberosTestHex.FromHex("00112233445566778899AABBCCDDEEFF");

    [Test]
    public async Task UnwrapMessage_rejects_out_of_order_sequence_numbers()
    {
        var sender = new KerberosSession(Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        var receiver = new KerberosSession(Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        _ = sender.WrapMessage([0x01], confidential: false);
        byte[] secondToken = sender.WrapMessage([0x02], confidential: false);

        Exception? thrown = CaptureException(() => receiver.UnwrapMessage(secondToken, out _));

        await Assert.That(thrown is InvalidOperationException).IsTrue();
        await Assert.That(thrown!.Message).Contains("sequence");
    }

    [Test]
    public async Task UnwrapMessage_rejects_replayed_old_sequence_numbers()
    {
        var sender = new KerberosSession(Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        var receiver = new KerberosSession(Key, EncryptionType.AES128_CTS_HMAC_SHA1_96);
        byte[] firstToken = sender.WrapMessage([0x01], confidential: false);

        _ = receiver.UnwrapMessage(firstToken, out _);
        Exception? thrown = CaptureException(() => receiver.UnwrapMessage(firstToken, out _));

        await Assert.That(thrown is InvalidOperationException).IsTrue();
        await Assert.That(thrown!.Message).Contains("sequence");
    }

    private static Exception? CaptureException(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
