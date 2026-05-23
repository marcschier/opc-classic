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
/// The managed async-first OPC AE server contract.
/// </summary>
public interface IAeServer : IAsyncDisposable
{
    /// <summary>Raised when the server emits <c>IOPCShutdown::ShutdownRequest</c>.</summary>
    event EventHandler<EventArgs>? ServerShutdown;

    /// <summary>Retrieve AE server runtime state.</summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Browse the event-area namespace starting at <paramref name="areaQualifiedName"/>.
    /// Empty string = root. Servers stream the result lazily.
    /// </summary>
    IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(
        string areaQualifiedName,
        CancellationToken cancellationToken = default);

    /// <summary>List event categories the server supports for the given event types.</summary>
    Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(
        EventType eventTypes,
        CancellationToken cancellationToken = default);

    /// <summary>List the conditions the server defines for the given category.</summary>
    Task<IReadOnlyList<string>> QueryConditionNamesAsync(
        uint eventCategory,
        CancellationToken cancellationToken = default);

    /// <summary>Acknowledge a batch of condition events.</summary>
    Task<IReadOnlyList<AckResult>> AcknowledgeAsync(
        string actor,
        string? comment,
        IReadOnlyList<ConditionRef> conditions,
        CancellationToken cancellationToken = default);

    /// <summary>Enable monitoring of conditions for an area / source.</summary>
    Task<OpcResultId> EnableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default);

    /// <summary>Disable monitoring of conditions for an area / source.</summary>
    Task<OpcResultId> DisableConditionsByAreaAsync(
        IReadOnlyList<string> areaQualifiedNames,
        CancellationToken cancellationToken = default);

    /// <summary>Create a server-side event subscription and return its handle.</summary>
    Task<IAeSubscription> CreateSubscriptionAsync(
        bool active,
        int bufferTimeMs,
        int maxBufferSize,
        CancellationToken cancellationToken = default);
}

