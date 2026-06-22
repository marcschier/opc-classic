// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class OpcDaGroupTests
{
    [Test]
    public async Task Constructor_initializes_state_from_creation_parameters()
    {
        var group = new OpcDaGroup(
            name: "G1",
            serverHandle: 42,
            clientHandle: 7,
            active: true,
            requestedUpdateRate: 1000,
            timeBias: -300,
            percentDeadband: 5.5f,
            localeId: 1033);

        await Assert.That(group.Name).IsEqualTo("G1");
        await Assert.That(group.ServerHandle).IsEqualTo(42);
        await Assert.That(group.ClientHandle).IsEqualTo(7);
        await Assert.That(group.Active).IsTrue();
        await Assert.That(group.UpdateRate).IsEqualTo(1000);
        await Assert.That(group.TimeBias).IsEqualTo(-300);
        await Assert.That(group.PercentDeadband).IsEqualTo(5.5f);
        await Assert.That(group.LocaleId).IsEqualTo(1033);
        await Assert.That(group.KeepAliveTime).IsEqualTo(0);
    }

    [Test]
    public async Task GetStateAsync_returns_current_snapshot()
    {
        var group = CreateGroup();

        OpcGroupState state = await group.GetStateAsync(TestContext.Current!.CancellationToken);

        await Assert.That(state.ServerHandle).IsEqualTo(group.ServerHandle);
        await Assert.That(state.ClientHandle).IsEqualTo(group.ClientHandle);
        await Assert.That(state.Name).IsEqualTo(group.Name);
        await Assert.That(state.Active).IsEqualTo(group.Active);
        await Assert.That(state.UpdateRate).IsEqualTo(group.UpdateRate);
        await Assert.That(state.TimeBias).IsEqualTo(group.TimeBias);
        await Assert.That(state.PercentDeadband).IsEqualTo(group.PercentDeadband);
        await Assert.That(state.LocaleId).IsEqualTo(group.LocaleId);
    }

    [Test]
    public async Task SetStateAsync_updates_all_fields()
    {
        var group = CreateGroup();
        await group.SetStateAsync(
            requestedUpdateRate: 2000,
            active: false,
            timeBias: 60,
            percentDeadband: 1f,
            localeId: 2052,
            clientGroupHandle: 99,
            out int revisedUpdateRate,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(group.UpdateRate).IsEqualTo(2000);
        await Assert.That(group.Active).IsFalse();
        await Assert.That(group.TimeBias).IsEqualTo(60);
        await Assert.That(group.PercentDeadband).IsEqualTo(1f);
        await Assert.That(group.LocaleId).IsEqualTo(2052);
        await Assert.That(group.ClientHandle).IsEqualTo(99);
        await Assert.That(revisedUpdateRate).IsEqualTo(2000);
    }

    [Test]
    public async Task SetNameAsync_changes_group_name()
    {
        var group = CreateGroup();

        await group.SetNameAsync("Renamed", TestContext.Current!.CancellationToken);

        await Assert.That(group.Name).IsEqualTo("Renamed");
    }

    [Test]
    public async Task SetKeepAliveAsync_returns_previous_value_and_updates_state()
    {
        var group = CreateGroup();

        int previous = await group.SetKeepAliveAsync(5000, TestContext.Current!.CancellationToken);
        int current = await group.GetKeepAliveAsync(TestContext.Current!.CancellationToken);

        await Assert.That(previous).IsEqualTo(0);
        await Assert.That(current).IsEqualTo(5000);
    }

    [Test]
    public async Task CloneGroupAsync_returns_interface_ref_with_requested_iid()
    {
        var group = CreateGroup();
        Guid iid = Guid.NewGuid();

        var clone = await group.CloneGroupAsync("clone", iid, TestContext.Current!.CancellationToken);

        await Assert.That(clone.Iid).IsEqualTo(iid);
        await Assert.That(clone.Ipid).IsNotEqualTo(Guid.Empty);
    }

    private static OpcDaGroup CreateGroup() => new(
        name: "test-group",
        serverHandle: 100,
        clientHandle: 1,
        active: true,
        requestedUpdateRate: 1000,
        timeBias: 0,
        percentDeadband: 0f,
        localeId: 1033);
}
