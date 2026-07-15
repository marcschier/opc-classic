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
    private readonly ILogger _logger;
    private byte[]? _ntlmSessionKey;
    private DecodeCursor? _cursor;
    private bool _secretStateCleared;
    private bool _cursorOperationsClosed;
    private int _disposed;

    internal Action? SecretCleanupObserved { get; set; }

    public CaptureSession(
        string id,
        string sourceName,
        ICaptureSource source,
        string sessionFolder,
        CaptureStartRequest request,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(id);
        ArgumentException.ThrowIfNullOrEmpty(sourceName);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrEmpty(sessionFolder);
        ArgumentNullException.ThrowIfNull(request);

        Id = id;
        SourceName = sourceName;
        Source = source;
        SessionFolder = sessionFolder;
        _ntlmSessionKey = request.NtlmSessionKey?.ToArray();
        Request = request with { NtlmSessionKey = null };
        _logger = logger ?? NullLogger.Instance;
    }

    public string Id { get; }
    public string SourceName { get; }
    public ICaptureSource Source { get; }
    public string SessionFolder { get; }
    public CaptureStartRequest Request { get; }
    public CaptureSessionState State { get; private set; } = CaptureSessionState.Starting;
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
        if (State is CaptureSessionState.Completed
                or CaptureSessionState.Failed
                or CaptureSessionState.Disposed)
        {
            return;
        }

        State = CaptureSessionState.Stopping;
        try
        {
            await Source.StopAsync(cancellationToken).ConfigureAwait(false);
            StoppedAt = DateTimeOffset.UtcNow;
            State = CaptureSessionState.Completed;
            LastTouchedAt = DateTimeOffset.UtcNow;
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Capture session {SessionId} completed ({Packets} packets, {Bytes} bytes).",
                    Id, Source.PacketCount, Source.ByteCount);
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
            State = CaptureSessionState.Failed;
            await ClearSecretStateAsync().ConfigureAwait(false);
            _logger.LogError(ex, "Capture session {SessionId} failed to stop.", Id);
            throw;
        }
    }

    public void Touch() => LastTouchedAt = DateTimeOffset.UtcNow;

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
        bool sessionDone = IsTailCompleteState(State);
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
        LastTouchedAt = DateTimeOffset.UtcNow;
        return cursor.ReadSubscriberSnapshot(
            subscriberId,
            sinceIndex,
            IsTailCompleteState(State),
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
        if (State is CaptureSessionState.Failed or CaptureSessionState.Disposed)
        {
            return;
        }
        long consumed = cursor.PacketsConsumed;
        if (Source.PacketCount <= consumed)
        {
            return;
        }

        long read = 0;
        if (Source is IIncrementalCaptureSource incremental)
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
            await foreach (CapturedPacket packet in Source.ReadAllAsync(
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
        cursor.PacketsConsumed = consumed + read;
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

        try
        {
            await Source.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Capture session {SessionId} source dispose error.", Id);
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
        private readonly object _producerLock = new();
        private readonly CancellationTokenSource _producerStopping = new();
        private Task? _producerTask;

        public DecodeCursor(ILogger logger, NtlmPassiveUnwrapper? unwrapper)
        {
            _logger = logger;
            Unwrapper = unwrapper;
            Decoder = new OpcDcomDecoder(unwrapper, logger);
        }

        public OpcDcomDecoder Decoder { get; private set; }
        public NtlmPassiveUnwrapper? Unwrapper { get; private set; }
        public long PacketsConsumed { get; set; }
        public long TotalEmitted { get; private set; }
        public AsyncOperationGate Operations { get; } = new(nameof(DecodeCursor));

        public void Decode(CapturedPacket packet)
        {
            foreach (DecodedOpcPdu pdu in Decoder.Decode(packet))
            {
                _recovery.Add(new IndexedPdu(TotalEmitted++, pdu));
                if (_recovery.Count > RecoveryCapacity)
                {
                    _recovery.RemoveAt(0);
                }
            }
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
            Unwrapper?.Dispose();
            Unwrapper = null;
            Decoder = new OpcDcomDecoder(_logger);
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
            _producerStopping.Dispose();
        }

        private long RecoveryFrom => _recovery.Count == 0 ? TotalEmitted : _recovery[0].Index;

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
}

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
