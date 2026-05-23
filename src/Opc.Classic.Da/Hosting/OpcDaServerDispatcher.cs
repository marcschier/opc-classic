//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Hosting;

/// <summary>Default DA per-method dispatcher for managed server hosting.</summary>
public sealed class OpcDaServerDispatcher : IOpcDaServerDispatcher
{
    private const int InitialResponseBufferSize = 1024;

    private readonly IOpcDaServer _server;

    /// <summary>Initializes a new instance of the <see cref="OpcDaServerDispatcher"/> class.</summary>
    public OpcDaServerDispatcher(IOpcDaServer server)
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
            if (interfaceId == IOPCServer.InterfaceId)
            {
                return opnum switch
                {
                    4 => await DispatchGetErrorStringAsync(requestPayload, cancellationToken).ConfigureAwait(false),
                    6 => await DispatchGetStatusAsync(cancellationToken).ConfigureAwait(false),
                    7 => await DispatchRemoveGroupAsync(requestPayload, cancellationToken).ConfigureAwait(false),
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
            NdrOpcServerStatusCodec.Write(ref writer, status));
        return new NdrCallResult(OpcResultId.Ok.Code, response);
    }

    private async Task<NdrCallResult> DispatchRemoveGroupAsync(
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        int serverHandle;
        bool force;
        var reader = new NdrReader(request.Span);
        serverHandle = reader.ReadInt32();
        force = reader.ReadInt32() != 0;

        await _server.RemoveGroupAsync(serverHandle, force, cancellationToken).ConfigureAwait(false);
        return new NdrCallResult(OpcResultId.Ok.Code, ReadOnlyMemory<byte>.Empty);
    }

    private async Task<NdrCallResult> DispatchGetErrorStringAsync(
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        int errorCode;
        int localeId;
        var reader = new NdrReader(request.Span);
        errorCode = reader.ReadInt32();
        localeId = reader.ReadInt32();

        string message = await _server.GetErrorStringAsync(errorCode, localeId, cancellationToken).ConfigureAwait(false);
        ReadOnlyMemory<byte> response = WriteResponse((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(message));
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
