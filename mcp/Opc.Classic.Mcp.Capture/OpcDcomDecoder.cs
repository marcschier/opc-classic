// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
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
    private const int MaxCompletedFlowTombstones = 4096;
    private readonly ILogger _logger;
    private readonly NtlmPassiveUnwrapper? _unwrapper;
    private readonly Dictionary<FlowKey, FlowState> _flows = new();
    private readonly Dictionary<FlowKey, CompletedFlowTombstone> _completedFlowTombstones = new();
    private readonly Queue<CompletedFlowTombstone> _completedFlowTombstoneOrder = [];
    private long _nextConnectionGeneration;
    private long _nextTombstoneGeneration;
    private bool _completed;

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
        if (_completed)
        {
            throw new InvalidOperationException("The decoder has already been finalized.");
        }

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

        DecodedDcomFrame? validationFailure = null;
        try
        {
            ValidateCapturedFrame(packet);
        }
        catch (InvalidDataException ex)
        {
            validationFailure = PacketFailure(packet, "invalid_or_truncated_packet", ex.Message);
        }
        if (validationFailure is not null)
        {
            yield return validationFailure;
            yield break;
        }

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

        FlowKey key = new(srcIp, tcp.SourcePort, dstIp, tcp.DestinationPort);
        FlowKey reverseKey = new(dstIp, tcp.DestinationPort, srcIp, tcp.SourcePort);
        if (tcp.Synchronize)
        {
            if (_completedFlowTombstones.TryGetValue(
                    key,
                    out CompletedFlowTombstone? tombstone)
                && tombstone.SynSequence == tcp.SequenceNumber)
            {
                yield break;
            }

            RemoveCompletedFlowTombstone(key);
            RemoveCompletedFlowTombstone(reverseKey);
        }
        else if (_completedFlowTombstones.ContainsKey(key))
        {
            yield break;
        }

        if (tcp.Synchronize
            && _flows.TryGetValue(key, out FlowState? existing)
            && existing.StartsNewGeneration(tcp.SequenceNumber))
        {
            IReadOnlyList<DecodedDcomFrame> completed =
                CompleteConnection(existing.Connection);
            RemoveConnection(existing.Connection);
            foreach (DecodedDcomFrame frame in completed)
            {
                yield return frame;
            }
        }

        (FlowState flow, FlowState reverseFlow) = GetOrCreateFlowPair(key, reverseKey);
        if (flow.Connection.Completed && !tcp.Synchronize)
        {
            yield break;
        }

        byte[] payload = tcp.PayloadData ?? [];
        flow.AppendTcpSegment(
            tcp.SequenceNumber,
            payload,
            packet.Timestamp,
            tcp.Synchronize,
            tcp.Finished,
            tcp.Reset);

        foreach (DecodedDcomFrame decoded in DrainFlow(flow, reverseFlow))
        {
            yield return decoded;
        }

        if (tcp.Reset)
        {
            flow.Connection.ResetObserved = true;
            IReadOnlyList<DecodedDcomFrame> completed =
                CompleteConnection(flow.Connection);
            EvictCompletedConnection(flow.Connection);
            foreach (DecodedDcomFrame frame in completed)
            {
                yield return frame;
            }
        }
        else if (flow.FinObserved
            && reverseFlow.FinObserved
            && flow.IsCompleteThroughFin
            && reverseFlow.IsCompleteThroughFin)
        {
            IReadOnlyList<DecodedDcomFrame> completed =
                CompleteConnection(flow.Connection);
            EvictCompletedConnection(flow.Connection);
            foreach (DecodedDcomFrame frame in completed)
            {
                yield return frame;
            }
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
        ProtectionLevel protection = (ProtectionLevel)frame[verifierStart + 1];
        if (protection is not (ProtectionLevel.PROTECTION_LEVEL_INTEGRITY
            or ProtectionLevel.PROTECTION_LEVEL_PRIVACY))
        {
            return (NtlmUnwrapStatus.InvalidTrailerLength,
                $"Unsupported NTLM auth level {(byte)protection}; expected packet integrity (5) or privacy (6).");
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
        if (!flow.ProtectionSequenceReliable)
        {
            return (NtlmUnwrapStatus.SignatureMismatch,
                "TCP reassembly encountered a gap or resynchronization; NTLM counters were intentionally left unchanged because protected PDUs may be missing.");
        }

        // MS-RPCE signs the complete PDU through the sec_trailer, but packet
        // privacy leaves the common and PDU-specific fixed headers clear.
        // Confidentiality begins at the stub and includes auth padding.
        int signedLength = verifierStart + authVerifierHeaderLength;
        int confidentialOffset = GetConfidentialOffset(frame);
        if (confidentialOffset > verifierStart)
        {
            return (NtlmUnwrapStatus.InvalidTrailerLength,
                $"PDU fixed header ends at {confidentialOffset}, after verifier_start={verifierStart}.");
        }
        int confidentialLength = verifierStart - confidentialOffset;
        Span<byte> signedRegion = frame.AsSpan(0, signedLength);
        ReadOnlySpan<byte> authTrailer = frame.AsSpan(fragLength - authLength, authLength);

        NtlmUnwrapResult result = _unwrapper.TryUnwrap(
            direction,
            signedRegion,
            confidentialOffset,
            confidentialLength,
            authTrailer,
            protection);
        if (result.Succeeded)
        {
            // Copy plaintext back AND strip the auth verifier header + auth
            // value (mirror the production receiver's StripAuthenticationVerifier):
            // update frag_length to the pre-verifier length and zero auth_length
            // so PduCodec sees a clean PDU body (it expects no auth trailer
            // once the wire layer has handled the protection).
            BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2), (ushort)strippedLength);
            BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, 2), 0);
            // Zero out auth padding + verifier header + auth value so none of
            // those bytes can be decoded as stub data.
            Array.Clear(frame, strippedLength, frame.Length - strippedLength);
        }

        return (result.Status, result.Reason);
    }

    private static int GetConfidentialOffset(ReadOnlySpan<byte> frame) =>
        frame[ConnectionOrientedPdu.TYPE_OFFSET] switch
        {
            PtypeRequest => ConnectionOrientedPdu.HEADER_LENGTH
                + 8
                + ((frame[ConnectionOrientedPdu.FLAGS_OFFSET]
                    & ConnectionOrientedPdu.PFC_OBJECT_UUID) != 0 ? 16 : 0),
            PtypeResponse => ConnectionOrientedPdu.HEADER_LENGTH + 8,
            PtypeFault => ConnectionOrientedPdu.HEADER_LENGTH + 16,
            _ => throw new InvalidOperationException(
                "Packet protection is only valid for request, response, and fault PDUs."),
        };

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
        DateTimeOffset timestamp,
        NtlmDirection? assumedDirection = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(srcIp);
        ArgumentNullException.ThrowIfNull(dstIp);
        if (_completed)
        {
            throw new InvalidOperationException("The decoder has already been finalized.");
        }

        var key = new FlowKey(srcIp, srcPort, dstIp, dstPort);
        var reverseKey = new FlowKey(dstIp, dstPort, srcIp, srcPort);
        (FlowState flow, FlowState reverseFlow) = GetOrCreateFlowPair(key, reverseKey);
        if (assumedDirection is NtlmDirection direction)
        {
            flow.KnownDirection ??= direction;
            reverseFlow.KnownDirection ??= direction == NtlmDirection.ClientToServer
                ? NtlmDirection.ServerToClient
                : NtlmDirection.ClientToServer;
        }

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
        DateTimeOffset timestamp,
        NtlmDirection? assumedDirection = null)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(srcIp);
        ArgumentNullException.ThrowIfNull(dstIp);
        if (_completed)
        {
            throw new InvalidOperationException("The decoder has already been finalized.");
        }

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
        if (assumedDirection is NtlmDirection direction)
        {
            flow.KnownDirection ??= direction;
            reverseFlow.KnownDirection ??= direction == NtlmDirection.ClientToServer
                ? NtlmDirection.ServerToClient
                : NtlmDirection.ClientToServer;
        }

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

        var connection = new ConnectionState(++_nextConnectionGeneration);
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
        if (_completed)
        {
            return [];
        }
        _completed = true;

        var output = new List<DecodedDcomFrame>();
        var connections = new HashSet<ConnectionState>(ReferenceEqualityComparer.Instance);
        foreach (FlowState flow in _flows.Values)
        {
            connections.Add(flow.Connection);
        }
        foreach (ConnectionState connection in connections)
        {
            output.AddRange(CompleteConnection(connection));
        }
        _flows.Clear();
        _completedFlowTombstones.Clear();
        _completedFlowTombstoneOrder.Clear();
        return output;
    }

    private IReadOnlyList<DecodedDcomFrame> DrainFlow(
        FlowState flow,
        FlowState reverseFlow)
    {
        var output = new List<DecodedDcomFrame>();
        foreach (DcomDecodeFailure failure in flow.TakeFailures())
        {
            output.Add(new DecodedDcomFrame(
                Pdu: null,
                PduType: "unknown",
                RawFrame: null,
                StubBytes: null,
                Failure: failure));
        }

        while (flow.TryDequeueFrame(out byte[]? frame))
        {
            output.Add(TryDecodeFrame(
                frame,
                flow.LastTimestamp,
                flow,
                reverseFlow,
                flow.Key));
        }
        foreach (DcomDecodeFailure failure in flow.TakeFailures())
        {
            output.Add(new DecodedDcomFrame(
                Pdu: null,
                PduType: "unknown",
                RawFrame: null,
                StubBytes: null,
                Failure: failure));
        }
        return output;
    }

    private IReadOnlyList<DecodedDcomFrame> CompleteFlow(
        FlowState flow,
        FlowState reverseFlow)
    {
        flow.CompleteTcpReassembly();
        var output = new List<DecodedDcomFrame>(DrainFlow(flow, reverseFlow));
        byte[] remaining = flow.TakeRemainingBytes();
        if (remaining.Length > 0)
        {
            output.Add(FramingFailure(
                remaining,
                remaining.Length < ConnectionOrientedPdu.HEADER_LENGTH
                    ? "truncated_header"
                    : "truncated_fragment",
                remaining.Length < ConnectionOrientedPdu.HEADER_LENGTH
                    ? $"DCE/RPC stream ended with {remaining.Length} byte(s), before the common header completed."
                    : $"DCE/RPC stream ended before frag_length completed ({remaining.Length} byte(s) available)."));
        }
        return output;
    }

    private IReadOnlyList<DecodedDcomFrame> CompleteConnection(ConnectionState connection)
    {
        if (connection.CompletionDrained)
        {
            return [];
        }

        connection.Completed = true;
        connection.CompletionDrained = true;
        FlowState[] flows = _flows.Values
            .Where(flow => ReferenceEquals(flow.Connection, connection))
            .ToArray();
        var output = new List<DecodedDcomFrame>();
        foreach (FlowState flow in flows)
        {
            var reverseKey = new FlowKey(
                flow.Key.DstIp,
                flow.Key.DstPort,
                flow.Key.SrcIp,
                flow.Key.SrcPort);
            FlowState reverseFlow = _flows.TryGetValue(reverseKey, out FlowState? reverse)
                && ReferenceEquals(reverse.Connection, connection)
                ? reverse
                : flow;
            output.AddRange(CompleteFlow(flow, reverseFlow));
        }
        return output;
    }

    private void RemoveConnection(ConnectionState connection)
    {
        FlowKey[] keys = _flows
            .Where(pair => ReferenceEquals(pair.Value.Connection, connection))
            .Select(pair => pair.Key)
            .ToArray();
        foreach (FlowKey key in keys)
        {
            _flows.Remove(key);
        }
    }

    private void EvictCompletedConnection(ConnectionState connection)
    {
        FlowState[] flows = _flows.Values
            .Where(flow => ReferenceEquals(flow.Connection, connection))
            .ToArray();
        RemoveConnection(connection);
        foreach (FlowState flow in flows)
        {
            AddCompletedFlowTombstone(flow.Key, flow.SynSequence);
        }
    }

    private void AddCompletedFlowTombstone(
        FlowKey key,
        uint? synSequence)
    {
        long generation = ++_nextTombstoneGeneration;
        var tombstone = new CompletedFlowTombstone(
            key,
            generation,
            synSequence);
        _completedFlowTombstones[key] = tombstone;
        _completedFlowTombstoneOrder.Enqueue(tombstone);
        while (_completedFlowTombstoneOrder.Count > MaxCompletedFlowTombstones)
        {
            CompletedFlowTombstone expired =
                _completedFlowTombstoneOrder.Dequeue();
            if (_completedFlowTombstones.TryGetValue(
                    expired.Key,
                    out CompletedFlowTombstone? current)
                && current.Generation == expired.Generation)
            {
                _completedFlowTombstones.Remove(expired.Key);
            }
        }
    }

    private void RemoveCompletedFlowTombstone(FlowKey key)
    {
        _completedFlowTombstones.Remove(key);
    }

    internal int TrackedFlowCount => _flows.Count;

    internal int CompletedFlowTombstoneCount => _completedFlowTombstones.Count;

    private sealed record CompletedFlowTombstone(
        FlowKey Key,
        long Generation,
        uint? SynSequence);

    private static DecodedDcomFrame FramingFailure(byte[] frame, string code, string message)
        => new(
            Pdu: null,
            PduType: GetPduType(frame),
            RawFrame: frame,
            StubBytes: null,
            Failure: new DcomDecodeFailure("framing", code, message));

    private static DecodedDcomFrame PacketFailure(CapturedPacket packet, string code, string message)
    {
        const int maxContextBytes = 64;
        byte[] context = packet.Data.Span[..Math.Min(packet.Data.Length, maxContextBytes)].ToArray();
        return new DecodedDcomFrame(
            Pdu: null,
            PduType: "unknown",
            RawFrame: context,
            StubBytes: null,
            Failure: new DcomDecodeFailure("packet", code, message));
    }

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
        public ConnectionState(long generation) => Generation = generation;

        public long Generation { get; }
        public Dictionary<int, Guid> PendingContexts { get; } = new();
        public Dictionary<int, Guid> ConfirmedContexts { get; } = new();
        public List<int> LastBindContextIds { get; } = new();
        public Dictionary<int, CallCorrelation> Calls { get; } = new();
        public bool SynObserved { get; set; }
        public bool ApplicationDataObserved { get; set; }
        public bool FinObserved { get; set; }
        public bool ResetObserved { get; set; }
        public bool Completed { get; set; }
        public bool CompletionDrained { get; set; }
    }

    private sealed class FlowState
    {
        private const int MaxPendingTcpSegments = 4096;
        private const int MaxResyncScanBytes = 4096;
        private const long TcpSequenceSpace = 1L << 32;
        private const long TcpHalfSequenceSpace = 1L << 31;

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
        public bool ProtectionSequenceReliable { get; private set; } = true;
        public DateTimeOffset LastTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;
        public bool FinObserved { get; private set; }
        public bool IsCompleteThroughFin =>
            FinObserved
            && _finalDataSequence is long finalDataSequence
            && (_nextSequence is long nextSequence
                ? nextSequence >= finalDataSequence
                : !_applicationDataObserved && _pendingSegments.Count == 0);
        public uint? SynSequence => _synSequence;

        private readonly List<byte> _buffer = new();
        private readonly SortedDictionary<long, byte[]> _pendingSegments = new();
        private readonly Queue<DcomDecodeFailure> _failures = new();
        private long? _nextSequence;
        private long? _sequenceReference;
        private long? _finalDataSequence;
        private uint? _synSequence;
        private bool _sequenceMode;
        private bool _finalized;
        private bool _applicationDataObserved;

        public FlowState(FlowKey key, ConnectionState connection)
        {
            Key = key;
            Connection = connection;
        }

        public bool StartsNewGeneration(uint synSequence)
        {
            if (Connection.Completed || Connection.ResetObserved || Connection.FinObserved)
            {
                return true;
            }
            if (_synSequence is uint observed)
            {
                return observed != synSequence;
            }
            return !Connection.SynObserved && Connection.ApplicationDataObserved;
        }

        public void Append(ReadOnlySpan<byte> bytes)
        {
            if (_finalized)
            {
                throw new InvalidOperationException("Cannot append bytes after TCP flow finalization.");
            }
            _buffer.AddRange(bytes);
        }

        public void AppendTcpSegment(
            uint sequenceNumber,
            ReadOnlySpan<byte> bytes,
            DateTimeOffset timestamp,
            bool syn,
            bool fin,
            bool reset)
        {
            LastTimestamp = timestamp;
            if (_finalized)
            {
                return;
            }

            // Preserve the existing synthetic-packet behavior where sequence
            // zero means "append in arrival order" until a real sequence or
            // TCP lifecycle flag is observed.
            if (sequenceNumber == 0 && !_sequenceMode && !syn && !fin && !reset)
            {
                if (!bytes.IsEmpty)
                {
                    Connection.ApplicationDataObserved = true;
                    _applicationDataObserved = true;
                    Append(bytes);
                }
                return;
            }

            _sequenceMode = true;
            long sequence = UnwrapSequence(sequenceNumber);
            if (syn)
            {
                _synSequence ??= sequenceNumber;
                Connection.SynObserved = true;
                sequence = checked(sequence + 1);
                if (_nextSequence is null
                    && _pendingSegments.Count == 0
                    && _buffer.Count == 0)
                {
                    _nextSequence = sequence;
                }
            }

            if (!bytes.IsEmpty)
            {
                Connection.ApplicationDataObserved = true;
                _applicationDataObserved = true;
                AddPendingSegment(sequence, bytes);
            }

            long dataEnd = checked(sequence + bytes.Length);
            long observedEnd = checked(dataEnd + (fin ? 1 : 0));
            if (_sequenceReference is null || observedEnd > _sequenceReference)
            {
                _sequenceReference = observedEnd;
            }

            if (fin)
            {
                FinObserved = true;
                Connection.FinObserved = true;
                _finalDataSequence = _finalDataSequence is long existingEnd
                    ? Math.Max(existingEnd, dataEnd)
                    : dataEnd;
            }
            if (reset)
            {
                Connection.ResetObserved = true;
            }

            if (bytes.IsEmpty)
            {
                return;
            }
            if (_nextSequence is null)
            {
                TryStartOrderedStream();
            }
            else
            {
                DrainContiguousSegments();
            }
        }

        public void CompleteTcpReassembly()
        {
            if (_finalized)
            {
                return;
            }
            _finalized = true;
            if (!_sequenceMode)
            {
                return;
            }

            while (_pendingSegments.Count > 0)
            {
                if (_nextSequence is null)
                {
                    ResyncAtNextPlausibleHeader(
                        "capture ended before the initial TCP sequence could be established");
                    continue;
                }

                int before = _pendingSegments.Count;
                DrainContiguousSegments();
                RemoveConsumedSegments(_nextSequence.Value);
                if (_pendingSegments.Count < before)
                {
                    continue;
                }

                long nextAvailable = _pendingSegments.First().Key;
                long expected = _nextSequence.Value;
                if (nextAvailable > expected)
                {
                    RecordGapAndReset(expected, nextAvailable);
                }
                else
                {
                    DiscardPendingBefore(checked(expected + 1));
                }
                ResyncAtNextPlausibleHeader("resynchronized after a TCP sequence gap");
            }

            if (_nextSequence is long finalExpected
                && _finalDataSequence is long finalData
                && finalData > finalExpected)
            {
                RecordGapAndReset(finalExpected, finalData);
            }
        }

        public IReadOnlyList<DcomDecodeFailure> TakeFailures()
        {
            if (_failures.Count == 0)
            {
                return [];
            }
            var failures = new List<DcomDecodeFailure>(_failures.Count);
            while (_failures.TryDequeue(out DcomDecodeFailure? failure))
            {
                failures.Add(failure);
            }
            return failures;
        }

        public bool TryDequeueFrame([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out byte[]? frame)
        {
            while (true)
            {
                frame = null;
                if (_buffer.Count < ConnectionOrientedPdu.HEADER_LENGTH)
                {
                    return false;
                }

                if (!IsPlausibleDceHeader(CollectionsMarshal.AsSpan(_buffer)))
                {
                    int resyncOffset = FindPlausibleHeader(_buffer, startOffset: 1);
                    int dropped = resyncOffset >= 0
                        ? resyncOffset
                        : Math.Min(
                            MaxResyncScanBytes,
                            Math.Max(1, _buffer.Count - (ConnectionOrientedPdu.HEADER_LENGTH - 1)));
                    byte[] context = _buffer.Take(Math.Min(dropped, 64)).ToArray();
                    _buffer.RemoveRange(0, dropped);
                    ProtectionSequenceReliable = false;
                    _failures.Enqueue(new DcomDecodeFailure(
                        "tcp_reassembly",
                        "invalid_dce_header_resync",
                        $"Discarded {dropped} byte(s) while searching for the next plausible DCE/RPC header. Context={Convert.ToHexString(context)}"));
                    continue;
                }

                int fragLength = _buffer[8] | (_buffer[9] << 8);
                if (_buffer.Count < fragLength)
                {
                    return false;
                }

                frame = _buffer.GetRange(0, fragLength).ToArray();
                _buffer.RemoveRange(0, fragLength);
                return true;
            }
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

        private void AddPendingSegment(long sequence, ReadOnlySpan<byte> bytes)
        {
            byte[] incoming = bytes.ToArray();
            long incomingEnd = checked(sequence + incoming.Length);
            if (_nextSequence is long expected && incomingEnd <= expected)
            {
                return;
            }

            if (_pendingSegments.TryGetValue(sequence, out byte[]? existing))
            {
                if (existing.Length >= incoming.Length)
                {
                    return;
                }
                existing.CopyTo(incoming, 0);
            }
            _pendingSegments[sequence] = incoming;
            if (_pendingSegments.Count <= MaxPendingTcpSegments)
            {
                return;
            }

            KeyValuePair<long, byte[]> farthest = _pendingSegments.Last();
            _pendingSegments.Remove(farthest.Key);
            ProtectionSequenceReliable = false;
            _failures.Enqueue(new DcomDecodeFailure(
                "tcp_reassembly",
                "pending_segment_overflow",
                $"TCP flow {Key} generation {Connection.Generation} exceeded "
                    + $"{MaxPendingTcpSegments} pending segments; dropped sequence {farthest.Key}."));
        }

        private long UnwrapSequence(uint sequenceNumber)
        {
            if (_sequenceReference is not long reference)
            {
                _sequenceReference = sequenceNumber;
                return sequenceNumber;
            }

            long candidate = (reference & ~0xFFFF_FFFFL) + sequenceNumber;
            if (candidate - reference > TcpHalfSequenceSpace)
            {
                candidate -= TcpSequenceSpace;
            }
            else if (reference - candidate > TcpHalfSequenceSpace)
            {
                candidate += TcpSequenceSpace;
            }
            return candidate;
        }

        private void TryStartOrderedStream()
        {
            if (_pendingSegments.Count == 0)
            {
                return;
            }
            long firstSequence = _pendingSegments.First().Key;
            byte[] contiguous = BuildContiguousPreview(
                firstSequence,
                ConnectionOrientedPdu.HEADER_LENGTH);
            if (contiguous.Length < ConnectionOrientedPdu.HEADER_LENGTH
                || !IsPlausibleDceHeader(contiguous))
            {
                return;
            }

            _nextSequence = firstSequence;
            DrainContiguousSegments();
        }

        private void DrainContiguousSegments()
        {
            if (_nextSequence is not long expected)
            {
                return;
            }

            while (TryFindCoveringSegment(expected, out long start, out byte[]? segment))
            {
                int offset = checked((int)(expected - start));
                ReadOnlySpan<byte> newBytes = segment.AsSpan(offset);
                _buffer.AddRange(newBytes);
                expected = checked(expected + newBytes.Length);
                RemoveConsumedSegments(expected);
            }
            _nextSequence = expected;
        }

        private bool TryFindCoveringSegment(long expected, out long start, out byte[]? segment)
        {
            start = 0;
            segment = null;
            foreach (KeyValuePair<long, byte[]> candidate in _pendingSegments)
            {
                if (candidate.Key > expected)
                {
                    break;
                }
                long end = checked(candidate.Key + candidate.Value.Length);
                if (end > expected
                    && (segment is null || end > checked(start + segment.Length)))
                {
                    start = candidate.Key;
                    segment = candidate.Value;
                }
            }
            return segment is not null;
        }

        private void RemoveConsumedSegments(long expected)
        {
            long[] consumed = _pendingSegments
                .Where(pair => checked(pair.Key + pair.Value.Length) <= expected)
                .Select(pair => pair.Key)
                .ToArray();
            foreach (long key in consumed)
            {
                _pendingSegments.Remove(key);
            }
        }

        private void DiscardPendingBefore(long position)
        {
            byte[]? longestSuffix = null;
            long[] preceding = _pendingSegments.Keys
                .TakeWhile(key => key < position)
                .ToArray();
            foreach (long key in preceding)
            {
                byte[] segment = _pendingSegments[key];
                _pendingSegments.Remove(key);
                long end = checked(key + segment.Length);
                if (end <= position)
                {
                    continue;
                }
                byte[] suffix = segment.AsSpan(checked((int)(position - key))).ToArray();
                if (longestSuffix is null || suffix.Length > longestSuffix.Length)
                {
                    longestSuffix = suffix;
                }
            }

            if (longestSuffix is null)
            {
                return;
            }
            if (_pendingSegments.TryGetValue(position, out byte[]? existing))
            {
                if (existing.Length >= longestSuffix.Length)
                {
                    return;
                }
                existing.CopyTo(longestSuffix, 0);
            }
            _pendingSegments[position] = longestSuffix;
        }

        private byte[] BuildContiguousPreview(long firstSequence, int maximumBytes)
        {
            var preview = new List<byte>(maximumBytes);
            long expected = firstSequence;
            while (preview.Count < maximumBytes
                && TryFindCoveringSegment(expected, out long start, out byte[]? segment))
            {
                int offset = checked((int)(expected - start));
                ReadOnlySpan<byte> bytes = segment.AsSpan(offset);
                int take = Math.Min(bytes.Length, maximumBytes - preview.Count);
                preview.AddRange(bytes[..take]);
                expected = checked(expected + take);
                if (take < bytes.Length)
                {
                    break;
                }
            }
            return preview.ToArray();
        }

        private void RecordGapAndReset(long expected, long nextAvailable)
        {
            if (nextAvailable <= expected)
            {
                return;
            }
            ProtectionSequenceReliable = false;
            _failures.Enqueue(new DcomDecodeFailure(
                "tcp_reassembly",
                "tcp_sequence_gap",
                $"TCP flow {Key} generation {Connection.Generation} is missing unwrapped "
                    + $"sequence range [{expected}, {nextAvailable - 1}]. "
                    + "NTLM unwrap is disabled for the resynchronized flow."));
            if (_buffer.Count > 0)
            {
                _failures.Enqueue(new DcomDecodeFailure(
                    "tcp_reassembly",
                    "partial_dce_frame_dropped",
                    $"Dropped {_buffer.Count} buffered DCE/RPC byte(s) because a TCP gap interrupted the frame."));
                _buffer.Clear();
            }
            _nextSequence = null;
        }

        private void ResyncAtNextPlausibleHeader(string reason)
        {
            if (_pendingSegments.Count == 0)
            {
                return;
            }

            long firstSequence = _pendingSegments.First().Key;
            byte[] contiguous = BuildContiguousPreview(
                firstSequence,
                MaxResyncScanBytes + ConnectionOrientedPdu.HEADER_LENGTH - 1);
            int offset = FindPlausibleHeader(contiguous, startOffset: 0);
            if (offset >= 0)
            {
                if (offset > 0)
                {
                    ProtectionSequenceReliable = false;
                    _failures.Enqueue(new DcomDecodeFailure(
                        "tcp_reassembly",
                        "tcp_resynchronized",
                        $"Dropped {offset} byte(s) at unwrapped TCP sequence {firstSequence} "
                            + $"and resumed at a plausible DCE/RPC header ({reason})."));
                }
                long resynchronized = checked(firstSequence + offset);
                DiscardPendingBefore(resynchronized);
                _nextSequence = resynchronized;
                DrainContiguousSegments();
                return;
            }

            int dropped = Math.Min(contiguous.Length, MaxResyncScanBytes);
            if (dropped == 0)
            {
                _pendingSegments.Remove(firstSequence);
                return;
            }
            DiscardPendingBefore(checked(firstSequence + dropped));
            ProtectionSequenceReliable = false;
            _failures.Enqueue(new DcomDecodeFailure(
                "tcp_reassembly",
                "tcp_resync_drop",
                $"Dropped {dropped} byte(s) at unwrapped TCP sequence {firstSequence}; "
                    + $"no DCE/RPC header was found ({reason})."));
        }

        private static int FindPlausibleHeader(IReadOnlyList<byte> bytes, int startOffset)
        {
            int last = Math.Min(
                bytes.Count - ConnectionOrientedPdu.HEADER_LENGTH,
                MaxResyncScanBytes);
            Span<byte> header = stackalloc byte[ConnectionOrientedPdu.HEADER_LENGTH];
            for (int offset = startOffset; offset <= last; offset++)
            {
                for (int i = 0; i < header.Length; i++)
                {
                    header[i] = bytes[offset + i];
                }
                if (IsPlausibleDceHeader(header))
                {
                    return offset;
                }
            }
            return -1;
        }

        private static bool IsPlausibleDceHeader(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length < ConnectionOrientedPdu.HEADER_LENGTH
                || bytes[0] != 5
                || bytes[1] > 1
                || bytes[2] > 19)
            {
                return false;
            }
            int fragLength = bytes[8] | (bytes[9] << 8);
            return fragLength >= ConnectionOrientedPdu.HEADER_LENGTH;
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
