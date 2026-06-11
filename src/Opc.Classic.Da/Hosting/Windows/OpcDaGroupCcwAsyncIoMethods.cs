//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// IOPCAsyncIO2/3 method bodies bound into the <see cref="OpcDaGroupCcw"/> vtables.
/// </summary>
/// <remarks>
/// Handle-array methods dispatch to the managed group. VARIANT-bearing writes and
/// OPCITEMVQT writes use <see cref="ComVariantMarshaler"/> for native VARIANT slots.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcDaGroupCcwAsyncIoMethods
{
    private const int S_FALSE = 1;
    private const int OpcItemVqtTrailerSize = 24;

    private static int OpcItemVqtSize => ComVariantMarshaler.VariantSize + OpcItemVqtTrailerSize;

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Read(IntPtr pThis, uint dwCount, IntPtr phServer, uint dwTransactionId, uint* pdwCancelId, IntPtr* ppErrors)
    {
        ZeroAsyncOuts(pdwCancelId, ppErrors);
        if (!HasAsyncHandleArgs(dwCount, phServer, pdwCancelId, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            int[] handles = ReadInt32Array(phServer, checked((int)dwCount));
#pragma warning disable VSTHRD002
            int cancelId = ((IOPCAsyncIO2)group!).ReadAsync(handles, unchecked((int)dwTransactionId), out int[] errors,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pdwCancelId = unchecked((uint)cancelId);
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
    public static int Write(
        IntPtr pThis,
        uint dwCount,
        IntPtr phServer,
        IntPtr pItemValues,
        uint dwTransactionId,
        uint* pdwCancelId,
        IntPtr* ppErrors)
    {
        ZeroAsyncOuts(pdwCancelId, ppErrors);
        if (!HasAsyncValueArgs(dwCount, phServer, pItemValues, pdwCancelId, ppErrors))
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
            int cancelId = ((IOPCAsyncIO2)group!).WriteAsync(handles, values, unchecked((int)dwTransactionId),
                out int[] errors, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pdwCancelId = unchecked((uint)cancelId);
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
    public static int Refresh2(IntPtr pThis, uint dwSource, uint dwTransactionId, uint* pdwCancelId)
    {
        if (pdwCancelId == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        *pdwCancelId = 0;
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            int cancelId = group!.Refresh2Async(unchecked((int)dwSource), unchecked((int)dwTransactionId), CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pdwCancelId = unchecked((uint)cancelId);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Cancel2(IntPtr pThis, uint dwCancelId)
    {
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            group!.Cancel2Async(unchecked((int)dwCancelId), CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetEnable(IntPtr pThis, int bEnable)
    {
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            group!.SetEnableAsync(bEnable != 0, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetEnable(IntPtr pThis, int* pbEnable)
    {
        if (pbEnable == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        *pbEnable = 0;
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            bool enabled = group!.GetEnableAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pbEnable = enabled ? 1 : 0;
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int GetConnectionPointContainer(IntPtr pThis, IntPtr* ppCPC)
    {
        if (ppCPC != null)
        {
            *ppCPC = IntPtr.Zero;
        }
        if (ppCPC == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        OpcDaGroupCcw.CcwSession? session = OpcDaGroupCcw.ResolveSession(pThis);
        return session is null
            ? OpcDaGroupCcw.E_FAIL
            : OpcDaGroupCcw.ReturnTearoff(session, session.ConnectionPointContainerTearoff, ppCPC);
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int ReadMaxAge(
        IntPtr pThis,
        uint dwCount,
        IntPtr phServer,
        IntPtr pdwMaxAge,
        uint dwTransactionId,
        uint* pdwCancelId,
        IntPtr* ppErrors)
    {
        ZeroAsyncOuts(pdwCancelId, ppErrors);
        if (!HasAsyncMaxAgeArgs(dwCount, phServer, pdwMaxAge, pdwCancelId, ppErrors))
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
            int cancelId = group!.ReadMaxAgeAsync(handles, maxAges, unchecked((int)dwTransactionId), out int[] errors,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pdwCancelId = unchecked((uint)cancelId);
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
    public static int WriteVqt(
        IntPtr pThis,
        uint dwCount,
        IntPtr phServer,
        IntPtr pItemVqt,
        uint dwTransactionId,
        uint* pdwCancelId,
        IntPtr* ppErrors)
    {
        ZeroAsyncOuts(pdwCancelId, ppErrors);
        if (!HasAsyncVqtArgs(dwCount, phServer, pItemVqt, pdwCancelId, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }

        OpcDaGroupCcw.CcwSession? session = OpcDaGroupCcw.ResolveSession(pThis);
        if (session?.GroupHandle.Target is not OpcDaGroup group)
        {
            return OpcDaGroupCcw.E_FAIL;
        }

        try
        {
            int count = checked((int)dwCount);
            int[] handles = ReadInt32Array(phServer, count);
            OpcItemVqt[] values = ReadOpcItemVqtArray(pItemVqt, count);
#pragma warning disable VSTHRD002
            int cancelId = group.WriteVqtAsync(handles, values, unchecked((int)dwTransactionId), out int[] errors,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pdwCancelId = unchecked((uint)cancelId);
            *ppErrors = AllocateInt32Array(errors);
            FireWriteComplete(session, group, unchecked((int)dwTransactionId), handles, errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int RefreshMaxAge(IntPtr pThis, uint dwMaxAge, uint dwTransactionId, uint* pdwCancelId)
    {
        if (pdwCancelId == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        *pdwCancelId = 0;
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            int cancelId = group!.RefreshMaxAgeAsync(unchecked((int)dwMaxAge), unchecked((int)dwTransactionId), CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pdwCancelId = unchecked((uint)cancelId);
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

    private static bool HasAsyncHandleArgs(uint count, IntPtr handles, uint* cancelId, IntPtr* errors) =>
        count <= int.MaxValue && cancelId != null && errors != null && (count == 0 || handles != IntPtr.Zero);

    private static bool HasAsyncMaxAgeArgs(uint count, IntPtr handles, IntPtr maxAges, uint* cancelId, IntPtr* errors) =>
        HasAsyncHandleArgs(count, handles, cancelId, errors) && (count == 0 || maxAges != IntPtr.Zero);

    private static bool HasAsyncValueArgs(uint count, IntPtr handles, IntPtr values, uint* cancelId, IntPtr* errors) =>
        HasAsyncHandleArgs(count, handles, cancelId, errors) && (count == 0 || values != IntPtr.Zero);

    private static bool HasAsyncVqtArgs(uint count, IntPtr handles, IntPtr values, uint* cancelId, IntPtr* errors) =>
        count > 0 && count <= int.MaxValue && handles != IntPtr.Zero && values != IntPtr.Zero &&
        cancelId != null && errors != null;

    private static int MapHResult(Exception ex) => ex switch
    {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentNullException => OpcDaGroupCcw.E_INVALIDARG,
        ArgumentException => OpcDaGroupCcw.E_INVALIDARG,
        _ => OpcDaGroupCcw.E_FAIL,
    };

    private static void ZeroAsyncOuts(uint* pdwCancelId, IntPtr* ppErrors)
    {
        if (pdwCancelId != null)
        {
            *pdwCancelId = 0;
        }
        if (ppErrors != null)
        {
            *ppErrors = IntPtr.Zero;
        }
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

    private static void FireWriteComplete(
        OpcDaGroupCcw.CcwSession session,
        OpcDaGroup group,
        int transactionId,
        int[] serverHandles,
        int[] errors)
    {
        if (session.ScmSinks.IsEmpty)
        {
            return;
        }

        int[] clientHandles = ResolveClientHandles(group, serverHandles);
        int masterError = GetMasterError(errors);
        foreach (OpcDataCallbackProxy sink in session.ScmSinks.Values)
        {
            sink.OnWriteComplete(transactionId, group.ClientHandle, masterError, clientHandles, errors);
        }
    }

    private static int[] ResolveClientHandles(OpcDaGroup group, int[] serverHandles)
    {
        var clientHandles = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            clientHandles[i] = group.GetItem(serverHandles[i])?.ClientHandle ?? 0;
        }
        return clientHandles;
    }

    private static int GetMasterError(int[] errors)
    {
        for (int i = 0; i < errors.Length; i++)
        {
            if (errors[i] < 0)
            {
                return S_FALSE;
            }
        }
        return OpcDaGroupCcw.S_OK;
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
}
