//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Cpx;
using Opc.Classic.Cpx.Dcom;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Mcp.Tests;

public sealed class CpxToolsTests {
    [Test]
    public async Task Cpx_tools_get_complex_type_for_da_item_via_mcp_client() {
        var syntheticCpx = new SyntheticCpxDaServer();
        string channelName = "cpx-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryDaConnectionRegistry.Register(channelName, syntheticCpx.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object> {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcComplexTypeDto complexType = await server.CallToolAsync<OpcComplexTypeDto>(
            "opcclassic.cpx.get_complex_type",
            new Dictionary<string, object> {
                ["sessionId"] = session.SessionId,
                ["itemId"] = "Device.Motor",
            }).ConfigureAwait(false);

        await Assert.That(complexType.ItemId).IsEqualTo("Device.Motor");
        await Assert.That(complexType.TypeId).IsEqualTo(SyntheticCpxDaServer.TypeGuid);
        await Assert.That(complexType.DictionaryId).IsEqualTo("SampleDictionary");
        await Assert.That(complexType.AvailableFilters).Contains("Raw");
    }

    [Test]
    public async Task Cpx_tools_get_type_system_and_dictionary_via_mcp_client() {
        var syntheticCpx = new SyntheticCpxDaServer();
        string channelName = "cpx-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryDaConnectionRegistry.Register(channelName, syntheticCpx.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object> {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcTypeSystemDto typeSystem = await server.CallToolAsync<OpcTypeSystemDto>(
            "opcclassic.cpx.get_type_system",
            new Dictionary<string, object> {
                ["sessionId"] = session.SessionId,
                ["typeSystemId"] = "binary",
            }).ConfigureAwait(false);
        OpcTypeDictionaryDto dictionary = await server.CallToolAsync<OpcTypeDictionaryDto>(
            "opcclassic.cpx.get_dictionary",
            new Dictionary<string, object> {
                ["sessionId"] = session.SessionId,
                ["dictionaryId"] = "SampleDictionary",
            }).ConfigureAwait(false);

        await Assert.That(typeSystem.TypeSystemId).IsEqualTo(TypeDictionary.OpcBinaryTypeSystemId);
        await Assert.That(typeSystem.NamespacePath).IsEqualTo("/CPX/OPCBinary");
        await Assert.That(dictionary.TypeSystemId).IsEqualTo(TypeDictionary.OpcBinaryTypeSystemId);
        await Assert.That(dictionary.Types[0].TypeId).IsEqualTo("FunctionBlockHeader");
        await Assert.That(dictionary.ParseError).IsNull();
    }

    private sealed class SyntheticCpxDaServer : IOpcDaServer {
        public static readonly Guid TypeGuid = new("11111111-2222-3333-4444-555555555555");
        private const string DictionaryXml = """
            <?xml version="1.0" encoding="utf-8" ?>
            <TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/"
                            xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                            DefaultBigEndian="true">
              <TypeDescription TypeID="FunctionBlockHeader">
                <CharString Name="Block Tag" xsi:type="Ascii" Length="8" />
                <Integer Name="Execution Time" xsi:type="Int32" />
              </TypeDescription>
            </TypeDictionary>
            """;
        private readonly OpcDaServerDispatcher _serverDispatcher;

        public SyntheticCpxDaServer() {
            _serverDispatcher = new OpcDaServerDispatcher(this);
            Channel = new InMemoryCallChannel(DispatchAsync);
        }

        public InMemoryCallChannel Channel { get; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            return Task.FromResult(new OpcServerStatus {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = now,
                LastUpdateTime = now,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0),
                VendorInfo = "Synthetic MCP CPX Server",
                GroupCount = 0,
                BandWidth = 0,
            });
        }

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientGroupHandle, int timeBias, float percentDeadband, int localeId, Guid requestedInterfaceId, out int serverGroupHandle, out int revisedUpdateRate, out IOpcInterfaceRef group, CancellationToken cancellationToken = default) {
            throw new NotSupportedException();
        }

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult("Synthetic CPX error");

        private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) {
            cancellationToken.ThrowIfCancellationRequested();
            if (interfaceId == IOPCServer.InterfaceId) {
                return _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken);
            }

            if (interfaceId == IOPCComplexDataItem.InterfaceId) {
                return DispatchComplexDataItem(opnum);
            }

            if (interfaceId == IOPCComplexDataItem2.InterfaceId) {
                return DispatchComplexDataItem2(opnum);
            }

            if (interfaceId == IOPCTypeLibrary.InterfaceId) {
                return DispatchTypeLibrary(opnum);
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> DispatchComplexDataItem(int opnum) {
            if (opnum == IOPCComplexDataItem.Opnums.GetTypeItemIDAsync) {
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Types.Motor"));
            }

            if (opnum == IOPCComplexDataItem.Opnums.GetUnconvertedItemIDAsync) {
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Device.Motor.Raw"));
            }

            if (opnum == IOPCComplexDataItem.Opnums.GetDataFilterAsync) {
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Raw"));
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> DispatchComplexDataItem2(int opnum) {
            if (opnum == IOPCComplexDataItem2.Opnums.GetTypeIDAsync) {
                return Result((ref NdrWriter writer) => writer.WriteGuid(TypeGuid));
            }

            if (opnum == IOPCComplexDataItem2.Opnums.GetDictionaryIDAsync) {
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("SampleDictionary"));
            }

            if (opnum == IOPCComplexDataItem2.Opnums.GetAvailableFiltersAsync) {
                return Result((ref NdrWriter writer) => WriteStringArray(ref writer, "Raw", "Engineering"));
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> DispatchTypeLibrary(int opnum) {
            if (opnum == IOPCTypeLibrary.Opnums.GetDictionaryAsync) {
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(DictionaryXml));
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> Result(NdrWriteAction write) => Task.FromResult(new NdrCallResult(0, WritePayload(write)));
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 4096) {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static void WriteStringArray(ref NdrWriter writer, params string[] values) {
        writer.WriteUInt32((uint)values.Length);
        foreach (string value in values) {
            writer.WriteUnicodeStringPtr(value);
        }
    }
}
