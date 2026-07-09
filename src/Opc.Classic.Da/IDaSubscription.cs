// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// A live subscription (server-side OPC DA group) — created by
/// <see cref="IDaServer.CreateSubscriptionAsync(SubscriptionState, CancellationToken)"/>.
/// Disposing cancels the subscription and removes the group from the server.
/// </summary>
/// <remarks>
/// Each subscription instance owns one server-side group and one client-side
/// callback sink (<c>IOPCDataCallback</c>). The <see cref="DataChanges"/>
/// stream is the managed-async wrapper around <c>OnDataChange</c>; iterating
/// it pulls each batched callback as a <see cref="DataChange"/>.
/// </remarks>
public interface IDaSubscription : IAsyncDisposable
{
    /// <summary>
    /// Current state of the subscription (server-confirmed).
    /// </summary>
    SubscriptionState State { get; }

    /// <summary>
    /// The per-callback stream of pushed data updates from the server.
    /// </summary>
    IAsyncEnumerable<DataChange> DataChanges { get; }

    /// <summary>
    /// Update the subscription's state on the server (rate, active, deadband, ...).
    /// </summary>
    Task SetStateAsync(SubscriptionState state, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add items to the subscription. Returns per-item add results.
    /// </summary>
    Task<IReadOnlyList<IdentifiedResult>> AddItemsAsync(
        IReadOnlyList<Item> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove previously-added items by their server-assigned handles.
    /// </summary>
    Task<IReadOnlyList<IdentifiedResult>> RemoveItemsAsync(
        IReadOnlyList<int> serverHandles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate or deactivate a subset of items. Deactivated items stop firing
    /// OnDataChange callbacks but remain in the subscription.
    /// </summary>
    Task<IReadOnlyList<IdentifiedResult>> SetActiveStateAsync(
        IReadOnlyList<int> serverHandles,
        bool active,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synchronously read the current values of <paramref name="serverHandles"/>.
    /// When <paramref name="fromCache"/> is true (DA's default), values come from
    /// the server's cache; otherwise the server reads from the underlying device.
    /// </summary>
    Task<IReadOnlyList<ItemValueResult>> ReadAsync(
        IReadOnlyList<int> serverHandles,
        bool fromCache,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Write to a subset of subscribed items. Use <see cref="IDaServer.WriteAsync"/>
    /// for one-shot writes that don't justify creating a subscription.
    /// </summary>
    Task<IReadOnlyList<IdentifiedResult>> WriteAsync(
        IReadOnlyList<int> serverHandles,
        IReadOnlyList<object?> values,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Force a refresh — server immediately sends an OnDataChange for all
    /// active items, regardless of whether the values changed.
    /// Returns the transaction ID; the corresponding <see cref="DataChange"/>
    /// will arrive on <see cref="DataChanges"/> with that ID.
    /// </summary>
    Task<int> RefreshAsync(bool fromCache, CancellationToken cancellationToken = default);
}
