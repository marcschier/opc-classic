//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Hosting;

/// <summary>
/// Hosted service that starts and stops all registered OPC Classic server hosts.
/// </summary>
public sealed class ClassicHostedService : IHostedService {
    private static readonly Action<ILogger, string, string, Exception?> StartingServer = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(1, nameof(StartingServer)),
        "Starting OPC {SpecName} server {ProgId}");

    private static readonly Action<ILogger, string, string, Exception?> StoppingServer = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        new EventId(2, nameof(StoppingServer)),
        "Stopping OPC {SpecName} server {ProgId}");

    private readonly IEnumerable<IOpcServerHost> _hosts;
    private readonly ILogger<ClassicHostedService> _logger;

    /// <summary>Initializes a new instance of the <see cref="ClassicHostedService"/> class.</summary>
    public ClassicHostedService(
        IEnumerable<IOpcServerHost> hosts,
        ILogger<ClassicHostedService> logger) {
        _hosts = hosts ?? throw new ArgumentNullException(nameof(hosts));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken) {
        foreach (var host in _hosts) {
            LogStartingServer(host);
            await host.StartAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken) {
        foreach (var host in _hosts) {
            LogStoppingServer(host);
            await host.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    private void LogStartingServer(IOpcServerHost host) {
        if (_logger.IsEnabled(LogLevel.Information)) {
            StartingServer(_logger, host.SpecName, host.Registration.ProgId, null);
        }
    }

    private void LogStoppingServer(IOpcServerHost host) {
        if (_logger.IsEnabled(LogLevel.Information)) {
            StoppingServer(_logger, host.SpecName, host.Registration.ProgId, null);
        }
    }
}
