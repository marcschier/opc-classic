//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class KerberosAuthInfoTests
{
    [Test]
    public async Task KerberosAuthInfo_validates_required_fields()
    {
        await Assert.That(() =>
        {
            _ = new KerberosAuthInfo(string.Empty, "RPCSS/server.example.com", "alice", null, null, null);
        }).Throws<ArgumentException>();

        await Assert.That(() =>
        {
            _ = new KerberosAuthInfo("EXAMPLE.COM", string.Empty, "alice", null, null, null);
        }).Throws<ArgumentException>();

        await Assert.That(() =>
        {
            _ = new KerberosAuthInfo("EXAMPLE.COM", "RPCSS/server.example.com", string.Empty, null, null, null);
        }).Throws<ArgumentException>();
    }

    [Test]
    public async Task KerberosAuthInfo_accepts_optional_fields()
    {
        var authInfo = new KerberosAuthInfo("EXAMPLE.COM", "RPCSS/server.example.com", "alice", null, null, null);
        IKerberosAuthInfo contract = authInfo;

        await Assert.That(contract.Realm).IsEqualTo("EXAMPLE.COM");
        await Assert.That(contract.Spn).IsEqualTo("RPCSS/server.example.com");
        await Assert.That(contract.Username).IsEqualTo("alice");
        await Assert.That(contract.Domain).IsNull();
        await Assert.That(authInfo.Password).IsNull();
        await Assert.That(authInfo.KeytabPath).IsNull();
    }
}
