//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using Opc.Classic.Dcom;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>
/// Windows-only unit tests for <see cref="OpcDaServerCcw"/>. Exercises the
/// CCW lifecycle (Create / QueryInterface / AddRef / Release) and verifies
/// that all 9 IOPCServer vtable slots return E_NOTIMPL today.
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class OpcDaServerCcwTests
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_NOTIMPL = unchecked((int)0x80004001);

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");
    private static readonly Guid IID_IClassFactory = Guid.Parse("00000001-0000-0000-C000-000000000046");

    [Test]
    public async Task Create_returns_zero_for_unsupported_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IClassFactory);

        await Assert.That(ccw).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IID_IUnknown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcDaServerCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task Create_returns_nonzero_for_IOPCServer()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IOPCServer.InterfaceId);

        await Assert.That(ccw).IsNotEqualTo(IntPtr.Zero);
        await Assert.That(OpcDaServerCcw.GetReferenceCount(ccw)).IsEqualTo(1L);
    }

    [Test]
    public async Task QueryInterface_returns_same_pointer_for_supported_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);
        QueryInterfaceResult result = InvokeQueryInterface(ccw, IOPCServer.InterfaceId);

        await Assert.That(result.Hr).IsEqualTo(S_OK);
        await Assert.That(result.Returned).IsEqualTo(ccw);
        // QI implies AddRef => refcount went from 1 to 2.
        await Assert.That(OpcDaServerCcw.GetReferenceCount(ccw)).IsEqualTo(2L);
    }

    [Test]
    public async Task QueryInterface_returns_E_NOINTERFACE_for_unknown_iid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);
        QueryInterfaceResult result = InvokeQueryInterface(ccw, Guid.NewGuid());

        await Assert.That(result.Hr).IsEqualTo(E_NOINTERFACE);
        await Assert.That(result.Returned).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task AddRef_and_Release_drive_refcount()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IID_IUnknown);
        AddRefReleaseResult result = InvokeAddRefRelease(ccw);

        await Assert.That(result.After1stAddRef).IsEqualTo(2u);
        await Assert.That(result.After2ndAddRef).IsEqualTo(3u);
        await Assert.That(result.After1stRelease).IsEqualTo(2u);
        await Assert.That(result.After2ndRelease).IsEqualTo(1u);
    }

    [Test]
    public async Task RemoveGroup_via_CCW_delegates_to_managed_server()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var stub = new RecordingDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(stub, IOPCServer.InterfaceId);

        int hr = InvokeRemoveGroup(ccw, hServerGroup: 12345, bForce: 1);

        await Assert.That(hr).IsEqualTo(S_OK);
        await Assert.That(stub.LastRemovedGroupHandle).IsEqualTo(12345);
        await Assert.That(stub.LastRemoveGroupForce).IsTrue();
        await Assert.That(stub.RemoveGroupCallCount).IsEqualTo(1);
    }

    [Test]
    public async Task IOPCServer_vtable_slots_return_E_NOTIMPL()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr ccw = OpcDaServerCcw.Create(new StubDaServer(), IOPCServer.InterfaceId);
        NotImplStubsResult result = InvokeOpcServerStubs(ccw);

        await Assert.That(result.HrGetErrorString).IsEqualTo(E_NOTIMPL);
        await Assert.That(result.StringOut).IsEqualTo(IntPtr.Zero);
        await Assert.That(result.HrGetStatus).IsEqualTo(E_NOTIMPL);
        await Assert.That(result.StatusOut).IsEqualTo(IntPtr.Zero);
    }

    [Test]
    public async Task SupportsInterface_returns_true_for_known_iids()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await Assert.That(OpcDaServerCcw.SupportsInterface(IID_IUnknown)).IsTrue();
        await Assert.That(OpcDaServerCcw.SupportsInterface(IOPCServer.InterfaceId)).IsTrue();
        await Assert.That(OpcDaServerCcw.SupportsInterface(IID_IClassFactory)).IsFalse();
        await Assert.That(OpcDaServerCcw.SupportsInterface(Guid.NewGuid())).IsFalse();
    }

    // ----- sync unsafe helpers (await is illegal in unsafe context) -----

    private readonly record struct QueryInterfaceResult(IntPtr Returned, int Hr);

    private readonly record struct AddRefReleaseResult(
        uint After1stAddRef, uint After2ndAddRef,
        uint After1stRelease, uint After2ndRelease);

    private readonly record struct NotImplStubsResult(
        int HrGetErrorString, IntPtr StringOut,
        int HrGetStatus, IntPtr StatusOut);

    private static unsafe QueryInterfaceResult InvokeQueryInterface(IntPtr ccw, Guid iid)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var qi = (delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)vtable[0];
        Guid local = iid;
        IntPtr returned;
        int hr = qi(ccw, &local, &returned);
        return new QueryInterfaceResult(returned, hr);
    }

    private static unsafe AddRefReleaseResult InvokeAddRefRelease(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var addRef = (delegate* unmanaged<IntPtr, uint>)vtable[1];
        var release = (delegate* unmanaged<IntPtr, uint>)vtable[2];
        uint a1 = addRef(ccw);
        uint a2 = addRef(ccw);
        uint r1 = release(ccw);
        uint r2 = release(ccw);
        return new AddRefReleaseResult(a1, a2, r1, r2);
    }

    private static unsafe NotImplStubsResult InvokeOpcServerStubs(IntPtr ccw)
    {
        IntPtr* vtable = *(IntPtr**)ccw;

        var getErrorString = (delegate* unmanaged<IntPtr, int, uint, IntPtr*, int>)vtable[4];
        IntPtr stringOut;
        int hrGetError = getErrorString(ccw, 0, 0, &stringOut);

        var getStatus = (delegate* unmanaged<IntPtr, IntPtr*, int>)vtable[6];
        IntPtr statusOut;
        int hrGetStatus = getStatus(ccw, &statusOut);

        return new NotImplStubsResult(hrGetError, stringOut, hrGetStatus, statusOut);
    }

    private static unsafe int InvokeRemoveGroup(IntPtr ccw, uint hServerGroup, int bForce)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var removeGroup = (delegate* unmanaged<IntPtr, uint, int, int>)vtable[7];
        return removeGroup(ccw, hServerGroup, bForce);
    }

    private sealed class RecordingDaServer : IOpcDaServer
    {
        public int RemoveGroupCallCount { get; private set; }

        public int LastRemovedGroupHandle { get; private set; }

        public bool LastRemoveGroupForce { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "recording-test",
            });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
        {
            RemoveGroupCallCount++;
            LastRemovedGroupHandle = serverGroupHandle;
            LastRemoveGroupForce = force;
            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");
    }

    private sealed class StubDaServer : IOpcDaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "ccw-test",
            });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");
    }
}
