//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>JSON-friendly OPC Commands invocation result.</summary>
public sealed record OpcCommandInvocationDto(
    string? InvocationId,
    string CommandName,
    string CommandNamespace,
    string TargetId,
    bool Asynchronous,
    IReadOnlyList<string> Results,
    int HResult,
    string Message,
    bool Succeeded,
    int? RevisedUpdateFrequencyMs = null,
    IReadOnlyList<string>? PermittedControls = null);
