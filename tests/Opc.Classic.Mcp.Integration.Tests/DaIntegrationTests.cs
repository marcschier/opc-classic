// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Text.Json;
using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class DaIntegrationTests
{
    [Test]
    public async Task Da_tools_round_trip_against_full_simulation_server()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync();
        string sessionId = await host.CreateSessionAsync();

        OpcSessionDto connected = await host.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("da"),
            });
        OpcServerStatusDto status = await host.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.da.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId });
        OpcBrowseElementDto[] rootBrowse = await host.CallToolAsync<OpcBrowseElementDto[]>(
            "opcclassic.da.browse",
            new Dictionary<string, object> { ["sessionId"] = sessionId });
        OpcBrowseElementDto[] reactorBrowse = await host.CallToolAsync<OpcBrowseElementDto[]>(
            "opcclassic.da.browse",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemId"] = "Plant.Reactor1",
            });
        OpcBrowseElementDto[] bucketBrowse = await host.CallToolAsync<OpcBrowseElementDto[]>(
            "opcclassic.da.browse",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemId"] = "Bucket Brigade",
            });
        OpcBrowseElementDto[] properties = await host.CallToolAsync<OpcBrowseElementDto[]>(
            "opcclassic.da.get_properties",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemIds"] = new[] { "Plant.Reactor1.Temperature", "Bucket Brigade.Int4" },
                ["propertyIds"] = new[] { 1, 2, 5, 100 },
                ["returnValues"] = true,
            });
        OpcGroupStateDto group = await host.CallToolAsync<OpcGroupStateDto>(
            "opcclassic.da.add_group",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["name"] = "simulation-da-integration",
                ["clientHandle"] = 700,
                ["updateRateMs"] = 100,
            });
        OpcResultDto[] addResults = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.da.add_items",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["itemIds"] = new[] { "Plant.Reactor1.Temperature", "Bucket Brigade.Int4", "Signals.Sine" },
                ["clientHandles"] = new[] { 11, 12, 13 },
            });
        int[] serverHandles = addResults.Select(static result => result.ServerHandle.GetValueOrDefault()).ToArray();
        OpcItemValueDto[] initialRead = await host.CallToolAsync<OpcItemValueDto[]>(
            "opcclassic.da.read_sync",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["serverHandles"] = serverHandles,
            });
        OpcResultDto[] writableWrite = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.da.write_sync",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["serverHandles"] = new[] { serverHandles[1] },
                ["values"] = new object[] { 1234 },
            });
        OpcItemValueDto[] afterWrite = await host.CallToolAsync<OpcItemValueDto[]>(
            "opcclassic.da.read_sync",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["serverHandles"] = new[] { serverHandles[1] },
            });
        OpcResultDto[] readOnlyWrite = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.da.write_sync",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["serverHandles"] = new[] { serverHandles[2] },
                ["values"] = new object[] { 12.5 },
            });
        OpcResultDto removed = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.da.remove_group",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["groupHandle"] = group.ServerGroupHandle,
            });

        await Assert.That(connected.DaConnected).IsTrue();
        await Assert.That(status.Spec).IsEqualTo("Da");
        await Assert.That(status.VendorInfo).IsEqualTo(host.Model.VendorInfo);
        await Assert.That(status.IsOperational).IsTrue();
        await Assert.That(rootBrowse.Where(static element => element.HasChildren).Select(static element => element.ItemName)).Contains("Random");
        await Assert.That(rootBrowse.Where(static element => element.HasChildren).Select(static element => element.ItemName)).Contains("Signals");
        await Assert.That(rootBrowse.Where(static element => element.HasChildren).Select(static element => element.ItemName)).Contains("Bucket Brigade");
        await Assert.That(rootBrowse.Where(static element => element.HasChildren).Select(static element => element.ItemName)).Contains("Plant");
        await Assert.That(reactorBrowse.Where(static element => element.IsItem).Select(static element => element.ItemName)).Contains("Plant.Reactor1.Temperature");
        await Assert.That(bucketBrowse.Where(static element => element.IsItem).Select(static element => element.ItemName)).Contains("Bucket Brigade.Int4");
        await Assert.That(properties.Length).IsEqualTo(2);
        await Assert.That(properties[0].Properties.Any(static property => property.PropertyId == 100 && string.Equals(property.Value?.ToString(), "degC", StringComparison.Ordinal))).IsTrue();
        await Assert.That(properties[1].Properties.Any(static property => property.PropertyId == 5)).IsTrue();
        await Assert.That(group.Name).IsEqualTo("simulation-da-integration");
        await Assert.That(addResults.All(static result => result.Succeeded)).IsTrue();
        await Assert.That(addResults[0].ItemName).IsEqualTo("Plant.Reactor1.Temperature");
        await Assert.That(addResults[1].ItemName).IsEqualTo("Bucket Brigade.Int4");
        await Assert.That(addResults[2].ItemName).IsEqualTo("Signals.Sine");
        await Assert.That(initialRead.Length).IsEqualTo(3);
        await Assert.That(initialRead.All(static value => value.HResult == 0)).IsTrue();
        await Assert.That(initialRead.Select(static value => value.ItemName)).Contains("Plant.Reactor1.Temperature");
        await Assert.That(writableWrite.Single().Succeeded).IsTrue();
        await Assert.That(GetInt32(afterWrite.Single().Value)).IsEqualTo(1234);
        await Assert.That(readOnlyWrite.Single().Succeeded).IsFalse();
        await Assert.That(readOnlyWrite.Single().ItemName).IsEqualTo("Signals.Sine");
        await Assert.That(removed.Succeeded).IsTrue();
    }

    private static int GetInt32(object? value) => ((JsonElement)value!).GetInt32();
}
