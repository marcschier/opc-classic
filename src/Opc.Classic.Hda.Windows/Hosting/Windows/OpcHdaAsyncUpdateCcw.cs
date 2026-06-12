//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>Raw-vtable method bodies for the <c>IOPCHDA_AsyncUpdate</c> tearoff.</summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaAsyncUpdateCcw
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QueryCapabilities(IntPtr pThis, IntPtr pCapabilities) =>
        OpcHdaCcwUpdateHelpers.QueryCapabilities(pThis, pCapabilities);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Insert(IntPtr pThis, uint dwTransactionID, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pDataValues, IntPtr pQualities, IntPtr pdwCancelID, IntPtr* ppErrors) =>
        BeginUpdate(pThis, dwTransactionID, dwNumItems, phServer, ftTimeStamps, pDataValues, pQualities, pdwCancelID, ppErrors, OpcHdaUpdateKind.Insert);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Replace(IntPtr pThis, uint dwTransactionID, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pDataValues, IntPtr pQualities, IntPtr pdwCancelID, IntPtr* ppErrors) =>
        BeginUpdate(pThis, dwTransactionID, dwNumItems, phServer, ftTimeStamps, pDataValues, pQualities, pdwCancelID, ppErrors, OpcHdaUpdateKind.Replace);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int InsertReplace(IntPtr pThis, uint dwTransactionID, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pDataValues, IntPtr pQualities, IntPtr pdwCancelID, IntPtr* ppErrors) =>
        BeginUpdate(pThis, dwTransactionID, dwNumItems, phServer, ftTimeStamps, pDataValues, pQualities, pdwCancelID, ppErrors, OpcHdaUpdateKind.InsertReplace);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int DeleteRaw(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        OpcHdaCcwUpdateHelpers.WriteUInt32(pdwCancelID, 0);
        OpcHdaCcwUpdateHelpers.ZeroOut(ppErrors);
        if (!OpcHdaCcwUpdateHelpers.HasAsyncDeleteRawArgs(htStartTime, htEndTime, dwNumItems, phServer, pdwCancelID, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher) ||
            !OpcHdaCcwUpdateHelpers.TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            int count = OpcHdaCcwUpdateHelpers.CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaAsyncUpdateResult result = dispatcher!.BeginAsyncDeleteRawAsync(unchecked((int)dwTransactionID), startTime, endTime, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return CompleteBegin(session!, pdwCancelID, ppErrors, unchecked((int)dwTransactionID), handles, result, count);
        }
        catch (Exception ex)
        {
            return OpcHdaCcwUpdateHelpers.MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int DeleteAtTime(IntPtr pThis, uint dwTransactionID, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        OpcHdaCcwUpdateHelpers.WriteUInt32(pdwCancelID, 0);
        OpcHdaCcwUpdateHelpers.ZeroOut(ppErrors);
        if (!OpcHdaCcwUpdateHelpers.HasAsyncDeleteAtTimeArgs(dwNumItems, phServer, ftTimeStamps, pdwCancelID, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher) ||
            !OpcHdaCcwUpdateHelpers.TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            int count = OpcHdaCcwUpdateHelpers.CountToInt(dwNumItems);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(ftTimeStamps, count);
#pragma warning disable VSTHRD002
            OpcHdaAsyncUpdateResult result = dispatcher!.BeginAsyncDeleteAtTimeAsync(unchecked((int)dwTransactionID), handles, timestamps, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return CompleteBegin(session!, pdwCancelID, ppErrors, unchecked((int)dwTransactionID), handles, result, count);
        }
        catch (Exception ex)
        {
            return OpcHdaCcwUpdateHelpers.MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int Cancel(IntPtr pThis, uint dwCancelID)
    {
        if (!OpcHdaCcwUpdateHelpers.TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        return OpcHdaCcwUpdateHelpers.CancelOperation(session!, unchecked((int)dwCancelID));
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int BeginUpdate(
        IntPtr pThis,
        uint transactionId,
        uint countValue,
        IntPtr handlesPtr,
        IntPtr timestampsPtr,
        IntPtr valuesPtr,
        IntPtr qualitiesPtr,
        IntPtr cancelIdPtr,
        IntPtr* errorsOut,
        OpcHdaUpdateKind kind)
    {
        OpcHdaCcwUpdateHelpers.WriteUInt32(cancelIdPtr, 0);
        OpcHdaCcwUpdateHelpers.ZeroOut(errorsOut);
        if (!OpcHdaCcwUpdateHelpers.HasAsyncWriteArgs(countValue, handlesPtr, timestampsPtr, valuesPtr, qualitiesPtr, cancelIdPtr, errorsOut))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher) ||
            !OpcHdaCcwUpdateHelpers.TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            int count = OpcHdaCcwUpdateHelpers.CountToInt(countValue);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(handlesPtr, count);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(timestampsPtr, count);
            OpcVariant[] values = OpcHdaCcwUpdateHelpers.ReadVariantArray(valuesPtr, count);
            int[] qualities = OpcHdaItemMarshaler.ReadInt32Array(qualitiesPtr, count);
#pragma warning disable VSTHRD002
            OpcHdaAsyncUpdateResult result = kind switch
            {
                OpcHdaUpdateKind.Insert => dispatcher!.BeginAsyncInsertAsync(unchecked((int)transactionId), handles, timestamps, values, qualities, CancellationToken.None).GetAwaiter().GetResult(),
                OpcHdaUpdateKind.Replace => dispatcher!.BeginAsyncReplaceAsync(unchecked((int)transactionId), handles, timestamps, values, qualities, CancellationToken.None).GetAwaiter().GetResult(),
                _ => dispatcher!.BeginAsyncInsertReplaceAsync(unchecked((int)transactionId), handles, timestamps, values, qualities, CancellationToken.None).GetAwaiter().GetResult(),
            };
#pragma warning restore VSTHRD002
            return CompleteBegin(session!, cancelIdPtr, errorsOut, unchecked((int)transactionId), handles, result, count);
        }
        catch (Exception ex)
        {
            return OpcHdaCcwUpdateHelpers.MapHResult(ex);
        }
    }

    private static int CompleteBegin(
        OpcHdaServerCcw.CcwSession session,
        IntPtr cancelIdPtr,
        IntPtr* errorsOut,
        int transactionId,
        int[] handles,
        OpcHdaAsyncUpdateResult result,
        int count)
    {
        int[] errors = OpcHdaCcwUpdateHelpers.NormalizeErrors(result.Errors, count, OpcResultId.InvalidHandle.Code);
        int[] clientHandles = OpcHdaCcwUpdateHelpers.NormalizeClientHandles(result.ClientHandles, handles, count);
        *errorsOut = OpcHdaItemMarshaler.AllocateInt32Array(errors);
        int cancelId = OpcHdaCcwUpdateHelpers.RegisterPendingOperation(session, cancelIdPtr, result.CancelId, out _);
        OpcHdaCcwUpdateHelpers.QueueUpdateComplete(session, cancelId, transactionId, clientHandles, errors);
        return OpcHdaCcwUpdateHelpers.GetMasterHResult(errors);
    }
}
