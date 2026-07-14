// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using PacketDotNet;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Stateful DCE/RPC PDU decoder that bridges
/// <see cref="CapturedPacket"/> records back into structured
/// <see cref="DecodedOpcPdu"/> records by reusing
/// <see cref="PduCodec"/> + <see cref="OrpcEnvelope"/> from
/// <c>Opc.Classic.Dcom</c>.
/// </summary>
/// <remarks>
/// <para>
/// Two source flavours:
/// </para>
/// <list type="number">
///   <item><description>
///     Pcap-style frames (<see cref="CapturedPacket.LinkType"/> != 0):
///     extract Ethernet → IPv4/IPv6 → TCP, reassemble segments per
///     <c>(srcIp,srcPort,dstIp,dstPort)</c> flow into DCE/RPC PDU
///     frames (using the 16-bit fragment-length field at common-header
///     offset 8), then call <see cref="PduCodec.DecodePdu"/> on each
///     full PDU.
///   </description></item>
///   <item><description>
///     Hex-source records (<see cref="CapturedPacket.LinkType"/> == 0):
///     the captured bytes ARE already an ORPC body (request or
///     response), surfaced as a synthetic <c>orpc_body</c> PDU with the
///     metadata annotations carried through.
///   </description></item>
/// </list>
/// <para>
/// Bind / alter_context PDUs populate a per-flow context-id → IID
/// table, so subsequent request / response / fault PDUs on the same
/// flow can resolve their <see cref="DecodedOpcPdu.InterfaceId"/>.
/// </para>
/// </remarks>
public sealed class OpcDcomDecoder
{
    private readonly ILogger _logger;
    private readonly NtlmPassiveUnwrapper? _unwrapper;
    private readonly Dictionary<FlowKey, FlowState> _flows = new();

    public OpcDcomDecoder(ILogger? logger = null)
        : this(unwrapper: null, logger)
    {
    }

    /// <summary>
    /// Creates a decoder configured with an optional NTLM passive
    /// unwrapper. When non-null, captured sign/seal-protected
    /// Request / Response / Fault PDUs whose flow has been observed
    /// to start with a Bind (so per-direction roles are known) will
    /// be decrypted + verified inline. The resulting
    /// <see cref="DecodedOpcPdu.AuthUnwrapStatus"/> /
    /// <see cref="DecodedOpcPdu.AuthUnwrapReason"/> fields reflect
    /// the unwrap outcome; the PDU's stub field reflects the
    /// plaintext bytes on <see cref="NtlmUnwrapStatus.Decrypted"/>.
    /// </summary>
    /// <param name="unwrapper">
    /// Optional NTLM passive unwrapper. Null = no unwrap attempted
    /// (back-compat with the parameterless constructor).
    /// </param>
    /// <param name="logger">Optional logger.</param>
    public OpcDcomDecoder(NtlmPassiveUnwrapper? unwrapper, ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
        _unwrapper = unwrapper;
    }

    /// <summary>
    /// Decode a single captured packet into zero, one, or more
    /// <see cref="DecodedOpcPdu"/> records (one TCP segment may
    /// complete multiple PDUs when small; large PDUs require multiple
    /// segments).
    /// </summary>
    public IEnumerable<DecodedOpcPdu> Decode(CapturedPacket packet)
    {
        foreach (DecodedDcomFrame result in DecodeDetailed(packet))
        {
            if (result.Pdu is not null)
            {
                yield return result.Pdu;
            }
        }
    }

    /// <summary>
    /// Internal decode surface that retains raw frame/stub bytes and
    /// structured failures for replay without adding those bytes to the
    /// ordinary MCP DTO.
    /// </summary>
    internal IEnumerable<DecodedDcomFrame> DecodeDetailed(CapturedPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (packet.LinkType == 0)
        {
            DecodedOpcPdu pdu = DecodeHexSourceRecord(packet);
            yield return new DecodedDcomFrame(
                pdu,
                pdu.PduType,
                RawFrame: null,
                StubBytes: packet.Data.ToArray(),
                Failure: null);
            yield break;
        }

        if (packet.Data.IsEmpty)
        {
            yield break;
        }

        ValidateCapturedFrame(packet);

        Packet parsed;
        try
        {
            parsed = Packet.ParsePacket((LinkLayers)packet.LinkType, packet.Data.ToArray());
        }
        catch (Exception ex) when (ex is FormatException or IndexOutOfRangeException or ArgumentException)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(ex, "OpcDcomDecoder: skipping frame that PacketDotNet refused to parse.");
            }
            yield break;
        }

        TcpPacket? tcp = parsed.Extract<TcpPacket>();
        if (tcp is null)
        {
            yield break;
        }

        IPAddress? srcIp = null;
        IPAddress? dstIp = null;
        IPv4Packet? ipv4 = parsed.Extract<IPv4Packet>();
        if (ipv4 is not null)
        {
            srcIp = ipv4.SourceAddress;
            dstIp = ipv4.DestinationAddress;
        }
        else
        {
            IPv6Packet? ipv6 = parsed.Extract<IPv6Packet>();
            if (ipv6 is not null)
            {
                srcIp = ipv6.SourceAddress;
                dstIp = ipv6.DestinationAddress;
            }
        }

        if (srcIp is null || dstIp is null)
        {
            yield break;
        }

        byte[] payload = tcp.PayloadData;
        if (payload is null || payload.Length == 0)
        {
            yield break;
        }

        FlowKey key = new(srcIp, tcp.SourcePort, dstIp, tcp.DestinationPort);
        FlowKey reverseKey = new(dstIp, tcp.DestinationPort, srcIp, tcp.SourcePort);
        (FlowState flow, FlowState reverseFlow) = GetOrCreateFlowPair(key, reverseKey);

        flow.Append(payload);

        while (flow.TryDequeueFrame(out byte[]? frame))
        {
            yield return TryDecodeFrame(frame, packet.Timestamp, flow, reverseFlow, key);
        }
    }

    /// <summary>
    /// Convenience overload: decode an entire enumeration of captured
    /// packets, materialising the results into a list (call sites that
    /// stream should iterate via <see cref="Decode(CapturedPacket)"/>
    /// instead).
    /// </summary>
    public IReadOnlyList<DecodedOpcPdu> DecodeAll(IEnumerable<CapturedPacket> packets)
    {
        ArgumentNullException.ThrowIfNull(packets);
        var output = new List<DecodedOpcPdu>();
        foreach (CapturedPacket packet in packets)
        {
            foreach (DecodedOpcPdu decoded in Decode(packet))
            {
                output.Add(decoded);
            }
        }
        return output;
    }

    private DecodedDcomFrame TryDecodeFrame(byte[] frame, DateTimeOffset timestamp, FlowState flow, FlowState reverseFlow, FlowKey key)
    {
        // Pre-PduCodec hook: peek at PTYPE (frame[2]) to (a) propagate the
        // client/server orientation across both halves of the bidirectional
        // flow when we see a Bind on this side, and (b) attempt NTLM
        // auth-trailer unwrap when configured.
        if (frame.Length >= ConnectionOrientedPdu.HEADER_LENGTH)
        {
            byte ptype = frame[2];
            if (ptype == BindPdu.BIND_TYPE && flow.KnownDirection is null)
            {
                // First Bind PDU on this flow → this side is the client; the
                // reverse-direction flow is the server side. Idempotent —
                // only the first Bind wins, subsequent Binds on the same
                // flow leave the direction as-is (e.g. multi-bind sessions
                // re-using the same TCP connection).
                flow.KnownDirection = NtlmDirection.ClientToServer;
                reverseFlow.KnownDirection ??= NtlmDirection.ServerToClient;
            }
        }

        (NtlmUnwrapStatus? authStatus, string? authReason) = TryUnwrapInPlace(frame, flow);
        if (authStatus is NtlmUnwrapStatus.Decrypted or NtlmUnwrapStatus.IntegrityVerified)
        {
            int strippedLength = BinaryPrimitives.ReadUInt16LittleEndian(
                frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2));
            if (strippedLength < frame.Length)
            {
                Array.Resize(ref frame, strippedLength);
            }
        }

        ConnectionOrientedPdu pdu;
        try
        {
            pdu = PduCodec.DecodePdu(frame);
        }
        catch (Exception ex) when (ex is InvalidOperationException or IndexOutOfRangeException or ArgumentException)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(ex, "OpcDcomDecoder: PduCodec rejected frame ({Bytes} bytes) on flow {Flow}.", frame.Length, key);
            }
            return new DecodedDcomFrame(
                Pdu: null,
                PduType: GetPduType(frame),
                RawFrame: frame,
                StubBytes: null,
                Failure: new DcomDecodeFailure("pdu_codec", ex.GetType().Name, ex.Message));
        }

        DecodedOpcPdu? projected = pdu switch
        {
            BindPdu bind => ProjectBind(bind.ContextList, bind.CallId, timestamp, key, flow, isAlter: false),
            AlterContextPdu alter => ProjectBind(alter.ContextList, alter.CallId, timestamp, key, flow, isAlter: true),
            BindAcknowledgePdu ack => ProjectBindAck(ack.ResultList, ack.CallId, timestamp, key, flow, isAlter: false),
            AlterContextResponsePdu ack => ProjectBindAck(ack.ResultList, ack.CallId, timestamp, key, flow, isAlter: true),
            BindNoAcknowledgePdu nak => ProjectBindNak(nak, timestamp, key),
            RequestCoPdu request => ProjectRequest(request, timestamp, key, flow, authStatus, authReason),
            ResponseCoPdu response => ProjectResponse(response, timestamp, key, flow, authStatus, authReason),
            FaultCoPdu fault => ProjectFault(fault, timestamp, key, flow, authStatus, authReason),
            ShutdownPdu => ProjectSimple("shutdown", timestamp, key),
            Auth3Pdu => ProjectSimple("auth3", timestamp, key),
            CancelCoPdu => ProjectSimple("cancel", timestamp, key),
            OrphanedPdu => ProjectSimple("orphaned", timestamp, key),
            _ => null,
        };

        byte[]? stub = pdu switch
        {
            RequestCoPdu request => request.Stub,
            ResponseCoPdu response => response.Stub,
            FaultCoPdu fault => fault.Stub,
            _ => null,
        };
        return new DecodedDcomFrame(projected, GetPduType(frame), frame, stub, Failure: null);
    }

    // NTLMSSP auth-service code (MS-RPCE §2.2.1.1.7). Set in the auth verifier header's
    // auth_type field for NTLM-protected RPC PDUs. Other values include 0x09 SPNEGO,
    // 0x10 Kerberos — we only attempt unwrap for NTLM.
    private const byte AuthServiceCodeNtlm = 0x0A;
    // DCE/RPC PDU PTYPE constants (subset relevant to sign/seal unwrap).
    private const byte PtypeRequest = 0x00;
    private const byte PtypeResponse = 0x02;
    private const byte PtypeFault = 0x03;

    /// <summary>
    /// Attempts to decrypt + verify the NTLM sign-and-seal auth trailer
    /// on a captured Request / Response / Fault frame, mutating
    /// <paramref name="frame"/> in place from ciphertext to plaintext on
    /// success. Returns <c>(null, null)</c> when no unwrap was attempted
    /// (no unwrapper configured, PDU isn't sign/seal-protected, or auth
    /// scheme isn't NTLM); returns a non-null status otherwise so the
    /// caller can decorate the projected <see cref="DecodedOpcPdu"/>.
    /// </summary>
    /// <remarks>
    /// Wire-format compatibility: matches this codebase's body-only
    /// signing/sealing region. Additional external-peer packet-protection
    /// layouts remain a compatibility follow-up.
    /// </remarks>
    private (NtlmUnwrapStatus? Status, string? Reason) TryUnwrapInPlace(byte[] frame, FlowState flow)
    {
        if (_unwrapper is null)
        {
            return (null, null);
        }

        if (frame.Length < ConnectionOrientedPdu.HEADER_LENGTH)
        {
            return (null, null);
        }

        byte ptype = frame[2];
        if (ptype != PtypeRequest && ptype != PtypeResponse && ptype != PtypeFault)
        {
            return (null, null);
        }

        ushort fragLength = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2));
        ushort authLength = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, 2));
        if (authLength == 0)
        {
            return (null, null);
        }

        // Auth verifier header is 8 bytes immediately before the auth value.
        const int authVerifierHeaderLength = 8;
        if (fragLength > frame.Length
            || authLength > fragLength - ConnectionOrientedPdu.HEADER_LENGTH - authVerifierHeaderLength)
        {
            // Malformed; surface as InvalidTrailerLength so the operator can
            // see the unwrap path detected the inconsistency.
            return (NtlmUnwrapStatus.InvalidTrailerLength,
                $"Frame too short: frag_length={fragLength}, auth_length={authLength}, frame.Length={frame.Length}.");
        }

        int verifierStart = fragLength - authLength - authVerifierHeaderLength;
        byte authType = frame[verifierStart];
        if (authType != AuthServiceCodeNtlm)
        {
            // Different auth scheme (SPNEGO / Kerberos) — not in scope for
            // this unwrapper. Skip silently so the projection is unannotated.
            return (null, null);
        }

        int authPaddingLength = frame[verifierStart + 2];
        int strippedLength = verifierStart - authPaddingLength;
        if (strippedLength < ConnectionOrientedPdu.HEADER_LENGTH
            || authPaddingLength > verifierStart - ConnectionOrientedPdu.HEADER_LENGTH)
        {
            return (NtlmUnwrapStatus.InvalidTrailerLength,
                $"Invalid auth_pad_length={authPaddingLength} for verifier_start={verifierStart}.");
        }

        if (flow.KnownDirection is not { } direction)
        {
            // The capture started after the bind — counters are unrecoverable.
            return (NtlmUnwrapStatus.SignatureMismatch,
                "Direction unknown for this flow (capture started after the Bind handshake; per-direction NTLM sequence counters cannot be recovered passively).");
        }

        // Cipher region per production receiver's
        // `DcomCallChannel.VerifyPacketProtection`: pduBody starts AFTER the
        // 16-byte common header and runs up to (but excluding) the auth
        // verifier header. The common header MUST stay plaintext on the
        // wire so the receiver (and our own flow buffer) can parse
        // fragLength + authLength + ptype. NTLM HMAC + RC4 cover this
        // body-only region.
        int bodyStart = ConnectionOrientedPdu.HEADER_LENGTH;
        int bodyLength = verifierStart - bodyStart;
        byte[] cipherStub = new byte[bodyLength];
        Array.Copy(frame, bodyStart, cipherStub, 0, bodyLength);

        byte[] authTrailer = new byte[authLength];
        Array.Copy(frame, fragLength - authLength, authTrailer, 0, authLength);

        NtlmUnwrapResult result = _unwrapper.TryUnwrap(direction, cipherStub, authTrailer);
        if (result.Succeeded)
        {
            // Copy plaintext back AND strip the auth verifier header + auth
            // value (mirror the production receiver's StripAuthenticationVerifier):
            // update frag_length to the pre-verifier length and zero auth_length
            // so PduCodec sees a clean PDU body (it expects no auth trailer
            // once the wire layer has handled the protection).
            Array.Copy(cipherStub, 0, frame, bodyStart, bodyLength);
            BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2), (ushort)strippedLength);
            BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, 2), 0);
            // Zero out auth padding + verifier header + auth value so none of
            // those bytes can be decoded as stub data.
            Array.Clear(frame, strippedLength, frame.Length - strippedLength);
        }

        return (result.Status, result.Reason);
    }

    /// <summary>
    /// Test-only seam that feeds a raw DCE/RPC frame (no ethernet / IP
    /// / TCP framing) through the per-flow buffer + decode pipeline.
    /// Used by <c>OpcDcomDecoderTests</c> to exercise the NTLM unwrap
    /// path without standing up synthetic PacketDotNet packets.
    /// </summary>
    /// <param name="frame">Raw DCE/RPC PDU bytes; appended to the flow buffer in order.</param>
    /// <param name="srcIp">Synthetic source IPv4 address (e.g. 127.0.0.1).</param>
    /// <param name="srcPort">Synthetic source TCP port.</param>
    /// <param name="dstIp">Synthetic destination IPv4 address.</param>
    /// <param name="dstPort">Synthetic destination TCP port.</param>
    /// <param name="timestamp">UTC timestamp to attach to projected PDUs.</param>
    internal IEnumerable<DecodedOpcPdu> DecodeRawDcomFrame(
        byte[] frame,
        IPAddress srcIp,
        int srcPort,
        IPAddress dstIp,
        int dstPort,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(srcIp);
        ArgumentNullException.ThrowIfNull(dstIp);

        var key = new FlowKey(srcIp, srcPort, dstIp, dstPort);
        var reverseKey = new FlowKey(dstIp, dstPort, srcIp, srcPort);
        (FlowState flow, FlowState reverseFlow) = GetOrCreateFlowPair(key, reverseKey);

        flow.Append(frame);

        while (flow.TryDequeueFrame(out byte[]? next))
        {
            DecodedDcomFrame decoded = TryDecodeFrame(next, timestamp, flow, reverseFlow, key);
            if (decoded.Pdu is not null)
            {
                yield return decoded.Pdu;
            }
        }
    }

    /// <summary>
    /// Decodes exactly one raw DCE/RPC frame and reports framing or codec
    /// failures instead of buffering/suppressing them.
    /// </summary>
    internal DecodedDcomFrame DecodeRawDcomFrameStrict(
        byte[] frame,
        IPAddress srcIp,
        int srcPort,
        IPAddress dstIp,
        int dstPort,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(srcIp);
        ArgumentNullException.ThrowIfNull(dstIp);

        if (frame.Length == 0)
        {
            return FramingFailure(frame, "empty_frame", "DCE/RPC frame is empty.");
        }
        if (frame.Length < ConnectionOrientedPdu.HEADER_LENGTH)
        {
            return FramingFailure(
                frame,
                "truncated_header",
                $"DCE/RPC frame is {frame.Length} bytes; the common header requires {ConnectionOrientedPdu.HEADER_LENGTH}.");
        }

        int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2));
        if (fragmentLength < ConnectionOrientedPdu.HEADER_LENGTH)
        {
            return FramingFailure(
                frame,
                "invalid_fragment_length",
                $"DCE/RPC frag_length {fragmentLength} is smaller than the {ConnectionOrientedPdu.HEADER_LENGTH}-byte common header.");
        }
        if (fragmentLength != frame.Length)
        {
            string reason = fragmentLength > frame.Length
                ? $"DCE/RPC frame is truncated: frag_length={fragmentLength}, available={frame.Length}."
                : $"DCE/RPC input contains trailing bytes: frag_length={fragmentLength}, available={frame.Length}.";
            return FramingFailure(frame, "fragment_length_mismatch", reason);
        }

        var key = new FlowKey(srcIp, srcPort, dstIp, dstPort);
        var reverseKey = new FlowKey(dstIp, dstPort, srcIp, srcPort);
        (FlowState flow, FlowState reverseFlow) = GetOrCreateFlowPair(key, reverseKey);

        return TryDecodeFrame(frame.ToArray(), timestamp, flow, reverseFlow, key);
    }

    private (FlowState Flow, FlowState ReverseFlow) GetOrCreateFlowPair(FlowKey key, FlowKey reverseKey)
    {
        if (_flows.TryGetValue(key, out FlowState? flow))
        {
            if (!_flows.TryGetValue(reverseKey, out FlowState? reverseFlow))
            {
                reverseFlow = new FlowState(reverseKey, flow.Connection);
                _flows[reverseKey] = reverseFlow;
            }
            return (flow, reverseFlow);
        }

        if (_flows.TryGetValue(reverseKey, out FlowState? existingReverse))
        {
            flow = new FlowState(key, existingReverse.Connection);
            _flows[key] = flow;
            return (flow, existingReverse);
        }

        var connection = new ConnectionState();
        flow = new FlowState(key, connection);
        var reverse = new FlowState(reverseKey, connection);
        _flows[key] = flow;
        _flows[reverseKey] = reverse;
        return (flow, reverse);
    }

    /// <summary>
    /// Reports incomplete DCE/RPC bytes left in flow reassembly buffers after
    /// an offline source has been fully consumed.
    /// </summary>
    internal IEnumerable<DecodedDcomFrame> CompleteDetailed()
    {
        foreach (FlowState flow in _flows.Values)
        {
            byte[] remaining = flow.TakeRemainingBytes();
            if (remaining.Length == 0)
            {
                continue;
            }

            yield return FramingFailure(
                remaining,
                remaining.Length < ConnectionOrientedPdu.HEADER_LENGTH
                    ? "truncated_header"
                    : "truncated_fragment",
                remaining.Length < ConnectionOrientedPdu.HEADER_LENGTH
                    ? $"DCE/RPC stream ended with {remaining.Length} byte(s), before the common header completed."
                    : $"DCE/RPC stream ended before frag_length completed ({remaining.Length} byte(s) available).");
        }
    }

    private static DecodedDcomFrame FramingFailure(byte[] frame, string code, string message)
        => new(
            Pdu: null,
            PduType: GetPduType(frame),
            RawFrame: frame,
            StubBytes: null,
            Failure: new DcomDecodeFailure("framing", code, message));

    private static string GetPduType(ReadOnlySpan<byte> frame)
    {
        if (frame.Length <= ConnectionOrientedPdu.TYPE_OFFSET)
        {
            return "unknown";
        }

        return frame[ConnectionOrientedPdu.TYPE_OFFSET] switch
        {
            RequestCoPdu.REQUEST_TYPE => "request",
            ResponseCoPdu.RESPONSE_TYPE => "response",
            FaultCoPdu.FAULT_TYPE => "fault",
            BindPdu.BIND_TYPE => "bind",
            BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE => "bind_ack",
            BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE => "bind_nak",
            AlterContextPdu.ALTER_CONTEXT_TYPE => "alter_context",
            AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE => "alter_context_resp",
            ShutdownPdu.SHUTDOWN_TYPE => "shutdown",
            Auth3Pdu.AUTH3_TYPE => "auth3",
            CancelCoPdu.CANCEL_TYPE => "cancel",
            OrphanedPdu.ORPHANED_TYPE => "orphaned",
            _ => "unknown",
        };
    }

    private static DecodedOpcPdu DecodeHexSourceRecord(CapturedPacket packet)
    {
        // Hex-source records carry ORPC bodies directly with annotations
        // already containing the iid/opnum/direction/hresult banner. Map
        // those into the structured PDU view so the same summarize +
        // dcom-format tools work uniformly.
        IReadOnlyDictionary<string, string?> ann = packet.Annotations;

        Guid? iid = TryGetGuid(ann, "iid");
        int? opnum = TryGetInt(ann, "opnum");
        int? hresult = TryGetInt32Hex(ann, "hresult");
        string direction = (ann.TryGetValue("direction", out string? dir) ? dir : "request") ?? "request";

        return new DecodedOpcPdu
        {
            Timestamp = packet.Timestamp,
            PduType = "orpc_body",
            CallId = -1,
            InterfaceId = iid,
            Opnum = opnum,
            Hresult = direction == "response" ? hresult : null,
            RequestStubLength = direction == "request" ? packet.Data.Length : null,
            ResponseStubLength = direction == "response" ? packet.Data.Length : null,
            Annotations = ann,
        };
    }

    private static DecodedOpcPdu ProjectBind(PresentationContext[]? contextList, int callId, DateTimeOffset timestamp, FlowKey key, FlowState flow, bool isAlter)
    {
        flow.LastBindContextIds.Clear();
        var contexts = new List<PresentationContextInfo>(contextList?.Length ?? 0);
        if (contextList is not null)
        {
            foreach (PresentationContext c in contextList)
            {
                Guid iid = TryParseGuid(c.AbstractSyntax?.Uuid?.ToString());
                contexts.Add(new PresentationContextInfo(c.ContextId, iid, c.AbstractSyntax?.MajorVersion ?? 0, c.AbstractSyntax?.MinorVersion ?? 0));
                flow.LastBindContextIds.Add(c.ContextId);
                // Eagerly remember the context-id → IID mapping; the bind-ack/alter-
                // ack will trim it down to accepted entries, but having the
                // hopeful mapping lets us resolve calls even when the ack frame
                // was missed from the capture.
                if (iid != Guid.Empty)
                {
                    flow.PendingContexts[c.ContextId] = iid;
                }
            }
        }

        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = isAlter ? "alter_context" : "bind",
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = callId,
            ContextList = contexts,
        };
    }

    private static DecodedOpcPdu ProjectBindAck(PresentationResult[]? resultList, int callId, DateTimeOffset timestamp, FlowKey key, FlowState flow, bool isAlter)
    {
        IReadOnlyList<PresentationResultInfo> results = SnapshotResultList(resultList);
        for (int i = 0; i < results.Count; i++)
        {
            if (string.Equals(results[i].Result, "ACCEPTANCE", StringComparison.Ordinal)
                && i < flow.LastBindContextIds.Count
                && flow.PendingContexts.TryGetValue(flow.LastBindContextIds[i], out Guid iid))
            {
                flow.ConfirmedContexts[flow.LastBindContextIds[i]] = iid;
            }
        }

        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = isAlter ? "alter_context_resp" : "bind_ack",
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = callId,
            ResultList = results,
        };
    }

    private static DecodedOpcPdu ProjectBindNak(BindNoAcknowledgePdu nak, DateTimeOffset timestamp, FlowKey key)
    {
        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = "bind_nak",
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = nak.CallId,
        };
    }

    private static DecodedOpcPdu ProjectRequest(RequestCoPdu request, DateTimeOffset timestamp, FlowKey key, FlowState flow, NtlmUnwrapStatus? authStatus, string? authReason)
    {
        Guid? iid = null;
        if (flow.ConfirmedContexts.TryGetValue(request.ContextId, out Guid confirmed))
        {
            iid = confirmed;
        }
        else if (flow.PendingContexts.TryGetValue(request.ContextId, out Guid pending))
        {
            iid = pending;
        }

        Guid? objectIpid = TryParseGuid(request.Object?.ToString());
        flow.Calls[request.CallId] = new CallCorrelation(iid, request.Opnum, request.ContextId);

        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = "request",
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = request.CallId,
            ContextId = request.ContextId,
            Opnum = request.Opnum,
            InterfaceId = iid,
            ObjectIpid = objectIpid,
            RequestStubLength = request.Stub?.Length ?? 0,
            AuthUnwrapStatus = authStatus?.ToString(),
            AuthUnwrapReason = authReason,
        };
    }

    private static DecodedOpcPdu ProjectResponse(ResponseCoPdu response, DateTimeOffset timestamp, FlowKey key, FlowState flow, NtlmUnwrapStatus? authStatus, string? authReason)
    {
        Guid? iid = null;
        if (flow.ConfirmedContexts.TryGetValue(response.ContextId, out Guid confirmed))
        {
            iid = confirmed;
        }
        int? opnum = null;
        if (flow.Calls.TryGetValue(response.CallId, out CallCorrelation? call))
        {
            iid ??= call.InterfaceId;
            opnum = call.Opnum;
            if (response.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
            {
                flow.Calls.Remove(response.CallId);
            }
        }

        // The HRESULT is the trailing 4 bytes of the ORPC envelope when present;
        // ResponseCoPdu.Stub = ORPCTHAT (header) + body + (optional) HRESULT.
        // OrpcEnvelope.ExtractResponseBody returns the body sans envelope;
        // here we want the raw HRESULT, so peek at the last 4 bytes of the
        // stub when long enough.
        int? hresult = null;
        if (response.Stub is { Length: >= 4 } stub)
        {
            hresult = BinaryPrimitives.ReadInt32LittleEndian(stub.AsSpan(stub.Length - 4, 4));
        }

        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = "response",
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = response.CallId,
            ContextId = response.ContextId,
            Opnum = opnum,
            InterfaceId = iid,
            Hresult = hresult,
            ResponseStubLength = response.Stub?.Length ?? 0,
            AuthUnwrapStatus = authStatus?.ToString(),
            AuthUnwrapReason = authReason,
        };
    }

    private static DecodedOpcPdu ProjectFault(FaultCoPdu fault, DateTimeOffset timestamp, FlowKey key, FlowState flow, NtlmUnwrapStatus? authStatus, string? authReason)
    {
        Guid? iid = null;
        if (flow.ConfirmedContexts.TryGetValue(fault.ContextId, out Guid confirmed))
        {
            iid = confirmed;
        }
        int? opnum = null;
        if (flow.Calls.TryGetValue(fault.CallId, out CallCorrelation? call))
        {
            iid ??= call.InterfaceId;
            opnum = call.Opnum;
            if (fault.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG))
            {
                flow.Calls.Remove(fault.CallId);
            }
        }

        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = "fault",
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = fault.CallId,
            ContextId = fault.ContextId,
            Opnum = opnum,
            InterfaceId = iid,
            FaultStatus = unchecked((int)fault.Status),
            AuthUnwrapStatus = authStatus?.ToString(),
            AuthUnwrapReason = authReason,
        };
    }

    private static DecodedOpcPdu ProjectSimple(string pduType, DateTimeOffset timestamp, FlowKey key)
    {
        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = pduType,
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = -1,
        };
    }

    private static IReadOnlyList<PresentationResultInfo> SnapshotResultList(PresentationResult[]? results)
    {
        if (results is null || results.Length == 0)
        {
            return Array.Empty<PresentationResultInfo>();
        }
        var list = new List<PresentationResultInfo>(results.Length);
        foreach (PresentationResult r in results)
        {
            list.Add(new PresentationResultInfo(
                Result: r.Result.ToString(),
                Reason: r.Reason.ToString()));
        }
        return list;
    }

    private static Guid TryParseGuid(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Guid.Empty;
        }
        return Guid.TryParse(text, out Guid parsed) ? parsed : Guid.Empty;
    }

    private static Guid? TryGetGuid(IReadOnlyDictionary<string, string?> annotations, string key)
    {
        if (annotations.TryGetValue(key, out string? value)
            && !string.IsNullOrWhiteSpace(value)
            && Guid.TryParse(value, out Guid parsed))
        {
            return parsed;
        }
        return null;
    }

    private static int? TryGetInt(IReadOnlyDictionary<string, string?> annotations, string key)
    {
        if (annotations.TryGetValue(key, out string? value)
            && !string.IsNullOrWhiteSpace(value)
            && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed))
        {
            return parsed;
        }
        return null;
    }

    private static int? TryGetInt32Hex(IReadOnlyDictionary<string, string?> annotations, string key)
    {
        if (!annotations.TryGetValue(key, out string? value) || string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        if (trimmed.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[2..];
        }

        if (uint.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint parsed))
        {
            return unchecked((int)parsed);
        }

        return null;
    }

    private static string FormatEndpoint(IPAddress ip, int port) =>
        string.Create(CultureInfo.InvariantCulture, $"{ip}:{port}");

    private static void ValidateCapturedFrame(CapturedPacket packet)
    {
        if (packet.LinkType != (int)LinkLayers.Ethernet)
        {
            return;
        }

        ReadOnlySpan<byte> data = packet.Data.Span;
        RequireFrameLength(data.Length, 14, 0);

        int etherTypeOffset = 12;
        ushort etherType = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(etherTypeOffset, 2));
        int networkOffset = 14;
        if (etherType == 0x8100 || etherType == 0x88A8)
        {
            RequireFrameLength(data.Length, networkOffset + 4, networkOffset);
            etherType = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(networkOffset + 2, 2));
            networkOffset += 4;
        }

        switch (etherType)
        {
            case 0x0800:
                ValidateIpv4Frame(data, networkOffset);
                break;
            case 0x86DD:
                ValidateIpv6Frame(data, networkOffset);
                break;
        }
    }

    private static void ValidateIpv4Frame(ReadOnlySpan<byte> data, int ipOffset)
    {
        RequireFrameLength(data.Length, ipOffset + 20, ipOffset);

        int version = data[ipOffset] >> 4;
        int headerLength = (data[ipOffset] & 0x0F) * 4;
        if (version != 4)
        {
            throw new InvalidDataException($"ethernet frame has invalid IPv4 version {version} at offset {ipOffset}.");
        }

        if (headerLength < 20)
        {
            throw new InvalidDataException($"ethernet frame has invalid IPv4 header length {headerLength} at offset {ipOffset}.");
        }

        RequireFrameLength(data.Length, ipOffset + headerLength, ipOffset);

        int totalLength = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(ipOffset + 2, 2));
        if (totalLength < headerLength)
        {
            throw new InvalidDataException($"ethernet frame has invalid IPv4 total length {totalLength} at offset {ipOffset}.");
        }

        RequireFrameLength(data.Length, ipOffset + totalLength, ipOffset);

        if (data[ipOffset + 9] == 6)
        {
            ValidateTcpSegment(data, ipOffset + headerLength, ipOffset + totalLength);
        }
    }

    private static void ValidateIpv6Frame(ReadOnlySpan<byte> data, int ipOffset)
    {
        RequireFrameLength(data.Length, ipOffset + 40, ipOffset);

        int payloadLength = BinaryPrimitives.ReadUInt16BigEndian(data.Slice(ipOffset + 4, 2));
        int totalLength = 40 + payloadLength;
        RequireFrameLength(data.Length, ipOffset + totalLength, ipOffset);

        if (data[ipOffset + 6] == 6)
        {
            ValidateTcpSegment(data, ipOffset + 40, ipOffset + totalLength);
        }
    }

    private static void ValidateTcpSegment(ReadOnlySpan<byte> data, int tcpOffset, int segmentEnd)
    {
        RequireFrameLength(data.Length, tcpOffset + 20, tcpOffset);
        if (segmentEnd < tcpOffset + 20)
        {
            throw new InvalidDataException($"ethernet frame too short: expected {tcpOffset + 20} bytes, got {segmentEnd} at offset {tcpOffset}.");
        }

        int headerLength = (data[tcpOffset + 12] >> 4) * 4;
        if (headerLength < 20)
        {
            throw new InvalidDataException($"ethernet frame has invalid TCP header length {headerLength} at offset {tcpOffset}.");
        }

        if (segmentEnd < tcpOffset + headerLength)
        {
            throw new InvalidDataException($"ethernet frame too short: expected {tcpOffset + headerLength} bytes, got {segmentEnd} at offset {tcpOffset}.");
        }

        RequireFrameLength(data.Length, tcpOffset + headerLength, tcpOffset);
    }

    private static void RequireFrameLength(int actualLength, int expectedLength, int offset)
    {
        if (actualLength < expectedLength)
        {
            throw new InvalidDataException($"ethernet frame too short: expected {expectedLength} bytes, got {actualLength} at offset {offset}.");
        }
    }

    private sealed record class FlowKey(IPAddress SrcIp, int SrcPort, IPAddress DstIp, int DstPort);

    private sealed record CallCorrelation(Guid? InterfaceId, int Opnum, int ContextId);

    private sealed class ConnectionState
    {
        public Dictionary<int, Guid> PendingContexts { get; } = new();
        public Dictionary<int, Guid> ConfirmedContexts { get; } = new();
        public List<int> LastBindContextIds { get; } = new();
        public Dictionary<int, CallCorrelation> Calls { get; } = new();
    }

    private sealed class FlowState
    {
        public FlowKey Key { get; }
        public ConnectionState Connection { get; }
        public Dictionary<int, Guid> PendingContexts => Connection.PendingContexts;
        public Dictionary<int, Guid> ConfirmedContexts => Connection.ConfirmedContexts;
        public List<int> LastBindContextIds => Connection.LastBindContextIds;
        public Dictionary<int, CallCorrelation> Calls => Connection.Calls;

        /// <summary>
        /// Direction of this flow relative to the DCOM connection
        /// (client→server vs server→client). Set on the first
        /// observed Bind PDU per flow (the side that sends the Bind
        /// is the client; the reverse-direction flow is the server
        /// side). Null when no Bind has been observed yet on this
        /// flow OR its reverse — typically because the capture
        /// started after the connection was already established.
        /// Required by the NTLM passive unwrapper to pick the
        /// correct sub-key + sequence counter.
        /// </summary>
        public NtlmDirection? KnownDirection { get; set; }

        private readonly List<byte> _buffer = new();

        public FlowState(FlowKey key, ConnectionState connection)
        {
            Key = key;
            Connection = connection;
        }

        public void Append(ReadOnlySpan<byte> bytes) => _buffer.AddRange(bytes);

        public bool TryDequeueFrame([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? frame)
        {
            frame = null;
            if (_buffer.Count < ConnectionOrientedPdu.HEADER_LENGTH)
            {
                return false;
            }

            // Common header offset 8 is a little-endian USHORT frag_length.
            int fragLength = _buffer[8] | (_buffer[9] << 8);
            if (fragLength < ConnectionOrientedPdu.HEADER_LENGTH || _buffer.Count < fragLength)
            {
                return false;
            }

            frame = _buffer.GetRange(0, fragLength).ToArray();
            _buffer.RemoveRange(0, fragLength);
            return true;
        }

        public byte[] TakeRemainingBytes()
        {
            if (_buffer.Count == 0)
            {
                return Array.Empty<byte>();
            }

            byte[] remaining = _buffer.ToArray();
            _buffer.Clear();
            return remaining;
        }
    }
}

/// <summary>
/// Internal decode result used by byte-level replay. Raw bytes stay off the
/// public <see cref="DecodedOpcPdu"/> MCP serialization surface.
/// </summary>
internal sealed record DecodedDcomFrame(
    DecodedOpcPdu? Pdu,
    string PduType,
    byte[]? RawFrame,
    byte[]? StubBytes,
    DcomDecodeFailure? Failure);

/// <summary>
/// Structured framing/codec failure retained for replay and ad-hoc decode
/// diagnostics.
/// </summary>
internal sealed record DcomDecodeFailure(string Stage, string Code, string Message);
