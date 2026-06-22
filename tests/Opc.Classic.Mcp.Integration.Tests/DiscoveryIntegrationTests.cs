// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Samples.SimulationServer.Discovery;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class DiscoveryIntegrationTests
{
    [Test]
    public async Task Discovery_enumerates_da20_server_against_full_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);

        OpcServerDescriptorDto[] descriptors = await EnumerateServersAsync(host, OpcGuids.CATID_OPCDAServer20).ConfigureAwait(false);

        await Assert.That(descriptors.Length).IsEqualTo(1);
        OpcServerDescriptorDto descriptor = descriptors.Single(static descriptor =>
            descriptor.ClassId == new Guid("10138C2C-0000-0000-0000-00000000DA20"));
        await Assert.That(descriptor.ProgId).IsEqualTo("Opc.Classic.Simulation.DA.1");
        await Assert.That(descriptor.UserType).IsEqualTo("Opc.Classic Simulation DA Server");
        await Assert.That(descriptor.VerIndProgId).IsEqualTo("Opc.Classic.Simulation.DA");
        await Assert.That(descriptor.Host).IsEqualTo(SimDiscoveryModule.DiscoveryHost);
        await Assert.That(descriptor.Categories.Any(category => category == OpcGuids.CATID_OPCDAServer20)).IsTrue();
    }

    [Test]
    public async Task Discovery_enumerates_ae_and_hda_servers_against_full_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);

        OpcServerDescriptorDto[] descriptors = await EnumerateServersAsync(
            host,
            OpcGuids.CATID_OPCAEServer10,
            OpcGuids.CATID_OPCHDAServer10).ConfigureAwait(false);

        await Assert.That(descriptors.Length).IsEqualTo(2);
        OpcServerDescriptorDto ae = descriptors.Single(static descriptor =>
            descriptor.ClassId == new Guid("10138C2C-0000-0000-0000-00000000AE10"));
        await Assert.That(ae.ProgId).IsEqualTo("Opc.Classic.Simulation.AE.1");
        await Assert.That(ae.UserType).IsEqualTo("Opc.Classic Simulation AE Server");
        await Assert.That(ae.VerIndProgId).IsEqualTo("Opc.Classic.Simulation.AE");
        await Assert.That(ae.Host).IsEqualTo(SimDiscoveryModule.DiscoveryHost);
        await Assert.That(ae.Categories.Any(category => category == OpcGuids.CATID_OPCAEServer10)).IsTrue();

        OpcServerDescriptorDto hda = descriptors.Single(static descriptor =>
            descriptor.ClassId == new Guid("10138C2C-0000-0000-0000-00000000AD10"));
        await Assert.That(hda.ProgId).IsEqualTo("Opc.Classic.Simulation.HDA.1");
        await Assert.That(hda.UserType).IsEqualTo("Opc.Classic Simulation HDA Server");
        await Assert.That(hda.VerIndProgId).IsEqualTo("Opc.Classic.Simulation.HDA");
        await Assert.That(hda.Host).IsEqualTo(SimDiscoveryModule.DiscoveryHost);
        await Assert.That(hda.Categories.Any(category => category == OpcGuids.CATID_OPCHDAServer10)).IsTrue();
    }

    private static Task<OpcServerDescriptorDto[]> EnumerateServersAsync(SimulationMcpHost host, params Guid[] categoryIds) =>
        host.CallToolAsync<OpcServerDescriptorDto[]>(
            "opcclassic.discovery.enumerate_servers",
            new Dictionary<string, object>
            {
                ["host"] = SimDiscoveryModule.DiscoveryHost,
                ["categoryIds"] = categoryIds.Select(static categoryId => categoryId.ToString("D")).ToArray(),
            });
}
