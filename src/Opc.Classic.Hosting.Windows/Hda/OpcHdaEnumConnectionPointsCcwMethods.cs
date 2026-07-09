// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Method bodies for the <see cref="OpcHdaEnumConnectionPointsCcw"/> vtable.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaEnumConnectionPointsCcwMethods
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
            return OpcHdaEnumConnectionPointsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionPointsCcw.E_FAIL;
        }

        try
        {
            int fetched = enumerator!.Next(cConnections, ppCP);
            if (pcFetched != null)
            {
                *pcFetched = (uint)fetched;
            }
            return cConnections <= int.MaxValue && fetched == (int)cConnections
                ? OpcHdaEnumConnectionPointsCcw.S_OK
                : OpcHdaEnumConnectionPointsCcw.S_FALSE;
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
        if (!TryResolve(pThis, out OpcHdaEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionPointsCcw.E_FAIL;
        }
        try
        {
            int skipped = enumerator!.Skip(cConnections);
            return cConnections <= int.MaxValue && skipped == (int)cConnections
                ? OpcHdaEnumConnectionPointsCcw.S_OK
                : OpcHdaEnumConnectionPointsCcw.S_FALSE;
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
        if (!TryResolve(pThis, out OpcHdaEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionPointsCcw.E_FAIL;
        }
        try
        {
            enumerator!.Reset();
            return OpcHdaEnumConnectionPointsCcw.S_OK;
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
            return OpcHdaEnumConnectionPointsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaEnumConnectionPointsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionPointsCcw.E_FAIL;
        }
        try
        {
            *ppEnum = OpcHdaEnumConnectionPointsCcw.Create(enumerator!.Clone());
            return OpcHdaEnumConnectionPointsCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolve(IntPtr pThis, out OpcHdaEnumConnectionPointsEnumerator? enumerator)
    {
        enumerator = OpcHdaEnumConnectionPointsCcw.ResolveEnumerator(pThis);
        return enumerator is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        ArgumentNullException => OpcHdaEnumConnectionPointsCcw.E_INVALIDARG,
        ArgumentException => OpcHdaEnumConnectionPointsCcw.E_INVALIDARG,
        ObjectDisposedException => OpcHdaEnumConnectionPointsCcw.E_FAIL,
        _ => OpcHdaEnumConnectionPointsCcw.E_FAIL,
    };
}
