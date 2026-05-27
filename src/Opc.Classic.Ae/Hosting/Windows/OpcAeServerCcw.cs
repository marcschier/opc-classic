//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting.Windows;

/// <summary>
/// Windows COM-callable wrapper (CCW) over an <see cref="IOpcAeServer"/>.
/// Exposes separate tearoff vtables for <c>IUnknown</c>,
/// <c>IOPCEventServer</c>, and a legacy direct <c>IOPCEventSubscriptionMgt</c> tearoff.
/// </summary>
/// <remarks>
/// <para>
/// <b>Identity.</b> Each supported interface is a separate native tearoff
/// pointer. <c>QueryInterface(IID_IUnknown)</c> on any tearoff returns the
/// canonical identity pointer. All tearoffs share one <see cref="CcwSession"/>
/// holding the refcount and managed server <see cref="GCHandle"/>.
/// </para>
/// <para>
/// <b>Subscription tearoff.</b> AE clients normally receive
/// <c>IOPCEventSubscriptionMgt</c> from <c>CreateEventSubscription</c>, which
/// returns a dedicated <see cref="OpcAeSubscriptionCcw" />. The legacy direct
/// subscription tearoff remains reachable by direct QI for tests or for managed
/// servers that also implement <see cref="IOPCEventSubscriptionMgt"/>.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcAeServerCcw
{
    internal const int S_OK = 0;
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    // Tearoff pointer -> session. Multiple tearoffs map to the same session.
    internal static readonly ConcurrentDictionary<IntPtr, CcwSession> s_tearoffs = new();

    /// <summary>Builds a CCW around <paramref name="server"/> for the requested IID with refcount = 1.</summary>
    /// <returns>A CCW tearoff pointer, or <see cref="IntPtr.Zero"/> if the IID isn't supported.</returns>
    public static IntPtr Create(IOpcAeServer server, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var serverHandle = GCHandle.Alloc(server, GCHandleType.Normal);
        var dispatcherHandle = GCHandle.Alloc(new OpcAeServerDispatcher(server), GCHandleType.Normal);
        var session = new CcwSession(serverHandle, dispatcherHandle) { RefCount = 1 };

        IntPtr* unknownVtable = AllocateUnknownVtable();
        IntPtr* eventServerVtable = AllocateEventServerVtable();
        IntPtr* subscriptionMgtVtable = AllocateSubscriptionMgtVtable();

        IntPtr unknownTearoff = AllocateTearoff(unknownVtable);
        IntPtr eventServerTearoff = AllocateTearoff(eventServerVtable);
        IntPtr subscriptionMgtTearoff = AllocateTearoff(subscriptionMgtVtable);

        session.UnknownTearoff = unknownTearoff;
        session.UnknownVtable = unknownVtable;
        session.EventServerTearoff = eventServerTearoff;
        session.EventServerVtable = eventServerVtable;
        session.SubscriptionMgtTearoff = subscriptionMgtTearoff;
        session.SubscriptionMgtVtable = subscriptionMgtVtable;

        s_tearoffs[unknownTearoff] = session;
        s_tearoffs[eventServerTearoff] = session;
        s_tearoffs[subscriptionMgtTearoff] = session;

        return ResolveTearoff(session, requestedIid);
    }

    /// <summary>Builds a dispatcher-backed CCW for tests and hosts that already route AE calls.</summary>
    /// <returns>A CCW tearoff pointer, or <see cref="IntPtr.Zero"/> if the IID isn't supported.</returns>
    public static IntPtr Create(IOpcAeServerDispatcher dispatcher, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var dispatcherHandle = GCHandle.Alloc(dispatcher, GCHandleType.Normal);
        var session = new CcwSession(default, dispatcherHandle) { RefCount = 1 };

        IntPtr* unknownVtable = AllocateUnknownVtable();
        IntPtr* eventServerVtable = AllocateEventServerVtable();
        IntPtr* subscriptionMgtVtable = AllocateSubscriptionMgtVtable();

        IntPtr unknownTearoff = AllocateTearoff(unknownVtable);
        IntPtr eventServerTearoff = AllocateTearoff(eventServerVtable);
        IntPtr subscriptionMgtTearoff = AllocateTearoff(subscriptionMgtVtable);

        session.UnknownTearoff = unknownTearoff;
        session.UnknownVtable = unknownVtable;
        session.EventServerTearoff = eventServerTearoff;
        session.EventServerVtable = eventServerVtable;
        session.SubscriptionMgtTearoff = subscriptionMgtTearoff;
        session.SubscriptionMgtVtable = subscriptionMgtVtable;

        s_tearoffs[unknownTearoff] = session;
        s_tearoffs[eventServerTearoff] = session;
        s_tearoffs[subscriptionMgtTearoff] = session;

        return ResolveTearoff(session, requestedIid);
    }

    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown ||
        iid == IOPCEventServer.InterfaceId ||
        iid == IOPCEventSubscriptionMgt.InterfaceId;

    /// <summary>Test helper: returns the current refcount, or -1 if the pointer is not a known tearoff.</summary>
    public static long GetReferenceCount(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? Interlocked.Read(ref session.RefCount)
            : -1L;

    /// <summary>Test helper: looks up the canonical IUnknown tearoff for a CCW pointer.</summary>
    public static IntPtr GetUnknownTearoff(IntPtr anyTearoff) =>
        s_tearoffs.TryGetValue(anyTearoff, out CcwSession? session)
            ? session.UnknownTearoff
            : IntPtr.Zero;

    internal static IOpcAeServer? ResolveServer(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) && session.ServerHandle.IsAllocated
            ? session.ServerHandle.Target as IOpcAeServer
            : null;

    internal static IOpcAeServerDispatcher? ResolveDispatcher(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) && session.DispatcherHandle.IsAllocated
            ? session.DispatcherHandle.Target as IOpcAeServerDispatcher
            : null;

    internal static IOPCEventSubscriptionMgt? ResolveSubscription(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session) && session.ServerHandle.IsAllocated
            ? session.ServerHandle.Target as IOPCEventSubscriptionMgt
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateUnknownVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(3 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateEventServerVtable()
    {
        // 3 IUnknown + 16 IOPCEventServer methods (opnums 3..18) = 19 slots.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(19 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetStatus;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, int, int, Guid*, IntPtr*, IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.CreateEventSubscription;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.QueryAvailableFilters;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, IntPtr*, int>)&OpcAeServerCcwMethods.QueryEventCategories;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.QueryConditionNames;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.QuerySubConditionNames;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.QuerySourceConditions;
        v[10] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, IntPtr*, IntPtr*, int>)&OpcAeServerCcwMethods.QueryEventAttributes;
        v[11] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int, IntPtr, IntPtr, int, IntPtr, IntPtr*, IntPtr*, IntPtr*, int>)&OpcAeServerCcwMethods.TranslateToItemIDs;
        v[12] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetConditionState;
        v[13] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.EnableConditionByArea;
        v[14] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.EnableConditionBySource;
        v[15] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.DisableConditionByArea;
        v[16] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.DisableConditionBySource;
        v[17] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.AckCondition;
        v[18] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&OpcAeServerCcwMethods.CreateAreaBrowser;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateSubscriptionMgtVtable()
    {
        // 3 IUnknown + 8 IOPCEventSubscriptionMgt methods (opnums 3..10) = 11 slots.
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(11 * sizeof(IntPtr)));
        FillUnknownSlots(v);
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, IntPtr, int, int, int, IntPtr, int, IntPtr, int>)&OpcAeServerCcwMethods.SetFilter;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr, IntPtr, IntPtr, IntPtr*, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetFilter;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int, int, IntPtr, int>)&OpcAeServerCcwMethods.SetReturnedAttributes;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, int>)&OpcAeServerCcwMethods.GetReturnedAttributes;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&OpcAeServerCcwMethods.Refresh;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, int, int>)&OpcAeServerCcwMethods.CancelRefresh;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.GetState;
        v[10] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, int, IntPtr, IntPtr, int>)&OpcAeServerCcwMethods.SetState;
        return v;
    }

    private static void FillUnknownSlots(IntPtr* v)
    {
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateTearoff(IntPtr* vtable)
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
        if (riid == null)
        {
            *ppv = IntPtr.Zero;
            return E_INVALIDARG;
        }
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
        }

        IntPtr target = ResolveTearoff(session, *riid);
        if (target == IntPtr.Zero)
        {
            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
        }

        *ppv = target;
        Interlocked.Increment(ref session.RefCount);
        return S_OK;
    }

    private static IntPtr ResolveTearoff(CcwSession session, Guid iid)
    {
        if (iid == IID_IUnknown)
        {
            return session.UnknownTearoff;
        }
        if (iid == IOPCEventServer.InterfaceId)
        {
            return session.EventServerTearoff;
        }
        if (iid == IOPCEventSubscriptionMgt.InterfaceId)
        {
            return session.SubscriptionMgtTearoff;
        }
        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis)
    {
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return 1;
        }
        return (uint)Interlocked.Increment(ref session.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        if (!s_tearoffs.TryGetValue(pThis, out CcwSession? session))
        {
            return 0;
        }
        long next = Interlocked.Decrement(ref session.RefCount);
        if (next > 0)
        {
            return (uint)next;
        }
        DisposeSession(session);
        return 0;
    }

    private static void DisposeSession(CcwSession session)
    {
        if (Interlocked.Exchange(ref session.Disposed, 1) != 0)
        {
            return;
        }
        s_tearoffs.TryRemove(session.UnknownTearoff, out _);
        s_tearoffs.TryRemove(session.EventServerTearoff, out _);
        s_tearoffs.TryRemove(session.SubscriptionMgtTearoff, out _);
        FreeTearoffs(session);
        FreeVtables(session);
        if (session.ServerHandle.IsAllocated)
        {
            session.ServerHandle.Free();
        }
        if (session.DispatcherHandle.IsAllocated)
        {
            session.DispatcherHandle.Free();
        }
    }

    private static void FreeTearoffs(CcwSession session)
    {
        if (session.UnknownTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.UnknownTearoff);
        }
        if (session.EventServerTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.EventServerTearoff);
        }
        if (session.SubscriptionMgtTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.SubscriptionMgtTearoff);
        }
    }

    private static void FreeVtables(CcwSession session)
    {
        if (session.UnknownVtable != null)
        {
            NativeMemory.Free(session.UnknownVtable);
        }
        if (session.EventServerVtable != null)
        {
            NativeMemory.Free(session.EventServerVtable);
        }
        if (session.SubscriptionMgtVtable != null)
        {
            NativeMemory.Free(session.SubscriptionMgtVtable);
        }
    }

    /// <summary>Shared state across all tearoffs of one CCW.</summary>
    internal sealed class CcwSession
    {
        public CcwSession(GCHandle serverHandle, GCHandle dispatcherHandle)
        {
            ServerHandle = serverHandle;
            DispatcherHandle = dispatcherHandle;
        }

        public GCHandle ServerHandle { get; }
        public GCHandle DispatcherHandle { get; }

        public long RefCount;
        public int Disposed;
        public IntPtr UnknownTearoff;
        public IntPtr* UnknownVtable;
        public IntPtr EventServerTearoff;
        public IntPtr* EventServerVtable;
        public IntPtr SubscriptionMgtTearoff;
        public IntPtr* SubscriptionMgtVtable;
    }
}
