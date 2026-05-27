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
/// Single-tearoff Windows CCW for <see cref="IOPCEventAreaBrowser" />.
/// </summary>
[SupportedOSPlatform("windows")]
public static unsafe class OpcAeAreaBrowserCcw
{
    internal const int S_OK = 0;
    internal const int S_FALSE = 1;
    internal const int E_NOINTERFACE = unchecked((int)0x80004002);
    internal const int E_INVALIDARG = unchecked((int)0x80070057);
    internal const int E_NOTIMPL = unchecked((int)0x80004001);
    internal const int E_FAIL = unchecked((int)0x80004005);

    internal static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_entries = new();

    /// <summary>Creates an <c>IOPCEventAreaBrowser</c> CCW with refcount = 1.</summary>
    public static IntPtr Create(IOpcAeAreaBrowserDispatcher browser, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(browser);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        var handle = GCHandle.Alloc(browser, GCHandleType.Normal);
        IntPtr* vtable = AllocateVtable();
        IntPtr instance = AllocateInstance(vtable);
        s_entries[instance] = new CcwEntry(handle, vtable) { RefCount = 1 };
        return instance;
    }

    /// <summary>Returns whether this CCW supports <paramref name="iid" />.</summary>
    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown || iid == IOPCEventAreaBrowser.InterfaceId;

    /// <summary>Test helper: returns the current refcount, or -1 if unknown.</summary>
    public static long GetReferenceCount(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? Interlocked.Read(ref entry.RefCount)
            : -1L;

    internal static IOpcAeAreaBrowserDispatcher? ResolveBrowser(IntPtr instance) =>
        s_entries.TryGetValue(instance, out CcwEntry? entry)
            ? entry.BrowserHandle.Target as IOpcAeAreaBrowserDispatcher
            : null;

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte size.")]
    private static IntPtr* AllocateVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(7 * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, int>)&ChangeBrowsePosition;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr, IntPtr*, int>)&BrowseOPCAreas;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, int>)&GetQualifiedAreaName;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, int>)&GetQualifiedSourceName;
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
        if (riid == null)
        {
            return E_INVALIDARG;
        }
        if (!s_entries.TryGetValue(pThis, out CcwEntry? entry) || !SupportsInterface(*riid))
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

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int ChangeBrowsePosition(IntPtr pThis, int dwBrowseDirection, IntPtr szString)
    {
        if (!TryResolveBrowser(pThis, out IOpcAeAreaBrowserDispatcher? browser))
        {
            return E_FAIL;
        }
        try
        {
            string? position = szString == IntPtr.Zero ? null : Marshal.PtrToStringUni(szString);
#pragma warning disable VSTHRD002
            browser!.ChangeBrowsePositionAsync(dwBrowseDirection, position, CancellationToken.None).GetAwaiter().GetResult();
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
    private static int BrowseOPCAreas(IntPtr pThis, int dwBrowseFilterType, IntPtr szFilterCriteria, IntPtr* ppIEnumString)
    {
        WriteNull(ppIEnumString);
        if (ppIEnumString == null)
        {
            return E_INVALIDARG;
        }
        if (!TryResolveBrowser(pThis, out IOpcAeAreaBrowserDispatcher? browser))
        {
            return E_FAIL;
        }
        try
        {
            string filterCriteria = szFilterCriteria == IntPtr.Zero
                ? string.Empty
                : Marshal.PtrToStringUni(szFilterCriteria) ?? string.Empty;
#pragma warning disable VSTHRD002
            string[] names = browser!.BrowseAreasAsync(dwBrowseFilterType, filterCriteria, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppIEnumString = OpcEnumStringCcw.Create(names ?? Array.Empty<string>());
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int GetQualifiedAreaName(IntPtr pThis, IntPtr szAreaName, IntPtr* pszQualifiedAreaName)
    {
        return GetQualifiedNameCore(
            pThis,
            szAreaName,
            pszQualifiedAreaName,
            static (browser, name) => browser.GetQualifiedAreaNameAsync(name, CancellationToken.None));
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int GetQualifiedSourceName(IntPtr pThis, IntPtr szSourceName, IntPtr* pszQualifiedSourceName)
    {
        return GetQualifiedNameCore(
            pThis,
            szSourceName,
            pszQualifiedSourceName,
            static (browser, name) => browser.GetQualifiedSourceNameAsync(name, CancellationToken.None));
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int GetQualifiedNameCore(
        IntPtr pThis,
        IntPtr inputName,
        IntPtr* outputName,
        Func<IOpcAeAreaBrowserDispatcher, string, System.Threading.Tasks.Task<string>> resolveAsync)
    {
        WriteNull(outputName);
        if (outputName == null)
        {
            return E_INVALIDARG;
        }
        if (!TryResolveBrowser(pThis, out IOpcAeAreaBrowserDispatcher? browser))
        {
            return E_FAIL;
        }
        try
        {
            string name = inputName == IntPtr.Zero ? string.Empty : Marshal.PtrToStringUni(inputName) ?? string.Empty;
#pragma warning disable VSTHRD002
            string qualifiedName = resolveAsync(browser!, name).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *outputName = Marshal.StringToBSTR(qualifiedName ?? string.Empty);
            return S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolveBrowser(IntPtr pThis, out IOpcAeAreaBrowserDispatcher? browser)
    {
        browser = ResolveBrowser(pThis);
        return browser is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        OpcException opcEx => opcEx.ResultId.Code,
        NotImplementedException => E_NOTIMPL,
        ArgumentNullException => E_INVALIDARG,
        ArgumentException => E_INVALIDARG,
        _ => E_FAIL,
    };

    private static void WriteNull(IntPtr* pp)
    {
        if (pp != null)
        {
            *pp = IntPtr.Zero;
        }
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
        if (entry.BrowserHandle.IsAllocated)
        {
            entry.BrowserHandle.Free();
        }
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle browserHandle, IntPtr* vtable)
        {
            BrowserHandle = browserHandle;
            Vtable = vtable;
        }

        public GCHandle BrowserHandle { get; }
        public IntPtr* Vtable { get; }
        public long RefCount;
        public int Disposed;
    }
}

