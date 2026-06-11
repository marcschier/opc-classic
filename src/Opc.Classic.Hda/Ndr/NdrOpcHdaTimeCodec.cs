//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC HDA <c>OPCHDA_TIME</c> struct,
/// matching <c>tagOPCHDA_TIME</c> in opchda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     INT32     bString          - Win32 BOOL (-1 = use szTime, 0 = use ftTime)
///     LPWSTR    szTime           - server-side parsed when bString != 0
///     UINT32    filetime.dwLowDateTime
///     UINT32    filetime.dwHighDateTime
/// </code>
/// Both szTime and ftTime are always present in the wire form; the
/// <c>bString</c> flag tells the consumer which to honour.
/// </remarks>
public static class NdrOpcHdaTimeCodec
{
    private const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes a single OPCHDA_TIME in NDR.</summary>
    /// <remarks>
    /// NDR layout per OPC HDA 1.20 §3.4 (OPCHDA_TIME struct):
    ///   primary part:  BOOL bString (4) + LPWSTR szTime referent (4) + FILETIME ftTime (8)
    ///   deferred part: szTime body (max_count + offset + actual_count + WSTR chars)
    /// The OS proxy/stub serializes all primary parts first then deferred parts,
    /// so a naive inline write of WriteUnicodeStringPtr (referent + body) puts
    /// the body BEFORE ftTime and breaks RPC decoding (RPC_S_BAD_STUB_DATA).
    /// </remarks>
    public static void Write(ref NdrWriter writer, OpcHdaTime value)
    {
        ArgumentNullException.ThrowIfNull(value);

        long fileTimeTicks = value.IsStringExpression
            ? 0L
            : value.Timestamp.UtcTicks - FileTimeEpochOffsetTicks;

        // Primary part: bString + szTime referent + ftTime.
        writer.WriteInt32(value.IsStringExpression ? Win32BoolTrue : 0);
        bool hasString = value.StringExpression is not null;
        if (hasString)
        {
            _ = writer.WriteReferentId();
        }
        else
        {
            writer.WriteNullReferent();
        }
        writer.WriteFileTime(fileTimeTicks);

        // Deferred part: szTime body, only when the referent is non-null.
        if (hasString)
        {
            writer.WriteUnicodeString(value.StringExpression!);
        }
    }

    /// <summary>Decodes a single OPCHDA_TIME from NDR.</summary>
    public static OpcHdaTime Read(ref NdrReader reader)
    {
        int bString = reader.ReadInt32();
        uint szTimeRef = reader.ReadUInt32();
        long fileTimeTicks = reader.ReadFileTime();
        string? szTime = szTimeRef != 0 ? reader.ReadUnicodeString() : null;

        bool isString = bString != 0;
        DateTimeOffset timestamp = isString
            ? default
            : new DateTimeOffset(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);

        return new OpcHdaTime(
            IsStringExpression: isString,
            StringExpression: isString ? szTime : null,
            Timestamp: timestamp);
    }
}
