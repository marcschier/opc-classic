//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for OpcDaGroup's per-item deadband + sampling rate impls
/// (cap-b4 + cap-b5). The group itself implements IOPCItemDeadbandMgt and
/// IOPCItemSamplingMgt by reading/writing the corresponding OpcDaItem state
/// fields. Default-impl behaviour (return-not-set-for-every-handle) is
/// preserved when no per-item override has been set.
/// </summary>
public sealed class OpcDaGroupItemStateTests {
    [Test]
    public async Task SetItemDeadband_then_GetItemDeadband_round_trips_through_item_state() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemDeadbandMgt mgr = group;

        int[] setErrors = await mgr.SetItemDeadbandAsync(new[] { handle }, new[] { 12.5f }, TestContext.Current!.CancellationToken);
        await mgr.GetItemDeadbandAsync(new[] { handle }, out float[] read, out int[] readErrors, TestContext.Current.CancellationToken);

        await Assert.That(setErrors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(readErrors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(read[0]).IsEqualTo(12.5f);
        await Assert.That(group.GetItem(handle)!.PercentDeadband).IsEqualTo(12.5f);
    }

    [Test]
    public async Task SetItemDeadband_with_out_of_range_value_returns_OPC_E_RANGE() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemDeadbandMgt mgr = group;

        int[] errors = await mgr.SetItemDeadbandAsync(new[] { handle }, new[] { 150f }, TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.Range.Code);
        await Assert.That(group.GetItem(handle)!.PercentDeadband).IsNull();
    }

    [Test]
    public async Task GetItemDeadband_before_set_returns_OPC_E_DEADBANDNOTSET() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemDeadbandMgt mgr = group;

        await mgr.GetItemDeadbandAsync(new[] { handle }, out _, out int[] errors, TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.DeadbandNotSet.Code);
    }

    [Test]
    public async Task ClearItemDeadband_resets_to_null() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemDeadbandMgt mgr = group;
        await mgr.SetItemDeadbandAsync(new[] { handle }, new[] { 5f }, TestContext.Current!.CancellationToken);

        int[] errors = await mgr.ClearItemDeadbandAsync(new[] { handle }, TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(group.GetItem(handle)!.PercentDeadband).IsNull();
    }

    [Test]
    public async Task SetItemDeadband_for_unknown_handle_returns_OPC_E_INVALIDHANDLE() {
        OpcDaGroup group = CreateGroup();
        IOPCItemDeadbandMgt mgr = group;

        int[] errors = await mgr.SetItemDeadbandAsync(new[] { 99999 }, new[] { 1f }, TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.InvalidHandle.Code);
    }

    [Test]
    public async Task SetItemSamplingRate_round_trips_through_item_state() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemSamplingMgt mgr = group;

        await mgr.SetItemSamplingRateAsync(new[] { handle }, new[] { 250 }, out int[] revised, out int[] setErrors, TestContext.Current!.CancellationToken);
        await mgr.GetItemSamplingRateAsync(new[] { handle }, out int[] read, out int[] readErrors, TestContext.Current.CancellationToken);

        await Assert.That(setErrors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(revised[0]).IsEqualTo(250);
        await Assert.That(readErrors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(read[0]).IsEqualTo(250);
    }

    [Test]
    public async Task GetItemSamplingRate_before_set_returns_OPC_E_RATENOTSET() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemSamplingMgt mgr = group;

        await mgr.GetItemSamplingRateAsync(new[] { handle }, out _, out int[] errors, TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.RateNotSet.Code);
    }

    [Test]
    public async Task ClearItemSamplingRate_resets_to_null() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemSamplingMgt mgr = group;
        await mgr.SetItemSamplingRateAsync(new[] { handle }, new[] { 100 }, out _, out _, TestContext.Current!.CancellationToken);

        int[] errors = await mgr.ClearItemSamplingRateAsync(new[] { handle }, TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(group.GetItem(handle)!.SamplingRate).IsNull();
    }

    [Test]
    public async Task SetItemBufferEnable_round_trips_through_item_state() {
        OpcDaGroup group = CreateGroup();
        int handle = await AddItem(group, "Tag.A");
        IOPCItemSamplingMgt mgr = group;

        int[] setErrors = await mgr.SetItemBufferEnableAsync(new[] { handle }, new[] { true }, TestContext.Current!.CancellationToken);
        await mgr.GetItemBufferEnableAsync(new[] { handle }, out bool[] read, out int[] readErrors, TestContext.Current.CancellationToken);

        await Assert.That(setErrors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(read[0]).IsTrue();
        await Assert.That(readErrors[0]).IsEqualTo(OpcResultId.Ok.Code);
    }

    private static async Task<int> AddItem(OpcDaGroup group, string itemId) {
        var defs = new[] { new OpcItemDef("", itemId, true, 1, null, VarType.VT_I4) };
        await group.AddItemsAsync(defs, out OpcItemResult[] results, out _, TestContext.Current!.CancellationToken);
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
