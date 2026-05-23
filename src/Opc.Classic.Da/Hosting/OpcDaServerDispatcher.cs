//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>DA dispatcher adapter that delegates to the source-generated IOPCServer dispatcher.</summary>
public sealed class OpcDaServerDispatcher : IOpcDaServerDispatcher
{
    private readonly IOPCServerServerDispatcher _serverDispatcher;

    /// <summary>Initializes a new instance of the <see cref="OpcDaServerDispatcher" /> class.</summary>
    public OpcDaServerDispatcher(IOpcDaServer server) =>
        _serverDispatcher = new IOPCServerServerDispatcher(server ?? throw new ArgumentNullException(nameof(server)));

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCServer.InterfaceId)
        {
            return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
        }

        return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
            .ToNdrCallResult();
    }
}
