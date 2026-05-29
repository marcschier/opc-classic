//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Ae.Hosting.Windows;

/// <summary>Helpers for OPC AE Windows CCW CoTaskMem array marshaling.</summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcAeArrayMarshaler
{
    private const ushort VT_EMPTY = 0;
    private const ushort VT_NULL = 1;
    private const ushort VT_I2 = 2;
    private const ushort VT_I4 = 3;
    private const ushort VT_R4 = 4;
    private const ushort VT_R8 = 5;
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
    private const ushort VT_FILETIME = 64;
    private const ushort VT_ARRAY = 0x2000;

    public static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

    public static void AllocateBstrArray(IReadOnlyList<string?> strings, out int count, out IntPtr arrayPtr)
    {
        ArgumentNullException.ThrowIfNull(strings);
        count = strings.Count;
        if (count == 0)
        {
            arrayPtr = IntPtr.Zero;
            return;
        }

        arrayPtr = Marshal.AllocCoTaskMem(checked((count + 1) * IntPtr.Size));
        for (int i = 0; i <= count; i++)
        {
            Marshal.WriteIntPtr(arrayPtr, i * IntPtr.Size, IntPtr.Zero);
        }

        try
        {
            for (int i = 0; i < count; i++)
            {
                Marshal.WriteIntPtr(arrayPtr, i * IntPtr.Size, Marshal.StringToBSTR(strings[i] ?? string.Empty));
            }
        }
        catch
        {
            FreeBstrArray(arrayPtr, count);
            arrayPtr = IntPtr.Zero;
            throw;
        }
    }

    public static void AllocateDwordArray(IReadOnlyList<int> values, out int count, out IntPtr arrayPtr)
    {
        ArgumentNullException.ThrowIfNull(values);
        count = values.Count;
        if (count == 0)
        {
            arrayPtr = IntPtr.Zero;
            return;
        }

        arrayPtr = Marshal.AllocCoTaskMem(checked(count * sizeof(int)));
        for (int i = 0; i < count; i++)
        {
            Marshal.WriteInt32(arrayPtr, i * sizeof(int), values[i]);
        }
    }

    public static void AllocateUInt16Array(IReadOnlyList<ushort> values, out int count, out IntPtr arrayPtr)
    {
        ArgumentNullException.ThrowIfNull(values);
        count = values.Count;
        if (count == 0)
        {
            arrayPtr = IntPtr.Zero;
            return;
        }

        arrayPtr = Marshal.AllocCoTaskMem(checked(count * sizeof(ushort)));
        for (int i = 0; i < count; i++)
        {
            Marshal.WriteInt16(arrayPtr, i * sizeof(ushort), unchecked((short)values[i]));
        }
    }

    public static void AllocateHResultArray(IReadOnlyList<int> hresults, out int count, out IntPtr arrayPtr) =>
        AllocateDwordArray(hresults, out count, out arrayPtr);

    public static void AllocateGuidArray(IReadOnlyList<Guid> guids, out int count, out IntPtr arrayPtr)
    {
        ArgumentNullException.ThrowIfNull(guids);
        count = guids.Count;
        if (count == 0)
        {
            arrayPtr = IntPtr.Zero;
            return;
        }

        arrayPtr = Marshal.AllocCoTaskMem(checked(count * sizeof(Guid)));
        Guid* destination = (Guid*)arrayPtr;
        for (int i = 0; i < count; i++)
        {
            destination[i] = guids[i];
        }
    }

    public static string[] ReadBstrArray(IntPtr arrayPtr, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
        {
            return [];
        }
        if (arrayPtr == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(arrayPtr));
        }

        var values = new string[count];
        for (int i = 0; i < count; i++)
        {
            IntPtr valuePtr = Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size);
            values[i] = valuePtr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringBSTR(valuePtr) ?? string.Empty;
        }
        return values;
    }

    internal static int[] ReadDwordArray(IntPtr arrayPtr, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
        {
            return [];
        }
        if (arrayPtr == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(arrayPtr));
        }

        var values = new int[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = Marshal.ReadInt32(arrayPtr, i * sizeof(int));
        }
        return values;
    }

    internal static long[] ReadFileTimeArray(IntPtr arrayPtr, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
        {
            return [];
        }
        if (arrayPtr == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(arrayPtr));
        }

        var values = new long[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = Marshal.ReadInt64(arrayPtr, i * sizeof(long));
        }
        return values;
    }

    [SuppressMessage("Maintainability", "MA0051:Method is too long", Justification = "Native OPCCONDITIONSTATE allocation owns correlated cleanup state.")]
    internal static IntPtr AllocateConditionState(OpcConditionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        IntPtr activeSubConditionPtr = IntPtr.Zero;
        IntPtr activeDefinitionPtr = IntPtr.Zero;
        IntPtr activeDescriptionPtr = IntPtr.Zero;
        IntPtr acknowledgerPtr = IntPtr.Zero;
        IntPtr commentPtr = IntPtr.Zero;
        IntPtr subConditionNamesPtr = IntPtr.Zero;
        IntPtr subConditionDefinitionsPtr = IntPtr.Zero;
        IntPtr subConditionSeveritiesPtr = IntPtr.Zero;
        IntPtr subConditionDescriptionsPtr = IntPtr.Zero;
        IntPtr eventAttributesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        IntPtr statePtr = IntPtr.Zero;

        try
        {
            activeSubConditionPtr = AllocateBstr(state.ActiveSubCondition);
            activeDefinitionPtr = AllocateBstr(state.ActiveSubConditionDefinition);
            activeDescriptionPtr = AllocateBstr(state.ActiveSubConditionDescription);
            acknowledgerPtr = AllocateBstr(state.AcknowledgerId);
            commentPtr = AllocateBstr(state.Comment);
            AllocateBstrArray(state.SubConditionNames, out int subConditionNameCount, out subConditionNamesPtr);
            AllocateBstrArray(state.SubConditionDefinitions, out int subConditionDefinitionCount, out subConditionDefinitionsPtr);
            AllocateUInt32Array(state.SubConditionSeverities, out int subConditionSeverityCount, out subConditionSeveritiesPtr);
            AllocateBstrArray(state.SubConditionDescriptions, out int subConditionDescriptionCount, out subConditionDescriptionsPtr);
            EnsureSameLength(subConditionNameCount, subConditionDefinitionCount, nameof(state.SubConditionDefinitions));
            EnsureSameLength(subConditionNameCount, subConditionSeverityCount, nameof(state.SubConditionSeverities));
            EnsureSameLength(subConditionNameCount, subConditionDescriptionCount, nameof(state.SubConditionDescriptions));
            AllocateVariantArray(state.EventAttributes, out int eventAttributeCount, out eventAttributesPtr);
            AllocateHResultArray(state.Errors, out int errorCount, out errorsPtr);
            EnsureSameLength(eventAttributeCount, errorCount, nameof(state.Errors));

            var native = new OPCCONDITIONSTATE_NATIVE
            {
                wState = state.State,
                wReserved1 = 0,
                szActiveSubCondition = activeSubConditionPtr,
                szASCDefinition = activeDefinitionPtr,
                dwASCSeverity = state.ActiveSubConditionSeverity,
                szASCDescription = activeDescriptionPtr,
                wQuality = state.Quality.RawValue,
                wReserved2 = 0,
                ftLastAckTime = ToFileTime(state.LastAckTime),
                ftSubCondLastActive = ToFileTime(state.SubConditionLastActive),
                ftCondLastActive = ToFileTime(state.ConditionLastActive),
                ftCondLastInactive = ToFileTime(state.ConditionLastInactive),
                szAcknowledgerID = acknowledgerPtr,
                szComment = commentPtr,
                dwNumSCs = subConditionNameCount,
                pszSCNames = subConditionNamesPtr,
                pszSCDefinitions = subConditionDefinitionsPtr,
                pdwSCSeverities = subConditionSeveritiesPtr,
                pszSCDescriptions = subConditionDescriptionsPtr,
                dwNumEventAttrs = eventAttributeCount,
                pEventAttributes = eventAttributesPtr,
                pErrors = errorsPtr,
            };

            statePtr = Marshal.AllocCoTaskMem(Marshal.SizeOf<OPCCONDITIONSTATE_NATIVE>());
            Marshal.StructureToPtr(native, statePtr, fDeleteOld: false);
            activeSubConditionPtr = IntPtr.Zero;
            activeDefinitionPtr = IntPtr.Zero;
            activeDescriptionPtr = IntPtr.Zero;
            acknowledgerPtr = IntPtr.Zero;
            commentPtr = IntPtr.Zero;
            subConditionNamesPtr = IntPtr.Zero;
            subConditionDefinitionsPtr = IntPtr.Zero;
            subConditionSeveritiesPtr = IntPtr.Zero;
            subConditionDescriptionsPtr = IntPtr.Zero;
            eventAttributesPtr = IntPtr.Zero;
            errorsPtr = IntPtr.Zero;
            return statePtr;
        }
        catch
        {
            FreeBstr(activeSubConditionPtr);
            FreeBstr(activeDefinitionPtr);
            FreeBstr(activeDescriptionPtr);
            FreeBstr(acknowledgerPtr);
            FreeBstr(commentPtr);
            FreeBstrArray(subConditionNamesPtr, state.SubConditionCount);
            FreeBstrArray(subConditionDefinitionsPtr, state.SubConditionCount);
            FreeCoTaskMem(subConditionSeveritiesPtr);
            FreeBstrArray(subConditionDescriptionsPtr, state.SubConditionCount);
            FreeVariantArray(eventAttributesPtr, state.EventAttributeCount);
            FreeCoTaskMem(errorsPtr);
            FreeCoTaskMem(statePtr);
            throw;
        }
    }

    internal static void FreeCoTaskMem(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(ptr);
        }
    }

    internal static void FreeBstrArray(IntPtr arrayPtr, int count)
    {
        if (arrayPtr == IntPtr.Zero)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            FreeBstr(Marshal.ReadIntPtr(arrayPtr, i * IntPtr.Size));
        }
        Marshal.FreeCoTaskMem(arrayPtr);
    }

    internal static void FreeVariantArray(IntPtr arrayPtr, int count)
    {
        if (arrayPtr == IntPtr.Zero)
        {
            return;
        }

        int variantSize = VariantSize;
        for (int i = 0; i < count; i++)
        {
            ClearVariant(arrayPtr + (i * variantSize));
        }
        Marshal.FreeCoTaskMem(arrayPtr);
    }

    private static void AllocateUInt32Array(IReadOnlyList<uint> values, out int count, out IntPtr arrayPtr)
    {
        ArgumentNullException.ThrowIfNull(values);
        count = values.Count;
        if (count == 0)
        {
            arrayPtr = IntPtr.Zero;
            return;
        }

        arrayPtr = Marshal.AllocCoTaskMem(checked(count * sizeof(int)));
        for (int i = 0; i < count; i++)
        {
            Marshal.WriteInt32(arrayPtr, i * sizeof(int), unchecked((int)values[i]));
        }
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static void AllocateVariantArray(IReadOnlyList<OpcVariant> values, out int count, out IntPtr arrayPtr)
    {
        ArgumentNullException.ThrowIfNull(values);
        count = values.Count;
        if (count == 0)
        {
            arrayPtr = IntPtr.Zero;
            return;
        }

        int variantSize = VariantSize;
        arrayPtr = Marshal.AllocCoTaskMem(checked(count * variantSize));
        NativeMemory.Clear((void*)arrayPtr, (nuint)(count * variantSize));
        try
        {
            for (int i = 0; i < count; i++)
            {
                WriteVariant(arrayPtr + (i * variantSize), values[i]);
            }
        }
        catch
        {
            FreeVariantArray(arrayPtr, count);
            arrayPtr = IntPtr.Zero;
            throw;
        }
    }

    private static IntPtr AllocateBstr(string? value) =>
        value is null ? IntPtr.Zero : Marshal.StringToBSTR(value);

    private static void FreeBstr(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Marshal.FreeBSTR(ptr);
        }
    }

    private static void EnsureSameLength(int expected, int actual, string arrayName)
    {
        if (actual != expected)
        {
            throw new ArgumentException($"{arrayName} length {actual} must equal {expected}.", arrayName);
        }
    }

    private static long ToFileTime(DateTimeOffset value) => value.UtcDateTime.ToFileTimeUtc();

    private static int ValueOffset => 8;

    private static void WriteVariant(IntPtr dest, OpcVariant variant)
    {
        NativeMemory.Clear((void*)dest, (nuint)VariantSize);
        ushort vt = (ushort)variant.Type;
        Marshal.WriteInt16(dest, unchecked((short)vt));
        if ((vt & VT_ARRAY) != 0)
        {
            WriteSafeArrayPayload(dest, variant);
            return;
        }
        WriteScalarPayload(dest, vt, variant.Boxed);
    }

    private static void ClearVariant(IntPtr ptr)
    {
        ushort vt = unchecked((ushort)Marshal.ReadInt16(ptr));
        if (vt == VT_BSTR)
        {
            FreeBstr(Marshal.ReadIntPtr(ptr, ValueOffset));
        }
        else if ((vt & VT_ARRAY) != 0)
        {
            IntPtr safeArrayPtr = Marshal.ReadIntPtr(ptr, ValueOffset);
            if (safeArrayPtr != IntPtr.Zero)
            {
                FreeSafeArray(safeArrayPtr, unchecked((ushort)(vt & ~VT_ARRAY)));
            }
        }
        NativeMemory.Clear((void*)ptr, (nuint)VariantSize);
    }

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
            case VT_FILETIME:
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
        }
    }

    private static void WriteSafeArrayPayload(IntPtr dest, OpcVariant variant)
    {
        OpcSafeArray? array = variant.AsSafeArray();
        if (array is null || array.Data.Length == 0)
        {
            Marshal.WriteIntPtr(dest, ValueOffset, IntPtr.Zero);
            return;
        }
        Marshal.WriteIntPtr(dest, ValueOffset, AllocateSafeArray(array));
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit SAFEARRAY descriptor and data byte sizes.")]
    private static IntPtr AllocateSafeArray(OpcSafeArray array)
    {
        ushort baseVt = (ushort)array.ElementType;
        int elementSize = ElementSizeOf(baseVt);
        int count = array.Data.Length;
        int pvDataOffset = 8 + IntPtr.Size;
        int boundsOffset = pvDataOffset + IntPtr.Size;
        int descriptorSize = boundsOffset + 8;
        IntPtr descriptor = IntPtr.Zero;
        IntPtr dataBuffer = IntPtr.Zero;

        try
        {
            descriptor = Marshal.AllocCoTaskMem(descriptorSize);
            NativeMemory.Clear((void*)descriptor, (nuint)descriptorSize);
            dataBuffer = Marshal.AllocCoTaskMem(checked(elementSize * count));
            NativeMemory.Clear((void*)dataBuffer, (nuint)(elementSize * count));
            Marshal.WriteInt16(descriptor, 0, 1);
            Marshal.WriteInt16(descriptor, 2, 0x10);
            Marshal.WriteInt32(descriptor, 4, elementSize);
            Marshal.WriteInt32(descriptor, 8, 0);
            Marshal.WriteIntPtr(descriptor, pvDataOffset, dataBuffer);
            Marshal.WriteInt32(descriptor, boundsOffset, count);
            Marshal.WriteInt32(descriptor, boundsOffset + 4, 0);
            WriteSafeArrayData(dataBuffer, array, baseVt, elementSize);
            dataBuffer = IntPtr.Zero;
            return descriptor;
        }
        catch
        {
            FreeCoTaskMem(dataBuffer);
            FreeCoTaskMem(descriptor);
            throw;
        }
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
                    FreeBstr(Marshal.ReadIntPtr(dataBuffer, i * IntPtr.Size));
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
        VT_I4 or VT_UI4 or VT_ERROR or VT_R4 => 4,
        VT_I8 or VT_UI8 or VT_R8 or VT_DATE or VT_FILETIME => 8,
        VT_BSTR => IntPtr.Size,
        _ => IntPtr.Size,
    };

    [SuppressMessage("Design", "CA1502:Avoid excessive complexity", Justification = "SAFEARRAY element dispatch requires one branch per VARENUM code.")]
    private static void WriteSafeArrayData(IntPtr dataBuffer, OpcSafeArray array, ushort baseVt, int elementSize)
    {
        Array data = array.Data;
        for (int i = 0; i < data.Length; i++)
        {
            IntPtr slot = dataBuffer + (i * elementSize);
            object? value = data.GetValue(i);
            switch (baseVt)
            {
                case VT_I1: Marshal.WriteByte(slot, unchecked((byte)(sbyte)(value ?? (sbyte)0))); break;
                case VT_UI1: Marshal.WriteByte(slot, (byte)(value ?? (byte)0)); break;
                case VT_I2: Marshal.WriteInt16(slot, (short)(value ?? (short)0)); break;
                case VT_UI2: Marshal.WriteInt16(slot, unchecked((short)(ushort)(value ?? (ushort)0))); break;
                case VT_BOOL: Marshal.WriteInt16(slot, value is bool b && b ? unchecked((short)0xFFFF) : (short)0); break;
                case VT_I4: case VT_ERROR: Marshal.WriteInt32(slot, (int)(value ?? 0)); break;
                case VT_UI4: Marshal.WriteInt32(slot, unchecked((int)(uint)(value ?? 0u))); break;
                case VT_R4: Marshal.WriteInt32(slot, BitConverter.SingleToInt32Bits((float)(value ?? 0f))); break;
                case VT_I8: case VT_FILETIME: Marshal.WriteInt64(slot, (long)(value ?? 0L)); break;
                case VT_UI8: Marshal.WriteInt64(slot, unchecked((long)(ulong)(value ?? 0ul))); break;
                case VT_R8: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits((double)(value ?? 0d))); break;
                case VT_DATE: Marshal.WriteInt64(slot, BitConverter.DoubleToInt64Bits(((DateTime?)value ?? DateTime.UnixEpoch).ToOADate())); break;
                case VT_BSTR: Marshal.WriteIntPtr(slot, AllocateBstr(value as string)); break;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct OPCCONDITIONSTATE_NATIVE
    {
        public ushort wState;
        public ushort wReserved1;
        public IntPtr szActiveSubCondition;
        public IntPtr szASCDefinition;
        public uint dwASCSeverity;
        public IntPtr szASCDescription;
        public ushort wQuality;
        public ushort wReserved2;
        public long ftLastAckTime;
        public long ftSubCondLastActive;
        public long ftCondLastActive;
        public long ftCondLastInactive;
        public IntPtr szAcknowledgerID;
        public IntPtr szComment;
        public int dwNumSCs;
        public IntPtr pszSCNames;
        public IntPtr pszSCDefinitions;
        public IntPtr pdwSCSeverities;
        public IntPtr pszSCDescriptions;
        public int dwNumEventAttrs;
        public IntPtr pEventAttributes;
        public IntPtr pErrors;
    }
}
