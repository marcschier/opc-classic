//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Update vtable entries and shared raw-marshaling helpers are tightly coupled.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>Raw-vtable method bodies for the <c>IOPCHDA_SyncUpdate</c> tearoff.</summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaSyncUpdateCcw
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QueryCapabilities(IntPtr pThis, IntPtr pCapabilities) =>
        OpcHdaCcwUpdateHelpers.QueryCapabilities(pThis, pCapabilities);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Insert(IntPtr pThis, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pDataValues, IntPtr pQualities, IntPtr* ppErrors) =>
        Update(pThis, dwNumItems, phServer, ftTimeStamps, pDataValues, pQualities, ppErrors, OpcHdaUpdateKind.Insert);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Replace(IntPtr pThis, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pDataValues, IntPtr pQualities, IntPtr* ppErrors) =>
        Update(pThis, dwNumItems, phServer, ftTimeStamps, pDataValues, pQualities, ppErrors, OpcHdaUpdateKind.Replace);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int InsertReplace(IntPtr pThis, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pDataValues, IntPtr pQualities, IntPtr* ppErrors) =>
        Update(pThis, dwNumItems, phServer, ftTimeStamps, pDataValues, pQualities, ppErrors, OpcHdaUpdateKind.InsertReplace);

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int DeleteRaw(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint dwNumItems, IntPtr phServer, IntPtr* ppErrors)
    {
        OpcHdaCcwUpdateHelpers.ZeroOut(ppErrors);
        if (!OpcHdaCcwUpdateHelpers.HasDeleteRawArgs(htStartTime, htEndTime, dwNumItems, phServer, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher))
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
            int[] returnedErrors = dispatcher!.DeleteRawAsync(startTime, endTime, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = OpcHdaCcwUpdateHelpers.NormalizeErrors(returnedErrors, count, OpcResultId.InvalidHandle.Code);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            return OpcHdaCcwUpdateHelpers.GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return OpcHdaCcwUpdateHelpers.MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int DeleteAtTime(IntPtr pThis, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr* ppErrors)
    {
        OpcHdaCcwUpdateHelpers.ZeroOut(ppErrors);
        if (!OpcHdaCcwUpdateHelpers.HasDeleteAtTimeArgs(dwNumItems, phServer, ftTimeStamps, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            int count = OpcHdaCcwUpdateHelpers.CountToInt(dwNumItems);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(ftTimeStamps, count);
#pragma warning disable VSTHRD002
            int[] returnedErrors = dispatcher!.DeleteAtTimeAsync(handles, timestamps, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = OpcHdaCcwUpdateHelpers.NormalizeErrors(returnedErrors, count, OpcResultId.InvalidHandle.Code);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            return OpcHdaCcwUpdateHelpers.GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return OpcHdaCcwUpdateHelpers.MapHResult(ex);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Update(IntPtr pThis, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pDataValues, IntPtr pQualities, IntPtr* ppErrors, OpcHdaUpdateKind kind)
    {
        OpcHdaCcwUpdateHelpers.ZeroOut(ppErrors);
        if (!OpcHdaCcwUpdateHelpers.HasWriteArgs(dwNumItems, phServer, ftTimeStamps, pDataValues, pQualities, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!OpcHdaCcwUpdateHelpers.TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            int count = OpcHdaCcwUpdateHelpers.CountToInt(dwNumItems);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(ftTimeStamps, count);
            OpcVariant[] values = OpcHdaCcwUpdateHelpers.ReadVariantArray(pDataValues, count);
            int[] qualities = OpcHdaItemMarshaler.ReadInt32Array(pQualities, count);
#pragma warning disable VSTHRD002
            int[] returnedErrors = kind switch
            {
                OpcHdaUpdateKind.Insert => dispatcher!.InsertAsync(handles, timestamps, values, qualities, CancellationToken.None).GetAwaiter().GetResult(),
                OpcHdaUpdateKind.Replace => dispatcher!.ReplaceAsync(handles, timestamps, values, qualities, CancellationToken.None).GetAwaiter().GetResult(),
                _ => dispatcher!.InsertReplaceAsync(handles, timestamps, values, qualities, CancellationToken.None).GetAwaiter().GetResult(),
            };
#pragma warning restore VSTHRD002
            int[] errors = OpcHdaCcwUpdateHelpers.NormalizeErrors(returnedErrors, count, OpcResultId.InvalidHandle.Code);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            return OpcHdaCcwUpdateHelpers.GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return OpcHdaCcwUpdateHelpers.MapHResult(ex);
        }
    }
}

internal enum OpcHdaUpdateKind
{
    Insert,
    Replace,
    InsertReplace,
}

/// <summary>Shared raw-vtable helpers for HDA update and playback tearoffs.</summary>
[SupportedOSPlatform("windows")]
internal static class OpcHdaCcwUpdateHelpers
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QueryCapabilities(IntPtr pThis, IntPtr pCapabilities)
    {
        if (pCapabilities == IntPtr.Zero)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
#pragma warning disable VSTHRD002
            int capabilities = dispatcher!.UpdateCapabilitiesAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pCapabilities, capabilities);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    public static bool TryResolveDispatcher(IntPtr pThis, out IOpcHdaServerDispatcher? dispatcher)
    {
        dispatcher = OpcHdaServerCcw.ResolveDispatcher(pThis);
        return dispatcher is not null;
    }

    public static bool TryResolveSession(IntPtr pThis, out OpcHdaServerCcw.CcwSession? session)
    {
        session = OpcHdaServerCcw.ResolveSession(pThis);
        return session is not null;
    }

    public static bool HasPlaybackSource(IntPtr pThis)
    {
        IOpcHdaServer? server = OpcHdaServerCcw.ResolveServer(pThis);
        return server is IOPCHDA_Playback or IOPCHDA_SyncRead;
    }

    public static unsafe bool HasWriteArgs(uint count, IntPtr handles, IntPtr timestamps, IntPtr values, IntPtr qualities, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && handles != IntPtr.Zero && timestamps != IntPtr.Zero && values != IntPtr.Zero &&
        qualities != IntPtr.Zero && errors != null;

    public static unsafe bool HasDeleteRawArgs(IntPtr startTime, IntPtr endTime, uint count, IntPtr handles, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && startTime != IntPtr.Zero && endTime != IntPtr.Zero && handles != IntPtr.Zero && errors != null;

    public static unsafe bool HasDeleteAtTimeArgs(uint count, IntPtr handles, IntPtr timestamps, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && handles != IntPtr.Zero && timestamps != IntPtr.Zero && errors != null;

    public static unsafe bool HasAsyncWriteArgs(uint count, IntPtr handles, IntPtr timestamps, IntPtr values, IntPtr qualities, IntPtr cancelId, IntPtr* errors) =>
        HasWriteArgs(count, handles, timestamps, values, qualities, errors) && cancelId != IntPtr.Zero;

    public static unsafe bool HasAsyncDeleteRawArgs(IntPtr startTime, IntPtr endTime, uint count, IntPtr handles, IntPtr cancelId, IntPtr* errors) =>
        HasDeleteRawArgs(startTime, endTime, count, handles, errors) && cancelId != IntPtr.Zero;

    public static unsafe bool HasAsyncDeleteAtTimeArgs(uint count, IntPtr handles, IntPtr timestamps, IntPtr cancelId, IntPtr* errors) =>
        HasDeleteAtTimeArgs(count, handles, timestamps, errors) && cancelId != IntPtr.Zero;

    public static unsafe bool HasPlaybackRawArgs(IntPtr startTime, IntPtr endTime, uint count, IntPtr handles, IntPtr cancelId, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && startTime != IntPtr.Zero && endTime != IntPtr.Zero && handles != IntPtr.Zero &&
        cancelId != IntPtr.Zero && errors != null;

    public static unsafe bool HasPlaybackProcessedArgs(IntPtr startTime, IntPtr endTime, uint count, IntPtr handles, IntPtr aggregates, IntPtr cancelId, IntPtr* errors) =>
        HasPlaybackRawArgs(startTime, endTime, count, handles, cancelId, errors) && aggregates != IntPtr.Zero;

    public static int CountToInt(uint count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, (uint)int.MaxValue);
        return (int)count;
    }

    public static OpcVariant[] ReadVariantArray(IntPtr ptr, int count)
    {
        var values = new OpcVariant[count];
        for (int i = 0; i < count; i++)
        {
            IntPtr slot = IntPtr.Add(ptr, checked(i * NativeHdaVariantReader.VariantSize));
            if (!NativeHdaVariantReader.TryRead(slot, out values[i]))
            {
                throw new ArgumentException("A VARIANT update value could not be marshaled.", nameof(ptr));
            }
        }

        return values;
    }

    public static int[] NormalizeErrors(int[]? errors, int count, int missingError)
    {
        var normalized = new int[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = errors is not null && i < errors.Length ? errors[i] : missingError;
        }

        return normalized;
    }

    public static int[] NormalizeClientHandles(int[]? clientHandles, int[] serverHandles, int count)
    {
        var normalized = new int[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = clientHandles is not null && i < clientHandles.Length ? clientHandles[i] : serverHandles[i];
        }

        return normalized;
    }

    public static int[] CreateSucceededErrors(int count) => new int[count];

    public static int GetMasterHResult(int[] errors) => HasAnyFailure(errors) ? OpcHdaServerCcw.S_FALSE : OpcHdaServerCcw.S_OK;

    public static int MapHResult(Exception ex) => ex switch
    {
        OpcException opcEx => opcEx.ResultId.Code,
        COMException comEx => comEx.ErrorCode,
        ArgumentException => OpcHdaServerCcw.E_INVALIDARG,
        ArithmeticException => OpcHdaServerCcw.E_INVALIDARG,
        OperationCanceledException => OpcHdaServerCcw.E_FAIL,
        _ => OpcHdaServerCcw.E_FAIL,
    };

    public static int RegisterPendingOperation(OpcHdaServerCcw.CcwSession session, IntPtr pdwCancelID, int preferredCancelId, out CancellationToken cancellationToken)
    {
        int cancelId = preferredCancelId != 0 ? preferredCancelId : NextCancelId(session);
        var cts = new CancellationTokenSource();
        while (!session.PendingOperations.TryAdd(cancelId, cts))
        {
            cancelId = NextCancelId(session);
        }

        cancellationToken = cts.Token;
        WriteUInt32(pdwCancelID, unchecked((uint)cancelId));
        return cancelId;
    }

    public static void QueueUpdateComplete(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, int[] clientHandles, int[] errors) =>
        _ = Task.Run(() => CompleteUpdateCallback(session, cancelId, transactionId, clientHandles, errors));

    public static void QueuePlayback(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, IAsyncEnumerable<OpcHdaPlaybackEvent> playbackEvents) =>
        _ = Task.Run(() => CompletePlaybackAsync(session, cancelId, transactionId, playbackEvents));

    public static int CancelOperation(OpcHdaServerCcw.CcwSession session, int cancelId)
    {
        if (!session.PendingOperations.TryRemove(cancelId, out CancellationTokenSource? cts))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            cts.Cancel();
#pragma warning disable VSTHRD002
            session.Dispatcher.CancelAsync(cancelId, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
        }
        catch (NotSupportedException)
        {
        }
        finally
        {
            cts.Dispose();
        }

        FireCancelComplete(session, cancelId);
        return OpcHdaServerCcw.S_OK;
    }

    public static unsafe void ZeroOut(IntPtr* pp)
    {
        if (pp != null)
        {
            *pp = IntPtr.Zero;
        }
    }

    public static void WriteUInt32(IntPtr p, uint value)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt32(p, unchecked((int)value));
        }
    }

    private static int NextCancelId(OpcHdaServerCcw.CcwSession session)
    {
        int cancelId = Interlocked.Increment(ref session.NextCancelId);
        return cancelId == 0 ? Interlocked.Increment(ref session.NextCancelId) : cancelId;
    }

    private static async Task CompletePlaybackAsync(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, IAsyncEnumerable<OpcHdaPlaybackEvent> playbackEvents)
    {
        if (!session.PendingOperations.TryGetValue(cancelId, out CancellationTokenSource? cts))
        {
            return;
        }

        try
        {
            await foreach (OpcHdaPlaybackEvent playbackEvent in playbackEvents.WithCancellation(cts.Token).ConfigureAwait(false))
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                FirePlayback(session, transactionId, playbackEvent);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (session.PendingOperations.TryRemove(cancelId, out CancellationTokenSource? removed))
            {
                removed.Dispose();
            }
        }
    }

    private static void CompleteUpdateCallback(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, int[] clientHandles, int[] errors)
    {
        if (!session.PendingOperations.TryRemove(cancelId, out CancellationTokenSource? cts))
        {
            return;
        }

        try
        {
            if (!cts.IsCancellationRequested)
            {
                FireUpdateComplete(session, transactionId, clientHandles, errors);
            }
        }
        finally
        {
            cts.Dispose();
        }
    }

    private static void FireUpdateComplete(OpcHdaServerCcw.CcwSession session, int transactionId, int[] clientHandles, int[] errors)
    {
        int status = GetMasterHResult(errors);
        foreach (OpcHdaCallbackProxy sink in session.ScmSinks.Values)
        {
            try
            {
                sink.OnUpdateComplete(transactionId, status, clientHandles, errors);
            }
            catch (COMException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private static void FirePlayback(OpcHdaServerCcw.CcwSession session, int transactionId, OpcHdaPlaybackEvent playbackEvent)
    {
        foreach (OpcHdaCallbackProxy sink in session.ScmSinks.Values)
        {
            try
            {
                sink.OnPlayback(transactionId, playbackEvent.Status, playbackEvent.Items, playbackEvent.Errors);
            }
            catch (COMException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private static void FireCancelComplete(OpcHdaServerCcw.CcwSession session, int cancelId)
    {
        foreach (OpcHdaCallbackProxy sink in session.ScmSinks.Values)
        {
            try
            {
                sink.OnCancelComplete(cancelId);
            }
            catch (COMException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    private static bool HasAnyFailure(int[] errors)
    {
        for (int i = 0; i < errors.Length; i++)
        {
            if (errors[i] < 0)
            {
                return true;
            }
        }

        return false;
    }

    private static void WriteInt32(IntPtr p, int value)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt32(p, value);
        }
    }
}
