//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Method bodies for the <see cref="OpcEnumConnectionPointsCcw"/> vtable.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcEnumConnectionPointsCcwMethods
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Next(IntPtr pThis, uint cConnections, IntPtr* ppCP, uint* pcFetched)
    {
        if (pcFetched != null)
        {
            *pcFetched = 0;
        }
        if ((cConnections > 0 && ppCP == null) || (cConnections > 1 && pcFetched == null))
        {
            return OpcEnumConnectionPointsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcEnumConnectionPointsCcw.E_FAIL;
        }

        try
        {
            int fetched = enumerator!.Next(cConnections, ppCP);
            if (pcFetched != null)
            {
                *pcFetched = (uint)fetched;
            }
            return cConnections <= int.MaxValue && fetched == (int)cConnections
                ? OpcEnumConnectionPointsCcw.S_OK
                : OpcEnumConnectionPointsCcw.S_FALSE;
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
        if (!TryResolve(pThis, out OpcEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcEnumConnectionPointsCcw.E_FAIL;
        }
        try
        {
            int skipped = enumerator!.Skip(cConnections);
            return cConnections <= int.MaxValue && skipped == (int)cConnections
                ? OpcEnumConnectionPointsCcw.S_OK
                : OpcEnumConnectionPointsCcw.S_FALSE;
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
        if (!TryResolve(pThis, out OpcEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcEnumConnectionPointsCcw.E_FAIL;
        }
        try
        {
            enumerator!.Reset();
            return OpcEnumConnectionPointsCcw.S_OK;
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
            return OpcEnumConnectionPointsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcEnumConnectionPointsCcw.E_FAIL;
        }
        try
        {
            *ppEnum = OpcEnumConnectionPointsCcw.Create(enumerator!.Clone());
            return OpcEnumConnectionPointsCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolve(IntPtr pThis, out OpcEnumConnectionPointsEnumerator? enumerator)
    {
        enumerator = OpcEnumConnectionPointsCcw.ResolveEnumerator(pThis);
        return enumerator is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        ArgumentNullException => OpcEnumConnectionPointsCcw.E_INVALIDARG,
        ArgumentException => OpcEnumConnectionPointsCcw.E_INVALIDARG,
        ObjectDisposedException => OpcEnumConnectionPointsCcw.E_FAIL,
        _ => OpcEnumConnectionPointsCcw.E_FAIL,
    };
}
