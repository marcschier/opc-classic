// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC Commands state-change notification.
/// </summary>
public sealed record OpcCommandStateDto(
    string InvocationId,
    int EventCount,
    IReadOnlyList<string> PermittedControls,
    bool NoStateChange,
    string State,
    int HResult,
    string Message,
    bool Succeeded,
    DateTimeOffset Timestamp);
