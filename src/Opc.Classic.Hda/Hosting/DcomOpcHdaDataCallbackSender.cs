// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Globalization;
using System.Net;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Transport;

namespace Opc.Classic.Hda.Hosting;

/// <summary>
/// Cross-platform outbound DCOM sender for <see cref="IOPCHDA_DataCallback"/> callbacks.
/// </summary>
public sealed class DcomOpcHdaDataCallbackSender : IAsyncDisposable
{
    private readonly IOpcInterfaceRef _sinkRef;
    private readonly DcomCallChannelFactory _channelFactory;
    private readonly Func<IAuthContext> _authContextFactory;
    private readonly string _fallbackHost;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private DcomCallChannel? _channel;
    private IOPCHDA_DataCallbackClientProxy? _proxy;

    /// <summary>
    /// Initializes a new instance of the <see cref="DcomOpcHdaDataCallbackSender"/> class.
    /// </summary>
    public DcomOpcHdaDataCallbackSender(
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
    public static DcomOpcHdaDataCallbackSender Create(
        IOpcInterfaceRef sinkRef,
        DcomCallChannelFactory channelFactory,
        OpcConnectData connectData,
        string fallbackHost = "localhost") =>
        new(sinkRef, channelFactory, () => NtlmAuthentication.CreateAuthContext(connectData), fallbackHost);

    /// <summary>
    /// Creates a TCP-only callback sender for ncacn_ip_tcp callback OBJREFs.
    /// </summary>
    public static DcomOpcHdaDataCallbackSender CreateTcpOnly(
        IOpcInterfaceRef sinkRef,
        OpcConnectData connectData,
        string fallbackHost = "localhost") =>
        Create(sinkRef, new DcomCallChannelFactory(new CallbackTcpTransportFactory()), connectData, fallbackHost);

    /// <summary>
    /// Delivers <c>IOPCHDA_DataCallback::OnDataChange</c>.
    /// </summary>
    public async Task OnDataChangeAsync(
        int transactionId,
        int status,
        OpcHdaItem[] itemValues,
        int[] errors,
        CancellationToken cancellationToken = default)
    {
        IOPCHDA_DataCallbackClientProxy proxy = await GetProxyAsync(cancellationToken).ConfigureAwait(false);
        await proxy.OnDataChangeAsync(transactionId, status, itemValues, errors, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Delivers <c>IOPCHDA_DataCallback::OnReadComplete</c>.
    /// </summary>
    public async Task OnReadCompleteAsync(
        int transactionId,
        int status,
        OpcHdaItem[] itemValues,
        int[] errors,
        CancellationToken cancellationToken = default)
    {
        IOPCHDA_DataCallbackClientProxy proxy = await GetProxyAsync(cancellationToken).ConfigureAwait(false);
        await proxy.OnReadCompleteAsync(transactionId, status, itemValues, errors, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            await _channel.DisposeAsync().ConfigureAwait(false);
        }
        _connectLock.Dispose();
    }

    private async Task<IOPCHDA_DataCallbackClientProxy> GetProxyAsync(CancellationToken cancellationToken)
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

            _channel = await DcomOutboundCallbackChannel.ConnectAsync(
                _sinkRef,
                _channelFactory,
                _authContextFactory,
                _fallbackHost,
                IOPCHDA_DataCallback.InterfaceId,
                cancellationToken).ConfigureAwait(false);
            _proxy = new IOPCHDA_DataCallbackClientProxy(_channel);
            return _proxy;
        }
        finally
        {
            _connectLock.Release();
        }
    }

    private sealed class CallbackTcpTransportFactory : IAsyncTransportFactory
    {
        public async ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            return endpoint switch
            {
                DnsEndPoint dns => await TcpClientTransport.ConnectAsync(dns.Host, dns.Port, cancellationToken).ConfigureAwait(false),
                IPEndPoint ip => await TcpClientTransport.ConnectAsync(ip.Address.ToString(), ip.Port, cancellationToken).ConfigureAwait(false),
                _ => throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Endpoint type '{0}' is not supported by the TCP-only HDA callback transport.", endpoint.GetType().FullName)),
            };
        }
    }
}
