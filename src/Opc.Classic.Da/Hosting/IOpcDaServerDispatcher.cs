//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Dispatches NDR-encoded DA DCOM calls to a managed DA server implementation.
/// </summary>
public interface IOpcDaServerDispatcher
{
    /// <summary>
    /// Routes an incoming interface/opnum request and returns an HRESULT plus NDR response body.
    /// </summary>
    Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken);
}
