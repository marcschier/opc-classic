//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Track AW3: FILETIME-decode hypothesis-matrix fuzz tests for the Batch
// NdrOpcBatchSummaryCodec + NdrOpcBatchSummaryFilterCodec swept by AW2.
//

using System;
using System.IO;
using Opc.Classic;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Batch.Tests;

public sealed class BatchFileTimeFuzzTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1_024)
    {
        var buf = new byte[capacity];
        var writer = new NdrWriter(buf);
        write(ref writer);
        return buf[..writer.Position];
    }

    [Test]
    [Arguments(-1L)]
    [Arguments(long.MaxValue)]
    public async Task BatchSummary_ActualStartTime_OutOfRange_ThrowsAndNamesField(long bogus)
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            // Match NdrOpcBatchSummaryCodec.Read layout (strings + scalars then 2 FILETIMEs)
            w.WriteUnicodeStringPtr("id");
            w.WriteUnicodeStringPtr("desc");
            w.WriteUnicodeStringPtr("item");
            w.WriteUnicodeStringPtr("recipe");
            w.WriteSingle(0f);                   // batchSize
            w.WriteUnicodeStringPtr(null);       // engineeringUnits
            w.WriteUnicodeStringPtr(null);       // execState
            w.WriteUnicodeStringPtr(null);       // execMode
            w.WriteFileTime(bogus);              // ftActualStartTime FIRST → named
            w.WriteFileTime(0L);                 // ftActualEndTime
        });

        try
        {
            var reader = new NdrReader(wire);
            _ = NdrOpcBatchSummaryCodec.Read(ref reader);
            throw new Exception("expected InvalidDataException");
        }
        catch (InvalidDataException ex)
        {
            await Assert.That(ex.Message).Contains("OPCBATCHSUMMARY.ftActualStartTime");
        }
    }

    [Test]
    [Arguments(-1L)]
    [Arguments(long.MaxValue)]
    public async Task BatchSummaryFilter_MinStartTime_OutOfRange_ThrowsAndNamesField(long bogus)
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            // Match NdrOpcBatchSummaryFilterCodec.Read layout (9 strings/scalars then 4 FILETIMEs)
            w.WriteUnicodeStringPtr("id");
            w.WriteUnicodeStringPtr("desc");
            w.WriteUnicodeStringPtr("item");
            w.WriteUnicodeStringPtr("recipe");
            w.WriteSingle(0f);                   // minBatchSize
            w.WriteSingle(0f);                   // maxBatchSize
            w.WriteUnicodeStringPtr(null);       // engineeringUnits
            w.WriteUnicodeStringPtr(null);       // execState
            w.WriteUnicodeStringPtr(null);       // execMode
            w.WriteFileTime(bogus);              // ftMinStartTime FIRST → named
            w.WriteFileTime(0L);                 // ftMaxStartTime
            w.WriteFileTime(0L);                 // ftMinEndTime
            w.WriteFileTime(0L);                 // ftMaxEndTime
        });

        try
        {
            var reader = new NdrReader(wire);
            _ = NdrOpcBatchSummaryFilterCodec.Read(ref reader);
            throw new Exception("expected InvalidDataException");
        }
        catch (InvalidDataException ex)
        {
            await Assert.That(ex.Message).Contains("OPCBATCHSUMMARYFILTER.ftMinStartTime");
        }
    }
}
