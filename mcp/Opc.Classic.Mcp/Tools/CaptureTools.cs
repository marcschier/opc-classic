// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
    private readonly ICaptureTargetResolver _targetResolver;
    private readonly Func<string, ICaptureSource> _captureSourceFactory;

    /// <summary>
    /// Creates the capture tool set, injected by the host.
    /// </summary>
    public CaptureTools(CaptureSessionManager manager)
        : this(
            manager,
            new CaptureTargetResolver(),
            folder => new PcapCaptureSource(folder))
    {
    }

    internal CaptureTools(
        CaptureSessionManager manager,
        ICaptureTargetResolver targetResolver,
        Func<string, ICaptureSource> captureSourceFactory)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _targetResolver = targetResolver ?? throw new ArgumentNullException(nameof(targetResolver));
        _captureSourceFactory = captureSourceFactory ?? throw new ArgumentNullException(nameof(captureSourceFactory));
    }

    /// <summary>
    /// Lists NICs available for live capture.
    /// </summary>
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
#pragma warning disable RCS1075 // SharpPcap LinkType throws PcapException before the device is opened — best-effort probe in the discovery path
            catch (Exception) { /* device LinkType only known after open */ }
#pragma warning restore RCS1075

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
#pragma warning disable RCS1075 // SharpPcap Addresses enumeration can throw on Linux/Windows for individual NICs — tolerate per-NIC failures
            catch (Exception) { /* tolerate address enumeration failures on individual NICs */ }
#pragma warning restore RCS1075

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

    /// <summary>
    /// Starts a new capture session.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.start", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Begin a network packet capture session. Optional target fields start broad DCOM capture before target resolution. Authenticated OPCEnum/activation requires ambientSso=true.")]
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
        [Description("Optional explicit list of OPC server data ports. When set AND bpfFilter is null, narrows the default port-range filter to 'tcp and (port 135 or port P1 or port P2 …)'. Reduces captured noise dramatically when the target server ports are known (look them up via opcclassic.discovery.enumerate_servers + opcclassic.da.connect, or read them from your operator run-book).")]
        int[]? serverPorts = null,
        [Description("DEVELOPER-ONLY. Optional 32-character hex-encoded 16-byte NTLMv2 session key for opt-in auth-trailer unwrap of sign/seal-protected DCOM traffic in capture.tail/get/summarize. Never log or persist the key. Capture MUST start BEFORE the NTLM Type3 handshake or per-direction sequence counters will drift and unwrap will fail.")]
        string? ntlmSessionKeyHex = null,
        CancellationToken cancellationToken = default,
        [Description("Optional OPC target host. When any target field is supplied, capture starts with the broad DCOM filter before discovery/activation runs.")]
        string? targetHost = null,
        [Description("Optional OPC ProgID to resolve through the existing OPCEnum connection normalization path after capture starts.")]
        string? progId = null,
        [Description("Optional OPC CLSID to activate after capture starts.")]
        string? clsid = null,
        [Description("Optional dcom://, opcda://, opcae://, opchda://, tcp://, or inmemory:// target connection string.")]
        string? connectionString = null,
        [Description("Explicit opt-in to use the process/current-logon Windows identity for OPCEnum discovery and DCOM activation against the target. Default false: no ambient credential connection is attempted.")]
        bool ambientSso = false)
    {
        byte[]? sessionKey = null;
        CaptureSession? session = null;
        if (!string.IsNullOrWhiteSpace(ntlmSessionKeyHex))
        {
            sessionKey = ParseNtlmSessionKey(ntlmSessionKeyHex);
        }
        try
        {
            bool resolveTarget = HasTarget(targetHost, progId, clsid, connectionString);
            var requested = new CaptureStartRequest(
                InterfaceName: interfaceName,
                BpfFilter: bpfFilter,
                Promiscuous: promiscuous,
                MaxBytes: maxBytes,
                MaxPackets: maxPackets,
                MaxDurationSeconds: maxDurationSeconds,
                ServerPorts: serverPorts,
                NtlmSessionKey: sessionKey,
                TargetHost: targetHost,
                ProgId: progId,
                Clsid: clsid,
                ConnectionString: connectionString,
                AmbientSso: ambientSso);
            CaptureStartRequest startup = resolveTarget
                ? requested with { BpfFilter = null, ServerPorts = null }
                : requested;

            session = await _manager.CreateAndStartAsync(
                PcapCaptureSource.SourceName,
                _captureSourceFactory,
                startup,
                cancellationToken).ConfigureAwait(false);

            if (resolveTarget)
            {
                CaptureTargetMetadata target = await _targetResolver.ResolveAsync(
                    targetHost,
                    progId,
                    clsid,
                    connectionString,
                    cancellationToken,
                    ambientSso).ConfigureAwait(false);
                session.SetTarget(target);

                IReadOnlyList<int> discoveredPorts = target.Status is "resolved" or "activated"
                    ? target.Ports
                    : [];
                int[] effectivePorts = (serverPorts ?? [])
                    .Concat(discoveredPorts)
                    .Where(port => port is > 0 and <= 65535)
                    .Distinct()
                    .Order()
                    .ToArray();
                string desiredFilter = string.IsNullOrWhiteSpace(bpfFilter)
                    ? PcapCaptureSource.BuildServerPortBpfFilter(effectivePorts)
                    : bpfFilter;
                CaptureFilterTransitionResult transition =
                    await session.ReplaceFilterAsync(
                        desiredFilter,
                        requested with { ServerPorts = effectivePorts },
                        cancellationToken).ConfigureAwait(false);
                if (!transition.Succeeded)
                {
                    session.SetTarget(target with
                    {
                        Status = target.Status + "_filter_failed",
                        Error = transition.Error,
                    });
                }
            }
            return CaptureSessionDto.From(session);
        }
        catch (OperationCanceledException)
        {
            if (session is not null)
            {
                await _manager.RemoveAsync(session.Id, CancellationToken.None).ConfigureAwait(false);
            }
            throw;
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
        finally
        {
            if (sessionKey is not null)
            {
                CryptographicOperations.ZeroMemory(sessionKey);
            }
        }
    }

    private static bool HasTarget(
        string? targetHost,
        string? progId,
        string? clsid,
        string? connectionString) =>
        !string.IsNullOrWhiteSpace(targetHost)
        || !string.IsNullOrWhiteSpace(progId)
        || !string.IsNullOrWhiteSpace(clsid)
        || !string.IsNullOrWhiteSpace(connectionString);

    /// <summary>
    /// Atomically replaces the filter on a running capture session.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.set_filter", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Replace a running capture session's BPF filter. Uses a live source update when supported, otherwise starts a replacement source before retiring the prior source. Returns an explicit transition report; failed updates leave the prior capture visible.")]
    public async Task<CaptureFilterTransitionResult> SetCaptureFilter(
        [Description("Capture session id returned by opcclassic.capture.start.")]
        string sessionId,
        [Description("Replacement BPF filter.")]
        string bpfFilter,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        return await session.ReplaceFilterAsync(
            bpfFilter,
            session.Request with { BpfFilter = bpfFilter, ServerPorts = null },
            cancellationToken).ConfigureAwait(false);
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

    /// <summary>
    /// Stops a capture session and finalises the trace.
    /// </summary>
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

    /// <summary>
    /// Lists known capture sessions.
    /// </summary>
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

    /// <summary>
    /// Returns the trace bytes in the requested format.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.get", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Return the captured trace as text. Formats: 'pcap-path' (one retained pcap), 'pcap-paths' (JSON array for restarted captures), 'dcom' (decoded DCE/RPC PDU summary, default), 'json' (raw PDU records). Binary pcap bytes are not inlined.")]
    public async Task<string> GetCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        [Description("Output format: 'dcom' (default), 'json', 'pcap-path' (single segment), or 'pcap-paths' (all retained segment paths).")]
        string format = "dcom",
        [Description("Maximum PDUs to decode/return (default 200).")]
        int maxPdus = 200,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        if (string.Equals(format, "pcap-path", StringComparison.OrdinalIgnoreCase)
            || string.Equals(format, "pcap-paths", StringComparison.OrdinalIgnoreCase))
        {
            IReadOnlyList<string> paths = session.RawPcapFilePaths;
            if (paths.Count == 0)
            {
                throw new McpException("This capture source does not produce a libpcap file.");
            }
            if (string.Equals(format, "pcap-path", StringComparison.OrdinalIgnoreCase))
            {
                return paths.Count == 1
                    ? paths[0]
                    : throw new McpException(
                        "This capture contains multiple retained pcap segments. Request format='pcap-paths' to retrieve every path.");
            }
            return JsonSerializer.Serialize(paths, s_jsonOptions);
        }

        NtlmPassiveUnwrapper? unwrapper = session.CreateUnwrapper();
        try
        {
            var decoder = new OpcDcomDecoder(unwrapper);
            var pdus = new List<DecodedOpcPdu>();
            await foreach (CapturedPacket pkt in session.ReadAllAsync(maxPackets: null, cancellationToken).ConfigureAwait(false))
            {
                foreach (DecodedOpcPdu pdu in decoder.Decode(pkt))
                {
                    if (pdus.Count < maxPdus)
                    {
                        pdus.Add(pdu);
                    }
                }
            }
            foreach (DecodedDcomFrame completed in decoder.CompleteDetailed())
            {
                if (completed.Pdu is not null && pdus.Count < maxPdus)
                {
                    pdus.Add(completed.Pdu);
                }
            }

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
    [Description("Polling-cursor tail of a LIVE (or completed) capture. Named cursors retain a bounded indexed replay window, so retrying the same sinceIndex returns the same available window; advancing sinceIndex explicitly acknowledges prior records. Poll repeatedly with the previous nextIndex and stop when done=true.")]
    public async Task<CaptureTailResultDto> TailCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        [Description("Maximum PDUs to return in this call (default 200, hard cap 5000 to keep MCP payload bounded).")]
        int max = 200,
        [Description("Cursor returned by the previous tail call as nextIndex. Pass 0 for the first call.")]
        long sinceIndex = 0,
        CancellationToken cancellationToken = default,
        [Description("Optional stable subscriber id for a bounded authoritative server-owned replay cursor. Retry with the same sinceIndex after a lost response; advance to nextIndex to acknowledge. Omit for caller-owned cursor behavior.")]
        string? subscriberId = null,
        [Description("Per-subscriber retained-PDU capacity (1..5000, default 1024). Older unacknowledged PDUs are dropped and returned as inclusive drop ranges.")]
        int? subscriberCapacity = null)
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
            DrainTailResult result = await session.DrainSubscriberTailAsync(
                sinceIndex,
                effectiveMax,
                subscriberId,
                subscriberCapacity,
                cancellationToken).ConfigureAwait(false);
            return new CaptureTailResultDto
            {
                SessionId = sessionId,
                Pdus = result.Pdus,
                NextIndex = result.NextIndex,
                TotalEmitted = result.TotalEmitted,
                Done = result.Done,
                SessionState = result.SessionState,
                SubscriberId = result.SubscriberId,
                SubscriberCapacity = result.SubscriberCapacity,
                Overflowed = result.Overflowed,
                DroppedRanges = result.DroppedRanges ?? [],
            };
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(Name = "opcclassic.capture.close_cursor", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Dispose a named capture.tail cursor without stopping the capture.")]
    public async Task<bool> CloseCaptureCursor(
        string sessionId,
        string subscriberId,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }
        return await session.CloseTailSubscriberAsync(subscriberId, cancellationToken).ConfigureAwait(false);
    }

    [McpServerTool(Name = "opcclassic.capture.subscribe_notifications", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = false)]
    [Description("Subscribe the current MCP client to advisory capture index/state/drop notifications.")]
    public async Task<CaptureNotificationSubscriptionDto> SubscribeCaptureNotifications(
        McpServer server,
        string sessionId,
        long sinceIndex = 0,
        string? subscriberId = null,
        int subscriberCapacity = CaptureNotificationSubscription.DefaultSubscriberCapacity,
        int notificationQueueCapacity = CaptureNotificationSubscription.DefaultNotificationQueueCapacity,
        int pollIntervalMilliseconds = CaptureNotificationSubscription.DefaultPollIntervalMilliseconds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            CaptureNotificationSubscriptionInfo info =
                await _manager.SubscribeNotificationsAsync(
                    sessionId,
                    sinceIndex,
                    subscriberId,
                    subscriberCapacity,
                    notificationQueueCapacity,
                    pollIntervalMilliseconds,
                    new McpCaptureNotificationPublisher(server),
                    cancellationToken).ConfigureAwait(false);
            return CaptureNotificationSubscriptionDto.From(info);
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
    }

    [McpServerTool(Name = "opcclassic.capture.unsubscribe_notifications", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Stop and dispose a capture notification subscription.")]
    public Task<bool> UnsubscribeCaptureNotifications(string subscriptionId) =>
        _manager.UnsubscribeNotificationsAsync(subscriptionId);

    /// <summary>
    /// Returns a top-N roll-up of a completed capture.
    /// </summary>
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

        NtlmPassiveUnwrapper? unwrapper = session.CreateUnwrapper();
        try
        {
            var decoder = new OpcDcomDecoder(unwrapper);
            var pdus = new List<DecodedOpcPdu>();
            await foreach (CapturedPacket pkt in session.ReadAllAsync(maxPackets: null, cancellationToken).ConfigureAwait(false))
            {
                foreach (DecodedOpcPdu pdu in decoder.Decode(pkt))
                {
                    pdus.Add(pdu);
                }
            }
            foreach (DecodedDcomFrame completed in decoder.CompleteDetailed())
            {
                if (completed.Pdu is not null)
                {
                    pdus.Add(completed.Pdu);
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
        string hex,
        [Description("DEVELOPER-ONLY. Optional 32-character hex NTLMv2 session key. The owned key copy and derived keys are zeroed/disposed before return.")]
        string? ntlmSessionKeyHex = null)
    {
        if (hex is null)
        {
            throw CreateDecodeException("hex", "null_input", "Hex input is required.", byteCount: 0, context: null);
        }
        byte[]? sessionKey = null;
        NtlmPassiveUnwrapper? unwrapper = null;
        try
        {
            byte[] bytes = ParseHex(hex);
            if (!string.IsNullOrWhiteSpace(ntlmSessionKeyHex))
            {
                sessionKey = ParseNtlmSessionKey(ntlmSessionKeyHex);
                unwrapper = new NtlmPassiveUnwrapper(sessionKey);
            }
            var decoder = new OpcDcomDecoder(unwrapper);
            NtlmDirection? assumedDirection = bytes.Length > 2
                ? bytes[2] switch
                {
                    Opc.Classic.Dcom.Rpc.pdu.RequestCoPdu.REQUEST_TYPE => NtlmDirection.ClientToServer,
                    Opc.Classic.Dcom.Rpc.pdu.ResponseCoPdu.RESPONSE_TYPE
                        or Opc.Classic.Dcom.Rpc.pdu.FaultCoPdu.FAULT_TYPE => NtlmDirection.ServerToClient,
                    _ => null,
                }
                : null;
            DecodedDcomFrame decoded = decoder.DecodeRawDcomFrameStrict(
                bytes,
                IPAddress.Loopback,
                srcPort: 49152,
                IPAddress.Loopback,
                dstPort: 135,
                DateTimeOffset.UtcNow,
                assumedDirection);
            if (decoded.Failure is not null || decoded.Pdu is null)
            {
                DcomDecodeFailure failure = decoded.Failure
                    ?? new DcomDecodeFailure("projection", "unsupported_pdu", "The frame decoded but did not project to a supported PDU.");
                throw CreateDecodeException(
                    failure.Stage,
                    failure.Code,
                    failure.Message,
                    bytes.Length,
                    bytes);
            }

            return JsonSerializer.Serialize(new[] { decoded.Pdu }, s_jsonOptions);
        }
        finally
        {
            unwrapper?.Dispose();
            if (sessionKey is not null)
            {
                CryptographicOperations.ZeroMemory(sessionKey);
            }
        }
    }

    /// <summary>
    /// Decodes an external pcap/pcapng file without creating a live session.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.decode_file", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Decode a bounded external pcap/pcapng file containing Ethernet IPv4/IPv6 TCP traffic. Reports IP fragmentation, truncation, incomplete DCE/RPC streams, and likely mid-session starts.")]
    public async Task<CaptureFileDecodeResult> DecodeFile(
        [Description("Path to an external .pcap or .pcapng file.")]
        string path,
        [Description("Maximum file size in bytes (default 50 MB, hard cap 200 MB).")]
        long maxFileBytes = CaptureFileProcessor.DefaultMaxFileBytes,
        [Description("Maximum packets to inspect (default 100000, hard cap 1000000).")]
        int maxPackets = CaptureFileProcessor.DefaultMaxPackets,
        [Description("Maximum decoded records to retain (default 5000, hard cap 50000).")]
        int maxPdus = CaptureFileProcessor.DefaultMaxPdus,
        [Description("DEVELOPER-ONLY. Optional 32-character hex NTLMv2 session key; never logged or persisted.")]
        string? ntlmSessionKeyHex = null,
        CancellationToken cancellationToken = default)
    {
        byte[]? sessionKey = null;
        NtlmPassiveUnwrapper? unwrapper = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(ntlmSessionKeyHex))
            {
                sessionKey = ParseNtlmSessionKey(ntlmSessionKeyHex);
                unwrapper = new NtlmPassiveUnwrapper(sessionKey);
            }
            return await CaptureFileProcessor.DecodeAsync(
                path,
                maxFileBytes,
                maxPackets,
                maxPdus,
                unwrapper,
                cancellationToken).ConfigureAwait(false);
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
        finally
        {
            unwrapper?.Dispose();
            if (sessionKey is not null)
            {
                CryptographicOperations.ZeroMemory(sessionKey);
            }
        }
    }

    /// <summary>
    /// Replays an external pcap/pcapng file through ORPC validation.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.replay_file", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Replay a bounded external pcap/pcapng file through PduCodec and ORPC validation, with the same fragmentation and mid-session status as decode_file.")]
    public async Task<CaptureFileReplayResult> ReplayFile(
        [Description("Path to an external .pcap or .pcapng file.")]
        string path,
        [Description("Maximum file size in bytes (default 50 MB, hard cap 200 MB).")]
        long maxFileBytes = CaptureFileProcessor.DefaultMaxFileBytes,
        [Description("Maximum packets to inspect (default 100000, hard cap 1000000).")]
        int maxPackets = CaptureFileProcessor.DefaultMaxPackets,
        [Description("Maximum decoded/replay records to retain (default 5000, hard cap 50000).")]
        int maxPdus = CaptureFileProcessor.DefaultMaxPdus,
        [Description("DEVELOPER-ONLY. Optional 32-character hex NTLMv2 session key; never logged or persisted.")]
        string? ntlmSessionKeyHex = null,
        CancellationToken cancellationToken = default)
    {
        byte[]? sessionKey = null;
        NtlmPassiveUnwrapper? unwrapper = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(ntlmSessionKeyHex))
            {
                sessionKey = ParseNtlmSessionKey(ntlmSessionKeyHex);
                unwrapper = new NtlmPassiveUnwrapper(sessionKey);
            }
            return await CaptureFileProcessor.ReplayAsync(
                path,
                maxFileBytes,
                maxPackets,
                maxPdus,
                unwrapper,
                cancellationToken).ConfigureAwait(false);
        }
        catch (CaptureException ex)
        {
            throw new McpException(ex.Message);
        }
        finally
        {
            unwrapper?.Dispose();
            if (sessionKey is not null)
            {
                CryptographicOperations.ZeroMemory(sessionKey);
            }
        }
    }

    /// <summary>
    /// Replay tool: re-decodes captured DCE/RPC frames and validates their
    /// ORPC envelopes, reporting per-(IID,opnum,direction) outcomes.
    /// </summary>
    [McpServerTool(Name = "opcclassic.capture.replay", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Replay captured request/response/fault frames through PduCodec, validate ORPC_THIS/ORPC_THAT envelopes, and report per-(IID,opnum,direction) succeeded/failed/skipped counts with bounded first-failure hex context.")]
    public async Task<ReplayReport> ReplayCapture(
        [Description("Capture session id from opcclassic.capture.start.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (!_manager.TryGet(sessionId, out CaptureSession session))
        {
            throw new McpException($"Capture session '{sessionId}' not found.");
        }

        NtlmPassiveUnwrapper? unwrapper = session.CreateUnwrapper();
        try
        {
            var decoder = new OpcDcomDecoder(unwrapper);
            var frames = new List<DecodedDcomFrame>();
            await foreach (CapturedPacket pkt in session.ReadAllAsync(maxPackets: null, cancellationToken).ConfigureAwait(false))
            {
                frames.AddRange(decoder.DecodeDetailed(pkt));
            }
            frames.AddRange(decoder.CompleteDetailed());

            var replay = new OrpcReplayTool();
            return replay.ReplayDetailed(frames, cancellationToken);
        }
        finally
        {
            unwrapper?.Dispose();
        }
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
        const int maxPduBytes = ushort.MaxValue;
        const int maxHexChars = maxPduBytes * 2;
        var cleaned = new StringBuilder(Math.Min(hex.Length, maxHexChars));
        bool sawHexDigit = false;
        bool consumedPrefix = false;
        for (int i = 0; i < hex.Length; i++)
        {
            char c = hex[i];
            if (char.IsWhiteSpace(c) || c == ',' || c == ':' || c == ';')
            {
                continue;
            }

            if (!sawHexDigit
                && !consumedPrefix
                && c == '0'
                && i + 1 < hex.Length
                && (hex[i + 1] == 'x' || hex[i + 1] == 'X'))
            {
                consumedPrefix = true;
                i++;
                continue;
            }

            if (!Uri.IsHexDigit(c))
            {
                throw CreateDecodeException(
                    "hex",
                    "invalid_character",
                    $"Hex input contains invalid character '{c}' at offset {i}.",
                    byteCount: cleaned.Length / 2,
                    context: null);
            }
            if (cleaned.Length >= maxHexChars)
            {
                throw CreateDecodeException(
                    "hex",
                    "input_too_large",
                    $"Hex input exceeds the maximum raw DCE/RPC PDU size of {maxPduBytes} bytes.",
                    byteCount: cleaned.Length / 2,
                    context: null);
            }

            cleaned.Append(c);
            sawHexDigit = true;
        }

        if ((cleaned.Length & 1) != 0)
        {
            throw CreateDecodeException(
                "hex",
                "odd_nibble_count",
                "Hex input has an odd nibble count after stripping whitespace/prefix.",
                byteCount: cleaned.Length / 2,
                context: null);
        }

        try
        {
            return Convert.FromHexString(cleaned.ToString());
        }
        catch (FormatException ex)
        {
            throw CreateDecodeException(
                "hex",
                "invalid_character",
                ex.Message,
                cleaned.Length / 2,
                context: null);
        }
    }

    private static McpException CreateDecodeException(
        string stage,
        string code,
        string message,
        int byteCount,
        byte[]? context)
    {
        string? hexContext = context is { Length: > 0 }
            ? Convert.ToHexString(context.AsSpan(0, Math.Min(context.Length, 16)))
            : null;
        string payload = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["code"] = "capture_decode_failed",
            ["stage"] = stage,
            ["reason"] = code,
            ["message"] = message,
            ["byteCount"] = byteCount,
            ["hexContext"] = hexContext,
        });
        return new McpException(payload);
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

/// <summary>
/// NIC info DTO surfaced by opcclassic.capture.list_interfaces.
/// </summary>
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
    /// <summary>
    /// The capture session id that owns this drain.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// The next decoded-PDU window (length &le; <c>max</c>).
    /// </summary>
    public required IReadOnlyList<DecodedOpcPdu> Pdus { get; init; }

    /// <summary>
    /// Cursor to pass as <c>sinceIndex</c> on the next call.
    /// </summary>
    public required long NextIndex { get; init; }

    /// <summary>
    /// Total PDU count emitted by the session decoder so far.
    /// </summary>
    public required long TotalEmitted { get; init; }

    /// <summary>
    /// True when the session has ended AND the cursor is caught up.
    /// </summary>
    public required bool Done { get; init; }

    /// <summary>
    /// Underlying session lifecycle state at the time of the drain.
    /// </summary>
    public required CaptureSessionState SessionState { get; init; }
    public string? SubscriberId { get; init; }
    public int? SubscriberCapacity { get; init; }
    public bool Overflowed { get; init; }
    public IReadOnlyList<CaptureDropRange> DroppedRanges { get; init; } = [];
}

/// <summary>
/// Advisory MCP capture notification subscription metadata.
/// </summary>
public sealed record class CaptureNotificationSubscriptionDto
{
    public required string SubscriptionId { get; init; }

    public required string SessionId { get; init; }

    public required string SubscriberId { get; init; }

    public long SinceIndex { get; init; }

    public int SubscriberCapacity { get; init; }

    public int NotificationQueueCapacity { get; init; }

    public int PollIntervalMilliseconds { get; init; }

    internal static CaptureNotificationSubscriptionDto From(CaptureNotificationSubscriptionInfo info) =>
        new()
        {
            SubscriptionId = info.SubscriptionId,
            SessionId = info.SessionId,
            SubscriberId = info.SubscriberId,
            SinceIndex = info.SinceIndex,
            SubscriberCapacity = info.SubscriberCapacity,
            NotificationQueueCapacity = info.NotificationQueueCapacity,
            PollIntervalMilliseconds = info.PollIntervalMilliseconds,
        };
}

/// <summary>
/// Capture session info DTO surfaced by opcclassic.capture.* tools.
/// </summary>
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
    public IReadOnlyList<string> RawPcapFilePaths { get; init; } = [];
    public string? EffectiveFilter { get; init; }
    public CaptureTargetMetadata? Target { get; init; }
    public CaptureFilterTransitionResult? FilterTransition { get; init; }

    public static CaptureSessionDto From(CaptureSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        CaptureSessionSnapshot snapshot = session.GetSnapshot();
        return new CaptureSessionDto
        {
            SessionId = snapshot.Id,
            Source = snapshot.SourceName,
            State = snapshot.State,
            StartedAt = snapshot.StartedAt,
            StoppedAt = snapshot.StoppedAt,
            PacketCount = snapshot.PacketCount,
            ByteCount = snapshot.ByteCount,
            InterfaceName = snapshot.Request.InterfaceName,
            Filter = snapshot.Request.BpfFilter,
            EffectiveFilter = snapshot.EffectiveFilter,
            Error = snapshot.Error,
            RawPcapFilePath = snapshot.RawPcapFilePaths.Count == 1
                ? snapshot.RawPcapFilePaths[0]
                : null,
            RawPcapFilePaths = snapshot.RawPcapFilePaths,
            Target = snapshot.Target,
            FilterTransition = snapshot.FilterTransition,
        };
    }
}
