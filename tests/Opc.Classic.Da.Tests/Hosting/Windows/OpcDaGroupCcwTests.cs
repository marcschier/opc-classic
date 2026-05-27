//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
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
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_NOTIMPL = unchecked((int)0x80004001);

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
    public async Task CloneGroup_returns_E_NOTIMPL_until_marshaling_wired()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr gsmPtr = Helpers.InvokeQI(ccw, IOPCGroupStateMgt.InterfaceId);
        int hr = Helpers.InvokeCloneGroup(gsmPtr);

        await Assert.That(hr).IsEqualTo(E_NOTIMPL);
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
    public async Task AddItems_returns_E_NOTIMPL_until_OPCITEMDEF_marshaling_wired()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaGroupCcw.Create(NewGroup());
        IntPtr itemMgtPtr = Helpers.InvokeQI(ccw, IOPCItemMgt.InterfaceId);
        int hr = Helpers.InvokeAddItems(itemMgtPtr);

        await Assert.That(hr).IsEqualTo(E_NOTIMPL);
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

        internal static int InvokeCloneGroup(IntPtr gsmPtr)
        {
            IntPtr* vtable = *(IntPtr**)gsmPtr;
            var clone = (delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)vtable[6];
            IntPtr ppUnk;
            Guid iid = IID_IUnknown;
            return clone(gsmPtr, IntPtr.Zero, &iid, &ppUnk);
        }

        internal static RemoveItemsResult InvokeRemoveItems(IntPtr itemMgtPtr, int[] handles)
        {
            IntPtr* vtable = *(IntPtr**)itemMgtPtr;
            var removeItems = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, int>)vtable[5];

            IntPtr pHandles = Marshal.AllocCoTaskMem(handles.Length * sizeof(int));
            Marshal.Copy(handles, 0, pHandles, handles.Length);
            IntPtr ppErrors;
            int hr = removeItems(itemMgtPtr, (uint)handles.Length, pHandles, &ppErrors);

            int[] errors = new int[handles.Length];
            if (ppErrors != IntPtr.Zero)
            {
                Marshal.Copy(ppErrors, errors, 0, handles.Length);
                Marshal.FreeCoTaskMem(ppErrors);
            }
            Marshal.FreeCoTaskMem(pHandles);
            return new RemoveItemsResult(hr, errors);
        }

        internal static int InvokeAddItems(IntPtr itemMgtPtr)
        {
            IntPtr* vtable = *(IntPtr**)itemMgtPtr;
            var addItems = (delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)vtable[3];
            IntPtr ppResults, ppErrors;
            return addItems(itemMgtPtr, 0, IntPtr.Zero, &ppResults, &ppErrors);
        }
    }
}
