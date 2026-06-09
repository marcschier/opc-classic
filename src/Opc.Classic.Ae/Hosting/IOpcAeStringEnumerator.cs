//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>Optional managed shape for AE browser implementations that expose an in-process string enumerator.</summary>
public interface IOpcAeStringEnumerator : IEnumString {
    /// <summary>Snapshots the enumerator contents to a managed string array.</summary>
    Task<string[]> ToArrayAsync(CancellationToken cancellationToken = default);
}
