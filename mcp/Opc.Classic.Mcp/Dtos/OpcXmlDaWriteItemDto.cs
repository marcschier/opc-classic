// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC XML-DA write item request.
/// </summary>
public sealed record OpcXmlDaWriteItemDto(
    string ItemName,
    object? Value,
    string? ClientItemHandle = null,
    string? ValueType = null);
