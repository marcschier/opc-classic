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

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Minimal IUnknown-only CCW for an <see cref="OpcDaGroup"/> instance.
/// Used as the interface pointer returned by
/// <c>OpcDaServerCcw.AddGroup</c>/<c>GetGroupByName</c>/<c>CreateGroupEnumerator</c>
/// so the COM contract is satisfied (the client receives a valid CCW)
/// even though full per-interface vtables (<c>IOPCGroupStateMgt</c>,
/// <c>IOPCItemMgt</c>, <c>IOPCSyncIO</c>, etc.) are deferred.
/// </summary>
/// <remarks>
/// <para>
/// Once SCM marshals the CCW back to the client process, the client's
/// proxy/stub layer (opcproxy.dll) will <c>QueryInterface</c> for the
/// requested OPC interface; this CCW currently returns
/// <c>E_NOINTERFACE</c> for everything except <c>IID_IUnknown</c>, so the
/// client sees a clean failure mode rather than a memory-corruption
/// crash from a bogus vtable.
/// </para>
/// <para>
/// The cross-platform DCOM transport path
/// (<see cref="Opc.Classic.Dcom.Transport.OpcServerListener"/>) reaches
/// the group's full interface set via the per-IPID
/// <see cref="Opc.Classic.Dcom.Transport.OpcObjectRegistry"/> entries
/// registered at <c>AddGroup</c> time. The Windows-CCW path adds
/// IUnknown identity for the SCM-activated scenario.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcDaGroupCcw
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_INVALIDARG = unchecked((int)0x80070057);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_ccws = new();

    /// <summary>Builds an IUnknown-only CCW around <paramref name="group"/>.</summary>
    /// <returns>The CCW pointer with ref count = 1.</returns>
    public static IntPtr Create(OpcDaGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        var handle = GCHandle.Alloc(group, GCHandleType.Normal);
        s_ccws[instance] = new CcwEntry(handle) { RefCount = 1 };
        return instance;
    }

    /// <summary>Test helper: returns the current reference count, or -1 if the pointer is not a known CCW.</summary>
    public static long GetReferenceCount(IntPtr ccw) =>
        s_ccws.TryGetValue(ccw, out CcwEntry? entry) ? Interlocked.Read(ref entry.RefCount) : -1L;

    [SuppressMessage(
        "Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating IntPtr-sized native vtable with explicit byte count.")]
    private static IntPtr* AllocateVtable()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(3 * sizeof(IntPtr)));
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        return vtable;
    }

    [SuppressMessage(
        "Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating IntPtr-sized CCW header with explicit byte count.")]
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
        if (riid == null)
        {
            *ppv = IntPtr.Zero;
            return E_INVALIDARG;
        }

        if (*riid == IID_IUnknown)
        {
            *ppv = pThis;
            if (s_ccws.TryGetValue(pThis, out CcwEntry? entry))
            {
                Interlocked.Increment(ref entry.RefCount);
            }
            return S_OK;
        }

        *ppv = IntPtr.Zero;
        return E_NOINTERFACE;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis)
    {
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return 1;
        }
        return (uint)Interlocked.Increment(ref entry.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return 0;
        }
        long next = Interlocked.Decrement(ref entry.RefCount);
        return next < 0 ? 0 : (uint)next;
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle groupHandle)
        {
            GroupHandle = groupHandle;
        }

        public GCHandle GroupHandle { get; }

        public long RefCount;
    }
}
