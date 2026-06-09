//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc;
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
/// for the CTT-against-managed-server scenarios; full NTLM/Kerberos
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
public sealed class RpcServerConnectionProcessor {
    private const int AuthenticationVerifierHeaderLength = 8;
    private const int E_ACCESSDENIED = unchecked((int)0x80070005u);

    private static readonly Action<ILogger, EndPoint, Exception?> ProcessorStarted =
        LoggerMessage.Define<EndPoint>(LogLevel.Debug, new EventId(1, nameof(ProcessorStarted)),
            "RpcServerConnectionProcessor: started for {Remote}");

    private static readonly Action<ILogger, EndPoint, Exception?> ProcessorClosed =
        LoggerMessage.Define<EndPoint>(LogLevel.Debug, new EventId(2, nameof(ProcessorClosed)),
            "RpcServerConnectionProcessor: closed for {Remote}");

    private static readonly Action<ILogger, EndPoint, int, Exception?> AuthRejected =
        LoggerMessage.Define<EndPoint, int>(LogLevel.Warning, new EventId(3, nameof(AuthRejected)),
            "RpcServerConnectionProcessor: rejecting authenticated PDU from {Remote} (auth_length={AuthLength}); dispatcher does not accept RPC authentication context");

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
        : this(dispatchers, objectRegistry: null, logger) {
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
        ILogger? logger = null) {
        ArgumentNullException.ThrowIfNull(dispatchers);
        _dispatchers = dispatchers;
        _objectRegistry = objectRegistry;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>Gets the registered interface IDs (for diagnostics / tests).</summary>
    public IReadOnlyCollection<Guid> SupportedInterfaces => (IReadOnlyCollection<Guid>)_dispatchers.Keys;

    /// <summary>
    /// Runs the request loop for one accepted connection until the peer
    /// closes, sends a <see cref="ShutdownPdu"/>, or
    /// <paramref name="cancellationToken"/> fires.
    /// </summary>
    public async ValueTask ProcessConnectionAsync(IAsyncTransport transport, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(transport);

        var contextMap = new Dictionary<int, Guid>();
        int maxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE;

        ProcessorStarted(_logger, transport.RemoteEndpoint, null);
        try {
            while (!cancellationToken.IsCancellationRequested) {
                byte[]? frame = await ReadFrameOrNullAsync(transport, cancellationToken).ConfigureAwait(false);
                if (frame is null) {
                    return;
                }

                if (!TryStripAuthenticationVerifier(transport, frame, out AuthenticationStrippedFrame stripped)) {
                    return;
                }

                if (!TryDecodePdu(transport, stripped.PduBytes, out ConnectionOrientedPdu? pdu)) {
                    return;
                }

                bool keepGoing = await HandlePduAsync(transport, pdu!, stripped.Authentication, contextMap, maxTransmitFragment, cancellationToken)
                    .ConfigureAwait(false);
                if (!keepGoing) {
                    return;
                }

                if (pdu is BindAcknowledgePdu ackUpdate && ackUpdate.MaxTransmitFragment > 0) {
                    maxTransmitFragment = ackUpdate.MaxTransmitFragment;
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException) {
            ProcessorFaulted(_logger, transport.RemoteEndpoint, ex);
        }
        finally {
            ProcessorClosed(_logger, transport.RemoteEndpoint, null);
        }
    }

    private static async ValueTask<byte[]?> ReadFrameOrNullAsync(
        IAsyncTransport transport, CancellationToken cancellationToken) {
        try {
            return await PduCodec.ReadPduFrameAsync(transport.Input, cancellationToken).ConfigureAwait(false);
        }
        catch (EndOfStreamException) {
            return null;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return null;
        }
    }

    private bool TryStripAuthenticationVerifier(
        IAsyncTransport transport,
        byte[] frame,
        out AuthenticationStrippedFrame stripped) {
        try {
            stripped = StripAuthenticationVerifier(frame);
            return true;
        }
        catch (InvalidOperationException ex) {
            UnsupportedPduType(_logger, transport.RemoteEndpoint, frame[ConnectionOrientedPdu.TYPE_OFFSET], ex);
            stripped = default;
            return false;
        }
    }

    private static AuthenticationStrippedFrame StripAuthenticationVerifier(byte[] frame) {
        int authLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET));
        if (authLength == 0) {
            return new AuthenticationStrippedFrame(frame, RpcPduAuthentication.None);
        }

        int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET));
        int verifierStart = fragmentLength - authLength - AuthenticationVerifierHeaderLength;
        if (verifierStart < ConnectionOrientedPdu.HEADER_LENGTH
            || verifierStart + AuthenticationVerifierHeaderLength > frame.Length) {
            throw new InvalidOperationException("DCE/RPC authentication verifier is malformed.");
        }

        int padding = frame[verifierStart + 2];
        int strippedLength = verifierStart - padding;
        if (strippedLength < ConnectionOrientedPdu.HEADER_LENGTH || strippedLength > frame.Length) {
            throw new InvalidOperationException("DCE/RPC authentication verifier padding is malformed.");
        }

        var authentication = new RpcPduAuthentication(
            true,
            authLength,
            ToOpcProtectionLevel((ProtectionLevel)frame[verifierStart + 1]));

        byte[] pduBytes = frame.AsSpan(0, strippedLength).ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)strippedLength);
        BinaryPrimitives.WriteUInt16LittleEndian(pduBytes.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), 0);
        return new AuthenticationStrippedFrame(pduBytes, authentication);
    }

    private bool TryDecodePdu(IAsyncTransport transport, byte[] frame, out ConnectionOrientedPdu? pdu) {
        try {
            pdu = PduCodec.DecodePdu(frame);
            return true;
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException) {
            UnsupportedPduType(_logger, transport.RemoteEndpoint, frame[ConnectionOrientedPdu.TYPE_OFFSET], ex);
            pdu = null;
            return false;
        }
    }

    private async ValueTask<bool> HandlePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        RpcPduAuthentication authentication,
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken) {
        switch (pdu) {
            case BindPdu bind:
                if (authentication.IsAuthenticated && !AuthenticatedBindAllowed(bind)) {
                    AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
                    await WriteBindNakAsync(transport, bind.CallId,
                        BindNoAcknowledgeReason.REASON_NOT_SPECIFIED, cancellationToken).ConfigureAwait(false);
                    return false;
                }

                BindAcknowledgePdu ack = BuildBindAck(bind, contextMap);
                await WritePduAsync(transport, ack, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
                return true;

            case AlterContextPdu alter:
                AlterContextResponsePdu alterAck = BuildAlterContextResponse(alter, contextMap);
                await WritePduAsync(transport, alterAck, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
                return true;

            case RequestCoPdu request:
                await HandleRequestAsync(transport, request, authentication, contextMap, maxTransmitFragment, cancellationToken)
                    .ConfigureAwait(false);
                return true;

            case ShutdownPdu:
                return false;

            case CancelCoPdu or OrphanedPdu:
                return true;

            default:
                UnsupportedPduType(_logger, transport.RemoteEndpoint, pdu.Type, null);
                await WriteFaultAsync(transport, pdu.CallId, 0, FaultCode.UNSPECIFIED_REJECTION,
                    maxTransmitFragment, cancellationToken).ConfigureAwait(false);
                return true;
        }
    }

    private BindAcknowledgePdu BuildBindAck(BindPdu bind, Dictionary<int, Guid> contextMap) {
        PresentationResult[] results = NegotiateContexts(bind.ContextList, contextMap);
        int associationGroupId = bind.AssociationGroupId != 0
            ? bind.AssociationGroupId
            : Random.Shared.Next(1, int.MaxValue);

        int negotiatedMaxTransmit = Math.Min(bind.MaxTransmitFragment, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);
        int negotiatedMaxReceive = Math.Min(bind.MaxReceiveFragment, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

        return new BindAcknowledgePdu {
            MaxTransmitFragment = negotiatedMaxTransmit,
            MaxReceiveFragment = negotiatedMaxReceive,
            AssociationGroupId = associationGroupId,
            SecondaryAddress = new Port(),
            ResultList = results,
            CallId = bind.CallId,
        };
    }

    private AlterContextResponsePdu BuildAlterContextResponse(AlterContextPdu alter, Dictionary<int, Guid> contextMap) {
        PresentationResult[] results = NegotiateContexts(alter.ContextList, contextMap);
        return new AlterContextResponsePdu {
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
        Dictionary<int, Guid> contextMap) {
        if (proposedContexts is null || proposedContexts.Length == 0) {
            return [];
        }

        var ndrTransferSyntax = new PresentationSyntax(NdrCodec.NDR_SYNTAX);
        var results = new PresentationResult[proposedContexts.Length];
        for (int i = 0; i < proposedContexts.Length; i++) {
            PresentationContext proposal = proposedContexts[i];
            if (!TryGuidFromUuid(proposal.AbstractSyntax?.Uuid, out Guid interfaceId)
                || !SupportsInterface(interfaceId)) {
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

            if (!HasNdrTransferSyntax(proposal.TransferSyntaxes, ndrTransferSyntax)) {
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
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken) {
        if (!contextMap.TryGetValue(request.ContextId, out Guid interfaceId)) {
            UnknownContext(_logger, transport.RemoteEndpoint, request.ContextId, null);
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        IOpcServerDispatcher? dispatcher = ResolveDispatcher(request, interfaceId);
        if (dispatcher is null) {
            UnknownContext(_logger, transport.RemoteEndpoint, request.ContextId, null);
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!TryExtractRequestBody(request, out ReadOnlyMemory<byte> body)) {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        DispatchResult? result = await TryDispatchAsync(transport, dispatcher, interfaceId, request, authentication, body, cancellationToken)
            .ConfigureAwait(false);
        if (result is null) {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        await WriteRequestOutcomeAsync(transport, request, result.Value, maxTransmitFragment, cancellationToken)
            .ConfigureAwait(false);
    }

    private static bool TryExtractRequestBody(RequestCoPdu request, out ReadOnlyMemory<byte> body) {
        body = ReadOnlyMemory<byte>.Empty;
        if (request.Stub is null) {
            return true;
        }
        try {
            body = OrpcEnvelope.ExtractRequestBody(request.Stub);
            return true;
        }
        catch (InvalidOperationException) {
            return false;
        }
    }

    private async ValueTask<DispatchResult?> TryDispatchAsync(
        IAsyncTransport transport,
        IOpcServerDispatcher dispatcher,
        Guid interfaceId,
        RequestCoPdu request,
        RpcPduAuthentication authentication,
        ReadOnlyMemory<byte> body,
        CancellationToken cancellationToken) {
        try {
            if (dispatcher is IRpcRequestContextDispatcher contextDispatcher) {
                var requestContext = new RpcRequestContext(
                    authentication.IsAuthenticated,
                    authentication.ProtectionLevel,
                    transport.RemoteEndpoint);
                return await contextDispatcher.DispatchAsync(request.Opnum, body, requestContext, cancellationToken).ConfigureAwait(false);
            }

            if (authentication.IsAuthenticated) {
                AuthRejected(_logger, transport.RemoteEndpoint, authentication.AuthLength, null);
                return DispatchResult.Fault(E_ACCESSDENIED);
            }

            return await dispatcher.DispatchAsync(request.Opnum, body, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        }
        catch (Exception ex) {
            DispatcherThrew(_logger, transport.RemoteEndpoint, interfaceId, request.Opnum, ex);
            return null;
        }
    }

    private static async ValueTask WriteRequestOutcomeAsync(
        IAsyncTransport transport,
        RequestCoPdu request,
        DispatchResult result,
        int maxTransmitFragment,
        CancellationToken cancellationToken) {
        if (result.IsFailure) {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                (FaultCode)result.Hresult, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        byte[] responseStub = OrpcEnvelope.BuildResponseStub(result.Payload);
        var response = new ResponseCoPdu {
            AllocationHint = responseStub.Length,
            ContextId = request.ContextId,
            Stub = responseStub,
            CallId = request.CallId,
        };
        await WritePduAsync(transport, response, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
    }

    private bool SupportsInterface(Guid interfaceId) =>
        _dispatchers.ContainsKey(interfaceId) || (_objectRegistry?.ContainsInterface(interfaceId) ?? false);

    private IOpcServerDispatcher? ResolveDispatcher(RequestCoPdu request, Guid interfaceId) {
        // Per-object route: if the request carries an Object UUID and the
        // registry knows it for the requested interface, prefer it over
        // the root map. Otherwise fall through to the root dispatcher set.
        if (_objectRegistry is not null && request.Object is not null
            && Guid.TryParse(request.Object.ToString(), out Guid ipid)
            && _objectRegistry.TryGetDispatcher(ipid, interfaceId, out IOpcServerDispatcher perObject)) {
            return perObject;
        }

        return _dispatchers.TryGetValue(interfaceId, out IOpcServerDispatcher? root) ? root : null;
    }

    private static async ValueTask WritePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        int maxTransmitFragment,
        CancellationToken cancellationToken) {
        if (pdu is IFragmentable fragmentable) {
            var fragments = fragmentable.GetFragments(maxTransmitFragment);
            while (fragments.HasNext()) {
                await WriteSinglePduAsync(transport, fragments.Next(), maxTransmitFragment, cancellationToken)
                    .ConfigureAwait(false);
            }
            return;
        }

        await WriteSinglePduAsync(transport, pdu, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WriteSinglePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        int maxTransmitFragment,
        CancellationToken cancellationToken) {
        byte[] bytes = PduCodec.EncodePdu(pdu, maxTransmitFragment);
        Memory<byte> destination = transport.Output.GetMemory(bytes.Length);
        bytes.AsSpan().CopyTo(destination.Span);
        transport.Output.Advance(bytes.Length);
        await transport.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WriteFaultAsync(
        IAsyncTransport transport,
        int callId,
        int contextId,
        FaultCode status,
        int maxTransmitFragment,
        CancellationToken cancellationToken) {
        var fault = new FaultCoPdu {
            CallId = callId,
            ContextId = contextId,
            Status = status,
        };
        fault.SetFlag(ConnectionOrientedPdu.PFC_DID_NOT_EXECUTE, true);
        await WritePduAsync(transport, fault, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WriteBindNakAsync(
        IAsyncTransport transport,
        int callId,
        BindNoAcknowledgeReason reason,
        CancellationToken cancellationToken) {
        var nak = new BindNoAcknowledgePdu {
            CallId = callId,
            RejectReason = reason,
        };
        await WriteSinglePduAsync(transport, nak, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE, cancellationToken)
            .ConfigureAwait(false);
    }

    private bool AuthenticatedBindAllowed(BindPdu bind) {
        if (bind.ContextList is null || bind.ContextList.Length == 0) {
            return false;
        }

        bool hasKnownContext = false;
        foreach (PresentationContext context in bind.ContextList) {
            if (TryGuidFromUuid(context.AbstractSyntax?.Uuid, out Guid interfaceId)
                && _dispatchers.TryGetValue(interfaceId, out IOpcServerDispatcher? dispatcher)) {
                hasKnownContext = true;
                if (dispatcher is not IRpcRequestContextDispatcher) {
                    return false;
                }
            }
        }

        return hasKnownContext;
    }

    private static OpcProtectionLevel ToOpcProtectionLevel(ProtectionLevel protectionLevel) => protectionLevel switch {
        ProtectionLevel.PROTECTION_LEVEL_CONNECT => OpcProtectionLevel.Connect,
        ProtectionLevel.PROTECTION_LEVEL_CALL => OpcProtectionLevel.Call,
        ProtectionLevel.PROTECTION_LEVEL_PACKET => OpcProtectionLevel.Packet,
        ProtectionLevel.PROTECTION_LEVEL_INTEGRITY => OpcProtectionLevel.Integrity,
        ProtectionLevel.PROTECTION_LEVEL_PRIVACY => OpcProtectionLevel.Privacy,
        _ => OpcProtectionLevel.None,
    };

    private static bool TryGuidFromUuid(UUID? uuid, out Guid value) {
        if (uuid is null) {
            value = Guid.Empty;
            return false;
        }
        return Guid.TryParse(uuid.ToString(), out value);
    }

    private static bool HasNdrTransferSyntax(PresentationSyntax[]? transferSyntaxes, PresentationSyntax ndr) {
        if (transferSyntaxes is null) {
            return false;
        }

        Guid ndrUuid = Guid.TryParse(ndr.Uuid?.ToString(), out Guid g) ? g : Guid.Empty;
        foreach (PresentationSyntax candidate in transferSyntaxes) {
            if (candidate?.Uuid is null) {
                continue;
            }

            if (Guid.TryParse(candidate.Uuid.ToString(), out Guid candidateUuid)
                && candidateUuid == ndrUuid
                && candidate.MajorVersion == ndr.MajorVersion
                && candidate.MinorVersion == ndr.MinorVersion) {
                return true;
            }
        }
        return false;
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct RpcPduAuthentication(
        bool IsAuthenticated,
        int AuthLength,
        OpcProtectionLevel ProtectionLevel) {
        public static RpcPduAuthentication None { get; } = new(false, 0, OpcProtectionLevel.None);
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct AuthenticationStrippedFrame(
        byte[] PduBytes,
        RpcPduAuthentication Authentication);
}
