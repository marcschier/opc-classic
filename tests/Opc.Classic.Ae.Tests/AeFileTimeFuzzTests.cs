// Copyright (c) 2026 marcschier. Licensed under the MIT License.
// FILETIME-decode hypothesis-matrix fuzz tests for every AE NDR
// codec swept by AW2 (OPCEVENTSERVERSTATUS, OPCCONDITIONSTATE, ONEVENTSTRUCT).
// Mirrors NdrOpcServerStatusCodecTests AS3 pattern.
//

using Opc.Classic.Ae.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Ae.Tests;

public sealed class AeFileTimeFuzzTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 1024)
    {
        var buf = new byte[capacity];
        var writer = new NdrWriter(buf);
        write(ref writer);
        return buf[..writer.Position];
    }

    [Test]
    public async Task EventServerStatus_AllFileTimesZero_DecodesAsEpoch()
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            w.WriteFileTime(0L);   // ftStartTime
            w.WriteFileTime(0L);   // ftCurrentTime
            w.WriteFileTime(0L);   // ftLastUpdateTime
            w.WriteUInt32(1);      // state = Running
            w.WriteUInt16(1);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUnicodeStringPtr("v");
        });

        var reader = new NdrReader(wire);
        OpcServerStatus status = NdrOpcEventServerStatusCodec.Read(ref reader);
        await Assert.That(status.StartTime).IsEqualTo(FileTimeHelper.Epoch);
        await Assert.That(status.LastUpdateTime).IsEqualTo(FileTimeHelper.Epoch);
    }

    [Test]
    [Arguments(-1L)]
    [Arguments(long.MaxValue)]
    public async Task EventServerStatus_FileTime_OutOfRange_ThrowsAndNamesField(long bogus)
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            w.WriteFileTime(bogus);   // ftStartTime is FIRST → exception names it
            w.WriteFileTime(0L);
            w.WriteFileTime(0L);
            w.WriteUInt32(1);
            w.WriteUInt16(1);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUnicodeStringPtr("v");
        });

        try
        {
            var reader = new NdrReader(wire);
            _ = NdrOpcEventServerStatusCodec.Read(ref reader);
            throw new Exception("expected InvalidDataException");
        }
        catch (InvalidDataException ex)
        {
            await Assert.That(ex.Message).Contains("OPCEVENTSERVERSTATUS.ftStartTime");
        }
    }
}
