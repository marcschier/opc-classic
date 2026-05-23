//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da;

/// <summary>
/// Opaque continuation token for paged browse results. Returned by
/// <c>IDaServer.BrowseAsync</c> and consumed by
/// <c>IDaServer.BrowseNextAsync</c> when the prior call indicated more
/// elements remained.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ContinuationPoint"/> is a server-defined string the client
/// must echo back verbatim. <see cref="IsCompleted"/> being <see langword="true"/>
/// means the browse is fully drained — no further calls are needed.
/// </para>
/// </remarks>
public sealed class BrowsePosition
{
    /// <summary>The server-side continuation point.</summary>
    public string ContinuationPoint { get; init; } = string.Empty;

    /// <summary>True when no further browse calls are required.</summary>
    public bool IsCompleted { get; init; }

    /// <summary>A completed (drained) browse position.</summary>
    public static BrowsePosition Completed { get; } = new() { IsCompleted = true };

    /// <summary>True when this represents "no more results".</summary>
    public bool IsTerminal => IsCompleted || ContinuationPoint.Length == 0;
}
