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
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for OpcDaGroup's IConnectionPoint subscription wireup + the
/// TriggerDataChangeAsync fan-out helper (ocom-7b).
/// </summary>
public sealed class OpcDaGroupSubscriptionTests {
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
    public async Task GetConnectionInterfaceAsync_returns_IID_IOPCDataCallback() {
        var group = CreateGroup();
        IConnectionPoint cp = group;

        Guid iid = await cp.GetConnectionInterfaceAsync(TestContext.Current!.CancellationToken);

        await Assert.That(iid).IsEqualTo(IOPCDataCallback.InterfaceId);
    }

    [Test]
    public async Task AdviseAsync_returns_unique_cookies() {
        var group = CreateGroup();
        IConnectionPoint cp = group;

        int cookie1 = await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);
        int cookie2 = await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        await Assert.That(cookie1).IsNotEqualTo(cookie2);
        await Assert.That(group.SubscriptionCount).IsEqualTo(2);
    }

    [Test]
    public async Task UnadviseAsync_removes_subscription() {
        var group = CreateGroup();
        IConnectionPoint cp = group;
        int cookie = await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        await cp.UnadviseAsync(cookie, TestContext.Current!.CancellationToken);

        await Assert.That(group.SubscriptionCount).IsEqualTo(0);
    }

    [Test]
    public async Task TriggerDataChangeAsync_invokes_sender_once_per_sink() {
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
            sender: (sink, payload, ct) => {
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
    public async Task TriggerDataChangeAsync_is_noop_when_callbacks_disabled() {
        var group = CreateGroup();
        IConnectionPoint cp = group;
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        IOPCAsyncIO2 async2 = group;
        await async2.SetEnableAsync(false, TestContext.Current!.CancellationToken);

        int callbackCount = 0;
        await group.TriggerDataChangeAsync(
            transactionId: 1,
            serverHandles: [],
            sender: (_, _, _) => {
                Interlocked.Increment(ref callbackCount);
                return Task.CompletedTask;
            },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(callbackCount).IsEqualTo(0);
    }

    [Test]
    public async Task TriggerDataChangeAsync_skips_unknown_server_handles() {
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

    [Test]
    public async Task IConnectionPointContainer_FindConnectionPointAsync_returns_connection_point_for_DataCallback() {
        var group = CreateGroup();
        IConnectionPointContainer container = group;

        var iref = await container.FindConnectionPointAsync(
            IOPCDataCallback.InterfaceId,
            TestContext.Current!.CancellationToken);

        await Assert.That(iref.Iid).IsEqualTo(IConnectionPoint.InterfaceId);
        await Assert.That(iref.Ipid).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task IConnectionPointContainer_FindConnectionPointAsync_throws_for_unknown_iid() {
        var group = CreateGroup();
        IConnectionPointContainer container = group;

        await Assert.That(async () => {
            _ = await container.FindConnectionPointAsync(
                Guid.NewGuid(),
                TestContext.Current!.CancellationToken);
        }).Throws<Opc.Classic.OpcException>();
    }

    [Test]
    public async Task IConnectionPointContainer_EnumConnectionPointsAsync_returns_interface_ref() {
        var group = CreateGroup();
        IConnectionPointContainer container = group;

        var iref = await container.EnumConnectionPointsAsync(TestContext.Current!.CancellationToken);

        await Assert.That(iref.Ipid).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task UnadviseAsync_with_unknown_cookie_throws_CONNECT_E_NOCONNECTION() {
        var group = CreateGroup();
        IConnectionPoint cp = group;

        OpcException? ex = await Assert.That(async () => {
            await cp.UnadviseAsync(cookie: 99999, TestContext.Current!.CancellationToken);
        }).Throws<OpcException>();
        await Assert.That(ex).IsNotNull();
        await Assert.That(ex!.ResultId.Code).IsEqualTo(unchecked((int)0x80040200));
    }

    [Test]
    public async Task Cancel2Async_records_cancel_id_for_subsequent_TriggerCancelComplete() {
        var group = CreateGroup();
        IOPCAsyncIO2 async2 = group;

        await async2.Cancel2Async(cancelId: 4242, TestContext.Current!.CancellationToken);

        await Assert.That(group.LastCancel2Id).IsEqualTo(4242);
    }

    [Test]
    public async Task TriggerCancelCompleteAsync_invokes_sender_once_per_sink() {
        var group = CreateGroup();
        IConnectionPoint cp = group;
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        var payloads = new List<OpcDaGroup.CancelCompletePayload>();
        await group.TriggerCancelCompleteAsync(
            transactionId: 99,
            sender: (_, p, _) => { payloads.Add(p); return Task.CompletedTask; },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(payloads.Count).IsEqualTo(2);
        await Assert.That(payloads[0].TransactionId).IsEqualTo(99);
        await Assert.That(payloads[0].GroupHandle).IsEqualTo(7);
    }

    [Test]
    public async Task TriggerCancelCompleteAsync_is_noop_when_callbacks_disabled() {
        var group = CreateGroup();
        IConnectionPoint cp = group;
        await cp.AdviseAsync(SampleSink, TestContext.Current!.CancellationToken);

        IOPCAsyncIO2 async2 = group;
        await async2.SetEnableAsync(false, TestContext.Current!.CancellationToken);

        int callbacks = 0;
        await group.TriggerCancelCompleteAsync(
            transactionId: 1,
            sender: (_, _, _) => { Interlocked.Increment(ref callbacks); return Task.CompletedTask; },
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(callbacks).IsEqualTo(0);
    }

    [Test]
    public async Task AdviseAsync_IOpcDataCallbackSink_overload_adds_to_directSinks_and_TriggerDataChange_invokes_OnDataChange() {
        // cap-c8: TriggerDataChangeAsync fans out to both _sinks (IOpcInterfaceRef
        // path) and _directSinks (IOpcDataCallbackSink path) so the Windows
        // CCW's OpcDataCallbackProxy participates in the same fan-out.
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");
        await group.WriteAsync([handle], [new OpcVariant(VarType.VT_I4, 42)], TestContext.Current!.CancellationToken);

        var directSink = new RecordingDataCallbackSink();
        int directCookie = await group.AdviseAsync(directSink, TestContext.Current.CancellationToken);

        await group.TriggerDataChangeAsync(
            transactionId: 5,
            serverHandles: [handle],
            sender: (_, _, _) => Task.CompletedTask,
            cancellationToken: TestContext.Current.CancellationToken);

        await Assert.That(directCookie).IsGreaterThan(0);
        await Assert.That(directSink.DataChangeCount).IsEqualTo(1);
        await Assert.That(directSink.LastDataChange!.TransactionId).IsEqualTo(5);
    }

    [Test]
    public async Task TriggerCancelCompleteAsync_invokes_OnCancelComplete_on_direct_sinks() {
        var group = CreateGroup();
        var directSink = new RecordingDataCallbackSink();
        await group.AdviseAsync(directSink, TestContext.Current!.CancellationToken);

        await group.TriggerCancelCompleteAsync(
            transactionId: 99,
            sender: (_, _, _) => Task.CompletedTask,
            cancellationToken: TestContext.Current.CancellationToken);

        await Assert.That(directSink.CancelCompleteCount).IsEqualTo(1);
        await Assert.That(directSink.LastCancelComplete!.TransactionId).IsEqualTo(99);
    }

    [Test]
    public async Task UnadviseAsync_removes_direct_sink_so_subsequent_TriggerDataChange_does_not_invoke_it() {
        var group = CreateGroup();
        var directSink = new RecordingDataCallbackSink();
        int cookie = await group.AdviseAsync(directSink, TestContext.Current!.CancellationToken);

        IConnectionPoint cp = group;
        await cp.UnadviseAsync(cookie, TestContext.Current.CancellationToken);

        await group.TriggerDataChangeAsync(
            transactionId: 1,
            serverHandles: [],
            sender: (_, _, _) => Task.CompletedTask,
            cancellationToken: TestContext.Current.CancellationToken);

        await Assert.That(directSink.DataChangeCount).IsEqualTo(0);
    }

    [Test]
    public async Task AdviseAsync_IOpcDataCallbackSink_with_null_throws_ArgumentNullException() {
        var group = CreateGroup();
        await Assert.That(async () => await group.AdviseAsync(
            (IOpcDataCallbackSink)null!,
            TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
    }

    private sealed class RecordingDataCallbackSink : IOpcDataCallbackSink {
        public int DataChangeCount { get; private set; }

        public int ReadCompleteCount { get; private set; }

        public int WriteCompleteCount { get; private set; }

        public int CancelCompleteCount { get; private set; }

        public OpcDaGroup.DataChangePayload? LastDataChange { get; private set; }

        public OpcDaGroup.CancelCompletePayload? LastCancelComplete { get; private set; }

        public void OnDataChange(OpcDaGroup.DataChangePayload payload) {
            DataChangeCount++;
            LastDataChange = payload;
        }

        public void OnReadComplete(OpcDaGroup.DataChangePayload payload) => ReadCompleteCount++;

        public void OnWriteComplete(int transactionId, int groupHandle, int masterError, int[] clientHandles, int[] errors) =>
            WriteCompleteCount++;

        public void OnCancelComplete(OpcDaGroup.CancelCompletePayload payload) {
            CancelCompleteCount++;
            LastCancelComplete = payload;
        }

        public void Dispose() {
            // Recording sink owns no native resources.
        }
    }

    private static async Task<int> AddSingleItem(OpcDaGroup group, string itemId) {
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
