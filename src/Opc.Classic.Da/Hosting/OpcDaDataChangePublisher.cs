//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1031 // Subscriber failures are isolated so fan-out can continue.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default server-side data-change publisher for advised OPC DA callbacks.
/// </summary>
public sealed class OpcDaDataChangePublisher : IOpcDaDataChangePublisher, IAsyncDisposable
{
    private static readonly Action<ILogger, int, Exception?> SubscriberThrew = LoggerMessage.Define<int>(
        LogLevel.Warning,
        new EventId(1, nameof(SubscriberThrew)),
        "OpcDaDataChangePublisher subscriber {Cookie} threw; continuing fan-out");

    private readonly ILogger<OpcDaDataChangePublisher> _logger;
    private readonly ConcurrentDictionary<int, OpcDaSubscriberEntry> _subscribers;
    private int _nextCookie;

    /// <summary>Initializes a new instance of the <see cref="OpcDaDataChangePublisher" /> class.</summary>
    public OpcDaDataChangePublisher(ILogger<OpcDaDataChangePublisher> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _subscribers = new();
    }

    /// <summary>
    /// Registers a callback. Returns the cookie (Advise return value) the client
    /// uses for Unadvise. Mirrors IConnectionPoint::Advise semantics.
    /// </summary>
    /// <param name="callback">Callback invoked for each published data-change batch.</param>
    /// <returns>The advise cookie used to remove the subscriber.</returns>
    public int Advise(Func<OpcDaDataChange, CancellationToken, ValueTask> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        var cookie = Interlocked.Increment(ref _nextCookie);
        _subscribers[cookie] = new OpcDaSubscriberEntry(callback);
        return cookie;
    }

    /// <summary>Removes an advised callback by cookie.</summary>
    /// <param name="cookie">The cookie returned from <see cref="Advise" />.</param>
    public void Unadvise(int cookie) => _subscribers.TryRemove(cookie, out _);

    /// <inheritdoc />
    public async ValueTask PublishAsync(OpcDaDataChange change, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(change);
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var (cookie, entry) in _subscribers)
        {
            try
            {
                await entry.Callback(change, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                SubscriberThrew(_logger, cookie, ex);
            }
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _subscribers.Clear();
        return ValueTask.CompletedTask;
    }

    private sealed record OpcDaSubscriberEntry(
        Func<OpcDaDataChange, CancellationToken, ValueTask> Callback);
}
