// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class OpcDaGroupEnumerationSnapshotTests
{
    [Test]
    [Arguments(OpcDaGroupEnumerationScope.PrivateConnections, 1, true, true, false)]
    [Arguments(OpcDaGroupEnumerationScope.PublicConnections, 2, true, false, true)]
    [Arguments(OpcDaGroupEnumerationScope.AllConnections, 3, true, true, true)]
    [Arguments(OpcDaGroupEnumerationScope.Private, 4, false, true, false)]
    [Arguments(OpcDaGroupEnumerationScope.Public, 5, false, false, true)]
    [Arguments(OpcDaGroupEnumerationScope.All, 6, false, true, true)]
    public async Task Scope_matches_wire_ABI_and_classification(
        OpcDaGroupEnumerationScope scope,
        int wireValue,
        bool connections,
        bool includesPrivate,
        bool includesPublic)
    {
        await Assert.That((int)scope).IsEqualTo(wireValue);
        await Assert.That(OpcDaGroupEnumerationScopeExtensions.FromWireValue(wireValue)).IsEqualTo(scope);
        await Assert.That(scope.IsConnectionScope()).IsEqualTo(connections);
        await Assert.That(scope.IsNameScope()).IsEqualTo(!connections);
        await Assert.That(scope.IncludesPrivateGroups()).IsEqualTo(includesPrivate);
        await Assert.That(scope.IncludesPublicGroups()).IsEqualTo(includesPublic);
    }

    [Test]
    [Arguments(OpcDaGroupEnumerationScope.PrivateConnections, "private", 1, 0)]
    [Arguments(OpcDaGroupEnumerationScope.PublicConnections, "public", 0, 1)]
    [Arguments(OpcDaGroupEnumerationScope.AllConnections, "private|public", 1, 1)]
    [Arguments(OpcDaGroupEnumerationScope.Private, "private", 1, 0)]
    [Arguments(OpcDaGroupEnumerationScope.Public, "public", 0, 1)]
    [Arguments(OpcDaGroupEnumerationScope.All, "private|public", 1, 1)]
    public async Task All_six_scopes_select_expected_groups(
        OpcDaGroupEnumerationScope scope,
        string expectedNames,
        int expectedPrivateCount,
        int expectedPublicCount)
    {
        IOpcDaServer server = new SnapshotServer(
            [CreateGroup("private", 1)],
            [CreateGroup("public", 2)]);

        OpcDaGroupEnumerationSnapshot snapshot = await server.CreateGroupEnumerationSnapshotAsync(
            scope,
            TestContext.Current!.CancellationToken);

        await Assert.That(snapshot.PrivateGroups.Count).IsEqualTo(expectedPrivateCount);
        await Assert.That(snapshot.PublicGroups.Count).IsEqualTo(expectedPublicCount);
        await Assert.That(string.Join("|", snapshot.Names)).IsEqualTo(expectedNames);
    }

    [Test]
    public async Task Snapshot_remains_immutable_after_mutation()
    {
        OpcDaGroup first = CreateGroup("first", 1);
        OpcDaGroup second = CreateGroup("second", 2);
        var serverImpl = new SnapshotServer([first, second], []);
        IOpcDaServer server = serverImpl;

        OpcDaGroupEnumerationSnapshot snapshot = await server.CreateGroupEnumerationSnapshotAsync(
            OpcDaGroupEnumerationScope.All,
            TestContext.Current!.CancellationToken);
        serverImpl.ReplacePrivate(CreateGroup("third", 3));
        await second.SetNameAsync("renamed", TestContext.Current!.CancellationToken);

        await Assert.That(snapshot.Groups[0]).IsSameReferenceAs(first);
        await Assert.That(snapshot.Groups[1]).IsSameReferenceAs(second);
        await Assert.That(string.Join("|", snapshot.Names)).IsEqualTo("first|second");
        _ = await Assert.ThrowsAsync<NotSupportedException>(
            () => Task.Run(() => ((IList<OpcDaGroup>)snapshot.Groups).Clear()));
    }

    [Test]
    [Arguments(OpcDaGroupEnumerationScope.AllConnections)]
    [Arguments(OpcDaGroupEnumerationScope.All)]
    public async Task All_scope_uses_one_atomic_combined_snapshot(OpcDaGroupEnumerationScope scope)
    {
        var serverImpl = new MutationWindowServer();
        IOpcDaServer server = serverImpl;

        OpcDaGroupEnumerationSnapshot snapshot = await server.CreateGroupEnumerationSnapshotAsync(
            scope,
            TestContext.Current!.CancellationToken);

        await Assert.That(serverImpl.SeparateSnapshotCalls).IsEqualTo(0);
        await Assert.That(serverImpl.CombinedSnapshotCalls).IsEqualTo(1);
        await Assert.That(string.Join("|", snapshot.Names)).IsEqualTo("private-1|public-1");
    }

    [Test]
    public async Task Concurrent_pair_mutation_never_mixes_generations()
    {
        var serverImpl = new AtomicPairSnapshotServer();
        IOpcDaServer server = serverImpl;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        using var start = new Barrier(2);
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        Task writer = Task.Run(async () =>
        {
            start.SignalAndWait(cts.Token);
            for (int generation = 1; generation <= 2_000; generation++)
            {
                serverImpl.ReplacePair(generation);
                if ((generation & 15) == 0)
                {
                    await Task.Yield();
                }
            }
        }, cts.Token);
        Task reader = ReadAtomicPairsAsync(server, start, cts.Token);

        await Task.WhenAll(writer, reader);
    }

    [Test]
    public async Task Invalid_scope_and_cancellation_are_rejected()
    {
        IOpcDaServer server = new SnapshotServer([], []);
        _ = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => server.CreateGroupEnumerationSnapshotAsync((OpcDaGroupEnumerationScope)0));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        _ = await Assert.ThrowsAsync<OperationCanceledException>(
            () => server.CreateGroupEnumerationSnapshotAsync(OpcDaGroupEnumerationScope.All, cts.Token));
    }

    private static Task ReadAtomicPairsAsync(
        IOpcDaServer server,
        Barrier start,
        CancellationToken cancellationToken) =>
        Task.Run(async () =>
        {
            start.SignalAndWait(cancellationToken);
            for (int i = 0; i < 2_000; i++)
            {
                OpcDaGroupEnumerationSnapshot snapshot = await server.CreateGroupEnumerationSnapshotAsync(
                    OpcDaGroupEnumerationScope.All,
                    cancellationToken);
                string privateGeneration = snapshot.Names[0]["private-".Length..];
                string publicGeneration = snapshot.Names[1]["public-".Length..];
                await Assert.That(privateGeneration).IsEqualTo(publicGeneration);
            }
        }, cancellationToken);

    private static OpcDaGroup CreateGroup(string name, int handle) => new(
        name,
        handle,
        handle,
        active: true,
        requestedUpdateRate: 1000,
        timeBias: 0,
        percentDeadband: 0,
        localeId: 1033);

    private abstract class ServerBase : IOpcDaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus());

        public Task<int> AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(
            int serverGroupHandle,
            bool force,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<string> GetErrorStringAsync(
            int errorCode,
            int localeId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(string.Empty);

        public abstract Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default);

        public abstract Task<IReadOnlyList<OpcDaGroup>> SnapshotPublicGroupsAsync(
            CancellationToken cancellationToken = default);

        public abstract Task<OpcDaGroupSetSnapshot> SnapshotAllGroupsAsync(
            CancellationToken cancellationToken = default);
    }

    private sealed class SnapshotServer : ServerBase
    {
        private readonly Lock _lock = new();
        private readonly List<OpcDaGroup> _privateGroups;
        private readonly List<OpcDaGroup> _publicGroups;

        public SnapshotServer(
            IEnumerable<OpcDaGroup> privateGroups,
            IEnumerable<OpcDaGroup> publicGroups)
        {
            _privateGroups = [.. privateGroups];
            _publicGroups = [.. publicGroups];
        }

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_lock)
            {
                return Task.FromResult<IReadOnlyList<OpcDaGroup>>([.. _privateGroups]);
            }
        }

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPublicGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_lock)
            {
                return Task.FromResult<IReadOnlyList<OpcDaGroup>>([.. _publicGroups]);
            }
        }

        public override Task<OpcDaGroupSetSnapshot> SnapshotAllGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_lock)
            {
                return Task.FromResult(new OpcDaGroupSetSnapshot(_privateGroups, _publicGroups));
            }
        }

        public void ReplacePrivate(OpcDaGroup group)
        {
            lock (_lock)
            {
                _privateGroups.Clear();
                _privateGroups.Add(group);
            }
        }
    }

    private sealed class MutationWindowServer : ServerBase
    {
        private int _generation = 1;

        public int SeparateSnapshotCalls;
        public int CombinedSnapshotCalls;

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref SeparateSnapshotCalls);
            int generation = _generation++;
            return Task.FromResult<IReadOnlyList<OpcDaGroup>>(
                [CreateGroup($"private-{generation}", generation)]);
        }

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPublicGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref SeparateSnapshotCalls);
            int generation = _generation;
            return Task.FromResult<IReadOnlyList<OpcDaGroup>>(
                [CreateGroup($"public-{generation}", -generation)]);
        }

        public override Task<OpcDaGroupSetSnapshot> SnapshotAllGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref CombinedSnapshotCalls);
            int generation = _generation;
            return Task.FromResult(new OpcDaGroupSetSnapshot(
                [CreateGroup($"private-{generation}", generation)],
                [CreateGroup($"public-{generation}", -generation)]));
        }
    }

    private sealed class AtomicPairSnapshotServer : ServerBase
    {
        private readonly Lock _lock = new();
        private OpcDaGroup _privateGroup = CreateGroup("private-0", 1);
        private OpcDaGroup _publicGroup = CreateGroup("public-0", -1);

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OpcDaGroup>>([_privateGroup]);

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPublicGroupsAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<OpcDaGroup>>([_publicGroup]);

        public override Task<OpcDaGroupSetSnapshot> SnapshotAllGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_lock)
            {
                return Task.FromResult(new OpcDaGroupSetSnapshot(
                    [_privateGroup],
                    [_publicGroup]));
            }
        }

        public void ReplacePair(int generation)
        {
            lock (_lock)
            {
                _privateGroup = CreateGroup($"private-{generation}", generation);
                _publicGroup = CreateGroup($"public-{generation}", -generation);
            }
        }
    }
}
