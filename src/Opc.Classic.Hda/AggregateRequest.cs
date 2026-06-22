// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Hda;

/// <summary>
/// Per-item aggregate configuration passed to
/// <see cref="IHdaServer.ReadProcessedAsync"/>.
/// </summary>
/// <param name="ItemId">The item ID.</param>
/// <param name="Aggregate">The HDA built-in (or vendor-defined) aggregate.</param>
public readonly record struct AggregateRequest(string ItemId, HdaAggregate Aggregate);
