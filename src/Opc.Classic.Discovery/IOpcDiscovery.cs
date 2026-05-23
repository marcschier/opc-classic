//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Generic;
using System.Threading;

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
