//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // ICallChannel + NdrCallResult are tightly-coupled grouped types

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic;

/// <summary>
/// Transport-agnostic DCE/RPC (DCOM) call channel.
/// </summary>
/// <remarks>
/// The contract every generated call-shim binds against. Implementations
/// own the connection lifecycle and the (NTLMv2 / Kerberos / SPNEGO)
/// authentication; the call-shim's only responsibility is to NDR-encode
/// the request body and NDR-decode the response.
/// <para>
/// Production implementation is provided by Opc.Classic.Dcom's CallBuilder
/// wrapped in <c>DcomCallChannel</c>. A managed
/// <c>InMemoryCallChannel</c> exists for unit tests and managed loopback
/// integration tests.
/// </para>
/// </remarks>
public interface ICallChannel
{
    /// <summary>
    /// Sends an outbound NDR-encoded request frame to the given interface +
    /// opnum and awaits the response.
    /// </summary>
    /// <param name="interfaceId">The destination interface IID.</param>
    /// <param name="opnum">The destination method's DCE/RPC opnum.</param>
    /// <param name="requestPayload">The NDR-encoded request body (excluding RPC headers — the channel adds those).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The HRESULT returned by the server plus the NDR-encoded response body.
    /// Implementations must not throw on application-level HRESULT failures
    /// (those belong in <see cref="NdrCallResult.Hresult"/>); they should
    /// only throw on transport / authentication / cancellation failures.
    /// </returns>
    Task<NdrCallResult> InvokeAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an <see cref="ICallChannel.InvokeAsync"/> call: the server's
/// returned HRESULT plus the NDR-encoded response body.
/// </summary>
/// <param name="Hresult">The HRESULT (e.g. <c>S_OK = 0</c>, <c>OPC_E_UNKNOWNITEMID = 0xC0040007</c>).</param>
/// <param name="ResponsePayload">The NDR-encoded response body — empty on failed calls.</param>
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Auto)]
public readonly record struct NdrCallResult(int Hresult, ReadOnlyMemory<byte> ResponsePayload)
{
    /// <summary>True if the HRESULT severity bit is clear (success).</summary>
    public bool IsSuccess => (Hresult & unchecked((int)0x80000000u)) == 0;

    /// <summary>True if the HRESULT severity bit is set (failure).</summary>
    public bool IsFailure => !IsSuccess;
}
