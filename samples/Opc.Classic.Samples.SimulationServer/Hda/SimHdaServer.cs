// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.SimulationServer.Hda;

/// <summary>
/// In-memory OPC HDA server backed by the shared deterministic simulation model.
/// </summary>
public sealed class SimHdaServer : IOpcHdaServer, IHdaServer, IOPCHDA_SyncRead, IOPCHDA_SyncUpdate, IOPCHDA_SyncAnnotations
{
    private const int MaxReturnValues = 1000;
    private static readonly HdaAggregate[] SupportedAggregates =
    [
        HdaAggregate.Interpolative,
        HdaAggregate.Average,
        HdaAggregate.Minimum,
        HdaAggregate.Maximum,
        HdaAggregate.Count,
    ];

    private readonly SimulatedPlantModel _model;
    private readonly object _gate = new();
    private readonly Dictionary<int, HandleRegistration> _handles = new();
    private readonly Dictionary<string, SortedDictionary<DateTimeOffset, HistoryPoint>> _overlay = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<DeletedRange>> _deleted = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<AnnotationPoint>> _annotations = new(StringComparer.OrdinalIgnoreCase);
    private readonly OpcHdaServerDispatcher _serverDispatcher;
    private readonly IOPCHDA_SyncReadServerDispatcher _syncReadDispatcher;
    private readonly IOPCHDA_SyncUpdateServerDispatcher _syncUpdateDispatcher;
    private readonly IOPCHDA_SyncAnnotationsServerDispatcher _syncAnnotationsDispatcher;
    private int _nextHandle = 2000;

    /// <summary>
    /// Initializes a new simulation HDA server instance.
    /// </summary>
    public SimHdaServer(SimulatedPlantModel model, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _model = model;
        _serverDispatcher = new OpcHdaServerDispatcher(this);
        _syncReadDispatcher = new IOPCHDA_SyncReadServerDispatcher(this);
        _syncUpdateDispatcher = new IOPCHDA_SyncUpdateServerDispatcher(this);
        _syncAnnotationsDispatcher = new IOPCHDA_SyncAnnotationsServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? ServerShutdown;

    /// <summary>
    /// Gets the in-memory call channel registered for HDA clients.
    /// </summary>
    public InMemoryCallChannel Channel { get; }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = _model.StartTimeUtc,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = _model.ServerVersion,
            VendorInfo = _model.VendorInfo,
            MaxReturnValues = MaxReturnValues,
        });
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<HdaBrowseElement> BrowseAsync(string itemIdPrefix, HdaBrowseType browseType, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIdPrefix);
        await Task.CompletedTask.ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        string branch = itemIdPrefix.Trim('.');
        if (browseType is HdaBrowseType.Branch or HdaBrowseType.Flat or HdaBrowseType.Items)
        {
            foreach (string child in _model.BrowseBranches(branch))
            {
                string itemId = branch.Length == 0 ? child : branch + "." + child;
                yield return new HdaBrowseElement { Name = child, ItemId = itemId, BrowseType = HdaBrowseType.Branch };
            }
        }

        if (browseType is HdaBrowseType.Leaf or HdaBrowseType.Flat or HdaBrowseType.Items)
        {
            IEnumerable<SimulatedTag> leaves = branch.Length == 0 && browseType == HdaBrowseType.Flat ? _model.Tags : _model.BrowseLeaves(branch);
            foreach (SimulatedTag tag in leaves.OrderBy(static tag => tag.ItemId, StringComparer.OrdinalIgnoreCase))
            {
                yield return new HdaBrowseElement { Name = tag.Name, ItemId = tag.ItemId, BrowseType = HdaBrowseType.Leaf };
            }
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HdaAggregate>> GetSupportedAggregatesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<HdaAggregate>>(Array.AsReadOnly(SupportedAggregates));
    }

    /// <inheritdoc />
    public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(itemIds.Select(itemId => _model.TryGetTag(itemId, out _) ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code).ToArray());
    }

    /// <inheritdoc />
    public Task<int[]> GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(clientHandles);
        cancellationToken.ThrowIfCancellationRequested();

        var handles = new int[itemIds.Length];
        lock (_gate)
        {
            for (int i = 0; i < itemIds.Length; i++)
            {
                if (!_model.TryGetTag(itemIds[i], out SimulatedTag tag))
                {
                    continue;
                }

                int handle = Interlocked.Increment(ref _nextHandle);
                _handles[handle] = new HandleRegistration(tag.ItemId, i < clientHandles.Length ? clientHandles[i] : i + 1);
                handles[i] = handle;
            }
        }

        return Task.FromResult(handles);
    }

    /// <inheritdoc />
    public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        var errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                errors[i] = _handles.Remove(serverHandles[i]) ? OpcResultId.Ok.Code : OpcResultId.InvalidHandle.Code;
            }
        }

        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task GetItemAttributesAsync(out int[] attributeIds, out string[] attributeNames, out string[] attributeDescriptions, out int[] attributeDataTypes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        attributeIds = [1, 2, 3];
        attributeNames = ["DataType", "Description", "EngUnits"];
        attributeDescriptions = ["Canonical data type", "Item description", "Engineering units"];
        attributeDataTypes = [(int)VarType.VT_I2, (int)VarType.VT_BSTR, (int)VarType.VT_BSTR];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task GetAggregatesAsync(out int[] aggregateIds, out string[] aggregateNames, out string[] aggregateDescriptions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        aggregateIds = SupportedAggregates.Select(static aggregate => (int)aggregate).ToArray();
        aggregateNames = SupportedAggregates.Select(static aggregate => aggregate.ToString()).ToArray();
        aggregateDescriptions = ["Interpolated value", "Arithmetic average", "Minimum value", "Maximum value", "Sample count"];
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OpcHdaItem[]> ReadRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = Resolve(startTime, now);
        DateTimeOffset end = Resolve(endTime, now);
        int limit = NormalizeLimit(maxValues);
        return Task.FromResult(serverHandles.Select(handle => RawItem(handle, start, end, limit, bounds)).ToArray());
    }

    /// <inheritdoc />
    public Task<OpcHdaItem[]> ReadProcessedAsync(OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(aggregateIds);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = Resolve(startTime, now);
        DateTimeOffset end = Resolve(endTime, now);
        TimeSpan interval = resampleIntervalFileTime <= 0 ? TimeSpan.FromMinutes(1) : TimeSpan.FromTicks(resampleIntervalFileTime);
        return Task.FromResult(serverHandles.Select((handle, index) => ProcessedItem(handle, start, end, interval, AggregateAt(aggregateIds, index))).ToArray());
    }

    /// <inheritdoc />
    public Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset[] timestamps = timestampFileTimes.Select(DateTimeOffset.FromFileTime).ToArray();
        return Task.FromResult(serverHandles.Select(handle => AtTimeItem(handle, timestamps)).ToArray());
    }

    /// <inheritdoc />
    public Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = Resolve(startTime, now);
        DateTimeOffset end = Resolve(endTime, now);
        int limit = NormalizeLimit(maxValues);
        return Task.FromResult(serverHandles.Select(handle => ModifiedItem(handle, start, end, limit)).ToArray());
    }

    /// <inheritdoc />
    public Task<OpcHdaAttribute[]> ReadAttributeAsync(OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(attributeIds);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset timestamp = Resolve(endTime, DateTimeOffset.UtcNow);
        return Task.FromResult(attributeIds.Select(attributeId => Attribute(serverHandle, attributeId, timestamp)).ToArray());
    }

    /// <inheritdoc />
    public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(0x1F);
    }

    /// <inheritdoc />
    public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(dataValues);
        ArgumentNullException.ThrowIfNull(qualities);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Upsert(serverHandles, timestampFileTimes, dataValues, qualities, replaceOnly: false, editType: 1));
    }

    /// <inheritdoc />
    public Task<int[]> ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(dataValues);
        ArgumentNullException.ThrowIfNull(qualities);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Upsert(serverHandles, timestampFileTimes, dataValues, qualities, replaceOnly: true, editType: 2));
    }

    /// <inheritdoc />
    public Task<int[]> InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(dataValues);
        ArgumentNullException.ThrowIfNull(qualities);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Upsert(serverHandles, timestampFileTimes, dataValues, qualities, replaceOnly: false, editType: 3));
    }

    /// <inheritdoc />
    public Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = Resolve(startTime, now);
        DateTimeOffset end = Resolve(endTime, now);
        if (start > end)
        {
            (start, end) = (end, start);
        }

        var results = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (!TryGetTagLocked(serverHandles[i], out SimulatedTag tag, out _))
                {
                    results[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                RemoveOverlayLocked(tag.ItemId, start, end);
                RangesLocked(tag.ItemId).Add(new DeletedRange(start, end));
                results[i] = OpcResultId.Ok.Code;
            }
        }

        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task<int[]> DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        cancellationToken.ThrowIfCancellationRequested();
        var results = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (!TryGetTagLocked(serverHandles[i], out SimulatedTag tag, out _))
                {
                    results[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                DateTimeOffset timestamp = DateTimeOffset.FromFileTime(i < timestampFileTimes.Length ? timestampFileTimes[i] : 0);
                if (_overlay.TryGetValue(tag.ItemId, out SortedDictionary<DateTimeOffset, HistoryPoint>? points))
                {
                    _ = points.Remove(timestamp);
                }

                RangesLocked(tag.ItemId).Add(new DeletedRange(timestamp, timestamp));
                results[i] = OpcResultId.Ok.Code;
            }
        }

        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task<OpcHdaAnnotation[]> ReadAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = Resolve(startTime, now);
        DateTimeOffset end = Resolve(endTime, now);
        return Task.FromResult(serverHandles.Select(handle => AnnotationItem(handle, start, end, now)).ToArray());
    }

    /// <inheritdoc />
    public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(annotationValues);
        cancellationToken.ThrowIfCancellationRequested();
        var results = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (!TryGetTagLocked(serverHandles[i], out SimulatedTag tag, out _))
                {
                    results[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                List<AnnotationPoint> points = AnnotationsLocked(tag.ItemId);
                if (i < annotationValues.Length)
                {
                    Append(points, annotationValues[i]);
                }
                else
                {
                    DateTimeOffset timestamp = DateTimeOffset.FromFileTime(i < timestampFileTimes.Length ? timestampFileTimes[i] : 0);
                    points.Add(new AnnotationPoint(timestamp, DateTimeOffset.UtcNow, "Annotation", "operator"));
                }

                results[i] = OpcResultId.Ok.Code;
            }
        }

        return Task.FromResult(results);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HdaReadResult>> ReadRawAsync(IReadOnlyList<string> itemIds, HdaTime startTime, HdaTime endTime, int maxValuesPerItem, bool includeBounds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = startTime.ResolveAt(now);
        DateTimeOffset end = endTime.ResolveAt(now);
        int limit = NormalizeLimit(maxValuesPerItem);
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(itemId => RawResult(itemId, start, end, limit, includeBounds)).ToArray());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HdaReadResult>> ReadProcessedAsync(IReadOnlyList<AggregateRequest> requests, HdaTime startTime, HdaTime endTime, TimeSpan resampleInterval, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = startTime.ResolveAt(now);
        DateTimeOffset end = endTime.ResolveAt(now);
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(requests.Select(request => ProcessedResult(request.ItemId, start, end, resampleInterval, request.Aggregate)).ToArray());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HdaReadResult>> ReadAtTimeAsync(IReadOnlyList<string> itemIds, IReadOnlyList<DateTimeOffset> timestamps, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(timestamps);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(itemId => AtTimeResult(itemId, timestamps)).ToArray());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HdaAnnotationResult>> ReadAnnotationsAsync(IReadOnlyList<string> itemIds, HdaTime startTime, HdaTime endTime, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateTimeOffset start = startTime.ResolveAt(now);
        DateTimeOffset end = endTime.ResolveAt(now);
        return Task.FromResult<IReadOnlyList<HdaAnnotationResult>>(itemIds.Select(itemId => AnnotationResult(itemId, start, end, now)).ToArray());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<HdaReadResult>> ReadNextAsync(IReadOnlyList<string> itemIds, IReadOnlyList<int> continuationHandles, int maxValuesPerItem, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(continuationHandles);
        _ = maxValuesPerItem;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(static itemId => new HdaReadResult { ItemId = itemId }).ToArray());
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    private static void Append(List<AnnotationPoint> points, OpcHdaAnnotation annotation)
    {
        for (int i = 0; i < annotation.Timestamps.Length; i++)
        {
            string text = i < annotation.Annotations.Length ? annotation.Annotations[i] ?? string.Empty : string.Empty;
            DateTimeOffset annotationTime = i < annotation.AnnotationTimes.Length ? annotation.AnnotationTimes[i] : DateTimeOffset.UtcNow;
            string user = i < annotation.Users.Length ? annotation.Users[i] ?? "operator" : "operator";
            points.Add(new AnnotationPoint(annotation.Timestamps[i], annotationTime, text, user));
        }
    }

    private static HdaAggregate AggregateAt(int[] aggregateIds, int index)
    {
        int id = index < aggregateIds.Length ? aggregateIds[index] : (int)HdaAggregate.Average;
        return Enum.IsDefined(typeof(HdaAggregate), id) ? (HdaAggregate)id : HdaAggregate.Average;
    }

    private static DateTimeOffset Anchor(DateTimeOffset start, DateTimeOffset end, DateTimeOffset now)
    {
        if (start > end)
        {
            (start, end) = (end, start);
        }

        if (start == end)
        {
            return start;
        }

        DateTimeOffset midpoint = start.AddTicks((end - start).Ticks / 2);
        return midpoint <= now ? midpoint : now;
    }

    private static TimeSpan Interval(DateTimeOffset start, DateTimeOffset end, int limit)
    {
        TimeSpan range = end >= start ? end - start : start - end;
        if (range <= TimeSpan.Zero)
        {
            return TimeSpan.FromSeconds(1);
        }

        return TimeSpan.FromTicks(Math.Max(TimeSpan.FromSeconds(1).Ticks, range.Ticks / (Math.Clamp(limit, 2, MaxReturnValues) - 1)));
    }

    private static int NormalizeLimit(int requested) => requested <= 0 ? MaxReturnValues : Math.Min(requested, MaxReturnValues);

    private static DateTimeOffset Resolve(OpcHdaTime time, DateTimeOffset now) =>
        time.IsStringExpression ? HdaTime.Relative(time.StringExpression ?? "NOW").ResolveAt(now) : time.Timestamp.ToUniversalTime();

    private static double ToDouble(object? value) =>
        value switch
        {
            null => 0.0,
            bool b => b ? 1.0 : 0.0,
            string s => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed) ? parsed : 0.0,
            IConvertible convertible => convertible.ToDouble(CultureInfo.InvariantCulture),
            _ => 0.0,
        };

    private static VarType ToVarType(SimulatedDataType dataType) =>
        dataType switch
        {
            SimulatedDataType.Boolean => VarType.VT_BOOL,
            SimulatedDataType.Int16 => VarType.VT_I2,
            SimulatedDataType.Int32 => VarType.VT_I4,
            SimulatedDataType.Single => VarType.VT_R4,
            SimulatedDataType.Double => VarType.VT_R8,
            SimulatedDataType.String => VarType.VT_BSTR,
            _ => VarType.VT_EMPTY,
        };

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        if (interfaceId == IOPCHDA_Server.InterfaceId)
        {
            return _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCHDA_SyncRead.InterfaceId)
        {
            return ToCallResultAsync(_syncReadDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCHDA_SyncUpdate.InterfaceId)
        {
            return ToCallResultAsync(_syncUpdateDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCHDA_SyncAnnotations.InterfaceId)
        {
            return ToCallResultAsync(_syncAnnotationsDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
    }

    private static async Task<NdrCallResult> ToCallResultAsync(ValueTask<DispatchResult> dispatch) =>
        (await dispatch.ConfigureAwait(false)).ToNdrCallResult();

    private object Aggregate(SimulatedTag tag, IReadOnlyList<HistoryPoint> points, HdaAggregate aggregate, DateTimeOffset bucketStart)
    {
        if (aggregate == HdaAggregate.Interpolative || points.Count == 0)
        {
            return ValueAt(tag, bucketStart);
        }

        return aggregate switch
        {
            HdaAggregate.Count => points.Count,
            HdaAggregate.Minimum => points.Min(static point => ToDouble(point.Value)),
            HdaAggregate.Maximum => points.Max(static point => ToDouble(point.Value)),
            _ => points.Average(static point => ToDouble(point.Value)),
        };
    }

    private OpcHdaAnnotation AnnotationItem(int serverHandle, DateTimeOffset start, DateTimeOffset end, DateTimeOffset now)
    {
        if (!TryGetTag(serverHandle, out SimulatedTag tag, out int clientHandle))
        {
            return new OpcHdaAnnotation(serverHandle, [], [], [], []);
        }

        List<AnnotationPoint> points = AnnotationPoints(tag.ItemId, start, end, now);
        return new OpcHdaAnnotation(clientHandle, [.. points.Select(static point => point.Timestamp)], [.. points.Select(static point => (string?)point.Text)], [.. points.Select(static point => point.AnnotationTime)], [.. points.Select(static point => (string?)point.User)]);
    }

    private HdaAnnotationResult AnnotationResult(string itemId, DateTimeOffset start, DateTimeOffset end, DateTimeOffset now)
    {
        if (!_model.TryGetTag(itemId, out SimulatedTag tag))
        {
            return new HdaAnnotationResult { ItemId = itemId, ResultId = OpcResultId.UnknownItemId };
        }

        return new HdaAnnotationResult
        {
            ItemId = tag.ItemId,
            Annotations = AnnotationPoints(tag.ItemId, start, end, now)
                .Select(static point => new HdaAnnotation { Timestamp = point.Timestamp, AnnotationTime = point.AnnotationTime, AnnotationText = point.Text, User = point.User })
                .ToArray(),
        };
    }

    private List<AnnotationPoint> AnnotationPoints(string itemId, DateTimeOffset start, DateTimeOffset end, DateTimeOffset now)
    {
        if (start > end)
        {
            (start, end) = (end, start);
        }

        var points = new List<AnnotationPoint> { new(Anchor(start, end, now), now, "Calibration note for " + itemId, "historian") };
        lock (_gate)
        {
            if (_annotations.TryGetValue(itemId, out List<AnnotationPoint>? annotations))
            {
                points.AddRange(annotations);
            }
        }

        return points.Where(point => point.Timestamp >= start && point.Timestamp <= end).OrderBy(static point => point.Timestamp).ToList();
    }

    private OpcHdaItem AtTimeItem(int serverHandle, DateTimeOffset[] timestamps)
    {
        if (!TryGetTag(serverHandle, out SimulatedTag tag, out int clientHandle))
        {
            return new OpcHdaItem(serverHandle, 0, [], [], []);
        }

        return new OpcHdaItem(clientHandle, 0, timestamps, [.. timestamps.Select(static _ => (uint)OpcQuality.Good.RawValue)], [.. timestamps.Select(timestamp => Variant(tag, ValueAt(tag, timestamp)))]);
    }

    private HdaReadResult AtTimeResult(string itemId, IReadOnlyList<DateTimeOffset> timestamps)
    {
        if (!_model.TryGetTag(itemId, out SimulatedTag tag))
        {
            return new HdaReadResult { ItemId = itemId, ResultId = OpcResultId.UnknownItemId };
        }

        return new HdaReadResult { ItemId = tag.ItemId, Values = timestamps.Select(timestamp => new HdaItemValue { Timestamp = timestamp, Value = ValueAt(tag, timestamp), Quality = OpcQuality.Good }).ToArray() };
    }

    private OpcHdaAttribute Attribute(int serverHandle, int attributeId, DateTimeOffset timestamp)
    {
        OpcVariant value = TryGetTag(serverHandle, out SimulatedTag tag, out int clientHandle)
            ? attributeId switch
            {
                1 => OpcVariant.FromInt16((short)ToVarType(tag.DataType)),
                2 => OpcVariant.FromString(tag.ItemId),
                3 => OpcVariant.FromString(tag.Units ?? string.Empty),
                _ => OpcVariant.FromString(string.Empty),
            }
            : OpcVariant.Empty;
        return new OpcHdaAttribute(clientHandle, attributeId, [timestamp], [value]);
    }

    private static object Coerce(SimulatedDataType dataType, object? value)
    {
        double d = ToDouble(value);
        return dataType switch
        {
            SimulatedDataType.Boolean => value is bool b ? b : d >= 0.5,
            SimulatedDataType.Int16 => (short)Math.Round(d),
            SimulatedDataType.Int32 => (int)Math.Round(d),
            SimulatedDataType.Single => (float)d,
            SimulatedDataType.Double => d,
            SimulatedDataType.String => value as string ?? d.ToString("0.###", CultureInfo.InvariantCulture),
            _ => value ?? string.Empty,
        };
    }

    private OpcHdaModifiedItem ModifiedItem(int serverHandle, DateTimeOffset start, DateTimeOffset end, int maxValues)
    {
        if (!TryGetTag(serverHandle, out SimulatedTag tag, out int clientHandle))
        {
            return new OpcHdaModifiedItem(serverHandle, [], [], [], [], [], []);
        }

        IReadOnlyList<HistoryPoint> points = ModifiedPoints(tag, start, end, maxValues);
        return new OpcHdaModifiedItem(clientHandle, [.. points.Select(static point => point.Timestamp)], [.. points.Select(static point => point.Quality)], [.. points.Select(point => Variant(tag, point.Value))], [.. points.Select(static point => point.ModificationTime)], [.. points.Select(static point => point.EditType)], [.. points.Select(static point => (string?)point.User)]);
    }

    private List<HistoryPoint> ModifiedPoints(SimulatedTag tag, DateTimeOffset start, DateTimeOffset end, int maxValues)
    {
        if (start > end)
        {
            (start, end) = (end, start);
        }

        List<HistoryPoint> points;
        lock (_gate)
        {
            points = _overlay.TryGetValue(tag.ItemId, out SortedDictionary<DateTimeOffset, HistoryPoint>? overlay)
                ? overlay.Values.Where(point => point.Timestamp >= start && point.Timestamp <= end).ToList()
                : [];
        }

        if (points.Count == 0)
        {
            points.AddRange(_model.History(tag, start, end, Interval(start, end, maxValues)).Take(maxValues).Select(static sample => HistoryPoint.Original(sample.Timestamp, sample.Value)));
        }

        return points.OrderBy(static point => point.Timestamp).Take(maxValues).ToList();
    }

    private OpcHdaItem ProcessedItem(int serverHandle, DateTimeOffset start, DateTimeOffset end, TimeSpan interval, HdaAggregate aggregate)
    {
        if (!TryGetTag(serverHandle, out SimulatedTag tag, out int clientHandle))
        {
            return new OpcHdaItem(serverHandle, (int)aggregate, [], [], []);
        }

        IReadOnlyList<HistoryPoint> points = ProcessedPoints(tag, start, end, interval, aggregate);
        return new OpcHdaItem(clientHandle, (int)aggregate, [.. points.Select(static point => point.Timestamp)], [.. points.Select(static point => point.Quality)], [.. points.Select(point => Variant(tag, point.Value))]);
    }

    private HdaReadResult ProcessedResult(string itemId, DateTimeOffset start, DateTimeOffset end, TimeSpan interval, HdaAggregate aggregate)
    {
        if (!_model.TryGetTag(itemId, out SimulatedTag tag))
        {
            return new HdaReadResult { ItemId = itemId, ResultId = OpcResultId.UnknownItemId };
        }

        return new HdaReadResult { ItemId = tag.ItemId, Values = [.. ProcessedPoints(tag, start, end, interval, aggregate).Select(static point => new HdaItemValue { Timestamp = point.Timestamp, Value = point.Value, Quality = new OpcQuality((ushort)point.Quality) })] };
    }

    private IReadOnlyList<HistoryPoint> ProcessedPoints(SimulatedTag tag, DateTimeOffset start, DateTimeOffset end, TimeSpan interval, HdaAggregate aggregate)
    {
        if (start > end)
        {
            (start, end) = (end, start);
        }

        if (interval <= TimeSpan.Zero)
        {
            interval = TimeSpan.FromMinutes(1);
        }

        var points = new List<HistoryPoint>();
        for (DateTimeOffset t = start; t <= end && points.Count < MaxReturnValues; t = t.Add(interval))
        {
            DateTimeOffset bucketEnd = t.Add(interval) > end ? end : t.Add(interval);
            IReadOnlyList<HistoryPoint> raw = RawPoints(tag, t, bucketEnd, MaxReturnValues, includeBounds: true);
            points.Add(HistoryPoint.Original(t, Aggregate(tag, raw, aggregate, t)));
            if (bucketEnd == end)
            {
                break;
            }
        }

        return points;
    }

    private OpcHdaItem RawItem(int serverHandle, DateTimeOffset start, DateTimeOffset end, int maxValues, bool bounds)
    {
        if (!TryGetTag(serverHandle, out SimulatedTag tag, out int clientHandle))
        {
            return new OpcHdaItem(serverHandle, 0, [], [], []);
        }

        IReadOnlyList<HistoryPoint> points = RawPoints(tag, start, end, maxValues, bounds);
        return new OpcHdaItem(clientHandle, 0, [.. points.Select(static point => point.Timestamp)], [.. points.Select(static point => point.Quality)], [.. points.Select(point => Variant(tag, point.Value))]);
    }

    private HdaReadResult RawResult(string itemId, DateTimeOffset start, DateTimeOffset end, int maxValues, bool bounds)
    {
        if (!_model.TryGetTag(itemId, out SimulatedTag tag))
        {
            return new HdaReadResult { ItemId = itemId, ResultId = OpcResultId.UnknownItemId };
        }

        return new HdaReadResult { ItemId = tag.ItemId, Values = [.. RawPoints(tag, start, end, maxValues, bounds).Select(static point => new HdaItemValue { Timestamp = point.Timestamp, Value = point.Value, Quality = new OpcQuality((ushort)point.Quality) })] };
    }

    private IReadOnlyList<HistoryPoint> RawPoints(SimulatedTag tag, DateTimeOffset start, DateTimeOffset end, int maxValues, bool includeBounds)
    {
        if (start > end)
        {
            (start, end) = (end, start);
        }

        var values = new SortedDictionary<DateTimeOffset, HistoryPoint>();
        List<DeletedRange> ranges;
        lock (_gate)
        {
            ranges = _deleted.TryGetValue(tag.ItemId, out List<DeletedRange>? existing) ? [.. existing] : [];
        }

        foreach ((DateTimeOffset timestamp, object value) in _model.History(tag, start, end, Interval(start, end, maxValues)))
        {
            if ((includeBounds || (timestamp != start && timestamp != end)) && !ranges.Any(range => timestamp >= range.Start && timestamp <= range.End))
            {
                values[timestamp] = HistoryPoint.Original(timestamp, value);
            }
        }

        lock (_gate)
        {
            if (_overlay.TryGetValue(tag.ItemId, out SortedDictionary<DateTimeOffset, HistoryPoint>? overlay))
            {
                foreach (HistoryPoint point in overlay.Values.Where(point => point.Timestamp >= start && point.Timestamp <= end))
                {
                    values[point.Timestamp] = point;
                }
            }
        }

        return values.Values.Take(maxValues).ToArray();
    }

    private int[] Upsert(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, bool replaceOnly, uint editType)
    {
        var results = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (!TryGetTagLocked(serverHandles[i], out SimulatedTag tag, out _))
                {
                    results[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                DateTimeOffset timestamp = DateTimeOffset.FromFileTime(i < timestampFileTimes.Length ? timestampFileTimes[i] : 0);
                SortedDictionary<DateTimeOffset, HistoryPoint> points = OverlayLocked(tag.ItemId);
                if (replaceOnly && !points.ContainsKey(timestamp))
                {
                    points[timestamp] = HistoryPoint.Modified(timestamp, _model.ValueAt(tag, timestamp), (uint)OpcQuality.Good.RawValue, editType, "historian");
                }

                object value = Coerce(tag.DataType, i < dataValues.Length ? dataValues[i].Boxed : null);
                uint quality = i < qualities.Length && qualities[i] != 0 ? unchecked((uint)qualities[i]) : (uint)OpcQuality.Good.RawValue;
                points[timestamp] = HistoryPoint.Modified(timestamp, value, quality, editType, "operator");
                results[i] = OpcResultId.Ok.Code;
            }
        }

        return results;
    }

    private object ValueAt(SimulatedTag tag, DateTimeOffset timestamp)
    {
        lock (_gate)
        {
            if (_overlay.TryGetValue(tag.ItemId, out SortedDictionary<DateTimeOffset, HistoryPoint>? points) && points.TryGetValue(timestamp, out HistoryPoint point))
            {
                return point.Value;
            }
        }

        return _model.ValueAt(tag, timestamp);
    }

    private static OpcVariant Variant(SimulatedTag tag, object? value) =>
        tag.DataType switch
        {
            SimulatedDataType.Boolean => OpcVariant.FromBoolean(value is bool b ? b : ToDouble(value) >= 0.5),
            SimulatedDataType.Int16 => OpcVariant.FromInt16(value is short s ? s : (short)Math.Round(ToDouble(value))),
            SimulatedDataType.Int32 => OpcVariant.FromInt32(value is int i ? i : (int)Math.Round(ToDouble(value))),
            SimulatedDataType.Single => OpcVariant.FromSingle(value is float f ? f : (float)ToDouble(value)),
            SimulatedDataType.Double => OpcVariant.FromDouble(value is double d ? d : ToDouble(value)),
            SimulatedDataType.String => OpcVariant.FromString(value as string ?? string.Empty),
            _ => OpcVariant.FromString(value?.ToString() ?? string.Empty),
        };

    private List<AnnotationPoint> AnnotationsLocked(string itemId)
    {
        if (!_annotations.TryGetValue(itemId, out List<AnnotationPoint>? points))
        {
            points = [];
            _annotations[itemId] = points;
        }

        return points;
    }

    private SortedDictionary<DateTimeOffset, HistoryPoint> OverlayLocked(string itemId)
    {
        if (!_overlay.TryGetValue(itemId, out SortedDictionary<DateTimeOffset, HistoryPoint>? points))
        {
            points = [];
            _overlay[itemId] = points;
        }

        return points;
    }

    private List<DeletedRange> RangesLocked(string itemId)
    {
        if (!_deleted.TryGetValue(itemId, out List<DeletedRange>? ranges))
        {
            ranges = [];
            _deleted[itemId] = ranges;
        }

        return ranges;
    }

    private void RemoveOverlayLocked(string itemId, DateTimeOffset start, DateTimeOffset end)
    {
        if (!_overlay.TryGetValue(itemId, out SortedDictionary<DateTimeOffset, HistoryPoint>? points))
        {
            return;
        }

        foreach (DateTimeOffset timestamp in points.Keys.Where(timestamp => timestamp >= start && timestamp <= end).ToArray())
        {
            _ = points.Remove(timestamp);
        }
    }

    private bool TryGetTag(int serverHandle, out SimulatedTag tag, out int clientHandle)
    {
        lock (_gate)
        {
            return TryGetTagLocked(serverHandle, out tag, out clientHandle);
        }
    }

    private bool TryGetTagLocked(int serverHandle, out SimulatedTag tag, out int clientHandle)
    {
        if (_handles.TryGetValue(serverHandle, out HandleRegistration registration) && _model.TryGetTag(registration.ItemId, out SimulatedTag found))
        {
            tag = found;
            clientHandle = registration.ClientHandle;
            return true;
        }

        tag = null!;
        clientHandle = serverHandle;
        return false;
    }

    private readonly record struct AnnotationPoint(DateTimeOffset Timestamp, DateTimeOffset AnnotationTime, string Text, string User);

    private readonly record struct DeletedRange(DateTimeOffset Start, DateTimeOffset End);

    private readonly record struct HandleRegistration(string ItemId, int ClientHandle);

    private readonly record struct HistoryPoint(DateTimeOffset Timestamp, object Value, uint Quality, DateTimeOffset ModificationTime, uint EditType, string User)
    {
        public static HistoryPoint Modified(DateTimeOffset timestamp, object value, uint quality, uint editType, string user) =>
            new(timestamp, value, quality, DateTimeOffset.UtcNow, editType, user);

        public static HistoryPoint Original(DateTimeOffset timestamp, object value) =>
            new(timestamp, value, (uint)OpcQuality.Good.RawValue, timestamp, 0, "historian");
    }
}
