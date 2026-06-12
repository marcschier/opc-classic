//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Hosting.Windows;
using Opc.Classic.Testing;

namespace Opc.Classic.Da.Tests;

public sealed class SetClientNameTests
{
    [Test]
    public async Task Client_round_trip_delivers_name_to_server()
    {
        var server = new RecordingDaServer();
        var dispatcher = new OpcDaServerDispatcher(server);
        var channel = new InMemoryCallChannel((iid, opnum, payload, cancellationToken) =>
            dispatcher.DispatchAsync(iid, opnum, payload, cancellationToken));
        await using var client = new LoopbackDaServerClient(channel);

        await client.SetClientNameAsync(ReadFirstClientName(), CancellationToken.None);

        await Assert.That(server.ClientName).IsEqualTo(ReadFirstClientName());
        await Assert.That(dispatcher.ClientName).IsEqualTo(ReadFirstClientName());
    }

    [Test]
    public async Task Multiple_calls_update_stored_name()
    {
        var server = new RecordingDaServer();
        var dispatcher = new OpcDaServerDispatcher(server);
        var channel = new InMemoryCallChannel((iid, opnum, payload, cancellationToken) =>
            dispatcher.DispatchAsync(iid, opnum, payload, cancellationToken));
        await using var client = new LoopbackDaServerClient(channel);

        await client.SetClientNameAsync(ReadFirstClientName(), CancellationToken.None);
        await client.SetClientNameAsync(ReadSecondClientName(), CancellationToken.None);

        await Assert.That(server.ClientName).IsEqualTo(ReadSecondClientName());
        await Assert.That(dispatcher.ClientName).IsEqualTo(ReadSecondClientName());
    }

    [Test]
    public async Task Default_implementer_noop_succeeds()
    {
        IDaServer server = new DefaultDaServer();

        bool completed = false;
        await server.SetClientNameAsync(ReadFirstClientName(), CancellationToken.None);
        completed = true;

        await Assert.That(completed).IsTrue();
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public async Task Windows_ccw_SetClientName_forwards_to_managed_server()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new RecordingDaServer();
        IntPtr ccw = OpcDaServerCcw.Create(server, IOPCCommon.InterfaceId);

        int hr = InvokeCcwSetClientName(ccw, ReadCcwClientName());

        await Assert.That(hr).IsEqualTo(0);
        await Assert.That(server.ClientName).IsEqualTo(ReadCcwClientName());
    }

    private static string ReadFirstClientName() => "diagnostic-client";
    private static string ReadSecondClientName() => "updated-client";
    private static string ReadCcwClientName() => "ccw-client";

    private static unsafe int InvokeCcwSetClientName(IntPtr ccw, string clientName)
    {
        IntPtr* vtable = *(IntPtr**)ccw;
        var setClientName = (delegate* unmanaged<IntPtr, IntPtr, int>)vtable[7];
        IntPtr namePtr = Marshal.StringToCoTaskMemUni(clientName);
        try
        {
            return setClientName(ccw, namePtr);
        }
        finally
        {
            Marshal.FreeCoTaskMem(namePtr);
        }
    }

    private sealed class LoopbackDaServerClient : IDaServer
    {
        private readonly IOPCCommonClientProxy _commonProxy;

        public LoopbackDaServerClient(InMemoryCallChannel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);
            _commonProxy = new IOPCCommonClientProxy(channel);
        }

        public event EventHandler<ServerShutdownEventArgs>? ServerShutdown
        {
            add { }
            remove { }
        }

        public int LocaleId => 0;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Da });

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult(resultId.Description ?? string.Empty);

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default) =>
            _commonProxy.SetClientNameAsync(clientName, cancellationToken);

        public async IAsyncEnumerable<BrowseElement> BrowseAsync(
            string itemPath,
            BrowseFilters filters = BrowseFilters.All,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = itemPath; _ = filters;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemValueResult>>(Array.Empty<ItemValueResult>());

        public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(
            IReadOnlyList<ItemIdentifier> itemIds,
            IReadOnlyList<PropertyID> propertyIds,
            bool returnValues,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemPropertyResult>>(Array.Empty<ItemPropertyResult>());

        public Task<IDaSubscription> CreateSubscriptionAsync(SubscriptionState state, CancellationToken cancellationToken = default) =>
            Task.FromResult<IDaSubscription>(new EmptySubscription());

        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;
    }

    private sealed class RecordingDaServer : IOpcDaServer, IDaServer
    {
        public string ClientName { get; private set; } = string.Empty;

        public event EventHandler<ServerShutdownEventArgs>? ServerShutdown
        {
            add { }
            remove { }
        }

        public int LocaleId => 0;

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            ClientName = clientName;
            return Task.CompletedTask;
        }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Da });

        public Task<int> AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult(resultId.Description ?? string.Empty);

        public async IAsyncEnumerable<BrowseElement> BrowseAsync(
            string itemPath,
            BrowseFilters filters = BrowseFilters.All,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = itemPath; _ = filters;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemValueResult>>(Array.Empty<ItemValueResult>());

        public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(
            IReadOnlyList<ItemIdentifier> itemIds,
            IReadOnlyList<PropertyID> propertyIds,
            bool returnValues,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemPropertyResult>>(Array.Empty<ItemPropertyResult>());

        public Task<IDaSubscription> CreateSubscriptionAsync(SubscriptionState state, CancellationToken cancellationToken = default) =>
            Task.FromResult<IDaSubscription>(new EmptySubscription());

        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;
    }

    private sealed class DefaultDaServer : IDaServer
    {
        public event EventHandler<ServerShutdownEventArgs>? ServerShutdown
        {
            add { }
            remove { }
        }

        public int LocaleId => 0;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Da });

        public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<int>>(Array.Empty<int>());

        public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
            Task.FromResult(resultId.Description ?? string.Empty);

        public async IAsyncEnumerable<BrowseElement> BrowseAsync(
            string itemPath,
            BrowseFilters filters = BrowseFilters.All,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            _ = itemPath; _ = filters;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }

        public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemValueResult>>(Array.Empty<ItemValueResult>());

        public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(
            IReadOnlyList<ItemIdentifier> itemIds,
            IReadOnlyList<PropertyID> propertyIds,
            bool returnValues,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemPropertyResult>>(Array.Empty<ItemPropertyResult>());

        public Task<IDaSubscription> CreateSubscriptionAsync(SubscriptionState state, CancellationToken cancellationToken = default) =>
            Task.FromResult<IDaSubscription>(new EmptySubscription());

        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;
    }

    private sealed class EmptySubscription : IDaSubscription
    {
        public SubscriptionState State => new();
        public IAsyncEnumerable<DataChange> DataChanges => EmptyDataChanges();

        public Task SetStateAsync(SubscriptionState state, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IReadOnlyList<IdentifiedResult>> AddItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<IdentifiedResult>> RemoveItemsAsync(IReadOnlyList<int> serverHandles, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<IdentifiedResult>> SetActiveStateAsync(IReadOnlyList<int> serverHandles, bool active, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<int> serverHandles, bool fromCache, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<ItemValueResult>>(Array.Empty<ItemValueResult>());

        public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<int> serverHandles, IReadOnlyList<object?> values, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

        public Task<int> RefreshAsync(bool fromCache, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public ValueTask DisposeAsync() =>
            ValueTask.CompletedTask;

        private static async IAsyncEnumerable<DataChange> EmptyDataChanges()
        {
            await Task.CompletedTask.ConfigureAwait(false);
            yield break;
        }
    }
}
