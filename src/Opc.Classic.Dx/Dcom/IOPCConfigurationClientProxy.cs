//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dx.Ndr;
using Opc.Classic.Ndr;

#pragma warning disable MA0048 // Small proxy helper is grouped with the proxy implementation.

namespace Opc.Classic.Dx.Dcom;

/// <summary>Hand-written DX configuration proxy with OPCDX structure codec support.</summary>
public sealed class IOPCConfigurationClientProxy : IOPCConfiguration {
    private const int PayloadCapacity = 64 * 1024;
    private readonly ICallChannel _channel;

    /// <summary>Creates an <c>IOPCConfiguration</c> client proxy.</summary>
    public IOPCConfigurationClientProxy(ICallChannel channel) {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }

    /// <inheritdoc />
    public Task<DxSourceServer[]> QuerySourceServersAsync(CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.QuerySourceServersAsync,
            ReadOnlyMemory<byte>.Empty,
            DecodeSourceServers,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxGeneralResponse> AddSourceServersAsync(DxSourceServer[] sourceServers, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.AddSourceServersAsync,
            WritePayload((ref NdrWriter writer) => NdrOpcDxSourceServerArrayCodec.Write(ref writer, sourceServers)),
            DecodeGeneralResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxGeneralResponse> ModifySourceServersAsync(DxSourceServer[] sourceServers, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.ModifySourceServersAsync,
            WritePayload((ref NdrWriter writer) => NdrOpcDxSourceServerArrayCodec.Write(ref writer, sourceServers)),
            DecodeGeneralResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxGeneralResponse> DeleteSourceServersAsync(DxItemIdentifier[] sourceServers, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.DeleteSourceServersAsync,
            WritePayload((ref NdrWriter writer) => NdrOpcDxItemIdentifierArrayCodec.Write(ref writer, sourceServers)),
            DecodeGeneralResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxGeneralResponse> CopyDefaultServerAttributesAsync(bool configToStatus, DxItemIdentifier[] sourceServers, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.CopyDefaultServerAttributesAsync,
            WritePayload((ref NdrWriter writer) => {
                writer.WriteInt32(configToStatus ? NdrOpcDxClientProxyHelpers.Win32BoolTrue : 0);
                NdrOpcDxItemIdentifierArrayCodec.Write(ref writer, sourceServers);
            }),
            DecodeGeneralResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxConnectionQueryResult> QueryDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.QueryDXConnectionsAsync,
            WritePayload((ref NdrWriter writer) => {
                writer.WriteUnicodeStringPtr(browsePath);
                NdrOpcDxConnectionArrayCodec.Write(ref writer, connectionMasks);
                writer.WriteInt32(recursive ? NdrOpcDxClientProxyHelpers.Win32BoolTrue : 0);
            }),
            DecodeConnectionQueryResult,
            cancellationToken);

    /// <inheritdoc />
    public async Task<string[]> QueryDXConnectionNamesAsync(string browsePath, string[] connectionMasks, bool recursive, CancellationToken cancellationToken = default) {
        DxConnection[] masks = (connectionMasks ?? Array.Empty<string>())
            .Select(name => new DxConnection(name: name, mask: (int)DxMask.Name))
            .ToArray();
        DxConnectionQueryResult result = await QueryDXConnectionsAsync(browsePath, masks, recursive, cancellationToken).ConfigureAwait(false);
        return result.Connections.Select(connection => connection.Name ?? string.Empty).ToArray();
    }

    /// <inheritdoc />
    public Task<DxGeneralResponse> AddDXConnectionsAsync(DxConnection[] connections, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.AddDXConnectionsAsync,
            WritePayload((ref NdrWriter writer) => NdrOpcDxConnectionArrayCodec.Write(ref writer, connections)),
            DecodeGeneralResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxUpdateConnectionsResult> UpdateDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive, DxConnection connectionDefinition, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.UpdateDXConnectionsAsync,
            WritePayload((ref NdrWriter writer) => {
                writer.WriteUnicodeStringPtr(browsePath);
                NdrOpcDxConnectionArrayCodec.Write(ref writer, connectionMasks);
                writer.WriteInt32(recursive ? NdrOpcDxClientProxyHelpers.Win32BoolTrue : 0);
                NdrOpcDxConnectionCodec.Write(ref writer, connectionDefinition);
            }),
            DecodeUpdateConnectionsResult,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxGeneralResponse> ModifyDXConnectionsAsync(DxConnection[] connections, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.ModifyDXConnectionsAsync,
            WritePayload((ref NdrWriter writer) => NdrOpcDxConnectionArrayCodec.Write(ref writer, connections)),
            DecodeGeneralResponse,
            cancellationToken);

    /// <inheritdoc />
    public Task<int[]> DeleteDXConnectionsAsync(string browsePath, string[] connectionNames, bool recursive, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.DeleteDXConnectionsAsync,
            WritePayload((ref NdrWriter writer) => {
                writer.WriteUnicodeStringPtr(browsePath);
                NdrOpcDxStringArrayCodec.Write(ref writer, connectionNames);
                writer.WriteInt32(recursive ? NdrOpcDxClientProxyHelpers.Win32BoolTrue : 0);
            }),
            DecodeInt32Array,
            cancellationToken);

    /// <inheritdoc />
    public Task<DxUpdateConnectionsResult> CopyDefaultDXConnectionAttributesAsync(bool configToStatus, string browsePath, DxConnection[] connectionMasks, bool recursive, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.CopyDefaultDXConnectionAttributesAsync,
            WritePayload((ref NdrWriter writer) => {
                writer.WriteInt32(configToStatus ? NdrOpcDxClientProxyHelpers.Win32BoolTrue : 0);
                writer.WriteUnicodeStringPtr(browsePath);
                NdrOpcDxConnectionArrayCodec.Write(ref writer, connectionMasks);
                writer.WriteInt32(recursive ? NdrOpcDxClientProxyHelpers.Win32BoolTrue : 0);
            }),
            DecodeUpdateConnectionsResult,
            cancellationToken);

    /// <inheritdoc />
    public Task<string> ResetConfigurationAsync(string configurationVersion, CancellationToken cancellationToken = default) =>
        InvokeAsync(
            IOPCConfiguration.Opnums.ResetConfigurationAsync,
            WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(configurationVersion)),
            DecodeString,
            cancellationToken);

    private async Task<T> InvokeAsync<T>(int opnum, ReadOnlyMemory<byte> payload, Func<ReadOnlyMemory<byte>, T> decode, CancellationToken cancellationToken) {
        NdrCallResult result = await _channel.InvokeAsync(
            IOPCConfiguration.InterfaceId,
            opnum,
            payload,
            cancellationToken).ConfigureAwait(false);

        if (result.IsFailure) {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }

        return decode(result.ResponsePayload);
    }

    private static DxSourceServer[] DecodeSourceServers(ReadOnlyMemory<byte> payload) {
        var reader = new NdrReader(payload.Span);
        return NdrOpcDxSourceServerArrayCodec.Read(ref reader);
    }

    private static DxGeneralResponse DecodeGeneralResponse(ReadOnlyMemory<byte> payload) {
        var reader = new NdrReader(payload.Span);
        return NdrOpcDxGeneralResponseCodec.Read(ref reader);
    }

    private static DxConnectionQueryResult DecodeConnectionQueryResult(ReadOnlyMemory<byte> payload) {
        var reader = new NdrReader(payload.Span);
        int[] errors = NdrOpcDxInt32ArrayCodec.Read(ref reader);
        DxConnection[] connections = NdrOpcDxConnectionArrayCodec.Read(ref reader);
        return new DxConnectionQueryResult(errors, connections);
    }

    private static DxUpdateConnectionsResult DecodeUpdateConnectionsResult(ReadOnlyMemory<byte> payload) {
        var reader = new NdrReader(payload.Span);
        int[] errors = NdrOpcDxInt32ArrayCodec.Read(ref reader);
        DxGeneralResponse response = NdrOpcDxGeneralResponseCodec.Read(ref reader);
        return new DxUpdateConnectionsResult(errors, response);
    }

    private static int[] DecodeInt32Array(ReadOnlyMemory<byte> payload) {
        var reader = new NdrReader(payload.Span);
        return NdrOpcDxInt32ArrayCodec.Read(ref reader);
    }

    private static string DecodeString(ReadOnlyMemory<byte> payload) {
        var reader = new NdrReader(payload.Span);
        return reader.ReadUnicodeStringPtr() ?? string.Empty;
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write) {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(PayloadCapacity);
        try {
            var writer = new NdrWriter(buffer);
            write(ref writer);
            return buffer.AsSpan(0, writer.Position).ToArray();
        }
        finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}

internal static class NdrOpcDxClientProxyHelpers {
    internal const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);
}
