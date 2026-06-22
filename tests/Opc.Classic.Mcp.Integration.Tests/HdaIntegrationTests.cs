// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Globalization;
using System.Text.Json;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Samples.SimulationServer;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class HdaIntegrationTests
{
    private const string TemperatureItem = "Plant.Reactor1.Temperature";
    private static readonly DateTimeOffset WindowStart = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset WindowEnd = new(2026, 1, 1, 0, 4, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdateTimestamp = new(2026, 1, 1, 0, 12, 34, TimeSpan.Zero);

    [Test]
    public async Task Hda_full_simulation_reads_updates_deletes_and_annotations_round_trip()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync();
        string sessionId = await host.CreateSessionAsync();
        SimulatedTag temperature = ReactorTemperatureTag(host);

        OpcResultDto connected = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.hda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("hda"),
            });
        OpcServerStatusDto status = await host.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.hda.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId });
        OpcHdaBrowseElementDto[] browse = await host.CallToolAsync<OpcHdaBrowseElementDto[]>(
            "opcclassic.hda.browse",
            new Dictionary<string, object> { ["sessionId"] = sessionId, ["browseType"] = "flat" });
        OpcHdaItemHandleDto[] handles = await host.CallToolAsync<OpcHdaItemHandleDto[]>(
            "opcclassic.hda.get_item_handles",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemIds"] = new[] { TemperatureItem },
                ["clientHandles"] = new[] { 101 },
            });
        int handle = handles[0].ServerHandle;
        OpcHdaReadResultDto[] raw = await host.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_raw",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["startTime"] = Iso(WindowStart),
                ["endTime"] = Iso(WindowEnd),
                ["maxValuesPerItem"] = 5,
                ["includeBounds"] = true,
            });
        OpcHdaReadResultDto[] processed = await host.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_processed",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["startTime"] = Iso(WindowStart),
                ["endTime"] = Iso(WindowStart.AddMinutes(2)),
                ["resampleIntervalSeconds"] = 60,
                ["aggregate"] = "Average",
            });
        OpcResultDto[] inserted = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.insert_data",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["timestamps"] = new[] { UpdateTimestamp },
                ["values"] = new[] { 123.25 },
            });
        OpcHdaReadResultDto[] afterInsert = await host.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_raw",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["startTime"] = Iso(UpdateTimestamp),
                ["endTime"] = Iso(UpdateTimestamp),
                ["maxValuesPerItem"] = 1,
                ["includeBounds"] = true,
            });
        OpcResultDto[] replaced = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.replace_data",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["timestamps"] = new[] { UpdateTimestamp },
                ["values"] = new[] { 124.5 },
            });
        OpcHdaReadResultDto[] afterReplace = await host.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_raw",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["startTime"] = Iso(UpdateTimestamp),
                ["endTime"] = Iso(UpdateTimestamp),
                ["maxValuesPerItem"] = 1,
                ["includeBounds"] = true,
            });
        OpcResultDto[] insertReplaced = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.insert_replace_data",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["timestamps"] = new[] { UpdateTimestamp },
                ["values"] = new[] { 125.75 },
            });
        OpcResultDto[] deleted = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.delete_at_time",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["timestamps"] = new[] { UpdateTimestamp },
            });
        OpcHdaReadResultDto[] afterDelete = await host.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_raw",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["startTime"] = Iso(UpdateTimestamp),
                ["endTime"] = Iso(UpdateTimestamp),
                ["maxValuesPerItem"] = 1,
                ["includeBounds"] = true,
            });
        OpcHdaAnnotationResultDto[] initialAnnotations = await host.CallToolAsync<OpcHdaAnnotationResultDto[]>(
            "opcclassic.hda.read_annotations",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["startTime"] = Iso(WindowStart),
                ["endTime"] = Iso(WindowEnd),
            });
        OpcResultDto[] annotationInsert = await host.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.insert_annotations",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["timestamps"] = new[] { UpdateTimestamp },
                ["annotationTexts"] = new[] { "integration operator note" },
                ["users"] = new[] { "integration.tester" },
            });
        OpcHdaAnnotationResultDto[] afterAnnotationInsert = await host.CallToolAsync<OpcHdaAnnotationResultDto[]>(
            "opcclassic.hda.read_annotations",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["serverHandles"] = new[] { handle },
                ["startTime"] = Iso(UpdateTimestamp),
                ["endTime"] = Iso(UpdateTimestamp),
            });

        IReadOnlyList<(DateTimeOffset Timestamp, object Value)> expectedRaw = host.Model.History(temperature, WindowStart, WindowEnd, TimeSpan.FromMinutes(1));
        double expectedFirstAverage = host.Model
            .History(temperature, WindowStart, WindowStart.AddMinutes(1), TimeSpan.FromSeconds(1))
            .Average(static sample => Convert.ToDouble(sample.Value, CultureInfo.InvariantCulture));

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Spec).IsEqualTo("Hda");
        await Assert.That(status.VendorInfo).IsEqualTo(host.Model.VendorInfo);
        await Assert.That(browse.Select(static element => element.ItemId)).Contains(TemperatureItem);
        await Assert.That(handles.Length).IsEqualTo(1);
        await Assert.That(handles[0].Succeeded).IsTrue();
        await Assert.That(handles[0].ClientHandle).IsEqualTo(101);
        await Assert.That(handle).IsGreaterThan(0);
        await Assert.That(raw.Length).IsEqualTo(1);
        await Assert.That(raw[0].ItemId).IsEqualTo(TemperatureItem);
        await Assert.That(raw[0].Values.Count).IsEqualTo(5);
        await Assert.That(raw[0].Values.Select(static value => value.Timestamp)).IsEquivalentTo(expectedRaw.Select(static sample => sample.Timestamp));
        await Assert.That(raw[0].Values.SequenceEqual(raw[0].Values.OrderBy(static value => value.Timestamp))).IsTrue();
        await Assert.That(GetDouble(raw[0].Values[0].Value)).IsEqualTo(Convert.ToDouble(expectedRaw[0].Value, CultureInfo.InvariantCulture));
        await Assert.That(processed.Length).IsEqualTo(1);
        await Assert.That(processed[0].Aggregate).IsEqualTo("Average");
        await Assert.That(processed[0].Values.Count).IsGreaterThan(0);
        await Assert.That(Math.Abs(GetDouble(processed[0].Values[0].Value) - expectedFirstAverage)).IsLessThan(0.000000001);
        await Assert.That(inserted[0].Succeeded).IsTrue();
        await Assert.That(afterInsert[0].Values.Count).IsEqualTo(1);
        await Assert.That(afterInsert[0].Values[0].Timestamp).IsEqualTo(UpdateTimestamp);
        await Assert.That(GetDouble(afterInsert[0].Values[0].Value)).IsEqualTo(123.25);
        await Assert.That(replaced[0].Succeeded).IsTrue();
        await Assert.That(insertReplaced[0].Succeeded).IsTrue();
        await Assert.That(afterReplace[0].Values.Count).IsEqualTo(1);
        await Assert.That(afterReplace[0].Values[0].Timestamp).IsEqualTo(UpdateTimestamp);
        await Assert.That(GetDouble(afterReplace[0].Values[0].Value)).IsEqualTo(124.5);
        await Assert.That(deleted[0].Succeeded).IsTrue();
        await Assert.That(afterDelete[0].Values.Count).IsEqualTo(0);
        await Assert.That(initialAnnotations[0].Annotations.Select(static annotation => annotation.AnnotationText)).Contains("Calibration note for " + TemperatureItem);
        await Assert.That(annotationInsert[0].Succeeded).IsTrue();
        await Assert.That(afterAnnotationInsert[0].Annotations.Select(static annotation => annotation.AnnotationText)).Contains("integration operator note");
        await Assert.That(afterAnnotationInsert[0].Annotations.Single(static annotation => annotation.AnnotationText == "integration operator note").User).IsEqualTo("integration.tester");
    }

    private static SimulatedTag ReactorTemperatureTag(SimulationMcpHost host)
    {
        bool found = host.Model.TryGetTag(TemperatureItem, out SimulatedTag tag);
        return found ? tag : throw new InvalidOperationException("Simulation model did not contain " + TemperatureItem + ".");
    }

    private static string Iso(DateTimeOffset timestamp) => timestamp.ToString("O", CultureInfo.InvariantCulture);

    private static double GetDouble(object? value) => ((JsonElement)value!).GetDouble();
}
