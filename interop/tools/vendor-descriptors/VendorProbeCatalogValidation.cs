// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Text.Json;

namespace Opc.Classic.VendorDescriptors;

public readonly record struct VendorProbeValidationError(string Path, string Message);

public sealed class VendorProbeValidationException : FormatException
{
    public VendorProbeValidationException()
        : base("Vendor probe catalog validation failed.")
    {
        Errors = [];
    }

    public VendorProbeValidationException(string? message)
        : base(message)
    {
        Errors = [];
    }

    public VendorProbeValidationException(string? message, Exception? innerException)
        : base(message, innerException)
    {
        Errors = [];
    }

    public VendorProbeValidationException(
        string source,
        IReadOnlyList<VendorProbeValidationError> errors,
        Exception? innerException = null)
        : base(CreateMessage(source, errors), innerException)
    {
        Errors = errors;
    }

    public IReadOnlyList<VendorProbeValidationError> Errors { get; }

    private static string CreateMessage(
        string source,
        IReadOnlyList<VendorProbeValidationError> errors)
    {
        return $"Vendor probe catalog '{source}' is invalid:{Environment.NewLine}" +
            string.Join(
                Environment.NewLine,
                errors.Select(error => $"{error.Path}: {error.Message}"));
    }
}

public static class VendorProbeCatalogValidator
{
    public const string SchemaVersion = "1.0";

    private static readonly HashSet<string> TargetKinds =
        new(["da", "ae", "hda"], StringComparer.Ordinal);

    private static readonly HashSet<string> Specifications =
        new(["da", "ae", "hda"], StringComparer.Ordinal);

    private static readonly HashSet<string> FixtureVariants =
        new(
            ["standard", "empty", "malformed", "truncated", "vendor-extension"],
            StringComparer.Ordinal);

    public static IReadOnlyList<VendorProbeValidationError> Validate(
        VendorProbeCatalogDescriptor? descriptor)
    {
        var errors = new List<VendorProbeValidationError>();
        if (descriptor is null)
        {
            errors.Add(new("$", "Descriptor is required."));
            return errors;
        }

        RequireEqual(descriptor.SchemaVersion, SchemaVersion, "$.schemaVersion", errors);
        ValidateId(descriptor.Id, "$.id", errors);
        RequireText(descriptor.Vendor, "$.vendor", errors);
        RequireText(descriptor.Product, "$.product", errors);
        ValidateTarget(descriptor.Target, errors);

        List<string> capabilities = descriptor.Capabilities ?? [];
        ValidateUniqueStrings(capabilities, "$.capabilities", errors);
        for (int index = 0; index < capabilities.Count; index++)
        {
            ValidateId(capabilities[index], $"$.capabilities[{index}]", errors);
        }

        ValidatePrerequisites(descriptor.Prerequisites, errors);
        ValidateArguments(descriptor.Arguments, errors);
        HashSet<string> fixtureIds = ValidateFixtures(descriptor.Fixtures, errors);
        ValidateProbes(descriptor.Probes, capabilities, fixtureIds, errors);
        ValidateLegal(descriptor.Legal, errors);
        return errors;
    }

    private static void ValidateTarget(
        VendorTarget? target,
        List<VendorProbeValidationError> errors)
    {
        if (target is null)
        {
            errors.Add(new("$.target", "Target is required."));
            return;
        }

        if (!TargetKinds.Contains(target.Kind))
        {
            errors.Add(new("$.target.kind", "Target kind must be da, ae, or hda."));
        }

        RequireText(target.Progid, "$.target.progid", errors);
        if (!Guid.TryParse(target.Clsid, out _))
        {
            errors.Add(new("$.target.clsid", "Target CLSID must be a GUID."));
        }
    }

    private static void ValidatePrerequisites(
        List<VendorPrerequisite>? prerequisites,
        List<VendorProbeValidationError> errors)
    {
        if (prerequisites is null)
        {
            errors.Add(new("$.prerequisites", "Prerequisite list is required."));
            return;
        }

        ValidateUnique(prerequisites, value => value.Id, "$.prerequisites", errors);
        for (int index = 0; index < prerequisites.Count; index++)
        {
            VendorPrerequisite? prerequisite = prerequisites[index];
            string path = $"$.prerequisites[{index}]";
            if (prerequisite is null)
            {
                errors.Add(new(path, "Null entries are forbidden."));
                continue;
            }

            ValidateId(prerequisite.Id, $"{path}.id", errors);
            RequireText(prerequisite.Description, $"{path}.description", errors);
            if (prerequisite.Artifact is not null)
            {
                if (!IsRootToken(prerequisite.Artifact.RootToken))
                {
                    errors.Add(new(
                        $"{path}.artifact.rootToken",
                        "Root token must use upper-case letters, digits, and underscores."));
                }

                if (!IsSafeRelativePath(prerequisite.Artifact.RelativePath))
                {
                    errors.Add(new(
                        $"{path}.artifact.relativePath",
                        "Artifact path must be safe and relative."));
                }
            }
        }
    }

    private static void ValidateArguments(
        Dictionary<string, JsonElement>? arguments,
        List<VendorProbeValidationError> errors)
    {
        if (arguments is null)
        {
            errors.Add(new("$.arguments", "Arguments object is required."));
            return;
        }

        foreach ((string name, JsonElement value) in arguments)
        {
            if (!Specifications.Contains(name))
            {
                errors.Add(new($"$.arguments.{name}", "Only da, ae, and hda arguments are allowed."));
            }

            ValidateFiniteNumbers(value, $"$.arguments.{name}", errors);
        }

        if (arguments.TryGetValue("da", out JsonElement da) &&
            da.ValueKind == JsonValueKind.Object)
        {
            int itemCount = GetArrayLength(da, "itemIds");
            int handleCount = GetArrayLength(da, "clientHandles");
            int valueCount = GetArrayLength(da, "writeValues");
            if (itemCount == 0 || itemCount != handleCount || itemCount != valueCount)
            {
                errors.Add(new(
                    "$.arguments.da",
                    "itemIds, clientHandles, and writeValues must have identical non-zero lengths."));
            }
        }
    }

    private static HashSet<string> ValidateFixtures(
        List<VendorFixture>? fixtures,
        List<VendorProbeValidationError> errors)
    {
        var fixtureIds = new HashSet<string>(StringComparer.Ordinal);
        if (fixtures is null)
        {
            errors.Add(new("$.fixtures", "Fixture list is required."));
            return fixtureIds;
        }

        for (int index = 0; index < fixtures.Count; index++)
        {
            VendorFixture? fixture = fixtures[index];
            string path = $"$.fixtures[{index}]";
            if (fixture is null)
            {
                errors.Add(new(path, "Null entries are forbidden."));
                continue;
            }

            ValidateId(fixture.Id, $"{path}.id", errors);
            if (!fixtureIds.Add(fixture.Id))
            {
                errors.Add(new($"{path}.id", $"Duplicate fixture id '{fixture.Id}'."));
            }

            if (!Specifications.Contains(fixture.Specification))
            {
                errors.Add(new($"{path}.specification", "Unsupported specification."));
            }

            if (!FixtureVariants.Contains(fixture.Variant))
            {
                errors.Add(new($"{path}.variant", "Unsupported fixture variant."));
            }

            if (!string.Equals(fixture.Encoding, "hex", StringComparison.Ordinal))
            {
                errors.Add(new($"{path}.encoding", "Only hex fixtures are supported."));
            }

            if (!fixture.Redistributable)
            {
                errors.Add(new($"{path}.redistributable", "Fixtures must be redistributable."));
            }

            if (fixture.ExpectedDecode is not ("success" or "failure"))
            {
                errors.Add(new($"{path}.expectedDecode", "Unsupported decode outcome."));
            }

            if (!IsSafeFixturePath(fixture.Path))
            {
                errors.Add(new($"{path}.path", "Fixture path must be beneath fixtures/."));
            }
        }

        return fixtureIds;
    }

    private static void ValidateProbes(
        List<VendorProbeScenario>? probes,
        List<string> capabilities,
        HashSet<string> fixtureIds,
        List<VendorProbeValidationError> errors)
    {
        if (probes is null || probes.Count == 0)
        {
            errors.Add(new("$.probes", "At least one probe is required."));
            return;
        }

        var probeIds = new HashSet<string>(StringComparer.Ordinal);
        var capabilitySet = new HashSet<string>(capabilities, StringComparer.Ordinal);
        for (int index = 0; index < probes.Count; index++)
        {
            VendorProbeScenario? probe = probes[index];
            string path = $"$.probes[{index}]";
            if (probe is null)
            {
                errors.Add(new(path, "Null entries are forbidden."));
                continue;
            }

            ValidateId(probe.Id, $"{path}.id", errors);
            if (!probeIds.Add(probe.Id))
            {
                errors.Add(new($"{path}.id", $"Duplicate probe id '{probe.Id}'."));
            }

            ValidateId(probe.Type, $"{path}.type", errors);
            foreach (string capability in probe.Requires ?? [])
            {
                if (!capabilitySet.Contains(capability))
                {
                    errors.Add(new(
                        $"{path}.requires",
                        $"Capability '{capability}' is not declared."));
                }
            }

            if (probe.Type == "fixture-decode" &&
                (probe.FixtureId is null || !fixtureIds.Contains(probe.FixtureId)))
            {
                errors.Add(new(
                    $"{path}.fixtureId",
                    "Fixture decode probe must reference a declared fixture id."));
            }

            ValidateExpectation(probe.Expected, $"{path}.expected", errors);
        }
    }

    private static void ValidateExpectation(
        VendorProbeExpectation? expected,
        string path,
        List<VendorProbeValidationError> errors)
    {
        if (expected is null)
        {
            errors.Add(new(path, "Expected result is required."));
            return;
        }

        if (expected.Outcome is not ("success" or "failure" or "skip"))
        {
            errors.Add(new($"{path}.outcome", "Unsupported expected outcome."));
        }

        if (expected.MinimumCount < 0)
        {
            errors.Add(new($"{path}.minimumCount", "Minimum count cannot be negative."));
        }

        if (expected.HResult is JsonElement hresult)
        {
            ValidateFiniteNumbers(hresult, $"{path}.hResult", errors);
        }
    }

    private static void ValidateLegal(
        VendorLegalMetadata? legal,
        List<VendorProbeValidationError> errors)
    {
        if (legal is null)
        {
            errors.Add(new("$.legal", "Legal metadata is required."));
            return;
        }

        if (!Uri.TryCreate(legal.SourceUrl, UriKind.Absolute, out Uri? source) ||
            source.Scheme != Uri.UriSchemeHttps ||
            source.UserInfo.Length != 0)
        {
            errors.Add(new("$.legal.sourceUrl", "Source URL must be absolute HTTPS without user information."));
        }
    }

    private static void ValidateFiniteNumbers(
        JsonElement value,
        string path,
        List<VendorProbeValidationError> errors)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Number:
                if (value.TryGetDouble(out double number) && !double.IsFinite(number))
                {
                    errors.Add(new(path, "Non-finite numbers are forbidden."));
                }

                break;
            case JsonValueKind.Object:
                foreach (JsonProperty property in value.EnumerateObject())
                {
                    ValidateFiniteNumbers(property.Value, $"{path}.{property.Name}", errors);
                }

                break;
            case JsonValueKind.Array:
                int index = 0;
                foreach (JsonElement item in value.EnumerateArray())
                {
                    ValidateFiniteNumbers(item, $"{path}[{index}]", errors);
                    index++;
                }

                break;
        }
    }

    private static int GetArrayLength(JsonElement value, string propertyName)
    {
        return value.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Array
            ? property.GetArrayLength()
            : 0;
    }

    private static bool IsSafeRelativePath(string value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
            !Path.IsPathFullyQualified(value) &&
            value.Split(['/', '\\']).All(segment => segment is not ("" or "." or ".."));
    }

    private static bool IsSafeFixturePath(string value)
    {
        return value.StartsWith("fixtures/", StringComparison.Ordinal) &&
            !value.Contains('\\') &&
            IsSafeRelativePath(value);
    }

    private static bool IsRootToken(string value)
    {
        return !string.IsNullOrEmpty(value) &&
            value[0] is >= 'A' and <= 'Z' &&
            value.All(character =>
                character is >= 'A' and <= 'Z' or >= '0' and <= '9' or '_');
    }

    private static void ValidateId(
        string? value,
        string path,
        List<VendorProbeValidationError> errors)
    {
        if (string.IsNullOrEmpty(value) ||
            value[0] == '-' ||
            value[^1] == '-' ||
            value.Any(character =>
                !(character is >= 'a' and <= 'z' or >= '0' and <= '9' or '-')))
        {
            errors.Add(new(path, "Value must be a lower-case kebab-case id."));
        }
    }

    private static void ValidateUniqueStrings(
        List<string> values,
        string path,
        List<VendorProbeValidationError> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < values.Count; index++)
        {
            if (!seen.Add(values[index]))
            {
                errors.Add(new($"{path}[{index}]", $"Duplicate value '{values[index]}'."));
            }
        }
    }

    private static void ValidateUnique<T>(
        IEnumerable<T> values,
        Func<T, string> getKey,
        string path,
        List<VendorProbeValidationError> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        int index = 0;
        foreach (T value in values)
        {
            string key = getKey(value);
            if (!seen.Add(key))
            {
                errors.Add(new($"{path}[{index}].id", $"Duplicate id '{key}'."));
            }

            index++;
        }
    }

    private static void RequireText(
        string? value,
        string path,
        List<VendorProbeValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(new(path, "Value is required."));
        }
    }

    private static void RequireEqual(
        string actual,
        string expected,
        string path,
        List<VendorProbeValidationError> errors)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            errors.Add(new(path, $"Expected '{expected}', got '{actual}'."));
        }
    }
}
