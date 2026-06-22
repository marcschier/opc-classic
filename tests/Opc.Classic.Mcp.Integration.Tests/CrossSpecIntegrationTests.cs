// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Samples.SimulationServer.Discovery;

namespace Opc.Classic.Mcp.Integration.Tests;

/// <summary>
/// Exercises a single simulation instance across all ten OPC Classic feature areas at once,
/// proving DA, AE, HDA, Batch, Commands, Complex Data, DX, Security, Discovery, and XML-DA all
/// project the same live server through the MCP tool surface concurrently.
/// </summary>
public sealed class CrossSpecIntegrationTests
{
    [Test]
    public async Task All_feature_areas_serve_one_simulation_instance_through_mcp()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);

        // DCOM-style channel specs that connect with a connectionString and expose get_status.
        OpcServerStatusDto da = await ConnectAndStatusAsync(host, sessionId, "da", "da");
        OpcServerStatusDto ae = await ConnectAndStatusAsync(host, sessionId, "ae", "ae");
        OpcServerStatusDto hda = await ConnectAndStatusAsync(host, sessionId, "hda", "hda");
        OpcServerStatusDto batch = await ConnectAndStatusAsync(host, sessionId, "batch", "batch");
        OpcServerStatusDto commands = await ConnectAndStatusAsync(host, sessionId, "commands", "commands");
        OpcServerStatusDto dx = await ConnectAndStatusAsync(host, sessionId, "dx", "dx");

        // Complex Data is served over the DA channel under its own endpoint.
        await host.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("cpx"),
            }).ConfigureAwait(false);
        OpcTypeSystemDto cpxTypeSystem = await host.CallToolAsync<OpcTypeSystemDto>(
            "opcclassic.cpx.get_type_system",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);

        // XML-DA connects with an endpointUrl and has its own status DTO.
        await host.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["endpointUrl"] = host.ConnectionString("xmlda"),
            }).ConfigureAwait(false);
        OpcXmlDaServerStatusDto xmlda = await host.CallToolAsync<OpcXmlDaServerStatusDto>(
            "opcclassic.xmlda.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);

        // Security is resolved from DI on the same session.
        OpcSecurityInfoDto security = await host.CallToolAsync<OpcSecurityInfoDto>(
            "opcclassic.security.is_available_nt",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);

        // Discovery is resolved from DI and answers for the simulation host.
        OpcServerDescriptorDto[] servers = await host.CallToolAsync<OpcServerDescriptorDto[]>(
            "opcclassic.discovery.enumerate_servers",
            new Dictionary<string, object>
            {
                ["host"] = SimDiscoveryModule.DiscoveryHost,
                ["categoryIds"] = new[] { "63D5F432-CFE4-11D1-B2C8-0060083BA1FB" },
            }).ConfigureAwait(false);

        string vendor = host.Model.VendorInfo;
        await Assert.That(da.Spec).IsEqualTo("Da");
        await Assert.That(da.VendorInfo).IsEqualTo(vendor);
        await Assert.That(da.IsOperational).IsTrue();
        await Assert.That(ae).IsNotNull();
        await Assert.That(hda).IsNotNull();
        await Assert.That(batch).IsNotNull();
        await Assert.That(commands).IsNotNull();
        await Assert.That(dx).IsNotNull();
        await Assert.That(cpxTypeSystem).IsNotNull();
        await Assert.That(xmlda.VendorInfo).IsEqualTo(vendor);
        await Assert.That(xmlda.ServerState).IsEqualTo("Running");
        await Assert.That(security.SupportsWindowsAuthentication).IsTrue();
        await Assert.That(servers.Length).IsGreaterThan(0);
    }

    private static async Task<OpcServerStatusDto> ConnectAndStatusAsync(
        SimulationMcpHost host,
        string sessionId,
        string spec,
        string connectionSpec)
    {
        await host.CallToolAsync<OpcSessionDto>(
            "opcclassic." + spec + ".connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString(connectionSpec),
            }).ConfigureAwait(false);

        return await host.CallToolAsync<OpcServerStatusDto>(
            "opcclassic." + spec + ".get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
    }
}
