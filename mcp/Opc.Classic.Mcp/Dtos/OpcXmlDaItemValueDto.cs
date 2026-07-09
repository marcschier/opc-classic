// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC XML-DA item value.
/// </summary>
public sealed record OpcXmlDaItemValueDto(
    string ItemName,
    string? ClientItemHandle,
    object? Value,
    string? ValueType,
    string? RawValue,
    ushort Quality,
    string QualityText,
    DateTimeOffset? Timestamp,
    string? ResultId,
    string ResultCode);

/// <summary>
/// JSON-friendly OPC XML-DA write result.
/// </summary>
public sealed record OpcXmlDaWriteResultDto(
    string ItemName,
    string? ClientItemHandle,
    string? ResultId,
    string ResultCode,
    string? ErrorText);

/// <summary>
/// JSON-friendly OPC XML-DA subscription result.
/// </summary>
public sealed record OpcXmlDaSubscriptionDto(
    string ServerSubHandle,
    int RevisedSamplingRate,
    string ServerState,
    IReadOnlyList<OpcXmlDaItemValueDto> Items);

/// <summary>
/// JSON-friendly OPC XML-DA polled-refresh result.
/// </summary>
public sealed record OpcXmlDaSubscriptionPollDto(
    string ServerState,
    bool DataBufferOverflow,
    IReadOnlyList<string> InvalidServerSubHandles,
    IReadOnlyList<OpcXmlDaSubscriptionItemListDto> ItemLists);

/// <summary>
/// JSON-friendly OPC XML-DA subscription item list.
/// </summary>
public sealed record OpcXmlDaSubscriptionItemListDto(
    string SubscriptionHandle,
    IReadOnlyList<OpcXmlDaItemValueDto> Items);
