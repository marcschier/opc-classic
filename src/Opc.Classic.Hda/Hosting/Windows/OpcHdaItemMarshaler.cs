//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // VARIANT writer is grouped with OPCHDA_* marshaling for native buffer locality.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>Native OPCHDA_* allocation helpers for Windows HDA CCWs.</summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaItemMarshaler
{
    private static int PointerAlignedAfterThreeDwords => Align(3 * sizeof(int), IntPtr.Size);
    private static int PointerAlignedAfterTwoDwords => Align(2 * sizeof(int), IntPtr.Size);
    private static int HdaTimeStringOffset => IntPtr.Size == 8 ? 8 : 4;
    private static int HdaTimeFileTimeOffset => HdaTimeStringOffset + IntPtr.Size;

    public static int ItemSize => PointerAlignedAfterThreeDwords + (3 * IntPtr.Size);
    public static int AttributeSize => PointerAlignedAfterThreeDwords + (2 * IntPtr.Size);
    public static int ModifiedItemSize => PointerAlignedAfterTwoDwords + (6 * IntPtr.Size);
    public static int AnnotationSize => PointerAlignedAfterTwoDwords + (4 * IntPtr.Size);

    public static OpcHdaTime ReadHdaTime(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
        {
            throw new ArgumentException("OPCHDA_TIME pointer is null.", nameof(ptr));
        }

        bool isString = Marshal.ReadInt32(ptr) != 0;
        IntPtr stringPtr = Marshal.ReadIntPtr(ptr, HdaTimeStringOffset);
        long fileTime = Marshal.ReadInt64(ptr, HdaTimeFileTimeOffset);
        if (isString)
        {
            string? expression = Marshal.PtrToStringUni(stringPtr);
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("OPCHDA_TIME string expression is null or empty.", nameof(ptr));
            }

            return OpcHdaTime.FromString(expression);
        }

        return OpcHdaTime.FromTimestamp(DateTimeOffset.FromFileTime(fileTime));
    }

    public static int[] ReadInt32Array(IntPtr ptr, int count)
    {
        var values = new int[count];
        if (count > 0)
        {
            Marshal.Copy(ptr, values, 0, count);
        }

        return values;
    }

    public static long[] ReadFileTimeArray(IntPtr ptr, int count)
    {
        var values = new long[count];
        for (int i = 0; i < count; i++)
        {
            long value = Marshal.ReadInt64(ptr, checked(i * sizeof(long)));
            _ = DateTimeOffset.FromFileTime(value);
            values[i] = value;
        }

        return values;
    }

    public static IntPtr AllocateInt32Array(int[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(int)));
        Marshal.Copy(values, 0, ptr, values.Length);
        return ptr;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    public static IntPtr AllocateItemArray(OpcHdaItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(items.Length * ItemSize));
        NativeMemory.Clear((void*)ptr, (nuint)checked(items.Length * ItemSize));
        bool completed = false;
        try
        {
            for (int i = 0; i < items.Length; i++)
            {
                WriteItem(IntPtr.Add(ptr, checked(i * ItemSize)), items[i]);
            }

            completed = true;
            return ptr;
        }
        finally
        {
            if (!completed)
            {
                FreeItemArray(ptr, items.Length);
            }
        }
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    public static IntPtr AllocateModifiedItemArray(OpcHdaModifiedItem[] items)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(items.Length * ModifiedItemSize));
        NativeMemory.Clear((void*)ptr, (nuint)checked(items.Length * ModifiedItemSize));
        bool completed = false;
        try
        {
            for (int i = 0; i < items.Length; i++)
            {
                WriteModifiedItem(IntPtr.Add(ptr, checked(i * ModifiedItemSize)), items[i]);
            }

            completed = true;
            return ptr;
        }
        finally
        {
            if (!completed)
            {
                FreeModifiedItemArray(ptr, items.Length);
            }
        }
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    public static IntPtr AllocateAttributeArray(OpcHdaAttribute[] attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(attributes.Length * AttributeSize));
        NativeMemory.Clear((void*)ptr, (nuint)checked(attributes.Length * AttributeSize));
        bool completed = false;
        try
        {
            for (int i = 0; i < attributes.Length; i++)
            {
                WriteAttribute(IntPtr.Add(ptr, checked(i * AttributeSize)), attributes[i]);
            }

            completed = true;
            return ptr;
        }
        finally
        {
            if (!completed)
            {
                FreeAttributeArray(ptr, attributes.Length);
            }
        }
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    public static IntPtr AllocateAnnotationArray(OpcHdaAnnotation[] annotations)
    {
        ArgumentNullException.ThrowIfNull(annotations);
        if (annotations.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(annotations.Length * AnnotationSize));
        NativeMemory.Clear((void*)ptr, (nuint)checked(annotations.Length * AnnotationSize));
        bool completed = false;
        try
        {
            for (int i = 0; i < annotations.Length; i++)
            {
                WriteAnnotation(IntPtr.Add(ptr, checked(i * AnnotationSize)), annotations[i]);
            }

            completed = true;
            return ptr;
        }
        finally
        {
            if (!completed)
            {
                FreeAnnotationArray(ptr, annotations.Length);
            }
        }
    }

    public static void FreeItemArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            FreeItem(IntPtr.Add(ptr, checked(i * ItemSize)));
        }

        Marshal.FreeCoTaskMem(ptr);
    }

    public static void FreeModifiedItemArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            FreeModifiedItem(IntPtr.Add(ptr, checked(i * ModifiedItemSize)));
        }

        Marshal.FreeCoTaskMem(ptr);
    }

    public static void FreeAttributeArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            FreeAttribute(IntPtr.Add(ptr, checked(i * AttributeSize)));
        }

        Marshal.FreeCoTaskMem(ptr);
    }

    public static void FreeAnnotationArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            FreeAnnotation(IntPtr.Add(ptr, checked(i * AnnotationSize)));
        }

        Marshal.FreeCoTaskMem(ptr);
    }

    private static void WriteItem(IntPtr slot, OpcHdaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        int count = item.Timestamps.Length;
        Marshal.WriteInt32(slot, 0, item.ClientHandle);
        Marshal.WriteInt32(slot, 4, item.AggregateHandle);
        Marshal.WriteInt32(slot, 8, count);
        WritePointer(slot, PointerAlignedAfterThreeDwords, AllocateFileTimeArray(item.Timestamps));
        WritePointer(slot, PointerAlignedAfterThreeDwords + IntPtr.Size, AllocateUInt32Array(item.Qualities));
        WritePointer(slot, PointerAlignedAfterThreeDwords + (2 * IntPtr.Size), AllocateVariantArray(item.Values));
    }

    private static void WriteModifiedItem(IntPtr slot, OpcHdaModifiedItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        int count = item.Timestamps.Length;
        Marshal.WriteInt32(slot, 0, item.ClientHandle);
        Marshal.WriteInt32(slot, 4, count);
        int offset = PointerAlignedAfterTwoDwords;
        WritePointer(slot, offset, AllocateFileTimeArray(item.Timestamps));
        WritePointer(slot, offset + IntPtr.Size, AllocateUInt32Array(item.Qualities));
        WritePointer(slot, offset + (2 * IntPtr.Size), AllocateVariantArray(item.Values));
        WritePointer(slot, offset + (3 * IntPtr.Size), AllocateFileTimeArray(item.ModificationTimes));
        WritePointer(slot, offset + (4 * IntPtr.Size), AllocateUInt32Array(item.EditTypes));
        WritePointer(slot, offset + (5 * IntPtr.Size), AllocateStringPointerArray(item.Users));
    }

    private static void WriteAttribute(IntPtr slot, OpcHdaAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);
        int count = attribute.Timestamps.Length;
        Marshal.WriteInt32(slot, 0, attribute.ClientHandle);
        Marshal.WriteInt32(slot, 4, count);
        Marshal.WriteInt32(slot, 8, attribute.AttributeId);
        WritePointer(slot, PointerAlignedAfterThreeDwords, AllocateFileTimeArray(attribute.Timestamps));
        WritePointer(slot, PointerAlignedAfterThreeDwords + IntPtr.Size, AllocateVariantArray(attribute.Values));
    }

    private static void WriteAnnotation(IntPtr slot, OpcHdaAnnotation annotation)
    {
        ArgumentNullException.ThrowIfNull(annotation);
        int count = annotation.Timestamps.Length;
        Marshal.WriteInt32(slot, 0, annotation.ClientHandle);
        Marshal.WriteInt32(slot, 4, count);
        int offset = PointerAlignedAfterTwoDwords;
        WritePointer(slot, offset, AllocateFileTimeArray(annotation.Timestamps));
        WritePointer(slot, offset + IntPtr.Size, AllocateStringPointerArray(annotation.Annotations));
        WritePointer(slot, offset + (2 * IntPtr.Size), AllocateFileTimeArray(annotation.AnnotationTimes));
        WritePointer(slot, offset + (3 * IntPtr.Size), AllocateStringPointerArray(annotation.Users));
    }

    private static void FreeItem(IntPtr slot)
    {
        int count = Math.Max(0, Marshal.ReadInt32(slot, 8));
        int offset = PointerAlignedAfterThreeDwords;
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset));
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset + IntPtr.Size));
        FreeVariantArray(ReadPointer(slot, offset + (2 * IntPtr.Size)), count);
    }

    private static void FreeModifiedItem(IntPtr slot)
    {
        int count = Math.Max(0, Marshal.ReadInt32(slot, 4));
        int offset = PointerAlignedAfterTwoDwords;
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset));
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset + IntPtr.Size));
        FreeVariantArray(ReadPointer(slot, offset + (2 * IntPtr.Size)), count);
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset + (3 * IntPtr.Size)));
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset + (4 * IntPtr.Size)));
        FreeStringPointerArray(ReadPointer(slot, offset + (5 * IntPtr.Size)), count);
    }

    private static void FreeAttribute(IntPtr slot)
    {
        int count = Math.Max(0, Marshal.ReadInt32(slot, 4));
        int offset = PointerAlignedAfterThreeDwords;
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset));
        FreeVariantArray(ReadPointer(slot, offset + IntPtr.Size), count);
    }

    private static void FreeAnnotation(IntPtr slot)
    {
        int count = Math.Max(0, Marshal.ReadInt32(slot, 4));
        int offset = PointerAlignedAfterTwoDwords;
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset));
        FreeStringPointerArray(ReadPointer(slot, offset + IntPtr.Size), count);
        Marshal.FreeCoTaskMem(ReadPointer(slot, offset + (2 * IntPtr.Size)));
        FreeStringPointerArray(ReadPointer(slot, offset + (3 * IntPtr.Size)), count);
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateFileTimeArray(DateTimeOffset[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(long)));
        for (int i = 0; i < values.Length; i++)
        {
            Marshal.WriteInt64(ptr, checked(i * sizeof(long)), ToFileTime(values[i]));
        }

        return ptr;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateUInt32Array(uint[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(int)));
        for (int i = 0; i < values.Length; i++)
        {
            Marshal.WriteInt32(ptr, checked(i * sizeof(int)), unchecked((int)values[i]));
        }

        return ptr;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateVariantArray(OpcVariant[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }

        int variantSize = OpcHdaVariantMarshaler.VariantSize;
        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * variantSize));
        NativeMemory.Clear((void*)ptr, (nuint)checked(values.Length * variantSize));
        bool completed = false;
        try
        {
            for (int i = 0; i < values.Length; i++)
            {
                OpcHdaVariantMarshaler.WriteVariant(IntPtr.Add(ptr, checked(i * variantSize)), values[i]);
            }

            completed = true;
            return ptr;
        }
        finally
        {
            if (!completed)
            {
                FreeVariantArray(ptr, values.Length);
            }
        }
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateStringPointerArray(string?[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * IntPtr.Size));
        NativeMemory.Clear((void*)ptr, (nuint)checked(values.Length * IntPtr.Size));
        bool completed = false;
        try
        {
            for (int i = 0; i < values.Length; i++)
            {
                WritePointer(ptr, i * IntPtr.Size, values[i] is null ? IntPtr.Zero : Marshal.StringToCoTaskMemUni(values[i]));
            }

            completed = true;
            return ptr;
        }
        finally
        {
            if (!completed)
            {
                FreeStringPointerArray(ptr, values.Length);
            }
        }
    }

    private static void FreeVariantArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }

        int variantSize = OpcHdaVariantMarshaler.VariantSize;
        for (int i = 0; i < count; i++)
        {
            OpcHdaVariantMarshaler.ClearVariant(IntPtr.Add(ptr, checked(i * variantSize)));
        }

        Marshal.FreeCoTaskMem(ptr);
    }

    private static void FreeStringPointerArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Marshal.FreeCoTaskMem(ReadPointer(ptr, checked(i * IntPtr.Size)));
        }

        Marshal.FreeCoTaskMem(ptr);
    }

    private static long ToFileTime(DateTimeOffset value) => value == default ? 0L : value.ToFileTime();

    private static int Align(int value, int alignment) => (value + alignment - 1) & ~(alignment - 1);

    private static void WritePointer(IntPtr basePtr, int offset, IntPtr value) => Marshal.WriteIntPtr(basePtr, offset, value);

    private static IntPtr ReadPointer(IntPtr basePtr, int offset) => Marshal.ReadIntPtr(basePtr, offset);
}

/// <summary>OAUT VARIANT writer for HDA native CCW buffers.</summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaVariantMarshaler
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
    private const ushort VT_I1 = 16;
    private const ushort VT_UI1 = 17;
    private const ushort VT_UI2 = 18;
    private const ushort VT_UI4 = 19;
    private const ushort VT_I8 = 20;
    private const ushort VT_UI8 = 21;
    private const ushort VT_INT = 22;
    private const ushort VT_UINT = 23;
    private const ushort VT_FILETIME = 64;
    private const ushort VT_ARRAY = 0x2000;

    public static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

    public static void WriteVariant(IntPtr dest, OpcVariant variant)
    {
        if (dest == IntPtr.Zero)
        {
            return;
        }

        WriteEmpty(dest);
        ushort vt = (ushort)variant.Type;
        Marshal.WriteInt16(dest, unchecked((short)vt));
        if ((vt & VT_ARRAY) != 0)
        {
            WriteSafeArrayPayload(dest, variant);
            return;
        }

        WriteScalarPayload(dest, vt, variant.Boxed);
    }

    public static void ClearVariant(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }

        ushort vt = unchecked((ushort)Marshal.ReadInt16(ptr));
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
            IntPtr safeArray = Marshal.ReadIntPtr(ptr, ValueOffset);
            if (safeArray != IntPtr.Zero)
            {
                FreeSafeArray(safeArray, (ushort)(vt & ~VT_ARRAY));
            }
        }

        WriteEmpty(ptr);
    }

    private static int ValueOffset => 8;

    private static void WriteEmpty(IntPtr dest) => NativeMemory.Clear((void*)dest, (nuint)VariantSize);

    [SuppressMessage("Design", "CA1502:Avoid excessive complexity", Justification = "VARIANT scalar dispatch requires one branch per VARENUM code.")]
    private static void WriteScalarPayload(IntPtr dest, ushort vt, object? boxed)
    {
        IntPtr value = dest + ValueOffset;
        switch (vt)
        {
            case VT_I1:
                Marshal.WriteByte(value, unchecked((byte)(sbyte)(boxed ?? (sbyte)0)));
                break;
            case VT_UI1:
                Marshal.WriteByte(value, (byte)(boxed ?? (byte)0));
                break;
            case VT_I2:
                Marshal.WriteInt16(value, (short)(boxed ?? (short)0));
                break;
            case VT_UI2:
                Marshal.WriteInt16(value, unchecked((short)(ushort)(boxed ?? (ushort)0)));
                break;
            case VT_BOOL:
                Marshal.WriteInt16(value, boxed is bool b && b ? unchecked((short)0xFFFF) : (short)0);
                break;
            case VT_I4:
            case VT_INT:
            case VT_ERROR:
                Marshal.WriteInt32(value, (int)(boxed ?? 0));
                break;
            case VT_UI4:
            case VT_UINT:
                Marshal.WriteInt32(value, unchecked((int)(uint)(boxed ?? 0u)));
                break;
            case VT_R4:
                Marshal.WriteInt32(value, BitConverter.SingleToInt32Bits((float)(boxed ?? 0f)));
                break;
            case VT_I8:
            case VT_FILETIME:
            case VT_CY:
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
                Marshal.WriteIntPtr(value, boxed is null ? IntPtr.Zero : Marshal.StringToBSTR((string)boxed));
                break;
            case VT_EMPTY:
            case VT_NULL:
                break;
        }
    }

    private static void WriteSafeArrayPayload(IntPtr dest, OpcVariant variant)
    {
        OpcSafeArray? array = variant.AsSafeArray();
        Marshal.WriteIntPtr(dest, ValueOffset, array is null || array.Data.Length == 0 ? IntPtr.Zero : AllocateSafeArray(array));
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte sizes for descriptor and data buffers.")]
    private static IntPtr AllocateSafeArray(OpcSafeArray array)
    {
        ushort baseVt = (ushort)array.ElementType;
        uint elementSize = (uint)ElementSizeOf(baseVt);
        uint count = unchecked((uint)array.Data.Length);
        int pvDataOffset = 8 + IntPtr.Size;
        int boundsOffset = pvDataOffset + IntPtr.Size;
        int totalDescriptorSize = boundsOffset + 8;
        IntPtr descriptor = Marshal.AllocCoTaskMem(totalDescriptorSize);
        IntPtr dataBuffer = Marshal.AllocCoTaskMem(checked((int)(elementSize * count)));
        Marshal.WriteInt16(descriptor, 0, 1);
        Marshal.WriteInt16(descriptor, 2, 0x10);
        Marshal.WriteInt32(descriptor, 4, (int)elementSize);
        Marshal.WriteInt32(descriptor, 8, 0);
        Marshal.WriteIntPtr(descriptor, pvDataOffset, dataBuffer);
        Marshal.WriteInt32(descriptor, boundsOffset, (int)count);
        Marshal.WriteInt32(descriptor, boundsOffset + 4, 0);
        WriteSafeArrayData(dataBuffer, array, baseVt, elementSize);
        return descriptor;
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
                int count = Marshal.ReadInt32(descriptor, boundsOffset);
                for (int i = 0; i < count; i++)
                {
                    IntPtr bstr = Marshal.ReadIntPtr(dataBuffer, checked(i * IntPtr.Size));
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

    private static int ElementSizeOf(ushort baseVt) => baseVt switch
    {
        VT_I1 or VT_UI1 => 1,
        VT_I2 or VT_UI2 or VT_BOOL => 2,
        VT_I4 or VT_UI4 or VT_INT or VT_UINT or VT_ERROR or VT_R4 => 4,
        VT_I8 or VT_UI8 or VT_R8 or VT_DATE or VT_FILETIME or VT_CY => 8,
        VT_BSTR => IntPtr.Size,
        _ => IntPtr.Size,
    };

    [SuppressMessage("Design", "CA1502:Avoid excessive complexity", Justification = "SAFEARRAY element dispatch requires one branch per VARENUM code.")]
    private static void WriteSafeArrayData(IntPtr dataBuffer, OpcSafeArray array, ushort baseVt, uint elementSize)
    {
        Array data = array.Data;
        for (int i = 0; i < data.Length; i++)
        {
            IntPtr slot = dataBuffer + checked((int)(i * elementSize));
            object? value = data.GetValue(i);
            switch (baseVt)
            {
                case VT_I1: Marshal.WriteByte(slot, unchecked((byte)(sbyte)(value ?? (sbyte)0))); break;
                case VT_UI1: Marshal.WriteByte(slot, (byte)(value ?? (byte)0)); break;
                case VT_I2: Marshal.WriteInt16(slot, (short)(value ?? (short)0)); break;
                case VT_UI2: Marshal.WriteInt16(slot, unchecked((short)(ushort)(value ?? (ushort)0))); break;
                case VT_BOOL: Marshal.WriteInt16(slot, value is bool b && b ? unchecked((short)0xFFFF) : (short)0); break;
                case VT_I4: case VT_INT: case VT_ERROR: Marshal.WriteInt32(slot, (int)(value ?? 0)); break;
                case VT_UI4: case VT_UINT: Marshal.WriteInt32(slot, unchecked((int)(uint)(value ?? 0u))); break;
                case VT_R4: Marshal.WriteInt32(slot, BitConverter.SingleToInt32Bits((float)(value ?? 0f))); break;
                case VT_I8: case VT_FILETIME: case VT_CY: Marshal.WriteInt64(slot, (long)(value ?? 0L)); break;
                case VT_UI8: Marshal.WriteInt64(slot, unchecked((long)(ulong)(value ?? 0ul))); break;
                case VT_R8: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits((double)(value ?? 0d))); break;
                case VT_DATE: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits(((DateTime?)value ?? DateTime.UnixEpoch).ToOADate())); break;
                case VT_BSTR: Marshal.WriteIntPtr(slot, value is null ? IntPtr.Zero : Marshal.StringToBSTR((string)value)); break;
            }
        }
    }
}
