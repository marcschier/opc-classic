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
public static class NdrOpcConditionStateCodec {
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes a single OPCCONDITIONSTATE in NDR (matching the MS-DCOM
    /// proxy/stub wire format: primary fields first, deferred pointer bodies after).</summary>
    public static void Write(ref NdrWriter writer, OpcConditionState state) {
        ArgumentNullException.ThrowIfNull(state);

        WritePrimary(ref writer, state);
        WriteDeferred(ref writer, state);
    }

    private static void WritePrimary(ref NdrWriter writer, OpcConditionState state) {
        bool hasActiveSubCondition = state.ActiveSubCondition is not null;
        bool hasActiveDefinition = state.ActiveSubConditionDefinition is not null;
        bool hasActiveDescription = state.ActiveSubConditionDescription is not null;
        bool hasAcknowledger = state.AcknowledgerId is not null;
        bool hasComment = state.Comment is not null;
        int subCount = state.SubConditionCount;
        int eventAttrCount = state.EventAttributeCount;

        writer.WriteUInt16(state.State);
        writer.WriteUInt16(0); // wReserved1
        WriteReferent(ref writer, hasActiveSubCondition);
        WriteReferent(ref writer, hasActiveDefinition);
        writer.WriteUInt32(state.ActiveSubConditionSeverity);
        WriteReferent(ref writer, hasActiveDescription);
        writer.WriteUInt16(state.Quality.RawValue);
        writer.WriteUInt16(0); // wReserved2
        writer.WriteFileTime(ToFileTime(state.LastAckTime));
        writer.WriteFileTime(ToFileTime(state.SubConditionLastActive));
        writer.WriteFileTime(ToFileTime(state.ConditionLastActive));
        writer.WriteFileTime(ToFileTime(state.ConditionLastInactive));
        WriteReferent(ref writer, hasAcknowledger);
        WriteReferent(ref writer, hasComment);
        writer.WriteUInt32(unchecked((uint)subCount));
        WriteReferent(ref writer, subCount > 0);
        WriteReferent(ref writer, subCount > 0);
        WriteReferent(ref writer, subCount > 0);
        WriteReferent(ref writer, subCount > 0);
        writer.WriteUInt32(unchecked((uint)eventAttrCount));
        WriteReferent(ref writer, eventAttrCount > 0);
        WriteReferent(ref writer, eventAttrCount > 0);
    }

    private static void WriteDeferred(ref NdrWriter writer, OpcConditionState state) {
        if (state.ActiveSubCondition is string s1) { writer.WriteUnicodeString(s1); }
        if (state.ActiveSubConditionDefinition is string s2) { writer.WriteUnicodeString(s2); }
        if (state.ActiveSubConditionDescription is string s3) { writer.WriteUnicodeString(s3); }
        if (state.AcknowledgerId is string s4) { writer.WriteUnicodeString(s4); }
        if (state.Comment is string s5) { writer.WriteUnicodeString(s5); }
        if (state.SubConditionCount > 0) {
            WriteLpwstrArrayBody(ref writer, state.SubConditionNames);
            WriteLpwstrArrayBody(ref writer, state.SubConditionDefinitions);
            writer.WriteConformantUInt32Array(state.SubConditionSeverities);
            WriteLpwstrArrayBody(ref writer, state.SubConditionDescriptions);
        }
        if (state.EventAttributeCount > 0) {
            WriteVariantArrayBody(ref writer, state.EventAttributes);
            writer.WriteConformantInt32Array(state.Errors);
        }
    }

    /// <summary>Decodes a single OPCCONDITIONSTATE from NDR (matching the MS-DCOM
    /// proxy/stub wire format).</summary>
    public static OpcConditionState Read(ref NdrReader reader) {
        ConditionStatePrimary primary = ReadPrimary(ref reader);
        return ReadDeferred(ref reader, primary);
    }

    private static ConditionStatePrimary ReadPrimary(ref NdrReader reader) {
        ushort conditionState = reader.ReadUInt16();
        _ = reader.ReadUInt16(); // wReserved1
        bool hasActiveSubCondition = reader.ReadUInt32() != 0;
        bool hasActiveDefinition = reader.ReadUInt32() != 0;
        uint activeSeverity = reader.ReadUInt32();
        bool hasActiveDescription = reader.ReadUInt32() != 0;
        var quality = new OpcQuality(reader.ReadUInt16());
        _ = reader.ReadUInt16(); // wReserved2
        DateTimeOffset lastAckTime = ReadAndDecodeFileTime(ref reader, "ftLastAckTime");
        DateTimeOffset subConditionLastActive = ReadAndDecodeFileTime(ref reader, "ftSubCondLastActive");
        DateTimeOffset conditionLastActive = ReadAndDecodeFileTime(ref reader, "ftCondLastActive");
        DateTimeOffset conditionLastInactive = ReadAndDecodeFileTime(ref reader, "ftCondLastInactive");
        bool hasAcknowledger = reader.ReadUInt32() != 0;
        bool hasComment = reader.ReadUInt32() != 0;
        int subCount = ToInt32Count(reader.ReadUInt32(), "dwNumSCs");
        bool hasSubNames = reader.ReadUInt32() != 0;
        bool hasSubDefinitions = reader.ReadUInt32() != 0;
        bool hasSubSeverities = reader.ReadUInt32() != 0;
        bool hasSubDescriptions = reader.ReadUInt32() != 0;
        int eventAttrCount = ToInt32Count(reader.ReadUInt32(), "dwNumEventAttrs");
        bool hasEventAttrs = reader.ReadUInt32() != 0;
        bool hasErrors = reader.ReadUInt32() != 0;

        return new ConditionStatePrimary(
            conditionState, hasActiveSubCondition, hasActiveDefinition, activeSeverity, hasActiveDescription,
            quality, lastAckTime, subConditionLastActive, conditionLastActive, conditionLastInactive,
            hasAcknowledger, hasComment, subCount, hasSubNames, hasSubDefinitions, hasSubSeverities, hasSubDescriptions,
            eventAttrCount, hasEventAttrs, hasErrors);
    }

    private static OpcConditionState ReadDeferred(ref NdrReader reader, ConditionStatePrimary primary) {
        string? activeSubCondition = primary.HasActiveSubCondition ? reader.ReadUnicodeString() : null;
        string? activeDefinition = primary.HasActiveDefinition ? reader.ReadUnicodeString() : null;
        string? activeDescription = primary.HasActiveDescription ? reader.ReadUnicodeString() : null;
        string? acknowledgerId = primary.HasAcknowledger ? reader.ReadUnicodeString() : null;
        string? comment = primary.HasComment ? reader.ReadUnicodeString() : null;
        string?[] subNames = primary.HasSubNames ? ReadLpwstrArrayBody(ref reader, primary.SubCount) : Array.Empty<string?>();
        string?[] subDefinitions = primary.HasSubDefinitions ? ReadLpwstrArrayBody(ref reader, primary.SubCount) : Array.Empty<string?>();
        uint[] subSeverities = primary.HasSubSeverities ? reader.ReadConformantUInt32Array() : Array.Empty<uint>();
        string?[] subDescriptions = primary.HasSubDescriptions ? ReadLpwstrArrayBody(ref reader, primary.SubCount) : Array.Empty<string?>();
        OpcVariant[] eventAttrs = primary.HasEventAttrs ? ReadVariantArrayBody(ref reader, primary.EventAttrCount) : Array.Empty<OpcVariant>();
        int[] errors = primary.HasErrors ? reader.ReadConformantInt32Array() : Array.Empty<int>();

        return new OpcConditionState(
            primary.State, activeSubCondition, activeDefinition, primary.ActiveSeverity, activeDescription,
            primary.Quality, primary.LastAckTime, primary.SubConditionLastActive,
            primary.ConditionLastActive, primary.ConditionLastInactive,
            acknowledgerId, comment, subNames, subDefinitions, subSeverities, subDescriptions,
            eventAttrs, errors);
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
    private readonly record struct ConditionStatePrimary(
        ushort State, bool HasActiveSubCondition, bool HasActiveDefinition, uint ActiveSeverity, bool HasActiveDescription,
        OpcQuality Quality, DateTimeOffset LastAckTime, DateTimeOffset SubConditionLastActive,
        DateTimeOffset ConditionLastActive, DateTimeOffset ConditionLastInactive,
        bool HasAcknowledger, bool HasComment, int SubCount, bool HasSubNames, bool HasSubDefinitions, bool HasSubSeverities, bool HasSubDescriptions,
        int EventAttrCount, bool HasEventAttrs, bool HasErrors);

    private static void WriteReferent(ref NdrWriter writer, bool hasValue) {
        if (hasValue) {
            _ = writer.WriteReferentId();
        }
        else {
            writer.WriteNullReferent();
        }
    }

    private static void WriteLpwstrArrayBody(ref NdrWriter writer, string?[] values) {
        // Array body: max_count + per-element [unique] referents + per-element bodies (deferred within array).
        writer.WriteUInt32(unchecked((uint)values.Length));
        for (int i = 0; i < values.Length; i++) {
            WriteReferent(ref writer, values[i] is not null);
        }
        for (int i = 0; i < values.Length; i++) {
            if (values[i] is string body) {
                writer.WriteUnicodeString(body);
            }
        }
    }

    private static string?[] ReadLpwstrArrayBody(ref NdrReader reader, int count) {
        uint rawConformance = reader.ReadUInt32();
        int conformance = ToInt32Count(rawConformance, "pszSC array");
        if (conformance != count) {
            throw new InvalidDataException($"OPCCONDITIONSTATE pszSC array conformance {conformance} did not match dwNumSCs {count}.");
        }
        var refs = new bool[count];
        for (int i = 0; i < count; i++) {
            refs[i] = reader.ReadUInt32() != 0;
        }
        var values = new string?[count];
        for (int i = 0; i < count; i++) {
            values[i] = refs[i] ? reader.ReadUnicodeString() : null;
        }
        return values;
    }

    private static void WriteVariantArrayBody(ref NdrWriter writer, OpcVariant[] attributes) {
        writer.WriteUInt32(unchecked((uint)attributes.Length));
        for (int i = 0; i < attributes.Length; i++) {
            writer.WriteVariant(attributes[i]);
        }
    }

    private static OpcVariant[] ReadVariantArrayBody(ref NdrReader reader, int count) {
        uint conformance = reader.ReadUInt32();
        int observed = ToInt32Count(conformance, "pEventAttributes");
        if (observed != count) {
            throw new InvalidDataException($"OPCCONDITIONSTATE pEventAttributes conformance {observed} did not match dwNumEventAttrs {count}.");
        }
        var attributes = new OpcVariant[count];
        for (int i = 0; i < count; i++) {
            attributes[i] = reader.ReadVariant();
        }
        return attributes;
    }

    private static void ValidateArrayLength(Array array, int expectedLength, string arrayName, string countName) {
        if (array.Length != expectedLength) {
            throw new InvalidDataException(
                $"OPCCONDITIONSTATE {arrayName} length {array.Length} did not match {countName} {expectedLength}.");
        }
    }

    private static int ToInt32Count(uint count, string fieldName) {
        if (count > (uint)int.MaxValue) {
            throw new InvalidDataException($"OPCCONDITIONSTATE {fieldName} {count} too large.");
        }
        return unchecked((int)count);
    }

    private static long ToFileTime(DateTimeOffset value) => value.UtcTicks - FileTimeEpochOffsetTicks;

    private static DateTimeOffset ReadAndDecodeFileTime(ref NdrReader reader, string fieldName) {
        long raw = reader.ReadFileTime();
        if (FileTimeHelper.TryFromFileTime(raw, out DateTimeOffset value)) {
            return value;
        }
        throw new InvalidDataException(
            $"OPCCONDITIONSTATE.{fieldName} FILETIME value 0x{raw:X16} ({raw}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
    }
}
