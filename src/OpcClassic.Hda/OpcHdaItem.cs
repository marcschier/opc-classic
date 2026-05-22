//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Hda;

/// <summary>
/// OPC HDA's <c>OPCHDA_ITEM</c> — a series of timestamped, quality-tagged
/// values for one historical item, optionally aggregated. Returned by
/// <c>IOPCHDA_SyncRead</c> and friends.
/// </summary>
public sealed record OpcHdaItem
{
    /// <summary>Constructor — validates the three parallel arrays have the same length.</summary>
    /// <param name="clientHandle">Client correlation handle.</param>
    /// <param name="aggregateHandle">Aggregate ID (0 = no aggregate applied).</param>
    /// <param name="timestamps">UTC timestamps; same length as <paramref name="qualities"/> and <paramref name="values"/>.</param>
    /// <param name="qualities">HDA-style quality DWORDs.</param>
    /// <param name="values">Per-sample values.</param>
    public OpcHdaItem(
        int clientHandle,
        int aggregateHandle,
        DateTimeOffset[] timestamps,
        uint[] qualities,
        OpcVariant[] values)
    {
        ArgumentNullException.ThrowIfNull(timestamps);
        ArgumentNullException.ThrowIfNull(qualities);
        ArgumentNullException.ThrowIfNull(values);
        if (timestamps.Length != qualities.Length || qualities.Length != values.Length)
        {
            throw new ArgumentException(
                $"Parallel arrays must have equal length: timestamps={timestamps.Length}, qualities={qualities.Length}, values={values.Length}.",
                nameof(values));
        }

        ClientHandle = clientHandle;
        AggregateHandle = aggregateHandle;
        Timestamps = timestamps;
        Qualities = qualities;
        Values = values;
    }

    /// <summary>Client correlation handle.</summary>
    public int ClientHandle { get; }

    /// <summary>Aggregate ID (0 = no aggregate).</summary>
    public int AggregateHandle { get; }

    /// <summary>UTC timestamps. Same length as <see cref="Qualities"/> and <see cref="Values"/>.</summary>
    public DateTimeOffset[] Timestamps { get; }

    /// <summary>HDA quality DWORDs.</summary>
    public uint[] Qualities { get; }

    /// <summary>Per-sample values.</summary>
    public OpcVariant[] Values { get; }
}
