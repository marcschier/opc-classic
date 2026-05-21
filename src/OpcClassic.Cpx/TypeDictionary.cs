//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix — OPCBinary "type dictionary" is the spec term

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpcClassic.Cpx;

/// <summary>
/// Managed in-memory representation of an OPCBinary type dictionary —
/// the XSD-shaped document that OPC Complex Data servers expose as a string
/// via the item property <c>DictionaryItemId</c> (= 300).
/// </summary>
/// <remarks>
/// <para>
/// The dictionary is a collection of named <see cref="StructType"/>s, indexed
/// by name. A complex-data item's value is encoded according to one specific
/// type in this dictionary (selected by the item's
/// <c>TypeDescriptionItemId</c> = 301 property).
/// </para>
/// <para>
/// Loading the dictionary from XSD bytes is the responsibility of a future
/// <c>OpcBinaryReader</c> (Phase 9B continuation). This type is a plain
/// AOT-clean container — no XML parsing concerns leak into here.
/// </para>
/// </remarks>
public sealed class TypeDictionary
{
    private readonly Dictionary<string, StructType> _types;

    /// <summary>Construct with the given types.</summary>
    public TypeDictionary(IEnumerable<StructType> types)
    {
        ArgumentNullException.ThrowIfNull(types);
        _types = types.ToDictionary(
            t => t.Name,
            StringComparer.Ordinal);
    }

    /// <summary>Construct from an existing dictionary (copy by reference).</summary>
    public static TypeDictionary FromTypes(params StructType[] types) =>
        new(types);

    /// <summary>The dictionary's default struct, if one is marked <see cref="StructType.IsDefault"/>.</summary>
    public StructType? Default => _types.Values.FirstOrDefault(t => t.IsDefault);

    /// <summary>All struct types in the dictionary, in insertion order.</summary>
    public IReadOnlyCollection<StructType> Types => _types.Values;

    /// <summary>Look up a struct by name. Returns <see langword="null"/> when not found.</summary>
    public StructType? TryGet(string name) =>
        _types.TryGetValue(name, out var t) ? t : null;

    /// <summary>True if this dictionary defines a struct named <paramref name="name"/>.</summary>
    public bool Contains(string name) => _types.ContainsKey(name);
}
