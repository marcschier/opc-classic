// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable CA1031 // Detached HDA callback completion must isolate faulting clients.

using System.Collections.Concurrent;
using Opc.Classic.Dcom;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using DaConnectionPoint = Opc.Classic.Da.Dcom.IConnectionPoint;
using DaConnectionPointContainer = Opc.Classic.Da.Dcom.IConnectionPointContainer;

namespace Opc.Classic.Samples.SimulationServer.Transports;

/// <summary>
/// A managed OPC HDA server, backed by the shared <see cref="SimulatedPlantModel" />, that the
/// <see cref="OpcHdaServerHost" /> serves over the real cross-platform transport. It answers
/// the HDA "root" calls (status, item attributes, aggregates, item-handle management, and
/// item-id validation) against the model's deterministic historian. Raw/processed read
/// tearoffs require object-IPID routing not yet exposed over the wire.
/// </summary>
public sealed class SimHdaHostServer : IOpcHdaServer, IOPCHDA_SyncRead, IOPCHDA_AsyncRead, DaConnectionPointContainer, DaConnectionPoint
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
    private readonly SimulatedPlantModel _model;
    private readonly Func<IOpcInterfaceRef, DcomOpcHdaDataCallbackSender>? _callbackSenderFactory;
    private readonly ConcurrentDictionary<int, HandleEntry> _handles = new();
    private readonly ConcurrentDictionary<int, DcomOpcHdaDataCallbackSender> _callbacks = new();
    private readonly ConcurrentDictionary<int, CancellationTokenSource> _asyncOperations = new();
    private int _nextHandle = 0x5000;
    private int _nextConnection;
    private int _nextCancelId;

    /// <summary>Initializes a new instance of the <see cref="SimHdaHostServer" /> class.</summary>
    /// <param name="model">The shared deterministic plant model to serve.</param>
    public SimHdaHostServer(
        SimulatedPlantModel model,
        Func<IOpcInterfaceRef, DcomOpcHdaDataCallbackSender>? callbackSenderFactory = null)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _callbackSenderFactory = callbackSenderFactory;
    }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = _model.ServerVersion,
            MaxReturnValues = 500,
            VendorInfo = _model.VendorInfo + " (HDA)",
        });
    }

    /// <inheritdoc />
    public Task GetItemAttributesAsync(
        out int[] attributeIds,
        out string[] attributeNames,
        out string[] attributeDescriptions,
        out int[] attributeDataTypes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        attributeIds = [1, 2];
        attributeNames = ["DataType", "Description"];
        attributeDescriptions = ["Variant type", "Human text"];
        attributeDataTypes = [(int)VarType.VT_I4, (int)VarType.VT_BSTR];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task GetAggregatesAsync(
        out int[] aggregateIds,
        out string[] aggregateNames,
        out string[] aggregateDescriptions,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        aggregateIds = [1, 4, 5, 6];
        aggregateNames = ["Interpolative", "Average", "Minimum", "Maximum"];
        aggregateDescriptions = ["Interpolated value", "Time average", "Minimum value", "Maximum value"];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int[]> GetItemHandlesAsync(
        string[] itemIds,
        int[] clientHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(clientHandles);
        cancellationToken.ThrowIfCancellationRequested();
        var handles = new int[itemIds.Length];
        for (int i = 0; i < itemIds.Length; i++)
        {
            if (_model.TryGetTag(itemIds[i], out _))
            {
                handles[i] = Interlocked.Increment(ref _nextHandle);
                _handles[handles[i]] = new HandleEntry(itemIds[i], i < clientHandles.Length ? clientHandles[i] : i + 1);
            }
            else
            {
                handles[i] = 0;
            }
        }

        return Task.FromResult(handles);
    }

    /// <inheritdoc />
    public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Array.ConvertAll(serverHandles, handle => _handles.TryRemove(handle, out _) ? OpcResultId.Ok.Code : OpcResultId.InvalidHandle.Code));
    }

    /// <inheritdoc />
    public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Array.ConvertAll(
            itemIds,
            itemId => _model.TryGetTag(itemId, out _) ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code));
    }

    public Task<OpcHdaItem[]> ReadRawAsync(
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        bool bounds,
        int[] serverHandles,
        CancellationToken cancellationToken = default)
    {
        _ = bounds;
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset start = ResolveTime(startTime);
        DateTimeOffset end = ResolveTime(endTime);
        if (start > end)
        {
            (start, end) = (end, start);
        }

        int limit = maxValues <= 0 ? 100 : maxValues;
        return Task.FromResult(Array.ConvertAll(serverHandles, handle => ReadRawItem(handle, start, end, limit)));
    }

    public Task<OpcHdaItem[]> ReadProcessedAsync(
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        long resampleIntervalFileTime,
        int[] serverHandles,
        int[] aggregateIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(aggregateIds);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset start = ResolveTime(startTime);
        DateTimeOffset end = ResolveTime(endTime);
        TimeSpan interval = resampleIntervalFileTime > 0 ? TimeSpan.FromTicks(resampleIntervalFileTime) : TimeSpan.FromMinutes(1);
        return Task.FromResult(serverHandles.Select((handle, index) => ReadProcessedItem(handle, start, end, interval, index < aggregateIds.Length ? aggregateIds[index] : 0)).ToArray());
    }

    public Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        return ReadRawAsync(
            OpcHdaTime.FromTimestamp(timestampFileTimes.Length == 0 ? DateTimeOffset.UtcNow : DateTimeOffset.FromFileTime(timestampFileTimes[0])),
            OpcHdaTime.FromTimestamp(timestampFileTimes.Length == 0 ? DateTimeOffset.UtcNow : DateTimeOffset.FromFileTime(timestampFileTimes[^1])),
            timestampFileTimes.Length,
            bounds: true,
            serverHandles,
            cancellationToken);
    }

    public Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default) =>
        Task.FromResult(Array.Empty<OpcHdaModifiedItem>());

    public Task<OpcHdaAttribute[]> ReadAttributeAsync(OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(attributeIds);
        int clientHandle = _handles.TryGetValue(serverHandle, out HandleEntry? entry) ? entry.ClientHandle : 0;
        return Task.FromResult(Array.ConvertAll(attributeIds, id => new OpcHdaAttribute(clientHandle, id, [DateTimeOffset.UtcNow], [OpcVariant.FromString(AttributeText(serverHandle, id))])));
    }

    public Task<int> ReadRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default) =>
        StartReadCompletionAsync(transactionId, startTime, endTime, maxValues, bounds, serverHandles, cancellationToken);

    public Task<int> AdviseRawAsync(int transactionId, OpcHdaTime startTime, long updateIntervalFileTime, int[] serverHandles, CancellationToken cancellationToken = default) =>
        StartReadCompletionAsync(transactionId, startTime, OpcHdaTime.FromTimestamp(DateTimeOffset.UtcNow), 1, bounds: true, serverHandles, cancellationToken, dataChange: true);

    public Task<int> ReadProcessedAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default) =>
        StartReadCompletionAsync(transactionId, startTime, endTime, 0, bounds: true, serverHandles, cancellationToken);

    public Task<int> AdviseProcessedAsync(int transactionId, OpcHdaTime startTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, int intervalCount, CancellationToken cancellationToken = default) =>
        AdviseRawAsync(transactionId, startTime, resampleIntervalFileTime, serverHandles, cancellationToken);

    public Task<int> ReadAtTimeAsync(int transactionId, long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        return StartReadCompletionAsync(
            transactionId,
            OpcHdaTime.FromTimestamp(timestampFileTimes.Length == 0 ? DateTimeOffset.UtcNow : DateTimeOffset.FromFileTime(timestampFileTimes[0])),
            OpcHdaTime.FromTimestamp(timestampFileTimes.Length == 0 ? DateTimeOffset.UtcNow : DateTimeOffset.FromFileTime(timestampFileTimes[^1])),
            timestampFileTimes.Length,
            bounds: true,
            serverHandles,
            cancellationToken);
    }

    public Task<int> ReadModifiedAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default) =>
        StartReadCompletionAsync(transactionId, startTime, endTime, maxValues, bounds: true, serverHandles, cancellationToken);

    public Task<int> ReadAttributeAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attributeIds);
        return StartReadCompletionAsync(transactionId, startTime, endTime, attributeIds.Length, bounds: true, [serverHandle], cancellationToken);
    }

    public async Task CancelAsync(int cancelId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_asyncOperations.TryRemove(cancelId, out CancellationTokenSource? cts))
        {
            await cts.CancelAsync().ConfigureAwait(false);
            cts.Dispose();
        }
    }

    public Task<IOpcInterfaceRef> EnumConnectionPointsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(OpcGuids.IID_IEnumConnectionPoints, 0, 1, 1, 1, Guid.CreateVersion7(), 0, []));
    }

    public Task<IOpcInterfaceRef> FindConnectionPointAsync(Guid iid, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (iid != IOPCHDA_DataCallback.InterfaceId)
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }

        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(DaConnectionPoint.InterfaceId, 0, 1, 1, 1, Guid.CreateVersion7(), 0, []));
    }

    public Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(IOPCHDA_DataCallback.InterfaceId);
    }

    public Task<int> AdviseAsync(IOpcInterfaceRef sink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);
        cancellationToken.ThrowIfCancellationRequested();
        if (sink.Iid != IOPCHDA_DataCallback.InterfaceId || _callbackSenderFactory is null)
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }

        int cookie = Interlocked.Increment(ref _nextConnection);
        _callbacks[cookie] = _callbackSenderFactory(sink);
        return Task.FromResult(cookie);
    }

    public async Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_callbacks.TryRemove(cookie, out DcomOpcHdaDataCallbackSender? sender))
        {
            await sender.DisposeAsync().ConfigureAwait(false);
        }
    }

    private Task<int> StartReadCompletionAsync(
        int transactionId,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        bool bounds,
        int[] serverHandles,
        CancellationToken cancellationToken,
        bool dataChange = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int cancelId = Interlocked.Increment(ref _nextCancelId);
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _asyncOperations[cancelId] = cts;
        _ = CompleteReadAsync(cancelId, transactionId, startTime, endTime, maxValues, bounds, serverHandles.ToArray(), dataChange, cts);
        return Task.FromResult(cancelId);
    }

    private async Task CompleteReadAsync(
        int cancelId,
        int transactionId,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        bool bounds,
        int[] serverHandles,
        bool dataChange,
        CancellationTokenSource cts)
    {
        try
        {
            OpcHdaItem[] items = await ReadRawAsync(startTime, endTime, maxValues, bounds, serverHandles, cts.Token).ConfigureAwait(false);
            int[] errors = Array.ConvertAll(serverHandles, handle => _handles.ContainsKey(handle) ? OpcResultId.Ok.Code : OpcResultId.InvalidHandle.Code);
            foreach (KeyValuePair<int, DcomOpcHdaDataCallbackSender> entry in _callbacks.ToArray())
            {
                try
                {
                    if (dataChange)
                    {
                        await entry.Value.OnDataChangeAsync(transactionId, OpcResultId.Ok.Code, items, errors, cts.Token).ConfigureAwait(false);
                    }
                    else
                    {
                        await entry.Value.OnReadCompleteAsync(transactionId, OpcResultId.Ok.Code, items, errors, cts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    await DropCallbackAsync(entry.Key).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _ = ex;
        }
        finally
        {
            _asyncOperations.TryRemove(cancelId, out _);
            cts.Dispose();
        }
    }

    private async Task DropCallbackAsync(int cookie)
    {
        if (_callbacks.TryRemove(cookie, out DcomOpcHdaDataCallbackSender? sender))
        {
            await sender.DisposeAsync().ConfigureAwait(false);
        }
    }

    private OpcHdaItem ReadRawItem(int serverHandle, DateTimeOffset start, DateTimeOffset end, int maxValues)
    {
        if (!_handles.TryGetValue(serverHandle, out HandleEntry? entry) || !_model.TryGetTag(entry.ItemId, out SimulatedTag tag))
        {
            return new OpcHdaItem(0, 0, [], [], []);
        }

        IReadOnlyList<(DateTimeOffset Timestamp, object Value)> history = _model.History(tag, start, end, TimeSpan.FromSeconds(1));
        var selected = history.Take(Math.Max(1, maxValues)).ToArray();
        return new OpcHdaItem(
            entry.ClientHandle,
            0,
            selected.Select(static point => point.Timestamp).ToArray(),
            selected.Select(static _ => (uint)OpcQuality.Good.RawValue).ToArray(),
            selected.Select(static point => ToVariant(point.Value)).ToArray());
    }

    private OpcHdaItem ReadProcessedItem(int serverHandle, DateTimeOffset start, DateTimeOffset end, TimeSpan interval, int aggregateId)
    {
        if (!_handles.TryGetValue(serverHandle, out HandleEntry? entry) || !_model.TryGetTag(entry.ItemId, out SimulatedTag tag))
        {
            return new OpcHdaItem(0, aggregateId, [], [], []);
        }

        var timestamps = new List<DateTimeOffset>();
        var qualities = new List<uint>();
        var values = new List<OpcVariant>();
        for (DateTimeOffset cursor = start; cursor < end; cursor = cursor.Add(interval))
        {
            timestamps.Add(cursor);
            qualities.Add((uint)OpcQuality.Good.RawValue);
            values.Add(ToVariant(_model.ValueAt(tag, cursor)));
        }

        return new OpcHdaItem(entry.ClientHandle, aggregateId, [.. timestamps], [.. qualities], [.. values]);
    }

    private static DateTimeOffset ResolveTime(OpcHdaTime time) =>
        time.IsStringExpression ? DateTimeOffset.UtcNow : time.Timestamp;

    private static string AttributeText(int serverHandle, int attributeId) =>
        string.Create(System.Globalization.CultureInfo.InvariantCulture, $"Handle {serverHandle} attribute {attributeId}");

    private static OpcVariant ToVariant(object? value) => value switch
    {
        null => OpcVariant.Empty,
        OpcVariant variant => variant,
        bool boolean => OpcVariant.FromBoolean(boolean),
        short int16 => OpcVariant.FromInt16(int16),
        int int32 => OpcVariant.FromInt32(int32),
        float single => OpcVariant.FromSingle(single),
        double real => OpcVariant.FromDouble(real),
        string text => OpcVariant.FromString(text),
        _ => OpcVariant.Empty,
    };

    private sealed record HandleEntry(string ItemId, int ClientHandle);
}
