// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Text;

namespace Opc.Classic.Dx;

/// <summary>
/// Builds canonical OPC DX database item paths using the <c>/DX/{X}/{Y}</c> convention.
/// </summary>
public static class DxNamespace
{
    /// <summary>
    /// DX path separator.
    /// </summary>
    public const string Separator = "/";

    /// <summary>
    /// DX database root segment.
    /// </summary>
    public const string RootSegment = "DX";

    /// <summary>
    /// Server status branch segment.
    /// </summary>
    public const string ServerStatusSegment = "ServerStatus";

    /// <summary>
    /// DX connections root branch segment.
    /// </summary>
    public const string ConnectionsRootSegment = "DXConnectionsRoot";

    /// <summary>
    /// Source servers root branch segment.
    /// </summary>
    public const string SourceServersRootSegment = "SourceServers";

    /// <summary>
    /// Status child segment.
    /// </summary>
    public const string StatusSegment = "Status";

    /// <summary>
    /// Canonical DX root path.
    /// </summary>
    public static string RootPath => Separator + RootSegment;

    /// <summary>
    /// Canonical <c>/DX/ServerStatus</c> path.
    /// </summary>
    public static string ServerStatusPath => Join(ServerStatusSegment);

    /// <summary>
    /// Canonical <c>/DX/DXConnectionsRoot</c> path.
    /// </summary>
    public static string ConnectionsRootPath => Join(ConnectionsRootSegment);

    /// <summary>
    /// Canonical <c>/DX/SourceServers</c> path.
    /// </summary>
    public static string SourceServersRootPath => Join(SourceServersRootSegment);

    /// <summary>
    /// Builds a canonical path under <c>/DX</c>.
    /// </summary>
    public static string Join(params string?[] segments)
    {
        var builder = new StringBuilder(RootPath);
        if (segments is null)
        {
            return builder.ToString();
        }

        foreach (var segment in segments)
        {
            AppendSegment(builder, segment);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Builds a connection path under <c>/DX/DXConnectionsRoot</c>.
    /// </summary>
    public static string ConnectionPath(params string?[] browsePathAndName)
    {
        browsePathAndName ??= Array.Empty<string?>();
        return Join(Prepend(ConnectionsRootSegment, browsePathAndName));
    }

    /// <summary>
    /// Builds a connection status path under <c>/DX/DXConnectionsRoot</c>.
    /// </summary>
    public static string ConnectionStatusPath(params string?[] browsePathAndName)
    {
        browsePathAndName ??= Array.Empty<string?>();
        return Join(Append(Prepend(ConnectionsRootSegment, browsePathAndName), StatusSegment));
    }

    /// <summary>
    /// Builds a source-server path under <c>/DX/SourceServers</c>.
    /// </summary>
    public static string SourceServerPath(string sourceServerName) =>
        Join(SourceServersRootSegment, sourceServerName);

    /// <summary>
    /// Builds a source-server status path under <c>/DX/SourceServers</c>.
    /// </summary>
    public static string SourceServerStatusPath(string sourceServerName) =>
        Join(SourceServersRootSegment, sourceServerName, StatusSegment);

    private static void AppendSegment(StringBuilder builder, string? segment)
    {
        if (string.IsNullOrWhiteSpace(segment))
        {
            return;
        }

        foreach (var part in segment.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (part.Length == 0 || string.Equals(part, RootSegment, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            builder.Append('/').Append(part);
        }
    }

    private static string?[] Prepend(string head, string?[] tail)
    {
        var result = new string?[tail.Length + 1];
        result[0] = head;
        Array.Copy(tail, 0, result, 1, tail.Length);
        return result;
    }

    private static string?[] Append(string?[] head, string tail)
    {
        var result = new string?[head.Length + 1];
        Array.Copy(head, result, head.Length);
        result[^1] = tail;
        return result;
    }
}
