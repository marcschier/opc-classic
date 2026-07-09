// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Hda;

/// <summary>
/// OPC HDA's <c>OPCHDA_MODIFIEDITEM</c> — modified historical values for
/// one item, including edit metadata and the modifying user per sample.
/// </summary>
public sealed record OpcHdaModifiedItem
{
    /// <summary>
    /// Constructor — validates all six parallel arrays have the same length.
    /// </summary>
    /// <param name="clientHandle">Client correlation handle.</param>
    /// <param name="timestamps">UTC timestamps; parallel with all per-sample arrays.</param>
    /// <param name="qualities">HDA-style quality DWORDs.</param>
    /// <param name="values">Per-sample values.</param>
    /// <param name="modificationTimes">UTC timestamps when each value was modified.</param>
    /// <param name="editTypes">OPCHDA_EDITTYPE values, carried as UInt32.</param>
    /// <param name="users">User names associated with each modification; entries may be null.</param>
    public OpcHdaModifiedItem(
        int clientHandle,
        DateTimeOffset[] timestamps,
        uint[] qualities,
        OpcVariant[] values,
        DateTimeOffset[] modificationTimes,
        uint[] editTypes,
        string?[] users)
    {
        ArgumentNullException.ThrowIfNull(timestamps);
        ArgumentNullException.ThrowIfNull(qualities);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(modificationTimes);
        ArgumentNullException.ThrowIfNull(editTypes);
        ArgumentNullException.ThrowIfNull(users);
        ValidateParallelArrayLengths(timestamps, qualities, values, modificationTimes, editTypes, users);

        ClientHandle = clientHandle;
        Timestamps = timestamps;
        Qualities = qualities;
        Values = values;
        ModificationTimes = modificationTimes;
        EditTypes = editTypes;
        Users = users;
    }

    /// <summary>
    /// Client correlation handle.
    /// </summary>
    public int ClientHandle { get; }

    /// <summary>
    /// UTC timestamps; parallel with <see cref="Values"/>.
    /// </summary>
    public DateTimeOffset[] Timestamps { get; }

    /// <summary>
    /// HDA quality DWORDs.
    /// </summary>
    public uint[] Qualities { get; }

    /// <summary>
    /// Per-sample values.
    /// </summary>
    public OpcVariant[] Values { get; }

    /// <summary>
    /// UTC timestamps when each value was modified.
    /// </summary>
    public DateTimeOffset[] ModificationTimes { get; }

    /// <summary>
    /// OPCHDA_EDITTYPE values, carried as UInt32.
    /// </summary>
    public uint[] EditTypes { get; }

    /// <summary>
    /// User names associated with each modification; entries may be null.
    /// </summary>
    public string?[] Users { get; }

    private static void ValidateParallelArrayLengths(
        DateTimeOffset[] timestamps,
        uint[] qualities,
        OpcVariant[] values,
        DateTimeOffset[] modificationTimes,
        uint[] editTypes,
        string?[] users)
    {
        int count = timestamps.Length;
        if (qualities.Length != count || values.Length != count ||
            modificationTimes.Length != count || editTypes.Length != count || users.Length != count)
        {
            throw new ArgumentException(
                $"Parallel arrays must have equal length: timestamps={timestamps.Length}, " +
                $"qualities={qualities.Length}, values={values.Length}, " +
                $"modificationTimes={modificationTimes.Length}, editTypes={editTypes.Length}, users={users.Length}.",
                nameof(values));
        }
    }
}
