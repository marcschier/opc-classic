// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC Data Access 2.05a backward-compatibility interfaces.
//
// New consumer code SHOULD prefer the DA 3.0 surface in
// OpcClassic.Da.Dcom (IOPCSyncIO2, IOPCAsyncIO2/AsyncIO3).
// These V20 declarations exist for connectivity to legacy servers
// that did not implement DA 3.0.

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCSyncIO not IOpcSyncIO)
#pragma warning disable MA0048 // Legacy DA 2.05a shims are clearer grouped than split across files

using System.Threading;
using System.Threading.Tasks;
using OpcClassic;
using OpcClassic.Generators;

namespace OpcClassic.Da.V20.Dcom;

/// <summary>
/// IOPCSyncIO (DA 2.05a back-compat). Superseded by IOPCSyncIO2 (DA 3.0).
/// Use IOPCSyncIO2 in new code; this exists for connectivity to legacy servers.
/// </summary>
[OpcInterface("39C13A52-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCSyncIO
{
    /// <summary>
    /// <c>IOPCSyncIO::Write</c> (opnum 4). Writes item values and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(4)]
    Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default);

    // Read returns OPCITEMSTATE[] plus HRESULT[] and needs a multi-out result record codec.
}

/// <summary>
/// IOPCAsyncIO (DA 2.05a back-compat). Superseded by IOPCAsyncIO2/IOPCAsyncIO3.
/// </summary>
[OpcInterface("39C13A53-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCAsyncIO
{
    /// <summary>
    /// <c>IOPCAsyncIO::Refresh</c> (opnum 5). Starts an async refresh and returns the transaction ID.
    /// </summary>
    [OpcMethod(5)]
    Task<int> RefreshAsync(int connection, int dataSource, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO::Cancel</c> (opnum 6). Cancels a pending async transaction.
    /// </summary>
    [OpcMethod(6)]
    Task CancelAsync(int transactionId, CancellationToken cancellationToken = default);

    // Read and Write return transaction IDs plus per-item HRESULT arrays.
}
