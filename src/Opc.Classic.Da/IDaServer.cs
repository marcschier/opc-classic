//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da;

/// <summary>
/// The managed async-first OPC DA server contract — what a consumer of this
/// stack uses to talk to a remote OPC DA server (or to an in-process
/// managed server hosted via <c>Opc.Classic.Hosting</c>).
/// </summary>
/// <remarks>
/// <para>
/// All methods are async with explicit <see cref="CancellationToken"/>s.
/// Implementations may be cross-platform (over <c>Opc.Classic.Dcom</c>) or
/// Windows-only (over Windows COM RCWs); the contract is identical either way.
/// </para>
/// <para>
/// The interface intentionally diverges from the legacy <c>Opc.Da.IServer</c>
/// shape in three ways:
/// </para>
/// <list type="number">
///   <item><description>Async signatures (<see cref="Task{T}"/> / <see cref="IAsyncEnumerable{T}"/>) replace synchronous ones.</description></item>
///   <item><description>Cancellation tokens flow through every operation.</description></item>
///   <item><description>Browse returns an <see cref="IAsyncEnumerable{T}"/> — the consumer doesn't manage <see cref="BrowsePosition"/> tokens manually.</description></item>
/// </list>
/// </remarks>
public interface IDaServer : IAsyncDisposable
{
    /// <summary>
    /// Raised when the server emits <c>IOPCShutdown::ShutdownRequest</c>.
    /// </summary>
    event EventHandler<ServerShutdownEventArgs>? ServerShutdown;

    /// <summary>
    /// Negotiated server LCID — the locale the server is serving messages in.
    /// </summary>
    int LocaleId { get; }

    /// <summary>
    /// Retrieve server runtime state (start time, current time, version, ...).
    /// </summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Set the active locale for subsequent server-supplied messages.
    /// Throws <see cref="OpcDaException"/> if the LCID is unsupported.
    /// </summary>
    Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// List the locale IDs the server supports.
    /// </summary>
    Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolve an OPC HRESULT to the server's human-readable text in the current locale.
    /// </summary>
    Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Supply a client name that servers may use for diagnostics and logging.
    /// </summary>
    Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientName);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Browse the server's address space starting at <paramref name="itemPath"/>
    /// (use empty string for the root). Implementations stream results;
    /// continuations are handled internally.
    /// </summary>
    IAsyncEnumerable<BrowseElement> BrowseAsync(
        string itemPath,
        BrowseFilters filters = BrowseFilters.All,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// One-shot read using DA 3.0's stateless <c>IOPCItemIO::Read</c>. For
    /// long-lived item bindings prefer
    /// <see cref="CreateSubscriptionAsync(SubscriptionState, CancellationToken)"/>.
    /// </summary>
    Task<IReadOnlyList<ItemValueResult>> ReadAsync(
        IReadOnlyList<Item> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// One-shot write using DA 3.0's stateless <c>IOPCItemIO::WriteVQT</c>.
    /// </summary>
    Task<IReadOnlyList<IdentifiedResult>> WriteAsync(
        IReadOnlyList<ItemValue> values,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate that <paramref name="items"/> exist on the server (no side effects).
    /// </summary>
    Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(
        IReadOnlyList<Item> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetch the requested <see cref="PropertyID"/>s for each of
    /// <paramref name="itemIds"/>. Pass <paramref name="returnValues"/>=true to
    /// inline the property values; otherwise only metadata is returned.
    /// </summary>
    Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(
        IReadOnlyList<ItemIdentifier> itemIds,
        IReadOnlyList<PropertyID> propertyIds,
        bool returnValues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a server-side subscription (group) and return its handle.
    /// </summary>
    Task<IDaSubscription> CreateSubscriptionAsync(
        SubscriptionState state,
        CancellationToken cancellationToken = default);
}
