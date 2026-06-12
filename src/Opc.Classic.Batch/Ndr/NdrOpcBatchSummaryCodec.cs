//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Ndr;

namespace Opc.Classic.Batch.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC Batch <c>OPCBATCHSUMMARY</c> struct,
/// matching <c>tagOPCBATCHSUMMARY</c> in opcbc.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     LPWSTR    szID                 - unique-pointer LPWSTR
///     LPWSTR    szDescription        - unique-pointer LPWSTR
///     LPWSTR    szOPCItemID          - unique-pointer LPWSTR
///     LPWSTR    szMasterRecipeID     - unique-pointer LPWSTR
///     FLOAT     fBatchSize
///     LPWSTR    szEU                 - unique-pointer LPWSTR
///     LPWSTR    szExecutionState     - unique-pointer LPWSTR
///     LPWSTR    szExecutionMode      - unique-pointer LPWSTR
///     FILETIME  ftActualStartTime
///     FILETIME  ftActualEndTime
/// </code>
/// </remarks>
public static class NdrOpcBatchSummaryCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L; // 1601-01-01 UTC in .NET ticks

    /// <summary>Encodes a single OPCBATCHSUMMARY in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcBatchSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        writer.WriteUnicodeStringPtr(summary.Id);
        writer.WriteUnicodeStringPtr(summary.Description);
        writer.WriteUnicodeStringPtr(summary.OpcItemId);
        writer.WriteUnicodeStringPtr(summary.MasterRecipeId);
        writer.WriteSingle(summary.BatchSize);
        writer.WriteUnicodeStringPtr(summary.EngineeringUnits);
        writer.WriteUnicodeStringPtr(summary.ExecutionState);
        writer.WriteUnicodeStringPtr(summary.ExecutionMode);
        writer.WriteFileTime(ToFileTime(summary.ActualStartTime));
        writer.WriteFileTime(ToFileTime(summary.ActualEndTime));
    }

    /// <summary>Decodes a single OPCBATCHSUMMARY from NDR.</summary>
    public static OpcBatchSummary Read(ref NdrReader reader)
    {
        string? id = reader.ReadUnicodeStringPtr();
        string? description = reader.ReadUnicodeStringPtr();
        string? opcItemId = reader.ReadUnicodeStringPtr();
        string? masterRecipeId = reader.ReadUnicodeStringPtr();
        float batchSize = reader.ReadSingle();
        string? engineeringUnits = reader.ReadUnicodeStringPtr();
        string? executionState = reader.ReadUnicodeStringPtr();
        string? executionMode = reader.ReadUnicodeStringPtr();
        DateTimeOffset actualStartTime = ReadAndDecodeFileTime(ref reader, "ftActualStartTime");
        DateTimeOffset actualEndTime = ReadAndDecodeFileTime(ref reader, "ftActualEndTime");

        return new OpcBatchSummary(
            Id: id,
            Description: description,
            OpcItemId: opcItemId,
            MasterRecipeId: masterRecipeId,
            BatchSize: batchSize,
            EngineeringUnits: engineeringUnits,
            ExecutionState: executionState,
            ExecutionMode: executionMode,
            ActualStartTime: actualStartTime,
            ActualEndTime: actualEndTime);
    }

    private static long ToFileTime(DateTimeOffset value) =>
        value.UtcTicks - FileTimeEpochOffsetTicks;

    private static DateTimeOffset ReadAndDecodeFileTime(ref NdrReader reader, string fieldName)
    {
        long raw = reader.ReadFileTime();
        if (FileTimeHelper.TryFromFileTime(raw, out DateTimeOffset value))
        {
            return value;
        }
        throw new InvalidDataException(
            $"OPCBATCHSUMMARY.{fieldName} FILETIME value 0x{raw:X16} ({raw}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
    }
}
