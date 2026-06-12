//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Kerberos.NET.Crypto;
using Opc.Classic.Dcom.Kerberos.Spnego;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosSpnegoMicProviderTests
{
    [Test]
    public async Task KerberosMicProvider_uses_session_GetMic_and_VerifyMic_for_mechListMIC()
    {
        var session = new KerberosSession(
            KerberosTestHex.FromHex("00112233445566778899AABBCCDDEEFF"),
            EncryptionType.AES128_CTS_HMAC_SHA1_96);
        var provider = new KerberosMicProvider(session);
        byte[] mechList = [0x30, 0x0B, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x12, 0x01, 0x02, 0x02];

        byte[] mic = provider.GetMic(mechList);
        bool verified = provider.VerifyMic(mechList, mic);

        await Assert.That(verified).IsTrue();
    }
}
