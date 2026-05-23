//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hosting;

namespace Opc.Classic.Hda.Hosting;

/// <summary>
/// HDA-specific <see cref="IOpcServerHost"/> implementation for managed in-process servers.
/// </summary>
public sealed class OpcHdaServerHost : IOpcServerHost
{
    private static readonly Action<ILogger, Guid, string, Exception?> StartingHost = LoggerMessage.Define<Guid, string>(
        LogLevel.Information,
        new EventId(1, nameof(StartingHost)),
        "OpcHdaServerHost starting: CLSID={Clsid}, ProgId={ProgId}");

    private static readonly Action<ILogger, Guid, Exception?> StoppingHost = LoggerMessage.Define<Guid>(
        LogLevel.Information,
        new EventId(2, nameof(StoppingHost)),
        "OpcHdaServerHost stopping: CLSID={Clsid}");

    private readonly IOpcHdaServer _serverImpl;
    private readonly OpcHdaServerOptions _options;
    private readonly ILogger<OpcHdaServerHost> _logger;
    private CancellationTokenSource? _acceptCts;
    private Task? _acceptTask;

    /// <summary>Initializes a new instance of the <see cref="OpcHdaServerHost"/> class.</summary>
    public OpcHdaServerHost(
        IOpcHdaServer serverImpl,
        IOptions<OpcHdaServerOptions> options,
        ILogger<OpcHdaServerHost> logger)
    {
        _serverImpl = serverImpl ?? throw new ArgumentNullException(nameof(serverImpl));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string SpecName => "HDA";

    /// <inheritdoc />
    public OpcClsidRegistration Registration => new(
        Clsid: _options.Clsid,
        ProgId: _options.ProgId,
        AssemblyName: typeof(IOpcHdaServer).Assembly.GetName().Name ?? "Opc.Classic.Hda",
        TypeName: _serverImpl.GetType().FullName ?? "Unknown",
        FriendlyName: _options.FriendlyName);

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartingHost(_logger, _options.Clsid, _options.ProgId, null);

        var dispatcher = new IOPCHDA_ServerServerDispatcher(_serverImpl);
        _acceptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _acceptTask = AcceptConnectionsAsync(dispatcher, _acceptCts.Token);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        StoppingHost(_logger, _options.Clsid, null);

        CancellationTokenSource? acceptCts = _acceptCts;
        Task? acceptTask = _acceptTask;
        _acceptCts = null;
        _acceptTask = null;

        if (acceptCts is not null)
        {
            await acceptCts.CancelAsync().ConfigureAwait(false);
        }

        if (acceptTask is not null)
        {
            try
            {
                await acceptTask.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (acceptCts?.IsCancellationRequested == true)
            {
            }
        }

        acceptCts?.Dispose();
    }

    private static async Task AcceptConnectionsAsync(
        IOpcServerDispatcher dispatcher,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);

        await Task.Delay(TimeSpan.Zero, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
    }
}
