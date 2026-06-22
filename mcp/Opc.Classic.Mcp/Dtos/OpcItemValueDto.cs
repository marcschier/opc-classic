// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC DA value with quality, timestamp, and HRESULT.
/// </summary>
public sealed record OpcItemValueDto(
    string ItemName,
    string? ItemPath,
    int ClientHandle,
    int? ServerHandle,
    object? Value,
    string? ValueType,
    ushort Quality,
    string QualityText,
    DateTimeOffset Timestamp,
    int HResult,
    string Message);
