//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Browser CCW, vtable methods, and IEnumString helper are tightly coupled.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>Single-tearoff Windows CCW for OPC HDA <c>IOPCHDA_Browser</c>.</summary>
[SupportedOSPlatform("windows")]
public static unsafe class OpcHdaBrowserCcw {
    internal const int S_OK = 0;
    internal const int S_FALSE = 1;
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_FAIL = unchecked((int)0x80004005);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_entries = new();

    /// <summary>Creates an <c>IOPCHDA_Browser</c> CCW with refcount = 1.</summary>
    public static IntPtr Create(IOpcHdaServerDispatcher dispatcher, IReadOnlyList<OpcHdaBrowseFilter> filters) {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(filters);

        var state = new BrowserState(dispatcher, CopyFilters(filters));
        var handle = GCHandle.Alloc(state, GCHandleType.Normal);
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_entries[instance] = new CcwEntry(handle, vtable) { RefCount = 1 };
        return instance;
    }

    /// <summary>Test helper: returns the current refcount, or -1 if unknown.</summary>
    public static long GetReferenceCount(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? Interlocked.Read(ref entry.RefCount)
            : -1L;

    internal static BrowserState? ResolveState(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? entry.StateHandle.Target as BrowserState
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateVtable() {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr*, int>)&OpcHdaBrowserCcwMethods.GetEnum;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, int>)&OpcHdaBrowserCcwMethods.ChangeBrowsePosition;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, int>)&OpcHdaBrowserCcwMethods.GetItemID;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcHdaBrowserCcwMethods.GetBranchPosition;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInstance(IntPtr* vtable) {
        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;
        return (IntPtr)instance;
    }

    [UnmanagedCallersOnly]
    private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv) {
        if (ppv == null) {
            return E_INVALIDARG;
        }

        *ppv = IntPtr.Zero;
        if (riid == null || !s_entries.TryGetValue(pThis, out CcwEntry? entry)) {
            return riid == null ? E_INVALIDARG : E_NOINTERFACE;
        }
        if (*riid != IID_IUnknown && *riid != IOPCHDA_Browser.InterfaceId) {
            return E_NOINTERFACE;
        }

        *ppv = pThis;
        Interlocked.Increment(ref entry.RefCount);
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis) {
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry)) {
            return 1;
        }

        return (uint)Interlocked.Increment(ref entry.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis) {
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry)) {
            return 0;
        }

        long next = Interlocked.Decrement(ref entry.RefCount);
        if (next > 0) {
            return (uint)next;
        }

        DisposeEntry(pThis, entry);
        return 0;
    }

    private static void DisposeEntry(IntPtr instance, CcwEntry entry) {
        if (Interlocked.Exchange(ref entry.Disposed, 1) != 0) {
            return;
        }

        s_entries.TryRemove(instance, out _);
        NativeMemory.Free((void*)instance);
        NativeMemory.Free(entry.Vtable);
        if (entry.StateHandle.IsAllocated) {
            entry.StateHandle.Free();
        }
    }

    private static OpcHdaBrowseFilter[] CopyFilters(IReadOnlyList<OpcHdaBrowseFilter> filters) {
        var copy = new OpcHdaBrowseFilter[filters.Count];
        for (int i = 0; i < filters.Count; i++) {
            copy[i] = filters[i];
        }

        return copy;
    }

    internal sealed class BrowserState {
        private readonly Lock _lock = new();
        private string _branchPosition = string.Empty;

        public BrowserState(IOpcHdaServerDispatcher dispatcher, OpcHdaBrowseFilter[] filters) {
            Dispatcher = dispatcher;
            Filters = filters;
        }

        public IOpcHdaServerDispatcher Dispatcher { get; }

        public OpcHdaBrowseFilter[] Filters { get; }

        public string GetBranchPosition() {
            lock (_lock) {
                return _branchPosition;
            }
        }

        public void SetBranchPosition(string branchPosition) {
            lock (_lock) {
                _branchPosition = branchPosition;
            }
        }
    }

    private sealed class CcwEntry {
        public CcwEntry(GCHandle stateHandle, IntPtr* vtable) {
            StateHandle = stateHandle;
            Vtable = vtable;
        }

        public GCHandle StateHandle { get; }
        public IntPtr* Vtable { get; }
        public long RefCount;
        public int Disposed;
    }
}

/// <summary>Method bodies for <see cref="OpcHdaBrowserCcw" />.</summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaBrowserCcwMethods {
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetEnum(IntPtr pThis, uint dwBrowseType, IntPtr* ppIEnumString) {
        ZeroOut(ppIEnumString);
        if (ppIEnumString == null) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaBrowserCcw.BrowserState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }
        if (!TryMapBrowseType(dwBrowseType, out HdaBrowseType browseType)) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }

        try {
            string branchPosition = state!.GetBranchPosition();
#pragma warning disable VSTHRD002
            IReadOnlyList<string> values = state.Dispatcher.BrowseAsync(
                branchPosition,
                browseType,
                state.Filters,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppIEnumString = OpcHdaEnumStringCcw.Create(values);
            return OpcHdaBrowserCcw.S_OK;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int ChangeBrowsePosition(IntPtr pThis, uint dwBrowseDirection, IntPtr szString) {
        if (!TryResolve(pThis, out OpcHdaBrowserCcw.BrowserState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }
        if (!IsValidBrowseDirection(dwBrowseDirection)) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }

        try {
            string browseString = ReadInputString(szString);
            string currentPosition = state!.GetBranchPosition();
#pragma warning disable VSTHRD002
            string nextPosition = state.Dispatcher.ChangeBrowsePositionAsync(
                currentPosition,
                checked((int)dwBrowseDirection),
                browseString,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            state.SetBranchPosition(nextPosition ?? string.Empty);
            return OpcHdaBrowserCcw.S_OK;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetItemID(IntPtr pThis, IntPtr szNode, IntPtr* pszItemID) {
        ZeroOut(pszItemID);
        if (pszItemID == null || szNode == IntPtr.Zero) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaBrowserCcw.BrowserState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }

        try {
            string branchPosition = state!.GetBranchPosition();
            string node = ReadInputString(szNode);
#pragma warning disable VSTHRD002
            string itemId = state.Dispatcher.GetItemIdAsync(branchPosition, node, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pszItemID = AllocateBstr(itemId);
            return OpcHdaBrowserCcw.S_OK;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetBranchPosition(IntPtr pThis, IntPtr* pszBranchPos) {
        ZeroOut(pszBranchPos);
        if (pszBranchPos == null) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaBrowserCcw.BrowserState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }

        try {
            string branchPosition = state!.GetBranchPosition();
#pragma warning disable VSTHRD002
            string value = state.Dispatcher.GetBranchPositionAsync(branchPosition, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *pszBranchPos = AllocateBstr(value);
            return OpcHdaBrowserCcw.S_OK;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    private static bool TryResolve(IntPtr pThis, out OpcHdaBrowserCcw.BrowserState? state) {
        state = OpcHdaBrowserCcw.ResolveState(pThis);
        return state is not null;
    }

    private static bool TryMapBrowseType(uint value, out HdaBrowseType browseType) {
        browseType = value switch {
            1 => HdaBrowseType.Branch,
            2 => HdaBrowseType.Leaf,
            3 => HdaBrowseType.Flat,
            4 => HdaBrowseType.Items,
            _ => default,
        };
        return value is >= 1 and <= 4;
    }

    private static bool IsValidBrowseDirection(uint value) => value is >= 1 and <= 3;

    private static string ReadInputString(IntPtr value) =>
        value == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(value) ?? string.Empty;

    private static IntPtr AllocateBstr(string? value) => Marshal.StringToBSTR(value ?? string.Empty);

    private static int MapHResult(Exception ex) => ex switch {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentException => OpcHdaBrowserCcw.E_INVALIDARG,
        _ => OpcHdaBrowserCcw.E_FAIL,
    };

    private static void ZeroOut(IntPtr* pp) {
        if (pp != null) {
            *pp = IntPtr.Zero;
        }
    }
}

[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaEnumStringCcw {
    private static readonly Guid s_iidIUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_entries = new();

    public static IntPtr Create(IReadOnlyList<string> values) => Create(values, 0);

    private static IntPtr Create(IReadOnlyList<string> values, int position) {
        ArgumentNullException.ThrowIfNull(values);

        var state = new EnumStringState(CopyValues(values), position);
        var handle = GCHandle.Alloc(state, GCHandleType.Normal);
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_entries[instance] = new CcwEntry(handle, vtable) { RefCount = 1 };
        return instance;
    }

    internal static EnumStringState? ResolveState(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? entry.StateHandle.Target as EnumStringState
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateVtable() {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, uint*, int>)&OpcHdaEnumStringCcwMethods.Next;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&OpcHdaEnumStringCcwMethods.Skip;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int>)&OpcHdaEnumStringCcwMethods.Reset;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&OpcHdaEnumStringCcwMethods.Clone;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr AllocateInstance(IntPtr* vtable) {
        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;
        return (IntPtr)instance;
    }

    [UnmanagedCallersOnly]
    private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv) {
        if (ppv == null) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }

        *ppv = IntPtr.Zero;
        if (riid == null || !s_entries.TryGetValue(pThis, out CcwEntry? entry)) {
            return riid == null ? OpcHdaBrowserCcw.E_INVALIDARG : OpcHdaBrowserCcw.E_NOINTERFACE;
        }
        if (*riid != s_iidIUnknown && *riid != OpcGuids.IID_IEnumString) {
            return OpcHdaBrowserCcw.E_NOINTERFACE;
        }

        *ppv = pThis;
        Interlocked.Increment(ref entry.RefCount);
        return OpcHdaBrowserCcw.S_OK;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis) {
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry)) {
            return 1;
        }

        return (uint)Interlocked.Increment(ref entry.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis) {
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry)) {
            return 0;
        }

        long next = Interlocked.Decrement(ref entry.RefCount);
        if (next > 0) {
            return (uint)next;
        }

        DisposeEntry(pThis, entry);
        return 0;
    }

    internal static IntPtr Clone(EnumStringState state) {
        int position;
        string[] values;
        lock (state.Lock) {
            position = state.Position;
            values = state.Values;
        }

        return Create(values, position);
    }

    private static void DisposeEntry(IntPtr instance, CcwEntry entry) {
        if (Interlocked.Exchange(ref entry.Disposed, 1) != 0) {
            return;
        }

        s_entries.TryRemove(instance, out _);
        NativeMemory.Free((void*)instance);
        NativeMemory.Free(entry.Vtable);
        if (entry.StateHandle.IsAllocated) {
            entry.StateHandle.Free();
        }
    }

    private static string[] CopyValues(IReadOnlyList<string> values) {
        var copy = new string[values.Count];
        for (int i = 0; i < values.Count; i++) {
            copy[i] = values[i] ?? string.Empty;
        }

        return copy;
    }

    internal sealed class EnumStringState {
        public EnumStringState(string[] values, int position) {
            Values = values;
            Position = Math.Clamp(position, 0, values.Length);
        }

        public Lock Lock { get; } = new();
        public string[] Values { get; }
        public int Position { get; set; }
    }

    private sealed class CcwEntry {
        public CcwEntry(GCHandle stateHandle, IntPtr* vtable) {
            StateHandle = stateHandle;
            Vtable = vtable;
        }

        public GCHandle StateHandle { get; }
        public IntPtr* Vtable { get; }
        public long RefCount;
        public int Disposed;
    }
}

[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaEnumStringCcwMethods {
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Next(IntPtr pThis, uint celt, IntPtr rgelt, uint* pceltFetched) {
        if (pceltFetched != null) {
            *pceltFetched = 0;
        }
        if (rgelt == IntPtr.Zero || (celt > 1 && pceltFetched == null)) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaEnumStringCcw.EnumStringState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }

        try {
            uint fetched = 0;
            lock (state!.Lock) {
                while (fetched < celt && state.Position < state.Values.Length) {
                    Marshal.WriteIntPtr(rgelt, checked((int)fetched) * IntPtr.Size, AllocateLpwStr(state.Values[state.Position]));
                    state.Position++;
                    fetched++;
                }
            }

            if (pceltFetched != null) {
                *pceltFetched = fetched;
            }

            return fetched == celt ? OpcHdaBrowserCcw.S_OK : OpcHdaBrowserCcw.S_FALSE;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Skip(IntPtr pThis, uint celt) {
        if (!TryResolve(pThis, out OpcHdaEnumStringCcw.EnumStringState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }

        try {
            lock (state!.Lock) {
                int requested = celt > int.MaxValue ? int.MaxValue : (int)celt;
                int available = state.Values.Length - state.Position;
                state.Position += Math.Min(requested, available);
                return requested <= available ? OpcHdaBrowserCcw.S_OK : OpcHdaBrowserCcw.S_FALSE;
            }
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int Reset(IntPtr pThis) {
        if (!TryResolve(pThis, out OpcHdaEnumStringCcw.EnumStringState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }

        lock (state!.Lock) {
            state.Position = 0;
        }

        return OpcHdaBrowserCcw.S_OK;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Clone(IntPtr pThis, IntPtr* ppEnum) {
        ZeroOut(ppEnum);
        if (ppEnum == null) {
            return OpcHdaBrowserCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaEnumStringCcw.EnumStringState? state)) {
            return OpcHdaBrowserCcw.E_FAIL;
        }

        try {
            *ppEnum = OpcHdaEnumStringCcw.Clone(state!);
            return OpcHdaBrowserCcw.S_OK;
        }
        catch (Exception ex) {
            return MapHResult(ex);
        }
    }

    private static bool TryResolve(IntPtr pThis, out OpcHdaEnumStringCcw.EnumStringState? state) {
        state = OpcHdaEnumStringCcw.ResolveState(pThis);
        return state is not null;
    }

    private static IntPtr AllocateLpwStr(string value) => Marshal.StringToCoTaskMemUni(value);

    private static int MapHResult(Exception ex) => ex switch {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentException => OpcHdaBrowserCcw.E_INVALIDARG,
        _ => OpcHdaBrowserCcw.E_FAIL,
    };

    private static void ZeroOut(IntPtr* pp) {
        if (pp != null) {
            *pp = IntPtr.Zero;
        }
    }
}

internal static class NativeHdaVariantReader {
    private const ushort VT_EMPTY = 0;
    private const ushort VT_NULL = 1;
    private const ushort VT_I2 = 2;
    private const ushort VT_I4 = 3;
    private const ushort VT_R4 = 4;
    private const ushort VT_R8 = 5;
    private const ushort VT_CY = 6;
    private const ushort VT_DATE = 7;
    private const ushort VT_BSTR = 8;
    private const ushort VT_ERROR = 10;
    private const ushort VT_BOOL = 11;
    private const ushort VT_I1 = 16;
    private const ushort VT_UI1 = 17;
    private const ushort VT_UI2 = 18;
    private const ushort VT_UI4 = 19;
    private const ushort VT_I8 = 20;
    private const ushort VT_UI8 = 21;
    private const ushort VT_INT = 22;
    private const ushort VT_UINT = 23;
    private const ushort VT_LPWSTR = 31;
    private const ushort VT_FILETIME = 64;
    private const ushort VT_ARRAY = 0x2000;
    private const ushort VT_BYREF = 0x4000;

    public static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

    public static bool TryRead(IntPtr source, out OpcVariant value) {
        value = OpcVariant.Empty;
        if (source == IntPtr.Zero) {
            return false;
        }

        ushort vt = unchecked((ushort)Marshal.ReadInt16(source));
        if ((vt & (VT_ARRAY | VT_BYREF)) != 0) {
            return false;
        }

        IntPtr payload = source + 8;
        value = vt switch {
            VT_EMPTY => OpcVariant.Empty,
            VT_NULL => OpcVariant.Null,
            VT_I1 => OpcVariant.FromInt8(unchecked((sbyte)Marshal.ReadByte(payload))),
            VT_UI1 => OpcVariant.FromUInt8(Marshal.ReadByte(payload)),
            VT_I2 => OpcVariant.FromInt16(Marshal.ReadInt16(payload)),
            VT_UI2 => OpcVariant.FromUInt16(unchecked((ushort)Marshal.ReadInt16(payload))),
            VT_BOOL => OpcVariant.FromBoolean(Marshal.ReadInt16(payload) != 0),
            VT_I4 or VT_INT => OpcVariant.FromInt32(Marshal.ReadInt32(payload)),
            VT_UI4 or VT_UINT => OpcVariant.FromUInt32(unchecked((uint)Marshal.ReadInt32(payload))),
            VT_ERROR => OpcVariant.FromError(Marshal.ReadInt32(payload)),
            VT_R4 => OpcVariant.FromSingle(BitConverter.Int32BitsToSingle(Marshal.ReadInt32(payload))),
            VT_R8 => OpcVariant.FromDouble(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(payload))),
            VT_CY => new OpcVariant(VarType.VT_CY, Marshal.ReadInt64(payload)),
            VT_DATE => OpcVariant.FromDate(DateTime.FromOADate(BitConverter.Int64BitsToDouble(Marshal.ReadInt64(payload)))),
            VT_BSTR => OpcVariant.FromString(Marshal.PtrToStringBSTR(Marshal.ReadIntPtr(payload)) ?? string.Empty),
            VT_LPWSTR => OpcVariant.FromString(Marshal.PtrToStringUni(Marshal.ReadIntPtr(payload)) ?? string.Empty),
            VT_I8 => OpcVariant.FromInt64(Marshal.ReadInt64(payload)),
            VT_UI8 => OpcVariant.FromUInt64(unchecked((ulong)Marshal.ReadInt64(payload))),
            VT_FILETIME => OpcVariant.FromFileTime(Marshal.ReadInt64(payload)),
            _ => OpcVariant.Empty,
        };
        return vt is VT_EMPTY or VT_NULL or VT_I1 or VT_UI1 or VT_I2 or VT_UI2 or VT_BOOL or VT_I4 or VT_INT or
            VT_UI4 or VT_UINT or VT_ERROR or VT_R4 or VT_R8 or VT_CY or VT_DATE or VT_BSTR or VT_LPWSTR or VT_I8 or
            VT_UI8 or VT_FILETIME;
    }
}
