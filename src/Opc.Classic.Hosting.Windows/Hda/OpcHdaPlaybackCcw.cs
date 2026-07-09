// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Raw-vtable method bodies for the <c>IOPCHDA_Playback</c> tearoff.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaPlaybackCcw
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int ReadRawWithUpdate(
        IntPtr pThis,
        uint dwTransactionID,
        IntPtr htStartTime,
        IntPtr htEndTime,
        uint dwNumValues,
        long ftUpdateDuration,
        long ftUpdateInterval,
        uint dwNumItems,
        IntPtr phServer,
        IntPtr pdwCancelID,
        IntPtr* ppErrors)
    {
        OpcHdaCcwUpdateHelpers.WriteUInt32(pdwCancelID, 0);
        OpcHdaCcwUpdateHelpers.ZeroOut(ppErrors);
        if (!OpcHdaCcwUpdateHelpers.HasPlaybackRawArgs(htStartTime, htEndTime, dwNumItems, phServer, pdwCancelID, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.HasPlaybackSource(pThis))
        {
            return OpcHdaServerCcw.E_NOTIMPL;
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
            int[] errors = OpcHdaCcwUpdateHelpers.CreateSucceededErrors(count);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int cancelId = OpcHdaCcwUpdateHelpers.RegisterPendingOperation(session!, pdwCancelID, 0, out CancellationToken token);
            IAsyncEnumerable<OpcHdaPlaybackEvent> playbackEvents = dispatcher!.BeginPlaybackRawAsync(
                unchecked((int)dwTransactionID),
                startTime,
                endTime,
                OpcHdaCcwUpdateHelpers.CountToInt(dwNumValues),
                ftUpdateDuration,
                ftUpdateInterval,
                handles,
                token);
            OpcHdaCcwUpdateHelpers.QueuePlayback(session!, cancelId, unchecked((int)dwTransactionID), playbackEvents);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return OpcHdaCcwUpdateHelpers.MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int ReadProcessedWithUpdate(
        IntPtr pThis,
        uint dwTransactionID,
        IntPtr htStartTime,
        IntPtr htEndTime,
        long ftResampleInterval,
        uint dwNumIntervals,
        long ftUpdateInterval,
        uint dwNumItems,
        IntPtr phServer,
        IntPtr haAggregate,
        IntPtr pdwCancelID,
        IntPtr* ppErrors)
    {
        OpcHdaCcwUpdateHelpers.WriteUInt32(pdwCancelID, 0);
        OpcHdaCcwUpdateHelpers.ZeroOut(ppErrors);
        if (!OpcHdaCcwUpdateHelpers.HasPlaybackProcessedArgs(htStartTime, htEndTime, dwNumItems, phServer, haAggregate, pdwCancelID, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.HasPlaybackSource(pThis))
        {
            return OpcHdaServerCcw.E_NOTIMPL;
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
            int[] aggregateIds = OpcHdaItemMarshaler.ReadInt32Array(haAggregate, count);
            int[] errors = OpcHdaCcwUpdateHelpers.CreateSucceededErrors(count);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            int cancelId = OpcHdaCcwUpdateHelpers.RegisterPendingOperation(session!, pdwCancelID, 0, out CancellationToken token);
            IAsyncEnumerable<OpcHdaPlaybackEvent> playbackEvents = dispatcher!.BeginPlaybackProcessedAsync(
                unchecked((int)dwTransactionID),
                startTime,
                endTime,
                ftResampleInterval,
                OpcHdaCcwUpdateHelpers.CountToInt(dwNumIntervals),
                ftUpdateInterval,
                handles,
                aggregateIds,
                token);
            OpcHdaCcwUpdateHelpers.QueuePlayback(session!, cancelId, unchecked((int)dwTransactionID), playbackEvents);
            return OpcHdaServerCcw.S_OK;
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
}
