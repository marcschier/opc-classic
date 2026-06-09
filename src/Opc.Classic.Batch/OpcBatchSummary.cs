//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Batch;

/// <summary>
/// OPC Batch's <c>OPCBATCHSUMMARY</c> — summary information for a batch returned by
/// <c>IOPCBatchServer2::GetBatches</c> / <c>IEnumOPCBatchSummary</c>.
/// </summary>
/// <param name="Id">Batch identifier.</param>
/// <param name="Description">Human-readable batch description.</param>
/// <param name="OpcItemId">Server item identifier associated with the batch.</param>
/// <param name="MasterRecipeId">Master recipe identifier.</param>
/// <param name="BatchSize">Batch quantity.</param>
/// <param name="EngineeringUnits">Engineering units for <paramref name="BatchSize"/>.</param>
/// <param name="ExecutionState">Batch execution state.</param>
/// <param name="ExecutionMode">Batch execution mode.</param>
/// <param name="ActualStartTime">Actual batch start timestamp.</param>
/// <param name="ActualEndTime">Actual batch end timestamp.</param>
public sealed record OpcBatchSummary(
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
