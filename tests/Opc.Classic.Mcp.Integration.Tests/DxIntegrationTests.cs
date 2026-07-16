// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class DxIntegrationTests
{
    [Test]
    public async Task Dx_configuration_round_trips_against_full_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);

        OpcSessionDto connected = await host.CallToolAsync<OpcSessionDto>(
            "opcclassic.dx.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("dx"),
            }).ConfigureAwait(false);
        OpcServerStatusDto status = await host.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.dx.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcDxSourceServerDto[] sources = await host.CallToolAsync<OpcDxSourceServerDto[]>(
            "opcclassic.dx.query_source_servers",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        string[] seededConnections = await host.CallToolAsync<string[]>(
            "opcclassic.dx.query_connections",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        string[] reactorConnections = await host.CallToolAsync<string[]>(
            "opcclassic.dx.query_connections",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["browsePath"] = "Plant",
                ["connectionMasks"] = new[] { "*Temperature*" },
                ["recursive"] = true,
            }).ConfigureAwait(false);

        var connection = new OpcDxConnectionDto(
            Name: "IntegrationTemperatureMirror",
            Description: "Integration source to target mirror",
            ItemPath: "Plant.Reactor1",
            ItemName: "TemperatureIntegrationMirror",
            Version: "cfg-integration",
            SourceServerName: "SimulationDA",
            SourceItemPath: "Plant.Reactor1",
            SourceItemName: "Temperature",
            TargetItemPath: "Bucket Brigade",
            TargetItemName: "Real8",
            UpdateRateMilliseconds: 750,
            DeadbandPercent: 0.25f);
        OpcResultDto added = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.add_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connection"] = connection,
            }).ConfigureAwait(false);
        string[] addedConnections = await host.CallToolAsync<string[]>(
            "opcclassic.dx.query_connections",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["browsePath"] = "Plant.Reactor1",
                ["connectionMasks"] = new[] { "Integration*" },
            }).ConfigureAwait(false);
        OpcResultDto modified = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.modify_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connection"] = connection with { Description = "Modified integration mirror" },
            }).ConfigureAwait(false);
        OpcResultDto updated = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.update_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionName"] = "Integration*",
                ["browsePath"] = "Plant.Reactor1",
                ["connection"] = connection with { UpdateRateMilliseconds = 250 },
            }).ConfigureAwait(false);
        OpcResultDto deleted = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.delete_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionName"] = "IntegrationTemperatureMirror",
                ["browsePath"] = "Plant.Reactor1",
            }).ConfigureAwait(false);
        string[] afterDelete = await host.CallToolAsync<string[]>(
            "opcclassic.dx.query_connections",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionMasks"] = new[] { "Integration*" },
            }).ConfigureAwait(false);
        OpcResultDto reset = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.reset_configuration",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["configurationVersion"] = "4",
            }).ConfigureAwait(false);

        await Assert.That(connected.DaConnected).IsTrue();
        await Assert.That(status.Spec).IsEqualTo("Dx");
        await Assert.That(status.State).IsEqualTo("Running");
        await Assert.That(status.VendorInfo).IsEqualTo("Opc.Classic Simulation DX Reference Engine");
        await Assert.That(status.GroupCount).IsEqualTo(2);
        await Assert.That(sources.Length).IsEqualTo(1);
        await Assert.That(sources[0].Name).IsEqualTo("SimulationDA");
        await Assert.That(sources[0].ServerUrl).IsEqualTo("opcda://simulation/Opc.Classic.Simulation.DA.1");
        await Assert.That(sources[0].Description).IsEqualTo("Deterministic managed DA source endpoint");
        await Assert.That(seededConnections.SequenceEqual(new[]
        {
            "ReactorPressureDisabled",
            "ReactorTemperatureToBucket",
        })).IsTrue();
        await Assert.That(reactorConnections.SequenceEqual(new[] { "ReactorTemperatureToBucket" })).IsTrue();
        await Assert.That(added.Succeeded).IsTrue();
        await Assert.That(addedConnections.SequenceEqual(new[] { "IntegrationTemperatureMirror" })).IsTrue();
        await Assert.That(modified.Succeeded).IsTrue();
        await Assert.That(updated.Succeeded).IsTrue();
        await Assert.That(deleted.Succeeded).IsTrue();
        await Assert.That(afterDelete.Length).IsEqualTo(0);
        await Assert.That(reset.Succeeded).IsTrue();
        await Assert.That(reset.ValueType).IsEqualTo("5");
    }
}
