// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC Commands command description.
/// </summary>
public sealed record OpcCommandDescriptionDto(
    string CommandName,
    string CommandNamespace,
    string Description);
