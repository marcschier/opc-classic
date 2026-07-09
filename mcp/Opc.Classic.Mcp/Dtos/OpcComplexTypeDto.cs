// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC Complex Data item type description.
/// </summary>
public sealed record OpcComplexTypeDto(
    string ItemId,
    Guid TypeId,
    string DictionaryId,
    string? TypeItemId,
    string? UnconvertedItemId,
    string? DataFilter,
    IReadOnlyList<string> AvailableFilters);

/// <summary>
/// JSON-friendly OPC Complex Data type-system descriptor.
/// </summary>
public sealed record OpcTypeSystemDto(
    string TypeSystemId,
    bool Supported,
    string NamespacePath,
    IReadOnlyList<string> SupportedTypeSystemIds);

/// <summary>
/// JSON-friendly OPC Complex Data dictionary.
/// </summary>
public sealed record OpcTypeDictionaryDto(
    string DictionaryId,
    string TypeSystemId,
    string? Name,
    string Dictionary,
    IReadOnlyList<OpcComplexTypeDescriptionDto> Types,
    string? ParseError);

/// <summary>
/// JSON-friendly OPC Complex Data type description.
/// </summary>
public sealed record OpcComplexTypeDescriptionDto(
    string Name,
    string TypeId,
    string Kind,
    bool IsComplex,
    IReadOnlyList<OpcComplexTypeFieldDto> Fields);

/// <summary>
/// JSON-friendly OPC Complex Data type field.
/// </summary>
public sealed record OpcComplexTypeFieldDto(
    string Name,
    string Kind,
    string? TypeId,
    int? Length,
    int? ElementCount,
    string? ElementCountFieldName,
    string? FieldTerminator,
    string? ByteOrder,
    string? StringEncoding,
    int? CharWidth,
    string? Format);
