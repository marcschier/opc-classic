//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Sessions;

/// <summary>Thread-safe in-memory OPC Classic session manager.</summary>
public sealed class OpcSessionManager : IOpcSessionManager, IDisposable
{
    private static readonly TimeSpan DefaultIdleExpiry = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan SweepInterval = TimeSpan.FromSeconds(60);
    private readonly ConcurrentDictionary<string, OpcSession> _sessions = new(StringComparer.Ordinal);
    private readonly Timer _timer;
    private bool _disposed;

    /// <summary>Creates a session manager and starts the expiry sweeper.</summary>
    public OpcSessionManager() => _timer = new Timer(SweepExpiredSessions, null, SweepInterval, SweepInterval);

    /// <inheritdoc />
    public OpcSession CreateSession(TimeSpan? idleExpiry = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var session = new OpcSession(Guid.NewGuid().ToString("N"), idleExpiry ?? DefaultIdleExpiry);
        if (!_sessions.TryAdd(session.SessionId, session))
        {
            throw new InvalidOperationException("Unable to create a unique OPC session identifier.");
        }

        return session;
    }

    /// <inheritdoc />
    public OpcSession GetSession(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        if (!TryGetSession(sessionId, out OpcSession? session))
        {
            throw new KeyNotFoundException($"OPC session '{sessionId}' was not found or has expired.");
        }

        session.Touch();
        return session;
    }

    /// <inheritdoc />
    public bool TryGetSession(string sessionId, out OpcSession session)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_sessions.TryGetValue(sessionId, out OpcSession? existing))
        {
            session = null!;
            return false;
        }

        if (existing.IsExpired(DateTimeOffset.UtcNow))
        {
            _ = CloseSession(sessionId);
            session = null!;
            return false;
        }

        session = existing;
        return true;
    }

    /// <inheritdoc />
    public bool CloseSession(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        if (!_sessions.TryRemove(sessionId, out OpcSession? session))
        {
            return false;
        }

        DisposeSession(session);
        return true;
    }

    /// <inheritdoc />
    public IReadOnlyList<OpcSessionDto> ListSessions()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        SweepExpiredSessions(null);
        return _sessions.Values
            .OrderBy(static session => session.CreatedAt)
            .Select(ToDto)
            .ToArray();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _timer.Dispose();
        foreach (OpcSession session in _sessions.Values)
        {
            DisposeSession(session);
        }

        _sessions.Clear();
    }

    private void SweepExpiredSessions(object? state)
    {
        _ = state;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (KeyValuePair<string, OpcSession> pair in _sessions)
        {
            if (pair.Value.IsExpired(now) && _sessions.TryRemove(pair.Key, out OpcSession? removed))
            {
                DisposeSession(removed);
            }
        }
    }

    private static OpcSessionDto ToDto(OpcSession session)
    {
        DaClientState? da = session.DaClient;
        return new OpcSessionDto(
            session.SessionId,
            session.CreatedAt,
            session.LastUsedAt,
            session.LastUsedAt.Add(session.IdleExpiry),
            checked((int)Math.Ceiling(session.IdleExpiry.TotalSeconds)),
            da is not null,
            da?.Host,
            da?.ProgId,
            da?.Clsid);
    }

    private static void DisposeSession(OpcSession session) =>
        session.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
}
