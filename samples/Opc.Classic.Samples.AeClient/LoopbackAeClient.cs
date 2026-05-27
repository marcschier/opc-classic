// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Runtime.CompilerServices;
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
        "Connected to AE sample server");

    private static readonly Action<ILogger, Exception?> DisconnectedMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(2, nameof(DisconnectAsync)),
        "Disconnected from AE sample server");

    private readonly IOPCEventServer _eventServerProxy;
    private readonly ILogger<LoopbackAeClient> _logger;
    private readonly InProcessAeServer? _server;
    private bool _connected;

    public LoopbackAeClient(IOPCEventServer eventServerProxy, ILogger<LoopbackAeClient> logger)
    {
        _eventServerProxy = eventServerProxy ?? throw new ArgumentNullException(nameof(eventServerProxy));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public LoopbackAeClient(
        IOPCEventServer eventServerProxy,
        InProcessAeServer server,
        ILogger<LoopbackAeClient> logger)
        : this(eventServerProxy, logger)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
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

    public async IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
        string areaQualifiedName,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        if (_server is not null)
        {
            await foreach (AreaBrowseElement element in _server.BrowseAreasAsync(areaQualifiedName, cancellationToken).ConfigureAwait(false))
            {
                yield return element;
            }
        }
    }

    public async Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(
        EventType eventTypes,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        if (_server is not null)
        {
            return await _server.QueryEventCategoriesAsync(eventTypes, cancellationToken).ConfigureAwait(false);
        }

        await _eventServerProxy.QueryEventCategoriesAsync((int)eventTypes, out int[] categories, out _, cancellationToken).ConfigureAwait(false);
        return categories.Select(static category => unchecked((uint)category)).ToArray();
    }

    public async Task<IReadOnlyList<string>> QueryConditionNamesAsync(
        uint eventCategory,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        if (_server is not null)
        {
            return await _server.QueryConditionNamesAsync(eventCategory, cancellationToken).ConfigureAwait(false);
        }

        return await _eventServerProxy.QueryConditionNamesAsync(unchecked((int)eventCategory), cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
        string actor,
        string? comment,
        IReadOnlyList<ConditionRef> conditions,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        if (_server is not null)
        {
            return await _server.AcknowledgeAsync(actor, comment, conditions, cancellationToken).ConfigureAwait(false);
        }

        int[] errors = await _eventServerProxy.AckConditionAsync(
            actor,
            comment ?? string.Empty,
            new long[conditions.Count],
            new int[conditions.Count],
            conditions.Select(static condition => condition.Source).ToArray(),
            conditions.Select(static condition => condition.ConditionName).ToArray(),
            cancellationToken).ConfigureAwait(false);
        return ToAckResults(conditions, errors);
    }

    public async Task<OpcResultId> EnableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        if (_server is not null)
        {
            return await _server.EnableConditionsByAreaAsync(areaQualifiedNames, cancellationToken).ConfigureAwait(false);
        }

        await _eventServerProxy.EnableConditionByAreaAsync(areaQualifiedNames.ToArray(), cancellationToken).ConfigureAwait(false);
        return OpcResultId.Ok;
    }

    public async Task<OpcResultId> DisableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        if (_server is not null)
        {
            return await _server.DisableConditionsByAreaAsync(areaQualifiedNames, cancellationToken).ConfigureAwait(false);
        }

        await _eventServerProxy.DisableConditionByAreaAsync(areaQualifiedNames.ToArray(), cancellationToken).ConfigureAwait(false);
        return OpcResultId.Ok;
    }

    public async Task<IAeSubscription> CreateSubscriptionAsync(
        bool active,
        int bufferTimeMs,
        int maxBufferSize,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisconnected();
        if (_server is not null)
        {
            return await _server.CreateSubscriptionAsync(active, bufferTimeMs, maxBufferSize, cancellationToken).ConfigureAwait(false);
        }

        await _eventServerProxy.CreateEventSubscriptionAsync(
            active,
            bufferTimeMs,
            maxBufferSize,
            clientSubscription: 1,
            requestedInterfaceId: IOPCEventSubscriptionMgt.InterfaceId,
            subscription: out _,
            revisedBufferTime: out int revisedBufferTime,
            revisedMaxSize: out int revisedMaxSize,
            cancellationToken).ConfigureAwait(false);
        return new RemoteAeSubscription(active, revisedBufferTime, revisedMaxSize);
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync().ConfigureAwait(false);
        if (_server is not null)
        {
            _server.ServerShutdown -= OnServerShutdown;
        }
    }

    private static IReadOnlyList<AckResult> ToAckResults(IReadOnlyList<ConditionRef> conditions, int[] errors) =>
        conditions.Select((condition, index) => new AckResult
        {
            Condition = condition,
            ResultId = new OpcResultId(index < errors.Length ? errors[index] : OpcResultId.Fail.Code, null),
        }).ToArray();

    private void OnServerShutdown(object? sender, EventArgs args)
    {
        ServerShutdown?.Invoke(this, args);
    }

    private void ThrowIfDisconnected()
    {
        if (!_connected)
        {
            throw new InvalidOperationException("The AE sample client is not connected.");
        }
    }
}
