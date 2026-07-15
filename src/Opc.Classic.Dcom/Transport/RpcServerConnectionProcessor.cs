// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Auth;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Hosting;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Per-connection DCE/RPC server-side processor for managed OPC servers.
/// Each accepted <see cref="IAsyncTransport"/> hands off to
/// <see cref="ProcessConnectionAsync"/>, which loops reading PDUs and
/// dispatching them to the registered
/// <see cref="IOpcServerDispatcher"/> for the matching interface.
/// </summary>
/// <remarks>
/// <para>
/// This processor is the <c>ocom-1</c> foundation for the managed
/// server-side path; ocom-2 wires it into the DA/AE/HDA hosts. Until
/// ocom-7 lands, callbacks and outbound request dispatch are not
/// supported — the processor handles inbound requests only.
/// </para>
/// <para>
/// Authenticated binds (PDUs carrying an auth verifier — i.e.
/// <c>AUTH_LENGTH_OFFSET</c> non-zero) are accepted only by dispatchers
/// that explicitly consume the RPC request context. The relaxed DCOM ACLs
/// in the fleet's <c>dcom-test-acls.reg</c> still permit anonymous calls
/// for the managed-server smoke scenarios; full NTLM/Kerberos
/// server-side verifier validation remains a deliberate follow-up
/// (ocom-7's spike).
/// </para>
/// <para>
/// <b>Single-object scope.</b> The processor routes by presentation
/// context-id only. <see cref="RequestCoPdu.Object"/> (the optional
/// per-call object UUID for sub-object routing) is currently ignored;
/// this is adequate for <c>IOPCServer</c> root-object calls but will
/// need an IPID/object registry once <c>IOPCGroupStateMgt</c> /
/// <c>IOPCItemMgt</c> calls arrive (ocom-3).
/// </para>
/// <para>
/// The per-connection presentation-context map is single-threaded by
/// design: one request loop per <see cref="IAsyncTransport"/>, no
/// shared mutable state. The processor does not advertise
/// <see cref="ConnectionOrientedPdu.PFC_CONC_MPX"/>.
/// </para>
/// </remarks>
public sealed class RpcServerConnectionProcessor
{
    private const int AuthenticationVerifierHeaderLength = 8;

    private static readonly Action<ILogger, EndPoint, Exception?> ProcessorStarted =
        LoggerMessage.Define<EndPoint>(LogLevel.Debug, new EventId(1, nameof(ProcessorStarted)),
            "RpcServerConnectionProcessor: started for {Remote}");

    private static readonly Action<ILogger, EndPoint, Exception?> ProcessorClosed =
        LoggerMessage.Define<EndPoint>(LogLevel.Debug, new EventId(2, nameof(ProcessorClosed)),
            "RpcServerConnectionProcessor: closed for {Remote}");

    private static readonly Action<ILogger, EndPoint, int, Exception?> AuthRejected =
        LoggerMessage.Define<EndPoint, int>(LogLevel.Warning, new EventId(3, nameof(AuthRejected)),
            "RpcServerConnectionProcessor: rejecting authenticated PDU from {Remote} (auth_length={AuthLength})");

    private static readonly Action<ILogger, EndPoint, int, Exception?> UnsupportedPduType =
        LoggerMessage.Define<EndPoint, int>(LogLevel.Warning, new EventId(4, nameof(UnsupportedPduType)),
            "RpcServerConnectionProcessor: unsupported PDU type 0x{Type:X2} from {Remote}");

    private static readonly Action<ILogger, EndPoint, int, Exception?> UnknownContext =
        LoggerMessage.Define<EndPoint, int>(LogLevel.Warning, new EventId(5, nameof(UnknownContext)),
            "RpcServerConnectionProcessor: request from {Remote} referenced unknown context-id {ContextId}");

    private static readonly Action<ILogger, EndPoint, Guid, int, Exception> DispatcherThrew =
        LoggerMessage.Define<EndPoint, Guid, int>(LogLevel.Error, new EventId(6, nameof(DispatcherThrew)),
            "RpcServerConnectionProcessor: dispatcher for {Interface} opnum {Opnum} threw (peer {Remote})");

    private static readonly Action<ILogger, EndPoint, Exception> ProcessorFaulted =
        LoggerMessage.Define<EndPoint>(LogLevel.Error, new EventId(7, nameof(ProcessorFaulted)),
            "RpcServerConnectionProcessor: connection from {Remote} faulted");

    private readonly IReadOnlyDictionary<Guid, IOpcServerDispatcher> _dispatchers;
    private readonly OpcObjectRegistry? _objectRegistry;
    private readonly RpcServerAuthenticationOptions _authenticationOptions;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a processor that routes PDUs for the supplied
    /// interface set.
    /// </summary>
    /// <param name="dispatchers">
    /// Interface-ID → dispatcher for the root server object (calls
    /// without <c>PFC_OBJECT_UUID</c>). Typically populated from the
    /// source-generated <c>*ServerDispatcher</c> wrappers around the
    /// managed server implementations.
    /// </param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    public RpcServerConnectionProcessor(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers,
        ILogger? logger = null)
        : this(dispatchers, objectRegistry: null, authenticationSource: null, logger)
    {
    }

    /// <summary>
    /// Initializes a processor with an explicit server-side NTLM authentication source.
    /// </summary>
    /// <param name="dispatchers">Root-object dispatchers (called when no Object UUID is present).</param>
    /// <param name="authenticationSource">
    /// Optional credential source for inbound NTLM authenticated binds; <see langword="null"/> uses
    /// <see cref="AuthenticationSource.DefaultInstance"/>.
    /// </param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    public RpcServerConnectionProcessor(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers,
        AuthenticationSource? authenticationSource,
        ILogger? logger = null)
        : this(dispatchers, objectRegistry: null, authenticationSource, logger)
    {
    }

    /// <summary>
    /// Initializes a processor that routes PDUs for the supplied root
    /// interface set plus a per-object IPID registry.
    /// </summary>
    /// <param name="dispatchers">Root-object dispatchers (called when no Object UUID is present).</param>
    /// <param name="objectRegistry">
    /// Optional registry consulted when the inbound request carries an
    /// Object UUID. <see langword="null"/> falls back to root-only
    /// behavior.
    /// </param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    public RpcServerConnectionProcessor(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers,
        OpcObjectRegistry? objectRegistry,
        ILogger? logger = null)
        : this(dispatchers, objectRegistry, authenticationSource: null, logger)
    {
    }

    /// <summary>
    /// Initializes a processor that routes PDUs for the supplied root
    /// interface set plus a per-object IPID registry and NTLM authentication source.
    /// </summary>
    /// <param name="dispatchers">Root-object dispatchers (called when no Object UUID is present).</param>
    /// <param name="objectRegistry">
    /// Optional registry consulted when the inbound request carries an
    /// Object UUID. <see langword="null"/> falls back to root-only
    /// behavior.
    /// </param>
    /// <param name="authenticationSource">
    /// Optional credential source for inbound NTLM authenticated binds; <see langword="null"/> uses
    /// <see cref="AuthenticationSource.DefaultInstance"/>.
    /// </param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    public RpcServerConnectionProcessor(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers,
        OpcObjectRegistry? objectRegistry,
        AuthenticationSource? authenticationSource,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dispatchers);
        _dispatchers = dispatchers;
        _objectRegistry = objectRegistry;
        AuthenticationSource source = authenticationSource ?? AuthenticationSource.DefaultInstance;
        _authenticationOptions = CreateLegacyAuthenticationOptions(source);
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Initializes a processor with mechanism-neutral server authentication configuration.
    /// </summary>
    /// <param name="authenticationOptions">
    /// Explicit provider selection, authorization mapping, and authentication policy.
    /// </param>
    /// <param name="dispatchers">Root-object dispatchers (called when no Object UUID is present).</param>
    /// <param name="objectRegistry">Optional per-object IPID registry.</param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    public RpcServerConnectionProcessor(
        RpcServerAuthenticationOptions authenticationOptions,
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers,
        OpcObjectRegistry? objectRegistry = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(authenticationOptions);
        ArgumentNullException.ThrowIfNull(dispatchers);
        _dispatchers = dispatchers;
        _objectRegistry = objectRegistry;
        _authenticationOptions = authenticationOptions;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Gets the registered interface IDs (for diagnostics / tests).
    /// </summary>
    public IReadOnlyCollection<Guid> SupportedInterfaces => (IReadOnlyCollection<Guid>)_dispatchers.Keys;

    private static RpcServerAuthenticationOptions CreateLegacyAuthenticationOptions(
        AuthenticationSource source)
    {
        var providers = new RpcServerAuthenticationProviderRegistry();
        bool requireAuthentication = source is not NullAuthenticationSource;
        if (requireAuthentication)
        {
            providers.Register(source);
        }

        return new RpcServerAuthenticationOptions(
            providers,
            requireAuthentication: requireAuthentication);
    }

    /// <summary>
    /// Runs the request loop for one accepted connection until the peer
    /// closes, sends a <see cref="ShutdownPdu"/>, or
    /// <paramref name="cancellationToken"/> fires.
    /// </summary>
    public async ValueTask ProcessConnectionAsync(IAsyncTransport transport, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(transport);

        var contextMap = new Dictionary<int, Guid>();
        var authState = new RpcServerAuthenticationState(_authenticationOptions);
        int maxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE;

        ProcessorStarted(_logger, transport.RemoteEndpoint, null);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                byte[]? frame = await ReadFrameOrNullAsync(transport, cancellationToken).ConfigureAwait(false);
                if (frame is null)
                {
                    return;
                }

                if (!TryStripAuthenticationVerifier(transport, frame, out AuthenticationStrippedFrame stripped))
                {
                    return;
                }

                if (!TryVerifyRequiredPacketProtection(transport, stripped, authState, out bool packetProtectionVerified))
                {
                    return;
                }

                if (!TryDecodePdu(transport, stripped.PduBytes, out ConnectionOrientedPdu? pdu))
                {
                    return;
                }

                bool keepGoing = await HandlePduAsync(transport, pdu!, stripped.Authentication,
                    packetProtectionVerified, authState, contextMap, maxTransmitFragment, cancellationToken)
                    .ConfigureAwait(false);
                if (!keepGoing)
                {
                    return;
                }

                if (pdu is BindAcknowledgePdu ackUpdate && ackUpdate.MaxTransmitFragment > 0)
                {
                    maxTransmitFragment = ackUpdate.MaxTransmitFragment;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ProcessorFaulted(_logger, transport.RemoteEndpoint, ex);
        }
        finally
        {
            ProcessorClosed(_logger, transport.RemoteEndpoint, null);
        }
    }

    private static async ValueTask<byte[]?> ReadFrameOrNullAsync(
        IAsyncTransport transport, CancellationToken cancellationToken)
    {
        try
        {
            return await PduCodec.ReadPduFrameAsync(transport.Input, cancellationToken).ConfigureAwait(false);
        }
        catch (EndOfStreamException)
        {
            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return null;
        }
    }

    private bool TryStripAuthenticationVerifier(
        IAsyncTransport transport,
        byte[] frame,
        out AuthenticationStrippedFrame stripped)
    {
        try
        {
            stripped = StripAuthenticationVerifier(frame);
            return true;
        }
        catch (InvalidOperationException ex)
        {
            UnsupportedPduType(_logger, transport.RemoteEndpoint, frame[ConnectionOrientedPdu.TYPE_OFFSET], ex);
            stripped = default;
            return false;
        }
    }

    private static AuthenticationStrippedFrame StripAuthenticationVerifier(byte[] frame)
    {
        int authLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET));
        if (authLength == 0)
        {
            return new AuthenticationStrippedFrame(frame, frame, RpcPduAuthentication.None);
        }

        int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET));
        int verifierStart = fragmentLength - authLength - AuthenticationVerifierHeaderLength;
        if (verifierStart < ConnectionOrientedPdu.HEADER_LENGTH
            || verifierStart + AuthenticationVerifierHeaderLength > frame.Length)
        {
            throw new InvalidOperationException("DCE/RPC authentication verifier is malformed.");
        }

        int padding = frame[verifierStart + 2];
        int strippedLength = verifierStart - padding;
        if (strippedLength < ConnectionOrientedPdu.HEADER_LENGTH || strippedLength > frame.Length)
        {
            throw new InvalidOperationException("DCE/RPC authentication verifier padding is malformed.");
        }

        int contextId = BinaryPrimitives.ReadInt32LittleEndian(frame.AsSpan(verifierStart + 4));
        byte[] authValue = frame.AsSpan(verifierStart + AuthenticationVerifierHeaderLength, authLength).ToArray();
        var authentication = new RpcPduAuthentication(
            true,
            authLength,
            frame[verifierStart],
            ToOpcProtectionLevel((ProtectionLevel)frame[verifierStart + 1]),
            contextId,
            authValue);

        // The verification input per MS-RPCE §3.3.1.5.2.2 is the entire PDU except the
        // trailing auth_value: the common header (with its ORIGINAL on-the-wire
        // frag_length/auth_length), body, auth padding, and the 8-byte sec_trailer header.
        byte[] verificationPduBytes = frame.AsSpan(0, verifierStart + AuthenticationVerifierHeaderLength).ToArray();

        byte[] pduBytes = frame.AsSpan(0, strippedLength).ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)strippedLength);
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), 0);
        return new AuthenticationStrippedFrame(pduBytes, verificationPduBytes, authentication);
    }

    private bool TryDecodePdu(IAsyncTransport transport, byte[] frame, out ConnectionOrientedPdu? pdu)
    {
        try
        {
            pdu = PduCodec.DecodePdu(frame);
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException)
        {
            UnsupportedPduType(_logger, transport.RemoteEndpoint, frame[ConnectionOrientedPdu.TYPE_OFFSET], ex);
            pdu = null;
            return false;
        }
    }

    private async ValueTask<bool> HandlePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        RpcPduAuthentication authentication,
        bool packetProtectionVerified,
        RpcServerAuthenticationState authState,
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        switch (pdu)
        {
            case BindPdu bind:
                return await HandleBindAsync(transport, bind, authentication, authState, contextMap, maxTransmitFragment, cancellationToken)
                    .ConfigureAwait(false);

            case AlterContextPdu alter:
                return await HandleAlterContextAsync(transport, alter, authentication, authState, contextMap, maxTransmitFragment, cancellationToken)
                    .ConfigureAwait(false);

            case Auth3Pdu:
                return TryCompleteAuthentication(
                    transport,
                    authentication,
                    authState,
                    cancellationToken);

            case RequestCoPdu request when authState.RequiresAuthentication && !authState.IsEstablished:
                AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
                await WriteFaultAsync(transport, request.CallId, request.ContextId,
                    FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
                return false;

            case RequestCoPdu request:
                await HandleRequestAsync(
                    transport,
                    request,
                    authentication,
                    packetProtectionVerified,
                    authState,
                    contextMap,
                    maxTransmitFragment,
                    cancellationToken)
                    .ConfigureAwait(false);
                return true;

            case ShutdownPdu:
                return false;

            case CancelCoPdu or OrphanedPdu:
                return true;

            default:
                UnsupportedPduType(_logger, transport.RemoteEndpoint, pdu.Type, null);
                await WriteFaultAsync(transport, pdu.CallId, 0, FaultCode.UNSPECIFIED_REJECTION,
                    maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
                return true;
        }
    }

    private async ValueTask<bool> HandleBindAsync(
        IAsyncTransport transport,
        BindPdu bind,
        RpcPduAuthentication authentication,
        RpcServerAuthenticationState authState,
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        if (authentication.IsAuthenticated)
        {
            AuthenticationTokenAcceptance acceptance = TryAcceptAuthenticationToken(
                transport,
                authentication,
                authState,
                cancellationToken,
                out RpcServerAuthenticationTokenResult result);
            if (acceptance == AuthenticationTokenAcceptance.ProviderNotFound
                && !authState.RequiresAuthentication
                && AuthenticatedBindAllowed(bind))
            {
                authState.Reset();
                BindAcknowledgePdu optionalAck = BuildBindAck(bind, contextMap);
                await WriteSinglePduAsync(transport, optionalAck, maxTransmitFragment, cancellationToken)
                    .ConfigureAwait(false);
                return true;
            }
            if (acceptance != AuthenticationTokenAcceptance.Accepted)
            {
                await WriteBindNakAsync(transport, bind.CallId,
                    BindNoAcknowledgeReason.REASON_NOT_SPECIFIED, cancellationToken).ConfigureAwait(false);
                return false;
            }

            BindAcknowledgePdu authenticatedAck = BuildBindAck(bind, contextMap);
            if (result.ResponseToken.IsEmpty)
            {
                await WriteSinglePduAsync(
                    transport,
                    authenticatedAck,
                    maxTransmitFragment,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await WritePduAsync(
                    transport,
                    authenticatedAck,
                    maxTransmitFragment,
                    authentication,
                    result.ResponseToken,
                    cancellationToken).ConfigureAwait(false);
            }
            return true;
        }

        BindAcknowledgePdu ack = BuildBindAck(bind, contextMap);
        await WritePduAsync(transport, ack, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private async ValueTask<bool> HandleAlterContextAsync(
        IAsyncTransport transport,
        AlterContextPdu alter,
        RpcPduAuthentication authentication,
        RpcServerAuthenticationState authState,
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        AlterContextResponsePdu alterAck = BuildAlterContextResponse(alter, contextMap);
        if (authentication.IsAuthenticated && authentication.AuthValue.Length > 0)
        {
            AuthenticationTokenAcceptance acceptance = TryAcceptAuthenticationToken(
                transport,
                authentication,
                authState,
                cancellationToken,
                out RpcServerAuthenticationTokenResult result);
            if (acceptance != AuthenticationTokenAcceptance.Accepted)
            {
                await WriteBindNakAsync(transport, alter.CallId,
                    BindNoAcknowledgeReason.REASON_NOT_SPECIFIED, cancellationToken).ConfigureAwait(false);
                return false;
            }

            if (result.ResponseToken.IsEmpty)
            {
                await WriteSinglePduAsync(
                    transport,
                    alterAck,
                    maxTransmitFragment,
                    cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await WritePduAsync(
                    transport,
                    alterAck,
                    maxTransmitFragment,
                    authentication,
                    result.ResponseToken,
                    cancellationToken).ConfigureAwait(false);
            }
            return true;
        }

        await WritePduAsync(transport, alterAck, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private AuthenticationTokenAcceptance TryAcceptAuthenticationToken(
        IAsyncTransport transport,
        RpcPduAuthentication authentication,
        RpcServerAuthenticationState authState,
        CancellationToken cancellationToken,
        out RpcServerAuthenticationTokenResult result)
    {
        result = default;
        if (authentication.AuthValue.Length == 0)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
            return AuthenticationTokenAcceptance.Rejected;
        }

        try
        {
            if (!authState.TryAcceptToken(authentication, cancellationToken, out result))
            {
                return AuthenticationTokenAcceptance.ProviderNotFound;
            }
            if (result.Session is null && result.ResponseToken.IsEmpty)
            {
                AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
                return AuthenticationTokenAcceptance.Rejected;
            }

            return AuthenticationTokenAcceptance.Accepted;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IOException or SecurityException)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, ex);
            return AuthenticationTokenAcceptance.Rejected;
        }
    }

    private bool TryCompleteAuthentication(
        IAsyncTransport transport,
        RpcPduAuthentication authentication,
        RpcServerAuthenticationState authState,
        CancellationToken cancellationToken)
    {
        AuthenticationTokenAcceptance acceptance = TryAcceptAuthenticationToken(
            transport,
            authentication,
            authState,
            cancellationToken,
            out RpcServerAuthenticationTokenResult result);
        return acceptance == AuthenticationTokenAcceptance.Accepted
            && result.Session is not null
            && result.ResponseToken.IsEmpty;
    }

    private BindAcknowledgePdu BuildBindAck(BindPdu bind, Dictionary<int, Guid> contextMap)
    {
        PresentationResult[] results = NegotiateContexts(bind.ContextList, contextMap);
        int associationGroupId = bind.AssociationGroupId != 0
            ? bind.AssociationGroupId
            : Random.Shared.Next(1, int.MaxValue);

        int negotiatedMaxTransmit = Math.Min(bind.MaxTransmitFragment, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        int negotiatedMaxReceive = Math.Min(bind.MaxReceiveFragment, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

        return new BindAcknowledgePdu
        {
            MaxTransmitFragment = negotiatedMaxTransmit,
            MaxReceiveFragment = negotiatedMaxReceive,
            AssociationGroupId = associationGroupId,
            SecondaryAddress = new Port(),
            ResultList = results,
            CallId = bind.CallId,
        };
    }

    private AlterContextResponsePdu BuildAlterContextResponse(AlterContextPdu alter, Dictionary<int, Guid> contextMap)
    {
        PresentationResult[] results = NegotiateContexts(alter.ContextList, contextMap);
        return new AlterContextResponsePdu
        {
            MaxTransmitFragment = alter.MaxTransmitFragment > 0 ? alter.MaxTransmitFragment : ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = alter.MaxReceiveFragment > 0 ? alter.MaxReceiveFragment : ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            AssociationGroupId = alter.AssociationGroupId,
            SecondaryAddress = new Port(),
            ResultList = results,
            CallId = alter.CallId,
        };
    }

    private PresentationResult[] NegotiateContexts(
        PresentationContext[] proposedContexts,
        Dictionary<int, Guid> contextMap)
    {
        if (proposedContexts is null || proposedContexts.Length == 0)
        {
            return [];
        }

        var ndrTransferSyntax = new PresentationSyntax(NdrCodec.NDR_SYNTAX);
        var results = new PresentationResult[proposedContexts.Length];
        for (int i = 0; i < proposedContexts.Length; i++)
        {
            PresentationContext proposal = proposedContexts[i];
            if (!TryGuidFromUuid(proposal.AbstractSyntax?.Uuid, out Guid interfaceId)
                || !SupportsInterface(interfaceId))
            {
                // PresentationResult.Read always reads a TransferSyntax, so we
                // always include one on rejection too (otherwise the peer's
                // decode underflows). Use the proposed NDR syntax as a
                // placeholder; the result code communicates the actual reject.
                results[i] = new PresentationResult(
                    PresentationResultCode.PROVIDER_REJECTION,
                    PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED,
                    ndrTransferSyntax);
                continue;
            }

            if (!HasNdrTransferSyntax(proposal.TransferSyntaxes, ndrTransferSyntax))
            {
                results[i] = new PresentationResult(
                    PresentationResultCode.PROVIDER_REJECTION,
                    PresentationResultReason.PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED,
                    ndrTransferSyntax);
                continue;
            }

            contextMap[proposal.ContextId] = interfaceId;
            results[i] = new PresentationResult(ndrTransferSyntax);
        }
        return results;
    }

    private async ValueTask HandleRequestAsync(
        IAsyncTransport transport,
        RequestCoPdu request,
        RpcPduAuthentication authentication,
        bool packetProtectionVerified,
        RpcServerAuthenticationState authState,
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        if (!contextMap.TryGetValue(request.ContextId, out Guid interfaceId))
        {
            UnknownContext(_logger, transport.RemoteEndpoint, request.ContextId, null);
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
            return;
        }

        IOpcServerDispatcher? dispatcher = ResolveDispatcher(request, interfaceId);
        if (dispatcher is null)
        {
            UnknownContext(_logger, transport.RemoteEndpoint, request.ContextId, null);
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
            return;
        }

        bool isRawNdr = dispatcher is IRpcRawNdrDispatcher;
        if (!TryExtractRequestBody(request, isRawNdr, out ReadOnlyMemory<byte> body))
        {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
            return;
        }

        DispatchResult? result = await TryDispatchAsync(
            transport,
            dispatcher,
            interfaceId,
            request,
            authentication,
            packetProtectionVerified,
            authState,
            body,
            cancellationToken)
            .ConfigureAwait(false);
        if (result is null)
        {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
            return;
        }

        await WriteRequestOutcomeAsync(transport, request, result.Value, isRawNdr, maxTransmitFragment, authState, cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool TryExtractRequestBody(RequestCoPdu request, bool isRawNdr, out ReadOnlyMemory<byte> body)
    {
        body = ReadOnlyMemory<byte>.Empty;
        if (request.Stub is null)
        {
            return true;
        }
        if (isRawNdr)
        {
            body = request.Stub;
            return true;
        }
        try
        {
            body = OrpcEnvelope.ExtractRequestBody(request.Stub);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private async ValueTask<DispatchResult?> TryDispatchAsync(
        IAsyncTransport transport,
        IOpcServerDispatcher dispatcher,
        Guid interfaceId,
        RequestCoPdu request,
        RpcPduAuthentication authentication,
        bool packetProtectionVerified,
        RpcServerAuthenticationState authState,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        try
        {
            if (dispatcher is IRpcRequestContextDispatcher contextDispatcher)
            {
                // Authorization for context-aware dispatchers (activation, IRemUnknown) must be
                // derived from the established authentication session, never from the per-packet sec_trailer.
                // A request PDU can carry a forged trailer (auth_length > 0, attacker-chosen
                // auth_level) that is never cryptographically verified when no context is
                // established (ShouldProtectPackets is false), so trusting authentication.* here
                // would let an unauthenticated/downgraded client spoof authenticated, integrity-
                // protected activation. Report the established-session signal and its negotiated
                // protection floor instead.
                bool verificationSatisfied =
                    !authState.RequiresPacketProtection || packetProtectionVerified;
                bool establishedSessionVisible =
                    authState.IsEstablished && verificationSatisfied;
                bool establishedAndAuthenticated =
                    authentication.IsAuthenticated && establishedSessionVisible;
                var requestContext = new RpcRequestContext(
                    establishedAndAuthenticated,
                    establishedSessionVisible,
                    establishedSessionVisible
                        ? authState.ProtectionLevel
                        : OpcProtectionLevel.None,
                    transport.RemoteEndpoint)
                {
                    AuthenticationService = establishedSessionVisible
                        ? authState.AuthenticationService
                        : 0,
                    Principal = establishedSessionVisible
                        ? authState.Principal
                        : null,
                };
                return await contextDispatcher.DispatchAsync(request.Opnum, body, requestContext, cancellationToken).ConfigureAwait(false);
            }

            if (authentication.IsAuthenticated && !authState.IsEstablished)
            {
                AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
                return DispatchResult.Fault(global::Opc.Classic.OpcResultId.AccessDenied.Code);
            }

            return await dispatcher.DispatchAsync(request.Opnum, body, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            DispatcherThrew(_logger, transport.RemoteEndpoint, interfaceId, request.Opnum, ex);
            return null;
        }
    }

    private static async ValueTask WriteRequestOutcomeAsync(
        IAsyncTransport transport,
        RequestCoPdu request,
        DispatchResult result,
        bool isRawNdr,
        int maxTransmitFragment,
        RpcServerAuthenticationState authState,
        CancellationToken cancellationToken)
    {
        if (result.IsFailure)
        {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                (FaultCode)result.Hresult, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
            return;
        }

        byte[] responseStub = isRawNdr ? result.Payload.ToArray() : OrpcEnvelope.BuildResponseStub(result.Payload);
        var response = new ResponseCoPdu
        {
            AllocationHint = responseStub.Length,
            ContextId = request.ContextId,
            Stub = responseStub,
            CallId = request.CallId,
        };
        await WritePduAsync(transport, response, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
    }

    private bool SupportsInterface(Guid interfaceId) =>
        _dispatchers.ContainsKey(interfaceId) || (_objectRegistry?.ContainsInterface(interfaceId) ?? false);

    private IOpcServerDispatcher? ResolveDispatcher(RequestCoPdu request, Guid interfaceId)
    {
        // Per-object route: if the request carries an Object UUID and the
        // registry knows it for the requested interface, prefer it over
        // the root map. Otherwise fall through to the root dispatcher set.
        if (_objectRegistry is not null && request.Object is not null
            && Guid.TryParse(request.Object.ToString(), out Guid ipid)
            && _objectRegistry.TryGetDispatcher(ipid, interfaceId, out IOpcServerDispatcher perObject))
        {
            return perObject;
        }

        return _dispatchers.TryGetValue(interfaceId, out IOpcServerDispatcher? root) ? root : null;
    }

    private static async ValueTask WritePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        int maxTransmitFragment,
        RpcServerAuthenticationState authState,
        CancellationToken cancellationToken)
    {
        if (pdu is IFragmentable fragmentable)
        {
            foreach (var fragment in fragmentable.GetFragments(maxTransmitFragment))
            {
                await WriteSinglePduAsync(transport, fragment, maxTransmitFragment, authState, cancellationToken)
                    .ConfigureAwait(false);
            }
            return;
        }

        await WriteSinglePduAsync(transport, pdu, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WritePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        int maxTransmitFragment,
        RpcPduAuthentication authentication,
        ReadOnlyMemory<byte> authenticationBody,
        CancellationToken cancellationToken)
    {
        byte[] bytes = PduCodec.EncodePdu(pdu, maxTransmitFragment);
        bytes = AttachAuthenticationVerifier(bytes, authentication, authenticationBody);
        await WriteBytesAsync(transport, bytes, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WriteSinglePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        int maxTransmitFragment,
        RpcServerAuthenticationState authState,
        CancellationToken cancellationToken)
    {
        byte[] bytes = PduCodec.EncodePdu(pdu, maxTransmitFragment);
        bytes = ApplyPacketProtection(bytes, authState);
        await WriteBytesAsync(transport, bytes, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WriteFaultAsync(
        IAsyncTransport transport,
        int callId,
        int contextId,
        FaultCode status,
        int maxTransmitFragment,
        RpcServerAuthenticationState authState,
        CancellationToken cancellationToken)
    {
        var fault = new FaultCoPdu
        {
            CallId = callId,
            ContextId = contextId,
            Status = status,
        };
        fault.SetFlag(ConnectionOrientedPdu.PFC_DID_NOT_EXECUTE, true);
        await WritePduAsync(transport, fault, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WriteBindNakAsync(
        IAsyncTransport transport,
        int callId,
        BindNoAcknowledgeReason reason,
        CancellationToken cancellationToken)
    {
        var nak = new BindNoAcknowledgePdu
        {
            CallId = callId,
            RejectReason = reason,
        };
        await WriteSinglePduAsync(transport, nak, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async ValueTask WriteSinglePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        byte[] bytes = PduCodec.EncodePdu(pdu, maxTransmitFragment);
        await WriteBytesAsync(transport, bytes, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WriteBytesAsync(
        IAsyncTransport transport,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        Memory<byte> destination = transport.Output.GetMemory(bytes.Length);
        bytes.AsSpan().CopyTo(destination.Span);
        transport.Output.Advance(bytes.Length);
        await transport.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private bool TryVerifyRequiredPacketProtection(
        IAsyncTransport transport,
        AuthenticationStrippedFrame stripped,
        RpcServerAuthenticationState authState,
        out bool packetProtectionVerified)
    {
        packetProtectionVerified = false;
        byte pduType = stripped.PduBytes[ConnectionOrientedPdu.TYPE_OFFSET];
        if (!authState.RequiresPacketProtection)
        {
            return true;
        }
        if (!authState.ShouldProtectPackets)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, stripped.Authentication.AuthLength, null);
            return false;
        }

        if (pduType != RequestCoPdu.REQUEST_TYPE)
        {
            return true;
        }

        if (stripped.Authentication.AuthenticationServiceCode != authState.AuthenticationService
            || stripped.Authentication.AuthValue.Length == 0)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, stripped.Authentication.AuthLength, null);
            return false;
        }

        Span<byte> signedRegion = stripped.VerificationPduBytes;
        int confidentialOffset = ConnectionOrientedPdu.HEADER_LENGTH;
        int confidentialLength = signedRegion.Length - AuthenticationVerifierHeaderLength - confidentialOffset;
        if (!authState.VerifyAndUnseal(signedRegion, confidentialOffset, confidentialLength, stripped.Authentication.AuthValue))
        {
            AuthRejected(_logger, transport.RemoteEndpoint, stripped.Authentication.AuthLength, null);
            return false;
        }

        signedRegion
            .Slice(ConnectionOrientedPdu.HEADER_LENGTH, stripped.PduBytes.Length - ConnectionOrientedPdu.HEADER_LENGTH)
            .CopyTo(stripped.PduBytes.AsSpan(ConnectionOrientedPdu.HEADER_LENGTH));
        packetProtectionVerified = true;
        return true;
    }

    private bool AuthenticatedBindAllowed(BindPdu bind)
    {
        if (bind.ContextList is null || bind.ContextList.Length == 0)
        {
            return false;
        }

        bool hasKnownContext = false;
        foreach (PresentationContext context in bind.ContextList)
        {
            if (TryGuidFromUuid(context.AbstractSyntax?.Uuid, out Guid interfaceId)
                && _dispatchers.TryGetValue(interfaceId, out IOpcServerDispatcher? dispatcher))
            {
                hasKnownContext = true;
                if (dispatcher is not IRpcRequestContextDispatcher)
                {
                    return false;
                }
            }
        }

        return hasKnownContext;
    }

    private static byte[] ApplyPacketProtection(byte[] pduBytes, RpcServerAuthenticationState authState)
    {
        if (!authState.ShouldProtectPackets)
        {
            return pduBytes;
        }

        int padding = PaddingTo(pduBytes.Length, 4);
        int verifierStart = pduBytes.Length + padding;
        int signedLengthWithoutAuthValue = verifierStart + AuthenticationVerifierHeaderLength;
        int confidentialLength = verifierStart - ConnectionOrientedPdu.HEADER_LENGTH;
        int authValueLength = authState.GetVerifierLength(
            signedLengthWithoutAuthValue,
            confidentialLength);
        int fragmentLength = verifierStart + AuthenticationVerifierHeaderLength + authValueLength;
        if (fragmentLength > ushort.MaxValue)
        {
            throw new InvalidOperationException("DCE/RPC fragment length exceeds the 16-bit PDU limit.");
        }

        byte[] protectedPdu = new byte[fragmentLength];
        pduBytes.CopyTo(protectedPdu, 0);
        Span<byte> verifier = protectedPdu.AsSpan(verifierStart, AuthenticationVerifierHeaderLength);
        verifier[0] = checked((byte)authState.AuthenticationService);
        verifier[1] = (byte)ToRpcProtectionLevel(authState.ProtectionLevel);
        verifier[2] = (byte)padding;
        verifier[3] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(verifier[4..], authState.ContextId);

        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), (ushort)authValueLength);

        // Sign the entire signed region (common header + body + auth padding + sec_trailer
        // header), excluding the trailing auth_value (MS-RPCE §3.3.1.5.2.2). At Privacy the
        // stub sub-range is sealed in place.
        int signedLength = signedLengthWithoutAuthValue;
        int confidentialOffset = ConnectionOrientedPdu.HEADER_LENGTH;
        authState.SignAndSeal(protectedPdu.AsSpan(0, signedLength), confidentialOffset, confidentialLength, out byte[] signature);
        if (signature.Length != authValueLength)
        {
            throw new InvalidOperationException(
                $"Auth context returned a {signature.Length}-byte signature; DCE/RPC expects {authValueLength}.");
        }

        signature.CopyTo(protectedPdu.AsSpan(verifierStart + AuthenticationVerifierHeaderLength, authValueLength));
        return protectedPdu;
    }

    private static byte[] AttachAuthenticationVerifier(
        byte[] pduBytes,
        RpcPduAuthentication authentication,
        ReadOnlyMemory<byte> body)
    {
        int padding = PaddingTo(pduBytes.Length, 4);
        int verifierStart = pduBytes.Length + padding;
        int fragmentLength = verifierStart + AuthenticationVerifierHeaderLength + body.Length;
        if (fragmentLength > ushort.MaxValue)
        {
            throw new InvalidOperationException("DCE/RPC fragment length exceeds the 16-bit PDU limit.");
        }

        byte[] result = new byte[fragmentLength];
        pduBytes.CopyTo(result, 0);
        body.Span.CopyTo(result.AsSpan(verifierStart + AuthenticationVerifierHeaderLength));
        Span<byte> verifier = result.AsSpan(verifierStart, AuthenticationVerifierHeaderLength);
        verifier[0] = authentication.AuthenticationServiceCode;
        verifier[1] = (byte)ToRpcProtectionLevel(authentication.ProtectionLevel);
        verifier[2] = (byte)padding;
        verifier[3] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(verifier[4..], authentication.ContextId);

        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), (ushort)body.Length);
        return result;
    }

    private static OpcProtectionLevel ToOpcProtectionLevel(ProtectionLevel protectionLevel) => protectionLevel switch
    {
        ProtectionLevel.PROTECTION_LEVEL_CONNECT => OpcProtectionLevel.Connect,
        ProtectionLevel.PROTECTION_LEVEL_CALL => OpcProtectionLevel.Call,
        ProtectionLevel.PROTECTION_LEVEL_PACKET => OpcProtectionLevel.Packet,
        ProtectionLevel.PROTECTION_LEVEL_INTEGRITY => OpcProtectionLevel.Integrity,
        ProtectionLevel.PROTECTION_LEVEL_PRIVACY => OpcProtectionLevel.Privacy,
        _ => OpcProtectionLevel.None,
    };

    private static ProtectionLevel ToRpcProtectionLevel(OpcProtectionLevel protectionLevel) => protectionLevel switch
    {
        OpcProtectionLevel.None => ProtectionLevel.PROTECTION_LEVEL_NONE,
        OpcProtectionLevel.Connect => ProtectionLevel.PROTECTION_LEVEL_CONNECT,
        OpcProtectionLevel.Call => ProtectionLevel.PROTECTION_LEVEL_CALL,
        OpcProtectionLevel.Packet => ProtectionLevel.PROTECTION_LEVEL_PACKET,
        OpcProtectionLevel.Integrity => ProtectionLevel.PROTECTION_LEVEL_INTEGRITY,
        OpcProtectionLevel.Privacy => ProtectionLevel.PROTECTION_LEVEL_PRIVACY,
        _ => ProtectionLevel.PROTECTION_LEVEL_NONE,
    };

    private static int PaddingTo(int length, int alignment)
    {
        int remainder = length % alignment;
        return remainder == 0 ? 0 : alignment - remainder;
    }

    private static bool TryGuidFromUuid(UUID? uuid, out Guid value)
    {
        if (uuid is null)
        {
            value = Guid.Empty;
            return false;
        }
        return Guid.TryParse(uuid.ToString(), out value);
    }

    private static bool HasNdrTransferSyntax(PresentationSyntax[]? transferSyntaxes, PresentationSyntax ndr)
    {
        if (transferSyntaxes is null)
        {
            return false;
        }

        Guid ndrUuid = Guid.TryParse(ndr.Uuid?.ToString(), out Guid g) ? g : Guid.Empty;
        foreach (PresentationSyntax candidate in transferSyntaxes)
        {
            if (candidate?.Uuid is null)
            {
                continue;
            }

            if (Guid.TryParse(candidate.Uuid.ToString(), out Guid candidateUuid)
                && candidateUuid == ndrUuid
                && candidate.MajorVersion == ndr.MajorVersion
                && candidate.MinorVersion == ndr.MinorVersion)
            {
                return true;
            }
        }
        return false;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct RpcPduAuthentication(
        bool IsAuthenticated,
        int AuthLength,
        byte AuthenticationServiceCode,
        OpcProtectionLevel ProtectionLevel,
        int ContextId,
        byte[] AuthValue)
    {
        public static RpcPduAuthentication None { get; } = new(false, 0, 0, OpcProtectionLevel.None, 0, []);
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct AuthenticationStrippedFrame(
        byte[] PduBytes,
        byte[] VerificationPduBytes,
        RpcPduAuthentication Authentication);

    private enum AuthenticationTokenAcceptance
    {
        Accepted,
        ProviderNotFound,
        Rejected,
    }

    private sealed class RpcServerAuthenticationState
    {
        private readonly RpcServerAuthenticationOptions _options;
        private IRpcServerAuthenticationAcceptor? _acceptor;
        private RpcServerAuthenticationSession? _session;
        private OpcProtectionLevel _protectionFloor;

        public RpcServerAuthenticationState(RpcServerAuthenticationOptions options) =>
            _options = options;

        public bool RequiresAuthentication => _options.RequireAuthentication;

        public bool IsEstablished => _session is not null;

        public int AuthenticationService { get; private set; }

        public int ContextId { get; private set; }

        public System.Security.Principal.IPrincipal? Principal => _session?.Principal;

        public OpcProtectionLevel ProtectionLevel { get; private set; }

        public bool ShouldProtectPackets =>
            RequiresPacketProtection && EstablishedProtectionContext is not null;

        public bool RequiresPacketProtection =>
            IsEstablished && _protectionFloor >= OpcProtectionLevel.Integrity;

        public int GetVerifierLength(int signedRegionLength, int confidentialLength) =>
            EstablishedProtectionContext?.GetVerifierLength(signedRegionLength, confidentialLength)
            ?? throw new InvalidOperationException("RPC packet protection is not established.");

        public bool TryAcceptToken(
            RpcPduAuthentication authentication,
            CancellationToken cancellationToken,
            out RpcServerAuthenticationTokenResult result)
        {
            result = default;
            if (IsEstablished)
            {
                if (authentication.ProtectionLevel < _protectionFloor)
                {
                    throw new InvalidOperationException("DCE/RPC authentication protection level cannot be downgraded.");
                }
            }

            if (_acceptor is null || AuthenticationService != authentication.AuthenticationServiceCode)
            {
                if (!_options.ProviderSelector.TryGetProvider(
                    authentication.AuthenticationServiceCode,
                    out IRpcServerAuthenticationProvider? provider))
                {
                    return false;
                }
                if (provider.AuthenticationService != authentication.AuthenticationServiceCode)
                {
                    throw new InvalidOperationException(
                        "RPC authentication provider selector returned a provider for a different service.");
                }

                _acceptor = provider.CreateAcceptor()
                    ?? throw new InvalidOperationException("RPC authentication provider returned no acceptor.");
                AuthenticationService = provider.AuthenticationService;
            }

            ContextId = authentication.ContextId;
            ProtectionLevel = authentication.ProtectionLevel;
            result = _acceptor.AcceptToken(
                authentication.AuthValue,
                authentication.ProtectionLevel,
                cancellationToken);
            if (result.Session is not null)
            {
                if (result.Session.AuthenticationService != AuthenticationService)
                {
                    throw new InvalidOperationException(
                        "Authentication session service does not match the selected provider.");
                }

                System.Security.Principal.IPrincipal principal =
                    _options.AuthorizationMapper.MapPrincipal(result.Session.Principal)
                    ?? throw new InvalidOperationException("Authorization mapper returned no principal.");
                _session = result.Session.WithPrincipal(principal);
                ProtectionLevel = _session.ProtectionLevel;
                if (ProtectionLevel > _protectionFloor)
                {
                    _protectionFloor = ProtectionLevel;
                }
            }
            else if (!result.ResponseToken.IsEmpty)
            {
                _session = null;
            }

            return true;
        }

        public void Reset()
        {
            _acceptor = null;
            _session = null;
            _protectionFloor = OpcProtectionLevel.None;
            AuthenticationService = 0;
            ContextId = 0;
            ProtectionLevel = OpcProtectionLevel.None;
        }

        public void SignAndSeal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, out byte[] signature)
        {
            if (_protectionFloor < OpcProtectionLevel.Integrity)
            {
                signature = [];
                return;
            }

            EstablishedProtectionContext!.Protect(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                out signature);
        }

        public bool VerifyAndUnseal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, ReadOnlyMemory<byte> signature)
        {
            if (_protectionFloor < OpcProtectionLevel.Integrity)
            {
                return signature.IsEmpty;
            }

            return EstablishedProtectionContext!.Unprotect(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                signature);
        }

        private IRpcServerProtectionContext? EstablishedProtectionContext =>
            _session?.ProtectionContext;
    }
}
