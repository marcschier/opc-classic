//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Reflection;
using Kerberos.NET;
using Kerberos.NET.Entities;
using Opc.Classic.Dcom.Kerberos;
using TUnit.Core;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosChannelBindingChecksumTests {
    [Test]
    public async Task Create_EncodesMsKileGssChecksumWithChannelBindingHash() {
        byte[] channelBindingsHash = Convert.FromHexString("00112233445566778899AABBCCDDEEFF");

        KrbChecksum checksum = KerberosChannelBindingChecksum.Create(
            channelBindingsHash,
            GssContextEstablishmentFlag.GSS_C_MUTUAL_FLAG);
        DelegationInfo decoded = checksum.DecodeDelegation();

        await Assert.That((int)checksum.Type).IsEqualTo(KerberosChannelBindingChecksum.KrbApChecksumTypeGss);
        await Assert.That(decoded.ChannelBinding.ToArray().SequenceEqual(channelBindingsHash)).IsTrue();
        await Assert.That(decoded.Flags).IsEqualTo(GssContextEstablishmentFlag.GSS_C_MUTUAL_FLAG);
    }

    [Test]
    public async Task KerberosConnectionContext_RequestServiceTicketCarriesChannelBindingChecksum() {
        byte[] channelBindingsHash = Convert.FromHexString("FFEEDDCCBBAA99887766554433221100");
        var authInfo = new KerberosAuthInfo("EXAMPLE.COM", "RPCSS/server.example.com", "alice", null, "password", null);

        RequestServiceTicket request = CreateRequestServiceTicket(authInfo, channelBindingsHash);
        KrbChecksum checksum = request.AuthenticatorChecksum;
        DelegationInfo decoded = checksum.DecodeDelegation();

        await Assert.That(request.ServicePrincipalName).IsEqualTo("RPCSS/server.example.com");
        await Assert.That((int)checksum.Type).IsEqualTo(KerberosChannelBindingChecksum.KrbApChecksumTypeGss);
        await Assert.That(decoded.ChannelBinding.ToArray().SequenceEqual(channelBindingsHash)).IsTrue();
    }

    [Test]
    public async Task KerberosConnectionContext_NoChannelBindingLeavesAuthenticatorChecksumUnset() {
        var authInfo = new KerberosAuthInfo("EXAMPLE.COM", "RPCSS/server.example.com", "alice", null, "password", null);

        RequestServiceTicket request = CreateRequestServiceTicket(authInfo, channelBindingsHash: null);

        await Assert.That(request.AuthenticatorChecksum).IsNull();
    }

    private static RequestServiceTicket CreateRequestServiceTicket(
        KerberosAuthInfo authInfo,
        ReadOnlyMemory<byte>? channelBindingsHash) {
        MethodInfo method = typeof(KerberosConnectionContext).GetMethod(
            "CreateRequestServiceTicket",
            BindingFlags.Static | BindingFlags.NonPublic)!;
        return (RequestServiceTicket)method.Invoke(null, new object?[] { authInfo, channelBindingsHash })!;
    }
}
