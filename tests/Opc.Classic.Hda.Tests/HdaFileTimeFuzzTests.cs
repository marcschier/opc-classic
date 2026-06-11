//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// FILETIME-decode hypothesis-matrix fuzz tests for the HDA
// NdrOpcHdaServerStatusCodec swept by AW2. Mirrors the AS3 pattern.
//

using System;
using System.IO;
using Opc.Classic;
using Opc.Classic.Hda.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Hda.Tests;

public sealed class HdaFileTimeFuzzTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 512)
    {
        var buf = new byte[capacity];
        var writer = new NdrWriter(buf);
        write(ref writer);
        return buf[..writer.Position];
    }

    [Test]
    public async Task HdaServerStatus_FileTime_Zero_DecodesAsEpoch()
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            w.WriteUInt32(1);                  // historian status = Up = Running
            // [out] FILETIME** pftCurrentTime: 4-byte unique-pointer referent
            // before each FILETIME on the wire (matches the OS COM proxy/stub
            // shape that the production codec now emits).
            _ = w.WriteReferentId();
            w.WriteFileTime(0L);               // ftCurrentTime
            _ = w.WriteReferentId();
            w.WriteFileTime(0L);               // ftStartTime
            w.WriteUInt16(1);                  // major
            w.WriteUInt16(0);                  // minor
            w.WriteUInt16(0);                  // build
            w.WriteUInt16(0);                  // reserved
            w.WriteUInt32(0);                  // maxReturnValues
            w.WriteUnicodeStringPtr(null);     // status string
            w.WriteUnicodeStringPtr("v");      // vendor
        });

        var reader = new NdrReader(wire);
        OpcServerStatus status = NdrOpcHdaServerStatusCodec.Read(ref reader);
        await Assert.That(status.CurrentTime).IsEqualTo(FileTimeHelper.Epoch);
        await Assert.That(status.StartTime).IsEqualTo(FileTimeHelper.Epoch);
    }

    [Test]
    [Arguments(-1L)]
    [Arguments(long.MaxValue)]
    public async Task HdaServerStatus_FileTime_OutOfRange_ThrowsAndNamesField(long bogus)
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            w.WriteUInt32(1);
            _ = w.WriteReferentId();
            w.WriteFileTime(bogus);            // ftCurrentTime FIRST → named
            _ = w.WriteReferentId();
            w.WriteFileTime(0L);
            w.WriteUInt16(1);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt32(0);
            w.WriteUnicodeStringPtr(null);
            w.WriteUnicodeStringPtr("v");
        });

        try
        {
            var reader = new NdrReader(wire);
            _ = NdrOpcHdaServerStatusCodec.Read(ref reader);
            throw new Exception("expected InvalidDataException");
        }
        catch (InvalidDataException ex)
        {
            await Assert.That(ex.Message).Contains("OPCHDA_SERVERSTATUS.ftCurrentTime");
        }
    }
}
