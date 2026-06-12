//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// IOPCSyncIO(2) method bodies bound into the <see cref="OpcDaGroupCcw"/> vtables.
/// </summary>
/// <remarks>
/// Value-bearing sync I/O methods bridge native OPC arrays to the managed group
/// implementation using <see cref="ComVariantMarshaler"/> for VARIANT slots.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcDaGroupCcwSyncIoMethods
{
    private const int OpcItemStateVariantOffset = 16;
    private const int OpcItemVqtTrailerSize = 24;

    private static int OpcItemStateSize => OpcItemStateVariantOffset + ComVariantMarshaler.VariantSize;

    private static int OpcItemVqtSize => ComVariantMarshaler.VariantSize + OpcItemVqtTrailerSize;

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Read(IntPtr pThis, uint dwSource, uint dwCount, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        ZeroOut(ppItemValues);
        ZeroOut(ppErrors);
        if (!HasHandleOutArgs(dwCount, phServer, ppItemValues, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            int count = checked((int)dwCount);
            int[] handles = ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcItemState[] states = group!.ReadAsync(unchecked((int)dwSource), handles, out int[] errors,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            AssignReadOuts(ppItemValues, ppErrors, states, errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Write(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr pItemValues, IntPtr* ppErrors)
    {
        ZeroOut(ppErrors);
        if (!HasWriteArgs(dwCount, phServer, pItemValues, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            int count = checked((int)dwCount);
            int[] handles = ReadInt32Array(phServer, count);
            OpcVariant[] values = ReadVariantArray(pItemValues, count);
#pragma warning disable VSTHRD002
            int[] errors = group!.WriteAsync(handles, values, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppErrors = AllocateInt32Array(errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int ReadMaxAge(
        IntPtr pThis,
        uint dwCount,
        IntPtr phServer,
        IntPtr pdwMaxAge,
        IntPtr* ppvValues,
        IntPtr* ppwQualities,
        IntPtr* ppftTimeStamps,
        IntPtr* ppErrors)
    {
        ZeroReadMaxAgeOuts(ppvValues, ppwQualities, ppftTimeStamps, ppErrors);
        if (!HasReadMaxAgeArgs(dwCount, phServer, pdwMaxAge, ppvValues, ppwQualities, ppftTimeStamps, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            int count = checked((int)dwCount);
            int[] handles = ReadInt32Array(phServer, count);
            int[] maxAges = ReadInt32Array(pdwMaxAge, count);
#pragma warning disable VSTHRD002
            group!.ReadMaxAgeAsync(handles, maxAges, out OpcVariant[] values, out ushort[] qualities,
                out long[] timestamps, out int[] errors, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            AssignReadMaxAgeOuts(ppvValues, ppwQualities, ppftTimeStamps, ppErrors, values, qualities, timestamps, errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int WriteVqt(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr pItemVqt, IntPtr* ppErrors)
    {
        ZeroOut(ppErrors);
        if (!HasWriteArgs(dwCount, phServer, pItemVqt, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            int count = checked((int)dwCount);
            int[] handles = ReadInt32Array(phServer, count);
            OpcItemVqt[] values = ReadOpcItemVqtArray(pItemVqt, count);
#pragma warning disable VSTHRD002
            int[] errors = group!.WriteVqtAsync(handles, values, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppErrors = AllocateInt32Array(errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolveGroup(IntPtr pThis, out OpcDaGroup? group)
    {
        group = OpcDaGroupCcw.ResolveGroup(pThis);
        return group is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentNullException => OpcDaGroupCcw.E_INVALIDARG,
        ArgumentException => OpcDaGroupCcw.E_INVALIDARG,
        _ => OpcDaGroupCcw.E_FAIL,
    };

    private static bool HasHandleOutArgs(uint count, IntPtr handles, IntPtr* ppValues, IntPtr* ppErrors) =>
        count <= int.MaxValue && ppValues != null && ppErrors != null && (count == 0 || handles != IntPtr.Zero);

    private static bool HasWriteArgs(uint count, IntPtr handles, IntPtr values, IntPtr* ppErrors) =>
        count <= int.MaxValue && ppErrors != null && (count == 0 || (handles != IntPtr.Zero && values != IntPtr.Zero));

    private static bool HasReadMaxAgeArgs(
        uint count,
        IntPtr handles,
        IntPtr maxAges,
        IntPtr* values,
        IntPtr* qualities,
        IntPtr* timestamps,
        IntPtr* errors) =>
        HasHandleOutArgs(count, handles, values, errors) && qualities != null && timestamps != null &&
        (count == 0 || maxAges != IntPtr.Zero);

    private static void AssignReadOuts(IntPtr* ppItemValues, IntPtr* ppErrors, OpcItemState[] states, int[] errors)
    {
        IntPtr itemValuesPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            itemValuesPtr = AllocateOpcItemStateArray(states);
            errorsPtr = AllocateInt32Array(errors);
            *ppItemValues = itemValuesPtr;
            *ppErrors = errorsPtr;
            itemValuesPtr = IntPtr.Zero;
            errorsPtr = IntPtr.Zero;
        }
        finally
        {
            FreeOpcItemStateArray(itemValuesPtr, states.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    private static void AssignReadMaxAgeOuts(
        IntPtr* ppvValues,
        IntPtr* ppwQualities,
        IntPtr* ppftTimeStamps,
        IntPtr* ppErrors,
        OpcVariant[] values,
        ushort[] qualities,
        long[] timestamps,
        int[] errors)
    {
        IntPtr valuesPtr = IntPtr.Zero;
        IntPtr qualitiesPtr = IntPtr.Zero;
        IntPtr timestampsPtr = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            valuesPtr = AllocateVariantArray(values);
            qualitiesPtr = AllocateUInt16Array(qualities);
            timestampsPtr = AllocateInt64Array(timestamps);
            errorsPtr = AllocateInt32Array(errors);
            AssignReadMaxAgePointers(ppvValues, ppwQualities, ppftTimeStamps, ppErrors, valuesPtr, qualitiesPtr, timestampsPtr, errorsPtr);
            valuesPtr = qualitiesPtr = timestampsPtr = errorsPtr = IntPtr.Zero;
        }
        finally
        {
            FreeVariantArray(valuesPtr, values.Length);
            FreeCoTaskMem(qualitiesPtr, timestampsPtr, errorsPtr);
        }
    }

    private static void AssignReadMaxAgePointers(
        IntPtr* ppvValues,
        IntPtr* ppwQualities,
        IntPtr* ppftTimeStamps,
        IntPtr* ppErrors,
        IntPtr valuesPtr,
        IntPtr qualitiesPtr,
        IntPtr timestampsPtr,
        IntPtr errorsPtr)
    {
        *ppvValues = valuesPtr;
        *ppwQualities = qualitiesPtr;
        *ppftTimeStamps = timestampsPtr;
        *ppErrors = errorsPtr;
    }

    private static int[] ReadInt32Array(IntPtr ptr, int count)
    {
        var values = new int[count];
        if (count > 0)
        {
            Marshal.Copy(ptr, values, 0, count);
        }
        return values;
    }

    private static OpcVariant[] ReadVariantArray(IntPtr ptr, int count)
    {
        var values = new OpcVariant[count];
        int variantSize = ComVariantMarshaler.VariantSize;
        for (int i = 0; i < count; i++)
        {
            values[i] = ComVariantMarshaler.ReadVariant(IntPtr.Add(ptr, checked(i * variantSize)));
        }
        return values;
    }

    private static OpcItemVqt[] ReadOpcItemVqtArray(IntPtr ptr, int count)
    {
        var values = new OpcItemVqt[count];
        int size = OpcItemVqtSize;
        for (int i = 0; i < count; i++)
        {
            values[i] = ReadOpcItemVqt(IntPtr.Add(ptr, checked(i * size)));
        }
        return values;
    }

    private static OpcItemVqt ReadOpcItemVqt(IntPtr slot)
    {
        int variantSize = ComVariantMarshaler.VariantSize;
        OpcVariant value = ComVariantMarshaler.ReadVariant(slot);
        bool qualitySpecified = Marshal.ReadInt32(slot, variantSize) != 0;
        ushort quality = unchecked((ushort)Marshal.ReadInt16(slot, variantSize + 4));
        bool timestampSpecified = Marshal.ReadInt32(slot, variantSize + 8) != 0;
        long timestamp = Marshal.ReadInt64(slot, variantSize + 16);
        return new OpcItemVqt(value, qualitySpecified ? new OpcQuality(quality) : null,
            timestampSpecified ? DateTimeOffset.FromFileTime(timestamp) : null);
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateOpcItemStateArray(OpcItemState[] states)
    {
        int size = OpcItemStateSize;
        int byteCount = Math.Max(1, checked(states.Length * size));
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        int written = 0;
        try
        {
            for (; written < states.Length; written++)
            {
                WriteOpcItemState(IntPtr.Add(ptr, checked(written * size)), states[written]);
            }
            return ptr;
        }
        catch
        {
            FreeOpcItemStateArray(ptr, written);
            throw;
        }
    }

    private static void WriteOpcItemState(IntPtr slot, OpcItemState state)
    {
        Marshal.WriteInt32(slot, state.ClientHandle);
        Marshal.WriteInt64(slot, 4, state.Timestamp.ToFileTime());
        Marshal.WriteInt16(slot, 12, unchecked((short)state.Quality.RawValue));
        Marshal.WriteInt16(slot, 14, 0);
        ComVariantMarshaler.WriteVariant(IntPtr.Add(slot, OpcItemStateVariantOffset), state.Value);
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateVariantArray(OpcVariant[] values)
    {
        int variantSize = ComVariantMarshaler.VariantSize;
        int byteCount = Math.Max(1, checked(values.Length * variantSize));
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        int written = 0;
        try
        {
            for (; written < values.Length; written++)
            {
                ComVariantMarshaler.WriteVariant(IntPtr.Add(ptr, checked(written * variantSize)), values[written]);
            }
            return ptr;
        }
        catch
        {
            FreeVariantArray(ptr, written);
            throw;
        }
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInt32Array(int[] values)
    {
        int byteCount = Math.Max(1, checked(values.Length * sizeof(int)));
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        if (values.Length > 0)
        {
            Marshal.Copy(values, 0, ptr, values.Length);
        }
        return ptr;
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateUInt16Array(ushort[] values)
    {
        int byteCount = Math.Max(1, checked(values.Length * sizeof(ushort)));
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        for (int i = 0; i < values.Length; i++)
        {
            Marshal.WriteInt16(ptr, checked(i * sizeof(ushort)), unchecked((short)values[i]));
        }
        return ptr;
    }

    [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInt64Array(long[] values)
    {
        int byteCount = Math.Max(1, checked(values.Length * sizeof(long)));
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        Marshal.Copy(values, 0, ptr, values.Length);
        return ptr;
    }

    private static void FreeOpcItemStateArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }
        int size = OpcItemStateSize;
        for (int i = 0; i < count; i++)
        {
            ComVariantMarshaler.ClearVariant(IntPtr.Add(ptr, checked(i * size + OpcItemStateVariantOffset)));
        }
        Marshal.FreeCoTaskMem(ptr);
    }

    private static void FreeVariantArray(IntPtr ptr, int count)
    {
        if (ptr == IntPtr.Zero)
        {
            return;
        }
        int variantSize = ComVariantMarshaler.VariantSize;
        for (int i = 0; i < count; i++)
        {
            ComVariantMarshaler.ClearVariant(IntPtr.Add(ptr, checked(i * variantSize)));
        }
        Marshal.FreeCoTaskMem(ptr);
    }

    private static void FreeCoTaskMem(params IntPtr[] pointers)
    {
        foreach (IntPtr pointer in pointers)
        {
            Marshal.FreeCoTaskMem(pointer);
        }
    }

    private static void ZeroReadMaxAgeOuts(IntPtr* values, IntPtr* qualities, IntPtr* timestamps, IntPtr* errors)
    {
        ZeroOut(values);
        ZeroOut(qualities);
        ZeroOut(timestamps);
        ZeroOut(errors);
    }

    private static void ZeroOut(IntPtr* ppv)
    {
        if (ppv != null)
        {
            *ppv = IntPtr.Zero;
        }
    }
}
