//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using OpcClassic.Ndr;

namespace OpcClassic.Hda.Ndr;

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
    public static void Write(ref NdrWriter writer, OpcHdaTime value)
    {
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteInt32(value.IsStringExpression ? Win32BoolTrue : 0);
        writer.WriteUnicodeStringPtr(value.StringExpression);

        long fileTimeTicks = value.IsStringExpression
            ? 0L
            : value.Timestamp.UtcTicks - FileTimeEpochOffsetTicks;
        writer.WriteFileTime(fileTimeTicks);
    }

    /// <summary>Decodes a single OPCHDA_TIME from NDR.</summary>
    public static OpcHdaTime Read(ref NdrReader reader)
    {
        int bString = reader.ReadInt32();
        string? szTime = reader.ReadUnicodeStringPtr();
        long fileTimeTicks = reader.ReadFileTime();

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
