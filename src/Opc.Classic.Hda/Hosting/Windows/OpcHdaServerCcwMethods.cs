//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
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
    public static int SyncReadRaw(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, int bBounds, uint dwNumItems, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        // OPCHDA_ITEM needs nested FILETIME, DWORD and native VARIANT arrays.
        // Deferred until this assembly has reusable OAUT VARIANT allocation helpers.
        _ = pThis; _ = htStartTime; _ = htEndTime; _ = dwNumValues; _ = bBounds; _ = dwNumItems; _ = phServer;
        ZeroOut(ppItemValues);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int SyncReadProcessed(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, long ftResampleInterval, uint dwNumItems, IntPtr phServer, IntPtr haAggregate, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        _ = pThis; _ = htStartTime; _ = htEndTime; _ = ftResampleInterval; _ = dwNumItems; _ = phServer; _ = haAggregate;
        ZeroOut(ppItemValues);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int SyncReadAtTime(IntPtr pThis, uint dwNumTimeStamps, IntPtr ftTimeStamps, uint dwNumItems, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwNumTimeStamps; _ = ftTimeStamps; _ = dwNumItems; _ = phServer;
        ZeroOut(ppItemValues);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int SyncReadModified(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, uint dwNumItems, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        _ = pThis; _ = htStartTime; _ = htEndTime; _ = dwNumValues; _ = dwNumItems; _ = phServer;
        ZeroOut(ppItemValues);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int SyncReadAttribute(IntPtr pThis, IntPtr htStartTime, IntPtr htEndTime, uint hServer, uint dwNumAttributes, IntPtr pdwAttributeIDs, IntPtr* ppAttributeValues, IntPtr* ppErrors)
    {
        _ = pThis; _ = htStartTime; _ = htEndTime; _ = hServer; _ = dwNumAttributes; _ = pdwAttributeIDs;
        ZeroOut(ppAttributeValues);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncReadRaw(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, int bBounds, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        // Async HDA read completion requires an OPCHDA_HISTORYREADCALLBACK / data
        // callback sink connection; the in-process server contract does not expose it yet.
        _ = pThis; _ = dwTransactionID; _ = htStartTime; _ = htEndTime; _ = dwNumValues; _ = bBounds; _ = dwNumItems; _ = phServer;
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncAdviseRaw(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, long ftUpdateInterval, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwTransactionID; _ = htStartTime; _ = ftUpdateInterval; _ = dwNumItems; _ = phServer;
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncReadProcessed(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, long ftResampleInterval, uint dwNumItems, IntPtr phServer, IntPtr haAggregate, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwTransactionID; _ = htStartTime; _ = htEndTime; _ = ftResampleInterval; _ = dwNumItems; _ = phServer; _ = haAggregate;
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncAdviseProcessed(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, long ftResampleInterval, uint dwNumItems, IntPtr phServer, IntPtr haAggregate, uint dwNumIntervals, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwTransactionID; _ = htStartTime; _ = ftResampleInterval; _ = dwNumItems; _ = phServer; _ = haAggregate; _ = dwNumIntervals;
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncReadAtTime(IntPtr pThis, uint dwTransactionID, uint dwNumTimeStamps, IntPtr ftTimeStamps, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwTransactionID; _ = dwNumTimeStamps; _ = ftTimeStamps; _ = dwNumItems; _ = phServer;
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncReadModified(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint dwNumValues, uint dwNumItems, IntPtr phServer, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwTransactionID; _ = htStartTime; _ = htEndTime; _ = dwNumValues; _ = dwNumItems; _ = phServer;
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncReadAttribute(IntPtr pThis, uint dwTransactionID, IntPtr htStartTime, IntPtr htEndTime, uint hServer, uint dwNumAttributes, IntPtr pdwAttributeIDs, IntPtr pdwCancelID, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwTransactionID; _ = htStartTime; _ = htEndTime; _ = hServer; _ = dwNumAttributes; _ = pdwAttributeIDs;
        WriteUInt32(pdwCancelID, 0);
        ZeroOut(ppErrors);
        return OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AsyncCancel(IntPtr pThis, uint dwCancelID)
    {
        _ = pThis; _ = dwCancelID;
        return OpcHdaServerCcw.E_NOTIMPL;
    }

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
