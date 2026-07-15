// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Optional live-source capability for replacing a filter without restarting the source.
/// </summary>
public interface ICaptureFilterController
{
    /// <summary>
    /// Filter currently applied by the source.
    /// </summary>
    string? EffectiveFilter { get; }

    /// <summary>
    /// Attempts an atomic live filter update. Implementations must leave the prior
    /// filter active when this method throws.
    /// </summary>
    CaptureSourceFilterUpdateResult TryUpdateFilter(
        string filter,
        CancellationToken cancellationToken);
}

/// <summary>
/// Outcome of a source-level live filter update attempt.
/// </summary>
public enum CaptureSourceFilterUpdateStatus
{
    Updated,
    RestartRequired,
}

/// <summary>
/// Source-level filter update result used by <see cref="CaptureSession"/> to
/// select an in-place transition or a controlled source restart.
/// </summary>
public sealed record CaptureSourceFilterUpdateResult(
    CaptureSourceFilterUpdateStatus Status,
    string? Detail = null)
{
    public static CaptureSourceFilterUpdateResult Updated { get; } =
        new(CaptureSourceFilterUpdateStatus.Updated);

    public static CaptureSourceFilterUpdateResult RestartRequired(string? detail = null) =>
        new(CaptureSourceFilterUpdateStatus.RestartRequired, detail);
}
