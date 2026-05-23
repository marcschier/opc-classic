//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace OpcClassic.Ae.Hosting;

/// <summary>Contract implemented by user code to provide an in-process managed AE server.</summary>
public interface IOpcAeServer
{
    /// <summary>Gets the AE server runtime status snapshot.</summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the AE filter mask supported by the server.</summary>
    Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default);
}
