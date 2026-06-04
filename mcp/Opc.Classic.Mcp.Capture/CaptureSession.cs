//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
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
public sealed class CaptureSession : IAsyncDisposable
{
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
        ILogger? logger = null)
    {
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
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            try
            {
                await Source.StartAsync(Request, cancellationToken).ConfigureAwait(false);
                StartedAt = DateTimeOffset.UtcNow;
                State = CaptureSessionState.Running;
                LastTouchedAt = DateTimeOffset.UtcNow;
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Capture session {SessionId} started ({Source}).", Id, SourceName);
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                State = CaptureSessionState.Failed;
                _logger.LogError(ex, "Capture session {SessionId} failed to start.", Id);
                throw;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Stops the underlying source. Idempotent.</summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (State is CaptureSessionState.Completed
                     or CaptureSessionState.Failed
                     or CaptureSessionState.Disposed)
            {
                return;
            }

            State = CaptureSessionState.Stopping;
            try
            {
                await Source.StopAsync(cancellationToken).ConfigureAwait(false);
                StoppedAt = DateTimeOffset.UtcNow;
                State = CaptureSessionState.Completed;
                LastTouchedAt = DateTimeOffset.UtcNow;
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Capture session {SessionId} completed ({Packets} packets, {Bytes} bytes).",
                        Id, Source.PacketCount, Source.ByteCount);
                }
            }
            catch (Exception ex)
            {
                Error = ex.Message;
                State = CaptureSessionState.Failed;
                _logger.LogError(ex, "Capture session {SessionId} failed to stop.", Id);
                throw;
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>Marks the session as touched for LRU bookkeeping.</summary>
    public void Touch() => LastTouchedAt = DateTimeOffset.UtcNow;

    /// <inheritdoc/>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Dispose path must release native resources + scratch folder regardless of source-side errors; logging is sufficient.")]
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        try
        {
            await Source.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Capture session {SessionId} source dispose error.", Id);
        }

        try
        {
            if (Directory.Exists(SessionFolder))
            {
                Directory.Delete(SessionFolder, recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Capture session {SessionId} folder cleanup error.", Id);
        }

        State = CaptureSessionState.Disposed;
        _lock.Dispose();
    }
}
