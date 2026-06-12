//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Opc.Classic.Diagnostics;

/// <summary>
/// <see cref="ICallChannel"/> decorator that writes each request / response
/// payload to a per-call hex dump under a configurable directory.
/// </summary>
/// <remarks>
/// File layout per call:
/// <code>
/// &lt;timestamp&gt;_&lt;sequence&gt;_&lt;tag&gt;_iid-&lt;iid&gt;_op-&lt;opnum&gt;.hex
/// </code>
/// Each file contains the request hex, the response HRESULT, and the response
/// hex separated by labelled banners so the file is self-describing when an
/// engineer opens it cold. Failures during write are swallowed (the
/// diagnostic must not change call semantics on failure).
/// </remarks>
public sealed class WireCapturingCallChannel : ICallChannel
{
    private readonly ICallChannel _inner;
    private readonly string _directory;
    private readonly string _contextTag;
    private long _sequence;

    /// <summary>
    /// Creates a new capturing decorator.
    /// </summary>
    public WireCapturingCallChannel(ICallChannel inner, string directory, string contextTag)
    {
        ArgumentNullException.ThrowIfNull(inner);
        ArgumentNullException.ThrowIfNull(directory);
        ArgumentNullException.ThrowIfNull(contextTag);

        _inner = inner;
        _directory = directory;
        _contextTag = SanitizeForFilename(contextTag);
    }

    /// <inheritdoc />
    public async Task<NdrCallResult> InvokeAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        long seq = Interlocked.Increment(ref _sequence);
        NdrCallResult result = await _inner.InvokeAsync(interfaceId, opnum, requestPayload, cancellationToken)
            .ConfigureAwait(false);
        TryCapture(seq, interfaceId, opnum, requestPayload, result);
        return result;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Capture is a diagnostic; failures must not propagate into call semantics.")]
    private void TryCapture(long sequence, Guid interfaceId, int opnum,
        ReadOnlyMemory<byte> requestPayload, NdrCallResult result)
    {
        try
        {
            Directory.CreateDirectory(_directory);
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddTHHmmss.fff", CultureInfo.InvariantCulture);
            string fileName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}_{1:D6}_{2}_iid-{3:N}_op-{4}.hex",
                timestamp, sequence, _contextTag, interfaceId, opnum);
            string path = Path.Combine(_directory, fileName);

            var sb = new StringBuilder(256 + (requestPayload.Length * 4) + (result.ResponsePayload.Length * 4));
            sb.Append("# Opc.Classic wire capture\n");
            sb.Append("# context: ").Append(_contextTag).Append('\n');
            sb.Append("# iid:     ").Append(interfaceId.ToString("D", CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("# opnum:   ").Append(opnum.ToString(CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("# hresult: 0x").Append(result.Hresult.ToString("X8", CultureInfo.InvariantCulture)).Append('\n');
            sb.Append("# timestamp_utc: ").Append(timestamp).Append('\n');
            sb.Append('\n');
            sb.Append("## request (").Append(requestPayload.Length.ToString(CultureInfo.InvariantCulture)).Append(" bytes)\n");
            AppendHexDump(sb, requestPayload.Span);
            sb.Append('\n');
            sb.Append("## response (").Append(result.ResponsePayload.Length.ToString(CultureInfo.InvariantCulture)).Append(" bytes)\n");
            AppendHexDump(sb, result.ResponsePayload.Span);

            File.WriteAllText(path, sb.ToString());
        }
        catch
        {
            // intentionally swallowed — diagnostic write must never alter call behavior.
        }
    }

    private static void AppendHexDump(StringBuilder sb, ReadOnlySpan<byte> bytes)
    {
        if (bytes.IsEmpty)
        {
            sb.Append("  (empty)\n");
            return;
        }

        const int RowBytes = 16;
        for (int row = 0; row < bytes.Length; row += RowBytes)
        {
            sb.Append(row.ToString("X4", CultureInfo.InvariantCulture)).Append(":  ");
            int rowEnd = Math.Min(row + RowBytes, bytes.Length);
            for (int col = row; col < row + RowBytes; col++)
            {
                if (col < rowEnd)
                {
                    sb.Append(bytes[col].ToString("x2", CultureInfo.InvariantCulture)).Append(' ');
                }
                else
                {
                    sb.Append("   ");
                }
                if (col == row + 7) { sb.Append(' '); }
            }
            sb.Append(' ');
            for (int col = row; col < rowEnd; col++)
            {
                byte b = bytes[col];
                sb.Append(b >= 0x20 && b < 0x7F ? (char)b : '.');
            }
            sb.Append('\n');
        }
    }

    private static string SanitizeForFilename(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            if (Array.IndexOf(invalid, c) >= 0 || c == ' ')
            {
                sb.Append('-');
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.Length == 0 ? "unknown" : sb.ToString();
    }
}
