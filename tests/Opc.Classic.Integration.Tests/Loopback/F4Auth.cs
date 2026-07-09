// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;
using System.Net;
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Remoting;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Integration.Tests.Support;
using Opc.Classic.Ndr;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F4Auth
{
    private const string KerberosSkipReason =
        "NTLMv2 authenticated calls over the managed TCP listener are supported; Kerberos still requires the KDC fixture covered by KerberosKdcFixtureTests plus server-side Kerberos acceptor wiring on the listener.";

    private const string SpnegoSkipReason =
        "NTLMv2 authenticated calls over the managed TCP listener are supported; SPNEGO still requires server-side negotiation wiring on the listener before it can select NTLMv2 or Kerberos.";

    private const string Domain = "LOOPBACK";
    private const string User = "phase1-user";
    private const string Password = "phase1-password";

    [Test]
    public async Task Ntlmv2_authenticates_the_managed_loopback_call_path()
    {
        StubDaServer server = StubDaServer.CompatMatrixNet10Server();
        await using OpcServerListener listener = await StartListenerAsync(server);
        await using DcomCallChannel channel = await ConnectAsync(listener, Password, OpcProtectionLevel.Integrity);
        var proxy = new IOPCServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(TestContext.Current!.CancellationToken);

        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.VendorInfo).IsEqualTo(server.VendorInfo);
    }

    [Test]
    public async Task Ntlmv2_rejects_wrong_password_on_managed_loopback_call_path()
    {
        await using OpcServerListener listener = await StartListenerAsync(StubDaServer.CompatMatrixNet10Server());
        await using DcomCallChannel channel = await ConnectAsync(listener, "wrong-password", OpcProtectionLevel.Integrity);
        var proxy = new IOPCServerClientProxy(channel);

        await Assert.That(async () => await proxy.GetStatusAsync(TestContext.Current!.CancellationToken))
            .Throws<Exception>();
    }

    [Test]
    public async Task Auth_required_listener_rejects_anonymous_bind_and_plain_request()
    {
        await using OpcServerListener listener = await StartListenerAsync(StubDaServer.CompatMatrixNet10Server());
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            TestContext.Current!.CancellationToken);
        await using var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);
        var proxy = new IOPCServerClientProxy(channel);

        await Assert.That(async () => await proxy.GetStatusAsync(TestContext.Current!.CancellationToken))
            .Throws<Exception>();
    }

    [Test]
    public async Task Ntlmv2_privacy_authenticates_and_seals_the_managed_loopback_call_path()
    {
        StubDaServer server = StubDaServer.CompatMatrixNet10Server();
        await using OpcServerListener listener = await StartListenerAsync(server);
        await using DcomCallChannel channel = await ConnectAsync(listener, Password, OpcProtectionLevel.Privacy);
        var proxy = new IOPCServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(TestContext.Current!.CancellationToken);

        await Assert.That(status.VendorInfo).IsEqualTo(server.VendorInfo);
    }

    [Test]
    [Arguments(OpcProtectionLevel.Integrity)]
    [Arguments(OpcProtectionLevel.Privacy)]
    public async Task Ntlmv2_packet_protection_accepts_non_aligned_request_stubs(OpcProtectionLevel protectionLevel)
    {
        Guid interfaceId = Guid.NewGuid();
        var dispatcher = new EchoDispatcher();
        await using OpcServerListener listener = await StartListenerAsync(interfaceId, dispatcher);
        await using DcomCallChannel channel = await ConnectAsync(listener, Password, protectionLevel);

        NdrCallResult result = await channel.InvokeAsync(
            interfaceId,
            opnum: 7,
            requestPayload: new byte[] { 0x5A },
            TestContext.Current!.CancellationToken);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(dispatcher.LastPayload.ToArray()).IsEquivalentTo(new byte[] { 0x5A });
        await Assert.That(result.ResponsePayload.ToArray()).IsEquivalentTo(new byte[] { 0xA5, 0x01, 0x02 });
    }

    [Test]
    public async Task Established_privacy_session_rejects_weaker_rechallenge_before_unsigned_requests()
    {
        Guid interfaceId = Guid.NewGuid();
        await using OpcServerListener listener = await StartListenerAsync(interfaceId, new EchoDispatcher());
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        await using TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            TestContext.Current!.CancellationToken);

        IAuthContext privacy = CreateAuthContext(endpoint, Password, OpcProtectionLevel.Privacy);
        byte[] type1 = privacy.BuildInitialToken();
        await WriteFrameAsync(transport, AttachAuthenticationVerifier(
            PduCodec.EncodePdu(NewBindForInterface(interfaceId, contextId: 0, callId: 1), ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE),
            OpcProtectionLevel.Privacy,
            type1));
        byte[] bindAck = await PduCodec.ReadPduFrameAsync(transport.Input, TestContext.Current!.CancellationToken);
        byte[] type2 = ExtractAuthenticationValue(bindAck);
        byte[] type3 = privacy.ProcessChallengeToken(type2);
        await WriteFrameAsync(transport, AttachAuthenticationVerifier(
            PduCodec.EncodePdu(new Auth3Pdu { CallId = 1 }, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE),
            OpcProtectionLevel.Privacy,
            type3));

        IAuthContext weaker = CreateAuthContext(endpoint, Password, OpcProtectionLevel.Connect);
        byte[] weakerType1 = weaker.BuildInitialToken();
        await WriteFrameAsync(transport, AttachAuthenticationVerifier(
            PduCodec.EncodePdu(NewAlterContextForInterface(interfaceId, contextId: 1, callId: 2), ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE),
            OpcProtectionLevel.Connect,
            weakerType1));

        byte[] rejection = await PduCodec.ReadPduFrameAsync(transport.Input, TestContext.Current!.CancellationToken);
        ConnectionOrientedPdu pdu = PduCodec.DecodePdu(rejection);
        await Assert.That(pdu).IsTypeOf<BindNoAcknowledgePdu>();
    }

    [Test]
    public async Task Ntlmv2_authenticates_full_da_graph_and_runs_lifecycle_over_activation()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(20));
        CancellationToken cancellationToken = timeout.Token;

        await using AuthenticatedActivationFixture fixture = await StartActivationListenerAsync(cancellationToken);
        IPEndPoint endpoint = (IPEndPoint)fixture.Listener.LocalEndpoint;
        Guid[] rootIids =
        [
            IOPCServer.InterfaceId,
            IOPCCommon.InterfaceId,
            IOPCBrowse.InterfaceId,
            IOPCBrowseServerAddressSpace.InterfaceId,
            IOPCItemProperties.InterfaceId,
        ];

        RemoteActivationResponse activationResponse = await fixture.ActivationServer.RemoteActivationAsync(
            new RemoteActivationRequest(fixture.DaClsid, rootIids, 3, 0, [0x07]),
            cancellationToken);

        await Assert.That(activationResponse.Hresult).IsEqualTo(0);
        await Assert.That(activationResponse.InterfaceResults.Select(static result => result.Hresult).All(IsSuccess)).IsTrue();
        IOpcInterfaceRef rootRef = ReadObjRef(activationResponse.InterfaceResults[0].ObjRef);
        await Assert.That(rootRef.Iid).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(rootRef.Ipid).IsNotEqualTo(Guid.Empty);

        await using DcomCallChannel channel = await ConnectAuthenticatedAsync(
            endpoint,
            cancellationToken);
        channel.RegisterInterfaceIpid(RemUnknownServerDispatcher.InterfaceId, activationResponse.IpidRemUnknown);
        var unknown = new IRemUnknownClientProxy(channel);
        OpcRemQIResult[] rootQi = await unknown.RemQueryInterfaceAsync(
            rootRef.Ipid,
            cRefs: 1,
            cIids: (ushort)rootIids.Length,
            iids: rootIids,
            cancellationToken);
        await Assert.That(rootQi.Select(static result => result.Hresult).All(IsSuccess)).IsTrue();
        await Assert.That(rootQi.Select(static result => result.Ipid).All(static ipid => ipid != Guid.Empty)).IsTrue();

        RegisterQiResults(channel, rootIids, rootQi);

        var common = new IOPCCommonClientProxy(channel);
        await common.SetClientNameAsync("Opc.Classic authenticated DA lifecycle", cancellationToken);

        var browse = new IOPCBrowseClientProxy(channel);
        string? continuation = string.Empty;
        await browse.BrowseAsync(
            string.Empty,
            ref continuation,
            maxElementsReturned: 0,
            browseFilter: 1,
            elementNameFilter: "*",
            vendorFilter: string.Empty,
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: Array.Empty<int>(),
            out bool moreElements,
            out OpcBrowseElementResult[] elements,
            cancellationToken);
        await Assert.That(moreElements).IsFalse();
        await Assert.That(elements.Select(static element => element.ItemId).Contains("Bucket Brigade")).IsTrue();

        var itemProperties = new IOPCItemPropertiesClientProxy(channel);
        await itemProperties.QueryAvailablePropertiesAsync(
            "Bucket Brigade.Int4",
            out int[] propertyIds,
            out string[] descriptions,
            out ushort[] dataTypes,
            cancellationToken);
        await Assert.That(propertyIds.Length).IsEqualTo(descriptions.Length);
        await Assert.That(propertyIds.Length).IsEqualTo(dataTypes.Length);

        var server = new IOPCServerClientProxy(channel);
        await server.AddGroupAsync(
            name: "auth-lifecycle-group",
            active: true,
            requestedUpdateRate: 250,
            clientGroupHandle: 0x6406,
            timeBias: 0,
            percentDeadband: 0.0f,
            localeId: 1033,
            requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
            serverGroupHandle: out int serverGroupHandle,
            revisedUpdateRate: out int revisedUpdateRate,
            group: out IOpcInterfaceRef groupRef,
            cancellationToken);
        await Assert.That(serverGroupHandle).IsGreaterThan(0);
        await Assert.That(revisedUpdateRate).IsEqualTo(250);
        await Assert.That(groupRef.Ipid).IsNotEqualTo(Guid.Empty);

        Guid[] groupIids =
        [
            IOPCGroupStateMgt.InterfaceId,
            IOPCGroupStateMgt2.InterfaceId,
            IOPCItemMgt.InterfaceId,
            IOPCSyncIO.InterfaceId,
            IOPCSyncIO2.InterfaceId,
            IConnectionPointContainer.InterfaceId,
        ];
        OpcRemQIResult[] groupQi = await unknown.RemQueryInterfaceAsync(
            groupRef.Ipid,
            cRefs: 1,
            cIids: (ushort)groupIids.Length,
            iids: groupIids,
            cancellationToken);
        await Assert.That(groupQi.Select(static result => result.Hresult).All(IsSuccess)).IsTrue();
        await Assert.That(groupQi.Select(static result => result.Ipid).All(static ipid => ipid != Guid.Empty)).IsTrue();

        RegisterQiResults(channel, groupIids, groupQi);

        var groupState = new IOPCGroupStateMgtClientProxy(channel);
        OpcGroupState state = await groupState.GetStateAsync(cancellationToken);
        await Assert.That(state.ServerHandle).IsEqualTo(serverGroupHandle);
        await Assert.That(state.Name).IsEqualTo("auth-lifecycle-group");

        var itemMgt = new IOPCItemMgtClientProxy(channel);
        await itemMgt.AddItemsAsync(
            [new OpcItemDef(null, "Bucket Brigade.Int4", Active: true, ClientHandle: 42, Blob: [], VarType.VT_EMPTY)],
            out OpcItemResult[] addResults,
            out int[] addErrors,
            cancellationToken);
        await Assert.That(addErrors.All(IsSuccess)).IsTrue();
        await Assert.That(addResults[0].ServerHandle).IsGreaterThan(0);

        fixture.Server.RefreshFromModel();
        var syncIo = new IOPCSyncIOClientProxy(channel);
        OpcItemState[] deviceRead = await syncIo.ReadAsync(2, [addResults[0].ServerHandle], out int[] deviceErrors, cancellationToken);
        await Assert.That(deviceErrors.All(IsSuccess)).IsTrue();
        await Assert.That(deviceRead[0].Value.IsEmpty).IsFalse();

        int[] writeErrors = await syncIo.WriteAsync([addResults[0].ServerHandle], [OpcVariant.FromInt32(1234)], cancellationToken);
        await Assert.That(writeErrors.All(IsSuccess)).IsTrue();
        OpcItemState[] cacheRead = await syncIo.ReadAsync(1, [addResults[0].ServerHandle], out int[] cacheErrors, cancellationToken);
        await Assert.That(cacheErrors.All(IsSuccess)).IsTrue();
        await Assert.That(cacheRead[0].Value.AsInt32()).IsEqualTo(1234);

        await server.RemoveGroupAsync(serverGroupHandle, force: true, cancellationToken);
    }

    [Test, Skip(KerberosSkipReason)]
    public void Kerberos_authenticates_the_managed_loopback_call_path()
    {
        // TODO: use the Testcontainers KDC fixture to issue tickets and authenticate the loopback channel.
    }

    [Test, Skip(SpnegoSkipReason)]
    public void Spnego_negotiates_ntlmv2_or_kerberos_for_the_managed_loopback_call_path()
    {
        // TODO: exercise SPNEGO negotiation and assert the selected NTLMv2/Kerberos mechanism is enforced.
    }

    private static async Task<OpcServerListener> StartListenerAsync(StubDaServer server)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCServer.InterfaceId] = new IOPCServerServerDispatcher(server),
            },
            new ConfiguredAuthenticationSource(User, Password, Domain));
        var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(TestContext.Current!.CancellationToken);
        return listener;
    }

    private static async Task<OpcServerListener> StartListenerAsync(Guid interfaceId, IOpcServerDispatcher dispatcher)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [interfaceId] = dispatcher,
            },
            new ConfiguredAuthenticationSource(User, Password, Domain));
        var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(TestContext.Current!.CancellationToken);
        return listener;
    }

    private static async Task<AuthenticatedActivationFixture> StartActivationListenerAsync(CancellationToken cancellationToken)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var objectRegistry = new OpcObjectRegistry();
        var model = new SimulatedPlantModel();
        var server = new SimDaHostServer(model, objectRegistry);
        OpcServerListener? listener = null;
        var objectExporter = new IObjectExporterDispatcher(
            endpointProvider: () => listener?.LocalEndpoint as IPEndPoint,
            objectRegistry: objectRegistry);
        Guid daClsid = Guid.NewGuid();
        var activationServer = new DaActivationServer(
            daClsid,
            server,
            objectRegistry,
            endpointProvider: () => listener?.LocalEndpoint as IPEndPoint,
            remUnknownIpid: objectExporter.IRemUnknownIpid);
        var logger = new TestLogger();
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [ActivationServer.InterfaceId] = new ActivationServer(activationServer, logger),
                [IObjectExporterDispatcher.InterfaceId] = objectExporter,
            },
            objectRegistry,
            new ConfiguredAuthenticationSource(User, Password, Domain),
            logger);
        listener = new OpcServerListener(endpoint, processor, logger);
        await listener.StartAsync(cancellationToken);
        return new AuthenticatedActivationFixture(listener, server, activationServer, daClsid);
    }

    private static async Task<DcomCallChannel> ConnectAsync(
        OpcServerListener listener,
        string password,
        OpcProtectionLevel protectionLevel)
    {
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var credentials = new NetworkCredential(User, password, Domain);
        var connectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse($"opcda://{endpoint.Address}:{endpoint.Port}/Loopback.Auth"),
            credentials,
            protectionLevel);
        IAuthContext authContext = NtlmAuthentication.CreateAuthContext(connectData);
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            TestContext.Current!.CancellationToken);
        return new DcomCallChannel(transport, authContext);
    }

    private static async Task<DcomCallChannel> ConnectAuthenticatedAsync(
        IPEndPoint endpoint,
        CancellationToken cancellationToken)
    {
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            cancellationToken);
        return new DcomCallChannel(
            transport,
            CreateAuthContext(endpoint, Password, OpcProtectionLevel.Connect),
            [
                RemUnknownServerDispatcher.InterfaceId,
                IOPCServer.InterfaceId,
                IOPCCommon.InterfaceId,
                IOPCBrowse.InterfaceId,
                IOPCBrowseServerAddressSpace.InterfaceId,
                IOPCItemProperties.InterfaceId,
                IOPCGroupStateMgt.InterfaceId,
                IOPCGroupStateMgt2.InterfaceId,
                IOPCItemMgt.InterfaceId,
                IOPCSyncIO.InterfaceId,
                IOPCSyncIO2.InterfaceId,
                IConnectionPointContainer.InterfaceId,
            ]);
    }

    private static IAuthContext CreateAuthContext(IPEndPoint endpoint, string password, OpcProtectionLevel protectionLevel)
    {
        var credentials = new NetworkCredential(User, password, Domain);
        var connectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse($"opcda://{endpoint.Address}:{endpoint.Port}/Loopback.Auth"),
            credentials,
            protectionLevel);
        return NtlmAuthentication.CreateAuthContext(connectData);
    }

    private static BindPdu NewBindForInterface(Guid interfaceId, int contextId, int callId) =>
        new()
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList = [BuildContext(interfaceId, contextId)],
        };

    private static AlterContextPdu NewAlterContextForInterface(Guid interfaceId, int contextId, int callId) =>
        new()
        {
            CallId = callId,
            AssociationGroupId = 0,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ContextList = [BuildContext(interfaceId, contextId)],
        };

    private static PresentationContext BuildContext(Guid interfaceId, int contextId) =>
        new(contextId, new PresentationSyntax(new UUID(interfaceId.ToString("D")), 0, 0));

    private static async Task WriteFrameAsync(TcpClientTransport transport, byte[] frame)
    {
        Memory<byte> output = transport.Output.GetMemory(frame.Length);
        frame.AsSpan().CopyTo(output.Span);
        transport.Output.Advance(frame.Length);
        await transport.FlushAsync(TestContext.Current!.CancellationToken);
    }

    private static byte[] AttachAuthenticationVerifier(byte[] pduBytes, OpcProtectionLevel protectionLevel, ReadOnlySpan<byte> authValue)
    {
        const int headerLength = 8;
        int padding = PaddingTo(pduBytes.Length, 4);
        int verifierStart = pduBytes.Length + padding;
        int fragmentLength = verifierStart + headerLength + authValue.Length;
        var result = new byte[fragmentLength];
        pduBytes.CopyTo(result, 0);
        Span<byte> verifier = result.AsSpan(verifierStart, headerLength);
        verifier[0] = NtlmAuthentication.AUTHENTICATIONSERVICENTLM;
        verifier[1] = (byte)ToRpcProtectionLevel(protectionLevel);
        verifier[2] = (byte)padding;
        verifier[3] = 0;
        BinaryPrimitives.WriteInt32LittleEndian(verifier[4..], 0);
        authValue.CopyTo(result.AsSpan(verifierStart + headerLength));
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET), (ushort)fragmentLength);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET), (ushort)authValue.Length);
        return result;
    }

    private static byte[] ExtractAuthenticationValue(byte[] frame)
    {
        const int headerLength = 8;
        int authLength = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET));
        int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET));
        int verifierStart = fragmentLength - authLength - headerLength;
        return frame.AsSpan(verifierStart + headerLength, authLength).ToArray();
    }

    private static ProtectionLevel ToRpcProtectionLevel(OpcProtectionLevel protectionLevel) => protectionLevel switch
    {
        OpcProtectionLevel.None => ProtectionLevel.PROTECTION_LEVEL_NONE,
        OpcProtectionLevel.Connect => ProtectionLevel.PROTECTION_LEVEL_CONNECT,
        OpcProtectionLevel.Call => ProtectionLevel.PROTECTION_LEVEL_CALL,
        OpcProtectionLevel.Packet => ProtectionLevel.PROTECTION_LEVEL_PACKET,
        OpcProtectionLevel.Integrity => ProtectionLevel.PROTECTION_LEVEL_INTEGRITY,
        OpcProtectionLevel.Privacy => ProtectionLevel.PROTECTION_LEVEL_PRIVACY,
        _ => ProtectionLevel.PROTECTION_LEVEL_NONE,
    };

    private static int PaddingTo(int length, int alignment)
    {
        int remainder = length % alignment;
        return remainder == 0 ? 0 : alignment - remainder;
    }

    private static IOpcInterfaceRef ReadObjRef(ReadOnlyMemory<byte> objRef)
    {
        var reader = new NdrReader(objRef.Span);
        return OpcInterfaceRefCodec.Read(ref reader);
    }

    private static void RegisterQiResults(DcomCallChannel channel, Guid[] iids, OpcRemQIResult[] results)
    {
        for (int i = 0; i < iids.Length; i++)
        {
            channel.RegisterInterfaceIpid(iids[i], results[i].Ipid);
        }
    }

    private static bool IsSuccess(int hresult) => hresult >= 0;

    private static byte[] EncodeStandardObjRef(Guid iid, Guid oxid, Guid oid, Guid ipid, ReadOnlyMemory<byte> oxidBindings)
    {
        (ushort securityOffset, ushort[] resolverBindings) = DecodeDualStringArray(oxidBindings.Span);
        var interfaceRef = new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: UInt64FromGuid(oxid),
            oid: UInt64FromGuid(oid),
            ipid: ipid,
            securityOffset: securityOffset,
            resolverBindings: resolverBindings);
        var buffer = new byte[256 + resolverBindings.Length * sizeof(ushort)];
        var writer = new NdrWriter(buffer);
        OpcInterfaceRefCodec.Write(ref writer, interfaceRef);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static (ushort SecurityOffset, ushort[] Bindings) DecodeDualStringArray(ReadOnlySpan<byte> dualStringArray)
    {
        if (dualStringArray.Length < 4)
        {
            return (0, Array.Empty<ushort>());
        }

        var reader = new NdrReader(dualStringArray);
        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        var bindings = new ushort[entryCount];
        for (int i = 0; i < bindings.Length; i++)
        {
            bindings[i] = reader.ReadUInt16();
        }

        return (securityOffset, bindings);
    }

    private static ulong UInt64FromGuid(Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        bool ok = value.TryWriteBytes(bytes);
        if (!ok)
        {
            throw new InvalidOperationException("Guid.TryWriteBytes failed unexpectedly.");
        }

        return BitConverter.ToUInt64(bytes);
    }

    private sealed class AuthenticatedActivationFixture : IAsyncDisposable
    {
        public AuthenticatedActivationFixture(
            OpcServerListener listener,
            SimDaHostServer server,
            IActivationServer activationServer,
            Guid daClsid)
        {
            Listener = listener;
            Server = server;
            ActivationServer = activationServer;
            DaClsid = daClsid;
        }

        public OpcServerListener Listener { get; }

        public SimDaHostServer Server { get; }

        public IActivationServer ActivationServer { get; }

        public Guid DaClsid { get; }

        public async ValueTask DisposeAsync() => await Listener.DisposeAsync();
    }

    private sealed class EchoDispatcher : IOpcServerDispatcher
    {
        public ReadOnlyMemory<byte> LastPayload { get; private set; }

        public ValueTask<DispatchResult> DispatchAsync(
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastPayload = requestPayload.ToArray();
            return ValueTask.FromResult(DispatchResult.Success(new byte[] { 0xA5, 0x01, 0x02 }));
        }
    }

    private sealed class DaActivationServer : IActivationServer
    {
        private const uint AuthnHintPacketIntegrity = 5;
        private static readonly (ushort Major, ushort Minor) ServerComVersion = (5, 1);
        private static readonly int ENoInterface = OpcResultId.NoInterface.Code;

        private readonly Guid _clsid;
        private readonly SimDaHostServer _server;
        private readonly OpcObjectRegistry _objectRegistry;
        private readonly Func<IPEndPoint?> _endpointProvider;
        private readonly Guid _remUnknownIpid;

        public DaActivationServer(
            Guid clsid,
            SimDaHostServer server,
            OpcObjectRegistry objectRegistry,
            Func<IPEndPoint?> endpointProvider,
            Guid remUnknownIpid)
        {
            _clsid = clsid;
            _server = server;
            _objectRegistry = objectRegistry;
            _endpointProvider = endpointProvider;
            _remUnknownIpid = remUnknownIpid;
        }

        public async Task<int> RemoteActivationAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default)
        {
            RemoteActivationResponse response = await RemoteActivationAsync(
                new RemoteActivationRequest(clsid, [requestedIid], 3, 0, [0x07]),
                cancellationToken);
            return response.Hresult;
        }

        public Task<RemoteActivationResponse> RemoteActivationAsync(
            RemoteActivationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (request.Clsid != _clsid)
            {
                return Task.FromResult(new RemoteActivationResponse(
                    unchecked((int)0x80040154u),
                    Guid.Empty,
                    _remUnknownIpid,
                    AuthnHintPacketIntegrity,
                    ServerComVersion,
                    Array.Empty<RemoteActivationInterfaceResult>()));
            }

            IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers = BuildDispatchers();
            Guid ipid = _objectRegistry.Register(dispatchers, publicRefs: 1);
            Guid oxid = Guid.NewGuid();
            Guid oid = Guid.NewGuid();
            byte[] bindings = IObjectExporterDispatcher.EncodeDualStringArrayForListener(_endpointProvider());
            var results = new RemoteActivationInterfaceResult[Math.Max(1, request.RequestedIids.Count)];
            for (int i = 0; i < results.Length; i++)
            {
                Guid iid = request.RequestedIids.Count == 0 ? IOPCServer.InterfaceId : request.RequestedIids[i];
                results[i] = dispatchers.ContainsKey(iid)
                    ? new RemoteActivationInterfaceResult(0, EncodeStandardObjRef(iid, oxid, oid, ipid, bindings))
                    : new RemoteActivationInterfaceResult(ENoInterface, Array.Empty<byte>());
            }

            return Task.FromResult(new RemoteActivationResponse(
                0,
                oxid,
                _remUnknownIpid,
                AuthnHintPacketIntegrity,
                ServerComVersion,
                results)
            {
                OxidBindings = bindings,
            });
        }

        private IReadOnlyDictionary<Guid, IOpcServerDispatcher> BuildDispatchers()
        {
            var daDispatcher = new OpcDaServerDispatcher(_server);
            IOpcAddressSpace addressSpace = _server.BuildAddressSpace();
            return new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCServer.InterfaceId] = daDispatcher.ServerDispatcher,
                [IOPCCommon.InterfaceId] = daDispatcher.CommonDispatcher,
                [IOPCBrowseServerAddressSpace.InterfaceId] = new IOPCBrowseServerAddressSpaceServerDispatcher(new DefaultBrowseServerAddressSpace(addressSpace)),
                [IOPCBrowse.InterfaceId] = new IOPCBrowseServerDispatcher(new DefaultBrowse(addressSpace)),
                [IOPCItemProperties.InterfaceId] = new IOPCItemPropertiesServerDispatcher(new DefaultItemProperties(NullItemPropertyProvider.Instance)),
            };
        }
    }

    private sealed class TestLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull =>
            null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Warning;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            Console.WriteLine(formatter(state, exception));
            if (exception is not null)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
