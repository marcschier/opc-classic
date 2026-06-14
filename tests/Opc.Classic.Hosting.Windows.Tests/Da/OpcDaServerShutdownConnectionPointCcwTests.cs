//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.Versioning;
using Opc.Classic.Da;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using Opc.Classic.Hosting.Windows.Tests;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcDaServerShutdownConnectionPointCcwTests
{
    private const int S_OK = 0;

    [Test]
    public async Task Shutdown_connection_point_advises_sink_and_fires_on_server_shutdown()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new ShutdownDaServer();
        IntPtr unknown = OpcDaServerCcw.Create(server, Guid.Parse("00000000-0000-0000-C000-000000000046"));
        IntPtr connectionPoint = ShutdownConnectionPointCcwTestHelpers.FindShutdownConnectionPoint(unknown);
        IntPtr sink = ShutdownConnectionPointCcwTestHelpers.CreateSinkStub();
        try
        {
            (int hr, uint cookie) = ShutdownConnectionPointCcwTestHelpers.Advise(connectionPoint, sink);
            server.RaiseShutdown("maintenance");

            await Assert.That(hr).IsEqualTo(S_OK);
            await Assert.That(cookie).IsGreaterThan(0U);
            await Assert.That(ShutdownConnectionPointCcwTestHelpers.GetReasons(sink)).IsEquivalentTo(["maintenance"]);

            int unadviseHr = ShutdownConnectionPointCcwTestHelpers.Unadvise(connectionPoint, cookie);
            server.RaiseShutdown("ignored");
            await Assert.That(unadviseHr).IsEqualTo(S_OK);
            await Assert.That(ShutdownConnectionPointCcwTestHelpers.GetReasons(sink)).IsEquivalentTo(["maintenance"]);
        }
        finally
        {
            ShutdownConnectionPointCcwTestHelpers.DestroySinkStub(sink);
        }
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
}
