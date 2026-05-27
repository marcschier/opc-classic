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
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Windows COM-callable wrapper (CCW) over an <see cref="OpcDaGroup"/>
/// instance. Exposes <c>IUnknown</c>, <c>IOPCGroupStateMgt</c>,
/// <c>IOPCGroupStateMgt2</c>, and <c>IOPCItemMgt</c> vtables allocated in
/// native memory so SCM-activated DCOM clients can invoke real group
/// operations after <c>IOPCServer::AddGroup</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Identity.</b> Each supported interface is a separate native "tearoff"
/// pointer. <c>QueryInterface(IID_IUnknown)</c> on any tearoff returns the
/// canonical IUnknown identity pointer (rule MS-DCOM §3.2.6). All tearoffs
/// share a single <see cref="CcwSession"/> holding the refcount and the
/// <see cref="GCHandle"/> on the managed group.
/// </para>
/// <para>
/// <b>Method coverage.</b> The simple-marshaling methods on
/// IOPCGroupStateMgt(2) + IOPCItemMgt are wired to real bodies on the
/// underlying managed group. The complex-marshaling methods (CloneGroup,
/// AddItems, ValidateItems, CreateEnumerator — all involving OPCITEMDEF or
/// IUnknown-returning out-params) currently return E_NOTIMPL pending full
/// COM marshaling. See <see cref="OpcDaGroupCcwMethods"/> for the wired
/// bodies.
/// </para>
/// <para>
/// <b>Lifetime.</b> When the last <c>Release</c> drops the shared refcount to
/// zero, all tearoff instances, vtables, and the GCHandle are freed. The
/// CCW is fully self-contained — no leak-at-exit.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcDaGroupCcw
{
    internal const int S_OK = 0;
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    // Tearoff pointer -> session. Multiple tearoffs map to the same session.
    internal static readonly ConcurrentDictionary<IntPtr, CcwSession> s_tearoffs = new();

    /// <summary>Builds a CCW around <paramref name="group"/> and returns the IUnknown identity pointer with refcount = 1.</summary>
    public static IntPtr Create(OpcDaGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);

        var handle = GCHandle.Alloc(group, GCHandleType.Normal);
        var session = new CcwSession(handle) { RefCount = 1 };

        IntPtr* unknownVtable = AllocateUnknownVtable();
        IntPtr* gsmVtable = AllocateGroupStateMgt2Vtable();
        IntPtr* itemMgtVtable = AllocateItemMgtVtable();

        IntPtr unknownTearoff = AllocateTearoff(unknownVtable);
        IntPtr gsmTearoff = AllocateTearoff(gsmVtable);
        IntPtr itemMgtTearoff = AllocateTearoff(itemMgtVtable);

        session.UnknownTearoff = unknownTearoff;
        session.UnknownVtable = unknownVtable;
        session.GroupStateMgtTearoff = gsmTearoff;
        session.GroupStateMgtVtable = gsmVtable;
        session.ItemMgtTearoff = itemMgtTearoff;
        session.ItemMgtVtable = itemMgtVtable;

        s_tearoffs[unknownTearoff] = session;
        s_tearoffs[gsmTearoff] = session;
        s_tearoffs[itemMgtTearoff] = session;

        return unknownTearoff;
    }

    /// <summary>Test helper: returns the current refcount, or -1 if the pointer is not a known tearoff.</summary>
    public static long GetReferenceCount(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? Interlocked.Read(ref session.RefCount)
            : -1L;

    /// <summary>Test helper: looks up the canonical IUnknown tearoff for a CCW pointer (any tearoff of the same session).</summary>
    public static IntPtr GetUnknownTearoff(IntPtr anyTearoff) =>
        s_tearoffs.TryGetValue(anyTearoff, out CcwSession? session)
            ? session.UnknownTearoff
            : IntPtr.Zero;

    internal static OpcDaGroup? ResolveGroup(IntPtr tearoff) =>
        s_tearoffs.TryGetValue(tearoff, out CcwSession? session)
            ? session.GroupHandle.Target as OpcDaGroup
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateUnknownVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(3 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateGroupStateMgt2Vtable()
    {
        // 3 IUnknown + 4 IOPCGroupStateMgt + 2 IOPCGroupStateMgt2 = 9 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(9 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.GetState;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.SetState;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.SetName;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)&OpcDaGroupCcwMethods.CloneGroup;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&OpcDaGroupCcwMethods.SetKeepAlive;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&OpcDaGroupCcwMethods.GetKeepAlive;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateItemMgtVtable()
    {
        // 3 IUnknown + 7 IOPCItemMgt = 10 slots
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(10 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)&OpcDaGroupCcwMethods.AddItems;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, int, IntPtr*, IntPtr*, int>)&OpcDaGroupCcwMethods.ValidateItems;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, int>)&OpcDaGroupCcwMethods.RemoveItems;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, int, IntPtr*, int>)&OpcDaGroupCcwMethods.SetActiveState;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcDaGroupCcwMethods.SetClientHandles;
        v[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&OpcDaGroupCcwMethods.SetDatatypes;
        v[9] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&OpcDaGroupCcwMethods.CreateEnumerator;
        return v;
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
        if (iid == IOPCGroupStateMgt.InterfaceId || iid == IOPCGroupStateMgt2.InterfaceId)
        {
            return session.GroupStateMgtTearoff;
        }
        if (iid == IOPCItemMgt.InterfaceId)
        {
            return session.ItemMgtTearoff;
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
        s_tearoffs.TryRemove(session.GroupStateMgtTearoff, out _);
        s_tearoffs.TryRemove(session.ItemMgtTearoff, out _);

        if (session.UnknownTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.UnknownTearoff);
        }
        if (session.GroupStateMgtTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.GroupStateMgtTearoff);
        }
        if (session.ItemMgtTearoff != IntPtr.Zero)
        {
            NativeMemory.Free((void*)session.ItemMgtTearoff);
        }
        if (session.UnknownVtable != null)
        {
            NativeMemory.Free(session.UnknownVtable);
        }
        if (session.GroupStateMgtVtable != null)
        {
            NativeMemory.Free(session.GroupStateMgtVtable);
        }
        if (session.ItemMgtVtable != null)
        {
            NativeMemory.Free(session.ItemMgtVtable);
        }
        if (session.GroupHandle.IsAllocated)
        {
            session.GroupHandle.Free();
        }
    }

    /// <summary>Shared state across all tearoffs of one CCW.</summary>
    internal sealed class CcwSession
    {
        public CcwSession(GCHandle groupHandle)
        {
            GroupHandle = groupHandle;
        }

        public GCHandle GroupHandle { get; }

        public long RefCount;
        public int Disposed;
        public IntPtr UnknownTearoff;
        public IntPtr* UnknownVtable;
        public IntPtr GroupStateMgtTearoff;
        public IntPtr* GroupStateMgtVtable;
        public IntPtr ItemMgtTearoff;
        public IntPtr* ItemMgtVtable;
    }
}
