//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Hosting;

namespace Opc.Classic.Da.Hosting;

/// <summary>DA dispatcher adapter that delegates to source-generated OPC DA dispatchers.</summary>
public sealed class OpcDaServerDispatcher : IOpcDaServerDispatcher, IOPCCommon
{
    private static readonly Action<ILogger, string, Exception?> ClientNameSet = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(1, nameof(ClientNameSet)),
        "OPC DA client name set: {ClientName}");

    private readonly IOpcDaServer _server;
    private readonly IOPCServerServerDispatcher _serverDispatcher;
    private readonly IOPCCommonServerDispatcher _commonDispatcher;
    private readonly ConnectionDiagnostics _connectionContext = new();
    private readonly ILogger _logger;

    /// <summary>Initializes a new instance of the <see cref="OpcDaServerDispatcher" /> class.</summary>
    public OpcDaServerDispatcher(IOpcDaServer server, ILogger? logger = null)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _logger = logger ?? NullLogger.Instance;
        _serverDispatcher = new IOPCServerServerDispatcher(_server);
        _commonDispatcher = new IOPCCommonServerDispatcher(this);
    }

    /// <summary>Gets the latest client name supplied through <c>IOPCCommon::SetClientName</c>.</summary>
    public string ClientName => _connectionContext.ClientName;

    internal IOpcServerDispatcher ServerDispatcher => _serverDispatcher;

    internal IOpcServerDispatcher CommonDispatcher => _commonDispatcher;

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId == IOPCServer.InterfaceId)
        {
            return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        if (interfaceId == IOPCCommon.InterfaceId)
        {
            return (await _commonDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
    }

    /// <inheritdoc />
    public Task SetLocaleIdAsync(int localeId, CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.SetLocaleIdAsync(localeId, cancellationToken)
            : NotImplementedAsync();

    /// <inheritdoc />
    public Task<int> GetLocaleIdAsync(CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.GetLocaleIdAsync(cancellationToken)
            : NotImplementedAsync<int>();

    /// <inheritdoc />
    public Task<int[]> QueryAvailableLocaleIdsAsync(CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.QueryAvailableLocaleIdsAsync(cancellationToken)
            : NotImplementedAsync<int[]>();

    /// <inheritdoc />
    public Task<string> GetErrorStringAsync(int errorCode, CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.GetErrorStringAsync(errorCode, cancellationToken)
            : _server.GetErrorStringAsync(errorCode, localeId: 0, cancellationToken);

    /// <inheritdoc />
    public Task SetClientNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();
        _connectionContext.ClientName = name;
        ClientNameSet(_logger, name, null);

        return _server is IDaServer daServer
            ? daServer.SetClientNameAsync(name, cancellationToken)
            : Task.CompletedTask;
    }

    private static Task NotImplementedAsync() =>
        Task.FromException(new OpcException(OpcResultId.NotImplemented));

    private static Task<T> NotImplementedAsync<T>() =>
        Task.FromException<T>(new OpcException(OpcResultId.NotImplemented));

    private sealed class ConnectionDiagnostics
    {
        public string ClientName { get; set; } = string.Empty;
    }
}
