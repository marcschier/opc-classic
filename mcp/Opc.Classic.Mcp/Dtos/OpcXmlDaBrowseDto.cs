// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC XML-DA browse response.
/// </summary>
public sealed record OpcXmlDaBrowseResponseDto(
    string ServerState,
    IReadOnlyList<OpcXmlDaBrowseElementDto> Elements,
    string ContinuationPoint,
    bool MoreElements);

/// <summary>
/// JSON-friendly OPC XML-DA browse element.
/// </summary>
public sealed record OpcXmlDaBrowseElementDto(
    string Name,
    string ItemPath,
    string ItemName,
    bool IsItem,
    bool HasChildren);

/// <summary>
/// JSON-friendly OPC XML-DA get-properties response.
/// </summary>
public sealed record OpcXmlDaGetPropertiesResponseDto(
    string ServerState,
    IReadOnlyList<OpcXmlDaItemPropertyListDto> PropertyLists);

/// <summary>
/// JSON-friendly OPC XML-DA properties for one item.
/// </summary>
public sealed record OpcXmlDaItemPropertyListDto(
    string ItemName,
    string ItemPath,
    IReadOnlyList<OpcXmlDaPropertyValueDto> Properties,
    string? ResultId,
    string ResultCode);

/// <summary>
/// JSON-friendly OPC XML-DA property value.
/// </summary>
public sealed record OpcXmlDaPropertyValueDto(
    string Name,
    string? Description,
    object? Value,
    string? ValueType,
    string? RawValue,
    string? ResultId,
    string ResultCode);
