// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>
/// Optional managed shape for AE browser implementations that expose an in-process string enumerator.
/// </summary>
public interface IOpcAeStringEnumerator : IEnumString
{
    /// <summary>
    /// Snapshots the enumerator contents to a managed string array.
    /// </summary>
    Task<string[]> ToArrayAsync(CancellationToken cancellationToken = default);
}
