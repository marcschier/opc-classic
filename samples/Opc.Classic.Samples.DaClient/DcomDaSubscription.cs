// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Runtime.CompilerServices;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Samples.DaClient;

public sealed class DcomDaSubscription : IDaSubscription
{
    private const int CacheDataSource = 1;
    private const int DeviceDataSource = 2;

    private readonly List<DataChange> _changes = [];
    private readonly SemaphoreSlim _signal = new(0);
    private readonly Dictionary<int, ItemBinding> _items = new();
    private readonly IOPCServer _serverProxy;
    private readonly IOPCItemMgtClientProxy _itemMgtProxy;
    private readonly IOPCSyncIOClientProxy _syncIoProxy;
    private readonly int _serverGroupHandle;
    private bool _disposed;
    private int _nextTransaction;

    public DcomDaSubscription(
        IOPCServer serverProxy,
        IOPCItemMgtClientProxy itemMgtProxy,
        IOPCSyncIOClientProxy syncIoProxy,
        int serverGroupHandle,
        SubscriptionState state)
    {
        _serverProxy = serverProxy ?? throw new ArgumentNullException(nameof(serverProxy));
        _itemMgtProxy = itemMgtProxy ?? throw new ArgumentNullException(nameof(itemMgtProxy));
        _syncIoProxy = syncIoProxy ?? throw new ArgumentNullException(nameof(syncIoProxy));
        _serverGroupHandle = serverGroupHandle;
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    public SubscriptionState State { get; private set; }

    public IAsyncEnumerable<DataChange> DataChanges => ReadChangesAsync();

    public Task SetStateAsync(SubscriptionState state, CancellationToken cancellationToken = default)
    {
        State = state ?? throw new ArgumentNullException(nameof(state));
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<IdentifiedResult>> AddItemsAsync(
        IReadOnlyList<Item> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        await _itemMgtProxy.AddItemsAsync(ToItemDefinitions(items), out OpcItemResult[] addResults, out int[] errors, cancellationToken).ConfigureAwait(false);
        for (int index = 0; index < items.Count && index < addResults.Length; index++)
        {
            if (index >= errors.Length || new OpcResultId(errors[index], null).IsSuccess)
            {
                _items[items[index].ClientHandle] = new ItemBinding(items[index], addResults[index].ServerHandle);
            }
        }

        return ToIdentifiedResults(items, errors);
    }

    public async Task<IReadOnlyList<IdentifiedResult>> RemoveItemsAsync(
        IReadOnlyList<int> serverHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        int[] handles = ResolveServerHandles(serverHandles);
        int[] errors = await _itemMgtProxy.RemoveItemsAsync(handles, cancellationToken).ConfigureAwait(false);
        foreach (int handle in serverHandles)
        {
            _items.Remove(handle);
        }

        return ToIdentifiedResults(serverHandles.Select(static handle => new Item($"#{handle}")).ToArray(), errors);
    }

    public Task<IReadOnlyList<IdentifiedResult>> SetActiveStateAsync(
        IReadOnlyList<int> serverHandles,
        bool active,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        int[] handles = ResolveServerHandles(serverHandles);
        return SetActiveStateCoreAsync(serverHandles, handles, active, cancellationToken);
    }

    public async Task<IReadOnlyList<ItemValueResult>> ReadAsync(
        IReadOnlyList<int> serverHandles,
        bool fromCache,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        int[] handles = ResolveServerHandles(serverHandles);
        OpcItemState[] states = await _syncIoProxy.ReadAsync(fromCache ? CacheDataSource : DeviceDataSource, handles, out int[] errors, cancellationToken).ConfigureAwait(false);
        return ToValueResults(serverHandles, states, errors);
    }

    public async Task<IReadOnlyList<IdentifiedResult>> WriteAsync(
        IReadOnlyList<int> serverHandles,
        IReadOnlyList<object?> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);
        int[] handles = ResolveServerHandles(serverHandles);
        OpcVariant[] variants = values.Select(OpcVariantConverter.FromObject).ToArray();
        int[] errors = await _syncIoProxy.WriteAsync(handles, variants, cancellationToken).ConfigureAwait(false);
        return ToIdentifiedResults(serverHandles.Select(static handle => new Item($"#{handle}")).ToArray(), errors);
    }

    public async Task<int> RefreshAsync(bool fromCache, CancellationToken cancellationToken = default)
    {
        int transaction = Interlocked.Increment(ref _nextTransaction);
        IReadOnlyList<int> clientHandles = _items.Keys.ToArray();
        IReadOnlyList<ItemValueResult> values = await ReadAsync(clientHandles, fromCache, cancellationToken).ConfigureAwait(false);
        _changes.Add(new DataChange { TransactionId = transaction, Items = values });
        _signal.Release();
        return transaction;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _signal.Release();
        await _serverProxy.RemoveGroupAsync(_serverGroupHandle, force: true, CancellationToken.None).ConfigureAwait(false);
        _signal.Dispose();
    }

    private async Task<IReadOnlyList<IdentifiedResult>> SetActiveStateCoreAsync(
        IReadOnlyList<int> requestedHandles,
        int[] handles,
        bool active,
        CancellationToken cancellationToken)
    {
        int[] errors = await _itemMgtProxy.SetActiveStateAsync(handles, active, cancellationToken).ConfigureAwait(false);
        return ToIdentifiedResults(requestedHandles.Select(static handle => new Item($"#{handle}")).ToArray(), errors);
    }

    private int[] ResolveServerHandles(IReadOnlyList<int> handles) =>
        handles.Select(handle => _items.TryGetValue(handle, out ItemBinding binding) ? binding.ServerHandle : handle).ToArray();

    private IReadOnlyList<ItemValueResult> ToValueResults(IReadOnlyList<int> requestedHandles, OpcItemState[] states, int[] errors) =>
        requestedHandles.Select((handle, index) =>
        {
            ItemBinding binding = _items.TryGetValue(handle, out ItemBinding found)
                ? found
                : new ItemBinding(new Item($"#{handle}") { ClientHandle = handle }, handle);
            OpcItemState state = index < states.Length
                ? states[index]
                : new OpcItemState(handle, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Null);
            return new ItemValueResult(binding.Item.ItemName, binding.Item.Path)
            {
                ClientHandle = binding.Item.ClientHandle,
                Value = OpcVariantConverter.ToObject(state.Value),
                Quality = state.Quality,
                Timestamp = state.Timestamp,
                ResultId = new OpcResultId(index < errors.Length ? errors[index] : OpcResultId.Fail.Code, null),
            };
        }).ToArray();

    private async IAsyncEnumerable<DataChange> ReadChangesAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        while (true)
        {
            await _signal.WaitAsync(ct).ConfigureAwait(false);
            if (_changes.Count > 0)
            {
                DataChange change = _changes[0];
                _changes.RemoveAt(0);
                yield return change;
            }
            else if (_disposed)
            {
                yield break;
            }
        }
    }

    private static OpcItemDef[] ToItemDefinitions(IReadOnlyList<Item> items) =>
        items.Select(static item => new OpcItemDef(item.Path, item.ItemName, Active: true, item.ClientHandle, Blob: [], VarType.VT_EMPTY)).ToArray();

    private static IReadOnlyList<IdentifiedResult> ToIdentifiedResults(IReadOnlyList<ItemIdentifier> items, int[] errors) =>
        items.Select((item, index) => new IdentifiedResult(item)
        {
            ClientHandle = item is Item typedItem ? typedItem.ClientHandle : 0,
            ResultId = new OpcResultId(index < errors.Length ? errors[index] : OpcResultId.Fail.Code, null),
        }).ToArray();

    private readonly record struct ItemBinding(Item Item, int ServerHandle);
}
