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
/// <b>Anonymous-only.</b> Authenticated binds (PDUs carrying an auth
/// verifier — i.e. <c>AUTH_LENGTH_OFFSET</c> non-zero) are rejected by
/// closing the connection immediately. The relaxed DCOM ACLs in the
/// fleet's <c>dcom-test-acls.reg</c> permit anonymous calls for the
/// CTT-against-managed-server scenarios; NTLM/Kerberos-authenticated
/// server-side handling is a deliberate follow-up (ocom-7's spike).
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
    private static readonly Action<ILogger, EndPoint, Exception?> ProcessorStarted =
        LoggerMessage.Define<EndPoint>(LogLevel.Debug, new EventId(1, nameof(ProcessorStarted)),
            "RpcServerConnectionProcessor: started for {Remote}");

    private static readonly Action<ILogger, EndPoint, Exception?> ProcessorClosed =
        LoggerMessage.Define<EndPoint>(LogLevel.Debug, new EventId(2, nameof(ProcessorClosed)),
            "RpcServerConnectionProcessor: closed for {Remote}");

    private static readonly Action<ILogger, EndPoint, int, Exception?> AuthRejected =
        LoggerMessage.Define<EndPoint, int>(LogLevel.Warning, new EventId(3, nameof(AuthRejected)),
            "RpcServerConnectionProcessor: rejecting authenticated PDU from {Remote} (auth_length={AuthLength}); anonymous-only listener");

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
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a processor that routes PDUs for the supplied
    /// interface set.
    /// </summary>
    /// <param name="dispatchers">
    /// Interface-ID → dispatcher. Typically populated from the
    /// source-generated <c>*ServerDispatcher</c> wrappers around the
    /// managed server implementations.
    /// </param>
    /// <param name="logger">Optional logger; defaults to <see cref="NullLogger.Instance"/>.</param>
    public RpcServerConnectionProcessor(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(dispatchers);
        _dispatchers = dispatchers;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>Gets the registered interface IDs (for diagnostics / tests).</summary>
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

                if (await RejectIfAuthenticatedAsync(transport, frame, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }

                if (!TryDecodePdu(transport, frame, out ConnectionOrientedPdu? pdu))
                {
                    return;
                }

                bool keepGoing = await HandlePduAsync(transport, pdu!, contextMap, maxTransmitFragment, cancellationToken)
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

    private async ValueTask<bool> RejectIfAuthenticatedAsync(
        IAsyncTransport transport, byte[] frame, CancellationToken cancellationToken)
    {
        int authLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET));
        if (authLength == 0)
        {
            return false;
        }

        AuthRejected(_logger, transport.RemoteEndpoint, authLength, null);
        if (frame[ConnectionOrientedPdu.TYPE_OFFSET] == BindPdu.BIND_TYPE)
        {
            int callId = BinaryPrimitives.ReadInt32LittleEndian(
                frame.AsSpan(ConnectionOrientedPdu.CALL_ID_OFFSET));
            await WriteBindNakAsync(transport, callId,
                BindNoAcknowledgeReason.REASON_NOT_SPECIFIED, cancellationToken).ConfigureAwait(false);
        }
        return true;
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
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        switch (pdu)
        {
            case BindPdu bind:
                BindAcknowledgePdu ack = BuildBindAck(bind, contextMap);
                await WritePduAsync(transport, ack, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
                return true;

            case AlterContextPdu alter:
                AlterContextResponsePdu alterAck = BuildAlterContextResponse(alter, contextMap);
                await WritePduAsync(transport, alterAck, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
                return true;

            case RequestCoPdu request:
                await HandleRequestAsync(transport, request, contextMap, maxTransmitFragment, cancellationToken)
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
                || !_dispatchers.ContainsKey(interfaceId))
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
        Dictionary<int, Guid> contextMap,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        if (!contextMap.TryGetValue(request.ContextId, out Guid interfaceId)
            || !_dispatchers.TryGetValue(interfaceId, out IOpcServerDispatcher? dispatcher))
        {
            UnknownContext(_logger, transport.RemoteEndpoint, request.ContextId, null);
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        ReadOnlyMemory<byte> body;
        try
        {
            body = request.Stub is null
                ? ReadOnlyMemory<byte>.Empty
                : OrpcEnvelope.ExtractRequestBody(request.Stub);
        }
        catch (InvalidOperationException)
        {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        DispatchResult result;
        try
        {
            result = await dispatcher.DispatchAsync(request.Opnum, body, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            DispatcherThrew(_logger, transport.RemoteEndpoint, interfaceId, request.Opnum, ex);
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                FaultCode.UNSPECIFIED_REJECTION, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (result.IsFailure)
        {
            await WriteFaultAsync(transport, request.CallId, request.ContextId,
                (FaultCode)result.Hresult, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
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
        await WritePduAsync(transport, response, maxTransmitFragment, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask WritePduAsync(
        IAsyncTransport transport,
        ConnectionOrientedPdu pdu,
        int maxTransmitFragment,
        CancellationToken cancellationToken)
    {
        if (pdu is IFragmentable fragmentable)
        {
            var fragments = fragmentable.GetFragments(maxTransmitFragment);
            while (fragments.HasNext())
            {
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
        CancellationToken cancellationToken)
    {
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
        CancellationToken cancellationToken)
    {
        var fault = new FaultCoPdu
        {
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
}
