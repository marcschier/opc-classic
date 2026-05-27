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
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Minimal Windows COM-callable wrapper (CCW) over an
/// <see cref="IOpcHdaServer"/>. Exposes <c>IUnknown</c> + supports QI for
/// <c>IID_IOPCHDA_Server</c> (returns the same CCW pointer).
/// </summary>
/// <remarks>
/// This is parity infrastructure for SCM activation. Per-method dispatch
/// for IOPCHDA_Server is provided by the cross-platform DCOM transport
/// (<c>OpcServerListener</c>); the Windows CCW path supplies the
/// COM identity contract so opcproxy + ole32 can hand the CCW to clients
/// after activation. Real per-method vtables are a future workstream
/// mirroring the OpcDaGroupCcw pattern.
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcHdaServerCcw
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_INVALIDARG = unchecked((int)0x80070057);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_ccws = new();

    public static IntPtr Create(IOpcHdaServer server, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        var handle = GCHandle.Alloc(server, GCHandleType.Normal);
        s_ccws[instance] = new CcwEntry(handle, vtable) { RefCount = 1 };
        return instance;
    }

    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown || iid == IOPCHDA_Server.InterfaceId;

    public static long GetReferenceCount(IntPtr ccw) =>
        s_ccws.TryGetValue(ccw, out CcwEntry? entry) ? Interlocked.Read(ref entry.RefCount) : -1L;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(3 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        return v;
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
        if (riid == null)
        {
            *ppv = IntPtr.Zero;
            return E_INVALIDARG;
        }
        if (SupportsInterface(*riid))
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
        if (next > 0)
        {
            return (uint)next;
        }
        DisposeEntry(pThis, entry);
        return 0;
    }

    private static void DisposeEntry(IntPtr ccw, CcwEntry entry)
    {
        if (Interlocked.Exchange(ref entry.Disposed, 1) != 0)
        {
            return;
        }
        s_ccws.TryRemove(ccw, out _);
        NativeMemory.Free((void*)ccw);
        if (entry.Vtable != null)
        {
            NativeMemory.Free(entry.Vtable);
        }
        if (entry.ServerHandle.IsAllocated)
        {
            entry.ServerHandle.Free();
        }
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle serverHandle, IntPtr* vtable)
        {
            ServerHandle = serverHandle;
            Vtable = vtable;
        }

        public GCHandle ServerHandle { get; }

        public IntPtr* Vtable { get; }

        public long RefCount;
        public int Disposed;
    }
}
