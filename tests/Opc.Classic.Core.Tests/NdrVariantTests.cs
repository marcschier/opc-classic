//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Round-trip + invariant tests for the NDR VARIANT scalar wire format.
//

using System;
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
        // Header (16 bytes) + 0 body = 16 bytes total
        await Assert.That(bytes.Length).IsEqualTo(16);
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
        // VARIANT_BOOL TRUE is -1 (0xFFFF as USHORT)
        await Assert.That(bytes[16]).IsEqualTo((byte)0xFF);
        await Assert.That(bytes[17]).IsEqualTo((byte)0xFF);
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
        // Header (16) + Int32 body (4) = 20
        await Assert.That(bytes.Length).IsEqualTo(20);
        // cbSize = remaining bytes after cbSize itself = 16 + 4 - 8 = 12
        await Assert.That(BitConverter.ToUInt32(bytes, 0)).IsEqualTo(12u);
        // rpcReserved = 0
        await Assert.That(BitConverter.ToUInt32(bytes, 4)).IsEqualTo(0u);
        // vt = VT_I4 (3)
        await Assert.That(BitConverter.ToUInt16(bytes, 8)).IsEqualTo((ushort)3);
        // wReserved1/2/3 = 0
        await Assert.That(BitConverter.ToUInt16(bytes, 10)).IsEqualTo((ushort)0);
        await Assert.That(BitConverter.ToUInt16(bytes, 12)).IsEqualTo((ushort)0);
        await Assert.That(BitConverter.ToUInt16(bytes, 14)).IsEqualTo((ushort)0);
        // body
        await Assert.That(BitConverter.ToInt32(bytes, 16)).IsEqualTo(0x12345678);
    }

    [Test]
    public async Task Reader_RejectsNonZeroRpcReserved()
    {
        var bytes = WriteOne((ref NdrWriter w) => w.WriteVariant(OpcVariant.FromInt32(42)));
        // Corrupt rpcReserved (bytes 4..7)
        bytes[4] = 0xFF;
        bool threw = false;
        try { ReadOne(bytes); }
        catch (System.IO.InvalidDataException) { threw = true; }
        await Assert.That(threw).IsTrue();
    }
}
