// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Dcom.Tests.Activation;

/// <summary>
/// Focused wire-level tests for the modern <see cref="RemoteSCMActivatorDispatcher" />
/// (IRemoteSCMActivator opnums 3/4): opnum routing, the authenticated-and-integrity activation
/// gate, request NDR decode, response shaping, and malformed-input handling. The end-to-end
/// activation flow is covered separately by ManagedDcomFullStackE2ETests / DaActivationTransportTests;
/// this isolates the dispatcher edges that those happy-path tests do not exercise.
/// </summary>
public sealed class RemoteSCMActivatorDispatcherTests
{
    private static readonly Guid Clsid = new("d9a0b0c1-5e21-49c7-9c0e-2d7b6a1f0001");
    private static readonly Guid RequestedIid = new("39c13a4d-011e-11d0-9675-0020afd8adb3"); // IID_IOPCServer
    private static readonly byte[] ObjRefBytes = [0x4D, 0x45, 0x4F, 0x57, 0x01, 0x02, 0x03, 0x04];

    private const int EInvalidArg = unchecked((int)0x80070057u);
    private const int ENoInterface = unchecked((int)0x80004002u);

    [Test]
    public async Task Unknown_opnum_is_not_routed_to_the_activator()
    {
        var activator = new RecordingActivator(ObjRefBytes);
        var dispatcher = new RemoteSCMActivatorDispatcher(activator);

        await dispatcher.DispatchAsync(5, BuildActivationRequest(Clsid, RequestedIid), CancellationToken.None);

        await Assert.That(activator.CreateInstanceCalls).IsEqualTo(0);
        await Assert.That(activator.GetClassObjectCalls).IsEqualTo(0);
    }

    [Test]
    public async Task Unauthenticated_create_instance_is_denied()
    {
        var activator = new RecordingActivator(ObjRefBytes);
        var dispatcher = new RemoteSCMActivatorDispatcher(activator);

        DispatchResult result = await dispatcher.DispatchAsync(
            4, BuildActivationRequest(Clsid, RequestedIid), CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.AccessDenied.Code);
        await Assert.That(activator.CreateInstanceCalls).IsEqualTo(0);
    }

    [Test]
    public async Task Authenticated_but_below_integrity_is_denied()
    {
        var activator = new RecordingActivator(ObjRefBytes);
        var dispatcher = new RemoteSCMActivatorDispatcher(activator);

        DispatchResult result = await ((IRpcRequestContextDispatcher)dispatcher).DispatchAsync(
            4,
            BuildActivationRequest(Clsid, RequestedIid),
            Context(OpcProtectionLevel.Connect),
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.AccessDenied.Code);
        await Assert.That(activator.CreateInstanceCalls).IsEqualTo(0);
    }

    [Test]
    public async Task Authenticated_integrity_create_instance_routes_and_returns_objref()
    {
        var activator = new RecordingActivator(ObjRefBytes);
        var dispatcher = new RemoteSCMActivatorDispatcher(activator);

        DispatchResult result = await ((IRpcRequestContextDispatcher)dispatcher).DispatchAsync(
            4,
            BuildActivationRequest(Clsid, RequestedIid),
            Context(OpcProtectionLevel.Integrity),
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(activator.CreateInstanceCalls).IsEqualTo(1);
        await Assert.That(activator.LastCreateRequest!.Clsid).IsEqualTo(Clsid);
        await Assert.That(activator.LastCreateRequest!.RequestedIid).IsEqualTo(RequestedIid);

        (int hresult, byte[] objRef) = DecodeResponse(result.Payload.Span);
        await Assert.That(hresult).IsEqualTo(0);
        await Assert.That(objRef.SequenceEqual(ObjRefBytes)).IsTrue();
    }

    [Test]
    public async Task Modern_create_instance_round_trips_two_iids_through_activation_client()
    {
        Guid optionalIid = new("f31dfde2-07b6-11d2-b2d8-0060083ba1fb");
        var activator = new RecordingActivator(ObjRefBytes);
        var dispatcher = new RemoteSCMActivatorDispatcher(activator);
        var channel = new InMemoryCallChannelBuilder()
            .Register(RemoteSCMActivatorDispatcher.InterfaceId, 4, async (_, _, payload, cancellationToken) =>
            {
                DispatchResult result = await ((IRpcRequestContextDispatcher)dispatcher).DispatchAsync(
                    4,
                    payload,
                    Context(OpcProtectionLevel.Integrity),
                    cancellationToken).ConfigureAwait(false);
                return new NdrCallResult(result.Hresult, result.Payload);
            })
            .Build();
        var client = new ActivationClient(channel);

        ActivationPropertiesOutData response = await client.RemoteCreateInstanceAsync(
            Clsid,
            new[] { "ncacn_ip_tcp" },
            new[] { RequestedIid, optionalIid });

        await Assert.That(activator.LastCreateRequest!.RequestedIids.Count).IsEqualTo(2);
        await Assert.That(activator.LastCreateRequest!.RequestedIids[0]).IsEqualTo(RequestedIid);
        await Assert.That(activator.LastCreateRequest!.RequestedIids[1]).IsEqualTo(optionalIid);
        await Assert.That(response.InterfaceResults.Count).IsEqualTo(2);
        await Assert.That(response.InterfaceResults[0].Iid).IsEqualTo(RequestedIid);
        await Assert.That(response.InterfaceResults[0].Hresult).IsEqualTo(0);
        await Assert.That(response.InterfaceResults[0].ObjRef.SequenceEqual(ObjRefBytes)).IsTrue();
        await Assert.That(response.InterfaceResults[1].Iid).IsEqualTo(optionalIid);
        await Assert.That(response.InterfaceResults[1].Hresult).IsEqualTo(ENoInterface);
        await Assert.That(response.InterfaceResults[1].ObjRef.Length).IsEqualTo(0);
    }

    [Test]
    public async Task Authenticated_integrity_get_class_object_routes_to_opnum_three()
    {
        var activator = new RecordingActivator(ObjRefBytes);
        var dispatcher = new RemoteSCMActivatorDispatcher(activator);

        DispatchResult result = await ((IRpcRequestContextDispatcher)dispatcher).DispatchAsync(
            3,
            BuildActivationRequest(Clsid, RequestedIid),
            Context(OpcProtectionLevel.Integrity),
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(activator.GetClassObjectCalls).IsEqualTo(1);
        await Assert.That(activator.CreateInstanceCalls).IsEqualTo(0);

        (int hresult, byte[] objRef) = DecodeResponse(result.Payload.Span);
        await Assert.That(hresult).IsEqualTo(0);
        await Assert.That(objRef.SequenceEqual(ObjRefBytes)).IsTrue();
    }

    [Test]
    public async Task Malformed_request_body_is_faulted_without_invoking_the_activator()
    {
        var activator = new RecordingActivator(ObjRefBytes);
        var dispatcher = new RemoteSCMActivatorDispatcher(activator);

        // Truncated: shorter than the two required GUIDs, so the NDR decode throws and is mapped to E_INVALIDARG.
        DispatchResult result = await ((IRpcRequestContextDispatcher)dispatcher).DispatchAsync(
            4,
            new byte[] { 0x01, 0x02, 0x03, 0x04 },
            Context(OpcProtectionLevel.Integrity),
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(EInvalidArg);
        await Assert.That(activator.CreateInstanceCalls).IsEqualTo(0);
    }

    private static RpcRequestContext Context(OpcProtectionLevel protectionLevel) =>
        new(
            IsAuthenticated: true,
            IsEstablished: true,
            ProtectionLevel: protectionLevel,
            RemoteEndpoint: new IPEndPoint(IPAddress.Loopback, 5001));

    private static byte[] BuildActivationRequest(Guid clsid, Guid requestedIid)
    {
        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        writer.WriteGuid(clsid);
        writer.WriteGuid(requestedIid);
        writer.WriteUInt32(1);       // protocol-sequence count
        writer.WriteInt32(0x07);     // ncacn_ip_tcp
        writer.WriteUInt32(0);       // activation-properties length
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static (int Hresult, byte[] ObjRef) DecodeResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        int hresult = reader.ReadInt32();
        uint length = reader.ReadUInt32();
        byte[] objRef = reader.ReadRawBytes(checked((int)length)).ToArray();
        return (hresult, objRef);
    }

    private sealed class RecordingActivator : IRemoteSCMActivatorServer
    {
        private readonly byte[] _objRef;

        public RecordingActivator(byte[] objRef) => _objRef = objRef;

        public int CreateInstanceCalls { get; private set; }
        public int GetClassObjectCalls { get; private set; }
        public RemoteCreateInstanceRequest? LastCreateRequest { get; private set; }
        public RemoteGetClassObjectRequest? LastGetClassRequest { get; private set; }

        public Task<RemoteCreateInstanceResponse> RemoteCreateInstanceAsync(
            RemoteCreateInstanceRequest request,
            CancellationToken cancellationToken = default)
        {
            CreateInstanceCalls++;
            LastCreateRequest = request;
            Guid[] requestedIids = request.RequestedIids.Count == 0 ? new[] { request.RequestedIid } : request.RequestedIids.ToArray();
            var interfaceResults = new ActivationInterfaceResult[requestedIids.Length];
            for (int i = 0; i < interfaceResults.Length; i++)
            {
                bool isPrimary = i == 0;
                interfaceResults[i] = new ActivationInterfaceResult(
                    requestedIids[i],
                    isPrimary ? 0 : ENoInterface,
                    isPrimary ? _objRef : Array.Empty<byte>());
            }

            return Task.FromResult(new RemoteCreateInstanceResponse(0, Guid.NewGuid(), Guid.NewGuid(), _objRef)
            {
                OxidValue = 0x0102030405060708,
                IpidRemUnknown = new Guid("11111111-2222-3333-4444-555555555555"),
                OxidBindings = CreateDualStringArray(),
                InterfaceResults = interfaceResults,
            });
        }

        public Task<RemoteGetClassObjectResponse> RemoteGetClassObjectAsync(
            RemoteGetClassObjectRequest request,
            CancellationToken cancellationToken = default)
        {
            GetClassObjectCalls++;
            LastGetClassRequest = request;
            return Task.FromResult(new RemoteGetClassObjectResponse(0, Guid.NewGuid(), Guid.NewGuid(), _objRef));
        }

        public Task<int> RemoteGetClassObjectAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);

        public Task<int> RemoteCreateInstanceAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

    private static byte[] CreateDualStringArray() =>
    [
        0x02, 0x00,
        0x01, 0x00,
        0x07, 0x00,
        0x00, 0x00,
    ];
}
