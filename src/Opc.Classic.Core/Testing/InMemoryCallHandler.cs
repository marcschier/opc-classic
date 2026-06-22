// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Testing;

/// <summary>
/// Simulates the server-side behavior for an <see cref="InMemoryCallChannel" /> invocation.
/// </summary>
/// <param name="interfaceId">The destination interface IID.</param>
/// <param name="opnum">The destination DCE/RPC operation number.</param>
/// <param name="requestPayload">The NDR-encoded request body.</param>
/// <param name="cancellationToken">Cancellation token for the simulated call.</param>
/// <returns>The simulated DCE/RPC call result.</returns>
public delegate Task<NdrCallResult> InMemoryCallHandler(
    Guid interfaceId,
    int opnum,
    ReadOnlyMemory<byte> requestPayload,
    CancellationToken cancellationToken);
