//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dx.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dx.Tests;

public sealed class DxNdrCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task CodecRegistry_RegistersSixteenDxCodecs()
    {
        await Assert.That(NdrOpcDxCodecRegistry.RegisteredCodecNames.Count).IsEqualTo(16);
        await Assert.That(NdrOpcDxCodecRegistry.RegisteredCodecNames).Contains("OPCDX_CONNECTION");
        await Assert.That(NdrOpcDxCodecRegistry.RegisteredCodecNames).Contains("OPCDX_SOURCE_SERVER_STATUS");
    }

    [Test]
    public async Task ConnectionCodec_RoundTripsMasksAndVariants()
    {
        var connection = new DxConnection(
            name: "C1",
            description: "Mirror tank level",
            itemPath: "DX/DXConnectionsRoot/Area1",
            itemName: "C1",
            version: "v1",
            browsePaths: new[] { "Area1", "Area1/Tank1" },
            keyword: "level",
            defaultSourceItemConnected: true,
            defaultTargetItemConnected: true,
            defaultOverridden: false,
            defaultOverrideValue: OpcVariant.FromInt32(7),
            substituteValue: OpcVariant.FromString("sub"),
            enableSubstituteValue: true,
            targetItemPath: "HMI",
            targetItemName: "Tank1.Level",
            sourceServerName: "PLC1",
            sourceItemPath: "PLC",
            sourceItemName: "Level",
            sourceItemQueueSize: 10,
            updateRateMilliseconds: 250,
            deadbandPercent: 1.5f,
            vendorData: "vendor");

        DxConnection decoded = RoundTrip(connection, NdrOpcDxConnectionCodec.Write, NdrOpcDxConnectionCodec.Read);

        await Assert.That(decoded.Name).IsEqualTo("C1");
        await Assert.That(decoded.BrowsePaths.Length).IsEqualTo(2);
        await Assert.That(decoded.DefaultOverrideValue?.AsInt32()).IsEqualTo(7);
        await Assert.That(decoded.SubstituteValue?.AsString()).IsEqualTo("sub");
        await Assert.That((decoded.Mask & (int)DxMask.SourceItemQueueSize) != 0).IsTrue();
    }

    [Test]
    public async Task SourceServerCodec_RoundTripsMaskFields()
    {
        var source = new DxSourceServer(
            name: "PLC1",
            serverUrl: "opcda://plc1/Vendor.OPC.1",
            description: "Main PLC",
            serverType: "COM-DA2.05",
            itemPath: "DX/SourceServers",
            itemName: "PLC1",
            version: "v1",
            defaultConnected: true);

        DxSourceServer decoded = RoundTrip(source, NdrOpcDxSourceServerCodec.Write, NdrOpcDxSourceServerCodec.Read);

        await Assert.That(decoded.Name).IsEqualTo("PLC1");
        await Assert.That(decoded.ServerUrl).IsEqualTo("opcda://plc1/Vendor.OPC.1");
        await Assert.That(decoded.DefaultConnected).IsEqualTo(true);
        await Assert.That((decoded.Mask & (int)DxMask.ServerUrl) != 0).IsTrue();
    }

    [Test]
    public async Task GeneralResponseCodec_RoundTripsIdentifiedResults()
    {
        var response = new DxGeneralResponse(
            "cfg-2",
            new[]
            {
                new DxIdentifiedResult("DX/SourceServers", "PLC1", "v1", OpcDxError.E_VERSION_MISMATCH),
            });

        DxGeneralResponse decoded = RoundTrip(response, NdrOpcDxGeneralResponseCodec.Write, NdrOpcDxGeneralResponseCodec.Read);

        await Assert.That(decoded.ConfigurationVersion).IsEqualTo("cfg-2");
        await Assert.That(decoded.IdentifiedResults.Length).IsEqualTo(1);
        await Assert.That(decoded.IdentifiedResults[0].ResultId.Code).IsEqualTo(OpcDxError.E_VERSION_MISMATCH.Code);
    }

    [Test]
    public async Task ServerStatusCodec_RoundTripsStatusRecord()
    {
        var status = new DxServerStatus(
            DxServerState.Running,
            "cfg-3",
            2,
            100,
            true,
            OpcDxError.S_OK,
            null,
            new[] { "COM-DA2.05", "XML-DA1.0" },
            32);

        DxServerStatus decoded = RoundTrip(status, NdrOpcDxServerStatusCodec.Write, NdrOpcDxServerStatusCodec.Read);

        await Assert.That(decoded.ServerState).IsEqualTo(DxServerState.Running);
        await Assert.That(decoded.SourceServerTypes.Length).IsEqualTo(2);
        await Assert.That(decoded.MaxQueueSize).IsEqualTo(32u);
    }

    [Test]
    public async Task ConnectionStatusCodec_RoundTripsStatusRecord()
    {
        var timestamp = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var status = new DxConnectionStatus(
            DxConnectionState.Operational,
            OpcVariant.FromSingle(12.5f),
            timestamp,
            new DxQuality(DxQualityStatus.Good, DxLimitStatus.None, 0),
            OpcDxError.S_OK,
            null,
            OpcVariant.FromSingle(12.5f),
            timestamp,
            new DxQuality(DxQualityStatus.Good, DxLimitStatus.None, 0),
            OpcDxError.S_OK,
            null,
            250,
            4,
            1,
            true,
            true,
            false,
            OpcVariant.Empty);

        DxConnectionStatus decoded = RoundTrip(status, NdrOpcDxConnectionStatusCodec.Write, NdrOpcDxConnectionStatusCodec.Read);

        await Assert.That(decoded.DxConnectionState).IsEqualTo(DxConnectionState.Operational);
        await Assert.That(decoded.WriteValue.AsSingle()).IsEqualTo(12.5f);
        await Assert.That(decoded.ActualUpdateRate).IsEqualTo(250u);
        await Assert.That(decoded.SourceItemConnected).IsTrue();
    }

    [Test]
    public async Task SourceServerStatusCodec_RoundTripsStatusRecord()
    {
        var timestamp = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var status = new DxSourceServerStatus(
            DxConnectStatus.Connected,
            OpcDxError.S_OK,
            null,
            timestamp,
            timestamp,
            0,
            42,
            timestamp,
            true);

        DxSourceServerStatus decoded = RoundTrip(status, NdrOpcDxSourceServerStatusCodec.Write, NdrOpcDxSourceServerStatusCodec.Read);

        await Assert.That(decoded.ConnectStatus).IsEqualTo(DxConnectStatus.Connected);
        await Assert.That(decoded.PingTime).IsEqualTo(42u);
        await Assert.That(decoded.SourceServerConnected).IsTrue();
    }

    [Test]
    public async Task NamespaceHelpers_BuildCanonicalDxPaths()
    {
        await Assert.That(DxNamespace.ServerStatusPath).IsEqualTo("/DX/ServerStatus");
        await Assert.That(DxNamespace.ConnectionPath("Area1", "Tank1")).IsEqualTo("/DX/DXConnectionsRoot/Area1/Tank1");
        await Assert.That(DxNamespace.ConnectionStatusPath("/DX/Area1/Tank1/")).IsEqualTo("/DX/DXConnectionsRoot/Area1/Tank1/Status");
        await Assert.That(DxNamespace.SourceServerStatusPath("PLC1")).IsEqualTo("/DX/SourceServers/PLC1/Status");
    }

    [Test]
    public async Task OpcDxError_TableContainsSpecHresults()
    {
        await Assert.That(OpcDxError.All.Count).IsGreaterThanOrEqualTo(55);
        await Assert.That(OpcDxError.E_VERSION_MISMATCH.Code).IsEqualTo(unchecked((int)0xC0040703u));
        await Assert.That(OpcDxError.E_CONNECTIONS_EXIST.Code).IsEqualTo(unchecked((int)0xC004070Eu));
        await Assert.That(OpcDxError.S_CLAMP.Code).IsEqualTo(0x00040782);
    }

    private static T RoundTrip<T>(T value, NdrWriteFunc<T> write, NdrReadFunc<T> read)
    {
        var payload = WritePayload((ref NdrWriter writer) => write(ref writer, value));
        var reader = new NdrReader(payload.Span);
        return read(ref reader);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 8192)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private delegate void NdrWriteFunc<in T>(ref NdrWriter writer, T value);
    private delegate T NdrReadFunc<out T>(ref NdrReader reader);
}
