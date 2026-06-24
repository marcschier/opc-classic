// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Ae.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// Cross-platform outbound DCOM sender for <see cref="IOPCEventSink"/> callbacks.
/// </summary>
public sealed class DcomOpcEventSinkSender : IAsyncDisposable
{
    private readonly IOpcInterfaceRef _sinkRef;
    private readonly DcomCallChannelFactory _channelFactory;
    private readonly Func<IAuthContext> _authContextFactory;
    private readonly string _fallbackHost;
    private DcomCallChannel? _channel;
    private IOPCEventSinkClientProxy? _proxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="DcomOpcEventSinkSender"/> class.
    /// </summary>
    public DcomOpcEventSinkSender(
        IOpcInterfaceRef sinkRef,
        DcomCallChannelFactory channelFactory,
        Func<IAuthContext> authContextFactory,
        string fallbackHost = "localhost")
    {
        _sinkRef = sinkRef ?? throw new ArgumentNullException(nameof(sinkRef));
        _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
        _authContextFactory = authContextFactory ?? throw new ArgumentNullException(nameof(authContextFactory));
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackHost);
        _fallbackHost = fallbackHost;
    }

    /// <summary>
    /// Creates a sender from normal OPC connect data.
    /// </summary>
    public static DcomOpcEventSinkSender Create(
        IOpcInterfaceRef sinkRef,
        DcomCallChannelFactory channelFactory,
        OpcConnectData connectData,
        string fallbackHost = "localhost") =>
        new(sinkRef, channelFactory, () => NtlmAuthentication.CreateAuthContext(connectData), fallbackHost);

    /// <summary>
    /// Delivers <c>IOPCEventSink::OnEvent</c>.
    /// </summary>
    public async Task OnEventAsync(
        int clientSubscription,
        bool refresh,
        bool lastRefresh,
        OpcEventNotification[] events,
        CancellationToken cancellationToken = default)
    {
        IOPCEventSinkClientProxy proxy = await GetProxyAsync(cancellationToken).ConfigureAwait(false);
        await proxy.OnEventAsync(clientSubscription, refresh, lastRefresh, events, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync().ConfigureAwait(false);
        }
    }

    private async Task<IOPCEventSinkClientProxy> GetProxyAsync(CancellationToken cancellationToken)
    {
        if (_proxy is not null)
        {
            return _proxy;
        }

        _channel = await DcomOutboundCallbackChannel.ConnectAsync(
            _sinkRef,
            _channelFactory,
            _authContextFactory,
            _fallbackHost,
            IOPCEventSink.InterfaceId,
            cancellationToken).ConfigureAwait(false);
        _proxy = new IOPCEventSinkClientProxy(_channel);
        return _proxy;
    }
}
