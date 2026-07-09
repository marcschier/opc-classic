// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default managed implementation of <see cref="IOPCItemIO"/> (DA 3.0 §4.3.7
/// stateless one-shot read/write surface). Bridges the wire-shape
/// <c>IOPCItemIO::Read</c> / <c>IOPCItemIO::WriteVQT</c> calls onto
/// <see cref="IOpcDaServer.ReadAsync"/> / <see cref="IOpcDaServer.WriteVQTAsync"/>
/// so any managed DA server that implements <see cref="IOpcDaServer"/> gets
/// IOPCItemIO out of the box.
/// </summary>
public sealed class DefaultItemIO : IOPCItemIO
{
    private readonly IOpcDaServer _serverImpl;

    /// <summary>
    /// Construct.
    /// </summary>
    /// <param name="serverImpl">The managed DA server implementation to route to.</param>
    public DefaultItemIO(IOpcDaServer serverImpl)
    {
        ArgumentNullException.ThrowIfNull(serverImpl);
        _serverImpl = serverImpl;
    }

    /// <inheritdoc />
    public Task ReadAsync(
        string[] itemIds,
        int[] maxAges,
        out OpcVariant[] values,
        out ushort[] qualities,
        out long[] timestamps,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(maxAges);
        cancellationToken.ThrowIfCancellationRequested();

        var count = itemIds.Length;
        values = new OpcVariant[count];
        qualities = new ushort[count];
        timestamps = new long[count];
        errors = new int[count];

        if (count == 0)
        {
            return Task.CompletedTask;
        }

        var items = new Item[count];
        for (var i = 0; i < count; i++)
        {
            items[i] = new Item(itemIds[i] ?? string.Empty);
        }

#pragma warning disable VSTHRD002, VSTHRD103, VSTHRD104 // The wire surface is sync (out params); bridge here.
        IReadOnlyList<ItemValueResult> results;
        try
        {
            results = _serverImpl.ReadAsync(items, cancellationToken).GetAwaiter().GetResult();
        }
        catch (OpcException ex)
        {
            Array.Fill(errors, ex.ResultId.Code);
            return Task.CompletedTask;
        }
#pragma warning restore VSTHRD002, VSTHRD103, VSTHRD104

        var resolved = Math.Min(results.Count, count);
        for (var i = 0; i < resolved; i++)
        {
            var result = results[i];
            errors[i] = result.ResultId.Code;
            qualities[i] = result.Quality.RawValue;
            timestamps[i] = result.Timestamp.UtcDateTime.ToFileTimeUtc();
            values[i] = ConvertToVariant(result.Value);
        }

        for (var i = resolved; i < count; i++)
        {
            errors[i] = OpcResultId.Fail.Code;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<int[]> WriteVqtAsync(
        string[] itemIds,
        OpcItemVqt[] values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(values);
        cancellationToken.ThrowIfCancellationRequested();

        var count = itemIds.Length;
        var errors = new int[count];

        if (count == 0)
        {
            return errors;
        }

        if (values.Length != count)
        {
            Array.Fill(errors, OpcResultId.InvalidArg.Code);
            return errors;
        }

        var writes = new ItemValue[count];
        for (var i = 0; i < count; i++)
        {
            var vqt = values[i];
            writes[i] = new ItemValue(itemIds[i] ?? string.Empty)
            {
                Value = vqt?.Value.Boxed,
                Quality = vqt?.Quality ?? OpcQuality.Good,
                Timestamp = vqt?.Timestamp ?? DateTimeOffset.UtcNow,
            };
        }

        IReadOnlyList<IdentifiedResult> results;
        try
        {
            results = await _serverImpl.WriteVQTAsync(writes, cancellationToken).ConfigureAwait(false);
        }
        catch (OpcException ex)
        {
            Array.Fill(errors, ex.ResultId.Code);
            return errors;
        }

        var resolved = Math.Min(results.Count, count);
        for (var i = 0; i < resolved; i++)
        {
            errors[i] = results[i].ResultId.Code;
        }

        for (var i = resolved; i < count; i++)
        {
            errors[i] = OpcResultId.Fail.Code;
        }

        return errors;
    }

    private static OpcVariant ConvertToVariant(object? value) =>
        value switch
        {
            null => OpcVariant.Empty,
            OpcVariant variant => variant,
            bool b => OpcVariant.FromBoolean(b),
            sbyte sb => OpcVariant.FromInt8(sb),
            byte b => OpcVariant.FromUInt8(b),
            short s => OpcVariant.FromInt16(s),
            ushort us => OpcVariant.FromUInt16(us),
            int i => OpcVariant.FromInt32(i),
            uint ui => OpcVariant.FromUInt32(ui),
            long l => OpcVariant.FromInt64(l),
            ulong ul => OpcVariant.FromUInt64(ul),
            float f => OpcVariant.FromSingle(f),
            double d => OpcVariant.FromDouble(d),
            string s => OpcVariant.FromString(s),
            DateTime dt => OpcVariant.FromDate(dt),
            DateTimeOffset dto => OpcVariant.FromDate(dto.UtcDateTime),
            Guid g => OpcVariant.FromClsid(g),
            _ => OpcVariant.Empty,
        };
}
