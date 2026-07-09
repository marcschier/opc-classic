// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
//

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
            // IOPCSyncIO::Read IDL: [in] OPCDATASOURCE dwSource, [in] DWORD dwCount,
            // [in, size_is(dwCount)] OPCHANDLE *phServer. Both the explicit dwCount
            // AND the array's own NDR max_count must be written (the array is a
            // top-level [ref] under DCE 1.1 §14.3.10.3, so no outer referent).
            writer.WriteInt32(dataSource);
            writer.WriteUInt32((uint)serverHandles.Length);   // dwCount sibling
            writer.WriteConformantInt32Array(serverHandles);  // max_count + DWORDs
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
        // Response IDL: [out, size_is(,dwCount)] OPCITEMSTATE **ppItemValues,
        // [out, size_is(,dwCount)] HRESULT **ppErrors. Both T** are unique
        // pointers; consume the outer referent + max_count, then use the
        // deferred-pile codec because OPCITEMSTATE contains a [unique]
        // VARIANT whose body lives in the deferred section (per Matrikon
        // wire capture).
        OpcItemState[] states = ReadConformantOpcItemStateArray(ref reader);
        int[] errors = ReadUniqueInt32Array(ref reader);
        return new ReadResult(states, errors);
    }

    private static OpcItemState[] ReadConformantOpcItemStateArray(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        int count = reader.ReadInt32();
        return NdrOpcItemStateCodec.ReadConformantArray(ref reader, count);
    }

    public async Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);

        ReadOnlyMemory<byte> payload = WritePayload((ref NdrWriter writer) =>
        {
            // IOPCSyncIO::Write IDL: [in] DWORD dwCount, [in, size_is(dwCount)]
            // OPCHANDLE *phServer, [in, size_is(dwCount)] VARIANT *pItemValues.
            // Wire layout matches the generator's [OpcEmitArrayCount] +
            // [OpcVariantElements] emission so dispatcher and client agree.
            writer.WriteUInt32((uint)serverHandles.Length);   // dwCount sibling
            writer.WriteConformantInt32Array(serverHandles);  // max_count + DWORDs
            writer.WriteUInt32((uint)values.Length);          // max_count for VARIANT[]
            foreach (OpcVariant value in values)
            {
                NdrVariantExtensions.WriteVariantElement(ref writer, value);
            }
        });
        NdrCallResult result = await _channel.InvokeAsync(
            IOPCSyncIO.InterfaceId,
            IOPCSyncIO.Opnums.WriteAsync,
            payload,
            cancellationToken).ConfigureAwait(false);

        ThrowIfFailed(result);
        var reader = new NdrReader(result.ResponsePayload.Span);
        // Response IDL: [out, size_is(,dwCount)] HRESULT **ppErrors.
        // Unique pointer to a conformant array — consume the referent.
        return ReadUniqueInt32Array(ref reader);
    }

    private sealed record ReadResult(OpcItemState[] States, int[] Errors);

    private static T[] ReadUniqueArray<T>(ref NdrReader reader, NdrReadFunc<T> read)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        int count = reader.ReadInt32();
        if (count <= 0) { return []; }
        var values = new T[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = read(ref reader);
        }
        return values;
    }

    private static int[] ReadUniqueInt32Array(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        return reader.ReadConformantInt32Array();
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
