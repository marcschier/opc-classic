//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Writes <see cref="DecodedOpcPdu"/> request/response pairs as
/// <c>.hex</c> files matching the format produced by
/// <c>Opc.Classic.Diagnostics.WireCapturingCallChannel</c>.
/// </summary>
/// <remarks>
/// <para>
/// File layout (one ORPC call per file):
/// </para>
/// <code>
/// # Opc.Classic wire capture
/// # context: live-&lt;sessionId&gt;
/// # iid: &lt;iid:D&gt;
/// # opnum: &lt;n&gt;
/// # hresult: 0x&lt;HRESULT:X8&gt;
/// # timestamp_utc: &lt;ISO 8601&gt;
/// # direction: bidir
///
/// ## request (N bytes)
/// 0000: aa bb cc ...
///
/// ## response (M bytes)
/// 0000: aa bb cc ...
/// </code>
/// <para>
/// Filename matches the live-capture convention so any existing
/// consumer (probe_servers.py replay, WireCaptureFile.Load) works
/// against capture-derived dumps unchanged.
/// </para>
/// <para>
/// Pairs request + response by <see cref="DecodedOpcPdu.CallId"/> +
/// flow direction. PDUs that don't have a matching pair (orphan
/// request or response) are written with the missing section empty —
/// the file is still self-describing.
/// </para>
/// </remarks>
public sealed class HexFormatBridge
{
    private readonly ILogger _logger;
    private readonly string _directory;
    private readonly string _contextTag;
    private long _sequence;

    public HexFormatBridge(string directory, string contextTag, ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(directory);
        ArgumentException.ThrowIfNullOrEmpty(contextTag);
        _directory = directory;
        _contextTag = SanitizeForFilename(contextTag);
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Writes a single matched request/response pair as a `.hex` file.
    /// Either side may be null when only one half of the call was
    /// captured.
    /// </summary>
    public string? Write(DecodedOpcPdu? request, ReadOnlyMemory<byte> requestStub,
        DecodedOpcPdu? response, ReadOnlyMemory<byte> responseStub)
    {
        DecodedOpcPdu? source = request ?? response;
        if (source is null)
        {
            return null;
        }

        long seq = System.Threading.Interlocked.Increment(ref _sequence);
        return TryCapture(seq, source, requestStub, response, responseStub);
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Hex-bridge writes are diagnostic; capture-time failures must not propagate.")]
    private string? TryCapture(long sequence, DecodedOpcPdu source,
        ReadOnlyMemory<byte> requestStub,
        DecodedOpcPdu? response, ReadOnlyMemory<byte> responseStub)
    {
        try
        {
            Directory.CreateDirectory(_directory);
            string timestamp = source.Timestamp.UtcDateTime.ToString("yyyyMMddTHHmmss.fff", CultureInfo.InvariantCulture);
            int opnum = source.Opnum ?? response?.Opnum ?? -1;
            Guid iid = source.InterfaceId ?? response?.InterfaceId ?? Guid.Empty;
            string fileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1:D6}_{2}_iid-{3:N}_op-{4}.hex",
                timestamp, sequence, _contextTag, iid, opnum);
            string path = Path.Combine(_directory, fileName);

            var sb = new StringBuilder(256 + (requestStub.Length * 4) + (responseStub.Length * 4));
            sb.Append("# Opc.Classic wire capture (from network packet capture)\n");
            sb.Append("# context: ").Append(_contextTag).Append('\n');
            sb.Append("# iid: ").Append(iid.ToString("D", CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("# opnum: ").Append(opnum.ToString(CultureInfo.InvariantCulture)).Append('\n');
            if (response?.Hresult is int hr)
            {
                sb.Append("# hresult: 0x").Append(hr.ToString("X8", CultureInfo.InvariantCulture)).Append('\n');
            }
            sb.Append("# timestamp_utc: ").Append(source.Timestamp.ToString("O", CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("# call_id: ").Append(source.CallId.ToString(CultureInfo.InvariantCulture)).Append('\n');
            if (source.ObjectIpid is Guid ipid && ipid != Guid.Empty)
            {
                sb.Append("# object_ipid: ").Append(ipid.ToString("D", CultureInfo.InvariantCulture)).Append('\n');
            }
            sb.Append('\n');

            AppendSection(sb, "request", requestStub.Span);
            sb.Append('\n');
            AppendSection(sb, "response", responseStub.Span);

            File.WriteAllText(path, sb.ToString());
            return path;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning(ex, "HexFormatBridge: failed to write capture file under {Directory}.", _directory);
            }
            return null;
        }
    }

    private static void AppendSection(StringBuilder sb, string label, ReadOnlySpan<byte> bytes)
    {
        sb.Append("## ").Append(label).Append(" (").Append(bytes.Length.ToString(CultureInfo.InvariantCulture)).Append(" bytes)\n");
        const int kRow = 16;
        for (int offset = 0; offset < bytes.Length; offset += kRow)
        {
            int take = Math.Min(kRow, bytes.Length - offset);
            sb.Append(offset.ToString("X4", CultureInfo.InvariantCulture)).Append(": ");
            for (int j = 0; j < take; j++)
            {
                sb.Append(bytes[offset + j].ToString("x2", CultureInfo.InvariantCulture)).Append(' ');
            }
            sb.Append('\n');
        }
    }

    private static string SanitizeForFilename(string tag)
    {
        var sb = new StringBuilder(tag.Length);
        foreach (char c in tag)
        {
            sb.Append(char.IsLetterOrDigit(c) || c == '-' || c == '_' || c == '.' ? c : '-');
        }
        return sb.ToString();
    }
}
