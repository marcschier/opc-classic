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
/// to OPC HRESULTs. OPCITEMDEF arrays and interface-pointer-returning
/// methods allocate COM-owned memory with CoTaskMemAlloc-compatible helpers.
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int CloneGroup(IntPtr pThis, IntPtr szName, Guid* riid, IntPtr* ppUnk)
    {
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        if (ppUnk == null || riid == null || szName == IntPtr.Zero)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            string name = Marshal.PtrToStringUni(szName) ?? string.Empty;
            OpcDaGroup clone = CloneGroupForCcw(group!, name);
            IntPtr ccw = OpcDaGroupCcw.Create(clone);
            return ReturnRequestedInterfacePointer(ccw, riid, ppUnk);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int AddItems(IntPtr pThis, uint dwCount, IntPtr pItemArray, IntPtr* ppAddResults, IntPtr* ppErrors)
    {
        ZeroItemResultOuts(ppAddResults, ppErrors);
        if (!HasValidItemArrayArguments(dwCount, pItemArray, ppAddResults, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            OpcItemDef[] defs = ReadItemDefinitions(pItemArray, dwCount);
#pragma warning disable VSTHRD002
            group!.AddItemsAsync(defs, out OpcItemResult[] results, out int[] errors, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppAddResults = AllocateOpcItemResultArray(results);
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
    public static int ValidateItems(IntPtr pThis, uint dwCount, IntPtr pItemArray, int bBlobUpdate, IntPtr* ppValidationResults, IntPtr* ppErrors)
    {
        ZeroItemResultOuts(ppValidationResults, ppErrors);
        if (!HasValidItemArrayArguments(dwCount, pItemArray, ppValidationResults, ppErrors))
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            OpcItemDef[] defs = ReadItemDefinitions(pItemArray, dwCount);
#pragma warning disable VSTHRD002
            group!.ValidateItemsAsync(defs, bBlobUpdate != 0, out OpcItemResult[] results, out int[] errors, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppValidationResults = AllocateOpcItemResultArray(results);
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
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Cross-unmanaged-boundary catch.")]
    public static int CreateEnumerator(IntPtr pThis, Guid* riid, IntPtr* ppUnk)
    {
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        if (ppUnk == null || riid == null)
        {
            return OpcDaGroupCcw.E_INVALIDARG;
        }
        if (!s_groupResolve(pThis, out OpcDaGroup? group))
        {
            return OpcDaGroupCcw.E_FAIL;
        }
        try
        {
            var enumerator = new OpcDaItemAttributesEnumerator(group!.BuildItemAttributesSnapshot(), registry: null);
            IntPtr ccw = OpcEnumOpcItemAttributesCcw.Create(enumerator);
            return ReturnRequestedInterfacePointer(ccw, riid, ppUnk);
        }
        catch (Exception ex)
        {
            return MapHResult(ex);
        }
    }

    // ===== Helpers =====

    // Native OPCITEMDEF/OPCITEMRESULT use natural alignment (no Pack)
    // so pointer fields land on 8-byte boundaries on x64, matching MIDL's
    // default. A packed layout caused Windows DCOM's proxy/stub to read
    // garbage when marshalling AddItems results, closing the wire
    // connection mid-call (manifests as RPC_S_CALL_FAILED on the client).
    [StructLayout(LayoutKind.Sequential)]
    private struct OPCITEMDEF_NATIVE
    {
        public IntPtr szAccessPath;
        public IntPtr szItemID;
        public int bActive;
        public uint hClient;
        public uint dwBlobSize;
        public IntPtr pBlob;
        public ushort vtRequestedDataType;
        public ushort wReserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct OPCITEMRESULT_NATIVE
    {
        public uint hServer;
        public ushort vtCanonicalDataType;
        public ushort wReserved;
        public uint dwAccessRights;
        public uint dwBlobSize;
        public IntPtr pBlob;
    }

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

    private static bool HasValidItemArrayArguments(uint count, IntPtr items, IntPtr* ppResults, IntPtr* ppErrors) =>
        ppResults != null && ppErrors != null && count <= int.MaxValue && (count == 0 || items != IntPtr.Zero);

    private static void ZeroItemResultOuts(IntPtr* ppResults, IntPtr* ppErrors)
    {
        if (ppResults != null)
        {
            *ppResults = IntPtr.Zero;
        }
        if (ppErrors != null)
        {
            *ppErrors = IntPtr.Zero;
        }
    }

    private static OpcItemDef[] ReadItemDefinitions(IntPtr pItemArray, uint count)
    {
        int itemCount = checked((int)count);
        var defs = new OpcItemDef[itemCount];
        int size = Marshal.SizeOf<OPCITEMDEF_NATIVE>();
        for (int i = 0; i < itemCount; i++)
        {
            var native = Marshal.PtrToStructure<OPCITEMDEF_NATIVE>(IntPtr.Add(pItemArray, i * size));
            defs[i] = new OpcItemDef(
                Marshal.PtrToStringUni(native.szAccessPath),
                Marshal.PtrToStringUni(native.szItemID),
                native.bActive != 0,
                unchecked((int)native.hClient),
                ReadBlob(native.pBlob, native.dwBlobSize),
                (VarType)native.vtRequestedDataType);
        }
        return defs;
    }

    private static byte[] ReadBlob(IntPtr pBlob, uint blobSize)
    {
        if (blobSize == 0)
        {
            return Array.Empty<byte>();
        }
        if (pBlob == IntPtr.Zero || blobSize > int.MaxValue)
        {
            throw new ArgumentException("Invalid OPC item blob pointer or size.", nameof(blobSize));
        }
        var blob = new byte[(int)blobSize];
        Marshal.Copy(pBlob, blob, 0, blob.Length);
        return blob;
    }

    private static IntPtr AllocateOpcItemResultArray(OpcItemResult[] results)
    {
        int size = Marshal.SizeOf<OPCITEMRESULT_NATIVE>();
        int byteCount = Math.Max(1, checked(results.Length * size));
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        for (int i = 0; i < results.Length; i++)
        {
            Marshal.StructureToPtr(ToNative(results[i]), IntPtr.Add(ptr, i * size), fDeleteOld: false);
        }
        return ptr;
    }

    private static OPCITEMRESULT_NATIVE ToNative(OpcItemResult result)
    {
        byte[] blob = result.Blob ?? Array.Empty<byte>();
        return new OPCITEMRESULT_NATIVE
        {
            hServer = unchecked((uint)result.ServerHandle),
            vtCanonicalDataType = (ushort)result.CanonicalDataType,
            wReserved = 0,
            dwAccessRights = unchecked((uint)result.AccessRights),
            dwBlobSize = unchecked((uint)blob.Length),
            pBlob = AllocateBlob(blob),
        };
    }

    private static IntPtr AllocateBlob(byte[] blob)
    {
        if (blob.Length == 0)
        {
            return IntPtr.Zero;
        }
        IntPtr ptr = Marshal.AllocCoTaskMem(blob.Length);
        Marshal.Copy(blob, 0, ptr, blob.Length);
        return ptr;
    }

    private static OpcDaGroup CloneGroupForCcw(OpcDaGroup source, string name)
    {
        // This clone is CCW-scope only; the current managed CloneGroupAsync surface returns a synthetic IOpcInterfaceRef.
        var clone = new OpcDaGroup(name, Random.Shared.Next(int.MaxValue / 2, int.MaxValue), source.ClientHandle,
            source.Active, source.UpdateRate, source.TimeBias, source.PercentDeadband, source.LocaleId);
        var defs = new OpcItemDef[source.Items.Count];
        int index = 0;
        foreach (OpcDaItem item in source.Items)
        {
            defs[index++] = new OpcItemDef(item.AccessPath, item.ItemId, item.Active, item.ClientHandle,
                Array.Empty<byte>(), (VarType)item.RequestedDatatype);
        }
#pragma warning disable VSTHRD002
        clone.AddItemsAsync(defs, out OpcItemResult[] _, out int[] _, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        return clone;
    }

    private static int ReturnRequestedInterfacePointer(IntPtr ccw, Guid* riid, IntPtr* ppUnk)
    {
        if (*riid == OpcDaGroupCcw.IID_IUnknown)
        {
            *ppUnk = ccw;
            return OpcDaGroupCcw.S_OK;
        }
        IntPtr* vtable = *(IntPtr**)ccw;
        var queryInterface = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
        IntPtr requested;
        int hr = queryInterface(ccw, riid, &requested);
        ReleaseInterfacePointer(ccw);
        if (hr == OpcDaGroupCcw.S_OK)
        {
            *ppUnk = requested;
        }
        return hr;
    }

    private static void ReleaseInterfacePointer(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
        release(ccw);
    }

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
