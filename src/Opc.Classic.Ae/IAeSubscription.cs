//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Ae;

/// <summary>
/// A live OPC AE event subscription. Disposing cancels the subscription and
/// removes the server-side state.
/// </summary>
public interface IAeSubscription : IAsyncDisposable {
    /// <summary>True if the subscription is currently delivering events.</summary>
    bool Active { get; }

    /// <summary>Current filter applied server-side.</summary>
    SubscriptionFilter Filter { get; }

    /// <summary>
    /// The pushed event stream. Each <see cref="EventNotification"/> is one
    /// event the server has emitted matching the active filter. Iteration
    /// completes when the subscription is disposed or the server shuts down.
    /// </summary>
    IAsyncEnumerable<EventNotification> Events { get; }

    /// <summary>Activate or deactivate event delivery.</summary>
    Task SetActiveAsync(bool active, CancellationToken cancellationToken = default);

    /// <summary>Apply a new server-side filter.</summary>
    Task SetFilterAsync(SubscriptionFilter filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Force an immediate "refresh" — server re-emits the active state of
    /// every condition currently in alarm. Used after a reconnect to
    /// resynchronize.
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an in-progress refresh started by <see cref="RefreshAsync"/>.
    /// </summary>
    Task CancelRefreshAsync(CancellationToken cancellationToken = default);
}
