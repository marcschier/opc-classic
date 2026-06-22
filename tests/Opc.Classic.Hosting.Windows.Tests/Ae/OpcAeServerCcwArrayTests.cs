// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Hosting.Windows;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcAeServerCcwArrayTests
{
    private const int S_OK = 0;
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private const int E_FAIL = unchecked((int)0x80004005);
    private static readonly Guid s_iidUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    [Test]
    public async Task Query_family_happy_paths_marshal_correlated_arrays()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new RecordingDispatcher();
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);

        Helpers.CategoryResult categories = Helpers.InvokeQueryEventCategories(eventServer, 7);
        Helpers.StringArrayResult conditionNames = Helpers.InvokeQueryConditionNames(eventServer, 1001);
        Helpers.StringArrayResult subConditionNames = Helpers.InvokeQuerySubConditionNames(eventServer, "Level");
        Helpers.StringArrayResult sourceConditions = Helpers.InvokeQuerySourceConditions(eventServer, "Plant1.AreaA.Tank7");
        Helpers.AttributeResult attributes = Helpers.InvokeQueryEventAttributes(eventServer, 1001);

        await Assert.That(categories.Hr).IsEqualTo(S_OK);
        await Assert.That(categories.Ids).IsEquivalentTo([1001, 1002, 1003]);
        await Assert.That(categories.Descriptions).IsEquivalentTo(["Process", "System", "Tracking"]);
        await Assert.That(dispatcher.LastQueryEventType).IsEqualTo(7);
        await Assert.That(conditionNames.Hr).IsEqualTo(S_OK);
        await Assert.That(conditionNames.Values).IsEquivalentTo(["Level", "Pressure"]);
        await Assert.That(dispatcher.LastConditionCategory).IsEqualTo(1001);
        await Assert.That(subConditionNames.Hr).IsEqualTo(S_OK);
        await Assert.That(subConditionNames.Values).IsEquivalentTo(["Hi", "HiHi"]);
        await Assert.That(dispatcher.LastSubConditionName).IsEqualTo("Level");
        await Assert.That(sourceConditions.Hr).IsEqualTo(S_OK);
        await Assert.That(sourceConditions.Values).IsEquivalentTo(["Level", "ValveFailure"]);
        await Assert.That(dispatcher.LastSource).IsEqualTo("Plant1.AreaA.Tank7");
        await Assert.That(attributes.Hr).IsEqualTo(S_OK);
        await Assert.That(attributes.Ids).IsEquivalentTo([1, 2, 3, 4]);
        await Assert.That(attributes.Descriptions).IsEquivalentTo(["Batch", "Operator", "Area", "Temperature"]);
        await Assert.That(attributes.Types).IsEquivalentTo([(ushort)VarType.VT_BSTR, (ushort)VarType.VT_BSTR, (ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8]);
    }

    [Test]
    public async Task Translate_and_condition_state_happy_paths_roundtrip_native_payloads()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new RecordingDispatcher();
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);
        int[] requestedAttributes = [501, 502, 503];

        Helpers.TranslateResult translated = Helpers.InvokeTranslateToItemIDs(
            eventServer,
            "Plant1.AreaA.Tank7",
            1001,
            "Level",
            "HiHi",
            requestedAttributes);
        Helpers.ConditionStateResult state = Helpers.InvokeGetConditionState(eventServer, "Plant1.AreaA.Tank7", "Level", [1, 2]);

        await Assert.That(translated.Hr).IsEqualTo(S_OK);
        await Assert.That(translated.ItemIds).IsEquivalentTo(["Tank7.Level", "Tank7.Operator", "Tank7.Area"]);
        await Assert.That(translated.NodeNames).IsEquivalentTo(["Tank7", "OperatorNode", "AreaNode"]);
        await Assert.That(translated.Clsids).IsEquivalentTo(dispatcher.TranslateClsids);
        await Assert.That(dispatcher.LastTranslateAttributeIds).IsEquivalentTo(requestedAttributes);
        await Assert.That(state.Hr).IsEqualTo(S_OK);
        await Assert.That(state.Native.wState).IsEqualTo((ushort)3);
        await Assert.That(state.ActiveSubCondition).IsEqualTo("HiHi");
        await Assert.That(state.ActiveDefinition).IsEqualTo("High-high level alarm");
        await Assert.That(state.ActiveSeverity).IsEqualTo(900u);
        await Assert.That(state.Quality).IsEqualTo(OpcQuality.Good.RawValue);
        await Assert.That(state.SubConditionNames).IsEquivalentTo(["Hi", "HiHi"]);
        await Assert.That(state.SubConditionSeverities).IsEquivalentTo([700, 900]);
        await Assert.That(state.EventAttributeStrings).IsEquivalentTo(["Batch42", "42"]);
        await Assert.That(state.Errors).IsEquivalentTo([S_OK, E_FAIL]);
    }

    [Test]
    public async Task Condition_ops_and_ack_happy_paths_dispatch_arrays_and_errors()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new RecordingDispatcher();
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);

        int enableAreaHr = Helpers.InvokeConditionNameArray(eventServer, 13, ["Plant1.AreaA", "Plant1.AreaB"]);
        int enableSourceHr = Helpers.InvokeConditionNameArray(eventServer, 14, ["Tank7", "Pump1"]);
        int disableAreaHr = Helpers.InvokeConditionNameArray(eventServer, 15, ["Plant2.AreaA"]);
        int disableSourceHr = Helpers.InvokeConditionNameArray(eventServer, 16, ["Valve9", "Valve10"]);
        Helpers.AckResult ack = Helpers.InvokeAckCondition(
            eventServer,
            "operator1",
            "acknowledged",
            ["Tank7", "Pump1"],
            ["Level", "Pressure"],
            [new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero).ToFileTime(), new DateTimeOffset(2026, 1, 2, 3, 4, 6, TimeSpan.Zero).ToFileTime()],
            [11, 12]);

        await Assert.That(enableAreaHr).IsEqualTo(S_OK);
        await Assert.That(enableSourceHr).IsEqualTo(S_OK);
        await Assert.That(disableAreaHr).IsEqualTo(S_OK);
        await Assert.That(disableSourceHr).IsEqualTo(S_OK);
        await Assert.That(dispatcher.EnabledAreas).IsEquivalentTo(["Plant1.AreaA", "Plant1.AreaB"]);
        await Assert.That(dispatcher.EnabledSources).IsEquivalentTo(["Tank7", "Pump1"]);
        await Assert.That(dispatcher.DisabledAreas).IsEquivalentTo(["Plant2.AreaA"]);
        await Assert.That(dispatcher.DisabledSources).IsEquivalentTo(["Valve9", "Valve10"]);
        await Assert.That(ack.Hr).IsEqualTo(S_OK);
        await Assert.That(ack.Errors).IsEquivalentTo([S_OK, E_FAIL]);
        await Assert.That(dispatcher.LastAcknowledgerId).IsEqualTo("operator1");
        await Assert.That(dispatcher.LastAckSources).IsEquivalentTo(["Tank7", "Pump1"]);
        await Assert.That(dispatcher.LastAckCookies).IsEquivalentTo([11, 12]);
    }

    [Test]
    public async Task Returned_attributes_happy_path_uses_managed_subscription_selection()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var server = new RecordingReturnedAttributesServer();
        IntPtr ccw = OpcAeServerCcw.Create(server, s_iidUnknown);
        IntPtr subscription = Helpers.InvokeQI(ccw, IOPCEventSubscriptionMgt.InterfaceId);

        int setHr = Helpers.InvokeSetReturnedAttributes(subscription, 1001, [9, 8, 7]);
        Helpers.ReturnedAttributesResult returned = Helpers.InvokeGetReturnedAttributes(subscription, 1001);

        await Assert.That(setHr).IsEqualTo(S_OK);
        await Assert.That(server.LastReturnedAttributeCategory).IsEqualTo(1001);
        await Assert.That(server.ReturnedAttributes).IsEquivalentTo([9, 8, 7]);
        await Assert.That(returned.Hr).IsEqualTo(S_OK);
        await Assert.That(returned.AttributeIds).IsEquivalentTo([9, 8, 7]);
    }

    [Test]
    public async Task Invalid_arguments_return_E_INVALIDARG_and_clear_outputs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new RecordingDispatcher();
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);
        var server = new RecordingReturnedAttributesServer();
        IntPtr subscription = Helpers.InvokeQI(OpcAeServerCcw.Create(server, s_iidUnknown), IOPCEventSubscriptionMgt.InterfaceId);

        await Assert.That(Helpers.InvokeQueryEventCategoriesRaw(eventServer, IntPtr.Zero).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeQueryConditionNamesRaw(eventServer, IntPtr.Zero).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeQuerySubConditionNamesRaw(eventServer, IntPtr.Zero, Helpers.AllocateInt32()).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeQuerySourceConditionsRaw(eventServer, IntPtr.Zero, Helpers.AllocateInt32()).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeQueryEventAttributesRaw(eventServer, IntPtr.Zero).Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeTranslateToItemIDsInvalid(eventServer)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeGetConditionStateInvalid(eventServer)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeConditionNameArrayRaw(eventServer, 13, 0, IntPtr.Zero)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeConditionNameArrayRaw(eventServer, 14, 0, IntPtr.Zero)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeConditionNameArrayRaw(eventServer, 15, 0, IntPtr.Zero)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeConditionNameArrayRaw(eventServer, 16, 0, IntPtr.Zero)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeAckConditionInvalid(eventServer)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeSetReturnedAttributesRaw(subscription, 1001, 1, IntPtr.Zero)).IsEqualTo(E_INVALIDARG);
        await Assert.That(Helpers.InvokeGetReturnedAttributesRaw(subscription, 1001, IntPtr.Zero).Hr).IsEqualTo(E_INVALIDARG);
    }

    [Test]
    public async Task Mismatched_dispatcher_arrays_return_E_INVALIDARG_with_null_outputs()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new RecordingDispatcher { EventCategoryDescriptions = ["only-one"] };
        IntPtr eventServer = Helpers.CreateEventServer(dispatcher);

        Helpers.CategoryResult categories = Helpers.InvokeQueryEventCategories(eventServer, 7);
        dispatcher.EventCategoryDescriptions = ["Process", "System", "Tracking"];
        dispatcher.AttributeTypes = [(ushort)VarType.VT_BSTR];
        Helpers.AttributeResult attributes = Helpers.InvokeQueryEventAttributes(eventServer, 1001);
        dispatcher.AttributeTypes = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_BSTR, (ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
        dispatcher.TranslateNodeNames = ["too-short"];
        Helpers.TranslateResult translated = Helpers.InvokeTranslateToItemIDs(eventServer, "Source", 1001, "Condition", "Sub", [1, 2, 3]);
        dispatcher.TranslateNodeNames = ["Tank7", "OperatorNode", "AreaNode"];
        dispatcher.AckErrors = [S_OK];
        Helpers.AckResult ack = Helpers.InvokeAckCondition(eventServer, "operator1", "comment", ["S1", "S2"], ["C1", "C2"], [1, 2], [10, 11]);

        await Assert.That(categories.Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(categories.Ids).IsEmpty();
        await Assert.That(attributes.Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(attributes.Ids).IsEmpty();
        await Assert.That(translated.Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(translated.ItemIds).IsEmpty();
        await Assert.That(ack.Hr).IsEqualTo(E_INVALIDARG);
        await Assert.That(ack.Errors).IsEmpty();
    }

    private sealed class RecordingDispatcher : IOpcAeServerDispatcher
    {
        public int[] EventCategories { get; set; } = [1001, 1002, 1003];
        public string[] EventCategoryDescriptions { get; set; } = ["Process", "System", "Tracking"];
        public string[] ConditionNames { get; set; } = ["Level", "Pressure"];
        public string[] SubConditionNames { get; set; } = ["Hi", "HiHi"];
        public string[] SourceConditions { get; set; } = ["Level", "ValveFailure"];
        public int[] AttributeIds { get; set; } = [1, 2, 3, 4];
        public string[] AttributeDescriptions { get; set; } = ["Batch", "Operator", "Area", "Temperature"];
        public ushort[] AttributeTypes { get; set; } = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_BSTR, (ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
        public string[] TranslateItemIds { get; set; } = ["Tank7.Level", "Tank7.Operator", "Tank7.Area"];
        public string[] TranslateNodeNames { get; set; } = ["Tank7", "OperatorNode", "AreaNode"];
        public Guid[] TranslateClsids { get; } = [Guid.Parse("11111111-1111-1111-1111-111111111111"), Guid.Parse("22222222-2222-2222-2222-222222222222"), Guid.Parse("33333333-3333-3333-3333-333333333333")];
        public int[] AckErrors { get; set; } = [S_OK, E_FAIL];
        public int LastQueryEventType { get; private set; }
        public int LastConditionCategory { get; private set; }
        public string? LastSubConditionName { get; private set; }
        public string? LastSource { get; private set; }
        public int[] LastTranslateAttributeIds { get; private set; } = [];
        public string[] EnabledAreas { get; private set; } = [];
        public string[] EnabledSources { get; private set; } = [];
        public string[] DisabledAreas { get; private set; } = [];
        public string[] DisabledSources { get; private set; } = [];
        public string? LastAcknowledgerId { get; private set; }
        public string[] LastAckSources { get; private set; } = [];
        public int[] LastAckCookies { get; private set; } = [];

        public Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
            Task.FromResult(new NdrCallResult(E_FAIL, ReadOnlyMemory<byte>.Empty));

        public Task QueryEventCategoriesAsync(int eventType, out int[] eventCategories, out string[] eventCategoryDescriptions, CancellationToken cancellationToken = default)
        {
            LastQueryEventType = eventType;
            eventCategories = EventCategories;
            eventCategoryDescriptions = EventCategoryDescriptions;
            return Task.CompletedTask;
        }

        public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
        {
            LastConditionCategory = eventCategory;
            return Task.FromResult(ConditionNames);
        }

        public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
        {
            LastSubConditionName = conditionName;
            return Task.FromResult(SubConditionNames);
        }

        public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
        {
            LastSource = source;
            return Task.FromResult(SourceConditions);
        }

        public Task QueryEventAttributesAsync(int eventCategory, out int[] attributeIds, out string[] attributeDescriptions, out ushort[] attributeTypes, CancellationToken cancellationToken = default)
        {
            _ = eventCategory;
            attributeIds = AttributeIds;
            attributeDescriptions = AttributeDescriptions;
            attributeTypes = AttributeTypes;
            return Task.CompletedTask;
        }

        public Task TranslateToItemIDsAsync(string source, int eventCategory, string conditionName, string subconditionName, int[] associatedAttributeIds, out string[] attributeItemIds, out string[] nodeNames, out Guid[] classIds, CancellationToken cancellationToken = default)
        {
            _ = source;
            _ = eventCategory;
            _ = conditionName;
            _ = subconditionName;
            LastTranslateAttributeIds = associatedAttributeIds;
            attributeItemIds = TranslateItemIds;
            nodeNames = TranslateNodeNames;
            classIds = TranslateClsids;
            return Task.CompletedTask;
        }

        public Task<OpcConditionState> GetConditionStateAsync(string source, string conditionName, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            _ = source;
            _ = conditionName;
            _ = attributeIds;
            return Task.FromResult(BuildConditionState());
        }

        public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
        {
            EnabledAreas = areas;
            return Task.CompletedTask;
        }

        public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
        {
            EnabledSources = sources;
            return Task.CompletedTask;
        }

        public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
        {
            DisabledAreas = areas;
            return Task.CompletedTask;
        }

        public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
        {
            DisabledSources = sources;
            return Task.CompletedTask;
        }

        public Task<int[]> AckConditionAsync(int dwCount, string acknowledgerId, string comment, string[] sources, string[] conditionNames, long[] activeTimes, int[] cookies, CancellationToken cancellationToken = default)
        {
            _ = dwCount;
            _ = comment;
            _ = activeTimes;
            _ = conditionNames;
            LastAcknowledgerId = acknowledgerId;
            LastAckCookies = cookies;
            LastAckSources = sources;
            return Task.FromResult(AckErrors);
        }

        private static OpcConditionState BuildConditionState() => new(
            state: 3,
            activeSubCondition: "HiHi",
            activeSubConditionDefinition: "High-high level alarm",
            activeSubConditionSeverity: 900,
            activeSubConditionDescription: "Tank level is high-high",
            quality: OpcQuality.Good,
            lastAckTime: new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            subConditionLastActive: new DateTimeOffset(2026, 1, 2, 3, 4, 6, TimeSpan.Zero),
            conditionLastActive: new DateTimeOffset(2026, 1, 2, 3, 4, 7, TimeSpan.Zero),
            conditionLastInactive: new DateTimeOffset(2026, 1, 2, 3, 4, 8, TimeSpan.Zero),
            acknowledgerId: "operator1",
            comment: "acknowledged",
            subConditionNames: ["Hi", "HiHi"],
            subConditionDefinitions: ["High level", "High-high level"],
            subConditionSeverities: [700, 900],
            subConditionDescriptions: ["High", "High-high"],
            eventAttributes: [OpcVariant.FromString("Batch42"), OpcVariant.FromInt32(42)],
            errors: [S_OK, E_FAIL]);
    }

    private sealed class RecordingReturnedAttributesServer : IOpcAeServer, IOPCEventSubscriptionMgt
    {
        public int LastReturnedAttributeCategory { get; private set; }
        public int[] ReturnedAttributes { get; private set; } = [];

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Ae, VendorInfo = "test" });

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task SetFilterAsync(int eventType, int[] eventCategories, int lowSeverity, int highSeverity, string[] areas, string[] sources, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task GetFilterAsync(out int eventType, out int[] eventCategories, out int lowSeverity, out int highSeverity, out string[] areas, out string[] sources, CancellationToken cancellationToken = default)
        {
            eventType = 0;
            eventCategories = [];
            lowSeverity = 0;
            highSeverity = 0;
            areas = [];
            sources = [];
            return Task.CompletedTask;
        }

        public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            LastReturnedAttributeCategory = eventCategory;
            ReturnedAttributes = attributeIds;
            return Task.CompletedTask;
        }

        public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default)
        {
            LastReturnedAttributeCategory = eventCategory;
            return Task.FromResult(ReturnedAttributes);
        }

        public Task RefreshAsync(int connection, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task GetStateAsync(out bool active, out int bufferTime, out int maxSize, out int clientSubscription, CancellationToken cancellationToken = default)
        {
            active = true;
            bufferTime = 0;
            maxSize = 0;
            clientSubscription = 0;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(bool active, int bufferTime, int maxSize, int clientSubscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken cancellationToken = default)
        {
            _ = active;
            revisedBufferTime = bufferTime;
            revisedMaxSize = maxSize;
            return Task.CompletedTask;
        }
    }

    private static class Helpers
    {
        internal readonly record struct CategoryResult(int Hr, int[] Ids, string[] Descriptions);
        internal readonly record struct StringArrayResult(int Hr, string[] Values);
        internal readonly record struct AttributeResult(int Hr, int[] Ids, string[] Descriptions, ushort[] Types);
        internal readonly record struct TranslateResult(int Hr, string[] ItemIds, string[] NodeNames, Guid[] Clsids);
        internal readonly record struct AckResult(int Hr, int[] Errors);
        internal readonly record struct ReturnedAttributesResult(int Hr, int[] AttributeIds);
        internal readonly record struct ConditionStateResult(int Hr, OPCCONDITIONSTATE_NATIVE Native, string? ActiveSubCondition, string? ActiveDefinition, uint ActiveSeverity, ushort Quality, string[] SubConditionNames, int[] SubConditionSeverities, string[] EventAttributeStrings, int[] Errors);

        internal static IntPtr CreateEventServer(IOpcAeServerDispatcher dispatcher)
        {
            IntPtr ccw = OpcAeServerCcw.Create(dispatcher, s_iidUnknown);
            return InvokeQI(ccw, IOPCEventServer.InterfaceId);
        }

        internal static IntPtr InvokeQI(IntPtr ccw, Guid iid)
        {
            QueryInterfaceDelegate qi = GetMethod<QueryInterfaceDelegate>(ccw, 0);
            int hr = qi(ccw, ref iid, out IntPtr returned);
            return hr == S_OK ? returned : IntPtr.Zero;
        }

        internal static CategoryResult InvokeQueryEventCategories(IntPtr eventServer, int eventType)
        {
            QueryEventCategoriesDelegate query = GetMethod<QueryEventCategoriesDelegate>(eventServer, 6);
            IntPtr pCount = AllocateInt32();
            IntPtr idsPtr = IntPtr.Zero;
            IntPtr descriptionsPtr = IntPtr.Zero;
            try
            {
                int hr = query(eventServer, eventType, pCount, out idsPtr, out descriptionsPtr);
                int count = Marshal.ReadInt32(pCount);
                return new CategoryResult(hr, ReadInt32Array(idsPtr, count), ReadBstrArray(descriptionsPtr, count));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pCount);
                FreeCoTaskMem(idsPtr);
                FreeBstrArray(descriptionsPtr, CountBstrPointers(descriptionsPtr));
            }
        }

        internal static (int Hr, IntPtr Output) InvokeQueryEventCategoriesRaw(IntPtr eventServer, IntPtr pCount)
        {
            QueryEventCategoriesDelegate query = GetMethod<QueryEventCategoriesDelegate>(eventServer, 6);
            int hr = query(eventServer, 0, pCount, out IntPtr output, out _);
            return (hr, output);
        }

        internal static StringArrayResult InvokeQueryConditionNames(IntPtr eventServer, int eventCategory)
        {
            QueryConditionNamesDelegate query = GetMethod<QueryConditionNamesDelegate>(eventServer, 7);
            return InvokeStringArray((IntPtr self, IntPtr pCount, out IntPtr values) => query(self, eventCategory, pCount, out values), eventServer);
        }

        internal static (int Hr, IntPtr Output) InvokeQueryConditionNamesRaw(IntPtr eventServer, IntPtr pCount)
        {
            QueryConditionNamesDelegate query = GetMethod<QueryConditionNamesDelegate>(eventServer, 7);
            int hr = query(eventServer, 0, pCount, out IntPtr output);
            return (hr, output);
        }

        internal static StringArrayResult InvokeQuerySubConditionNames(IntPtr eventServer, string conditionName)
        {
            QuerySubConditionNamesDelegate query = GetMethod<QuerySubConditionNamesDelegate>(eventServer, 8);
            IntPtr conditionNamePtr = Marshal.StringToCoTaskMemUni(conditionName);
            try
            {
                return InvokeStringArray((IntPtr self, IntPtr pCount, out IntPtr values) => query(self, conditionNamePtr, pCount, out values), eventServer);
            }
            finally
            {
                Marshal.FreeCoTaskMem(conditionNamePtr);
            }
        }

        internal static (int Hr, IntPtr Output) InvokeQuerySubConditionNamesRaw(IntPtr eventServer, IntPtr conditionName, IntPtr pCount)
        {
            QuerySubConditionNamesDelegate query = GetMethod<QuerySubConditionNamesDelegate>(eventServer, 8);
            int hr = query(eventServer, conditionName, pCount, out IntPtr output);
            return (hr, output);
        }

        internal static StringArrayResult InvokeQuerySourceConditions(IntPtr eventServer, string source)
        {
            QuerySourceConditionsDelegate query = GetMethod<QuerySourceConditionsDelegate>(eventServer, 9);
            IntPtr sourcePtr = Marshal.StringToCoTaskMemUni(source);
            try
            {
                return InvokeStringArray((IntPtr self, IntPtr pCount, out IntPtr values) => query(self, sourcePtr, pCount, out values), eventServer);
            }
            finally
            {
                Marshal.FreeCoTaskMem(sourcePtr);
            }
        }

        internal static (int Hr, IntPtr Output) InvokeQuerySourceConditionsRaw(IntPtr eventServer, IntPtr source, IntPtr pCount)
        {
            QuerySourceConditionsDelegate query = GetMethod<QuerySourceConditionsDelegate>(eventServer, 9);
            int hr = query(eventServer, source, pCount, out IntPtr output);
            return (hr, output);
        }

        internal static AttributeResult InvokeQueryEventAttributes(IntPtr eventServer, int eventCategory)
        {
            QueryEventAttributesDelegate query = GetMethod<QueryEventAttributesDelegate>(eventServer, 10);
            IntPtr pCount = AllocateInt32();
            IntPtr idsPtr = IntPtr.Zero;
            IntPtr descriptionsPtr = IntPtr.Zero;
            IntPtr typesPtr = IntPtr.Zero;
            try
            {
                int hr = query(eventServer, eventCategory, pCount, out idsPtr, out descriptionsPtr, out typesPtr);
                int count = Marshal.ReadInt32(pCount);
                return new AttributeResult(hr, ReadInt32Array(idsPtr, count), ReadBstrArray(descriptionsPtr, count), ReadUInt16Array(typesPtr, count));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pCount);
                FreeCoTaskMem(idsPtr);
                FreeBstrArray(descriptionsPtr, CountBstrPointers(descriptionsPtr));
                FreeCoTaskMem(typesPtr);
            }
        }

        internal static (int Hr, IntPtr Output) InvokeQueryEventAttributesRaw(IntPtr eventServer, IntPtr pCount)
        {
            QueryEventAttributesDelegate query = GetMethod<QueryEventAttributesDelegate>(eventServer, 10);
            int hr = query(eventServer, 0, pCount, out IntPtr output, out _, out _);
            return (hr, output);
        }

        internal static TranslateResult InvokeTranslateToItemIDs(IntPtr eventServer, string source, int category, string condition, string subcondition, int[] attributeIds)
        {
            TranslateToItemIDsDelegate translate = GetMethod<TranslateToItemIDsDelegate>(eventServer, 11);
            IntPtr sourcePtr = Marshal.StringToCoTaskMemUni(source);
            IntPtr conditionPtr = Marshal.StringToCoTaskMemUni(condition);
            IntPtr subconditionPtr = Marshal.StringToCoTaskMemUni(subcondition);
            IntPtr attributeIdsPtr = AllocateInt32Array(attributeIds);
            IntPtr itemIdsPtr = IntPtr.Zero;
            IntPtr nodeNamesPtr = IntPtr.Zero;
            IntPtr clsidsPtr = IntPtr.Zero;
            try
            {
                int hr = translate(eventServer, sourcePtr, category, conditionPtr, subconditionPtr, attributeIds.Length, attributeIdsPtr, out itemIdsPtr, out nodeNamesPtr, out clsidsPtr);
                return new TranslateResult(hr, ReadBstrArray(itemIdsPtr, attributeIds.Length), ReadBstrArray(nodeNamesPtr, attributeIds.Length), ReadGuidArray(clsidsPtr, attributeIds.Length));
            }
            finally
            {
                Marshal.FreeCoTaskMem(sourcePtr);
                Marshal.FreeCoTaskMem(conditionPtr);
                Marshal.FreeCoTaskMem(subconditionPtr);
                FreeCoTaskMem(attributeIdsPtr);
                FreeBstrArray(itemIdsPtr, CountBstrPointers(itemIdsPtr));
                FreeBstrArray(nodeNamesPtr, CountBstrPointers(nodeNamesPtr));
                FreeCoTaskMem(clsidsPtr);
            }
        }

        internal static int InvokeTranslateToItemIDsInvalid(IntPtr eventServer)
        {
            TranslateToItemIDsDelegate translate = GetMethod<TranslateToItemIDsDelegate>(eventServer, 11);
            return translate(eventServer, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, out _, out _, out _);
        }

        internal static ConditionStateResult InvokeGetConditionState(IntPtr eventServer, string source, string condition, int[] attributeIds)
        {
            GetConditionStateDelegate getState = GetMethod<GetConditionStateDelegate>(eventServer, 12);
            IntPtr sourcePtr = Marshal.StringToCoTaskMemUni(source);
            IntPtr conditionPtr = Marshal.StringToCoTaskMemUni(condition);
            IntPtr attributeIdsPtr = AllocateInt32Array(attributeIds);
            IntPtr statePtr = IntPtr.Zero;
            try
            {
                int hr = getState(eventServer, sourcePtr, conditionPtr, attributeIds.Length, attributeIdsPtr, out statePtr);
                if (statePtr == IntPtr.Zero)
                {
                    return new ConditionStateResult(hr, default, null, null, 0, 0, [], [], [], []);
                }

                OPCCONDITIONSTATE_NATIVE native = Marshal.PtrToStructure<OPCCONDITIONSTATE_NATIVE>(statePtr);
                string[] subConditionNames = ReadBstrArray(native.pszSCNames, native.dwNumSCs);
                int[] subConditionSeverities = ReadInt32Array(native.pdwSCSeverities, native.dwNumSCs);
                string[] attributes = ReadVariantStrings(native.pEventAttributes, native.dwNumEventAttrs);
                int[] errors = ReadInt32Array(native.pErrors, native.dwNumEventAttrs);
                return new ConditionStateResult(
                    hr,
                    native,
                    ReadBstr(native.szActiveSubCondition),
                    ReadBstr(native.szASCDefinition),
                    native.dwASCSeverity,
                    native.wQuality,
                    subConditionNames,
                    subConditionSeverities,
                    attributes,
                    errors);
            }
            finally
            {
                Marshal.FreeCoTaskMem(sourcePtr);
                Marshal.FreeCoTaskMem(conditionPtr);
                FreeCoTaskMem(attributeIdsPtr);
                FreeConditionState(statePtr);
            }
        }

        internal static int InvokeGetConditionStateInvalid(IntPtr eventServer)
        {
            GetConditionStateDelegate getState = GetMethod<GetConditionStateDelegate>(eventServer, 12);
            return getState(eventServer, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, out _);
        }

        internal static int InvokeConditionNameArray(IntPtr eventServer, int slot, string[] values)
        {
            ConditionNameArrayDelegate invoke = GetMethod<ConditionNameArrayDelegate>(eventServer, slot);
            IntPtr valuesPtr = AllocateBstrArray(values);
            try
            {
                return invoke(eventServer, values.Length, valuesPtr);
            }
            finally
            {
                FreeBstrArray(valuesPtr, values.Length);
            }
        }

        internal static int InvokeConditionNameArrayRaw(IntPtr eventServer, int slot, int count, IntPtr values)
        {
            ConditionNameArrayDelegate invoke = GetMethod<ConditionNameArrayDelegate>(eventServer, slot);
            return invoke(eventServer, count, values);
        }

        internal static AckResult InvokeAckCondition(IntPtr eventServer, string acknowledger, string comment, string[] sources, string[] conditions, long[] activeTimes, int[] cookies)
        {
            AckConditionDelegate ack = GetMethod<AckConditionDelegate>(eventServer, 17);
            IntPtr acknowledgerPtr = Marshal.StringToCoTaskMemUni(acknowledger);
            IntPtr commentPtr = Marshal.StringToCoTaskMemUni(comment);
            IntPtr sourcesPtr = AllocateBstrArray(sources);
            IntPtr conditionsPtr = AllocateBstrArray(conditions);
            IntPtr activeTimesPtr = AllocateInt64Array(activeTimes);
            IntPtr cookiesPtr = AllocateInt32Array(cookies);
            IntPtr errorsPtr = IntPtr.Zero;
            try
            {
                int hr = ack(eventServer, sources.Length, acknowledgerPtr, commentPtr, sourcesPtr, conditionsPtr, activeTimesPtr, cookiesPtr, out errorsPtr);
                return new AckResult(hr, ReadInt32Array(errorsPtr, sources.Length));
            }
            finally
            {
                Marshal.FreeCoTaskMem(acknowledgerPtr);
                Marshal.FreeCoTaskMem(commentPtr);
                FreeBstrArray(sourcesPtr, sources.Length);
                FreeBstrArray(conditionsPtr, conditions.Length);
                FreeCoTaskMem(activeTimesPtr);
                FreeCoTaskMem(cookiesPtr);
                FreeCoTaskMem(errorsPtr);
            }
        }

        internal static int InvokeAckConditionInvalid(IntPtr eventServer)
        {
            AckConditionDelegate ack = GetMethod<AckConditionDelegate>(eventServer, 17);
            return ack(eventServer, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out _);
        }

        internal static int InvokeSetReturnedAttributes(IntPtr subscription, int eventCategory, int[] attributeIds)
        {
            SetReturnedAttributesDelegate set = GetMethod<SetReturnedAttributesDelegate>(subscription, 5);
            IntPtr idsPtr = AllocateInt32Array(attributeIds);
            try
            {
                return set(subscription, eventCategory, attributeIds.Length, idsPtr);
            }
            finally
            {
                FreeCoTaskMem(idsPtr);
            }
        }

        internal static int InvokeSetReturnedAttributesRaw(IntPtr subscription, int eventCategory, int count, IntPtr attributeIds)
        {
            SetReturnedAttributesDelegate set = GetMethod<SetReturnedAttributesDelegate>(subscription, 5);
            return set(subscription, eventCategory, count, attributeIds);
        }

        internal static ReturnedAttributesResult InvokeGetReturnedAttributes(IntPtr subscription, int eventCategory)
        {
            GetReturnedAttributesDelegate get = GetMethod<GetReturnedAttributesDelegate>(subscription, 6);
            IntPtr pCount = AllocateInt32();
            IntPtr idsPtr = IntPtr.Zero;
            try
            {
                int hr = get(subscription, eventCategory, pCount, out idsPtr);
                int count = Marshal.ReadInt32(pCount);
                return new ReturnedAttributesResult(hr, ReadInt32Array(idsPtr, count));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pCount);
                FreeCoTaskMem(idsPtr);
            }
        }

        internal static (int Hr, IntPtr Output) InvokeGetReturnedAttributesRaw(IntPtr subscription, int eventCategory, IntPtr pCount)
        {
            GetReturnedAttributesDelegate get = GetMethod<GetReturnedAttributesDelegate>(subscription, 6);
            int hr = get(subscription, eventCategory, pCount, out IntPtr output);
            return (hr, output);
        }

        internal static IntPtr AllocateInt32()
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(sizeof(int));
            Marshal.WriteInt32(ptr, 0);
            return ptr;
        }

        private delegate int StringArrayInvoker(IntPtr self, IntPtr pCount, out IntPtr values);

        private static StringArrayResult InvokeStringArray(StringArrayInvoker invoker, IntPtr eventServer)
        {
            IntPtr pCount = AllocateInt32();
            IntPtr valuesPtr = IntPtr.Zero;
            try
            {
                int hr = invoker(eventServer, pCount, out valuesPtr);
                int count = Marshal.ReadInt32(pCount);
                return new StringArrayResult(hr, ReadBstrArray(valuesPtr, count));
            }
            finally
            {
                Marshal.FreeCoTaskMem(pCount);
                FreeBstrArray(valuesPtr, CountBstrPointers(valuesPtr));
            }
        }

        private static IntPtr AllocateInt32Array(int[] values)
        {
            if (values.Length == 0)
            {
                return IntPtr.Zero;
            }
            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(int));
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteInt32(ptr, i * sizeof(int), values[i]);
            }
            return ptr;
        }

        private static IntPtr AllocateInt64Array(long[] values)
        {
            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * sizeof(long));
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteInt64(ptr, i * sizeof(long), values[i]);
            }
            return ptr;
        }

        private static IntPtr AllocateBstrArray(string[] values)
        {
            if (values.Length == 0)
            {
                return IntPtr.Zero;
            }
            IntPtr ptr = Marshal.AllocCoTaskMem(values.Length * IntPtr.Size);
            for (int i = 0; i < values.Length; i++)
            {
                Marshal.WriteIntPtr(ptr, i * IntPtr.Size, Marshal.StringToBSTR(values[i]));
            }
            return ptr;
        }

        private static int[] ReadInt32Array(IntPtr ptr, int count)
        {
            if (count == 0 || ptr == IntPtr.Zero)
            {
                return [];
            }
            var values = new int[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = Marshal.ReadInt32(ptr, i * sizeof(int));
            }
            return values;
        }

        private static ushort[] ReadUInt16Array(IntPtr ptr, int count)
        {
            if (count == 0 || ptr == IntPtr.Zero)
            {
                return [];
            }
            var values = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = unchecked((ushort)Marshal.ReadInt16(ptr, i * sizeof(ushort)));
            }
            return values;
        }

        private static string[] ReadBstrArray(IntPtr ptr, int count)
        {
            if (count == 0 || ptr == IntPtr.Zero)
            {
                return [];
            }
            var values = new string[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadBstr(Marshal.ReadIntPtr(ptr, i * IntPtr.Size)) ?? string.Empty;
            }
            return values;
        }

        private static Guid[] ReadGuidArray(IntPtr ptr, int count)
        {
            if (count == 0 || ptr == IntPtr.Zero)
            {
                return [];
            }
            var values = new Guid[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = Marshal.PtrToStructure<Guid>(ptr + (i * 16));
            }
            return values;
        }

        private static string[] ReadVariantStrings(IntPtr ptr, int count)
        {
            if (count == 0 || ptr == IntPtr.Zero)
            {
                return [];
            }
            var values = new string[count];
            int variantSize = IntPtr.Size == 8 ? 24 : 16;
            for (int i = 0; i < count; i++)
            {
                IntPtr slot = ptr + (i * variantSize);
                ushort vt = unchecked((ushort)Marshal.ReadInt16(slot));
                IntPtr value = slot + 8;
                values[i] = vt switch
                {
                    (ushort)VarType.VT_BSTR => ReadBstr(Marshal.ReadIntPtr(value)) ?? string.Empty,
                    (ushort)VarType.VT_I4 => Marshal.ReadInt32(value).ToString(System.Globalization.CultureInfo.InvariantCulture),
                    _ => string.Empty,
                };
            }
            return values;
        }

        private static string? ReadBstr(IntPtr ptr) =>
            ptr == IntPtr.Zero ? null : Marshal.PtrToStringBSTR(ptr);

        private static int CountBstrPointers(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return 0;
            }
            int count = 0;
            while (Marshal.ReadIntPtr(ptr, count * IntPtr.Size) != IntPtr.Zero)
            {
                count++;
            }
            return count;
        }

        private static void FreeCoTaskMem(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        private static void FreeBstrArray(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }
            for (int i = 0; i < count; i++)
            {
                IntPtr bstr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                if (bstr != IntPtr.Zero)
                {
                    Marshal.FreeBSTR(bstr);
                }
            }
            Marshal.FreeCoTaskMem(ptr);
        }

        private static void FreeConditionState(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }
            OPCCONDITIONSTATE_NATIVE native = Marshal.PtrToStructure<OPCCONDITIONSTATE_NATIVE>(ptr);
            FreeBstr(native.szActiveSubCondition);
            FreeBstr(native.szASCDefinition);
            FreeBstr(native.szASCDescription);
            FreeBstr(native.szAcknowledgerID);
            FreeBstr(native.szComment);
            FreeBstrArray(native.pszSCNames, native.dwNumSCs);
            FreeBstrArray(native.pszSCDefinitions, native.dwNumSCs);
            FreeCoTaskMem(native.pdwSCSeverities);
            FreeBstrArray(native.pszSCDescriptions, native.dwNumSCs);
            FreeVariantArray(native.pEventAttributes, native.dwNumEventAttrs);
            FreeCoTaskMem(native.pErrors);
            Marshal.FreeCoTaskMem(ptr);
        }

        private static void FreeVariantArray(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }
            int variantSize = IntPtr.Size == 8 ? 24 : 16;
            for (int i = 0; i < count; i++)
            {
                IntPtr slot = ptr + (i * variantSize);
                ushort vt = unchecked((ushort)Marshal.ReadInt16(slot));
                if (vt == (ushort)VarType.VT_BSTR)
                {
                    FreeBstr(Marshal.ReadIntPtr(slot + 8));
                }
            }
            Marshal.FreeCoTaskMem(ptr);
        }

        private static void FreeBstr(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeBSTR(ptr);
            }
        }

        private static T GetMethod<T>(IntPtr tearoff, int slot)
            where T : Delegate
        {
            IntPtr vtable = Marshal.ReadIntPtr(tearoff);
            IntPtr method = Marshal.ReadIntPtr(vtable, slot * IntPtr.Size);
            return Marshal.GetDelegateForFunctionPointer<T>(method);
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryInterfaceDelegate(IntPtr pThis, ref Guid riid, out IntPtr ppv);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryEventCategoriesDelegate(IntPtr pThis, int eventType, IntPtr pCount, out IntPtr ppCategories, out IntPtr ppDescriptions);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryConditionNamesDelegate(IntPtr pThis, int eventCategory, IntPtr pCount, out IntPtr ppNames);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QuerySubConditionNamesDelegate(IntPtr pThis, IntPtr conditionName, IntPtr pCount, out IntPtr ppNames);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QuerySourceConditionsDelegate(IntPtr pThis, IntPtr source, IntPtr pCount, out IntPtr ppNames);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int QueryEventAttributesDelegate(IntPtr pThis, int eventCategory, IntPtr pCount, out IntPtr ppIds, out IntPtr ppDescriptions, out IntPtr ppTypes);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int TranslateToItemIDsDelegate(IntPtr pThis, IntPtr source, int eventCategory, IntPtr conditionName, IntPtr subconditionName, int count, IntPtr assocAttrIds, out IntPtr ppItemIds, out IntPtr ppNodeNames, out IntPtr ppClsids);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetConditionStateDelegate(IntPtr pThis, IntPtr source, IntPtr conditionName, int eventAttrCount, IntPtr attributeIds, out IntPtr ppConditionState);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int ConditionNameArrayDelegate(IntPtr pThis, int count, IntPtr names);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int AckConditionDelegate(IntPtr pThis, int count, IntPtr acknowledgerId, IntPtr comment, IntPtr sources, IntPtr conditionNames, IntPtr activeTimes, IntPtr cookies, out IntPtr ppErrors);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SetReturnedAttributesDelegate(IntPtr pThis, int eventCategory, int count, IntPtr attributeIds);
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int GetReturnedAttributesDelegate(IntPtr pThis, int eventCategory, IntPtr pCount, out IntPtr ppAttributeIds);

        // Mirror production OpcAeArrayMarshaler.OPCCONDITIONSTATE_NATIVE which
        // uses natural alignment (no Pack) so x64 pointer fields land at
        // 8-byte aligned offsets the MIDL stub expects (see DR7 fix).
        [StructLayout(LayoutKind.Sequential)]
        internal struct OPCCONDITIONSTATE_NATIVE
        {
            public ushort wState;
            public ushort wReserved1;
            public IntPtr szActiveSubCondition;
            public IntPtr szASCDefinition;
            public uint dwASCSeverity;
            public IntPtr szASCDescription;
            public ushort wQuality;
            public ushort wReserved2;
            public long ftLastAckTime;
            public long ftSubCondLastActive;
            public long ftCondLastActive;
            public long ftCondLastInactive;
            public IntPtr szAcknowledgerID;
            public IntPtr szComment;
            public int dwNumSCs;
            public IntPtr pszSCNames;
            public IntPtr pszSCDefinitions;
            public IntPtr pdwSCSeverities;
            public IntPtr pszSCDescriptions;
            public int dwNumEventAttrs;
            public IntPtr pEventAttributes;
            public IntPtr pErrors;
        }
    }
}
