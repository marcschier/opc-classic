//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Mcp.Capture;
using SharpPcap.LibPcap;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// MCP tool surface for OPC Classic network packet capture.
/// </summary>
/// <remarks>
/// <para>
/// All tools live under the <c>opcclassic.capture.*</c> namespace and
/// operate against a process-wide
/// <see cref="CaptureSessionManager"/> registered as a singleton in
/// <c>Program.cs</c>.
/// </para>
/// <para>
/// <strong>Privileges</strong>: live NIC capture requires Administrator
/// on Windows (with Npcap installed) or root / CAP_NET_ADMIN+CAP_NET_RAW
/// on Linux. The <c>opcclassic.capture.start</c> tool fails with an
/// actionable <see cref="McpException"/> when those prerequisites are
/// missing.
/// </para>
/// </remarks>
public sealed class CaptureTools
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly CaptureSessionManager _manager;

    /// <summary>Creates the capture tool set, injected by the host.</summary>
    public CaptureTools(CaptureSessionManager manager)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
    }

    /// <summary>Lists NICs available for live capture.</summary>
    [McpServerTool(Name = "opcclassic.capture.list_interfaces", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Enumerate NICs that can be used as 'interfaceName' for opcclassic.capture.start. On Windows requires Npcap; on Linux a libpcap install.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "list_interfaces is a top-level MCP tool that maps every libpcap/Npcap initialization failure into a user-actionable McpException message.")]
    public IReadOnlyList<CaptureInterfaceDto> ListInterfaces()
    {
        var result = new List<CaptureInterfaceDto>();
        LibPcapLiveDeviceList? devices;
        try
        {
            devices = LibPcapLiveDeviceList.Instance;
        }
        catch (Exception ex)
        {
            throw new McpException(
                "Unable to enumerate network interfaces (libpcap/Npcap installed and process has required privileges?): " + ex.Message);
        }

        foreach (LibPcapLiveDevice d in devices)
        {
            string? linkType = null;
            try { linkType = d.LinkType.ToString(); }
            catch (Exception) { /* device LinkType only known after open */ }

            var addresses = new List<string>();
            try
            {
                if (d.Addresses is not null)
                {
                    foreach (PcapAddress? a in d.Addresses)
                    {
                        System.Net.IPAddress? ip = a?.Addr?.ipAddress;
                        if (ip is not null)
                        {
                            addresses.Add(ip.ToString());
                        }
                    }
                }
            }
            catch (Exception) { /* tolerate address enumeration failures on individual NICs */ }

            result.Add(new CaptureInterfaceDto(
                Name: d.Name ?? string.Empty,
                FriendlyName: TryGetFriendlyName(d.Name) ?? d.Description,
                Description: d.Description,
                Addresses: addresses,
                LinkType: linkType,
                IsLoopback: addresses.Any(a => string.Equals(a, "127.0.0.1", StringComparison.Ordinal) || string.Equals(a, "::1", StringComparison.Ordinal))));
        }

        return result;
    }

    /// <summary>Starts a new capture session.</summary>
    [McpServerTool(Name = "opcclassic.capture.start", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = false)]
    [Description("Begin a network packet capture session. Defaults the BPF filter to TCP DCOM (port 135 + dynamic range). Returns the capture session id; use opcclassic.capture.stop + opcclassic.capture.get to retrieve the trace.")]
    public async Task<CaptureSessionDto> StartCapture(
        [Description("Network interface name from opcclassic.capture.list_interfaces (required for pcap source).")]
        string interfaceName,
        [Description("Optional BPF filter override. Default = TCP port 135 + dynamic ephemeral range (Opc DCOM). Takes precedence over serverPorts when both are set.")]
        string? bpfFilter = null,
        [Description("True (default) opens the interface in promiscuous mode.")]
        bool promiscuous = true,
        [Description("Optional cap on captured bytes; defaults to 50 MB.")]
        long? maxBytes = null,
        [Description("Optional cap on captured frame count; null = no per-frame cap.")]
        long? maxPackets = null,
        [Description("Optional cap on wall-clock duration in seconds; defaults to 1800 (30 min).")]
        int? maxDurationSeconds = null,
        [Description("Optional explicit list of OPC server data ports. When set AND bpfFilter is null, narrows the default port-range filter to 'tcp and (port 135 or port P1 or port P2 …)'. Reduces captured noise dramatically when the target server ports are known (look them up via opcclassic.discovery.list_servers + opcclassic.da.connect, or read them from your operator run-book).")]
        int[]? serverPorts = null,
        [Description("DEVELOPER-ONLY. Optional 32-character hex-encoded 16-byte NTLMv2 session key for opt-in auth-trailer unwrap of sign/seal-protected DCOM traffic. Never log or persist the key. Capture MUST start BEFORE the NTLM Type3 handshake or per-direction sequence counters will drift and unwrap will fail. The wire-level NtlmPassiveUnwrapper is usable from offline pcap-analysis scripts today; full in-decoder integration (decoder reads auth_length from frame, extracts trailer, surfaces NtlmUnwrapStatus on each DecodedOpcPdu) is tracked as a CA9-c follow-up — passing this param today validates + plumbs the key but does not yet decrypt PDUs inline.")]
        string? ntlmSessionKeyHex = null,
        CancellationToken cancellationToken = default)
    {
        byte[]? sessionKey = null;
        if (!string.IsNullOrWhiteSpace(ntlmSessionKeyHex))
        {
            sessionKey = ParseNtlmSessionKey(ntlmSessionKeyHex);
        }
        try
        {
            CaptureSession session = await _manager.CreateAndStartAsync(
                PcapCaptureSource.SourceName,
                folder => new PcapCaptureSource(folder),
                new CaptureStartRequest(
                    InterfaceName: interfaceName,
                    BpfFilter: bpfFilter,
                    Promiscuous: promiscuous,
                    MaxBytes: maxBytes,
                    MaxPackets: maxPackets,
                    MaxDurationSeconds: maxDurationSeconds,
                    ServerPorts: serverPorts,
                    NtlmSessionKey: sessionKey),
                cancellationToken).ConfigureAwait(false);
            return CaptureSessionDto.From(session);
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    /// <summary>
    /// Parses a 32-hex-char NTLM session key into a 16-byte array.
    /// Strict on length + hex-character validity so the operator gets
    /// an actionable error before the capture even starts.
    /// </summary>
    private static byte[] ParseNtlmSessionKey(string hex)
    {
        ArgumentNullException.ThrowIfNull(hex);
        // Accept optional 0x prefix, strip whitespace + separators.
        string cleaned = StripHexFormatting(hex);
        if (cleaned.Length != 32)
        {
            throw new McpException(
                $"ntlmSessionKeyHex must be exactly 32 hex characters (16 bytes); got {cleaned.Length}.");
        }
        try
        {
            return Convert.FromHexString(cleaned);
        }
        catch (FormatException ex)
        {
            throw new McpException($"ntlmSessionKeyHex contains non-hex characters: {ex.Message}");
        }
    }

    private static string StripHexFormatting(string hex)
    {
        const string prefix = "0x";
        ReadOnlySpan<char> input = hex.AsSpan();
        if (input.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            input = input[prefix.Length..];
        }
        var sb = new StringBuilder(input.Length);
        foreach (char c in input)
        {
            if (!char.IsWhiteSpace(c) && c != ':' && c != '-' && c != ',' && c != ';')
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    /// <summary>Stops a capture session and finalises the trace.</summary>
    [McpServerTool(Name = "opcclassic.capture.stop", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Stop an in-progress capture. After this returns, the trace is safe to read via opcclassic.capture.get.")]
    public async Task<CaptureSessionDto> StopCapture(
        [Description("Capture session id returned by opcclassic.capture.start.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        try
        {
            await session.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }

        return CaptureSessionDto.From(session);
    }

    /// <summary>Lists known capture sessions.</summary>
    [McpServerTool(Name = "opcclassic.capture.list", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("List capture sessions; pass state=active|running|completed|failed|all (default = all).")]
    public IReadOnlyList<CaptureSessionDto> ListCaptures(
        [Description("Optional state filter: active (=Starting|Running), running, completed, failed, all (default).")]
        string? state = null)
    {
        CaptureSessionState? filter = ParseStateFilter(state);
        IReadOnlyList<CaptureSession> sessions = _manager.List(filter);
        var result = new List<CaptureSessionDto>(sessions.Count);
        foreach (CaptureSession s in sessions)
        {
            if (state is { Length: > 0 } && string.Equals(state, "active", StringComparison.OrdinalIgnoreCase))
            {
                if (s.State is CaptureSessionState.Starting or CaptureSessionState.Running)
                {
                    result.Add(CaptureSessionDto.From(s));
                }
                continue;
            }
            result.Add(CaptureSessionDto.From(s));
        }
        return result;
    }

    /// <summary>Returns the trace bytes in the requested format.</summary>
    [McpServerTool(Name = "opcclassic.capture.get", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Return the captured trace as text. Formats: 'pcap-path' (returns full path to the libpcap file, suitable for opening in Wireshark), 'dcom' (decoded DCE/RPC PDU summary, default), 'json' (raw PDU records). Binary pcap bytes are NOT inlined to keep MCP payload bounded; open the file path with Wireshark.")]
    public async Task<string> GetCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        [Description("Output format: 'dcom' (default, structured PDU view), 'json' (raw decoded PDUs), 'pcap-path' (file path to the libpcap file).")]
        string format = "dcom",
        [Description("Maximum PDUs to decode/return (default 200).")]
        int maxPdus = 200,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        if (string.Equals(format, "pcap-path", StringComparison.OrdinalIgnoreCase))
        {
            string? path = session.Source.GetRawPcapFilePath();
            return path is null
                ? throw new McpException("This capture source does not produce a libpcap file.")
                : path;
        }

        NtlmPassiveUnwrapper? unwrapper = CaptureSession.BuildUnwrapper(session.Request);
        try
        {
            var decoder = new OpcDcomDecoder(unwrapper);
            var pdus = new List<DecodedOpcPdu>();
            int decoded = 0;
            await foreach (CapturedPacket pkt in session.Source.ReadAllAsync(maxPackets: null, cancellationToken).ConfigureAwait(false))
            {
                foreach (DecodedOpcPdu pdu in decoder.Decode(pkt))
                {
                    pdus.Add(pdu);
                    decoded++;
                    if (decoded >= maxPdus)
                    {
                        goto done;
                    }
                }
            }
        done:

            if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            {
                return JsonSerializer.Serialize(pdus, s_jsonOptions);
            }

            // 'dcom' (default): human-readable per-PDU summary
            var sb = new StringBuilder();
            sb.AppendLine(CultureInfo.InvariantCulture, $"# Opc.Classic capture session {sessionId} — {pdus.Count} PDUs");
            foreach (DecodedOpcPdu pdu in pdus)
            {
                sb.AppendLine(CultureInfo.InvariantCulture, $"{pdu.Timestamp:O}  {pdu.PduType,-20} {pdu.SourceEndpoint,-22} -> {pdu.DestinationEndpoint,-22} call_id={pdu.CallId}");
                if (pdu.InterfaceId is Guid iid && iid != Guid.Empty)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"   iid={iid:D}  opnum={pdu.Opnum?.ToString(CultureInfo.InvariantCulture) ?? "-"}  ipid={pdu.ObjectIpid?.ToString("D", CultureInfo.InvariantCulture) ?? "-"}");
                }
                if (pdu.Hresult is int hr)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"   hresult=0x{hr:X8}");
                }
                if (pdu.FaultStatus is int fault)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture, $"   fault_status=0x{fault:X8}");
                }
                if (pdu.ContextList.Count > 0)
                {
                    foreach (PresentationContextInfo c in pdu.ContextList)
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture, $"   ctx[{c.ContextId}] iid={c.AbstractSyntaxIid:D} ver={c.MajorVersion}.{c.MinorVersion}");
                    }
                }
                if (pdu.ResultList.Count > 0)
                {
                    for (int i = 0; i < pdu.ResultList.Count; i++)
                    {
                        sb.AppendLine(CultureInfo.InvariantCulture, $"   result[{i}] {pdu.ResultList[i].Result}; {pdu.ResultList[i].Reason}");
                    }
                }
            }
            return sb.ToString();
        }
        finally
        {
            // Zero NTLM session-derived sub-keys promptly when the one-shot
            // decoder goes out of scope. The per-session TailCapture cursor
            // owns its own unwrapper and disposes it via CaptureSession.DisposeAsync.
            unwrapper?.Dispose();
        }
    }

    /// <summary>
    /// Polling-cursor "tail" of a live capture: returns the next
    /// decoded-PDU window since the caller's cursor. The caller polls
    /// at whatever cadence they want (e.g. every 100 ms) by passing
    /// the previous response's <c>nextIndex</c> as <c>sinceIndex</c>.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.tail", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Polling-cursor tail of a LIVE (or completed) capture. Returns the next decoded-PDU window since the caller's cursor. Idempotent given the same sinceIndex. To follow a live stream, poll repeatedly at your preferred cadence (e.g. 100-500 ms) passing the previous response's nextIndex as sinceIndex; stop when done=true (session ended AND cursor caught up).")]
    public async Task<CaptureTailResultDto> TailCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        [Description("Maximum PDUs to return in this call (default 200, hard cap 5000 to keep MCP payload bounded).")]
        int max = 200,
        [Description("Cursor returned by the previous tail call as nextIndex. Pass 0 for the first call.")]
        long sinceIndex = 0,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        // Hard upper-bound the per-call cap so a misbehaving client can't
        // request a huge window and stall the MCP transport. The cursor
        // pattern means the caller just polls more often to drain quickly.
        int effectiveMax = max <= 0 ? 200 : Math.Min(max, 5000);

        try
        {
            DrainTailResult result = await session.DrainTailAsync(sinceIndex, effectiveMax, cancellationToken).ConfigureAwait(false);
            return new CaptureTailResultDto
            {
                SessionId = sessionId,
                Pdus = result.Pdus,
                NextIndex = result.NextIndex,
                TotalEmitted = result.TotalEmitted,
                Done = result.Done,
                SessionState = result.SessionState,
            };
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    /// <summary>Returns a top-N roll-up of a completed capture.</summary>
    [McpServerTool(Name = "opcclassic.capture.summarize", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Returns top-N talkers, ports, IIDs, opnums, IPIDs, fault codes, and bind-reject reasons for a completed capture session.")]
    public async Task<CaptureSummary> SummarizeCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        [Description("Top-N entries per category (default 10).")]
        int top = 10,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        NtlmPassiveUnwrapper? unwrapper = CaptureSession.BuildUnwrapper(session.Request);
        try
        {
            var decoder = new OpcDcomDecoder(unwrapper);
            var pdus = new List<DecodedOpcPdu>();
            await foreach (CapturedPacket pkt in session.Source.ReadAllAsync(maxPackets: null, cancellationToken).ConfigureAwait(false))
            {
                foreach (DecodedOpcPdu pdu in decoder.Decode(pkt))
                {
                    pdus.Add(pdu);
                }
            }
            return CaptureSummarizer.Summarize(sessionId, pdus, top);
        }
        finally
        {
            unwrapper?.Dispose();
        }
    }

    /// <summary>
    /// Removes a capture session (stops it if still running, then disposes).
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.remove", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = false)]
    [Description("Stop (if needed) + dispose a capture session. The session's scratch folder is removed. Returns true when removed.")]
    public async Task<bool> RemoveCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _manager.RemoveAsync(sessionId, cancellationToken).ConfigureAwait(false);
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    /// <summary>
    /// Decode a single ad-hoc DCE/RPC PDU frame for diagnostic use
    /// (paste hex bytes; get a structured view).
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.decode_pdu", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Decode a single DCE/RPC PDU frame from hex bytes. Useful for ad-hoc inspection of a captured byte string without standing up a full capture session.")]
    public string DecodePdu(
        [Description("Hex string of the raw frame bytes (with or without whitespace / 0x prefix).")]
        string hex)
    {
        ArgumentException.ThrowIfNullOrEmpty(hex);
        byte[] bytes = ParseHex(hex);
        var decoder = new OpcDcomDecoder();
        IEnumerable<DecodedOpcPdu> decoded = decoder.Decode(new CapturedPacket(
            Timestamp: DateTimeOffset.UtcNow,
            OriginalLength: bytes.Length,
            Data: bytes,
            LinkType: 0,  // treat as a bare ORPC body; for raw DCE/RPC PDUs the caller wraps via opcclassic.capture.start instead
            Annotations: new Dictionary<string, string?> { ["source"] = "ad-hoc decode_pdu" }));

        return JsonSerializer.Serialize(decoded.ToArray(), s_jsonOptions);
    }

    /// <summary>
    /// Replay tool: walks the captured ORPC bodies and reports per-(IID,opnum)
    /// success/failure counts. Highest-leverage diagnostic for live captures
    /// against unfamiliar servers.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.replay", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Replay captured ORPC bodies through NdrReader and report per-(IID,opnum) counts. Surfaces malformed payloads + serves as a regression harness against captured live traffic.")]
    public async Task<ReplayReport> ReplayCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        var decoder = new OpcDcomDecoder();
        var pdus = new List<DecodedOpcPdu>();
        await foreach (CapturedPacket pkt in session.Source.ReadAllAsync(maxPackets: null, cancellationToken).ConfigureAwait(false))
        {
            foreach (DecodedOpcPdu pdu in decoder.Decode(pkt))
            {
                pdus.Add(pdu);
            }
        }

        var replay = new OrpcReplayTool();
        return replay.Replay(pdus);
    }

    private static CaptureSessionState? ParseStateFilter(string? state)
    {
        if (string.IsNullOrWhiteSpace(state) || string.Equals(state, "all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return state.ToLowerInvariant() switch
        {
            "starting" => CaptureSessionState.Starting,
            "running" or "active" => CaptureSessionState.Running,
            "stopping" => CaptureSessionState.Stopping,
            "completed" => CaptureSessionState.Completed,
            "failed" => CaptureSessionState.Failed,
            "disposed" => CaptureSessionState.Disposed,
            _ => throw new McpException($"Unknown state filter '{state}'. Use active|running|completed|failed|all."),
        };
    }

    private static byte[] ParseHex(string hex)
    {
        // Strip whitespace + optional 0x prefix + commas; tolerate hex dumps like "01 02 0a, 0xff".
        Span<char> buffer = stackalloc char[Math.Min(hex.Length, 16384)];
        int idx = 0;
        for (int i = 0; i < hex.Length && idx < buffer.Length; i++)
        {
            char c = hex[i];
            if (char.IsWhiteSpace(c) || c == ',' || c == ':' || c == ';')
            {
                continue;
            }

            if (c == '0' && i + 1 < hex.Length && (hex[i + 1] == 'x' || hex[i + 1] == 'X'))
            {
                i++;
                continue;
            }
            buffer[idx++] = c;
        }

        if ((idx & 1) != 0)
        {
            throw new McpException("Hex input has an odd nibble count after stripping whitespace/prefix.");
        }

        byte[] result = new byte[idx / 2];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = byte.Parse(buffer.Slice(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        return result;
    }

    private static string? TryGetFriendlyName(string? interfaceName)
    {
        if (string.IsNullOrEmpty(interfaceName))
        {
            return null;
        }

        try
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (interfaceName.Contains(ni.Id, StringComparison.OrdinalIgnoreCase)
                    || interfaceName.Contains(ni.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return ni.Name;
                }
            }
        }
        catch (NetworkInformationException) { /* tolerate */ }
        return null;
    }
}

/// <summary>NIC info DTO surfaced by opcclassic.capture.list_interfaces.</summary>
public sealed record class CaptureInterfaceDto(
    string Name,
    string? FriendlyName,
    string? Description,
    IReadOnlyList<string> Addresses,
    string? LinkType,
    bool IsLoopback);

/// <summary>
/// Result envelope returned by <c>opcclassic.capture.tail</c>. The
/// caller drives the polling loop by reading <see cref="NextIndex"/>
/// from each response and passing it as <c>sinceIndex</c> on the next
/// call. Stop polling when <see cref="Done"/> is true.
/// </summary>
public sealed record class CaptureTailResultDto
{
    /// <summary>The capture session id that owns this drain.</summary>
    public required string SessionId { get; init; }

    /// <summary>The next decoded-PDU window (length &le; <c>max</c>).</summary>
    public required IReadOnlyList<DecodedOpcPdu> Pdus { get; init; }

    /// <summary>Cursor to pass as <c>sinceIndex</c> on the next call.</summary>
    public required long NextIndex { get; init; }

    /// <summary>Total PDU count emitted by the session decoder so far.</summary>
    public required long TotalEmitted { get; init; }

    /// <summary>True when the session has ended AND the cursor is caught up.</summary>
    public required bool Done { get; init; }

    /// <summary>Underlying session lifecycle state at the time of the drain.</summary>
    public required CaptureSessionState SessionState { get; init; }
}

/// <summary>Capture session info DTO surfaced by opcclassic.capture.* tools.</summary>
public sealed record class CaptureSessionDto
{
    public required string SessionId { get; init; }
    public required string Source { get; init; }
    public required CaptureSessionState State { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? StoppedAt { get; init; }
    public long PacketCount { get; init; }
    public long ByteCount { get; init; }
    public string? InterfaceName { get; init; }
    public string? Filter { get; init; }
    public string? Error { get; init; }
    public string? RawPcapFilePath { get; init; }

    public static CaptureSessionDto From(CaptureSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return new CaptureSessionDto
        {
            SessionId = session.Id,
            Source = session.SourceName,
            State = session.State,
            StartedAt = session.StartedAt,
            StoppedAt = session.StoppedAt,
            PacketCount = session.Source.PacketCount,
            ByteCount = session.Source.ByteCount,
            InterfaceName = session.Request.InterfaceName,
            Filter = session.Request.BpfFilter,
            Error = session.Error,
            RawPcapFilePath = session.Source.GetRawPcapFilePath(),
        };
    }
}
