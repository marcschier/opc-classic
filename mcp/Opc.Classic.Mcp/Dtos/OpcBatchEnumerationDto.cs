//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>JSON-friendly OPC Batch enumeration set descriptor.</summary>
public sealed record OpcBatchEnumerationSetDto(int EnumerationSetId, string Name);

/// <summary>JSON-friendly OPC Batch enumeration value descriptor.</summary>
public sealed record OpcBatchEnumerationDto(int EnumerationSetId, int Value, string Name);
