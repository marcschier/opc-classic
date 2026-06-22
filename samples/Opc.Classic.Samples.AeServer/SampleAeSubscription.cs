// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.CompilerServices;
using Opc.Classic.Ae;

namespace Opc.Classic.Samples.AeServer;

internal sealed class SampleAeSubscription : IAeSubscription
{
    private static readonly TimeSpan EventDeliveryDelay = TimeSpan.FromMilliseconds(250);
    private readonly Func<IReadOnlyList<EventNotification>> _eventsFactory;
    private readonly int _bufferTimeMs;
    private readonly int _maxBufferSize;
    private bool _active;
    private bool _disposed;

    public SampleAeSubscription(
        bool active,
        Func<IReadOnlyList<EventNotification>> eventsFactory,
        int bufferTimeMs,
        int maxBufferSize)
    {
        _eventsFactory = eventsFactory ?? throw new ArgumentNullException(nameof(eventsFactory));
        _active = active;
        _bufferTimeMs = bufferTimeMs;
        _maxBufferSize = maxBufferSize;
    }

    public bool Active => _active;
    public SubscriptionFilter Filter { get; private set; } = new();
    public IAsyncEnumerable<EventNotification> Events => ReadEventsAsync();

    public Task SetActiveAsync(bool active, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        _active = active;
        return Task.CompletedTask;
    }

    public Task SetFilterAsync(SubscriptionFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        Filter = filter;
        return Task.CompletedTask;
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return Task.CompletedTask;
    }

    public Task CancelRefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ThrowIfDisposed();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _active = false;
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    private async IAsyncEnumerable<EventNotification> ReadEventsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        foreach (EventNotification notification in ApplyBufferPolicy(_eventsFactory()))
        {
            await Task.Delay(EventDeliveryDelay, cancellationToken).ConfigureAwait(false);
            if (_disposed)
            {
                yield break;
            }

            if (_active && MatchesFilter(Filter, notification))
            {
                yield return notification;
            }
        }
    }

    private IReadOnlyList<EventNotification> ApplyBufferPolicy(IReadOnlyList<EventNotification> notifications)
    {
        if (_bufferTimeMs < 0 || _maxBufferSize <= 0 || notifications.Count <= _maxBufferSize)
        {
            return notifications;
        }

        return notifications.Take(_maxBufferSize).ToArray();
    }

    private static bool MatchesFilter(SubscriptionFilter filter, EventNotification notification)
    {
        if ((filter.EventTypes & notification.EventType) == 0)
        {
            return false;
        }

        if (notification.Severity < filter.MinSeverity || notification.Severity > filter.MaxSeverity)
        {
            return false;
        }

        if (filter.EventCategories.Count > 0 && !filter.EventCategories.Contains(notification.EventCategory))
        {
            return false;
        }

        if (filter.Sources.Count > 0 && !filter.Sources.Contains(notification.Source, StringComparer.Ordinal))
        {
            return false;
        }

        if (filter.Areas.Count > 0 && !filter.Areas.Any(area => IsInArea(notification.Source, area)))
        {
            return false;
        }

        return true;
    }

    private static bool IsInArea(string source, string area) =>
        string.Equals(source, area, StringComparison.Ordinal) ||
        source.StartsWith(string.Concat(area, "."), StringComparison.Ordinal);

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
