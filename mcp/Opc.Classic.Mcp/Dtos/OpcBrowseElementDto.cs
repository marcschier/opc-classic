//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>JSON-friendly OPC DA browse element.</summary>
public sealed record OpcBrowseElementDto(
    string Name,
    string ItemName,
    string? ItemPath,
    bool IsItem,
    bool HasChildren,
    IReadOnlyList<OpcItemPropertyDto> Properties);

/// <summary>JSON-friendly OPC DA item property.</summary>
public sealed record OpcItemPropertyDto(
    int PropertyId,
    string? Name,
    string Description,
    string? DataType,
    object? Value,
    int HResult,
    string Message,
    string? ItemName,
    string? ItemPath);
