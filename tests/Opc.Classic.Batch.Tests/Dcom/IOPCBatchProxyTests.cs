// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Batch.Dcom;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Batch.Tests.Dcom;

public sealed class IOPCBatchProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task BatchServer_GetDelimiter_invokes_channel_and_decodes_string()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("/"));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCBatchServerClientProxy(channel);
        string delimiter = await proxy.GetDelimiterAsync(CancellationToken.None);

        int expectedOpnum = IOPCBatchServer.Opnums.GetDelimiterAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCBatchServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(delimiter).IsEqualTo("/");
    }

    [Test]
    public async Task Generated_BatchServer_proxy_and_dispatcher_match_delimiter_known_answer()
    {
        var dispatcher = new IOPCBatchServerServerDispatcher(new BatchServerStub());
        var channel = new InMemoryCallChannel(async (_, opnum, payload, cancellationToken) =>
        {
            DispatchResult dispatched = await dispatcher.DispatchAsync(opnum, payload, cancellationToken);
            return dispatched.ToNdrCallResult();
        });

        string delimiter = await new IOPCBatchServerClientProxy(channel).GetDelimiterAsync(CancellationToken.None);
        DispatchResult response = await dispatcher.DispatchAsync(
            IOPCBatchServer.Opnums.GetDelimiterAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(delimiter).IsEqualTo("/");
        await Assert.That(response.Payload.ToArray()).IsEquivalentTo(new byte[]
        {
            0x00, 0x00, 0x02, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x2F, 0x00, 0x00, 0x00,
        });
    }

    [Test]
    public async Task BatchServer_CreateEnumerator_round_trips_interface_ref()
    {
        Guid requestedRiid = IEnumOPCBatchSummary.InterfaceId;
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        Guid observedRiid = Guid.Empty;
        ReadOnlyMemory<byte> responsePayload = EncodeObjRef(requestedRiid);
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            observedRiid = reader.ReadGuid();
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCBatchServerClientProxy(channel);
        IOpcInterfaceRef interfaceRef = await proxy.CreateEnumeratorAsync(requestedRiid, CancellationToken.None);

        int expectedOpnum = IOPCBatchServer.Opnums.CreateEnumeratorAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCBatchServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedRiid).IsEqualTo(requestedRiid);
        await Assert.That(interfaceRef.Iid).IsEqualTo(requestedRiid);
    }

    [Test]
    public async Task Generated_BatchServer_CreateEnumerator_uses_MInterfacePointer_known_answer()
    {
        var dispatcher = new IOPCBatchServerServerDispatcher(new BatchServerStub());
        ReadOnlyMemory<byte> responsePayload = ReadOnlyMemory<byte>.Empty;
        var channel = new InMemoryCallChannel(async (_, opnum, payload, cancellationToken) =>
        {
            DispatchResult result = await dispatcher.DispatchAsync(opnum, payload, cancellationToken);
            responsePayload = result.Payload;
            return result.ToNdrCallResult();
        });

        IOpcInterfaceRef result = await new IOPCBatchServerClientProxy(channel)
            .CreateEnumeratorAsync(IEnumOPCBatchSummary.InterfaceId, CancellationToken.None);

        await Assert.That(result.Iid).IsEqualTo(IEnumOPCBatchSummary.InterfaceId);
        await Assert.That(Convert.ToHexString(responsePayload.Span[..16]))
            .IsEqualTo("0000020044000000440000004D454F57");
    }

    [Test]
    public async Task BatchServer2_CreateFilteredEnumerator_round_trips_filter_model_and_interface_ref()
    {
        Guid requestedRiid = IEnumOPCBatchSummary.InterfaceId;
        var filter = new OpcBatchSummaryFilter(
            Id: "B-2026",
            Description: "Description",
            OpcItemId: "Batch.B-2026",
            MasterRecipeId: "MR-1",
            MinBatchSize: 1.25f,
            MaxBatchSize: 2.5f,
            EngineeringUnits: "kg",
            ExecutionState: "RUNNING",
            ExecutionMode: "AUTOMATIC",
            MinStartTime: new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            MaxStartTime: new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero),
            MinEndTime: new DateTimeOffset(2026, 1, 3, 0, 0, 0, TimeSpan.Zero),
            MaxEndTime: new DateTimeOffset(2026, 1, 4, 0, 0, 0, TimeSpan.Zero));
        OpcBatchSummaryFilter? observedFilter = null;
        string? observedModel = null;
        Guid observedRiid = Guid.Empty;
        ReadOnlyMemory<byte> responsePayload = EncodeObjRef(requestedRiid);
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            Ensure(iid == IOPCBatchServer2.InterfaceId);
            Ensure(opnum == IOPCBatchServer2.Opnums.CreateFilteredEnumeratorAsync);
            var reader = new NdrReader(payload.Span);
            observedRiid = reader.ReadGuid();
            observedFilter = NdrOpcBatchSummaryFilterCodec.Read(ref reader);
            observedModel = reader.ReadUnicodeStringPtr();
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCBatchServer2ClientProxy(channel);
        IOpcInterfaceRef interfaceRef = await proxy.CreateFilteredEnumeratorAsync(
            requestedRiid,
            filter,
            "OPCBBatchModel",
            CancellationToken.None);

        await Assert.That(observedRiid).IsEqualTo(requestedRiid);
        await Assert.That(observedFilter).IsEqualTo(filter);
        await Assert.That(observedModel).IsEqualTo("OPCBBatchModel");
        await Assert.That(interfaceRef.Iid).IsEqualTo(requestedRiid);
    }

    [Test]
    public async Task BatchSummaryEnumerator_Clone_round_trips_interface_ref()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        Guid expectedIid = IEnumOPCBatchSummary.InterfaceId;
        ReadOnlyMemory<byte> responsePayload = EncodeObjRef(expectedIid);
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IEnumOPCBatchSummaryClientProxy(channel);
        IOpcInterfaceRef interfaceRef = await proxy.CloneAsync(CancellationToken.None);

        int expectedOpnum = IEnumOPCBatchSummary.Opnums.CloneAsync;
        await Assert.That(observedIid).IsEqualTo(IEnumOPCBatchSummary.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(interfaceRef.Iid).IsEqualTo(expectedIid);
    }

    [Test]
    public async Task BatchSummaryEnumerator_Count_decodes_int32()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(3));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IEnumOPCBatchSummaryClientProxy(channel);
        int count = await proxy.CountAsync(CancellationToken.None);

        int expectedOpnum = IEnumOPCBatchSummary.Opnums.CountAsync;
        await Assert.That(observedIid).IsEqualTo(IEnumOPCBatchSummary.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(count).IsEqualTo(3);
    }

    [Test]
    public async Task Generated_BatchSummary_Next_allows_short_final_page()
    {
        var expected = new OpcBatchSummary(
            "B-1",
            "Final page",
            "Batch.B-1",
            "MR-1",
            1.0f,
            "kg",
            "COMPLETE",
            "AUTOMATIC",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UnixEpoch);
        var dispatcher = new IEnumOPCBatchSummaryServerDispatcher(new BatchSummaryStub(expected));
        var channel = new InMemoryCallChannel(async (_, opnum, payload, cancellationToken) =>
        {
            DispatchResult result = await dispatcher.DispatchAsync(opnum, payload, cancellationToken);
            return result.ToNdrCallResult();
        });

        OpcBatchSummary[] page = await new IEnumOPCBatchSummaryClientProxy(channel)
            .NextAsync(10, CancellationToken.None);

        await Assert.That(page).IsEquivalentTo(new[] { expected });
    }

    [Test]
    public async Task Generated_BatchSummary_Next_matches_native_outer_pointer_and_deferred_fixture()
    {
        var summary = new OpcBatchSummary(
            "B",
            null,
            null,
            null,
            1.5f,
            null,
            null,
            null,
            FileTimeHelper.Epoch,
            FileTimeHelper.Epoch);
        var dispatcher = new IEnumOPCBatchSummaryServerDispatcher(new BatchSummaryStub(summary));
        byte[] request = [0x03, 0x00, 0x00, 0x00];
        byte[] expected =
        [
            0x00, 0x00, 0x02, 0x00, // ppSummaryArray outer referent
            0x01, 0x00, 0x00, 0x00, // conformant max_count = pceltFetched
            0x04, 0x00, 0x02, 0x00, // szID referent
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0xC0, 0x3F,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00, // deferred szID max_count
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x42, 0x00, 0x00, 0x00,
            0x01, 0x00, 0x00, 0x00, // pceltFetched
        ];

        DispatchResult response = await dispatcher.DispatchAsync(
            IEnumOPCBatchSummary.Opnums.NextAsync,
            request,
            CancellationToken.None);

        await Assert.That(response.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(response.Payload.ToArray()).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Generated_BatchSummary_Clone_dispatches_opnum_6_interface_pointer()
    {
        var dispatcher = new IEnumOPCBatchSummaryServerDispatcher(
            new BatchSummaryStub(
                new OpcBatchSummary(
                    "B",
                    null,
                    null,
                    null,
                    0,
                    null,
                    null,
                    null,
                    FileTimeHelper.Epoch,
                    FileTimeHelper.Epoch)));

        DispatchResult response = await dispatcher.DispatchAsync(
            IEnumOPCBatchSummary.Opnums.CloneAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);
        var reader = new NdrReader(response.Payload.Span);
        IOpcInterfaceRef? clone = OpcMInterfacePointerCodec.Read(ref reader);

        await Assert.That(response.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(clone?.Iid).IsEqualTo(IEnumOPCBatchSummary.InterfaceId);
    }

    [Test]
    public async Task EnumerationSets_QueryEnumerationSets_round_trips_parallel_arrays()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
        {
            WriteInt32Array(ref writer, 0, 2, 6);
            WriteStringArray(ref writer, "OPCB_ENUM_PHYS", "OPCB_ENUM_STATE", "OPCB_ENUM_RE_USE");
        });
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEnumerationSetsClientProxy(channel);
        await proxy.QueryEnumerationSetsAsync(out int[] setIds, out string[] setNames, CancellationToken.None);

        int expectedOpnum = IOPCEnumerationSets.Opnums.QueryEnumerationSetsAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCEnumerationSets.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(setIds).IsEquivalentTo([0, 2, 6]);
        await Assert.That(setNames).IsEquivalentTo(["OPCB_ENUM_PHYS", "OPCB_ENUM_STATE", "OPCB_ENUM_RE_USE"]);
    }

    [Test]
    public async Task EnumerationSets_QueryEnumeration_round_trips_set_value_and_name()
    {
        int observedSet = -1;
        int observedValue = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("RUNNING"));
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            Ensure(iid == IOPCEnumerationSets.InterfaceId);
            Ensure(opnum == IOPCEnumerationSets.Opnums.QueryEnumerationAsync);
            var reader = new NdrReader(payload.Span);
            observedSet = reader.ReadInt32();
            observedValue = reader.ReadInt32();
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEnumerationSetsClientProxy(channel);
        string name = await proxy.QueryEnumerationAsync(2, 1, CancellationToken.None);

        await Assert.That(observedSet).IsEqualTo(2);
        await Assert.That(observedValue).IsEqualTo(1);
        await Assert.That(name).IsEqualTo("RUNNING");
    }

    [Test]
    public async Task EnumerationSets_QueryEnumerationList_round_trips_parallel_arrays()
    {
        int observedSet = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
        {
            WriteInt32Array(ref writer, 0, 1, 2);
            WriteStringArray(ref writer, "IDLE", "RUNNING", "COMPLETE");
        });
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            Ensure(iid == IOPCEnumerationSets.InterfaceId);
            Ensure(opnum == IOPCEnumerationSets.Opnums.QueryEnumerationListAsync);
            var reader = new NdrReader(payload.Span);
            observedSet = reader.ReadInt32();
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEnumerationSetsClientProxy(channel);
        await proxy.QueryEnumerationListAsync(2, out int[] values, out string[] names, CancellationToken.None);

        await Assert.That(observedSet).IsEqualTo(2);
        await Assert.That(values).IsEquivalentTo([0, 1, 2]);
        await Assert.That(names).IsEquivalentTo(["IDLE", "RUNNING", "COMPLETE"]);
    }

    private static ReadOnlyMemory<byte> EncodeObjRef(Guid iid) => WritePayload((ref NdrWriter writer) =>
        OpcMInterfacePointerCodec.Write(ref writer, CreateObjRef(iid)));

    private static IOpcInterfaceRef CreateObjRef(Guid iid) =>
        new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 5,
            oxid: 1,
            oid: 2,
            ipid: Guid.Parse("A8080DA2-E23E-11D2-AFA7-00C04F539422"),
            securityOffset: 0,
            resolverBindings: []);

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 2048)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static void WriteInt32Array(ref NdrWriter writer, params int[] values)
    {
        writer.WriteUInt32((uint)values.Length);
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    private static void WriteStringArray(ref NdrWriter writer, params string[] values)
    {
        writer.WriteUInt32((uint)values.Length);
        foreach (string value in values)
        {
            writer.WriteUnicodeStringPtr(value);
        }
    }

    private static void Ensure(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Unexpected round-trip payload.");
        }
    }

    private sealed class BatchServerStub : IOPCBatchServer
    {
        public Task<string> GetDelimiterAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult("/");

        public Task<IOpcInterfaceRef> CreateEnumeratorAsync(
            Guid riid,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IOpcInterfaceRef>(
                new OpcInterfaceRef(
                    riid,
                    flags: 0,
                    publicRefs: 1,
                    oxid: 2,
                    oid: 3,
                    ipid: Guid.Parse("A8080DA2-E23E-11D2-AFA7-00C04F539422"),
                    securityOffset: 0,
                    resolverBindings: []));
    }

    private sealed class BatchSummaryStub : IEnumOPCBatchSummary
    {
        private readonly OpcBatchSummary _summary;

        public BatchSummaryStub(OpcBatchSummary summary) => _summary = summary;

        public Task<OpcBatchSummary[]> NextAsync(int count, CancellationToken cancellationToken = default) =>
            Task.FromResult(count > 0 ? new[] { _summary } : Array.Empty<OpcBatchSummary>());

        public Task SkipAsync(int count, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ResetAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateObjRef(IEnumOPCBatchSummary.InterfaceId));

        public Task<int> CountAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}
