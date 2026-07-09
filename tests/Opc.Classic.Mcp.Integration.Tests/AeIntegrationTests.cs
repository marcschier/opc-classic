// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class AeIntegrationTests
{
    private const int ConditionCategory = 0x1002;
    private const string ReactorArea = "Plant.Reactor1";
    private const string ReactorSource = "Plant.Reactor1.Temperature";
    private const string ReactorCondition = "TemperatureHigh";

    [Test]
    public async Task Ae_full_simulation_connect_browse_query_refresh_and_acknowledge_round_trip()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync();
        string sessionId = await host.CreateSessionAsync();

        OpcResultDto connected = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("ae"),
            });
        OpcServerStatusDto status = await host.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.ae.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId });
        OpcAreaBrowseElementDto[] root = await host.CallToolAsync<OpcAreaBrowseElementDto[]>(
            "opcclassic.ae.browse_areas",
            new Dictionary<string, object> { ["sessionId"] = sessionId });
        OpcAreaBrowseElementDto[] plant = await host.CallToolAsync<OpcAreaBrowseElementDto[]>(
            "opcclassic.ae.browse_areas",
            new Dictionary<string, object> { ["sessionId"] = sessionId, ["areaQualifiedName"] = "Plant" });
        OpcAreaBrowseElementDto[] reactor1 = await host.CallToolAsync<OpcAreaBrowseElementDto[]>(
            "opcclassic.ae.browse_areas",
            new Dictionary<string, object> { ["sessionId"] = sessionId, ["areaQualifiedName"] = ReactorArea });
        OpcEventCategoryDto[] categories = await host.CallToolAsync<OpcEventCategoryDto[]>(
            "opcclassic.ae.query_event_categories",
            new Dictionary<string, object> { ["sessionId"] = sessionId, ["eventTypes"] = "all" });
        OpcEventAttributeDto[] attributes = await host.CallToolAsync<OpcEventAttributeDto[]>(
            "opcclassic.ae.query_event_attributes",
            new Dictionary<string, object> { ["sessionId"] = sessionId, ["eventCategory"] = ConditionCategory });
        OpcConditionStateDto condition = await host.CallToolAsync<OpcConditionStateDto>(
            "opcclassic.ae.get_condition_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["source"] = ReactorSource,
                ["conditionName"] = ReactorCondition,
                ["attributeIds"] = new[] { 10, 11, 12, 13 },
            });
        OpcAeSubscriptionDto subscription = await host.CallToolAsync<OpcAeSubscriptionDto>(
            "opcclassic.ae.create_subscription",
            new Dictionary<string, object> { ["sessionId"] = sessionId, ["bufferTimeMs"] = 50, ["maxBufferSize"] = 20 });
        OpcAeSubscriptionDto filtered = await host.CallToolAsync<OpcAeSubscriptionDto>(
            "opcclassic.ae.set_filter",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
                ["eventTypes"] = "condition",
                ["eventCategories"] = new[] { ConditionCategory },
                ["minSeverity"] = 0,
                ["maxSeverity"] = 1000,
                ["areas"] = new[] { ReactorArea },
                ["sources"] = new[] { ReactorSource },
            });
        OpcEventNotificationDto[] initialEvents = await host.CallToolAsync<OpcEventNotificationDto[]>(
            "opcclassic.ae.poll_events",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
                ["waitMilliseconds"] = 1000,
            });
        OpcResultDto refreshed = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.refresh_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
            });
        OpcEventNotificationDto[] refreshedEvents = await host.CallToolAsync<OpcEventNotificationDto[]>(
            "opcclassic.ae.poll_events",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
                ["waitMilliseconds"] = 1000,
            });
        OpcResultDto[] ack = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.ae.ack_condition",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["source"] = ReactorSource,
                ["conditionName"] = ReactorCondition,
                ["actor"] = "operator.integration",
                ["comment"] = "ack via full simulation MCP integration test",
            });
        OpcConditionStateDto acknowledged = await host.CallToolAsync<OpcConditionStateDto>(
            "opcclassic.ae.get_condition_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["source"] = ReactorSource,
                ["conditionName"] = ReactorCondition,
                ["attributeIds"] = new[] { 10, 11, 12, 13 },
            });
        OpcResultDto canceled = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.cancel_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
            });

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Spec).IsEqualTo("Ae");
        await Assert.That(status.VendorInfo).IsEqualTo(host.Model.VendorInfo);
        await Assert.That(root.Select(static element => element.QualifiedName)).Contains("Plant");
        await Assert.That(plant.Select(static element => element.QualifiedName)).Contains("Plant.Reactor1");
        await Assert.That(plant.Select(static element => element.QualifiedName)).Contains("Plant.Reactor2");
        await Assert.That(reactor1.Select(static element => element.QualifiedName)).Contains(ReactorSource);
        await Assert.That(reactor1.Single(static element => element.QualifiedName == ReactorSource).IsSource).IsTrue();
        await Assert.That(categories.Select(static category => category.EventCategory)).Contains(ConditionCategory);
        await Assert.That(attributes.Select(static attribute => attribute.Description)).IsEquivalentTo(new[] { "Area", "Limit", "Units", "Current Value" });
        await Assert.That(condition.SubConditionNames).IsEquivalentTo(new[] { "High", "HighHigh" });
        await Assert.That(condition.EventAttributes.Count).IsEqualTo(4);
        await Assert.That(filtered.SubscriptionId).IsEqualTo(subscription.SubscriptionId);
        await Assert.That(initialEvents.Select(static notification => notification.ConditionName!)).Contains(ReactorCondition);
        await Assert.That(initialEvents.All(static notification => notification.Source == ReactorSource)).IsTrue();
        await Assert.That(refreshed.Succeeded).IsTrue();
        await Assert.That(refreshedEvents.Select(static notification => notification.ConditionName!)).Contains(ReactorCondition);
        await Assert.That(ack.Length).IsEqualTo(1);
        await Assert.That(ack[0].Succeeded).IsTrue();
        await Assert.That(acknowledged.AcknowledgerId).IsEqualTo("operator.integration");
        await Assert.That(acknowledged.Comment).IsEqualTo("ack via full simulation MCP integration test");
        await Assert.That(canceled.Succeeded).IsTrue();
    }
}
