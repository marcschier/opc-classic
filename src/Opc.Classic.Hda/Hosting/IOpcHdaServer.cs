//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Hda.Hosting;

/// <summary>Contract implemented by user code to provide an in-process managed HDA server.</summary>
public interface IOpcHdaServer
{
    /// <summary>Gets the HDA historian runtime status snapshot.</summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Validates HDA item IDs and returns per-item HRESULTs.</summary>
    Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default);
}
