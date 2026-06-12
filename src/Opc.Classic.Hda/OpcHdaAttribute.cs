//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1711 // Identifier ending in 'Attribute' - OPCHDA_ATTRIBUTE spec name preserved

namespace Opc.Classic.Hda;

/// <summary>
/// OPC HDA's <c>OPCHDA_ATTRIBUTE</c> — a series of timestamped attribute
/// values for one item attribute. Returned by
/// <c>IOPCHDA_SyncRead::ReadAttribute</c>.
/// </summary>
public sealed record OpcHdaAttribute
{
    /// <summary>
    /// Constructor — validates the parallel arrays have the same length.
    /// </summary>
    /// <param name="clientHandle">Client correlation handle.</param>
    /// <param name="attributeId">The HDA attribute being read (1=DataType, 2=Description, 100..=vendor).</param>
    /// <param name="timestamps">UTC timestamps; same length as <paramref name="values"/>.</param>
    /// <param name="values">Per-sample attribute values.</param>
    public OpcHdaAttribute(
        int clientHandle,
        int attributeId,
        DateTimeOffset[] timestamps,
        OpcVariant[] values)
    {
        ArgumentNullException.ThrowIfNull(timestamps);
        ArgumentNullException.ThrowIfNull(values);
        if (timestamps.Length != values.Length)
        {
            throw new ArgumentException(
                $"Parallel arrays must have equal length: timestamps={timestamps.Length}, values={values.Length}.",
                nameof(values));
        }

        ClientHandle = clientHandle;
        AttributeId = attributeId;
        Timestamps = timestamps;
        Values = values;
    }

    /// <summary>
    /// Client correlation handle.
    /// </summary>
    public int ClientHandle { get; }

    /// <summary>
    /// HDA attribute ID.
    /// </summary>
    public int AttributeId { get; }

    /// <summary>
    /// UTC timestamps; parallel with <see cref="Values"/>.
    /// </summary>
    public DateTimeOffset[] Timestamps { get; }

    /// <summary>
    /// Per-sample attribute values.
    /// </summary>
    public OpcVariant[] Values { get; }
}
