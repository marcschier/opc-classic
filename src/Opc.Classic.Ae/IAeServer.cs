// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Ae;

/// <summary>
/// The managed async-first OPC AE server contract.
/// </summary>
public interface IAeServer : IAsyncDisposable
{
    /// <summary>
    /// Raised when the server emits <c>IOPCShutdown::ShutdownRequest</c>.
    /// </summary>
    event EventHandler<EventArgs>? ServerShutdown;

    /// <summary>
    /// Retrieve AE server runtime state.
    /// </summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Negotiated server LCID for localized AE server strings.
    /// </summary>
    int LocaleId => 0;

    /// <summary>
    /// Set the active locale for subsequent server-supplied strings.
    /// </summary>
    Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default)
    {
        _ = localeId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <summary>
    /// List the locale IDs the server supports.
    /// </summary>
    Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<int>>(new[] { LocaleId });
    }

    /// <summary>
    /// Resolve an HRESULT to the server's human-readable text in the current locale.
    /// </summary>
    Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(resultId.ToString());
    }

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
    /// Browse the event-area namespace starting at <paramref name="areaQualifiedName"/>.
    /// Empty string = root. Servers stream the result lazily.
    /// </summary>
    IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
        string areaQualifiedName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List event categories the server supports for the given event types.
    /// </summary>
    Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(
        EventType eventTypes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List the conditions the server defines for the given category.
    /// </summary>
    Task<IReadOnlyList<string>> QueryConditionNamesAsync(
        uint eventCategory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledge a batch of condition events.
    /// </summary>
    Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
        string actor,
        string? comment,
        IReadOnlyList<ConditionRef> conditions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable monitoring of conditions for an area / source.
    /// </summary>
    Task<OpcResultId> EnableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable monitoring of conditions for an area / source.
    /// </summary>
    Task<OpcResultId> DisableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a server-side event subscription and return its handle.
    /// </summary>
    Task<IAeSubscription> CreateSubscriptionAsync(
        bool active,
        int bufferTimeMs,
        int maxBufferSize,
        CancellationToken cancellationToken = default);
}
