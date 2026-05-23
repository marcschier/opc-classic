//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class NdrOpcServerStatusCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcServerStatus ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcServerStatusCodec.Read(ref r);
    }

    private static OpcServerStatus BuildSample(string vendor = "Acme OPC Inc.") => new()
    {
        Spec = OpcStatusSpec.Da,
        StartTime = new DateTimeOffset(2026, 5, 22, 0, 0, 0, TimeSpan.Zero),
        CurrentTime = new DateTimeOffset(2026, 5, 22, 10, 30, 0, TimeSpan.Zero),
        LastUpdateTime = new DateTimeOffset(2026, 5, 22, 10, 29, 50, TimeSpan.Zero),
        State = OpcServerState.Running,
        ServerVersion = new Version(2, 5, 1),
        GroupCount = 17,
        BandWidth = 4500,
        VendorInfo = vendor,
    };

    [Test]
    public async Task RoundTrip_TypicalRunningServer()
    {
        var input = BuildSample();
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(back.ServerVersion.Major).IsEqualTo(2);
        await Assert.That(back.ServerVersion.Minor).IsEqualTo(5);
        await Assert.That(back.ServerVersion.Build).IsEqualTo(1);
        await Assert.That(back.GroupCount).IsEqualTo(17);
        await Assert.That(back.BandWidth).IsEqualTo(4500u);
        await Assert.That(back.VendorInfo).IsEqualTo("Acme OPC Inc.");
    }

    [Test]
    public async Task RoundTrip_PreservesTimestamps()
    {
        var input = BuildSample();
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.StartTime.UtcDateTime).IsEqualTo(input.StartTime.UtcDateTime);
        await Assert.That(back.CurrentTime.UtcDateTime).IsEqualTo(input.CurrentTime.UtcDateTime);
        await Assert.That(back.LastUpdateTime.UtcDateTime).IsEqualTo(input.LastUpdateTime.UtcDateTime);
    }

    [Test]
    public async Task RoundTrip_EmptyVendorInfo()
    {
        var input = BuildSample(vendor: string.Empty);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.VendorInfo).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task RoundTrip_UnicodeVendor()
    {
        var input = BuildSample(vendor: "Müller Industriewerke 株式会社");
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.VendorInfo).IsEqualTo("Müller Industriewerke 株式会社");
    }

    [Test]
    public async Task RoundTrip_AllStates()
    {
        foreach (var state in new[] { OpcServerState.Running, OpcServerState.Failed, OpcServerState.NoConfig, OpcServerState.Suspended, OpcServerState.Test })
        {
            var sample = BuildSample();
            var input = new OpcServerStatus
            {
                Spec = sample.Spec,
                StartTime = sample.StartTime,
                CurrentTime = sample.CurrentTime,
                LastUpdateTime = sample.LastUpdateTime,
                State = state,
                ServerVersion = sample.ServerVersion,
                GroupCount = sample.GroupCount,
                BandWidth = sample.BandWidth,
                VendorInfo = sample.VendorInfo,
            };
            var bytes = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, input), capacity: 512);
            var back = ReadOne(bytes);
            await Assert.That(back.State).IsEqualTo(state);
        }
    }

    [Test]
    public async Task DecodedSpec_IsDa()
    {
        var input = BuildSample();
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcServerStatusCodec.Write(ref w, input), capacity: 512);
        var back = ReadOne(bytes);
        await Assert.That(back.Spec).IsEqualTo(OpcStatusSpec.Da);
    }
}
