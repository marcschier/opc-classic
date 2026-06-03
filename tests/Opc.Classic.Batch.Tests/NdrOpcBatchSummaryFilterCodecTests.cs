//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Batch;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Batch.Tests;

public sealed class NdrOpcBatchSummaryFilterCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcBatchSummaryFilter ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcBatchSummaryFilterCodec.Read(ref r);
    }

    private static OpcBatchSummaryFilter RoundTrip(OpcBatchSummaryFilter input)
    {
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBatchSummaryFilterCodec.Write(ref w, input));
        return ReadOne(bytes);
    }

    [Test]
    public async Task RoundTrip_WideOpenFilter()
    {
        var minTime = DateTimeOffset.UnixEpoch.AddYears(-50);
        var maxTime = DateTimeOffset.UnixEpoch.AddYears(100);
        var input = new OpcBatchSummaryFilter(
            Id: null,
            Description: null,
            OpcItemId: null,
            MasterRecipeId: null,
            MinBatchSize: 0.0f,
            MaxBatchSize: float.MaxValue,
            EngineeringUnits: null,
            ExecutionState: null,
            ExecutionMode: null,
            MinStartTime: minTime,
            MaxStartTime: maxTime,
            MinEndTime: minTime,
            MaxEndTime: maxTime);

        var back = RoundTrip(input);

        await Assert.That(back).IsEqualTo(input);
    }

    [Test]
    public async Task RoundTrip_NarrowTimeWindow()
    {
        var startMin = new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero);
        var startMax = startMin.AddHours(1);
        var endMin = new DateTimeOffset(2026, 5, 22, 12, 0, 0, TimeSpan.Zero);
        var endMax = endMin.AddHours(1);
        var input = new OpcBatchSummaryFilter(
            Id: "BATCH-2026-05-22-001",
            Description: "production lot",
            OpcItemId: "Batch.Area1.UnitA",
            MasterRecipeId: "MR-42",
            MinBatchSize: 0.0f,
            MaxBatchSize: float.MaxValue,
            EngineeringUnits: "kg",
            ExecutionState: "Complete",
            ExecutionMode: "Automatic",
            MinStartTime: startMin,
            MaxStartTime: startMax,
            MinEndTime: endMin,
            MaxEndTime: endMax);

        var back = RoundTrip(input);

        await Assert.That(back).IsEqualTo(input);
    }

    [Test]
    public async Task RoundTrip_RealisticBatchSizeRange()
    {
        var input = new OpcBatchSummaryFilter(
            Id: "Batch",
            Description: "blend",
            OpcItemId: "Plant1.Line2.Batch",
            MasterRecipeId: "Recipe-A",
            MinBatchSize: 50.0f,
            MaxBatchSize: 200.0f,
            EngineeringUnits: "L",
            ExecutionState: "Running",
            ExecutionMode: "Manual",
            MinStartTime: DateTimeOffset.UnixEpoch,
            MaxStartTime: DateTimeOffset.UnixEpoch.AddDays(1),
            MinEndTime: DateTimeOffset.UnixEpoch.AddDays(1),
            MaxEndTime: DateTimeOffset.UnixEpoch.AddDays(2));

        var back = RoundTrip(input);

        await Assert.That(back.MinBatchSize).IsEqualTo(50.0f);
        await Assert.That(back.MaxBatchSize).IsEqualTo(200.0f);
        await Assert.That(back).IsEqualTo(input);
    }

    [Test]
    public async Task RoundTrip_AllEmptyStrings()
    {
        // FILETIME = Epoch (1601-01-01 UTC) is the natural "zero" wire value
        // for OPC Batch timestamp fields. DateTimeOffset.MinValue would round-trip
        // through a negative FILETIME which the strict decode (Track AW) rejects
        // as out-of-range per the FILETIME spec.
        var input = new OpcBatchSummaryFilter(
            Id: string.Empty,
            Description: string.Empty,
            OpcItemId: string.Empty,
            MasterRecipeId: string.Empty,
            MinBatchSize: default,
            MaxBatchSize: default,
            EngineeringUnits: string.Empty,
            ExecutionState: string.Empty,
            ExecutionMode: string.Empty,
            MinStartTime: FileTimeHelper.Epoch,
            MaxStartTime: FileTimeHelper.Epoch,
            MinEndTime: FileTimeHelper.Epoch,
            MaxEndTime: FileTimeHelper.Epoch);

        var back = RoundTrip(input);

        await Assert.That(back).IsEqualTo(input);
    }
}
