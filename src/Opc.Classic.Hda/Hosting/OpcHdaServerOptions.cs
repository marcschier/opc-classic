//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Hda.Hosting;

/// <summary>
/// Configuration for a managed OPC HDA server host.
/// </summary>
public sealed record OpcHdaServerOptions
{
    /// <summary>
    /// Gets the COM class identifier exposed to OPC HDA clients.
    /// </summary>
    public required Guid Clsid { get; set; }

    /// <summary>
    /// Gets the ProgID exposed to OPC HDA clients.
    /// </summary>
    public required string ProgId { get; set; }

    /// <summary>
    /// Gets the optional human-readable display name.
    /// </summary>
    public string? FriendlyName { get; set; }

    /// <summary>
    /// Gets the ncacn_ip_tcp listener endpoint.
    /// </summary>
    public string? ListenAddress { get; set; } = "127.0.0.1:0";
}
