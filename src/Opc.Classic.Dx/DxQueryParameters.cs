//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

#pragma warning disable MA0048 // Small query helper types are intentionally grouped with DxQueryParameters.

namespace Opc.Classic.Dx;

/// <summary>Controls which DX browse nodes a query should return.</summary>
public enum DxBrowseFilter
{
    /// <summary>Return all matching browse nodes.</summary>
    All = 0,

    /// <summary>Return branch/folder nodes only.</summary>
    Branches = 1,

    /// <summary>Return DX connection leaves only.</summary>
    Connections = 2,

    /// <summary>Return DX source-server leaves only.</summary>
    SourceServers = 3,
}

/// <summary>Path within the DX browse hierarchy.</summary>
public readonly record struct DxBrowsePath
{
    /// <summary>Constructs a browse path.</summary>
    public DxBrowsePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        Path = path;
    }

    /// <summary>Server-defined browse path.</summary>
    public string Path { get; }

    /// <inheritdoc />
    public override string ToString() => Path;
}

/// <summary>Optional item identifier used to narrow DX source-server or connection queries.</summary>
public sealed record DxItemQuery(
    string? ItemPath = null,
    string? ItemName = null,
    string? Version = null);

/// <summary>Query parameters used to browse or operate on matching DX entities.</summary>
public sealed record DxQueryParameters
{
    /// <summary>Constructs DX query parameters.</summary>
    public DxQueryParameters(
        DxBrowsePath? browsePath = null,
        bool recursive = false,
        DxConnection[]? connectionMasks = null,
        DxBrowseFilter browseFilter = DxBrowseFilter.All,
        DxItemQuery? itemQuery = null)
    {
        BrowsePath = browsePath;
        Recursive = recursive;
        ConnectionMasks = NormalizeConnectionMasks(connectionMasks);
        BrowseFilter = browseFilter;
        ItemQuery = itemQuery;
    }

    /// <summary>Browse path to search from. Null means the server's DX root.</summary>
    public DxBrowsePath? BrowsePath { get; init; }

    /// <summary>Whether descendant folders under <see cref="BrowsePath"/> should be searched.</summary>
    public bool Recursive { get; init; }

    /// <summary>Connection masks supplied to DX query/update/delete methods.</summary>
    public DxConnection[] ConnectionMasks { get; init; }

    /// <summary>Browse-node kind filter.</summary>
    public DxBrowseFilter BrowseFilter { get; init; }

    /// <summary>Optional item identifier query for source-server operations.</summary>
    public DxItemQuery? ItemQuery { get; init; }

    private static DxConnection[] NormalizeConnectionMasks(DxConnection[]? connectionMasks)
    {
        if (connectionMasks is null || connectionMasks.Length == 0)
        {
            return Array.Empty<DxConnection>();
        }

        var copy = new DxConnection[connectionMasks.Length];
        for (var i = 0; i < connectionMasks.Length; i++)
        {
            var mask = connectionMasks[i];
            ArgumentNullException.ThrowIfNull(mask);
            copy[i] = mask;
        }

        return copy;
    }
}