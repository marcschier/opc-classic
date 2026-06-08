//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class CoreValueCodecRegressionTests
{
    [Test]
    public async Task ComVersion_DefaultConstructor_UsesDcomFiveFour()
    {
        var version = new ComVersion();

        await Assert.That(version.MajorVersion).IsEqualTo(5);
        await Assert.That(version.MinorVersion).IsEqualTo(4);
    }

    [Test]
    public async Task ComVersion_CustomConstructor_PreservesComponents()
    {
        var version = new ComVersion(7, 9);

        await Assert.That(version.MajorVersion).IsEqualTo(7);
        await Assert.That(version.MinorVersion).IsEqualTo(9);
    }

    [Test]
    public async Task Currency_StringWithFraction_SplitsUnitsAndFractionalUnits()
    {
        var currency = new Currency("12.0340");

        await Assert.That(currency.Units).IsEqualTo(12);
        await Assert.That(currency.FractionalUnits).IsEqualTo(340);
    }

    [Test]
    public async Task Currency_StringWithoutLeadingUnit_NormalizesUnits()
    {
        var currency = new Currency(".5000");

        await Assert.That(currency.Units).IsEqualTo(0);
        await Assert.That(currency.FractionalUnits).IsEqualTo(5000);
    }

    [Test]
    public async Task Currency_StringWithoutFraction_NormalizesZeroFraction()
    {
        var currency = new Currency("42.");

        await Assert.That(currency.Units).IsEqualTo(42);
        await Assert.That(currency.FractionalUnits).IsEqualTo(0);
    }

    [Test]
    public async Task Currency_TwoPartConstructor_PreservesSignedParts()
    {
        var currency = new Currency(-7, -1250);

        await Assert.That(currency.Units).IsEqualTo(-7);
        await Assert.That(currency.FractionalUnits).IsEqualTo(-1250);
    }

    [Test]
    public async Task Scode_OkSingleton_UsesZeroHresult()
    {
        await Assert.That(Scode.Ok.ErrorCode).IsEqualTo(0);
    }

    [Test]
    public async Task Scode_ErrorCodeConstructor_PreservesSignedHresult()
    {
        var scode = new Scode(ErrorCode.E_INVALIDARG);

        await Assert.That(scode.ErrorCode).IsEqualTo(unchecked((int)ErrorCode.E_INVALIDARG));
    }

    [Test]
    public async Task NdrException_ReasonConstructor_PreservesMessageAndReason()
    {
        var exception = new NdrException(NdrException.InvalidConformance, NdrException.InvalidArrayConformance);

        await Assert.That(exception.Message).IsEqualTo(NdrException.InvalidConformance);
        await Assert.That(exception.Reason).IsEqualTo(NdrException.InvalidArrayConformance);
        await Assert.That(exception.GetReason()).IsEqualTo(NdrException.InvalidArrayConformance);
    }

    [Test]
    public async Task NdrException_InnerConstructor_PreservesInnerException()
    {
        var inner = new InvalidOperationException("inner failure");
        var exception = new NdrException("outer failure", inner);

        await Assert.That(exception.Message).IsEqualTo("outer failure");
        await Assert.That(exception.InnerException).IsEqualTo(inner);
    }

    [Test]
    public async Task ComString_BstrConstructor_ExposesBstrTypeAndValue()
    {
        var value = new ComString("Alpha");

        await Assert.That(value.Type).IsEqualTo(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
        await Assert.That(value.String).IsEqualTo("Alpha");
    }

    [Test]
    public async Task ComString_NullString_NormalizesToEmptyBstr()
    {
        var value = new ComString(null!);

        await Assert.That(value.Type).IsEqualTo(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
        await Assert.That(value.String).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ComString_LpwstrConstructor_UsesLpwstrTypeWithoutVariantWrappers()
    {
        var value = new ComString("Wide", InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR);

        await Assert.That(value.Type).IsEqualTo(InteropFlags.FLAG_REPRESENTATION_STRING_LPWSTR);
        await Assert.That(value.String).IsEqualTo("Wide");
        await Assert.That(value.Variant is null).IsTrue();
        await Assert.That(value.VariantByRef is null).IsTrue();
    }

    [Test]
    public async Task ComString_InvalidRepresentation_ThrowsArgumentException()
    {
        var exception = Capture<ArgumentException>(() => _ = new ComString("bad", 0x7F));

        await Assert.That(exception.ParamName).IsEqualTo("type");
    }

    [Test]
    public async Task ComString_BstrVariantWrappers_ExposeByValueAndByRefVartypes()
    {
        var value = new ComString("Wrapped");

        await Assert.That(value.Variant.Type).IsEqualTo(VariantType.VT_BSTR);
        await Assert.That(value.Variant.ObjectAsString2).IsEqualTo("Wrapped");
        await Assert.That(value.VariantByRef.Type).IsEqualTo(VariantType.VT_BYREF_VT_BSTR);
        await Assert.That(value.VariantByRef.ObjectAsString2).IsEqualTo("Wrapped");
    }

    [Test]
    public async Task Variant_Int32Constructor_ExposesI4Value()
    {
        var variant = new Variant(123456);

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_I4);
        await Assert.That(variant.ObjectAsInt).IsEqualTo(123456);
        await Assert.That(variant.IsByRef).IsFalse();
        await Assert.That(variant.IsArray).IsFalse();
    }

    [Test]
    public async Task Variant_UInt32Constructor_ExposesUi4Value()
    {
        var variant = new Variant(0xAABBCCDDu);

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_UI4);
        await Assert.That(variant.ObjectAsUnsigned).IsEqualTo(0xAABBCCDDu);
    }

    [Test]
    public async Task Variant_DoubleConstructor_ExposesR8Value()
    {
        var variant = new Variant(1234.5d);

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_R8);
        await Assert.That(variant.ObjectAsDouble).IsEqualTo(1234.5d);
    }

    [Test]
    public async Task Variant_BooleanConstructor_UsesVariantBoolTypeAndFlag()
    {
        var variant = new Variant(true);

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_BOOL);
        await Assert.That(variant.ObjectAsBoolean).IsTrue();
        await Assert.That(variant.Flag).IsEqualTo(InteropFlags.FLAG_REPRESENTATION_VARIANT_BOOL);
    }

    [Test]
    public async Task Variant_CharConstructor_UsesI1Vartype()
    {
        var variant = new Variant('Z');

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_I1);
        await Assert.That(variant.ObjectAsChar).IsEqualTo('Z');
    }

    [Test]
    public async Task Variant_StringConstructor_ExposesBstrValue()
    {
        var variant = new Variant("Opc");

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_BSTR);
        await Assert.That(variant.ObjectAsString.Type).IsEqualTo(InteropFlags.FLAG_REPRESENTATION_STRING_BSTR);
        await Assert.That(variant.ObjectAsString2).IsEqualTo("Opc");
    }

    [Test]
    public async Task Variant_NullFactory_ExposesVtNullAndZeroPayload()
    {
        Variant variant = Variant.CreateNULL();

        await Assert.That(variant.IsNull).IsTrue();
        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_NULL);
        await Assert.That(variant.Object).IsEqualTo(0);
    }

    [Test]
    public async Task Variant_OptionalParamFactory_UsesParamNotFoundScode()
    {
        Variant variant = Variant.CreateOPTIONAL_PARAM();

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_ERROR);
        await Assert.That(variant.ObjectAsSCODE).IsEqualTo(unchecked((int)ErrorCode.DISP_E_PARAMNOTFOUND));
    }

    [Test]
    public async Task Variant_ByRefInt32Constructor_SetsByRefVartype()
    {
        var variant = new Variant(99, isByRef: true);

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_BYREF_VT_I4);
        await Assert.That(variant.IsByRef).IsTrue();
        await Assert.That(variant.ObjectAsInt).IsEqualTo(99);
    }

    [Test]
    public async Task Variant_ScodeConstructor_ExposesErrorVartype()
    {
        var variant = new Variant(new Scode(ErrorCode.E_NOINTERFACE));

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_ERROR);
        await Assert.That(variant.ObjectAsSCODE).IsEqualTo(unchecked((int)ErrorCode.E_NOINTERFACE));
    }

    [Test]
    public async Task Variant_DateConstructor_ExposesDateValue()
    {
        var value = new DateTime(2026, 6, 7, 12, 30, 0, DateTimeKind.Utc);
        var variant = new Variant(value);

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_DATE);
        await Assert.That(variant.ObjectAsDate).IsEqualTo(value);
    }

    [Test]
    public async Task Variant_CurrencyConstructor_ExposesCurrencyValue()
    {
        var variant = new Variant(new Currency(18, 2500));
        var currency = (Currency)variant.Object;

        await Assert.That(variant.Type).IsEqualTo(VariantType.VT_CY);
        await Assert.That(currency.Units).IsEqualTo(18);
        await Assert.That(currency.FractionalUnits).IsEqualTo(2500);
    }

    [Test]
    public async Task ComArray_TemplateConstructor_ExposesConformantArrayMetadata()
    {
        var array = new ComArray(typeof(ComString), [3], 1, isConformant: true);

        await Assert.That(array.ArrayType).IsEqualTo(typeof(ComString));
        await Assert.That(array.Dimensions).IsEqualTo(1);
        await Assert.That(array.UpperBounds[0]).IsEqualTo(3);
        await Assert.That(array.Conformant).IsTrue();
        await Assert.That(array.Varying).IsFalse();
    }

    [Test]
    public async Task Struct_AddMember_AppendsAndReportsSize()
    {
        var structure = new Struct();

        structure.AddMember(7);
        structure.AddMember("tail");

        await Assert.That(structure.Size).IsEqualTo(2);
        await Assert.That(structure.GetMember(0)).IsEqualTo(7);
        await Assert.That(structure.GetMember(1)).IsEqualTo("tail");
    }

    [Test]
    public async Task Struct_AddMemberWithIndex_InsertsAtPosition()
    {
        var structure = new Struct();

        structure.AddMember(1);
        structure.AddMember(1, 2);

        await Assert.That(structure.Size).IsEqualTo(2);
        await Assert.That(structure.GetMember(0)).IsEqualTo(1);
        await Assert.That(structure.GetMember(1)).IsEqualTo(2);
    }

    [Test]
    public async Task Struct_AddMember_NullMember_StoresZeroSentinel()
    {
        var structure = new Struct();

        structure.AddMember(null);

        await Assert.That(structure.Size).IsEqualTo(1);
        await Assert.That(structure.GetMember(0)).IsEqualTo(0);
    }

    [Test]
    public async Task Struct_AddNonArrayAfterTrailingArray_ThrowsArrayAtEndInteropException()
    {
        var structure = new Struct();
        structure.AddMember(new ComArray(new[] { new ComString("x") }, isConformant: true));

        InteropException exception = Capture<InteropException>(() => structure.AddMember(123));

        await Assert.That(exception.ErrorCode).IsEqualTo(ErrorCode.INTEROP_STRUCT_ARRAY_AT_END);
    }

    [Test]
    public async Task Struct_AddArrayAtNonTailPosition_ThrowsArrayOnlyAtEndInteropException()
    {
        var structure = new Struct();
        structure.AddMember(123);

        InteropException exception = Capture<InteropException>(
            () => structure.AddMember(0, new ComArray(new[] { new ComString("x") }, isConformant: true)));

        await Assert.That(exception.ErrorCode).IsEqualTo(ErrorCode.INTEROP_STRUCT_ARRAY_ONLY_AT_END);
    }

    [Test]
    public async Task Union_ConstructorRejectsUnsupportedDiscriminant()
    {
        var exception = Capture<ArgumentException>(() => _ = new Union(typeof(string)));

        await Assert.That(exception.ParamName).IsEqualTo("discriminantClass");
    }

    [Test]
    public async Task Union_AddMember_StoresMemberByDiscriminant()
    {
        var union = new Union(typeof(short));

        union.AddMember((short)7, 1234);

        await Assert.That(union.Members.Count).IsEqualTo(1);
        await Assert.That(union.Members[(short)7]).IsEqualTo(1234);
        await Assert.That(union.ToString()).IsEqualTo("[members: 1]");
    }

    [Test]
    public async Task Union_AddMemberMismatchedDiscriminant_ThrowsInteropException()
    {
        var union = new Union(typeof(short));

        InteropException exception = Capture<InteropException>(() => union.AddMember(7, 1234));

        await Assert.That(exception.ErrorCode).IsEqualTo(ErrorCode.INTEROP_UNION_DISCRMINANT_MISMATCH);
    }

    [Test]
    public async Task Union_AddNullStructMember_StoresEmptySentinel()
    {
        var union = new Union(typeof(int));

        union.AddMember(3, (Struct)null!);

        await Assert.That(ReferenceEquals(union.Members[3], Struct.MEMBER_IS_EMPTY)).IsTrue();
    }

    [Test]
    public async Task Union_RemoveMember_RemovesDiscriminant()
    {
        var union = new Union(typeof(bool));
        union.AddMember(true, "enabled");

        union.RemoveMember(true);

        await Assert.That(union.Members.Count).IsEqualTo(0);
    }

    private static TException Capture<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected exception of type {typeof(TException).Name}.");
    }
}
