//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureStartRequestTests
{
    [Test]
    public async Task Constructor_Defaults_MatchCaptureToolDefaults()
    {
        var request = new CaptureStartRequest();

        await Assert.That(request.InterfaceName).IsNull();
        await Assert.That(request.BpfFilter).IsNull();
        await Assert.That(request.Promiscuous).IsTrue();
        await Assert.That(request.MaxBytes).IsNull();
        await Assert.That(request.MaxPackets).IsNull();
        await Assert.That(request.MaxDurationSeconds).IsNull();
        await Assert.That(request.ReplaySourceDirectory).IsNull();
        await Assert.That(request.ServerPorts).IsNull();
    }

    [Test]
    public async Task Constructor_ServerPortsSupplied_IsPersisted()
    {
        var request = new CaptureStartRequest(
            InterfaceName: "eth0",
            ServerPorts: [51301, 51302]);

        await Assert.That(request.ServerPorts).IsNotNull();
        await Assert.That(request.ServerPorts!.Count).IsEqualTo(2);
        await Assert.That(request.ServerPorts[0]).IsEqualTo(51301);
        await Assert.That(request.ServerPorts[1]).IsEqualTo(51302);
    }

    [Test]
    public async Task Constructor_NtlmSessionKey_IsPersisted_AndRedactedInToString()
    {
        var key = new byte[16];
        for (int i = 0; i < 16; i++) { key[i] = (byte)(0xC0 + i); }
        var request = new CaptureStartRequest(InterfaceName: "lo", NtlmSessionKey: key);

        await Assert.That(request.NtlmSessionKey).IsNotNull();
        await Assert.That(request.NtlmSessionKey!.Length).IsEqualTo(16);

        string str = request.ToString();
        // Redaction: the raw bytes (e.g. C0, C1, ...) MUST NOT appear; "REDACTED" MUST.
        await Assert.That(str).Contains("REDACTED");
        await Assert.That(str).Contains("16 bytes");
        await Assert.That(str).DoesNotContain("C0");
        await Assert.That(str).DoesNotContain("0xC0");
    }

    [Test]
    public async Task Constructor_NullNtlmSessionKey_TostringSaysNull()
    {
        var request = new CaptureStartRequest(InterfaceName: "lo");
        string str = request.ToString();

        await Assert.That(str).Contains("NtlmSessionKey = null");
        await Assert.That(str).DoesNotContain("REDACTED");
    }

    [Test]
    public async Task WithExpression_ReplacesSelectedValuesAndPreservesRecordValueEquality()
    {
        var original = new CaptureStartRequest(InterfaceName: "eth0", MaxPackets: 10);
        CaptureStartRequest changed = original with
        {
            BpfFilter = "tcp port 135",
            Promiscuous = false,
            ReplaySourceDirectory = "captures",
        };
        var expected = new CaptureStartRequest(
            InterfaceName: "eth0",
            BpfFilter: "tcp port 135",
            Promiscuous: false,
            MaxPackets: 10,
            ReplaySourceDirectory: "captures");

        await Assert.That(changed).IsEqualTo(expected);
        await Assert.That(original.BpfFilter).IsNull();
        await Assert.That(original.Promiscuous).IsTrue();
    }
}
