//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Ae;

/// <summary>
/// Result of an OPC AE acknowledge call (one row per condition acked).
/// </summary>
public sealed class AckResult
{
    /// <summary>The condition that was acked.</summary>
    public required ConditionRef Condition { get; init; }

    /// <summary>HRESULT — Ok if acked successfully.</summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;
}
