//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureSessionStateTests
{
    [Test]
    public async Task EnumMembers_AreOrderedAsLifecycleStateMachineExpects()
    {
        CaptureSessionState[] states = Enum.GetValues<CaptureSessionState>();

        await Assert.That((int)states[0]).IsEqualTo(0);
        await Assert.That((int)states[1]).IsEqualTo(1);
        await Assert.That((int)states[2]).IsEqualTo(2);
        await Assert.That((int)states[3]).IsEqualTo(3);
        await Assert.That((int)states[4]).IsEqualTo(4);
        await Assert.That((int)states[5]).IsEqualTo(5);
    }

    [Test]
    public async Task GetNames_ReturnsAllPublicStates()
    {
        string[] names = Enum.GetNames<CaptureSessionState>();

        await Assert.That(names.Length).IsEqualTo(6);
        await Assert.That(names[0]).IsEqualTo("Starting");
        await Assert.That(names[5]).IsEqualTo("Disposed");
    }
}
