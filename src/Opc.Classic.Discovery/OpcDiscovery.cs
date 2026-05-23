//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Discovery;

/// <summary>
/// Convenience entry points for OPC Classic server discovery.
/// </summary>
public static class OpcDiscovery
{
    /// <summary>
    /// Enumerates OPC servers on <paramref name="host" /> through OPCEnum / OPC.ServerList.1.
    /// </summary>
    public static Task<OpcServerDescriptor[]> EnumerateAsync(
        string host,
        IEnumerable<Guid>? categories = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        var client = new OpcEnumClient(host, new DcomOpcEnumCallChannelFactory(), categories);
        return client.EnumerateAsync(host, categories, cancellationToken);
    }
}
