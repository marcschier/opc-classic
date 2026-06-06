//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // End-to-end tests assert captured pipeline state.

using Opc.Classic.Dcom;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
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
        // HistoricalDataStore (DR9.1) seeds at a 10-second cadence, so a
        // [start, start+20s] window with bounds=true contains exactly 3
        // samples (start, +10s, +20s) — matching the maxValues=3 cap below.
        DateTimeOffset end = start.AddSeconds(20);
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

        // Flat browse returns Sensor branch + 3 Sensor leaves + 3 Random leaves = 7.
        // The Random.* items live alongside Sensor.* so cross-impl matrix probes
        // that target default Matrikon/TestServer item IDs work against this
        // sample server (HistoricalDataStore seeds Random.Int4/Real8/String).
        await Assert.That(root.Length).IsEqualTo(7);
        await Assert.That(root.Any(static element => element.Name == "Sensor" && element.ItemId == "Sensor" && element.BrowseType == HdaBrowseType.Branch)).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Sensor.Temperature" && element.BrowseType == HdaBrowseType.Leaf)).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Sensor.Pressure" && element.Name == "Pressure")).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Sensor.FlowRate" && element.Name == "FlowRate")).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Random.Int4" && element.BrowseType == HdaBrowseType.Leaf)).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Random.Real8" && element.BrowseType == HdaBrowseType.Leaf)).IsTrue();
        await Assert.That(root.Any(static element => element.ItemId == "Random.String" && element.BrowseType == HdaBrowseType.Leaf)).IsTrue();
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

    [Test, Category("EndToEnd")]
    public async Task ServerMetadata_Then_ItemAttributesAndAggregatesRoundTrip()
    {
        var channel = new InMemoryCallChannel(new OpcHdaServerDispatcher(new ServerMetadataImpl()).DispatchAsync);
        var client = new IOPCHDA_ServerClientProxy(channel);

        await client.GetItemAttributesAsync(
            out int[] attributeIds,
            out string[] attributeNames,
            out string[] attributeDescriptions,
            out int[] attributeDataTypes,
            CancellationToken.None);
        await client.GetAggregatesAsync(
            out int[] aggregateIds,
            out string[] aggregateNames,
            out string[] aggregateDescriptions,
            CancellationToken.None);

        await Assert.That(attributeIds).IsEquivalentTo([1, 2, 3, 4, 5]);
        await Assert.That(attributeNames[1]).IsEqualTo("Description");
        await Assert.That(attributeDescriptions[2]).IsEqualTo("Engineering units");
        await Assert.That(attributeDataTypes).IsEquivalentTo([(int)VarType.VT_I2, (int)VarType.VT_BSTR, (int)VarType.VT_BSTR, (int)VarType.VT_BOOL, (int)VarType.VT_BOOL]);
        await Assert.That(aggregateIds).IsEquivalentTo([(int)HdaAggregate.Interpolative, (int)HdaAggregate.Average, (int)HdaAggregate.TimeAverage]);
        await Assert.That(aggregateNames[2]).IsEqualTo("TimeAverage");
        await Assert.That(aggregateDescriptions[0]).IsEqualTo("Interpolated value");
        await Assert.That(channel.CallLog.Select(static call => call.Opnum)).IsEquivalentTo([IOPCHDA_Server.Opnums.GetItemAttributesAsync, IOPCHDA_Server.Opnums.GetAggregatesAsync]);
    }

    [Test, Category("EndToEnd")]
    public async Task SyncReadMethods_Then_AllReadShapesRoundTrip()
    {
        var channel = ChannelFor(IOPCHDA_SyncRead.InterfaceId, new SyncReadRoundTripImpl().DispatchAsync);
        var client = new IOPCHDA_SyncReadClientProxy(channel);
        long atTime = EndToEndNdr.ToFileTime(SampleTimestamp());

        OpcHdaItem[] raw = await client.ReadRawAsync(SampleStart(), SampleEnd(), 2, bounds: true, [501], CancellationToken.None);
        OpcHdaItem[] processed = await client.ReadProcessedAsync(SampleStart(), SampleEnd(), TimeSpan.FromSeconds(10).Ticks, [501], [(int)HdaAggregate.Average], CancellationToken.None);
        OpcHdaItem[] atTimes = await client.ReadAtTimeAsync([atTime], [501], CancellationToken.None);
        OpcHdaModifiedItem[] modified = await client.ReadModifiedAsync(SampleStart(), SampleEnd(), 1, [501], CancellationToken.None);
        OpcHdaAttribute[] attributes = await client.ReadAttributeAsync(SampleStart(), SampleEnd(), 501, [3], CancellationToken.None);

        await Assert.That(raw[0].Values[0].AsDouble()).IsEqualTo(10.5);
        await Assert.That(processed[0].AggregateHandle).IsEqualTo((int)HdaAggregate.Average);
        await Assert.That(atTimes[0].Timestamps[0]).IsEqualTo(SampleTimestamp());
        await Assert.That(modified[0].Users[0]).IsEqualTo("historian");
        await Assert.That(attributes[0].AttributeId).IsEqualTo(3);
        await Assert.That(attributes[0].Values[0].AsString()).IsEqualTo("degC");
        await Assert.That(channel.CallLog.Select(static call => call.Opnum)).IsEquivalentTo([
            IOPCHDA_SyncRead.Opnums.ReadRawAsync,
            IOPCHDA_SyncRead.Opnums.ReadProcessedAsync,
            IOPCHDA_SyncRead.Opnums.ReadAtTimeAsync,
            IOPCHDA_SyncRead.Opnums.ReadModifiedAsync,
            IOPCHDA_SyncRead.Opnums.ReadAttributeAsync]);
    }

    [Test, Category("EndToEnd")]
    public async Task SyncUpdateMethods_Then_CapabilitiesAndPerItemResultsRoundTrip()
    {
        var channel = ChannelFor(IOPCHDA_SyncUpdate.InterfaceId, new SyncUpdateRoundTripImpl().DispatchAsync);
        var client = new IOPCHDA_SyncUpdateClientProxy(channel);
        long timestamp = EndToEndNdr.ToFileTime(SampleTimestamp());
        int quality = OpcQuality.Good.RawValue;

        int capabilities = await client.QueryCapabilitiesAsync(CancellationToken.None);
        int[] insert = await client.InsertAsync([501], [timestamp], [OpcVariant.FromDouble(1.0)], [quality], CancellationToken.None);
        int[] replace = await client.ReplaceAsync([501], [timestamp], [OpcVariant.FromDouble(2.0)], [quality], CancellationToken.None);
        int[] insertReplace = await client.InsertReplaceAsync([501], [timestamp], [OpcVariant.FromDouble(3.0)], [quality], CancellationToken.None);
        int[] deleteRaw = await client.DeleteRawAsync(SampleStart(), SampleEnd(), [501], CancellationToken.None);
        int[] deleteAtTime = await client.DeleteAtTimeAsync([501], [timestamp], CancellationToken.None);

        await Assert.That(capabilities).IsEqualTo(0x1F);
        await Assert.That(insert).IsEquivalentTo([OpcHdaErrors.OPCHDA_S_INSERTED]);
        await Assert.That(replace).IsEquivalentTo([OpcHdaErrors.OPCHDA_S_REPLACED]);
        await Assert.That(insertReplace).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(deleteRaw).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(deleteAtTime).IsEquivalentTo([OpcHdaErrors.OPCHDA_E_NODATAEXISTS]);
        await Assert.That(channel.CallLog.Count).IsEqualTo(6);
    }

    [Test, Category("EndToEnd")]
    public async Task SyncAnnotationsMethods_Then_CapabilitiesReadAndInsertRoundTrip()
    {
        var channel = ChannelFor(IOPCHDA_SyncAnnotations.InterfaceId, new SyncAnnotationsRoundTripImpl().DispatchAsync);
        var client = new IOPCHDA_SyncAnnotationsClientProxy(channel);
        OpcHdaAnnotation annotation = SampleAnnotation();

        int capabilities = await client.QueryCapabilitiesAsync(CancellationToken.None);
        OpcHdaAnnotation[] read = await client.ReadAsync(SampleStart(), SampleEnd(), [501], CancellationToken.None);
        int[] insert = await client.InsertAsync([501], [EndToEndNdr.ToFileTime(SampleTimestamp())], [annotation], CancellationToken.None);

        await Assert.That(capabilities).IsEqualTo(0x03);
        await Assert.That(read[0].Annotations[0]).IsEqualTo("operator note");
        await Assert.That(read[0].Users[0]).IsEqualTo("historian");
        await Assert.That(insert).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(channel.CallLog.Select(static call => call.Opnum)).IsEquivalentTo([
            IOPCHDA_SyncAnnotations.Opnums.QueryCapabilitiesAsync,
            IOPCHDA_SyncAnnotations.Opnums.ReadAsync,
            IOPCHDA_SyncAnnotations.Opnums.InsertAsync]);
    }

    [Test, Category("EndToEnd")]
    public async Task AsyncReadMethods_Then_TransactionAndCancelIdsRoundTrip()
    {
        var impl = new AsyncReadRoundTripImpl();
        var channel = ChannelFor(IOPCHDA_AsyncRead.InterfaceId, impl.DispatchAsync);
        var client = new IOPCHDA_AsyncReadClientProxy(channel);
        long timestamp = EndToEndNdr.ToFileTime(SampleTimestamp());

        int rawCancel = await client.ReadRawAsync(10, SampleStart(), SampleEnd(), 2, bounds: true, [501], CancellationToken.None);
        int adviseRawCancel = await client.AdviseRawAsync(11, SampleStart(), TimeSpan.FromSeconds(1).Ticks, [501], CancellationToken.None);
        int processedCancel = await client.ReadProcessedAsync(12, SampleStart(), SampleEnd(), TimeSpan.FromSeconds(10).Ticks, [501], [(int)HdaAggregate.Average], CancellationToken.None);
        int adviseProcessedCancel = await client.AdviseProcessedAsync(13, SampleStart(), TimeSpan.FromSeconds(10).Ticks, [501], [(int)HdaAggregate.Average], 2, CancellationToken.None);
        int atTimeCancel = await client.ReadAtTimeAsync(14, [timestamp], [501], CancellationToken.None);
        int modifiedCancel = await client.ReadModifiedAsync(15, SampleStart(), SampleEnd(), 2, [501], CancellationToken.None);
        int attributeCancel = await client.ReadAttributeAsync(16, SampleStart(), SampleEnd(), 501, [3], CancellationToken.None);
        await client.CancelAsync(attributeCancel, CancellationToken.None);

        await Assert.That(rawCancel).IsEqualTo(1010);
        await Assert.That(adviseRawCancel).IsEqualTo(1011);
        await Assert.That(processedCancel).IsEqualTo(1012);
        await Assert.That(adviseProcessedCancel).IsEqualTo(1013);
        await Assert.That(atTimeCancel).IsEqualTo(1014);
        await Assert.That(modifiedCancel).IsEqualTo(1015);
        await Assert.That(attributeCancel).IsEqualTo(1016);
        await Assert.That(impl.CancelledId).IsEqualTo(1016);
        await Assert.That(channel.CallLog.Count).IsEqualTo(8);
    }

    [Test, Category("EndToEnd")]
    public async Task AsyncUpdateMethods_Then_TransactionAndCancelIdsRoundTrip()
    {
        var impl = new AsyncUpdateRoundTripImpl();
        var channel = ChannelFor(IOPCHDA_AsyncUpdate.InterfaceId, impl.DispatchAsync);
        var client = new IOPCHDA_AsyncUpdateClientProxy(channel);
        long timestamp = EndToEndNdr.ToFileTime(SampleTimestamp());
        int quality = OpcQuality.Good.RawValue;

        int capabilities = await client.QueryCapabilitiesAsync(CancellationToken.None);
        int insertCancel = await client.InsertAsync(20, [501], [timestamp], [OpcVariant.FromDouble(1.0)], [quality], CancellationToken.None);
        int replaceCancel = await client.ReplaceAsync(21, [501], [timestamp], [OpcVariant.FromDouble(2.0)], [quality], CancellationToken.None);
        int insertReplaceCancel = await client.InsertReplaceAsync(22, [501], [timestamp], [OpcVariant.FromDouble(3.0)], [quality], CancellationToken.None);
        int deleteRawCancel = await client.DeleteRawAsync(23, SampleStart(), SampleEnd(), [501], CancellationToken.None);
        int deleteAtTimeCancel = await client.DeleteAtTimeAsync(24, [501], [timestamp], CancellationToken.None);
        await client.CancelAsync(deleteAtTimeCancel, CancellationToken.None);

        await Assert.That(capabilities).IsEqualTo(0x1F);
        await Assert.That(insertCancel).IsEqualTo(2020);
        await Assert.That(replaceCancel).IsEqualTo(2021);
        await Assert.That(insertReplaceCancel).IsEqualTo(2022);
        await Assert.That(deleteRawCancel).IsEqualTo(2023);
        await Assert.That(deleteAtTimeCancel).IsEqualTo(2024);
        await Assert.That(impl.CancelledId).IsEqualTo(2024);
        await Assert.That(channel.CallLog.Count).IsEqualTo(7);
    }

    [Test, Category("EndToEnd")]
    public async Task AsyncAnnotationsMethods_Then_ReadInsertAndCancelRoundTrip()
    {
        var impl = new AsyncAnnotationsRoundTripImpl();
        var channel = ChannelFor(IOPCHDA_AsyncAnnotations.InterfaceId, impl.DispatchAsync);
        var client = new IOPCHDA_AsyncAnnotationsClientProxy(channel);

        int capabilities = await client.QueryCapabilitiesAsync(CancellationToken.None);
        int readCancel = await client.ReadAsync(30, SampleStart(), SampleEnd(), [501], CancellationToken.None);
        int insertCancel = await client.InsertAsync(31, [501], [EndToEndNdr.ToFileTime(SampleTimestamp())], [SampleAnnotation()], CancellationToken.None);
        await client.CancelAsync(insertCancel, CancellationToken.None);

        await Assert.That(capabilities).IsEqualTo(0x03);
        await Assert.That(readCancel).IsEqualTo(3030);
        await Assert.That(insertCancel).IsEqualTo(3031);
        await Assert.That(impl.CancelledId).IsEqualTo(3031);
        await Assert.That(channel.CallLog.Count).IsEqualTo(4);
    }

    [Test, Category("EndToEnd")]
    public async Task PlaybackMethods_Then_RawProcessedAndCancelRoundTrip()
    {
        var impl = new PlaybackRoundTripImpl();
        var channel = ChannelFor(IOPCHDA_Playback.InterfaceId, impl.DispatchAsync);
        var client = new IOPCHDA_PlaybackClientProxy(channel);

        int rawCancel = await client.ReadRawWithUpdateAsync(40, SampleStart(), SampleEnd(), 2, TimeSpan.FromSeconds(30).Ticks, TimeSpan.FromSeconds(5).Ticks, [501], CancellationToken.None);
        int processedCancel = await client.ReadProcessedWithUpdateAsync(41, SampleStart(), SampleEnd(), TimeSpan.FromSeconds(10).Ticks, 2, TimeSpan.FromSeconds(5).Ticks, [501], [(int)HdaAggregate.Average], CancellationToken.None);
        await client.CancelAsync(processedCancel, CancellationToken.None);

        await Assert.That(rawCancel).IsEqualTo(4040);
        await Assert.That(processedCancel).IsEqualTo(4041);
        await Assert.That(impl.CancelledId).IsEqualTo(4041);
        await Assert.That(channel.CallLog.Select(static call => call.Opnum)).IsEquivalentTo([
            IOPCHDA_Playback.Opnums.ReadRawWithUpdateAsync,
            IOPCHDA_Playback.Opnums.ReadProcessedWithUpdateAsync,
            IOPCHDA_Playback.Opnums.CancelAsync]);
    }

    [Test, Category("EndToEnd")]
    public async Task BrowserMethods_Then_GeneratedProxyDecodesEnumAndNames()
    {
        var channel = BrowserChannel();
        var client = new IOPCHDA_BrowserClientProxy(channel);

        IOpcInterfaceRef enumRef = await client.GetEnumAsync((int)HdaBrowseType.Flat, CancellationToken.None);
        await client.ChangeBrowsePositionAsync(1, "Sensor", CancellationToken.None);
        string itemId = await client.GetItemIDAsync("Temperature", CancellationToken.None);
        string branch = await client.GetBranchPositionAsync(CancellationToken.None);

        await Assert.That(enumRef.Iid).IsEqualTo(OpcGuids.IID_IEnumString);
        await Assert.That(itemId).IsEqualTo("Sensor.Temperature");
        await Assert.That(branch).IsEqualTo("Sensor");
        await Assert.That(channel.CallLog.Select(static call => call.Opnum)).IsEquivalentTo([
            IOPCHDA_Browser.Opnums.GetEnumAsync,
            IOPCHDA_Browser.Opnums.ChangeBrowsePositionAsync,
            IOPCHDA_Browser.Opnums.GetItemIDAsync,
            IOPCHDA_Browser.Opnums.GetBranchPositionAsync]);
    }

    [Test, Category("EndToEnd")]
    public async Task DataCallbackDelivery_Then_AllCallbackMethodsReachSink()
    {
        var sink = new DataCallbackSink();
        var channel = ChannelFor(IOPCHDA_DataCallback.InterfaceId, new IOPCHDA_DataCallbackServerDispatcher(sink).DispatchAsync);
        var client = new IOPCHDA_DataCallbackClientProxy(channel);

        await client.OnDataChangeAsync(50, OpcResultId.Ok.Code, [SampleItem()], [OpcResultId.Ok.Code], CancellationToken.None);
        await client.OnReadCompleteAsync(51, OpcResultId.Ok.Code, [SampleItem(value: 51.5)], [OpcResultId.Ok.Code], CancellationToken.None);
        await client.OnReadModifiedCompleteAsync(52, OpcResultId.Ok.Code, [SampleModifiedItem()], [OpcResultId.Ok.Code], CancellationToken.None);
        await client.OnReadAttributeCompleteAsync(53, OpcResultId.Ok.Code, 501, [SampleAttribute()], [OpcResultId.Ok.Code], CancellationToken.None);
        await client.OnReadAnnotationsAsync(54, OpcResultId.Ok.Code, [SampleAnnotation()], [OpcResultId.Ok.Code], CancellationToken.None);
        await client.OnInsertAnnotationsAsync(55, OpcResultId.Ok.Code, [501], [OpcResultId.Ok.Code], CancellationToken.None);
        await client.OnPlaybackAsync(56, OpcResultId.Ok.Code, [SampleItem(value: 56.5)], [OpcResultId.Ok.Code], CancellationToken.None);
        await client.OnUpdateCompleteAsync(57, OpcResultId.Ok.Code, [501], [OpcHdaErrors.OPCHDA_S_REPLACED], CancellationToken.None);
        await client.OnCancelCompleteAsync(4057, CancellationToken.None);

        await Assert.That(sink.DataChangeTransactionId).IsEqualTo(50);
        await Assert.That(sink.ReadCompleteValue).IsEqualTo(51.5);
        await Assert.That(sink.ModifiedUser).IsEqualTo("historian");
        await Assert.That(sink.AttributeClientHandle).IsEqualTo(501);
        await Assert.That(sink.AnnotationUser).IsEqualTo("historian");
        await Assert.That(sink.InsertAnnotationClientHandle).IsEqualTo(501);
        await Assert.That(sink.PlaybackValue).IsEqualTo(56.5);
        await Assert.That(sink.UpdateError).IsEqualTo(OpcHdaErrors.OPCHDA_S_REPLACED);
        await Assert.That(sink.CancelId).IsEqualTo(4057);
        await Assert.That(channel.CallLog.Count).IsEqualTo(9);
    }

    private static InMemoryCallChannel ChannelFor(
        Guid interfaceId,
        Func<int, ReadOnlyMemory<byte>, CancellationToken, ValueTask<DispatchResult>> dispatch) =>
        new((iid, opnum, payload, cancellationToken) =>
        {
            if (iid != interfaceId)
            {
                return Task.FromResult(NotImplemented());
            }

            return dispatch(opnum, payload, cancellationToken).ToCallResultAsync();
        });

    private static InMemoryCallChannel BrowserChannel() =>
        new((iid, opnum, payload, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (iid != IOPCHDA_Browser.InterfaceId)
            {
                return Task.FromResult(NotImplemented());
            }

            var reader = new NdrReader(payload.Span);
            ReadOnlyMemory<byte> response = opnum switch
            {
                IOPCHDA_Browser.Opnums.GetEnumAsync => ReadBrowserTypeAndWriteEnumRef(ref reader),
                IOPCHDA_Browser.Opnums.ChangeBrowsePositionAsync => ReadBrowsePositionAndWriteEmpty(ref reader),
                IOPCHDA_Browser.Opnums.GetItemIDAsync => ReadNodeAndWriteItemId(ref reader),
                IOPCHDA_Browser.Opnums.GetBranchPositionAsync => WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Sensor")),
                _ => ReadOnlyMemory<byte>.Empty,
            };

            return Task.FromResult(new NdrCallResult(opnum is >= 3 and <= 6 ? OpcResultId.Ok.Code : OpcResultId.NotImplemented.Code, response));
        });

    private static ReadOnlyMemory<byte> ReadBrowserTypeAndWriteEnumRef(ref NdrReader reader)
    {
        int browseType = reader.ReadInt32();
        if (browseType != (int)HdaBrowseType.Flat)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        return WritePayload(WriteEnumStringRef);
    }

    private static ReadOnlyMemory<byte> ReadBrowsePositionAndWriteEmpty(ref NdrReader reader)
    {
        _ = reader.ReadInt32();
        _ = reader.ReadUnicodeStringPtr();
        return ReadOnlyMemory<byte>.Empty;
    }

    private static ReadOnlyMemory<byte> ReadNodeAndWriteItemId(ref NdrReader reader)
    {
        string node = reader.ReadUnicodeStringPtr() ?? string.Empty;
        return WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Sensor." + node));
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 8192)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position).ToArray();
    }

    private static void WriteEnumStringRef(ref NdrWriter writer)
    {
        writer.WriteUInt32(0x574F454Du);
        writer.WriteUInt32(1u);
        writer.WriteGuid(OpcGuids.IID_IEnumString);
        writer.WriteUInt32(0u);
        writer.WriteUInt32(5u);
        writer.WriteUInt64(0x1122334455667788UL);
        writer.WriteUInt64(0x8877665544332211UL);
        writer.WriteGuid(new Guid("12345678-1234-5678-9ABC-DEF012345678"));
        writer.WriteUInt16(1);
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
    }

    private static NdrCallResult NotImplemented() =>
        new(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);

    private static DateTimeOffset SampleTimestamp() =>
        new(2026, 5, 22, 10, 0, 0, TimeSpan.Zero);

    private static OpcHdaTime SampleStart() =>
        OpcHdaTime.FromTimestamp(SampleTimestamp().AddMinutes(-10));

    private static OpcHdaTime SampleEnd() =>
        OpcHdaTime.FromTimestamp(SampleTimestamp().AddMinutes(10));

    private static OpcHdaItem SampleItem(int clientHandle = 77, int aggregateHandle = 0, double value = 42.5) =>
        new(
            clientHandle,
            aggregateHandle,
            [SampleTimestamp()],
            [(uint)OpcQuality.Good.RawValue],
            [OpcVariant.FromDouble(value)]);

    private static OpcHdaModifiedItem SampleModifiedItem() =>
        new(
            77,
            [SampleTimestamp()],
            [(uint)OpcQuality.Good.RawValue],
            [OpcVariant.FromDouble(43.5)],
            [SampleTimestamp().AddMinutes(1)],
            [1u],
            ["historian"]);

    private static OpcHdaAttribute SampleAttribute(int clientHandle = 77, int attributeId = 3) =>
        new(
            clientHandle,
            attributeId,
            [SampleTimestamp()],
            [OpcVariant.FromString("degC")]);

    private static OpcHdaAnnotation SampleAnnotation() =>
        new(
            77,
            [SampleTimestamp()],
            ["operator note"],
            [SampleTimestamp().AddMinutes(1)],
            ["historian"]);

    private sealed class ServerMetadataImpl : IOpcHdaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Hda,
                State = OpcServerState.Running,
                StartTime = SampleTimestamp().AddHours(-1),
                CurrentTime = SampleTimestamp(),
                ServerVersion = new Version(1, 2, 0),
                VendorInfo = "HDA metadata test",
                MaxReturnValues = 1024,
            });
        }

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(itemIds.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }

        public Task<int[]> GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(itemIds.Select(static (_, index) => 500 + index).ToArray());
        }

        public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }

        public Task GetItemAttributesAsync(
            out int[] attributeIds,
            out string[] attributeNames,
            out string[] attributeDescriptions,
            out int[] attributeDataTypes,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attributeIds = [1, 2, 3, 4, 5];
            attributeNames = ["DataType", "Description", "EngUnits", "Stepped", "Archiving"];
            attributeDescriptions = ["Canonical data type", "Description", "Engineering units", "Step interpolation", "Archiving active"];
            attributeDataTypes = [(int)VarType.VT_I2, (int)VarType.VT_BSTR, (int)VarType.VT_BSTR, (int)VarType.VT_BOOL, (int)VarType.VT_BOOL];
            return Task.CompletedTask;
        }

        public Task GetAggregatesAsync(
            out int[] aggregateIds,
            out string[] aggregateNames,
            out string[] aggregateDescriptions,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            aggregateIds = [(int)HdaAggregate.Interpolative, (int)HdaAggregate.Average, (int)HdaAggregate.TimeAverage];
            aggregateNames = ["Interpolative", "Average", "TimeAverage"];
            aggregateDescriptions = ["Interpolated value", "Time average", "Time-weighted average"];
            return Task.CompletedTask;
        }
    }

    private sealed class SyncReadRoundTripImpl : IOPCHDA_SyncRead
    {
        private readonly IOPCHDA_SyncReadServerDispatcher _dispatcher;

        public SyncReadRoundTripImpl() =>
            _dispatcher = new IOPCHDA_SyncReadServerDispatcher(this);

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            _dispatcher.DispatchAsync(opnum, payload, cancellationToken);

        public Task<OpcHdaItem[]> ReadRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = maxValues;
            _ = bounds;
            return Task.FromResult(new[] { SampleItem(serverHandles[0], value: 10.5) });
        }

        public Task<OpcHdaItem[]> ReadProcessedAsync(OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = resampleIntervalFileTime;
            return Task.FromResult(new[] { SampleItem(serverHandles[0], aggregateIds[0], 20.5) });
        }

        public Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new[]
            {
                new OpcHdaItem(
                    serverHandles[0],
                    0,
                    [EndToEndNdr.FromFileTime(timestampFileTimes[0])],
                    [(uint)OpcQuality.Good.RawValue],
                    [OpcVariant.FromDouble(30.5)]),
            });
        }

        public Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = maxValues;
            _ = serverHandles;
            return Task.FromResult(new[] { SampleModifiedItem() });
        }

        public Task<OpcHdaAttribute[]> ReadAttributeAsync(OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = serverHandle;
            return Task.FromResult(new[] { SampleAttribute(serverHandle, attributeIds[0]) });
        }
    }

    private sealed class SyncUpdateRoundTripImpl : IOPCHDA_SyncUpdate
    {
        private readonly IOPCHDA_SyncUpdateServerDispatcher _dispatcher;

        public SyncUpdateRoundTripImpl() =>
            _dispatcher = new IOPCHDA_SyncUpdateServerDispatcher(this);

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            _dispatcher.DispatchAsync(opnum, payload, cancellationToken);

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(0x1F);
        }

        public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            _ = dataValues;
            _ = qualities;
            return Task.FromResult(serverHandles.Select(static _ => OpcHdaErrors.OPCHDA_S_INSERTED).ToArray());
        }

        public Task<int[]> ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            _ = dataValues;
            _ = qualities;
            return Task.FromResult(serverHandles.Select(static _ => OpcHdaErrors.OPCHDA_S_REPLACED).ToArray());
        }

        public Task<int[]> InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            _ = dataValues;
            _ = qualities;
            return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }

        public Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }

        public Task<int[]> DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            return Task.FromResult(serverHandles.Select(static _ => OpcHdaErrors.OPCHDA_E_NODATAEXISTS).ToArray());
        }
    }

    private sealed class SyncAnnotationsRoundTripImpl : IOPCHDA_SyncAnnotations
    {
        private readonly IOPCHDA_SyncAnnotationsServerDispatcher _dispatcher;

        public SyncAnnotationsRoundTripImpl() =>
            _dispatcher = new IOPCHDA_SyncAnnotationsServerDispatcher(this);

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            _dispatcher.DispatchAsync(opnum, payload, cancellationToken);

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(0x03);
        }

        public Task<OpcHdaAnnotation[]> ReadAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = serverHandles;
            return Task.FromResult(new[] { SampleAnnotation() });
        }

        public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            _ = annotationValues;
            return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }
    }

    private sealed class AsyncReadRoundTripImpl : IOPCHDA_AsyncRead
    {
        private readonly IOPCHDA_AsyncReadServerDispatcher _dispatcher;

        public AsyncReadRoundTripImpl() =>
            _dispatcher = new IOPCHDA_AsyncReadServerDispatcher(this);

        public int CancelledId { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            _dispatcher.DispatchAsync(opnum, payload, cancellationToken);

        public Task<int> ReadRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = maxValues;
            _ = bounds;
            _ = serverHandles;
            return Task.FromResult(transactionId + 1000);
        }

        public Task<int> AdviseRawAsync(int transactionId, OpcHdaTime startTime, long updateIntervalFileTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = updateIntervalFileTime;
            _ = serverHandles;
            return Task.FromResult(transactionId + 1000);
        }

        public Task<int> ReadProcessedAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = resampleIntervalFileTime;
            _ = serverHandles;
            _ = aggregateIds;
            return Task.FromResult(transactionId + 1000);
        }

        public Task<int> AdviseProcessedAsync(int transactionId, OpcHdaTime startTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, int intervalCount, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = resampleIntervalFileTime;
            _ = serverHandles;
            _ = aggregateIds;
            _ = intervalCount;
            return Task.FromResult(transactionId + 1000);
        }

        public Task<int> ReadAtTimeAsync(int transactionId, long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            _ = serverHandles;
            return Task.FromResult(transactionId + 1000);
        }

        public Task<int> ReadModifiedAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = maxValues;
            _ = serverHandles;
            return Task.FromResult(transactionId + 1000);
        }

        public Task<int> ReadAttributeAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = serverHandle;
            _ = attributeIds;
            return Task.FromResult(transactionId + 1000);
        }

        public Task CancelAsync(int cancelId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancelledId = cancelId;
            return Task.CompletedTask;
        }
    }

    private sealed class AsyncUpdateRoundTripImpl : IOPCHDA_AsyncUpdate
    {
        private readonly IOPCHDA_AsyncUpdateServerDispatcher _dispatcher;

        public AsyncUpdateRoundTripImpl() =>
            _dispatcher = new IOPCHDA_AsyncUpdateServerDispatcher(this);

        public int CancelledId { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            _dispatcher.DispatchAsync(opnum, payload, cancellationToken);

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(0x1F);
        }

        public Task<int> InsertAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default) =>
            CompleteAsync(transactionId, serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken);

        public Task<int> ReplaceAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default) =>
            CompleteAsync(transactionId, serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken);

        public Task<int> InsertReplaceAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default) =>
            CompleteAsync(transactionId, serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken);

        public Task<int> DeleteRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = serverHandles;
            return Task.FromResult(transactionId + 2000);
        }

        public Task<int> DeleteAtTimeAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = serverHandles;
            _ = timestampFileTimes;
            return Task.FromResult(transactionId + 2000);
        }

        public Task CancelAsync(int cancelId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancelledId = cancelId;
            return Task.CompletedTask;
        }

        private static Task<int> CompleteAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = serverHandles;
            _ = timestampFileTimes;
            _ = dataValues;
            _ = qualities;
            return Task.FromResult(transactionId + 2000);
        }
    }

    private sealed class AsyncAnnotationsRoundTripImpl : IOPCHDA_AsyncAnnotations
    {
        private readonly IOPCHDA_AsyncAnnotationsServerDispatcher _dispatcher;

        public AsyncAnnotationsRoundTripImpl() =>
            _dispatcher = new IOPCHDA_AsyncAnnotationsServerDispatcher(this);

        public int CancelledId { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            _dispatcher.DispatchAsync(opnum, payload, cancellationToken);

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(0x03);
        }

        public Task<int> ReadAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = serverHandles;
            return Task.FromResult(transactionId + 3000);
        }

        public Task<int> InsertAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = serverHandles;
            _ = timestampFileTimes;
            _ = annotationValues;
            return Task.FromResult(transactionId + 3000);
        }

        public Task CancelAsync(int cancelId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancelledId = cancelId;
            return Task.CompletedTask;
        }
    }

    private sealed class PlaybackRoundTripImpl : IOPCHDA_Playback
    {
        private readonly IOPCHDA_PlaybackServerDispatcher _dispatcher;

        public PlaybackRoundTripImpl() =>
            _dispatcher = new IOPCHDA_PlaybackServerDispatcher(this);

        public int CancelledId { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken) =>
            _dispatcher.DispatchAsync(opnum, payload, cancellationToken);

        public Task<int> ReadRawWithUpdateAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, long updateDurationFileTime, long updateIntervalFileTime, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = maxValues;
            _ = updateDurationFileTime;
            _ = updateIntervalFileTime;
            _ = serverHandles;
            return Task.FromResult(transactionId + 4000);
        }

        public Task<int> ReadProcessedWithUpdateAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int intervalCount, long updateIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = resampleIntervalFileTime;
            _ = intervalCount;
            _ = updateIntervalFileTime;
            _ = serverHandles;
            _ = aggregateIds;
            return Task.FromResult(transactionId + 4000);
        }

        public Task CancelAsync(int cancelId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancelledId = cancelId;
            return Task.CompletedTask;
        }
    }

    private sealed class DataCallbackSink : IOPCHDA_DataCallback
    {
        public int DataChangeTransactionId { get; private set; }

        public double? ReadCompleteValue { get; private set; }

        public string? ModifiedUser { get; private set; }

        public int AttributeClientHandle { get; private set; }

        public string? AnnotationUser { get; private set; }

        public int InsertAnnotationClientHandle { get; private set; }

        public double? PlaybackValue { get; private set; }

        public int UpdateError { get; private set; }

        public int CancelId { get; private set; }

        public Task OnDataChangeAsync(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = status;
            _ = itemValues;
            _ = errors;
            DataChangeTransactionId = transactionId;
            return Task.CompletedTask;
        }

        public Task OnReadCompleteAsync(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = status;
            _ = errors;
            ReadCompleteValue = itemValues[0].Values[0].AsDouble();
            return Task.CompletedTask;
        }

        public Task OnReadModifiedCompleteAsync(int transactionId, int status, OpcHdaModifiedItem[] itemValues, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = status;
            _ = errors;
            ModifiedUser = itemValues[0].Users[0];
            return Task.CompletedTask;
        }

        public Task OnReadAttributeCompleteAsync(int transactionId, int status, int clientHandle, OpcHdaAttribute[] attributeValues, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = status;
            _ = attributeValues;
            _ = errors;
            AttributeClientHandle = clientHandle;
            return Task.CompletedTask;
        }

        public Task OnReadAnnotationsAsync(int transactionId, int status, OpcHdaAnnotation[] annotationValues, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = status;
            _ = errors;
            AnnotationUser = annotationValues[0].Users[0];
            return Task.CompletedTask;
        }

        public Task OnInsertAnnotationsAsync(int transactionId, int status, int[] clientHandles, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = status;
            _ = errors;
            InsertAnnotationClientHandle = clientHandles[0];
            return Task.CompletedTask;
        }

        public Task OnPlaybackAsync(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = status;
            _ = errors;
            PlaybackValue = itemValues[0].Values[0].AsDouble();
            return Task.CompletedTask;
        }

        public Task OnUpdateCompleteAsync(int transactionId, int status, int[] clientHandles, int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = status;
            _ = clientHandles;
            UpdateError = errors[0];
            return Task.CompletedTask;
        }

        public Task OnCancelCompleteAsync(int cancelId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CancelId = cancelId;
            return Task.CompletedTask;
        }
    }
}
