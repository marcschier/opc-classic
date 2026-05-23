//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Abstraction over per-connection Kerberos AP-REQ / AP-REP handshake state.
/// </summary>
public interface IKerberosConnectionContext
{
    /// <summary>
    /// Acquires an AP-REQ token for the configured service principal.
    /// </summary>
    /// <param name="channelBindingsHash">
    /// Optional RFC 2744 MD5 channel-bindings hash to embed in the future
    /// KERB_AD_RESTRICTION_ENTRY authorization-data element.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for KDC I/O.</param>
    /// <returns>The AP-REQ token bytes.</returns>
    Task<byte[]> AcquireApRequestAsync(
        ReadOnlyMemory<byte>? channelBindingsHash,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes the server AP-REP token and returns the derived session key.
    /// </summary>
    /// <param name="apReply">AP-REP token bytes returned by the server.</param>
    /// <param name="cancellationToken">Cancellation token for AP-REP processing.</param>
    /// <returns>The derived session key bytes.</returns>
    Task<byte[]> ProcessApResponseAsync(ReadOnlyMemory<byte> apReply, CancellationToken cancellationToken = default);
}
