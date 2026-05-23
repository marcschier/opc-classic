//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // File name is fixed by phase contract; type name preserves NDR codec convention.

using System;
using System.IO;
using Opc.Classic.Ndr;

namespace Opc.Classic.Ae.Ndr;

/// <summary>
/// NDR encoder / decoder for OPC AE's <c>ONEVENTSTRUCT</c> notification payload.
/// </summary>
public static class NdrOpcEventNotificationCodec
{
    private const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);
    private const int Win32BoolFalse = 0;
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes a single ONEVENTSTRUCT in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcEventNotification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        writer.WriteUInt16(notification.ChangeMask);
        writer.WriteUInt16(notification.NewState);
        writer.WriteUnicodeStringPtr(notification.Source);
        writer.WriteFileTime(ToFileTime(notification.Time));
        writer.WriteUnicodeStringPtr(notification.Message);
        writer.WriteUInt32(notification.EventType);
        writer.WriteUInt32(notification.EventCategory);
        writer.WriteUInt32(notification.Severity);
        writer.WriteUnicodeStringPtr(notification.ConditionName);
        writer.WriteUnicodeStringPtr(notification.SubconditionName);
        writer.WriteUInt16(notification.Quality.RawValue);
        writer.WriteUInt16(0);
        writer.WriteInt32(notification.AckRequired ? Win32BoolTrue : Win32BoolFalse);
        writer.WriteFileTime(ToFileTime(notification.ActiveTime));
        writer.WriteUInt32(notification.Cookie);
        WriteEventAttributes(ref writer, notification.EventAttributes);
        writer.WriteUnicodeStringPtr(notification.ActorId);
    }

    /// <summary>Decodes a single ONEVENTSTRUCT from NDR.</summary>
    public static OpcEventNotification Read(ref NdrReader reader)
    {
        ushort changeMask = reader.ReadUInt16();
        ushort newState = reader.ReadUInt16();
        string? source = reader.ReadUnicodeStringPtr();
        DateTimeOffset time = FromFileTime(reader.ReadFileTime());
        string? message = reader.ReadUnicodeStringPtr();
        uint eventType = reader.ReadUInt32();
        uint eventCategory = reader.ReadUInt32();
        uint severity = reader.ReadUInt32();
        string? conditionName = reader.ReadUnicodeStringPtr();
        string? subconditionName = reader.ReadUnicodeStringPtr();
        var quality = new OpcQuality(reader.ReadUInt16());
        _ = reader.ReadUInt16();
        bool ackRequired = reader.ReadInt32() != 0;
        DateTimeOffset activeTime = FromFileTime(reader.ReadFileTime());
        uint cookie = reader.ReadUInt32();
        OpcVariant[] eventAttributes = ReadEventAttributes(ref reader);
        string? actorId = reader.ReadUnicodeStringPtr();

        return new OpcEventNotification(
            changeMask,
            newState,
            source,
            time,
            message,
            eventType,
            eventCategory,
            severity,
            conditionName,
            subconditionName,
            quality,
            ackRequired,
            activeTime,
            cookie,
            eventAttributes,
            actorId);
    }

    private static void WriteEventAttributes(ref NdrWriter writer, OpcVariant[] attributes)
    {
        int count = attributes.Length;
        writer.WriteUInt32(unchecked((uint)count));
        writer.WriteUInt32(unchecked((uint)count));
        for (int i = 0; i < count; i++)
        {
            writer.WriteVariant(attributes[i]);
        }
    }

    private static OpcVariant[] ReadEventAttributes(ref NdrReader reader)
    {
        uint rawCount = reader.ReadUInt32();
        int count = ToInt32Count(rawCount, "dwNumEventAttrs");
        uint conformance = reader.ReadUInt32();
        if (conformance != rawCount)
        {
            throw new InvalidDataException(
                $"ONEVENTSTRUCT pEventAttributes conformance {conformance} did not match dwNumEventAttrs {rawCount}.");
        }

        var attributes = new OpcVariant[count];
        for (int i = 0; i < count; i++)
        {
            attributes[i] = reader.ReadVariant();
        }
        return attributes;
    }

    private static int ToInt32Count(uint count, string fieldName)
    {
        if (count > (uint)int.MaxValue)
        {
            throw new InvalidDataException($"ONEVENTSTRUCT {fieldName} {count} too large.");
        }
        return unchecked((int)count);
    }

    private static long ToFileTime(DateTimeOffset value) => value.UtcTicks - FileTimeEpochOffsetTicks;

    private static DateTimeOffset FromFileTime(long fileTimeTicks) =>
        new(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
}
