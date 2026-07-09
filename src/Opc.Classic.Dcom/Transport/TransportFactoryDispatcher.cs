// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using System.Runtime.Versioning;
using System.Security.Principal;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Selects the appropriate <see cref="IAsyncTransport"/> factory based on
/// the concrete <see cref="EndPoint"/> type returned by the activation
/// resolver. Used by the DCOM connect flow after
/// <see cref="DualStringArrayResolver"/> has decoded the server's
/// resolver-bindings block:
/// <list type="bullet">
/// <item><see cref="DnsEndPoint"/> -> <see cref="TcpSocketTransportFactory"/>.</item>
/// <item><see cref="NcacnNpEndPoint"/> with a local host ->
/// <see cref="LocalNamedPipeTransportFactory"/> (kernel pipe).</item>
/// <item><see cref="NcacnNpEndPoint"/> with a remote host ->
/// <see cref="NcacnNpTransportFactory"/> (SMB2 tunnel).</item>
/// </list>
/// </summary>
public sealed class TransportFactoryDispatcher : IAsyncTransportFactory
{
    private readonly IAsyncTransportFactory _tcpFactory;
    private readonly IAsyncTransportFactory? _localPipeFactory;
    private readonly IAsyncTransportFactory? _remotePipeFactory;
    private readonly Func<string, bool>? _isLocalHost;

    /// <summary>
    /// Constructs a dispatcher with the supplied per-transport factories.
    /// Any factory may be <see langword="null"/> to indicate the
    /// corresponding transport is unsupported in this context (calls to
    /// <see cref="ConnectAsync"/> for an unsupported endpoint type throw
    /// <see cref="NotSupportedException"/>).
    /// </summary>
    public TransportFactoryDispatcher(
        IAsyncTransportFactory tcpFactory,
        IAsyncTransportFactory? localPipeFactory = null,
        IAsyncTransportFactory? remotePipeFactory = null,
        Func<string, bool>? isLocalHost = null)
    {
        _tcpFactory = tcpFactory ?? throw new ArgumentNullException(nameof(tcpFactory));
        _localPipeFactory = localPipeFactory;
        _remotePipeFactory = remotePipeFactory;
        _isLocalHost = isLocalHost;
    }

    /// <summary>
    /// Convenience factory that wires a TCP socket factory plus the
    /// Windows local-pipe factory. Remote pipe activations throw
    /// <see cref="NotSupportedException"/> unless a remote factory is
    /// supplied explicitly.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static TransportFactoryDispatcher CreateWindowsLocal(
        IAsyncTransportFactory tcpFactory,
        TokenImpersonationLevel impersonationLevel = TokenImpersonationLevel.Impersonation,
        IAsyncTransportFactory? remotePipeFactory = null)
    {
        var localPipeFactory = new LocalNamedPipeTransportFactory(impersonationLevel);
        return new TransportFactoryDispatcher(
            tcpFactory,
            localPipeFactory,
            remotePipeFactory,
            localPipeFactory.IsLocalHost);
    }

    /// <inheritdoc />
    public async ValueTask<IAsyncTransport> ConnectAsync(
        EndPoint endpoint,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        switch (endpoint)
        {
            case DnsEndPoint:
            case IPEndPoint:
                return await _tcpFactory.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

            case NcacnNpEndPoint pipeEndpoint:
                if (_isLocalHost is not null && _isLocalHost(pipeEndpoint.Host))
                {
                    if (_localPipeFactory is null)
                    {
                        throw new NotSupportedException(
                            "Local named-pipe transport is not registered with this dispatcher.");
                    }
                    return await _localPipeFactory.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
                }

                if (_remotePipeFactory is null)
                {
                    throw new NotSupportedException(
                        $"Remote named-pipe transport is not registered with this dispatcher (host '{pipeEndpoint.Host}').");
                }
                return await _remotePipeFactory.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);

            default:
                throw new NotSupportedException(
                    $"Endpoint type '{endpoint.GetType().FullName}' is not handled by TransportFactoryDispatcher.");
        }
    }
}
