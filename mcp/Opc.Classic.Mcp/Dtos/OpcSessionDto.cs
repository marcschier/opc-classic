// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly description of an MCP OPC Classic session.
/// </summary>
public sealed record OpcSessionDto(
    string SessionId,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastUsedAt,
    DateTimeOffset ExpiresAt,
    int IdleExpirySeconds,
    bool DaConnected,
    string? DaHost,
    string? DaProgId,
    Guid? DaClsid);
