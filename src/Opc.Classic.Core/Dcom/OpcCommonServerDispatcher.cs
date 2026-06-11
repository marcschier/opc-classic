//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom;

/// <summary>Server-side dispatcher for OPC Common <c>IOPCCommon</c> debug/metadata methods.</summary>
public sealed class OpcCommonServerDispatcher : IOpcServerDispatcher
{
    private readonly IOpcCommonServer _server;

    /// <summary>Initializes a new instance of the <see cref="OpcCommonServerDispatcher" /> class.</summary>
    public OpcCommonServerDispatcher(IOpcCommonServer server) =>
        _server = server ?? throw new ArgumentNullException(nameof(server));

    /// <inheritdoc />
    public async ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (opnum != OpcCommonClientProxy.Opnums.SetClientName)
            {
                return DispatchResult.NotImplemented(opnum);
            }

            var reader = new NdrReader(requestPayload.Span);
            string clientName = reader.ReadUnicodeStringPtr() ?? string.Empty;
            await _server.SetClientNameAsync(clientName, cancellationToken).ConfigureAwait(false);
            return DispatchResult.Success(Array.Empty<byte>());
        }
        catch (OpcException exception)
        {
            return DispatchResult.Fault(exception.ResultId.Code);
        }
    }
}
