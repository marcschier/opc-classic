// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hosting.Windows;

[SupportedOSPlatform("windows")]
public static unsafe class OpcEnumUnknownCcw
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private const int E_FAIL = unchecked((int)0x80004005);
    private static readonly int s_eNoInterface = OpcResultId.NoInterface.Code;
    private static readonly Guid s_iidIUnknown = new("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, Entry> s_entries = new();
    private static readonly IntPtr s_vtable = AllocateVtable();

    public static IntPtr Create(IReadOnlyList<IntPtr> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        return Create(new State(values));
    }

    public static long GetReferenceCount(IntPtr instance) =>
        s_entries.TryGetValue(instance, out Entry? entry)
            ? Interlocked.Read(ref entry.RefCount)
            : -1L;

    private static IntPtr Create(State state)
    {
        GCHandle handle = default;
        IntPtr instance = IntPtr.Zero;
        try
        {
            handle = GCHandle.Alloc(state, GCHandleType.Normal);
            instance = AllocateInstance();
            if (!s_entries.TryAdd(instance, new Entry(handle) { RefCount = 1 }))
            {
                throw new InvalidOperationException("Could not register IEnumUnknown.");
            }
            return instance;
        }
        catch
        {
            if (instance != IntPtr.Zero)
            {
                NativeMemory.Free((void*)instance);
            }
            if (handle.IsAllocated)
            {
                handle.Free();
            }
            state.Dispose();
            throw;
        }
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateVtable()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr*, uint*, int>)&Next;
        vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&Skip;
        vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, int>)&Reset;
        vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&Clone;
        return (IntPtr)vtable;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInstance()
    {
        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = s_vtable;
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
        if (!s_entries.TryGetValue(pThis, out Entry? entry)
            || (*riid != s_iidIUnknown && *riid != OpcGuids.IID_IEnumUnknown))
        {
            return s_eNoInterface;
        }
        *ppv = pThis;
        Interlocked.Increment(ref entry.RefCount);
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis) =>
        s_entries.TryGetValue(pThis, out Entry? entry)
            ? (uint)Interlocked.Increment(ref entry.RefCount)
            : 0;

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        if (!s_entries.TryGetValue(pThis, out Entry? entry))
        {
            return 0;
        }
        long next = Interlocked.Decrement(ref entry.RefCount);
        if (next > 0)
        {
            return (uint)next;
        }
        if (next == 0)
        {
            DisposeEntry(pThis, entry);
        }
        return 0;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "COM boundary.")]
    private static int Next(IntPtr pThis, uint count, IntPtr* values, uint* fetched)
    {
        if (fetched != null)
        {
            *fetched = 0;
        }
        if ((count > 0 && values == null) || (count > 1 && fetched == null))
        {
            return E_INVALIDARG;
        }
        if (!TryResolve(pThis, out State? state))
        {
            return E_FAIL;
        }
        try
        {
            int actual = state!.Next(count, values);
            if (fetched != null)
            {
                *fetched = (uint)actual;
            }
            return (ulong)actual == count ? S_OK : S_FALSE;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    private static int Skip(IntPtr pThis, uint count)
    {
        if (!TryResolve(pThis, out State? state))
        {
            return E_FAIL;
        }
        return (ulong)state!.Skip(count) == count ? S_OK : S_FALSE;
    }

    [UnmanagedCallersOnly]
    private static int Reset(IntPtr pThis)
    {
        if (!TryResolve(pThis, out State? state))
        {
            return E_FAIL;
        }
        state!.Reset();
        return S_OK;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "COM boundary.")]
    private static int Clone(IntPtr pThis, IntPtr* clone)
    {
        if (clone == null)
        {
            return E_INVALIDARG;
        }
        *clone = IntPtr.Zero;
        if (!TryResolve(pThis, out State? state))
        {
            return E_FAIL;
        }
        try
        {
            *clone = Create(state!.Clone());
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolve(IntPtr instance, out State? state)
    {
        state = s_entries.TryGetValue(instance, out Entry? entry)
            ? entry.Handle.Target as State
            : null;
        return state is not null;
    }

    private static int MapHResult(Exception ex) =>
        ex is ArgumentException ? E_INVALIDARG : E_FAIL;

    private static void DisposeEntry(IntPtr instance, Entry entry)
    {
        if (Interlocked.Exchange(ref entry.Disposed, 1) != 0)
        {
            return;
        }
        s_entries.TryRemove(instance, out _);
        if (entry.Handle.Target is IDisposable disposable)
        {
            disposable.Dispose();
        }
        if (entry.Handle.IsAllocated)
        {
            entry.Handle.Free();
        }
        NativeMemory.Free((void*)instance);
    }

    private sealed class Entry
    {
        public Entry(GCHandle handle) => Handle = handle;
        public GCHandle Handle { get; }
        public long RefCount;
        public int Disposed;
    }

    private sealed class State : IDisposable
    {
        private readonly Lock _lock = new();
        private readonly IntPtr[] _values;
        private int _position;
        private bool _disposed;

        public State(IReadOnlyList<IntPtr> values)
            : this(AddRefSnapshot(values), 0)
        {
        }

        private State(IntPtr[] values, int position)
        {
            _values = values;
            _position = position;
        }

        public int Next(uint count, IntPtr* output)
        {
            lock (_lock)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                int available = _values.Length - _position;
                int requested = count > int.MaxValue ? int.MaxValue : (int)count;
                int actual = Math.Min(requested, available);
                for (int i = 0; i < actual; i++)
                {
                    IntPtr value = _values[_position++];
                    AddRefPointer(value);
                    output[i] = value;
                }
                return actual;
            }
        }

        public int Skip(uint count)
        {
            lock (_lock)
            {
                int available = _values.Length - _position;
                int actual = (int)Math.Min((ulong)count, (ulong)available);
                _position += actual;
                return actual;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _position = 0;
            }
        }

        public State Clone()
        {
            lock (_lock)
            {
                return new State(AddRefSnapshot(_values), _position);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    return;
                }
                _disposed = true;
                foreach (IntPtr value in _values)
                {
                    ReleasePointer(value);
                }
            }
        }

        private static IntPtr[] AddRefSnapshot(IReadOnlyList<IntPtr> values)
        {
            var copy = new IntPtr[values.Count];
            int owned = 0;
            try
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i] == IntPtr.Zero)
                    {
                        throw new ArgumentException("IUnknown snapshot contains null.", nameof(values));
                    }
                    AddRefPointer(values[i]);
                    copy[i] = values[i];
                    owned++;
                }
                return copy;
            }
            catch
            {
                for (int i = 0; i < owned; i++)
                {
                    ReleasePointer(copy[i]);
                }
                throw;
            }
        }

        private static void AddRefPointer(IntPtr value)
        {
            IntPtr* vtable = *(IntPtr**)value;
            var addRef = (delegate* unmanaged<IntPtr, uint>)vtable[1];
            _ = addRef(value);
        }

        private static void ReleasePointer(IntPtr value)
        {
            IntPtr* vtable = *(IntPtr**)value;
            var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
            _ = release(value);
        }
    }
}
