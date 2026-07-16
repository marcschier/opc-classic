// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Optional contract for sources that can stop independently of a session stop request.
/// </summary>
public interface ICaptureSourceCompletion
{
    /// <summary>
    /// Completes when the source naturally stops, or faults when it stops because of an error.
    /// Explicit <see cref="ICaptureSource.StopAsync"/> calls do not need to complete this task.
    /// </summary>
    Task Completion { get; }
}
