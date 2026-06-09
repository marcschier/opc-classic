//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>JSON-friendly OPC XML-DA read item request.</summary>
public sealed record OpcXmlDaReadItemDto(
    string ItemName,
    string? ClientItemHandle = null,
    int MaxAge = 0);
