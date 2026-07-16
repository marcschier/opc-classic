// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Opc.Classic.Mcp.Tests;

public sealed class McpPackageContentTests
{
    private const string PinnedSchemaUri = "https://static.modelcontextprotocol.io/schemas/2025-10-17/server.schema.json";
    private const string VersionPlaceholder = "0.0.0-placeholder";
    private const string ExpectedDescription = "Cross-platform MCP server for OPC Classic discovery and DA, AE, HDA, and XML-DA clients.";
    private static readonly byte[] VersionPlaceholderBytes = "0.0.0-placeholder"u8.ToArray();
    private static readonly string[] ExpectedPackageIds =
    [
        "Opc.Classic.Mcp",
        "Opc.Classic.Mcp.linux-arm64",
        "Opc.Classic.Mcp.linux-musl-x64",
        "Opc.Classic.Mcp.linux-x64",
        "Opc.Classic.Mcp.osx-arm64",
        "Opc.Classic.Mcp.win-arm64",
        "Opc.Classic.Mcp.win-x64",
    ];
    private static readonly Regex ServerNamePattern = new(
        "^[a-zA-Z0-9.-]+/[a-zA-Z0-9._-]+$",
        RegexOptions.CultureInvariant | RegexOptions.NonBacktracking);

    [Test]
    [Category("Packaging")]
    public async Task Pack_Emits_stamped_registry_metadata_in_base_and_all_rid_packages()
    {
        string repositoryRoot = FindRepositoryRoot();
        string outputDirectory = Path.Combine(AppContext.BaseDirectory, $"mcp-package-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outputDirectory);

        try
        {
            ProcessResult result = await PackAsync(repositoryRoot, outputDirectory);
            await Assert.That(result.ExitCode).IsEqualTo(0)
                .Because($"dotnet pack failed.{Environment.NewLine}{result.StandardOutput}{Environment.NewLine}{result.StandardError}");

            string[] packagePaths = Directory.GetFiles(outputDirectory, "*.nupkg");
            await Assert.That(packagePaths.Length).IsEqualTo(ExpectedPackageIds.Length);

            var actualPackageIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string packagePath in packagePaths)
            {
                string packageId = await ValidatePackageAsync(packagePath);
                actualPackageIds.Add(packageId);
            }

            foreach (string expectedPackageId in ExpectedPackageIds)
            {
                await Assert.That(actualPackageIds.Contains(expectedPackageId)).IsTrue()
                    .Because($"Package '{expectedPackageId}' was not emitted.");
            }
        }

        finally
        {
            Directory.Delete(outputDirectory, recursive: true);
        }
    }

    private static async Task<string> ValidatePackageAsync(string packagePath)
    {
        using ZipArchive archive = ZipFile.OpenRead(packagePath);
        ZipArchiveEntry nuspecEntry = archive.Entries.Single(entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal));
        XDocument nuspec = XDocument.Parse(ReadEntry(nuspecEntry));
        XNamespace ns = nuspec.Root?.Name.Namespace ?? throw new InvalidDataException("The package nuspec has no root element.");
        XElement metadata = nuspec.Root?.Element(ns + "metadata") ?? throw new InvalidDataException("The package nuspec has no metadata element.");
        string packageId = metadata.Element(ns + "id")?.Value ?? throw new InvalidDataException("The package nuspec has no id.");
        string packageVersion = metadata.Element(ns + "version")?.Value ?? throw new InvalidDataException("The package nuspec has no version.");

        ZipArchiveEntry[] serverEntries = archive.Entries
            .Where(entry => entry.FullName.EndsWith(".mcp/server.json", StringComparison.Ordinal))
            .ToArray();
        await Assert.That(serverEntries.Length).IsEqualTo(1)
            .Because($"Package '{packageId}' must contain exactly one generated .mcp/server.json.");
        ZipArchiveEntry serverEntry = serverEntries.SingleOrDefault()
            ?? throw new InvalidDataException($"Package '{packageId}' does not contain .mcp/server.json.");
        await Assert.That(serverEntry.FullName).IsEqualTo(".mcp/server.json");

        var placeholderEntries = new List<string>();
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            if (ContainsVersionPlaceholder(entry))
            {
                placeholderEntries.Add(entry.FullName);
            }
        }
        await Assert.That(placeholderEntries.Count).IsEqualTo(0)
            .Because($"Package '{packageId}' contains {VersionPlaceholder} in: {string.Join(", ", placeholderEntries)}");

        using JsonDocument server = JsonDocument.Parse(ReadEntry(serverEntry));
        JsonElement root = server.RootElement;
        JsonElement package = root.GetProperty("packages")[0];
        IReadOnlyList<string> schemaErrors = ValidatePinnedServerSchema(root);

        await Assert.That(schemaErrors.Count).IsEqualTo(0)
            .Because($"{packageId}:{Environment.NewLine}{string.Join(Environment.NewLine, schemaErrors)}");
        await Assert.That(root.GetProperty("$schema").GetString()).IsEqualTo(PinnedSchemaUri);
        await Assert.That(root.GetProperty("name").GetString()).IsEqualTo("io.github.marcschier/opc-classic");
        await Assert.That(root.GetProperty("description").GetString()).IsEqualTo(ExpectedDescription);
        await Assert.That(root.GetProperty("version").GetString()).IsEqualTo(packageVersion);
        await Assert.That(root.GetProperty("version").GetString()).IsNotEqualTo(VersionPlaceholder);
        await Assert.That(package.GetProperty("registryType").GetString()).IsEqualTo("nuget");
        await Assert.That(package.GetProperty("identifier").GetString()).IsEqualTo("Opc.Classic.Mcp");
        await Assert.That(package.GetProperty("version").GetString()).IsEqualTo(packageVersion);
        await Assert.That(package.GetProperty("version").GetString()).IsNotEqualTo(VersionPlaceholder);
        await Assert.That(package.GetProperty("transport").GetProperty("type").GetString()).IsEqualTo("stdio");
        await Assert.That(package.GetProperty("packageArguments").GetArrayLength()).IsEqualTo(0);
        await Assert.That(package.GetProperty("environmentVariables").GetArrayLength()).IsEqualTo(0);
        await Assert.That(root.GetProperty("repository").GetProperty("url").GetString()).IsEqualTo("https://github.com/marcschier/opc-classic");
        await Assert.That(root.GetProperty("repository").GetProperty("source").GetString()).IsEqualTo("github");
        return packageId;
    }

    private static bool ContainsVersionPlaceholder(ZipArchiveEntry entry)
    {
        using Stream stream = entry.Open();
        int overlap = VersionPlaceholderBytes.Length - 1;
        var buffer = new byte[8192 + overlap];
        int carry = 0;

        while (true)
        {
            int read = stream.Read(buffer, carry, 8192);
            if (read == 0)
            {
                return false;
            }

            int length = carry + read;
            if (buffer.AsSpan(0, length).IndexOf(VersionPlaceholderBytes) >= 0)
            {
                return true;
            }

            carry = Math.Min(overlap, length);
            buffer.AsSpan(length - carry, carry).CopyTo(buffer);
        }
    }

    private static IReadOnlyList<string> ValidatePinnedServerSchema(JsonElement root)
    {
        var errors = new List<string>();
        if (root.ValueKind != JsonValueKind.Object)
        {
            errors.Add("The document root must be an object.");
            return errors;
        }

        ValidateRequiredString(root, "$schema", minLength: 1, maxLength: int.MaxValue, errors);
        ValidateRequiredString(root, "description", minLength: 1, maxLength: 100, errors);
        ValidateRequiredString(root, "name", minLength: 3, maxLength: 200, errors, ServerNamePattern);
        ValidateRequiredString(root, "version", minLength: 0, maxLength: 255, errors);

        if (root.TryGetProperty("$schema", out JsonElement schema) &&
            (!Uri.TryCreate(schema.GetString(), UriKind.Absolute, out Uri? schemaUri) ||
             !string.Equals(schemaUri.AbsoluteUri, PinnedSchemaUri, StringComparison.Ordinal)))
        {
            errors.Add($"$schema must be the pinned URI {PinnedSchemaUri}.");
        }

        if (!root.TryGetProperty("packages", out JsonElement packages) || packages.ValueKind != JsonValueKind.Array)
        {
            errors.Add("packages must be an array.");
        }
        else
        {
            foreach (JsonElement package in packages.EnumerateArray())
            {
                if (package.ValueKind != JsonValueKind.Object)
                {
                    errors.Add("Each packages entry must be an object.");
                    continue;
                }

                ValidateRequiredString(package, "registryType", minLength: 1, maxLength: int.MaxValue, errors);
                ValidateRequiredString(package, "identifier", minLength: 1, maxLength: int.MaxValue, errors);
                if (package.TryGetProperty("version", out JsonElement packageVersion))
                {
                    ValidateString(packageVersion, "packages[].version", minLength: 1, maxLength: int.MaxValue, errors);
                    if (string.Equals(packageVersion.GetString(), "latest", StringComparison.Ordinal))
                    {
                        errors.Add("packages[].version must be a specific version, not latest.");
                    }
                }

                ValidateArrayWhenPresent(package, "packageArguments", errors);
                ValidateArrayWhenPresent(package, "environmentVariables", errors);
                if (!package.TryGetProperty("transport", out JsonElement transport) || transport.ValueKind != JsonValueKind.Object)
                {
                    errors.Add("packages[].transport must be an object.");
                }
                else if (!transport.TryGetProperty("type", out JsonElement transportType) ||
                         transportType.ValueKind != JsonValueKind.String ||
                         !string.Equals(transportType.GetString(), "stdio", StringComparison.Ordinal))
                {
                    errors.Add("The packaged transport must have string type stdio.");
                }
            }
        }

        if (!root.TryGetProperty("repository", out JsonElement repository) || repository.ValueKind != JsonValueKind.Object)
        {
            errors.Add("repository must be an object.");
        }
        else
        {
            ValidateRequiredString(repository, "url", minLength: 1, maxLength: int.MaxValue, errors);
            ValidateRequiredString(repository, "source", minLength: 1, maxLength: int.MaxValue, errors);
            if (repository.TryGetProperty("url", out JsonElement repositoryUrl) &&
                !Uri.TryCreate(repositoryUrl.GetString(), UriKind.Absolute, out _))
            {
                errors.Add("repository.url must be an absolute URI.");
            }
        }

        return errors;
    }

    private static void ValidateRequiredString(
        JsonElement parent,
        string propertyName,
        int minLength,
        int maxLength,
        ICollection<string> errors,
        Regex? pattern = null)
    {
        if (!parent.TryGetProperty(propertyName, out JsonElement value))
        {
            errors.Add($"{propertyName} is required.");
            return;
        }

        ValidateString(value, propertyName, minLength, maxLength, errors, pattern);
    }

    private static void ValidateString(
        JsonElement value,
        string propertyName,
        int minLength,
        int maxLength,
        ICollection<string> errors,
        Regex? pattern = null)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            errors.Add($"{propertyName} must be a string.");
            return;
        }

        string text = value.GetString() ?? string.Empty;
        if (text.Length < minLength || text.Length > maxLength)
        {
            errors.Add($"{propertyName} length must be between {minLength} and {maxLength} characters.");
        }
        if (pattern is not null && !pattern.IsMatch(text))
        {
            errors.Add($"{propertyName} does not match the pinned schema pattern.");
        }
    }

    private static void ValidateArrayWhenPresent(JsonElement parent, string propertyName, ICollection<string> errors)
    {
        if (parent.TryGetProperty(propertyName, out JsonElement value) && value.ValueKind != JsonValueKind.Array)
        {
            errors.Add($"packages[].{propertyName} must be an array.");
        }
    }

    private static async Task<ProcessResult> PackAsync(string repositoryRoot, string outputDirectory)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = repositoryRoot,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };
        process.StartInfo.ArgumentList.Add("pack");
        process.StartInfo.ArgumentList.Add(Path.Combine(repositoryRoot, "mcp", "Opc.Classic.Mcp", "Opc.Classic.Mcp.csproj"));
        process.StartInfo.ArgumentList.Add("--no-restore");
        process.StartInfo.ArgumentList.Add("--configuration");
        process.StartInfo.ArgumentList.Add("Release");
        process.StartInfo.ArgumentList.Add($"-p:PackageOutputPath={outputDirectory}");
        process.StartInfo.ArgumentList.Add("--verbosity");
        process.StartInfo.ArgumentList.Add("minimal");

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start dotnet pack.");
        }

        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        TimeSpan timeoutDuration = OperatingSystem.IsWindows()
            ? TimeSpan.FromMinutes(10)
            : TimeSpan.FromMinutes(30);
        using var timeout = new CancellationTokenSource(timeoutDuration);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException(
                $"dotnet pack did not complete within {timeoutDuration.TotalMinutes:0} minutes.");
        }

        return new ProcessResult(process.ExitCode, await standardOutput, await standardError);
    }

    private static string ReadEntry(ZipArchiveEntry entry)
    {
        using Stream stream = entry.Open();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Opc.Classic.slnx")))
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
