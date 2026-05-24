// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Dcom;

public sealed class IOPCSyncIOClientProxy : IOPCSyncIO
{
    private readonly ICallChannel _channel;

    public IOPCSyncIOClientProxy(ICallChannel channel) =>
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));

    public Task<OpcItemState[]> ReadAsync(
        int dataSource,
        int[] serverHandles,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);

        ReadOnlyMemory<byte> payload = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteInt32(dataSource);
            writer.WriteConformantInt32Array(serverHandles);
        });
#pragma warning disable VSTHRD103 // Matches generated out-parameter proxy pattern: out values must be assigned before returning Task.
        ReadResult decoded = InvokeReadAsync(payload, cancellationToken).GetAwaiter().GetResult();
#pragma warning restore VSTHRD103
        errors = decoded.Errors;
        return Task.FromResult(decoded.States);
    }

    private async Task<ReadResult> InvokeReadAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        NdrCallResult result = await _channel.InvokeAsync(
            IOPCSyncIO.InterfaceId,
            IOPCSyncIO.Opnums.ReadAsync,
            payload,
            cancellationToken).ConfigureAwait(false);

        ThrowIfFailed(result);
        var reader = new NdrReader(result.ResponsePayload.Span);
        OpcItemState[] states = ReadArray(ref reader, NdrOpcItemStateCodec.Read);
        int[] errors = reader.ReadConformantInt32Array();
        return new ReadResult(states, errors);
    }

    public async Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);

        ReadOnlyMemory<byte> payload = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteConformantInt32Array(serverHandles);
            writer.WriteUInt32((uint)values.Length);
            foreach (OpcVariant value in values)
            {
                NdrVariantExtensions.WriteVariant(ref writer, value);
            }
        });
        NdrCallResult result = await _channel.InvokeAsync(
            IOPCSyncIO.InterfaceId,
            IOPCSyncIO.Opnums.WriteAsync,
            payload,
            cancellationToken).ConfigureAwait(false);

        ThrowIfFailed(result);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return reader.ReadConformantInt32Array();
    }

    private sealed record ReadResult(OpcItemState[] States, int[] Errors);

    private static T[] ReadArray<T>(ref NdrReader reader, NdrReadFunc<T> read)
    {
        int count = reader.ReadInt32();
        var values = new T[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = read(ref reader);
        }

        return values;
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write)
    {
        for (int size = 1024; size <= 65536; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                write(ref writer);
                return buffer.AsMemory(0, writer.Position);
            }
            catch (InvalidOperationException) when (size < 65536)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode IOPCSyncIO payload within 65536 bytes.");
    }

    private static void ThrowIfFailed(NdrCallResult result)
    {
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }
    }

    private delegate T NdrReadFunc<T>(ref NdrReader reader);

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
