//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Net;
using Opc.Classic;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class OpcConnectDataTests
{
    private static readonly OpcUrl TestUrl = OpcUrl.Parse("opcda://host/Test.Server");

    [Test]
    public async Task Default_IsNtlmV2_Integrity()
    {
        var creds = new NetworkCredential("u", "p", "d");
        var cd = new OpcConnectData(TestUrl, creds);
        await Assert.That(cd.AuthMode).IsEqualTo(OpcAuthMode.NtlmV2);
        await Assert.That(cd.ProtectionLevel).IsEqualTo(OpcProtectionLevel.Integrity);
        await Assert.That(cd.OperationTimeout).IsNull();
    }

    [Test]
    public async Task DefaultProtectionLevel_ExpandsToIntegrity()
    {
        var cd = new OpcConnectData(TestUrl, new NetworkCredential("u", "p"),
            protectionLevel: OpcProtectionLevel.Default);
        await Assert.That(cd.ProtectionLevel).IsEqualTo(OpcProtectionLevel.Integrity);
    }

    [Test]
    public async Task Anonymous_FactoryRequiresNoCredentials()
    {
        var cd = OpcConnectData.Anonymous(TestUrl);
        await Assert.That(cd.AuthMode).IsEqualTo(OpcAuthMode.Anonymous);
        await Assert.That(cd.Credentials).IsNull();
    }

    [Test]
    public async Task WithNtlmV2_PreservesCredentials()
    {
        var creds = new NetworkCredential("alice", "secret", "CORP");
        var cd = OpcConnectData.WithNtlmV2(TestUrl, creds);

        await Assert.That(cd.AuthMode).IsEqualTo(OpcAuthMode.NtlmV2);
        await Assert.That(cd.Credentials).IsNotNull();
        await Assert.That(cd.Credentials!.UserName).IsEqualTo("alice");
        await Assert.That(cd.Credentials.Domain).IsEqualTo("CORP");
    }

    [Test]
    public async Task WithKerberos_SetsKerberosMode()
    {
        var creds = new NetworkCredential("alice@CORP", "secret");
        var cd = OpcConnectData.WithKerberos(TestUrl, creds);
        await Assert.That(cd.AuthMode).IsEqualTo(OpcAuthMode.Kerberos);
    }

    [Test]
    public async Task NullUrl_Throws()
    {
        await Assert.That(() => { _ = new OpcConnectData(null!, new NetworkCredential("u", "p")); })
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Anonymous_WithCredentials_Throws()
    {
        await Assert.That(() =>
        {
            _ = new OpcConnectData(TestUrl, new NetworkCredential("u", "p"), authMode: OpcAuthMode.Anonymous);
        }).Throws<ArgumentException>();
    }

    [Test]
    public async Task NonAnonymous_WithoutCredentials_Throws()
    {
        await Assert.That(() => { _ = new OpcConnectData(TestUrl, credentials: null, authMode: OpcAuthMode.NtlmV2); })
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task NonPositiveTimeout_Throws()
    {
        await Assert.That(() =>
        {
            _ = new OpcConnectData(TestUrl, new NetworkCredential("u", "p"),
                operationTimeout: TimeSpan.Zero);
        }).Throws<ArgumentOutOfRangeException>();

        await Assert.That(() =>
        {
            _ = new OpcConnectData(TestUrl, new NetworkCredential("u", "p"),
                operationTimeout: TimeSpan.FromSeconds(-1));
        }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task PositiveTimeout_Accepted()
    {
        var timeout = TimeSpan.FromSeconds(15);
        var cd = new OpcConnectData(TestUrl, new NetworkCredential("u", "p"), operationTimeout: timeout);
        await Assert.That(cd.OperationTimeout).IsEqualTo(timeout);
    }
}
