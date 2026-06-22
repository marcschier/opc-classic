// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC server registration returned by discovery.
/// </summary>
public sealed record OpcServerDescriptorDto(
    Guid ClassId,
    string ProgId,
    string UserType,
    string? VerIndProgId,
    IReadOnlyList<Guid> Categories,
    string Host);
