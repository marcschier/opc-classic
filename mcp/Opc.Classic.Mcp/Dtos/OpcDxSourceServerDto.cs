// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC DX source-server definition.
/// </summary>
public sealed record OpcDxSourceServerDto(
    string? Name = null,
    string? ServerUrl = null,
    string? Description = null,
    string? ServerType = null,
    string? ItemPath = null,
    string? ItemName = null,
    string? Version = null,
    bool? DefaultConnected = null,
    int Mask = 0,
    int Reserved = 0);
