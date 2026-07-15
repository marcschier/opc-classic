// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.ObjectModel;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Immutable point-in-time snapshot of private and public DA groups captured
/// under one implementation synchronization boundary.
/// </summary>
public sealed class OpcDaGroupSetSnapshot
{
    /// <summary>Initializes a snapshot by copying both group collections.</summary>
    public OpcDaGroupSetSnapshot(
        IReadOnlyList<OpcDaGroup> privateGroups,
        IReadOnlyList<OpcDaGroup> publicGroups)
    {
        PrivateGroups = CopyGroups(privateGroups, nameof(privateGroups));
        PublicGroups = CopyGroups(publicGroups, nameof(publicGroups));
    }

    /// <summary>Gets private groups captured in the snapshot.</summary>
    public IReadOnlyList<OpcDaGroup> PrivateGroups { get; }

    /// <summary>Gets public groups captured in the snapshot.</summary>
    public IReadOnlyList<OpcDaGroup> PublicGroups { get; }

    internal static ReadOnlyCollection<OpcDaGroup> CopyGroups(
        IReadOnlyList<OpcDaGroup> source,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(source, parameterName);
        var copy = new OpcDaGroup[source.Count];
        for (int i = 0; i < source.Count; i++)
        {
            copy[i] = source[i] ?? throw new ArgumentException(
                "Group snapshots cannot contain null entries.",
                parameterName);
        }
        return Array.AsReadOnly(copy);
    }
}
