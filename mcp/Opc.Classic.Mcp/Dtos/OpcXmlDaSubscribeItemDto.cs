// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC XML-DA subscribe item request.
/// </summary>
public sealed record OpcXmlDaSubscribeItemDto(
    string ItemName,
    string? ClientItemHandle = null,
    int RequestedSamplingRate = 0,
    float Deadband = 0f);
