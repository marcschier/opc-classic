// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting.Windows;

[SupportedOSPlatform("windows")]
internal static unsafe class OpcEnumStringCcw
{
    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_entries = new();

    public static IntPtr Create(IReadOnlyList<string> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var state = new StringEnumeratorState(values);
        var handle = GCHandle.Alloc(state, GCHandleType.Normal);
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_entries[instance] = new CcwEntry(handle, vtable) { RefCount = 1 };
        return instance;
    }

    internal static long GetReferenceCount(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? Interlocked.Read(ref entry.RefCount)
            : -1L;

    private static IntPtr Create(StringEnumeratorState state)
    {
        var handle = GCHandle.Alloc(state, GCHandleType.Normal);
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_entries[instance] = new CcwEntry(handle, vtable) { RefCount = 1 };
        return instance;
    }

    internal static StringEnumeratorState? ResolveState(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? entry.StateHandle.Target as StringEnumeratorState
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, uint*, int>)&Next;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&Skip;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int>)&Reset;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&Clone;
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
            return OpcAeAreaBrowserCcw.E_INVALIDARG;
        }
        *ppv = IntPtr.Zero;
        if (riid == null)
        {
            return OpcAeAreaBrowserCcw.E_INVALIDARG;
        }
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry)
            || (*riid != OpcAeAreaBrowserCcw.IID_IUnknown && *riid != IEnumString.InterfaceId))
        {
            return OpcAeAreaBrowserCcw.E_NOINTERFACE;
        }

        *ppv = pThis;
        Interlocked.Increment(ref entry.RefCount);
        return OpcAeAreaBrowserCcw.S_OK;
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

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Next(IntPtr pThis, uint celt, IntPtr rgelt, uint* pceltFetched)
    {
        if (pceltFetched != null)
        {
            *pceltFetched = 0;
        }
        if (rgelt == IntPtr.Zero || (pceltFetched == null && celt != 1))
        {
            return OpcAeAreaBrowserCcw.E_INVALIDARG;
        }
        if (!TryResolveState(pThis, out StringEnumeratorState? state))
        {
            return OpcAeAreaBrowserCcw.E_FAIL;
        }
        try
        {
            int requested = celt > int.MaxValue ? int.MaxValue : (int)celt;
            ClearStringSlots(rgelt, requested);
            string[] fetched = state!.Next(requested);
            WriteStringSlots(rgelt, fetched);
            if (pceltFetched != null)
            {
                *pceltFetched = (uint)fetched.Length;
            }
            return fetched.Length == requested ? OpcAeAreaBrowserCcw.S_OK : OpcAeAreaBrowserCcw.S_FALSE;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Skip(IntPtr pThis, uint celt)
    {
        if (!TryResolveState(pThis, out StringEnumeratorState? state))
        {
            return OpcAeAreaBrowserCcw.E_FAIL;
        }
        try
        {
            int requested = celt > int.MaxValue ? int.MaxValue : (int)celt;
            bool skippedAll = celt <= int.MaxValue && state!.Skip(requested);
            return skippedAll ? OpcAeAreaBrowserCcw.S_OK : OpcAeAreaBrowserCcw.S_FALSE;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Reset(IntPtr pThis)
    {
        if (!TryResolveState(pThis, out StringEnumeratorState? state))
        {
            return OpcAeAreaBrowserCcw.E_FAIL;
        }
        try
        {
            state!.Reset();
            return OpcAeAreaBrowserCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int Clone(IntPtr pThis, IntPtr* ppEnum)
    {
        if (ppEnum != null)
        {
            *ppEnum = IntPtr.Zero;
        }
        if (ppEnum == null)
        {
            return OpcAeAreaBrowserCcw.E_INVALIDARG;
        }
        if (!TryResolveState(pThis, out StringEnumeratorState? state))
        {
            return OpcAeAreaBrowserCcw.E_FAIL;
        }
        try
        {
            *ppEnum = Create(state!.Clone());
            return OpcAeAreaBrowserCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolveState(IntPtr pThis, out StringEnumeratorState? state)
    {
        state = ResolveState(pThis);
        return state is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        ArgumentNullException => OpcAeAreaBrowserCcw.E_INVALIDARG,
        ArgumentException => OpcAeAreaBrowserCcw.E_INVALIDARG,
        _ => OpcAeAreaBrowserCcw.E_FAIL,
    };

    private static void ClearStringSlots(IntPtr rgelt, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Marshal.WriteIntPtr(rgelt, i * IntPtr.Size, IntPtr.Zero);
        }
    }

    private static void WriteStringSlots(IntPtr rgelt, string[] values)
    {
        int written = 0;
        try
        {
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteIntPtr(rgelt, i * IntPtr.Size, AllocateLpwStr(values[i]));
                written = i + 1;
            }
        }
        catch
        {
            FreeStringSlots(rgelt, written);
            throw;
        }
    }

    private static void FreeStringSlots(IntPtr rgelt, int count)
    {
        for (int i = 0; i < count; i++)
        {
            IntPtr value = Marshal.ReadIntPtr(rgelt, i * IntPtr.Size);
            if (value != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(value);
                Marshal.WriteIntPtr(rgelt, i * IntPtr.Size, IntPtr.Zero);
            }
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

    private static void DisposeEntry(IntPtr instance, CcwEntry entry)
    {
        if (Interlocked.Exchange(ref entry.Disposed, 1) != 0)
        {
            return;
        }
        s_entries.TryRemove(instance, out _);
        NativeMemory.Free((void*)instance);
        NativeMemory.Free(entry.Vtable);
        if (entry.StateHandle.IsAllocated)
        {
            entry.StateHandle.Free();
        }
    }

    internal sealed class StringEnumeratorState
    {
        private readonly string[] _values;
        private readonly Lock _lock = new();
        private int _position;

        public StringEnumeratorState(IReadOnlyList<string> values)
            : this(Copy(values), 0)
        {
        }

        private StringEnumeratorState(string[] values, int position)
        {
            _values = values;
            _position = position;
        }

        public string[] Next(int count)
        {
            lock (_lock)
            {
                int available = _values.Length - _position;
                int take = Math.Min(Math.Max(count, 0), available);
                var result = new string[take];
                Array.Copy(_values, _position, result, 0, take);
                _position += take;
                return result;
            }
        }

        public bool Skip(int count)
        {
            lock (_lock)
            {
                int requested = Math.Max(count, 0);
                int available = _values.Length - _position;
                int skipped = Math.Min(requested, available);
                _position += skipped;
                return skipped == requested;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _position = 0;
            }
        }

        public StringEnumeratorState Clone()
        {
            lock (_lock)
            {
                return new StringEnumeratorState(_values, _position);
            }
        }

        private static string[] Copy(IReadOnlyList<string> values)
        {
            var copy = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                copy[i] = values[i] ?? string.Empty;
            }
            return copy;
        }
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle stateHandle, IntPtr* vtable)
        {
            StateHandle = stateHandle;
            Vtable = vtable;
        }

        public GCHandle StateHandle { get; }
        public IntPtr* Vtable { get; }
        public long RefCount;
        public int Disposed;
    }
}
