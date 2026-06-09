//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Batch.Dcom;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Batch.Tests.Dcom;

public sealed class IOPCBatchProxyTests {
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task BatchServer_GetDelimiter_invokes_channel_and_decodes_string() {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("/"));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) => {
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
    public async Task BatchServer_CreateEnumerator_round_trips_interface_ref() {
        Guid requestedRiid = IEnumOPCBatchSummary.InterfaceId;
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        Guid observedRiid = Guid.Empty;
        ReadOnlyMemory<byte> responsePayload = EncodeObjRef(requestedRiid);
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) => {
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
    public async Task BatchServer2_CreateFilteredEnumerator_round_trips_filter_model_and_interface_ref() {
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
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) => {
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
    public async Task BatchSummaryEnumerator_Clone_round_trips_interface_ref() {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        Guid expectedIid = IEnumOPCBatchSummary.InterfaceId;
        ReadOnlyMemory<byte> responsePayload = EncodeObjRef(expectedIid);
        var channel = new InMemoryCallChannel((iid, opnum, _, _) => {
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
    public async Task BatchSummaryEnumerator_Count_decodes_int32() {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(3));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) => {
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
    public async Task EnumerationSets_QueryEnumerationSets_round_trips_parallel_arrays() {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => {
            WriteInt32Array(ref writer, 0, 2, 6);
            WriteStringArray(ref writer, "OPCB_ENUM_PHYS", "OPCB_ENUM_STATE", "OPCB_ENUM_RE_USE");
        });
        var channel = new InMemoryCallChannel((iid, opnum, _, _) => {
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
    public async Task EnumerationSets_QueryEnumeration_round_trips_set_value_and_name() {
        int observedSet = -1;
        int observedValue = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("RUNNING"));
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) => {
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
    public async Task EnumerationSets_QueryEnumerationList_round_trips_parallel_arrays() {
        int observedSet = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => {
            WriteInt32Array(ref writer, 0, 1, 2);
            WriteStringArray(ref writer, "IDLE", "RUNNING", "COMPLETE");
        });
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) => {
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

    private static ReadOnlyMemory<byte> EncodeObjRef(Guid iid) => WritePayload((ref NdrWriter writer) => {
        writer.WriteUInt32(0x574F454Du);
        writer.WriteUInt32(0x00000001u);
        writer.WriteGuid(iid);
        writer.WriteUInt32(0);
        writer.WriteUInt32(5);
        writer.WriteUInt64(1);
        writer.WriteUInt64(2);
        writer.WriteGuid(Guid.NewGuid());
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
    });

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 2048) {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static void WriteInt32Array(ref NdrWriter writer, params int[] values) {
        writer.WriteUInt32((uint)values.Length);
        foreach (int value in values) {
            writer.WriteInt32(value);
        }
    }

    private static void WriteStringArray(ref NdrWriter writer, params string[] values) {
        writer.WriteUInt32((uint)values.Length);
        foreach (string value in values) {
            writer.WriteUnicodeStringPtr(value);
        }
    }

    private static void Ensure(bool condition) {
        if (!condition) {
            throw new InvalidOperationException("Unexpected round-trip payload.");
        }
    }
}
