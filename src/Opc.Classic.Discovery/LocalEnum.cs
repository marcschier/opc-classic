//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using Opc.Classic;

namespace Opc.Classic.Discovery;

/// <summary>
/// Discovers locally configured OPC Classic servers and, on Windows, OPC COM registrations.
/// </summary>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Discovery strategies are intentionally named after OPC enumeration sources.")]
public sealed class LocalEnum : IOpcDiscovery
{
    private static readonly Guid[] OpcCategoryIds =
    {
        OpcGuids.CATID_OPCDAServer10,
        OpcGuids.CATID_OPCDAServer20,
        OpcGuids.CATID_OPCDAServer30,
        OpcGuids.CATID_OPCAEServer10,
        OpcGuids.CATID_OPCHDAServer10,
        OpcGuids.CATID_OPCDXServer10,
        OpcGuids.CATID_OPCBatchServer10,
        OpcGuids.CATID_OPCBatchServer20,
        OpcGuids.CATID_OPCCMDServer10,
        OpcGuids.CATID_XMLDAServer10,
    };

    private readonly IReadOnlyList<OpcServerEntry> _configuredEntries;
    private readonly bool _includeWindowsRegistry;

    /// <summary>
    /// Initializes a local enumerator from <c>Opc.Classic:Servers</c> configuration.
    /// </summary>
    public LocalEnum(IConfiguration configuration, bool includeWindowsRegistry = true)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _configuredEntries = ReadConfiguredEntries(configuration);
        _includeWindowsRegistry = includeWindowsRegistry;
    }

    /// <summary>
    /// Initializes a local enumerator from an explicit in-memory server list.
    /// </summary>
    public LocalEnum(IEnumerable<OpcServerEntry> entries, bool includeWindowsRegistry = true)
    {
        ArgumentNullException.ThrowIfNull(entries);

        _configuredEntries = entries.ToArray();
        _includeWindowsRegistry = includeWindowsRegistry;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (var entry in _configuredEntries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (host is null || IsHostMatch(entry.Host, host))
            {
                yield return entry;
            }
        }

        if (!_includeWindowsRegistry || !IsLocalHost(host) || !OperatingSystem.IsWindows())
        {
            yield break;
        }

        foreach (var entry in EnumerateWindowsClsids())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return entry;
        }
    }

    private static List<OpcServerEntry> ReadConfiguredEntries(IConfiguration configuration)
    {
        var entries = new List<OpcServerEntry>();
        foreach (var section in configuration.GetSection("Opc.Classic:Servers").GetChildren())
        {
            var entry = ReadConfiguredEntry(section);
            if (entry is not null)
            {
                entries.Add(entry);
            }
        }

        return entries;
    }

    private static OpcServerEntry? ReadConfiguredEntry(IConfiguration section)
    {
        var clsidText = section["Clsid"];
        if (!Guid.TryParse(clsidText, out var clsid))
        {
            return null;
        }

        var progId = section["ProgId"];
        if (string.IsNullOrWhiteSpace(progId))
        {
            return null;
        }

        var friendlyName = section["FriendlyName"];
        if (string.IsNullOrWhiteSpace(friendlyName))
        {
            friendlyName = progId;
        }

        var host = section["Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            host = "localhost";
        }

        return new OpcServerEntry(
            clsid,
            progId,
            friendlyName,
            host,
            ReadCategoryIds(section.GetSection("SupportedCategories")));
    }

    private static List<Guid> ReadCategoryIds(IConfiguration section)
    {
        var categoryIds = new List<Guid>();
        foreach (var child in section.GetChildren())
        {
            if (Guid.TryParse(child.Value, out var categoryId))
            {
                categoryIds.Add(categoryId);
            }
        }

        return categoryIds;
    }

    private static bool IsHostMatch(string entryHost, string requestedHost) =>
        string.Equals(entryHost, requestedHost, StringComparison.OrdinalIgnoreCase)
        || (IsLocalHost(entryHost) && IsLocalHost(requestedHost));

    private static bool IsLocalHost(string? host) =>
        string.IsNullOrWhiteSpace(host)
        || string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
        || string.Equals(host, ".", StringComparison.OrdinalIgnoreCase)
        || string.Equals(host, Environment.MachineName, StringComparison.OrdinalIgnoreCase);

    [SupportedOSPlatform("windows")]
    private static IEnumerable<OpcServerEntry> EnumerateWindowsClsids()
    {
        using var classes = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Classes\CLSID");
        if (classes is null)
        {
            yield break;
        }

        foreach (var clsidText in classes.GetSubKeyNames())
        {
            if (!Guid.TryParse(clsidText, out var clsid))
            {
                continue;
            }

            using var clsidKey = classes.OpenSubKey(clsidText);
            if (clsidKey is null)
            {
                continue;
            }

            var supportedCategories = EnumerateImplementedOpcCategories(clsidKey).ToArray();
            if (supportedCategories.Length == 0)
            {
                continue;
            }

            var progId = ReadDefaultSubKeyValue(clsidKey, "ProgID")
                ?? ReadDefaultSubKeyValue(clsidKey, "VersionIndependentProgID")
                ?? clsid.ToString("B");
            var friendlyName = clsidKey.GetValue(null) as string;

            yield return new OpcServerEntry(
                clsid,
                progId,
                string.IsNullOrWhiteSpace(friendlyName) ? progId : friendlyName,
                "localhost",
                supportedCategories);
        }
    }

    [SupportedOSPlatform("windows")]
    private static IEnumerable<Guid> EnumerateImplementedOpcCategories(RegistryKey clsidKey)
    {
        using var categories = clsidKey.OpenSubKey("Implemented Categories");
        if (categories is null)
        {
            yield break;
        }

        foreach (var categoryText in categories.GetSubKeyNames())
        {
            if (Guid.TryParse(categoryText, out var categoryId) && IsOpcCategory(categoryId))
            {
                yield return categoryId;
            }
        }
    }

    [SupportedOSPlatform("windows")]
    private static string? ReadDefaultSubKeyValue(RegistryKey parentKey, string subKeyName)
    {
        using var subKey = parentKey.OpenSubKey(subKeyName);
        return subKey?.GetValue(null) as string;
    }

    private static bool IsOpcCategory(Guid categoryId)
    {
        foreach (var opcCategoryId in OpcCategoryIds)
        {
            if (opcCategoryId == categoryId)
            {
                return true;
            }
        }

        return false;
    }
}
