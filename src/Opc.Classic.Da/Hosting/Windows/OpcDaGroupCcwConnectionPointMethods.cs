//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// IConnectionPoint and IConnectionPointContainer method bodies bound into the
/// <see cref="OpcDaGroupCcw"/> vtables.
/// </summary>
/// <remarks>
/// Windows SCM callback sinks are held as <see cref="OpcDataCallbackProxy"/>
/// instances on the CCW session and intentionally do not populate the managed
/// transport subscription dictionary.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcDaGroupCcwConnectionPointMethods
{
    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetConnectionInterface(IntPtr pThis, Guid* piid)
    {
        if (piid == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            *piid = group!.GetConnectionInterfaceAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int GetConnectionPointContainer(IntPtr pThis, IntPtr* ppCPC)
    {
        if (ppCPC != null)
        {
            *ppCPC = IntPtr.Zero;
        }
        if (ppCPC == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        OpcDaGroupCcw.CcwSession? session = OpcDaGroupCcw.ResolveSession(pThis);
        return session is null
            ? OpcDaGroupCcw.E_FAIL
            : OpcDaGroupCcw.ReturnTearoff(session, session.ConnectionPointContainerTearoff, ppCPC);
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
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        OpcDaGroupCcw.CcwSession? session = OpcDaGroupCcw.ResolveSession(pThis);
        if (session is null)
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        return AdviseCore(session, pUnk, pdwCookie);
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int Unadvise(IntPtr pThis, uint dwCookie)
    {
        OpcDaGroupCcw.CcwSession? session = OpcDaGroupCcw.ResolveSession(pThis);
        if (session is null)
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            int cookie = unchecked((int)dwCookie);
            if (!session.ScmSinks.TryRemove(cookie, out OpcDataCallbackProxy? proxy))
            {
                return OpcDaGroupCcw.CONNECT_E_NOCONNECTION;
            }
            // cap-c8: also remove from the managed OpcDaGroup's _directSinks
            // so trigger fan-out stops invoking the disposed proxy.
            OpcDaGroup? group = session.GroupHandle.Target as OpcDaGroup;
            if (group is not null)
            {
                try
                {
#pragma warning disable VSTHRD002
                    group.UnadviseAsync(cookie, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
                }
                catch (OpcException)
                {
                    // Already unregistered (e.g. group disposed) — proceed with proxy.Dispose anyway.
                }
            }
            proxy.Dispose();
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int EnumConnections(IntPtr pThis, IntPtr* ppEnum)
    {
        ZeroOut(ppEnum);
        _ = pThis;
        // IEnumConnections CCW infrastructure is intentionally deferred for the MVP.
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int EnumConnectionPoints(IntPtr pThis, IntPtr* ppEnum)
    {
        ZeroOut(ppEnum);
        _ = pThis;
        // IEnumConnectionPoints CCW infrastructure is intentionally deferred for the MVP.
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int FindConnectionPoint(IntPtr pThis, Guid* riid, IntPtr* ppCP)
    {
        ZeroOut(ppCP);
        if (riid == null || ppCP == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        OpcDaGroupCcw.CcwSession? session = OpcDaGroupCcw.ResolveSession(pThis);
        if (session is null || !TryResolveGroup(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            _ = group!.FindConnectionPointAsync(*riid, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcDaGroupCcw.ReturnTearoff(session, session.ConnectionPointTearoff, ppCP);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    private static int AdviseCore(OpcDaGroupCcw.CcwSession session, IntPtr pUnk, uint* pdwCookie)
    {
        OpcDataCallbackProxy? proxy = null;
        try
        {
            proxy = new OpcDataCallbackProxy(pUnk);
            // cap-c8: register the proxy with the managed OpcDaGroup as an
            // IOpcDataCallbackSink so TriggerDataChangeAsync /
            // TriggerCancelCompleteAsync fan-out reaches the SCM-activated
            // client. Use the cookie returned by the group; share between
            // _directSinks (managed) and CcwSession.ScmSinks (CCW lifecycle).
            OpcDaGroup? group = session.GroupHandle.Target as OpcDaGroup;
            int cookie;
            if (group is not null)
            {
#pragma warning disable VSTHRD002
                cookie = group.AdviseAsync((IOpcDataCallbackSink)proxy, CancellationToken.None)
                    .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            }
            else
            {
                cookie = Interlocked.Increment(ref session.NextScmSinkCookie);
            }
            if (!session.ScmSinks.TryAdd(cookie, proxy))
            {
                proxy.Dispose();
                return OpcDaGroupCcw.E_FAIL;
            }
            proxy = null;
            *pdwCookie = unchecked((uint)cookie);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            proxy?.Dispose();
            return MapHResult(ex);
        }
    }

    private static bool TryResolveGroup(IntPtr pThis, out OpcDaGroup? group)
    {
        group = OpcDaGroupCcw.ResolveGroup(pThis);
        return group is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        COMException comEx => comEx.ErrorCode,
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentNullException => OpcDaGroupCcw.E_INVALIDARG,
        ArgumentException => OpcDaGroupCcw.E_INVALIDARG,
        _ => OpcDaGroupCcw.E_FAIL,
    };

    private static void ZeroOut(IntPtr* ppv)
    {
        if (ppv != null)
        {
            *ppv = IntPtr.Zero;
        }
    }
}
