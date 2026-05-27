//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Windows COM VARIANT marshaling helpers. Reads + writes the 16-byte
/// (x86) / 24-byte (x64) tagged-union VARIANT struct in native memory,
/// plus the BSTR + SAFEARRAY descriptors carried via VT_BSTR / VT_ARRAY
/// modifiers.
/// </summary>
/// <remarks>
/// <para>
/// <b>Layout (x64).</b>
/// <code>
/// struct VARIANT {                    // 24 bytes total on x64
///     VARTYPE vt;                     // 2 bytes
///     WORD wReserved1, wReserved2;    // 4 bytes (3, kept zero)
///     WORD wReserved3;                // .... ditto
///     union {                         // 16-byte union (8-byte alignment on x64)
///         LONGLONG llVal;
///         double dblVal;
///         BSTR bstrVal;               // == LPWSTR ptr (4-byte length prefix)
///         SAFEARRAY* parray;
///         ...
///     };
/// }
/// </code>
/// On x86 the union is 8 bytes (16-byte total VARIANT). We use the
/// pointer-size determined by <see cref="IntPtr.Size"/> rather than
/// conditional compilation to stay AOT-clean.
/// </para>
/// <para>
/// <b>BSTR contract.</b> A BSTR is a length-prefixed UTF-16 pointer:
/// 4-byte length prefix + UTF-16 payload + NUL terminator. The pointer
/// returned to the caller is at the start of the UTF-16 data
/// (immediately after the length prefix). Allocated via
/// <c>Marshal.StringToBSTR</c> / freed via <c>Marshal.FreeBSTR</c>.
/// </para>
/// <para>
/// <b>SAFEARRAY contract.</b> One-dimensional only for OPC scalar item
/// values. Layout:
/// <code>
/// struct SAFEARRAY {
///     USHORT cDims;                  // dimension count
///     USHORT fFeatures;              // FADF_* flags (FADF_FIXEDSIZE=0x10, FADF_BSTR=0x100, ...)
///     ULONG  cbElements;             // bytes per element
///     ULONG  cLocks;                 // lock depth
///     PVOID  pvData;                 // pointer to element buffer
///     SAFEARRAYBOUND rgsabound[cDims]; // one descriptor per dim
/// }
/// struct SAFEARRAYBOUND { ULONG cElements; LONG lLbound; }
/// </code>
/// Allocated via <see cref="Marshal.AllocCoTaskMem(int)"/> for the
/// descriptor + a separate AllocCoTaskMem for the data buffer; both must
/// be freed by the consumer. For VT_BSTR-element SAFEARRAYs each BSTR
/// must additionally be <see cref="Marshal.FreeBSTR(IntPtr)"/>'d.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class ComVariantMarshaler
{
    private const ushort VT_EMPTY = 0;
    private const ushort VT_NULL = 1;
    private const ushort VT_I2 = 2;
    private const ushort VT_I4 = 3;
    private const ushort VT_R4 = 4;
    private const ushort VT_R8 = 5;
    private const ushort VT_CY = 6;
    private const ushort VT_DATE = 7;
    private const ushort VT_BSTR = 8;
    private const ushort VT_ERROR = 10;
    private const ushort VT_BOOL = 11;
    private const ushort VT_VARIANT = 12;
    private const ushort VT_I1 = 16;
    private const ushort VT_UI1 = 17;
    private const ushort VT_UI2 = 18;
    private const ushort VT_UI4 = 19;
    private const ushort VT_I8 = 20;
    private const ushort VT_UI8 = 21;
    private const ushort VT_ARRAY = 0x2000;

    /// <summary>The size of a single VARIANT struct in native memory.</summary>
    /// <remarks>
    /// On x86 = 16 bytes (vt+2*3 reserved+8-byte union). On x64 the union
    /// is 16 bytes for 8-byte pointer alignment, total = 24 bytes. We use
    /// IntPtr.Size to discriminate.
    /// </remarks>
    public static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

    /// <summary>Writes an empty VARIANT (vt = VT_EMPTY, value bits zero) at <paramref name="dest"/>.</summary>
    public static void WriteEmpty(IntPtr dest)
    {
        if (dest == IntPtr.Zero)
        {
            return;
        }
        NativeMemory.Clear((void*)dest, (nuint)VariantSize);
    }

    /// <summary>Marshals <paramref name="variant"/> into the VARIANT slot at <paramref name="dest"/>.</summary>
    /// <remarks>Caller-allocated VARIANT slot. Any heap allocations (BSTR / SAFEARRAY) become
    /// the caller's responsibility to free via <see cref="ClearVariant"/>.</remarks>
    public static void WriteVariant(IntPtr dest, OpcVariant variant)
    {
        if (dest == IntPtr.Zero)
        {
            return;
        }
        WriteEmpty(dest);
        ushort vt = (ushort)variant.Type;
        Marshal.WriteInt16(dest, (short)vt);

        if ((vt & VT_ARRAY) != 0)
        {
            WriteSafeArrayPayload(dest, variant);
            return;
        }
        WriteScalarPayload(dest, vt, variant.Boxed);
    }

    /// <summary>Reads a managed <see cref="OpcVariant"/> from the VARIANT slot at <paramref name="src"/>.</summary>
    public static OpcVariant ReadVariant(IntPtr src)
    {
        if (src == IntPtr.Zero)
        {
            return OpcVariant.Empty;
        }
        ushort vt = (ushort)Marshal.ReadInt16(src);
        if ((vt & VT_ARRAY) != 0)
        {
            return ReadSafeArrayPayload(src, vt);
        }
        return ReadScalarPayload(src, vt);
    }

    /// <summary>Frees BSTR / SAFEARRAY heap allocations referenced by the VARIANT slot at <paramref name="ptr"/>.</summary>
    /// <remarks>After this call the slot's vt is set to VT_EMPTY. Equivalent to <c>VariantClear</c>.</remarks>
    public static void ClearVariant(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }
        ushort vt = (ushort)Marshal.ReadInt16(ptr);
        if (vt == VT_BSTR)
        {
            IntPtr bstr = Marshal.ReadIntPtr(ptr, ValueOffset);
            if (bstr != IntPtr.Zero)
            {
                Marshal.FreeBSTR(bstr);
            }
        }
        else if ((vt & VT_ARRAY) != 0)
        {
            IntPtr safeArrayPtr = Marshal.ReadIntPtr(ptr, ValueOffset);
            if (safeArrayPtr != IntPtr.Zero)
            {
                FreeSafeArray(safeArrayPtr, (ushort)(vt & ~VT_ARRAY));
            }
        }
        WriteEmpty(ptr);
    }

    /// <summary>Allocates a managed BSTR from <paramref name="value"/>. Returns IntPtr.Zero for null.</summary>
    public static IntPtr AllocateBstr(string? value) =>
        value is null ? IntPtr.Zero : Marshal.StringToBSTR(value);

    /// <summary>Reads a BSTR pointer back to a managed string. Returns null for IntPtr.Zero.</summary>
    public static string? ReadBstr(IntPtr bstr) =>
        bstr == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(bstr);

    private static int ValueOffset => 8;

    [SuppressMessage("Design", "CA1502:Avoid excessive complexity",
        Justification = "VARIANT scalar dispatch fundamentally requires one branch per VARENUM code.")]
    private static void WriteScalarPayload(IntPtr dest, ushort vt, object? boxed)
    {
        IntPtr value = dest + ValueOffset;
        switch (vt)
        {
            case VT_I1:
                Marshal.WriteByte(value, (byte)(sbyte)(boxed ?? (sbyte)0));
                break;
            case VT_UI1:
                Marshal.WriteByte(value, (byte)(boxed ?? (byte)0));
                break;
            case VT_I2:
                Marshal.WriteInt16(value, (short)(boxed ?? (short)0));
                break;
            case VT_UI2:
                Marshal.WriteInt16(value, (short)(ushort)(boxed ?? (ushort)0));
                break;
            case VT_BOOL:
                Marshal.WriteInt16(value, (boxed is bool b && b) ? unchecked((short)0xFFFF) : (short)0);
                break;
            case VT_I4:
            case VT_ERROR:
                Marshal.WriteInt32(value, (int)(boxed ?? 0));
                break;
            case VT_UI4:
                Marshal.WriteInt32(value, unchecked((int)(uint)(boxed ?? 0u)));
                break;
            case VT_R4:
                Marshal.WriteInt32(value, BitConverter.SingleToInt32Bits((float)(boxed ?? 0f)));
                break;
            case VT_I8:
                Marshal.WriteInt64(value, (long)(boxed ?? 0L));
                break;
            case VT_UI8:
                Marshal.WriteInt64(value, unchecked((long)(ulong)(boxed ?? 0ul)));
                break;
            case VT_R8:
                Marshal.WriteInt64(value, BitConverter.DoubleToInt64Bits((double)(boxed ?? 0d)));
                break;
            case VT_DATE:
                Marshal.WriteInt64(value, BitConverter.DoubleToInt64Bits(((DateTime?)boxed ?? DateTime.UnixEpoch).ToOADate()));
                break;
            case VT_BSTR:
                Marshal.WriteIntPtr(value, AllocateBstr(boxed as string));
                break;
            case VT_EMPTY:
            case VT_NULL:
                break;
            default:
                // Unsupported VARTYPE: leave value bits zero. Loud failures
                // are inappropriate from a cross-unmanaged-boundary helper;
                // the COM client gets vt + zero value which is at worst
                // semantically meaningless and at best a soft no-op.
                break;
        }
    }

    [SuppressMessage("Design", "CA1502:Avoid excessive complexity",
        Justification = "VARIANT scalar dispatch fundamentally requires one branch per VARENUM code.")]
    private static OpcVariant ReadScalarPayload(IntPtr src, ushort vt)
    {
        IntPtr value = src + ValueOffset;
        return vt switch
        {
            VT_EMPTY => OpcVariant.Empty,
            VT_NULL => OpcVariant.Null,
            VT_I1 => OpcVariant.FromInt8((sbyte)Marshal.ReadByte(value)),
            VT_UI1 => OpcVariant.FromUInt8(Marshal.ReadByte(value)),
            VT_I2 => OpcVariant.FromInt16(Marshal.ReadInt16(value)),
            VT_UI2 => OpcVariant.FromUInt16((ushort)Marshal.ReadInt16(value)),
            VT_BOOL => OpcVariant.FromBoolean(Marshal.ReadInt16(value) != 0),
            VT_I4 => OpcVariant.FromInt32(Marshal.ReadInt32(value)),
            VT_UI4 => OpcVariant.FromUInt32(unchecked((uint)Marshal.ReadInt32(value))),
            VT_ERROR => OpcVariant.FromError(Marshal.ReadInt32(value)),
            VT_R4 => OpcVariant.FromSingle(BitConverter.Int32BitsToSingle(Marshal.ReadInt32(value))),
            VT_I8 => OpcVariant.FromInt64(Marshal.ReadInt64(value)),
            VT_UI8 => OpcVariant.FromUInt64(unchecked((ulong)Marshal.ReadInt64(value))),
            VT_R8 => OpcVariant.FromDouble(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(value))),
            VT_DATE => OpcVariant.FromDate(DateTime.FromOADate(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(value)))),
            VT_BSTR => OpcVariant.FromString(ReadBstr(Marshal.ReadIntPtr(value)) ?? string.Empty),
            _ => OpcVariant.Empty,
        };
    }

    private static void WriteSafeArrayPayload(IntPtr dest, OpcVariant variant)
    {
        OpcSafeArray? array = variant.AsSafeArray();
        if (array is null || array.Data.Length == 0)
        {
            Marshal.WriteIntPtr(dest, ValueOffset, IntPtr.Zero);
            return;
        }
        IntPtr safeArrayPtr = AllocateSafeArray(array);
        Marshal.WriteIntPtr(dest, ValueOffset, safeArrayPtr);
    }

    private static OpcVariant ReadSafeArrayPayload(IntPtr src, ushort vt)
    {
        IntPtr safeArrayPtr = Marshal.ReadIntPtr(src, ValueOffset);
        if (safeArrayPtr == IntPtr.Zero)
        {
            return OpcVariant.Empty;
        }
        ushort baseVt = (ushort)(vt & ~VT_ARRAY);
        OpcSafeArray array = ReadSafeArray(safeArrayPtr, baseVt);
        return OpcVariant.FromSafeArray(array);
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Explicit byte size; one SAFEARRAY descriptor + cElements * cbElements data buffer.")]
    private static IntPtr AllocateSafeArray(OpcSafeArray array)
    {
        ushort baseVt = (ushort)array.ElementType;
        uint elementSize = (uint)ElementSizeOf(baseVt);
        uint count = (uint)array.Data.Length;

        // SAFEARRAY native layout (cDims=1):
        //   USHORT cDims      @0  (2 bytes)
        //   USHORT fFeatures  @2  (2 bytes)
        //   ULONG  cbElements @4  (4 bytes)
        //   ULONG  cLocks     @8  (4 bytes)
        //   PVOID  pvData     @8 + IntPtr.Size (4 or 8 bytes; padded to ptr alignment on x64)
        //   SAFEARRAYBOUND rgsabound[1] @8 + 2*IntPtr.Size  (8 bytes per bound)
        int pvDataOffset = 8 + IntPtr.Size;
        int boundsOffset = pvDataOffset + IntPtr.Size;
        int totalDescriptorSize = boundsOffset + 8;

        IntPtr descriptor = Marshal.AllocCoTaskMem(totalDescriptorSize);
        IntPtr dataBuffer = Marshal.AllocCoTaskMem(checked((int)(elementSize * count)));

        Marshal.WriteInt16(descriptor, 0, 1); // cDims
        Marshal.WriteInt16(descriptor, 2, 0x10); // fFeatures = FADF_FIXEDSIZE
        Marshal.WriteInt32(descriptor, 4, (int)elementSize);
        Marshal.WriteInt32(descriptor, 8, 0); // cLocks
        Marshal.WriteIntPtr(descriptor, pvDataOffset, dataBuffer);

        Marshal.WriteInt32(descriptor, boundsOffset, (int)count);
        Marshal.WriteInt32(descriptor, boundsOffset + 4, 0); // lLbound = 0

        WriteSafeArrayData(dataBuffer, array, baseVt, elementSize);
        return descriptor;
    }

    private static OpcSafeArray ReadSafeArray(IntPtr descriptor, ushort baseVt)
    {
        int cDims = Marshal.ReadInt16(descriptor, 0);
        if (cDims != 1)
        {
            return new OpcSafeArray((VarType)baseVt, BuildEmptyTypedArray(baseVt));
        }
        int pvDataOffset = 8 + IntPtr.Size;
        int boundsOffset = pvDataOffset + IntPtr.Size;
        uint count = (uint)Marshal.ReadInt32(descriptor, boundsOffset);
        IntPtr dataBuffer = Marshal.ReadIntPtr(descriptor, pvDataOffset);
        if (dataBuffer == IntPtr.Zero || count == 0)
        {
            return new OpcSafeArray((VarType)baseVt, BuildEmptyTypedArray(baseVt));
        }
        Array elements = ReadSafeArrayData(dataBuffer, baseVt, count);
        return new OpcSafeArray((VarType)baseVt, elements);
    }

    private static void FreeSafeArray(IntPtr descriptor, ushort baseVt)
    {
        int pvDataOffset = 8 + IntPtr.Size;
        int boundsOffset = pvDataOffset + IntPtr.Size;
        IntPtr dataBuffer = Marshal.ReadIntPtr(descriptor, pvDataOffset);
        if (dataBuffer != IntPtr.Zero)
        {
            if (baseVt == VT_BSTR)
            {
                uint count = (uint)Marshal.ReadInt32(descriptor, boundsOffset);
                for (uint i = 0; i < count; i++)
                {
                    IntPtr bstr = Marshal.ReadIntPtr(dataBuffer, (int)(i * (uint)IntPtr.Size));
                    if (bstr != IntPtr.Zero)
                    {
                        Marshal.FreeBSTR(bstr);
                    }
                }
            }
            Marshal.FreeCoTaskMem(dataBuffer);
        }
        Marshal.FreeCoTaskMem(descriptor);
    }

    private static Array BuildEmptyTypedArray(ushort baseVt) => baseVt switch
    {
        VT_I1 => Array.Empty<sbyte>(),
        VT_UI1 => Array.Empty<byte>(),
        VT_I2 => Array.Empty<short>(),
        VT_UI2 => Array.Empty<ushort>(),
        VT_BOOL => Array.Empty<bool>(),
        VT_I4 or VT_ERROR => Array.Empty<int>(),
        VT_UI4 => Array.Empty<uint>(),
        VT_R4 => Array.Empty<float>(),
        VT_I8 => Array.Empty<long>(),
        VT_UI8 => Array.Empty<ulong>(),
        VT_R8 => Array.Empty<double>(),
        VT_DATE => Array.Empty<DateTime>(),
        VT_BSTR => Array.Empty<string>(),
        _ => Array.Empty<object>(),
    };

    private static int ElementSizeOf(ushort baseVt) => baseVt switch
    {
        VT_I1 or VT_UI1 => 1,
        VT_I2 or VT_UI2 or VT_BOOL => 2,
        VT_I4 or VT_UI4 or VT_ERROR or VT_R4 => 4,
        VT_I8 or VT_UI8 or VT_R8 or VT_DATE => 8,
        VT_BSTR => IntPtr.Size,
        _ => IntPtr.Size,
    };

    [SuppressMessage("Design", "CA1502:Avoid excessive complexity",
        Justification = "SAFEARRAY element-dispatch fundamentally requires one branch per VARENUM code.")]
    private static void WriteSafeArrayData(IntPtr dataBuffer, OpcSafeArray array, ushort baseVt, uint elementSize)
    {
        Array data = array.Data;
        for (int i = 0; i < data.Length; i++)
        {
            IntPtr slot = dataBuffer + (int)(i * elementSize);
            object? value = data.GetValue(i);
            switch (baseVt)
            {
                case VT_I1: Marshal.WriteByte(slot, (byte)(sbyte)(value ?? (sbyte)0)); break;
                case VT_UI1: Marshal.WriteByte(slot, (byte)(value ?? (byte)0)); break;
                case VT_I2: Marshal.WriteInt16(slot, (short)(value ?? (short)0)); break;
                case VT_UI2: Marshal.WriteInt16(slot, (short)(ushort)(value ?? (ushort)0)); break;
                case VT_BOOL: Marshal.WriteInt16(slot, (value is bool b && b) ? unchecked((short)0xFFFF) : (short)0); break;
                case VT_I4: case VT_ERROR: Marshal.WriteInt32(slot, (int)(value ?? 0)); break;
                case VT_UI4: Marshal.WriteInt32(slot, unchecked((int)(uint)(value ?? 0u))); break;
                case VT_R4: Marshal.WriteInt32(slot, BitConverter.SingleToInt32Bits((float)(value ?? 0f))); break;
                case VT_I8: Marshal.WriteInt64(slot, (long)(value ?? 0L)); break;
                case VT_UI8: Marshal.WriteInt64(slot, unchecked((long)(ulong)(value ?? 0ul))); break;
                case VT_R8: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits((double)(value ?? 0d))); break;
                case VT_DATE: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits(((DateTime?)value ?? DateTime.UnixEpoch).ToOADate())); break;
                case VT_BSTR: Marshal.WriteIntPtr(slot, AllocateBstr(value as string)); break;
                default: break;
            }
        }
    }

    [SuppressMessage("Design", "CA1502:Avoid excessive complexity",
        Justification = "SAFEARRAY element-dispatch fundamentally requires one branch per VARENUM code.")]
    private static Array ReadSafeArrayData(IntPtr dataBuffer, ushort baseVt, uint count)
    {
        uint elementSize = (uint)ElementSizeOf(baseVt);
        Array result = baseVt switch
        {
            VT_I1 => new sbyte[count],
            VT_UI1 => new byte[count],
            VT_I2 => new short[count],
            VT_UI2 => new ushort[count],
            VT_BOOL => new bool[count],
            VT_I4 or VT_ERROR => new int[count],
            VT_UI4 => new uint[count],
            VT_R4 => new float[count],
            VT_I8 => new long[count],
            VT_UI8 => new ulong[count],
            VT_R8 => new double[count],
            VT_DATE => new DateTime[count],
            VT_BSTR => new string[count],
            _ => new object?[count],
        };
        for (uint i = 0; i < count; i++)
        {
            IntPtr slot = dataBuffer + (int)(i * elementSize);
            object? value = baseVt switch
            {
                VT_I1 => (sbyte)Marshal.ReadByte(slot),
                VT_UI1 => Marshal.ReadByte(slot),
                VT_I2 => Marshal.ReadInt16(slot),
                VT_UI2 => (ushort)Marshal.ReadInt16(slot),
                VT_BOOL => Marshal.ReadInt16(slot) != 0,
                VT_I4 or VT_ERROR => Marshal.ReadInt32(slot),
                VT_UI4 => unchecked((uint)Marshal.ReadInt32(slot)),
                VT_R4 => BitConverter.Int32BitsToSingle(Marshal.ReadInt32(slot)),
                VT_I8 => Marshal.ReadInt64(slot),
                VT_UI8 => unchecked((ulong)Marshal.ReadInt64(slot)),
                VT_R8 => BitConverter.Int64BitsToDouble(Marshal.ReadInt64(slot)),
                VT_DATE => DateTime.FromOADate(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(slot))),
                VT_BSTR => ReadBstr(Marshal.ReadIntPtr(slot)) ?? string.Empty,
                _ => null,
            };
            result.SetValue(value, (int)i);
        }
        return result;
    }
}
