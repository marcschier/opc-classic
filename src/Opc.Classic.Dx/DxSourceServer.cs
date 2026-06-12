//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;

namespace Opc.Classic.Dx;

/// <summary>
/// OPC DX's <c>OpcDxSourceServer</c> — a registered upstream OPC server that
/// DX connections may read from.
/// </summary>
public sealed record DxSourceServer
{
    /// <summary>Constructs a source-server definition.</summary>
    /// <remarks>
    /// <paramref name="serverUrl"/> is a string and not a <see cref="Uri"/> because OPC URLs
    /// use custom non-registered schemes (e.g. <c>opcda://</c>) that <see cref="Uri"/>'s parser
    /// does not understand consistently across platforms.
    /// </remarks>
    [SuppressMessage(
        "Design", "CA1054:URI-like parameters should not be strings",
        Justification = "serverUrl carries a ProgID-qualified OPC DA URL (opcda://...) which is not a registered System.Uri scheme.")]
    public DxSourceServer(
        string? name = null,
        string? serverUrl = null,
        string? description = null,
        string? serverType = null,
        string? itemPath = null,
        string? itemName = null,
        string? version = null,
        bool? defaultConnected = null,
        int mask = 0,
        int reserved = 0)
    {
        Name = name;
        ServerUrl = serverUrl;
        Description = description;
        ServerType = serverType;
        ItemPath = itemPath;
        ItemName = itemName;
        Version = version;
        DefaultConnected = defaultConnected;
        Mask = mask == 0 ? ComputeMask(this) : mask;
        Reserved = reserved;
    }

    /// <summary>Native <c>dwMask</c> presence bits.</summary>
    public int Mask { get; init; }

    /// <summary>Friendly name used by DX connections to refer to this source server.</summary>
    public string? Name { get; init; }

    /// <summary>Source server URL, typically an OPC DA URL or ProgID-qualified endpoint.</summary>
    /// <remarks>See <see cref="DxSourceServer(string?, string?, string?, string?, string?, string?, string?, bool?, int, int)"/>.</remarks>
    [SuppressMessage(
        "Design", "CA1056:URI-like properties should not be strings",
        Justification = "ServerUrl carries a ProgID-qualified OPC DA URL (opcda://...) which is not a registered System.Uri scheme.")]
    public string? ServerUrl { get; init; }

    /// <summary>Server-defined description.</summary>
    public string? Description { get; init; }

    /// <summary>Server type string advertised by the DX server.</summary>
    public string? ServerType { get; init; }

    /// <summary>Configuration item path that identifies the source-server object.</summary>
    public string? ItemPath { get; init; }

    /// <summary>Configuration item name that identifies the source-server object.</summary>
    public string? ItemName { get; init; }

    /// <summary>Configuration version associated with the source-server object.</summary>
    public string? Version { get; init; }

    /// <summary>Default source-server connected state, or null when unspecified.</summary>
    public bool? DefaultConnected { get; init; }

    /// <summary>Reserved DWORD carried by the native structure.</summary>
    public int Reserved { get; init; }

    private static int ComputeMask(DxSourceServer source)
    {
        var mask = DxMask.None;
        AddIf(!string.IsNullOrEmpty(source.ItemPath), DxMask.ItemPath, ref mask);
        AddIf(!string.IsNullOrEmpty(source.ItemName), DxMask.ItemName, ref mask);
        AddIf(!string.IsNullOrEmpty(source.Version), DxMask.Version, ref mask);
        AddIf(!string.IsNullOrEmpty(source.Name), DxMask.Name, ref mask);
        AddIf(!string.IsNullOrEmpty(source.Description), DxMask.Description, ref mask);
        AddIf(!string.IsNullOrEmpty(source.ServerType), DxMask.ServerType, ref mask);
        AddIf(!string.IsNullOrEmpty(source.ServerUrl), DxMask.ServerUrl, ref mask);
        AddIf(source.DefaultConnected.HasValue, DxMask.DefaultSourceServerConnected, ref mask);
        return (int)mask;
    }

    private static void AddIf(bool condition, DxMask value, ref DxMask mask)
    {
        if (condition)
        {
            mask |= value;
        }
    }
}
