//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using Opc.Classic.Ndr;

namespace Opc.Classic.Ae.Ndr;

/// <summary>
/// NDR encoder / decoder for OPC AE's <c>OPCCONDITIONSTATE</c> payload.
/// </summary>
public static class NdrOpcConditionStateCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes a single OPCCONDITIONSTATE in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcConditionState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        writer.WriteUInt16(state.State);
        writer.WriteUInt16(0);
        writer.WriteUnicodeStringPtr(state.ActiveSubCondition);
        writer.WriteUnicodeStringPtr(state.ActiveSubConditionDefinition);
        writer.WriteUInt32(state.ActiveSubConditionSeverity);
        writer.WriteUnicodeStringPtr(state.ActiveSubConditionDescription);
        writer.WriteUInt16(state.Quality.RawValue);
        writer.WriteUInt16(0);
        WriteTimestamps(ref writer, state);
        writer.WriteUnicodeStringPtr(state.AcknowledgerId);
        writer.WriteUnicodeStringPtr(state.Comment);
        WriteSubConditions(ref writer, state);
        WriteEventAttributes(ref writer, state.EventAttributes, state.Errors);
    }

    /// <summary>Decodes a single OPCCONDITIONSTATE from NDR.</summary>
    public static OpcConditionState Read(ref NdrReader reader)
    {
        ushort conditionState = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        string? activeSubCondition = reader.ReadUnicodeStringPtr();
        string? activeDefinition = reader.ReadUnicodeStringPtr();
        uint activeSeverity = reader.ReadUInt32();
        string? activeDescription = reader.ReadUnicodeStringPtr();
        var quality = new OpcQuality(reader.ReadUInt16());
        _ = reader.ReadUInt16();
        DateTimeOffset lastAckTime = FromFileTime(reader.ReadFileTime());
        DateTimeOffset subConditionLastActive = FromFileTime(reader.ReadFileTime());
        DateTimeOffset conditionLastActive = FromFileTime(reader.ReadFileTime());
        DateTimeOffset conditionLastInactive = FromFileTime(reader.ReadFileTime());
        string? acknowledgerId = reader.ReadUnicodeStringPtr();
        string? comment = reader.ReadUnicodeStringPtr();
        var subConditions = ReadSubConditions(ref reader);
        var eventAttributes = ReadEventAttributes(ref reader);

        return new OpcConditionState(
            conditionState,
            activeSubCondition,
            activeDefinition,
            activeSeverity,
            activeDescription,
            quality,
            lastAckTime,
            subConditionLastActive,
            conditionLastActive,
            conditionLastInactive,
            acknowledgerId,
            comment,
            subConditions.Names,
            subConditions.Definitions,
            subConditions.Severities,
            subConditions.Descriptions,
            eventAttributes.Attributes,
            eventAttributes.Errors);
    }

    private static void WriteTimestamps(ref NdrWriter writer, OpcConditionState state)
    {
        writer.WriteFileTime(ToFileTime(state.LastAckTime));
        writer.WriteFileTime(ToFileTime(state.SubConditionLastActive));
        writer.WriteFileTime(ToFileTime(state.ConditionLastActive));
        writer.WriteFileTime(ToFileTime(state.ConditionLastInactive));
    }

    private static void WriteSubConditions(ref NdrWriter writer, OpcConditionState state)
    {
        writer.WriteUInt32(unchecked((uint)state.SubConditionCount));
        WriteLpwstrArray(ref writer, state.SubConditionNames);
        WriteLpwstrArray(ref writer, state.SubConditionDefinitions);
        writer.WriteConformantUInt32Array(state.SubConditionSeverities);
        WriteLpwstrArray(ref writer, state.SubConditionDescriptions);
    }

    private static (string?[] Names, string?[] Definitions, uint[] Severities, string?[] Descriptions) ReadSubConditions(
        ref NdrReader reader)
    {
        uint rawCount = reader.ReadUInt32();
        int count = ToInt32Count(rawCount, "dwNumSCs");
        string?[] names = ReadLpwstrArray(ref reader, count, "pszSCNames");
        string?[] definitions = ReadLpwstrArray(ref reader, count, "pszSCDefinitions");
        uint[] severities = reader.ReadConformantUInt32Array();
        ValidateArrayLength(severities, count, "pdwSCSeverities", "dwNumSCs");
        string?[] descriptions = ReadLpwstrArray(ref reader, count, "pszSCDescriptions");

        return (names, definitions, severities, descriptions);
    }

    private static void WriteLpwstrArray(ref NdrWriter writer, string?[] values)
    {
        writer.WriteUInt32(unchecked((uint)values.Length));
        for (int i = 0; i < values.Length; i++)
        {
            writer.WriteUnicodeStringPtr(values[i]);
        }
    }

    private static string?[] ReadLpwstrArray(ref NdrReader reader, int count, string fieldName)
    {
        uint rawConformance = reader.ReadUInt32();
        int conformance = ToInt32Count(rawConformance, fieldName);
        if (conformance != count)
        {
            throw new InvalidDataException(
                $"OPCCONDITIONSTATE {fieldName} conformance {conformance} did not match dwNumSCs {count}.");
        }

        var values = new string?[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = reader.ReadUnicodeStringPtr();
        }
        return values;
    }

    private static void WriteEventAttributes(ref NdrWriter writer, OpcVariant[] attributes, int[] errors)
    {
        writer.WriteUInt32(unchecked((uint)attributes.Length));
        writer.WriteUInt32(unchecked((uint)attributes.Length));
        for (int i = 0; i < attributes.Length; i++)
        {
            writer.WriteVariant(attributes[i]);
        }
        writer.WriteConformantInt32Array(errors);
    }

    private static (OpcVariant[] Attributes, int[] Errors) ReadEventAttributes(ref NdrReader reader)
    {
        uint rawCount = reader.ReadUInt32();
        int count = ToInt32Count(rawCount, "dwNumEventAttrs");
        OpcVariant[] attributes = ReadVariantArray(ref reader, count, rawCount);
        int[] errors = reader.ReadConformantInt32Array();
        ValidateArrayLength(errors, count, "pErrors", "dwNumEventAttrs");
        return (attributes, errors);
    }

    private static OpcVariant[] ReadVariantArray(ref NdrReader reader, int count, uint rawCount)
    {
        uint conformance = reader.ReadUInt32();
        if (conformance != rawCount)
        {
            throw new InvalidDataException(
                $"OPCCONDITIONSTATE pEventAttributes conformance {conformance} did not match dwNumEventAttrs {rawCount}.");
        }

        var attributes = new OpcVariant[count];
        for (int i = 0; i < count; i++)
        {
            attributes[i] = reader.ReadVariant();
        }
        return attributes;
    }

    private static void ValidateArrayLength(Array array, int expectedLength, string arrayName, string countName)
    {
        if (array.Length != expectedLength)
        {
            throw new InvalidDataException(
                $"OPCCONDITIONSTATE {arrayName} length {array.Length} did not match {countName} {expectedLength}.");
        }
    }

    private static int ToInt32Count(uint count, string fieldName)
    {
        if (count > (uint)int.MaxValue)
        {
            throw new InvalidDataException($"OPCCONDITIONSTATE {fieldName} {count} too large.");
        }
        return unchecked((int)count);
    }

    private static long ToFileTime(DateTimeOffset value) => value.UtcTicks - FileTimeEpochOffsetTicks;

    private static DateTimeOffset FromFileTime(long fileTimeTicks) =>
        new(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
}
