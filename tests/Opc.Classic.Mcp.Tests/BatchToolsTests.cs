//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Batch;
using Opc.Classic.Batch.Dcom;
using Opc.Classic.Batch.Ndr;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Mcp.Tests;

public sealed class BatchToolsTests
{
    [Test]
    public async Task Batch_tools_query_summaries_status_and_disconnect_via_mcp_client()
    {
        var syntheticBatch = new SyntheticBatchServer();
        string channelName = "batch-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryBatchConnectionRegistry.Register(channelName, syntheticBatch.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcResultDto connected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);
        OpcResultDto status = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.get_status",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcBatchSummaryDto[] summaries = await server.CallToolAsync<OpcBatchSummaryDto[]>(
            "opcclassic.batch.query_batch_summaries",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["id"] = "B-2026",
                ["executionState"] = "RUNNING",
                ["maxResults"] = 10,
            }).ConfigureAwait(false);
        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Message).Contains("delimiter='/'");
        await Assert.That(summaries.Length).IsEqualTo(2);
        await Assert.That(summaries[0].Id).IsEqualTo("B-2026-001");
        await Assert.That(syntheticBatch.ObservedFilter?.Id).IsEqualTo("B-2026");
        await Assert.That(syntheticBatch.ObservedFilter?.ExecutionState).IsEqualTo("RUNNING");
        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    [Test]
    public async Task Batch_tools_query_enumeration_sets_values_and_lists_via_mcp_client()
    {
        var syntheticBatch = new SyntheticBatchServer();
        string channelName = "batch-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryBatchConnectionRegistry.Register(channelName, syntheticBatch.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcBatchEnumerationSetDto[] sets = await server.CallToolAsync<OpcBatchEnumerationSetDto[]>(
            "opcclassic.batch.query_enumeration_sets",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcBatchEnumerationDto state = await server.CallToolAsync<OpcBatchEnumerationDto>(
            "opcclassic.batch.query_enumeration",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["enumerationSetId"] = 2,
                ["enumerationValue"] = 1,
            }).ConfigureAwait(false);
        OpcBatchEnumerationDto[] states = await server.CallToolAsync<OpcBatchEnumerationDto[]>(
            "opcclassic.batch.query_enumeration_list",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["enumerationSetId"] = 2,
            }).ConfigureAwait(false);

        await Assert.That(sets.Select(static set => set.Name)).Contains("OPCB_ENUM_STATE");
        await Assert.That(state.Name).IsEqualTo("RUNNING");
        await Assert.That(states.Select(static value => value.Name)).Contains("COMPLETE");
    }

    private sealed class SyntheticBatchServer
    {
        private readonly OpcBatchSummary[] _summaries =
        [
            new("B-2026-001", "First batch", "Batch.B-2026-001", "MR-1", 10.5f, "kg", "RUNNING", "AUTOMATIC", new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 1, 1, 0, 0, TimeSpan.Zero)),
            new("B-2026-002", "Second batch", "Batch.B-2026-002", "MR-2", 12.0f, "kg", "COMPLETE", "MANUAL", new DateTimeOffset(2026, 1, 2, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2026, 1, 2, 1, 0, 0, TimeSpan.Zero)),
        ];
        private int _position;

        public SyntheticBatchServer() => Channel = new InMemoryCallChannel(DispatchAsync);

        public InMemoryCallChannel Channel { get; }
        public OpcBatchSummaryFilter? ObservedFilter { get; private set; }

        private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (interfaceId == IOPCBatchServer.InterfaceId)
            {
                if (opnum == IOPCBatchServer.Opnums.GetDelimiterAsync)
                {
                    return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("/"));
                }

                if (opnum == IOPCBatchServer.Opnums.CreateEnumeratorAsync)
                {
                    _position = 0;
                    return Task.FromResult(new NdrCallResult(0, EncodeObjRef(IEnumOPCBatchSummary.InterfaceId)));
                }
            }

            if (interfaceId == IOPCBatchServer2.InterfaceId && opnum == IOPCBatchServer2.Opnums.CreateFilteredEnumeratorAsync)
            {
                var reader = new NdrReader(requestPayload.Span);
                _ = reader.ReadGuid();
                ObservedFilter = NdrOpcBatchSummaryFilterCodec.Read(ref reader);
                _ = reader.ReadUnicodeStringPtr();
                _position = 0;
                return Task.FromResult(new NdrCallResult(0, EncodeObjRef(IEnumOPCBatchSummary.InterfaceId)));
            }

            if (interfaceId == IEnumOPCBatchSummary.InterfaceId && opnum == IEnumOPCBatchSummary.Opnums.NextAsync)
            {
                var reader = new NdrReader(requestPayload.Span);
                int count = reader.ReadInt32();
                OpcBatchSummary[] page = _summaries.Skip(_position).Take(count).ToArray();
                _position += page.Length;
                return Result((ref NdrWriter writer) =>
                {
                    writer.WriteUInt32((uint)page.Length);
                    foreach (OpcBatchSummary summary in page)
                    {
                        NdrOpcBatchSummaryCodec.Write(ref writer, summary);
                    }
                });
            }

            if (interfaceId == IOPCEnumerationSets.InterfaceId)
            {
                return DispatchEnumerationSets(opnum, requestPayload);
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> DispatchEnumerationSets(int opnum, ReadOnlyMemory<byte> requestPayload)
        {
            if (opnum == IOPCEnumerationSets.Opnums.QueryEnumerationSetsAsync)
            {
                return Result((ref NdrWriter writer) =>
                {
                    WriteInt32Array(ref writer, 0, 2);
                    WriteStringArray(ref writer, "OPCB_ENUM_PHYS", "OPCB_ENUM_STATE");
                });
            }

            if (opnum == IOPCEnumerationSets.Opnums.QueryEnumerationAsync)
            {
                var reader = new NdrReader(requestPayload.Span);
                int setId = reader.ReadInt32();
                int value = reader.ReadInt32();
                string name = setId == 2 && value == 1 ? "RUNNING" : "UNKNOWN";
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(name));
            }

            if (opnum == IOPCEnumerationSets.Opnums.QueryEnumerationListAsync)
            {
                return Result((ref NdrWriter writer) =>
                {
                    WriteInt32Array(ref writer, 0, 1, 2);
                    WriteStringArray(ref writer, "IDLE", "RUNNING", "COMPLETE");
                });
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> Result(NdrWriteAction write) => Task.FromResult(new NdrCallResult(0, WritePayload(write)));
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static ReadOnlyMemory<byte> EncodeObjRef(Guid iid) => WritePayload((ref NdrWriter writer) =>
    {
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

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 4096)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
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
}
