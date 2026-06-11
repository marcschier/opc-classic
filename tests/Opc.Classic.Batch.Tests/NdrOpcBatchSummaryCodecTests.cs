//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Batch.Tests;

public sealed class NdrOpcBatchSummaryCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1_024)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcBatchSummary ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcBatchSummaryCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_TypicalBatchSummary()
    {
        var input = new OpcBatchSummary(
            Id: "B001",
            Description: "First Batch",
            OpcItemId: "Batch.Area1.Unit1.B001",
            MasterRecipeId: "MR-001",
            BatchSize: 100.5f,
            EngineeringUnits: "kg",
            ExecutionState: "Complete",
            ExecutionMode: "Automatic",
            ActualStartTime: DateTimeOffset.UnixEpoch,
            ActualEndTime: DateTimeOffset.UnixEpoch.AddHours(2));

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBatchSummaryCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        await Assert.That(back).IsEqualTo(input);
    }

    [Test]
    public async Task RoundTrip_AllNullStringFields_DefaultScalars()
    {
        // FILETIME = 0 (Epoch) is the natural "zero" wire value for OPC Batch
        // timestamp fields. DateTimeOffset.MinValue (year 0001) would round-trip
        // through a negative FILETIME which the strict decode rejects
        // as out-of-range per the FILETIME spec.
        var input = new OpcBatchSummary(
            Id: null,
            Description: null,
            OpcItemId: null,
            MasterRecipeId: null,
            BatchSize: 0f,
            EngineeringUnits: null,
            ExecutionState: null,
            ExecutionMode: null,
            ActualStartTime: FileTimeHelper.Epoch,
            ActualEndTime: FileTimeHelper.Epoch);

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBatchSummaryCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        await Assert.That(back).IsEqualTo(input);
    }

    [Test]
    public async Task RoundTrip_NonAsciiStrings()
    {
        var input = new OpcBatchSummary(
            Id: "B-温度-01",
            Description: "Müller",
            OpcItemId: "Anlage.Σ.Batch",
            MasterRecipeId: "Rezept-äöü",
            BatchSize: 12.25f,
            EngineeringUnits: "m³",
            ExecutionState: "Läuft",
            ExecutionMode: "Manuell",
            ActualStartTime: DateTimeOffset.UnixEpoch.AddMinutes(1),
            ActualEndTime: DateTimeOffset.UnixEpoch.AddMinutes(2));

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBatchSummaryCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        await Assert.That(back).IsEqualTo(input);
    }

    [Test]
    public async Task RoundTrip_EpochBoundaryTimes()
    {
        var input = new OpcBatchSummary(
            Id: "B-EPOCH",
            Description: "Epoch boundary",
            OpcItemId: "Batch.Epoch",
            MasterRecipeId: "MR-EPOCH",
            BatchSize: 1f,
            EngineeringUnits: "batch",
            ExecutionState: "Done",
            ExecutionMode: "Automatic",
            ActualStartTime: DateTimeOffset.UnixEpoch,
            ActualEndTime: DateTimeOffset.UnixEpoch.AddYears(50));

        var bytes = WriteOne((ref NdrWriter w) => NdrOpcBatchSummaryCodec.Write(ref w, input));
        var back = ReadOne(bytes);

        await Assert.That(back.ActualStartTime).IsEqualTo(DateTimeOffset.UnixEpoch);
        await Assert.That(back.ActualEndTime).IsEqualTo(DateTimeOffset.UnixEpoch.AddYears(50));
        await Assert.That(back).IsEqualTo(input);
    }
}
