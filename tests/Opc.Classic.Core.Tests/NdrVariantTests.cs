//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Round-trip + invariant tests for the NDR VARIANT scalar wire format.
//

using System;
using System.IO;
using Opc.Classic;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class NdrVariantTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 128)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcVariant ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return r.ReadVariant();
    }

    [Test]
    public async Task Empty_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.Empty));
        // Header (16) + ULONG discriminator (4) + 0 body = 20 bytes total
        await Assert.That(bytes.Length).IsEqualTo(20);
        await Assert.That(ReadOne(bytes)).IsEqualTo(OpcVariant.Empty);
    }

    [Test]
    public async Task Null_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.Null));
        await Assert.That(ReadOne(bytes).Type).IsEqualTo(VarType.VT_NULL);
    }

    [Test]
    public async Task Int8_RoundTrips_Negative()
    {
        var input = OpcVariant.FromInt8(-1);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsInt8()).IsEqualTo((sbyte)-1);
    }

    [Test]
    public async Task UInt8_RoundTrips_MaxValue()
    {
        var input = OpcVariant.FromUInt8(255);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsUInt8()).IsEqualTo((byte)255);
    }

    [Test]
    public async Task Int16_RoundTrips()
    {
        var input = OpcVariant.FromInt16(-32768);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsInt16()).IsEqualTo((short)-32768);
    }

    [Test]
    public async Task UInt16_RoundTrips()
    {
        var input = OpcVariant.FromUInt16(0xCAFE);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsUInt16()).IsEqualTo((ushort)0xCAFE);
    }

    [Test]
    public async Task Boolean_RoundTrips_True()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromBoolean(true)));
        await Assert.That(ReadOne(bytes).AsBoolean()).IsTrue();
    }

    [Test]
    public async Task Boolean_RoundTrips_False()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromBoolean(false)));
        await Assert.That(ReadOne(bytes).AsBoolean()).IsFalse();
    }

    [Test]
    public async Task Boolean_WireForm_IsMinusOne_ForTrue()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromBoolean(true)));
        // 16 hdr + 4 ULONG discriminator + 2 VARIANT_BOOL body. TRUE = 0xFFFF.
        await Assert.That(bytes[20]).IsEqualTo((byte)0xFF);
        await Assert.That(bytes[21]).IsEqualTo((byte)0xFF);
    }

    [Test]
    public async Task Int32_RoundTrips()
    {
        var input = OpcVariant.FromInt32(unchecked((int)0xDEADBEEFu));
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsInt32()).IsEqualTo(unchecked((int)0xDEADBEEFu));
    }

    [Test]
    public async Task UInt32_RoundTrips()
    {
        var input = OpcVariant.FromUInt32(0xCAFEBABEu);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsUInt32()).IsEqualTo(0xCAFEBABEu);
    }

    [Test]
    public async Task Single_RoundTrips()
    {
        var input = OpcVariant.FromSingle(3.14159f);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsSingle()).IsEqualTo(3.14159f);
    }

    [Test]
    public async Task Double_RoundTrips()
    {
        var input = OpcVariant.FromDouble(2.71828182845904);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsDouble()).IsEqualTo(2.71828182845904);
    }

    [Test]
    public async Task Error_RoundTrips_Hresult()
    {
        var input = OpcVariant.FromError(unchecked((int)0xC0040001u));
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsError()).IsEqualTo(unchecked((int)0xC0040001u));
    }

    [Test]
    public async Task Int64_RoundTrips()
    {
        var input = OpcVariant.FromInt64(0x0102030405060708L);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsInt64()).IsEqualTo(0x0102030405060708L);
    }

    [Test]
    public async Task UInt64_RoundTrips()
    {
        var input = OpcVariant.FromUInt64(ulong.MaxValue);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsUInt64()).IsEqualTo(ulong.MaxValue);
    }

    [Test]
    public async Task Date_RoundTrips_AsOADate()
    {
        // OLE DATE is "days since 1899-12-30 as a double".
        var inputDt = new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc);
        var input = OpcVariant.FromDate(inputDt);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        await Assert.That(ReadOne(bytes).AsDate()).IsEqualTo(inputDt);
    }

    [Test]
    public async Task FileTime_RoundTrips()
    {
        const long ft = 0x01_DA_AB_CD_EF_01_23_45L;
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromFileTime(ft)));
        await Assert.That(ReadOne(bytes).AsFileTime()).IsEqualTo(ft);
    }

    [Test]
    public async Task Clsid_RoundTrips()
    {
        var g = new Guid("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromClsid(g)));
        await Assert.That(ReadOne(bytes).AsClsid()).IsEqualTo(g);
    }

    [Test]
    public async Task UnsupportedType_Throws_OnWrite()
    {
        Action action = () =>
        {
            var buf = new byte[64];
            var w = new NdrWriter(buf);
            w.WriteVariant(new OpcVariant(VarType.VT_DECIMAL, new byte[16]));
        };
        bool threw = false;
        try { action(); }
        catch (InvalidOperationException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Bstr_RoundTrips_Ascii()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
            w.WriteVariant(OpcVariant.FromString("hello")));
        await Assert.That(ReadOne(bytes).AsString()).IsEqualTo("hello");
    }

    [Test]
    public async Task Bstr_RoundTrips_Empty()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
            w.WriteVariant(OpcVariant.FromString(string.Empty)));
        await Assert.That(ReadOne(bytes).AsString()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Bstr_RoundTrips_NonAscii()
    {
        var input = "Ä-中文-🙂";
        var bytes = WriteOne((ref NdrWriter w) =>
            w.WriteVariant(OpcVariant.FromString(input)), capacity: 128);
        await Assert.That(ReadOne(bytes).AsString()).IsEqualTo(input);
    }

    [Test]
    public async Task Bstr_NullPayload_RoundTrips()
    {
        var bytes = WriteOne((ref NdrWriter w) =>
            w.WriteVariant(new OpcVariant(VarType.VT_BSTR, null)));
        var r = ReadOne(bytes);
        await Assert.That(r.Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(r.Boxed).IsNull();
    }

    [Test]
    public async Task Header_Layout_HasExpectedFixedBytes()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromInt32(0x12345678)));
        // 16 hdr + 4 ULONG discriminator + 4 Int32 body = 24 bytes total
        await Assert.That(bytes.Length).IsEqualTo(24);
        // clSize in quad-words (8-byte units) = ceil(24/8) = 3
        await Assert.That(BitConverter.ToUInt32(bytes, 0)).IsEqualTo(3u);
        // rpcReserved = 0
        await Assert.That(BitConverter.ToUInt32(bytes, 4)).IsEqualTo(0u);
        // vt = VT_I4 (3)
        await Assert.That(BitConverter.ToUInt16(bytes, 8)).IsEqualTo((ushort)3);
        // wReserved1/2/3 = 0
        await Assert.That(BitConverter.ToUInt16(bytes, 10)).IsEqualTo((ushort)0);
        await Assert.That(BitConverter.ToUInt16(bytes, 12)).IsEqualTo((ushort)0);
        await Assert.That(BitConverter.ToUInt16(bytes, 14)).IsEqualTo((ushort)0);
        // switch_type(ULONG) discriminator = vt as ULONG (per MS-OAUT non-encapsulated union)
        await Assert.That(BitConverter.ToUInt32(bytes, 16)).IsEqualTo(3u);
        // body
        await Assert.That(BitConverter.ToInt32(bytes, 20)).IsEqualTo(0x12345678);
    }

    [Test]
    public async Task Reader_TolerantToNonZeroRpcReserved()
    {
        // Per MS-OAUT §2.2.29.2, rpcReserved SHOULD be 0 but receivers MUST
        // tolerate any value. Matrikon Simulation has been observed sending
        // non-zero rpcReserved bytes in OPCITEMSTATE.vDataValue, so the reader
        // must accept them and continue with the wire body that follows.
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromInt32(42)));
        // Corrupt rpcReserved (bytes 4..7) — should not affect decode.
        bytes[4] = 0xFF;
        OpcVariant decoded = ReadOne(bytes);
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_I4);
        await Assert.That((int?)decoded.Boxed).IsEqualTo(42);
    }

    [Test]
    public async Task ByRef_Int32_RoundTrips()
    {
        var input = OpcVariant.FromByRef(VarType.VT_I4, 123456);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        var read = ReadOne(bytes);
        await Assert.That(read.Type).IsEqualTo((VarType)((ushort)VarType.VT_I4 | (ushort)VarType.VT_BYREF));
        await Assert.That((int?)read.Boxed).IsEqualTo(123456);
    }

    [Test]
    public async Task ByRef_Double_RoundTrips()
    {
        var input = OpcVariant.FromByRef(VarType.VT_R8, 123.25d);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input));
        var read = ReadOne(bytes);
        await Assert.That(read.Type).IsEqualTo((VarType)((ushort)VarType.VT_R8 | (ushort)VarType.VT_BYREF));
        await Assert.That((double?)read.Boxed).IsEqualTo(123.25d);
    }

    [Test]
    public async Task ByRef_Bstr_RoundTrips()
    {
        var input = OpcVariant.FromByRef(VarType.VT_BSTR, "byref-text");
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input), capacity: 256);
        var read = ReadOne(bytes);
        await Assert.That(read.Type).IsEqualTo((VarType)((ushort)VarType.VT_BSTR | (ushort)VarType.VT_BYREF));
        await Assert.That((string?)read.Boxed).IsEqualTo("byref-text");
    }

    [Test]
    public async Task ByRef_Variant_RoundTrips()
    {
        var nested = OpcVariant.FromString("nested");
        var input = OpcVariant.FromByRef(VarType.VT_VARIANT, nested);
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(input), capacity: 256);
        var read = ReadOne(bytes);
        await Assert.That(read.Type).IsEqualTo((VarType)((ushort)VarType.VT_VARIANT | (ushort)VarType.VT_BYREF));
        await Assert.That(read.AsVariant()).IsEqualTo(nested);
    }

    [Test]
    public async Task VariantArray_WithMixedNestedValues_RoundTrips()
    {
        var array = OpcSafeArray.OfVariant(new[]
        {
            OpcVariant.FromInt32(7),
            OpcVariant.FromString("tag"),
            OpcVariant.FromDouble(2.5d),
        });
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromSafeArray(array)), capacity: 512);
        OpcSafeArray? readArray = ReadOne(bytes).AsSafeArray();
        var values = (OpcVariant[])readArray!.Data;
        await Assert.That(readArray.Features).IsEqualTo(SafeArrayFeatures.HaveVartype | SafeArrayFeatures.Variant);
        await Assert.That(values[0].AsInt32()).IsEqualTo(7);
        await Assert.That(values[1].AsString()).IsEqualTo("tag");
        await Assert.That(values[2].AsDouble()).IsEqualTo(2.5d);
    }

    [Test]
    public async Task Record_WithRegisteredRecordInfo_RoundTrips()
    {
        var info = new OpcRecordInfo(
            new Guid("6D88A608-407A-4F1F-A8F0-AE2B10BBA875"),
            "SampleRecord",
            new[]
            {
                new OpcRecordField("Id", VarType.VT_I4),
                new OpcRecordField("Name", VarType.VT_BSTR),
                new OpcRecordField("Value", VarType.VT_R8),
            });
        RecordInfoRegistry.Register(info);
        try
        {
            var record = new OpcRecordValue(info, new object?[] { 42, "Pump", 9.75d });
            var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromRecord(record)), capacity: 512);
            OpcRecordValue? read = ReadOne(bytes).AsRecord();
            await Assert.That(read).IsEqualTo(record);
            await Assert.That((int?)read!.Values[0]).IsEqualTo(42);
            await Assert.That((string?)read.Values[1]).IsEqualTo("Pump");
            await Assert.That((double?)read.Values[2]).IsEqualTo(9.75d);
        }
        finally
        {
            _ = RecordInfoRegistry.Unregister(info.Id);
        }
    }

    [Test]
    public async Task VariantNesting_BeyondLimit_Throws()
    {
        OpcVariant value = OpcVariant.FromInt32(1);
        for (int i = 0; i <= NdrVariantExtensions.MaxVariantRecursionDepth; i++)
        {
            value = OpcVariant.FromVariant(value);
        }

        bool threw = false;
        try
        {
            _ = WriteOne((ref NdrWriter w) => w.WriteVariant(value), capacity: 4096);
        }
        catch (InvalidDataException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }
}
