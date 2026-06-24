// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using System.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Rpc;
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
    private readonly AuthenticationSource _authenticationSource;
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
    /// Initializes a processor with an explicit server-side authentication source.
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
    /// interface set plus a per-object IPID registry and authentication source.
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
        _authenticationSource = authenticationSource ?? AuthenticationSource.DefaultInstance;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Gets the registered interface IDs (for diagnostics / tests).
    /// </summary>
    public IReadOnlyCollection<Guid> SupportedInterfaces => (IReadOnlyCollection<Guid>)_dispatchers.Keys;

    /// <summary>
    /// Runs the request loop for one accepted connection until the peer
    /// closes, sends a <see cref="ShutdownPdu"/>, or
    /// <paramref name="cancellationToken"/> fires.
    /// </summary>
    public async ValueTask ProcessConnectionAsync(IAsyncTransport transport, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(transport);

        var contextMap = new Dictionary<int, Guid>();
        var authState = new RpcServerAuthenticationState(_authenticationSource);
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

                if (!TryVerifyRequiredPacketProtection(transport, stripped, authState))
                {
                    return;
                }

                if (!TryDecodePdu(transport, stripped.PduBytes, out ConnectionOrientedPdu? pdu))
                {
                    return;
                }

                bool keepGoing = await HandlePduAsync(transport, pdu!, stripped.Authentication, authState, contextMap, maxTransmitFragment, cancellationToken)
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
            return new AuthenticationStrippedFrame(frame, RpcPduAuthentication.None);
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

        byte[] pduBytes = frame.AsSpan(0, strippedLength).ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)strippedLength);
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), 0);
        return new AuthenticationStrippedFrame(pduBytes, authentication);
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
                return TryCompleteNtlmAuthentication(transport, authentication, authState);

            case RequestCoPdu request when authState.HasAuthenticationSource && !authState.IsEstablished:
                AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
                await WriteFaultAsync(transport, request.CallId, request.ContextId,
                    FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
                return false;

            case RequestCoPdu request:
                await HandleRequestAsync(transport, request, authentication, authState, contextMap, maxTransmitFragment, cancellationToken)
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
            if (!authentication.IsNtlm)
            {
                if (!AuthenticatedBindAllowed(bind))
                {
                    await WriteBindNakAsync(transport, bind.CallId,
                        BindNoAcknowledgeReason.REASON_NOT_SPECIFIED, cancellationToken).ConfigureAwait(false);
                    return false;
                }

                BindAcknowledgePdu legacyAck = BuildBindAck(bind, contextMap);
                await WritePduAsync(transport, legacyAck, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
                return true;
            }

            if (!TryCreateNtlmChallenge(transport, authentication, authState, out byte[] challengeToken))
            {
                await WriteBindNakAsync(transport, bind.CallId,
                    BindNoAcknowledgeReason.REASON_NOT_SPECIFIED, cancellationToken).ConfigureAwait(false);
                return false;
            }

            BindAcknowledgePdu authenticatedAck = BuildBindAck(bind, contextMap);
            await WritePduAsync(transport, authenticatedAck, maxTransmitFragment, authentication, challengeToken, cancellationToken)
                .ConfigureAwait(false);
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
            if (!TryCreateNtlmChallenge(transport, authentication, authState, out byte[] challengeToken))
            {
                await WriteBindNakAsync(transport, alter.CallId,
                    BindNoAcknowledgeReason.REASON_NOT_SPECIFIED, cancellationToken).ConfigureAwait(false);
                return false;
            }

            await WritePduAsync(transport, alterAck, maxTransmitFragment, authentication, challengeToken, cancellationToken)
                .ConfigureAwait(false);
            return true;
        }

        await WritePduAsync(transport, alterAck, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
        return true;
    }

    private bool TryCreateNtlmChallenge(
        IAsyncTransport transport,
        RpcPduAuthentication authentication,
        RpcServerAuthenticationState authState,
        out byte[] challengeToken)
    {
        challengeToken = [];
        if (!authState.HasAuthenticationSource
            || authentication.AuthenticationServiceCode != NtlmAuthentication.AUTHENTICATIONSERVICENTLM
            || authentication.AuthValue.Length == 0)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
            return false;
        }

        try
        {
            var type1 = new Type1Message(authentication.AuthValue);
            challengeToken = authState.CreateChallenge(type1, authentication.ProtectionLevel);
            return challengeToken.Length > 0;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or IOException)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, ex);
            return false;
        }
    }

    private bool TryCompleteNtlmAuthentication(
        IAsyncTransport transport,
        RpcPduAuthentication authentication,
        RpcServerAuthenticationState authState)
    {
        if (authentication.AuthenticationServiceCode != NtlmAuthentication.AUTHENTICATIONSERVICENTLM
            || authentication.AuthValue.Length == 0)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
            return false;
        }

        try
        {
            authState.Authenticate(new Type3Message(authentication.AuthValue));
            return true;
        }
        catch (Exception ex) when (ex is SecurityException or InvalidOperationException or ArgumentException or IOException)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, ex);
            return false;
        }
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

        if (!TryExtractRequestBody(request, out ReadOnlyMemory<byte> body))
        {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
            return;
        }

        DispatchResult? result = await TryDispatchAsync(transport, dispatcher, interfaceId, request, authentication, authState, body, cancellationToken)
            .ConfigureAwait(false);
        if (result is null)
        {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, authState, cancellationToken).ConfigureAwait(false);
            return;
        }

        await WriteRequestOutcomeAsync(transport, request, result.Value, maxTransmitFragment, authState, cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool TryExtractRequestBody(RequestCoPdu request, out ReadOnlyMemory<byte> body)
    {
        body = ReadOnlyMemory<byte>.Empty;
        if (request.Stub is null)
        {
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
        RpcServerAuthenticationState authState,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken)
    {
        try
        {
            if (dispatcher is IRpcRequestContextDispatcher contextDispatcher)
            {
                var requestContext = new RpcRequestContext(
                    authentication.IsAuthenticated,
                    authentication.ProtectionLevel,
                    transport.RemoteEndpoint);
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

        byte[] responseStub = OrpcEnvelope.BuildResponseStub(result.Payload);
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
        RpcServerAuthenticationState authState)
    {
        byte pduType = stripped.PduBytes[ConnectionOrientedPdu.TYPE_OFFSET];
        if (!authState.ShouldProtectPackets)
        {
            return true;
        }

        if (pduType != RequestCoPdu.REQUEST_TYPE)
        {
            return true;
        }

        if (!stripped.Authentication.IsNtlm || stripped.Authentication.AuthValue.Length == 0)
        {
            AuthRejected(_logger, transport.RemoteEndpoint, stripped.Authentication.AuthLength, null);
            return false;
        }

        if (!authState.VerifyAndUnseal(stripped.PduBytes.AsSpan(ConnectionOrientedPdu.HEADER_LENGTH), stripped.Authentication.AuthValue))
        {
            AuthRejected(_logger, transport.RemoteEndpoint, stripped.Authentication.AuthLength, null);
            return false;
        }

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
        int authValueLength = authState.VerifierLength;
        int fragmentLength = verifierStart + AuthenticationVerifierHeaderLength + authValueLength;
        if (fragmentLength > ushort.MaxValue)
        {
            throw new InvalidOperationException("DCE/RPC fragment length exceeds the 16-bit PDU limit.");
        }

        byte[] protectedPdu = new byte[fragmentLength];
        pduBytes.CopyTo(protectedPdu, 0);
        Span<byte> verifier = protectedPdu.AsSpan(verifierStart, AuthenticationVerifierHeaderLength);
        verifier[0] = NtlmAuthentication.AUTHENTICATIONSERVICENTLM;
        verifier[1] = (byte)ToRpcProtectionLevel(authState.ProtectionLevel);
        verifier[2] = (byte)padding;
        verifier[3] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(verifier[4..], 0);

        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(protectedPdu.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), (ushort)authValueLength);

        int bodyStart = ConnectionOrientedPdu.HEADER_LENGTH;
        int bodyLength = verifierStart - bodyStart;
        authState.SignAndSeal(protectedPdu.AsSpan(bodyStart, bodyLength), out byte[] signature);
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

    private static bool IsPacketProtectedPdu(byte pduType) =>
        pduType is RequestCoPdu.REQUEST_TYPE or ResponseCoPdu.RESPONSE_TYPE or FaultCoPdu.FAULT_TYPE;

    private static int PaddingTo(int length, int alignment)
    {
        int remainder = length % alignment;
        return remainder == 0 ? 0 : alignment - remainder;
    }

    private static NdrCodec CreateNdrCodec(byte[] buffer)
    {
        var ndrBuffer = new NdrBuffer(buffer, 0);
        ndrBuffer.SetLength(buffer.Length);
        return new NdrCodec { Buffer = ndrBuffer, Format = NdrFormat.DEFAULT_FORMAT };
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

        public bool IsNtlm => AuthenticationServiceCode == NtlmAuthentication.AUTHENTICATIONSERVICENTLM;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct AuthenticationStrippedFrame(
        byte[] PduBytes,
        RpcPduAuthentication Authentication);

    private sealed class RpcServerAuthenticationState
    {
        private readonly AuthenticationSource _source;
        private readonly PropertyBag _properties = new();
        private Type2Message? _type2;
        private NtlmAuthentication? _context;

        public RpcServerAuthenticationState(AuthenticationSource source) =>
            _source = source;

        public bool HasAuthenticationSource => _source is not NullAuthenticationSource;

        public bool IsEstablished => _context?.Security is not null;

        public OpcProtectionLevel ProtectionLevel { get; private set; }

        public bool ShouldProtectPackets => IsEstablished && ProtectionLevel >= OpcProtectionLevel.Integrity;

        public int VerifierLength => EstablishedSecurity.VerifierLength;

        public byte[] CreateChallenge(Type1Message type1, OpcProtectionLevel protectionLevel)
        {
            ProtectionLevel = protectionLevel;
            byte[] token = _source.CreateChallenge(_properties, type1);
            _type2 = new Type2Message(token);
            return token;
        }

        public void Authenticate(Type3Message type3)
        {
            if (_type2 is null)
            {
                throw new InvalidOperationException("NTLM Type3 received before Type2 challenge was created.");
            }

            _source.Authenticate(_properties, _type2, type3);
            _context = ConfiguredAuthenticationSource.GetEstablishedContext(_properties)
                ?? throw new InvalidOperationException("Authentication source did not establish an NTLM security context.");
        }

        public void SignAndSeal(Span<byte> pduBody, out byte[] signature)
        {
            if (ProtectionLevel < OpcProtectionLevel.Integrity)
            {
                signature = [];
                return;
            }

            ISecurity security = EstablishedSecurity;
            var buffer = new byte[pduBody.Length + security.VerifierLength];
            pduBody.CopyTo(buffer.AsSpan());
            NdrCodec ndr = CreateNdrCodec(buffer);
            security.ProcessOutgoing(ndr, 0, pduBody.Length, pduBody.Length, isFragmented: false);
            buffer.AsSpan(0, pduBody.Length).CopyTo(pduBody);
            signature = buffer.AsSpan(pduBody.Length, security.VerifierLength).ToArray();
        }

        public bool VerifyAndUnseal(Span<byte> pduBody, ReadOnlyMemory<byte> signature)
        {
            if (ProtectionLevel < OpcProtectionLevel.Integrity)
            {
                return signature.IsEmpty;
            }

            ISecurity security = EstablishedSecurity;
            if (signature.Length != security.VerifierLength)
            {
                return false;
            }

            var buffer = new byte[pduBody.Length + security.VerifierLength];
            pduBody.CopyTo(buffer.AsSpan());
            signature.Span.CopyTo(buffer.AsSpan(pduBody.Length));
            NdrCodec ndr = CreateNdrCodec(buffer);
            try
            {
                security.ProcessIncoming(ndr, 0, pduBody.Length, pduBody.Length, isFragmented: false);
            }
            catch (IntegrityException)
            {
                return false;
            }

            buffer.AsSpan(0, pduBody.Length).CopyTo(pduBody);
            return true;
        }

        private ISecurity EstablishedSecurity => _context?.Security ?? throw new InvalidOperationException(
            "NTLM session security is not established.");
    }
}
