// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Hosting;

/// <summary>
/// Dispatches an NDR-encoded server-side OPC DCOM call for a single interface.
/// </summary>
public interface IOpcServerDispatcher
{
    /// <summary>
    /// Routes an incoming opnum request and returns an HRESULT plus NDR response body.
    /// </summary>
    ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default);
}
