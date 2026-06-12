//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting.Windows;

/// <summary>
/// Windows CCW for <see cref="IOPCEventSubscriptionMgt" /> and its event-sink connection point.
/// </summary>
[SupportedOSPlatform("windows")]
public static unsafe class OpcAeSubscriptionCcw
{
    internal const int S_OK = 0;
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);
    internal const int CONNECT_E_NOCONNECTION = unchecked((int)0x80040200);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_entries = new();

    /// <summary>
    /// Creates an <c>IOPCEventSubscriptionMgt</c> CCW with refcount = 1.
    /// </summary>
    public static IntPtr Create(IOPCEventSubscriptionMgt subscription, Guid requestedIid) =>
        Create(subscription, requestedIid, ownerDispatcher: null);

    internal static IntPtr Create(IOPCEventSubscriptionMgt subscription, Guid requestedIid, IOpcAeServerDispatcher? ownerDispatcher)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var subscriptionHandle = GCHandle.Alloc(subscription, GCHandleType.Normal);
        GCHandle dispatcherHandle = ownerDispatcher is null ? default : GCHandle.Alloc(ownerDispatcher, GCHandleType.Normal);
        var entry = new CcwEntry(subscriptionHandle, dispatcherHandle) { RefCount = 1 };
        AllocateEntryTearoffs(entry);
        RegisterEntryTearoffs(entry);
        return ResolveTearoff(entry, requestedIid);
    }

    /// <summary>
    /// Returns whether this CCW supports <paramref name="iid" />.
    /// </summary>
    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown ||
        iid == IOPCEventSubscriptionMgt.InterfaceId ||
        iid == OpcGuids.IID_IConnectionPoint ||
        iid == OpcGuids.IID_IConnectionPointContainer;

    /// <summary>
    /// Test helper: returns the current refcount, or -1 if unknown.
    /// </summary>
    public static long GetReferenceCount(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? Interlocked.Read(ref entry.RefCount)
            : -1L;

    /// <summary>
    /// Test helper: returns the number of advised Windows callback sinks.
    /// </summary>
    public static int GetScmSinkCount(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? entry.ScmSinks.Count
            : -1;

    internal static IOPCEventSubscriptionMgt? ResolveSubscription(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? entry.SubscriptionHandle.Target as IOPCEventSubscriptionMgt
            : null;

    private static void AllocateEntryTearoffs(CcwEntry entry)
    {
        entry.SubscriptionMgtVtable = AllocateSubscriptionMgtVtable();
        entry.ConnectionPointVtable = AllocateConnectionPointVtable();
        entry.ConnectionPointContainerVtable = AllocateConnectionPointContainerVtable();
        entry.SubscriptionMgtTearoff = AllocateInstance(entry.SubscriptionMgtVtable);
        entry.ConnectionPointTearoff = AllocateInstance(entry.ConnectionPointVtable);
        entry.ConnectionPointContainerTearoff = AllocateInstance(entry.ConnectionPointContainerVtable);
    }

    private static void RegisterEntryTearoffs(CcwEntry entry)
    {
        s_entries[entry.SubscriptionMgtTearoff] = entry;
        s_entries[entry.ConnectionPointTearoff] = entry;
        s_entries[entry.ConnectionPointContainerTearoff] = entry;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSubscriptionMgtVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(11 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, IntPtr, int, int, int, IntPtr, int, IntPtr, int>)&SetFilter;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr, IntPtr*, int>)&GetFilter;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, IntPtr, int>)&SelectReturnedAttributes;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, int>)&GetReturnedAttributes;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&Refresh;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&CancelRefresh;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&GetState;
        v[10] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr, int>)&SetState;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(8 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, int>)&GetConnectionInterface;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&GetConnectionPointContainer;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, uint*, int>)&Advise;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&Unadvise;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&EnumConnections;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateConnectionPointContainerVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(5 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&EnumConnectionPoints;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&FindConnectionPoint;
        return v;
    }

    private static void FillUnknownSlots(IntPtr* v)
    {
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInstance(IntPtr* vtable)
    {
        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;
        return (IntPtr)instance;
    }

    [UnmanagedCallersOnly]
    private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
    {
        if (ppv == null)
        {
            return E_INVALIDARG;
        }
        *ppv = IntPtr.Zero;
        if (riid == null)
        {
            return E_INVALIDARG;
        }
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOINTERFACE;
        }

        IntPtr target = ResolveTearoff(entry, *riid);
        if (target == IntPtr.Zero)
        {
            return E_NOINTERFACE;
        }

        *ppv = target;
        Interlocked.Increment(ref entry.RefCount);
        return S_OK;
    }

    private static IntPtr ResolveTearoff(CcwEntry entry, Guid iid)
    {
        if (iid == IID_IUnknown || iid == IOPCEventSubscriptionMgt.InterfaceId)
        {
            return entry.SubscriptionMgtTearoff;
        }
        if (iid == OpcGuids.IID_IConnectionPoint)
        {
            return entry.ConnectionPointTearoff;
        }
        if (iid == OpcGuids.IID_IConnectionPointContainer)
        {
            return entry.ConnectionPointContainerTearoff;
        }
        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis)
    {
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry))
        {
            return 1;
        }
        return (uint)Interlocked.Increment(ref entry.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry))
        {
            return 0;
        }
        long next = Interlocked.Decrement(ref entry.RefCount);
        if (next > 0)
        {
            return (uint)next;
        }
        DisposeEntry(entry);
        return 0;
    }

    [UnmanagedCallersOnly]
    private static int GetConnectionInterface(IntPtr pThis, Guid* piid)
    {
        _ = pThis;
        if (piid == null)
        {
            return E_INVALIDARG;
        }
        *piid = IOPCEventSink.InterfaceId;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int GetConnectionPointContainer(IntPtr pThis, IntPtr* ppCpc)
    {
        WriteNull(ppCpc);
        if (ppCpc == null)
        {
            return E_INVALIDARG;
        }
        return s_entries.TryGetValue(pThis, out CcwEntry? entry)
            ? ReturnTearoff(entry, entry.ConnectionPointContainerTearoff, ppCpc)
            : E_FAIL;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Advise(IntPtr pThis, IntPtr pUnk, uint* pdwCookie)
    {
        if (pdwCookie != null)
        {
            *pdwCookie = 0;
        }
        if (pdwCookie == null || pUnk == IntPtr.Zero)
        {
            return E_INVALIDARG;
        }
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_FAIL;
        }
        return AdviseCore(entry, pUnk, pdwCookie);
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Unadvise(IntPtr pThis, uint dwCookie)
    {
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_FAIL;
        }
        int cookie = unchecked((int)dwCookie);
        if (!entry.ScmSinks.TryRemove(cookie, out ScmSinkEntry? sinkEntry))
        {
            return CONNECT_E_NOCONNECTION;
        }
        try
        {
            if (sinkEntry.RegistrationTarget != SinkRegistrationTarget.Local)
            {
                UnregisterSink(entry, cookie, sinkEntry.RegistrationTarget);
            }
            sinkEntry.Proxy.Dispose();
            return S_OK;
        }
        catch (Exception ex)
        {
            sinkEntry.Proxy.Dispose();
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    private static int EnumConnections(IntPtr pThis, IntPtr* ppEnum)
    {
        _ = pThis;
        WriteNull(ppEnum);
        return ppEnum == null ? E_INVALIDARG : E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int EnumConnectionPoints(IntPtr pThis, IntPtr* ppEnum)
    {
        _ = pThis;
        WriteNull(ppEnum);
        return ppEnum == null ? E_INVALIDARG : E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int FindConnectionPoint(IntPtr pThis, Guid* riid, IntPtr* ppCp)
    {
        WriteNull(ppCp);
        if (riid == null || ppCp == null)
        {
            return E_INVALIDARG;
        }
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_FAIL;
        }
        return *riid == IOPCEventSink.InterfaceId
            ? ReturnTearoff(entry, entry.ConnectionPointTearoff, ppCp)
            : E_NOINTERFACE;
    }

    private static int ReturnTearoff(CcwEntry entry, IntPtr tearoff, IntPtr* ppv)
    {
        if (ppv == null || tearoff == IntPtr.Zero)
        {
            return E_INVALIDARG;
        }
        *ppv = tearoff;
        Interlocked.Increment(ref entry.RefCount);
        return S_OK;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int AdviseCore(CcwEntry entry, IntPtr pUnk, uint* pdwCookie)
    {
        OpcAeEventSinkProxy? proxy = null;
        try
        {
            proxy = new OpcAeEventSinkProxy(pUnk);
            SinkRegistrationTarget registrationTarget = RegisterSink(entry, proxy, out int cookie);
            if (!entry.ScmSinks.TryAdd(cookie, new ScmSinkEntry(proxy, registrationTarget)))
            {
                if (registrationTarget != SinkRegistrationTarget.Local)
                {
                    UnregisterSink(entry, cookie, registrationTarget);
                }
                proxy.Dispose();
                return E_FAIL;
            }
            proxy = null;
            *pdwCookie = unchecked((uint)cookie);
            return S_OK;
        }
        catch (Exception ex)
        {
            proxy?.Dispose();
            return MapHResult(ex);
        }
    }

    private static SinkRegistrationTarget RegisterSink(CcwEntry entry, IOPCEventSink proxy, out int cookie)
    {
        IOPCEventSubscriptionMgt? subscription = entry.SubscriptionHandle.Target as IOPCEventSubscriptionMgt;
        IOpcAeServerDispatcher? dispatcher = entry.DispatcherHandle.IsAllocated
            ? entry.DispatcherHandle.Target as IOpcAeServerDispatcher
            : null;
        if (dispatcher is not null && subscription is not null)
        {
            try
            {
#pragma warning disable VSTHRD002
                cookie = dispatcher.AdviseEventSinkAsync(subscription, proxy, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
                return SinkRegistrationTarget.Dispatcher;
            }
            catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
            {
            }
        }
        if (subscription is IOpcAeEventSinkRegistration registration)
        {
#pragma warning disable VSTHRD002
            cookie = registration.AdviseEventSinkAsync(proxy, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return SinkRegistrationTarget.Subscription;
        }
        cookie = Interlocked.Increment(ref entry.NextScmSinkCookie);
        return SinkRegistrationTarget.Local;
    }

    private static void UnregisterSink(CcwEntry entry, int cookie, SinkRegistrationTarget registrationTarget)
    {
        IOPCEventSubscriptionMgt? subscription = entry.SubscriptionHandle.Target as IOPCEventSubscriptionMgt;
        if (registrationTarget == SinkRegistrationTarget.Dispatcher && subscription is not null && entry.DispatcherHandle.IsAllocated && entry.DispatcherHandle.Target is IOpcAeServerDispatcher dispatcher)
        {
#pragma warning disable VSTHRD002
            dispatcher.UnadviseEventSinkAsync(subscription, cookie, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return;
        }
        if (registrationTarget == SinkRegistrationTarget.Subscription && subscription is IOpcAeEventSinkRegistration registration)
        {
#pragma warning disable VSTHRD002
            registration.UnadviseEventSinkAsync(cookie, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int SetFilter(IntPtr pThis, int dwEventType, int dwNumCategories, IntPtr pdwEventCategories, int dwLowSeverity, int dwHighSeverity, int dwNumAreas, IntPtr pszAreaList, int dwNumSources, IntPtr pszSourceList)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
        }
        try
        {
            int[] categories = ReadInt32Array(dwNumCategories, pdwEventCategories);
            string[] areas = ReadStringPointerArray(dwNumAreas, pszAreaList);
            string[] sources = ReadStringPointerArray(dwNumSources, pszSourceList);
#pragma warning disable VSTHRD002
            subscription!.SetFilterAsync(dwEventType, categories, dwLowSeverity, dwHighSeverity, areas, sources, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int GetFilter(IntPtr pThis, IntPtr pEventType, IntPtr pCategoryCount, IntPtr* ppEventCategories, IntPtr pLowSeverity, IntPtr pHighSeverity, IntPtr pAreaCount, IntPtr* ppAreaList, IntPtr pSourceCount, IntPtr* ppSourceList)
    {
        WriteNull(ppEventCategories);
        WriteNull(ppAreaList);
        WriteNull(ppSourceList);
        if (pEventType == IntPtr.Zero || pCategoryCount == IntPtr.Zero || ppEventCategories == null || pLowSeverity == IntPtr.Zero || pHighSeverity == IntPtr.Zero || pAreaCount == IntPtr.Zero || ppAreaList == null || pSourceCount == IntPtr.Zero || ppSourceList == null)
        {
            return E_INVALIDARG;
        }
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
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
            categoriesPtr = AllocateInt32Array(categories);
            areasPtr = AllocateStringPointerArray(areas);
            sourcesPtr = AllocateStringPointerArray(sources);

            WriteInt32(pEventType, eventType);
            WriteInt32(pCategoryCount, categories.Length);
            *ppEventCategories = categoriesPtr;
            WriteInt32(pLowSeverity, lowSeverity);
            WriteInt32(pHighSeverity, highSeverity);
            WriteInt32(pAreaCount, areas.Length);
            *ppAreaList = areasPtr;
            WriteInt32(pSourceCount, sources.Length);
            *ppSourceList = sourcesPtr;
            categoriesPtr = IntPtr.Zero;
            areasPtr = IntPtr.Zero;
            sourcesPtr = IntPtr.Zero;
            return S_OK;
        }
        catch (Exception ex)
        {
            FreeCoTaskMem(categoriesPtr);
            FreeStringPointerArray(areasPtr);
            FreeStringPointerArray(sourcesPtr);
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int SelectReturnedAttributes(IntPtr pThis, int dwEventCategory, int dwCount, IntPtr dwAttributeIDs)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
        }
        try
        {
            int[] attributeIds = ReadInt32Array(dwCount, dwAttributeIDs);
#pragma warning disable VSTHRD002
            subscription!.SetReturnedAttributesAsync(dwEventCategory, attributeIds, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int GetReturnedAttributes(IntPtr pThis, int dwEventCategory, IntPtr pCount, IntPtr* ppAttributeIDs)
    {
        WriteNull(ppAttributeIDs);
        if (pCount == IntPtr.Zero || ppAttributeIDs == null)
        {
            return E_INVALIDARG;
        }
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            int[] attributeIds = subscription!.GetReturnedAttributesAsync(dwEventCategory, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            attributeIds ??= Array.Empty<int>();
            *ppAttributeIDs = AllocateInt32Array(attributeIds);
            WriteInt32(pCount, attributeIds.Length);
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Refresh(IntPtr pThis, int dwConnection)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            subscription!.RefreshAsync(dwConnection, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int CancelRefresh(IntPtr pThis, int dwConnection)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            subscription!.CancelRefreshAsync(dwConnection, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int GetState(IntPtr pThis, IntPtr pbActive, IntPtr pdwBufferTime, IntPtr pdwMaxSize, IntPtr phClientSubscription)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            subscription!.GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pbActive, active ? 1 : 0);
            WriteInt32(pdwBufferTime, bufferTime);
            WriteInt32(pdwMaxSize, maxSize);
            WriteInt32(phClientSubscription, clientSubscription);
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int SetState(IntPtr pThis, IntPtr pbActive, IntPtr pdwBufferTime, IntPtr pdwMaxSize, int hClientSubscription, IntPtr pdwRevisedBufferTime, IntPtr pdwRevisedMaxSize)
    {
        if (!TryResolveSubscription(pThis, out IOPCEventSubscriptionMgt? subscription))
        {
            return E_FAIL;
        }
        try
        {
            ReadCurrentState(subscription!, out bool active, out int bufferTime, out int maxSize);
            ApplyRequestedState(pbActive, pdwBufferTime, pdwMaxSize, ref active, ref bufferTime, ref maxSize);
#pragma warning disable VSTHRD002
            subscription!.SetStateAsync(active, bufferTime, maxSize, hClientSubscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteInt32(pdwRevisedBufferTime, revisedBufferTime);
            WriteInt32(pdwRevisedMaxSize, revisedMaxSize);
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolveSubscription(IntPtr pThis, out IOPCEventSubscriptionMgt? subscription)
    {
        subscription = ResolveSubscription(pThis);
        return subscription is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        OpcException opcEx => opcEx.ResultId.Code,
        NotImplementedException => E_NOTIMPL,
        ArgumentNullException => E_INVALIDARG,
        ArgumentOutOfRangeException => E_INVALIDARG,
        ArgumentException => E_INVALIDARG,
        ObjectDisposedException => E_FAIL,
        _ => E_FAIL,
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

    internal static int[] ReadInt32Array(int count, IntPtr values)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
        {
            return Array.Empty<int>();
        }
        if (values == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var result = new int[count];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Marshal.ReadInt32(values, i * sizeof(int));
        }
        return result;
    }

    internal static string[] ReadStringPointerArray(int count, IntPtr values)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
        {
            return Array.Empty<string>();
        }
        if (values == IntPtr.Zero)
        {
            throw new ArgumentNullException(nameof(values));
        }

        var result = new string[count];
        for (int i = 0; i < result.Length; i++)
        {
            IntPtr valuePtr = Marshal.ReadIntPtr(values, i * IntPtr.Size);
            result[i] = valuePtr == IntPtr.Zero ? string.Empty : Marshal.PtrToStringBSTR(valuePtr) ?? string.Empty;
        }
        return result;
    }

    internal static IntPtr AllocateInt32Array(int[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
        for (int i = 0; i < values.Length; i++)
        {
            Marshal.WriteInt32(ptr, i * sizeof(int), values[i]);
        }
        return ptr;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cleanup and rethrow after partial native allocation.")]
    internal static IntPtr AllocateStringPointerArray(string[] values)
    {
        if (values.Length == 0)
        {
            return IntPtr.Zero;
        }

        IntPtr arrayPtr = Marshal.AllocCoTaskMem((values.Length + 1) * IntPtr.Size);
        for (int i = 0; i <= values.Length; i++)
        {
            Marshal.WriteIntPtr(arrayPtr, i * IntPtr.Size, IntPtr.Zero);
        }

        try
        {
            for (int i = 0; i < values.Length; i++)
            {
                IntPtr valuePtr = Marshal.StringToBSTR(values[i] ?? string.Empty);
                Marshal.WriteIntPtr(arrayPtr, i * IntPtr.Size, valuePtr);
            }
            return arrayPtr;
        }
        catch
        {
            FreeStringPointerArray(arrayPtr);
            throw;
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

    internal static void FreeCoTaskMem(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            Marshal.FreeCoTaskMem(ptr);
        }
    }

    internal static void FreeStringPointerArray(IntPtr arrayPtr)
    {
        if (arrayPtr == IntPtr.Zero)
        {
            return;
        }

        int offset = 0;
        while (true)
        {
            IntPtr valuePtr = Marshal.ReadIntPtr(arrayPtr, offset);
            if (valuePtr == IntPtr.Zero)
            {
                break;
            }
            Marshal.FreeBSTR(valuePtr);
            offset += IntPtr.Size;
        }
        Marshal.FreeCoTaskMem(arrayPtr);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Release must free native state even if managed cleanup fails.")]
    private static void DisposeEntry(CcwEntry entry)
    {
        if (Interlocked.Exchange(ref entry.Disposed, 1) != 0)
        {
            return;
        }

        try
        {
            RemoveOrDisposeSubscription(entry);
        }
        catch
        {
        }

        DisposeScmSinks(entry);
        RemoveEntryTearoffs(entry);
        FreeEntryTearoffs(entry);
        FreeEntryVtables(entry);
        if (entry.SubscriptionHandle.IsAllocated)
        {
            entry.SubscriptionHandle.Free();
        }
        if (entry.DispatcherHandle.IsAllocated)
        {
            entry.DispatcherHandle.Free();
        }
    }

    private static void DisposeScmSinks(CcwEntry entry)
    {
        foreach (ScmSinkEntry sink in entry.ScmSinks.Values)
        {
            sink.Proxy.Dispose();
        }
        entry.ScmSinks.Clear();
    }

    private static void RemoveEntryTearoffs(CcwEntry entry)
    {
        s_entries.TryRemove(entry.SubscriptionMgtTearoff, out _);
        s_entries.TryRemove(entry.ConnectionPointTearoff, out _);
        s_entries.TryRemove(entry.ConnectionPointContainerTearoff, out _);
    }

    private static void FreeEntryTearoffs(CcwEntry entry)
    {
        FreeNative(entry.SubscriptionMgtTearoff);
        FreeNative(entry.ConnectionPointTearoff);
        FreeNative(entry.ConnectionPointContainerTearoff);
    }

    private static void FreeEntryVtables(CcwEntry entry)
    {
        FreeNative(entry.SubscriptionMgtVtable);
        FreeNative(entry.ConnectionPointVtable);
        FreeNative(entry.ConnectionPointContainerVtable);
    }

    private static void FreeNative(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            NativeMemory.Free((void*)ptr);
        }
    }

    private static void FreeNative(IntPtr* ptr)
    {
        if (ptr != null)
        {
            NativeMemory.Free(ptr);
        }
    }

    private static void RemoveOrDisposeSubscription(CcwEntry entry)
    {
        if (entry.SubscriptionHandle.Target is not IOPCEventSubscriptionMgt subscription)
        {
            return;
        }

        IOpcAeServerDispatcher? dispatcher = entry.DispatcherHandle.IsAllocated
            ? entry.DispatcherHandle.Target as IOpcAeServerDispatcher
            : null;
        if (dispatcher is not null)
        {
#pragma warning disable VSTHRD002
            dispatcher.RemoveSubscriptionAsync(subscription, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return;
        }

        if (subscription is IAsyncDisposable asyncDisposable)
        {
#pragma warning disable VSTHRD002
            asyncDisposable.DisposeAsync().AsTask().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        }
        else if (subscription is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle subscriptionHandle, GCHandle dispatcherHandle)
        {
            SubscriptionHandle = subscriptionHandle;
            DispatcherHandle = dispatcherHandle;
        }

        public GCHandle SubscriptionHandle { get; }
        public GCHandle DispatcherHandle { get; }
        public long RefCount;
        public int Disposed;
        public IntPtr SubscriptionMgtTearoff;
        public IntPtr* SubscriptionMgtVtable;
        public IntPtr ConnectionPointTearoff;
        public IntPtr* ConnectionPointVtable;
        public IntPtr ConnectionPointContainerTearoff;
        public IntPtr* ConnectionPointContainerVtable;
        public int NextScmSinkCookie;
        public ConcurrentDictionary<int, ScmSinkEntry> ScmSinks { get; } = new();
    }

    private sealed class ScmSinkEntry
    {
        public ScmSinkEntry(OpcAeEventSinkProxy proxy, SinkRegistrationTarget registrationTarget)
        {
            Proxy = proxy;
            RegistrationTarget = registrationTarget;
        }

        public OpcAeEventSinkProxy Proxy { get; }
        public SinkRegistrationTarget RegistrationTarget { get; }
    }

    private enum SinkRegistrationTarget
    {
        Local,
        Dispatcher,
        Subscription,
    }
}
