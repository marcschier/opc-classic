//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

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
