//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Hda.Dcom;
using OpcClassic.Ndr;

namespace OpcClassic.Hda.Hosting;

/// <summary>Default HDA per-method dispatcher for managed server hosting.</summary>
public sealed class OpcHdaServerDispatcher : IOpcHdaServerDispatcher
{
    private const int InitialResponseBufferSize = 1024;
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    private readonly IOpcHdaServer _server;

    /// <summary>Initializes a new instance of the <see cref="OpcHdaServerDispatcher"/> class.</summary>
    public OpcHdaServerDispatcher(IOpcHdaServer server)
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
            if (interfaceId == IOPCHDA_Server.InterfaceId)
            {
                return opnum switch
                {
                    5 => await DispatchGetHistorianStatusAsync(cancellationToken).ConfigureAwait(false),
                    8 => await DispatchValidateItemIdsAsync(requestPayload, cancellationToken).ConfigureAwait(false),
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

    private async Task<NdrCallResult> DispatchGetHistorianStatusAsync(CancellationToken cancellationToken)
    {
        OpcServerStatus status = await _server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        ReadOnlyMemory<byte> response = WriteResponse((ref NdrWriter writer) => WriteHistorianStatus(ref writer, status));
        return new NdrCallResult(OpcResultId.Ok.Code, response);
    }

    private async Task<NdrCallResult> DispatchValidateItemIdsAsync(
        ReadOnlyMemory<byte> request,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(request.Span);
        string[] itemIds = ReadStringArray(ref reader);
        int[] results = await _server.ValidateItemIdsAsync(itemIds, cancellationToken).ConfigureAwait(false);
        ReadOnlyMemory<byte> response = WriteResponse((ref NdrWriter writer) => WriteIntArray(ref writer, results));
        return new NdrCallResult(OpcResultId.Ok.Code, response);
    }

    private static string[] ReadStringArray(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new string[count];
        for (int index = 0; index < count; index++)
        {
            values[index] = reader.ReadUnicodeStringPtr() ?? string.Empty;
        }

        return values;
    }

    private static void WriteIntArray(ref NdrWriter writer, ReadOnlySpan<int> values)
    {
        writer.WriteUInt32(unchecked((uint)values.Length));
        for (int index = 0; index < values.Length; index++)
        {
            writer.WriteInt32(values[index]);
        }
    }

    private static void WriteHistorianStatus(ref NdrWriter writer, OpcServerStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        writer.WriteUInt32(ToHistorianStatus(status.State));
        writer.WriteFileTime(ToFileTime(status.CurrentTime));
        writer.WriteFileTime(ToFileTime(status.StartTime));
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Major));
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Minor));
        writer.WriteUInt16(checked((ushort)Math.Max(0, status.ServerVersion.Build)));
        writer.WriteUInt16(0);
        writer.WriteUInt32(checked((uint)Math.Max(0, status.MaxReturnValues)));
        writer.WriteUnicodeStringPtr(status.State.ToString());
        writer.WriteUnicodeStringPtr(status.VendorInfo);
    }

    private static uint ToHistorianStatus(OpcServerState state) => state switch
    {
        OpcServerState.Running => 1u,
        OpcServerState.Failed or OpcServerState.CommFault => 2u,
        _ => 3u,
    };

    private static long ToFileTime(DateTimeOffset value) =>
        value.UtcTicks - FileTimeEpochOffsetTicks;

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
