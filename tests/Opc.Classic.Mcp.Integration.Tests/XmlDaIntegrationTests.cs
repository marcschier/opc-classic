// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Text.Json;
using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class XmlDaIntegrationTests
{
    [Test]
    public async Task XmlDa_status_and_browse_round_trips_against_full_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await ConnectAsync(host).ConfigureAwait(false);

        OpcXmlDaServerStatusDto status = await host.CallToolAsync<OpcXmlDaServerStatusDto>(
            "opcclassic.xmlda.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcXmlDaBrowseResponseDto root = await BrowseAsync(host, sessionId).ConfigureAwait(false);
        OpcXmlDaBrowseResponseDto bucket = await BrowseAsync(host, sessionId, "Bucket Brigade").ConfigureAwait(false);
        OpcXmlDaBrowseResponseDto plant = await BrowseAsync(host, sessionId, "Plant").ConfigureAwait(false);
        OpcXmlDaBrowseResponseDto reactor1 = await BrowseAsync(host, sessionId, "Plant.Reactor1").ConfigureAwait(false);

        await Assert.That(status.VendorInfo).IsEqualTo("Opc.Classic Full-Feature Simulation Server");
        await Assert.That(status.ProductVersion).IsEqualTo("1.0.0");
        await Assert.That(status.ServerState).IsEqualTo("Running");
        await Assert.That(status.SupportedLocaleIds.SequenceEqual(new[] { "en-US" })).IsTrue();
        await Assert.That(status.SupportedInterfaceVersions.SequenceEqual(new[] { "XML_DA_Version_1_0" })).IsTrue();
        await Assert.That(root.Elements.Select(static element => element.ItemName).SequenceEqual(new[]
        {
            "Bucket Brigade",
            "Plant",
            "Random",
            "Signals",
        })).IsTrue();
        await Assert.That(root.Elements.All(static element => !element.IsItem && element.HasChildren)).IsTrue();
        await Assert.That(bucket.Elements.Select(static element => element.ItemName).SequenceEqual(new[]
        {
            "Bucket Brigade.Boolean",
            "Bucket Brigade.Int4",
            "Bucket Brigade.Real8",
            "Bucket Brigade.String",
        })).IsTrue();
        await Assert.That(bucket.Elements.All(static element => element.IsItem && !element.HasChildren)).IsTrue();
        await Assert.That(plant.Elements.Select(static element => element.ItemName).SequenceEqual(new[]
        {
            "Plant.Reactor1",
            "Plant.Reactor2",
        })).IsTrue();
        await Assert.That(plant.Elements.All(static element => !element.IsItem && element.HasChildren)).IsTrue();
        await Assert.That(reactor1.Elements.Select(static element => element.ItemName).SequenceEqual(new[]
        {
            "Plant.Reactor1.Level",
            "Plant.Reactor1.Pressure",
            "Plant.Reactor1.Temperature",
        })).IsTrue();
        await Assert.That(reactor1.Elements.All(static element => element.IsItem && !element.HasChildren)).IsTrue();
    }

    [Test]
    public async Task XmlDa_read_write_and_properties_round_trip_against_full_simulation_model()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await ConnectAsync(host).ConfigureAwait(false);

        OpcXmlDaGetPropertiesResponseDto properties = await host.CallToolAsync<OpcXmlDaGetPropertiesResponseDto>(
            "opcclassic.xmlda.get_properties",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemNames"] = new[] { "Plant.Reactor1.Temperature", "Bucket Brigade.Int4" },
                ["returnPropertyValues"] = true,
            }).ConfigureAwait(false);
        OpcXmlDaItemValueDto[] initialRead = await host.CallToolAsync<OpcXmlDaItemValueDto[]>(
            "opcclassic.xmlda.read",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["items"] = new[] { new OpcXmlDaReadItemDto("Plant.Reactor1.Temperature", "temperature") },
            }).ConfigureAwait(false);
        OpcXmlDaWriteResultDto[] write = await host.CallToolAsync<OpcXmlDaWriteResultDto[]>(
            "opcclassic.xmlda.write",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["items"] = new[] { new OpcXmlDaWriteItemDto("Bucket Brigade.Int4", 1234, "int4", "int") },
            }).ConfigureAwait(false);
        OpcXmlDaItemValueDto[] afterWrite = await host.CallToolAsync<OpcXmlDaItemValueDto[]>(
            "opcclassic.xmlda.read",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["items"] = new[] { new OpcXmlDaReadItemDto("Bucket Brigade.Int4", "int4") },
            }).ConfigureAwait(false);

        bool foundTemperature = host.Model.TryGetTag("Plant.Reactor1.Temperature", out var temperature);
        double expectedTemperature = (double)host.Model.ValueAt(temperature, initialRead[0].Timestamp!.Value);
        OpcXmlDaItemPropertyListDto temperatureProperties = properties.PropertyLists.Single(static list => list.ItemName == "Plant.Reactor1.Temperature");
        OpcXmlDaItemPropertyListDto int4Properties = properties.PropertyLists.Single(static list => list.ItemName == "Bucket Brigade.Int4");

        await Assert.That(properties.ServerState).IsEqualTo("Running");
        await Assert.That(properties.PropertyLists.Count).IsEqualTo(2);
        await Assert.That(temperatureProperties.Properties.Single(static property => property.Name == "DataType").ValueType).IsEqualTo("String");
        await Assert.That(GetString(temperatureProperties.Properties.Single(static property => property.Name == "DataType").Value)).IsEqualTo("Double");
        await Assert.That(GetString(temperatureProperties.Properties.Single(static property => property.Name == "AccessRights").Value)).IsEqualTo("read");
        await Assert.That(GetString(temperatureProperties.Properties.Single(static property => property.Name == "EngineeringUnits").Value)).IsEqualTo("degC");
        await Assert.That(GetString(temperatureProperties.Properties.Single(static property => property.Name == "ItemID").Value)).IsEqualTo("Plant.Reactor1.Temperature");
        await Assert.That(GetString(int4Properties.Properties.Single(static property => property.Name == "DataType").Value)).IsEqualTo("Int32");
        await Assert.That(GetString(int4Properties.Properties.Single(static property => property.Name == "AccessRights").Value)).IsEqualTo("read/write");
        await Assert.That(foundTemperature).IsTrue();
        await Assert.That(initialRead[0].ItemName).IsEqualTo("Plant.Reactor1.Temperature");
        await Assert.That(initialRead[0].ClientItemHandle).IsEqualTo("temperature");
        await Assert.That(initialRead[0].ValueType).IsEqualTo("Double");
        await Assert.That(initialRead[0].QualityText).StartsWith("Good");
        await Assert.That(initialRead[0].ResultCode).IsEqualTo("Ok");
        await Assert.That(GetDouble(initialRead[0].Value)).IsEqualTo(expectedTemperature);
        await Assert.That(write.Length).IsEqualTo(1);
        await Assert.That(write[0].ItemName).IsEqualTo("Bucket Brigade.Int4");
        await Assert.That(write[0].ClientItemHandle).IsEqualTo("int4");
        await Assert.That(write[0].ResultCode).IsEqualTo("Ok");
        await Assert.That(afterWrite.Length).IsEqualTo(1);
        await Assert.That(afterWrite[0].ItemName).IsEqualTo("Bucket Brigade.Int4");
        await Assert.That(afterWrite[0].ValueType).IsEqualTo("Int32");
        await Assert.That(afterWrite[0].QualityText).StartsWith("Good");
        await Assert.That(afterWrite[0].ResultCode).IsEqualTo("Ok");
        await Assert.That(GetInt32(afterWrite[0].Value)).IsEqualTo(1234);
    }

    [Test]
    public async Task XmlDa_subscription_poll_cancel_round_trips_against_full_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await ConnectAsync(host).ConfigureAwait(false);

        OpcXmlDaSubscriptionDto subscription = await host.CallToolAsync<OpcXmlDaSubscriptionDto>(
            "opcclassic.xmlda.subscribe",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["items"] = new[] { new OpcXmlDaSubscribeItemDto("Plant.Reactor1.Temperature", "sub-temperature") },
                ["requestedSamplingRate"] = 100,
                ["returnValuesOnReply"] = true,
            }).ConfigureAwait(false);
        OpcXmlDaSubscriptionPollDto poll = await host.CallToolAsync<OpcXmlDaSubscriptionPollDto>(
            "opcclassic.xmlda.poll_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverSubHandles"] = new[] { subscription.ServerSubHandle },
                ["returnAllItems"] = true,
            }).ConfigureAwait(false);
        OpcResultDto cancel = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.cancel_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverSubHandle"] = subscription.ServerSubHandle,
            }).ConfigureAwait(false);

        await Assert.That(subscription.ServerSubHandle.Length).IsGreaterThan(0);
        await Assert.That(subscription.RevisedSamplingRate).IsEqualTo(100);
        await Assert.That(subscription.ServerState).IsEqualTo("Running");
        await Assert.That(subscription.Items.Count).IsEqualTo(1);
        await Assert.That(subscription.Items[0].ItemName).IsEqualTo("Plant.Reactor1.Temperature");
        await Assert.That(subscription.Items[0].ClientItemHandle).IsEqualTo("sub-temperature");
        await Assert.That(subscription.Items[0].QualityText).StartsWith("Good");
        await Assert.That(subscription.Items[0].ResultCode).IsEqualTo("Ok");
        await Assert.That(poll.ServerState).IsEqualTo("Running");
        await Assert.That(poll.DataBufferOverflow).IsFalse();
        await Assert.That(poll.InvalidServerSubHandles.Count).IsEqualTo(0);
        await Assert.That(poll.ItemLists.Count).IsEqualTo(1);
        await Assert.That(poll.ItemLists[0].SubscriptionHandle).IsEqualTo(subscription.ServerSubHandle);
        await Assert.That(poll.ItemLists[0].Items.Count).IsEqualTo(1);
        await Assert.That(poll.ItemLists[0].Items[0].ItemName).IsEqualTo("Plant.Reactor1.Temperature");
        await Assert.That(poll.ItemLists[0].Items[0].ClientItemHandle).IsEqualTo("sub-temperature");
        await Assert.That(poll.ItemLists[0].Items[0].QualityText).StartsWith("Good");
        await Assert.That(cancel.Succeeded).IsTrue();
        await Assert.That(cancel.SubscriptionId).IsEqualTo(subscription.ServerSubHandle);
    }

    private static async Task<string> ConnectAsync(SimulationMcpHost host)
    {
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);
        OpcResultDto connected = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.xmlda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["endpointUrl"] = host.ConnectionString("xmlda"),
            }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(connected.ItemName).IsEqualTo(host.ConnectionString("xmlda"));
        return sessionId;
    }

    private static Task<OpcXmlDaBrowseResponseDto> BrowseAsync(
        SimulationMcpHost host,
        string sessionId,
        string itemName = "") =>
        host.CallToolAsync<OpcXmlDaBrowseResponseDto>(
            "opcclassic.xmlda.browse",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemName"] = itemName,
            });

    private static double GetDouble(object? value) => ((JsonElement)value!).GetDouble();

    private static int GetInt32(object? value) => ((JsonElement)value!).GetInt32();

    private static string? GetString(object? value) => ((JsonElement)value!).GetString();
}
