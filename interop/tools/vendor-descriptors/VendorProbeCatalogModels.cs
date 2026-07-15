// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Opc.Classic.VendorDescriptors;

public sealed class VendorProbeCatalogDescriptor
{
    public required string SchemaVersion { get; init; }

    public required string Id { get; init; }

    public required string Vendor { get; init; }

    public required string Product { get; init; }

    public required VendorTarget Target { get; init; }

    public required List<string> Capabilities { get; init; }

    public required List<VendorPrerequisite> Prerequisites { get; init; }

    public required Dictionary<string, JsonElement> Arguments { get; init; }

    public required List<VendorFixture> Fixtures { get; init; }

    public required List<VendorProbeScenario> Probes { get; init; }

    public required VendorLegalMetadata Legal { get; init; }
}

public sealed class VendorTarget
{
    public required string Kind { get; init; }

    public required string Progid { get; init; }

    public required string Clsid { get; init; }
}

public sealed class VendorPrerequisite
{
    public required string Id { get; init; }

    public required string Description { get; init; }

    public required bool Required { get; init; }

    public VendorArtifact? Artifact { get; init; }
}

public sealed class VendorArtifact
{
    public required string RootToken { get; init; }

    public required string RelativePath { get; init; }
}

public sealed class VendorFixture
{
    public required string Id { get; init; }

    public required string Specification { get; init; }

    public required string Variant { get; init; }

    public required string Path { get; init; }

    public required string Encoding { get; init; }

    public required bool Redistributable { get; init; }

    public required string ExpectedDecode { get; init; }
}

public sealed class VendorProbeScenario
{
    public required string Id { get; init; }

    public required string Type { get; init; }

    public required List<string> Requires { get; init; }

    public string? Tool { get; init; }

    public string? FixtureId { get; init; }

    public required VendorProbeExpectation Expected { get; init; }

    public required List<VendorExpectedFailure> ExpectedFailures { get; init; }
}

public sealed class VendorProbeExpectation
{
    public required string Outcome { get; init; }

    public int? MinimumCount { get; init; }

    public string? ItemId { get; init; }

    public JsonElement? HResult { get; init; }
}

public sealed class VendorExpectedFailure
{
    public required string Code { get; init; }

    public required string Description { get; init; }
}

public sealed class VendorLegalMetadata
{
    public required bool RedistributionAllowed { get; init; }

    public required string SourceUrl { get; init; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow)]
[JsonSerializable(typeof(VendorProbeCatalogDescriptor))]
public sealed partial class VendorProbeCatalogJsonContext : JsonSerializerContext;
