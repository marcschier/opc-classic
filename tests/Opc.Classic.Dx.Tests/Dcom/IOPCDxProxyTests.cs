//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dx;
using Opc.Classic.Dx.Dcom;
using Opc.Classic.Dx.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

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
            observedConnections = NdrOpcDxConnectionArrayCodec.Read(ref reader);
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
            (ref NdrWriter writer) => NdrOpcDxSourceServerArrayCodec.Write(ref writer, new[] { new DxSourceServer("PLC1", "opcda://plc1/Vendor.OPC.1") }),
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
        {
            NdrOpcDxInt32ArrayCodec.Write(ref writer, new[] { OpcDxError.E_VERSION_MISMATCH.Code });
            NdrOpcDxGeneralResponseCodec.Write(ref writer, new DxGeneralResponse("cfg-5"));
        }, capacity: 4096);
        var channel = new InMemoryCallChannel((_, _, payload, _) =>
        {
            var reader = new NdrReader(payload.Span);
            observedBrowsePath = reader.ReadUnicodeStringPtr();
            _ = NdrOpcDxConnectionArrayCodec.Read(ref reader);
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

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 512)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }
}
