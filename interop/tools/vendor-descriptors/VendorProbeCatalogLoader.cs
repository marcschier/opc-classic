// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Text.Json;

namespace Opc.Classic.VendorDescriptors;

public static class VendorProbeCatalogLoader
{
    private const int MaximumBytes = 256 * 1024;

    private static readonly HashSet<string> ForbiddenNames =
        new(
            [
                "password", "secret", "token", "binary", "payload", "base64",
                "command", "script", "setupCommand", "installCommand",
            ],
            StringComparer.OrdinalIgnoreCase);

    public static VendorProbeCatalogDescriptor Load(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var file = new FileInfo(path);
        if (!file.Exists)
        {
            throw new FileNotFoundException("Vendor probe catalog was not found.", path);
        }

        if (file.Length > MaximumBytes)
        {
            throw new VendorProbeValidationException(
                path,
                [new("$", $"Descriptor exceeds {MaximumBytes} bytes.")]);
        }

        return LoadJson(File.ReadAllText(path), path);
    }

    public static VendorProbeCatalogDescriptor LoadJson(
        string json,
        string source = "<memory>")
    {
        ArgumentNullException.ThrowIfNull(json);
        try
        {
            using JsonDocument document = JsonDocument.Parse(
                json,
                new JsonDocumentOptions
                {
                    AllowTrailingCommas = false,
                    CommentHandling = JsonCommentHandling.Disallow,
                    MaxDepth = 64,
                });
            var securityErrors = new List<VendorProbeValidationError>();
            ValidateSafeContent(document.RootElement, "$", securityErrors);
            if (securityErrors.Count > 0)
            {
                throw new VendorProbeValidationException(source, securityErrors);
            }

            VendorProbeCatalogDescriptor? descriptor = JsonSerializer.Deserialize(
                json,
                VendorProbeCatalogJsonContext.Default.VendorProbeCatalogDescriptor);
            IReadOnlyList<VendorProbeValidationError> errors =
                VendorProbeCatalogValidator.Validate(descriptor);
            if (errors.Count > 0)
            {
                throw new VendorProbeValidationException(source, errors);
            }

            return descriptor!;
        }
        catch (VendorProbeValidationException)
        {
            throw;
        }
        catch (JsonException exception)
        {
            throw new VendorProbeValidationException(
                source,
                [new("$", $"Invalid descriptor JSON: {exception.Message}")],
                exception);
        }
    }

    private static void ValidateSafeContent(
        JsonElement value,
        string path,
        List<VendorProbeValidationError> errors)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (JsonProperty property in value.EnumerateObject())
                {
                    string childPath = $"{path}.{property.Name}";
                    if (ForbiddenNames.Contains(property.Name))
                    {
                        errors.Add(new(
                            childPath,
                            "Sensitive, binary, script, and command content is forbidden."));
                    }

                    ValidateSafeContent(property.Value, childPath, errors);
                }

                break;
            case JsonValueKind.Array:
                int index = 0;
                foreach (JsonElement item in value.EnumerateArray())
                {
                    ValidateSafeContent(item, $"{path}[{index}]", errors);
                    index++;
                }

                break;
            case JsonValueKind.String:
                string text = value.GetString()!;
                if (Path.IsPathFullyQualified(text) || text.StartsWith(@"\\", StringComparison.Ordinal))
                {
                    errors.Add(new(path, "Absolute and UNC paths are forbidden."));
                }

                break;
        }
    }
}
