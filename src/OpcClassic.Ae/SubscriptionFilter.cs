//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace OpcClassic.Ae;

/// <summary>
/// Server-side filter applied to an OPC AE event subscription
/// (<c>IOPCEventSubscriptionMgt::SetFilter</c>). Events that don't match
/// the filter are not delivered to this subscription's <c>IOPCEventSink</c>.
/// </summary>
public sealed class SubscriptionFilter
{
    /// <summary>Event-type kinds to deliver. Default is all (Simple|Tracking|Condition).</summary>
    public EventType EventTypes { get; init; } = EventType.All;

    /// <summary>Minimum severity (0..1000) — events below this are suppressed.</summary>
    public int MinSeverity { get; init; }

    /// <summary>Maximum severity (0..1000) — events above this are suppressed. 1000 by default.</summary>
    public int MaxSeverity { get; init; } = 1000;

    /// <summary>
    /// Event categories to include. Empty list = all categories. Categories
    /// are server-defined uint IDs; consult the server's
    /// <c>IOPCEventServer::QueryEventCategories</c> for valid values.
    /// </summary>
    public IReadOnlyList<uint> EventCategories { get; init; } = Array.Empty<uint>();

    /// <summary>
    /// Area filter strings (server-specific path patterns). Empty list = all areas.
    /// </summary>
    public IReadOnlyList<string> Areas { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Event-source filter strings (server-specific source patterns). Empty list = all sources.
    /// </summary>
    public IReadOnlyList<string> Sources { get; init; } = Array.Empty<string>();

    /// <summary>True if any non-trivial filter criterion is set.</summary>
    public bool HasAnyCriterion =>
        EventTypes != EventType.All
        || MinSeverity > 0
        || MaxSeverity < 1000
        || EventCategories.Count > 0
        || Areas.Count > 0
        || Sources.Count > 0;
}
