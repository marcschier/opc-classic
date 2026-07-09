// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dx;

/// <summary>
/// The managed async-first OPC DX server contract (the <c>IOPCConfiguration</c>
/// surface from OPC DX 1.0).
/// </summary>
/// <remarks>
/// DX is fundamentally a configuration interface — it manipulates connection
/// metadata; it does NOT push runtime data. The actual data flow happens
/// inside the DX server as it bridges between source servers and the local
/// target server.
/// </remarks>
public interface IDxServer : IAsyncDisposable
{
    /// <summary>
    /// Raised when the server emits a shutdown notification.
    /// </summary>
    event EventHandler<EventArgs>? ServerShutdown;

    /// <summary>
    /// Retrieve DX server runtime state.
    /// </summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// List source servers registered with this DX server.
    /// </summary>
    Task<IReadOnlyList<DxSourceServer>> GetSourceServersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Register or update a source server.
    /// </summary>
    Task<OpcResultId> AddOrUpdateSourceServerAsync(
        DxSourceServer source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a source server (and all connections that referenced it).
    /// </summary>
    Task<OpcResultId> RemoveSourceServerAsync(
        string sourceServerName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List connections matching the optional name filter (empty = all).
    /// </summary>
    Task<IReadOnlyList<DxConnection>> GetConnectionsAsync(
        string? nameFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new connection or update an existing one by <see cref="DxConnection.Name"/>.
    /// </summary>
    Task<OpcResultId> AddOrUpdateConnectionAsync(
        DxConnection connection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove a connection by name.
    /// </summary>
    Task<OpcResultId> RemoveConnectionAsync(
        string connectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset (clear) all configured connections and source servers.
    /// </summary>
    Task<OpcResultId> ResetConfigurationAsync(
        CancellationToken cancellationToken = default);
}
