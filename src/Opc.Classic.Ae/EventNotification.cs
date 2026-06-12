//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Ae;

/// <summary>
/// Payload of an OPC AE event delivered via <c>IOPCEventSink::OnEvent</c>.
/// Mirrors the on-the-wire <c>ONEVENTSTRUCT</c> but as a managed init-only class.
/// </summary>
/// <remarks>
/// Fields are optional or sentinel-valued for event types that don't carry them
/// — e.g. <see cref="ConditionName"/> / <see cref="SubConditionName"/> are
/// <see langword="null"/> for simple events; <see cref="AckRequired"/> /
/// <see cref="ActiveTime"/> are only meaningful for condition events.
/// </remarks>
public sealed class EventNotification
{
    /// <summary>Identifier of the event source (item name or area name).</summary>
    public required string Source { get; init; }

    /// <summary>Server-supplied UTC timestamp of the event.</summary>
    public DateTimeOffset Time { get; init; }

    /// <summary>Human-readable event message.</summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>Event severity (0..1000 per OPC AE spec; higher = more severe).</summary>
    public int Severity { get; init; }

    /// <summary>Event-category descriptor (server-defined).</summary>
    public uint EventCategory { get; init; }

    /// <summary>Event-type kind (Simple / Tracking / Condition).</summary>
    public EventType EventType { get; init; } = EventType.Simple;

    /// <summary>For condition events: the condition name (null for simple events).</summary>
    public string? ConditionName { get; init; }

    /// <summary>For condition events: the active sub-condition (null otherwise).</summary>
    public string? SubConditionName { get; init; }

    /// <summary>For condition events: state of the condition at the moment of the event.</summary>
    public ConditionState NewState { get; init; }

    /// <summary>For condition events: whether acknowledgment is required.</summary>
    public bool AckRequired { get; init; }

    /// <summary>For condition events: time the condition became active.</summary>
    public DateTimeOffset ActiveTime { get; init; }

    /// <summary>For condition events: server-assigned cookie used to ack the event.</summary>
    public int Cookie { get; init; }

    /// <summary>For tracking events: the actor (user/operator) name.</summary>
    public string? Actor { get; init; }

    /// <summary>Quality of the event data (analog to DA quality).</summary>
    public OpcQuality Quality { get; init; } = OpcQuality.Good;

    /// <summary>
    /// Per-attribute additional values. Keys are attribute IDs (server-defined)
    /// and values are the corresponding attribute payloads.
    /// </summary>
    public IReadOnlyDictionary<uint, object?> Attributes { get; init; } =
        new Dictionary<uint, object?>();
}
