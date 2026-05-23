//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // End-to-end tests assert captured pipeline state.

using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.EndToEnd;

public sealed class HdaEndToEndTests
{
    [Test, Category("EndToEnd")]
    public async Task ConnectAndGetHistorianStatus_Then_StatusFieldsFlowBack()
    {
        var pipeline = new HdaEndToEndPipeline();

        OpcServerStatus status = await pipeline.Server.GetStatusAsync(CancellationToken.None);

        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Hda);
        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.ServerVersion).IsEqualTo(new Version(1, 0, 0));
        await Assert.That(status.MaxReturnValues).IsEqualTo(10_000);
        await Assert.That(status.VendorInfo).IsEqualTo("Opc.Classic .NET HDA Sample");
        await Assert.That(status.StartTime).IsNotEqualTo(default(DateTimeOffset));
        await Assert.That(status.CurrentTime).IsGreaterThanOrEqualTo(status.StartTime);
        await Assert.That(status.LastUpdateTime).IsEqualTo(default(DateTimeOffset));
        await Assert.That(pipeline.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCHDA_Server.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog[0].Opnum).IsEqualTo(IOPCHDA_Server.Opnums.GetStatusAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task ValidateItemIDs_Then_PerItemResultsRoundTrip()
    {
        var pipeline = new HdaEndToEndPipeline();
        string[] itemIds = ["Sensor.Temperature", "Sensor.Pressure", "Missing.Sensor"];

        int[] results = await pipeline.Server.ValidateItemIDsAsync(itemIds, CancellationToken.None);

        await Assert.That(results.Length).IsEqualTo(3);
        await Assert.That(results[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(results[1]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(results[2]).IsEqualTo(OpcResultId.UnknownItemId.Code);
        await Assert.That(pipeline.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCHDA_Server.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog[0].Opnum).IsEqualTo(IOPCHDA_Server.Opnums.ValidateItemIDsAsync);
        await Assert.That(pipeline.Channel.CallLog[0].PayloadLength).IsGreaterThan(0);
    }

    [Test, Category("EndToEnd")]
    public async Task SyncReadRawValues_Then_TimestampsQualityAndValuesRoundTrip()
    {
        var pipeline = new HdaEndToEndPipeline();
        string itemId = "Sensor.Temperature";
        DateTimeOffset start = pipeline.Store.StartTime;
        DateTimeOffset end = start.AddSeconds(10);
        int[] handles = await pipeline.Server.GetItemHandlesAsync([itemId], [0x701], CancellationToken.None);

        OpcHdaItem[] items = await pipeline.SyncRead.ReadRawAsync(
            OpcHdaTime.FromTimestamp(start),
            OpcHdaTime.FromTimestamp(end),
            maxValues: 3,
            bounds: true,
            handles,
            CancellationToken.None);
        (DateTimeOffset Time, double Value)[] expected = pipeline.Store.ReadRaw(itemId, start, end, 3).ToArray();

        await Assert.That(handles.Length).IsEqualTo(1);
        await Assert.That(handles[0]).IsGreaterThan(0);
        await Assert.That(items.Length).IsEqualTo(1);
        await Assert.That(items[0].ClientHandle).IsEqualTo(0x701);
        await Assert.That(items[0].AggregateHandle).IsEqualTo(0);
        await Assert.That(items[0].Timestamps.Length).IsEqualTo(3);
        await Assert.That(items[0].Qualities.Length).IsEqualTo(3);
        await Assert.That(items[0].Values.Length).IsEqualTo(3);
        for (int i = 0; i < expected.Length; i++)
        {
            await Assert.That(items[0].Timestamps[i]).IsEqualTo(expected[i].Time);
            await Assert.That(items[0].Qualities[i]).IsEqualTo(OpcQuality.Good.RawValue);
            await Assert.That(items[0].Values[i].Type).IsEqualTo(VarType.VT_R8);
            await Assert.That(items[0].Values[i].AsDouble()).IsEqualTo(expected[i].Value);
        }

        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCHDA_SyncRead.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCHDA_SyncRead.Opnums.ReadRawAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task SyncReadProcessedAverage_Then_AggregateCodeAndValuesRoundTrip()
    {
        var pipeline = new HdaEndToEndPipeline();
        string itemId = "Sensor.FlowRate";
        DateTimeOffset start = pipeline.Store.StartTime;
        DateTimeOffset end = start.AddSeconds(20);
        TimeSpan interval = TimeSpan.FromSeconds(10);
        int[] handles = await pipeline.Server.GetItemHandlesAsync([itemId], [0x702], CancellationToken.None);

        OpcHdaItem[] items = await pipeline.SyncRead.ReadProcessedAsync(
            OpcHdaTime.FromTimestamp(start),
            OpcHdaTime.FromTimestamp(end),
            interval.Ticks,
            handles,
            [(int)HdaAggregate.Average],
            CancellationToken.None);
        double firstAverage = pipeline.Store.ReadRaw(itemId, start, start.Add(interval), 0).Average(static sample => sample.Value);
        double secondAverage = pipeline.Store.ReadRaw(itemId, start.Add(interval), end, 0).Average(static sample => sample.Value);

        await Assert.That(items.Length).IsEqualTo(1);
        await Assert.That(items[0].ClientHandle).IsEqualTo(0x702);
        await Assert.That(items[0].AggregateHandle).IsEqualTo((int)HdaAggregate.Average);
        await Assert.That(items[0].Timestamps).IsEquivalentTo([start, start.Add(interval)]);
        await Assert.That(items[0].Qualities).IsEquivalentTo([(uint)OpcQuality.Good.RawValue, (uint)OpcQuality.Good.RawValue]);
        await Assert.That(items[0].Values.Length).IsEqualTo(2);
        await Assert.That(items[0].Values[0].Type).IsEqualTo(VarType.VT_R8);
        await Assert.That(items[0].Values[0].AsDouble()).IsEqualTo(firstAverage);
        await Assert.That(items[0].Values[1].Type).IsEqualTo(VarType.VT_R8);
        await Assert.That(items[0].Values[1].AsDouble()).IsEqualTo(secondAverage);
        await Assert.That(pipeline.LastProcessedAggregate).IsEqualTo(HdaAggregate.Average);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCHDA_SyncRead.Opnums.ReadProcessedAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task ReadAnnotations_Then_ItemAnnotationsAreReturned()
    {
        var pipeline = new HdaEndToEndPipeline();
        string itemId = "Sensor.Temperature";
        _ = await pipeline.Server.GetItemHandlesAsync([itemId], [0x703], CancellationToken.None);

        HdaAnnotationWireResult[] results = await pipeline.ReadAnnotationsViaWireAsync([itemId], CancellationToken.None);

        await Assert.That(results.Length).IsEqualTo(1);
        await Assert.That(results[0].ItemId).IsEqualTo(itemId);
        await Assert.That(results[0].Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(results[0].Annotations.Count).IsEqualTo(1);
        HdaAnnotation annotation = results[0].Annotations[0];
        await Assert.That(annotation.Timestamp).IsEqualTo(pipeline.Store.EndTime.AddMinutes(-30));
        await Assert.That(annotation.AnnotationTime).IsEqualTo(pipeline.Store.EndTime.AddMinutes(-29));
        await Assert.That(annotation.AnnotationText).IsEqualTo($"Calibration note for {itemId}");
        await Assert.That(annotation.User).IsEqualTo("operator.e2e");
        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCHDA_SyncAnnotations.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().PayloadLength).IsGreaterThan(0);
    }

    [Test, Category("EndToEnd")]
    public async Task Browse_Then_HdaHierarchyIsReturned()
    {
        var pipeline = new HdaEndToEndPipeline();

        HdaBrowseWireElement[] root = await pipeline.BrowseViaWireAsync(string.Empty, HdaBrowseType.Flat, CancellationToken.None);
        HdaBrowseWireElement[] sensor = await pipeline.BrowseViaWireAsync("Sensor", HdaBrowseType.Leaf, CancellationToken.None);

        await Assert.That(root.Length).IsEqualTo(4);
        await Assert.That(root[0].Name).IsEqualTo("Sensor");
        await Assert.That(root[0].ItemId).IsEqualTo("Sensor");
        await Assert.That(root[0].BrowseType).IsEqualTo(HdaBrowseType.Branch);
        await Assert.That(root.Any(static element => element.ItemId == "Sensor.Temperature" && element.BrowseType == HdaBrowseType.Leaf)).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Sensor.Pressure" && element.Name == "Pressure")).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Sensor.FlowRate" && element.Name == "FlowRate")).IsTrue();
        await Assert.That(sensor.Length).IsEqualTo(3);
        await Assert.That(sensor.All(static element => element.BrowseType == HdaBrowseType.Leaf)).IsTrue();
        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCHDA_Browser.InterfaceId);
    }

    [Test, Category("EndToEnd")]
    public async Task ReleaseItemHandles_Then_ServerHandleCleanupReturnsPerItemResults()
    {
        var pipeline = new HdaEndToEndPipeline();
        int[] handles = await pipeline.Server.GetItemHandlesAsync(["Sensor.Temperature", "Sensor.Pressure"], [0x704, 0x705], CancellationToken.None);

        int[] firstRelease = await pipeline.Server.ReleaseItemHandlesAsync(handles, CancellationToken.None);
        int[] secondRelease = await pipeline.Server.ReleaseItemHandlesAsync(handles, CancellationToken.None);

        await Assert.That(handles.Length).IsEqualTo(2);
        await Assert.That(handles.All(static handle => handle > 0)).IsTrue();
        await Assert.That(firstRelease).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.Ok.Code]);
        await Assert.That(secondRelease).IsEquivalentTo([OpcResultId.InvalidHandle.Code, OpcResultId.InvalidHandle.Code]);
        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCHDA_Server.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCHDA_Server.Opnums.ReleaseItemHandlesAsync);
    }
}
