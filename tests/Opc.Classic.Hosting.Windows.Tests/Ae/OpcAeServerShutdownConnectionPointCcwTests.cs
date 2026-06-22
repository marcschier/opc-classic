// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.Versioning;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Hosting.Windows;
using Opc.Classic.Hosting.Windows.Tests;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcAeServerShutdownConnectionPointCcwTests
{
    private const int S_OK = 0;

    [Test]
    public async Task Shutdown_connection_point_advises_sink_and_fires_on_server_shutdown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new ShutdownAeServer();
        IntPtr unknown = OpcAeServerCcw.Create(server, Guid.Parse("00000000-0000-0000-C000-000000000046"));
        IntPtr connectionPoint = ShutdownConnectionPointCcwTestHelpers.FindShutdownConnectionPoint(unknown);
        IntPtr sink = ShutdownConnectionPointCcwTestHelpers.CreateSinkStub();
        try
        {
            (int hr, uint cookie) = ShutdownConnectionPointCcwTestHelpers.Advise(connectionPoint, sink);
            server.RaiseShutdown();

            await Assert.That(hr).IsEqualTo(S_OK);
            await Assert.That(cookie).IsGreaterThan(0U);
            await Assert.That(ShutdownConnectionPointCcwTestHelpers.GetReasons(sink)).IsEquivalentTo([string.Empty]);

            int unadviseHr = ShutdownConnectionPointCcwTestHelpers.Unadvise(connectionPoint, cookie);
            server.RaiseShutdown();
            await Assert.That(unadviseHr).IsEqualTo(S_OK);
            await Assert.That(ShutdownConnectionPointCcwTestHelpers.GetReasons(sink)).IsEquivalentTo([string.Empty]);
        }
        finally
        {
            ShutdownConnectionPointCcwTestHelpers.DestroySinkStub(sink);
        }
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
}
