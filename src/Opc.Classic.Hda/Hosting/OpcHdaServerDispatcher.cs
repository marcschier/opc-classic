//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.CompilerServices;
using Opc.Classic.Dcom;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hosting;

namespace Opc.Classic.Hda.Hosting;

/// <summary>
/// HDA dispatcher adapter that delegates to the source-generated IOPCHDA_Server dispatcher.
/// </summary>
public sealed class OpcHdaServerDispatcher : IOpcHdaServerDispatcher, IOpcCommonServer
{
    private const int OpchdaEqual = 1;
    private const int OpchdaNotEqual = 6;

    private readonly IOpcHdaServer _server;
    private readonly IOPCHDA_ServerServerDispatcher _serverDispatcher;
    private readonly OpcCommonServerDispatcher _commonDispatcher;
    private int _localeId;
    private string _clientName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcHdaServerDispatcher" /> class.
    /// </summary>
    public OpcHdaServerDispatcher(IOpcHdaServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _serverDispatcher = new IOPCHDA_ServerServerDispatcher(_server);
        _commonDispatcher = new OpcCommonServerDispatcher(this);
        _localeId = server.LocaleId;
    }

    internal IOpcServerDispatcher ServerDispatcher => _serverDispatcher;

    internal IOpcServerDispatcher CommonDispatcher => _commonDispatcher;

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId == IOPCHDA_Server.InterfaceId)
        {
            return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        if (interfaceId == OpcCommonClientProxy.InterfaceId)
        {
            return (await _commonDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
    }

    /// <inheritdoc />
    public async Task SetLocaleIdAsync(int localeId, CancellationToken cancellationToken = default)
    {
        await _server.SetLocaleAsync(localeId, cancellationToken).ConfigureAwait(false);
        _localeId = localeId;
    }

    /// <inheritdoc />
    public Task<int> GetLocaleIdAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_localeId);
    }

    /// <inheritdoc />
    public async Task<int[]> QueryAvailableLocaleIdsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<int> localeIds = await _server.GetSupportedLocalesAsync(cancellationToken).ConfigureAwait(false);
        return localeIds switch
        {
            int[] array => array,
            _ => localeIds.ToArray(),
        };
    }

    /// <inheritdoc />
    public Task<string> GetErrorStringAsync(int errorCode, CancellationToken cancellationToken = default) =>
        _server.GetErrorTextAsync(new OpcResultId(errorCode, null), cancellationToken);

    /// <inheritdoc />
    public async Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(clientName);
        await _server.SetClientNameAsync(clientName, cancellationToken).ConfigureAwait(false);
        _clientName = clientName;
    }

    /// <inheritdoc />
    public async Task<int[]> ValidateBrowseFiltersAsync(
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();

        HashSet<int>? supportedAttributeIds = await TryGetSupportedAttributeIdsAsync(cancellationToken).ConfigureAwait(false);
        var errors = new int[filters.Count];
        for (int i = 0; i < filters.Count; i++)
        {
            errors[i] = ValidateBrowseFilter(filters[i], supportedAttributeIds);
        }

        return errors;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> BrowseAsync(
        string branchPosition,
        HdaBrowseType browseType,
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new List<string>();
        await foreach (HdaBrowseElement element in _server.BrowseAsync(branchPosition, browseType, cancellationToken).ConfigureAwait(false))
        {
            if (ShouldInclude(element, browseType))
            {
                values.Add(ToBrowseString(element, browseType));
            }
        }

        return values;
    }

    /// <inheritdoc />
    public Task<string> ChangeBrowsePositionAsync(
        string currentBranchPosition,
        int browseDirection,
        string? browseString,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(browseDirection switch
        {
            1 => MoveUp(currentBranchPosition),
            2 when !string.IsNullOrEmpty(browseString) => string.IsNullOrEmpty(currentBranchPosition)
                ? browseString
                : currentBranchPosition + "." + browseString,
            3 => browseString ?? string.Empty,
            _ => throw new OpcException(OpcResultId.InvalidArg),
        });
    }

    /// <inheritdoc />
    public Task<string> GetItemIdAsync(
        string branchPosition,
        string node,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(branchPosition) || string.IsNullOrEmpty(node))
        {
            return Task.FromResult(node);
        }

        return Task.FromResult(node.StartsWith(branchPosition + ".", StringComparison.Ordinal)
            ? node
            : branchPosition + "." + node);
    }

    /// <inheritdoc />
    public Task<string> GetBranchPositionAsync(
        string branchPosition,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(branchPosition);
    }

    /// <inheritdoc />
    public async Task<int> UpdateCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_server is IOPCHDA_SyncUpdate syncUpdate)
        {
            return await syncUpdate.QueryCapabilitiesAsync(cancellationToken).ConfigureAwait(false);
        }
        if (_server is IOPCHDA_AsyncUpdate asyncUpdate)
        {
            return await asyncUpdate.QueryCapabilitiesAsync(cancellationToken).ConfigureAwait(false);
        }

        throw new OpcException(OpcResultId.NotImplemented);
    }

    /// <inheritdoc />
    public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ValidateUpdateArrays(serverHandles, timestampFileTimes, dataValues, qualities);
        cancellationToken.ThrowIfCancellationRequested();
        return GetSyncUpdate().InsertAsync(serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int[]> ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ValidateUpdateArrays(serverHandles, timestampFileTimes, dataValues, qualities);
        cancellationToken.ThrowIfCancellationRequested();
        return GetSyncUpdate().ReplaceAsync(serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int[]> InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ValidateUpdateArrays(serverHandles, timestampFileTimes, dataValues, qualities);
        cancellationToken.ThrowIfCancellationRequested();
        return GetSyncUpdate().InsertReplaceAsync(serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return GetSyncUpdate().DeleteRawAsync(startTime, endTime, serverHandles, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int[]> DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ValidateLength(serverHandles.Length, timestampFileTimes.Length, nameof(timestampFileTimes));
        cancellationToken.ThrowIfCancellationRequested();
        return GetSyncUpdate().DeleteAtTimeAsync(serverHandles, timestampFileTimes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<OpcHdaAsyncUpdateResult> BeginAsyncInsertAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ValidateUpdateArrays(serverHandles, timestampFileTimes, dataValues, qualities);
        cancellationToken.ThrowIfCancellationRequested();
        if (_server is IOPCHDA_AsyncUpdate asyncUpdate)
        {
            int cancelId = await asyncUpdate.InsertAsync(transactionId, serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken).ConfigureAwait(false);
            return SucceededAsyncUpdate(cancelId, serverHandles);
        }

        int[] errors = await InsertAsync(serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken).ConfigureAwait(false);
        return new OpcHdaAsyncUpdateResult(0, CopyArray(serverHandles), errors);
    }

    /// <inheritdoc />
    public async Task<OpcHdaAsyncUpdateResult> BeginAsyncReplaceAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ValidateUpdateArrays(serverHandles, timestampFileTimes, dataValues, qualities);
        cancellationToken.ThrowIfCancellationRequested();
        if (_server is IOPCHDA_AsyncUpdate asyncUpdate)
        {
            int cancelId = await asyncUpdate.ReplaceAsync(transactionId, serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken).ConfigureAwait(false);
            return SucceededAsyncUpdate(cancelId, serverHandles);
        }

        int[] errors = await ReplaceAsync(serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken).ConfigureAwait(false);
        return new OpcHdaAsyncUpdateResult(0, CopyArray(serverHandles), errors);
    }

    /// <inheritdoc />
    public async Task<OpcHdaAsyncUpdateResult> BeginAsyncInsertReplaceAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ValidateUpdateArrays(serverHandles, timestampFileTimes, dataValues, qualities);
        cancellationToken.ThrowIfCancellationRequested();
        if (_server is IOPCHDA_AsyncUpdate asyncUpdate)
        {
            int cancelId = await asyncUpdate.InsertReplaceAsync(transactionId, serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken).ConfigureAwait(false);
            return SucceededAsyncUpdate(cancelId, serverHandles);
        }

        int[] errors = await InsertReplaceAsync(serverHandles, timestampFileTimes, dataValues, qualities, cancellationToken).ConfigureAwait(false);
        return new OpcHdaAsyncUpdateResult(0, CopyArray(serverHandles), errors);
    }

    /// <inheritdoc />
    public async Task<OpcHdaAsyncUpdateResult> BeginAsyncDeleteRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        if (_server is IOPCHDA_AsyncUpdate asyncUpdate)
        {
            int cancelId = await asyncUpdate.DeleteRawAsync(transactionId, startTime, endTime, serverHandles, cancellationToken).ConfigureAwait(false);
            return SucceededAsyncUpdate(cancelId, serverHandles);
        }

        int[] errors = await DeleteRawAsync(startTime, endTime, serverHandles, cancellationToken).ConfigureAwait(false);
        return new OpcHdaAsyncUpdateResult(0, CopyArray(serverHandles), errors);
    }

    /// <inheritdoc />
    public async Task<OpcHdaAsyncUpdateResult> BeginAsyncDeleteAtTimeAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ValidateLength(serverHandles.Length, timestampFileTimes.Length, nameof(timestampFileTimes));
        cancellationToken.ThrowIfCancellationRequested();
        if (_server is IOPCHDA_AsyncUpdate asyncUpdate)
        {
            int cancelId = await asyncUpdate.DeleteAtTimeAsync(transactionId, serverHandles, timestampFileTimes, cancellationToken).ConfigureAwait(false);
            return SucceededAsyncUpdate(cancelId, serverHandles);
        }

        int[] errors = await DeleteAtTimeAsync(serverHandles, timestampFileTimes, cancellationToken).ConfigureAwait(false);
        return new OpcHdaAsyncUpdateResult(0, CopyArray(serverHandles), errors);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<OpcHdaPlaybackEvent> BeginPlaybackRawAsync(
        int transactionId,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        long updateDurationFileTime,
        long updateIntervalFileTime,
        int[] serverHandles,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        if (_server is IOPCHDA_Playback playback && _server is not IOPCHDA_SyncRead)
        {
            await playback.ReadRawWithUpdateAsync(transactionId, startTime, endTime, maxValues, updateDurationFileTime, updateIntervalFileTime, serverHandles, cancellationToken).ConfigureAwait(false);
            yield break;
        }
        if (_server is not IOPCHDA_SyncRead syncRead)
        {
            throw new OpcException(OpcResultId.NotImplemented);
        }

        int iterations = GetPlaybackIterationCount(updateDurationFileTime, updateIntervalFileTime);
        TimeSpan delay = ToPlaybackDelay(updateIntervalFileTime);
        for (int i = 0; i < iterations; i++)
        {
            OpcHdaItem[] items = await syncRead.ReadRawAsync(startTime, endTime, maxValues, false, serverHandles, cancellationToken).ConfigureAwait(false);
            yield return BuildPlaybackEvent(items, serverHandles.Length);
            if (i + 1 < iterations)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<OpcHdaPlaybackEvent> BeginPlaybackProcessedAsync(
        int transactionId,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        long resampleIntervalFileTime,
        int intervalCount,
        long updateIntervalFileTime,
        int[] serverHandles,
        int[] aggregateIds,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(aggregateIds);
        ValidateLength(serverHandles.Length, aggregateIds.Length, nameof(aggregateIds));
        cancellationToken.ThrowIfCancellationRequested();

        if (_server is IOPCHDA_Playback playback && _server is not IOPCHDA_SyncRead)
        {
            await playback.ReadProcessedWithUpdateAsync(transactionId, startTime, endTime, resampleIntervalFileTime, intervalCount, updateIntervalFileTime, serverHandles, aggregateIds, cancellationToken).ConfigureAwait(false);
            yield break;
        }
        if (_server is not IOPCHDA_SyncRead syncRead)
        {
            throw new OpcException(OpcResultId.NotImplemented);
        }

        int iterations = Math.Clamp(intervalCount, 1, 64);
        TimeSpan delay = ToPlaybackDelay(updateIntervalFileTime);
        for (int i = 0; i < iterations; i++)
        {
            OpcHdaItem[] items = await syncRead.ReadProcessedAsync(startTime, endTime, resampleIntervalFileTime, serverHandles, aggregateIds, cancellationToken).ConfigureAwait(false);
            yield return BuildPlaybackEvent(items, serverHandles.Length);
            if (i + 1 < iterations)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public async Task CancelAsync(int cancelId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (_server is IOPCHDA_AsyncUpdate asyncUpdate)
        {
            await asyncUpdate.CancelAsync(cancelId, cancellationToken).ConfigureAwait(false);
            return;
        }
        if (_server is IOPCHDA_Playback playback)
        {
            await playback.CancelAsync(cancelId, cancellationToken).ConfigureAwait(false);
            return;
        }

        throw new OpcException(OpcResultId.NotImplemented);
    }

    /// <inheritdoc />
    public Task<int[]> InsertAnnotationsAsync(
        int[] serverHandles,
        long[] timestampFileTimes,
        OpcHdaAnnotation[] annotationValues,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(annotationValues);
        ValidateLength(serverHandles.Length, timestampFileTimes.Length, nameof(timestampFileTimes));
        ValidateLength(serverHandles.Length, annotationValues.Length, nameof(annotationValues));
        cancellationToken.ThrowIfCancellationRequested();

        if (_server is not IOPCHDA_SyncAnnotations annotations)
        {
            throw new OpcException(OpcResultId.NotImplemented);
        }

        return annotations.InsertAsync(serverHandles, timestampFileTimes, annotationValues, cancellationToken);
    }

    /// <inheritdoc />
    public Task<OpcHdaAdviseSubscription> AdviseRawAsync(
        int[] serverHandles,
        OpcHdaTime startTime,
        long updateIntervalFileTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(startTime);
        cancellationToken.ThrowIfCancellationRequested();

        if (_server is not IOPCHDA_SyncRead syncRead)
        {
            throw new OpcException(OpcResultId.NotImplemented);
        }

        TimeSpan updateInterval = ToPositiveTimeSpan(updateIntervalFileTime, nameof(updateIntervalFileTime));
        int[] handles = CopyArray(serverHandles);
        int[] errors = new int[handles.Length];
        return Task.FromResult(new OpcHdaAdviseSubscription(
            errors,
            AdviseRawUpdatesAsync(syncRead, handles, startTime, updateInterval, cancellationToken)));
    }

    /// <inheritdoc />
    public Task<OpcHdaAdviseSubscription> AdviseProcessedAsync(
        int[] serverHandles,
        OpcHdaTime startTime,
        long resampleIntervalFileTime,
        int[] aggregateHandles,
        int intervalCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(aggregateHandles);
        ValidateLength(serverHandles.Length, aggregateHandles.Length, nameof(aggregateHandles));
        cancellationToken.ThrowIfCancellationRequested();

        if (_server is not IOPCHDA_SyncRead syncRead)
        {
            throw new OpcException(OpcResultId.NotImplemented);
        }
        if (intervalCount <= 0)
        {
            throw new OpcException(OpcResultId.InvalidArg);
        }

        TimeSpan resampleInterval = ToPositiveTimeSpan(resampleIntervalFileTime, nameof(resampleIntervalFileTime));
        int[] handles = CopyArray(serverHandles);
        int[] aggregates = CopyArray(aggregateHandles);
        int[] errors = new int[handles.Length];
        return Task.FromResult(new OpcHdaAdviseSubscription(
            errors,
            AdviseProcessedUpdatesAsync(syncRead, handles, startTime, resampleInterval, resampleIntervalFileTime, aggregates, intervalCount, cancellationToken)));
    }

    private async Task<HashSet<int>?> TryGetSupportedAttributeIdsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ((IOPCHDA_Server)_server).GetItemAttributesAsync(
                out int[] attributeIds,
                out string[] attributeNames,
                out string[] attributeDescriptions,
                out int[] attributeDataTypes,
                cancellationToken).ConfigureAwait(false);
            _ = attributeNames;
            _ = attributeDescriptions;
            _ = attributeDataTypes;
            return attributeIds.Length == 0 ? null : new HashSet<int>(attributeIds);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return null;
        }
    }

    private static async IAsyncEnumerable<OpcHdaDataUpdate> AdviseRawUpdatesAsync(
        IOPCHDA_SyncRead syncRead,
        int[] serverHandles,
        OpcHdaTime startTime,
        TimeSpan updateInterval,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DateTimeOffset current = ResolveStartTime(startTime);
        while (true)
        {
            await Task.Delay(updateInterval, cancellationToken).ConfigureAwait(false);
            DateTimeOffset next = current + updateInterval;
            OpcHdaItem[] items = await syncRead.ReadRawAsync(
                OpcHdaTime.FromTimestamp(current),
                OpcHdaTime.FromTimestamp(next),
                0,
                false,
                serverHandles,
                cancellationToken).ConfigureAwait(false);
            yield return BuildUpdate(items, serverHandles.Length);
            current = next;
        }
    }

    private static async IAsyncEnumerable<OpcHdaDataUpdate> AdviseProcessedUpdatesAsync(
        IOPCHDA_SyncRead syncRead,
        int[] serverHandles,
        OpcHdaTime startTime,
        TimeSpan resampleInterval,
        long resampleIntervalFileTime,
        int[] aggregateHandles,
        int intervalCount,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        DateTimeOffset current = ResolveStartTime(startTime);
        while (true)
        {
            TimeSpan updateInterval = TimeSpan.FromTicks(checked(resampleInterval.Ticks * intervalCount));
            await Task.Delay(updateInterval, cancellationToken).ConfigureAwait(false);
            DateTimeOffset next = current + updateInterval;
            OpcHdaItem[] items = await syncRead.ReadProcessedAsync(
                OpcHdaTime.FromTimestamp(current),
                OpcHdaTime.FromTimestamp(next),
                resampleIntervalFileTime,
                serverHandles,
                aggregateHandles,
                cancellationToken).ConfigureAwait(false);
            yield return BuildUpdate(items, serverHandles.Length);
            current = next;
        }
    }

    private IOPCHDA_SyncUpdate GetSyncUpdate() =>
        _server as IOPCHDA_SyncUpdate ?? throw new OpcException(OpcResultId.NotImplemented);

    private static void ValidateUpdateArrays(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(dataValues);
        ArgumentNullException.ThrowIfNull(qualities);
        ValidateLength(serverHandles.Length, timestampFileTimes.Length, nameof(timestampFileTimes));
        ValidateLength(serverHandles.Length, dataValues.Length, nameof(dataValues));
        ValidateLength(serverHandles.Length, qualities.Length, nameof(qualities));
    }

    private static OpcHdaAsyncUpdateResult SucceededAsyncUpdate(int cancelId, int[] serverHandles) =>
        new(cancelId, CopyArray(serverHandles), new int[serverHandles.Length]);

    private static OpcHdaDataUpdate BuildUpdate(OpcHdaItem[] items, int expectedCount) =>
        new(NormalizeItems(items, expectedCount), BuildResultErrors(items.Length, expectedCount, OpcResultId.InvalidHandle.Code));

    private static OpcHdaPlaybackEvent BuildPlaybackEvent(OpcHdaItem[] items, int expectedCount)
    {
        int[] errors = BuildResultErrors(items.Length, expectedCount, OpcResultId.InvalidHandle.Code);
        return new OpcHdaPlaybackEvent(GetMasterHResult(errors), NormalizeItems(items, expectedCount), errors);
    }

    private static int[] BuildResultErrors(int resultCount, int expectedCount, int missingError)
    {
        var errors = new int[expectedCount];
        for (int i = 0; i < expectedCount; i++)
        {
            errors[i] = i < resultCount ? OpcResultId.Ok.Code : missingError;
        }

        return errors;
    }

    private static OpcHdaItem[] NormalizeItems(OpcHdaItem[] items, int count)
    {
        var normalized = new OpcHdaItem[count];
        for (int i = 0; i < count; i++)
        {
            normalized[i] = i < items.Length ? items[i] : new OpcHdaItem(0, 0, [], [], []);
        }

        return normalized;
    }

    private static TimeSpan ToPositiveTimeSpan(long fileTimeTicks, string parameterName)
    {
        if (fileTimeTicks <= 0)
        {
            throw new OpcException(OpcResultId.InvalidArg);
        }

        try
        {
            return TimeSpan.FromTicks(fileTimeTicks);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ArgumentOutOfRangeException(parameterName, fileTimeTicks, ex.Message);
        }
    }

    private static int GetPlaybackIterationCount(long updateDurationFileTime, long updateIntervalFileTime)
    {
        if (updateDurationFileTime <= 0 || updateIntervalFileTime <= 0)
        {
            return 1;
        }

        long count = Math.Max(1, updateDurationFileTime / updateIntervalFileTime);
        return (int)Math.Clamp(count, 1, 64);
    }

    private static TimeSpan ToPlaybackDelay(long updateIntervalFileTime)
    {
        if (updateIntervalFileTime <= 0)
        {
            return TimeSpan.FromMilliseconds(1);
        }

        long ticks = Math.Clamp(updateIntervalFileTime, TimeSpan.FromMilliseconds(1).Ticks, TimeSpan.FromMilliseconds(100).Ticks);
        return TimeSpan.FromTicks(ticks);
    }

    private static int GetMasterHResult(int[] errors)
    {
        for (int i = 0; i < errors.Length; i++)
        {
            if (errors[i] < 0)
            {
                return 1;
            }
        }

        return OpcResultId.Ok.Code;
    }

    private static void ValidateLength(int expected, int actual, string parameterName)
    {
        if (actual != expected)
        {
            throw new ArgumentException($"Array length {actual} does not match expected {expected}.", parameterName);
        }
    }

    private static DateTimeOffset ResolveStartTime(OpcHdaTime startTime) =>
        startTime.IsStringExpression ? DateTimeOffset.UtcNow : startTime.Timestamp;

    private static int[] CopyArray(int[] values)
    {
        var copy = new int[values.Length];
        Array.Copy(values, copy, values.Length);
        return copy;
    }

    private static int ValidateBrowseFilter(OpcHdaBrowseFilter filter, HashSet<int>? supportedAttributeIds)
    {
        if (filter.AttributeId <= 0)
        {
            return OpcHdaErrors.OPCHDA_E_INVALIDATTRID;
        }
        if (filter.OperatorCode is < OpchdaEqual or > OpchdaNotEqual)
        {
            return OpcResultId.InvalidArg.Code;
        }
        if (supportedAttributeIds is not null && !supportedAttributeIds.Contains(filter.AttributeId))
        {
            return OpcHdaErrors.OPCHDA_E_UNKNOWNATTRID;
        }

        return OpcResultId.Ok.Code;
    }

    private static bool ShouldInclude(HdaBrowseElement element, HdaBrowseType browseType) => browseType switch
    {
        HdaBrowseType.Branch => element.BrowseType == HdaBrowseType.Branch,
        HdaBrowseType.Leaf => element.BrowseType == HdaBrowseType.Leaf,
        HdaBrowseType.Flat => element.BrowseType == HdaBrowseType.Flat,
        HdaBrowseType.Items => element.BrowseType is HdaBrowseType.Leaf or HdaBrowseType.Flat,
        _ => false,
    };

    private static string ToBrowseString(HdaBrowseElement element, HdaBrowseType browseType) =>
        browseType is HdaBrowseType.Flat or HdaBrowseType.Items
            ? string.IsNullOrEmpty(element.ItemId) ? element.Name : element.ItemId
            : element.Name;

    private static string MoveUp(string position)
    {
        if (string.IsNullOrEmpty(position))
        {
            return string.Empty;
        }

        int lastDot = position.LastIndexOf('.');
        return lastDot < 0 ? string.Empty : position[..lastDot];
    }
}
