//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class OpcVariantConverterTests
{
    [Test]
    public async Task FromObject_Null_BecomesVtNull()
    {
        var v = OpcVariantConverter.FromObject(null);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_NULL);
    }

    [Test]
    public async Task FromObject_Bool_BecomesVtBool()
    {
        var v = OpcVariantConverter.FromObject(true);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_BOOL);
        await Assert.That(v.AsBoolean()).IsTrue();
    }

    [Test]
    public async Task FromObject_Int32_BecomesVtI4()
    {
        var v = OpcVariantConverter.FromObject(42);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_I4);
        await Assert.That(v.AsInt32()).IsEqualTo(42);
    }

    [Test]
    public async Task FromObject_Double_BecomesVtR8()
    {
        var v = OpcVariantConverter.FromObject(3.14);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_R8);
        await Assert.That(v.AsDouble()).IsEqualTo(3.14);
    }

    [Test]
    public async Task FromObject_String_BecomesVtBstr()
    {
        var v = OpcVariantConverter.FromObject("hello");
        await Assert.That(v.Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(v.AsString()).IsEqualTo("hello");
    }

    [Test]
    public async Task FromObject_Guid_BecomesVtClsid()
    {
        var g = Guid.NewGuid();
        var v = OpcVariantConverter.FromObject(g);
        await Assert.That(v.Type).IsEqualTo(VarType.VT_CLSID);
        await Assert.That(v.AsClsid()).IsEqualTo(g);
    }

    [Test]
    public async Task FromObject_AllNumericTypes_MapCorrectly()
    {
        await Assert.That(OpcVariantConverter.FromObject((sbyte)-1).Type).IsEqualTo(VarType.VT_I1);
        await Assert.That(OpcVariantConverter.FromObject((byte)255).Type).IsEqualTo(VarType.VT_UI1);
        await Assert.That(OpcVariantConverter.FromObject((short)1).Type).IsEqualTo(VarType.VT_I2);
        await Assert.That(OpcVariantConverter.FromObject((ushort)2).Type).IsEqualTo(VarType.VT_UI2);
        await Assert.That(OpcVariantConverter.FromObject((uint)3).Type).IsEqualTo(VarType.VT_UI4);
        await Assert.That(OpcVariantConverter.FromObject(4L).Type).IsEqualTo(VarType.VT_I8);
        await Assert.That(OpcVariantConverter.FromObject(5UL).Type).IsEqualTo(VarType.VT_UI8);
        await Assert.That(OpcVariantConverter.FromObject(6.5f).Type).IsEqualTo(VarType.VT_R4);
    }

    [Test]
    public async Task FromObject_UnsupportedType_Throws()
    {
        bool threw = false;
        try
        {
            OpcVariantConverter.FromObject(new[] { 1, 2, 3 });  // arrays not yet supported
        }
        catch (ArgumentException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task ToObject_ReturnsTheBoxedValue()
    {
        var v = OpcVariant.FromInt32(42);
        await Assert.That(OpcVariantConverter.ToObject(v)).IsEqualTo((object)42);
    }

    [Test]
    public async Task ToObject_Null_ReturnsNull()
    {
        await Assert.That(OpcVariantConverter.ToObject(OpcVariant.Null)).IsNull();
        await Assert.That(OpcVariantConverter.ToObject(OpcVariant.Empty)).IsNull();
    }

    [Test]
    public async Task CanConvert_KnownTypes_ReturnsTrue()
    {
        await Assert.That(OpcVariantConverter.CanConvert(null)).IsTrue();
        await Assert.That(OpcVariantConverter.CanConvert("text")).IsTrue();
        await Assert.That(OpcVariantConverter.CanConvert(42)).IsTrue();
        await Assert.That(OpcVariantConverter.CanConvert(3.14)).IsTrue();
        await Assert.That(OpcVariantConverter.CanConvert(true)).IsTrue();
        await Assert.That(OpcVariantConverter.CanConvert(Guid.NewGuid())).IsTrue();
    }

    [Test]
    public async Task CanConvert_UnknownType_ReturnsFalse()
    {
        await Assert.That(OpcVariantConverter.CanConvert(new[] { 1, 2 })).IsFalse();
        await Assert.That(OpcVariantConverter.CanConvert(new object())).IsFalse();
    }

    [Test]
    public async Task RoundTrip_ObjectToVariantToObject_Preserves()
    {
        object[] inputs =
        {
            42, 3.14, "hello", true, (sbyte)-1, (byte)255, 9.5f, 12345L,
        };
        foreach (var input in inputs)
        {
            var v = OpcVariantConverter.FromObject(input);
            var back = OpcVariantConverter.ToObject(v);
            await Assert.That(back).IsEqualTo(input);
        }
    }
}
