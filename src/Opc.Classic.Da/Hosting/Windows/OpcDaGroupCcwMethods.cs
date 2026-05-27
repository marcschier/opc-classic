//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// IOPCGroupStateMgt(2) + IOPCItemMgt method bodies bound into the
/// <see cref="OpcDaGroupCcw"/> vtables.
/// </summary>
/// <remarks>
/// Methods follow the convention: validate pThis &#x2192; resolve managed
/// <see cref="OpcDaGroup"/> via <see cref="OpcDaGroupCcw.ResolveGroup"/>
/// &#x2192; sync-bridge to the managed async method &#x2192; map exceptions
/// to OPC HRESULTs. Complex interface-pointer-returning methods (CloneGroup,
/// CreateEnumerator) and OPCITEMDEF-marshaling methods (AddItems,
/// ValidateItems) return E_NOTIMPL until full COM marshaling is wired.
/// </remarks>
[SupportedOSPlatform("windows")]
internal static unsafe class OpcDaGroupCcwMethods
{
    // ===== IOPCGroupStateMgt =====

    [UnmanagedCallersOnly]
    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating fixed-size LPWSTR via AllocCoTaskMem; size passed in bytes.")]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Cross-unmanaged-boundary catch: any escaping managed exception would crash the process.")]
    public static int GetState(
        IntPtr pThis,
        IntPtr pUpdateRate,
        IntPtr ppActive,
        IntPtr ppName,
        IntPtr ppTimeBias,
        IntPtr ppPercentDeadband,
        IntPtr pLcid)
    {
        // The actual IOPCGroupStateMgt::GetState signature is:
        //   HRESULT GetState(DWORD* pUpdateRate, BOOL* pActive, LPWSTR* ppName,
        //                    LONG* pTimeBias, FLOAT* pPercentDeadband, DWORD* pLCID,
        //                    DWORD* phClientGroup, DWORD* phServerGroup)
        // 8 OUT params total; the simplified signature here writes the first 6 +
        // accepts the last 2 via the same IntPtr*-channel convention. We use IntPtr
        // (rather than typed pointers) and Marshal.* writes for ABI portability.
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            OpcGroupState state = group!.GetStateAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            WriteUInt32(pUpdateRate, (uint)state.UpdateRate);
            WriteInt32(ppActive, state.Active ? 1 : 0);
            WriteLpwStrPtr(ppName, state.Name);
            WriteInt32(ppTimeBias, state.TimeBias);
            WriteFloat(ppPercentDeadband, state.PercentDeadband);
            WriteUInt32(pLcid, (uint)state.LocaleId);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetState(
        IntPtr pThis,
        IntPtr pRequestedUpdateRate,
        int bActive,
        IntPtr pTimeBias,
        IntPtr pPercentDeadband,
        IntPtr pLCID,
        IntPtr phClientGroup,
        IntPtr pRevisedUpdateRate)
    {
        // Signature: SetState(DWORD* pRequestedUpdateRate, DWORD* pRevisedUpdateRate,
        //                    BOOL bActive, LONG* pTimeBias, FLOAT* pPercentDeadband,
        //                    DWORD* pLCID, DWORD* phClientGroup)
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            int requestedRate = pRequestedUpdateRate == IntPtr.Zero ? group!.UpdateRate : Marshal.ReadInt32(pRequestedUpdateRate);
            int timeBias = pTimeBias == IntPtr.Zero ? group!.TimeBias : Marshal.ReadInt32(pTimeBias);
            float percentDeadband = pPercentDeadband == IntPtr.Zero ? group!.PercentDeadband : ReadFloat(pPercentDeadband);
            int lcid = pLCID == IntPtr.Zero ? group!.LocaleId : Marshal.ReadInt32(pLCID);
            int clientHandle = phClientGroup == IntPtr.Zero ? group!.ClientHandle : Marshal.ReadInt32(phClientGroup);

#pragma warning disable VSTHRD002
            group!.SetStateAsync(requestedRate, bActive != 0, timeBias, percentDeadband, lcid, clientHandle, out int revisedRate, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            if (pRevisedUpdateRate != IntPtr.Zero)
            {
                Marshal.WriteInt32(pRevisedUpdateRate, revisedRate);
            }
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetName(IntPtr pThis, IntPtr szName)
    {
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        if (szName == IntPtr.Zero)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        try
        {
            string name = Marshal.PtrToStringUni(szName) ?? string.Empty;
#pragma warning disable VSTHRD002
            group!.SetNameAsync(name, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int CloneGroup(IntPtr pThis, IntPtr szName, Guid* riid, IntPtr* ppUnk)
    {
        // Returning a CCW interface pointer requires allocating a second
        // OpcDaGroupCcw for the cloned group; deferred to a follow-up.
        _ = pThis; _ = szName; _ = riid;
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    // ===== IOPCGroupStateMgt2 =====

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetKeepAlive(IntPtr pThis, int keepAliveTime, IntPtr pRevisedKeepAliveTime)
    {
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            int previous = group!.SetKeepAliveAsync(keepAliveTime, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            _ = previous;
            if (pRevisedKeepAliveTime != IntPtr.Zero)
            {
                Marshal.WriteInt32(pRevisedKeepAliveTime, keepAliveTime);
            }
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int GetKeepAlive(IntPtr pThis, IntPtr pKeepAliveTime)
    {
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
#pragma warning disable VSTHRD002
            int keepAlive = group!.GetKeepAliveAsync(CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            if (pKeepAliveTime != IntPtr.Zero)
            {
                Marshal.WriteInt32(pKeepAliveTime, keepAlive);
            }
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    // ===== IOPCItemMgt =====

    [UnmanagedCallersOnly]
    public static int AddItems(IntPtr pThis, uint dwCount, IntPtr pItemArray, IntPtr* ppAddResults, IntPtr* ppErrors)
    {
        // OPCITEMDEF array marshaling deferred.
        _ = pThis; _ = dwCount; _ = pItemArray;
        if (ppAddResults != null)
        {
            *ppAddResults = IntPtr.Zero;
        }
        if (ppErrors != null)
        {
            *ppErrors = IntPtr.Zero;
        }
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    public static int ValidateItems(IntPtr pThis, uint dwCount, IntPtr pItemArray, int bBlobUpdate, IntPtr* ppValidationResults, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwCount; _ = pItemArray; _ = bBlobUpdate;
        if (ppValidationResults != null)
        {
            *ppValidationResults = IntPtr.Zero;
        }
        if (ppErrors != null)
        {
            *ppErrors = IntPtr.Zero;
        }
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int RemoveItems(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr* ppErrors)
    {
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        if (ppErrors == null || (dwCount > 0 && phServer == IntPtr.Zero))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        try
        {
            int[] handles = ReadInt32Array(phServer, (int)dwCount);
#pragma warning disable VSTHRD002
            int[] errors = group!.RemoveItemsAsync(handles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppErrors = AllocateInt32Array(errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetActiveState(IntPtr pThis, uint dwCount, IntPtr phServer, int bActive, IntPtr* ppErrors)
    {
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        if (ppErrors == null || (dwCount > 0 && phServer == IntPtr.Zero))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        try
        {
            int[] handles = ReadInt32Array(phServer, (int)dwCount);
#pragma warning disable VSTHRD002
            int[] errors = group!.SetActiveStateAsync(handles, bActive != 0, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppErrors = AllocateInt32Array(errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetClientHandles(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr phClient, IntPtr* ppErrors)
    {
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        if (ppErrors == null || (dwCount > 0 && (phServer == IntPtr.Zero || phClient == IntPtr.Zero)))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        try
        {
            int[] handles = ReadInt32Array(phServer, (int)dwCount);
            int[] clientHandles = ReadInt32Array(phClient, (int)dwCount);
#pragma warning disable VSTHRD002
            int[] errors = group!.SetClientHandlesAsync(handles, clientHandles, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppErrors = AllocateInt32Array(errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int SetDatatypes(IntPtr pThis, uint dwCount, IntPtr phServer, IntPtr pRequestedDatatypes, IntPtr* ppErrors)
    {
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        if (ppErrors == null || (dwCount > 0 && (phServer == IntPtr.Zero || pRequestedDatatypes == IntPtr.Zero)))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        try
        {
            int[] handles = ReadInt32Array(phServer, (int)dwCount);
            ushort[] datatypes = ReadUInt16Array(pRequestedDatatypes, (int)dwCount);
#pragma warning disable VSTHRD002
            int[] errors = group!.SetDatatypesAsync(handles, datatypes, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppErrors = AllocateInt32Array(errors);
            return OpcDaGroupCcw.S_OK;
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    [UnmanagedCallersOnly]
    public static int CreateEnumerator(IntPtr pThis, Guid* riid, IntPtr* ppUnk)
    {
        _ = pThis; _ = riid;
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        return OpcDaGroupCcw.E_NOTIMPL;
    }

    // ===== Helpers =====

    private static readonly TryResolve s_groupResolve = TryResolveGroup;

    private delegate bool TryResolve(IntPtr pThis, out OpcDaGroup? group);

    private static bool TryResolveGroup(IntPtr pThis, out OpcDaGroup? group)
    {
        group = OpcDaGroupCcw.ResolveGroup(pThis);
        return group is not null;
    }

    private static int MapHResult(Exception ex) => ex switch
    {
        OpcException opcEx => opcEx.ResultId.Code,
        ArgumentNullException => OpcDaGroupCcw.E_INVALIDARG,
        ArgumentException => OpcDaGroupCcw.E_INVALIDARG,
        _ => OpcDaGroupCcw.E_FAIL,
    };

    private static void WriteUInt32(IntPtr p, uint v)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt32(p, unchecked((int)v));
        }
    }

    private static void WriteInt32(IntPtr p, int v)
    {
        if (p != IntPtr.Zero)
        {
            Marshal.WriteInt32(p, v);
        }
    }

    private static void WriteFloat(IntPtr p, float v)
    {
        if (p == IntPtr.Zero)
        {
            return;
        }
        int bits = BitConverter.SingleToInt32Bits(v);
        Marshal.WriteInt32(p, bits);
    }

    private static float ReadFloat(IntPtr p) => BitConverter.Int32BitsToSingle(Marshal.ReadInt32(p));

    private static void WriteLpwStrPtr(IntPtr ppwzOut, string? value)
    {
        if (ppwzOut == IntPtr.Zero)
        {
            return;
        }
        IntPtr s = AllocateLpwStr(value);
        Marshal.WriteIntPtr(ppwzOut, s);
    }

    private static IntPtr AllocateLpwStr(string? value)
    {
        if (value is null)
        {
            return IntPtr.Zero;
        }
        int byteCount = (value.Length + 1) * sizeof(char);
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        Marshal.Copy(value.ToCharArray(), 0, ptr, value.Length);
        Marshal.WriteInt16(ptr, value.Length * sizeof(char), 0);
        return ptr;
    }

    private static int[] ReadInt32Array(IntPtr ptr, int count)
    {
        var array = new int[count];
        if (count > 0)
        {
            Marshal.Copy(ptr, array, 0, count);
        }
        return array;
    }

    private static ushort[] ReadUInt16Array(IntPtr ptr, int count)
    {
        var array = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            array[i] = (ushort)Marshal.ReadInt16(ptr, i * sizeof(ushort));
        }
        return array;
    }

    private static IntPtr AllocateInt32Array(int[] values)
    {
        int byteCount = Math.Max(1, values.Length * sizeof(int));
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        if (values.Length > 0)
        {
            Marshal.Copy(values, 0, ptr, values.Length);
        }
        return ptr;
    }
}
