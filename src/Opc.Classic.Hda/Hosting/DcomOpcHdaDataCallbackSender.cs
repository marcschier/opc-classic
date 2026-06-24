// Copyright (c) 2026 marcschier. Licensed under the MIT License.

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
    }

    private async Task<IOPCHDA_DataCallbackClientProxy> GetProxyAsync(CancellationToken cancellationToken)
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
}
