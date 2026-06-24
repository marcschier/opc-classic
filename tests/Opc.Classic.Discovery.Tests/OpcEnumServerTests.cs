// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Discovery.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Discovery.Tests;

public sealed class OpcEnumServerTests
{
    private static readonly Guid DaClsid = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001");
    private static readonly Guid AeClsid = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0002");
    private static readonly Guid HdaClsid = new("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0003");

    [Test]
    public async Task ServerList2_enumerates_registered_opc_categories_with_routable_enum()
    {
        var objectRegistry = new OpcObjectRegistry();
        var server = new OpcEnumServer(CreateRegistry(), objectRegistry);
        var dispatcher = new IOPCServerList2ServerDispatcher(server);

        DispatchResult enumResult = await dispatcher.DispatchAsync(
            3,
            EncodeCategoryRequest([OpcGuids.CATID_OPCDAServer20], []),
            CancellationToken.None);

        await Assert.That(enumResult.Hresult).IsEqualTo(0);
        IOpcInterfaceRef enumRef;
        {
            var refReader = new NdrReader(enumResult.Payload.Span);
            enumRef = OpcInterfaceRefCodec.Read(ref refReader);
        }
        await Assert.That(enumRef.Iid).IsEqualTo(OpcGuids.IID_IOPCEnumGUID);
        await Assert.That(objectRegistry.TryGetDispatcher(enumRef.Ipid, OpcGuids.IID_IOPCEnumGUID, out IOpcServerDispatcher? enumDispatcher)).IsTrue();

        DispatchResult nextResult = await enumDispatcher!.DispatchAsync(3, EncodeCount(8), CancellationToken.None);
        Guid[] classIds;
        int fetched;
        {
            var nextReader = new NdrReader(nextResult.Payload.Span);
            classIds = nextReader.ReadVaryingConformantGuidArray();
            fetched = nextReader.ReadInt32();
        }

        await Assert.That(nextResult.Hresult).IsEqualTo(1);
        await Assert.That(fetched).IsEqualTo(1);
        await Assert.That(classIds).IsEquivalentTo(new[] { DaClsid });
    }

    [Test]
    public async Task ServerList2_returns_class_details_and_clsid_from_progid()
    {
        var dispatcher = new IOPCServerList2ServerDispatcher(new OpcEnumServer(CreateRegistry(), new OpcObjectRegistry()));

        DispatchResult detailsResult = await dispatcher.DispatchAsync(4, EncodeGuid(AeClsid), CancellationToken.None);
        string? progId;
        string? userType;
        string? versionIndependentProgId;
        {
            var detailsReader = new NdrReader(detailsResult.Payload.Span);
            progId = detailsReader.ReadUnicodeStringPtr();
            userType = detailsReader.ReadUnicodeStringPtr();
            versionIndependentProgId = detailsReader.ReadUnicodeStringPtr();
        }

        DispatchResult clsidResult = await dispatcher.DispatchAsync(5, EncodeString("Opc.Classic.Simulation.HDA.1"), CancellationToken.None);
        Guid clsid;
        {
            var clsidReader = new NdrReader(clsidResult.Payload.Span);
            clsid = clsidReader.ReadGuid();
        }

        await Assert.That(detailsResult.Hresult).IsEqualTo(0);
        await Assert.That(progId).IsEqualTo("Opc.Classic.Simulation.AE.1");
        await Assert.That(userType).IsEqualTo("Opc.Classic Full-Feature Simulation Server (AE)");
        await Assert.That(versionIndependentProgId).IsEqualTo("Opc.Classic.Simulation.AE");
        await Assert.That(clsidResult.Hresult).IsEqualTo(0);
        await Assert.That(clsid).IsEqualTo(HdaClsid);
    }

    [Test]
    public async Task ServerList2_enumerates_da_ae_and_hda_categories()
    {
        var objectRegistry = new OpcObjectRegistry();
        var dispatcher = new IOPCServerList2ServerDispatcher(new OpcEnumServer(CreateRegistry(), objectRegistry));

        Guid[] da = await EnumerateAsync(dispatcher, objectRegistry, OpcGuids.CATID_OPCDAServer30);
        Guid[] ae = await EnumerateAsync(dispatcher, objectRegistry, OpcGuids.CATID_OPCAEServer10);
        Guid[] hda = await EnumerateAsync(dispatcher, objectRegistry, OpcGuids.CATID_OPCHDAServer10);

        await Assert.That(da).IsEquivalentTo(new[] { DaClsid });
        await Assert.That(ae).IsEquivalentTo(new[] { AeClsid });
        await Assert.That(hda).IsEquivalentTo(new[] { HdaClsid });
    }

    private static async Task<Guid[]> EnumerateAsync(
        IOPCServerList2ServerDispatcher dispatcher,
        OpcObjectRegistry objectRegistry,
        Guid categoryId)
    {
        DispatchResult enumResult = await dispatcher.DispatchAsync(3, EncodeCategoryRequest([categoryId], []), CancellationToken.None);
        IOpcInterfaceRef enumRef;
        {
            var refReader = new NdrReader(enumResult.Payload.Span);
            enumRef = OpcInterfaceRefCodec.Read(ref refReader);
        }
        if (!objectRegistry.TryGetDispatcher(enumRef.Ipid, OpcGuids.IID_IOPCEnumGUID, out IOpcServerDispatcher? enumDispatcher))
        {
            throw new InvalidOperationException("Enumerator was not registered.");
        }

        DispatchResult nextResult = await enumDispatcher.DispatchAsync(3, EncodeCount(8), CancellationToken.None);
        {
            var nextReader = new NdrReader(nextResult.Payload.Span);
            return nextReader.ReadVaryingConformantGuidArray();
        }
    }

    private static InMemoryClsidRegistry CreateRegistry() => new(
    [
        new OpcClsidRegistration(
            DaClsid,
            "Opc.Classic.Simulation.DA.1",
            "Opc.Classic.Samples.SimulationServer",
            "SimDaHostServer",
            "Opc.Classic Full-Feature Simulation Server (DA)",
            [OpcGuids.CATID_OPCDAServer20, OpcGuids.CATID_OPCDAServer30]),
        new OpcClsidRegistration(
            AeClsid,
            "Opc.Classic.Simulation.AE.1",
            "Opc.Classic.Samples.SimulationServer",
            "SimAeHostServer",
            "Opc.Classic Full-Feature Simulation Server (AE)",
            [OpcGuids.CATID_OPCAEServer10]),
        new OpcClsidRegistration(
            HdaClsid,
            "Opc.Classic.Simulation.HDA.1",
            "Opc.Classic.Samples.SimulationServer",
            "SimHdaHostServer",
            "Opc.Classic Full-Feature Simulation Server (HDA)",
            [OpcGuids.CATID_OPCHDAServer10]),
    ]);

    private static byte[] EncodeCategoryRequest(Guid[] implementedCategories, Guid[] requiredCategories) =>
        WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32((uint)implementedCategories.Length);
            writer.WriteConformantGuidArray(implementedCategories);
            writer.WriteUInt32((uint)requiredCategories.Length);
            writer.WriteConformantGuidArray(requiredCategories);
        });

    private static byte[] EncodeCount(int count) =>
        WritePayload((ref NdrWriter writer) => writer.WriteInt32(count));

    private static byte[] EncodeGuid(Guid value) =>
        WritePayload((ref NdrWriter writer) => writer.WriteGuid(value));

    private static byte[] EncodeString(string value) =>
        WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(value));

    private static byte[] WritePayload(NdrWriteAction action)
    {
        var buffer = new byte[2048];
        var writer = new NdrWriter(buffer);
        action(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
