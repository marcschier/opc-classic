// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Ndr;

namespace Opc.Classic.Batch.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC Batch <c>OPCBATCHSUMMARYFILTER</c>
/// struct, matching <c>tagOPCBATCHSUMMARYFILTER</c> in opcbc.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     LPWSTR    szID
///     LPWSTR    szDescription
///     LPWSTR    szOPCItemID
///     LPWSTR    szMasterRecipeID
///     FLOAT     fMinBatchSize
///     FLOAT     fMaxBatchSize
///     LPWSTR    szEU
///     LPWSTR    szExecutionState
///     LPWSTR    szExecutionMode
///     FILETIME  ftMinStartTime
///     FILETIME  ftMaxStartTime
///     FILETIME  ftMinEndTime
///     FILETIME  ftMaxEndTime
/// </code>
/// </remarks>
public static class NdrOpcBatchSummaryFilterCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L; // 1601-01-01 UTC in .NET ticks

    /// <summary>
    /// Encodes a single OPCBATCHSUMMARYFILTER in NDR.
    /// </summary>
    public static void Write(ref NdrWriter writer, OpcBatchSummaryFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        writer.WriteUnicodeStringPtr(filter.Id);
        writer.WriteUnicodeStringPtr(filter.Description);
        writer.WriteUnicodeStringPtr(filter.OpcItemId);
        writer.WriteUnicodeStringPtr(filter.MasterRecipeId);
        writer.WriteSingle(filter.MinBatchSize);
        writer.WriteSingle(filter.MaxBatchSize);
        writer.WriteUnicodeStringPtr(filter.EngineeringUnits);
        writer.WriteUnicodeStringPtr(filter.ExecutionState);
        writer.WriteUnicodeStringPtr(filter.ExecutionMode);
        writer.WriteFileTime(ToFileTime(filter.MinStartTime));
        writer.WriteFileTime(ToFileTime(filter.MaxStartTime));
        writer.WriteFileTime(ToFileTime(filter.MinEndTime));
        writer.WriteFileTime(ToFileTime(filter.MaxEndTime));
    }

    /// <summary>
    /// Decodes a single OPCBATCHSUMMARYFILTER from NDR.
    /// </summary>
    public static OpcBatchSummaryFilter Read(ref NdrReader reader)
    {
        string? id = reader.ReadUnicodeStringPtr();
        string? description = reader.ReadUnicodeStringPtr();
        string? opcItemId = reader.ReadUnicodeStringPtr();
        string? masterRecipeId = reader.ReadUnicodeStringPtr();
        float minBatchSize = reader.ReadSingle();
        float maxBatchSize = reader.ReadSingle();
        string? engineeringUnits = reader.ReadUnicodeStringPtr();
        string? executionState = reader.ReadUnicodeStringPtr();
        string? executionMode = reader.ReadUnicodeStringPtr();
        DateTimeOffset minStartTime = ReadAndDecodeFileTime(ref reader, "ftMinStartTime");
        DateTimeOffset maxStartTime = ReadAndDecodeFileTime(ref reader, "ftMaxStartTime");
        DateTimeOffset minEndTime = ReadAndDecodeFileTime(ref reader, "ftMinEndTime");
        DateTimeOffset maxEndTime = ReadAndDecodeFileTime(ref reader, "ftMaxEndTime");

        return new OpcBatchSummaryFilter(
            Id: id,
            Description: description,
            OpcItemId: opcItemId,
            MasterRecipeId: masterRecipeId,
            MinBatchSize: minBatchSize,
            MaxBatchSize: maxBatchSize,
            EngineeringUnits: engineeringUnits,
            ExecutionState: executionState,
            ExecutionMode: executionMode,
            MinStartTime: minStartTime,
            MaxStartTime: maxStartTime,
            MinEndTime: minEndTime,
            MaxEndTime: maxEndTime);
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
            $"OPCBATCHSUMMARYFILTER.{fieldName} FILETIME value 0x{raw:X16} ({raw}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
    }
}
