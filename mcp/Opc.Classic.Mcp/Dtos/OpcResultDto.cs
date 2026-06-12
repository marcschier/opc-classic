//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC operation result with HRESULT details.
/// </summary>
public sealed record OpcResultDto(
    int HResult,
    string Message,
    bool Succeeded,
    string? ItemName = null,
    int? ClientHandle = null,
    int? ServerHandle = null,
    string? ValueType = null,
    int? AccessRights = null,
    string? SubscriptionId = null,
    int? TransactionId = null,
    int? CancelId = null);
