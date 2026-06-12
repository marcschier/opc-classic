//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

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
