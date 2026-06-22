// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable CA1707 // OPC IDL naming preserved (VT_I4 etc.)
#pragma warning disable CA1720 // Identifier contains type name — these mirror oaidl.h VARENUM verbatim
#pragma warning disable MA0048 // VarType + VarTypeMask helpers grouped under the enum they extend

namespace Opc.Classic;

/// <summary>
/// VARIANT type discriminator codes from oaidl.h's <c>VARENUM</c>
/// enumeration. Used by COM/OAUT's VARIANT structure and (transitively)
/// by every OPC interface that returns or accepts polymorphic values.
/// </summary>
/// <remarks>
/// The high-bit flag values (<see cref="VT_VECTOR"/>, <see cref="VT_ARRAY"/>,
/// <see cref="VT_BYREF"/>, <see cref="VT_RESERVED"/>) are bitwise-OR'd with
/// the base type. For example, a VT_ARRAY|VT_I4 represents a SAFEARRAY of
/// 32-bit signed integers.
/// </remarks>
public enum VarType : ushort
{
    /// <summary>
    /// Nothing set yet.
    /// </summary>
    VT_EMPTY = 0,
    /// <summary>
    /// SQL-style null.
    /// </summary>
    VT_NULL = 1,
    /// <summary>
    /// 16-bit signed integer.
    /// </summary>
    VT_I2 = 2,
    /// <summary>
    /// 32-bit signed integer.
    /// </summary>
    VT_I4 = 3,
    /// <summary>
    /// IEEE-754 single precision float.
    /// </summary>
    VT_R4 = 4,
    /// <summary>
    /// IEEE-754 double precision float.
    /// </summary>
    VT_R8 = 5,
    /// <summary>
    /// CY currency (64-bit scaled fixed-point).
    /// </summary>
    VT_CY = 6,
    /// <summary>
    /// OLE DATE (days since 1899-12-30 as double).
    /// </summary>
    VT_DATE = 7,
    /// <summary>
    /// BSTR (OLE Unicode string with length prefix).
    /// </summary>
    VT_BSTR = 8,
    /// <summary>
    /// IDispatch pointer.
    /// </summary>
    VT_DISPATCH = 9,
    /// <summary>
    /// SCODE (32-bit error code).
    /// </summary>
    VT_ERROR = 10,
    /// <summary>
    /// VARIANT_BOOL (-1 = true, 0 = false, as Int16 on the wire).
    /// </summary>
    VT_BOOL = 11,
    /// <summary>
    /// Nested VARIANT.
    /// </summary>
    VT_VARIANT = 12,
    /// <summary>
    /// IUnknown pointer.
    /// </summary>
    VT_UNKNOWN = 13,
    /// <summary>
    /// Fixed-point decimal (96 bits + scale).
    /// </summary>
    VT_DECIMAL = 14,
    /// <summary>
    /// 8-bit signed integer.
    /// </summary>
    VT_I1 = 16,
    /// <summary>
    /// 8-bit unsigned integer.
    /// </summary>
    VT_UI1 = 17,
    /// <summary>
    /// 16-bit unsigned integer.
    /// </summary>
    VT_UI2 = 18,
    /// <summary>
    /// 32-bit unsigned integer.
    /// </summary>
    VT_UI4 = 19,
    /// <summary>
    /// 64-bit signed integer.
    /// </summary>
    VT_I8 = 20,
    /// <summary>
    /// 64-bit unsigned integer.
    /// </summary>
    VT_UI8 = 21,
    /// <summary>
    /// Machine-sized signed integer (alias for I4/I8 depending on platform).
    /// </summary>
    VT_INT = 22,
    /// <summary>
    /// Machine-sized unsigned integer.
    /// </summary>
    VT_UINT = 23,
    /// <summary>
    /// Void / "no value" — used only as a function return type.
    /// </summary>
    VT_VOID = 24,
    /// <summary>
    /// HRESULT.
    /// </summary>
    VT_HRESULT = 25,
    /// <summary>
    /// Pointer (in type-info only).
    /// </summary>
    VT_PTR = 26,
    /// <summary>
    /// SAFEARRAY (in type-info only).
    /// </summary>
    VT_SAFEARRAY = 27,
    /// <summary>
    /// C-style array (in type-info only).
    /// </summary>
    VT_CARRAY = 28,
    /// <summary>
    /// User-defined type (in type-info only).
    /// </summary>
    VT_USERDEFINED = 29,
    /// <summary>
    /// Null-terminated ANSI string (in type-info only).
    /// </summary>
    VT_LPSTR = 30,
    /// <summary>
    /// Null-terminated Unicode string (in type-info only).
    /// </summary>
    VT_LPWSTR = 31,
    /// <summary>
    /// UDT record.
    /// </summary>
    VT_RECORD = 36,
    /// <summary>
    /// Signed integer the size of a pointer.
    /// </summary>
    VT_INT_PTR = 37,
    /// <summary>
    /// Unsigned integer the size of a pointer.
    /// </summary>
    VT_UINT_PTR = 38,
    /// <summary>
    /// Windows FILETIME (64-bit count of 100-ns intervals since 1601).
    /// </summary>
    VT_FILETIME = 64,
    /// <summary>
    /// Length-prefixed byte array.
    /// </summary>
    VT_BLOB = 65,
    /// <summary>
    /// IStream pointer.
    /// </summary>
    VT_STREAM = 66,
    /// <summary>
    /// IStorage pointer.
    /// </summary>
    VT_STORAGE = 67,
    /// <summary>
    /// IStream pointer (serialized version).
    /// </summary>
    VT_STREAMED_OBJECT = 68,
    /// <summary>
    /// IStorage pointer (serialized version).
    /// </summary>
    VT_STORED_OBJECT = 69,
    /// <summary>
    /// Length-prefixed blob with metadata.
    /// </summary>
    VT_BLOB_OBJECT = 70,
    /// <summary>
    /// Clipboard format.
    /// </summary>
    VT_CF = 71,
    /// <summary>
    /// CLSID / GUID.
    /// </summary>
    VT_CLSID = 72,

    // ---- Modifier flags (OR'd with the base type) ----

    /// <summary>
    /// Modifier: simple counted array (16 bytes header + data).
    /// </summary>
    VT_VECTOR = 0x1000,
    /// <summary>
    /// Modifier: SAFEARRAY.
    /// </summary>
    VT_ARRAY = 0x2000,
    /// <summary>
    /// Modifier: by-reference pointer to the value.
    /// </summary>
    VT_BYREF = 0x4000,
    /// <summary>
    /// Modifier: reserved for COM internal use.
    /// </summary>
    VT_RESERVED = 0x8000,
}

/// <summary>
/// Bit-mask helpers for the modifier flags in <see cref="VarType"/>.
/// </summary>
public static class VarTypeMask
{
    /// <summary>
    /// Bit mask isolating the base type (lower 12 bits).
    /// </summary>
    public const ushort BaseType = 0x0FFF;

    /// <summary>
    /// Bit mask covering all modifier flags.
    /// </summary>
    public const ushort Modifiers = 0xF000;

    /// <summary>
    /// Strips modifier flags off the given VarType, returning the base type.
    /// </summary>
    public static VarType BaseOf(VarType vt) => (VarType)((ushort)vt & BaseType);

    /// <summary>
    /// True if the VarType carries the VT_ARRAY modifier.
    /// </summary>
    public static bool IsArray(VarType vt) => ((ushort)vt & (ushort)VarType.VT_ARRAY) != 0;

    /// <summary>
    /// True if the VarType carries the VT_VECTOR modifier.
    /// </summary>
    public static bool IsVector(VarType vt) => ((ushort)vt & (ushort)VarType.VT_VECTOR) != 0;

    /// <summary>
    /// True if the VarType carries the VT_BYREF modifier.
    /// </summary>
    public static bool IsByRef(VarType vt) => ((ushort)vt & (ushort)VarType.VT_BYREF) != 0;
}
