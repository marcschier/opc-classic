//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.ComponentModel;
using System.Net;
using ModelContextProtocol.Server;
using Opc.Classic;
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
        [Description("Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\\user when a Windows domain is required.")]
        string? username = null,
        [Description("Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous discovery.")]
        string? password = null,
        [Description("True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied.")]
        bool useKerberos = false,
        [Description("True to authenticate using the current Windows logon via NegotiateAuthentication (no username/password needed). Windows-only.")]
        bool useSso = false,
        [Description(OpcMcpAuthLevel.Description)]
        string? authLevel = null,
        CancellationToken cancellationToken = default)
    {
        string targetHost = string.IsNullOrWhiteSpace(host) ? "localhost" : host.Trim();
        Guid[] categories = ParseCategoryIds(categoryIds);

        if (_discoveries.Count > 0)
        {
            return await EnumerateInjectedDiscoveryAsync(_discoveries[0], targetHost, categories, cancellationToken).ConfigureAwait(false);
        }

        OpcConnectData? connectData = CreateConnectData(targetHost, username, password, useKerberos, useSso, authLevel);
        OpcServerDescriptor[] descriptors = connectData is null
            ? await OpcDiscovery.EnumerateAsync(
                targetHost,
                categories.Length == 0 ? null : categories,
                cancellationToken).ConfigureAwait(false)
            : await OpcDiscovery.EnumerateAsync(
                targetHost,
                connectData,
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

    private static OpcConnectData? CreateConnectData(
        string host,
        string? username,
        string? password,
        bool useKerberos,
        bool useSso,
        string? authLevel)
    {
        OpcUrl url = OpcUrl.Parse($"opcda://{host}/OPC.ServerList.1");
        OpcProtectionLevel protectionLevel = OpcMcpAuthLevel.ParseOrDefault(authLevel);
        if (useSso)
        {
            return OpcConnectData.WithWindowsSso(url, protectionLevel);
        }

        NetworkCredential? credentials = CreateCredential(username, password);
        if (credentials is null)
        {
            return OpcMcpAuthLevel.IsSpecified(authLevel)
                ? new OpcConnectData(url, credentials: null, authMode: OpcAuthMode.Anonymous, protectionLevel: protectionLevel)
                : null;
        }

        return useKerberos
            ? OpcConnectData.WithKerberos(url, credentials, protectionLevel)
            : OpcConnectData.WithNtlmV2(url, credentials, protectionLevel);
    }

    private static NetworkCredential? CreateCredential(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        string user = username.Trim();
        string domain = string.Empty;
        int slash = user.IndexOf('\\', StringComparison.Ordinal);
        if (slash > 0 && slash < user.Length - 1)
        {
            domain = user[..slash];
            user = user[(slash + 1)..];
        }

        return new NetworkCredential(user, password ?? string.Empty, domain);
    }

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
