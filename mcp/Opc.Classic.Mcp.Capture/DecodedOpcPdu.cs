//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// One decoded DCE/RPC connection-oriented PDU surfaced by
/// <see cref="OpcDcomDecoder"/>. Suitable for JSON / CSV / text /
/// `dcom`-format MCP-tool output and for the
/// <c>opcclassic.capture.summarize</c> roll-ups (top IIDs, top opnums,
/// top fault codes, top IPIDs).
/// </summary>
public sealed record class DecodedOpcPdu
{
    /// <summary>
    /// UTC timestamp of the frame that completed this PDU.
    /// </summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// PDU type tag: <c>"request"</c>, <c>"response"</c>, <c>"fault"</c>,
    /// <c>"bind"</c>, <c>"bind_ack"</c>, <c>"bind_nak"</c>,
    /// <c>"alter_context"</c>, <c>"alter_context_resp"</c>,
    /// <c>"shutdown"</c>, <c>"auth3"</c>, <c>"cancel"</c>,
    /// <c>"orphaned"</c>, or <c>"orpc_body"</c> for hex-source records.
    /// </summary>
    public required string PduType { get; init; }

    /// <summary>
    /// Source TCP endpoint <c>host:port</c>; null for hex-source records.
    /// </summary>
    public string? SourceEndpoint { get; init; }

    /// <summary>
    /// Destination TCP endpoint <c>host:port</c>; null for hex-source records.
    /// </summary>
    public string? DestinationEndpoint { get; init; }

    /// <summary>
    /// DCE/RPC <c>call_id</c> common-header field; -1 when not available.
    /// </summary>
    public int CallId { get; init; } = -1;

    /// <summary>
    /// Presentation context id (for request/response/fault). When the
    /// flow has seen a matching bind, the decoder resolves
    /// <see cref="InterfaceId"/> from this.
    /// </summary>
    public int? ContextId { get; init; }

    /// <summary>
    /// Operation number (request).
    /// </summary>
    public int? Opnum { get; init; }

    /// <summary>
    /// Resolved interface IID for request/response/fault, looked up
    /// against the per-connection context table established by the
    /// matching bind/alter PDUs. Null when the flow's bind was not
    /// captured.
    /// </summary>
    public Guid? InterfaceId { get; init; }

    /// <summary>
    /// Object IPID from RequestCoPdu (PFC_OBJECT_UUID); null for non-request PDUs and root-routed requests.
    /// </summary>
    public Guid? ObjectIpid { get; init; }

    /// <summary>
    /// HRESULT extracted from a response PDU's ORPC envelope; null for non-response PDUs or when the envelope is too short to carry one.
    /// </summary>
    public int? Hresult { get; init; }

    /// <summary>
    /// RPC fault status code (FaultCoPdu); null for non-fault PDUs.
    /// </summary>
    public int? FaultStatus { get; init; }

    /// <summary>
    /// Request stub length in bytes (request); null for non-request PDUs.
    /// </summary>
    public int? RequestStubLength { get; init; }

    /// <summary>
    /// Response stub length in bytes (response); null for non-response PDUs.
    /// </summary>
    public int? ResponseStubLength { get; init; }

    /// <summary>
    /// Bind / alter context presentation-context list; empty for other PDUs.
    /// </summary>
    public IReadOnlyList<PresentationContextInfo> ContextList { get; init; } = Array.Empty<PresentationContextInfo>();

    /// <summary>
    /// Bind-ack / alter-context-response presentation result list; empty for other PDUs.
    /// </summary>
    public IReadOnlyList<PresentationResultInfo> ResultList { get; init; } = Array.Empty<PresentationResultInfo>();

    /// <summary>
    /// NTLM auth-trailer unwrap status when an
    /// <see cref="NtlmPassiveUnwrapper"/> was configured on the
    /// <see cref="OpcDcomDecoder"/> and the PDU carried a non-zero
    /// auth_length. Null when no unwrapper was configured OR the PDU
    /// had no auth trailer (e.g. bind / bind-ack / unprotected
    /// connections). Round-trip values mirror <see cref="NtlmUnwrapStatus"/>.
    /// </summary>
    public string? AuthUnwrapStatus { get; init; }

    /// <summary>
    /// Operator-friendly reason populated alongside
    /// <see cref="AuthUnwrapStatus"/> when unwrap fails (e.g. wrong
    /// session key, mid-session capture / counter drift). Null on
    /// success or when no unwrap was attempted.
    /// </summary>
    public string? AuthUnwrapReason { get; init; }

    /// <summary>
    /// Source-supplied annotation set carried through from the
    /// <see cref="CapturedPacket"/> (for example file metadata from a
    /// hex-source replay).
    /// </summary>
    public IReadOnlyDictionary<string, string?>? Annotations { get; init; }
}

/// <summary>
/// Presentation context advertised in a bind / alter_context PDU.
/// </summary>
public sealed record class PresentationContextInfo(int ContextId, Guid AbstractSyntaxIid, int MajorVersion, int MinorVersion);

/// <summary>
/// Presentation result returned in a bind_ack / alter_context_resp PDU.
/// </summary>
public sealed record class PresentationResultInfo(string Result, string Reason);
