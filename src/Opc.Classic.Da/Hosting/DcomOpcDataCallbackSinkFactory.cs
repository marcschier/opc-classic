// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Factory for authenticated DCOM <see cref="IOPCDataCallback"/> sink proxies.
/// </summary>
public sealed class DcomOpcDataCallbackSinkFactory : IOpcDataCallbackSinkFactory
{
    private readonly DcomCallChannelFactory _channelFactory;
    private readonly OpcConnectData _connectData;
    private readonly string _fallbackHost;
    private readonly ILoggerFactory _loggerFactory;
    private readonly TimeSpan? _deliveryTimeout;

    /// <summary>
    /// Initializes a new instance of the <see cref="DcomOpcDataCallbackSinkFactory"/> class.
    /// </summary>
    public DcomOpcDataCallbackSinkFactory(
        DcomCallChannelFactory channelFactory,
        OpcConnectData connectData,
        string fallbackHost = "localhost",
        ILoggerFactory? loggerFactory = null,
        TimeSpan? deliveryTimeout = null)
    {
        _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
        _connectData = connectData ?? throw new ArgumentNullException(nameof(connectData));
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackHost);
        _fallbackHost = fallbackHost;
        _loggerFactory = loggerFactory ?? NullLoggerFactory.Instance;
        _deliveryTimeout = deliveryTimeout;
    }

    /// <summary>
    /// Creates a TCP-only callback factory for the common ncacn_ip_tcp path.
    /// </summary>
    public static DcomOpcDataCallbackSinkFactory CreateTcpOnly(
        OpcConnectData connectData,
        string fallbackHost = "localhost",
        ILoggerFactory? loggerFactory = null,
        TimeSpan? deliveryTimeout = null) =>
        new(new DcomCallChannelFactory(new CallbackTcpTransportFactory()), connectData, fallbackHost, loggerFactory, deliveryTimeout);

    /// <inheritdoc />
    public IOpcDataCallbackSink Create(IOpcInterfaceRef sink) =>
        new DcomOpcDataCallbackSink(
            sink,
            _channelFactory,
            () => NtlmAuthentication.CreateAuthContext(_connectData),
            _fallbackHost,
            _loggerFactory.CreateLogger<DcomOpcDataCallbackSink>(),
            _deliveryTimeout);

    private sealed class CallbackTcpTransportFactory : IAsyncTransportFactory
    {
        public async ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            return endpoint switch
            {
                DnsEndPoint dns => await TcpClientTransport.ConnectAsync(dns.Host, dns.Port, cancellationToken).ConfigureAwait(false),
                IPEndPoint ip => await TcpClientTransport.ConnectAsync(ip.Address.ToString(), ip.Port, cancellationToken).ConfigureAwait(false),
                _ => throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "Endpoint type '{0}' is not supported by the TCP-only callback transport.", endpoint.GetType().FullName)),
            };
        }
    }
}
