//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC HDA browse element.
/// </summary>
public sealed record OpcHdaBrowseElementDto(
    string Name,
    string ItemId,
    string BrowseType);

/// <summary>
/// JSON-friendly OPC HDA item handle result.
/// </summary>
public sealed record OpcHdaItemHandleDto(
    string ItemId,
    int ClientHandle,
    int ServerHandle,
    int HResult,
    string Message,
    bool Succeeded);

/// <summary>
/// JSON-friendly OPC HDA timestamped value.
/// </summary>
public sealed record OpcHdaItemValueDto(
    DateTimeOffset Timestamp,
    object? Value,
    string? ValueType,
    uint Quality,
    string QualityText);

/// <summary>
/// JSON-friendly OPC HDA read result for one item.
/// </summary>
public sealed record OpcHdaReadResultDto(
    string ItemId,
    int ClientHandle,
    int? ServerHandle,
    int AggregateId,
    string? Aggregate,
    int HResult,
    string Message,
    int? ContinuationHandle,
    IReadOnlyList<OpcHdaItemValueDto> Values);

/// <summary>
/// JSON-friendly OPC HDA modified timestamped value.
/// </summary>
public sealed record OpcHdaModifiedValueDto(
    DateTimeOffset Timestamp,
    object? Value,
    string? ValueType,
    uint Quality,
    string QualityText,
    DateTimeOffset ModificationTime,
    uint EditType,
    string? User);

/// <summary>
/// JSON-friendly OPC HDA modified data read result for one item.
/// </summary>
public sealed record OpcHdaModifiedReadResultDto(
    string ItemId,
    int ClientHandle,
    int? ServerHandle,
    int HResult,
    string Message,
    IReadOnlyList<OpcHdaModifiedValueDto> Values);

/// <summary>
/// JSON-friendly OPC HDA attribute value.
/// </summary>
public sealed record OpcHdaAttributeValueDto(
    DateTimeOffset Timestamp,
    object? Value,
    string? ValueType);

/// <summary>
/// JSON-friendly OPC HDA attribute read result.
/// </summary>
public sealed record OpcHdaAttributeResultDto(
    string ItemId,
    int ClientHandle,
    int? ServerHandle,
    int AttributeId,
    int HResult,
    string Message,
    IReadOnlyList<OpcHdaAttributeValueDto> Values);

/// <summary>
/// JSON-friendly OPC HDA annotation.
/// </summary>
public sealed record OpcHdaAnnotationDto(
    DateTimeOffset Timestamp,
    DateTimeOffset AnnotationTime,
    string AnnotationText,
    string User);

/// <summary>
/// JSON-friendly OPC HDA annotation result for one item.
/// </summary>
public sealed record OpcHdaAnnotationResultDto(
    string ItemId,
    int ClientHandle,
    int? ServerHandle,
    int HResult,
    string Message,
    IReadOnlyList<OpcHdaAnnotationDto> Annotations);

/// <summary>
/// JSON-friendly OPC HDA aggregate metadata.
/// </summary>
public sealed record OpcHdaAggregateDto(
    int AggregateId,
    string Name,
    string Description);
