// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Dcom;

namespace Opc.Classic.Ae.Tests.Hosting;

public sealed class OpcAeServerShutdownConnectionPointTests
{
    [Test]
    public async Task Shutdown_connection_point_advises_sink_and_fires_on_server_shutdown()
    {
        var server = new ShutdownAeServer();
        var dispatcher = new OpcAeServerDispatcher(server);
        var container = (IConnectionPointContainer)dispatcher;
        var connectionPoint = (IConnectionPoint)dispatcher;
        var sink = new RecordingShutdownSink();

        IOpcInterfaceRef pointRef = await container.FindConnectionPointAsync(IOPCShutdown.InterfaceId, CancellationToken.None);
        int cookie = await connectionPoint.AdviseAsync(new OpcAeShutdownSinkRef(sink), CancellationToken.None);
        server.RaiseShutdown();

        await Assert.That(pointRef.Iid).IsEqualTo(IConnectionPoint.InterfaceId);
        await Assert.That(await connectionPoint.GetConnectionInterfaceAsync(CancellationToken.None)).IsEqualTo(IOPCShutdown.InterfaceId);
        await Assert.That(cookie).IsGreaterThan(0);
        await Assert.That(sink.Reasons).IsEquivalentTo([string.Empty]);

        await connectionPoint.UnadviseAsync(cookie, CancellationToken.None);
        server.RaiseShutdown();
        await Assert.That(sink.Reasons).IsEquivalentTo([string.Empty]);
    }

    private sealed class ShutdownAeServer : IOpcAeServer, IAeServer
    {
        public event EventHandler<EventArgs>? ServerShutdown;

        public void RaiseShutdown() => ServerShutdown?.Invoke(this, EventArgs.Empty);

        public int LocaleId => 0;

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<int>>([0]);

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult(resultId.ToString());

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Ae });

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

        public async IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(string areaQualifiedName, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = areaQualifiedName;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(EventType eventTypes, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<uint>>([]);

        public Task<IReadOnlyList<string>> QueryConditionNamesAsync(uint eventCategory, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(string actor, string? comment, IReadOnlyList<ConditionRef> conditions, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AckResult>>([]);

        public Task<OpcResultId> EnableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default) =>
            Task.FromResult(OpcResultId.Ok);

        public Task<OpcResultId> DisableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default) =>
            Task.FromResult(OpcResultId.Ok);

        public Task<IAeSubscription> CreateSubscriptionAsync(bool active, int bufferTimeMs, int maxBufferSize, CancellationToken cancellationToken = default) =>
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
