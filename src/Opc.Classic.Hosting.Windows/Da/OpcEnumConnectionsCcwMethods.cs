// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Method bodies for the <see cref="OpcEnumConnectionsCcw"/> vtable.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcEnumConnectionsCcwMethods
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Next(IntPtr pThis, uint cConnections, OpcConnectData* rgcd, uint* pcFetched)
    {
        if (pcFetched != null)
        {
            *pcFetched = 0;
        }
        if ((cConnections > 0 && rgcd == null) || (cConnections > 1 && pcFetched == null))
        {
            return OpcEnumConnectionsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcEnumConnectionsEnumerator? enumerator))
        {
            return OpcEnumConnectionsCcw.E_FAIL;
        }

        try
        {
            int fetched = enumerator!.Next(cConnections, rgcd);
            if (pcFetched != null)
            {
                *pcFetched = (uint)fetched;
            }
            return cConnections <= int.MaxValue && fetched == (int)cConnections
                ? OpcEnumConnectionsCcw.S_OK
                : OpcEnumConnectionsCcw.S_FALSE;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Skip(IntPtr pThis, uint cConnections)
    {
        if (!TryResolve(pThis, out OpcEnumConnectionsEnumerator? enumerator))
        {
            return OpcEnumConnectionsCcw.E_FAIL;
        }
        try
        {
            int skipped = enumerator!.Skip(cConnections);
            return cConnections <= int.MaxValue && skipped == (int)cConnections
                ? OpcEnumConnectionsCcw.S_OK
                : OpcEnumConnectionsCcw.S_FALSE;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Reset(IntPtr pThis)
    {
        if (!TryResolve(pThis, out OpcEnumConnectionsEnumerator? enumerator))
        {
            return OpcEnumConnectionsCcw.E_FAIL;
        }
        try
        {
            enumerator!.Reset();
            return OpcEnumConnectionsCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Clone(IntPtr pThis, IntPtr* ppEnum)
    {
        if (ppEnum != null)
        {
            *ppEnum = IntPtr.Zero;
        }
        if (ppEnum == null)
        {
            return OpcEnumConnectionsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcEnumConnectionsEnumerator? enumerator))
        {
            return OpcEnumConnectionsCcw.E_FAIL;
        }
        try
        {
            *ppEnum = OpcEnumConnectionsCcw.Create(enumerator!.Clone());
            return OpcEnumConnectionsCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolve(IntPtr pThis, out OpcEnumConnectionsEnumerator? enumerator)
    {
        enumerator = OpcEnumConnectionsCcw.ResolveEnumerator(pThis);
        return enumerator is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        ArgumentNullException => OpcEnumConnectionsCcw.E_INVALIDARG,
        ArgumentException => OpcEnumConnectionsCcw.E_INVALIDARG,
        ObjectDisposedException => OpcEnumConnectionsCcw.E_FAIL,
        _ => OpcEnumConnectionsCcw.E_FAIL,
    };
}
