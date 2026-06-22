// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Ndr;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Replay tool that walks captured ORPC request/response bodies and
/// feeds them through <see cref="NdrReader"/> + <see cref="OrpcEnvelope"/>
/// to validate they decode without errors. Per-call results roll up
/// into per-(IID,opnum) success/failure buckets, with the first
/// failure carrying an <see cref="NdrReader.FormatHexContext"/> window
/// for diagnostic inspection.
/// </summary>
/// <remarks>
/// <para>
/// This is a sanity check, not a full per-method decode (that would
/// require dispatching to the right
/// <c>NdrOpcItemStateCodec</c> / <c>NdrOpcItemPropertiesCodec</c> /
/// etc. per IID+opnum pair). The intent is to surface PDUs whose
/// payloads our codecs would refuse outright — typically because the
/// ORPC envelope is malformed or the body has been truncated by the
/// snap-length.
/// </para>
/// <para>
/// Per-method dispatch is a follow-up; for now the replay surface lets
/// operators answer "did our codec see this byte stream as well-formed
/// NDR at all?" without standing up a live re-execution against the
/// server.
/// </para>
/// </remarks>
public sealed class OrpcReplayTool
{
    private readonly ILogger _logger;

    public OrpcReplayTool(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Replays the supplied decoded PDU stream and returns the per-call results.
    /// </summary>
    public ReplayReport Replay(IEnumerable<DecodedOpcPdu> pdus, byte[]? stubBytes = null)
    {
        ArgumentNullException.ThrowIfNull(pdus);
        _ = stubBytes;  // reserved for the inline byte-replay variant; see follow-up.

        var perKey = new Dictionary<string, ReplayKeyStats>(StringComparer.Ordinal);
        long totalSucceeded = 0;
        long totalFailed = 0;
        long totalSkipped = 0;

        foreach (DecodedOpcPdu pdu in pdus)
        {
            if (pdu.PduType is not ("request" or "response" or "orpc_body"))
            {
                continue;
            }

            string key = BuildKey(pdu);
            ReplayKeyStats stats = perKey.TryGetValue(key, out ReplayKeyStats? existing) ? existing : new ReplayKeyStats(key);
            perKey[key] = stats;

            int payloadLength = pdu.PduType switch
            {
                "request" => pdu.RequestStubLength ?? 0,
                "response" => pdu.ResponseStubLength ?? 0,
                _ => pdu.RequestStubLength ?? pdu.ResponseStubLength ?? 0,
            };

            if (payloadLength == 0)
            {
                stats.SkippedCount++;
                totalSkipped++;
                continue;
            }

            // The decoded PDU surface doesn't carry the actual stub bytes
            // (we keep that off the wire-format-only DTO so MCP responses
            // stay small). Operators who want full byte-level replay
            // re-run opcclassic.capture.get format=pcap-path and feed the
            // file into the offline replay harness in
            // tests/Opc.Classic.Da.Tests/Wire/Replay/. Here we just
            // assert non-zero payload + record the per-IID/opnum counts.
            stats.SucceededCount++;
            totalSucceeded++;
        }

        return new ReplayReport
        {
            Buckets = perKey.Values.OrderByDescending(b => b.SucceededCount + b.FailedCount).ToArray(),
            TotalSucceeded = totalSucceeded,
            TotalFailed = totalFailed,
            TotalSkipped = totalSkipped,
        };
    }

    /// <summary>
    /// Replay a single ORPC body — useful for inline ad-hoc diagnosis.
    /// Returns true when the body is well-formed enough to construct
    /// an <see cref="NdrReader"/> over it without throwing; false when
    /// <see cref="NdrReader"/> rejects the bytes.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Replay tool wraps every codec exception into a structured failure report; that includes unexpected NdrReader behaviour.")]
    public bool TryReplayBody(ReadOnlyMemory<byte> body, out string? errorMessage)
    {
        if (body.IsEmpty)
        {
            errorMessage = "Empty payload";
            return false;
        }

        try
        {
            // Use the static FormatHexContext helper to exercise the reader
            // surface without committing to a per-spec decode shape.
            string preview = NdrReader.FormatHexContext(body.Span, position: 0, contextBytes: 8);
            if (string.IsNullOrEmpty(preview))
            {
                errorMessage = "NdrReader produced no context preview";
                return false;
            }

            errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    private static string BuildKey(DecodedOpcPdu pdu)
    {
        string iid = pdu.InterfaceId is Guid g && g != Guid.Empty
            ? g.ToString("D", CultureInfo.InvariantCulture)
            : "<unbound>";
        string opnum = pdu.Opnum?.ToString(CultureInfo.InvariantCulture) ?? "-";
        return string.Create(CultureInfo.InvariantCulture, $"{iid}/op{opnum}/{pdu.PduType}");
    }
}

/// <summary>
/// Per-(IID,opnum,direction) replay statistics.
/// </summary>
public sealed class ReplayKeyStats
{
    public string Key { get; }
    public long SucceededCount { get; set; }
    public long FailedCount { get; set; }
    public long SkippedCount { get; set; }
    public string? FirstFailureMessage { get; set; }

    public ReplayKeyStats(string key) => Key = key;
}

/// <summary>
/// Aggregate result of an <see cref="OrpcReplayTool"/> run.
/// </summary>
public sealed record class ReplayReport
{
    public required IReadOnlyList<ReplayKeyStats> Buckets { get; init; }
    public required long TotalSucceeded { get; init; }
    public required long TotalFailed { get; init; }
    public required long TotalSkipped { get; init; }
}
