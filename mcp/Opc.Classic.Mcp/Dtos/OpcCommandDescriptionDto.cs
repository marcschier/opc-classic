// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC Commands command description.
/// </summary>
public sealed record OpcCommandDescriptionDto(
    string CommandName,
    string CommandNamespace,
    string Description);
