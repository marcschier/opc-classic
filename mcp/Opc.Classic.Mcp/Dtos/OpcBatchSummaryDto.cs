//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC Batch summary.
/// </summary>
public sealed record OpcBatchSummaryDto(
    string? Id,
    string? Description,
    string? OpcItemId,
    string? MasterRecipeId,
    float BatchSize,
    string? EngineeringUnits,
    string? ExecutionState,
    string? ExecutionMode,
    DateTimeOffset ActualStartTime,
    DateTimeOffset ActualEndTime);
