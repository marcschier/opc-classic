//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dx;

/// <summary>
/// OPC DX's <c>OpcDxSourceServer</c> — a registered upstream OPC server that
/// DX connections may read from.
/// </summary>
public sealed record DxSourceServer
{
    /// <summary>Constructs a source-server definition.</summary>
    public DxSourceServer(
        string name,
        string serverUrl,
        string? description = null,
        string? serverType = null,
        string? itemPath = null,
        string? itemName = null,
        string? version = null,
        bool? defaultConnected = null,
        int mask = 0,
        int reserved = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(serverUrl);

        Name = name;
        ServerUrl = serverUrl;
        Description = description;
        ServerType = serverType;
        ItemPath = itemPath;
        ItemName = itemName;
        Version = version;
        DefaultConnected = defaultConnected;
        Mask = mask;
        Reserved = reserved;
    }

    /// <summary>Native <c>dwMask</c> presence bits. Codecs will interpret it later.</summary>
    public int Mask { get; init; }

    /// <summary>Friendly name used by DX connections to refer to this source server.</summary>
    public string Name { get; init; }

    /// <summary>Source server URL, typically an OPC DA URL or ProgID-qualified endpoint.</summary>
    public string ServerUrl { get; init; }

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
}