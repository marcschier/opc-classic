// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for OpcDaGroup's IOPCSyncIO2 + IOPCAsyncIO2/3 surface (ocom-3d).
/// </summary>
public sealed class OpcDaGroupAsyncIoTests
{
    [Test]
    public async Task ReadMaxAge_returns_current_snapshot()
    {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");
        await group.WriteAsync([handle], [new OpcVariant(VarType.VT_I4, 99)],
            TestContext.Current!.CancellationToken);

        IOPCSyncIO2 syncIo2 = group;
        await syncIo2.ReadMaxAgeAsync(
            [handle],
            [0],
            out OpcVariant[] values,
            out ushort[] qualities,
            out long[] timestamps,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(values[0].AsInt32()).IsEqualTo(99);
        await Assert.That(qualities[0]).IsNotEqualTo((ushort)0);
        await Assert.That(timestamps[0]).IsGreaterThan(0L);
    }

    [Test]
    public async Task WriteVqt_updates_value_with_supplied_quality_and_timestamp()
    {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");
        var ts = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero);
        var vqt = new OpcItemVqt(
            Value: new OpcVariant(VarType.VT_I4, 77),
            Quality: new OpcQuality(0x00C0),
            Timestamp: ts);

        IOPCSyncIO2 syncIo2 = group;
        int[] errors = await syncIo2.WriteVqtAsync([handle], [vqt],
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(0);
        OpcItemState snapshot = group.GetItem(handle)!.GetSnapshot();
        await Assert.That(snapshot.Value.AsInt32()).IsEqualTo(77);
        await Assert.That(snapshot.Timestamp).IsEqualTo(ts);
    }

    [Test]
    public async Task AsyncIO2_ReadAsync_returns_cancel_id_and_per_item_errors()
    {
        var group = CreateGroup();
        int knownHandle = await AddSingleItem(group, "Tag.A");

        IOPCAsyncIO2 async2 = group;
        int cancelId = await async2.ReadAsync(
            [knownHandle, 99999],
            transactionId: 1,
            out int[] errors,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(cancelId).IsGreaterThan(0);
        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(errors[1]).IsNotEqualTo(0);
    }

    [Test]
    public async Task AsyncIO2_WriteAsync_updates_item_and_returns_cancel_id()
    {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");

        IOPCAsyncIO2 async2 = group;
        int cancelId = await async2.WriteAsync(
            [handle],
            [new OpcVariant(VarType.VT_I4, 123)],
            transactionId: 2,
            out int[] errors,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(cancelId).IsGreaterThan(0);
        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(group.GetItem(handle)!.GetSnapshot().Value.AsInt32()).IsEqualTo(123);
    }

    [Test]
    public async Task SetEnable_then_GetEnable_round_trips()
    {
        var group = CreateGroup();
        IOPCAsyncIO2 async2 = group;

        await async2.SetEnableAsync(enabled: false, TestContext.Current!.CancellationToken);
        bool enabled1 = await async2.GetEnableAsync(TestContext.Current!.CancellationToken);
        await async2.SetEnableAsync(enabled: true, TestContext.Current!.CancellationToken);
        bool enabled2 = await async2.GetEnableAsync(TestContext.Current!.CancellationToken);

        await Assert.That(enabled1).IsFalse();
        await Assert.That(enabled2).IsTrue();
    }

    [Test]
    public async Task Refresh2_and_Cancel2_succeed()
    {
        var group = CreateGroup();
        IOPCAsyncIO2 async2 = group;

        int cancelId = await async2.Refresh2Async(dataSource: 1, transactionId: 7,
            TestContext.Current!.CancellationToken);
        await async2.Cancel2Async(cancelId, TestContext.Current!.CancellationToken);

        await Assert.That(cancelId).IsGreaterThan(0);
    }

    [Test]
    public async Task AsyncIO3_ReadMaxAge_returns_cancel_id_and_per_item_errors()
    {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");

        int cancelId = await ((IOPCAsyncIO3)group).ReadMaxAgeAsync(
            [handle],
            [1000],
            transactionId: 3,
            out int[] errors,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(cancelId).IsGreaterThan(0);
        await Assert.That(errors[0]).IsEqualTo(0);
    }

    [Test]
    public async Task AsyncIO3_WriteVqt_updates_item_with_VQT()
    {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");
        var vqt = new OpcItemVqt(new OpcVariant(VarType.VT_I4, 555));

        int cancelId = await ((IOPCAsyncIO3)group).WriteVqtAsync(
            [handle],
            [vqt],
            transactionId: 4,
            out int[] errors,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(cancelId).IsGreaterThan(0);
        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(group.GetItem(handle)!.GetSnapshot().Value.AsInt32()).IsEqualTo(555);
    }

    [Test]
    public async Task AsyncIO3_RefreshMaxAge_returns_cancel_id()
    {
        var group = CreateGroup();

        int cancelId = await ((IOPCAsyncIO3)group).RefreshMaxAgeAsync(
            maxAge: 1000,
            transactionId: 5,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(cancelId).IsGreaterThan(0);
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
