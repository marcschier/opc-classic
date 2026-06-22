// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // End-to-end tests assert captured pipeline state.

using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Integration.Tests.EndToEnd;

public sealed class AeEndToEndTests
{
    [Test, Category("EndToEnd")]
    public async Task ConnectAndGetStatus_Then_AeStatusFlowsBack()
    {
        var pipeline = new AeEndToEndPipeline();

        OpcServerStatus status = await pipeline.EventServer.GetStatusAsync(CancellationToken.None);

        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Ae);
        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.ServerVersion).IsEqualTo(new Version(1, 0, 0));
        await Assert.That(status.GroupCount).IsEqualTo(0);
        await Assert.That(status.BandWidth).IsEqualTo(0u);
        await Assert.That(status.VendorInfo).IsEqualTo("Opc.Classic .NET AE Sample");
        await Assert.That(status.StartTime).IsNotEqualTo(default(DateTimeOffset));
        await Assert.That(status.CurrentTime).IsGreaterThanOrEqualTo(status.StartTime);
        await Assert.That(status.LastUpdateTime).IsGreaterThanOrEqualTo(status.StartTime);
        await Assert.That(pipeline.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog[0].Opnum).IsEqualTo(IOPCEventServer.Opnums.GetStatusAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task QueryAvailableFilters_Then_FilterMaskFlowsThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        int filters = await pipeline.EventServer.QueryAvailableFiltersAsync(CancellationToken.None);

        await Assert.That(filters).IsEqualTo(0x1F);
        await Assert.That((filters & 0x01) != 0).IsTrue();
        await Assert.That((filters & 0x02) != 0).IsTrue();
        await Assert.That((filters & 0x04) != 0).IsTrue();
        await Assert.That(pipeline.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog[0].Opnum).IsEqualTo(IOPCEventServer.Opnums.QueryAvailableFiltersAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task CreateEventSubscription_Then_SubscriptionHandleReturned()
    {
        var pipeline = new AeEndToEndPipeline();
        var sink = new AeEventSink();

        AeSubscriptionHandle handle = await pipeline.CreateEventSubscriptionViaWireAsync(
            active: true,
            bufferTime: 100,
            maxSize: 10,
            clientHandle: 0xAA01,
            sink,
            CancellationToken.None);

        await Assert.That(handle.ServerHandle).IsGreaterThan(9000);
        await Assert.That(handle.ClientHandle).IsEqualTo(0xAA01);
        await Assert.That(handle.RevisedBufferTime).IsEqualTo(250);
        await Assert.That(handle.RevisedMaxSize).IsEqualTo(10);
        await Assert.That(pipeline.ActiveSubscriptionCount).IsEqualTo(1);
        await Assert.That(pipeline.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog[0].PayloadLength).IsGreaterThan(0);
    }

    [Test, Category("EndToEnd")]
    public async Task ReceiveEvent_Then_ClientAsyncEnumerationYieldsNotification()
    {
        var pipeline = new AeEndToEndPipeline();
        var sink = new AeEventSink();
        AeSubscriptionHandle handle = await pipeline.CreateEventSubscriptionViaWireAsync(
            active: true,
            bufferTime: 500,
            maxSize: 20,
            clientHandle: 0xAA02,
            sink,
            CancellationToken.None);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        Task<OpcEventNotification> nextEvent = ReadFirstAsync(sink, timeout.Token);
        OpcEventNotification expected = CreateNotification();

        await pipeline.EmitEventAsync(handle.ServerHandle, expected, timeout.Token);
        OpcEventNotification actual = await nextEvent;

        await Assert.That(actual.ChangeMask).IsEqualTo(expected.ChangeMask);
        await Assert.That(actual.NewState).IsEqualTo(expected.NewState);
        await Assert.That(actual.Source).IsEqualTo(expected.Source);
        await Assert.That(actual.Time).IsEqualTo(expected.Time);
        await Assert.That(actual.Message).IsEqualTo(expected.Message);
        await Assert.That(actual.EventType).IsEqualTo(expected.EventType);
        await Assert.That(actual.EventCategory).IsEqualTo(expected.EventCategory);
        await Assert.That(actual.Severity).IsEqualTo(expected.Severity);
        await Assert.That(actual.ConditionName).IsEqualTo(expected.ConditionName);
        await Assert.That(actual.SubconditionName).IsEqualTo(expected.SubconditionName);
        await Assert.That(actual.Quality).IsEqualTo(expected.Quality);
        await Assert.That(actual.AckRequired).IsEqualTo(expected.AckRequired);
        await Assert.That(actual.ActiveTime).IsEqualTo(expected.ActiveTime);
        await Assert.That(actual.Cookie).IsEqualTo(expected.Cookie);
        await Assert.That(actual.EventAttributes).IsEquivalentTo(expected.EventAttributes);
        await Assert.That(actual.ActorId).IsEqualTo(expected.ActorId);
        await Assert.That(sink.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCEventSink.InterfaceId);
        await Assert.That(sink.Channel.CallLog[0].Opnum).IsEqualTo(IOPCEventSink.Opnums.OnEventAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task AcknowledgeCondition_Then_ServerReceivesAckArguments()
    {
        var pipeline = new AeEndToEndPipeline();
        long activeTime = new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero).ToFileTime();

        int[] errors = await pipeline.EventServer.AckConditionAsync(
            1,
            "operator.alpha",
            "Acknowledged during E2E test",
            ["Plant1.AreaA.Tank7"],
            ["LevelHigh"],
            [activeTime],
            [0x6A7B],
            CancellationToken.None);

        AeAckObservation ack = pipeline.LastAck!;
        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(ack.AcknowledgerId).IsEqualTo("operator.alpha");
        await Assert.That(ack.Comment).IsEqualTo("Acknowledged during E2E test");
        await Assert.That(ack.ActiveTimes).IsEquivalentTo([activeTime]);
        await Assert.That(ack.Cookies).IsEquivalentTo([0x6A7B]);
        await Assert.That(ack.Sources).IsEquivalentTo(["Plant1.AreaA.Tank7"]);
        await Assert.That(ack.ConditionNames).IsEquivalentTo(["LevelHigh"]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer.Opnums.AckConditionAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task BrowseAreas_Then_AreaHierarchyReturned()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.AreaBrowser.ChangeBrowsePositionAsync(1, "Plant1", CancellationToken.None);
        string qualifiedArea = await pipeline.AreaBrowser.GetQualifiedAreaNameAsync("AreaA", CancellationToken.None);
        string qualifiedSource = await pipeline.AreaBrowser.GetQualifiedSourceNameAsync("Tank7", CancellationToken.None);
        string[] areas = await pipeline.BrowseAreasViaWireAsync(CancellationToken.None);

        await Assert.That(qualifiedArea).IsEqualTo("Plant1.AreaA");
        await Assert.That(qualifiedSource).IsEqualTo("Plant1.Tank7");
        await Assert.That(areas).IsEquivalentTo(["Plant1", "Plant1.AreaA", "Plant1.AreaB"]);
        await Assert.That(pipeline.Channel.CallLog.Any(static call => call.InterfaceId == IOPCEventAreaBrowser.InterfaceId)).IsTrue();
        await Assert.That(pipeline.Channel.CallLog.Last().PayloadLength).IsEqualTo(0);
    }

    [Test, Category("EndToEnd")]
    public async Task CancelAndCleanup_Then_SubscriptionIsRemoved()
    {
        var pipeline = new AeEndToEndPipeline();
        var sink = new AeEventSink();
        AeSubscriptionHandle handle = await pipeline.CreateEventSubscriptionViaWireAsync(
            active: true,
            bufferTime: 250,
            maxSize: 3,
            clientHandle: 0xAA03,
            sink,
            CancellationToken.None);

        await pipeline.SubscriptionMgt.CancelRefreshAsync(handle.ClientHandle, CancellationToken.None);
        bool removed = pipeline.CleanupSubscription(handle.ServerHandle);

        await Assert.That(removed).IsTrue();
        await Assert.That(pipeline.ActiveSubscriptionCount).IsEqualTo(0);
        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCEventSubscriptionMgt.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.CancelRefreshAsync);
        await Assert.That(pipeline.Channel.CallLog.Last().PayloadLength).IsGreaterThan(0);
    }

    [Test, Category("EndToEnd")]
    public async Task QueryEventCategories_Then_CategoryMetadataFlowsThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.EventServer.QueryEventCategoriesAsync(0x7, out int[] categories, out string[] descriptions, CancellationToken.None);

        await Assert.That(categories).IsEquivalentTo([0x1001, 0x1002, 0x1003]);
        await Assert.That(descriptions).IsEquivalentTo(["Simple", "Condition", "Tracking"]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer.Opnums.QueryEventCategoriesAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task QueryEventAttributes_Then_AttributeMetadataFlowsThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.EventServer.QueryEventAttributesAsync(0x1002, out int[] ids, out string[] descriptions, out ushort[] types, CancellationToken.None);

        await Assert.That(ids).IsEquivalentTo([10, 11]);
        await Assert.That(descriptions).IsEquivalentTo(["Area", "Limit"]);
        await Assert.That(types).IsEquivalentTo([(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer.Opnums.QueryEventAttributesAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task QueryConditionMetadata_Then_ConditionNamesFlowThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        string[] conditions = await pipeline.EventServer.QueryConditionNamesAsync(0x1002, CancellationToken.None);
        string[] subconditions = await pipeline.EventServer.QuerySubConditionNamesAsync("LevelHigh", CancellationToken.None);
        string[] sourceConditions = await pipeline.EventServer.QuerySourceConditionsAsync("Plant1.AreaA.Tank7", CancellationToken.None);

        await Assert.That(conditions).IsEquivalentTo(["LevelHigh", "PressureLow"]);
        await Assert.That(subconditions).IsEquivalentTo(["Hi", "HiHi"]);
        await Assert.That(sourceConditions).IsEquivalentTo(["LevelHigh"]);
        await Assert.That(pipeline.Channel.CallLog.Select(static call => call.Opnum)).Contains(IOPCEventServer.Opnums.QueryConditionNamesAsync);
        await Assert.That(pipeline.Channel.CallLog.Select(static call => call.Opnum)).Contains(IOPCEventServer.Opnums.QuerySubConditionNamesAsync);
        await Assert.That(pipeline.Channel.CallLog.Select(static call => call.Opnum)).Contains(IOPCEventServer.Opnums.QuerySourceConditionsAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task TranslateToItemIDs_Then_DaMappingsFlowThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.EventServer.TranslateToItemIDsAsync(
            "Plant1.AreaA.Tank7",
            0x1002,
            "LevelHigh",
            "HiHi",
            [10, 11],
            out string[] itemIds,
            out string[] nodeNames,
            out Guid[] classIds,
            CancellationToken.None);

        await Assert.That(itemIds).IsEquivalentTo(["Plant1.AreaA.Tank7.Attr10", "Plant1.AreaA.Tank7.Attr11"]);
        await Assert.That(nodeNames).IsEquivalentTo(["AeNode", "AeNode"]);
        await Assert.That(classIds).IsEquivalentTo([Guid.Empty, Guid.Empty]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer.Opnums.TranslateToItemIDsAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task GetConditionState_Then_StateSnapshotFlowsThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        OpcConditionState state = await pipeline.EventServer.GetConditionStateAsync(
            "Plant1.AreaA.Tank7",
            "LevelHigh",
            [10],
            CancellationToken.None);

        await Assert.That(state.ActiveSubCondition).IsEqualTo("HiHi");
        await Assert.That(state.SubConditionCount).IsEqualTo(2);
        await Assert.That(state.EventAttributeCount).IsEqualTo(1);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer.Opnums.GetConditionStateAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task EnableDisableConditions_Then_AllAreaAndSourceOpnumsRoute()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.EventServer.EnableConditionByAreaAsync(["Plant1.AreaA"], CancellationToken.None);
        await pipeline.EventServer.DisableConditionByAreaAsync(["Plant1.AreaA"], CancellationToken.None);
        await pipeline.EventServer.EnableConditionBySourceAsync(["Plant1.AreaA.Tank7"], CancellationToken.None);
        await pipeline.EventServer.DisableConditionBySourceAsync(["Plant1.AreaA.Tank7"], CancellationToken.None);

        int[] opnums = pipeline.Channel.CallLog.Select(static call => call.Opnum).ToArray();
        await Assert.That(opnums).Contains(IOPCEventServer.Opnums.EnableConditionByAreaAsync);
        await Assert.That(opnums).Contains(IOPCEventServer.Opnums.DisableConditionByAreaAsync);
        await Assert.That(opnums).Contains(IOPCEventServer.Opnums.EnableConditionBySourceAsync);
        await Assert.That(opnums).Contains(IOPCEventServer.Opnums.DisableConditionBySourceAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task SubscriptionFilter_SetThenGet_RoundTripsFilter()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.SubscriptionMgt.SetFilterAsync(0x4, [0x1002], 200, 800, ["Plant1.AreaA"], ["Plant1.AreaA.Tank7"], CancellationToken.None);
        await pipeline.SubscriptionMgt.GetFilterAsync(
            out int eventType,
            out int[] categories,
            out int lowSeverity,
            out int highSeverity,
            out string[] areas,
            out string[] sources,
            CancellationToken.None);

        await Assert.That(eventType).IsEqualTo(0x4);
        await Assert.That(categories).IsEquivalentTo([0x1002]);
        await Assert.That(lowSeverity).IsEqualTo(200);
        await Assert.That(highSeverity).IsEqualTo(800);
        await Assert.That(areas).IsEquivalentTo(["Plant1.AreaA"]);
        await Assert.That(sources).IsEquivalentTo(["Plant1.AreaA.Tank7"]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.GetFilterAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task SubscriptionState_SetThenGet_RoundTripsState()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.SubscriptionMgt.SetStateAsync(false, 10, 0, 0xAA03, out int revisedBufferTime, out int revisedMaxSize, CancellationToken.None);
        await pipeline.SubscriptionMgt.GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken.None);

        await Assert.That(revisedBufferTime).IsEqualTo(250);
        await Assert.That(revisedMaxSize).IsEqualTo(1);
        await Assert.That(active).IsFalse();
        await Assert.That(bufferTime).IsEqualTo(250);
        await Assert.That(maxSize).IsEqualTo(1);
        await Assert.That(clientSubscription).IsEqualTo(0xAA03);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.GetStateAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task ReturnedAttributes_SetThenGet_RoundTripsAttributeIds()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.SubscriptionMgt.SetReturnedAttributesAsync(0x1002, [10, 11], CancellationToken.None);
        int[] ids = await pipeline.SubscriptionMgt.GetReturnedAttributesAsync(0x1002, CancellationToken.None);

        await Assert.That(ids).IsEquivalentTo([10, 11]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.GetReturnedAttributesAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task Refresh_Then_RefreshOpnumRoutesToSubscription()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.SubscriptionMgt.RefreshAsync(0xAA03, CancellationToken.None);

        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCEventSubscriptionMgt.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.RefreshAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task EventServer2PerAreaErrors_Then_MixedResultsFlowThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        int[] errors = await pipeline.EventServer2.EnableConditionByArea2Async(["Plant1", "Missing"], CancellationToken.None);

        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.InvalidArg.Code]);
        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCEventServer2.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer2.Opnums.EnableConditionByArea2Async);
    }

    [Test, Category("EndToEnd")]
    public async Task GetEnableStateByArea_Then_DirectAndEffectiveStatesFlowThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.EventServer2.GetEnableStateByAreaAsync(
            ["Plant1", "Missing"],
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken.None);

        await Assert.That(enabled).IsEquivalentTo([true, false]);
        await Assert.That(effectivelyEnabled).IsEquivalentTo([true, false]);
        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.InvalidArg.Code]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer2.Opnums.GetEnableStateByAreaAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task GetEnableStateBySource_Then_DirectAndEffectiveStatesFlowThrough()
    {
        var pipeline = new AeEndToEndPipeline();

        await pipeline.EventServer2.GetEnableStateBySourceAsync(
            ["Plant1.AreaA.Tank7"],
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken.None);

        await Assert.That(enabled).IsEquivalentTo([true]);
        await Assert.That(effectivelyEnabled).IsEquivalentTo([true]);
        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventServer2.Opnums.GetEnableStateBySourceAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task KeepAlive_SetThenGet_RoundTripsRevisedKeepAlive()
    {
        var pipeline = new AeEndToEndPipeline();

        int revised = await pipeline.SubscriptionMgt2.SetKeepAliveAsync(25, CancellationToken.None);
        int current = await pipeline.SubscriptionMgt2.GetKeepAliveAsync(CancellationToken.None);

        await Assert.That(revised).IsEqualTo(1000);
        await Assert.That(current).IsEqualTo(1000);
        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCEventSubscriptionMgt2.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCEventSubscriptionMgt2.Opnums.GetKeepAliveAsync);
    }

    private static async Task<OpcEventNotification> ReadFirstAsync(AeEventSink sink, CancellationToken cancellationToken)
    {
        await foreach (OpcEventNotification notification in sink.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            return notification;
        }

        throw new InvalidOperationException("AE event sink completed before receiving a notification.");
    }

    private static OpcEventNotification CreateNotification() => new(
        changeMask: 0x0003,
        newState: 0x0002,
        source: "Plant1.AreaA.Tank7",
        time: new DateTimeOffset(2026, 2, 3, 4, 5, 6, TimeSpan.Zero),
        message: "Tank 7 level high",
        eventType: 0x0004,
        eventCategory: 0x1001,
        severity: 875,
        conditionName: "LevelHigh",
        subconditionName: "HiHi",
        quality: OpcQuality.Good.WithSubstatus(2),
        ackRequired: true,
        activeTime: new DateTimeOffset(2026, 2, 3, 4, 5, 0, TimeSpan.Zero),
        cookie: 0x6A7B,
        eventAttributes: [OpcVariant.FromString("north bay"), OpcVariant.FromDouble(98.6)],
        actorId: "operator.alpha");
}
