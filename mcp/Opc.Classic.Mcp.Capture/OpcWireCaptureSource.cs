//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Replays a directory of <c>.hex</c> files emitted by
/// <c>Opc.Classic.Diagnostics.WireCapturingCallChannel</c> as if they
/// were live capture frames.
/// </summary>
/// <remarks>
/// <para>
/// Each <c>.hex</c> file is one ORPC call pair (request + response)
/// with a banner of <c># key: value</c> metadata. This source yields
/// two <see cref="CapturedPacket"/> records per file:
/// </para>
/// <list type="number">
///   <item><description>The request payload, annotated with <c>direction=request</c> + iid + opnum + the file's metadata.</description></item>
///   <item><description>The response payload, annotated with <c>direction=response</c> + hresult + the same iid/opnum.</description></item>
/// </list>
/// <para>
/// <see cref="CapturedPacket.LinkType"/> is 0 (no link layer) — the
/// downstream decoder treats records from this source as ORPC bodies
/// directly, skipping the TCP reassembly step that
/// <see cref="PcapCaptureSource"/> output requires.
/// </para>
/// </remarks>
public sealed class OpcWireCaptureSource : ICaptureSource
{
    /// <summary>
    /// Stable source name surfaced via the MCP info DTO.
    /// </summary>
    public const string SourceName = "wirecapture";

    private static readonly Regex s_bannerLine = new(
        @"^#\s*([A-Za-z_][A-Za-z0-9_]*)\s*:\s*(.+?)\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex s_hexRow = new(
        @"^[0-9A-Fa-f]{4}:\s+((?:[0-9A-Fa-f]{2}\s+)+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ILogger _logger;
    private long _packetCount;
    private long _byteCount;
    private string? _replayDirectory;
    private List<string>? _files;
    private bool _started;

    public OpcWireCaptureSource(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <inheritdoc/>
    public long PacketCount => Interlocked.Read(ref _packetCount);

    /// <inheritdoc/>
    public long ByteCount => Interlocked.Read(ref _byteCount);

    /// <inheritdoc/>
    public int LinkType => 0;

    /// <inheritdoc/>
    public string? GetRawPcapFilePath() => null;

    /// <inheritdoc/>
    public Task StartAsync(CaptureStartRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.ReplaySourceDirectory))
        {
            throw new CaptureException(
                "wirecapture source requires 'replaySourceDirectory' pointing at a folder of .hex files produced by OpcWireCapture.");
        }

        if (!Directory.Exists(request.ReplaySourceDirectory))
        {
            throw new CaptureException(
                $"Replay directory '{request.ReplaySourceDirectory}' does not exist.");
        }

        _replayDirectory = request.ReplaySourceDirectory!;
        _files = Directory.EnumerateFiles(_replayDirectory, "*.hex", SearchOption.AllDirectories)
            .OrderBy(static p => p, StringComparer.Ordinal)
            .ToList();

        // Count synthetic frames + bytes up-front so the MCP info DTO reflects
        // accurate values immediately (no live arrival to track).
        long packets = 0;
        long bytes = 0;
        foreach (string path in _files)
        {
            try
            {
                ParsedHexFile parsed = ParseHexFile(path);
                if (parsed.RequestBytes.Count > 0)
                {
                    packets++;
                    bytes += parsed.RequestBytes.Count;
                }
                if (parsed.ResponseBytes.Count > 0)
                {
                    packets++;
                    bytes += parsed.ResponseBytes.Count;
                }
            }
            catch (Exception ex) when (ex is IOException or FormatException or UnauthorizedAccessException)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(ex,
                        "OpcWireCaptureSource: failed to pre-parse {File}; counted as zero.", path);
                }
            }
        }

        Interlocked.Exchange(ref _packetCount, packets);
        Interlocked.Exchange(ref _byteCount, bytes);
        _started = true;

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "OpcWireCaptureSource: pre-scanned {Files} .hex files ({Packets} synthetic frames, {Bytes} bytes) in {Directory}",
                _files.Count, packets, bytes, _replayDirectory);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<CapturedPacket> ReadAllAsync(
        long? maxPackets,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (!_started || _files is null)
        {
            yield break;
        }

        long limit = maxPackets ?? long.MaxValue;
        long emitted = 0;

        foreach (string path in _files)
        {
            if (emitted >= limit || cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            ParsedHexFile parsed;
            try
            {
                parsed = ParseHexFile(path);
            }
            catch (Exception ex) when (ex is IOException or FormatException or UnauthorizedAccessException)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                {
                    _logger.LogWarning(ex,
                        "OpcWireCaptureSource: skipping malformed file {File}.", path);
                }
                continue;
            }

            DateTimeOffset ts = ExtractTimestamp(parsed.Metadata, path);

            if (parsed.RequestBytes.Count > 0 && emitted < limit)
            {
                Dictionary<string, string?> annotations = ToAnnotations(parsed.Metadata, path, direction: "request");
                yield return new CapturedPacket(
                    Timestamp: ts,
                    OriginalLength: parsed.RequestBytes.Count,
                    Data: parsed.RequestBytes.ToArray(),
                    LinkType: 0,
                    Annotations: annotations);
                emitted++;
            }

            if (parsed.ResponseBytes.Count > 0 && emitted < limit)
            {
                Dictionary<string, string?> annotations = ToAnnotations(parsed.Metadata, path, direction: "response");
                yield return new CapturedPacket(
                    Timestamp: ts,
                    OriginalLength: parsed.ResponseBytes.Count,
                    Data: parsed.ResponseBytes.ToArray(),
                    LinkType: 0,
                    Annotations: annotations);
                emitted++;
            }

            if ((emitted & 0xFF) == 0)
            {
                await Task.Yield();
            }
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _files = null;
        return ValueTask.CompletedTask;
    }

    private static ParsedHexFile ParseHexFile(string path)
    {
        string text = File.ReadAllText(path);
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var requestBytes = new List<byte>(256);
        var responseBytes = new List<byte>(256);
        List<byte>? currentSection = null;

        foreach (string rawLine in text.Split('\n'))
        {
            string line = rawLine.TrimEnd('\r');
            if (line.StartsWith("## request", StringComparison.Ordinal))
            {
                currentSection = requestBytes;
                continue;
            }
            if (line.StartsWith("## response", StringComparison.Ordinal))
            {
                currentSection = responseBytes;
                continue;
            }
            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                Match banner = s_bannerLine.Match(line);
                if (banner.Success)
                {
                    metadata[banner.Groups[1].Value] = banner.Groups[2].Value;
                }
                continue;
            }
            if (currentSection is null || line.Length == 0)
            {
                continue;
            }
            Match row = s_hexRow.Match(line);
            if (!row.Success)
            {
                continue;
            }
            foreach (string token in row.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (token.Length != 2)
                {
                    continue;
                }
                currentSection.Add(byte.Parse(token, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
            }
        }

        return new ParsedHexFile(metadata, requestBytes, responseBytes);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Timestamp fallback is a diagnostic; any filesystem error falls back to UtcNow.")]
    private static DateTimeOffset ExtractTimestamp(IReadOnlyDictionary<string, string> metadata, string filePath)
    {
        if (metadata.TryGetValue("timestamp_utc", out string? value)
            && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset parsed))
        {
            return parsed;
        }

        // Fall back to the file's last-write time so ordered .hex dumps
        // retain relative time information even when the banner is absent.
        try
        {
            return new DateTimeOffset(File.GetLastWriteTimeUtc(filePath), TimeSpan.Zero);
        }
        catch
        {
            return DateTimeOffset.UtcNow;
        }
    }

    private static Dictionary<string, string?> ToAnnotations(
        IReadOnlyDictionary<string, string> metadata,
        string filePath,
        string direction)
    {
        var annotations = new Dictionary<string, string?>(metadata.Count + 2, StringComparer.OrdinalIgnoreCase)
        {
            ["direction"] = direction,
            ["source_file"] = Path.GetFileName(filePath),
        };
        foreach (KeyValuePair<string, string> entry in metadata)
        {
            annotations[entry.Key] = entry.Value;
        }
        return annotations;
    }

    private sealed record class ParsedHexFile(
        IReadOnlyDictionary<string, string> Metadata,
        List<byte> RequestBytes,
        List<byte> ResponseBytes);
}
