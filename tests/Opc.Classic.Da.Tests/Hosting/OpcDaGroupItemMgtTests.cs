//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Hosting;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for OpcDaGroup's IOPCItemMgt + IOPCSyncIO surface (ocom-3c).
/// </summary>
public sealed class OpcDaGroupItemMgtTests {
    [Test]
    public async Task AddItemsAsync_assigns_unique_server_handles() {
        var group = CreateGroup();
        var defs = new[]
        {
            new OpcItemDef("", "Tag.A", true, 1, null, VarType.VT_R8),
            new OpcItemDef("", "Tag.B", false, 2, null, VarType.VT_I4),
        };

        await group.AddItemsAsync(defs, out OpcItemResult[] results, out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(results.Length).IsEqualTo(2);
        await Assert.That(errors.All(static e => e == 0)).IsTrue();
        await Assert.That(results[0].ServerHandle).IsNotEqualTo(0);
        await Assert.That(results[1].ServerHandle).IsNotEqualTo(0);
        await Assert.That(results[0].ServerHandle).IsNotEqualTo(results[1].ServerHandle);
        await Assert.That(group.ItemCount).IsEqualTo(2);
    }

    [Test]
    public async Task AddItemsAsync_rejects_blank_item_id_with_unknown_item_id_hresult() {
        var group = CreateGroup();
        var defs = new[] { new OpcItemDef("", "", true, 1, null, VarType.VT_I4) };

        await group.AddItemsAsync(defs, out OpcItemResult[] results, out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsNotEqualTo(0);
        await Assert.That(results[0].ServerHandle).IsEqualTo(0);
        await Assert.That(group.ItemCount).IsEqualTo(0);
    }

    [Test]
    public async Task ValidateItemsAsync_does_not_add_items() {
        var group = CreateGroup();
        var defs = new[] { new OpcItemDef("", "Tag.A", true, 1, null, VarType.VT_I4) };

        await group.ValidateItemsAsync(defs, blobUpdate: false,
            out OpcItemResult[] _, out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(group.ItemCount).IsEqualTo(0);
    }

    [Test]
    public async Task RemoveItemsAsync_removes_known_handles_and_reports_invalid() {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");

        int[] errors = await group.RemoveItemsAsync([handle, 99999],
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(errors[1]).IsNotEqualTo(0);
        await Assert.That(group.ItemCount).IsEqualTo(0);
    }

    [Test]
    public async Task SetActiveStateAsync_updates_item_active_flag() {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A", active: false);

        int[] errors = await group.SetActiveStateAsync([handle], active: true,
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(group.GetItem(handle)!.Active).IsTrue();
    }

    [Test]
    public async Task SetClientHandlesAsync_updates_client_handles() {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");

        int[] errors = await group.SetClientHandlesAsync(
            [handle], [42],
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(group.GetItem(handle)!.ClientHandle).IsEqualTo(42);
    }

    [Test]
    public async Task SetDatatypesAsync_updates_requested_types() {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");

        int[] errors = await group.SetDatatypesAsync(
            [handle], [(ushort)VarType.VT_BSTR],
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(group.GetItem(handle)!.RequestedDatatype).IsEqualTo((ushort)VarType.VT_BSTR);
    }

    [Test]
    public async Task WriteAsync_then_ReadAsync_round_trips_value() {
        var group = CreateGroup();
        int handle = await AddSingleItem(group, "Tag.A");
        OpcVariant value = new(VarType.VT_I4, 42);

        int[] writeErrors = await group.WriteAsync([handle], [value],
            TestContext.Current!.CancellationToken);
        await Assert.That(writeErrors[0]).IsEqualTo(0);

        OpcItemState[] states = await group.ReadAsync(
            dataSource: 0,
            serverHandles: [handle],
            out int[] readErrors,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(readErrors[0]).IsEqualTo(0);
        await Assert.That(states[0].Value.AsInt32()).IsEqualTo(42);
    }

    [Test]
    public async Task ReadAsync_for_unknown_handle_returns_error() {
        var group = CreateGroup();

        OpcItemState[] states = await group.ReadAsync(
            dataSource: 0,
            serverHandles: [999],
            out int[] errors,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsNotEqualTo(0);
        await Assert.That(states[0].Value.IsEmpty).IsTrue();
    }

    [Test]
    public async Task WriteAsync_throws_on_length_mismatch() {
        var group = CreateGroup();

        await Assert.That(async () => {
            _ = await group.WriteAsync(
                [1, 2],
                [OpcVariant.Empty],
                TestContext.Current!.CancellationToken);
        }).Throws<ArgumentException>();
    }

    private static async Task<int> AddSingleItem(OpcDaGroup group, string itemId, bool active = true) {
        var defs = new[] { new OpcItemDef("", itemId, active, 1, null, VarType.VT_I4) };
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
