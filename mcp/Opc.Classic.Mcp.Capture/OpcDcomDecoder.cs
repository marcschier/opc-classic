//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private readonly Dictionary<FlowKey, FlowState> _flows = new();

    public OpcDcomDecoder(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Decode a single captured packet into zero, one, or more
    /// <see cref="DecodedOpcPdu"/> records (one TCP segment may
    /// complete multiple PDUs when small; large PDUs require multiple
    /// segments).
    /// </summary>
    public IEnumerable<DecodedOpcPdu> Decode(CapturedPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet);

        if (packet.LinkType == 0)
        {
            yield return DecodeHexSourceRecord(packet);
            yield break;
        }

        if (packet.Data.IsEmpty)
        {
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

        byte[] payload = tcp.PayloadData;
        if (payload is null || payload.Length == 0)
        {
            yield break;
        }

        FlowKey key = new(srcIp, tcp.SourcePort, dstIp, tcp.DestinationPort);
        if (!_flows.TryGetValue(key, out FlowState? flow))
        {
            flow = new FlowState(key);
            _flows[key] = flow;
        }

        flow.Append(payload);

        while (flow.TryDequeueFrame(out byte[]? frame))
        {
            DecodedOpcPdu? decoded = TryDecodeFrame(frame, packet.Timestamp, flow, key);
            if (decoded is not null)
            {
                yield return decoded;
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

    private DecodedOpcPdu? TryDecodeFrame(byte[] frame, DateTimeOffset timestamp, FlowState flow, FlowKey key)
    {
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
            return null;
        }

        return pdu switch
        {
            BindPdu bind => ProjectBind(bind.ContextList, bind.CallId, timestamp, key, flow, isAlter: false),
            AlterContextPdu alter => ProjectBind(alter.ContextList, alter.CallId, timestamp, key, flow, isAlter: true),
            BindAcknowledgePdu ack => ProjectBindAck(ack.ResultList, ack.CallId, timestamp, key, flow, isAlter: false),
            AlterContextResponsePdu ack => ProjectBindAck(ack.ResultList, ack.CallId, timestamp, key, flow, isAlter: true),
            BindNoAcknowledgePdu nak => ProjectBindNak(nak, timestamp, key),
            RequestCoPdu request => ProjectRequest(request, timestamp, key, flow),
            ResponseCoPdu response => ProjectResponse(response, timestamp, key, flow),
            FaultCoPdu fault => ProjectFault(fault, timestamp, key, flow),
            ShutdownPdu => ProjectSimple("shutdown", timestamp, key),
            Auth3Pdu => ProjectSimple("auth3", timestamp, key),
            CancelCoPdu => ProjectSimple("cancel", timestamp, key),
            OrphanedPdu => ProjectSimple("orphaned", timestamp, key),
            _ => null,
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

    private static DecodedOpcPdu ProjectRequest(RequestCoPdu request, DateTimeOffset timestamp, FlowKey key, FlowState flow)
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
        };
    }

    private static DecodedOpcPdu ProjectResponse(ResponseCoPdu response, DateTimeOffset timestamp, FlowKey key, FlowState flow)
    {
        Guid? iid = null;
        if (flow.ConfirmedContexts.TryGetValue(response.ContextId, out Guid confirmed))
        {
            iid = confirmed;
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
            InterfaceId = iid,
            Hresult = hresult,
            ResponseStubLength = response.Stub?.Length ?? 0,
        };
    }

    private static DecodedOpcPdu ProjectFault(FaultCoPdu fault, DateTimeOffset timestamp, FlowKey key, FlowState flow)
    {
        Guid? iid = null;
        if (flow.ConfirmedContexts.TryGetValue(fault.ContextId, out Guid confirmed))
        {
            iid = confirmed;
        }

        return new DecodedOpcPdu
        {
            Timestamp = timestamp,
            PduType = "fault",
            SourceEndpoint = FormatEndpoint(key.SrcIp, key.SrcPort),
            DestinationEndpoint = FormatEndpoint(key.DstIp, key.DstPort),
            CallId = fault.CallId,
            ContextId = fault.ContextId,
            InterfaceId = iid,
            FaultStatus = unchecked((int)fault.Status),
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

    private sealed record class FlowKey(IPAddress SrcIp, int SrcPort, IPAddress DstIp, int DstPort);

    private sealed class FlowState
    {
        public FlowKey Key { get; }
        public Dictionary<int, Guid> PendingContexts { get; } = new();
        public Dictionary<int, Guid> ConfirmedContexts { get; } = new();
        public List<int> LastBindContextIds { get; } = new();

        private readonly List<byte> _buffer = new();

        public FlowState(FlowKey key) => Key = key;

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
    }
}
