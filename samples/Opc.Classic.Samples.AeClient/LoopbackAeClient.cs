// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Samples.AeClient;

internal sealed class LoopbackAeClient : IAeServer
{
    private static readonly Action<ILogger, Exception?> ConnectedMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(ConnectAsync)),
        "Connected to in-memory AE loopback server");

    private static readonly Action<ILogger, Exception?> DisconnectedMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2, nameof(DisconnectAsync)),
        "Disconnected from in-memory AE loopback server");

    private readonly IOPCEventServer _eventServerProxy;
    private readonly InProcessAeServer _server;
    private readonly ILogger<LoopbackAeClient> _logger;
    private bool _connected;

    public LoopbackAeClient(
        IOPCEventServer eventServerProxy,
        InProcessAeServer server,
        ILogger<LoopbackAeClient> logger)
    {
        _eventServerProxy = eventServerProxy ?? throw new ArgumentNullException(nameof(eventServerProxy));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _server.ServerShutdown += OnServerShutdown;
    }

    public event EventHandler<EventArgs>? ServerShutdown;

    public Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connected = true;
        ConnectedMessage(_logger, null);
        return Task.CompletedTask;
    }

    public ValueTask DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_connected)
        {
            DisconnectedMessage(_logger, null);
        }

        _connected = false;
        return ValueTask.CompletedTask;
    }

    public async Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return await _eventServerProxy.GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    public IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
        string areaQualifiedName,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return _server.BrowseAreasAsync(areaQualifiedName, cancellationToken);
    }

    public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(
        EventType eventTypes,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return _server.QueryEventCategoriesAsync(eventTypes, cancellationToken);
    }

    public Task<IReadOnlyList<string>> QueryConditionNamesAsync(
        uint eventCategory,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return _server.QueryConditionNamesAsync(eventCategory, cancellationToken);
    }

    public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
        string actor,
        string? comment,
        IReadOnlyList<ConditionRef> conditions,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return _server.AcknowledgeAsync(actor, comment, conditions, cancellationToken);
    }

    public Task<OpcResultId> EnableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return _server.EnableConditionsByAreaAsync(areaQualifiedNames, cancellationToken);
    }

    public Task<OpcResultId> DisableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return _server.DisableConditionsByAreaAsync(areaQualifiedNames, cancellationToken);
    }

    public Task<IAeSubscription> CreateSubscriptionAsync(
        bool active,
        int bufferTimeMs,
        int maxBufferSize,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        return _server.CreateSubscriptionAsync(active, bufferTimeMs, maxBufferSize, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        _server.ServerShutdown -= OnServerShutdown;
    }

    private void OnServerShutdown(object? sender, EventArgs args)
    {
        ServerShutdown?.Invoke(this, args);
    }

    private void ThrowIfDisconnected()
    {
        if (!_connected)
        {
            throw new InvalidOperationException("The in-memory AE loopback client is not connected.");
        }
    }
}
