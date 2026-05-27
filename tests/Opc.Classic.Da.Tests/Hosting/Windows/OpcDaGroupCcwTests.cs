//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
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
    private const int E_NOTIMPL = unchecked((int)0x80004001);
    private const int CONNECT_E_NOCONNECTION = unchecked((int)0x80040200);

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

    [Test]
    public async Task QueryInterface_for_IOPCSyncIO_returns_real_tearoff()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr syncPtr = Helpers.InvokeQI(ccw, IOPCSyncIO.InterfaceId);

        await Assert.That(syncPtr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(syncPtr).IsNotEqualTo(ccw);
    }

    [Test]
    public async Task QueryInterface_for_IOPCSyncIO2_returns_distinct_tearoff()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr syncPtr = Helpers.InvokeQI(ccw, IOPCSyncIO.InterfaceId);
        IntPtr sync2Ptr = Helpers.InvokeQI(ccw, IOPCSyncIO2.InterfaceId);

        await Assert.That(sync2Ptr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(sync2Ptr).IsNotEqualTo(syncPtr);
    }

    [Test]
    public async Task IOPCSyncIO_read_and_write_return_notimpl_for_variant_mvp()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr syncPtr = Helpers.InvokeQI(ccw, IOPCSyncIO.InterfaceId);

        await Assert.That(Helpers.InvokeSyncRead(syncPtr)).IsEqualTo(E_NOTIMPL);
        await Assert.That(Helpers.InvokeSyncWrite(syncPtr)).IsEqualTo(E_NOTIMPL);
    }

    [Test]
    public async Task IOPCSyncIO2_maxage_and_vqt_return_notimpl_for_variant_mvp()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr sync2Ptr = Helpers.InvokeQI(ccw, IOPCSyncIO2.InterfaceId);

        await Assert.That(Helpers.InvokeSyncReadMaxAge(sync2Ptr)).IsEqualTo(E_NOTIMPL);
        await Assert.That(Helpers.InvokeSyncWriteVqt(sync2Ptr)).IsEqualTo(E_NOTIMPL);
    }

    [Test]
    public async Task QueryInterface_for_IOPCAsyncIO2_and_IOPCAsyncIO3_returns_tearoffs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr async2Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO2.InterfaceId);
        IntPtr async3Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO3.InterfaceId);

        await Assert.That(async2Ptr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(async3Ptr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(async3Ptr).IsNotEqualTo(async2Ptr);
    }

    [Test]
    public async Task IOPCAsyncIO2_read_dispatches_and_returns_cancel_id_and_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup();
        await AddManagedItems(group, "Tag.A");
        int knownHandle = group.Items.Single().ServerHandle;
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr async2Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO2.InterfaceId);
        Helpers.AsyncErrorsResult result = Helpers.InvokeAsyncRead(async2Ptr, [knownHandle, knownHandle + 1000]);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.CancelId).IsNotEqualTo(0);
        await Assert.That(result.Errors[0]).IsEqualTo(S_OK);
        await Assert.That(result.Errors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
    }

    [Test]
    public async Task IOPCAsyncIO2_setenable_getenable_round_trips()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr async2Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO2.InterfaceId);

        int setHr = Helpers.InvokeAsyncSetEnable(async2Ptr, enabled: false);
        Helpers.GetEnableResult enabled = Helpers.InvokeAsyncGetEnable(async2Ptr);

        await Assert.That(setHr).IsEqualTo(S_OK);
        await Assert.That(enabled.Hr).IsEqualTo(S_OK);
        await Assert.That(enabled.Enabled).IsFalse();
    }

    [Test]
    public async Task IOPCAsyncIO2_cancel2_dispatches_to_managed_group()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup();
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr async2Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO2.InterfaceId);

        int hr = Helpers.InvokeAsyncCancel2(async2Ptr, 1234);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(group.LastCancel2Id).IsEqualTo(1234);
    }

    [Test]
    public async Task IOPCAsyncIO2_refresh2_returns_nonzero_cancel_id()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr async2Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO2.InterfaceId);
        Helpers.CancelResult result = Helpers.InvokeAsyncRefresh2(async2Ptr);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.CancelId).IsNotEqualTo(0);
    }

    [Test]
    public async Task IOPCAsyncIO3_readmaxage_and_refreshmaxage_return_cancel_ids()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcDaGroup group = NewGroup();
        await AddManagedItems(group, "Tag.A");
        int knownHandle = group.Items.Single().ServerHandle;
        IntPtr ccw = OpcDaGroupCcw.Create(group);
        IntPtr async3Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO3.InterfaceId);

        Helpers.AsyncErrorsResult read = Helpers.InvokeAsyncReadMaxAge(async3Ptr, [knownHandle, knownHandle + 1000], [0, 0]);
        Helpers.CancelResult refresh = Helpers.InvokeAsyncRefreshMaxAge(async3Ptr);

        await Assert.That(read.Hr).IsEqualTo(S_OK);
        await Assert.That(read.CancelId).IsNotEqualTo(0);
        await Assert.That(read.Errors[0]).IsEqualTo(S_OK);
        await Assert.That(read.Errors[1]).IsEqualTo(OpcResultId.InvalidHandle.Code);
        await Assert.That(refresh.Hr).IsEqualTo(S_OK);
        await Assert.That(refresh.CancelId).IsNotEqualTo(0);
    }

    [Test]
    public async Task IOPCAsyncIO_write_methods_return_notimpl_for_variant_mvp()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr async2Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO2.InterfaceId);
        IntPtr async3Ptr = Helpers.InvokeQI(ccw, IOPCAsyncIO3.InterfaceId);

        await Assert.That(Helpers.InvokeAsyncWrite(async2Ptr)).IsEqualTo(E_NOTIMPL);
        await Assert.That(Helpers.InvokeAsyncWriteVqt(async3Ptr)).IsEqualTo(E_NOTIMPL);
    }

    [Test]
    public async Task QueryInterface_for_connection_point_interfaces_returns_tearoffs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr cpPtr = Helpers.InvokeQI(ccw, IConnectionPoint.InterfaceId);
        IntPtr cpcPtr = Helpers.InvokeQI(ccw, IConnectionPointContainer.InterfaceId);

        await Assert.That(cpPtr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(cpcPtr).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(cpPtr).IsNotEqualTo(cpcPtr);
    }

    [Test]
    public async Task IConnectionPoint_getconnectioninterface_returns_data_callback_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr cpPtr = Helpers.InvokeQI(ccw, IConnectionPoint.InterfaceId);
        Helpers.GuidResult result = Helpers.InvokeGetConnectionInterface(cpPtr);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Value).IsEqualTo(IOPCDataCallback.InterfaceId);
    }

    [Test]
    public async Task IConnectionPointContainer_findconnectionpoint_returns_connection_point_for_data_callback()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr cpPtr = Helpers.InvokeQI(ccw, IConnectionPoint.InterfaceId);
        IntPtr cpcPtr = Helpers.InvokeQI(ccw, IConnectionPointContainer.InterfaceId);
        Helpers.PointerResult result = Helpers.InvokeFindConnectionPoint(cpcPtr, IOPCDataCallback.InterfaceId);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Pointer).IsEqualTo(cpPtr);
    }

    [Test]
    public async Task IConnectionPointContainer_findconnectionpoint_for_unknown_iid_returns_no_connection()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr cpcPtr = Helpers.InvokeQI(ccw, IConnectionPointContainer.InterfaceId);
        Helpers.PointerResult result = Helpers.InvokeFindConnectionPoint(cpcPtr, Guid.NewGuid());

        await Assert.That(result.Hr).IsEqualTo(CONNECT_E_NOCONNECTION);
        await Assert.That(result.Pointer).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task IConnectionPoint_advise_and_unadvise_manage_scm_sink_dictionary()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateDataCallbackStub();
        try
        {
            IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
            IntPtr cpPtr = Helpers.InvokeQI(ccw, IConnectionPoint.InterfaceId);
            Helpers.AdviseResult advised = Helpers.InvokeAdvise(cpPtr, stub);

            await Assert.That(advised.Hr).IsEqualTo(S_OK);
            await Assert.That(advised.Cookie).IsNotEqualTo(0);
            await Assert.That(OpcDaGroupCcw.GetScmSinkCount(cpPtr)).IsEqualTo(1);

            int unadviseHr = Helpers.InvokeUnadvise(cpPtr, advised.Cookie);

            await Assert.That(unadviseHr).IsEqualTo(S_OK);
            await Assert.That(OpcDaGroupCcw.GetScmSinkCount(cpPtr)).IsEqualTo(0);
            await Assert.That(Helpers.GetDataCallbackStubReferenceCount(stub)).IsEqualTo(1L);
        }
        finally
        {
            Helpers.DestroyDataCallbackStub(stub);
        }
    }

    [Test]
    public async Task IConnectionPoint_unadvise_unknown_cookie_returns_no_connection()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr cpPtr = Helpers.InvokeQI(ccw, IConnectionPoint.InterfaceId);

        int hr = Helpers.InvokeUnadvise(cpPtr, 999);

        await Assert.That(hr).IsEqualTo(CONNECT_E_NOCONNECTION);
    }

    [Test]
    public async Task Release_to_zero_disposes_all_scm_sink_proxies()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr stub = Helpers.CreateDataCallbackStub();
        try
        {
            IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
            IntPtr cpPtr = Helpers.InvokeQI(ccw, IConnectionPoint.InterfaceId);
            Helpers.AdviseResult advised = Helpers.InvokeAdvise(cpPtr, stub);
            await Assert.That(advised.Hr).IsEqualTo(S_OK);
            await Assert.That(Helpers.GetDataCallbackStubReferenceCount(stub)).IsEqualTo(2L);

            Helpers.InvokeRelease(cpPtr);
            Helpers.InvokeRelease(ccw);

            await Assert.That(Helpers.GetDataCallbackStubReferenceCount(stub)).IsEqualTo(1L);
            await Assert.That(OpcDaGroupCcw.GetReferenceCount(ccw)).IsEqualTo(-1L);
        }
        finally
        {
            Helpers.DestroyDataCallbackStub(stub);
        }
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

        internal readonly record struct AsyncErrorsResult(int Hr, int CancelId, int[] Errors);

        internal readonly record struct CancelResult(int Hr, int CancelId);

        internal readonly record struct GetEnableResult(int Hr, bool Enabled);

        internal readonly record struct GuidResult(int Hr, Guid Value);

        internal readonly record struct PointerResult(int Hr, IntPtr Pointer);

        internal readonly record struct AdviseResult(int Hr, int Cookie);

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

        private const int DataCallbackVtableSlotCount = 7;

        private static readonly Guid s_iidDataCallback = IOPCDataCallback.InterfaceId;
        private static readonly ConcurrentDictionary<IntPtr, DataCallbackStubSession> s_dataCallbackStubs = new();

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

        internal static int InvokeSyncRead(IntPtr syncPtr)
        {
            IntPtr* vtable = *(IntPtr**)syncPtr;
            var read = (delegate* unmanaged<IntPtr, uint, uint, IntPtr, IntPtr*, IntPtr*, int>)vtable[3];
            IntPtr ppValues;
            IntPtr ppErrors;
            int hr = read(syncPtr, 1, 0, IntPtr.Zero, &ppValues, &ppErrors);
            FreeCoTaskMem(ppValues, ppErrors);
            return hr;
        }

        internal static int InvokeSyncWrite(IntPtr syncPtr)
        {
            IntPtr* vtable = *(IntPtr**)syncPtr;
            var write = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)vtable[4];
            IntPtr ppErrors;
            int hr = write(syncPtr, 0, IntPtr.Zero, IntPtr.Zero, &ppErrors);
            FreeCoTaskMem(ppErrors);
            return hr;
        }

        internal static int InvokeSyncReadMaxAge(IntPtr sync2Ptr)
        {
            IntPtr* vtable = *(IntPtr**)sync2Ptr;
            var read = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, IntPtr*, IntPtr*, IntPtr*, int>)vtable[5];
            IntPtr values;
            IntPtr qualities;
            IntPtr timestamps;
            IntPtr errors;
            int hr = read(sync2Ptr, 0, IntPtr.Zero, IntPtr.Zero, &values, &qualities, &timestamps, &errors);
            FreeCoTaskMem(values, qualities, timestamps, errors);
            return hr;
        }

        internal static int InvokeSyncWriteVqt(IntPtr sync2Ptr)
        {
            IntPtr* vtable = *(IntPtr**)sync2Ptr;
            var write = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)vtable[6];
            IntPtr ppErrors;
            int hr = write(sync2Ptr, 0, IntPtr.Zero, IntPtr.Zero, &ppErrors);
            FreeCoTaskMem(ppErrors);
            return hr;
        }

        internal static AsyncErrorsResult InvokeAsyncRead(IntPtr async2Ptr, int[] handles)
        {
            IntPtr* vtable = *(IntPtr**)async2Ptr;
            IntPtr pHandles = AllocateInt32ArrayForCall(handles);
            try
            {
                var read = (delegate* unmanaged<IntPtr, uint, IntPtr, uint, uint*, IntPtr*, int>)vtable[3];
                uint cancelId;
                IntPtr ppErrors;
                int hr = read(async2Ptr, (uint)handles.Length, pHandles, 77, &cancelId, &ppErrors);
                return new AsyncErrorsResult(hr, unchecked((int)cancelId), ReadErrorsAndFree(ppErrors, handles.Length));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pHandles);
            }
        }

        internal static int InvokeAsyncWrite(IntPtr async2Ptr)
        {
            IntPtr* vtable = *(IntPtr**)async2Ptr;
            var write = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint*, IntPtr*, int>)vtable[4];
            uint cancelId;
            IntPtr ppErrors;
            int hr = write(async2Ptr, 0, IntPtr.Zero, IntPtr.Zero, 77, &cancelId, &ppErrors);
            FreeCoTaskMem(ppErrors);
            return hr;
        }

        internal static CancelResult InvokeAsyncRefresh2(IntPtr async2Ptr)
        {
            IntPtr* vtable = *(IntPtr**)async2Ptr;
            var refresh = (delegate* unmanaged<IntPtr, uint, uint, uint*, int>)vtable[5];
            uint cancelId;
            int hr = refresh(async2Ptr, 1, 99, &cancelId);
            return new CancelResult(hr, unchecked((int)cancelId));
        }

        internal static int InvokeAsyncCancel2(IntPtr async2Ptr, int cancelId)
        {
            IntPtr* vtable = *(IntPtr**)async2Ptr;
            var cancel = (delegate* unmanaged<IntPtr, uint, int>)vtable[6];
            return cancel(async2Ptr, unchecked((uint)cancelId));
        }

        internal static int InvokeAsyncSetEnable(IntPtr async2Ptr, bool enabled)
        {
            IntPtr* vtable = *(IntPtr**)async2Ptr;
            var setEnable = (delegate* unmanaged<IntPtr, int, int>)vtable[7];
            return setEnable(async2Ptr, enabled ? 1 : 0);
        }

        internal static GetEnableResult InvokeAsyncGetEnable(IntPtr async2Ptr)
        {
            IntPtr* vtable = *(IntPtr**)async2Ptr;
            var getEnable = (delegate* unmanaged<IntPtr, int*, int>)vtable[8];
            int enabled;
            int hr = getEnable(async2Ptr, &enabled);
            return new GetEnableResult(hr, enabled != 0);
        }

        internal static AsyncErrorsResult InvokeAsyncReadMaxAge(IntPtr async3Ptr, int[] handles, int[] maxAges)
        {
            IntPtr* vtable = *(IntPtr**)async3Ptr;
            IntPtr pHandles = AllocateInt32ArrayForCall(handles);
            IntPtr pMaxAges = AllocateInt32ArrayForCall(maxAges);
            try
            {
                var read = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint*, IntPtr*, int>)vtable[9];
                uint cancelId;
                IntPtr ppErrors;
                int hr = read(async3Ptr, (uint)handles.Length, pHandles, pMaxAges, 88, &cancelId, &ppErrors);
                return new AsyncErrorsResult(hr, unchecked((int)cancelId), ReadErrorsAndFree(ppErrors, handles.Length));
            }
            finally
            {
                FreeCoTaskMem(pHandles, pMaxAges);
            }
        }

        internal static int InvokeAsyncWriteVqt(IntPtr async3Ptr)
        {
            IntPtr* vtable = *(IntPtr**)async3Ptr;
            var write = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, uint, uint*, IntPtr*, int>)vtable[10];
            uint cancelId;
            IntPtr ppErrors;
            int hr = write(async3Ptr, 0, IntPtr.Zero, IntPtr.Zero, 88, &cancelId, &ppErrors);
            FreeCoTaskMem(ppErrors);
            return hr;
        }

        internal static CancelResult InvokeAsyncRefreshMaxAge(IntPtr async3Ptr)
        {
            IntPtr* vtable = *(IntPtr**)async3Ptr;
            var refresh = (delegate* unmanaged<IntPtr, uint, uint, uint*, int>)vtable[11];
            uint cancelId;
            int hr = refresh(async3Ptr, 0, 88, &cancelId);
            return new CancelResult(hr, unchecked((int)cancelId));
        }

        internal static GuidResult InvokeGetConnectionInterface(IntPtr cpPtr)
        {
            IntPtr* vtable = *(IntPtr**)cpPtr;
            var getInterface = (delegate* unmanaged<IntPtr, Guid*, int>)vtable[3];
            Guid iid;
            int hr = getInterface(cpPtr, &iid);
            return new GuidResult(hr, iid);
        }

        internal static PointerResult InvokeFindConnectionPoint(IntPtr cpcPtr, Guid iid)
        {
            IntPtr* vtable = *(IntPtr**)cpcPtr;
            var find = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[4];
            Guid local = iid;
            IntPtr pointer;
            int hr = find(cpcPtr, &local, &pointer);
            return new PointerResult(hr, pointer);
        }

        internal static AdviseResult InvokeAdvise(IntPtr cpPtr, IntPtr sink)
        {
            IntPtr* vtable = *(IntPtr**)cpPtr;
            var advise = (delegate* unmanaged<IntPtr, IntPtr, uint*, int>)vtable[5];
            uint cookie;
            int hr = advise(cpPtr, sink, &cookie);
            return new AdviseResult(hr, unchecked((int)cookie));
        }

        internal static int InvokeUnadvise(IntPtr cpPtr, int cookie)
        {
            IntPtr* vtable = *(IntPtr**)cpPtr;
            var unadvise = (delegate* unmanaged<IntPtr, uint, int>)vtable[6];
            return unadvise(cpPtr, unchecked((uint)cookie));
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

        internal static IntPtr CreateDataCallbackStub()
        {
            IntPtr* vtable = AllocateDataCallbackStubVtable();
            IntPtr instance = AllocateDataCallbackStubInstance(vtable);
            s_dataCallbackStubs[instance] = new DataCallbackStubSession(vtable);
            return instance;
        }

        internal static void DestroyDataCallbackStub(IntPtr stub)
        {
            if (!s_dataCallbackStubs.TryRemove(stub, out DataCallbackStubSession? session))
            {
                return;
            }

            NativeMemory.Free((void*)stub);
            NativeMemory.Free(session.Vtable);
        }

        internal static long GetDataCallbackStubReferenceCount(IntPtr stub) =>
            s_dataCallbackStubs.TryGetValue(stub, out DataCallbackStubSession? session)
                ? Interlocked.Read(ref session.RefCount)
                : -1L;

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr* AllocateDataCallbackStubVtable()
        {
            IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(DataCallbackVtableSlotCount * sizeof(IntPtr)));
            vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&DataCallbackStubQueryInterface;
            vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&DataCallbackStubAddRef;
            vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&DataCallbackStubRelease;
            vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&DataCallbackStubOnDataChange;
            vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, int, uint, IntPtr, IntPtr, IntPtr, IntPtr, IntPtr, int>)&DataCallbackStubOnReadComplete;
            vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int, uint, IntPtr, IntPtr, int>)&DataCallbackStubOnWriteComplete;
            vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, uint, uint, int>)&DataCallbackStubOnCancelComplete;
            return vtable;
        }

        [SuppressMessage("Reliability", "CA2018", Justification = "Explicit byte size.")]
        private static IntPtr AllocateDataCallbackStubInstance(IntPtr* vtable)
        {
            IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
            instance[0] = (IntPtr)vtable;
            return (IntPtr)instance;
        }

        [UnmanagedCallersOnly]
        private static int DataCallbackStubQueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
        {
            if (ppv == null)
            {
                return E_INVALIDARG;
            }
            if (!s_dataCallbackStubs.TryGetValue(pThis, out DataCallbackStubSession? session) || riid == null)
            {
                *ppv = IntPtr.Zero;
                return E_NOINTERFACE;
            }
            if (*riid == IID_IUnknown || *riid == s_iidDataCallback)
            {
                *ppv = pThis;
                Interlocked.Increment(ref session.RefCount);
                return S_OK;
            }
            *ppv = IntPtr.Zero;
            return E_NOINTERFACE;
        }

        [UnmanagedCallersOnly]
        private static uint DataCallbackStubAddRef(IntPtr pThis)
        {
            if (!s_dataCallbackStubs.TryGetValue(pThis, out DataCallbackStubSession? session))
            {
                return 1;
            }
            return (uint)Interlocked.Increment(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        private static uint DataCallbackStubRelease(IntPtr pThis)
        {
            if (!s_dataCallbackStubs.TryGetValue(pThis, out DataCallbackStubSession? session))
            {
                return 0;
            }
            return (uint)Interlocked.Decrement(ref session.RefCount);
        }

        [UnmanagedCallersOnly]
        private static int DataCallbackStubOnDataChange(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterQuality,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr values,
            IntPtr qualities,
            IntPtr timestamps,
            IntPtr errors) => DataCallbackStubRecord(pThis);

        [UnmanagedCallersOnly]
        private static int DataCallbackStubOnReadComplete(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterQuality,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr values,
            IntPtr qualities,
            IntPtr timestamps,
            IntPtr errors) => DataCallbackStubRecord(pThis);

        [UnmanagedCallersOnly]
        private static int DataCallbackStubOnWriteComplete(
            IntPtr pThis,
            uint transactionId,
            uint groupHandle,
            int masterError,
            uint count,
            IntPtr clientItems,
            IntPtr errors) => DataCallbackStubRecord(pThis);

        [UnmanagedCallersOnly]
        private static int DataCallbackStubOnCancelComplete(IntPtr pThis, uint transactionId, uint groupHandle) =>
            DataCallbackStubRecord(pThis);

        private static int DataCallbackStubRecord(IntPtr pThis) =>
            s_dataCallbackStubs.ContainsKey(pThis) ? S_OK : E_NOINTERFACE;

        private static IntPtr AllocateInt32ArrayForCall(int[] values)
        {
            if (values.Length == 0)
            {
                return IntPtr.Zero;
            }
            IntPtr ptr = Marshal.AllocCoTaskMem(checked(values.Length * sizeof(int)));
            Marshal.Copy(values, 0, ptr, values.Length);
            return ptr;
        }

        private static void FreeCoTaskMem(params IntPtr[] pointers)
        {
            foreach (IntPtr pointer in pointers)
            {
                Marshal.FreeCoTaskMem(pointer);
            }
        }

        private sealed class DataCallbackStubSession(IntPtr* vtable)
        {
            internal readonly IntPtr* Vtable = vtable;
            internal long RefCount = 1;
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
