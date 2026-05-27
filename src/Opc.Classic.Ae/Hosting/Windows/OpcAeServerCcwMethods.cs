//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting.Windows;

/// <summary>
/// <c>IOPCEventServer</c> + <c>IOPCEventSubscriptionMgt</c> method bodies bound
/// into the <see cref="OpcAeServerCcw"/> vtables.
/// </summary>
/// <remarks>
/// Simple scalar methods dispatch to the managed server/subscription. Methods
/// involving interface-pointer returns or CoTaskMem arrays of strings, CLSIDs,
/// attributes, filters, or condition structs return <c>E_NOTIMPL</c> until full
/// COM marshaling is wired.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcAeServerCcwMethods
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetStatus(IntPtr pThis, IntPtr* ppEventServerStatus)
    {
        if (ppEventServerStatus == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        *ppEventServerStatus = IntPtr.Zero;
        if (!TryResolveServer(pThis, out IOpcAeServer? server))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            OpcServerStatus status = server!.GetStatusAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppEventServerStatus = AllocateOpcEventServerStatus(status);
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int CreateEventSubscription(IntPtr pThis, int active, int bufferTime, int maxSize, int clientSubscription, Guid* riid, IntPtr* ppUnk, IntPtr pRevisedBufferTime, IntPtr pRevisedMaxSize)
    {
        WriteNull(ppUnk);
        WriteInt32(pRevisedBufferTime, 0);
        WriteInt32(pRevisedMaxSize, 0);
        if (riid == null || ppUnk == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!OpcAeSubscriptionCcw.SupportsInterface(*riid))
        {
            return OpcAeServerCcw.E_NOINTERFACE;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            IOPCEventSubscriptionMgt subscription = dispatcher!.CreateEventSubscriptionAsync(
                active != 0,
                bufferTime,
                maxSize,
                clientSubscription,
                *riid,
                out int revisedBufferTime,
                out int revisedMaxSize,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            IntPtr subscriptionCcw = OpcAeSubscriptionCcw.Create(subscription, *riid, dispatcher);
            if (subscriptionCcw == IntPtr.Zero)
            {
                return OpcAeServerCcw.E_NOINTERFACE;
            }
            *ppUnk = subscriptionCcw;
            WriteInt32(pRevisedBufferTime, revisedBufferTime);
            WriteInt32(pRevisedMaxSize, revisedMaxSize);
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QueryAvailableFilters(IntPtr pThis, IntPtr pFilterMask)
    {
        if (!TryResolveServer(pThis, out IOpcAeServer? server))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            int filters = server!.QueryAvailableFiltersAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pFilterMask, filters);
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int QueryEventCategories(IntPtr pThis, int eventType, IntPtr pCount, IntPtr* ppEventCategories, IntPtr* ppEventCategoryDescs)
    {
        // Requires paired CoTaskMem arrays of DWORDs and LPWSTRs.
        _ = pThis; _ = eventType;
        WriteInt32(pCount, 0);
        WriteNull(ppEventCategories);
        WriteNull(ppEventCategoryDescs);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int QueryConditionNames(IntPtr pThis, int eventCategory, IntPtr pCount, IntPtr* ppConditionNames)
    {
        // Requires a CoTaskMem array of LPWSTR pointers.
        _ = pThis; _ = eventCategory;
        WriteInt32(pCount, 0);
        WriteNull(ppConditionNames);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int QuerySubConditionNames(IntPtr pThis, IntPtr conditionName, IntPtr pCount, IntPtr* ppSubConditionNames)
    {
        // Requires a CoTaskMem array of LPWSTR pointers.
        _ = pThis; _ = conditionName;
        WriteInt32(pCount, 0);
        WriteNull(ppSubConditionNames);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int QuerySourceConditions(IntPtr pThis, IntPtr source, IntPtr pCount, IntPtr* ppConditionNames)
    {
        // Requires a CoTaskMem array of LPWSTR pointers.
        _ = pThis; _ = source;
        WriteInt32(pCount, 0);
        WriteNull(ppConditionNames);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int QueryEventAttributes(IntPtr pThis, int eventCategory, IntPtr pCount, IntPtr* ppAttrIds, IntPtr* ppAttrDescs, IntPtr* ppAttrTypes)
    {
        // Requires three correlated CoTaskMem arrays: DWORDs, LPWSTRs, VARTYPEs.
        _ = pThis; _ = eventCategory;
        WriteInt32(pCount, 0);
        WriteNull(ppAttrIds);
        WriteNull(ppAttrDescs);
        WriteNull(ppAttrTypes);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int TranslateToItemIDs(IntPtr pThis, IntPtr source, int eventCategory, IntPtr conditionName, IntPtr subconditionName, int count, IntPtr assocAttrIds, IntPtr* ppAttrItemIds, IntPtr* ppNodeNames, IntPtr* ppClsids)
    {
        // Requires CoTaskMem arrays of LPWSTRs and CLSIDs sized by an input count.
        _ = pThis; _ = source; _ = eventCategory; _ = conditionName; _ = subconditionName; _ = count; _ = assocAttrIds;
        WriteNull(ppAttrItemIds);
        WriteNull(ppNodeNames);
        WriteNull(ppClsids);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int GetConditionState(IntPtr pThis, IntPtr source, IntPtr conditionName, int eventAttrCount, IntPtr attributeIds, IntPtr* ppConditionState)
    {
        // OPCCONDITIONSTATE contains nested LPWSTR, VARIANT, and HRESULT arrays.
        _ = pThis; _ = source; _ = conditionName; _ = eventAttrCount; _ = attributeIds;
        WriteNull(ppConditionState);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int EnableConditionByArea(IntPtr pThis, int areaCount, IntPtr areas)
    {
        // Requires LPWSTR array input marshaling.
        _ = pThis; _ = areaCount; _ = areas;
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int EnableConditionBySource(IntPtr pThis, int sourceCount, IntPtr sources)
    {
        // Requires LPWSTR array input marshaling.
        _ = pThis; _ = sourceCount; _ = sources;
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int DisableConditionByArea(IntPtr pThis, int areaCount, IntPtr areas)
    {
        // Requires LPWSTR array input marshaling.
        _ = pThis; _ = areaCount; _ = areas;
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int DisableConditionBySource(IntPtr pThis, int sourceCount, IntPtr sources)
    {
        // Requires LPWSTR array input marshaling.
        _ = pThis; _ = sourceCount; _ = sources;
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int AckCondition(IntPtr pThis, int count, IntPtr acknowledgerId, IntPtr comment, IntPtr sources, IntPtr conditionNames, IntPtr activeTimes, IntPtr cookies, IntPtr* ppErrors)
    {
        // Requires multiple correlated arrays including FILETIME values and per-call HRESULTs.
        _ = pThis; _ = count; _ = acknowledgerId; _ = comment; _ = sources; _ = conditionNames; _ = activeTimes; _ = cookies;
        WriteNull(ppErrors);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int CreateAreaBrowser(IntPtr pThis, Guid* riid, IntPtr* ppUnk)
    {
        WriteNull(ppUnk);
        if (riid == null || ppUnk == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!OpcAeAreaBrowserCcw.SupportsInterface(*riid))
        {
            return OpcAeServerCcw.E_NOINTERFACE;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            IOpcAeAreaBrowserDispatcher browser = dispatcher!.CreateAreaBrowserAsync(*riid, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            IntPtr browserCcw = OpcAeAreaBrowserCcw.Create(browser, *riid);
            if (browserCcw == IntPtr.Zero)
            {
                return OpcAeServerCcw.E_NOINTERFACE;
            }
            *ppUnk = browserCcw;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetFilter(IntPtr pThis, int eventType, int categoryCount, IntPtr eventCategories, int lowSeverity, int highSeverity, int areaCount, IntPtr areas, int sourceCount, IntPtr sources)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
            int[] categoryIds = OpcAeSubscriptionCcw.ReadInt32Array(categoryCount, eventCategories);
            string[] areaNames = OpcAeSubscriptionCcw.ReadStringPointerArray(areaCount, areas);
            string[] sourceNames = OpcAeSubscriptionCcw.ReadStringPointerArray(sourceCount, sources);
#pragma warning disable VSTHRD002
            subscription!.SetFilterAsync(eventType, categoryIds, lowSeverity, highSeverity, areaNames, sourceNames, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetFilter(IntPtr pThis, IntPtr pEventType, IntPtr pCategoryCount, IntPtr* ppEventCategories, IntPtr pLowSeverity, IntPtr pHighSeverity, IntPtr pAreaCount, IntPtr* ppAreaList, IntPtr pSourceCount, IntPtr* ppSourceList)
    {
        InitializeGetFilterOutputs(pEventType, pCategoryCount, ppEventCategories, pLowSeverity, pHighSeverity, pAreaCount, ppAreaList, pSourceCount, ppSourceList);
        if (IsInvalidGetFilterArgs(pEventType, pCategoryCount, ppEventCategories, pLowSeverity, pHighSeverity, pAreaCount, ppAreaList, pSourceCount, ppSourceList))
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr categoriesPtr = IntPtr.Zero;
        IntPtr areasPtr = IntPtr.Zero;
        IntPtr sourcesPtr = IntPtr.Zero;
        try
        {
#pragma warning disable VSTHRD002
            subscription!.GetFilterAsync(out int eventType, out int[] categories, out int lowSeverity, out int highSeverity, out string[] areas, out string[] sources, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            categories ??= Array.Empty<int>();
            areas ??= Array.Empty<string>();
            sources ??= Array.Empty<string>();
            categoriesPtr = OpcAeSubscriptionCcw.AllocateInt32Array(categories);
            areasPtr = OpcAeSubscriptionCcw.AllocateStringPointerArray(areas);
            sourcesPtr = OpcAeSubscriptionCcw.AllocateStringPointerArray(sources);
            AssignGetFilterOutputs(
                eventType,
                categories,
                categoriesPtr,
                lowSeverity,
                highSeverity,
                areas,
                areasPtr,
                sources,
                sourcesPtr,
                pEventType,
                pCategoryCount,
                ppEventCategories,
                pLowSeverity,
                pHighSeverity,
                pAreaCount,
                ppAreaList,
                pSourceCount,
                ppSourceList);
            categoriesPtr = IntPtr.Zero;
            areasPtr = IntPtr.Zero;
            sourcesPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeSubscriptionCcw.FreeCoTaskMem(categoriesPtr);
            OpcAeSubscriptionCcw.FreeStringPointerArray(areasPtr);
            OpcAeSubscriptionCcw.FreeStringPointerArray(sourcesPtr);
            return MapHResult(ex);
        }
    }

    private static void InitializeGetFilterOutputs(IntPtr pEventType, IntPtr pCategoryCount, IntPtr* ppEventCategories, IntPtr pLowSeverity, IntPtr pHighSeverity, IntPtr pAreaCount, IntPtr* ppAreaList, IntPtr pSourceCount, IntPtr* ppSourceList)
    {
        WriteInt32(pEventType, 0);
        WriteInt32(pCategoryCount, 0);
        WriteNull(ppEventCategories);
        WriteInt32(pLowSeverity, 0);
        WriteInt32(pHighSeverity, 0);
        WriteInt32(pAreaCount, 0);
        WriteNull(ppAreaList);
        WriteInt32(pSourceCount, 0);
        WriteNull(ppSourceList);
    }

    private static bool IsInvalidGetFilterArgs(IntPtr pEventType, IntPtr pCategoryCount, IntPtr* ppEventCategories, IntPtr pLowSeverity, IntPtr pHighSeverity, IntPtr pAreaCount, IntPtr* ppAreaList, IntPtr pSourceCount, IntPtr* ppSourceList) =>
        pEventType == IntPtr.Zero ||
        pCategoryCount == IntPtr.Zero ||
        ppEventCategories == null ||
        pLowSeverity == IntPtr.Zero ||
        pHighSeverity == IntPtr.Zero ||
        pAreaCount == IntPtr.Zero ||
        ppAreaList == null ||
        pSourceCount == IntPtr.Zero ||
        ppSourceList == null;

    private static void AssignGetFilterOutputs(
        int eventType,
        int[] categories,
        IntPtr categoriesPtr,
        int lowSeverity,
        int highSeverity,
        string[] areas,
        IntPtr areasPtr,
        string[] sources,
        IntPtr sourcesPtr,
        IntPtr pEventType,
        IntPtr pCategoryCount,
        IntPtr* ppEventCategories,
        IntPtr pLowSeverity,
        IntPtr pHighSeverity,
        IntPtr pAreaCount,
        IntPtr* ppAreaList,
        IntPtr pSourceCount,
        IntPtr* ppSourceList)
    {
        WriteInt32(pEventType, eventType);
        WriteInt32(pCategoryCount, categories.Length);
        *ppEventCategories = categoriesPtr;
        WriteInt32(pLowSeverity, lowSeverity);
        WriteInt32(pHighSeverity, highSeverity);
        WriteInt32(pAreaCount, areas.Length);
        *ppAreaList = areasPtr;
        WriteInt32(pSourceCount, sources.Length);
        *ppSourceList = sourcesPtr;
    }

    [UnmanagedCallersOnly]
    public static int SetReturnedAttributes(IntPtr pThis, int eventCategory, int count, IntPtr attributeIds)
    {
        // Requires DWORD array input marshaling.
        _ = pThis; _ = eventCategory; _ = count; _ = attributeIds;
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int GetReturnedAttributes(IntPtr pThis, int eventCategory, IntPtr pCount, IntPtr* ppAttributeIds)
    {
        // Requires allocating a CoTaskMem DWORD array.
        _ = pThis; _ = eventCategory;
        WriteInt32(pCount, 0);
        WriteNull(ppAttributeIds);
        return OpcAeServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Refresh(IntPtr pThis, int connection)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            subscription!.RefreshAsync(connection, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int CancelRefresh(IntPtr pThis, int connection)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            subscription!.CancelRefreshAsync(connection, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetState(IntPtr pThis, IntPtr pActive, IntPtr pBufferTime, IntPtr pMaxSize, IntPtr pClientSubscription)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            subscription!.GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pActive, active ? 1 : 0);
            WriteInt32(pBufferTime, bufferTime);
            WriteInt32(pMaxSize, maxSize);
            WriteInt32(pClientSubscription, clientSubscription);
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetState(IntPtr pThis, IntPtr pActive, IntPtr pBufferTime, IntPtr pMaxSize, int clientSubscription, IntPtr pRevisedBufferTime, IntPtr pRevisedMaxSize)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }
        try
        {
            ReadCurrentState(subscription!, out bool active, out int bufferTime, out int maxSize);
            ApplyRequestedState(pActive, pBufferTime, pMaxSize, ref active, ref bufferTime, ref maxSize);
#pragma warning disable VSTHRD002
            subscription!.SetStateAsync(active, bufferTime, maxSize, clientSubscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pRevisedBufferTime, revisedBufferTime);
            WriteInt32(pRevisedMaxSize, revisedMaxSize);
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolveServer(IntPtr pThis, out IOpcAeServer? server)
    {
        server = OpcAeServerCcw.ResolveServer(pThis);
        return server is not null;
    }

    private static bool TryResolveDispatcher(IntPtr pThis, out IOpcAeServerDispatcher? dispatcher)
    {
        dispatcher = OpcAeServerCcw.ResolveDispatcher(pThis);
        return dispatcher is not null;
    }

    private static bool TryResolveSubscription(IntPtr pThis, out IOPCEventSubscriptionMgt? subscription)
    {
        subscription = OpcAeServerCcw.ResolveSubscription(pThis);
        return subscription is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentNullException => OpcAeServerCcw.E_INVALIDARG,
        ArgumentOutOfRangeException => OpcAeServerCcw.E_INVALIDARG,
        ArgumentException => OpcAeServerCcw.E_INVALIDARG,
        _ => OpcAeServerCcw.E_FAIL,
    };

    private static void ReadCurrentState(IOPCEventSubscriptionMgt subscription, out bool active, out int bufferTime, out int maxSize)
    {
#pragma warning disable VSTHRD002
        subscription.GetStateAsync(out active, out bufferTime, out maxSize, out _, CancellationToken.None)
            .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
    }

    private static void ApplyRequestedState(IntPtr pActive, IntPtr pBufferTime, IntPtr pMaxSize, ref bool active, ref int bufferTime, ref int maxSize)
    {
        if (pActive != IntPtr.Zero)
        {
            active = Marshal.ReadInt32(pActive) != 0;
        }
        if (pBufferTime != IntPtr.Zero)
        {
            bufferTime = Marshal.ReadInt32(pBufferTime);
        }
        if (pMaxSize != IntPtr.Zero)
        {
            maxSize = Marshal.ReadInt32(pMaxSize);
        }
    }

    private static void WriteInt32(IntPtr p, int value)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt32(p, value);
        }
    }

    private static void WriteNull(IntPtr* pp)
    {
        if (pp != null)
        {
            *pp = IntPtr.Zero;
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

    private static IntPtr AllocateOpcEventServerStatus(OpcServerStatus status)
    {
        int size = sizeof(OPCEVENTSERVERSTATUS_NATIVE);
        IntPtr ptr = Marshal.AllocCoTaskMem(size);
        Version version = status.ServerVersion ?? new Version(1, 0, 0);
        var native = new OPCEVENTSERVERSTATUS_NATIVE
        {
            ftStartTime = status.StartTime.ToFileTime(),
            ftCurrentTime = status.CurrentTime.ToFileTime(),
            ftLastUpdateTime = status.LastUpdateTime.ToFileTime(),
            dwServerState = (int)status.State,
            wMajorVersion = (ushort)version.Major,
            wMinorVersion = (ushort)version.Minor,
            wBuildNumber = (ushort)Math.Max(0, version.Build),
            wReserved = 0,
            szVendorInfo = AllocateLpwStr(status.VendorInfo),
        };
        Marshal.StructureToPtr(native, ptr, fDeleteOld: false);
        return ptr;
    }

    /// <summary>Native OPC AE <c>OPCEVENTSERVERSTATUS</c> layout for CoTaskMemAlloc.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct OPCEVENTSERVERSTATUS_NATIVE
    {
        public long ftStartTime;
        public long ftCurrentTime;
        public long ftLastUpdateTime;
        public int dwServerState;
        public ushort wMajorVersion;
        public ushort wMinorVersion;
        public ushort wBuildNumber;
        public ushort wReserved;
        public IntPtr szVendorInfo;
    }
}
