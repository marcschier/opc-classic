//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Owns the registry of active + retained capture sessions. Enforces
/// the engine-wide concurrency cap and LRU eviction policy.
/// </summary>
/// <remarks>
/// <para>
/// Concurrency cap (default 8 active sessions) protects the host from
/// runaway captures consuming the NIC + disk indefinitely. The LRU
/// retention cap (default 32 completed sessions) keeps the historic
/// trace cache bounded so a long-running MCP host doesn't grow
/// unbounded.
/// </para>
/// <para>
/// All public methods are thread-safe via
/// <see cref="ConcurrentDictionary{TKey, TValue}"/> + a per-instance
/// <see cref="SemaphoreSlim"/> for the LRU eviction critical section.
/// </para>
/// </remarks>
public sealed class CaptureSessionManager : IAsyncDisposable {
    private const int kDefaultMaxActiveSessions = 8;
    private const int kDefaultMaxRetainedSessions = 32;

    private readonly ConcurrentDictionary<string, CaptureSession> _sessions = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _evictionLock = new(1, 1);
    private readonly ILogger _logger;
    private readonly string _scratchRoot;
    private int _disposed;

    /// <summary>Creates a manager rooted at the given scratch directory (per-session subfolders created lazily).</summary>
    /// <param name="scratchRoot">Parent directory for per-session scratch folders (raw pcap files etc.). Created if missing.</param>
    /// <param name="logger">Logger or null for none.</param>
    /// <param name="maxActiveSessions">Cap on concurrent Running/Starting sessions (default 8). Must be >= 1.</param>
    /// <param name="maxRetainedSessions">Cap on retained Completed/Failed sessions (default 32). Must be >= maxActiveSessions.</param>
    public CaptureSessionManager(
        string scratchRoot,
        ILogger? logger = null,
        int maxActiveSessions = kDefaultMaxActiveSessions,
        int maxRetainedSessions = kDefaultMaxRetainedSessions) {
        ArgumentException.ThrowIfNullOrEmpty(scratchRoot);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxActiveSessions, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxRetainedSessions, maxActiveSessions);

        _scratchRoot = scratchRoot;
        _logger = logger ?? NullLogger.Instance;
        MaxActiveSessions = maxActiveSessions;
        MaxRetainedSessions = maxRetainedSessions;
        Directory.CreateDirectory(scratchRoot);
    }

    /// <summary>Cap on concurrently Running/Starting sessions.</summary>
    public int MaxActiveSessions { get; }

    /// <summary>Cap on retained sessions in any state.</summary>
    public int MaxRetainedSessions { get; }

    /// <summary>Current registered session count (all states).</summary>
    public int Count => _sessions.Count;

    /// <summary>Currently Running or Starting sessions.</summary>
    public int ActiveCount {
        get {
            int active = 0;
            foreach (CaptureSession s in _sessions.Values) {
                if (s.State is CaptureSessionState.Starting or CaptureSessionState.Running) {
                    active++;
                }
            }
            return active;
        }
    }

    /// <summary>
    /// Registers + starts a new session. Allocates the per-session
    /// scratch folder, enforces the active-session cap, runs LRU
    /// eviction if the retention cap would be exceeded, and invokes
    /// <see cref="CaptureSession.StartAsync"/>.
    /// </summary>
    public async Task<CaptureSession> CreateAndStartAsync(
        string sourceName,
        Func<string, ICaptureSource> sourceFactory,
        CaptureStartRequest request,
        CancellationToken cancellationToken) {
        ObjectDisposedException.ThrowIf(_disposed != 0, this);
        ArgumentException.ThrowIfNullOrEmpty(sourceName);
        ArgumentNullException.ThrowIfNull(sourceFactory);
        ArgumentNullException.ThrowIfNull(request);

        if (ActiveCount >= MaxActiveSessions) {
            throw new CaptureException(
                $"Capture session limit reached ({MaxActiveSessions} active). Stop an existing session before starting another.");
        }

        await EnsureRetentionCapAsync(cancellationToken).ConfigureAwait(false);

        string id = Guid.NewGuid().ToString("N");
        string folder = Path.Combine(_scratchRoot, id);
        Directory.CreateDirectory(folder);

        ICaptureSource source = sourceFactory(folder);
        var session = new CaptureSession(id, sourceName, source, folder, request, _logger);
        if (!_sessions.TryAdd(id, session)) {
            // Astronomically unlikely; surface clearly rather than overwrite.
            await session.DisposeAsync().ConfigureAwait(false);
            throw new CaptureException("CaptureSessionManager could not allocate a fresh session id.");
        }

        try {
            await session.StartAsync(cancellationToken).ConfigureAwait(false);
            return session;
        }
        catch {
            _sessions.TryRemove(id, out _);
            await session.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Looks up a session by id. Touches it for LRU bookkeeping.</summary>
    public bool TryGet(string id, out CaptureSession session) {
        ArgumentException.ThrowIfNullOrEmpty(id);
        if (_sessions.TryGetValue(id, out CaptureSession? found)) {
            found.Touch();
            session = found;
            return true;
        }

        session = null!;
        return false;
    }

    /// <summary>Enumerates sessions, optionally filtered by state.</summary>
    public IReadOnlyList<CaptureSession> List(CaptureSessionState? state = null) {
        if (state is null) {
            return _sessions.Values.ToArray();
        }

        var filtered = new List<CaptureSession>();
        foreach (CaptureSession s in _sessions.Values) {
            if (s.State == state) {
                filtered.Add(s);
            }
        }
        return filtered;
    }

    /// <summary>
    /// Stops + disposes a session by id. Returns false when the id is
    /// not registered. Idempotent (returns true even if the session is
    /// already Completed/Failed/Disposed).
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Stop failure already logged by the session; the dispose path must run regardless.")]
    public async Task<bool> RemoveAsync(string id, CancellationToken cancellationToken) {
        ArgumentException.ThrowIfNullOrEmpty(id);
        if (!_sessions.TryRemove(id, out CaptureSession? session)) {
            return false;
        }

        try {
            await session.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        catch {
            // Stop failure already logged by the session; proceed to dispose.
        }

        await session.DisposeAsync().ConfigureAwait(false);
        return true;
    }

    private async Task EnsureRetentionCapAsync(CancellationToken cancellationToken) {
        if (_sessions.Count < MaxRetainedSessions) {
            return;
        }

        await _evictionLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            while (_sessions.Count >= MaxRetainedSessions) {
                CaptureSession? oldest = null;
                foreach (CaptureSession s in _sessions.Values) {
                    if (s.State is CaptureSessionState.Starting or CaptureSessionState.Running) {
                        // Don't evict active sessions; the active-session cap
                        // catches the all-active corner case before we get here.
                        continue;
                    }

                    if (oldest is null || s.LastTouchedAt < oldest.LastTouchedAt) {
                        oldest = s;
                    }
                }

                if (oldest is null) {
                    // All sessions are Running/Starting and at the retention cap —
                    // the caller's active-session cap check will surface a clearer
                    // message; bail out so we don't deadlock.
                    return;
                }

                if (_logger.IsEnabled(LogLevel.Information)) {
                    _logger.LogInformation(
                        "Capture session {SessionId} evicted (LRU; touched {Touched}).",
                        oldest.Id, oldest.LastTouchedAt);
                }

                _sessions.TryRemove(oldest.Id, out _);
                await oldest.DisposeAsync().ConfigureAwait(false);
            }
        }
        finally {
            _evictionLock.Release();
        }
    }

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Dispose path must release every session regardless of individual stop errors; sessions already logged failures.")]
    public async ValueTask DisposeAsync() {
        if (Interlocked.Exchange(ref _disposed, 1) != 0) {
            return;
        }

        foreach (CaptureSession session in _sessions.Values) {
            try {
                await session.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch {
                // Logged by the session.
            }

            await session.DisposeAsync().ConfigureAwait(false);
        }

        _sessions.Clear();
        _evictionLock.Dispose();
    }
}
