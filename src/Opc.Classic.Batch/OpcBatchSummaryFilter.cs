//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Batch;

/// <summary>
/// OPC Batch's <c>OPCBATCHSUMMARYFILTER</c> — the filter input to
/// <c>IOPCBatchServer2::GetBatches</c> for matching batch identification
/// fields, batch-size ranges, execution state/mode, and time windows.
/// </summary>
/// <param name="Id">Batch identifier substring filter.</param>
/// <param name="Description">Batch description substring filter.</param>
/// <param name="OpcItemId">OPC item identifier substring filter.</param>
/// <param name="MasterRecipeId">Master recipe identifier substring filter.</param>
/// <param name="MinBatchSize">Minimum batch size.</param>
/// <param name="MaxBatchSize">Maximum batch size.</param>
/// <param name="EngineeringUnits">Engineering units substring filter.</param>
/// <param name="ExecutionState">Execution state substring filter.</param>
/// <param name="ExecutionMode">Execution mode substring filter.</param>
/// <param name="MinStartTime">Minimum actual start time.</param>
/// <param name="MaxStartTime">Maximum actual start time.</param>
/// <param name="MinEndTime">Minimum actual end time.</param>
/// <param name="MaxEndTime">Maximum actual end time.</param>
public sealed record OpcBatchSummaryFilter(
    string? Id,
    string? Description,
    string? OpcItemId,
    string? MasterRecipeId,
    float MinBatchSize,
    float MaxBatchSize,
    string? EngineeringUnits,
    string? ExecutionState,
    string? ExecutionMode,
    DateTimeOffset MinStartTime,
    DateTimeOffset MaxStartTime,
    DateTimeOffset MinEndTime,
    DateTimeOffset MaxEndTime);
