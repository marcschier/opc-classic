//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Abstraction for a network trace source. Implementations buffer
/// captured frames to disk so completed captures can be replayed by
/// formatters or by <c>opcclassic.capture.replay</c>.
/// </summary>
/// <remarks>
/// Lifecycle: <see cref="StartAsync"/> opens the source, frames flow
/// into source-internal storage, <see cref="StopAsync"/> flushes, then
/// <see cref="ReadAllAsync"/> may be called any number of times to
/// replay. <see cref="DisposeAsync"/> releases the source-internal
/// storage.
/// </remarks>
public interface ICaptureSource : IAsyncDisposable
{
    /// <summary>
    /// Captured packet count seen so far (live).
    /// </summary>
    long PacketCount { get; }

    /// <summary>
    /// Captured byte count seen so far (live).
    /// </summary>
    long ByteCount { get; }

    /// <summary>
    /// Link-layer type identifier the source emits (LinkLayers value).
    /// 0 = Null (no link-layer data — e.g. for synthetic/file-replay sources).
    /// </summary>
    int LinkType { get; }

    /// <summary>
    /// Begin capturing. Implementations should validate
    /// <paramref name="request"/> (interface name, BPF, limits) and
    /// throw <see cref="CaptureException"/> with an actionable message
    /// on failure.
    /// </summary>
    Task StartAsync(CaptureStartRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Stop capturing and flush. After this returns, captured records
    /// are safe to enumerate via <see cref="ReadAllAsync"/>.
    /// Idempotent — calling more than once is a no-op.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Replay all captured records. May be called multiple times after
    /// <see cref="StopAsync"/> completes. <paramref name="maxPackets"/>
    /// caps the enumeration (null = no cap).
    /// </summary>
    IAsyncEnumerable<CapturedPacket> ReadAllAsync(long? maxPackets, CancellationToken cancellationToken);

    /// <summary>
    /// Path to the underlying raw pcap-format file produced by this
    /// source, or null if the source does not write one. Used by the
    /// pcap formatter to return the bytes verbatim.
    /// </summary>
    string? GetRawPcapFilePath();
}
