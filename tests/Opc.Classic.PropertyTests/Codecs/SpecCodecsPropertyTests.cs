//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

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

public sealed class SpecCodecsPropertyTests
{
    [Test]
    public Task DaOpcServerStatus_RoundTrips()
    {
        CodecProperty.DaServerStatusGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcServerStatus status) => NdrOpcServerStatusCodec.Write(ref writer, status),
            static (ref NdrReader reader) => NdrOpcServerStatusCodec.Read(ref reader),
            CodecProperty.OpcServerStatusEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemState_RoundTrips()
    {
        CodecProperty.OpcItemStateGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcItemState item) => NdrOpcItemStateCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcItemStateCodec.Read(ref reader),
            CodecProperty.OpcItemStateEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemResult_RoundTrips()
    {
        CodecProperty.OpcItemResultGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcItemResult item) => NdrOpcItemResultCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcItemResultCodec.Read(ref reader),
            CodecProperty.OpcItemResultEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemProperty_RoundTrips()
    {
        CodecProperty.OpcItemPropertyResultGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcItemPropertyResult item) => NdrOpcItemPropertyCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcItemPropertyCodec.Read(ref reader),
            CodecProperty.OpcItemPropertyResultEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemProperties_RoundTrips()
    {
        CodecProperty.OpcItemPropertiesGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcItemProperties item) => NdrOpcItemPropertiesCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcItemPropertiesCodec.Read(ref reader),
            CodecProperty.OpcItemPropertiesEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcBrowseElement_RoundTrips()
    {
        CodecProperty.OpcBrowseElementResultGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcBrowseElementResult item) => NdrOpcBrowseElementCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcBrowseElementCodec.Read(ref reader),
            CodecProperty.OpcBrowseElementResultEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemDef_RoundTrips()
    {
        CodecProperty.OpcItemDefGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcItemDef item) => NdrOpcItemDefCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcItemDefCodec.Read(ref reader),
            CodecProperty.OpcItemDefEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemAttributes_RoundTrips()
    {
        CodecProperty.OpcItemAttributesGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcItemAttributes item) => NdrOpcItemAttributesCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcItemAttributesCodec.Read(ref reader),
            CodecProperty.OpcItemAttributesEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcGroupState_RoundTrips()
    {
        CodecProperty.OpcGroupStateGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcGroupState item) => NdrOpcGroupStateCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcGroupStateCodec.Read(ref reader),
            CodecProperty.OpcGroupStateEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task DaOpcItemVqt_RoundTrips()
    {
        CodecProperty.OpcItemVqtGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcItemVqt item) => NdrOpcItemVqtCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcItemVqtCodec.Read(ref reader),
            CodecProperty.OpcItemVqtEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task AeOpcEventServerStatus_RoundTrips()
    {
        CodecProperty.AeServerStatusGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcServerStatus status) => NdrOpcEventServerStatusCodec.Write(ref writer, status),
            static (ref NdrReader reader) => NdrOpcEventServerStatusCodec.Read(ref reader),
            CodecProperty.OpcServerStatusEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task AeOpcConditionState_RoundTrips()
    {
        CodecProperty.OpcConditionStateGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcConditionState item) => NdrOpcConditionStateCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcConditionStateCodec.Read(ref reader),
            CodecProperty.OpcConditionStateEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task AeOpcEventNotification_RoundTrips()
    {
        CodecProperty.OpcEventNotificationGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcEventNotification item) => NdrOpcEventNotificationCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcEventNotificationCodec.Read(ref reader),
            CodecProperty.OpcEventNotificationEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaTime_RoundTrips()
    {
        CodecProperty.OpcHdaTimeGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcHdaTime item) => NdrOpcHdaTimeCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcHdaTimeCodec.Read(ref reader),
            CodecProperty.OpcHdaTimeEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcServerStatus_RoundTrips()
    {
        CodecProperty.HdaServerStatusGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcServerStatus status) => NdrOpcHdaServerStatusCodec.Write(ref writer, status),
            static (ref NdrReader reader) => NdrOpcHdaServerStatusCodec.Read(ref reader),
            CodecProperty.OpcServerStatusEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaItem_RoundTrips()
    {
        CodecProperty.OpcHdaItemGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcHdaItem item) => NdrOpcHdaItemCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcHdaItemCodec.Read(ref reader),
            CodecProperty.OpcHdaItemEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaAttribute_RoundTrips()
    {
        CodecProperty.OpcHdaAttributeGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcHdaAttribute item) => NdrOpcHdaAttributeCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcHdaAttributeCodec.Read(ref reader),
            CodecProperty.OpcHdaAttributeEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaModifiedItem_RoundTrips()
    {
        CodecProperty.OpcHdaModifiedItemGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcHdaModifiedItem item) => NdrOpcHdaModifiedItemCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcHdaModifiedItemCodec.Read(ref reader),
            CodecProperty.OpcHdaModifiedItemEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task HdaOpcHdaAnnotation_RoundTrips()
    {
        CodecProperty.OpcHdaAnnotationGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcHdaAnnotation item) => NdrOpcHdaAnnotationCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcHdaAnnotationCodec.Read(ref reader),
            CodecProperty.OpcHdaAnnotationEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task BatchOpcBatchSummary_RoundTrips()
    {
        CodecProperty.OpcBatchSummaryGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcBatchSummary item) => NdrOpcBatchSummaryCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcBatchSummaryCodec.Read(ref reader),
            CodecProperty.OpcBatchSummaryEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    [Test]
    public Task BatchOpcBatchSummaryFilter_RoundTrips()
    {
        CodecProperty.OpcBatchSummaryFilterGen.Sample(value => RoundTrips(
            value,
            static (ref NdrWriter writer, OpcBatchSummaryFilter item) => NdrOpcBatchSummaryFilterCodec.Write(ref writer, item),
            static (ref NdrReader reader) => NdrOpcBatchSummaryFilterCodec.Read(ref reader),
            CodecProperty.OpcBatchSummaryFilterEquals), iter: CodecProperty.SampleIterations);
        return Task.CompletedTask;
    }

    private static bool RoundTrips<T>(
        T value,
        NdrValueWriter<T> write,
        NdrValueReader<T> read,
        System.Func<T, T, bool> equals) => CodecProperty.RoundTrips(value, write, read, equals);
}
