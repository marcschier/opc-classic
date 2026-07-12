// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Activation;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Tests.Activation;

public sealed class ActivationPropertiesCodecTests
{
    private static readonly Guid TestClsid = new("B3AE5D6F-2A91-4F8B-9D2C-7E5B0C8F1A3E");
    private static readonly Guid IidOpcServer = new("39C13A4D-011E-11D0-9675-0020AFD8ADB3");

    [Test]
    public async Task RemoteCreateInstance_request_round_trips_activation_properties()
    {
        byte[] encoded = ActivationPropertiesCodec.EncodeRemoteCreateInstanceRequest(
            TestClsid,
            new[] { IidOpcServer },
            new ushort[] { 7 });

        var wireReader = new NdrReader(encoded);
        uint pUnkOuter = wireReader.ReadUInt32();
        uint pActProperties = wireReader.ReadUInt32();
        uint maxCount = wireReader.ReadUInt32();
        uint ulCntData = wireReader.ReadUInt32();
        await Assert.That(pUnkOuter).IsEqualTo(0u);
        await Assert.That(pActProperties).IsNotEqualTo(0u);
        await Assert.That(maxCount).IsEqualTo(ulCntData);
        await Assert.That(maxCount).IsGreaterThan(100u);

        RemoteCreateInstanceActivationRequest decoded =
            ActivationPropertiesCodec.DecodeRemoteCreateInstanceRequest(encoded);

        await Assert.That(decoded.ClassId).IsEqualTo(TestClsid);
        await Assert.That(decoded.RequestedIids.Count).IsEqualTo(1);
        await Assert.That(decoded.RequestedIids[0]).IsEqualTo(IidOpcServer);
        await Assert.That(decoded.RequestedProtocolSequences.Count).IsEqualTo(1);
        await Assert.That(decoded.RequestedProtocolSequences[0]).IsEqualTo((ushort)7);
        await Assert.That(decoded.ActivationPropertiesBlob.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task RemoteCreateInstance_response_round_trips_activation_properties_out()
    {
        byte[] objRef = CreateStandardObjRef(IidOpcServer);
        byte[] oxidBindings = { 4, 0, 2, 0, 7, 0, 0, 0, 0, 0, 0, 0 };
        var ipidRemUnknown = new Guid("11111111-2222-3333-4444-555555555555");

        byte[] encoded = ActivationPropertiesCodec.EncodeRemoteCreateInstanceResponse(
            0x0102030405060708,
            oxidBindings,
            ipidRemUnknown,
            authnHint: 6,
            serverVersion: (5, 7),
            new[] { new ActivationInterfaceResult(IidOpcServer, 0, objRef) });

        ActivationPropertiesOutData decoded =
            ActivationPropertiesCodec.DecodeRemoteCreateInstanceResponse(encoded);

        await Assert.That(decoded.Oxid).IsEqualTo(0x0102030405060708ul);
        await Assert.That(Convert.ToHexString(decoded.OxidBindings)).IsEqualTo(Convert.ToHexString(oxidBindings));
        await Assert.That(decoded.IpidRemUnknown).IsEqualTo(ipidRemUnknown);
        await Assert.That(decoded.AuthnHint).IsEqualTo(6u);
        await Assert.That(decoded.ServerVersion).IsEqualTo(((ushort)5, (ushort)7));
        await Assert.That(decoded.InterfaceResults.Count).IsEqualTo(1);
        await Assert.That(decoded.InterfaceResults[0].Iid).IsEqualTo(IidOpcServer);
        await Assert.That(decoded.InterfaceResults[0].Hresult).IsEqualTo(0);
        await Assert.That(Convert.ToHexString(decoded.InterfaceResults[0].ObjRef)).IsEqualTo(Convert.ToHexString(objRef));
    }

    private static byte[] CreateStandardObjRef(Guid iid)
    {
        var interfaceRef = new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 5,
            oxid: 0x0102030405060708,
            oid: 0x1112131415161718,
            ipid: new Guid("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"),
            securityOffset: 2,
            resolverBindings: new ushort[] { 4, 2, 7, 0 });
        var buffer = new byte[256];
        var writer = new NdrWriter(buffer);
        OpcInterfaceRefCodec.Write(ref writer, interfaceRef);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }
}
