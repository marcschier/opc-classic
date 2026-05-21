//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using OpcClassic;
using TUnit.Core;

namespace OpcClassic.Tests;

public sealed class OpcServerStatusTests
{
    [Test]
    public async Task IsOperational_OnlyWhenRunning()
    {
        var running = new OpcServerStatus { State = OpcServerState.Running };
        var failed = new OpcServerStatus { State = OpcServerState.Failed };
        var commFault = new OpcServerStatus { State = OpcServerState.CommFault };

        await Assert.That(running.IsOperational).IsTrue();
        await Assert.That(failed.IsOperational).IsFalse();
        await Assert.That(commFault.IsOperational).IsFalse();
    }

    [Test]
    public async Task Default_HasUnknownSpecAndState()
    {
        var s = new OpcServerStatus();
        await Assert.That(s.Spec).IsEqualTo(OpcStatusSpec.Unknown);
        await Assert.That(s.State).IsEqualTo(OpcServerState.Unknown);
        await Assert.That(s.VendorInfo).IsEqualTo(string.Empty);
        await Assert.That(s.ServerVersion).IsEqualTo(new Version(0, 0, 0));
    }

    [Test]
    public async Task InitializerSyntax_AssignsAllFields()
    {
        var start = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var current = new DateTimeOffset(2026, 5, 21, 12, 30, 0, TimeSpan.Zero);
        var lastUpdate = new DateTimeOffset(2026, 5, 21, 12, 29, 59, TimeSpan.Zero);

        var s = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = start,
            CurrentTime = current,
            LastUpdateTime = lastUpdate,
            State = OpcServerState.Running,
            ServerVersion = new Version(3, 0, 107),
            VendorInfo = "OPC Foundation Sample DA Server",
            GroupCount = 5,
            BandWidth = 1234,
        };

        await Assert.That(s.Spec).IsEqualTo(OpcStatusSpec.Da);
        await Assert.That(s.StartTime).IsEqualTo(start);
        await Assert.That(s.CurrentTime).IsEqualTo(current);
        await Assert.That(s.LastUpdateTime).IsEqualTo(lastUpdate);
        await Assert.That(s.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(s.ServerVersion).IsEqualTo(new Version(3, 0, 107));
        await Assert.That(s.VendorInfo).IsEqualTo("OPC Foundation Sample DA Server");
        await Assert.That(s.GroupCount).IsEqualTo(5);
        await Assert.That(s.BandWidth).IsEqualTo(1234u);
        await Assert.That(s.IsOperational).IsTrue();
    }

    [Test]
    public async Task ToString_IncludesKeyFields()
    {
        var s = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            ServerVersion = new Version(1, 20, 4),
            VendorInfo = "Acme HDA",
            State = OpcServerState.Suspended,
            CurrentTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
        };
        var s_string = s.ToString();
        await Assert.That(s_string).Contains("Hda");
        await Assert.That(s_string).Contains("Acme HDA");
        await Assert.That(s_string).Contains("Suspended");
    }
}
