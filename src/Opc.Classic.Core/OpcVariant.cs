//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic;

/// <summary>
/// Managed projection of the COM/OAUT <c>VARIANT</c> structure: a
/// type-tagged value carrier covering the scalar OPC value vocabulary.
/// </summary>
/// <remarks>
/// The "boxed" representation (Type + boxed value) trades a small
/// allocation per non-zero value for a vastly simpler API and full
/// AOT-cleanliness. For hot paths handling large array values, the
/// SAFEARRAY-specific helpers (forthcoming) bypass per-element boxing.
/// </remarks>
public readonly record struct OpcVariant
{
    /// <summary>The empty variant (vt = VT_EMPTY).</summary>
    public static OpcVariant Empty { get; } = new(VarType.VT_EMPTY, null);

    /// <summary>The SQL-style null variant (vt = VT_NULL).</summary>
    public static OpcVariant Null { get; } = new(VarType.VT_NULL, null);

    /// <summary>Constructs a variant with the given type and (boxed) value.</summary>
    public OpcVariant(VarType type, object? boxed)
    {
        Type = type;
        Boxed = boxed;
    }

    /// <summary>The OAUT type discriminator.</summary>
    public VarType Type { get; init; }

    /// <summary>The .NET-typed value (boxed). Null when <see cref="Type"/> is <see cref="VarType.VT_EMPTY"/> or <see cref="VarType.VT_NULL"/>.</summary>
    public object? Boxed { get; init; }

    /// <summary>True if the variant has no value (<c>VT_EMPTY</c> or <c>VT_NULL</c>).</summary>
    public bool IsEmpty => Type == VarType.VT_EMPTY || Type == VarType.VT_NULL;

    // ---- Factory methods for scalar types ----

    /// <summary>Creates a VT_I1 (signed 8-bit) variant.</summary>
    public static OpcVariant FromInt8(sbyte v) => new(VarType.VT_I1, v);

    /// <summary>Creates a VT_UI1 (unsigned 8-bit) variant.</summary>
    public static OpcVariant FromUInt8(byte v) => new(VarType.VT_UI1, v);

    /// <summary>Creates a VT_I2 (signed 16-bit) variant.</summary>
    public static OpcVariant FromInt16(short v) => new(VarType.VT_I2, v);

    /// <summary>Creates a VT_UI2 (unsigned 16-bit) variant.</summary>
    public static OpcVariant FromUInt16(ushort v) => new(VarType.VT_UI2, v);

    /// <summary>Creates a VT_I4 (signed 32-bit) variant.</summary>
    public static OpcVariant FromInt32(int v) => new(VarType.VT_I4, v);

    /// <summary>Creates a VT_UI4 (unsigned 32-bit) variant.</summary>
    public static OpcVariant FromUInt32(uint v) => new(VarType.VT_UI4, v);

    /// <summary>Creates a VT_I8 (signed 64-bit) variant.</summary>
    public static OpcVariant FromInt64(long v) => new(VarType.VT_I8, v);

    /// <summary>Creates a VT_UI8 (unsigned 64-bit) variant.</summary>
    public static OpcVariant FromUInt64(ulong v) => new(VarType.VT_UI8, v);

    /// <summary>Creates a VT_R4 (single-precision float) variant.</summary>
    public static OpcVariant FromSingle(float v) => new(VarType.VT_R4, v);

    /// <summary>Creates a VT_R8 (double-precision float) variant.</summary>
    public static OpcVariant FromDouble(double v) => new(VarType.VT_R8, v);

    /// <summary>Creates a VT_BSTR (OLE string) variant.</summary>
    public static OpcVariant FromString(string v) => new(VarType.VT_BSTR, v);

    /// <summary>
    /// Creates a VT_BOOL variant. Note: the OAUT wire representation is
    /// -1 for true and 0 for false (as VARIANT_BOOL = Int16); this carrier
    /// presents the .NET-natural bool.
    /// </summary>
    public static OpcVariant FromBoolean(bool v) => new(VarType.VT_BOOL, v);

    /// <summary>Creates a VT_DATE variant from a UTC <see cref="DateTime"/>.</summary>
    public static OpcVariant FromDate(DateTime v) => new(VarType.VT_DATE, v);

    /// <summary>Creates a VT_FILETIME variant from a 100-ns count since 1601-01-01 UTC.</summary>
    public static OpcVariant FromFileTime(long fileTime100ns) => new(VarType.VT_FILETIME, fileTime100ns);

    /// <summary>Creates a VT_ERROR variant from an HRESULT-like SCODE.</summary>
    public static OpcVariant FromError(int scode) => new(VarType.VT_ERROR, scode);

    /// <summary>Creates a VT_CLSID variant.</summary>
    public static OpcVariant FromClsid(Guid v) => new(VarType.VT_CLSID, v);

    /// <summary>Creates a VT_VARIANT variant that carries another VARIANT.</summary>
    public static OpcVariant FromVariant(OpcVariant v) => new(VarType.VT_VARIANT, v);

    /// <summary>Creates a VT_BYREF variant for the supplied base VARTYPE.</summary>
    public static OpcVariant FromByRef(VarType baseType, object? boxed) =>
        new((VarType)(((ushort)baseType & ~(ushort)VarType.VT_BYREF) | (ushort)VarType.VT_BYREF), boxed);

    /// <summary>Creates a VT_RECORD variant for a registered record layout.</summary>
    public static OpcVariant FromRecord(OpcRecordValue record)
    {
        ArgumentNullException.ThrowIfNull(record);
        return new OpcVariant(VarType.VT_RECORD, record);
    }

    /// <summary>
    /// Creates an array variant: the VARTYPE carries the VT_ARRAY modifier
    /// OR'd with the SAFEARRAY's element type, and the boxed value is the
    /// <see cref="OpcSafeArray"/> itself.
    /// </summary>
    public static OpcVariant FromSafeArray(OpcSafeArray array)
    {
        ArgumentNullException.ThrowIfNull(array);
        return new OpcVariant(
            (VarType)((ushort)array.ElementType | (ushort)VarType.VT_ARRAY),
            array);
    }

    // ---- Strongly-typed accessors ----

    /// <summary>Returns the int8 value if <see cref="Type"/> is <see cref="VarType.VT_I1"/>, else null.</summary>
    public sbyte? AsInt8() => Type == VarType.VT_I1 ? (sbyte?)Boxed : null;

    /// <summary>Returns the uint8 value if <see cref="Type"/> is <see cref="VarType.VT_UI1"/>, else null.</summary>
    public byte? AsUInt8() => Type == VarType.VT_UI1 ? (byte?)Boxed : null;

    /// <summary>Returns the int16 value if <see cref="Type"/> is <see cref="VarType.VT_I2"/>, else null.</summary>
    public short? AsInt16() => Type == VarType.VT_I2 ? (short?)Boxed : null;

    /// <summary>Returns the uint16 value if <see cref="Type"/> is <see cref="VarType.VT_UI2"/>, else null.</summary>
    public ushort? AsUInt16() => Type == VarType.VT_UI2 ? (ushort?)Boxed : null;

    /// <summary>Returns the int32 value if <see cref="Type"/> is <see cref="VarType.VT_I4"/>, else null.</summary>
    public int? AsInt32() => Type == VarType.VT_I4 ? (int?)Boxed : null;

    /// <summary>Returns the uint32 value if <see cref="Type"/> is <see cref="VarType.VT_UI4"/>, else null.</summary>
    public uint? AsUInt32() => Type == VarType.VT_UI4 ? (uint?)Boxed : null;

    /// <summary>Returns the int64 value if <see cref="Type"/> is <see cref="VarType.VT_I8"/>, else null.</summary>
    public long? AsInt64() => Type == VarType.VT_I8 ? (long?)Boxed : null;

    /// <summary>Returns the uint64 value if <see cref="Type"/> is <see cref="VarType.VT_UI8"/>, else null.</summary>
    public ulong? AsUInt64() => Type == VarType.VT_UI8 ? (ulong?)Boxed : null;

    /// <summary>Returns the float value if <see cref="Type"/> is <see cref="VarType.VT_R4"/>, else null.</summary>
    public float? AsSingle() => Type == VarType.VT_R4 ? (float?)Boxed : null;

    /// <summary>Returns the double value if <see cref="Type"/> is <see cref="VarType.VT_R8"/>, else null.</summary>
    public double? AsDouble() => Type == VarType.VT_R8 ? (double?)Boxed : null;

    /// <summary>Returns the string value if <see cref="Type"/> is <see cref="VarType.VT_BSTR"/>, else null.</summary>
    public string? AsString() => Type == VarType.VT_BSTR ? (string?)Boxed : null;

    /// <summary>Returns the bool value if <see cref="Type"/> is <see cref="VarType.VT_BOOL"/>, else null.</summary>
    public bool? AsBoolean() => Type == VarType.VT_BOOL ? (bool?)Boxed : null;

    /// <summary>Returns the date value if <see cref="Type"/> is <see cref="VarType.VT_DATE"/>, else null.</summary>
    public DateTime? AsDate() => Type == VarType.VT_DATE ? (DateTime?)Boxed : null;

    /// <summary>Returns the 100-ns FILETIME if <see cref="Type"/> is <see cref="VarType.VT_FILETIME"/>, else null.</summary>
    public long? AsFileTime() => Type == VarType.VT_FILETIME ? (long?)Boxed : null;

    /// <summary>Returns the SCODE if <see cref="Type"/> is <see cref="VarType.VT_ERROR"/>, else null.</summary>
    public int? AsError() => Type == VarType.VT_ERROR ? (int?)Boxed : null;

    /// <summary>Returns the GUID if <see cref="Type"/> is <see cref="VarType.VT_CLSID"/>, else null.</summary>
    public Guid? AsClsid() => Type == VarType.VT_CLSID ? (Guid?)Boxed : null;

    /// <summary>Returns the nested VARIANT when the base type is <see cref="VarType.VT_VARIANT"/>.</summary>
    public OpcVariant? AsVariant() => VarTypeMask.BaseOf(Type) == VarType.VT_VARIANT ? (OpcVariant?)Boxed : null;

    /// <summary>Returns the record value when the base type is <see cref="VarType.VT_RECORD"/>.</summary>
    public OpcRecordValue? AsRecord() => VarTypeMask.BaseOf(Type) == VarType.VT_RECORD ? Boxed as OpcRecordValue : null;

    /// <summary>Returns the referenced value if this variant carries the VT_BYREF modifier.</summary>
    public object? AsByRefValue() => VarTypeMask.IsByRef(Type) ? Boxed : null;

    /// <summary>
    /// Returns the carried <see cref="OpcSafeArray"/> if this variant carries
    /// the <see cref="VarType.VT_ARRAY"/> modifier, else <see langword="null"/>.
    /// </summary>
    public OpcSafeArray? AsSafeArray() => VarTypeMask.IsArray(Type) ? Boxed as OpcSafeArray : null;
}
