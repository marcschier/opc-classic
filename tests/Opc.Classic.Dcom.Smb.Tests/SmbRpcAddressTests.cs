//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dcom.Smb;
using Opc.Classic.Dcom.Smb.Rpc;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests;

public sealed class SmbRpcAddressTests {
    [Test]
    public async Task Parse_PlainHostShare_PipeName() {
        var p = SmbRpcAddress.Parse("smb://server/IPC$/winreg");
        await Assert.That(p.Host).IsEqualTo("server");
        await Assert.That(p.ShareName).IsEqualTo("IPC$");
        await Assert.That(p.PipeName).IsEqualTo("winreg");
        await Assert.That(p.UserName).IsNull();
        await Assert.That(p.Domain).IsNull();
        await Assert.That(p.Password).IsNull();
    }

    [Test]
    public async Task Parse_UserDomainAndPassword() {
        var p = SmbRpcAddress.Parse("smb://CORP;alice:s3cret@server/IPC$/winreg");
        await Assert.That(p.Host).IsEqualTo("server");
        await Assert.That(p.PipeName).IsEqualTo("winreg");
        await Assert.That(p.Domain).IsEqualTo("CORP");
        await Assert.That(p.UserName).IsEqualTo("alice");
        await Assert.That(p.Password).IsEqualTo("s3cret");
    }

    [Test]
    public async Task Parse_UrlEncodedPassword() {
        var p = SmbRpcAddress.Parse("smb://alice:hello%20world@server/IPC$/winreg");
        await Assert.That(p.UserName).IsEqualTo("alice");
        await Assert.That(p.Password).IsEqualTo("hello world");
    }

    [Test]
    public async Task Parse_RejectsNonSmbScheme() {
        bool threw = false;
        try { _ = SmbRpcAddress.Parse("http://example.com/IPC$/winreg"); }
        catch (FormatException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Format_RoundTrip() {
        var original = new SmbRpcAddress.Parsed(
            Host: "server",
            ShareName: "IPC$",
            PipeName: "winreg",
            UserName: "alice",
            Domain: "CORP",
            Password: "hello world");
        string formatted = SmbRpcAddress.Format(original);
        await Assert.That(formatted).Contains("CORP;alice");
        await Assert.That(formatted).Contains("@server/IPC$/winreg");
        var roundTripped = SmbRpcAddress.Parse(formatted);
        await Assert.That(roundTripped.Host).IsEqualTo(original.Host);
        await Assert.That(roundTripped.UserName).IsEqualTo(original.UserName);
        await Assert.That(roundTripped.Domain).IsEqualTo(original.Domain);
        await Assert.That(roundTripped.Password).IsEqualTo(original.Password);
    }
}
