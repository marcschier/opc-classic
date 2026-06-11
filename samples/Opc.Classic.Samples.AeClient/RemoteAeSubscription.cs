// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Ae;

namespace Opc.Classic.Samples.AeClient;

internal sealed class RemoteAeSubscription : IAeSubscription
{
    public RemoteAeSubscription(bool active, int bufferTimeMs, int maxBufferSize)
    {
        Active = active;
        _ = bufferTimeMs;
        _ = maxBufferSize;
    }

    public bool Active { get; private set; }

    public SubscriptionFilter Filter { get; private set; } = new();

    public IAsyncEnumerable<EventNotification> Events => ReadEventsAsync();

    public Task SetActiveAsync(bool active, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Active = active;
        return Task.CompletedTask;
    }

    public Task SetFilterAsync(SubscriptionFilter filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        cancellationToken.ThrowIfCancellationRequested();
        Filter = filter;
        return Task.CompletedTask;
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task CancelRefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        Active = false;
        return ValueTask.CompletedTask;
    }

    private static async IAsyncEnumerable<EventNotification> ReadEventsAsync()
    {
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }
}
