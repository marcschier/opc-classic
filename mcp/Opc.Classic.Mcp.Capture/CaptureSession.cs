// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Opc.Classic.Mcp.Capture;

public sealed class CaptureSession : IAsyncDisposable
{
    private readonly AsyncOperationGate _operations = new(nameof(CaptureSession));
    private readonly object _keyLock = new();
    private readonly object _cursorInitLock = new();
    private readonly object _snapshotLock = new();
    private readonly ILogger _logger;
    private readonly Func<string, ICaptureSource>? _sourceFactory;
    private SourceState _sourceState;
    private byte[]? _ntlmSessionKey;
    private DecodeCursor? _cursor;
    private bool _secretStateCleared;
    private bool _cursorOperationsClosed;
    private int _nextSourceSegment = 1;
    private int _disposed;
    private int _state = (int)CaptureSessionState.Starting;
    private CancellationTokenSource? _sourceCompletionCancellation;
    private Task? _sourceCompletionObserver;

    internal Action? SecretCleanupObserved { get; set; }

    public CaptureSession(
        string id,
        string sourceName,
        ICaptureSource source,
        string sessionFolder,
        CaptureStartRequest request,
        ILogger? logger = null,
        Func<string, ICaptureSource>? sourceFactory = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(sourceName);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(sessionFolder);
        ArgumentNullException.ThrowIfNull(request);

        Id = id;
        SourceName = sourceName;
        _sourceState = new SourceState(source, [source]);
        _sourceFactory = sourceFactory;
        SessionFolder = sessionFolder;
        _ntlmSessionKey = request.NtlmSessionKey?.ToArray();
        Request = request with { NtlmSessionKey = null };
        _logger = logger ?? NullLogger.Instance;
    }

    public string Id { get; }
    public string SourceName { get; }
    public ICaptureSource Source => Volatile.Read(ref _sourceState).Active;
    public long PacketCount => SumSources(static source => source.PacketCount);
    public long ByteCount => SumSources(static source => source.ByteCount);
    public int LinkType => Source.LinkType;
    public string? EffectiveFilter =>
        (Source as ICaptureFilterController)?.EffectiveFilter ?? Request.BpfFilter;
    public IReadOnlyList<string> RawPcapFilePaths
    {
        get
        {
            lock (_snapshotLock)
            {
                return GetRawPcapFilePaths(_sourceState);
            }
        }
    }
    public string SessionFolder { get; }
    public CaptureStartRequest Request { get; private set; }
    public CaptureTargetMetadata? Target { get; private set; }
    public CaptureFilterTransitionResult? LastFilterTransition { get; private set; }
    public CaptureSessionState State
    {
        get => (CaptureSessionState)Volatile.Read(ref _state);
        private set => Volatile.Write(ref _state, (int)value);
    }
    public DateTimeOffset? StartedAt { get; private set; }
    public DateTimeOffset? StoppedAt { get; private set; }
    public DateTimeOffset LastTouchedAt { get; private set; } = DateTimeOffset.UtcNow;
    public string? Error { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using AsyncOperationGate.Lease operation =
            await _operations.EnterAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await Source.StartAsync(Request, cancellationToken).ConfigureAwait(false);
            StartedAt = DateTimeOffset.UtcNow;
            State = CaptureSessionState.Running;
            LastTouchedAt = DateTimeOffset.UtcNow;
            ArmSourceCompletionObserver(Source);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Capture session {SessionId} started ({Source}).", Id, SourceName);
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            State = CaptureSessionState.Failed;
            await ClearSecretStateAsync().ConfigureAwait(false);
            _logger.LogError(ex, "Capture session {SessionId} failed to start.", Id);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        using AsyncOperationGate.Lease operation =
            await _operations.EnterAsync(cancellationToken).ConfigureAwait(false);
        Exception? failure = await StopCoreAsync(
            sourceFailure: null,
            cancellationToken).ConfigureAwait(false);
        if (failure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

    private async Task<Exception?> StopCoreAsync(
        Exception? sourceFailure,
        CancellationToken cancellationToken)
    {
        if (State is CaptureSessionState.Completed
                or CaptureSessionState.Failed
                or CaptureSessionState.Disposed)
        {
            return null;
        }

        CancelSourceCompletionObserver();
        State = CaptureSessionState.Stopping;
        Exception? failure = sourceFailure;
        try
        {
            await StopAllSourcesAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
        {
            failure ??= ex;
        }

        StoppedAt = DateTimeOffset.UtcNow;
        LastTouchedAt = DateTimeOffset.UtcNow;
        if (failure is null)
        {
            State = CaptureSessionState.Completed;
            await ClearSecretStateAsync().ConfigureAwait(false);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Capture session {SessionId} completed ({Packets} packets, {Bytes} bytes).",
                    Id, PacketCount, ByteCount);
            }
            return null;
        }

        Error = failure.Message;
        State = CaptureSessionState.Failed;
        await ClearSecretStateAsync().ConfigureAwait(false);
        _logger.LogError(failure, "Capture session {SessionId} failed to stop.", Id);
        return failure;
    }

    public void Touch() => LastTouchedAt = DateTimeOffset.UtcNow;

    internal void SetTarget(CaptureTargetMetadata target)
    {
        ArgumentNullException.ThrowIfNull(target);
        lock (_snapshotLock)
        {
            Target = target;
            LastTouchedAt = DateTimeOffset.UtcNow;
        }
    }

    internal void UpdateRequest(CaptureStartRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        lock (_snapshotLock)
        {
            Request = request with { NtlmSessionKey = null };
        }
    }

    internal CaptureSessionSnapshot GetSnapshot()
    {
        lock (_snapshotLock)
        {
            SourceState sources = _sourceState;
            IReadOnlyList<string> rawPaths = GetRawPcapFilePaths(sources);
            return new CaptureSessionSnapshot(
                Id,
                SourceName,
                State,
                StartedAt,
                StoppedAt,
                SumSources(sources, static source => source.PacketCount),
                SumSources(sources, static source => source.ByteCount),
                Request,
                Target,
                (sources.Active as ICaptureFilterController)?.EffectiveFilter ?? Request.BpfFilter,
                rawPaths,
                Error,
                LastFilterTransition);
        }
    }

    /// <summary>
    /// Replaces the filter on a running session. A source-provided live update is
    /// preferred; sources without that capability are restarted into a retained
    /// segment and atomically swapped into the session.
    /// </summary>
    public async Task<CaptureFilterTransitionResult> ReplaceFilterAsync(
        string filter,
        CaptureStartRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);
        ArgumentNullException.ThrowIfNull(request);

        DateTimeOffset startedAt = DateTimeOffset.UtcNow;
        using AsyncOperationGate.Lease operation =
            await _operations.EnterAsync(cancellationToken).ConfigureAwait(false);
        string? previousFilter = EffectiveFilter;
        if (State != CaptureSessionState.Running || _disposed != 0)
        {
            return SetFilterTransition(CreateFilterTransition(
                filter,
                previousFilter,
                CaptureFilterTransitionStatus.Failed,
                startedAt,
                previousFilter,
                $"Capture session '{Id}' is not running."));
        }

        CaptureStartRequest sanitizedRequest = request with
        {
            BpfFilter = filter,
            NtlmSessionKey = null,
        };
        if (string.Equals(previousFilter, filter, StringComparison.Ordinal))
        {
            lock (_snapshotLock)
            {
                Request = sanitizedRequest;
                LastTouchedAt = DateTimeOffset.UtcNow;
                LastFilterTransition = CreateFilterTransition(
                    filter,
                    previousFilter,
                    CaptureFilterTransitionStatus.Unchanged,
                    startedAt,
                    previousFilter,
                    error: null);
                return LastFilterTransition;
            }
        }

        ICaptureSource current = Source;
        if (current is ICaptureFilterController controller)
        {
            lock (_snapshotLock)
            {
                try
                {
                    CaptureSourceFilterUpdateResult update =
                        controller.TryUpdateFilter(filter, cancellationToken);
                    if (update.Status == CaptureSourceFilterUpdateStatus.Updated)
                    {
                        Request = sanitizedRequest;
                        LastTouchedAt = DateTimeOffset.UtcNow;
                        LastFilterTransition = CreateFilterTransition(
                            filter,
                            previousFilter,
                            CaptureFilterTransitionStatus.LiveUpdated,
                            startedAt,
                            controller.EffectiveFilter ?? filter,
                            error: null);
                        return LastFilterTransition;
                    }
                }
                catch (OperationCanceledException)
                {
                    LastTouchedAt = DateTimeOffset.UtcNow;
                    LastFilterTransition = CreateFilterTransition(
                        filter,
                        previousFilter,
                        CaptureFilterTransitionStatus.Canceled,
                        startedAt,
                        previousFilter,
                        "The filter transition was canceled before it became visible.");
                    throw;
                }
                catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
                {
                    string error = ex.Message;
                    if (!string.IsNullOrWhiteSpace(previousFilter)
                        && !string.Equals(
                            controller.EffectiveFilter,
                            previousFilter,
                            StringComparison.Ordinal))
                    {
                        try
                        {
                            CaptureSourceFilterUpdateResult rollback =
                                controller.TryUpdateFilter(previousFilter, CancellationToken.None);
                            if (rollback.Status != CaptureSourceFilterUpdateStatus.Updated)
                            {
                                error += " Rollback requires a source restart; the prior filter could not be confirmed.";
                            }
                        }
                        catch (Exception rollbackException)
                            when (rollbackException is not OutOfMemoryException and not ThreadAbortException)
                        {
                            error += $" Rollback failed: {rollbackException.Message}";
                        }
                    }

                    LastTouchedAt = DateTimeOffset.UtcNow;
                    LastFilterTransition = CreateFilterTransition(
                        filter,
                        previousFilter,
                        CaptureFilterTransitionStatus.Failed,
                        startedAt,
                        controller.EffectiveFilter ?? previousFilter,
                        error);
                    return LastFilterTransition;
                }
            }
        }

        if (_sourceFactory is null)
        {
            return SetFilterTransition(CreateFilterTransition(
                filter,
                previousFilter,
                CaptureFilterTransitionStatus.Failed,
                startedAt,
                previousFilter,
                "The capture source does not support live filter updates and no restart factory is available."));
        }

        return await RestartSourceForFilterAsync(
            current,
            filter,
            previousFilter,
            sanitizedRequest,
            startedAt,
            cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replays all retained source segments in transition order.
    /// </summary>
    public async IAsyncEnumerable<CapturedPacket> ReadAllAsync(
        long? maxPackets,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        long remaining = maxPackets ?? long.MaxValue;
        if (remaining <= 0)
        {
            yield break;
        }

        SourceState state = Volatile.Read(ref _sourceState);
        foreach (ICaptureSource source in state.Segments)
        {
            await foreach (CapturedPacket packet in source.ReadAllAsync(remaining, cancellationToken)
                .ConfigureAwait(false))
            {
                yield return packet;
                remaining--;
                if (remaining == 0)
                {
                    yield break;
                }
            }
        }
    }

    private async Task<CaptureFilterTransitionResult> RestartSourceForFilterAsync(
        ICaptureSource current,
        string filter,
        string? previousFilter,
        CaptureStartRequest request,
        DateTimeOffset startedAt,
        CancellationToken cancellationToken)
    {
        string segmentFolder = Path.Combine(
            SessionFolder,
            "segments",
            $"segment-{_nextSourceSegment++:D4}");
        Directory.CreateDirectory(segmentFolder);

        ICaptureSource? replacement = null;
        try
        {
            replacement = _sourceFactory!(segmentFolder);
            await replacement.StartAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            await DisposeReplacementAsync(replacement, segmentFolder).ConfigureAwait(false);
            SetFilterTransition(CreateFilterTransition(
                filter,
                previousFilter,
                CaptureFilterTransitionStatus.Canceled,
                startedAt,
                previousFilter,
                "The replacement source was canceled before the transition became visible."));
            throw;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
        {
            await DisposeReplacementAsync(replacement, segmentFolder).ConfigureAwait(false);
            return SetFilterTransition(CreateFilterTransition(
                filter,
                previousFilter,
                CaptureFilterTransitionStatus.Failed,
                startedAt,
                previousFilter,
                ex.Message));
        }

        string? cleanupWarning = null;
        try
        {
            await current.StopAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
        {
            cleanupWarning = $"Replacement source is active, but the prior source did not stop cleanly: {ex.Message}";
        }

        lock (_snapshotLock)
        {
            SourceState prior = _sourceState;
            var segments = new ICaptureSource[prior.Segments.Length + 1];
            prior.Segments.CopyTo(segments, 0);
            segments[^1] = replacement;
            Volatile.Write(ref _sourceState, new SourceState(replacement, segments));
            Request = request;
            LastTouchedAt = DateTimeOffset.UtcNow;
            ArmSourceCompletionObserver(replacement);
            LastFilterTransition = CreateFilterTransition(
                filter,
                previousFilter,
                cleanupWarning is null
                    ? CaptureFilterTransitionStatus.Restarted
                    : CaptureFilterTransitionStatus.RestartedWithCleanupWarning,
                startedAt,
                (replacement as ICaptureFilterController)?.EffectiveFilter ?? filter,
                cleanupWarning);
            return LastFilterTransition;
        }
    }

    private CaptureFilterTransitionResult SetFilterTransition(CaptureFilterTransitionResult result)
    {
        lock (_snapshotLock)
        {
            LastFilterTransition = result;
            LastTouchedAt = DateTimeOffset.UtcNow;
            return result;
        }
    }

    private CaptureFilterTransitionResult CreateFilterTransition(
        string filter,
        string? previousFilter,
        CaptureFilterTransitionStatus status,
        DateTimeOffset startedAt,
        string? effectiveFilter,
        string? error)
    {
        SourceState state = Volatile.Read(ref _sourceState);
        return new CaptureFilterTransitionResult
        {
            SessionId = Id,
            RequestedFilter = filter,
            PreviousFilter = previousFilter,
            EffectiveFilter = effectiveFilter,
            Status = status,
            StartedAt = startedAt,
            CompletedAt = DateTimeOffset.UtcNow,
            PreservedPacketCount = PacketCount,
            PreservedByteCount = ByteCount,
            SourceSegmentCount = state.Segments.Length,
            Error = error,
        };
    }

    private static async Task DisposeReplacementAsync(
        ICaptureSource? replacement,
        string segmentFolder)
    {
        if (replacement is not null)
        {
            try
            {
                await replacement.StopAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
            {
            }

            try
            {
                await replacement.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
            {
            }
        }

        try
        {
            if (Directory.Exists(segmentFolder))
            {
                Directory.Delete(segmentFolder, recursive: true);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private long SumSources(Func<ICaptureSource, long> selector)
    {
        SourceState state = Volatile.Read(ref _sourceState);
        return SumSources(state, selector);
    }

    private static long SumSources(
        SourceState state,
        Func<ICaptureSource, long> selector)
    {
        long total = 0;
        foreach (ICaptureSource source in state.Segments)
        {
            total += selector(source);
        }
        return total;
    }

    private static IReadOnlyList<string> GetRawPcapFilePaths(SourceState state)
    {
        var paths = new List<string>(state.Segments.Length);
        foreach (ICaptureSource source in state.Segments)
        {
            string? path = source.GetRawPcapFilePath();
            if (!string.IsNullOrWhiteSpace(path))
            {
                paths.Add(path);
            }
        }
        return paths;
    }

    private async Task StopAllSourcesAsync(CancellationToken cancellationToken)
    {
        Exception? firstFailure = null;
        SourceState state = Volatile.Read(ref _sourceState);
        for (int i = state.Segments.Length - 1; i >= 0; i--)
        {
            try
            {
                await state.Segments[i].StopAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
            {
                firstFailure ??= ex;
            }
        }

        if (firstFailure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(firstFailure).Throw();
        }
    }

    private void ArmSourceCompletionObserver(ICaptureSource source)
    {
        CancelSourceCompletionObserver();
        if (source is not ICaptureSourceCompletion completion)
        {
            return;
        }

        var cancellation = new CancellationTokenSource();
        _sourceCompletionCancellation = cancellation;
        _sourceCompletionObserver = ObserveSourceCompletionAsync(
            source,
            completion.Completion,
            cancellation);
    }

    private void CancelSourceCompletionObserver()
    {
        CancellationTokenSource? cancellation =
            Interlocked.Exchange(ref _sourceCompletionCancellation, null);
        cancellation?.Cancel();
    }

    private async Task ObserveSourceCompletionAsync(
        ICaptureSource source,
        Task completion,
        CancellationTokenSource cancellation)
    {
        Exception? sourceFailure = null;
        try
        {
            await completion.WaitAsync(cancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            cancellation.Dispose();
            return;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
        {
            sourceFailure = ex;
        }

        try
        {
            using AsyncOperationGate.Lease operation =
                await _operations.EnterAsync(CancellationToken.None).ConfigureAwait(false);
            if (_disposed != 0
                || State != CaptureSessionState.Running
                || !ReferenceEquals(Source, source))
            {
                return;
            }

            _ = await StopCoreAsync(sourceFailure, CancellationToken.None).ConfigureAwait(false);
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            cancellation.Dispose();
        }
    }

    internal Task<DrainTailResult> DrainTailAsync(
        long sinceIndex,
        int max,
        CancellationToken cancellationToken) =>
        DrainSubscriberTailAsync(
            sinceIndex,
            max,
            subscriberId: null,
            subscriberCapacity: null,
            cancellationToken);

    internal async Task<DrainTailResult> DrainSubscriberTailAsync(
        long sinceIndex,
        int max,
        string? subscriberId,
        int? subscriberCapacity,
        CancellationToken cancellationToken)
    {
        DecodeCursor cursor = GetOrCreateCursor();
        using AsyncOperationGate.Lease operation =
            await cursor.Operations.EnterAsync(cancellationToken).ConfigureAwait(false);
        await RefreshCursorCoreAsync(cursor, cancellationToken).ConfigureAwait(false);

        sinceIndex = Math.Max(0, sinceIndex);
        max = Math.Max(1, max);
        bool sessionDone = IsTailCompleteState(State) && cursor.IsCompleted;
        LastTouchedAt = DateTimeOffset.UtcNow;
        return subscriberId is null
            ? cursor.DrainLegacy(sinceIndex, max, sessionDone, State)
            : cursor.DrainSubscriber(
                subscriberId,
                sinceIndex,
                max,
                subscriberCapacity ?? DecodeCursor.DefaultSubscriberCapacity,
                sessionDone,
                State);
    }

    internal async Task<int> ReserveTailSubscriberAsync(
        string subscriberId,
        int subscriberCapacity,
        bool startProducer,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberId);
        DecodeCursor cursor = GetOrCreateCursor();
        using AsyncOperationGate.Lease operation =
            await cursor.Operations.EnterAsync(cancellationToken).ConfigureAwait(false);
        bool created = cursor.ReserveSubscriber(subscriberId, subscriberCapacity);
        try
        {
            await RefreshCursorCoreAsync(cursor, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (created)
            {
                cursor.CloseSubscriber(subscriberId);
            }
            throw;
        }

        if (startProducer)
        {
            cursor.StartProducer(this);
        }
        return cursor.GetSubscriberCapacity(subscriberId);
    }

    internal async Task<TailSubscriberSnapshot> ReadTailSubscriberSnapshotAsync(
        string subscriberId,
        long sinceIndex,
        CancellationToken cancellationToken)
    {
        DecodeCursor cursor = GetExistingCursor();
        using AsyncOperationGate.Lease operation =
            await cursor.Operations.EnterAsync(cancellationToken).ConfigureAwait(false);
        await RefreshCursorCoreAsync(cursor, cancellationToken).ConfigureAwait(false);
        LastTouchedAt = DateTimeOffset.UtcNow;
        return cursor.ReadSubscriberSnapshot(
            subscriberId,
            sinceIndex,
            IsTailCompleteState(State) && cursor.IsCompleted,
            State);
    }

    internal async Task<bool> CloseTailSubscriberAsync(
        string subscriberId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriberId);
        DecodeCursor? cursor;
        lock (_cursorInitLock)
        {
            cursor = _cursor;
        }
        if (cursor is null)
        {
            return false;
        }

        try
        {
            using AsyncOperationGate.Lease operation =
                await cursor.Operations.EnterAsync(cancellationToken).ConfigureAwait(false);
            return cursor.CloseSubscriber(subscriberId);
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
    }

    private DecodeCursor GetOrCreateCursor()
    {
        lock (_cursorInitLock)
        {
            if (_cursorOperationsClosed || _disposed != 0)
            {
                throw new ObjectDisposedException(nameof(CaptureSession));
            }
            return _cursor ??= new DecodeCursor(
                _logger,
                _secretStateCleared ? null : CreateUnwrapper());
        }
    }

    private DecodeCursor GetExistingCursor()
    {
        lock (_cursorInitLock)
        {
            if (_cursorOperationsClosed || _disposed != 0 || _cursor is null)
            {
                throw new ObjectDisposedException(nameof(CaptureSession));
            }
            return _cursor;
        }
    }

    private async Task RefreshCursorCoreAsync(
        DecodeCursor cursor,
        CancellationToken cancellationToken)
    {
        if (State is CaptureSessionState.Disposed)
        {
            return;
        }
        if (State is CaptureSessionState.Failed)
        {
            cursor.Complete();
            return;
        }

        SourceState sourceState = Volatile.Read(ref _sourceState);
        foreach (ICaptureSource source in sourceState.Segments)
        {
            long consumed = cursor.GetPacketsConsumed(source);
            if (source.PacketCount <= consumed)
            {
                continue;
            }

            long read = 0;
            if (source is IIncrementalCaptureSource incremental)
            {
                await foreach (CapturedPacket packet in incremental.ReadFromAsync(
                    consumed,
                    cancellationToken).ConfigureAwait(false))
                {
                    cursor.Decode(packet);
                    read++;
                }
            }
            else
            {
                long packetIndex = 0;
                await foreach (CapturedPacket packet in source.ReadAllAsync(
                    maxPackets: null,
                    cancellationToken).ConfigureAwait(false))
                {
                    if (packetIndex++ < consumed)
                    {
                        continue;
                    }
                    cursor.Decode(packet);
                    read++;
                }
            }

            cursor.SetPacketsConsumed(source, consumed + read);
        }
        if (IsTailCompleteState(State))
        {
            cursor.Complete();
        }
    }

    private async Task RunCursorProducerAsync(
        DecodeCursor cursor,
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using AsyncOperationGate.Lease operation =
                    await cursor.Operations.EnterAsync(cancellationToken).ConfigureAwait(false);
                await RefreshCursorCoreAsync(cursor, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not ThreadAbortException)
            {
                _logger.LogWarning(
                    ex,
                    "Capture session {SessionId} incremental decode producer failed; retrying.",
                    Id);
            }

            if (IsTailCompleteState(State))
            {
                return;
            }
            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        }
    }

    internal NtlmPassiveUnwrapper? CreateUnwrapper()
    {
        lock (_keyLock)
        {
            if (_ntlmSessionKey is not { Length: NtlmPassiveUnwrapper.VerifierLength } sessionKey)
            {
                return null;
            }
            try
            {
                return new NtlmPassiveUnwrapper(sessionKey);
            }
            catch
            {
                CryptographicOperations.ZeroMemory(sessionKey);
                _ntlmSessionKey = null;
                throw;
            }
        }
    }

    private void ClearOwnedSessionKey()
    {
        lock (_keyLock)
        {
            if (_ntlmSessionKey is null)
            {
                return;
            }
            CryptographicOperations.ZeroMemory(_ntlmSessionKey);
            _ntlmSessionKey = null;
        }
    }

    private async Task ClearSecretStateAsync()
    {
        DecodeCursor? cursor;
        lock (_cursorInitLock)
        {
            _secretStateCleared = true;
            ClearOwnedSessionKey();
            cursor = _cursor;
            SecretCleanupObserved?.Invoke();
        }
        if (cursor is null)
        {
            return;
        }
        try
        {
            using AsyncOperationGate.Lease operation =
                await cursor.Operations.EnterAsync(CancellationToken.None).ConfigureAwait(false);
            cursor.ResetWithoutUnwrapper();
        }
        catch (ObjectDisposedException)
        {
        }
    }

    private static bool IsTailCompleteState(CaptureSessionState state) =>
        state is CaptureSessionState.Completed
            or CaptureSessionState.Failed
            or CaptureSessionState.Disposed;

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Dispose must release capture resources regardless of source-side failures.")]
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        CancelSourceCompletionObserver();
        DecodeCursor? cursor;
        lock (_cursorInitLock)
        {
            _cursorOperationsClosed = true;
            cursor = _cursor;
            _cursor = null;
        }
        if (cursor is not null)
        {
            await cursor.DisposeAsync().ConfigureAwait(false);
        }
        await _operations.DisposeAsync().ConfigureAwait(false);
        if (_sourceCompletionObserver is Task completionObserver)
        {
            await completionObserver.ConfigureAwait(false);
        }

        SourceState sourceState = Volatile.Read(ref _sourceState);
        foreach (ICaptureSource source in sourceState.Segments)
        {
            try
            {
                await source.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Capture session {SessionId} source dispose error.", Id);
            }
        }
        try
        {
            if (Directory.Exists(SessionFolder))
            {
                Directory.Delete(SessionFolder, recursive: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Capture session {SessionId} folder cleanup error.", Id);
        }

        State = CaptureSessionState.Disposed;
        await ClearSecretStateAsync().ConfigureAwait(false);
    }

    private sealed class DecodeCursor
    {
        internal const int DefaultSubscriberCapacity = 1024;
        internal const int MaxSubscribers = 64;
        private const int MaxSubscriberCapacity = 5000;
        private const int RecoveryCapacity = 5000;

        private readonly ILogger _logger;
        private readonly List<IndexedPdu> _recovery = new(RecoveryCapacity);
        private readonly Dictionary<string, SubscriberCursor> _subscribers = new(StringComparer.Ordinal);
        private readonly Dictionary<ICaptureSource, long> _packetsConsumed =
            new(ReferenceEqualityComparer.Instance);
        private readonly object _producerLock = new();
        private readonly CancellationTokenSource _producerStopping = new();
        private Task? _producerTask;
        private bool _completed;

        public DecodeCursor(ILogger logger, NtlmPassiveUnwrapper? unwrapper)
        {
            _logger = logger;
            Unwrapper = unwrapper;
            Decoder = new OpcDcomDecoder(unwrapper, logger);
        }

        public OpcDcomDecoder Decoder { get; private set; }
        public NtlmPassiveUnwrapper? Unwrapper { get; private set; }
        public long TotalEmitted { get; private set; }
        public bool IsCompleted => _completed;
        public AsyncOperationGate Operations { get; } = new(nameof(DecodeCursor));

        public long GetPacketsConsumed(ICaptureSource source) =>
            _packetsConsumed.TryGetValue(source, out long consumed) ? consumed : 0;

        public void SetPacketsConsumed(ICaptureSource source, long consumed) =>
            _packetsConsumed[source] = consumed;

        public void Decode(CapturedPacket packet)
        {
            if (_completed)
            {
                return;
            }
            foreach (DecodedOpcPdu pdu in Decoder.Decode(packet))
            {
                Add(pdu);
            }
        }

        public void Complete()
        {
            if (_completed)
            {
                return;
            }
            foreach (DecodedDcomFrame frame in Decoder.CompleteDetailed())
            {
                if (frame.Pdu is not null)
                {
                    Add(frame.Pdu);
                }
            }
            _completed = true;
        }

        public void StartProducer(CaptureSession session)
        {
            lock (_producerLock)
            {
                _producerTask ??= session.RunCursorProducerAsync(this, _producerStopping.Token);
            }
        }

        public bool ReserveSubscriber(string subscriberId, int capacity)
        {
            string id = NormalizeSubscriberId(subscriberId);
            if (_subscribers.ContainsKey(id))
            {
                return false;
            }
            if (_subscribers.Count >= MaxSubscribers)
            {
                throw new CaptureException($"Capture subscriber limit reached ({MaxSubscribers} per session).");
            }
            int bounded = capacity <= 0
                ? DefaultSubscriberCapacity
                : Math.Min(capacity, MaxSubscriberCapacity);
            _subscribers.Add(id, new SubscriberCursor(id, bounded));
            return true;
        }

        public int GetSubscriberCapacity(string subscriberId)
        {
            string id = NormalizeSubscriberId(subscriberId);
            return _subscribers.TryGetValue(id, out SubscriberCursor? subscriber)
                ? subscriber.Capacity
                : throw new CaptureException($"Capture subscriber '{id}' is not reserved.");
        }

        public bool CloseSubscriber(string subscriberId) =>
            _subscribers.Remove(subscriberId.Trim());

        public DrainTailResult DrainLegacy(
            long sinceIndex,
            int max,
            bool done,
            CaptureSessionState state) =>
            ReadWindow(sinceIndex, max, RecoveryFrom, done, state, subscriber: null);

        public DrainTailResult DrainSubscriber(
            string subscriberId,
            long sinceIndex,
            int max,
            int capacity,
            bool done,
            CaptureSessionState state)
        {
            ReserveSubscriber(subscriberId, capacity);
            SubscriberCursor subscriber = _subscribers[NormalizeSubscriberId(subscriberId)];
            return ReadWindow(
                sinceIndex,
                max,
                Math.Max(RecoveryFrom, Math.Max(0, TotalEmitted - subscriber.Capacity)),
                done,
                state,
                subscriber);
        }

        public TailSubscriberSnapshot ReadSubscriberSnapshot(
            string subscriberId,
            long sinceIndex,
            bool done,
            CaptureSessionState state)
        {
            string id = NormalizeSubscriberId(subscriberId);
            if (!_subscribers.TryGetValue(id, out SubscriberCursor? subscriber))
            {
                throw new CaptureException($"Capture subscriber '{id}' is not reserved.");
            }
            long requested = Math.Min(Math.Max(0, sinceIndex), TotalEmitted);
            long available = Math.Max(
                RecoveryFrom,
                Math.Max(0, TotalEmitted - subscriber.Capacity));
            IReadOnlyList<CaptureDropRange> dropped = requested < available
                ? [new CaptureDropRange(requested, available - 1)]
                : [];
            return new TailSubscriberSnapshot(
                Math.Max(requested, available),
                TotalEmitted,
                TotalEmitted,
                done,
                state,
                dropped);
        }

        public void ResetWithoutUnwrapper()
        {
            Complete();
            Unwrapper?.Dispose();
            Unwrapper = null;
            Decoder = new OpcDcomDecoder(_logger);
            _completed = true;
        }

        public async ValueTask DisposeAsync()
        {
            _producerStopping.Cancel();
            ValueTask closing = Operations.DisposeAsync();
            Task? producer;
            lock (_producerLock)
            {
                producer = _producerTask;
            }
            if (producer is not null)
            {
                try
                {
                    await producer.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
            }
            await closing.ConfigureAwait(false);
            Unwrapper?.Dispose();
            _subscribers.Clear();
            _recovery.Clear();
            _packetsConsumed.Clear();
            _producerStopping.Dispose();
        }

        private long RecoveryFrom => _recovery.Count == 0 ? TotalEmitted : _recovery[0].Index;

        private void Add(DecodedOpcPdu pdu)
        {
            _recovery.Add(new IndexedPdu(TotalEmitted++, pdu));
            if (_recovery.Count > RecoveryCapacity)
            {
                _recovery.RemoveAt(0);
            }
        }

        private DrainTailResult ReadWindow(
            long sinceIndex,
            int max,
            long available,
            bool done,
            CaptureSessionState state,
            SubscriberCursor? subscriber)
        {
            long requested = Math.Min(Math.Max(0, sinceIndex), TotalEmitted);
            IReadOnlyList<CaptureDropRange> dropped = requested < available
                ? [new CaptureDropRange(requested, available - 1)]
                : [];
            long start = Math.Max(requested, available);
            var output = new List<DecodedOpcPdu>(Math.Min(max, _recovery.Count));
            long next = start;
            foreach (IndexedPdu item in _recovery)
            {
                if (item.Index < start)
                {
                    continue;
                }
                if (output.Count >= max)
                {
                    break;
                }
                output.Add(item.Pdu);
                next = item.Index + 1;
            }
            return new DrainTailResult(
                output,
                next,
                TotalEmitted,
                done && next >= TotalEmitted,
                state,
                subscriber?.Id,
                subscriber?.Capacity,
                dropped.Count > 0,
                dropped);
        }

        private static string NormalizeSubscriberId(string subscriberId)
        {
            string id = subscriberId.Trim();
            if (id.Length == 0 || id.Length > 128)
            {
                throw new CaptureException("subscriberId must contain 1..128 characters.");
            }
            return id;
        }

        private sealed record IndexedPdu(long Index, DecodedOpcPdu Pdu);
        private sealed record SubscriberCursor(string Id, int Capacity);
    }

    private sealed record SourceState(
        ICaptureSource Active,
        ICaptureSource[] Segments);
}

internal sealed record CaptureSessionSnapshot(
    string Id,
    string SourceName,
    CaptureSessionState State,
    DateTimeOffset? StartedAt,
    DateTimeOffset? StoppedAt,
    long PacketCount,
    long ByteCount,
    CaptureStartRequest Request,
    CaptureTargetMetadata? Target,
    string? EffectiveFilter,
    IReadOnlyList<string> RawPcapFilePaths,
    string? Error,
    CaptureFilterTransitionResult? FilterTransition);

internal sealed record DrainTailResult(
    IReadOnlyList<DecodedOpcPdu> Pdus,
    long NextIndex,
    long TotalEmitted,
    bool Done,
    CaptureSessionState SessionState,
    string? SubscriberId = null,
    int? SubscriberCapacity = null,
    bool Overflowed = false,
    IReadOnlyList<CaptureDropRange>? DroppedRanges = null);

internal sealed record TailSubscriberSnapshot(
    long FirstIndex,
    long NextIndex,
    long TotalEmitted,
    bool Done,
    CaptureSessionState SessionState,
    IReadOnlyList<CaptureDropRange> DroppedRanges);

public sealed record CaptureDropRange(long FirstIndex, long LastIndex);
