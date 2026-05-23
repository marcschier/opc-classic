//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class OpcUrlTests
{
    [Test]
    [Arguments("opcda://host/Matrikon.OPC.Simulation.1", OpcUrlScheme.Da, "host", 0, "Matrikon.OPC.Simulation.1", false)]
    [Arguments("opcae://server.example.com:5000/My.AE.Server", OpcUrlScheme.Ae, "server.example.com", 5000, "My.AE.Server", false)]
    [Arguments("opchda://10.0.0.1/Some.HDA.1", OpcUrlScheme.Hda, "10.0.0.1", 0, "Some.HDA.1", false)]
    [Arguments("opcdx://localhost/Vendor.Dx", OpcUrlScheme.Dx, "localhost", 0, "Vendor.Dx", false)]
    [Arguments("opc.xml-da://https-host/service.asmx", OpcUrlScheme.XmlDa, "https-host", 0, "service.asmx", false)]
    public async Task Parse_ValidScheme_DecomposesCorrectly(
        string input, OpcUrlScheme scheme, string host, int port, string serverId, bool isClsid)
    {
        var u = OpcUrl.Parse(input);
        await Assert.That(u.Scheme).IsEqualTo(scheme);
        await Assert.That(u.Host).IsEqualTo(host);
        await Assert.That(u.Port).IsEqualTo(port);
        await Assert.That(u.ServerId).IsEqualTo(serverId);
        await Assert.That(u.IsClsid).IsEqualTo(isClsid);
    }

    [Test]
    public async Task Parse_CaseInsensitive_Scheme()
    {
        var u = OpcUrl.Parse("OPCDA://Host/X.Y.Z");
        await Assert.That(u.Scheme).IsEqualTo(OpcUrlScheme.Da);
    }

    [Test]
    public async Task Parse_ClsidPath_IsClsidTrue()
    {
        var u = OpcUrl.Parse("opcda://host/{F8582CF2-88FB-11D0-B850-00C0F0104305}");
        await Assert.That(u.IsClsid).IsTrue();
        await Assert.That(u.ServerId).IsEqualTo("{F8582CF2-88FB-11D0-B850-00C0F0104305}");
    }

    [Test]
    public async Task Parse_StripsQueryString()
    {
        var u = OpcUrl.Parse("opcda://host/My.Server?foo=bar");
        await Assert.That(u.ServerId).IsEqualTo("My.Server");
    }

    [Test]
    public async Task TryParse_InvalidScheme_ReturnsFalse()
    {
        var ok = OpcUrl.TryParse("http://host/path", out var result);
        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryParse_MissingPath_ReturnsFalse()
    {
        var ok = OpcUrl.TryParse("opcda://host", out var result);
        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryParse_InvalidPort_ReturnsFalse()
    {
        var ok = OpcUrl.TryParse("opcda://host:99999/X", out var result);
        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task TryParse_Null_ReturnsFalse()
    {
        var ok = OpcUrl.TryParse(null, out var result);
        await Assert.That(ok).IsFalse();
        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task Equals_CaseInsensitiveOnHostAndServerId()
    {
        var a = OpcUrl.Parse("opcda://HOST/My.Server");
        var b = OpcUrl.Parse("opcda://host/my.server");
        await Assert.That(a.Equals(b)).IsTrue();
        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task ToString_ReturnsOriginal()
    {
        const string input = "opcda://host:5000/My.Server";
        var u = OpcUrl.Parse(input);
        await Assert.That(u.ToString()).IsEqualTo(input);
    }

    [Test]
    public async Task Parse_InvalidUrl_Throws()
    {
        await Assert.That(() => { OpcUrl.Parse("not a url at all"); })
            .Throws<System.FormatException>();
    }
}
