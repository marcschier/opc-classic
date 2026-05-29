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
/// Scalar and array-heavy methods dispatch to the managed server/subscription
/// and marshal OPC AE native CoTaskMem/BSTR payloads at the COM boundary.
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QueryEventCategories(IntPtr pThis, int eventType, IntPtr pCount, IntPtr* ppEventCategories, IntPtr* ppEventCategoryDescs)
    {
        WriteInt32(pCount, 0);
        WriteNull(ppEventCategories);
        WriteNull(ppEventCategoryDescs);
        if (pCount == IntPtr.Zero || ppEventCategories == null || ppEventCategoryDescs == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr categoriesPtr = IntPtr.Zero;
        IntPtr descriptionsPtr = IntPtr.Zero;
        int count = 0;
        try
        {
#pragma warning disable VSTHRD002
            dispatcher!.QueryEventCategoriesAsync(eventType, out int[] categories, out string[] descriptions, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            categories ??= [];
            descriptions ??= [];
            EnsureSameLength(categories.Length, descriptions.Length, nameof(descriptions));
            OpcAeArrayMarshaler.AllocateDwordArray(categories, out count, out categoriesPtr);
            OpcAeArrayMarshaler.AllocateBstrArray(descriptions, out int descriptionCount, out descriptionsPtr);
            EnsureSameLength(count, descriptionCount, nameof(descriptions));
            WriteInt32(pCount, count);
            *ppEventCategories = categoriesPtr;
            *ppEventCategoryDescs = descriptionsPtr;
            categoriesPtr = IntPtr.Zero;
            descriptionsPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeCoTaskMem(categoriesPtr);
            OpcAeArrayMarshaler.FreeBstrArray(descriptionsPtr, count);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QueryConditionNames(IntPtr pThis, int eventCategory, IntPtr pCount, IntPtr* ppConditionNames)
    {
        WriteInt32(pCount, 0);
        WriteNull(ppConditionNames);
        if (pCount == IntPtr.Zero || ppConditionNames == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr namesPtr = IntPtr.Zero;
        int count = 0;
        try
        {
#pragma warning disable VSTHRD002
            string[] names = dispatcher!.QueryConditionNamesAsync(eventCategory, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            names ??= [];
            OpcAeArrayMarshaler.AllocateBstrArray(names, out count, out namesPtr);
            WriteInt32(pCount, count);
            *ppConditionNames = namesPtr;
            namesPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeBstrArray(namesPtr, count);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QuerySubConditionNames(IntPtr pThis, IntPtr conditionName, IntPtr pCount, IntPtr* ppSubConditionNames)
    {
        WriteInt32(pCount, 0);
        WriteNull(ppSubConditionNames);
        if (pCount == IntPtr.Zero || ppSubConditionNames == null || conditionName == IntPtr.Zero)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr namesPtr = IntPtr.Zero;
        int count = 0;
        try
        {
            string name = Marshal.PtrToStringUni(conditionName) ?? string.Empty;
#pragma warning disable VSTHRD002
            string[] names = dispatcher!.QuerySubConditionNamesAsync(name, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            names ??= [];
            OpcAeArrayMarshaler.AllocateBstrArray(names, out count, out namesPtr);
            WriteInt32(pCount, count);
            *ppSubConditionNames = namesPtr;
            namesPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeBstrArray(namesPtr, count);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QuerySourceConditions(IntPtr pThis, IntPtr source, IntPtr pCount, IntPtr* ppConditionNames)
    {
        WriteInt32(pCount, 0);
        WriteNull(ppConditionNames);
        if (pCount == IntPtr.Zero || ppConditionNames == null || source == IntPtr.Zero)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr namesPtr = IntPtr.Zero;
        int count = 0;
        try
        {
            string sourceName = Marshal.PtrToStringUni(source) ?? string.Empty;
#pragma warning disable VSTHRD002
            string[] names = dispatcher!.QuerySourceConditionsAsync(sourceName, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            names ??= [];
            OpcAeArrayMarshaler.AllocateBstrArray(names, out count, out namesPtr);
            WriteInt32(pCount, count);
            *ppConditionNames = namesPtr;
            namesPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeBstrArray(namesPtr, count);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int QueryEventAttributes(IntPtr pThis, int eventCategory, IntPtr pCount, IntPtr* ppAttrIds, IntPtr* ppAttrDescs, IntPtr* ppAttrTypes)
    {
        WriteInt32(pCount, 0);
        WriteNull(ppAttrIds);
        WriteNull(ppAttrDescs);
        WriteNull(ppAttrTypes);
        if (pCount == IntPtr.Zero || ppAttrIds == null || ppAttrDescs == null || ppAttrTypes == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr idsPtr = IntPtr.Zero;
        IntPtr descriptionsPtr = IntPtr.Zero;
        IntPtr typesPtr = IntPtr.Zero;
        int count = 0;
        try
        {
#pragma warning disable VSTHRD002
            dispatcher!.QueryEventAttributesAsync(eventCategory, out int[] ids, out string[] descriptions, out ushort[] types, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ids ??= [];
            descriptions ??= [];
            types ??= [];
            EnsureSameLength(ids.Length, descriptions.Length, nameof(descriptions));
            EnsureSameLength(ids.Length, types.Length, nameof(types));
            OpcAeArrayMarshaler.AllocateDwordArray(ids, out count, out idsPtr);
            OpcAeArrayMarshaler.AllocateBstrArray(descriptions, out int descriptionCount, out descriptionsPtr);
            OpcAeArrayMarshaler.AllocateUInt16Array(types, out int typeCount, out typesPtr);
            EnsureSameLength(count, descriptionCount, nameof(descriptions));
            EnsureSameLength(count, typeCount, nameof(types));
            WriteInt32(pCount, count);
            *ppAttrIds = idsPtr;
            *ppAttrDescs = descriptionsPtr;
            *ppAttrTypes = typesPtr;
            idsPtr = IntPtr.Zero;
            descriptionsPtr = IntPtr.Zero;
            typesPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeCoTaskMem(idsPtr);
            OpcAeArrayMarshaler.FreeBstrArray(descriptionsPtr, count);
            OpcAeArrayMarshaler.FreeCoTaskMem(typesPtr);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int TranslateToItemIDs(IntPtr pThis, IntPtr source, int eventCategory, IntPtr conditionName, IntPtr subconditionName, int count, IntPtr assocAttrIds, IntPtr* ppAttrItemIds, IntPtr* ppNodeNames, IntPtr* ppClsids)
    {
        WriteNull(ppAttrItemIds);
        WriteNull(ppNodeNames);
        WriteNull(ppClsids);
        if (source == IntPtr.Zero || conditionName == IntPtr.Zero || subconditionName == IntPtr.Zero || count <= 0 || assocAttrIds == IntPtr.Zero || ppAttrItemIds == null || ppNodeNames == null || ppClsids == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr itemIdsPtr = IntPtr.Zero;
        IntPtr nodeNamesPtr = IntPtr.Zero;
        IntPtr clsidsPtr = IntPtr.Zero;
        try
        {
            string sourceName = Marshal.PtrToStringUni(source) ?? string.Empty;
            string condition = Marshal.PtrToStringUni(conditionName) ?? string.Empty;
            string subcondition = Marshal.PtrToStringUni(subconditionName) ?? string.Empty;
            int[] attributeIds = OpcAeArrayMarshaler.ReadDwordArray(assocAttrIds, count);
#pragma warning disable VSTHRD002
            dispatcher!.TranslateToItemIDsAsync(sourceName, eventCategory, condition, subcondition, attributeIds, out string[] itemIds, out string[] nodeNames, out Guid[] clsids, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            itemIds ??= [];
            nodeNames ??= [];
            clsids ??= [];
            EnsureSameLength(count, itemIds.Length, nameof(itemIds));
            EnsureSameLength(count, nodeNames.Length, nameof(nodeNames));
            EnsureSameLength(count, clsids.Length, nameof(clsids));
            OpcAeArrayMarshaler.AllocateBstrArray(itemIds, out _, out itemIdsPtr);
            OpcAeArrayMarshaler.AllocateBstrArray(nodeNames, out _, out nodeNamesPtr);
            OpcAeArrayMarshaler.AllocateGuidArray(clsids, out _, out clsidsPtr);
            *ppAttrItemIds = itemIdsPtr;
            *ppNodeNames = nodeNamesPtr;
            *ppClsids = clsidsPtr;
            itemIdsPtr = IntPtr.Zero;
            nodeNamesPtr = IntPtr.Zero;
            clsidsPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeBstrArray(itemIdsPtr, count);
            OpcAeArrayMarshaler.FreeBstrArray(nodeNamesPtr, count);
            OpcAeArrayMarshaler.FreeCoTaskMem(clsidsPtr);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetConditionState(IntPtr pThis, IntPtr source, IntPtr conditionName, int eventAttrCount, IntPtr attributeIds, IntPtr* ppConditionState)
    {
        WriteNull(ppConditionState);
        if (source == IntPtr.Zero || conditionName == IntPtr.Zero || eventAttrCount < 0 || (eventAttrCount > 0 && attributeIds == IntPtr.Zero) || ppConditionState == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr statePtr = IntPtr.Zero;
        try
        {
            string sourceName = Marshal.PtrToStringUni(source) ?? string.Empty;
            string condition = Marshal.PtrToStringUni(conditionName) ?? string.Empty;
            int[] ids = OpcAeArrayMarshaler.ReadDwordArray(attributeIds, eventAttrCount);
#pragma warning disable VSTHRD002
            OpcConditionState state = dispatcher!.GetConditionStateAsync(sourceName, condition, ids, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            statePtr = OpcAeArrayMarshaler.AllocateConditionState(state);
            *ppConditionState = statePtr;
            statePtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeCoTaskMem(statePtr);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int EnableConditionByArea(IntPtr pThis, int areaCount, IntPtr areas) =>
        DispatchConditionNameArray(pThis, areaCount, areas, static (dispatcher, names) => dispatcher.EnableConditionByAreaAsync(names, CancellationToken.None));

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int EnableConditionBySource(IntPtr pThis, int sourceCount, IntPtr sources) =>
        DispatchConditionNameArray(pThis, sourceCount, sources, static (dispatcher, names) => dispatcher.EnableConditionBySourceAsync(names, CancellationToken.None));

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int DisableConditionByArea(IntPtr pThis, int areaCount, IntPtr areas) =>
        DispatchConditionNameArray(pThis, areaCount, areas, static (dispatcher, names) => dispatcher.DisableConditionByAreaAsync(names, CancellationToken.None));

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int DisableConditionBySource(IntPtr pThis, int sourceCount, IntPtr sources) =>
        DispatchConditionNameArray(pThis, sourceCount, sources, static (dispatcher, names) => dispatcher.DisableConditionBySourceAsync(names, CancellationToken.None));

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AckCondition(IntPtr pThis, int count, IntPtr acknowledgerId, IntPtr comment, IntPtr sources, IntPtr conditionNames, IntPtr activeTimes, IntPtr cookies, IntPtr* ppErrors)
    {
        WriteNull(ppErrors);
        if (count <= 0 || acknowledgerId == IntPtr.Zero || comment == IntPtr.Zero || sources == IntPtr.Zero || conditionNames == IntPtr.Zero || activeTimes == IntPtr.Zero || cookies == IntPtr.Zero || ppErrors == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr errorsPtr = IntPtr.Zero;
        try
        {
            string acknowledger = Marshal.PtrToStringUni(acknowledgerId) ?? string.Empty;
            string ackComment = Marshal.PtrToStringUni(comment) ?? string.Empty;
            string[] sourceNames = OpcAeArrayMarshaler.ReadBstrArray(sources, count);
            string[] conditionNameValues = OpcAeArrayMarshaler.ReadBstrArray(conditionNames, count);
            long[] activeTimeValues = OpcAeArrayMarshaler.ReadFileTimeArray(activeTimes, count);
            int[] cookieValues = OpcAeArrayMarshaler.ReadDwordArray(cookies, count);
#pragma warning disable VSTHRD002
            int[] errors = dispatcher!.AckConditionAsync(acknowledger, ackComment, activeTimeValues, cookieValues, sourceNames, conditionNameValues, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            errors ??= [];
            EnsureSameLength(count, errors.Length, nameof(errors));
            OpcAeArrayMarshaler.AllocateHResultArray(errors, out _, out errorsPtr);
            *ppErrors = errorsPtr;
            errorsPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeCoTaskMem(errorsPtr);
            return MapHResult(ex);
        }
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetReturnedAttributes(IntPtr pThis, int eventCategory, int count, IntPtr attributeIds)
    {
        if (count < 0 || (count > 0 && attributeIds == IntPtr.Zero))
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        try
        {
            int[] ids = OpcAeArrayMarshaler.ReadDwordArray(attributeIds, count);
#pragma warning disable VSTHRD002
            subscription!.SetReturnedAttributesAsync(eventCategory, ids, CancellationToken.None).GetAwaiter().GetResult();
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
    public static int GetReturnedAttributes(IntPtr pThis, int eventCategory, IntPtr pCount, IntPtr* ppAttributeIds)
    {
        WriteInt32(pCount, 0);
        WriteNull(ppAttributeIds);
        if (pCount == IntPtr.Zero || ppAttributeIds == null)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        IntPtr attributeIdsPtr = IntPtr.Zero;
        try
        {
#pragma warning disable VSTHRD002
            int[] ids = subscription!.GetReturnedAttributesAsync(eventCategory, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            ids ??= [];
            OpcAeArrayMarshaler.AllocateDwordArray(ids, out int count, out attributeIdsPtr);
            WriteInt32(pCount, count);
            *ppAttributeIds = attributeIdsPtr;
            attributeIdsPtr = IntPtr.Zero;
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            OpcAeArrayMarshaler.FreeCoTaskMem(attributeIdsPtr);
            return MapHResult(ex);
        }
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

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int DispatchConditionNameArray(
        IntPtr pThis,
        int count,
        IntPtr names,
        Func<IOpcAeServerDispatcher, string[], Task> dispatchAsync)
    {
        ArgumentNullException.ThrowIfNull(dispatchAsync);
        if (count <= 0 || names == IntPtr.Zero)
        {
            return OpcAeServerCcw.E_INVALIDARG;
        }
        if (!TryResolveDispatcher(pThis, out IOpcAeServerDispatcher? dispatcher))
        {
            return OpcAeServerCcw.E_FAIL;
        }

        try
        {
            string[] nameValues = OpcAeArrayMarshaler.ReadBstrArray(names, count);
#pragma warning disable VSTHRD002
            dispatchAsync(dispatcher!, nameValues).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcAeServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static void EnsureSameLength(int expected, int actual, string arrayName)
    {
        if (actual != expected)
        {
            throw new ArgumentException($"{arrayName} length {actual} must equal {expected}.", arrayName);
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
