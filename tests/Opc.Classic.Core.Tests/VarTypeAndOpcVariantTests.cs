//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class VarTypeAndOpcVariantTests
{
    // ---- VarType enum + mask helpers ----

    private static int IntValue(VarType vt) => (int)vt;

    [Test]
    public async Task VarType_BasicCodes_MatchOaidl()
    {
        await Assert.That(IntValue(VarType.VT_EMPTY)).IsEqualTo(0);
        await Assert.That(IntValue(VarType.VT_NULL)).IsEqualTo(1);
        await Assert.That(IntValue(VarType.VT_I4)).IsEqualTo(3);
        await Assert.That(IntValue(VarType.VT_R8)).IsEqualTo(5);
        await Assert.That(IntValue(VarType.VT_BSTR)).IsEqualTo(8);
        await Assert.That(IntValue(VarType.VT_BOOL)).IsEqualTo(11);
    }

    [Test]
    public async Task VarType_ModifierFlags_Bits()
    {
        await Assert.That(IntValue(VarType.VT_VECTOR)).IsEqualTo(0x1000);
        await Assert.That(IntValue(VarType.VT_ARRAY)).IsEqualTo(0x2000);
        await Assert.That(IntValue(VarType.VT_BYREF)).IsEqualTo(0x4000);
        await Assert.That(IntValue(VarType.VT_RESERVED)).IsEqualTo(0x8000);
    }

    [Test]
    public async Task VarTypeMask_BaseOf_StripsModifiers()
    {
        var combined = (VarType)((ushort)VarType.VT_R8 | (ushort)VarType.VT_ARRAY | (ushort)VarType.VT_BYREF);
        await Assert.That(VarTypeMask.BaseOf(combined)).IsEqualTo(VarType.VT_R8);
    }

    [Test]
    public async Task VarTypeMask_DetectsFlags()
    {
        var arrayInt = (VarType)((ushort)VarType.VT_I4 | (ushort)VarType.VT_ARRAY);
        await Assert.That(VarTypeMask.IsArray(arrayInt)).IsTrue();
        await Assert.That(VarTypeMask.IsByRef(arrayInt)).IsFalse();
        await Assert.That(VarTypeMask.IsVector(arrayInt)).IsFalse();

        var byrefDouble = (VarType)((ushort)VarType.VT_R8 | (ushort)VarType.VT_BYREF);
        await Assert.That(VarTypeMask.IsByRef(byrefDouble)).IsTrue();
        await Assert.That(VarTypeMask.IsArray(byrefDouble)).IsFalse();
    }

    // ---- OpcVariant factory + accessor round-trip ----

    [Test]
    public async Task Empty_HasVtEmpty_AndNullBoxed()
    {
        await Assert.That(OpcVariant.Empty.Type).IsEqualTo(VarType.VT_EMPTY);
        await Assert.That(OpcVariant.Empty.Boxed).IsNull();
        await Assert.That(OpcVariant.Empty.IsEmpty).IsTrue();
    }

    [Test]
    public async Task Null_HasVtNull()
    {
        await Assert.That(OpcVariant.Null.Type).IsEqualTo(VarType.VT_NULL);
        await Assert.That(OpcVariant.Null.IsEmpty).IsTrue();
    }

    [Test]
    public async Task FromInt32_RoundTrips()
    {
        var v = OpcVariant.FromInt32(42);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_I4);
        await Assert.That(v.AsInt32()).IsEqualTo(42);
        await Assert.That(v.AsDouble()).IsNull();
    }

    [Test]
    public async Task FromDouble_RoundTrips()
    {
        var v = OpcVariant.FromDouble(3.14159);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_R8);
        await Assert.That(v.AsDouble()).IsEqualTo(3.14159);
        await Assert.That(v.AsInt32()).IsNull();
    }

    [Test]
    public async Task FromString_RoundTrips()
    {
        var v = OpcVariant.FromString("hello");
        await Assert.That(v.Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(v.AsString()).IsEqualTo("hello");
    }

    [Test]
    public async Task FromBoolean_RoundTrips_Both()
    {
        var vt = OpcVariant.FromBoolean(true);
        var vf = OpcVariant.FromBoolean(false);
        await Assert.That(vt.Type).IsEqualTo(VarType.VT_BOOL);
        await Assert.That(vt.AsBoolean()).IsTrue();
        await Assert.That(vf.AsBoolean()).IsFalse();
    }

    [Test]
    public async Task FromInt8_Through_UInt64_RoundTrips()
    {
        await Assert.That(OpcVariant.FromInt8(-1).AsInt8()).IsEqualTo((sbyte)-1);
        await Assert.That(OpcVariant.FromUInt8(255).AsUInt8()).IsEqualTo((byte)255);
        await Assert.That(OpcVariant.FromInt16(-32768).AsInt16()).IsEqualTo((short)-32768);
        await Assert.That(OpcVariant.FromUInt16(65535).AsUInt16()).IsEqualTo((ushort)65535);
        await Assert.That(OpcVariant.FromUInt32(0xDEADBEEFu).AsUInt32()).IsEqualTo(0xDEADBEEFu);
        await Assert.That(OpcVariant.FromInt64(long.MinValue).AsInt64()).IsEqualTo(long.MinValue);
        await Assert.That(OpcVariant.FromUInt64(ulong.MaxValue).AsUInt64()).IsEqualTo(ulong.MaxValue);
    }

    [Test]
    public async Task FromSingle_RoundTrips()
    {
        var v = OpcVariant.FromSingle(2.5f);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_R4);
        await Assert.That(v.AsSingle()).IsEqualTo(2.5f);
    }

    [Test]
    public async Task FromDate_RoundTrips()
    {
        var dt = new DateTime(2026, 5, 22, 8, 0, 0, DateTimeKind.Utc);
        var v = OpcVariant.FromDate(dt);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_DATE);
        await Assert.That(v.AsDate()).IsEqualTo(dt);
    }

    [Test]
    public async Task FromFileTime_RoundTrips()
    {
        const long ft = 0x01_DA_AB_CD_EF_01_23_45L;
        var v = OpcVariant.FromFileTime(ft);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_FILETIME);
        await Assert.That(v.AsFileTime()).IsEqualTo(ft);
    }

    [Test]
    public async Task FromError_RoundTrips()
    {
        var v = OpcVariant.FromError(unchecked((int)0xC0040001u));
        await Assert.That(v.Type).IsEqualTo(VarType.VT_ERROR);
        await Assert.That(v.AsError()).IsEqualTo(unchecked((int)0xC0040001u));
    }

    [Test]
    public async Task FromClsid_RoundTrips()
    {
        var g = new Guid("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        var v = OpcVariant.FromClsid(g);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_CLSID);
        await Assert.That(v.AsClsid()).IsEqualTo(g);
    }

    [Test]
    public async Task TypedAccessors_ReturnNullOnTypeMismatch()
    {
        var v = OpcVariant.FromInt32(42);
        await Assert.That(v.AsDouble()).IsNull();
        await Assert.That(v.AsString()).IsNull();
        await Assert.That(v.AsBoolean()).IsNull();
        await Assert.That(v.AsClsid()).IsNull();
    }

    [Test]
    public async Task Equality_IsValueBased()
    {
        var a = OpcVariant.FromInt32(7);
        var b = OpcVariant.FromInt32(7);
        var c = OpcVariant.FromInt32(8);
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a).IsNotEqualTo(c);
    }
}
