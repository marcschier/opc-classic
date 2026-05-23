//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting;

/// <summary>HDA dispatcher adapter that delegates to the source-generated IOPCHDA_Server dispatcher.</summary>
public sealed class OpcHdaServerDispatcher : IOpcHdaServerDispatcher
{
    private readonly IOPCHDA_ServerServerDispatcher _serverDispatcher;

    /// <summary>Initializes a new instance of the <see cref="OpcHdaServerDispatcher" /> class.</summary>
    public OpcHdaServerDispatcher(IOpcHdaServer server) =>
        _serverDispatcher = new IOPCHDA_ServerServerDispatcher(server ?? throw new ArgumentNullException(nameof(server)));

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCHDA_Server.InterfaceId)
        {
            return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
        }

        return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
            .ToNdrCallResult();
    }
}
