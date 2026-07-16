// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Immutable point-in-time group snapshot used by DA group enumerator adapters.
/// </summary>
public sealed class OpcDaGroupEnumerationSnapshot
{
    internal OpcDaGroupEnumerationSnapshot(
        OpcDaGroupEnumerationScope scope,
        IReadOnlyList<OpcDaGroup> privateGroups,
        IReadOnlyList<OpcDaGroup> publicGroups)
    {
        Scope = scope;
        PrivateGroups = OpcDaGroupSetSnapshot.CopyGroups(privateGroups, nameof(privateGroups));
        PublicGroups = OpcDaGroupSetSnapshot.CopyGroups(publicGroups, nameof(publicGroups));

        var groups = new OpcDaGroup[PrivateGroups.Count + PublicGroups.Count];
        for (int i = 0; i < PrivateGroups.Count; i++)
        {
            groups[i] = PrivateGroups[i];
        }
        for (int i = 0; i < PublicGroups.Count; i++)
        {
            groups[PrivateGroups.Count + i] = PublicGroups[i];
        }

        Groups = Array.AsReadOnly(groups);
        var names = new string[groups.Length];
        for (int i = 0; i < groups.Length; i++)
        {
            names[i] = groups[i].Name;
        }
        Names = Array.AsReadOnly(names);
    }

    /// <summary>Gets the validated scope used to create the snapshot.</summary>
    public OpcDaGroupEnumerationScope Scope { get; }

    /// <summary>Gets private groups included by the scope.</summary>
    public IReadOnlyList<OpcDaGroup> PrivateGroups { get; }

    /// <summary>Gets public groups included by the scope.</summary>
    public IReadOnlyList<OpcDaGroup> PublicGroups { get; }

    /// <summary>Gets combined identities, private groups preceding public groups.</summary>
    public IReadOnlyList<OpcDaGroup> Groups { get; }

    /// <summary>Gets group names captured when the snapshot was created.</summary>
    public IReadOnlyList<string> Names { get; }

    /// <summary>Gets whether the scope produces connection enumeration.</summary>
    public bool EnumeratesConnections => Scope.IsConnectionScope();

    /// <summary>Gets whether the scope produces name enumeration.</summary>
    public bool EnumeratesNames => Scope.IsNameScope();
}
