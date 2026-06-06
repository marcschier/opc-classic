//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // Tests assert captured wire metadata.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests.Dcom;

public sealed class IOPCAeDeferredMethodRoundTripTests
{
    [Test]
    public async Task EventServer_QueryEventCategories_round_trips_categories_and_descriptions()
    {
        var server = new RoundTripEventServer();
        var channel = Channel(IOPCEventServer.InterfaceId, new IOPCEventServerServerDispatcher(server));
        var proxy = new IOPCEventServerClientProxy(channel);

        await proxy.QueryEventCategoriesAsync(0x7, out int[] categories, out string[] descriptions, CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventServer.Opnums.QueryEventCategoriesAsync);
        await Assert.That(server.LastEventType).IsEqualTo(0x7);
        await Assert.That(categories).IsEquivalentTo([1001, 1002]);
        await Assert.That(descriptions).IsEquivalentTo(["Simple", "Condition"]);
    }

    [Test]
    public async Task EventServer_QueryEventAttributes_round_trips_attribute_metadata()
    {
        var server = new RoundTripEventServer();
        var channel = Channel(IOPCEventServer.InterfaceId, new IOPCEventServerServerDispatcher(server));
        var proxy = new IOPCEventServerClientProxy(channel);

        await proxy.QueryEventAttributesAsync(1002, out int[] ids, out string[] descriptions, out ushort[] dataTypes, CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventServer.Opnums.QueryEventAttributesAsync);
        await Assert.That(server.LastAttributeCategory).IsEqualTo(1002);
        await Assert.That(ids).IsEquivalentTo([10, 11]);
        await Assert.That(descriptions).IsEquivalentTo(["Area", "Limit"]);
        await Assert.That(dataTypes).IsEquivalentTo([(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8]);
    }

    [Test]
    public async Task EventServer_TranslateToItemIDs_round_trips_item_mappings()
    {
        var server = new RoundTripEventServer();
        var channel = Channel(IOPCEventServer.InterfaceId, new IOPCEventServerServerDispatcher(server));
        var proxy = new IOPCEventServerClientProxy(channel);
        Guid classId = new("11111111-2222-3333-4444-555555555555");
        server.TranslateClassIds = [classId];

        await proxy.TranslateToItemIDsAsync(
            "Plant1.AreaA.Tank7",
            1002,
            "LevelHigh",
            "HiHi",
            [10],
            out string[] itemIds,
            out string[] nodeNames,
            out Guid[] classIds,
            CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventServer.Opnums.TranslateToItemIDsAsync);
        await Assert.That(server.LastTranslateSource).IsEqualTo("Plant1.AreaA.Tank7");
        await Assert.That(itemIds).IsEquivalentTo(["Plant1.AreaA.Tank7.Area"]);
        await Assert.That(nodeNames).IsEquivalentTo(["Tank7"]);
        await Assert.That(classIds).IsEquivalentTo([classId]);
    }

    [Test]
    public async Task EventServer_GetConditionState_uses_correct_opnum_after_catalog_methods()
    {
        var server = new RoundTripEventServer();
        var channel = Channel(IOPCEventServer.InterfaceId, new IOPCEventServerServerDispatcher(server));
        var proxy = new IOPCEventServerClientProxy(channel);

        OpcConditionState state = await proxy.GetConditionStateAsync("S", "C", [1, 2], CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(12);
        await Assert.That(state.ActiveSubCondition).IsEqualTo("Hi");
    }

    [Test]
    public async Task EventSubscriptionMgt_GetFilter_round_trips_full_filter()
    {
        var subscription = new RoundTripSubscriptionMgt();
        var channel = Channel(IOPCEventSubscriptionMgt.InterfaceId, new IOPCEventSubscriptionMgtServerDispatcher(subscription));
        var proxy = new IOPCEventSubscriptionMgtClientProxy(channel);

        await proxy.GetFilterAsync(
            out int eventType,
            out int[] categories,
            out int lowSeverity,
            out int highSeverity,
            out string[] areas,
            out string[] sources,
            CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.GetFilterAsync);
        await Assert.That(eventType).IsEqualTo(0x5);
        await Assert.That(categories).IsEquivalentTo([1001, 1002]);
        await Assert.That(lowSeverity).IsEqualTo(100);
        await Assert.That(highSeverity).IsEqualTo(900);
        await Assert.That(areas).IsEquivalentTo(["Plant1.AreaA"]);
        await Assert.That(sources).IsEquivalentTo(["Plant1.AreaA.Tank7"]);
    }

    [Test]
    public async Task EventSubscriptionMgt_GetState_round_trips_state()
    {
        var subscription = new RoundTripSubscriptionMgt();
        var channel = Channel(IOPCEventSubscriptionMgt.InterfaceId, new IOPCEventSubscriptionMgtServerDispatcher(subscription));
        var proxy = new IOPCEventSubscriptionMgtClientProxy(channel);

        await proxy.GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.GetStateAsync);
        await Assert.That(active).IsTrue();
        await Assert.That(bufferTime).IsEqualTo(250);
        await Assert.That(maxSize).IsEqualTo(10);
        await Assert.That(clientSubscription).IsEqualTo(0xCAFE);
    }

    [Test]
    public async Task EventSubscriptionMgt_SetState_round_trips_revised_buffering()
    {
        var subscription = new RoundTripSubscriptionMgt();
        var channel = Channel(IOPCEventSubscriptionMgt.InterfaceId, new IOPCEventSubscriptionMgtServerDispatcher(subscription));
        var proxy = new IOPCEventSubscriptionMgtClientProxy(channel);

        await proxy.SetStateAsync(false, 10, 0, 0xBEEF, out int revisedBufferTime, out int revisedMaxSize, CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.SetStateAsync);
        await Assert.That(subscription.Active).IsFalse();
        await Assert.That(subscription.ClientSubscription).IsEqualTo(0xBEEF);
        await Assert.That(revisedBufferTime).IsEqualTo(250);
        await Assert.That(revisedMaxSize).IsEqualTo(1);
    }

    [Test]
    public async Task EventServer2_GetEnableStateByArea_round_trips_state_arrays()
    {
        var server = new RoundTripEventServer2();
        var channel = Channel(IOPCEventServer2.InterfaceId, new IOPCEventServer2ServerDispatcher(server));
        var proxy = new IOPCEventServer2ClientProxy(channel);

        await proxy.GetEnableStateByAreaAsync(
            ["Plant1", "Missing"],
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventServer2.Opnums.GetEnableStateByAreaAsync);
        await Assert.That(server.LastAreas).IsEquivalentTo(["Plant1", "Missing"]);
        await Assert.That(enabled).IsEquivalentTo([true, false]);
        await Assert.That(effectivelyEnabled).IsEquivalentTo([true, false]);
        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.InvalidArg.Code]);
    }

    [Test]
    public async Task EventServer2_GetEnableStateBySource_round_trips_state_arrays()
    {
        var server = new RoundTripEventServer2();
        var channel = Channel(IOPCEventServer2.InterfaceId, new IOPCEventServer2ServerDispatcher(server));
        var proxy = new IOPCEventServer2ClientProxy(channel);

        await proxy.GetEnableStateBySourceAsync(
            ["Plant1.AreaA.Tank7"],
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken.None);

        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCEventServer2.Opnums.GetEnableStateBySourceAsync);
        await Assert.That(server.LastSources).IsEquivalentTo(["Plant1.AreaA.Tank7"]);
        await Assert.That(enabled).IsEquivalentTo([true]);
        await Assert.That(effectivelyEnabled).IsEquivalentTo([true]);
        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code]);
    }

    [Test]
    public async Task EventServer_CreateEventSubscription_invokes_opnum_4_with_iface_request_shape()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        bool observedRequest = false;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            observedRequest = reader.ReadInt32() != 0 &&
                reader.ReadInt32() == 100 &&
                reader.ReadInt32() == 10 &&
                reader.ReadInt32() == 0x1234 &&
                reader.ReadGuid() == IOPCEventSubscriptionMgt.InterfaceId;
            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new IOPCEventServerClientProxy(channel);

        OpcException exception = await CaptureAsync<OpcException>(() => proxy.CreateEventSubscriptionAsync(
            true,
            100,
            10,
            0x1234,
            IOPCEventSubscriptionMgt.InterfaceId,
            out _,
            out _,
            out _,
            CancellationToken.None));

        await Assert.That(exception.ResultId.Code).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(observedIid).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(4);
        await Assert.That(observedRequest).IsTrue();
    }

    [Test]
    public async Task EventServer_CreateAreaBrowser_invokes_opnum_18_with_requested_iid()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        Guid observedRequestedIid = Guid.Empty;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            observedRequestedIid = reader.ReadGuid();
            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new IOPCEventServerClientProxy(channel);

        _ = await CaptureAsync<OpcException>(() => proxy.CreateAreaBrowserAsync(
            IOPCEventAreaBrowser.InterfaceId,
            out _,
            CancellationToken.None));

        await Assert.That(observedIid).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventServer.Opnums.CreateAreaBrowserAsync);
        await Assert.That(observedRequestedIid).IsEqualTo(IOPCEventAreaBrowser.InterfaceId);
    }

    [Test]
    public async Task EventAreaBrowser_BrowseOPCAreas_invokes_opnum_4_with_filter()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        bool observedRequest = false;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            observedRequest = reader.ReadInt32() == 1 && reader.ReadUnicodeStringPtr() == "Plant*";
            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new IOPCEventAreaBrowserClientProxy(channel);

        _ = await CaptureAsync<OpcException>(() => proxy.BrowseOPCAreasAsync(1, "Plant*", out _, CancellationToken.None));

        await Assert.That(observedIid).IsEqualTo(IOPCEventAreaBrowser.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventAreaBrowser.Opnums.BrowseOPCAreasAsync);
        await Assert.That(observedRequest).IsTrue();
    }

    private static InMemoryCallChannel Channel(Guid interfaceId, IOpcServerDispatcher dispatcher) =>
        new((iid, opnum, payload, cancellationToken) =>
        {
            if (iid != interfaceId)
            {
                return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
            }

            return DispatchAsync(dispatcher, opnum, payload, cancellationToken);
        });

    private static async Task<NdrCallResult> DispatchAsync(
        IOpcServerDispatcher dispatcher,
        int opnum,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken) =>
        (await dispatcher.DispatchAsync(opnum, payload, cancellationToken).ConfigureAwait(false)).ToNdrCallResult();

    private static async Task<TException> CaptureAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name} to be thrown.");
    }

    private sealed class RoundTripEventServer : IOPCEventServer
    {
        public int LastEventType { get; private set; }

        public int LastAttributeCategory { get; private set; }

        public string? LastTranslateSource { get; private set; }

        public Guid[] TranslateClassIds { get; set; } = [Guid.Empty];

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Ae, State = OpcServerState.Running });

        public Task CreateEventSubscriptionAsync(
            bool active,
            int bufferTime,
            int maxSize,
            int clientSubscription,
            Guid requestedInterfaceId,
            out IOPCEventSubscriptionMgt subscription,
            out int revisedBufferTime,
            out int revisedMaxSize,
            CancellationToken cancellationToken = default)
        {
            subscription = default!;
            revisedBufferTime = bufferTime;
            revisedMaxSize = maxSize;
            return Task.CompletedTask;
        }

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) => Task.FromResult(0x1F);

        public Task QueryEventCategoriesAsync(
            int eventType,
            out int[] eventCategories,
            out string[] eventCategoryDescriptions,
            CancellationToken cancellationToken = default)
        {
            LastEventType = eventType;
            eventCategories = [1001, 1002];
            eventCategoryDescriptions = ["Simple", "Condition"];
            return Task.CompletedTask;
        }

        public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default) =>
            Task.FromResult(new[] { "LevelHigh" });

        public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default) =>
            Task.FromResult(new[] { "Hi" });

        public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default) =>
            Task.FromResult(new[] { "LevelHigh" });

        public Task QueryEventAttributesAsync(
            int eventCategory,
            out int[] attributeIds,
            out string[] attributeDescriptions,
            out ushort[] attributeTypes,
            CancellationToken cancellationToken = default)
        {
            LastAttributeCategory = eventCategory;
            attributeIds = [10, 11];
            attributeDescriptions = ["Area", "Limit"];
            attributeTypes = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
            return Task.CompletedTask;
        }

        public Task TranslateToItemIDsAsync(
            string source,
            int eventCategory,
            string conditionName,
            string subconditionName,
            int[] associatedAttributeIds,
            out string[] attributeItemIds,
            out string[] nodeNames,
            out Guid[] classIds,
            CancellationToken cancellationToken = default)
        {
            LastTranslateSource = source;
            attributeItemIds = associatedAttributeIds.Select(id => source + ".Area").ToArray();
            nodeNames = associatedAttributeIds.Select(_ => "Tank7").ToArray();
            classIds = TranslateClassIds;
            _ = eventCategory;
            _ = conditionName;
            _ = subconditionName;
            return Task.CompletedTask;
        }

        public Task<OpcConditionState> GetConditionStateAsync(
            string source,
            string conditionName,
            int[] attributeIds,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcConditionState(
                state: 3,
                activeSubCondition: "Hi",
                activeSubConditionDefinition: "High level",
                activeSubConditionSeverity: 900,
                activeSubConditionDescription: "High level active",
                quality: OpcQuality.Good,
                lastAckTime: DateTimeOffset.UnixEpoch,
                subConditionLastActive: DateTimeOffset.UnixEpoch,
                conditionLastActive: DateTimeOffset.UnixEpoch,
                conditionLastInactive: DateTimeOffset.UnixEpoch,
                acknowledgerId: null,
                comment: null,
                subConditionNames: ["Hi"],
                subConditionDefinitions: ["High level"],
                subConditionSeverities: [900],
                subConditionDescriptions: ["High level active"],
                eventAttributes: [],
                errors: []));

        public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<int[]> AckConditionAsync(
            string acknowledgerId,
            string comment,
            string[] sources,
            string[] conditionNames,
            long[] activeTimes,
            int[] cookies,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(cookies.Select(static _ => OpcResultId.Ok.Code).ToArray());

        public Task CreateAreaBrowserAsync(
            Guid requestedInterfaceId,
            out IOPCEventAreaBrowser areaBrowser,
            CancellationToken cancellationToken = default)
        {
            areaBrowser = default!;
            return Task.CompletedTask;
        }
    }

    private sealed class RoundTripEventServer2 : IOPCEventServer2
    {
        public string[] LastAreas { get; private set; } = [];

        public string[] LastSources { get; private set; } = [];

        public Task<int[]> EnableConditionByArea2Async(string[] areas, CancellationToken cancellationToken = default) =>
            Task.FromResult(areas.Select(static _ => OpcResultId.Ok.Code).ToArray());

        public Task<int[]> EnableConditionBySource2Async(string[] sources, CancellationToken cancellationToken = default) =>
            Task.FromResult(sources.Select(static _ => OpcResultId.Ok.Code).ToArray());

        public Task<int[]> DisableConditionByArea2Async(string[] areas, CancellationToken cancellationToken = default) =>
            Task.FromResult(areas.Select(static _ => OpcResultId.Ok.Code).ToArray());

        public Task<int[]> DisableConditionBySource2Async(string[] sources, CancellationToken cancellationToken = default) =>
            Task.FromResult(sources.Select(static _ => OpcResultId.Ok.Code).ToArray());

        public Task GetEnableStateByAreaAsync(
            string[] areas,
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            LastAreas = areas;
            enabled = areas.Select(static area => area == "Plant1").ToArray();
            effectivelyEnabled = areas.Select(static area => area == "Plant1").ToArray();
            errors = areas.Select(static area => area == "Plant1" ? OpcResultId.Ok.Code : OpcResultId.InvalidArg.Code).ToArray();
            return Task.CompletedTask;
        }

        public Task GetEnableStateBySourceAsync(
            string[] sources,
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            LastSources = sources;
            enabled = sources.Select(static _ => true).ToArray();
            effectivelyEnabled = sources.Select(static _ => true).ToArray();
            errors = sources.Select(static _ => OpcResultId.Ok.Code).ToArray();
            return Task.CompletedTask;
        }
    }

    private sealed class RoundTripSubscriptionMgt : IOPCEventSubscriptionMgt
    {
        public bool Active { get; private set; } = true;

        public int ClientSubscription { get; private set; } = 0xCAFE;

        public Task SetFilterAsync(
            int eventType,
            int[] eventCategories,
            int lowSeverity,
            int highSeverity,
            string[] areas,
            string[] sources,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task GetFilterAsync(
            out int eventType,
            out int[] eventCategories,
            out int lowSeverity,
            out int highSeverity,
            out string[] areas,
            out string[] sources,
            CancellationToken cancellationToken = default)
        {
            eventType = 0x5;
            eventCategories = [1001, 1002];
            lowSeverity = 100;
            highSeverity = 900;
            areas = ["Plant1.AreaA"];
            sources = ["Plant1.AreaA.Tank7"];
            return Task.CompletedTask;
        }

        public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default) =>
            Task.FromResult(new[] { 10, 11 });

        public Task RefreshAsync(int connection, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task GetStateAsync(
            out bool active,
            out int bufferTime,
            out int maxSize,
            out int clientSubscription,
            CancellationToken cancellationToken = default)
        {
            active = Active;
            bufferTime = 250;
            maxSize = 10;
            clientSubscription = ClientSubscription;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(
            bool active,
            int bufferTime,
            int maxSize,
            int clientSubscription,
            out int revisedBufferTime,
            out int revisedMaxSize,
            CancellationToken cancellationToken = default)
        {
            Active = active;
            ClientSubscription = clientSubscription;
            revisedBufferTime = Math.Max(bufferTime, 250);
            revisedMaxSize = Math.Max(maxSize, 1);
            return Task.CompletedTask;
        }
    }
}
