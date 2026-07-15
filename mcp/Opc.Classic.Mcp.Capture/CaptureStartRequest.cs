// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Caller-supplied parameters to <see cref="ICaptureSource.StartAsync"/>.
/// </summary>
/// <param name="InterfaceName">
/// Name (or description) of the network interface for live sources;
/// null for sources that don't bind to a NIC.
/// </param>
/// <param name="BpfFilter">
/// Optional Berkeley Packet Filter expression. When null/empty the
/// source applies its default filter (typically the OPC Classic DCOM
/// port set for <see cref="PcapCaptureSource"/>). When non-null this
/// takes precedence over <paramref name="ServerPorts"/>.
/// </param>
/// <param name="Promiscuous">
/// True to open the interface in promiscuous mode (sees frames not
/// addressed to this host). Default true; falls back to non-promiscuous
/// on first open if the OS refuses elevation.
/// </param>
/// <param name="MaxBytes">
/// Stop the capture when the total captured bytes reach this value.
/// Null = engine default (50 MB).
/// </param>
/// <param name="MaxPackets">
/// Stop the capture when the total packet count reaches this value.
/// Null = no per-packet cap.
/// </param>
/// <param name="MaxDurationSeconds">
/// Stop the capture after this many wall-clock seconds since
/// <see cref="ICaptureSource.StartAsync"/>. Null = engine default
/// (1800 = 30 min).
/// </param>
/// <param name="ReplaySourceDirectory">
/// For <c>OpcWireCaptureSource</c>: directory of <c>.hex</c> files to
/// replay as if they were live frames. Ignored by live NIC sources.
/// </param>
/// <param name="ServerPorts">
/// Optional explicit list of TCP server data ports to include in the
/// auto-built BPF filter (in addition to port 135, the DCOM SCM
/// endpoint mapper). When non-empty AND <paramref name="BpfFilter"/>
/// is null/empty, the source narrows the default
/// <c>tcp and (port 135 or portrange 49152-65535)</c> filter down to
/// <c>tcp and (port 135 or port p1 or port p2 …)</c>. Reduces captured
/// noise dramatically when the target OPC server ports are known
/// (e.g. discovered out-of-band via OPCEnum or read from the operator
/// run-book). Ignored when an explicit BPF filter is supplied. Each
/// port must be a positive 1..65535 value; duplicates are tolerated
/// and de-duplicated before composing.
/// </param>
/// <param name="NtlmSessionKey">
/// Optional 16-byte NTLMv2 session key for the developer-only
/// auth-trailer unwrap path (see <see cref="NtlmPassiveUnwrapper"/>).
/// When non-null AND the captured traffic was sign/seal-protected
/// with NTLMv2, the capture engine will attempt to decrypt + verify
/// the auth trailer on every captured Request/Response PDU and
/// surface the outcome via <c>DecodedOpcPdu.AuthUnwrapStatus</c>.
/// SECURITY: developer-only / authorised-traffic-only. Never log or
/// persist the key. Capture must start BEFORE the NTLM Type3
/// handshake or per-direction counters will drift. Live-session
/// tail/get/summarize decoding performs the unwrap inline. Ad-hoc
/// decode/replay tools accept their own separately owned key input.
/// </param>
/// <param name="TargetHost">Optional target host used for capture-time endpoint discovery.</param>
/// <param name="ProgId">Optional OPC ProgID resolved after broad capture has started.</param>
/// <param name="Clsid">Optional OPC CLSID activated after broad capture has started.</param>
/// <param name="ConnectionString">Optional DCOM/OPC/TCP connection string describing the target.</param>
public sealed record class CaptureStartRequest(
    string? InterfaceName = null,
    string? BpfFilter = null,
    bool Promiscuous = true,
    long? MaxBytes = null,
    long? MaxPackets = null,
    int? MaxDurationSeconds = null,
    string? ReplaySourceDirectory = null,
    IReadOnlyList<int>? ServerPorts = null,
    byte[]? NtlmSessionKey = null,
    string? TargetHost = null,
    string? ProgId = null,
    string? Clsid = null,
    string? ConnectionString = null)
{
    /// <summary>
    /// Custom <see cref="ToString"/> that REDACTS the
    /// <see cref="NtlmSessionKey"/> byte array. The default
    /// record-auto-generated ToString would dump the raw key bytes —
    /// which we MUST NOT do because the key is equivalent to the
    /// session secret protecting authenticated DCOM traffic.
    /// </summary>
    public override string ToString()
    {
        string keyDescriptor = NtlmSessionKey is null
            ? "null"
            : $"REDACTED[{NtlmSessionKey.Length} bytes]";
        return $"{nameof(CaptureStartRequest)} {{ "
            + $"{nameof(InterfaceName)} = {InterfaceName ?? "null"}, "
            + $"{nameof(BpfFilter)} = {BpfFilter ?? "null"}, "
            + $"{nameof(Promiscuous)} = {Promiscuous}, "
            + $"{nameof(MaxBytes)} = {MaxBytes?.ToString() ?? "null"}, "
            + $"{nameof(MaxPackets)} = {MaxPackets?.ToString() ?? "null"}, "
            + $"{nameof(MaxDurationSeconds)} = {MaxDurationSeconds?.ToString() ?? "null"}, "
            + $"{nameof(ReplaySourceDirectory)} = {ReplaySourceDirectory ?? "null"}, "
            + $"{nameof(ServerPorts)} = {(ServerPorts is null ? "null" : "[" + string.Join(",", ServerPorts) + "]")}, "
            + $"{nameof(TargetHost)} = {TargetHost ?? "null"}, "
            + $"{nameof(ProgId)} = {ProgId ?? "null"}, "
            + $"{nameof(Clsid)} = {Clsid ?? "null"}, "
            + $"{nameof(ConnectionString)} = {ConnectionString ?? "null"}, "
            + $"{nameof(NtlmSessionKey)} = {keyDescriptor} }}";
    }
}
