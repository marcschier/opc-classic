// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Lifecycle state of a capture session as observed via the MCP tool
/// surface.
/// </summary>
public enum CaptureSessionState
{
    /// <summary>
    /// The session has been allocated but <see cref="ICaptureSource.StartAsync"/> hasn't completed yet.
    /// </summary>
    Starting,

    /// <summary>
    /// The source is actively capturing.
    /// </summary>
    Running,

    /// <summary>
    /// A stop request is in flight.
    /// </summary>
    Stopping,

    /// <summary>
    /// The session completed normally; the trace can be read.
    /// </summary>
    Completed,

    /// <summary>
    /// The session failed; see <see cref="CaptureSession.Error"/>.
    /// </summary>
    Failed,

    /// <summary>
    /// The session was disposed (LRU evicted or explicitly cleaned up).
    /// </summary>
    Disposed,
}
