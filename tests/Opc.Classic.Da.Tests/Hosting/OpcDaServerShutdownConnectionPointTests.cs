//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class OpcDaServerShutdownConnectionPointTests
{
    [Test]
    public async Task Shutdown_connection_point_advises_sink_and_fires_on_server_shutdown()
    {
        var server = new ShutdownDaServer();
        var dispatcher = new OpcDaServerDispatcher(server);
        var container = (IConnectionPointContainer)dispatcher;
        var connectionPoint = (IConnectionPoint)dispatcher;
        var sink = new RecordingShutdownSink();

        IOpcInterfaceRef pointRef = await container.FindConnectionPointAsync(IOPCShutdown.InterfaceId, CancellationToken.None);
        int cookie = await connectionPoint.AdviseAsync(new OpcDaShutdownSinkRef(sink), CancellationToken.None);
        server.RaiseShutdown("maintenance");

        await Assert.That(pointRef.Iid).IsEqualTo(IConnectionPoint.InterfaceId);
        await Assert.That(await connectionPoint.GetConnectionInterfaceAsync(CancellationToken.None)).IsEqualTo(IOPCShutdown.InterfaceId);
        await Assert.That(cookie).IsGreaterThan(0);
        await Assert.That(sink.Reasons).IsEquivalentTo(["maintenance"]);

        await connectionPoint.UnadviseAsync(cookie, CancellationToken.None);
        server.RaiseShutdown("ignored");
        await Assert.That(sink.Reasons).IsEquivalentTo(["maintenance"]);
    }

    private sealed class ShutdownDaServer : IOpcDaServer, IDaServer
    {
        public event EventHandler<ServerShutdownEventArgs>? ServerShutdown;

        public void RaiseShutdown(string reason) =>
            ServerShutdown?.Invoke(this, new ServerShutdownEventArgs { Reason = reason });

        public int LocaleId => 0;

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<int>>([0]);

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult(resultId.ToString());

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Da });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.Empty);

        public async IAsyncEnumerable<BrowseElement> BrowseAsync(string itemPath, BrowseFilters filters = BrowseFilters.All, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = itemPath;
            _ = filters;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemValueResult>>([]);

        public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>([]);

        public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>([]);

        public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(IReadOnlyList<ItemIdentifier> itemIds, IReadOnlyList<PropertyID> propertyIds, bool returnValues, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemPropertyResult>>([]);

        public Task<IDaSubscription> CreateSubscriptionAsync(SubscriptionState state, CancellationToken cancellationToken = default) =>
            throw new OpcException(OpcResultId.NotImplemented);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class RecordingShutdownSink : IOPCShutdown
    {
        public List<string> Reasons { get; } = [];

        public Task ShutdownRequestAsync(string reason, CancellationToken cancellationToken = default)
        {
            Reasons.Add(reason);
            return Task.CompletedTask;
        }
    }
}
