//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>JSON input model for inserting or replacing an HDA value.</summary>
public sealed record OpcHdaWriteValueDto(
    int ServerHandle,
    DateTimeOffset Timestamp,
    object? Value,
    int? Quality = null);

/// <summary>JSON input model for inserting an HDA annotation.</summary>
public sealed record OpcHdaWriteAnnotationDto(
    int ServerHandle,
    DateTimeOffset Timestamp,
    string AnnotationText,
    string User,
    DateTimeOffset? AnnotationTime = null);
