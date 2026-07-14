// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Ndr;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Replays captured DCE/RPC frames through <see cref="PduCodec"/> and validates
/// request/response ORPC envelopes before rolling results up by IID/opnum.
/// </summary>
public sealed class OrpcReplayTool
{
    private readonly ILogger _logger;

    public OrpcReplayTool(ILogger? logger = null)
    {
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>
    /// Compatibility surface for callers that only retained public PDU DTOs.
    /// Such records do not contain raw bytes and are therefore reported as
    /// skipped rather than being treated as validated.
    /// </summary>
    public ReplayReport Replay(
        IEnumerable<DecodedOpcPdu> pdus,
        byte[]? stubBytes = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pdus);
        _ = stubBytes;
        IEnumerable<DecodedDcomFrame> frames = pdus.Select(
            pdu => new DecodedDcomFrame(pdu, pdu.PduType, RawFrame: null, StubBytes: null, Failure: null));
        return ReplayDetailed(frames, cancellationToken);
    }

    /// <summary>
    /// Byte-level replay used by the MCP capture tool.
    /// </summary>
    internal ReplayReport ReplayDetailed(
        IEnumerable<DecodedDcomFrame> frames,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(frames);

        var perKey = new Dictionary<string, ReplayKeyStats>(StringComparer.Ordinal);
        var fragmentedCalls = new Dictionary<FragmentKey, FragmentAccumulator>();
        long totalSucceeded = 0;
        long totalFailed = 0;
        long totalSkipped = 0;

        foreach (DecodedDcomFrame frame in frames)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (frame.Failure is not null)
            {
                ReplayKeyStats failureStats = GetOrAddStats(perKey, BuildKey(frame));
                RecordFailure(failureStats, frame.Failure.Message, frame.RawFrame);
                totalFailed++;
                continue;
            }

            if (frame.PduType is not ("request" or "response" or "fault" or "orpc_body"))
            {
                continue;
            }

            ReplayKeyStats stats = GetOrAddStats(perKey, BuildKey(frame));

            if (frame.RawFrame is null)
            {
                stats.SkippedCount++;
                totalSkipped++;
                continue;
            }

            if (!TryDecodeFragment(
                    frame.RawFrame,
                    cancellationToken,
                    out ConnectionOrientedPdu? decoded,
                    out string? failure,
                    out byte[]? contextBytes))
            {
                RecordFailure(stats, failure ?? "Replay framing/PDU validation failed.", contextBytes ?? frame.RawFrame);
                totalFailed++;
                continue;
            }

            bool first = decoded.GetFlag(ConnectionOrientedPdu.PFC_FIRST_FRAG);
            bool last = decoded.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG);
            if (first && last)
            {
                if (TryValidateCompletePdu(decoded, cancellationToken, out failure, out contextBytes))
                {
                    stats.SucceededCount++;
                    totalSucceeded++;
                }
                else
                {
                    RecordFailure(stats, failure ?? "ORPC validation failed.", contextBytes ?? frame.RawFrame);
                    totalFailed++;
                }
                continue;
            }

            var fragmentKey = new FragmentKey(
                frame.Pdu?.SourceEndpoint ?? string.Empty,
                frame.Pdu?.DestinationEndpoint ?? string.Empty,
                decoded.CallId,
                frame.PduType);

            if (first)
            {
                if (fragmentedCalls.Remove(fragmentKey, out FragmentAccumulator? abandoned))
                {
                    RecordFailure(
                        abandoned.Stats,
                        "A new FIRST fragment arrived before the previous call completed.",
                        abandoned.FirstRawFrame);
                    totalFailed++;
                }
                fragmentedCalls[fragmentKey] = new FragmentAccumulator(stats, decoded, frame.RawFrame);
            }
            else
            {
                if (!fragmentedCalls.TryGetValue(fragmentKey, out FragmentAccumulator? accumulator))
                {
                    RecordFailure(stats, "Fragment arrived without a matching FIRST fragment.", frame.RawFrame);
                    totalFailed++;
                    continue;
                }
                accumulator.Fragments.Add(decoded);
            }

            if (!last)
            {
                continue;
            }

            FragmentAccumulator completed = fragmentedCalls[fragmentKey];
            fragmentedCalls.Remove(fragmentKey);
            if (TryReassembleAndValidate(completed, cancellationToken, out failure, out contextBytes))
            {
                completed.Stats.SucceededCount++;
                totalSucceeded++;
            }
            else
            {
                RecordFailure(
                    completed.Stats,
                    failure ?? "Fragment reassembly/ORPC validation failed.",
                    contextBytes ?? completed.FirstRawFrame);
                totalFailed++;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        foreach (FragmentAccumulator incomplete in fragmentedCalls.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            RecordFailure(
                incomplete.Stats,
                "Fragment sequence ended before a LAST fragment arrived.",
                incomplete.FirstRawFrame);
            totalFailed++;
        }
        cancellationToken.ThrowIfCancellationRequested();

        return new ReplayReport
        {
            Buckets = perKey.Values
                .Where(b => b.SucceededCount + b.FailedCount + b.SkippedCount > 0)
                .OrderByDescending(b => b.SucceededCount + b.FailedCount + b.SkippedCount)
                .ThenBy(b => b.Key, StringComparer.Ordinal)
                .ToArray(),
            TotalSucceeded = totalSucceeded,
            TotalFailed = totalFailed,
            TotalSkipped = totalSkipped,
        };
    }

    /// <summary>
    /// Validates an ad-hoc ORPC stub as either ORPC_THIS or ORPC_THAT.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Replay converts codec exceptions into structured diagnostic failures.")]
    public bool TryReplayBody(ReadOnlyMemory<byte> body, out string? errorMessage)
    {
        if (body.IsEmpty)
        {
            errorMessage = "Empty payload";
            return false;
        }

        try
        {
            ValidateRequestEnvelope(body.ToArray());
            errorMessage = null;
            return true;
        }
        catch (Exception requestException)
        {
            try
            {
                ValidateResponseEnvelope(body.ToArray());
                errorMessage = null;
                return true;
            }
            catch (Exception responseException)
            {
                errorMessage = $"ORPC_THIS: {requestException.Message}; ORPC_THAT: {responseException.Message}";
                return false;
            }
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Replay converts all framing/PDU/ORPC codec exceptions into structured failure buckets.")]
    private bool TryDecodeFragment(
        byte[] frame,
        CancellationToken cancellationToken,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ConnectionOrientedPdu? pdu,
        out string? failure,
        out byte[]? contextBytes)
    {
        pdu = null;
        contextBytes = frame;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (frame.Length < ConnectionOrientedPdu.HEADER_LENGTH)
            {
                throw new InvalidOperationException(
                    $"DCE/RPC frame is {frame.Length} bytes; expected at least {ConnectionOrientedPdu.HEADER_LENGTH}.");
            }

            int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
                frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2));
            if (fragmentLength != frame.Length)
            {
                throw new InvalidOperationException(
                    $"DCE/RPC frag_length mismatch: header={fragmentLength}, available={frame.Length}.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            pdu = PduCodec.DecodePdu(frame.ToArray());
            cancellationToken.ThrowIfCancellationRequested();
            failure = null;
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(ex, "ORPC replay rejected a {Bytes}-byte frame.", frame.Length);
            }
            failure = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Replay converts ORPC codec exceptions into structured failure buckets.")]
    private bool TryValidateCompletePdu(
        ConnectionOrientedPdu pdu,
        CancellationToken cancellationToken,
        out string? failure,
        out byte[]? contextBytes)
    {
        contextBytes = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            switch (pdu)
            {
                case RequestCoPdu request:
                    contextBytes = request.Stub;
                    ValidateRequestEnvelope(request.Stub);
                    break;
                case ResponseCoPdu response:
                    contextBytes = response.Stub;
                    ValidateResponseEnvelope(response.Stub);
                    break;
                case FaultCoPdu fault:
                    contextBytes = fault.Stub;
                    break;
                default:
                    throw new InvalidOperationException($"Replay expected request/response/fault, got PDU type {pdu.Type}.");
            }
            cancellationToken.ThrowIfCancellationRequested();
            failure = null;
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(ex, "ORPC replay rejected a reassembled PDU.");
            }
            failure = ex.GetType().Name + ": " + ex.Message;
            return false;
        }
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Replay converts fragment reassembly exceptions into structured failure buckets.")]
    private bool TryReassembleAndValidate(
        FragmentAccumulator accumulator,
        CancellationToken cancellationToken,
        out string? failure,
        out byte[]? contextBytes)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (accumulator.Fragments[0] is not IFragmentable fragmentable)
            {
                throw new InvalidOperationException("Fragmented PDU type does not support reassembly.");
            }

            ValidateFragmentMetadata(accumulator.Fragments);
            cancellationToken.ThrowIfCancellationRequested();
            ConnectionOrientedPdu reassembled = fragmentable.Reassemble(accumulator.Fragments);
            cancellationToken.ThrowIfCancellationRequested();
            return TryValidateCompletePdu(reassembled, cancellationToken, out failure, out contextBytes);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            failure = ex.GetType().Name + ": " + ex.Message;
            contextBytes = accumulator.FirstRawFrame;
            return false;
        }
    }

    private static void ValidateFragmentMetadata(IReadOnlyList<ConnectionOrientedPdu> fragments)
    {
        ConnectionOrientedPdu first = fragments[0];
        if (!first.GetFlag(ConnectionOrientedPdu.PFC_FIRST_FRAG))
        {
            throw new InvalidOperationException("Fragment sequence does not begin with FIRST.");
        }

        int totalStubLength = fragments.Sum(GetStubLength);
        int remainingStubLength = totalStubLength;
        for (int i = 0; i < fragments.Count; i++)
        {
            ConnectionOrientedPdu fragment = fragments[i];
            bool isFirst = i == 0;
            bool isLast = i == fragments.Count - 1;
            if (fragment.Type != first.Type || fragment.CallId != first.CallId)
            {
                throw new InvalidOperationException("Fragment type/call id changed within a sequence.");
            }
            if (fragment.GetFlag(ConnectionOrientedPdu.PFC_FIRST_FRAG) != isFirst
                || fragment.GetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG) != isLast)
            {
                throw new InvalidOperationException("Fragment FIRST/LAST flags are inconsistent with sequence order.");
            }
            if (GetContextId(fragment) != GetContextId(first))
            {
                throw new InvalidOperationException("Fragment context id changed within a sequence.");
            }
            int allocationHint = GetAllocationHint(fragment);
            if (allocationHint != 0 && allocationHint != remainingStubLength)
            {
                string prefix = isFirst ? "First fragment" : "Fragment";
                throw new InvalidOperationException(
                    $"{prefix} allocation hint {allocationHint} does not match remaining stub length {remainingStubLength}.");
            }

            if (first is RequestCoPdu firstRequest && fragment is RequestCoPdu request)
            {
                if (request.Opnum != firstRequest.Opnum)
                {
                    throw new InvalidOperationException("Request opnum changed within a fragment sequence.");
                }
                if (!string.Equals(
                        request.Object?.ToString(),
                        firstRequest.Object?.ToString(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Request object UUID changed within a fragment sequence.");
                }
            }

            remainingStubLength -= GetStubLength(fragment);
        }
    }

    private static int GetAllocationHint(ConnectionOrientedPdu pdu)
        => pdu switch
        {
            RequestCoPdu request => request.AllocationHint,
            ResponseCoPdu response => response.AllocationHint,
            FaultCoPdu fault => fault.AllocationHint,
            _ => throw new InvalidOperationException($"PDU type {pdu.Type} has no allocation hint."),
        };

    private static int GetContextId(ConnectionOrientedPdu pdu)
        => pdu switch
        {
            RequestCoPdu request => request.ContextId,
            ResponseCoPdu response => response.ContextId,
            FaultCoPdu fault => fault.ContextId,
            _ => throw new InvalidOperationException($"PDU type {pdu.Type} has no presentation context."),
        };

    private static int GetStubLength(ConnectionOrientedPdu pdu)
        => pdu switch
        {
            RequestCoPdu request => request.Stub?.Length ?? 0,
            ResponseCoPdu response => response.Stub?.Length ?? 0,
            FaultCoPdu fault => fault.Stub?.Length ?? 0,
            _ => 0,
        };

    private static void ValidateRequestEnvelope(byte[] stub)
    {
        ArgumentNullException.ThrowIfNull(stub);
        var reader = new NdrReader(stub);
        _ = OrpcThis.Read(ref reader);
    }

    private static void ValidateResponseEnvelope(byte[] stub)
    {
        ArgumentNullException.ThrowIfNull(stub);
        var reader = new NdrReader(stub);
        _ = OrpcThat.Read(ref reader);
    }

    private static void RecordFailure(ReplayKeyStats stats, string message, byte[]? contextBytes)
    {
        stats.FailedCount++;
        if (stats.FirstFailureMessage is not null)
        {
            return;
        }

        stats.FirstFailureMessage = message;
        stats.FirstFailureHexContext = contextBytes is { Length: > 0 }
            ? NdrReader.FormatHexContext(contextBytes, position: 0, contextBytes: Math.Min(16, contextBytes.Length))
            : "<empty>";
    }

    private static string BuildKey(DecodedDcomFrame frame)
    {
        if (frame.Pdu is null)
        {
            return "<unknown>";
        }

        Guid? interfaceId = frame.Pdu?.InterfaceId;
        string iid = interfaceId is Guid g && g != Guid.Empty
            ? g.ToString("D", CultureInfo.InvariantCulture)
            : "<unbound>";
        string opnum = frame.Pdu?.Opnum?.ToString(CultureInfo.InvariantCulture) ?? "-";
        return string.Create(CultureInfo.InvariantCulture, $"{iid}/op{opnum}/{frame.PduType}");
    }

    private static ReplayKeyStats GetOrAddStats(
        Dictionary<string, ReplayKeyStats> perKey,
        string key)
    {
        if (!perKey.TryGetValue(key, out ReplayKeyStats? stats))
        {
            stats = new ReplayKeyStats(key);
            perKey[key] = stats;
        }
        return stats;
    }

    private readonly record struct FragmentKey(
        string SourceEndpoint,
        string DestinationEndpoint,
        int CallId,
        string PduType);

    private sealed class FragmentAccumulator
    {
        public FragmentAccumulator(
            ReplayKeyStats stats,
            ConnectionOrientedPdu firstFragment,
            byte[] firstRawFrame)
        {
            Stats = stats;
            Fragments.Add(firstFragment);
            FirstRawFrame = firstRawFrame;
        }

        public ReplayKeyStats Stats { get; }
        public List<ConnectionOrientedPdu> Fragments { get; } = [];
        public byte[] FirstRawFrame { get; }
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
    public string? FirstFailureHexContext { get; set; }

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
