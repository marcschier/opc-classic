// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ndr;
using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Tests.Activation;

public sealed class RemoteSCMActivatorTests
{
    private const int CO_E_CLASSSTRING = unchecked((int)0x800401F3u);
    private static readonly Guid IidIClassFactory = Guid.Parse("00000001-0000-0000-C000-000000000046");

    [Test]
    public async Task RemoteCreateInstance_registered_factory_returns_standard_objref()
    {
        Guid clsid = Guid.NewGuid();
        Guid iid = Guid.NewGuid();
        var registry = new ClassFactoryRegistry();
        registry.Register(clsid, context =>
        {
            var definition = new LocalInterfaceDefinition(context.RequestedIid.ToString(), isDispInterface: false);
            return new ClassFactoryActivationResult(new TestServer(), definition);
        });
        var server = new RemoteSCMActivatorServer(registry);
        ActivationProperties properties = CreateActivationProperties(clsid, iid);

        RemoteCreateInstanceResponse response = await server.RemoteCreateInstanceAsync(
            new RemoteCreateInstanceRequest(clsid, iid, [7])
            {
                RawActivationProperties = ActivationInfoCodec.Encode(properties),
            });

        IOpcInterfaceRef objRef = DecodeObjRef(response.ObjRef);
        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.Oxid).IsNotEqualTo(Guid.Empty);
        await Assert.That(response.Oid).IsNotEqualTo(Guid.Empty);
        await Assert.That(response.Ipid).IsNotEqualTo(Guid.Empty);
        await Assert.That(response.ObjRef.Length).IsGreaterThan(0);
        await Assert.That(objRef.Iid).IsEqualTo(iid);
        await Assert.That(objRef.Oxid).IsNotEqualTo(0UL);
        await Assert.That(objRef.Oid).IsNotEqualTo(0UL);
        await Assert.That(objRef.Ipid).IsNotEqualTo(Guid.Empty);
        await Assert.That(objRef.ResolverBindings.Count).IsGreaterThan(0);
        await Assert.That(response.ActivationProperties.ScmReplyInfo).IsNotNull();
        await Assert.That(response.EncodedActivationProperties.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task RemoteCreateInstance_unknown_clsid_returns_CO_E_CLASSSTRING()
    {
        var server = new RemoteSCMActivatorServer(new ClassFactoryRegistry());
        var request = new RemoteCreateInstanceRequest(Guid.NewGuid(), Guid.NewGuid(), [7]);

        RemoteCreateInstanceResponse response = await server.RemoteCreateInstanceAsync(request);

        await Assert.That(response.Hresult).IsEqualTo(CO_E_CLASSSTRING);
        await Assert.That(response.ObjRef.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RemoteGetClassObject_registered_factory_returns_class_factory_objref()
    {
        Guid clsid = Guid.NewGuid();
        var registry = new ClassFactoryRegistry();
        registry.Register(clsid, _ => new TestServer());
        var server = new RemoteSCMActivatorServer(registry);

        RemoteGetClassObjectResponse response = await server.RemoteGetClassObjectAsync(
            new RemoteGetClassObjectRequest(clsid, IidIClassFactory, [7]));

        IOpcInterfaceRef objRef = DecodeObjRef(response.ObjRef);
        await Assert.That(response.Hresult).IsEqualTo(0);
        await Assert.That(response.Oxid).IsNotEqualTo(Guid.Empty);
        await Assert.That(response.Oid).IsNotEqualTo(Guid.Empty);
        await Assert.That(response.Ipid).IsNotEqualTo(Guid.Empty);
        await Assert.That(objRef.Iid).IsEqualTo(IidIClassFactory);
        await Assert.That(objRef.Oxid).IsNotEqualTo(0UL);
        await Assert.That(objRef.Oid).IsNotEqualTo(0UL);
        await Assert.That(objRef.ResolverBindings.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task Activation_properties_round_trip_all_activation_sub_properties()
    {
        Guid clsid = Guid.NewGuid();
        Guid iid = Guid.NewGuid();
        var replyObjRef = new byte[] { 0x4d, 0x45, 0x4f, 0x57 };
        var properties = new ActivationProperties(
            new SpecialPropertiesData(ActivationComVersion.V5_6, Mode: 1, ClassContext: 4, iid, [10, 20]),
            new InstanceInfo(clsid, iid, ClassContext: 4, Mode: 1),
            new LocationInfo("server-a", 1234, [7, 9]),
            new ScmReplyInfo(0, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), replyObjRef, copy: true),
            new SecurityInfo(AuthenticationLevel: 6, ImpersonationLevel: 3, Capabilities: 0x40));

        ActivationProperties decoded = ActivationInfoCodec.Decode(ActivationInfoCodec.Encode(properties));

        await Assert.That(decoded.SpecialProperties.ClientVersion).IsEqualTo(ActivationComVersion.V5_6);
        await Assert.That(decoded.SpecialProperties.Mode).IsEqualTo(1);
        await Assert.That(decoded.SpecialProperties.ClassContext).IsEqualTo(4);
        await Assert.That(decoded.SpecialProperties.RequestedIid).IsEqualTo(iid);
        await Assert.That(decoded.SpecialProperties.SpecialProperties.Count).IsEqualTo(2);
        await Assert.That(decoded.InstanceInfo!.Clsid).IsEqualTo(clsid);
        await Assert.That(decoded.InstanceInfo.RequestedIid).IsEqualTo(iid);
        await Assert.That(decoded.LocationInfo!.MachineName).IsEqualTo("server-a");
        await Assert.That(decoded.LocationInfo.ProtocolSequences.Count).IsEqualTo(2);
        await Assert.That(decoded.ScmReplyInfo!.ObjRef.Length).IsEqualTo(replyObjRef.Length);
        await Assert.That(decoded.SecurityInfo!.AuthenticationLevel).IsEqualTo(6);
        await Assert.That(decoded.SecurityInfo.ImpersonationLevel).IsEqualTo(3);
        await Assert.That(decoded.SecurityInfo.Capabilities).IsEqualTo(0x40);
    }

    private static ActivationProperties CreateActivationProperties(Guid clsid, Guid iid) => new(
        new SpecialPropertiesData(ActivationComVersion.V5_6, Mode: 0, ClassContext: 4, iid, [1]),
        new InstanceInfo(clsid, iid, ClassContext: 4, Mode: 0),
        new LocationInfo(null, Environment.ProcessId, [7]),
        null,
        new SecurityInfo(AuthenticationLevel: 6, ImpersonationLevel: 3, Capabilities: 0));

    private static IOpcInterfaceRef DecodeObjRef(byte[] objRef)
    {
        var reader = new NdrReader(objRef);
        return OpcInterfaceRefCodec.Read(ref reader);
    }

    private sealed class TestServer
    {
    }
}
