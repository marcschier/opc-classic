//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Ae;

/// <summary>
/// Managed projection of OPC AE's <c>OPCCONDITIONSTATE</c> payload returned by
/// <c>IOPCEventServer::GetConditionState</c>.
/// </summary>
public sealed record OpcConditionState
{
    /// <summary>Constructor for the immutable condition-state payload.</summary>
    public OpcConditionState(
        ushort state,
        string? activeSubCondition,
        string? activeSubConditionDefinition,
        uint activeSubConditionSeverity,
        string? activeSubConditionDescription,
        OpcQuality quality,
        DateTimeOffset lastAckTime,
        DateTimeOffset subConditionLastActive,
        DateTimeOffset conditionLastActive,
        DateTimeOffset conditionLastInactive,
        string? acknowledgerId,
        string? comment,
        string?[] subConditionNames,
        string?[] subConditionDefinitions,
        uint[] subConditionSeverities,
        string?[] subConditionDescriptions,
        OpcVariant[] eventAttributes,
        int[] errors)
    {
        ArgumentNullException.ThrowIfNull(subConditionNames);
        ArgumentNullException.ThrowIfNull(subConditionDefinitions);
        ArgumentNullException.ThrowIfNull(subConditionSeverities);
        ArgumentNullException.ThrowIfNull(subConditionDescriptions);
        ArgumentNullException.ThrowIfNull(eventAttributes);
        ArgumentNullException.ThrowIfNull(errors);

        int subConditionCount = subConditionNames.Length;
        ValidateLength(subConditionDefinitions, subConditionCount, nameof(subConditionDefinitions), "dwNumSCs");
        ValidateLength(subConditionSeverities, subConditionCount, nameof(subConditionSeverities), "dwNumSCs");
        ValidateLength(subConditionDescriptions, subConditionCount, nameof(subConditionDescriptions), "dwNumSCs");
        ValidateLength(errors, eventAttributes.Length, nameof(errors), "dwNumEventAttrs");

        State = state;
        ActiveSubCondition = activeSubCondition;
        ActiveSubConditionDefinition = activeSubConditionDefinition;
        ActiveSubConditionSeverity = activeSubConditionSeverity;
        ActiveSubConditionDescription = activeSubConditionDescription;
        Quality = quality;
        LastAckTime = lastAckTime;
        SubConditionLastActive = subConditionLastActive;
        ConditionLastActive = conditionLastActive;
        ConditionLastInactive = conditionLastInactive;
        AcknowledgerId = acknowledgerId;
        Comment = comment;
        SubConditionNames = subConditionNames;
        SubConditionDefinitions = subConditionDefinitions;
        SubConditionSeverities = subConditionSeverities;
        SubConditionDescriptions = subConditionDescriptions;
        EventAttributes = eventAttributes;
        Errors = errors;
    }

    /// <summary>Condition state flags as the AE wire <c>WORD</c>.</summary>
    public ushort State { get; }

    /// <summary>Currently active sub-condition name.</summary>
    public string? ActiveSubCondition { get; }

    /// <summary>Definition text for the active sub-condition.</summary>
    public string? ActiveSubConditionDefinition { get; }

    /// <summary>Severity associated with the active sub-condition.</summary>
    public uint ActiveSubConditionSeverity { get; }

    /// <summary>Description text for the active sub-condition.</summary>
    public string? ActiveSubConditionDescription { get; }

    /// <summary>OPC quality associated with the condition state.</summary>
    public OpcQuality Quality { get; }

    /// <summary>UTC time when the condition was last acknowledged.</summary>
    public DateTimeOffset LastAckTime { get; }

    /// <summary>UTC time when this sub-condition last became active.</summary>
    public DateTimeOffset SubConditionLastActive { get; }

    /// <summary>UTC time when the condition last became active.</summary>
    public DateTimeOffset ConditionLastActive { get; }

    /// <summary>UTC time when the condition last became inactive.</summary>
    public DateTimeOffset ConditionLastInactive { get; }

    /// <summary>Identifier of the last acknowledger.</summary>
    public string? AcknowledgerId { get; }

    /// <summary>Acknowledgement comment.</summary>
    public string? Comment { get; }

    /// <summary>Configured sub-condition names.</summary>
    public string?[] SubConditionNames { get; }

    /// <summary>Configured sub-condition definitions.</summary>
    public string?[] SubConditionDefinitions { get; }

    /// <summary>Configured sub-condition severities.</summary>
    public uint[] SubConditionSeverities { get; }

    /// <summary>Configured sub-condition descriptions.</summary>
    public string?[] SubConditionDescriptions { get; }

    /// <summary>Server-defined event attribute values.</summary>
    public OpcVariant[] EventAttributes { get; }

    /// <summary>Per-event-attribute HRESULT values.</summary>
    public int[] Errors { get; }

    /// <summary>Number of configured sub-conditions.</summary>
    public int SubConditionCount => SubConditionNames.Length;

    /// <summary>Number of event attributes.</summary>
    public int EventAttributeCount => EventAttributes.Length;

    private static void ValidateLength(Array array, int expectedLength, string arrayName, string wireCountName)
    {
        if (array.Length != expectedLength)
        {
            throw new ArgumentException(
                $"{arrayName} length {array.Length} must equal {wireCountName} {expectedLength}.",
                arrayName);
        }
    }
}
