//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// IOPCSyncIO(2) method bodies bound into the <see cref="OpcDaGroupCcw"/> vtables.
/// </summary>
/// <remarks>
/// These slots intentionally return <c>E_NOTIMPL</c> for the MVP because native
/// <c>OPCITEMSTATE[]</c>, <c>OPCITEMVQT[]</c>, and <c>VARIANT[]</c> marshaling is
/// a follow-up. The vtable shape is present so COM clients see the correct QI
/// contract while value-bearing marshaling remains out of scope.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcDaGroupCcwSyncIoMethods
{
    [UnmanagedCallersOnly]
    public static int Read(IntPtr pThis, uint dwSource, uint dwCount, IntPtr phServer, IntPtr* ppItemValues, IntPtr* ppErrors)
    {
        ZeroOut(ppItemValues);
        ZeroOut(ppErrors);
        _ = (pThis, dwSource, dwCount, phServer);
        // Full OPCITEMSTATE[] OUT marshaling requires VARIANT array support and is deferred.
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int Write(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr pItemValues, IntPtr* ppErrors)
    {
        ZeroOut(ppErrors);
        _ = (pThis, dwCount, phServer, pItemValues);
        // VARIANT[] IN marshaling is intentionally deferred for the MVP.
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int ReadMaxAge(
        IntPtr pThis,
        uint dwCount,
        IntPtr phServer,
        IntPtr pdwMaxAge,
        IntPtr* ppvValues,
        IntPtr* ppwQualities,
        IntPtr* ppftTimeStamps,
        IntPtr* ppErrors)
    {
        ZeroOut(ppvValues);
        ZeroOut(ppwQualities);
        ZeroOut(ppftTimeStamps);
        ZeroOut(ppErrors);
        _ = (pThis, dwCount, phServer, pdwMaxAge);
        // VARIANT[] OUT marshaling for values is intentionally deferred for the MVP.
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int WriteVqt(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr pItemVqt, IntPtr* ppErrors)
    {
        ZeroOut(ppErrors);
        _ = (pThis, dwCount, phServer, pItemVqt);
        // OPCITEMVQT contains VARIANT values; native marshaling is deferred for the MVP.
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    private static void ZeroOut(IntPtr* ppv)
    {
        if (ppv != null)
        {
            *ppv = IntPtr.Zero;
        }
    }
}
