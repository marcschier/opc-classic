//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix — OPCBinary "type dictionary" is the spec term

using System;
using System.Collections.Generic;
using System.Linq;

namespace Opc.Classic.Cpx;

/// <summary>
/// Managed in-memory representation of an OPCBinary type dictionary.
/// </summary>
/// <remarks>
/// OPC Complex Data servers identify a type system, dictionary, and type for
/// complex DA items. This wrapper keeps the dictionary metadata and its
/// <see cref="TypeDescription"/> entries AOT-clean while the CPX XML and
/// OPCBinary codecs parse and consume type-system dictionaries.
/// </remarks>
public sealed record TypeDictionary {
    /// <summary>The OPC DA type-system identifier for XML Schema dictionaries.</summary>
    public const string XmlSchemaTypeSystemId = "XMLSchema";

    /// <summary>The OPC DA type-system identifier for OPCBinary dictionaries.</summary>
    public const string OpcBinaryTypeSystemId = "OPCBinary";

    /// <summary>The OPCBinary XML namespace used by the type-dictionary schema.</summary>
    public const string OpcBinaryNamespace = "http://opcfoundation.org/OPCBinary/1.0/";

    /// <summary>The OPCBinary default string encoding.</summary>
    public const string DefaultOpcBinaryStringEncoding = "UCS-2";

    /// <summary>The OPCBinary default floating-point format.</summary>
    public const string DefaultOpcBinaryFloatFormat = "IEEE-754";

    private readonly TypeDescription[] _types;
    private readonly Dictionary<string, TypeDescription> _typesByName;
    private readonly Dictionary<string, TypeDescription> _typesById;

    /// <summary>Create an unnamed dictionary with the supplied types.</summary>
    public TypeDictionary(IEnumerable<TypeDescription> types)
        : this(string.Empty, types) {
    }

    /// <summary>Create a dictionary with the supplied metadata and types.</summary>
    public TypeDictionary(
        string name,
        IEnumerable<TypeDescription> types,
        bool defaultBigEndian = true,
        string defaultStringEncoding = DefaultOpcBinaryStringEncoding,
        int defaultCharWidth = 2,
        string defaultFloatFormat = DefaultOpcBinaryFloatFormat) {
        ArgumentNullException.ThrowIfNull(types);

        if (string.IsNullOrWhiteSpace(defaultStringEncoding)) {
            throw new ArgumentException("A type dictionary must have a non-empty default string encoding.", nameof(defaultStringEncoding));
        }

        if (defaultCharWidth <= 0) {
            throw new ArgumentOutOfRangeException(nameof(defaultCharWidth), defaultCharWidth, "Default character width must be positive.");
        }

        if (string.IsNullOrWhiteSpace(defaultFloatFormat)) {
            throw new ArgumentException("A type dictionary must have a non-empty default float format.", nameof(defaultFloatFormat));
        }

        Name = name ?? string.Empty;
        DefaultBigEndian = defaultBigEndian;
        DefaultStringEncoding = defaultStringEncoding;
        DefaultCharWidth = defaultCharWidth;
        DefaultFloatFormat = defaultFloatFormat;

        _types = types.ToArray();
        _typesByName = new Dictionary<string, TypeDescription>(StringComparer.Ordinal);
        _typesById = new Dictionary<string, TypeDescription>(StringComparer.Ordinal);

        foreach (var type in _types) {
            if (!_typesByName.TryAdd(type.Name, type)) {
                throw new ArgumentException($"Duplicate type description name '{type.Name}'.", nameof(types));
            }

            if (!_typesById.TryAdd(type.TypeId, type)) {
                throw new ArgumentException($"Duplicate type description identifier '{type.TypeId}'.", nameof(types));
            }
        }

        Types = Array.AsReadOnly(_types);
    }

    /// <summary>Dictionary name or URI.</summary>
    public string Name { get; }

    /// <summary>Default byte-order flag from the OPCBinary dictionary.</summary>
    public bool DefaultBigEndian { get; }

    /// <summary>Default string encoding for character fields.</summary>
    public string DefaultStringEncoding { get; }

    /// <summary>Default character width for character fields.</summary>
    public int DefaultCharWidth { get; }

    /// <summary>Default floating-point format.</summary>
    public string DefaultFloatFormat { get; }

    /// <summary>All type descriptions in dictionary order.</summary>
    public IReadOnlyList<TypeDescription> Types { get; }

    /// <summary>Create an unnamed dictionary from type descriptions.</summary>
    public static TypeDictionary FromTypes(params TypeDescription[] types) =>
        new(types);

    /// <summary>Look up a type description by name. Returns <see langword="null"/> when not found.</summary>
    public TypeDescription? TryGet(string name) {
        ArgumentNullException.ThrowIfNull(name);
        return _typesByName.TryGetValue(name, out var type) ? type : null;
    }

    /// <summary>Look up a type description by type identifier. Returns <see langword="null"/> when not found.</summary>
    public TypeDescription? TryGetByTypeId(string typeId) {
        ArgumentNullException.ThrowIfNull(typeId);
        return _typesById.TryGetValue(typeId, out var type) ? type : null;
    }

    /// <summary>True if this dictionary defines a type with <paramref name="name"/>.</summary>
    public bool Contains(string name) {
        ArgumentNullException.ThrowIfNull(name);
        return _typesByName.ContainsKey(name);
    }

    /// <inheritdoc />
    public bool Equals(TypeDictionary? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(Name, other.Name)
        && DefaultBigEndian == other.DefaultBigEndian
        && StringComparer.Ordinal.Equals(DefaultStringEncoding, other.DefaultStringEncoding)
        && DefaultCharWidth == other.DefaultCharWidth
        && StringComparer.Ordinal.Equals(DefaultFloatFormat, other.DefaultFloatFormat)
        && _types.SequenceEqual(other._types);

    /// <inheritdoc />
    public override int GetHashCode() {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(DefaultBigEndian);
        hash.Add(DefaultStringEncoding, StringComparer.Ordinal);
        hash.Add(DefaultCharWidth);
        hash.Add(DefaultFloatFormat, StringComparer.Ordinal);

        foreach (var type in _types) {
            hash.Add(type);
        }

        return hash.ToHashCode();
    }
}
