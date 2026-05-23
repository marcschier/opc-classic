//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Ae.Dcom;
using OpcClassic.Ae.Ndr;
using OpcClassic.Ndr;

namespace OpcClassic.Ae.Hosting;

/// <summary>Default AE per-method dispatcher for managed server hosting.</summary>
public sealed class OpcAeServerDispatcher : IOpcAeServerDispatcher
{
    private const int InitialResponseBufferSize = 1024;

    private readonly IOpcAeServer _server;

    /// <summary>Initializes a new instance of the <see cref="OpcAeServerDispatcher"/> class.</summary>
    public OpcAeServerDispatcher(IOpcAeServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
    }

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        try
        {
            if (interfaceId == IOPCEventServer.InterfaceId)
            {
                return opnum switch
                {
                    3 => await DispatchGetStatusAsync(cancellationToken).ConfigureAwait(false),
                    5 => await DispatchQueryAvailableFiltersAsync(cancellationToken).ConfigureAwait(false),
                    _ => NotImplemented(),
                };
            }

            return NotImplemented();
        }
        catch (OpcException exception)
        {
            return new NdrCallResult(exception.ResultId.Code, ReadOnlyMemory<byte>.Empty);
        }
    }

    private static NdrCallResult NotImplemented() =>
        new(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);

    private async Task<NdrCallResult> DispatchGetStatusAsync(CancellationToken cancellationToken)
    {
        OpcServerStatus status = await _server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        ReadOnlyMemory<byte> response = WriteResponse((ref NdrWriter writer) =>
            NdrOpcEventServerStatusCodec.Write(ref writer, status));
        return new NdrCallResult(OpcResultId.Ok.Code, response);
    }

    private async Task<NdrCallResult> DispatchQueryAvailableFiltersAsync(CancellationToken cancellationToken)
    {
        int filters = await _server.QueryAvailableFiltersAsync(cancellationToken).ConfigureAwait(false);
        ReadOnlyMemory<byte> response = WriteResponse((ref NdrWriter writer) => writer.WriteInt32(filters));
        return new NdrCallResult(OpcResultId.Ok.Code, response);
    }

    private static ReadOnlyMemory<byte> WriteResponse(NdrWriteAction write)
    {
        byte[] responseBuffer = ArrayPool<byte>.Shared.Rent(InitialResponseBufferSize);
        try
        {
            var writer = new NdrWriter(responseBuffer);
            write(ref writer);
            return responseBuffer.AsMemory(0, writer.Position).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(responseBuffer);
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
