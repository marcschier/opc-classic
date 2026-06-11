//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SharpPcap;
using SharpPcap.LibPcap;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// SharpPcap-backed live NIC capture source. Persists raw frames to a
/// libpcap-format file under the session folder for replay + native
/// pcap-export.
/// </summary>
/// <remarks>
/// <para>
/// Default safety limits (applied when the start request leaves the
/// matching field null):
/// </para>
/// <list type="bullet">
///   <item><description>50 MB total bytes captured</description></item>
///   <item><description>30 minutes wall-clock duration</description></item>
/// </list>
/// <para>
/// Privilege requirements: Administrator on Windows (Npcap); root or
/// CAP_NET_ADMIN/CAP_NET_RAW on Linux. The constructor enforces nothing;
/// <see cref="StartAsync"/> surfaces the privilege-related failure with
/// an actionable message via <see cref="CaptureException"/>.
/// </para>
/// </remarks>
public sealed class PcapCaptureSource : ICaptureSource
{
    /// <summary>Stable source name surfaced via the MCP info DTO.</summary>
    public const string SourceName = "pcap";

    private const int kDefaultMaxBytes = 50 * 1024 * 1024;
    private const int kDefaultMaxDurationSeconds = 30 * 60;
    private const string kPcapFileName = "capture.pcap";

    /// <summary>
    /// Default BPF filter for the OPC Classic DCOM-over-IP universe.
    /// Captures the SCM (port 135) plus the dynamic / private TCP port
    /// range that DCOM hands out to activated servers.
    /// </summary>
    public const string DefaultOpcBpfFilter = "tcp and (port 135 or (portrange 49152-65535))";

    /// <summary>
    /// Composes a BPF filter that captures TCP on port 135 (DCOM SCM
    /// endpoint mapper) PLUS the given explicit set of OPC server data
    /// ports. Used by <see cref="CaptureStartRequest.ServerPorts"/> to
    /// narrow the default port-range filter to a specific known set
    /// (dramatically reduces captured noise on busy NICs).
    /// </summary>
    /// <param name="serverPorts">
    /// Explicit port list (1..65535, duplicates tolerated). Null/empty
    /// returns <see cref="DefaultOpcBpfFilter"/> unchanged so callers
    /// don't need a special-case branch upstream. Ports outside the
    /// valid 1..65535 range are silently skipped (per BPF semantics
    /// they could not match anyway).
    /// </param>
    public static string BuildServerPortBpfFilter(IReadOnlyList<int>? serverPorts)
    {
        if (serverPorts is null || serverPorts.Count == 0)
        {
            return DefaultOpcBpfFilter;
        }

        var seen = new SortedSet<int>();
        foreach (int p in serverPorts)
        {
            if (p > 0 && p <= 65535)
            {
                seen.Add(p);
            }
        }

        if (seen.Count == 0)
        {
            return DefaultOpcBpfFilter;
        }

        // tcp and (port 135 or port P1 or port P2 ...). Always include
        // port 135 so the bind/activation traffic is still captured
        // alongside the activated data-port traffic.
        var sb = new System.Text.StringBuilder("tcp and (port 135");
        foreach (int p in seen)
        {
            if (p == 135)
            {
                continue;
            }
            sb.Append(" or port ").Append(p.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
        sb.Append(')');
        return sb.ToString();
    }

    private readonly ILogger _logger;
    private readonly string _filePath;
    private readonly Lock _lock = new();

    private LibPcapLiveDevice? _device;
    private CaptureFileWriterDevice? _writer;
    private long _packetCount;
    private long _byteCount;
    private long _maxBytes;
    private long _maxPackets;
    private DateTimeOffset _startedAt;
    private TimeSpan _maxDuration;
    private volatile bool _stopRequested;
    private int _linkType;

    public PcapCaptureSource(string sessionFolder, ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionFolder);
        _filePath = Path.Combine(sessionFolder, kPcapFileName);
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public long PacketCount => Interlocked.Read(ref _packetCount);

    /// <inheritdoc/>
    public long ByteCount => Interlocked.Read(ref _byteCount);

    /// <inheritdoc/>
    public int LinkType => _linkType;

    /// <inheritdoc/>
    public string? GetRawPcapFilePath()
        => File.Exists(_filePath) ? _filePath : null;

    /// <inheritdoc/>
    public Task StartAsync(CaptureStartRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.InterfaceName))
        {
            throw new CaptureException(
                "pcap source requires 'interfaceName'. Use opcclassic.capture.list_interfaces to discover names.");
        }

        _maxBytes = request.MaxBytes ?? kDefaultMaxBytes;
        _maxPackets = request.MaxPackets ?? long.MaxValue;
        int durationSeconds = request.MaxDurationSeconds ?? kDefaultMaxDurationSeconds;
        _maxDuration = TimeSpan.FromSeconds(durationSeconds);

        LibPcapLiveDevice? selected = ResolveDevice(request.InterfaceName!);

        bool promiscuous = request.Promiscuous;
        try
        {
            OpenDevice(selected, promiscuous);
        }
        catch (PcapException ex) when (promiscuous)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "PcapCaptureSource: promiscuous open failed on {Device}: {Reason}; retrying non-promiscuous.",
                    selected.Name, ex.Message);
            }
            OpenDevice(selected, promiscuous: false);
        }
        catch (PcapException ex)
        {
            throw new CaptureException(
                $"Failed to open '{selected.Name}' for capture: {ex.Message}. " +
                "On Windows the MCP server process needs Administrator + an installed Npcap; on Linux it needs root or CAP_NET_ADMIN/CAP_NET_RAW + a libpcap install.",
                ex);
        }

        string filter = string.IsNullOrWhiteSpace(request.BpfFilter)
            ? BuildServerPortBpfFilter(request.ServerPorts)
            : request.BpfFilter!;
        try
        {
            selected.Filter = filter;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
        {
            try { selected.Close(); } catch (PcapException) { /* tolerate cascading shutdown errors */ }
            throw new CaptureException(
                $"Invalid BPF filter '{filter}': {ex.Message}",
                ex);
        }

        _linkType = (int)selected.LinkType;
        _writer = new CaptureFileWriterDevice(_filePath, FileMode.Create);
        _writer.Open(new DeviceConfiguration { LinkLayerType = selected.LinkType });
        selected.OnPacketArrival += OnPacketArrival;
        _device = selected;
        _startedAt = DateTimeOffset.UtcNow;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "PcapCaptureSource: capturing on {Device} (LinkType={LinkType}) filter={Filter} maxBytes={MaxBytes} maxDurationSeconds={MaxDurationSeconds}",
                selected.Name, selected.LinkType, filter, _maxBytes, durationSeconds);
        }

#pragma warning disable CA1849 // SharpPcap exposes only the blocking StartCapture API.
        selected.StartCapture();
#pragma warning restore CA1849
        return Task.CompletedTask;
    }

    private static LibPcapLiveDevice ResolveDevice(string nameOrDescription)
    {
        LibPcapLiveDeviceList list = LibPcapLiveDeviceList.New();
        LibPcapLiveDevice? selected = null;
        foreach (LibPcapLiveDevice d in list)
        {
            if (selected is null
                && (string.Equals(d.Name, nameOrDescription, StringComparison.Ordinal)
                 || string.Equals(d.Description, nameOrDescription, StringComparison.Ordinal)))
            {
                selected = d;
            }
            else
            {
                d.Dispose();
            }
        }

        if (selected is null)
        {
            throw new CaptureException(
                $"Network interface '{nameOrDescription}' not found. Use opcclassic.capture.list_interfaces.");
        }

        return selected;
    }

    private static void OpenDevice(LibPcapLiveDevice device, bool promiscuous)
    {
        device.Open(
            mode: promiscuous ? DeviceModes.Promiscuous : DeviceModes.None,
            read_timeout: 1000);
    }

    private void OnPacketArrival(object sender, PacketCapture e)
    {
        if (_stopRequested)
        {
            return;
        }

        RawCapture pkt = e.GetPacket();
        int len = pkt.PacketLength;
        long packets = Interlocked.Increment(ref _packetCount);
        long bytes = Interlocked.Add(ref _byteCount, len);

        lock (_lock)
        {
            _writer?.Write(pkt);
        }

        if (bytes >= _maxBytes
            || packets >= _maxPackets
            || DateTimeOffset.UtcNow - _startedAt >= _maxDuration)
        {
            _stopRequested = true;
            // StopCapture is synchronous + blocking on the capture thread;
            // hand it off so we don't deadlock our own packet handler.
            _ = Task.Run(StopCaptureBackgroundAsync);
        }
    }

    private void StopCaptureBackgroundAsync()
    {
        try { _device?.StopCapture(); }
        catch (PcapException) { /* StopAsync handles final shutdown */ }
        catch (InvalidOperationException) { /* device already stopped */ }
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_lock)
        {
            if (_device != null)
            {
                try { _device.StopCapture(); }
                catch (PcapException) { /* tolerate already-stopped */ }
                catch (InvalidOperationException) { /* tolerate already-stopped */ }
                _device.OnPacketArrival -= OnPacketArrival;
                _device.Dispose();
                _device = null;
            }

            if (_writer != null)
            {
                try { _writer.Close(); }
                catch (PcapException) { /* tolerate already-closed */ }
                catch (InvalidOperationException) { /* tolerate already-closed */ }
                _writer.Dispose();
                _writer = null;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<CapturedPacket> ReadAllAsync(
        long? maxPackets,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            yield break;
        }

        long limit = maxPackets ?? long.MaxValue;
        long count = 0;
        using CaptureFileReaderDevice reader = new(_filePath);
        reader.Open();

        while (count < limit && !cancellationToken.IsCancellationRequested)
        {
            GetPacketStatus status = reader.GetNextPacket(out PacketCapture packetEvent);
            if (status != GetPacketStatus.PacketRead)
            {
                break;
            }

            RawCapture raw = packetEvent.GetPacket();
            byte[] data = raw.Data.ToArray();
            yield return new CapturedPacket(
                Timestamp: new DateTimeOffset(raw.Timeval.Date, TimeSpan.Zero),
                OriginalLength: raw.PacketLength,
                Data: data,
                LinkType: (int)raw.LinkLayerType,
                Annotations: kEmpty);
            count++;

            // Yield occasionally so a large file replay doesn't starve other awaits.
            if ((count & 0xFF) == 0)
            {
                await Task.Yield();
            }
        }
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Dispose path must release native pcap handle + writer regardless of error type.")]
    public async ValueTask DisposeAsync()
    {
        try
        {
            await StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch
        {
            // Suppress on dispose.
        }
    }

    private static readonly IReadOnlyDictionary<string, string?> kEmpty =
        new Dictionary<string, string?>(0);
}
