//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for OpcDaGroup's IConnectionPoint subscription wireup + the
/// TriggerDataChangeAsync fan-out helper (ocom-7b).
/// </summary>
public sealed class OpcDaGroupSubscriptionTests
{
    private static readonly IOpcInterfaceRef SampleSink = new OpcInterfaceRef(
        iid: IOPCDataCallback.InterfaceId,
        flags: 0,
        publicRefs: 1,
        oxid: 1,
        oid: 1,
        ipid: Guid.NewGuid(),
        securityOffset: 0,
        resolverBindings: Array.Empty<ushort>());

    [Test]
    public async Task GetConnectionInterfaceAsync_returns_IID_IOPCDataCallback()
    {
        var group = CreateGroup();
        IConnectionPoint cp = group;

        Guid iid = await cp.GetConnectionInterfaceAsync(TestContext.Current!.CancellationToken);

        await Assert.That(iid).IsEqualTo(IOPCDataCallback.InterfaceId);
    }

    [Test]
    public async Task AdviseAsync_returns_unique_cookies()
    {
        var group = CreateGroup();
        IConnectionPoint cp = group;

        int cookie1 = await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);
        int cookie2 = await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        await Assert.That(cookie1).IsNotEqualTo(cookie2);
        await Assert.That(group.SubscriptionCount).IsEqualTo(2);
    }

    [Test]
    public async Task UnadviseAsync_removes_subscription()
    {
        var group = CreateGroup();
        IConnectionPoint cp = group;
        int cookie = await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        await cp.UnadviseAsync(cookie, TestContext.Current!.CancellationToken);

        await Assert.That(group.SubscriptionCount).IsEqualTo(0);
    }

    [Test]
    public async Task TriggerDataChangeAsync_invokes_sender_once_per_sink()
    {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");
        await group.WriteAsync([handle], [new OpcVariant(VarType.VT_I4, 99)],
            TestContext.Current!.CancellationToken);

        IConnectionPoint cp = group;
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        var deliveries = new List<OpcDaGroup.DataChangePayload>();
        await group.TriggerDataChangeAsync(
            transactionId: 42,
            serverHandles: [handle],
            sender: (sink, payload, ct) =>
            {
                deliveries.Add(payload);
                return Task.CompletedTask;
            },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(deliveries.Count).IsEqualTo(2);
        await Assert.That(deliveries[0].TransactionId).IsEqualTo(42);
        await Assert.That(deliveries[0].Values[0].AsInt32()).IsEqualTo(99);
        await Assert.That(deliveries[0].ClientHandles.Length).IsEqualTo(1);
        await Assert.That(deliveries[0].Errors.All(static e => e == 0)).IsTrue();
    }

    [Test]
    public async Task TriggerDataChangeAsync_is_noop_when_callbacks_disabled()
    {
        var group = CreateGroup();
        IConnectionPoint cp = group;
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        IOPCAsyncIO2 async2 = group;
        await async2.SetEnableAsync(false, TestContext.Current!.CancellationToken);

        int callbackCount = 0;
        await group.TriggerDataChangeAsync(
            transactionId: 1,
            serverHandles: [],
            sender: (_, _, _) =>
            {
                Interlocked.Increment(ref callbackCount);
                return Task.CompletedTask;
            },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(callbackCount).IsEqualTo(0);
    }

    [Test]
    public async Task TriggerDataChangeAsync_skips_unknown_server_handles()
    {
        var group = CreateGroup();
        IConnectionPoint cp = group;
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        OpcDaGroup.DataChangePayload? captured = null;
        await group.TriggerDataChangeAsync(
            transactionId: 7,
            serverHandles: [99999],
            sender: (_, p, _) => { captured = p; return Task.CompletedTask; },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(captured).IsNotNull();
        await Assert.That(captured!.ClientHandles.Length).IsEqualTo(0);
        await Assert.That(captured.Values.Length).IsEqualTo(0);
    }

    private static async Task<int> AddSingleItem(OpcDaGroup group, string itemId)
    {
        var defs = new[] { new OpcItemDef("", itemId, true, 1, null, VarType.VT_I4) };
        await group.AddItemsAsync(defs, out OpcItemResult[] results, out int[] _,
            TestContext.Current!.CancellationToken);
        return results[0].ServerHandle;
    }

    private static OpcDaGroup CreateGroup() => new(
        name: "g",
        serverHandle: 1,
        clientHandle: 7,
        active: true,
        requestedUpdateRate: 1000,
        timeBias: 0,
        percentDeadband: 0f,
        localeId: 1033);
}
