// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers;
using Opc.Classic.Dx;
using Opc.Classic.Dx.Dcom;
using Opc.Classic.Dx.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Samples.SimulationServer.Dx;

internal sealed class SimDxDcomDispatcher(IOPCConfiguration server)
{
    private const int PayloadCapacity = 256 * 1024;

    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCConfiguration.InterfaceId)
        {
            return new NdrCallResult(
                OpcResultId.NotImplemented.Code,
                ReadOnlyMemory<byte>.Empty);
        }

        try
        {
            switch (opnum)
            {
                case 3:
                    DxSourceServer[] sources =
                        await server.QuerySourceServersAsync(cancellationToken)
                            .ConfigureAwait(false);
                    return WriteResult((ref NdrWriter writer) =>
                        NdrOpcDxSourceServerArrayCodec.Write(ref writer, sources));
                case 4:
                    return await DispatchSourceMutationAsync(
                        requestPayload,
                        add: true,
                        cancellationToken).ConfigureAwait(false);
                case 5:
                    return await DispatchSourceMutationAsync(
                        requestPayload,
                        add: false,
                        cancellationToken).ConfigureAwait(false);
                case 6:
                    DxGeneralResponse deletedSources =
                        await server.DeleteSourceServersAsync(
                            ReadItemIdentifiers(requestPayload),
                            cancellationToken).ConfigureAwait(false);
                    return WriteResult((ref NdrWriter writer) =>
                        NdrOpcDxGeneralResponseCodec.Write(ref writer, deletedSources));
                case 7:
                    return await DispatchCopyDefaultServerAttributesAsync(
                        requestPayload,
                        cancellationToken).ConfigureAwait(false);
                case 8:
                    return await DispatchQueryConnectionsAsync(
                        requestPayload,
                        cancellationToken).ConfigureAwait(false);
                case 9:
                    return await DispatchConnectionMutationAsync(
                        requestPayload,
                        add: true,
                        cancellationToken).ConfigureAwait(false);
                case 10:
                    return await DispatchUpdateConnectionsAsync(
                        requestPayload,
                        cancellationToken).ConfigureAwait(false);
                case 11:
                    return await DispatchConnectionMutationAsync(
                        requestPayload,
                        add: false,
                        cancellationToken).ConfigureAwait(false);
                case 12:
                    return await DispatchDeleteConnectionsAsync(
                        requestPayload,
                        cancellationToken).ConfigureAwait(false);
                case 13:
                    return await DispatchCopyDefaultConnectionAttributesAsync(
                        requestPayload,
                        cancellationToken).ConfigureAwait(false);
                case 14:
                    return await DispatchResetAsync(
                        requestPayload,
                        cancellationToken).ConfigureAwait(false);
                default:
                    return new NdrCallResult(
                        OpcResultId.NotImplemented.Code,
                        ReadOnlyMemory<byte>.Empty);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
    }

    private async Task<NdrCallResult> DispatchSourceMutationAsync(
        ReadOnlyMemory<byte> payload,
        bool add,
        CancellationToken cancellationToken)
    {
        DxSourceServer[] sources = ReadSourceServers(payload);
        DxGeneralResponse response = add
            ? await server.AddSourceServersAsync(sources, cancellationToken)
                .ConfigureAwait(false)
            : await server.ModifySourceServersAsync(sources, cancellationToken)
                .ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
            NdrOpcDxGeneralResponseCodec.Write(ref writer, response));
    }

    private async Task<NdrCallResult> DispatchConnectionMutationAsync(
        ReadOnlyMemory<byte> payload,
        bool add,
        CancellationToken cancellationToken)
    {
        DxConnection[] connections = ReadConnections(payload);
        DxGeneralResponse response = add
            ? await server.AddDXConnectionsAsync(connections, cancellationToken)
                .ConfigureAwait(false)
            : await server.ModifyDXConnectionsAsync(connections, cancellationToken)
                .ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
            NdrOpcDxGeneralResponseCodec.Write(ref writer, response));
    }

    private async Task<NdrCallResult> DispatchCopyDefaultServerAttributesAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(payload.Span);
        bool configToStatus = reader.ReadInt32() != 0;
        DxItemIdentifier[] identifiers =
            NdrOpcDxItemIdentifierArrayCodec.Read(ref reader);
        DxGeneralResponse response = await server.CopyDefaultServerAttributesAsync(
            configToStatus,
            identifiers,
            cancellationToken).ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
            NdrOpcDxGeneralResponseCodec.Write(ref writer, response));
    }

    private async Task<NdrCallResult> DispatchQueryConnectionsAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(payload.Span);
        string browsePath = reader.ReadUnicodeStringPtr() ?? string.Empty;
        DxConnection[] masks = NdrOpcDxConnectionArrayCodec.Read(ref reader);
        bool recursive = reader.ReadInt32() != 0;
        DxConnectionQueryResult response = await server.QueryDXConnectionsAsync(
            browsePath,
            masks,
            recursive,
            cancellationToken).ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
        {
            NdrOpcDxInt32ArrayCodec.Write(ref writer, response.Errors);
            NdrOpcDxConnectionArrayCodec.Write(ref writer, response.Connections);
        });
    }

    private async Task<NdrCallResult> DispatchUpdateConnectionsAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(payload.Span);
        string browsePath = reader.ReadUnicodeStringPtr() ?? string.Empty;
        DxConnection[] masks = NdrOpcDxConnectionArrayCodec.Read(ref reader);
        bool recursive = reader.ReadInt32() != 0;
        DxConnection definition = NdrOpcDxConnectionCodec.Read(ref reader);
        DxUpdateConnectionsResult response = await server.UpdateDXConnectionsAsync(
            browsePath,
            masks,
            recursive,
            definition,
            cancellationToken).ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
        {
            NdrOpcDxInt32ArrayCodec.Write(ref writer, response.Errors);
            NdrOpcDxGeneralResponseCodec.Write(ref writer, response.Response);
        });
    }

    private async Task<NdrCallResult> DispatchDeleteConnectionsAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(payload.Span);
        string browsePath = reader.ReadUnicodeStringPtr() ?? string.Empty;
        DxConnection[] masks = NdrOpcDxConnectionArrayCodec.Read(ref reader);
        bool recursive = reader.ReadInt32() != 0;
        DxDeleteConnectionsResult response = await server.DeleteDXConnectionsAsync(
            browsePath,
            masks,
            recursive,
            cancellationToken).ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
        {
            NdrOpcDxInt32ArrayCodec.Write(ref writer, response.MaskErrors);
            NdrOpcDxGeneralResponseCodec.Write(ref writer, response.Response);
        });
    }

    private async Task<NdrCallResult> DispatchCopyDefaultConnectionAttributesAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(payload.Span);
        bool configToStatus = reader.ReadInt32() != 0;
        string browsePath = reader.ReadUnicodeStringPtr() ?? string.Empty;
        DxConnection[] masks = NdrOpcDxConnectionArrayCodec.Read(ref reader);
        bool recursive = reader.ReadInt32() != 0;
        DxUpdateConnectionsResult response =
            await server.CopyDefaultDXConnectionAttributesAsync(
                configToStatus,
                browsePath,
                masks,
                recursive,
                cancellationToken).ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
        {
            NdrOpcDxInt32ArrayCodec.Write(ref writer, response.Errors);
            NdrOpcDxGeneralResponseCodec.Write(ref writer, response.Response);
        });
    }

    private async Task<NdrCallResult> DispatchResetAsync(
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(payload.Span);
        string version = reader.ReadUnicodeStringPtr() ?? string.Empty;
        string response = await server.ResetConfigurationAsync(
            version,
            cancellationToken).ConfigureAwait(false);
        return WriteResult((ref NdrWriter writer) =>
            writer.WriteUnicodeStringPtr(response));
    }

    private static DxSourceServer[] ReadSourceServers(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return NdrOpcDxSourceServerArrayCodec.Read(ref reader);
    }

    private static DxItemIdentifier[] ReadItemIdentifiers(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return NdrOpcDxItemIdentifierArrayCodec.Read(ref reader);
    }

    private static DxConnection[] ReadConnections(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return NdrOpcDxConnectionArrayCodec.Read(ref reader);
    }

    private static NdrCallResult WriteResult(NdrWriteAction write) =>
        new(0, WritePayload(write));

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(PayloadCapacity);
        try
        {
            var writer = new NdrWriter(buffer);
            write(ref writer);
            return buffer.AsMemory(0, writer.Position).ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
