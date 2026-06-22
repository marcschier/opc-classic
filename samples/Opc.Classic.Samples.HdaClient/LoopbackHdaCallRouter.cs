// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Collections.Concurrent;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Samples.HdaServer;

namespace Opc.Classic.Samples.HdaClient;

internal sealed class LoopbackHdaCallRouter
{
    private const int ResponseBufferSize = 64 * 1024;
    private const int GetHistorianStatusOpnum = 5;
    private const int AnnotationReadCapability = 1;
    private const int AsyncCancelIdOffset = 10_000;
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    private static readonly TimeSpan AsyncReadDelay = TimeSpan.FromSeconds(5);
    private readonly IOpcHdaServer _server;
    private readonly HistoricalDataStore _store;
    private readonly ConcurrentDictionary<int, ItemRegistration> _registrations = new();
    private readonly HdaAnnotationResult[] _annotations;
    private int _nextServerHandle = 1000;

    public LoopbackHdaCallRouter(IOpcHdaServer server, HistoricalDataStore store)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _annotations =
        [
            new HdaAnnotationResult
            {
                ItemId = "Sensor.Temperature",
                Annotations =
                [
                    new HdaAnnotation
                    {
                        Timestamp = store.EndTime.AddMinutes(-30),
                        AnnotationTime = store.EndTime.AddMinutes(-29),
                        AnnotationText = "Temperature probe calibration verified.",
                        User = "sample-client",
                    },
                ],
            },
        ];
    }

    public Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (interfaceId == IOPCHDA_Server.InterfaceId)
        {
            return DispatchServerAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCHDA_SyncRead.InterfaceId)
        {
            return DispatchSyncReadAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCHDA_AsyncRead.InterfaceId)
        {
            return DispatchAsyncReadAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCHDA_SyncAnnotations.InterfaceId)
        {
            return DispatchSyncAnnotationsAsync(opnum, requestPayload);
        }

        return Task.FromResult(NotImplemented());
    }

    public IReadOnlyCollection<string> ItemIds => _store.ItemIds;

    public IReadOnlyList<HdaAnnotationResult> ReadAnnotations(IReadOnlyList<string> itemIds)
    {
        ArgumentNullException.ThrowIfNull(itemIds);

        return itemIds
            .Select(itemId => _annotations.FirstOrDefault(annotation =>
                string.Equals(annotation.ItemId, itemId, StringComparison.OrdinalIgnoreCase))
                ?? new HdaAnnotationResult { ItemId = itemId })
            .ToArray();
    }

    public IEnumerable<HdaBrowseElement> Browse(string itemIdPrefix, HdaBrowseType browseType)
    {
        if (string.IsNullOrWhiteSpace(itemIdPrefix))
        {
            if (browseType is HdaBrowseType.Branch or HdaBrowseType.Flat)
            {
                yield return new HdaBrowseElement
                {
                    Name = "Sensor",
                    ItemId = "Sensor",
                    BrowseType = HdaBrowseType.Branch,
                };
            }

            if (browseType is HdaBrowseType.Leaf or HdaBrowseType.Flat)
            {
                foreach (string itemId in _store.ItemIds.Order(StringComparer.OrdinalIgnoreCase))
                {
                    yield return CreateLeaf(itemId);
                }
            }

            yield break;
        }

        if (itemIdPrefix.Equals("Sensor", StringComparison.OrdinalIgnoreCase) && browseType != HdaBrowseType.Branch)
        {
            foreach (string itemId in _store.ItemIds.Order(StringComparer.OrdinalIgnoreCase))
            {
                yield return CreateLeaf(itemId);
            }
        }
    }

    private static HdaBrowseElement CreateLeaf(string itemId) => new()
    {
        Name = itemId[(itemId.LastIndexOf('.') + 1)..],
        ItemId = itemId,
        BrowseType = HdaBrowseType.Leaf,
    };

    private Task<NdrCallResult> DispatchServerAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) => opnum switch
        {
            GetHistorianStatusOpnum => DispatchGetStatusAsync(cancellationToken),
            IOPCHDA_Server.Opnums.GetItemHandlesAsync => Task.FromResult(DispatchGetItemHandles(requestPayload)),
            IOPCHDA_Server.Opnums.ReleaseItemHandlesAsync => Task.FromResult(DispatchReleaseItemHandles(requestPayload)),
            IOPCHDA_Server.Opnums.ValidateItemIDsAsync => DispatchValidateItemIdsAsync(requestPayload, cancellationToken),
            _ => Task.FromResult(NotImplemented()),
        };

    private async Task<NdrCallResult> DispatchGetStatusAsync(CancellationToken cancellationToken)
    {
        OpcServerStatus status = await _server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        return Ok(WritePayload((ref NdrWriter writer) => WriteHistorianStatus(ref writer, status)));
    }

    private NdrCallResult DispatchGetItemHandles(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        string[] itemIds = ReadStringArray(ref reader);
        int[] clientHandles = ReadInt32Array(ref reader);
        var serverHandles = new int[itemIds.Length];

        for (int index = 0; index < itemIds.Length; index++)
        {
            if (!_store.Contains(itemIds[index]))
            {
                continue;
            }

            int serverHandle = Interlocked.Increment(ref _nextServerHandle);
            int clientHandle = index < clientHandles.Length ? clientHandles[index] : index + 1;
            _registrations[serverHandle] = new ItemRegistration(itemIds[index], clientHandle);
            serverHandles[index] = serverHandle;
        }

        return Ok(WritePayload((ref NdrWriter writer) => WriteInt32Array(ref writer, serverHandles)));
    }

    private NdrCallResult DispatchReleaseItemHandles(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        int[] serverHandles = ReadInt32Array(ref reader);
        var results = new int[serverHandles.Length];

        for (int index = 0; index < serverHandles.Length; index++)
        {
            results[index] = _registrations.TryRemove(serverHandles[index], out _)
                ? OpcResultId.Ok.Code
                : OpcResultId.InvalidHandle.Code;
        }

        return Ok(WritePayload((ref NdrWriter writer) => WriteInt32Array(ref writer, results)));
    }

    private async Task<NdrCallResult> DispatchValidateItemIdsAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        string[] itemIds = ReadStringArray(ref reader);
        int[] results = await _server.ValidateItemIdsAsync(itemIds, cancellationToken).ConfigureAwait(false);
        return Ok(WritePayload((ref NdrWriter writer) => WriteInt32Array(ref writer, results)));
    }

    private Task<NdrCallResult> DispatchSyncReadAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) => opnum switch
        {
            IOPCHDA_SyncRead.Opnums.ReadRawAsync => DispatchReadRawAsync(requestPayload, cancellationToken),
            IOPCHDA_SyncRead.Opnums.ReadProcessedAsync => DispatchReadProcessedAsync(requestPayload, cancellationToken),
            _ => Task.FromResult(NotImplemented()),
        };

    private async Task<NdrCallResult> DispatchReadRawAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        OpcHdaTime startTime = NdrOpcHdaTimeCodec.Read(ref reader);
        OpcHdaTime endTime = NdrOpcHdaTimeCodec.Read(ref reader);
        int maxValues = reader.ReadInt32();
        bool bounds = reader.ReadInt32() != 0;
        int[] serverHandles = ReadInt32Array(ref reader);
        _ = bounds;

        string[] itemIds = ResolveItemIds(serverHandles);
        OpcHdaItem[] items = await _server.ReadRawAsync(itemIds, startTime, endTime, maxValues, cancellationToken)
            .ConfigureAwait(false);
        OpcHdaItem[] correlatedItems = ReassignClientHandles(items, serverHandles);

        return Ok(WritePayload((ref NdrWriter writer) => WriteItemArray(ref writer, correlatedItems)));
    }

    private async Task<NdrCallResult> DispatchReadProcessedAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        OpcHdaTime startTime = NdrOpcHdaTimeCodec.Read(ref reader);
        OpcHdaTime endTime = NdrOpcHdaTimeCodec.Read(ref reader);
        TimeSpan resampleInterval = TimeSpan.FromTicks(Math.Abs(reader.ReadInt64()));
        int[] serverHandles = ReadInt32Array(ref reader);
        int[] aggregateIds = ReadInt32Array(ref reader);
        var correlatedItems = new OpcHdaItem[serverHandles.Length];

        for (int index = 0; index < serverHandles.Length; index++)
        {
            string itemId = ResolveItemId(serverHandles[index]);
            HdaAggregate aggregate = index < aggregateIds.Length ? (HdaAggregate)aggregateIds[index] : HdaAggregate.Average;
            OpcHdaItem[] items = await _server.ReadProcessedAsync(
                [itemId],
                startTime,
                endTime,
                resampleInterval,
                aggregate,
                cancellationToken).ConfigureAwait(false);
            correlatedItems[index] = ReassignClientHandle(items[0], serverHandles[index]);
        }

        return Ok(WritePayload((ref NdrWriter writer) => WriteItemArray(ref writer, correlatedItems)));
    }

    private static Task<NdrCallResult> DispatchAsyncReadAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) => opnum switch
        {
            IOPCHDA_AsyncRead.Opnums.ReadRawAsync => DispatchAsyncReadRawAsync(requestPayload, cancellationToken),
            IOPCHDA_AsyncRead.Opnums.ReadProcessedAsync => DispatchAsyncReadProcessedAsync(requestPayload, cancellationToken),
            IOPCHDA_AsyncRead.Opnums.CancelAsync => Task.FromResult(DispatchAsyncCancel(requestPayload)),
            _ => Task.FromResult(NotImplemented()),
        };

    private static async Task<NdrCallResult> DispatchAsyncReadRawAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int transactionId = reader.ReadInt32();
        _ = NdrOpcHdaTimeCodec.Read(ref reader);
        _ = NdrOpcHdaTimeCodec.Read(ref reader);
        _ = reader.ReadInt32();
        _ = reader.ReadInt32();
        _ = ReadInt32Array(ref reader);

        await Task.Delay(AsyncReadDelay, cancellationToken).ConfigureAwait(false);
        return Ok(WritePayload((ref NdrWriter writer) => writer.WriteInt32(transactionId + AsyncCancelIdOffset)));
    }

    private static async Task<NdrCallResult> DispatchAsyncReadProcessedAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        int transactionId = reader.ReadInt32();
        _ = NdrOpcHdaTimeCodec.Read(ref reader);
        _ = NdrOpcHdaTimeCodec.Read(ref reader);
        _ = reader.ReadInt64();
        _ = ReadInt32Array(ref reader);
        _ = ReadInt32Array(ref reader);

        await Task.Delay(AsyncReadDelay, cancellationToken).ConfigureAwait(false);
        return Ok(WritePayload((ref NdrWriter writer) => writer.WriteInt32(transactionId + AsyncCancelIdOffset)));
    }

    private static NdrCallResult DispatchAsyncCancel(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        _ = reader.ReadInt32();
        return Ok(ReadOnlyMemory<byte>.Empty);
    }

    private static Task<NdrCallResult> DispatchSyncAnnotationsAsync(int opnum, ReadOnlyMemory<byte> requestPayload)
    {
        _ = requestPayload;
        return Task.FromResult(opnum == IOPCHDA_SyncAnnotations.Opnums.QueryCapabilitiesAsync
            ? Ok(WritePayload((ref NdrWriter writer) => writer.WriteInt32(AnnotationReadCapability)))
            : NotImplemented());
    }

    private string[] ResolveItemIds(int[] serverHandles)
    {
        var itemIds = new string[serverHandles.Length];
        for (int index = 0; index < itemIds.Length; index++)
        {
            itemIds[index] = ResolveItemId(serverHandles[index]);
        }

        return itemIds;
    }

    private string ResolveItemId(int serverHandle) =>
        _registrations.TryGetValue(serverHandle, out ItemRegistration registration)
            ? registration.ItemId
            : string.Empty;

    private OpcHdaItem[] ReassignClientHandles(OpcHdaItem[] items, int[] serverHandles)
    {
        var correlatedItems = new OpcHdaItem[items.Length];
        for (int index = 0; index < items.Length; index++)
        {
            correlatedItems[index] = ReassignClientHandle(items[index], serverHandles[index]);
        }

        return correlatedItems;
    }

    private OpcHdaItem ReassignClientHandle(OpcHdaItem item, int serverHandle)
    {
        int clientHandle = _registrations.TryGetValue(serverHandle, out ItemRegistration registration)
            ? registration.ClientHandle
            : item.ClientHandle;

        return new OpcHdaItem(clientHandle, item.AggregateHandle, item.Timestamps, item.Qualities, item.Values);
    }

    private static string[] ReadStringArray(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new string[count];
        for (int index = 0; index < values.Length; index++)
        {
            values[index] = reader.ReadUnicodeStringPtr() ?? string.Empty;
        }

        return values;
    }

    private static int[] ReadInt32Array(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new int[count];
        for (int index = 0; index < values.Length; index++)
        {
            values[index] = reader.ReadInt32();
        }

        return values;
    }

    private static void WriteInt32Array(ref NdrWriter writer, ReadOnlySpan<int> values)
    {
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    private static void WriteItemArray(ref NdrWriter writer, ReadOnlySpan<OpcHdaItem> items)
    {
        writer.WriteUInt32(unchecked((uint)items.Length));
        foreach (OpcHdaItem item in items)
        {
            NdrOpcHdaItemCodec.Write(ref writer, item);
        }
    }

    private static void WriteHistorianStatus(ref NdrWriter writer, OpcServerStatus status)
    {
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

    private static NdrCallResult Ok(ReadOnlyMemory<byte> payload) => new(OpcResultId.Ok.Code, payload);
    private static NdrCallResult NotImplemented() => new(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[ResponseBufferSize];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position).ToArray();
    }

    private readonly record struct ItemRegistration(string ItemId, int ClientHandle);

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
