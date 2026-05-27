//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>
/// Windows-only unit tests for <see cref="OpcDaGroupCcw"/>. Covers the multi-tearoff
/// QI identity, refcount-to-zero cleanup, and the simple IOPCGroupStateMgt(2) +
/// IOPCItemMgt method bodies wired into the vtables.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcDaGroupCcwTests
{
    private const int S_OK = 0;
    private const int S_FALSE = 1;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_INVALIDARG = unchecked((int)0x80070057);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task Create_returns_nonzero_ccw_pointer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcDaGroupCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task Create_returns_distinct_ccw_per_call()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw1 = OpcDaGroupCcw.Create(NewGroup("a"));
        IntPtr ccw2 = OpcDaGroupCcw.Create(NewGroup("b"));

        await Assert.That(ccw1).IsNotEqualTo(ccw2);
    }

    [Test]
    public async Task GetReferenceCount_returns_negative_one_for_unknown_pointer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(OpcDaGroupCcw.GetReferenceCount(new IntPtr(0x12345678))).IsEqualTo(-1L);
    }

    [Test]
    public async Task QueryInterface_for_IOPCGroupStateMgt_returns_real_tearoff()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr gsmPtr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt.InterfaceId);

        await Assert.That(gsmPtr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(gsmPtr).IsNotEqualTo(ccw);
        await Assert.That(OpcDaGroupCcw.GetReferenceCount(gsmPtr)).IsEqualTo(2L);
    }

    [Test]
    public async Task QueryInterface_for_IOPCGroupStateMgt2_returns_real_tearoff()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr gsm2Ptr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt2.InterfaceId);

        await Assert.That(gsm2Ptr).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task QueryInterface_for_IOPCItemMgt_returns_real_tearoff()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);

        await Assert.That(itemMgtPtr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(itemMgtPtr).IsNotEqualTo(ccw);
    }

    [Test]
    public async Task QueryInterface_for_unsupported_iid_returns_E_NOINTERFACE()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        (int hr, IntPtr returned) = Helpers.InvokeQIRaw(ccw, Guid.NewGuid());

        await Assert.That(hr).IsEqualTo(E_NOINTERFACE);
        await Assert.That(returned).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task QueryInterface_for_IUnknown_returns_canonical_identity()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr gsmPtr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt.InterfaceId);

        // COM identity rule: QI(IUnknown) on any tearoff returns the same pointer.
        IntPtr fromGsm = Helpers.InvokeQI(gsmPtr, IID_IUnknown);
        IntPtr fromUnknown = Helpers.InvokeQI(ccw, IID_IUnknown);

        await Assert.That(fromGsm).IsEqualTo(fromUnknown);
    }

    [Test]
    public async Task Release_to_zero_frees_session_and_pointer_becomes_unknown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        Helpers.InvokeRelease(ccw);

        await Assert.That(OpcDaGroupCcw.GetReferenceCount(ccw)).IsEqualTo(-1L);
    }

    [Test]
    public async Task GetState_returns_managed_group_state_values()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup("MyGroup");
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr gsmPtr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt.InterfaceId);
        Helpers.GetStateResult result = Helpers.InvokeGetState(gsmPtr);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.UpdateRate).IsEqualTo(1000);
        await Assert.That(result.Active).IsEqualTo(1);
        await Assert.That(result.Name).IsEqualTo("MyGroup");
        await Assert.That(result.LocaleId).IsEqualTo(1033);
    }

    [Test]
    public async Task SetName_updates_managed_group_name()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup("Initial");
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr gsmPtr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt.InterfaceId);

        int hr = Helpers.InvokeSetName(gsmPtr, "Updated");

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(group.Name).IsEqualTo("Updated");
    }

    [Test]
    public async Task CloneGroup_returns_new_ccw_with_group_state_interface()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr gsmPtr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt.InterfaceId);
        Helpers.CloneGroupResult result = Helpers.InvokeCloneGroup(gsmPtr, "Clone");
        IntPtr cloneGsm = Helpers.InvokeQI(result.Pointer, IOPCGroupStateMgt.InterfaceId);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Pointer).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(cloneGsm).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task CloneGroup_copies_items_into_new_ccw_scope()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup();
        await AddManagedItems(group, "Tag.A", "Tag.B");
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr gsmPtr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt.InterfaceId);
        Helpers.CloneGroupResult clone = Helpers.InvokeCloneGroup(gsmPtr, "CloneWithItems");
        IntPtr cloneItemMgt = Helpers.InvokeQI(clone.Pointer, IOPCItemMgt.InterfaceId);
        Helpers.CreateEnumeratorResult created = Helpers.InvokeCreateEnumerator(cloneItemMgt, IEnumOPCItemAttributes.InterfaceId);
        Helpers.EnumNextResult next = Helpers.InvokeEnumNext(created.Pointer, 10);

        await Assert.That(clone.Hr).IsEqualTo(S_OK);
        await Assert.That(next.Hr).IsEqualTo(S_FALSE);
        await Assert.That(next.Fetched).IsEqualTo(2u);
    }

    [Test]
    public async Task RemoveItems_dispatches_to_managed_group()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup();
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        Helpers.RemoveItemsResult result = Helpers.InvokeRemoveItems(itemMgtPtr, new[] { 99, 100 });

        // Both handles are unknown to the empty group; expect OPC_E_INVALIDHANDLE per handle.
        int expected = OpcResultId.InvalidHandle.Code;
        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Errors[0]).IsEqualTo(expected);
        await Assert.That(result.Errors[1]).IsEqualTo(expected);
    }

    [Test]
    public async Task AddItems_marshals_one_item_and_returns_server_handle()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup();
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        Helpers.ItemMethodResult result = Helpers.InvokeAddItems(itemMgtPtr, [NewItemDef("Tag.A", 42)]);
        int managedHandle = group.Items.Single().ServerHandle;

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Results[0].ServerHandle).IsEqualTo(managedHandle);
        await Assert.That(result.Errors[0]).IsEqualTo(S_OK);
    }

    [Test]
    public async Task AddItems_zero_items_returns_empty_arrays()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        Helpers.ItemMethodResult result = Helpers.InvokeAddItems(itemMgtPtr, []);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Results.Length).IsEqualTo(0);
        await Assert.That(result.Errors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task AddItems_null_out_pointers_returns_invalidarg()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        int hr = Helpers.InvokeAddItemsWithNullOutPointers(itemMgtPtr);

        await Assert.That(hr).IsEqualTo(E_INVALIDARG);
    }

    [Test]
    public async Task AddItems_unknown_item_returns_per_item_error()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        Helpers.ItemMethodResult result = Helpers.InvokeAddItems(itemMgtPtr, [NewItemDef(string.Empty, 1)]);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Errors[0]).IsEqualTo(OpcResultId.UnknownItemId.Code);
    }

    [Test]
    public async Task ValidateItems_marshals_one_item_and_returns_validation_error()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        Helpers.ItemMethodResult result = Helpers.InvokeValidateItems(itemMgtPtr, [NewItemDef("Tag.A", 7)]);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Results[0].ServerHandle).IsEqualTo(0);
        await Assert.That(result.Errors[0]).IsEqualTo(S_OK);
    }

    [Test]
    public async Task CreateEnumerator_returns_ccw_for_requested_interface()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        Helpers.CreateEnumeratorResult result = Helpers.InvokeCreateEnumerator(itemMgtPtr, IEnumOPCItemAttributes.InterfaceId);
        IntPtr enumPtr = Helpers.InvokeQI(result.Pointer, IEnumOPCItemAttributes.InterfaceId);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Pointer).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(enumPtr).IsNotEqualTo(IntPtr.Zero);
    }

    private static OpcItemDef NewItemDef(string itemId, int clientHandle) =>
        new("", itemId, true, clientHandle, Array.Empty<byte>(), VarType.VT_I4);

    private static async Task AddManagedItems(OpcDaGroup group, params string[] itemIds)
    {
        OpcItemDef[] defs = itemIds.Select((itemId, index) => NewItemDef(itemId, index + 1)).ToArray();
        await group.AddItemsAsync(defs, out OpcItemResult[] _, out int[] _, TestContext.Current!.CancellationToken);
    }

    private static OpcDaGroup NewGroup(string name = "TestGroup") => new(
        name: name,
        serverHandle: 1,
        clientHandle: 100,
        active: true,
        requestedUpdateRate: 1000,
        timeBias: 0,
        percentDeadband: 0f,
        localeId: 1033);

    private static unsafe class Helpers
    {
        internal readonly record struct GetStateResult(int Hr, int UpdateRate, int Active, string? Name, int LocaleId);

        internal readonly record struct RemoveItemsResult(int Hr, int[] Errors);

        internal readonly record struct CloneGroupResult(int Hr, IntPtr Pointer);

        internal readonly record struct CreateEnumeratorResult(int Hr, IntPtr Pointer);

        internal readonly record struct ItemMethodResult(int Hr, NativeItemResult[] Results, int[] Errors);

        internal readonly record struct NativeItemResult(int ServerHandle, ushort CanonicalDataType, int AccessRights, int BlobSize);

        internal readonly record struct EnumNextResult(int Hr, uint Fetched, string?[] ItemIds);

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
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

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct OPCITEMRESULT_NATIVE
        {
            public uint hServer;
            public ushort vtCanonicalDataType;
            public ushort wReserved;
            public uint dwAccessRights;
            public uint dwBlobSize;
            public IntPtr pBlob;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct OPCITEMATTRIBUTES_NATIVE
        {
            public IntPtr szAccessPath;
            public IntPtr szItemID;
            public int bActive;
            public uint hClient;
            public uint hServer;
            public uint dwAccessRights;
            public uint dwBlobSize;
            public IntPtr pBlob;
            public ushort vtRequestedDataType;
            public ushort vtCanonicalDataType;
            public ushort wReserved1;
            public ushort wReserved2;
            public uint dwEUType;
            public long vEUInfo0;
            public long vEUInfo1;
        }

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var qi = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
            Guid local = iid;
            IntPtr returned;
            int hr = qi(ccw, &local, &returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static (int Hr, IntPtr Returned) InvokeQIRaw(IntPtr ccw, Guid iid)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var qi = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
            Guid local = iid;
            IntPtr returned;
            int hr = qi(ccw, &local, &returned);
            return (hr, returned);
        }

        internal static void InvokeRelease(IntPtr ccw)
        {
            IntPtr* vtable = *(IntPtr**)ccw;
            var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
            release(ccw);
        }

        internal static GetStateResult InvokeGetState(IntPtr gsmPtr)
        {
            IntPtr* vtable = *(IntPtr**)gsmPtr;
            var getState = (delegate* unmanaged<IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)vtable[3];

            IntPtr pRate = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pActive = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr ppName = Marshal.AllocCoTaskMem(IntPtr.Size);
            IntPtr pTimeBias = Marshal.AllocCoTaskMem(sizeof(int));
            IntPtr pDeadband = Marshal.AllocCoTaskMem(sizeof(float));
            IntPtr pLcid = Marshal.AllocCoTaskMem(sizeof(int));
            Marshal.WriteIntPtr(ppName, IntPtr.Zero);

            int hr = getState(gsmPtr, pRate, pActive, ppName, pTimeBias, pDeadband, pLcid);

            int rate = Marshal.ReadInt32(pRate);
            int active = Marshal.ReadInt32(pActive);
            IntPtr namePtr = Marshal.ReadIntPtr(ppName);
            string? name = Marshal.PtrToStringUni(namePtr);
            int lcid = Marshal.ReadInt32(pLcid);

            if (namePtr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(namePtr);
            }
            Marshal.FreeCoTaskMem(pRate);
            Marshal.FreeCoTaskMem(pActive);
            Marshal.FreeCoTaskMem(ppName);
            Marshal.FreeCoTaskMem(pTimeBias);
            Marshal.FreeCoTaskMem(pDeadband);
            Marshal.FreeCoTaskMem(pLcid);

            return new GetStateResult(hr, rate, active, name, lcid);
        }

        internal static int InvokeSetName(IntPtr gsmPtr, string name)
        {
            IntPtr* vtable = *(IntPtr**)gsmPtr;
            var setName = (delegate* unmanaged<IntPtr, IntPtr, int>)vtable[5];
            IntPtr namePtr = Marshal.StringToCoTaskMemUni(name);
            try
            {
                return setName(gsmPtr, namePtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(namePtr);
            }
        }

        internal static CloneGroupResult InvokeCloneGroup(IntPtr gsmPtr, string name)
        {
            IntPtr* vtable = *(IntPtr**)gsmPtr;
            var clone = (delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[6];
            IntPtr namePtr = Marshal.StringToCoTaskMemUni(name);
            try
            {
                IntPtr ppUnk;
                Guid iid = IID_IUnknown;
                int hr = clone(gsmPtr, namePtr, &iid, &ppUnk);
                return new CloneGroupResult(hr, ppUnk);
            }
            finally
            {
                Marshal.FreeCoTaskMem(namePtr);
            }
        }

        internal static RemoveItemsResult InvokeRemoveItems(IntPtr itemMgtPtr, int[] handles)
        {
            IntPtr* vtable = *(IntPtr**)itemMgtPtr;
            var removeItems = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, int>)vtable[5];
            IntPtr pHandles = Marshal.AllocCoTaskMem(handles.Length * sizeof(int));
            Marshal.Copy(handles, 0, pHandles, handles.Length);
            IntPtr ppErrors;
            int hr = removeItems(itemMgtPtr, (uint)handles.Length, pHandles, &ppErrors);
            int[] errors = ReadErrorsAndFree(ppErrors, handles.Length);
            Marshal.FreeCoTaskMem(pHandles);
            return new RemoveItemsResult(hr, errors);
        }

        internal static ItemMethodResult InvokeAddItems(IntPtr itemMgtPtr, OpcItemDef[] defs) =>
            InvokeItemMethod(itemMgtPtr, defs, vtableSlot: 3, blobUpdate: 0);

        internal static int InvokeAddItemsWithNullOutPointers(IntPtr itemMgtPtr)
        {
            IntPtr* vtable = *(IntPtr**)itemMgtPtr;
            var addItems = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)vtable[3];
            return addItems(itemMgtPtr, 0, IntPtr.Zero, null, null);
        }

        internal static ItemMethodResult InvokeValidateItems(IntPtr itemMgtPtr, OpcItemDef[] defs) =>
            InvokeItemMethod(itemMgtPtr, defs, vtableSlot: 4, blobUpdate: 1);

        internal static CreateEnumeratorResult InvokeCreateEnumerator(IntPtr itemMgtPtr, Guid iid)
        {
            IntPtr* vtable = *(IntPtr**)itemMgtPtr;
            var create = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[9];
            Guid local = iid;
            IntPtr ppUnk;
            int hr = create(itemMgtPtr, &local, &ppUnk);
            return new CreateEnumeratorResult(hr, ppUnk);
        }

        internal static EnumNextResult InvokeEnumNext(IntPtr enumPtr, uint count)
        {
            IntPtr* vtable = *(IntPtr**)enumPtr;
            var next = (delegate* unmanaged<IntPtr, uint, IntPtr*, uint*, int>)vtable[3];
            IntPtr ppItems;
            uint fetched;
            int hr = next(enumPtr, count, &ppItems, &fetched);
            string?[] itemIds = ReadItemIdsAndFree(ppItems, (int)fetched);
            return new EnumNextResult(hr, fetched, itemIds);
        }

        private static ItemMethodResult InvokeItemMethod(IntPtr itemMgtPtr, OpcItemDef[] defs, int vtableSlot, int blobUpdate)
        {
            IntPtr* vtable = *(IntPtr**)itemMgtPtr;
            IntPtr pItems = AllocateNativeItemDefs(defs, out IntPtr[] allocations);
            try
            {
                IntPtr ppResults;
                IntPtr ppErrors;
                int hr = InvokeItemMethodCore(vtable[vtableSlot], itemMgtPtr, defs.Length, pItems,
                    blobUpdate, &ppResults, &ppErrors);
                return new ItemMethodResult(hr, ReadItemResultsAndFree(ppResults, defs.Length),
                    ReadErrorsAndFree(ppErrors, defs.Length));
            }
            finally
            {
                FreeAllocations(allocations);
                Marshal.FreeCoTaskMem(pItems);
            }
        }

        private static int InvokeItemMethodCore(IntPtr method, IntPtr itemMgtPtr, int count, IntPtr pItems,
            int blobUpdate, IntPtr* ppResults, IntPtr* ppErrors)
        {
            if (blobUpdate < 0)
            {
                return E_INVALIDARG;
            }
            if (method == IntPtr.Zero)
            {
                return E_INVALIDARG;
            }
            if (blobUpdate == 0)
            {
                var add = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)method;
                return add(itemMgtPtr, (uint)count, pItems, ppResults, ppErrors);
            }
            var validate = (delegate* unmanaged<IntPtr, uint, IntPtr, int, IntPtr*, IntPtr*, int>)method;
            return validate(itemMgtPtr, (uint)count, pItems, blobUpdate, ppResults, ppErrors);
        }

        private static IntPtr AllocateNativeItemDefs(OpcItemDef[] defs, out IntPtr[] allocations)
        {
            allocations = new IntPtr[defs.Length * 3];
            if (defs.Length == 0)
            {
                return IntPtr.Zero;
            }
            int size = Marshal.SizeOf<OPCITEMDEF_NATIVE>();
            IntPtr pItems = Marshal.AllocCoTaskMem(checked(defs.Length * size));
            int allocationIndex = 0;
            for (int i = 0; i < defs.Length; i++)
            {
                OPCITEMDEF_NATIVE native = ToNativeItemDef(defs[i], allocations, ref allocationIndex);
                Marshal.StructureToPtr(native, IntPtr.Add(pItems, i * size), fDeleteOld: false);
            }
            return pItems;
        }

        private static OPCITEMDEF_NATIVE ToNativeItemDef(OpcItemDef def, IntPtr[] allocations, ref int allocationIndex)
        {
            byte[] blob = def.Blob ?? Array.Empty<byte>();
            IntPtr accessPath = Marshal.StringToCoTaskMemUni(def.AccessPath);
            IntPtr itemId = Marshal.StringToCoTaskMemUni(def.ItemId);
            IntPtr blobPtr = AllocateBlob(blob);
            allocations[allocationIndex++] = accessPath;
            allocations[allocationIndex++] = itemId;
            allocations[allocationIndex++] = blobPtr;
            return new OPCITEMDEF_NATIVE
            {
                szAccessPath = accessPath,
                szItemID = itemId,
                bActive = def.Active ? 1 : 0,
                hClient = unchecked((uint)def.ClientHandle),
                dwBlobSize = unchecked((uint)blob.Length),
                pBlob = blobPtr,
                vtRequestedDataType = (ushort)def.RequestedDataType,
            };
        }

        private static NativeItemResult[] ReadItemResultsAndFree(IntPtr ptr, int count)
        {
            var results = new NativeItemResult[count];
            int size = Marshal.SizeOf<OPCITEMRESULT_NATIVE>();
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                var native = Marshal.PtrToStructure<OPCITEMRESULT_NATIVE>(IntPtr.Add(ptr, i * size));
                results[i] = new NativeItemResult(unchecked((int)native.hServer), native.vtCanonicalDataType,
                    unchecked((int)native.dwAccessRights), unchecked((int)native.dwBlobSize));
                Marshal.FreeCoTaskMem(native.pBlob);
            }
            Marshal.FreeCoTaskMem(ptr);
            return results;
        }

        private static int[] ReadErrorsAndFree(IntPtr ptr, int count)
        {
            var errors = new int[count];
            if (ptr != IntPtr.Zero && count > 0)
            {
                Marshal.Copy(ptr, errors, 0, count);
            }
            Marshal.FreeCoTaskMem(ptr);
            return errors;
        }

        private static string?[] ReadItemIdsAndFree(IntPtr ptr, int count)
        {
            var itemIds = new string?[count];
            int size = Marshal.SizeOf<OPCITEMATTRIBUTES_NATIVE>();
            for (int i = 0; i < count && ptr != IntPtr.Zero; i++)
            {
                var native = Marshal.PtrToStructure<OPCITEMATTRIBUTES_NATIVE>(IntPtr.Add(ptr, i * size));
                itemIds[i] = Marshal.PtrToStringUni(native.szItemID);
                FreeNativeAttributes(native);
            }
            Marshal.FreeCoTaskMem(ptr);
            return itemIds;
        }

        private static void FreeNativeAttributes(OPCITEMATTRIBUTES_NATIVE native)
        {
            Marshal.FreeCoTaskMem(native.szAccessPath);
            Marshal.FreeCoTaskMem(native.szItemID);
            Marshal.FreeCoTaskMem(native.pBlob);
        }

        private static void FreeAllocations(IntPtr[] allocations)
        {
            foreach (IntPtr allocation in allocations)
            {
                Marshal.FreeCoTaskMem(allocation);
            }
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
    }
}
