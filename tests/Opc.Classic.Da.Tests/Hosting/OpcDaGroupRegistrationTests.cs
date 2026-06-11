//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Exercises the ocom-3b end-to-end pattern: an <see cref="IOpcDaServer"/>
/// implementation creates an <see cref="OpcDaGroup"/> on AddGroup, registers
/// it in the <see cref="OpcObjectRegistry"/>, and returns an
/// <see cref="IOpcInterfaceRef"/> carrying the assigned IPID. The mirror of
/// what <c>CttDaServer</c> does in the sample.
/// </summary>
public sealed class OpcDaGroupRegistrationTests
{
    [Test]
    public async Task AddGroup_registers_group_with_OpcObjectRegistry()
    {
        var registry = new OpcObjectRegistry();
        var server = new GroupTrackingServer(registry);

        await ((IOPCServer)server).AddGroupAsync(
            name: "G",
            active: true,
            requestedUpdateRate: 1000,
            clientGroupHandle: 7,
            timeBias: 0,
            percentDeadband: 0f,
            localeId: 1033,
            requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
            out int serverHandle,
            out int revisedRate,
            out IOpcInterfaceRef groupRef,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(serverHandle).IsGreaterThan(0);
        await Assert.That(revisedRate).IsEqualTo(1000);
        await Assert.That(groupRef.Iid).IsEqualTo(IOPCGroupStateMgt.InterfaceId);
        await Assert.That(groupRef.Ipid).IsNotEqualTo(Guid.Empty);
        await Assert.That(registry.Contains(groupRef.Ipid)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(groupRef.Ipid, IOPCGroupStateMgt.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(groupRef.Ipid, IOPCGroupStateMgt2.InterfaceId, out _)).IsTrue();
    }

    [Test]
    public async Task RemoveGroup_unregisters_from_OpcObjectRegistry()
    {
        var registry = new OpcObjectRegistry();
        var server = new GroupTrackingServer(registry);

        await ((IOPCServer)server).AddGroupAsync(
            name: "G", active: true, requestedUpdateRate: 1000, clientGroupHandle: 7,
            timeBias: 0, percentDeadband: 0f, localeId: 1033,
            requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
            out int serverHandle, out _, out IOpcInterfaceRef groupRef,
            cancellationToken: TestContext.Current!.CancellationToken);

        await server.RemoveGroupAsync(serverHandle, force: false, TestContext.Current!.CancellationToken);

        await Assert.That(registry.Contains(groupRef.Ipid)).IsFalse();
    }

    [Test]
    public async Task GetStatusAsync_GroupCount_reflects_added_and_removed_groups()
    {
        var registry = new OpcObjectRegistry();
        var server = new GroupTrackingServer(registry);

        OpcServerStatus statusEmpty = await server.GetStatusAsync(TestContext.Current!.CancellationToken);
        await Assert.That(statusEmpty.GroupCount).IsEqualTo(0);

        await ((IOPCServer)server).AddGroupAsync(
            name: "G1", active: true, requestedUpdateRate: 1000, clientGroupHandle: 1,
            timeBias: 0, percentDeadband: 0f, localeId: 1033,
            requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
            out int h1, out _, out _,
            cancellationToken: TestContext.Current!.CancellationToken);
        await ((IOPCServer)server).AddGroupAsync(
            name: "G2", active: true, requestedUpdateRate: 1000, clientGroupHandle: 2,
            timeBias: 0, percentDeadband: 0f, localeId: 1033,
            requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
            out _, out _, out _,
            cancellationToken: TestContext.Current!.CancellationToken);

        OpcServerStatus statusAfterAdd = await server.GetStatusAsync(TestContext.Current!.CancellationToken);
        await Assert.That(statusAfterAdd.GroupCount).IsEqualTo(2);

        await server.RemoveGroupAsync(h1, force: false, TestContext.Current!.CancellationToken);

        OpcServerStatus statusAfterRemove = await server.GetStatusAsync(TestContext.Current!.CancellationToken);
        await Assert.That(statusAfterRemove.GroupCount).IsEqualTo(1);
    }

    /// <summary>
    /// Mirror of CttDaServer for tests in this project. (CttDaServer lives in
    /// the samples assembly which has no test project; we mirror its
    /// AddGroup/RemoveGroup wireup pattern here to lock the architecture.)
    /// </summary>
    private sealed class GroupTrackingServer : IOpcDaServer
    {
        private readonly OpcObjectRegistry _registry;
        private readonly Dictionary<int, (OpcDaGroup Group, Guid Ipid)> _groups = new();
        private int _nextServerHandle = 1000;

        public GroupTrackingServer(OpcObjectRegistry registry) { _registry = registry; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "test",
                GroupCount = _groups.Count,
            });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Interlocked.Increment(ref _nextServerHandle));

        Task IOPCServer.AddGroupAsync(
            string name, bool active, int requestedUpdateRate, int clientGroupHandle,
            int timeBias, float percentDeadband, int localeId, Guid requestedInterfaceId,
            out int serverGroupHandle, out int revisedUpdateRate, out IOpcInterfaceRef group,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int handle = Interlocked.Increment(ref _nextServerHandle);
            var managedGroup = new OpcDaGroup(name, handle, clientGroupHandle, active,
                requestedUpdateRate, timeBias, percentDeadband, localeId);

            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCGroupStateMgt.InterfaceId] = new IOPCGroupStateMgtServerDispatcher(managedGroup),
                [IOPCGroupStateMgt2.InterfaceId] = new IOPCGroupStateMgt2ServerDispatcher(managedGroup),
            };
            Guid ipid = _registry.Register(dispatchers);
            _groups[handle] = (managedGroup, ipid);

            serverGroupHandle = handle;
            revisedUpdateRate = requestedUpdateRate;
            group = new OpcInterfaceRef(
                iid: requestedInterfaceId,
                flags: 0,
                publicRefs: 1,
                oxid: 1,
                oid: unchecked((ulong)handle),
                ipid: ipid,
                securityOffset: 0,
                resolverBindings: Array.Empty<ushort>());
            return Task.CompletedTask;
        }

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
        {
            if (_groups.TryGetValue(serverGroupHandle, out var entry))
            {
                _registry.Unregister(entry.Ipid);
                _groups.Remove(serverGroupHandle);
            }
            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");
    }
}
