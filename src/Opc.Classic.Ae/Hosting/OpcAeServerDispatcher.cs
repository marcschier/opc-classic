//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Hosting;

/// <summary>AE dispatcher adapter that delegates to the source-generated IOPCEventServer dispatcher.</summary>
public sealed class OpcAeServerDispatcher : IOpcAeServerDispatcher
{
    private readonly IOPCEventServerServerDispatcher _serverDispatcher;

    /// <summary>Initializes a new instance of the <see cref="OpcAeServerDispatcher" /> class.</summary>
    public OpcAeServerDispatcher(IOpcAeServer server) =>
        _serverDispatcher = new IOPCEventServerServerDispatcher(server ?? throw new ArgumentNullException(nameof(server)));

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCEventServer.InterfaceId)
        {
            return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
        }

        return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
            .ToNdrCallResult();
    }
}
