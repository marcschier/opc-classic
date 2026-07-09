// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Single-tearoff Windows CCW for OAIDL <c>IEnumConnectionPoints</c>.
/// </summary>
[SupportedOSPlatform("windows")]
public static unsafe class OpcHdaEnumConnectionPointsCcw
{
    internal const int S_OK = 0;
    internal const int S_FALSE = 1;
    internal static readonly int E_NOINTERFACE = global::Opc.Classic.OpcResultId.NoInterface.Code;
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_FAIL = unchecked((int)0x80004005);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_entries = new();

    /// <summary>
    /// Creates an <c>IEnumConnectionPoints</c> CCW with refcount = 1.
    /// </summary>
    internal static IntPtr Create(OpcHdaEnumConnectionPointsEnumerator enumerator)
    {
        ArgumentNullException.ThrowIfNull(enumerator);

        var handle = GCHandle.Alloc(enumerator, GCHandleType.Normal);
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_entries[instance] = new CcwEntry(handle, vtable) { RefCount = 1 };
        return instance;
    }

    /// <summary>
    /// Test helper: returns the current refcount, or -1 if unknown.
    /// </summary>
    public static long GetReferenceCount(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? Interlocked.Read(ref entry.RefCount)
            : -1L;

    internal static OpcHdaEnumConnectionPointsEnumerator? ResolveEnumerator(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? entry.EnumeratorHandle.Target as OpcHdaEnumConnectionPointsEnumerator
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr*, uint*, int>)&OpcHdaEnumConnectionPointsCcwMethods.Next;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcHdaEnumConnectionPointsCcwMethods.Skip;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int>)&OpcHdaEnumConnectionPointsCcwMethods.Reset;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcHdaEnumConnectionPointsCcwMethods.Clone;
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
        *ppv = IntPtr.Zero;
        if (riid == null || !s_entries.TryGetValue(pThis, out CcwEntry? entry))
        {
            return riid == null ? E_INVALIDARG : E_NOINTERFACE;
        }
        if (*riid != IID_IUnknown && *riid != OpcGuids.IID_IEnumConnectionPoints)
        {
            return E_NOINTERFACE;
        }

        *ppv = pThis;
        Interlocked.Increment(ref entry.RefCount);
        return S_OK;
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
        DisposeEntry(pThis, entry);
        return 0;
    }

    private static void DisposeEntry(IntPtr instance, CcwEntry entry)
    {
        if (Interlocked.Exchange(ref entry.Disposed, 1) != 0)
        {
            return;
        }
        s_entries.TryRemove(instance, out _);
        if (entry.EnumeratorHandle.Target is IDisposable disposable)
        {
            disposable.Dispose();
        }
        NativeMemory.Free((void*)instance);
        NativeMemory.Free(entry.Vtable);
        if (entry.EnumeratorHandle.IsAllocated)
        {
            entry.EnumeratorHandle.Free();
        }
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle enumeratorHandle, IntPtr* vtable)
        {
            EnumeratorHandle = enumeratorHandle;
            Vtable = vtable;
        }

        public GCHandle EnumeratorHandle { get; }
        public IntPtr* Vtable { get; }
        public long RefCount;
        public int Disposed;
    }
}
