//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Server CCW method bodies and async callback pump share session internals.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>Method bodies bound into the <see cref="OpcHdaServerCcw" /> HDA vtables.</summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaServerCcwMethods
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetItemAttributes(
        IntPtr pThis,
        IntPtr pdwCount,
        IntPtr* ppdwAttrID,
        IntPtr* ppszAttrName,
        IntPtr* ppszAttrDesc,
        IntPtr* ppvtAttrDataType)
    {
        if (!TryResolveServer(pThis, out IOpcHdaServer? server))
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (pdwCount == IntPtr.Zero || ppdwAttrID == null || ppszAttrName == null ||
            ppszAttrDesc == null || ppvtAttrDataType == null)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
#pragma warning disable VSTHRD002
            ((IOPCHDA_Server)server!).GetItemAttributesAsync(out int[] ids, out string[] names,
                out string[] descriptions, out int[] dataTypes, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateAttributeLengths(ids, names, descriptions, dataTypes);
            WriteUInt32(pdwCount, checked((uint)ids.Length));
            *ppdwAttrID = AllocateInt32Array(ids);
            *ppszAttrName = AllocateLpwStrArray(names);
            *ppszAttrDesc = AllocateLpwStrArray(descriptions);
            *ppvtAttrDataType = AllocateUInt16Array(dataTypes);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetAggregates(
        IntPtr pThis,
        IntPtr pdwCount,
        IntPtr* ppdwAggrID,
        IntPtr* ppszAggrName,
        IntPtr* ppszAggrDesc)
    {
        if (!TryResolveServer(pThis, out IOpcHdaServer? server))
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (pdwCount == IntPtr.Zero || ppdwAggrID == null || ppszAggrName == null || ppszAggrDesc == null)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
#pragma warning disable VSTHRD002
            ((IOPCHDA_Server)server!).GetAggregatesAsync(out int[] ids, out string[] names,
                out string[] descriptions, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateAggregateLengths(ids, names, descriptions);
            WriteUInt32(pdwCount, checked((uint)ids.Length));
            *ppdwAggrID = AllocateInt32Array(ids);
            *ppszAggrName = AllocateLpwStrArray(names);
            *ppszAggrDesc = AllocateLpwStrArray(descriptions);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetHistorianStatus(
        IntPtr pThis,
        IntPtr pwStatus,
        IntPtr* pftCurrentTime,
        IntPtr* pftStartTime,
        IntPtr pwMajorVersion,
        IntPtr pwMinorVersion,
        IntPtr pwBuildNumber,
        IntPtr pdwMaxReturnValues,
        IntPtr ppszStatusString,
        IntPtr ppszVendorInfo)
    {
        if (!TryResolveServer(pThis, out IOpcHdaServer? server))
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (!HasHistorianStatusOutParams(pwStatus, pftCurrentTime, pftStartTime, pdwMaxReturnValues))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
#pragma warning disable VSTHRD002
            OpcServerStatus status = server!.GetStatusAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pwStatus, ToHistorianStatus(status.State));
            *pftCurrentTime = AllocateFileTime(status.CurrentTime);
            *pftStartTime = AllocateFileTime(status.StartTime);
            WriteVersion(status.ServerVersion, pwMajorVersion, pwMinorVersion, pwBuildNumber);
            WriteUInt32(pdwMaxReturnValues, checked((uint)Math.Max(0, status.MaxReturnValues)));
            WriteLpwStrPtr(ppszStatusString, status.State.ToString());
            WriteLpwStrPtr(ppszVendorInfo, status.VendorInfo);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetItemHandles(
        IntPtr pThis,
        uint dwCount,
        IntPtr pszItemID,
        IntPtr phClient,
        IntPtr* pphServer,
        IntPtr* ppErrors)
    {
        if (!TryResolveServer(pThis, out IOpcHdaServer? server))
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (pphServer == null || ppErrors == null || (dwCount > 0 && (pszItemID == IntPtr.Zero || phClient == IntPtr.Zero)))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwCount);
            string[] itemIds = ReadLpwStrArray(pszItemID, count);
            int[] clientHandles = ReadInt32Array(phClient, count);
#pragma warning disable VSTHRD002
            int[] serverHandles = ((IOPCHDA_Server)server!).GetItemHandlesAsync(itemIds, clientHandles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, serverHandles.Length, nameof(serverHandles));
            *pphServer = AllocateInt32Array(serverHandles);
            *ppErrors = AllocateSucceededErrors(count);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int ReleaseItemHandles(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr* ppErrors)
    {
        if (!TryResolveServer(pThis, out IOpcHdaServer? server))
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (ppErrors == null || (dwCount > 0 && phServer == IntPtr.Zero))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwCount);
            int[] handles = ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            int[] errors = ((IOPCHDA_Server)server!).ReleaseItemHandlesAsync(handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, errors.Length, nameof(errors));
            *ppErrors = AllocateInt32Array(errors);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int ValidateItemIDs(IntPtr pThis, uint dwCount, IntPtr pszItemID, IntPtr* ppErrors)
    {
        if (!TryResolveServer(pThis, out IOpcHdaServer? server))
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (ppErrors == null || (dwCount > 0 && pszItemID == IntPtr.Zero))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwCount);
            string[] itemIds = ReadLpwStrArray(pszItemID, count);
#pragma warning disable VSTHRD002
            int[] errors = server!.ValidateItemIdsAsync(itemIds, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, errors.Length, nameof(errors));
            *ppErrors = AllocateInt32Array(errors);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int CreateBrowse(
        IntPtr pThis,
        uint dwCount,
        IntPtr pdwAttrID,
        IntPtr pOperator,
        IntPtr vFilter,
        IntPtr* pphBrowser,
        IntPtr* ppErrors)
    {
        ZeroOut(pphBrowser);
        ZeroOut(ppErrors);
        if (!TryResolveServer(pThis, out IOpcHdaServer? server))
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (pphBrowser == null || ppErrors == null || !HasBrowseFilterPointers(dwCount, pdwAttrID, pOperator, vFilter))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwCount);
            OpcHdaBrowseFilter[] filters = ReadBrowseFilters(count, pdwAttrID, pOperator, vFilter, out int[] localErrors);
            var dispatcher = new OpcHdaServerDispatcher(server!);
#pragma warning disable VSTHRD002
            int[] dispatcherErrors = dispatcher.ValidateBrowseFiltersAsync(filters, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, dispatcherErrors.Length, nameof(dispatcherErrors));

            int[] errors = MergeBrowseFilterErrors(localErrors, dispatcherErrors);
            *ppErrors = AllocateInt32Array(errors);
            *pphBrowser = OpcHdaBrowserCcw.Create(dispatcher, FilterSuccessfulBrowseFilters(filters, errors));
            return HasAnyFailure(errors) ? OpcHdaServerCcw.S_FALSE : OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncReadRaw(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, int bBounds, uint dwNumItems, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        ZeroReadOuts(ppItemValues, ppErrors);
        if (!HasItemReadArgs(dwNumItems, phServer, ppItemValues, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead))
        {
            return HasItemReadArgs(dwNumItems, phServer, ppItemValues, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaItem[] items = syncRead!.ReadRawAsync(startTime, endTime, CountToInt(dwNumValues), bBounds != 0, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            AssignItemOuts(ppItemValues, ppErrors, NormalizeItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncReadProcessed(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, long ftResampleInterval, uint dwNumItems, IntPtr phServer, IntPtr haAggregate, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        ZeroReadOuts(ppItemValues, ppErrors);
        if (!HasProcessedReadArgs(dwNumItems, phServer, haAggregate, ppItemValues, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead))
        {
            return HasProcessedReadArgs(dwNumItems, phServer, haAggregate, ppItemValues, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            int[] aggregates = OpcHdaItemMarshaler.ReadInt32Array(haAggregate, count);
#pragma warning disable VSTHRD002
            OpcHdaItem[] items = syncRead!.ReadProcessedAsync(startTime, endTime, ftResampleInterval, handles, aggregates, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            AssignItemOuts(ppItemValues, ppErrors, NormalizeItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncReadAtTime(IntPtr pThis, uint dwNumTimeStamps, IntPtr ftTimeStamps, uint dwNumItems, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        ZeroReadOuts(ppItemValues, ppErrors);
        if (!HasAtTimeReadArgs(dwNumTimeStamps, ftTimeStamps, dwNumItems, phServer, ppItemValues, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead))
        {
            return HasAtTimeReadArgs(dwNumTimeStamps, ftTimeStamps, dwNumItems, phServer, ppItemValues, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            int timestampCount = CountToInt(dwNumTimeStamps);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(ftTimeStamps, timestampCount);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaItem[] items = syncRead!.ReadAtTimeAsync(timestamps, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            AssignItemOuts(ppItemValues, ppErrors, NormalizeItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncReadModified(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, uint dwNumItems, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        ZeroReadOuts(ppItemValues, ppErrors);
        if (!HasItemReadArgs(dwNumItems, phServer, ppItemValues, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead))
        {
            return HasItemReadArgs(dwNumItems, phServer, ppItemValues, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaModifiedItem[] items = syncRead!.ReadModifiedAsync(startTime, endTime, CountToInt(dwNumValues), handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            AssignModifiedOuts(ppItemValues, ppErrors, NormalizeModifiedItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncReadAttribute(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint hServer, uint dwNumAttributes, IntPtr pdwAttributeIDs, IntPtr* ppAttributeValues, IntPtr* ppErrors)
    {
        ZeroReadOuts(ppAttributeValues, ppErrors);
        if (!HasAttributeReadArgs(dwNumAttributes, pdwAttributeIDs, ppAttributeValues, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead))
        {
            return HasAttributeReadArgs(dwNumAttributes, pdwAttributeIDs, ppAttributeValues, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumAttributes);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] attributeIds = OpcHdaItemMarshaler.ReadInt32Array(pdwAttributeIDs, count);
#pragma warning disable VSTHRD002
            OpcHdaAttribute[] attributes = syncRead!.ReadAttributeAsync(startTime, endTime, unchecked((int)hServer), attributeIds, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildAttributeErrors(attributeIds, attributes.Length, count);
            AssignAttributeOuts(ppAttributeValues, ppErrors, NormalizeAttributes(attributes, attributeIds, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncAnnotationsQueryCapabilities(IntPtr pThis, IntPtr pCapabilities)
    {
        if (pCapabilities == IntPtr.Zero)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!TryResolveSyncAnnotations(pThis, out IOPCHDA_SyncAnnotations? annotations))
        {
            return OpcHdaServerCcw.E_NOTIMPL;
        }

        try
        {
#pragma warning disable VSTHRD002
            int capabilities = annotations!.QueryCapabilitiesAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pCapabilities, capabilities);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncReadAnnotations(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint dwNumItems, IntPtr phServer, IntPtr* ppAnnotationValues, IntPtr* ppErrors)
    {
        ZeroReadOuts(ppAnnotationValues, ppErrors);
        if (!HasItemReadArgs(dwNumItems, phServer, ppAnnotationValues, ppErrors) || !TryResolveSyncAnnotations(pThis, out IOPCHDA_SyncAnnotations? annotations))
        {
            return HasItemReadArgs(dwNumItems, phServer, ppAnnotationValues, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaAnnotation[] values = annotations!.ReadAsync(startTime, endTime, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(values.Length, count, OpcResultId.InvalidHandle.Code);
            AssignAnnotationOuts(ppAnnotationValues, ppErrors, NormalizeAnnotations(values, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SyncAnnotationsInsert(IntPtr pThis, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pAnnotationValues, IntPtr* ppErrors)
    {
        ZeroOut(ppErrors);
        if (!HasAnnotationInsertArgs(dwNumItems, phServer, ftTimeStamps, pAnnotationValues, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(ftTimeStamps, count);
            OpcHdaAnnotation[] annotations = ReadAnnotationInputArray(pAnnotationValues, count);
#pragma warning disable VSTHRD002
            int[] errors = dispatcher!.InsertAnnotationsAsync(handles, timestamps, annotations, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, errors.Length, nameof(errors));
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncReadRaw(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, int bBounds, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncItemReadArgs(dwNumItems, phServer, pdwCancelID, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return HasAsyncItemReadArgs(dwNumItems, phServer, pdwCancelID, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaItem[] items = syncRead!.ReadRawAsync(startTime, endTime, CountToInt(dwNumValues), bBounds != 0, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            int cancelId = RegisterPendingOperation(session!, pdwCancelID);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            QueueReadComplete(session!, cancelId, unchecked((int)dwTransactionID), NormalizeItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncAdviseRaw(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, long ftUpdateInterval, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncAdviseRawArgs(dwNumItems, htStartTime, ftUpdateInterval, phServer, pdwCancelID, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        int cancelId = 0;
        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            cancelId = RegisterPendingOperation(session!, pdwCancelID);
            CancellationToken cancellationToken = GetPendingCancellationToken(session!, cancelId);
#pragma warning disable VSTHRD002
            OpcHdaAdviseSubscription subscription = dispatcher!.AdviseRawAsync(handles, startTime, ftUpdateInterval, cancellationToken).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, subscription.Errors.Length, nameof(subscription.Errors));
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(subscription.Errors);
            QueueDataChangeStream(session!, cancelId, unchecked((int)dwTransactionID), subscription.Updates);
            return GetMasterHResult(subscription.Errors);
        }
        catch (Exception ex)
        {
            RemovePendingOperation(session!, cancelId);
            WriteUInt32(pdwCancelID, 0);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncReadProcessed(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, long ftResampleInterval, uint dwNumItems, IntPtr phServer, IntPtr haAggregate, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncProcessedReadArgs(dwNumItems, phServer, haAggregate, pdwCancelID, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return HasAsyncProcessedReadArgs(dwNumItems, phServer, haAggregate, pdwCancelID, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            int[] aggregates = OpcHdaItemMarshaler.ReadInt32Array(haAggregate, count);
#pragma warning disable VSTHRD002
            OpcHdaItem[] items = syncRead!.ReadProcessedAsync(startTime, endTime, ftResampleInterval, handles, aggregates, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            int cancelId = RegisterPendingOperation(session!, pdwCancelID);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            QueueReadComplete(session!, cancelId, unchecked((int)dwTransactionID), NormalizeItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncAdviseProcessed(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, long ftResampleInterval, uint dwNumItems, IntPtr phServer, IntPtr haAggregate, uint dwNumIntervals, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncAdviseProcessedArgs(dwNumItems, htStartTime, ftResampleInterval, phServer, haAggregate, dwNumIntervals, pdwCancelID, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        int cancelId = 0;
        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            int[] aggregates = OpcHdaItemMarshaler.ReadInt32Array(haAggregate, count);
            cancelId = RegisterPendingOperation(session!, pdwCancelID);
            CancellationToken cancellationToken = GetPendingCancellationToken(session!, cancelId);
#pragma warning disable VSTHRD002
            OpcHdaAdviseSubscription subscription = dispatcher!.AdviseProcessedAsync(handles, startTime, ftResampleInterval, aggregates, CountToInt(dwNumIntervals), cancellationToken).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, subscription.Errors.Length, nameof(subscription.Errors));
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(subscription.Errors);
            QueueDataChangeStream(session!, cancelId, unchecked((int)dwTransactionID), subscription.Updates);
            return GetMasterHResult(subscription.Errors);
        }
        catch (Exception ex)
        {
            RemovePendingOperation(session!, cancelId);
            WriteUInt32(pdwCancelID, 0);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncReadAtTime(IntPtr pThis, uint dwTransactionID, uint dwNumTimeStamps, IntPtr ftTimeStamps, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncAtTimeReadArgs(dwNumTimeStamps, ftTimeStamps, dwNumItems, phServer, pdwCancelID, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return HasAsyncAtTimeReadArgs(dwNumTimeStamps, ftTimeStamps, dwNumItems, phServer, pdwCancelID, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(ftTimeStamps, CountToInt(dwNumTimeStamps));
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaItem[] items = syncRead!.ReadAtTimeAsync(timestamps, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            int cancelId = RegisterPendingOperation(session!, pdwCancelID);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            QueueReadComplete(session!, cancelId, unchecked((int)dwTransactionID), NormalizeItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncReadModified(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncItemReadArgs(dwNumItems, phServer, pdwCancelID, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return HasAsyncItemReadArgs(dwNumItems, phServer, pdwCancelID, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaModifiedItem[] items = syncRead!.ReadModifiedAsync(startTime, endTime, CountToInt(dwNumValues), handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(items.Length, count, OpcResultId.InvalidHandle.Code);
            int cancelId = RegisterPendingOperation(session!, pdwCancelID);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            QueueReadModifiedComplete(session!, cancelId, unchecked((int)dwTransactionID), NormalizeModifiedItems(items, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncReadAttribute(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint hServer, uint dwNumAttributes, IntPtr pdwAttributeIDs, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncAttributeReadArgs(dwNumAttributes, pdwAttributeIDs, pdwCancelID, ppErrors) || !TryResolveSyncRead(pThis, out IOPCHDA_SyncRead? syncRead) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return HasAsyncAttributeReadArgs(dwNumAttributes, pdwAttributeIDs, pdwCancelID, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumAttributes);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] attributeIds = OpcHdaItemMarshaler.ReadInt32Array(pdwAttributeIDs, count);
#pragma warning disable VSTHRD002
            OpcHdaAttribute[] attributes = syncRead!.ReadAttributeAsync(startTime, endTime, unchecked((int)hServer), attributeIds, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildAttributeErrors(attributeIds, attributes.Length, count);
            OpcHdaAttribute[] normalized = NormalizeAttributes(attributes, attributeIds, count);
            int cancelId = RegisterPendingOperation(session!, pdwCancelID);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            QueueReadAttributeComplete(session!, cancelId, unchecked((int)dwTransactionID), GetAttributeClientHandle(normalized), normalized, errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncAnnotationsQueryCapabilities(IntPtr pThis, IntPtr pCapabilities)
    {
        if (pCapabilities == IntPtr.Zero)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        try
        {
            int capabilities;
            if (TryResolveAsyncAnnotations(pThis, out IOPCHDA_AsyncAnnotations? annotations))
            {
#pragma warning disable VSTHRD002
                capabilities = annotations!.QueryCapabilitiesAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            }
            else if (TryResolveSyncAnnotations(pThis, out IOPCHDA_SyncAnnotations? syncAnnotations))
            {
#pragma warning disable VSTHRD002
                capabilities = syncAnnotations!.QueryCapabilitiesAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            }
            else
            {
                return OpcHdaServerCcw.E_NOTIMPL;
            }

            WriteInt32(pCapabilities, capabilities);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncReadAnnotations(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncItemReadArgs(dwNumItems, phServer, pdwCancelID, ppErrors) || !TryResolveSyncAnnotations(pThis, out IOPCHDA_SyncAnnotations? annotations) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return HasAsyncItemReadArgs(dwNumItems, phServer, pdwCancelID, ppErrors) ? OpcHdaServerCcw.E_NOTIMPL : OpcHdaServerCcw.E_INVALIDARG;
        }

        try
        {
            int count = CountToInt(dwNumItems);
            OpcHdaTime startTime = OpcHdaItemMarshaler.ReadHdaTime(htStartTime);
            OpcHdaTime endTime = OpcHdaItemMarshaler.ReadHdaTime(htEndTime);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
#pragma warning disable VSTHRD002
            OpcHdaAnnotation[] values = annotations!.ReadAsync(startTime, endTime, handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            int[] errors = BuildResultErrors(values.Length, count, OpcResultId.InvalidHandle.Code);
            int cancelId = RegisterPendingOperation(session!, pdwCancelID);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            QueueReadAnnotationsComplete(session!, cancelId, unchecked((int)dwTransactionID), NormalizeAnnotations(values, count), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AsyncAnnotationsInsert(IntPtr pThis, uint dwTransactionID, uint dwNumItems, IntPtr phServer, IntPtr ftTimeStamps, IntPtr pAnnotationValues, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        if (!HasAsyncAnnotationInsertArgs(dwNumItems, phServer, ftTimeStamps, pAnnotationValues, pdwCancelID, ppErrors))
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcHdaServerDispatcher? dispatcher) || !TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        int cancelId = 0;
        try
        {
            int count = CountToInt(dwNumItems);
            int[] handles = OpcHdaItemMarshaler.ReadInt32Array(phServer, count);
            long[] timestamps = OpcHdaItemMarshaler.ReadFileTimeArray(ftTimeStamps, count);
            OpcHdaAnnotation[] annotations = ReadAnnotationInputArray(pAnnotationValues, count);
#pragma warning disable VSTHRD002
            int[] errors = dispatcher!.InsertAnnotationsAsync(handles, timestamps, annotations, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ValidateLength(count, errors.Length, nameof(errors));
            cancelId = RegisterPendingOperation(session!, pdwCancelID);
            *ppErrors = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            QueueInsertAnnotationsComplete(session!, cancelId, unchecked((int)dwTransactionID), GetAnnotationClientHandles(annotations), errors);
            return GetMasterHResult(errors);
        }
        catch (Exception ex)
        {
            RemovePendingOperation(session!, cancelId);
            WriteUInt32(pdwCancelID, 0);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int AsyncCancel(IntPtr pThis, uint dwCancelID)
    {
        if (!TryResolveSession(pThis, out OpcHdaServerCcw.CcwSession? session))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        int cancelId = unchecked((int)dwCancelID);
        if (!session!.PendingOperations.TryRemove(cancelId, out CancellationTokenSource? cts))
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        cts.Cancel();
        cts.Dispose();
        FireCancelComplete(session, cancelId);
        return OpcHdaServerCcw.S_OK;
    }

    private static bool TryResolveSession(IntPtr pThis, out OpcHdaServerCcw.CcwSession? session)
    {
        session = OpcHdaServerCcw.ResolveSession(pThis);
        return session is not null;
    }

    private static bool TryResolveSyncRead(IntPtr pThis, out IOPCHDA_SyncRead? syncRead)
    {
        syncRead = OpcHdaServerCcw.ResolveServer(pThis) as IOPCHDA_SyncRead;
        return syncRead is not null;
    }

    private static bool TryResolveSyncAnnotations(IntPtr pThis, out IOPCHDA_SyncAnnotations? annotations)
    {
        annotations = OpcHdaServerCcw.ResolveServer(pThis) as IOPCHDA_SyncAnnotations;
        return annotations is not null;
    }

    private static bool TryResolveAsyncAnnotations(IntPtr pThis, out IOPCHDA_AsyncAnnotations? annotations)
    {
        annotations = OpcHdaServerCcw.ResolveServer(pThis) as IOPCHDA_AsyncAnnotations;
        return annotations is not null;
    }

    private static bool TryResolveDispatcher(IntPtr pThis, out IOpcHdaServerDispatcher? dispatcher)
    {
        IOpcHdaServer? server = OpcHdaServerCcw.ResolveServer(pThis);
        dispatcher = server as IOpcHdaServerDispatcher ?? (server is null ? null : new OpcHdaServerDispatcher(server));
        return dispatcher is not null;
    }

    private static bool HasItemReadArgs(uint count, IntPtr handles, IntPtr* values, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && handles != IntPtr.Zero && values != null && errors != null;

    private static bool HasProcessedReadArgs(uint count, IntPtr handles, IntPtr aggregateIds, IntPtr* values, IntPtr* errors) =>
        HasItemReadArgs(count, handles, values, errors) && aggregateIds != IntPtr.Zero;

    private static bool HasAtTimeReadArgs(uint timestampCount, IntPtr timestamps, uint count, IntPtr handles, IntPtr* values, IntPtr* errors) =>
        timestampCount is > 0 and <= int.MaxValue && timestamps != IntPtr.Zero && HasItemReadArgs(count, handles, values, errors);

    private static bool HasAttributeReadArgs(uint count, IntPtr attributeIds, IntPtr* values, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && attributeIds != IntPtr.Zero && values != null && errors != null;

    private static bool HasAsyncItemReadArgs(uint count, IntPtr handles, IntPtr cancelId, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && handles != IntPtr.Zero && cancelId != IntPtr.Zero && errors != null;

    private static bool HasAsyncProcessedReadArgs(uint count, IntPtr handles, IntPtr aggregateIds, IntPtr cancelId, IntPtr* errors) =>
        HasAsyncItemReadArgs(count, handles, cancelId, errors) && aggregateIds != IntPtr.Zero;

    private static bool HasAsyncAtTimeReadArgs(uint timestampCount, IntPtr timestamps, uint count, IntPtr handles, IntPtr cancelId, IntPtr* errors) =>
        timestampCount is > 0 and <= int.MaxValue && timestamps != IntPtr.Zero && HasAsyncItemReadArgs(count, handles, cancelId, errors);

    private static bool HasAsyncAttributeReadArgs(uint count, IntPtr attributeIds, IntPtr cancelId, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && attributeIds != IntPtr.Zero && cancelId != IntPtr.Zero && errors != null;

    private static bool HasAnnotationInsertArgs(uint count, IntPtr handles, IntPtr timestamps, IntPtr annotations, IntPtr* errors) =>
        count is > 0 and <= int.MaxValue && handles != IntPtr.Zero && timestamps != IntPtr.Zero && annotations != IntPtr.Zero && errors != null;

    private static bool HasAsyncAnnotationInsertArgs(uint count, IntPtr handles, IntPtr timestamps, IntPtr annotations, IntPtr cancelId, IntPtr* errors) =>
        HasAnnotationInsertArgs(count, handles, timestamps, annotations, errors) && cancelId != IntPtr.Zero;

    private static bool HasAsyncAdviseRawArgs(uint count, IntPtr startTime, long updateInterval, IntPtr handles, IntPtr cancelId, IntPtr* errors) =>
        HasAsyncItemReadArgs(count, handles, cancelId, errors) && startTime != IntPtr.Zero && updateInterval > 0;

    private static bool HasAsyncAdviseProcessedArgs(uint count, IntPtr startTime, long resampleInterval, IntPtr handles, IntPtr aggregateIds, uint intervalCount, IntPtr cancelId, IntPtr* errors) =>
        HasAsyncAdviseRawArgs(count, startTime, resampleInterval, handles, cancelId, errors) && aggregateIds != IntPtr.Zero && intervalCount is > 0 and <= int.MaxValue;

    private static void ZeroReadOuts(IntPtr* values, IntPtr* errors)
    {
        ZeroOut(values);
        ZeroOut(errors);
    }

    private static int[] BuildResultErrors(int resultCount, int expectedCount, int missingError)
    {
        var errors = new int[expectedCount];
        for (int i = 0; i < expectedCount; i++)
        {
            errors[i] = i < resultCount ? OpcHdaServerCcw.S_OK : missingError;
        }

        return errors;
    }

    private static int[] BuildAttributeErrors(int[] attributeIds, int resultCount, int expectedCount)
    {
        var errors = new int[expectedCount];
        for (int i = 0; i < expectedCount; i++)
        {
            errors[i] = attributeIds[i] <= 0
                ? OpcHdaErrors.OPCHDA_E_INVALIDATTRID
                : i < resultCount ? OpcHdaServerCcw.S_OK : OpcHdaErrors.OPCHDA_E_UNKNOWNATTRID;
        }

        return errors;
    }

    private static OpcHdaItem[] NormalizeItems(OpcHdaItem[] items, int count)
    {
        var normalized = new OpcHdaItem[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = i < items.Length ? items[i] : new OpcHdaItem(0, 0, [], [], []);
        }

        return normalized;
    }

    private static OpcHdaModifiedItem[] NormalizeModifiedItems(OpcHdaModifiedItem[] items, int count)
    {
        var normalized = new OpcHdaModifiedItem[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = i < items.Length ? items[i] : new OpcHdaModifiedItem(0, [], [], [], [], [], []);
        }

        return normalized;
    }

    private static OpcHdaAttribute[] NormalizeAttributes(OpcHdaAttribute[] attributes, int[] attributeIds, int count)
    {
        var normalized = new OpcHdaAttribute[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = i < attributes.Length ? attributes[i] : new OpcHdaAttribute(0, attributeIds[i], [], []);
        }

        return normalized;
    }

    private static OpcHdaAnnotation[] NormalizeAnnotations(OpcHdaAnnotation[] annotations, int count)
    {
        var normalized = new OpcHdaAnnotation[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = i < annotations.Length ? annotations[i] : new OpcHdaAnnotation(0, [], [], [], []);
        }

        return normalized;
    }

    private static void AssignItemOuts(IntPtr* valuesOut, IntPtr* errorsOut, OpcHdaItem[] items, int[] errors)
    {
        IntPtr values = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            values = OpcHdaItemMarshaler.AllocateItemArray(items);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            *valuesOut = values;
            *errorsOut = errorsPtr;
            values = errorsPtr = IntPtr.Zero;
        }
        finally
        {
            OpcHdaItemMarshaler.FreeItemArray(values, items.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    private static void AssignModifiedOuts(IntPtr* valuesOut, IntPtr* errorsOut, OpcHdaModifiedItem[] items, int[] errors)
    {
        IntPtr values = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            values = OpcHdaItemMarshaler.AllocateModifiedItemArray(items);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            *valuesOut = values;
            *errorsOut = errorsPtr;
            values = errorsPtr = IntPtr.Zero;
        }
        finally
        {
            OpcHdaItemMarshaler.FreeModifiedItemArray(values, items.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    private static void AssignAttributeOuts(IntPtr* valuesOut, IntPtr* errorsOut, OpcHdaAttribute[] attributes, int[] errors)
    {
        IntPtr values = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            values = OpcHdaItemMarshaler.AllocateAttributeArray(attributes);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            *valuesOut = values;
            *errorsOut = errorsPtr;
            values = errorsPtr = IntPtr.Zero;
        }
        finally
        {
            OpcHdaItemMarshaler.FreeAttributeArray(values, attributes.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    private static void AssignAnnotationOuts(IntPtr* valuesOut, IntPtr* errorsOut, OpcHdaAnnotation[] annotations, int[] errors)
    {
        IntPtr values = IntPtr.Zero;
        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            values = OpcHdaItemMarshaler.AllocateAnnotationArray(annotations);
            errorsPtr = OpcHdaItemMarshaler.AllocateInt32Array(errors);
            *valuesOut = values;
            *errorsOut = errorsPtr;
            values = errorsPtr = IntPtr.Zero;
        }
        finally
        {
            OpcHdaItemMarshaler.FreeAnnotationArray(values, annotations.Length);
            Marshal.FreeCoTaskMem(errorsPtr);
        }
    }

    private static int RegisterPendingOperation(OpcHdaServerCcw.CcwSession session, IntPtr pdwCancelID)
    {
        int cancelId = Interlocked.Increment(ref session.NextCancelId);
        if (cancelId == 0)
        {
            cancelId = Interlocked.Increment(ref session.NextCancelId);
        }

        var cts = new CancellationTokenSource();
        if (!session.PendingOperations.TryAdd(cancelId, cts))
        {
            cts.Dispose();
            throw new InvalidOperationException("Unable to register HDA async read operation.");
        }

        WriteUInt32(pdwCancelID, unchecked((uint)cancelId));
        return cancelId;
    }

    private static CancellationToken GetPendingCancellationToken(OpcHdaServerCcw.CcwSession session, int cancelId) =>
        session.PendingOperations.TryGetValue(cancelId, out CancellationTokenSource? cts)
            ? cts.Token
            : CancellationToken.None;

    private static void RemovePendingOperation(OpcHdaServerCcw.CcwSession session, int cancelId)
    {
        if (cancelId != 0 && session.PendingOperations.TryRemove(cancelId, out CancellationTokenSource? cts))
        {
            cts.Dispose();
        }
    }

    private static void QueueReadComplete(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, OpcHdaItem[] items, int[] errors) =>
        QueueCallback(session, cancelId, sink => sink.OnReadComplete(transactionId, GetMasterHResult(errors), items, errors));

    private static void QueueReadModifiedComplete(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, OpcHdaModifiedItem[] items, int[] errors) =>
        QueueCallback(session, cancelId, sink => sink.OnReadModifiedComplete(transactionId, GetMasterHResult(errors), items, errors));

    private static void QueueReadAttributeComplete(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, int clientHandle, OpcHdaAttribute[] attributes, int[] errors) =>
        QueueCallback(session, cancelId, sink => sink.OnReadAttributeComplete(transactionId, GetMasterHResult(errors), clientHandle, attributes, errors));

    private static void QueueReadAnnotationsComplete(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, OpcHdaAnnotation[] annotations, int[] errors) =>
        QueueCallback(session, cancelId, sink => sink.OnReadAnnotations(transactionId, GetMasterHResult(errors), annotations, errors));

    private static void QueueInsertAnnotationsComplete(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, int[] clientHandles, int[] errors) =>
        QueueCallback(session, cancelId, sink => sink.OnInsertAnnotations(transactionId, GetMasterHResult(errors), clientHandles, errors));

    private static void QueueDataChangeStream(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, IAsyncEnumerable<OpcHdaDataUpdate> updates) =>
        _ = Task.Run(() => PumpDataChangeStreamAsync(session, cancelId, transactionId, updates));

    private static void QueueCallback(OpcHdaServerCcw.CcwSession session, int cancelId, Action<OpcHdaCallbackProxy> callback)
    {
        _ = Task.Run(() => CompleteCallback(session, cancelId, callback));
    }

    private static void CompleteCallback(OpcHdaServerCcw.CcwSession session, int cancelId, Action<OpcHdaCallbackProxy> callback)
    {
        if (!session.PendingOperations.TryRemove(cancelId, out CancellationTokenSource? cts))
        {
            return;
        }

        try
        {
            if (!cts.IsCancellationRequested)
            {
                foreach (OpcHdaCallbackProxy sink in session.ScmSinks.Values)
                {
                    try
                    {
                        callback(sink);
                    }
                    catch (COMException)
                    {
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                }
            }
        }
        finally
        {
            cts.Dispose();
        }
    }

    private static Task PumpDataChangeStreamAsync(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, IAsyncEnumerable<OpcHdaDataUpdate> updates) =>
        OpcHdaServerCcwDataChangePump.PumpAsync(session, cancelId, transactionId, updates);

    private static void FireDataChange(OpcHdaServerCcw.CcwSession session, int transactionId, OpcHdaDataUpdate update)
    {
        int status = GetMasterHResult(update.Errors);
        foreach (OpcHdaCallbackProxy sink in session.ScmSinks.Values)
        {
            try
            {
                sink.OnDataChange(transactionId, status, update.ItemValues, update.Errors);
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

    private static int GetAttributeClientHandle(OpcHdaAttribute[] attributes) => attributes.Length == 0 ? 0 : attributes[0].ClientHandle;

    private static int GetMasterHResult(int[] errors) => HasAnyFailure(errors) ? OpcHdaServerCcw.S_FALSE : OpcHdaServerCcw.S_OK;

    private static bool TryResolveServer(IntPtr pThis, out IOpcHdaServer? server)
    {
        server = OpcHdaServerCcw.ResolveServer(pThis);
        return server is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentException => OpcHdaServerCcw.E_INVALIDARG,
        _ => OpcHdaServerCcw.E_FAIL,
    };

    private static bool HasHistorianStatusOutParams(IntPtr status, IntPtr* currentTime, IntPtr* startTime, IntPtr maxReturnValues) =>
        status != IntPtr.Zero && currentTime != null && startTime != null && maxReturnValues != IntPtr.Zero;

    private static int CountToInt(uint count)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(count, (uint)int.MaxValue);
        return (int)count;
    }

    private static bool HasBrowseFilterPointers(uint count, IntPtr pdwAttrID, IntPtr pOperator, IntPtr vFilter) =>
        count == 0 || (pdwAttrID != IntPtr.Zero && pOperator != IntPtr.Zero && vFilter != IntPtr.Zero);

    private static OpcHdaBrowseFilter[] ReadBrowseFilters(
        int count,
        IntPtr pdwAttrID,
        IntPtr pOperator,
        IntPtr vFilter,
        out int[] errors)
    {
        var filters = new OpcHdaBrowseFilter[count];
        errors = new int[count];
        for (int i = 0; i < count; i++)
        {
            int attributeId = Marshal.ReadInt32(pdwAttrID, checked(i * sizeof(int)));
            int operatorCode = Marshal.ReadInt32(pOperator, checked(i * sizeof(int)));
            IntPtr variantSlot = IntPtr.Add(vFilter, checked(i * NativeHdaVariantReader.VariantSize));
            if (!NativeHdaVariantReader.TryRead(variantSlot, out OpcVariant value))
            {
                errors[i] = OpcHdaErrors.OPCHDA_E_INVALIDDATATYPE;
            }
            else
            {
                errors[i] = ValidateBrowseFilterShape(attributeId, operatorCode);
            }

            filters[i] = new OpcHdaBrowseFilter(attributeId, operatorCode, value);
        }

        return filters;
    }

    private static int ValidateBrowseFilterShape(int attributeId, int operatorCode)
    {
        if (attributeId <= 0)
        {
            return OpcHdaErrors.OPCHDA_E_INVALIDATTRID;
        }
        if (operatorCode is < 1 or > 6)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        return OpcHdaServerCcw.S_OK;
    }

    private static int[] MergeBrowseFilterErrors(int[] localErrors, int[] dispatcherErrors)
    {
        var errors = new int[localErrors.Length];
        for (int i = 0; i < errors.Length; i++)
        {
            errors[i] = localErrors[i] != OpcHdaServerCcw.S_OK ? localErrors[i] : dispatcherErrors[i];
        }

        return errors;
    }

    private static OpcHdaBrowseFilter[] FilterSuccessfulBrowseFilters(OpcHdaBrowseFilter[] filters, int[] errors)
    {
        int validCount = 0;
        for (int i = 0; i < errors.Length; i++)
        {
            if (errors[i] == OpcHdaServerCcw.S_OK)
            {
                validCount++;
            }
        }

        var validFilters = new OpcHdaBrowseFilter[validCount];
        int target = 0;
        for (int i = 0; i < filters.Length; i++)
        {
            if (errors[i] == OpcHdaServerCcw.S_OK)
            {
                validFilters[target++] = filters[i];
            }
        }

        return validFilters;
    }

    private static bool HasAnyFailure(int[] errors)
    {
        for (int i = 0; i < errors.Length; i++)
        {
            if (errors[i] != OpcHdaServerCcw.S_OK)
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateAttributeLengths(int[] ids, string[] names, string[] descriptions, int[] dataTypes)
    {
        ValidateLength(ids.Length, names.Length, nameof(names));
        ValidateLength(ids.Length, descriptions.Length, nameof(descriptions));
        ValidateLength(ids.Length, dataTypes.Length, nameof(dataTypes));
    }

    private static void ValidateAggregateLengths(int[] ids, string[] names, string[] descriptions)
    {
        ValidateLength(ids.Length, names.Length, nameof(names));
        ValidateLength(ids.Length, descriptions.Length, nameof(descriptions));
    }

    private static void ValidateLength(int expected, int actual, string parameterName)
    {
        if (actual != expected)
        {
            throw new InvalidOperationException($"{parameterName} length {actual} does not match expected {expected}.");
        }
    }

    private static int ToHistorianStatus(OpcServerState state) => state switch
    {
        OpcServerState.Running => 1,
        OpcServerState.Failed or OpcServerState.CommFault => 2,
        _ => 3,
    };

    private static void WriteVersion(Version version, IntPtr major, IntPtr minor, IntPtr build)
    {
        WriteUInt16(major, ToUInt16(version.Major));
        WriteUInt16(minor, ToUInt16(version.Minor));
        WriteUInt16(build, ToUInt16(Math.Max(0, version.Build)));
    }

    private static ushort ToUInt16(int value) => checked((ushort)Math.Clamp(value, ushort.MinValue, ushort.MaxValue));

    private static string[] ReadLpwStrArray(IntPtr ptr, int count)
    {
        var values = new string[count];
        for (int i = 0; i < count; i++)
        {
            IntPtr s = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
            values[i] = Marshal.PtrToStringUni(s) ?? string.Empty;
        }
        return values;
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

    private static OpcHdaAnnotation[] ReadAnnotationInputArray(IntPtr ptr, int count)
    {
        var annotations = new OpcHdaAnnotation[count];
        for (int i = 0; i < count; i++)
        {
            annotations[i] = ReadAnnotationInput(IntPtr.Add(ptr, checked(i * AnnotationInputSize)));
        }

        return annotations;
    }

    private static OpcHdaAnnotation ReadAnnotationInput(IntPtr slot)
    {
        int clientHandle = Marshal.ReadInt32(slot);
        int valueCount = CountToInt(unchecked((uint)Marshal.ReadInt32(slot, sizeof(int))));
        int offset = PointerAlignedAfterTwoDwords;
        DateTimeOffset[] timestamps = ReadAnnotationFileTimes(Marshal.ReadIntPtr(slot, offset), valueCount);
        string?[] annotationValues = ReadStringPointerValues(Marshal.ReadIntPtr(slot, offset + IntPtr.Size), valueCount);
        DateTimeOffset[] annotationTimes = ReadAnnotationFileTimes(Marshal.ReadIntPtr(slot, offset + (2 * IntPtr.Size)), valueCount);
        string?[] users = ReadStringPointerValues(Marshal.ReadIntPtr(slot, offset + (3 * IntPtr.Size)), valueCount);
        return new OpcHdaAnnotation(clientHandle, timestamps, annotationValues, annotationTimes, users);
    }

    private static DateTimeOffset[] ReadAnnotationFileTimes(IntPtr ptr, int count)
    {
        if (count > 0 && ptr == IntPtr.Zero)
        {
            throw new ArgumentException("FILETIME array pointer is null.", nameof(ptr));
        }

        var values = new DateTimeOffset[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = DateTimeOffset.FromFileTime(Marshal.ReadInt64(ptr, checked(i * sizeof(long))));
        }

        return values;
    }

    private static string?[] ReadStringPointerValues(IntPtr ptr, int count)
    {
        if (count > 0 && ptr == IntPtr.Zero)
        {
            throw new ArgumentException("String pointer array is null.", nameof(ptr));
        }

        var values = new string?[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = Marshal.PtrToStringUni(Marshal.ReadIntPtr(ptr, checked(i * IntPtr.Size)));
        }

        return values;
    }

    private static int[] GetAnnotationClientHandles(OpcHdaAnnotation[] annotations)
    {
        var handles = new int[annotations.Length];
        for (int i = 0; i < annotations.Length; i++)
        {
            handles[i] = annotations[i].ClientHandle;
        }

        return handles;
    }

    private static int AnnotationInputSize => PointerAlignedAfterTwoDwords + (4 * IntPtr.Size);

    private static int PointerAlignedAfterTwoDwords => Align(2 * sizeof(int), IntPtr.Size);

    private static int Align(int value, int alignment) => (value + alignment - 1) & ~(alignment - 1);

    private static IntPtr AllocateSucceededErrors(int count)
    {
        var errors = new int[count];
        return AllocateInt32Array(errors);
    }

    private static IntPtr AllocateInt32Array(int[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
        Marshal.Copy(values, 0, ptr, values.Length);
        return ptr;
    }

    private static IntPtr AllocateUInt16Array(int[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(ushort));
        for (int i = 0; i < values.Length; i++)
        {
            Marshal.WriteInt16(ptr, i * sizeof(ushort), unchecked((short)ToUInt16(values[i])));
        }
        return ptr;
    }

    private static IntPtr AllocateLpwStrArray(string?[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * IntPtr.Size);
        for (int i = 0; i < values.Length; i++)
        {
            Marshal.WriteIntPtr(ptr, i * IntPtr.Size, AllocateLpwStr(values[i]));
        }
        return ptr;
    }

    private static IntPtr AllocateFileTime(DateTimeOffset value)
    {
        IntPtr ptr = Marshal.AllocCoTaskMem(sizeof(long));
        Marshal.WriteInt64(ptr, ToFileTime(value));
        return ptr;
    }

    private static long ToFileTime(DateTimeOffset value) => value == default ? 0L : value.ToFileTime();

    private static void WriteLpwStrPtr(IntPtr ppwzOut, string? value)
    {
        if (ppwzOut != IntPtr.Zero)
        {
            Marshal.WriteIntPtr(ppwzOut, AllocateLpwStr(value));
        }
    }

    private static IntPtr AllocateLpwStr(string? value)
    {
        if (value is null)
        {
            return IntPtr.Zero;
        }
        int byteCount = (value.Length + 1) * sizeof(char);
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        Marshal.Copy(value.ToCharArray(), 0, ptr, value.Length);
        Marshal.WriteInt16(ptr, value.Length * sizeof(char), 0);
        return ptr;
    }

    private static void WriteUInt32(IntPtr p, uint value)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt32(p, unchecked((int)value));
        }
    }

    internal static void FireDataChangeCallback(OpcHdaServerCcw.CcwSession session, int transactionId, OpcHdaDataUpdate update)
    {
        int status = GetMasterHResult(update.Errors);
        foreach (OpcHdaCallbackProxy sink in session.ScmSinks.Values)
        {
            try
            {
                sink.OnDataChange(transactionId, status, update.ItemValues, update.Errors);
            }
            catch (COMException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }
    }

    internal static void RemovePendingOperationForPump(OpcHdaServerCcw.CcwSession session, int cancelId) =>
        RemovePendingOperation(session, cancelId);

    private static void WriteInt32(IntPtr p, int value)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt32(p, value);
        }
    }

    private static void WriteUInt16(IntPtr p, ushort value)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt16(p, unchecked((short)value));
        }
    }

    private static void ZeroOut(IntPtr* pp)
    {
        if (pp != null)
        {
            *pp = IntPtr.Zero;
        }
    }
}

[SupportedOSPlatform("windows")]
internal static class OpcHdaServerCcwDataChangePump
{
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Background callback pump must clean up pending operations.")]
    public static async Task PumpAsync(OpcHdaServerCcw.CcwSession session, int cancelId, int transactionId, IAsyncEnumerable<OpcHdaDataUpdate> updates)
    {
        if (!session.PendingOperations.TryGetValue(cancelId, out CancellationTokenSource? cts))
        {
            return;
        }

        CancellationToken cancellationToken = cts.Token;
        try
        {
            await foreach (OpcHdaDataUpdate update in updates.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                OpcHdaServerCcwMethods.FireDataChangeCallback(session, transactionId, update);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception)
        {
        }
        finally
        {
            OpcHdaServerCcwMethods.RemovePendingOperationForPump(session, cancelId);
        }
    }
}
