// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Discovery;

/// <summary>
/// Discovers OPC Classic servers from a local or remote source.
/// </summary>
public interface IOpcDiscovery
{
    /// <summary>
    /// Enumerates discovered OPC Classic server registrations.
    /// </summary>
    IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        CancellationToken cancellationToken = default);
}
