// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Consumer-visible outcome of replacing a running capture filter.
/// </summary>
public enum CaptureFilterTransitionStatus
{
    None,
    Unchanged,
    LiveUpdated,
    Restarted,
    RestartedWithCleanupWarning,
    Failed,
    Canceled,
}

/// <summary>
/// Immutable transition report retained by the capture session.
/// </summary>
public sealed record CaptureFilterTransitionResult
{
    public required string SessionId { get; init; }
    public required string RequestedFilter { get; init; }
    public string? PreviousFilter { get; init; }
    public string? EffectiveFilter { get; init; }
    public required CaptureFilterTransitionStatus Status { get; init; }
    public required DateTimeOffset StartedAt { get; init; }
    public required DateTimeOffset CompletedAt { get; init; }
    public long PreservedPacketCount { get; init; }
    public long PreservedByteCount { get; init; }
    public int SourceSegmentCount { get; init; }
    public string? Error { get; init; }

    public bool Succeeded =>
        Status is CaptureFilterTransitionStatus.Unchanged
            or CaptureFilterTransitionStatus.LiveUpdated
            or CaptureFilterTransitionStatus.Restarted
            or CaptureFilterTransitionStatus.RestartedWithCleanupWarning;
}
