//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Linq;
using System.Threading.Tasks;
using CsCheck;
using Opc.Classic;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Batch;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Da;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Ndr;
using Opc.Classic.Ndr;
using TUnit.Core;

namespace Opc.Classic.PropertyTests.Codecs;

public sealed class NdrConformantArrayTests {
    [Test]
    public Task ByteArrays_RoundTrip() {
        Gen.Byte.Array[0, 256].Sample(RoundTripByteArray, iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task SByteArrays_RoundTrip() {
        Gen.SByte.Array[0, 256].Sample(values => CodecProperty.RoundTripsConformantArray(
            values,
            static (ref NdrWriter writer, sbyte value) => writer.WriteInt8(value),
            static (ref NdrReader reader) => reader.ReadInt8(),
            static (left, right) => left == right), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Int16Arrays_RoundTrip() {
        Gen.Short.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, short[] array) => writer.WriteConformantInt16Array(array),
            static (ref NdrReader reader) => reader.ReadConformantInt16Array(),
            static (left, right) => left.SequenceEqual(right)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task UInt16Arrays_RoundTrip() {
        Gen.UShort.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, ushort[] array) => writer.WriteConformantUInt16Array(array),
            static (ref NdrReader reader) => reader.ReadConformantUInt16Array(),
            static (left, right) => left.SequenceEqual(right)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Int32Arrays_RoundTrip() {
        Gen.Int.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, int[] array) => writer.WriteConformantInt32Array(array),
            static (ref NdrReader reader) => reader.ReadConformantInt32Array(),
            static (left, right) => left.SequenceEqual(right)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task UInt32Arrays_RoundTrip() {
        Gen.UInt.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, uint[] array) => writer.WriteConformantUInt32Array(array),
            static (ref NdrReader reader) => reader.ReadConformantUInt32Array(),
            static (left, right) => left.SequenceEqual(right)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task Int64Arrays_RoundTrip() {
        Gen.Long.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, long[] array) => writer.WriteConformantInt64Array(array),
            static (ref NdrReader reader) => reader.ReadConformantInt64Array(),
            static (left, right) => left.SequenceEqual(right)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task UInt64Arrays_RoundTrip() {
        Gen.ULong.Array[0, 256].Sample(values => CodecProperty.RoundTripsConformantArray(
            values,
            static (ref NdrWriter writer, ulong value) => writer.WriteUInt64(value),
            static (ref NdrReader reader) => reader.ReadUInt64(),
            static (left, right) => left == right), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task SingleArrays_RoundTrip_BitExactly() {
        Gen.Single.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, float[] array) => writer.WriteConformantSingleArray(array),
            static (ref NdrReader reader) => reader.ReadConformantSingleArray(),
            static (left, right) => CodecProperty.SequenceEqual(
                left,
                right,
                static (l, r) => BitConverter.SingleToUInt32Bits(l) == BitConverter.SingleToUInt32Bits(r))),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DoubleArrays_RoundTrip_BitExactly() {
        Gen.Double.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, double[] array) => writer.WriteConformantDoubleArray(array),
            static (ref NdrReader reader) => reader.ReadConformantDoubleArray(),
            static (left, right) => CodecProperty.SequenceEqual(
                left,
                right,
                static (l, r) => BitConverter.DoubleToUInt64Bits(l) == BitConverter.DoubleToUInt64Bits(r))),
            iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task BooleanArrays_RoundTrip() {
        Gen.Bool.Array[0, 256].Sample(values => CodecProperty.RoundTripsConformantArray(
            values,
            static (ref NdrWriter writer, bool value) => writer.WriteBoolean(value),
            static (ref NdrReader reader) => reader.ReadBoolean(),
            static (left, right) => left == right), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task GuidArrays_RoundTrip() {
        Gen.Guid.Array[0, 256].Sample(values => CodecProperty.RoundTrips(
            values,
            static (ref NdrWriter writer, Guid[] array) => writer.WriteConformantGuidArray(array),
            static (ref NdrReader reader) => reader.ReadConformantGuidArray(),
            static (left, right) => left.SequenceEqual(right)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task FileTimeArrays_RoundTrip() {
        Gen.Long.Array[0, 256].Sample(values => CodecProperty.RoundTripsConformantArray(
            values,
            static (ref NdrWriter writer, long value) => writer.WriteFileTime(value),
            static (ref NdrReader reader) => reader.ReadFileTime(),
            static (left, right) => left == right), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task LpwstrPointerArrays_RoundTrip() {
        CodecProperty.NullableShortStringGen.Array[0, 128].Sample(values => CodecProperty.RoundTripsConformantArray(
            values,
            static (ref NdrWriter writer, string? value) => writer.WriteUnicodeStringPtr(value),
            static (ref NdrReader reader) => reader.ReadUnicodeStringPtr(),
            static (left, right) => string.Equals(left, right, StringComparison.Ordinal),
            capacity: 32 * 1024), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task EmptyConformantArrays_RoundTrip() {
        Gen.Int[0, 0].Sample(_ =>
            RoundTripByteArray([]) &&
            CodecProperty.RoundTripsConformantArray(Array.Empty<sbyte>(),
                static (ref NdrWriter writer, sbyte value) => writer.WriteInt8(value),
                static (ref NdrReader reader) => reader.ReadInt8(),
                static (left, right) => left == right) &&
            CodecProperty.RoundTripsConformantArray(Array.Empty<OpcItemState>(),
                static (ref NdrWriter writer, OpcItemState value) => NdrOpcItemStateCodec.Write(ref writer, value),
                static (ref NdrReader reader) => NdrOpcItemStateCodec.Read(ref reader),
                CodecProperty.OpcItemStateEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task MaxLength256AlignmentArrays_RoundTrip() {
        Gen.Int.Array[256].Sample(values =>
            CodecProperty.RoundTrips(
                values,
                static (ref NdrWriter writer, int[] array) => writer.WriteConformantInt32Array(array),
                static (ref NdrReader reader) => reader.ReadConformantInt32Array(),
                static (left, right) => left.SequenceEqual(right)), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcServerStatusArrays_RoundTrip() {
        CodecProperty.DaServerStatusGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcServerStatus value) => NdrOpcServerStatusCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcServerStatusCodec.Read(ref reader),
            CodecProperty.OpcServerStatusEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemStateArrays_RoundTrip() {
        CodecProperty.OpcItemStateGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcItemState value) => NdrOpcItemStateCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcItemStateCodec.Read(ref reader),
            CodecProperty.OpcItemStateEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemResultArrays_RoundTrip() {
        CodecProperty.OpcItemResultGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcItemResult value) => NdrOpcItemResultCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcItemResultCodec.Read(ref reader),
            CodecProperty.OpcItemResultEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemPropertyArrays_RoundTrip() {
        CodecProperty.OpcItemPropertyResultGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcItemPropertyResult value) => NdrOpcItemPropertyCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcItemPropertyCodec.Read(ref reader),
            CodecProperty.OpcItemPropertyResultEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemPropertiesArrays_RoundTrip() {
        CodecProperty.OpcItemPropertiesGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcItemProperties value) => NdrOpcItemPropertiesCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcItemPropertiesCodec.Read(ref reader),
            CodecProperty.OpcItemPropertiesEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcBrowseElementArrays_RoundTrip() {
        CodecProperty.OpcBrowseElementResultGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcBrowseElementResult value) => NdrOpcBrowseElementCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcBrowseElementCodec.Read(ref reader),
            CodecProperty.OpcBrowseElementResultEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemDefArrays_RoundTrip() {
        CodecProperty.OpcItemDefGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcItemDef value) => NdrOpcItemDefCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcItemDefCodec.Read(ref reader),
            CodecProperty.OpcItemDefEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemAttributesArrays_RoundTrip() {
        CodecProperty.OpcItemAttributesGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcItemAttributes value) => NdrOpcItemAttributesCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcItemAttributesCodec.Read(ref reader),
            CodecProperty.OpcItemAttributesEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcGroupStateArrays_RoundTrip() {
        CodecProperty.OpcGroupStateGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcGroupState value) => NdrOpcGroupStateCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcGroupStateCodec.Read(ref reader),
            CodecProperty.OpcGroupStateEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemVqtArrays_RoundTrip() {
        CodecProperty.OpcItemVqtGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcItemVqt value) => NdrOpcItemVqtCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcItemVqtCodec.Read(ref reader),
            CodecProperty.OpcItemVqtEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task AeOpcEventServerStatusArrays_RoundTrip() {
        CodecProperty.AeServerStatusGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcServerStatus value) => NdrOpcEventServerStatusCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcEventServerStatusCodec.Read(ref reader),
            CodecProperty.OpcServerStatusEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task AeOpcConditionStateArrays_RoundTrip() {
        CodecProperty.OpcConditionStateGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcConditionState value) => NdrOpcConditionStateCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcConditionStateCodec.Read(ref reader),
            CodecProperty.OpcConditionStateEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task AeOpcEventNotificationArrays_RoundTrip() {
        CodecProperty.OpcEventNotificationGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcEventNotification value) => NdrOpcEventNotificationCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcEventNotificationCodec.Read(ref reader),
            CodecProperty.OpcEventNotificationEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaTimeArrays_RoundTrip() {
        CodecProperty.OpcHdaTimeGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcHdaTime value) => NdrOpcHdaTimeCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcHdaTimeCodec.Read(ref reader),
            CodecProperty.OpcHdaTimeEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcServerStatusArrays_RoundTrip() {
        CodecProperty.HdaServerStatusGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcServerStatus value) => NdrOpcHdaServerStatusCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcHdaServerStatusCodec.Read(ref reader),
            CodecProperty.OpcServerStatusEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaItemArrays_RoundTrip() {
        CodecProperty.OpcHdaItemGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcHdaItem value) => NdrOpcHdaItemCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcHdaItemCodec.Read(ref reader),
            CodecProperty.OpcHdaItemEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaAttributeArrays_RoundTrip() {
        CodecProperty.OpcHdaAttributeGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcHdaAttribute value) => NdrOpcHdaAttributeCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcHdaAttributeCodec.Read(ref reader),
            CodecProperty.OpcHdaAttributeEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaModifiedItemArrays_RoundTrip() {
        CodecProperty.OpcHdaModifiedItemGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcHdaModifiedItem value) => NdrOpcHdaModifiedItemCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcHdaModifiedItemCodec.Read(ref reader),
            CodecProperty.OpcHdaModifiedItemEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaAnnotationArrays_RoundTrip() {
        CodecProperty.OpcHdaAnnotationGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcHdaAnnotation value) => NdrOpcHdaAnnotationCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcHdaAnnotationCodec.Read(ref reader),
            CodecProperty.OpcHdaAnnotationEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task BatchOpcBatchSummaryArrays_RoundTrip() {
        CodecProperty.OpcBatchSummaryGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcBatchSummary value) => NdrOpcBatchSummaryCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcBatchSummaryCodec.Read(ref reader),
            CodecProperty.OpcBatchSummaryEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task BatchOpcBatchSummaryFilterArrays_RoundTrip() {
        CodecProperty.OpcBatchSummaryFilterGen.Array[0, 8].Sample(values => StructArrayRoundTrips(
            values,
            static (ref NdrWriter writer, OpcBatchSummaryFilter value) => NdrOpcBatchSummaryFilterCodec.Write(ref writer, value),
            static (ref NdrReader reader) => NdrOpcBatchSummaryFilterCodec.Read(ref reader),
            CodecProperty.OpcBatchSummaryFilterEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    private static bool RoundTripByteArray(byte[] values) => CodecProperty.RoundTrips(
        values,
        static (ref NdrWriter writer, byte[] array) => writer.WriteConformantByteArray(array),
        static (ref NdrReader reader) => reader.ReadConformantByteArray(),
        static (left, right) => left.SequenceEqual(right));

    private static bool StructArrayRoundTrips<T>(
        T[] values,
        NdrValueWriter<T> writeOne,
        NdrValueReader<T> readOne,
        Func<T, T, bool> equals) =>
        CodecProperty.RoundTripsConformantArray(values, writeOne, readOne, equals, capacity: 128 * 1024);
}
