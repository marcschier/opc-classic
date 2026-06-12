//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC XML-DA write item request.
/// </summary>
public sealed record OpcXmlDaWriteItemDto(
    string ItemName,
    object? Value,
    string? ClientItemHandle = null,
    string? ValueType = null);
