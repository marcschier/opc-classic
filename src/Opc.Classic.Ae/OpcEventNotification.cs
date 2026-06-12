//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Ae;

/// <summary>
/// Managed projection of OPC AE's <c>ONEVENTSTRUCT</c> notification payload.
/// </summary>
public sealed record OpcEventNotification
{
    /// <summary>
    /// Constructor for the immutable event notification payload.
    /// </summary>
    public OpcEventNotification(
        ushort changeMask,
        ushort newState,
        string? source,
        DateTimeOffset time,
        string? message,
        uint eventType,
        uint eventCategory,
        uint severity,
        string? conditionName,
        string? subconditionName,
        OpcQuality quality,
        bool ackRequired,
        DateTimeOffset activeTime,
        uint cookie,
        OpcVariant[] eventAttributes,
        string? actorId)
    {
        ArgumentNullException.ThrowIfNull(eventAttributes);

        ChangeMask = changeMask;
        NewState = newState;
        Source = source;
        Time = time;
        Message = message;
        EventType = eventType;
        EventCategory = eventCategory;
        Severity = severity;
        ConditionName = conditionName;
        SubconditionName = subconditionName;
        Quality = quality;
        AckRequired = ackRequired;
        ActiveTime = activeTime;
        Cookie = cookie;
        EventAttributes = eventAttributes;
        ActorId = actorId;
    }

    /// <summary>
    /// Condition change mask.
    /// </summary>
    public ushort ChangeMask { get; }

    /// <summary>
    /// New condition state.
    /// </summary>
    public ushort NewState { get; }

    /// <summary>
    /// Event source name.
    /// </summary>
    public string? Source { get; }

    /// <summary>
    /// Server-supplied UTC event timestamp.
    /// </summary>
    public DateTimeOffset Time { get; }

    /// <summary>
    /// Human-readable event message.
    /// </summary>
    public string? Message { get; }

    /// <summary>
    /// OPC AE event type.
    /// </summary>
    public uint EventType { get; }

    /// <summary>
    /// Server-defined event category.
    /// </summary>
    public uint EventCategory { get; }

    /// <summary>
    /// Event severity.
    /// </summary>
    public uint Severity { get; }

    /// <summary>
    /// Condition name for condition events.
    /// </summary>
    public string? ConditionName { get; }

    /// <summary>
    /// Subcondition name for condition events.
    /// </summary>
    public string? SubconditionName { get; }

    /// <summary>
    /// OPC quality associated with the event data.
    /// </summary>
    public OpcQuality Quality { get; }

    /// <summary>
    /// Whether the event requires acknowledgment.
    /// </summary>
    public bool AckRequired { get; }

    /// <summary>
    /// UTC timestamp when the condition became active.
    /// </summary>
    public DateTimeOffset ActiveTime { get; }

    /// <summary>
    /// Server-assigned cookie used when acknowledging the event.
    /// </summary>
    public uint Cookie { get; }

    /// <summary>
    /// Event attribute values in server-defined attribute order.
    /// </summary>
    public OpcVariant[] EventAttributes { get; }

    /// <summary>
    /// Tracking-event actor identifier.
    /// </summary>
    public string? ActorId { get; }
}
