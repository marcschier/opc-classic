//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting.Windows;

/// <summary>
/// IConnectionPoint and IConnectionPointContainer method bodies for HDA data callbacks.
/// </summary>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcHdaServerCcwConnectionPointMethods
{
    [UnmanagedCallersOnly]
    public static int GetConnectionInterface(IntPtr pThis, Guid* piid)
    {
        _ = pThis;
        if (piid == null)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        *piid = IOPCHDA_DataCallback.InterfaceId;
        return OpcHdaServerCcw.S_OK;
    }

    [UnmanagedCallersOnly]
    public static int GetConnectionPointContainer(IntPtr pThis, IntPtr* ppCPC)
    {
        ZeroOut(ppCPC);
        if (ppCPC == null)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        OpcHdaServerCcw.CcwSession? session = OpcHdaServerCcw.ResolveSession(pThis);
        return session is null
            ? OpcHdaServerCcw.E_FAIL
            : OpcHdaServerCcw.ReturnTearoff(session, session.ConnectionPointContainerTearoff, ppCPC);
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Advise(IntPtr pThis, IntPtr pUnk, uint* pdwCookie)
    {
        if (pdwCookie != null)
        {
            *pdwCookie = 0;
        }
        if (pdwCookie == null || pUnk == IntPtr.Zero)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        OpcHdaServerCcw.CcwSession? session = OpcHdaServerCcw.ResolveSession(pThis);
        if (session is null)
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        OpcHdaCallbackProxy? proxy = null;
        try
        {
            proxy = new OpcHdaCallbackProxy(pUnk);
            int cookie = Interlocked.Increment(ref session.NextScmSinkCookie);
            if (!session.ScmSinks.TryAdd(cookie, proxy))
            {
                proxy.Dispose();
                return OpcHdaServerCcw.E_FAIL;
            }

            proxy = null;
            *pdwCookie = unchecked((uint)cookie);
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            proxy?.Dispose();
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Unadvise(IntPtr pThis, uint dwCookie)
    {
        OpcHdaServerCcw.CcwSession? session = OpcHdaServerCcw.ResolveSession(pThis);
        if (session is null)
        {
            return OpcHdaServerCcw.E_FAIL;
        }

        try
        {
            int cookie = unchecked((int)dwCookie);
            if (!session.ScmSinks.TryRemove(cookie, out OpcHdaCallbackProxy? proxy))
            {
                return OpcHdaServerCcw.CONNECT_E_NOCONNECTION;
            }

            proxy.Dispose();
            return OpcHdaServerCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int EnumConnections(IntPtr pThis, IntPtr* ppEnum)
    {
        _ = pThis;
        ZeroOut(ppEnum);
        return ppEnum == null ? OpcHdaServerCcw.E_INVALIDARG : OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int EnumConnectionPoints(IntPtr pThis, IntPtr* ppEnum)
    {
        _ = pThis;
        ZeroOut(ppEnum);
        return ppEnum == null ? OpcHdaServerCcw.E_INVALIDARG : OpcHdaServerCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int FindConnectionPoint(IntPtr pThis, Guid* riid, IntPtr* ppCP)
    {
        ZeroOut(ppCP);
        if (riid == null || ppCP == null)
        {
            return OpcHdaServerCcw.E_INVALIDARG;
        }

        OpcHdaServerCcw.CcwSession? session = OpcHdaServerCcw.ResolveSession(pThis);
        if (session is null)
        {
            return OpcHdaServerCcw.E_FAIL;
        }
        if (*riid != IOPCHDA_DataCallback.InterfaceId)
        {
            return OpcHdaServerCcw.E_NOINTERFACE;
        }

        return OpcHdaServerCcw.ReturnTearoff(session, session.ConnectionPointTearoff, ppCP);
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        ArgumentException => OpcHdaServerCcw.E_INVALIDARG,
        ObjectDisposedException => OpcHdaServerCcw.E_FAIL,
        _ => OpcHdaServerCcw.E_FAIL,
    };

    private static void ZeroOut(IntPtr* ppv)
    {
        if (ppv != null)
        {
            *ppv = IntPtr.Zero;
        }
    }
}
