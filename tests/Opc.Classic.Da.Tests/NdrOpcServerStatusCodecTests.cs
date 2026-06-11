//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

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

    // -- FILETIME decode hypothesis matrix --

    [Test]
    public async Task Decode_FileTime_Zero_Yields1601Epoch()
    {
        byte[] wire = WireWithRawFileTimes(rawStartFileTime: 0L, rawCurrentFileTime: 0L, rawLastUpdateFileTime: 0L);
        OpcServerStatus back = ReadOne(wire);
        var epoch = new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero);
        await Assert.That(back.StartTime).IsEqualTo(epoch);
        await Assert.That(back.CurrentTime).IsEqualTo(epoch);
        await Assert.That(back.LastUpdateTime).IsEqualTo(epoch);
    }

    [Test]
    [Arguments(long.MinValue)]
    [Arguments(-1L)]
    [Arguments(long.MaxValue)]
    public async Task Decode_FileTime_OutOfRange_ThrowsInvalidDataException_WithContext(long bogusFileTime)
    {
        byte[] wire = WireWithRawFileTimes(rawStartFileTime: bogusFileTime, rawCurrentFileTime: 0L, rawLastUpdateFileTime: 0L);

        var thrown = await Assert.ThrowsAsync<System.IO.InvalidDataException>(() => Task.FromResult(ReadOne(wire)));
        await Assert.That(thrown.Message).Contains("OPCSERVERSTATUS.ftStartTime");
        await Assert.That(thrown.Message).Contains("FILETIME value");
        await Assert.That(thrown.Message).Contains("Wire context");
    }

    [Test]
    public async Task Decode_FileTime_NamesFailingField()
    {
        // Cause the LAST field (ftLastUpdateTime) to overflow; verify the exception names that field, not the earlier two.
        byte[] wire = WireWithRawFileTimes(rawStartFileTime: 0L, rawCurrentFileTime: 0L, rawLastUpdateFileTime: long.MaxValue);

        var thrown = await Assert.ThrowsAsync<System.IO.InvalidDataException>(() => Task.FromResult(ReadOne(wire)));
        await Assert.That(thrown.Message).Contains("OPCSERVERSTATUS.ftLastUpdateTime");
        await Assert.That(thrown.Message).DoesNotContain("ftStartTime");
        await Assert.That(thrown.Message).DoesNotContain("ftCurrentTime");
    }

    [Test]
    public async Task Decode_FileTime_NamesFailingField_StartTime()
    {
        // Symmetric coverage for the FIRST field: only ftStartTime is corrupt;
        // exception must name it (not the unread later fields).
        byte[] wire = WireWithRawFileTimes(rawStartFileTime: long.MaxValue, rawCurrentFileTime: 0L, rawLastUpdateFileTime: 0L);

        var thrown = await Assert.ThrowsAsync<System.IO.InvalidDataException>(() => Task.FromResult(ReadOne(wire)));
        await Assert.That(thrown.Message).Contains("OPCSERVERSTATUS.ftStartTime");
        await Assert.That(thrown.Message).DoesNotContain("ftCurrentTime");
        await Assert.That(thrown.Message).DoesNotContain("ftLastUpdateTime");
    }

    [Test]
    public async Task Decode_FileTime_NamesFailingField_CurrentTime()
    {
        // Symmetric coverage for the MIDDLE field: ftStartTime valid, ftCurrentTime corrupt.
        byte[] wire = WireWithRawFileTimes(rawStartFileTime: 0L, rawCurrentFileTime: -1L, rawLastUpdateFileTime: 0L);

        var thrown = await Assert.ThrowsAsync<System.IO.InvalidDataException>(() => Task.FromResult(ReadOne(wire)));
        await Assert.That(thrown.Message).Contains("OPCSERVERSTATUS.ftCurrentTime");
        await Assert.That(thrown.Message).DoesNotContain("ftStartTime");
        await Assert.That(thrown.Message).DoesNotContain("ftLastUpdateTime");
    }

    [Test]
    public async Task Decode_FileTime_MaxValid_DecodesToYear9999()
    {
        // Positive boundary test: the max FILETIME that still fits in DateTimeOffset
        // must decode cleanly, not be over-zealously rejected by the strict guard.
        const long FileTimeEpochOffsetTicks = 504911232000000000L;
        long maxRaw = DateTimeOffset.MaxValue.UtcTicks - FileTimeEpochOffsetTicks;

        byte[] wire = WireWithRawFileTimes(rawStartFileTime: maxRaw, rawCurrentFileTime: 0L, rawLastUpdateFileTime: 0L);
        OpcServerStatus back = ReadOne(wire);
        await Assert.That(back.StartTime.Year).IsEqualTo(9999);
    }

    /// <summary>
    /// Builds a synthetic OPCSERVERSTATUS wire payload where the three FILETIME fields hold
    /// arbitrary <see langword="long"/> values (bypassing the codec's writer-side validation).
    /// Used to exercise decode-side edge cases that the writer's <c>DateTimeOffset</c>
    /// roundtrip would never produce.
    /// </summary>
    private static byte[] WireWithRawFileTimes(long rawStartFileTime, long rawCurrentFileTime, long rawLastUpdateFileTime)
    {
        return WriteOne(
            (ref NdrWriter w) =>
            {
                w.WriteFileTime(rawStartFileTime);
                w.WriteFileTime(rawCurrentFileTime);
                w.WriteFileTime(rawLastUpdateFileTime);
                w.WriteUInt32((uint)OpcServerState.Running);
                w.WriteUInt32(0u);   // GroupCount
                w.WriteUInt32(0u);   // BandWidth
                w.WriteUInt16(1);    // Major
                w.WriteUInt16(0);    // Minor
                w.WriteUInt16(0);    // Build
                w.WriteUInt16(0);    // Reserved
                w.WriteUnicodeStringPtr("test");
            }, capacity: 512);
    }
}
