//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC AE area or source browse element.
/// </summary>
public sealed record OpcAreaBrowseElementDto(
    string Name,
    string QualifiedName,
    bool IsArea,
    bool IsSource);

/// <summary>
/// JSON-friendly OPC AE event category metadata.
/// </summary>
public sealed record OpcEventCategoryDto(
    int EventCategory,
    string Description,
    string EventTypes);

/// <summary>
/// JSON-friendly OPC AE event attribute metadata.
/// </summary>
public sealed record OpcEventAttributeDto(
    int AttributeId,
    string Description,
    ushort VarType,
    string DataType);

/// <summary>
/// JSON-friendly OPC AE event attribute value.
/// </summary>
public sealed record OpcEventAttributeValueDto(
    int? AttributeId,
    object? Value,
    string? ValueType,
    int HResult,
    string Message);

/// <summary>
/// JSON-friendly OPC AE event notification.
/// </summary>
public sealed record OpcEventNotificationDto(
    ushort ChangeMask,
    ushort NewState,
    string? Source,
    DateTimeOffset Time,
    string? Message,
    uint EventType,
    string EventTypeText,
    uint EventCategory,
    uint Severity,
    string? ConditionName,
    string? SubconditionName,
    ushort Quality,
    string QualityText,
    bool AckRequired,
    DateTimeOffset ActiveTime,
    uint Cookie,
    string? ActorId,
    IReadOnlyList<OpcEventAttributeValueDto> Attributes);

/// <summary>
/// JSON-friendly OPC AE condition state snapshot.
/// </summary>
public sealed record OpcConditionStateDto(
    ushort State,
    string StateText,
    string? ActiveSubCondition,
    string? ActiveSubConditionDefinition,
    uint ActiveSubConditionSeverity,
    string? ActiveSubConditionDescription,
    ushort Quality,
    string QualityText,
    DateTimeOffset LastAckTime,
    DateTimeOffset SubConditionLastActive,
    DateTimeOffset ConditionLastActive,
    DateTimeOffset ConditionLastInactive,
    string? AcknowledgerId,
    string? Comment,
    IReadOnlyList<string?> SubConditionNames,
    IReadOnlyList<string?> SubConditionDefinitions,
    IReadOnlyList<uint> SubConditionSeverities,
    IReadOnlyList<string?> SubConditionDescriptions,
    IReadOnlyList<OpcEventAttributeValueDto> EventAttributes);

/// <summary>
/// JSON-friendly OPC AE subscription state.
/// </summary>
public sealed record OpcAeSubscriptionDto(
    string SubscriptionId,
    int ClientSubscription,
    bool Active,
    int BufferTimeMs,
    int MaxBufferSize,
    int RevisedBufferTimeMs,
    int RevisedMaxBufferSize,
    int QueuedEventCount);
