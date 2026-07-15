// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Opc.Classic.VendorDescriptors.Tests;

public sealed class VendorProbeCatalogTests
{
    [Test]
    [Arguments("generic-opc-classic-template.json")]
    [Arguments("matrikon-opc-simulation-server.json")]
    [Arguments("opc-foundation-testserver.json")]
    public async Task Catalogs_LoadAndValidate(string fileName)
    {
        VendorProbeCatalogDescriptor descriptor =
            VendorProbeCatalogLoader.Load(FixturePath(fileName));

        await Assert.That(descriptor.SchemaVersion).IsEqualTo("1.0");
        await Assert.That(descriptor.Capabilities.Count).IsGreaterThan(0);
        await Assert.That(descriptor.Probes.Count).IsGreaterThan(0);
        await Assert.That(VendorProbeCatalogValidator.Validate(descriptor).Count)
            .IsEqualTo(0);
    }

    [Test]
    public async Task FixtureDecodeProbes_ReferenceDeclaredFixtureIds()
    {
        VendorProbeCatalogDescriptor descriptor =
            VendorProbeCatalogLoader.Load(FixturePath("generic-opc-classic-template.json"));
        HashSet<string> fixtureIds = descriptor.Fixtures
            .Select(fixture => fixture.Id)
            .ToHashSet(StringComparer.Ordinal);

        bool allDeclared = descriptor.Probes
            .Where(probe => probe.Type == "fixture-decode")
            .All(probe => probe.FixtureId is not null && fixtureIds.Contains(probe.FixtureId));

        await Assert.That(allDeclared).IsTrue();
    }

    [Test]
    public async Task ProbeCapabilities_MustBeDeclared()
    {
        JsonObject descriptor = ReadFixtureObject("matrikon-opc-simulation-server.json");
        descriptor["probes"]![0]!["requires"] = new JsonArray("not-declared");

        VendorProbeValidationException exception = CaptureInvalid(descriptor);

        await Assert.That(exception.Errors.Any(
            error => error.Message.Contains("not declared", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task FixtureDecodeProbe_RejectsMissingFixtureId()
    {
        JsonObject descriptor = ReadFixtureObject("matrikon-opc-simulation-server.json");
        JsonNode probe = descriptor["probes"]!.AsArray()
            .First(value => value!["type"]!.GetValue<string>() == "fixture-decode")!;
        probe["fixtureId"] = "missing-fixture";

        VendorProbeValidationException exception = CaptureInvalid(descriptor);

        await Assert.That(exception.Errors.Any(
            error => error.Path.EndsWith(".fixtureId", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task ProbeIds_MustBeUnique()
    {
        JsonObject descriptor = ReadFixtureObject("matrikon-opc-simulation-server.json");
        JsonArray probes = descriptor["probes"]!.AsArray();
        probes[1]!["id"] = probes[0]!["id"]!.GetValue<string>();

        VendorProbeValidationException exception = CaptureInvalid(descriptor);

        await Assert.That(exception.Errors.Any(
            error => error.Message.Contains("Duplicate probe id", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task DaArguments_RequireAlignedArrays()
    {
        JsonObject descriptor = ReadFixtureObject("opc-foundation-testserver.json");
        descriptor["arguments"]!["da"]!["clientHandles"] = new JsonArray(1, 2);

        VendorProbeValidationException exception = CaptureInvalid(descriptor);

        await Assert.That(exception.Errors.Any(
            error => error.Message.Contains("identical non-zero lengths", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task Loader_RejectsSensitiveProperties()
    {
        JsonObject descriptor = ReadFixtureObject("matrikon-opc-simulation-server.json");
        descriptor["password"] = "forbidden";

        VendorProbeValidationException exception = CaptureInvalid(descriptor);

        await Assert.That(exception.Errors.Any(
            error => error.Message.Contains("Sensitive", StringComparison.Ordinal))).IsTrue();
    }

    [Test]
    public async Task DaSyncWrite_DeclaresExpectedItemAndHResult()
    {
        VendorProbeCatalogDescriptor descriptor =
            VendorProbeCatalogLoader.Load(FixturePath("opc-foundation-testserver.json"));
        VendorProbeScenario probe = descriptor.Probes.Single(value => value.Id == "da-sync-write");

        await Assert.That(probe.Expected.ItemId).IsEqualTo("Test.Int32");
        await Assert.That(probe.Expected.HResult.HasValue).IsTrue();
    }

    [Test]
    public async Task Schema_DefinesStrictProbeExpectedShape()
    {
        using JsonDocument schema =
            JsonDocument.Parse(File.ReadAllText(FixturePath("vendor-probe-catalog-v1.schema.json")));
        JsonElement root = schema.RootElement;
        JsonElement probe = root.GetProperty("$defs").GetProperty("probe");
        JsonElement expected = probe.GetProperty("properties").GetProperty("expected");

        await Assert.That(root.GetProperty("additionalProperties").GetBoolean()).IsFalse();
        await Assert.That(probe.GetProperty("additionalProperties").GetBoolean()).IsFalse();
        await Assert.That(
            expected.GetProperty("properties").TryGetProperty("itemId", out _)).IsTrue();
        await Assert.That(
            expected.GetProperty("properties").TryGetProperty("hResult", out _)).IsTrue();
    }

    private static VendorProbeValidationException CaptureInvalid(JsonObject descriptor)
    {
        try
        {
            _ = VendorProbeCatalogLoader.LoadJson(descriptor.ToJsonString());
            throw new InvalidOperationException("Expected validation failure.");
        }
        catch (VendorProbeValidationException exception)
        {
            return exception;
        }
    }

    private static JsonObject ReadFixtureObject(string fileName)
    {
        return JsonNode.Parse(File.ReadAllText(FixturePath(fileName)))!.AsObject();
    }

    private static string FixturePath(string fileName)
    {
        return Path.Combine(AppContext.BaseDirectory, "Fixtures", fileName);
    }
}
