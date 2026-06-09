//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Wraps an <see cref="ICaptureSource"/> with a state machine,
/// per-session asynchronous lock, and lifecycle metadata. One per
/// MCP-tool-visible capture session.
/// </summary>
/// <remarks>
/// Lifecycle:
/// <c>Starting → Running → Stopping → (Completed | Failed) → Disposed.</c>
/// </remarks>
public sealed class CaptureSession : IAsyncDisposable {
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ILogger _logger;
    private int _disposed;

    /// <summary>Creates a session wrapping <paramref name="source"/> under the supplied identity.</summary>
    public CaptureSession(
        string id,
        string sourceName,
        ICaptureSource source,
        string sessionFolder,
        CaptureStartRequest request,
        ILogger? logger = null) {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(sourceName);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(sessionFolder);
        ArgumentNullException.ThrowIfNull(request);

        Id = id;
        SourceName = sourceName;
        Source = source;
        SessionFolder = sessionFolder;
        Request = request;
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>Opaque session identifier surfaced to the MCP caller.</summary>
    public string Id { get; }

    /// <summary>Name of the capture source ("pcap", "wirecapture", ...).</summary>
    public string SourceName { get; }

    /// <summary>Underlying capture source.</summary>
    public ICaptureSource Source { get; }

    /// <summary>Per-session scratch directory (auto-cleaned on Dispose).</summary>
    public string SessionFolder { get; }

    /// <summary>Caller-supplied start parameters; surfaced via the MCP session info DTO.</summary>
    public CaptureStartRequest Request { get; }

    /// <summary>Current state in the lifecycle.</summary>
    public CaptureSessionState State { get; private set; } = CaptureSessionState.Starting;

    /// <summary>UTC time <see cref="StartAsync"/> completed; null until then.</summary>
    public DateTimeOffset? StartedAt { get; private set; }

    /// <summary>UTC time <see cref="StopAsync"/> completed; null until then.</summary>
    public DateTimeOffset? StoppedAt { get; private set; }

    /// <summary>UTC time the session was last touched (for LRU eviction).</summary>
    public DateTimeOffset LastTouchedAt { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>Error message when <see cref="State"/> is <see cref="CaptureSessionState.Failed"/>; null otherwise.</summary>
    public string? Error { get; private set; }

    /// <summary>Starts the underlying source. Sets <see cref="State"/> to Running on success or Failed on throw.</summary>
    public async Task StartAsync(CancellationToken cancellationToken) {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            try {
                await Source.StartAsync(Request, cancellationToken).ConfigureAwait(false);
                StartedAt = DateTimeOffset.UtcNow;
                State = CaptureSessionState.Running;
                LastTouchedAt = DateTimeOffset.UtcNow;
                if (_logger.IsEnabled(LogLevel.Information)) {
                    _logger.LogInformation("Capture session {SessionId} started ({Source}).", Id, SourceName);
                }
            }
            catch (Exception ex) {
                Error = ex.Message;
                State = CaptureSessionState.Failed;
                _logger.LogError(ex, "Capture session {SessionId} failed to start.", Id);
                throw;
            }
        }
        finally {
            _lock.Release();
        }
    }

    /// <summary>Stops the underlying source. Idempotent.</summary>
    public async Task StopAsync(CancellationToken cancellationToken) {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            if (State is CaptureSessionState.Completed
                     or CaptureSessionState.Failed
                     or CaptureSessionState.Disposed) {
                return;
            }

            State = CaptureSessionState.Stopping;
            try {
                await Source.StopAsync(cancellationToken).ConfigureAwait(false);
                StoppedAt = DateTimeOffset.UtcNow;
                State = CaptureSessionState.Completed;
                LastTouchedAt = DateTimeOffset.UtcNow;
                if (_logger.IsEnabled(LogLevel.Information)) {
                    _logger.LogInformation(
                        "Capture session {SessionId} completed ({Packets} packets, {Bytes} bytes).",
                        Id, Source.PacketCount, Source.ByteCount);
                }
            }
            catch (Exception ex) {
                Error = ex.Message;
                State = CaptureSessionState.Failed;
                _logger.LogError(ex, "Capture session {SessionId} failed to stop.", Id);
                throw;
            }
        }
        finally {
            _lock.Release();
        }
    }

    /// <summary>Marks the session as touched for LRU bookkeeping.</summary>
    public void Touch() => LastTouchedAt = DateTimeOffset.UtcNow;

    private DecodeCursor? _cursor;
    private readonly object _cursorInitLock = new();

    /// <summary>
    /// Drains the next decoded-PDU window from the live capture trace
    /// (cursor-based "live tail" for the
    /// <c>opcclassic.capture.tail</c> MCP tool).
    /// </summary>
    /// <param name="sinceIndex">
    /// 0-based index into the per-session emitted-PDU list returned by
    /// a previous tail call's <see cref="DrainTailResult.NextIndex"/>.
    /// First call should pass <c>0</c>.
    /// </param>
    /// <param name="max">Upper bound on the number of PDUs returned in this call.</param>
    /// <returns>
    /// A snapshot of the next window of decoded PDUs together with the
    /// cursor value the caller should pass as <paramref name="sinceIndex"/>
    /// on the next poll and a <c>Done</c> flag set when the underlying
    /// session has Completed / Failed / Disposed AND the cache is fully
    /// caught up.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Implementation: each call re-reads the pcap file from the start
    /// and skips packets the cursor has already consumed (a few µs per
    /// already-seen packet at the libpcap layer; the cursor caches the
    /// long-lived <see cref="OpcDcomDecoder"/> so per-flow state — bind
    /// context-id → IID maps, fragment reassembly — survives across
    /// poll calls). For a 50 MB trace this is well under 100 ms per
    /// poll on commodity hardware; for very large traces the operator
    /// can poll less often or use <c>opcclassic.capture.get</c>
    /// afterward to grab the whole trace at once.
    /// </para>
    /// <para>
    /// The cursor is per-session and serialised by its own lock; two
    /// concurrent callers do NOT race the decoder state. Concurrent
    /// access patterns (poll + get; poll + stop) are safe because the
    /// tail path uses a SHARED-READ pcap reader and never mutates the
    /// pcap file.
    /// </para>
    /// </remarks>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Decoder errors on individual packets must not abort the entire tail-drain; the OpcDcomDecoder already swallows + logs malformed-frame errors internally.")]
    internal async Task<DrainTailResult> DrainTailAsync(
        long sinceIndex,
        int max,
        CancellationToken cancellationToken) {
        if (sinceIndex < 0) {
            sinceIndex = 0;
        }
        if (max <= 0) {
            max = 1;
        }

        DecodeCursor cursor = GetOrCreateCursor();

        await cursor.Lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            long packetIdx = 0;
            await foreach (CapturedPacket pkt in Source.ReadAllAsync(maxPackets: null, cancellationToken).ConfigureAwait(false)) {
                if (packetIdx < cursor.PacketsConsumed) {
                    packetIdx++;
                    continue;
                }

                foreach (DecodedOpcPdu pdu in cursor.Decoder.Decode(pkt)) {
                    cursor.Pdus.Add(pdu);
                }
                packetIdx++;
            }
            cursor.PacketsConsumed = packetIdx;

            long totalEmitted = cursor.Pdus.Count;
            long startIdx = Math.Min(sinceIndex, totalEmitted);
            long endExclusive = Math.Min(startIdx + max, totalEmitted);
            int sliceCount = (int)(endExclusive - startIdx);

            IReadOnlyList<DecodedOpcPdu> window = sliceCount > 0
                ? cursor.Pdus.GetRange((int)startIdx, sliceCount)
                : Array.Empty<DecodedOpcPdu>();

            bool sessionDone = State is CaptureSessionState.Completed
                                or CaptureSessionState.Failed
                                or CaptureSessionState.Disposed;
            bool done = sessionDone && endExclusive == totalEmitted;

            LastTouchedAt = DateTimeOffset.UtcNow;
            return new DrainTailResult(window, endExclusive, totalEmitted, done, State);
        }
        finally {
            cursor.Lock.Release();
        }
    }

    private DecodeCursor GetOrCreateCursor() {
        DecodeCursor? cursor = _cursor;
        if (cursor is not null) {
            return cursor;
        }

        lock (_cursorInitLock) {
            cursor = _cursor ??= new DecodeCursor(_logger, BuildUnwrapper(Request));
        }
        return cursor;
    }

    /// <summary>
    /// Creates an <see cref="NtlmPassiveUnwrapper"/> when the
    /// request supplied a 16-byte NTLM session key, or returns null
    /// (decoder skips all unwrap attempts). Centralised here so the
    /// per-session decoder + the one-shot decoders used by
    /// <c>opcclassic.capture.get</c> and <c>.summarize</c> share the
    /// same construction logic.
    /// </summary>
    internal static NtlmPassiveUnwrapper? BuildUnwrapper(CaptureStartRequest request) {
        ArgumentNullException.ThrowIfNull(request);
        if (request.NtlmSessionKey is not { Length: NtlmPassiveUnwrapper.VerifierLength } sk) {
            return null;
        }
        return new NtlmPassiveUnwrapper(sk);
    }

    /// <summary>
    /// Per-session decoded-PDU cache + long-lived decoder used by the
    /// <c>opcclassic.capture.tail</c> cursor-based polling path.
    /// </summary>
    private sealed class DecodeCursor {
        public DecodeCursor(ILogger logger, NtlmPassiveUnwrapper? unwrapper) {
            Unwrapper = unwrapper;
            Decoder = new OpcDcomDecoder(unwrapper, logger);
        }

        public OpcDcomDecoder Decoder { get; }

        public NtlmPassiveUnwrapper? Unwrapper { get; }

        public List<DecodedOpcPdu> Pdus { get; } = new();

        public long PacketsConsumed { get; set; }

        public SemaphoreSlim Lock { get; } = new(1, 1);
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Dispose path must release native resources + scratch folder regardless of source-side errors; logging is sufficient.")]
    public async ValueTask DisposeAsync() {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) {
            return;
        }

        try {
            await Source.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Capture session {SessionId} source dispose error.", Id);
        }

        try {
            if (Directory.Exists(SessionFolder)) {
                Directory.Delete(SessionFolder, recursive: true);
            }
        }
        catch (Exception ex) {
            _logger.LogWarning(ex, "Capture session {SessionId} folder cleanup error.", Id);
        }

        State = CaptureSessionState.Disposed;
        _cursor?.Lock.Dispose();
        _cursor?.Unwrapper?.Dispose();
        _lock.Dispose();
    }
}

/// <summary>
/// Snapshot returned by <see cref="CaptureSession.DrainTailAsync"/>.
/// </summary>
/// <param name="Pdus">
/// The next decoded-PDU window (length &le; <c>max</c>) starting at
/// the caller-supplied <c>sinceIndex</c>. Empty when the caller is
/// already caught up with the live stream.
/// </param>
/// <param name="NextIndex">
/// Cursor the caller should pass as <c>sinceIndex</c> on the next
/// <c>opcclassic.capture.tail</c> call. Increments past
/// <c>sinceIndex + Pdus.Count</c> only when more PDUs are emitted.
/// </param>
/// <param name="TotalEmitted">
/// Total PDU count emitted by the per-session decoder so far. Useful
/// for progress reporting; always &gt;= <c>NextIndex</c>.
/// </param>
/// <param name="Done">
/// True when the underlying session has Completed / Failed / Disposed
/// AND the cursor has consumed every available PDU. The caller can
/// stop polling once this is true.
/// </param>
/// <param name="SessionState">The underlying session's lifecycle state at the time of the drain.</param>
internal sealed record DrainTailResult(
    IReadOnlyList<DecodedOpcPdu> Pdus,
    long NextIndex,
    long TotalEmitted,
    bool Done,
    CaptureSessionState SessionState);
