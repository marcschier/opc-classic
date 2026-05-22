//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace OpcClassic.Cpx;

/// <summary>
/// Schema entry for a type exposed by OPC Complex Data metadata.
/// </summary>
/// <remarks>
/// This is the AOT-clean managed form of an OPCBinary <c>TypeDescription</c>
/// entry. XML loading and NDR codecs are intentionally deferred to the
/// Phase 9B follow-up generator work.
/// </remarks>
public sealed record TypeDescription
{
    private readonly TypeField[] _fields;

    /// <summary>Create a type description.</summary>
    public TypeDescription(
        string name,
        string typeId,
        TypeKind type,
        bool isComplex,
        IEnumerable<TypeField>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A type description must have a non-empty name.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException("A type description must have a non-empty type identifier.", nameof(typeId));
        }

        if (type == TypeKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "A type description must declare a concrete type kind.");
        }

        Name = name;
        TypeId = typeId;
        Type = type;
        IsComplex = isComplex;
        _fields = fields?.ToArray() ?? Array.Empty<TypeField>();
        Fields = Array.AsReadOnly(_fields);
    }

    /// <summary>Human-readable schema entry name.</summary>
    public string Name { get; }

    /// <summary>Stable type identifier used by DA complex-data properties.</summary>
    public string TypeId { get; }

    /// <summary>Top-level OPCBinary type kind.</summary>
    public TypeKind Type { get; }

    /// <summary>True when the type is composed from child fields.</summary>
    public bool IsComplex { get; }

    /// <summary>Fields that make up the type, in schema order.</summary>
    public IReadOnlyList<TypeField> Fields { get; }

    /// <inheritdoc />
    public bool Equals(TypeDescription? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(Name, other.Name)
        && StringComparer.Ordinal.Equals(TypeId, other.TypeId)
        && Type == other.Type
        && IsComplex == other.IsComplex
        && _fields.SequenceEqual(other._fields);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(TypeId, StringComparer.Ordinal);
        hash.Add(Type);
        hash.Add(IsComplex);

        foreach (var field in _fields)
        {
            hash.Add(field);
        }

        return hash.ToHashCode();
    }
}
