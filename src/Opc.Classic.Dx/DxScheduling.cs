// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable MA0048 // Clock, scheduler, and deterministic implementation are a cohesive API.

namespace Opc.Classic.Dx;

/// <summary>
/// Supplies wall-clock and monotonic time to DX runtime components.
/// </summary>
public interface IDxClock
{
    /// <summary>
    /// Current UTC time.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets a monotonically increasing timestamp.
    /// </summary>
    long GetTimestamp();

    /// <summary>
    /// Computes elapsed time between two monotonic timestamps.
    /// </summary>
    TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp);
}

/// <summary>
/// Schedules cancellable DX delays against an injectable clock.
/// </summary>
public interface IDxScheduler
{
    /// <summary>
    /// Clock used by this scheduler.
    /// </summary>
    IDxClock Clock { get; }

    /// <summary>
    /// Completes after the requested delay or cancellation.
    /// </summary>
    ValueTask DelayAsync(
        TimeSpan delay,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Production scheduler backed by <see cref="TimeProvider"/>.
/// </summary>
public sealed class SystemDxScheduler : IDxScheduler, IDxClock
{
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Shared scheduler backed by <see cref="TimeProvider.System"/>.
    /// </summary>
    public static SystemDxScheduler Instance { get; } = new(TimeProvider.System);

    /// <summary>
    /// Creates a scheduler over a time provider.
    /// </summary>
    public SystemDxScheduler(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public IDxClock Clock => this;

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _timeProvider.GetUtcNow();

    /// <inheritdoc />
    public long GetTimestamp() => _timeProvider.GetTimestamp();

    /// <inheritdoc />
    public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
        _timeProvider.GetElapsedTime(startingTimestamp, endingTimestamp);

    /// <inheritdoc />
    public ValueTask DelayAsync(
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);
        return new(Task.Delay(delay, _timeProvider, cancellationToken));
    }
}

/// <summary>
/// Manually advanced deterministic scheduler for update-rate and retry tests.
/// </summary>
public sealed class ManualDxScheduler : IDxScheduler, IDxClock
{
    private readonly Lock _syncRoot = new();
    private readonly List<PendingDelay> _pending = new();
    private DateTimeOffset _utcNow;
    private long _timestamp;
    private long _nextSequence;

    /// <summary>
    /// Creates a scheduler at the supplied UTC time.
    /// </summary>
    public ManualDxScheduler(DateTimeOffset? initialUtcNow = null)
    {
        _utcNow = initialUtcNow ?? DateTimeOffset.UnixEpoch;
    }

    /// <inheritdoc />
    public IDxClock Clock => this;

    /// <inheritdoc />
    public DateTimeOffset UtcNow
    {
        get
        {
            lock (_syncRoot)
            {
                return _utcNow;
            }
        }
    }

    /// <summary>
    /// Number of incomplete scheduled delays.
    /// </summary>
    public int PendingCount
    {
        get
        {
            lock (_syncRoot)
            {
                return _pending.Count;
            }
        }
    }

    /// <inheritdoc />
    public long GetTimestamp()
    {
        lock (_syncRoot)
        {
            return _timestamp;
        }
    }

    /// <inheritdoc />
    public TimeSpan GetElapsedTime(long startingTimestamp, long endingTimestamp) =>
        TimeSpan.FromTicks(checked(endingTimestamp - startingTimestamp));

    /// <inheritdoc />
    public ValueTask DelayAsync(
        TimeSpan delay,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(delay, TimeSpan.Zero);
        cancellationToken.ThrowIfCancellationRequested();
        if (delay == TimeSpan.Zero)
        {
            return ValueTask.CompletedTask;
        }

        PendingDelay pending;
        lock (_syncRoot)
        {
            var dueTimestamp = checked(_timestamp + delay.Ticks);
            pending = new PendingDelay(dueTimestamp, _nextSequence++);
            _pending.Add(pending);
        }

        var registration = cancellationToken.UnsafeRegister(
            static state =>
            {
                var cancellation = (CancellationState)state!;
                cancellation.Scheduler.Cancel(cancellation.Pending, cancellation.Token);
            },
            new CancellationState(this, pending, cancellationToken));
        pending.SetRegistration(registration);
        return new(pending.Task);
    }

    /// <summary>
    /// Advances time and completes every delay due at or before the new time.
    /// </summary>
    public void AdvanceBy(TimeSpan amount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(amount, TimeSpan.Zero);

        PendingDelay[] due;
        lock (_syncRoot)
        {
            _timestamp = checked(_timestamp + amount.Ticks);
            _utcNow = _utcNow.AddTicks(amount.Ticks);
            due = ExtractDueDelays();
        }

        foreach (var pending in due)
        {
            pending.Complete();
        }
    }

    private PendingDelay[] ExtractDueDelays()
    {
        var due = _pending
            .Where(pending => pending.DueTimestamp <= _timestamp)
            .OrderBy(pending => pending.DueTimestamp)
            .ThenBy(pending => pending.Sequence)
            .ToArray();
        if (due.Length > 0)
        {
            _pending.RemoveAll(pending => pending.DueTimestamp <= _timestamp);
        }

        return due;
    }

    private void Cancel(PendingDelay pending, CancellationToken cancellationToken)
    {
        lock (_syncRoot)
        {
            if (!_pending.Remove(pending))
            {
                return;
            }
        }

        pending.Cancel(cancellationToken);
    }

    private sealed record CancellationState(
        ManualDxScheduler Scheduler,
        PendingDelay Pending,
        CancellationToken Token);

    private sealed class PendingDelay
    {
        private readonly Lock _syncRoot = new();
        private readonly TaskCompletionSource _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        private CancellationTokenRegistration _registration;
        private bool _finished;
        private bool _registrationSet;

        public PendingDelay(long dueTimestamp, long sequence)
        {
            DueTimestamp = dueTimestamp;
            Sequence = sequence;
        }

        public long DueTimestamp { get; }

        public long Sequence { get; }

        public Task Task => _completion.Task;

        public void SetRegistration(CancellationTokenRegistration registration)
        {
            var dispose = false;
            lock (_syncRoot)
            {
                if (_finished)
                {
                    dispose = true;
                }
                else
                {
                    _registration = registration;
                    _registrationSet = true;
                }
            }

            if (dispose)
            {
                registration.Dispose();
            }
        }

        public void Complete()
        {
            Finish();
            _completion.TrySetResult();
        }

        public void Cancel(CancellationToken cancellationToken)
        {
            Finish();
            _completion.TrySetCanceled(cancellationToken);
        }

        private void Finish()
        {
            CancellationTokenRegistration registration = default;
            lock (_syncRoot)
            {
                if (_finished)
                {
                    return;
                }

                _finished = true;
                if (_registrationSet)
                {
                    registration = _registration;
                }
            }

            registration.Dispose();
        }
    }
}
