// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Ae;

/// <summary>
/// Result of an OPC AE acknowledge call (one row per condition acked).
/// </summary>
public sealed class AckResult
{
    /// <summary>
    /// The condition that was acked.
    /// </summary>
    public required ConditionRef Condition { get; init; }

    /// <summary>
    /// HRESULT — Ok if acked successfully.
    /// </summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;
}
