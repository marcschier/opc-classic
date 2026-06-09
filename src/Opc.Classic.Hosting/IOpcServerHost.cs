//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Hosting;

/// <summary>
/// Contract every per-spec server host (DA, AE, HDA, ...) implements.
/// The hosting infrastructure manages lifecycle via Microsoft.Extensions.Hosting.IHostedService.
/// </summary>
public interface IOpcServerHost {
    /// <summary>Gets the OPC Classic specification name implemented by this host.</summary>
    string SpecName { get; }

    /// <summary>Gets the COM class registration metadata for this host.</summary>
    OpcClsidRegistration Registration { get; }

    /// <summary>Starts the server host.</summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>Stops the server host.</summary>
    Task StopAsync(CancellationToken cancellationToken);
}
