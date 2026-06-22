// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class BatchIntegrationTests
{
    [Test]
    public async Task Batch_tools_query_seeded_summaries_and_filtered_results_against_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);

        OpcResultDto connected = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("batch"),
            }).ConfigureAwait(false);
        OpcResultDto status = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcBatchSummaryDto[] summaries = await host.CallToolAsync<OpcBatchSummaryDto[]>(
            "opcclassic.batch.query_batch_summaries",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["maxResults"] = 10,
            }).ConfigureAwait(false);
        OpcBatchSummaryDto[] runningSummaries = await host.CallToolAsync<OpcBatchSummaryDto[]>(
            "opcclassic.batch.query_batch_summaries",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["id"] = "B-2026",
                ["executionState"] = "RUNNING",
                ["maxResults"] = 10,
            }).ConfigureAwait(false);
        OpcResultDto disconnected = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.disconnect",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Message).Contains("delimiter='/'");
        await Assert.That(summaries.Length).IsEqualTo(3);
        await Assert.That(summaries.Select(static summary => summary.Id!)).Contains("B-2026-001");
        await Assert.That(summaries.Select(static summary => summary.Id!)).Contains("B-2026-002");
        await Assert.That(summaries.Select(static summary => summary.Id!)).Contains("B-2026-003");
        await Assert.That(summaries.Single(static summary => summary.Id == "B-2026-001").Description)
            .IsEqualTo("Starter culture fermentation");
        await Assert.That(summaries.Single(static summary => summary.Id == "B-2026-002").ExecutionMode)
            .IsEqualTo("MANUAL");
        await Assert.That(runningSummaries.Length).IsEqualTo(2);
        await Assert.That(runningSummaries.Select(static summary => summary.Id))
            .IsEquivalentTo(new[] { "B-2026-001", "B-2026-003" });
        await Assert.That(runningSummaries.All(static summary => summary.ExecutionState == "RUNNING")).IsTrue();
        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    [Test]
    public async Task Batch_tools_query_enumeration_sets_values_and_lists_against_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);
        _ = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.batch.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("batch"),
            }).ConfigureAwait(false);

        OpcBatchEnumerationSetDto[] sets = await host.CallToolAsync<OpcBatchEnumerationSetDto[]>(
            "opcclassic.batch.query_enumeration_sets",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcBatchEnumerationDto state = await host.CallToolAsync<OpcBatchEnumerationDto>(
            "opcclassic.batch.query_enumeration",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["enumerationSetId"] = 2,
                ["enumerationValue"] = 1,
            }).ConfigureAwait(false);
        OpcBatchEnumerationDto[] states = await host.CallToolAsync<OpcBatchEnumerationDto[]>(
            "opcclassic.batch.query_enumeration_list",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["enumerationSetId"] = 2,
            }).ConfigureAwait(false);

        await Assert.That(sets.Select(static set => set.Name))
            .IsEquivalentTo(new[] { "OPCB_ENUM_PHYS", "OPCB_ENUM_MODE", "OPCB_ENUM_STATE" });
        await Assert.That(sets.Single(static set => set.Name == "OPCB_ENUM_STATE").EnumerationSetId).IsEqualTo(2);
        await Assert.That(state).IsEqualTo(new OpcBatchEnumerationDto(2, 1, "RUNNING"));
        await Assert.That(states.Select(static value => value.Name))
            .IsEquivalentTo(new[] { "IDLE", "RUNNING", "COMPLETE", "HELD" });
    }
}
