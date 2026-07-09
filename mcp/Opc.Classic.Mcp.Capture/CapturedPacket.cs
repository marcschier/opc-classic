// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// One captured network frame (or higher-level record from a
/// non-link-layer source). Immutable.
/// </summary>
/// <param name="Timestamp">Wall-clock UTC time the frame was captured.</param>
/// <param name="OriginalLength">Original on-wire length in bytes (may exceed <paramref name="Data"/>.Length when snapped).</param>
/// <param name="Data">Captured bytes — empty for sources that don't produce link-layer frames.</param>
/// <param name="LinkType">Pcap-style link-layer type identifier (LinkLayers per SharpPcap).</param>
/// <param name="Annotations">Source-specific metadata (e.g. remote endpoint, source/dest IPs) for sources that decode beyond the link layer.</param>
public sealed record class CapturedPacket(
    DateTimeOffset Timestamp,
    int OriginalLength,
    ReadOnlyMemory<byte> Data,
    int LinkType,
    IReadOnlyDictionary<string, string?> Annotations);
