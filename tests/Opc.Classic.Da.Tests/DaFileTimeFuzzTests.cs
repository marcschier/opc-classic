// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
// FILETIME-decode hypothesis-matrix fuzz tests for every Da NDR
// codec swept by AW2. Mirror the AS3 pattern from NdrOpcServerStatusCodecTests:
// inject raw long values via NdrWriter.WriteFileTime that bypass the writer-
// side DateTimeOffset validation, then assert the reader either decodes them
// cleanly (0 = Epoch is valid) or throws InvalidDataException with a named-
// field message and wire-context hex window.
//

using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests;

public sealed class DaFileTimeFuzzTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var writer = new NdrWriter(buf);
        write(ref writer);
        return buf[..writer.Position];
    }

    [Test]
    public async Task ItemState_FileTime_Zero_Yields1601Epoch()
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            w.WriteUInt32(42);                   // hClient
            w.WriteFileTime(0L);                 // ftTimeStamp (Epoch)
            w.WriteUInt16(0xC0);                 // wQuality
            w.WriteUInt16(0);                    // wReserved
            // VARIANT body: VT_I4 with payload 99
            w.WriteUniquePointerReferent(true); NdrVariantExtensions.WriteVariant(ref w, OpcVariant.FromInt32(99));
        });

        var reader = new NdrReader(wire);
        OpcItemState state = NdrOpcItemStateCodec.Read(ref reader);
        await Assert.That(state.Timestamp).IsEqualTo(FileTimeHelper.Epoch);
        await Assert.That(state.ClientHandle).IsEqualTo(42);
    }

    [Test]
    [Arguments(-1L)]
    [Arguments(long.MinValue)]
    [Arguments(long.MaxValue)]
    public async Task ItemState_FileTime_OutOfRange_ThrowsInvalidDataException(long bogusFileTime)
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            w.WriteUInt32(0);
            w.WriteFileTime(bogusFileTime);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUniquePointerReferent(true); NdrVariantExtensions.WriteVariant(ref w, OpcVariant.Empty);
        });

        await Assert.That(() =>
        {
            var reader = new NdrReader(wire);
            _ = NdrOpcItemStateCodec.Read(ref reader);
        }).Throws<InvalidDataException>();
    }

    [Test]
    public async Task ItemState_FileTime_ErrorMessageNamesField()
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            w.WriteUInt32(0);
            w.WriteFileTime(-1L);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUniquePointerReferent(true); NdrVariantExtensions.WriteVariant(ref w, OpcVariant.Empty);
        });

        try
        {
            var reader = new NdrReader(wire);
            _ = NdrOpcItemStateCodec.Read(ref reader);
            throw new Exception("expected InvalidDataException");
        }
        catch (InvalidDataException ex)
        {
            await Assert.That(ex.Message).Contains("OPCITEMSTATE.ftTimeStamp");
            await Assert.That(ex.Message).Contains("Wire context");
        }
    }

    [Test]
    public async Task ItemVqt_FileTime_OutOfRange_WithBTimestampSet_ThrowsInvalidDataException()
    {
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            // VARIANT body
            NdrVariantExtensions.WriteVariant(ref w, OpcVariant.FromInt32(7));
            w.WriteInt32(1);                     // bQuality = true
            w.WriteUInt16(0xC0);                 // wQuality
            w.WriteUInt16(0);                    // wReserved
            w.WriteInt32(1);                     // bTimestamp = true (forces decode)
            w.WriteUInt32(0);                    // dwReserved
            w.WriteFileTime(long.MaxValue);      // bogus timestamp
        });

        try
        {
            var reader = new NdrReader(wire);
            _ = NdrOpcItemVqtCodec.Read(ref reader);
            throw new Exception("expected InvalidDataException");
        }
        catch (InvalidDataException ex)
        {
            await Assert.That(ex.Message).Contains("OPCITEMVQT.ftTimeStamp");
        }
    }

    [Test]
    public async Task ItemVqt_FileTime_OutOfRange_WithBTimestampClear_DoesNotThrow()
    {
        // When bTimestamp is 0 the codec MUST NOT decode the raw FILETIME at all
        // (consumer should never look at it). Verify the bogus value is harmless.
        byte[] wire = WriteOne((ref NdrWriter w) =>
        {
            NdrVariantExtensions.WriteVariant(ref w, OpcVariant.FromInt32(7));
            w.WriteInt32(0);                     // bQuality = false
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteInt32(0);                     // bTimestamp = false (skip)
            w.WriteUInt32(0);
            w.WriteFileTime(long.MaxValue);      // bogus, but ignored
        });

        var reader = new NdrReader(wire);
        OpcItemVqt vqt = NdrOpcItemVqtCodec.Read(ref reader);
        await Assert.That(vqt.Timestamp).IsEqualTo((DateTimeOffset?)null);
    }
}
