// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// Method bodies for the <see cref="OpcHdaEnumConnectionsCcw"/> vtable.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaEnumConnectionsCcwMethods
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Next(IntPtr pThis, uint cConnections, OpcHdaConnectData* rgcd, uint* pcFetched)
    {
        if (pcFetched != null)
        {
            *pcFetched = 0;
        }
        if ((cConnections > 0 && rgcd == null) || (cConnections > 1 && pcFetched == null))
        {
            return OpcHdaEnumConnectionsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaEnumConnectionsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionsCcw.E_FAIL;
        }

        try
        {
            int fetched = enumerator!.Next(cConnections, rgcd);
            if (pcFetched != null)
            {
                *pcFetched = (uint)fetched;
            }
            return cConnections <= int.MaxValue && fetched == (int)cConnections
                ? OpcHdaEnumConnectionsCcw.S_OK
                : OpcHdaEnumConnectionsCcw.S_FALSE;
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
        if (!TryResolve(pThis, out OpcHdaEnumConnectionsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionsCcw.E_FAIL;
        }
        try
        {
            int skipped = enumerator!.Skip(cConnections);
            return cConnections <= int.MaxValue && skipped == (int)cConnections
                ? OpcHdaEnumConnectionsCcw.S_OK
                : OpcHdaEnumConnectionsCcw.S_FALSE;
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
        if (!TryResolve(pThis, out OpcHdaEnumConnectionsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionsCcw.E_FAIL;
        }
        try
        {
            enumerator!.Reset();
            return OpcHdaEnumConnectionsCcw.S_OK;
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
            return OpcHdaEnumConnectionsCcw.E_INVALIDARG;
        }
        if (!TryResolve(pThis, out OpcHdaEnumConnectionsEnumerator? enumerator))
        {
            return OpcHdaEnumConnectionsCcw.E_FAIL;
        }
        try
        {
            *ppEnum = OpcHdaEnumConnectionsCcw.Create(enumerator!.Clone());
            return OpcHdaEnumConnectionsCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    private static bool TryResolve(IntPtr pThis, out OpcHdaEnumConnectionsEnumerator? enumerator)
    {
        enumerator = OpcHdaEnumConnectionsCcw.ResolveEnumerator(pThis);
        return enumerator is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        ArgumentNullException => OpcHdaEnumConnectionsCcw.E_INVALIDARG,
        ArgumentException => OpcHdaEnumConnectionsCcw.E_INVALIDARG,
        ObjectDisposedException => OpcHdaEnumConnectionsCcw.E_FAIL,
        _ => OpcHdaEnumConnectionsCcw.E_FAIL,
    };
}
