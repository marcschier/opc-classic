//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC Commands command description.
/// </summary>
public sealed record OpcCommandDescriptionDto(
    string CommandName,
    string CommandNamespace,
    string Description);
