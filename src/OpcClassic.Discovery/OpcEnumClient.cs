//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace OpcClassic.Discovery;

/// <summary>
/// Placeholder for OpcEnum (OPC.ServerList.1) DCOM server discovery.
/// </summary>
public sealed class OpcEnumClient : IOpcDiscovery
{
    /// <summary>
    /// Initializes the OpcEnum discovery scaffold.
    /// </summary>
    public OpcEnumClient(OpcClassic.OpcUrl serverListUrl)
    {
        ArgumentNullException.ThrowIfNull(serverListUrl);

        ServerListUrl = serverListUrl;
    }

    /// <summary>The OpcEnum server-list endpoint URL.</summary>
    public OpcClassic.OpcUrl ServerListUrl { get; }

    /// <inheritdoc />
    [SuppressMessage("Design", "MA0025:Implement the functionality", Justification = "This Phase 10A scaffold must throw NotImplementedException until IOPCServerList shims land.")]
    public IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Phase 10A follow-up: enumerate via OPC.ServerList.1 (CLSID_OpcServerList) using IOPCServerList shims once Phase 6B per-method NDR bodies are applied to IOPCServerList.");
    }
}
