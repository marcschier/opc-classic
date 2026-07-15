// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dx.Dcom;
using Opc.Classic.Dx.Ndr;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Dx.Tests.Dcom;

public sealed class IOPCDxProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task DXServer_GetVersion_invokes_channel_and_decodes_string()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("1.0"));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCDXServerClientProxy(channel);
        string version = await proxy.GetVersionAsync(CancellationToken.None);

        int expectedOpnum = IOPCDXServer.Opnums.GetVersionAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCDXServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(version).IsEqualTo("1.0");
    }

    [Test]
    public async Task Configuration_ResetConfiguration_encodes_version_and_decodes_new_version()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        int observedPayloadLength = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("v2"));
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCConfigurationClientProxy(channel);
        string newVersion = await proxy.ResetConfigurationAsync("v1", CancellationToken.None);

        int expectedOpnum = IOPCConfiguration.Opnums.ResetConfigurationAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCConfiguration.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
        await Assert.That(newVersion).IsEqualTo("v2");
    }

    [Test]
    public async Task Generated_Configuration_proxy_and_dispatcher_match_ResetConfiguration_known_answers()
    {
        var impl = new ConfigurationStub();
        var dispatcher = new IOPCConfigurationServerDispatcher(impl);
        byte[] request =
        [
            0x03, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x03, 0x00, 0x00, 0x00,
            0x76, 0x00, 0x31, 0x00, 0x00, 0x00,
        ];
        var channel = new InMemoryCallChannel(async (_, opnum, payload, cancellationToken) =>
        {
            await Assert.That(payload.ToArray()).IsEquivalentTo(request);
            DispatchResult dispatched = await dispatcher.DispatchAsync(opnum, payload, cancellationToken);
            return dispatched.ToNdrCallResult();
        });

        string version = await new IOPCConfigurationClientProxy(channel)
            .ResetConfigurationAsync("v1", CancellationToken.None);

        await Assert.That(impl.ConfigurationVersion).IsEqualTo("v1");
        await Assert.That(version).IsEqualTo("v2");
    }

    [Test]
    public async Task Configuration_AddDXConnections_encodes_connection_array_and_decodes_response()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        DxConnection[] observedConnections = Array.Empty<DxConnection>();
        ReadOnlyMemory<byte> responsePayload = WritePayload(
            (ref NdrWriter writer) => NdrOpcDxGeneralResponseCodec.Write(ref writer, new DxGeneralResponse("cfg-4")),
            capacity: 2048);
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            int explicitCount = reader.ReadInt32();
            observedConnections = NdrOpcDxConnectionArrayCodec.Read(ref reader);
            Ensure(explicitCount == observedConnections.Length);
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCConfigurationClientProxy(channel);
        DxGeneralResponse response = await proxy.AddDXConnectionsAsync(new[] { new DxConnection(name: "C1", sourceServerName: "PLC1") }, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCConfiguration.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCConfiguration.Opnums.AddDXConnectionsAsync);
        await Assert.That(observedConnections[0].Name).IsEqualTo("C1");
        await Assert.That(response.ConfigurationVersion).IsEqualTo("cfg-4");
    }

    [Test]
    public async Task Configuration_QuerySourceServers_decodes_source_server_array()
    {
        int observedPayloadLength = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload(
            (ref NdrWriter writer) =>
            {
                var servers = new[] { new DxSourceServer("PLC1", "opcda://plc1/Vendor.OPC.1") };
                writer.WriteUInt32((uint)servers.Length);
                NdrOpcDxSourceServerArrayCodec.WriteUnique(ref writer, servers);
            },
            capacity: 2048);
        var channel = new InMemoryCallChannel((_, _, payload, _) =>
        {
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCConfigurationClientProxy(channel);
        DxSourceServer[] servers = await proxy.QuerySourceServersAsync(CancellationToken.None);

        await Assert.That(observedPayloadLength).IsEqualTo(0);
        await Assert.That(servers.Length).IsEqualTo(1);
        await Assert.That(servers[0].Name).IsEqualTo("PLC1");
    }

    [Test]
    public async Task Configuration_UpdateDXConnections_decodes_errors_and_general_response()
    {
        string? observedBrowsePath = null;
        bool observedRecursive = false;
        DxConnection? observedDefinition = null;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
            NdrOpcDxUpdateConnectionsResultCodec.Write(
                ref writer,
                new DxUpdateConnectionsResult(
                    [OpcDxError.E_VERSION_MISMATCH.Code],
                    new DxGeneralResponse("cfg-5"))),
            capacity: 4096);
        var channel = new InMemoryCallChannel((_, _, payload, _) =>
        {
            var reader = new NdrReader(payload.Span);
            observedBrowsePath = reader.ReadUnicodeString();
            int explicitCount = reader.ReadInt32();
            _ = NdrOpcDxConnectionArrayCodec.Read(ref reader);
            Ensure(explicitCount == 1);
            observedRecursive = reader.ReadInt32() != 0;
            observedDefinition = NdrOpcDxConnectionCodec.Read(ref reader);
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCConfigurationClientProxy(channel);
        DxUpdateConnectionsResult result = await proxy.UpdateDXConnectionsAsync(
            "Area1",
            new[] { new DxConnection(name: "C1") },
            recursive: true,
            new DxConnection(name: "C1", updateRateMilliseconds: 1000),
            CancellationToken.None);

        await Assert.That(observedBrowsePath).IsEqualTo("Area1");
        await Assert.That(observedRecursive).IsTrue();
        await Assert.That(observedDefinition?.UpdateRateMilliseconds).IsEqualTo(1000);
        await Assert.That(result.Errors[0]).IsEqualTo(OpcDxError.E_VERSION_MISMATCH.Code);
        await Assert.That(result.Response.ConfigurationVersion).IsEqualTo("cfg-5");
    }

    [Test]
    public async Task Configuration_DeleteDXConnections_encodes_masks_and_decodes_response()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        string? observedBrowsePath = null;
        bool observedRecursive = false;
        DxConnection[] observedMasks = Array.Empty<DxConnection>();
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
            NdrOpcDxDeleteConnectionsResultCodec.Write(
                ref writer,
                new DxDeleteConnectionsResult(
                    [0, OpcDxError.E_INVALID_BROWSE_PATH.Code],
                    new DxGeneralResponse(
                        "cfg-delete-1",
                        [
                            new DxIdentifiedResult("Area1", "C1", "v7", OpcDxError.S_OK),
                        ]))),
            capacity: 4096);
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            observedBrowsePath = reader.ReadUnicodeString();
            int explicitCount = reader.ReadInt32();
            observedMasks = NdrOpcDxConnectionArrayCodec.Read(ref reader);
            Ensure(explicitCount == observedMasks.Length);
            observedRecursive = reader.ReadInt32() != 0;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCConfigurationClientProxy(channel);
        DxDeleteConnectionsResult result = await proxy.DeleteDXConnectionsAsync(
            "Area1",
            new[]
            {
                new DxConnection(name: "C1", mask: (int)DxMask.Name),
                new DxConnection(sourceServerName: "PLC1", sourceItemName: "Level"),
            },
            recursive: true,
            CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCConfiguration.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCConfiguration.Opnums.DeleteDXConnectionsAsync);
        await Assert.That(observedBrowsePath).IsEqualTo("Area1");
        await Assert.That(observedRecursive).IsTrue();
        await Assert.That(observedMasks.Length).IsEqualTo(2);
        await Assert.That(observedMasks[0].Name).IsEqualTo("C1");
        await Assert.That(observedMasks[1].SourceServerName).IsEqualTo("PLC1");
        await Assert.That(result.MaskErrors[0]).IsEqualTo(0);
        await Assert.That(result.MaskErrors[1]).IsEqualTo(OpcDxError.E_INVALID_BROWSE_PATH.Code);
        await Assert.That(result.ConfigurationVersion).IsEqualTo("cfg-delete-1");
        await Assert.That(result.IdentifiedResults[0].ItemName).IsEqualTo("C1");
    }

    [Test]
    public async Task Configuration_all_IDL_methods_use_native_counts_pointers_and_simple_ref_strings()
    {
        var requests = new Dictionary<int, byte[]>();
        ReadOnlyMemory<byte> generalResponse = new byte[16];
        ReadOnlyMemory<byte> queryResponse = new byte[12];
        ReadOnlyMemory<byte> updateResponse = new byte[20];
        ReadOnlyMemory<byte> resetResponse = WritePayload(
            (ref NdrWriter writer) => writer.WriteUnicodeStringPtr("v2"));
        var channel = new InMemoryCallChannel((_, opnum, payload, _) =>
        {
            requests[opnum] = payload.ToArray();
            ReadOnlyMemory<byte> response = opnum switch
            {
                3 => new byte[8],
                8 => queryResponse,
                10 or 12 or 13 => updateResponse,
                14 => resetResponse,
                _ => generalResponse,
            };
            return Task.FromResult(new NdrCallResult(0, response));
        });
        var proxy = new IOPCConfigurationClientProxy(channel);

        _ = await proxy.QuerySourceServersAsync();
        _ = await proxy.AddSourceServersAsync([]);
        _ = await proxy.ModifySourceServersAsync([]);
        _ = await proxy.DeleteSourceServersAsync([]);
        _ = await proxy.CopyDefaultServerAttributesAsync(true, []);
        _ = await proxy.QueryDXConnectionsAsync("A", [], false);
        _ = await proxy.AddDXConnectionsAsync([]);
        _ = await proxy.UpdateDXConnectionsAsync("A", [], false, new DxConnection());
        _ = await proxy.ModifyDXConnectionsAsync([]);
        _ = await proxy.DeleteDXConnectionsAsync("A", [], false);
        _ = await proxy.CopyDefaultDXConnectionAttributesAsync(true, "A", [], false);
        _ = await proxy.ResetConfigurationAsync("v1");

        byte[] countedEmpty = [0, 0, 0, 0, 0, 0, 0, 0];
        byte[] refStringAndEmptyMasks =
        [
            0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x41, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
        ];
        byte[] resetRequest =
        [
            0x03, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x03, 0x00, 0x00, 0x00,
            0x76, 0x00, 0x31, 0x00, 0x00, 0x00,
        ];

        await Assert.That(requests[3]).IsEmpty();
        await Assert.That(requests[4]).IsEquivalentTo(countedEmpty);
        await Assert.That(requests[5]).IsEquivalentTo(countedEmpty);
        await Assert.That(requests[6]).IsEquivalentTo(countedEmpty);
        await Assert.That(requests[7]).IsEquivalentTo(
            new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0 });
        await Assert.That(requests[8]).IsEquivalentTo(refStringAndEmptyMasks);
        await Assert.That(requests[9]).IsEquivalentTo(countedEmpty);
        await Assert.That(requests[11]).IsEquivalentTo(countedEmpty);
        await Assert.That(requests[12]).IsEquivalentTo(refStringAndEmptyMasks);
        await Assert.That(requests[13]).IsEquivalentTo(
            new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }.Concat(refStringAndEmptyMasks).ToArray());
        await Assert.That(requests[14]).IsEquivalentTo(resetRequest);

        byte[] updateRequest = requests[10];
        await Assert.That(updateRequest[..refStringAndEmptyMasks.Length])
            .IsEquivalentTo(refStringAndEmptyMasks);
        var definitionReader = new NdrReader(updateRequest);
        _ = definitionReader.ReadUnicodeString();
        _ = definitionReader.ReadUInt32();
        _ = NdrOpcDxConnectionArrayCodec.Read(ref definitionReader);
        _ = definitionReader.ReadInt32();
        _ = NdrOpcDxConnectionCodec.Read(ref definitionReader);
        await Assert.That(definitionReader.RemainingBytes).IsEqualTo(0);
    }

    [Test]
    public async Task Configuration_AddDXConnections_matches_native_MIDL_variant_and_later_pointer_order()
    {
        byte[]? request = null;
        var channel = new InMemoryCallChannel((_, _, payload, _) =>
        {
            request = payload.ToArray();
            return Task.FromResult(new NdrCallResult(0, new byte[16]));
        });
        var connection = new DxConnection(
            defaultOverrideValue: OpcVariant.FromString("B"),
            substituteValue: OpcVariant.FromInt32(0x11223344),
            enableSubstituteValue: true,
            sourceServerName: "SRC",
            sourceItemQueueSize: 0x55667788,
            updateRateMilliseconds: 0x12345678,
            deadbandPercent: 12.5f,
            vendorData: "VENDOR");

        _ = await new IOPCConfigurationClientProxy(channel)
            .AddDXConnectionsAsync([connection], CancellationToken.None);

        // Captured from the vendored x86 MIDL type format at OpcDx_p.c offset
        // 1578 via NdrMesTypeEncode, stripping the 16-byte MES pickle header
        // and its terminal type-pickle alignment pad (a method request has no
        // following field requiring that pad). The native BSTR referent is
        // process-specific, so it is normalized to this writer's next non-zero
        // referent (0x00020008). The request adds the standalone dwCount and
        // conformant max_count before the native FC_BOGUS_ARRAY element.
        const string expected =
            "0100000001000000009C3C000000000000000000000000000000000000000000" +
            "0000000000000000000000000000000000000000000000005573657255736572" +
            "FFFFFFFF00000000000000000000020000000000000000008877665578563412" +
            "0000484104000200050000000000000008000000000000000800000008000200" +
            "0100000002000000010000004200000003000000000000000300000000000000" +
            "0300000044332211040000000000000004000000530052004300000007000000" +
            "0000000007000000560045004E0044004F0052000000";

        await Assert.That(request).IsNotNull();
        await Assert.That(Convert.ToHexString(request!)).IsEqualTo(expected);
        await Assert.That(request![56..64]).IsEquivalentTo(
            new byte[] { 0x55, 0x73, 0x65, 0x72, 0x55, 0x73, 0x65, 0x72 });
        await Assert.That(BitConverter.ToUInt32(request, 76)).IsEqualTo(0x00020000u);
        await Assert.That(BitConverter.ToUInt32(request, 100)).IsEqualTo(0x00020004u);
        await Assert.That(BitConverter.ToUInt16(request, 112)).IsEqualTo((ushort)VarType.VT_BSTR);
        await Assert.That(BitConverter.ToUInt32(request, 124)).IsEqualTo(0x00020008u);
        await Assert.That(BitConverter.ToUInt16(request, 152)).IsEqualTo((ushort)VarType.VT_I4);
        await Assert.That(request[168..176]).IsEquivalentTo(
            new byte[] { 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        await Assert.That(request[188..196]).IsEquivalentTo(
            new byte[] { 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        uint nativeExplicitCount;
        DxConnection[] nativeConnections;
        {
            var nativeReader = new NdrReader(Convert.FromHexString(expected));
            nativeExplicitCount = nativeReader.ReadUInt32();
            nativeConnections = NdrOpcDxConnectionArrayCodec.Read(ref nativeReader);
        }
        await Assert.That(nativeExplicitCount).IsEqualTo(1u);
        await Assert.That(nativeConnections[0].DefaultOverrideValue?.AsString()).IsEqualTo("B");
        await Assert.That(nativeConnections[0].SubstituteValue?.AsInt32()).IsEqualTo(0x11223344);
        await Assert.That(nativeConnections[0].SourceServerName).IsEqualTo("SRC");
        await Assert.That(nativeConnections[0].VendorData).IsEqualTo("VENDOR");
    }

    [Test]
    public async Task Configuration_source_server_native_fixtures_defer_embedded_string_pointers()
    {
        byte[]? request = null;
        var dispatcher = new IOPCConfigurationServerDispatcher(new ConfigurationStub());
        var channel = new InMemoryCallChannel(async (_, opnum, payload, cancellationToken) =>
        {
            request = payload.ToArray();
            DispatchResult result = await dispatcher.DispatchAsync(opnum, payload, cancellationToken);
            return result.ToNdrCallResult();
        });
        var proxy = new IOPCConfigurationClientProxy(channel);
        var source = new DxSourceServer(name: "S");

        _ = await proxy.AddSourceServersAsync([source]);

        byte[] expectedRequest =
        [
            0x01, 0x00, 0x00, 0x00, // standalone dwCount
            0x01, 0x00, 0x00, 0x00, // conformant max_count
            0x10, 0x00, 0x00, 0x00, // dwMask = Name
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x02, 0x00, // szName referent
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x53, 0x00, 0x00, 0x00,
        ];
        await Assert.That(request).IsEquivalentTo(expectedRequest);

        DispatchResult getServers = await new IOPCConfigurationServerDispatcher(
            new ConfigurationStub { SourceServers = [source] })
            .DispatchAsync(
                IOPCConfiguration.Opnums.QuerySourceServersAsync,
                ReadOnlyMemory<byte>.Empty,
                CancellationToken.None);
        byte[] expectedResponse =
        [
            0x01, 0x00, 0x00, 0x00, // pdwCount
            0x00, 0x00, 0x02, 0x00, // ppServers outer referent
            0x01, 0x00, 0x00, 0x00, // conformant max_count
            0x10, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x04, 0x00, 0x02, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x53, 0x00, 0x00, 0x00,
        ];
        await Assert.That(getServers.Payload.ToArray()).IsEquivalentTo(expectedResponse);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 512)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static void Ensure(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Unexpected OPC DX wire payload.");
        }
    }

    private sealed class ConfigurationStub : IOPCConfiguration
    {
        public string? ConfigurationVersion { get; private set; }
        public DxSourceServer[] SourceServers { get; init; } = [];

        public Task<DxSourceServer[]> QuerySourceServersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(SourceServers);

        public Task<DxGeneralResponse> AddSourceServersAsync(DxSourceServer[] sourceServers, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxGeneralResponse("v2"));

        public Task<DxGeneralResponse> ModifySourceServersAsync(DxSourceServer[] sourceServers, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxGeneralResponse("v2"));

        public Task<DxGeneralResponse> DeleteSourceServersAsync(DxItemIdentifier[] sourceServers, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxGeneralResponse("v2"));

        public Task<DxGeneralResponse> CopyDefaultServerAttributesAsync(bool configToStatus, DxItemIdentifier[] sourceServers, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxGeneralResponse("v2"));

        public Task<DxConnectionQueryResult> QueryDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxConnectionQueryResult([], []));

        public Task<string[]> QueryDXConnectionNamesAsync(string browsePath, string[] connectionMasks, bool recursive, CancellationToken cancellationToken = default) =>
            Task.FromResult<string[]>([]);

        public Task<DxGeneralResponse> AddDXConnectionsAsync(DxConnection[] connections, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxGeneralResponse("v2"));

        public Task<DxUpdateConnectionsResult> UpdateDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive, DxConnection connectionDefinition, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxUpdateConnectionsResult([], new DxGeneralResponse("v2")));

        public Task<DxGeneralResponse> ModifyDXConnectionsAsync(DxConnection[] connections, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxGeneralResponse("v2"));

        public Task<DxDeleteConnectionsResult> DeleteDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxDeleteConnectionsResult([], new DxGeneralResponse("v2")));

        public Task<DxUpdateConnectionsResult> CopyDefaultDXConnectionAttributesAsync(bool configToStatus, string browsePath, DxConnection[] connectionMasks, bool recursive, CancellationToken cancellationToken = default) =>
            Task.FromResult(new DxUpdateConnectionsResult([], new DxGeneralResponse("v2")));

        public Task<string> ResetConfigurationAsync(string configurationVersion, CancellationToken cancellationToken = default)
        {
            ConfigurationVersion = configurationVersion;
            return Task.FromResult("v2");
        }
    }
}
