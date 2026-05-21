//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Dx;

/// <summary>
/// Description of a source server registered with a DX server, per
/// OPC DX 1.0 §4.2.
/// </summary>
public sealed class SourceServer
{
    /// <summary>The friendly name the DX server uses to identify the source.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The source server's URL (typically <c>opcda://host/progid</c>).</summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// Server-defined description ("Vendor PLC1 Backup Channel", etc.).
    /// </summary>
    public string Description { get; init; } = string.Empty;
}
