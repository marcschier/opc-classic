//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.ComponentModel;
using ModelContextProtocol.Server;
using Opc.Classic.Discovery;
using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Tools;

/// <summary>MCP tools for OPC Classic server discovery.</summary>
public sealed class DiscoveryTools
{
    private readonly IReadOnlyList<IOpcDiscovery> _discoveries;

    /// <summary>Creates the discovery tool set.</summary>
    public DiscoveryTools(IEnumerable<IOpcDiscovery> discoveries) =>
        _discoveries = (discoveries ?? throw new ArgumentNullException(nameof(discoveries))).ToArray();

    /// <summary>Enumerates registered OPC Classic servers on a host.</summary>
    [McpServerTool(Name = "opcclassic.discovery.enumerate_servers", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Enumerates OPC Classic server registrations on a host through OPCEnum / OPC.ServerList.1.")]
    public async Task<IReadOnlyList<OpcServerDescriptorDto>> EnumerateServers(
        [Description("Host name or IP address to query. Use localhost for the local machine.")]
        string host = "localhost",
        [Description("Optional OPC category GUID strings to filter, such as CATID_OPCDAServer20 or CATID_OPCDAServer30. Omit for the default OPCEnum categories.")]
        string[]? categoryIds = null,
        CancellationToken cancellationToken = default)
    {
        string targetHost = string.IsNullOrWhiteSpace(host) ? "localhost" : host.Trim();
        Guid[] categories = ParseCategoryIds(categoryIds);

        if (_discoveries.Count > 0)
        {
            return await EnumerateInjectedDiscoveryAsync(_discoveries[0], targetHost, categories, cancellationToken).ConfigureAwait(false);
        }

        OpcServerDescriptor[] descriptors = await OpcDiscovery.EnumerateAsync(
            targetHost,
            categories.Length == 0 ? null : categories,
            cancellationToken).ConfigureAwait(false);
        return descriptors.Select(descriptor => ToDto(descriptor, targetHost)).ToArray();
    }

    private static async Task<IReadOnlyList<OpcServerDescriptorDto>> EnumerateInjectedDiscoveryAsync(
        IOpcDiscovery discovery,
        string host,
        IReadOnlyList<Guid> categories,
        CancellationToken cancellationToken)
    {
        if (discovery is OpcEnumClient enumClient)
        {
            OpcServerDescriptor[] descriptors = await enumClient.EnumerateAsync(
                host,
                categories.Count == 0 ? null : categories,
                cancellationToken).ConfigureAwait(false);
            return descriptors.Select(descriptor => ToDto(descriptor, host)).ToArray();
        }

        var results = new List<OpcServerDescriptorDto>();
        await foreach (OpcServerEntry entry in discovery.DiscoverAsync(host, cancellationToken).ConfigureAwait(false))
        {
            results.Add(new OpcServerDescriptorDto(
                entry.Clsid,
                entry.ProgId,
                entry.FriendlyName,
                VerIndProgId: null,
                entry.SupportedCategories,
                entry.Host));
        }

        return results;
    }

    private static OpcServerDescriptorDto ToDto(OpcServerDescriptor descriptor, string host) =>
        new(descriptor.ClassId, descriptor.ProgId, descriptor.UserType, descriptor.VerIndProgId, descriptor.Categories, host);

    private static Guid[] ParseCategoryIds(IReadOnlyList<string>? categoryIds)
    {
        if (categoryIds is null || categoryIds.Count == 0)
        {
            return [];
        }

        var categories = new List<Guid>(categoryIds.Count);
        foreach (string categoryId in categoryIds)
        {
            if (!Guid.TryParse(categoryId, out Guid category))
            {
                throw new ArgumentException($"Category '{categoryId}' is not a GUID.", nameof(categoryIds));
            }

            categories.Add(category);
        }

        return categories.ToArray();
    }
}
