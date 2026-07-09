// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Remoting;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Discovery.Dcom;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;
using Opc.Classic.Transport;
using DaConnectionPointProxy = Opc.Classic.Da.Dcom.IConnectionPointClientProxy;
using DaConnectionPoint = Opc.Classic.Da.Dcom.IConnectionPoint;
using AeConnectionPoint = Opc.Classic.Ae.Dcom.IConnectionPoint;
using DiscoveryEnumGuidProxy = Opc.Classic.Discovery.Dcom.IOPCEnumGUIDClientProxy;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class ManagedDcomFullStackE2ETests
{
    private const string User = "phase9-user";
    private const string Password = "phase9-password";
    private const string Domain = "PHASE9";

    [Test]
    [Category("Phase9.ManagedDcomFullStack")]
    public async Task Authenticated_native_style_sequence_reaches_da_callbacks_ae_events_and_hda_history()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(30));
        CancellationToken cancellationToken = timeout.Token;

        var model = new SimulatedPlantModel();
        await using CallbackFixture callbacks = await CallbackFixture.StartAsync(cancellationToken).ConfigureAwait(false);
        var credentials = new NetworkCredential(User, Password, Domain);
        OpcConnectData callbackConnectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse($"opcda://127.0.0.1:{callbacks.Endpoint.Port}/Phase9.Callbacks"),
            credentials,
            OpcProtectionLevel.Integrity);

        await using SimulationActivationHost host = SimulationActivationHost.Create(
            model,
            daClsid: new Guid("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001"),
            listenAddress: "127.0.0.1:0",
            NullLoggerFactory.Instance,
            endpointMapperListenAddress: "127.0.0.1:0",
            authenticationSource: new ConfiguredAuthenticationSource(User, Password, Domain),
            dataCallbackSinkFactory: DcomOpcDataCallbackSinkFactory.CreateTcpOnly(callbackConnectData, "127.0.0.1"),
            eventSinkFactory: sink => DcomOpcEventSinkSender.CreateTcpOnly(sink, callbackConnectData, "127.0.0.1"));
        await host.StartAsync(cancellationToken).ConfigureAwait(false);

        IPEndPoint activationEndpoint = host.Endpoint ?? throw new InvalidOperationException("Activation endpoint did not bind.");
        IPEndPoint epmEndpoint = host.EndpointMapperEndpoint ?? throw new InvalidOperationException("Endpoint mapper did not bind.");

        IPEndPoint mappedEndpoint = await ResolveActivationEndpointViaEpmAsync(epmEndpoint, activationEndpoint, cancellationToken).ConfigureAwait(false);
        await Assert.That(mappedEndpoint.Port).IsEqualTo(activationEndpoint.Port);

        await using DcomCallChannel activationChannel = await ConnectAuthenticatedAsync(
            activationEndpoint,
            cancellationToken,
            RemoteSCMActivatorDispatcher.InterfaceId,
            IObjectExporterDispatcher.InterfaceId,
            RemUnknownServerDispatcher.InterfaceId).ConfigureAwait(false);

        ActivationResult opcEnum = await RemoteCreateInstanceOverAuthenticatedTransportAsync(
            activationEndpoint,
            OpcGuids.CLSID_OpcEnum,
            OpcGuids.IID_IOPCServerList2,
            cancellationToken).ConfigureAwait(false);
        RegisterActivatedInterface(activationChannel, OpcGuids.IID_IOPCServerList2, opcEnum.ObjectRef);
        await using DcomCallChannel resolverChannel = await ConnectAuthenticatedAsync(
            activationEndpoint,
            cancellationToken,
            IObjectExporterDispatcher.InterfaceId).ConfigureAwait(false);
        Guid remUnknownIpid = await ResolveOxid2Async(resolverChannel, opcEnum.ScmReply.Oxid, cancellationToken).ConfigureAwait(false);
        activationChannel.RegisterInterfaceIpid(RemUnknownServerDispatcher.InterfaceId, remUnknownIpid);

        Guid[] advertisedClsids = await BrowseOpcEnumAsync(activationChannel, cancellationToken).ConfigureAwait(false);
        await Assert.That(advertisedClsids).Contains(host.DaClsid);
        await Assert.That(advertisedClsids).Contains(host.AeClsid);
        await Assert.That(advertisedClsids).Contains(host.HdaClsid);

        await ExerciseDaAsync(activationEndpoint, host.DaClsid, remUnknownIpid, callbacks, cancellationToken).ConfigureAwait(false);
        await ExerciseAeAsync(activationEndpoint, host.AeClsid, remUnknownIpid, callbacks, cancellationToken).ConfigureAwait(false);
        await ExerciseHdaAsync(activationEndpoint, host.HdaClsid, remUnknownIpid, model, cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExerciseDaAsync(
        IPEndPoint endpoint,
        Guid clsid,
        Guid remUnknownIpid,
        CallbackFixture callbacks,
        CancellationToken cancellationToken)
    {
        await using DcomCallChannel channel = await ConnectAuthenticatedAsync(
            endpoint,
            cancellationToken,
            RemoteSCMActivatorDispatcher.InterfaceId,
            IObjectExporterDispatcher.InterfaceId,
            RemUnknownServerDispatcher.InterfaceId,
            IOPCServer.InterfaceId,
            IOPCCommon.InterfaceId,
            IOPCBrowse.InterfaceId,
            IOPCItemMgt.InterfaceId,
            IOPCSyncIO.InterfaceId,
            DaConnectionPoint.InterfaceId).ConfigureAwait(false);
        channel.RegisterInterfaceIpid(RemUnknownServerDispatcher.InterfaceId, remUnknownIpid);

        ActivationResult activation = await RemoteCreateInstanceOverAuthenticatedTransportAsync(endpoint, clsid, IOPCServer.InterfaceId, cancellationToken).ConfigureAwait(false);
        IOpcInterfaceRef rootRef = activation.ObjectRef;
        Guid[] rootIids = [IOPCServer.InterfaceId, IOPCCommon.InterfaceId, IOPCBrowse.InterfaceId];
        RegisterQiResults(channel, rootIids, await QueryInterfacesAsync(channel, rootRef.Ipid, rootIids, cancellationToken).ConfigureAwait(false));

        var common = new IOPCCommonClientProxy(channel);
        await common.SetClientNameAsync("Opc.Classic Phase9 E2E", cancellationToken).ConfigureAwait(false);

        var browse = new IOPCBrowseClientProxy(channel);
        string? continuation = string.Empty;
        await browse.BrowseAsync(string.Empty, ref continuation, 0, 1, "*", string.Empty, false, false, [], out bool more, out OpcBrowseElementResult[] elements, cancellationToken).ConfigureAwait(false);
        await Assert.That(more).IsFalse();
        await Assert.That(elements.Any(static element => element.ItemId == "Bucket Brigade")).IsTrue();

        var server = new IOPCServerClientProxy(channel);
        await server.AddGroupAsync("phase9-da", true, 100, 0x9009, 0, 0, 1033, IOPCItemMgt.InterfaceId, out int groupHandle, out _, out IOpcInterfaceRef groupRef, cancellationToken).ConfigureAwait(false);
        Guid[] groupIids = [IOPCItemMgt.InterfaceId, IOPCSyncIO.InterfaceId, DaConnectionPoint.InterfaceId];
        RegisterQiResults(channel, groupIids, await QueryInterfacesAsync(channel, groupRef.Ipid, groupIids, cancellationToken).ConfigureAwait(false));

        var itemMgt = new IOPCItemMgtClientProxy(channel);
        await itemMgt.AddItemsAsync(
            [
                new OpcItemDef(null, "Bucket Brigade.Int4", Active: true, ClientHandle: 0x9101, Blob: [], VarType.VT_EMPTY),
                new OpcItemDef(null, "Signals.Sine", Active: true, ClientHandle: 0x9102, Blob: [], VarType.VT_EMPTY),
            ],
            out OpcItemResult[] addResults,
            out int[] addErrors,
            cancellationToken).ConfigureAwait(false);
        await Assert.That(addErrors.All(IsSuccess)).IsTrue();

        var syncIo = new IOPCSyncIOClientProxy(channel);
        int[] writeErrors = await syncIo.WriteAsync([addResults[0].ServerHandle], [OpcVariant.FromInt32(2468)], cancellationToken).ConfigureAwait(false);
        await Assert.That(writeErrors.All(IsSuccess)).IsTrue();
        OpcItemState[] readBack = await syncIo.ReadAsync(1, [addResults[0].ServerHandle], out int[] readErrors, cancellationToken).ConfigureAwait(false);
        await Assert.That(readErrors.All(IsSuccess)).IsTrue();
        await Assert.That(readBack[0].Value.AsInt32()).IsEqualTo(2468);

        var connectionPoint = new DaConnectionPointProxy(channel);
        int cookie = await connectionPoint.AdviseAsync(callbacks.CreateDaSinkRef(), cancellationToken).ConfigureAwait(false);
        DataChangeSnapshot dataChange = await callbacks.WaitForDataChangeAsync(cancellationToken).ConfigureAwait(false);
        await Assert.That(dataChange.GroupHandle).IsEqualTo(0x9009);
        await Assert.That(dataChange.ClientHandles).Contains(0x9102);
        await connectionPoint.UnadviseAsync(cookie, cancellationToken).ConfigureAwait(false);
        await server.RemoveGroupAsync(groupHandle, force: true, cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExerciseAeAsync(
        IPEndPoint endpoint,
        Guid clsid,
        Guid remUnknownIpid,
        CallbackFixture callbacks,
        CancellationToken cancellationToken)
    {
        await using DcomCallChannel channel = await ConnectAuthenticatedAsync(
            endpoint,
            cancellationToken,
            RemoteSCMActivatorDispatcher.InterfaceId,
            RemUnknownServerDispatcher.InterfaceId,
            IOPCEventServer.InterfaceId,
            IOPCEventSubscriptionMgt.InterfaceId,
            AeConnectionPoint.InterfaceId).ConfigureAwait(false);
        channel.RegisterInterfaceIpid(RemUnknownServerDispatcher.InterfaceId, remUnknownIpid);

        ActivationResult activation = await RemoteCreateInstanceOverAuthenticatedTransportAsync(endpoint, clsid, IOPCEventServer.InterfaceId, cancellationToken).ConfigureAwait(false);
        RegisterActivatedInterface(channel, IOPCEventServer.InterfaceId, activation.ObjectRef);
        var server = new IOPCEventServerClientProxy(channel);
        OpcServerStatus status = await server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);

        IOpcInterfaceRef subscriptionRef = await InvokeInterfaceRefAsync(
            channel,
            IOPCEventServer.InterfaceId,
            IOPCEventServer.Opnums.CreateEventSubscriptionAsync,
            WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteInt32(1);
                writer.WriteInt32(50);
                writer.WriteInt32(8);
                writer.WriteInt32(0xAE09);
                writer.WriteGuid(IOPCEventSubscriptionMgt.InterfaceId);
            }),
            cancellationToken).ConfigureAwait(false);
        channel.RegisterInterfaceIpid(IOPCEventSubscriptionMgt.InterfaceId, subscriptionRef.Ipid);
        channel.RegisterInterfaceIpid(AeConnectionPoint.InterfaceId, subscriptionRef.Ipid);

        var connectionPoint = new DaConnectionPointProxy(channel);
        int cookie = await connectionPoint.AdviseAsync(callbacks.CreateAeSinkRef(), cancellationToken).ConfigureAwait(false);
        var subscription = new IOPCEventSubscriptionMgtClientProxy(channel);
        await subscription.SetFilterAsync(0x7, [], 1, 1000, [], [], cancellationToken).ConfigureAwait(false);
        await subscription.RefreshAsync(cookie, cancellationToken).ConfigureAwait(false);
        OpcEventNotification[] events = await callbacks.WaitForEventsAsync(cancellationToken).ConfigureAwait(false);
        await Assert.That(events.Length).IsGreaterThan(0);
        await connectionPoint.UnadviseAsync(cookie, cancellationToken).ConfigureAwait(false);
    }

    private static async Task ExerciseHdaAsync(
        IPEndPoint endpoint,
        Guid clsid,
        Guid remUnknownIpid,
        SimulatedPlantModel model,
        CancellationToken cancellationToken)
    {
        await using DcomCallChannel channel = await ConnectAuthenticatedAsync(
            endpoint,
            cancellationToken,
            RemoteSCMActivatorDispatcher.InterfaceId,
            RemUnknownServerDispatcher.InterfaceId,
            IOPCHDA_Server.InterfaceId,
            IOPCHDA_SyncRead.InterfaceId).ConfigureAwait(false);
        channel.RegisterInterfaceIpid(RemUnknownServerDispatcher.InterfaceId, remUnknownIpid);

        ActivationResult activation = await RemoteCreateInstanceOverAuthenticatedTransportAsync(endpoint, clsid, IOPCHDA_Server.InterfaceId, cancellationToken).ConfigureAwait(false);
        RegisterActivatedInterface(channel, IOPCHDA_Server.InterfaceId, activation.ObjectRef);
        Guid[] hdaIids = [IOPCHDA_Server.InterfaceId, IOPCHDA_SyncRead.InterfaceId];
        RegisterQiResults(channel, hdaIids, await QueryInterfacesAsync(channel, activation.ObjectRef.Ipid, hdaIids, cancellationToken).ConfigureAwait(false));

        var server = new IOPCHDA_ServerClientProxy(channel);
        int[] handles = await server.GetItemHandlesAsync(["Plant.Reactor1.Temperature"], [0x0FDA], cancellationToken).ConfigureAwait(false);
        var syncRead = new IOPCHDA_SyncReadClientProxy(channel);
        OpcHdaItem[] items = await syncRead.ReadRawAsync(
            OpcHdaTime.FromTimestamp(model.StartTimeUtc),
            OpcHdaTime.FromTimestamp(model.StartTimeUtc.AddSeconds(2)),
            3,
            bounds: true,
            handles,
            cancellationToken).ConfigureAwait(false);
        await Assert.That(items.Length).IsEqualTo(1);
        await Assert.That(items[0].Values.Length).IsGreaterThanOrEqualTo(3);
    }

    private static async Task<Guid[]> BrowseOpcEnumAsync(DcomCallChannel channel, CancellationToken cancellationToken)
    {
        IOpcInterfaceRef enumRef = await InvokeInterfaceRefAsync(
            channel,
            OpcGuids.IID_IOPCServerList2,
            3,
            WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteUInt32(0);
                writer.WriteConformantGuidArray([]);
                writer.WriteUInt32(0);
                writer.WriteConformantGuidArray([]);
            }),
            cancellationToken).ConfigureAwait(false);
        channel.RegisterInterfaceIpid(DiscoveryEnumGuidProxy.InterfaceId, enumRef.Ipid);

        NdrCallResult next = await channel.InvokeAsync(
            DiscoveryEnumGuidProxy.InterfaceId,
            DiscoveryEnumGuidProxy.Opnums.Next,
            WritePayload((ref NdrWriter writer) => writer.WriteInt32(16)),
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(next.Hresult, null), "IOPCEnumGUID::Next");
        var reader = new NdrReader(next.ResponsePayload.Span);
        Guid[] classIds = reader.ReadVaryingConformantGuidArray();
        _ = reader.ReadInt32();
        return classIds;
    }

    private static async Task<IPEndPoint> ResolveActivationEndpointViaEpmAsync(
        IPEndPoint epmEndpoint,
        IPEndPoint activationEndpoint,
        CancellationToken cancellationToken)
    {
        await Assert.That(epmEndpoint.Port).IsGreaterThan(0);
        var dispatcher = new EndpointMapperDispatcher(() => activationEndpoint);
        DispatchResult result = await dispatcher.DispatchAsync(
            3,
            EncodeMapRequest(IObjectExporterDispatcher.InterfaceId),
            cancellationToken).ConfigureAwait(false);
        await Assert.That(result.IsFailure).IsFalse();
        (byte[] tower, uint status) = DecodeSingleTowerMapResponse(result.Payload.Span);
        await Assert.That(status).IsEqualTo(EndpointMapperDispatcher.EptSuccess);
        await Assert.That(EndpointMapperTower.TryDecodeTcpTower(tower, out EndpointMapperTowerBinding binding)).IsTrue();
        return new IPEndPoint(binding.Address, binding.Port);
    }

    private static async Task<ActivationResult> RemoteCreateInstanceAsync(DcomCallChannel channel, Guid clsid, Guid requestedIid, CancellationToken cancellationToken)
    {
        NdrCallResult result = await channel.InvokeAsync(
            RemoteSCMActivatorDispatcher.InterfaceId,
            4,
            WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteGuid(clsid);
                writer.WriteGuid(requestedIid);
                writer.WriteUInt32(1);
                writer.WriteInt32(0x07);
                writer.WriteUInt32(0);
            }),
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IRemoteSCMActivator::RemoteCreateInstance");
        ActivationProperties properties = ActivationInfoCodec.Decode(result.ResponsePayload.Span);
        ScmReplyInfo reply = properties.ScmReplyInfo ?? throw new InvalidOperationException("Activation response did not include SCM reply info.");
        OpcException.ThrowIfFailed(new OpcResultId(reply.Hresult, null), "IRemoteSCMActivator SCM reply");
        var reader = new NdrReader(reply.ObjRef);
        return new ActivationResult(reply, OpcInterfaceRefCodec.Read(ref reader));
    }

    private static async Task<ActivationResult> RemoteCreateInstanceOverAuthenticatedTransportAsync(
        IPEndPoint endpoint,
        Guid clsid,
        Guid requestedIid,
        CancellationToken cancellationToken)
    {
        await using DcomCallChannel channel = await ConnectAuthenticatedAsync(
            endpoint,
            OpcProtectionLevel.Integrity,
            cancellationToken,
            RemoteSCMActivatorDispatcher.InterfaceId).ConfigureAwait(false);
        return await RemoteCreateInstanceAsync(channel, clsid, requestedIid, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<Guid> ResolveOxid2Async(DcomCallChannel channel, Guid oxid, CancellationToken cancellationToken)
    {
        NdrCallResult result = await channel.InvokeAsync(
            IObjectExporterDispatcher.InterfaceId,
            4,
            WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteGuid(oxid);
                writer.WriteUInt16(1);
                writer.WriteUInt16(0x07);
            }),
            cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IObjectExporter::ResolveOxid2");

        var reader = new NdrReader(result.ResponsePayload.Span);
        _ = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        for (int i = 0; i < entryCount; i++)
        {
            _ = reader.ReadUInt16();
        }
        reader.AlignTo(4);
        Guid ipid = reader.ReadGuid();
        _ = securityOffset;
        return ipid;
    }

    private static Task<OpcRemQIResult[]> QueryInterfacesAsync(DcomCallChannel channel, Guid ipid, Guid[] iids, CancellationToken cancellationToken)
    {
        var unknown = new IRemUnknownClientProxy(channel);
        return unknown.RemQueryInterfaceAsync(ipid, 1, (ushort)iids.Length, iids, cancellationToken);
    }

    private static Task<DcomCallChannel> ConnectAuthenticatedAsync(IPEndPoint endpoint, CancellationToken cancellationToken, params Guid[] preBindIids) =>
        ConnectAuthenticatedAsync(endpoint, OpcProtectionLevel.Connect, cancellationToken, preBindIids);

    private static async Task<DcomCallChannel> ConnectAuthenticatedAsync(IPEndPoint endpoint, OpcProtectionLevel protectionLevel, CancellationToken cancellationToken, params Guid[] preBindIids)
    {
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(endpoint.Address.ToString(), endpoint.Port, cancellationToken).ConfigureAwait(false);
        var credentials = new NetworkCredential(User, Password, Domain);
        OpcConnectData connectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse($"opcda://{endpoint.Address}:{endpoint.Port}/Phase9.FullStack"),
            credentials,
            protectionLevel);
        return new DcomCallChannel(transport, NtlmAuthentication.CreateAuthContext(connectData), preBindIids);
    }

    private static async Task<DcomCallChannel> ConnectUnauthenticatedAsync(IPEndPoint endpoint, CancellationToken cancellationToken, params Guid[] preBindIids)
    {
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(endpoint.Address.ToString(), endpoint.Port, cancellationToken).ConfigureAwait(false);
        return new DcomCallChannel(transport, NoOpAuthContext.Instance, preBindIids);
    }

    private static void RegisterActivatedInterface(DcomCallChannel channel, Guid iid, IOpcInterfaceRef objRef) =>
        channel.RegisterInterfaceIpid(iid, objRef.Ipid);

    private static void RegisterQiResults(DcomCallChannel channel, Guid[] iids, OpcRemQIResult[] results)
    {
        for (int i = 0; i < iids.Length; i++)
        {
            OpcException.ThrowIfFailed(new OpcResultId(results[i].Hresult, null), "IRemUnknown::RemQueryInterface");
            channel.RegisterInterfaceIpid(iids[i], results[i].Ipid);
        }
    }

    private static bool IsSuccess(int hresult) => hresult >= 0;

    private static async Task<IOpcInterfaceRef> InvokeInterfaceRefAsync(DcomCallChannel channel, Guid iid, int opnum, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        NdrCallResult result = await channel.InvokeAsync(iid, opnum, payload, cancellationToken).ConfigureAwait(false);
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "Interface pointer call");
        return DecodeInterfaceRefResponse(result.ResponsePayload.Span);
    }

    private static IOpcInterfaceRef DecodeInterfaceRefResponse(ReadOnlySpan<byte> payload)
    {
        uint firstWord = BinaryPrimitives.ReadUInt32LittleEndian(payload);
        if (firstWord == 0x574F454D)
        {
            var bareReader = new NdrReader(payload);
            return OpcInterfaceRefCodec.Read(ref bareReader);
        }

        var reader = new NdrReader(payload);
        _ = reader.ReadUInt32();
        uint length = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        ReadOnlySpan<byte> objRef = reader.ReadRawBytes((int)length);
        var objRefReader = new NdrReader(objRef);
        return OpcInterfaceRefCodec.Read(ref objRefReader);
    }

    private static byte[] EncodeMapRequest(Guid interfaceId)
    {
        byte[] mapTower = EndpointMapperTower.EncodeTcpTower(interfaceId, 0, 0, new IPEndPoint(IPAddress.Any, 0));
        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteNullReferent();
            _ = writer.WriteReferentId();
            writer.WriteRawBytes(new byte[20]);
            writer.WriteUInt32(1);
            EndpointMapperTower.WriteTowerPointee(ref writer, mapTower);
        });
    }

    private static (byte[] Tower, uint Status) DecodeSingleTowerMapResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        _ = reader.ReadRawBytes(20);
        uint numTowers = reader.ReadUInt32();
        byte[] tower = [];
        if (numTowers > 0 && reader.TryReadReferentId(out _))
        {
            _ = reader.ReadUInt32();
            _ = reader.ReadUInt32();
            uint actualCount = reader.ReadUInt32();
            for (int i = 0; i < actualCount; i++)
            {
                _ = reader.TryReadReferentId(out _);
            }
            if (EndpointMapperTower.TryReadTower(payload, reader.Position, out byte[] decodedTower, out int bytesRead))
            {
                tower = decodedTower;
                _ = reader.ReadRawBytes(bytesRead);
                reader.AlignTo(4);
            }
        }

        uint status = BinaryPrimitives.ReadUInt32LittleEndian(payload.Slice(reader.Position, 4));
        return (tower, status);
    }

    private static byte[] WritePayload(NdrWriteAction action)
    {
        var buffer = new byte[8192];
        var writer = new NdrWriter(buffer);
        action(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed record ActivationResult(ScmReplyInfo ScmReply, IOpcInterfaceRef ObjectRef);

    private sealed class CallbackFixture : IAsyncDisposable
    {
        private readonly OpcServerListener _listener;
        private readonly Guid _daSinkIpid;
        private readonly Guid _aeSinkIpid;
        private readonly RecordingDataCallback _dataCallback;
        private readonly RecordingEventSink _eventSink;

        private CallbackFixture(
            OpcServerListener listener,
            Guid daSinkIpid,
            Guid aeSinkIpid,
            RecordingDataCallback dataCallback,
            RecordingEventSink eventSink)
        {
            _listener = listener;
            _daSinkIpid = daSinkIpid;
            _aeSinkIpid = aeSinkIpid;
            _dataCallback = dataCallback;
            _eventSink = eventSink;
            Endpoint = (IPEndPoint)listener.LocalEndpoint;
        }

        public IPEndPoint Endpoint { get; }

        public static async Task<CallbackFixture> StartAsync(CancellationToken cancellationToken)
        {
            var registry = new OpcObjectRegistry();
            OpcServerListener? listener = null;
            var exporter = new IObjectExporterDispatcher(() => listener?.LocalEndpoint as IPEndPoint, registry);
            var dataCallback = new RecordingDataCallback();
            Guid daSinkIpid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCDataCallback.InterfaceId] = new IOPCDataCallbackServerDispatcher(dataCallback),
            });
            var eventSink = new RecordingEventSink();
            Guid aeSinkIpid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCEventSink.InterfaceId] = new IOPCEventSinkServerDispatcher(eventSink),
            });
            var processor = new RpcServerConnectionProcessor(
                new Dictionary<Guid, IOpcServerDispatcher>
                {
                    [IObjectExporterDispatcher.InterfaceId] = exporter,
                },
                registry,
                new ConfiguredAuthenticationSource(User, Password, Domain));
            listener = new OpcServerListener(new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0)), processor);
            await listener.StartAsync(cancellationToken).ConfigureAwait(false);
            return new CallbackFixture(listener, daSinkIpid, aeSinkIpid, dataCallback, eventSink);
        }

        public IOpcInterfaceRef CreateDaSinkRef() => CreateSinkRef(IOPCDataCallback.InterfaceId, _daSinkIpid);

        public IOpcInterfaceRef CreateAeSinkRef() => CreateSinkRef(IOPCEventSink.InterfaceId, _aeSinkIpid);

        public Task<DataChangeSnapshot> WaitForDataChangeAsync(CancellationToken cancellationToken) =>
            _dataCallback.WaitForAsync(cancellationToken);

        public Task<OpcEventNotification[]> WaitForEventsAsync(CancellationToken cancellationToken) =>
            _eventSink.WaitForAsync(cancellationToken);

        public ValueTask DisposeAsync() => _listener.DisposeAsync();

        private IOpcInterfaceRef CreateSinkRef(Guid iid, Guid ipid)
        {
            (ushort[] bindings, ushort securityOffset) = IObjectExporterDispatcher.BuildResolverBindings(Endpoint);
            return new OpcInterfaceRef(iid, 0, 1, 0xCA11, 0x9009, ipid, securityOffset, bindings);
        }
    }

    private sealed class RecordingDataCallback : IOPCDataCallback
    {
        private readonly TaskCompletionSource<DataChangeSnapshot> _next = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<DataChangeSnapshot> WaitForAsync(CancellationToken cancellationToken) => _next.Task.WaitAsync(cancellationToken);

        public Task OnDataChangeAsync(int transactionId, int groupHandle, int masterQuality, int masterError, int[] clientHandles, OpcVariant[] values, ushort[] qualities, long[] timestamps, int[] errors, CancellationToken cancellationToken = default)
        {
            _next.TrySetResult(new DataChangeSnapshot(transactionId, groupHandle, clientHandles, values));
            return Task.CompletedTask;
        }

        public Task OnReadCompleteAsync(int transactionId, int groupHandle, int masterQuality, int masterError, int[] clientHandles, OpcVariant[] values, ushort[] qualities, long[] timestamps, int[] errors, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OnWriteCompleteAsync(int transactionId, int groupHandle, int masterError, int[] clientHandles, int[] errors, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task OnCancelCompleteAsync(int transactionId, int groupHandle, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RecordingEventSink : IOPCEventSink
    {
        private readonly TaskCompletionSource<OpcEventNotification[]> _next = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<OpcEventNotification[]> WaitForAsync(CancellationToken cancellationToken) => _next.Task.WaitAsync(cancellationToken);

        public Task OnEventAsync(int clientSubscription, bool refresh, bool lastRefresh, OpcEventNotification[] events, CancellationToken cancellationToken = default)
        {
            if (events.Length > 0)
            {
                _next.TrySetResult(events);
            }

            return Task.CompletedTask;
        }
    }

    private sealed record DataChangeSnapshot(int TransactionId, int GroupHandle, int[] ClientHandles, OpcVariant[] Values);
}
