// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// TCP-capable client for legacy <c>IActivation::RemoteActivation</c> (opnum 0).
/// </summary>
public sealed class ActivationClient : IActivationClient, IAsyncDisposable
{
    private const int EndpointMapperPort = 135;
    private const int RemoteActivationOpnum = 0;
    private const int RemoteCreateInstanceOpnum = 4;
    private const uint DefaultClientImpersonationLevel = 3;
    private const uint DefaultMode = 0;
    private const ushort RpcProtocolSequenceTcp = 0x07;
    private const ushort RpcProtocolSequenceNamedPipe = 0x0F;

    private static readonly Guid ActivationInterfaceId = Guid.Parse(Interfaces.IID_IActivation);
    private static readonly Guid RemoteScmActivatorInterfaceId = Guid.Parse("000001A0-0000-0000-C000-000000000046");
    private readonly ICallChannel _channel;
    private readonly IAsyncDisposable? _ownedChannel;

    /// <summary>
    /// Creates a client over an existing DCOM call channel.
    /// </summary>
    public ActivationClient(ICallChannel channel)
        : this(channel, null)
    {
    }

    private ActivationClient(ICallChannel channel, IAsyncDisposable? ownedChannel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _ownedChannel = ownedChannel;
    }

    /// <summary>
    /// Connects to the endpoint mapper on TCP port 135 with no RPC authentication.
    /// </summary>
    public static Task<ActivationClient> ConnectTcpAsync(
        string host,
        CancellationToken cancellationToken = default) =>
        ConnectTcpAsync(host, EndpointMapperPort, NoOpAuthContext.Instance, cancellationToken);

    /// <summary>
    /// Connects to a TCP DCOM endpoint with the supplied authentication context.
    /// </summary>
    public static Task<ActivationClient> ConnectTcpAsync(
        string host,
        IAuthContext authContext,
        CancellationToken cancellationToken = default) =>
        ConnectTcpAsync(host, EndpointMapperPort, authContext, cancellationToken);

    /// <summary>
    /// Connects to a TCP DCOM endpoint with the supplied authentication context.
    /// </summary>
    public static async Task<ActivationClient> ConnectTcpAsync(
        string host,
        int port,
        IAuthContext authContext,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(authContext);
        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), port, "TCP port must be in the range [1, 65535].");
        }

        DcomCallChannel channel = await DcomCallChannelFactory.ConnectTcpAsync(host, port, authContext, cancellationToken).ConfigureAwait(false);
        return new ActivationClient(channel, channel);
    }

    /// <inheritdoc />
    public Task<RemoteActivationResponse> RemoteActivationAsync(
        Guid clsid,
        string[] protseqs,
        string? objectStorage,
        Guid[] iids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(protseqs);
        ArgumentNullException.ThrowIfNull(iids);

        ushort[] protocolSequences = NormalizeProtocolSequences(protseqs);
        var request = new RemoteActivationRequest(
            clsid,
            CopyIids(iids),
            DefaultClientImpersonationLevel,
            DefaultMode,
            protocolSequences)
        {
            ObjectName = string.IsNullOrEmpty(objectStorage) ? null : objectStorage,
        };
        return RemoteActivationAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<RemoteActivationResponse> RemoteActivationAsync(
        RemoteActivationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        byte[] payload = IActivationCodec.EncodeRemoteActivationRequest(request);
        NdrCallResult result = await _channel.InvokeAsync(
            ActivationInterfaceId,
            RemoteActivationOpnum,
            payload,
            cancellationToken).ConfigureAwait(false);

        if (result.IsFailure)
        {
            throw new InvalidOperationException($"IActivation::RemoteActivation RPC fault 0x{unchecked((uint)result.Hresult):X8}.");
        }

        return IActivationCodec.DecodeRemoteActivationResponse(result.ResponsePayload.Span, request.RequestedIids.Count);
    }

    /// <summary>
    /// Invokes MS-DCOM <c>IRemoteSCMActivator::RemoteCreateInstance</c> (opnum 4).
    /// </summary>
    public Task<ActivationPropertiesOutData> RemoteCreateInstanceAsync(
        Guid clsid,
        string[] protseqs,
        Guid[] iids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(protseqs);
        ArgumentNullException.ThrowIfNull(iids);

        ushort[] protocolSequences = NormalizeProtocolSequences(protseqs);
        return RemoteCreateInstanceAsync(clsid, protocolSequences, iids, cancellationToken);
    }

    /// <summary>
    /// Invokes MS-DCOM <c>IRemoteSCMActivator::RemoteCreateInstance</c> (opnum 4).
    /// </summary>
    public async Task<ActivationPropertiesOutData> RemoteCreateInstanceAsync(
        Guid clsid,
        IReadOnlyList<ushort> protocolSequences,
        IReadOnlyList<Guid> iids,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(protocolSequences);
        ArgumentNullException.ThrowIfNull(iids);
        cancellationToken.ThrowIfCancellationRequested();

        byte[] payload = ActivationPropertiesCodec.EncodeRemoteCreateInstanceRequest(
            clsid,
            iids,
            protocolSequences);
        const string operation = "IRemoteSCMActivator::RemoteCreateInstance";
        NdrCallResult result;
        try
        {
            result = await _channel.InvokeAsync(
                RemoteScmActivatorInterfaceId,
                RemoteCreateInstanceOpnum,
                payload,
                cancellationToken).ConfigureAwait(false);
        }
        catch (BindException ex)
        {
            throw new RemoteScmUnavailableException($"{operation} is unavailable on the remote SCM.", ex);
        }
        catch (PresentationContextRejectedException ex) when (ex.InterfaceId == RemoteScmActivatorInterfaceId)
        {
            throw new RemoteScmUnavailableException($"{operation} is unavailable on the remote SCM.", ex);
        }

        if (result.IsFailure)
        {
            throw new ActivationRpcException(operation, result.Hresult);
        }

        return ActivationPropertiesCodec.DecodeRemoteCreateInstanceResponse(result.ResponsePayload.Span);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_ownedChannel is not null)
        {
            await _ownedChannel.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static ushort[] NormalizeProtocolSequences(IReadOnlyList<string> protseqs)
    {
        if (protseqs.Count == 0)
        {
            throw new ArgumentException("At least one RPC protocol sequence is required.", nameof(protseqs));
        }

        var normalized = new ushort[protseqs.Count];
        for (int i = 0; i < protseqs.Count; i++)
        {
            normalized[i] = NormalizeProtocolSequence(protseqs[i]);
        }

        return normalized;
    }

    private static ushort NormalizeProtocolSequence(string protseq)
    {
        if (string.IsNullOrWhiteSpace(protseq))
        {
            throw new ArgumentException("RPC protocol sequence cannot be empty.", nameof(protseq));
        }

        string value = protseq.Trim();
        if (value.Equals("ncacn_ip_tcp", StringComparison.OrdinalIgnoreCase)
            || value.Equals("ip_tcp", StringComparison.OrdinalIgnoreCase)
            || value.Equals("tcp", StringComparison.OrdinalIgnoreCase)
            || value.Equals("7", StringComparison.Ordinal))
        {
            return RpcProtocolSequenceTcp;
        }

        // DCE/RPC over named pipes (protocol ID 0x0F). The local SCM matches
        // this against any server that calls RpcServerUseProtseq("ncacn_np", ...),
        // which is the default protseq for Windows-hosted local DCOM servers
        // (e.g. the OPC Foundation native TestServer). The activation
        // response carries an OBJREF whose string bindings encode the
        // server's pipe name; clients reach the server via
        // LocalNamedPipeTransport (kernel pipe) or NcacnNpTransport (SMB2)
        // depending on whether the host is local or remote.
        if (value.Equals("ncacn_np", StringComparison.OrdinalIgnoreCase)
            || value.Equals("np", StringComparison.OrdinalIgnoreCase)
            || value.Equals("pipe", StringComparison.OrdinalIgnoreCase)
            || value.Equals("15", StringComparison.Ordinal))
        {
            return RpcProtocolSequenceNamedPipe;
        }

        throw new ArgumentException(
            $"Unsupported RPC protocol sequence '{protseq}'. Supported values: ncacn_ip_tcp, ncacn_np.",
            nameof(protseq));
    }

    private static Guid[] CopyIids(IReadOnlyList<Guid> iids)
    {
        var copy = new Guid[iids.Count];
        for (int i = 0; i < iids.Count; i++)
        {
            copy[i] = iids[i];
        }

        return copy;
    }
}
