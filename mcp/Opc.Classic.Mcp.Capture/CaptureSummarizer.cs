// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Roll-up summary of a completed capture, built by walking the
/// <see cref="DecodedOpcPdu"/> stream from
/// <see cref="OpcDcomDecoder.DecodeAll"/>. Backs
/// <c>opcclassic.capture.summarize</c>.
/// </summary>
public sealed record class CaptureSummary
{
    public required string SessionId { get; init; }
    public required long PduCount { get; init; }
    public required double DurationSeconds { get; init; }
    public IReadOnlyList<TopEntry> TopPduTypes { get; init; } = Array.Empty<TopEntry>();
    public IReadOnlyList<TopEntry> TopSources { get; init; } = Array.Empty<TopEntry>();
    public IReadOnlyList<TopEntry> TopDestinations { get; init; } = Array.Empty<TopEntry>();
    public IReadOnlyList<TopEntry> TopInterfaces { get; init; } = Array.Empty<TopEntry>();
    public IReadOnlyList<TopEntry> TopOpnums { get; init; } = Array.Empty<TopEntry>();
    public IReadOnlyList<TopEntry> TopIpids { get; init; } = Array.Empty<TopEntry>();
    public IReadOnlyList<TopEntry> TopFaultCodes { get; init; } = Array.Empty<TopEntry>();
    public IReadOnlyList<TopEntry> TopBindRejectReasons { get; init; } = Array.Empty<TopEntry>();
}

/// <summary>
/// One bucket in a top-N roll-up.
/// </summary>
public sealed record class TopEntry(string Key, long Count);

/// <summary>
/// Builds <see cref="CaptureSummary"/> from a decoded-PDU stream.
/// </summary>
public static class CaptureSummarizer
{
    private const int kDefaultTop = 10;

    /// <summary>
    /// Build a summary; top-N defaults to 10 entries per category.
    /// </summary>
    public static CaptureSummary Summarize(string sessionId, IEnumerable<DecodedOpcPdu> pdus, int top = kDefaultTop)
    {
        ArgumentException.ThrowIfNullOrEmpty(sessionId);
        ArgumentNullException.ThrowIfNull(pdus);
        ArgumentOutOfRangeException.ThrowIfLessThan(top, 1);

        var pduTypes = new Dictionary<string, long>(StringComparer.Ordinal);
        var sources = new Dictionary<string, long>(StringComparer.Ordinal);
        var destinations = new Dictionary<string, long>(StringComparer.Ordinal);
        var interfaces = new Dictionary<string, long>(StringComparer.Ordinal);
        var opnums = new Dictionary<string, long>(StringComparer.Ordinal);
        var ipids = new Dictionary<string, long>(StringComparer.Ordinal);
        var faults = new Dictionary<string, long>(StringComparer.Ordinal);
        var rejects = new Dictionary<string, long>(StringComparer.Ordinal);

        long count = 0;
        DateTimeOffset? min = null;
        DateTimeOffset? max = null;

        foreach (DecodedOpcPdu pdu in pdus)
        {
            count++;
            Increment(pduTypes, pdu.PduType);
            if (pdu.SourceEndpoint is { Length: > 0 })
            {
                Increment(sources, pdu.SourceEndpoint);
            }

            if (pdu.DestinationEndpoint is { Length: > 0 })
            {
                Increment(destinations, pdu.DestinationEndpoint);
            }

            if (pdu.InterfaceId is Guid iid && iid != Guid.Empty)
            {
                Increment(interfaces, iid.ToString("D", CultureInfo.InvariantCulture));
            }

            if (pdu is { PduType: "request", InterfaceId: Guid riid, Opnum: int opnum })
            {
                string label = string.Create(CultureInfo.InvariantCulture, $"{riid:D}/op{opnum}");
                Increment(opnums, label);
            }
            if (pdu.ObjectIpid is Guid ipid && ipid != Guid.Empty)
            {
                Increment(ipids, ipid.ToString("D", CultureInfo.InvariantCulture));
            }

            if (pdu.FaultStatus is int fault)
            {
                Increment(faults, "0x" + fault.ToString("X8", CultureInfo.InvariantCulture));
            }

            foreach (PresentationResultInfo r in pdu.ResultList)
            {
                if (!string.Equals(r.Result, "ACCEPTANCE", StringComparison.Ordinal))
                {
                    string label = string.Create(CultureInfo.InvariantCulture, $"{r.Result};{r.Reason}");
                    Increment(rejects, label);
                }
            }

            if (min is null || pdu.Timestamp < min)
            {
                min = pdu.Timestamp;
            }

            if (max is null || pdu.Timestamp > max)
            {
                max = pdu.Timestamp;
            }
        }

        double durationSeconds = (min is null || max is null) ? 0.0 : (max.Value - min.Value).TotalSeconds;

        return new CaptureSummary
        {
            SessionId = sessionId,
            PduCount = count,
            DurationSeconds = durationSeconds,
            TopPduTypes = TopN(pduTypes, top),
            TopSources = TopN(sources, top),
            TopDestinations = TopN(destinations, top),
            TopInterfaces = TopN(interfaces, top),
            TopOpnums = TopN(opnums, top),
            TopIpids = TopN(ipids, top),
            TopFaultCodes = TopN(faults, top),
            TopBindRejectReasons = TopN(rejects, top),
        };
    }

    private static void Increment(Dictionary<string, long> map, string key)
    {
        if (map.TryGetValue(key, out long count))
        {
            map[key] = count + 1;
            return;
        }
        map[key] = 1;
    }

    private static IReadOnlyList<TopEntry> TopN(Dictionary<string, long> map, int top)
    {
        if (map.Count == 0)
        {
            return Array.Empty<TopEntry>();
        }
        return map
            .OrderByDescending(static kvp => kvp.Value)
            .ThenBy(static kvp => kvp.Key, StringComparer.Ordinal)
            .Take(top)
            .Select(static kvp => new TopEntry(kvp.Key, kvp.Value))
            .ToArray();
    }
}
