// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
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

    /// <summary>
    /// Encodes a single OPCBATCHSUMMARY in NDR.
    /// </summary>
    public static void Write(ref NdrWriter writer, OpcBatchSummary summary)
    {
        ArgumentNullException.ThrowIfNull(summary);

        WriteInline(ref writer, summary);
        WriteDeferred(ref writer, summary);
    }

    /// <summary>
    /// Decodes a single OPCBATCHSUMMARY from NDR.
    /// </summary>
    public static OpcBatchSummary Read(ref NdrReader reader)
    {
        SummaryInline inline = ReadInline(ref reader);
        return ApplyDeferred(ref reader, inline);
    }

    /// <summary>
    /// Writes the elements of a conformant OPCBATCHSUMMARY array after the
    /// caller has emitted the array max-count.
    /// </summary>
    public static void WriteConformantArrayBody(ref NdrWriter writer, OpcBatchSummary[] summaries)
    {
        ArgumentNullException.ThrowIfNull(summaries);
        foreach (OpcBatchSummary summary in summaries)
        {
            WriteInline(ref writer, summary);
        }
        foreach (OpcBatchSummary summary in summaries)
        {
            WriteDeferred(ref writer, summary);
        }
    }

    /// <summary>
    /// Reads the elements of a conformant OPCBATCHSUMMARY array after the
    /// caller has consumed the array max-count.
    /// </summary>
    public static OpcBatchSummary[] ReadConformantArrayBody(ref NdrReader reader, int count)
    {
        if (count < 0)
        {
            throw new InvalidDataException("OPCBATCHSUMMARY array count cannot be negative.");
        }
        if (count == 0)
        {
            return [];
        }

        var inline = new SummaryInline[count];
        for (int i = 0; i < count; i++)
        {
            inline[i] = ReadInline(ref reader);
        }
        var summaries = new OpcBatchSummary[count];
        for (int i = 0; i < count; i++)
        {
            summaries[i] = ApplyDeferred(ref reader, inline[i]);
        }
        return summaries;
    }

    private static void WriteInline(ref NdrWriter writer, OpcBatchSummary summary)
    {
        writer.WriteUniquePointerReferent(summary.Id is not null);
        writer.WriteUniquePointerReferent(summary.Description is not null);
        writer.WriteUniquePointerReferent(summary.OpcItemId is not null);
        writer.WriteUniquePointerReferent(summary.MasterRecipeId is not null);
        writer.WriteSingle(summary.BatchSize);
        writer.WriteUniquePointerReferent(summary.EngineeringUnits is not null);
        writer.WriteUniquePointerReferent(summary.ExecutionState is not null);
        writer.WriteUniquePointerReferent(summary.ExecutionMode is not null);
        writer.WriteFileTime(ToFileTime(summary.ActualStartTime));
        writer.WriteFileTime(ToFileTime(summary.ActualEndTime));
    }

    private static void WriteDeferred(ref NdrWriter writer, OpcBatchSummary summary)
    {
        WriteString(ref writer, summary.Id);
        WriteString(ref writer, summary.Description);
        WriteString(ref writer, summary.OpcItemId);
        WriteString(ref writer, summary.MasterRecipeId);
        WriteString(ref writer, summary.EngineeringUnits);
        WriteString(ref writer, summary.ExecutionState);
        WriteString(ref writer, summary.ExecutionMode);
    }

    private static void WriteString(ref NdrWriter writer, string? value)
    {
        if (value is not null)
        {
            writer.WriteUnicodeString(value);
        }
    }

    private static SummaryInline ReadInline(ref NdrReader reader) =>
        new(
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadSingle(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            ReadAndDecodeFileTime(ref reader, "ftActualStartTime"),
            ReadAndDecodeFileTime(ref reader, "ftActualEndTime"));

    private static OpcBatchSummary ApplyDeferred(ref NdrReader reader, SummaryInline inline) =>
        new(
            Id: ReadString(ref reader, inline.IdRef),
            Description: ReadString(ref reader, inline.DescriptionRef),
            OpcItemId: ReadString(ref reader, inline.OpcItemIdRef),
            MasterRecipeId: ReadString(ref reader, inline.MasterRecipeIdRef),
            BatchSize: inline.BatchSize,
            EngineeringUnits: ReadString(ref reader, inline.EngineeringUnitsRef),
            ExecutionState: ReadString(ref reader, inline.ExecutionStateRef),
            ExecutionMode: ReadString(ref reader, inline.ExecutionModeRef),
            ActualStartTime: inline.ActualStartTime,
            ActualEndTime: inline.ActualEndTime);

    private static string? ReadString(ref NdrReader reader, uint referent) =>
        referent == 0u ? null : reader.ReadUnicodeString();

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct SummaryInline(
        uint IdRef,
        uint DescriptionRef,
        uint OpcItemIdRef,
        uint MasterRecipeIdRef,
        float BatchSize,
        uint EngineeringUnitsRef,
        uint ExecutionStateRef,
        uint ExecutionModeRef,
        DateTimeOffset ActualStartTime,
        DateTimeOffset ActualEndTime);

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
