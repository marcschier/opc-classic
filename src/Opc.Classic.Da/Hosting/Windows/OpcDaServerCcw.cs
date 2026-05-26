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
/// Builds a Windows COM-callable wrapper (CCW) over an <see cref="IOpcDaServer"/>
/// instance. The CCW exposes <c>IUnknown</c> + <c>IOPCServer</c> vtables
/// allocated in native memory; vtable thunks are
/// <see cref="UnmanagedCallersOnlyAttribute"/>-decorated static methods so the
/// assembly remains NativeAOT-compatible (<c>[ComImport]</c> is banned in
/// <c>src/</c>).
/// </summary>
/// <remarks>
/// <para>
/// The OPC DA root server object (the one returned from
/// <c>IClassFactory::CreateInstance</c> on activation) supports
/// <c>IID_IUnknown</c> and <c>IID_IOPCServer</c>. Other interfaces
/// (<c>IOPCBrowse</c>, <c>IOPCCommon</c>, ...) will be added in follow-up
/// work alongside the per-object IPID registry (ocom-3b).
/// </para>
/// <para>
/// <b>Method stubs.</b> ocom-6 ships with all 9 <c>IOPCServer</c> vtable slots
/// returning <c>E_NOTIMPL</c>. This is enough for the SCM activation path to
/// complete (client gets a real interface pointer, marshaling succeeds); actual
/// per-method dispatch from native CCW into managed <see cref="IOpcDaServer"/>
/// is wired up by a follow-up task. Today the goal is to unblock the
/// "activation succeeds, method calls fail with a known error" stage rather
/// than the previous "activation fails with E_NOINTERFACE" stage.
/// </para>
/// <para>
/// <b>Lifetime.</b> CCW instances and their vtables are never freed
/// (leak-at-process-exit). Once handed to ole32, the pointer must remain
/// valid for the lifetime of the registration; freeing on
/// <c>Release</c>-to-zero would race with in-flight <c>QueryInterface</c>
/// calls. The managed <see cref="IOpcDaServer"/> is pinned via
/// <see cref="GCHandle"/>; the handle is also never freed.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcDaServerCcw
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private const int E_NOTIMPL = unchecked((int)0x80004001);
    private const int E_FAIL = unchecked((int)0x80004005);

    private const int VtableSlotCount = 12; // 3 IUnknown + 9 IOPCServer

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_ccws = new();

    /// <summary>
    /// Builds a CCW around <paramref name="server"/> and returns a pointer to
    /// the requested interface, or <see cref="IntPtr.Zero"/> if the interface
    /// is not supported by this CCW.
    /// </summary>
    /// <param name="server">The managed server instance to expose via COM.</param>
    /// <param name="requestedIid">
    /// The IID requested by <c>IClassFactory::CreateInstance</c>. The CCW
    /// supports <c>IID_IUnknown</c> and <see cref="Dcom.IOPCServer.InterfaceId"/>;
    /// other IIDs return <see cref="IntPtr.Zero"/>.
    /// </param>
    /// <returns>
    /// A CCW <see cref="IntPtr"/> with reference count = 1 (the caller's
    /// reference), or <see cref="IntPtr.Zero"/> for
    /// <c>E_NOINTERFACE</c>-equivalent.
    /// </returns>
    public static IntPtr Create(IOpcDaServer server, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        var handle = GCHandle.Alloc(server, GCHandleType.Normal);
        var entry = new CcwEntry(handle);
        entry.RefCount = 1;
        s_ccws[instance] = entry;
        return instance;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="iid"/> is one of the
    /// COM interfaces this CCW exposes.
    /// </summary>
    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown || iid == Dcom.IOPCServer.InterfaceId;

    /// <summary>
    /// Test helper: returns the current reference count for a CCW pointer, or
    /// <c>-1</c> if the pointer is not a known CCW.
    /// </summary>
    public static long GetReferenceCount(IntPtr ccw) =>
        s_ccws.TryGetValue(ccw, out CcwEntry? entry) ? Interlocked.Read(ref entry.RefCount) : -1L;

    [SuppressMessage(
        "Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating IntPtr-sized native vtable with explicit byte count.")]
    private static IntPtr* AllocateVtable()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(VtableSlotCount * sizeof(IntPtr)));
        // IUnknown
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        // IOPCServer (canonical opnum order per OPC DA 3.0 spec)
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int, uint, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr, Guid*, IntPtr*, int>)&AddGroup;
        vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, int, uint, IntPtr*, int>)&GetErrorString;
        vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)&GetGroupByName;
        vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&GetStatus;
        vtable[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, int>)&RemoveGroup;
        vtable[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, Guid*, IntPtr*, int>)&CreateGroupEnumerator;
        // Remaining slots are reserved; zero them so a misdirected dispatch
        // crashes loudly instead of into arbitrary memory.
        for (int i = 9; i < VtableSlotCount; i++)
        {
            vtable[i] = IntPtr.Zero;
        }
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
        // CCWs are never freed (leak-at-exit); see class remarks.
        return next < 0 ? 0 : (uint)next;
    }

    // ===== IOPCServer stubs (E_NOTIMPL until ocom-3b/follow-up wires real impls) =====

    [UnmanagedCallersOnly]
    private static int AddGroup(
        IntPtr pThis,
        IntPtr szName,
        int bActive,
        uint dwRequestedUpdateRate,
        uint hClientGroup,
        IntPtr pTimeBias,
        IntPtr pPercentDeadband,
        uint dwLCID,
        IntPtr phServerGroup,
        IntPtr pRevisedUpdateRate,
        Guid* riid,
        IntPtr* ppUnk)
    {
        _ = pThis; _ = szName; _ = bActive; _ = dwRequestedUpdateRate;
        _ = hClientGroup; _ = pTimeBias; _ = pPercentDeadband; _ = dwLCID;
        _ = phServerGroup; _ = pRevisedUpdateRate; _ = riid;
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int GetErrorString(IntPtr pThis, int dwError, uint dwLocale, IntPtr* ppString)
    {
        _ = pThis; _ = dwError; _ = dwLocale;
        if (ppString != null)
        {
            *ppString = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int GetGroupByName(IntPtr pThis, IntPtr szName, Guid* riid, IntPtr* ppUnk)
    {
        _ = pThis; _ = szName; _ = riid;
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int GetStatus(IntPtr pThis, IntPtr* ppServerStatus)
    {
        _ = pThis;
        if (ppServerStatus != null)
        {
            *ppServerStatus = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int RemoveGroup(IntPtr pThis, uint hServerGroup, int bForce)
    {
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOTIMPL;
        }
        if (entry.ServerHandle.Target is not IOpcDaServer server)
        {
            return E_NOTIMPL;
        }

        try
        {
#pragma warning disable VSTHRD002 // The CCW method runs synchronously across the COM ABI; bridge to the async impl via .GetAwaiter().GetResult().
            server.RemoveGroupAsync((int)hServerGroup, bForce != 0, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return S_OK;
        }
#pragma warning disable CA1031 // Cross-unmanaged-boundary catch: any escaping managed exception would crash the process.
        catch (Opc.Classic.OpcException opcEx)
        {
            return opcEx.ResultId.Code;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    [UnmanagedCallersOnly]
    private static int CreateGroupEnumerator(IntPtr pThis, uint dwScope, Guid* riid, IntPtr* ppUnk)
    {
        _ = pThis; _ = dwScope; _ = riid;
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle serverHandle)
        {
            ServerHandle = serverHandle;
        }

        public GCHandle ServerHandle { get; }

        public long RefCount;
    }
}
