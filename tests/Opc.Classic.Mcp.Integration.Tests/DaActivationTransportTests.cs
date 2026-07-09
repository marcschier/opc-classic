// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Ndr;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;
using RemoteCreateInstanceRequest = Opc.Classic.Dcom.Core.RemoteCreateInstanceRequest;
using RemoteCreateInstanceResponse = Opc.Classic.Dcom.Core.RemoteCreateInstanceResponse;

namespace Opc.Classic.Mcp.Integration.Tests;

/// <summary>
/// Verifies the simulation's modern DCOM cold-activation endpoint is hosted and reachable, and
/// that it enforces the activation security gate. A full authenticated cold-activation
/// (RemoteActivation → object call) additionally requires server-side NTLM bind handling on the
/// managed listener, which is not yet implemented (see <c>F4Auth</c> / <c>NtlmHandshakeProtocolTests</c>),
/// so an anonymous activation is correctly denied here. The activation handler itself
/// (<see cref="SimulationActivationServer" />) registers the DA generated dispatchers in the
/// shared object registry and returns the activated IPID — the modern (non-reflection) path that
/// becomes a working cold-activation once authenticated transport lands.
/// </summary>
public sealed class DaActivationTransportTests
{
    private static readonly Guid SimDaClsid = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001");

    [Test]
    public async Task Modern_activation_endpoint_is_hosted_and_denies_anonymous_activation()
    {
        var model = new SimulatedPlantModel();
        await using SimulationActivationHost host = SimulationActivationHost.Create(
            model,
            SimDaClsid,
            "127.0.0.1:0",
            NullLoggerFactory.Instance);
        await host.StartAsync().ConfigureAwait(false);

        IPEndPoint endpoint = host.Endpoint ?? throw new InvalidOperationException("No activation endpoint.");

        await using DcomCallChannel activationChannel = await ConnectAsync(endpoint, objectIpid: null, ActivationServer.InterfaceId).ConfigureAwait(false);
        var request = new RemoteActivationRequest(
            Clsid: SimDaClsid,
            RequestedIids: new[] { IOPCServer.InterfaceId },
            ClientImpLevel: 3,
            Mode: 0,
            RequestedProtocolSequences: new ushort[] { 0x07 });
        byte[] payload = IActivationCodec.EncodeRemoteActivationRequest(request);

        NdrCallResult result = await activationChannel.InvokeAsync(
            ActivationServer.InterfaceId,
            0,
            payload,
            CancellationToken.None).ConfigureAwait(false);

        // The endpoint is live and the IActivation security gate denies the anonymous call
        // (authenticated activation requires server-side NTLM bind handling — a core follow-up).
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.AccessDenied.Code);
    }

    [Test]
    public async Task Activation_handler_registers_routable_da_object_and_returns_ipid()
    {
        var model = new SimulatedPlantModel();
        var registry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, registry);
        var activationServer = new SimulationActivationServer(SimDaClsid, daServer, registry);

        RemoteActivationResponse response = await activationServer.RemoteActivationAsync(
            new RemoteActivationRequest(
                Clsid: SimDaClsid,
                RequestedIids: new[] { IOPCServer.InterfaceId },
                ClientImpLevel: 3,
                Mode: 0,
                RequestedProtocolSequences: new ushort[] { 0x07 }),
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.IpidRemUnknown).IsNotEqualTo(Guid.Empty);
        // The activated IPID is registered for IOPCServer, so object calls route to the DA server.
        await Assert.That(registry.ContainsInterface(IOPCServer.InterfaceId)).IsTrue();

        // The response must carry a real OBJREF that the production client decode path consumes
        // (OpcEnumClient reads InterfaceResults[0].ObjRef, never IpidRemUnknown). Decode it the
        // same way and verify it points at the registered IOPCServer IPID.
        await Assert.That(response.InterfaceResults.Count).IsEqualTo(1);
        RemoteActivationInterfaceResult interfaceResult = response.InterfaceResults[0];
        await Assert.That(interfaceResult.Hresult).IsEqualTo(0);
        await Assert.That(interfaceResult.ObjRef.IsEmpty).IsFalse();

        var reader = new NdrReader(interfaceResult.ObjRef.Span);
        IOpcInterfaceRef objRef = OpcInterfaceRefCodec.Read(ref reader);
        await Assert.That(objRef.Iid).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(objRef.Ipid).IsNotEqualTo(response.IpidRemUnknown);
        await Assert.That(registry.Contains(objRef.Ipid)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.IpidRemUnknown, RemUnknownServerDispatcher.InterfaceId, out _)).IsTrue();
    }

    [Test]
    public async Task RemoteCreateInstance_registers_root_interfaces_remunknown_and_bindings()
    {
        var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 24680);
        var model = new SimulatedPlantModel();
        var registry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, registry);
        var activationServer = new SimulationActivationServer(
            SimDaClsid,
            daServer,
            registry,
            endpointProvider: () => endpoint);

        RemoteCreateInstanceResponse response = await activationServer.RemoteCreateInstanceAsync(
            new RemoteCreateInstanceRequest(SimDaClsid, IOPCServer.InterfaceId, [0x07]),
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.IpidRemUnknown).IsNotEqualTo(Guid.Empty);
        await Assert.That(registry.TryGetDispatcher(response.IpidRemUnknown, RemUnknownServerDispatcher.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.Contains(response.Ipid)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.Ipid, IOPCServer.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.Ipid, IOPCCommon.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.Ipid, IOPCBrowse.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.Ipid, IOPCBrowseServerAddressSpace.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.Ipid, IOPCItemProperties.InterfaceId, out _)).IsTrue();

        var reader = new NdrReader(response.ObjRef);
        IOpcInterfaceRef objRef = OpcInterfaceRefCodec.Read(ref reader);
        await Assert.That(objRef.Iid).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(objRef.Ipid).IsEqualTo(response.Ipid);
        await Assert.That(objRef.Oxid).IsNotEqualTo(0UL);
        await Assert.That(objRef.Oid).IsNotEqualTo(0UL);
        (ushort[] bindings, ushort securityOffset) = DecodeDualStringArray(response.OxidBindings);
        await Assert.That(bindings.Length).IsGreaterThan(0);
        await Assert.That(ReadStringBinding(bindings)).IsEqualTo("127.0.0.1[24680]");
        await Assert.That(objRef.ResolverBindings.Count).IsEqualTo(bindings.Length);
        await Assert.That(objRef.SecurityOffset).IsEqualTo(securityOffset);
    }

    [Test]
    public async Task RemoteCreateInstance_for_opcenum_registers_server_list_interfaces()
    {
        var model = new SimulatedPlantModel();
        var registry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, registry);
        var activationServer = new SimulationActivationServer(SimDaClsid, daServer, registry);

        RemoteCreateInstanceResponse response = await activationServer.RemoteCreateInstanceAsync(
            new RemoteCreateInstanceRequest(OpcGuids.CLSID_OpcEnum, OpcGuids.IID_IOPCServerList2, [0x07]),
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.IpidRemUnknown).IsNotEqualTo(Guid.Empty);
        await Assert.That(registry.TryGetDispatcher(response.IpidRemUnknown, RemUnknownServerDispatcher.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.Ipid, OpcGuids.IID_IOPCServerList, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(response.Ipid, OpcGuids.IID_IOPCServerList2, out _)).IsTrue();

        var reader = new NdrReader(response.ObjRef);
        IOpcInterfaceRef objRef = OpcInterfaceRefCodec.Read(ref reader);
        await Assert.That(objRef.Iid).IsEqualTo(OpcGuids.IID_IOPCServerList2);
        await Assert.That(objRef.Ipid).IsEqualTo(response.Ipid);
    }

    [Test]
    public async Task Activation_handler_returns_non_empty_oxid_bindings_for_listener_endpoint()
    {
        var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 24680);
        var remUnknownIpid = new Guid("12345678-1234-1234-1234-1234567890ab");
        var model = new SimulatedPlantModel();
        var registry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, registry);
        var activationServer = new SimulationActivationServer(
            SimDaClsid,
            daServer,
            registry,
            endpointProvider: () => endpoint,
            remUnknownIpid: remUnknownIpid);

        RemoteActivationResponse response = await activationServer.RemoteActivationAsync(
            new RemoteActivationRequest(
                Clsid: SimDaClsid,
                RequestedIids: new[] { IOPCServer.InterfaceId },
                ClientImpLevel: 3,
                Mode: 0,
                RequestedProtocolSequences: new ushort[] { 0x07 }),
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.IpidRemUnknown).IsEqualTo(remUnknownIpid);
        (ushort[] bindings, ushort securityOffset) = DecodeDualStringArray(response.OxidBindings.Span);
        await Assert.That(bindings.Length).IsGreaterThan(0);
        await Assert.That(bindings[0]).IsEqualTo((ushort)0x07);
        await Assert.That(securityOffset).IsGreaterThan((ushort)0);
        await Assert.That(ReadStringBinding(bindings)).IsEqualTo("127.0.0.1[24680]");

        var reader = new NdrReader(response.InterfaceResults[0].ObjRef.Span);
        IOpcInterfaceRef objRef = OpcInterfaceRefCodec.Read(ref reader);
        await Assert.That(objRef.ResolverBindings.Count).IsEqualTo(bindings.Length);
        await Assert.That(objRef.SecurityOffset).IsEqualTo(securityOffset);
    }

    [Test]
    public async Task Wrong_clsid_activation_returns_class_not_registered_with_per_iid_results()
    {
        var model = new SimulatedPlantModel();
        var registry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, registry);
        var activationServer = new SimulationActivationServer(SimDaClsid, daServer, registry);

        RemoteActivationResponse response = await activationServer.RemoteActivationAsync(
            new RemoteActivationRequest(
                Clsid: new Guid("11111111-2222-3333-4444-555555555555"),
                RequestedIids: new[] { IOPCServer.InterfaceId },
                ClientImpLevel: 3,
                Mode: 0,
                RequestedProtocolSequences: new ushort[] { 0x07 }),
            CancellationToken.None).ConfigureAwait(false);

        // REGDB_E_CLASSNOTREG, with one zeroed per-IID result so the client decode succeeds
        // (MS-DCOM §3.1.2.5.2.3.1: pResults carries one entry per requested IID even on failure).
        await Assert.That(response.Hresult).IsEqualTo(unchecked((int)0x80040154u));
        await Assert.That(response.InterfaceResults.Count).IsEqualTo(1);
    }

    [Test]
    public async Task RemoteCreateInstance_wrong_clsid_returns_class_not_registered_without_objref()
    {
        var model = new SimulatedPlantModel();
        var registry = new OpcObjectRegistry();
        var daServer = new SimDaHostServer(model, registry);
        var activationServer = new SimulationActivationServer(SimDaClsid, daServer, registry);

        RemoteCreateInstanceResponse response = await activationServer.RemoteCreateInstanceAsync(
            new RemoteCreateInstanceRequest(new Guid("11111111-2222-3333-4444-555555555555"), IOPCServer.InterfaceId, [0x07]),
            CancellationToken.None).ConfigureAwait(false);

        await Assert.That(response.Hresult).IsEqualTo(unchecked((int)0x80040154u));
        await Assert.That(response.Ipid).IsEqualTo(Guid.Empty);
        await Assert.That(response.ObjRef.Length).IsEqualTo(0);
        await Assert.That(registry.TryGetDispatcher(response.IpidRemUnknown, RemUnknownServerDispatcher.InterfaceId, out _)).IsTrue();
    }

    private static async Task<DcomCallChannel> ConnectAsync(IPEndPoint endpoint, Guid? objectIpid, Guid preBindIid)
    {
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            CancellationToken.None).ConfigureAwait(false);
        Guid[] preBind = [preBindIid];
        return objectIpid is { } ipid
            ? new DcomCallChannel(transport, new NoOpAuthContext(), ipid, preBind)
            : new DcomCallChannel(transport, new NoOpAuthContext(), preBind);
    }

    private static (ushort[] Bindings, ushort SecurityOffset) DecodeDualStringArray(ReadOnlySpan<byte> dualStringArray)
    {
        var reader = new NdrReader(dualStringArray);
        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        var bindings = new ushort[entryCount];
        for (int i = 0; i < bindings.Length; i++)
        {
            bindings[i] = reader.ReadUInt16();
        }

        return (bindings, securityOffset);
    }

    private static string ReadStringBinding(ushort[] bindings)
    {
        var chars = new char[bindings.Length - 1];
        int count = 0;
        for (int i = 1; i < bindings.Length && bindings[i] != 0; i++)
        {
            chars[count++] = (char)bindings[i];
        }

        return new string(chars, 0, count);
    }
}
