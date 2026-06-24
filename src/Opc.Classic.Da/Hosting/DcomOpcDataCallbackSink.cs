// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable CA1031 // Callback delivery isolates unreachable clients from the server.
#pragma warning disable VSTHRD002 // IOpcDataCallbackSink is sync by design; transport calls are async.

using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Cross-platform DCOM transport implementation of <see cref="IOpcDataCallbackSink"/>.
/// </summary>
public sealed class DcomOpcDataCallbackSink : IOpcDataCallbackSink, IOpcDataCallbackSinkStatus
{
    private static readonly Action<ILogger, Guid, Exception?> CallbackDeliveryFailed = LoggerMessage.Define<Guid>(
        LogLevel.Warning,
        new EventId(1, nameof(CallbackDeliveryFailed)),
        "Dropping unreachable IOPCDataCallback sink {Ipid}");

    private readonly IOpcInterfaceRef _sinkRef;
    private readonly DcomCallChannelFactory _channelFactory;
    private readonly Func<IAuthContext> _authContextFactory;
    private readonly string _fallbackHost;
    private readonly ILogger _logger;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private DcomCallChannel? _channel;
    private IOPCDataCallbackClientProxy? _proxy;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="DcomOpcDataCallbackSink"/> class.
    /// </summary>
    public DcomOpcDataCallbackSink(
        IOpcInterfaceRef sinkRef,
        DcomCallChannelFactory channelFactory,
        Func<IAuthContext> authContextFactory,
        string fallbackHost,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(sinkRef);
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(authContextFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackHost);
        if (sinkRef.Iid != IOPCDataCallback.InterfaceId)
        {
            throw new ArgumentException("The sink OBJREF must target IOPCDataCallback.", nameof(sinkRef));
        }

        _sinkRef = sinkRef;
        _channelFactory = channelFactory;
        _authContextFactory = authContextFactory;
        _fallbackHost = fallbackHost;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc />
    public bool IsUnreachable { get; private set; }

    /// <inheritdoc />
    public void OnDataChange(OpcDaGroup.DataChangePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        DeliverAsync(proxy => proxy.OnDataChangeAsync(
            payload.TransactionId,
            payload.GroupHandle,
            payload.MasterQuality,
            payload.MasterError,
            payload.ClientHandles,
            payload.Values,
            payload.Qualities,
            payload.Timestamps,
            payload.Errors,
            CancellationToken.None)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void OnReadComplete(OpcDaGroup.DataChangePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        DeliverAsync(proxy => proxy.OnReadCompleteAsync(
            payload.TransactionId,
            payload.GroupHandle,
            payload.MasterQuality,
            payload.MasterError,
            payload.ClientHandles,
            payload.Values,
            payload.Qualities,
            payload.Timestamps,
            payload.Errors,
            CancellationToken.None)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void OnWriteComplete(
        int transactionId,
        int groupHandle,
        int masterError,
        int[] clientHandles,
        int[] errors)
    {
        ArgumentNullException.ThrowIfNull(clientHandles);
        ArgumentNullException.ThrowIfNull(errors);
        DeliverAsync(proxy => proxy.OnWriteCompleteAsync(
            transactionId,
            groupHandle,
            masterError,
            clientHandles,
            errors,
            CancellationToken.None)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void OnCancelComplete(OpcDaGroup.CancelCompletePayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        DeliverAsync(proxy => proxy.OnCancelCompleteAsync(
            payload.TransactionId,
            payload.GroupHandle,
            CancellationToken.None)).GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _connectLock.Dispose();
        _channel?.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private async Task DeliverAsync(Func<IOPCDataCallbackClientProxy, Task> invoke)
    {
        if (_disposed || IsUnreachable)
        {
            return;
        }

        try
        {
            IOPCDataCallbackClientProxy proxy = await GetProxyAsync(CancellationToken.None).ConfigureAwait(false);
            await invoke(proxy).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            IsUnreachable = true;
            CallbackDeliveryFailed(_logger, _sinkRef.Ipid, ex);
            await DisposeChannelAsync().ConfigureAwait(false);
        }
    }

    private async Task<IOPCDataCallbackClientProxy> GetProxyAsync(CancellationToken cancellationToken)
    {
        if (_proxy is not null)
        {
            return _proxy;
        }

        await _connectLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_proxy is not null)
            {
                return _proxy;
            }

            EndPoint endpoint = ResolveEndpoint();
            ICallChannel channel = await _channelFactory.ConnectActivatedAsync(
                endpoint,
                _authContextFactory(),
                _sinkRef.Ipid,
                [IOPCDataCallback.InterfaceId],
                cancellationToken).ConfigureAwait(false);

            _channel = channel as DcomCallChannel
                ?? throw new InvalidOperationException("The DCOM callback channel factory returned a non-DCOM channel.");
            _proxy = new IOPCDataCallbackClientProxy(_channel);
            return _proxy;
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private EndPoint ResolveEndpoint()
    {
        EndPoint? endpoint = DualStringArrayResolver.ResolveFirstTransport(_fallbackHost, _sinkRef.ResolverBindings);
        if (endpoint is null)
        {
            throw new InvalidOperationException("The callback OBJREF did not contain a supported ncacn_ip_tcp or ncacn_np binding.");
        }

        return endpoint;
    }

    private async ValueTask DisposeChannelAsync()
    {
        DcomCallChannel? channel = Interlocked.Exchange(ref _channel, null);
        _proxy = null;
        if (channel is not null)
        {
            await channel.DisposeAsync().ConfigureAwait(false);
        }
    }
}
