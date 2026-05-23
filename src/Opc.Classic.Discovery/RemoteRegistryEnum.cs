//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading;

namespace Opc.Classic.Discovery;

/// <summary>
/// Placeholder for remote-registry OPC Classic discovery over SMB.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "The Phase 10 public API intentionally names discovery strategies after OPC enumeration sources.")]
public sealed class RemoteRegistryEnum : IOpcDiscovery
{
    /// <summary>
    /// Initializes the remote-registry discovery scaffold.
    /// </summary>
    public RemoteRegistryEnum(string host, NetworkCredential credentials)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(credentials);

        Host = host;
        Credentials = credentials;
    }

    /// <summary>The remote host whose registry will be enumerated.</summary>
    public string Host { get; }

    /// <summary>The credentials that will authenticate to the remote registry transport.</summary>
    public NetworkCredential Credentials { get; }

    /// <inheritdoc />
    [SuppressMessage("Design", "MA0025:Implement the functionality", Justification = "This Phase 10B scaffold must throw NotImplementedException until the SharpCifs replacement lands.")]
    public IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Phase 10B follow-up: enumerate HKLM\\SOFTWARE\\Classes\\Component Categories\\{CATID}\\Implementations\\ via SharpInterop Remote Registry once Phase 2D SharpCifs replacement lands.");
    }
}
