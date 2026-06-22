// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// Schema entry for a type exposed by OPC Complex Data metadata.
/// </summary>
/// <remarks>
/// This is the AOT-clean managed form of an OPCBinary <c>TypeDescription</c>
/// entry or an XML Schema element/complexType entry.
/// </remarks>
public sealed record TypeDescription
{
    private readonly TypeField[] _fields;

    /// <summary>
    /// Create a type description.
    /// </summary>
    public TypeDescription(
        string name,
        string typeId,
        TypeKind type,
        bool isComplex,
        IEnumerable<TypeField>? fields = null,
        bool? defaultBigEndian = null,
        string? defaultStringEncoding = null,
        int? defaultCharWidth = null,
        string? defaultFloatFormat = null)
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

        if (defaultStringEncoding is not null && string.IsNullOrWhiteSpace(defaultStringEncoding))
        {
            throw new ArgumentException("Default string encoding must be non-empty when specified.", nameof(defaultStringEncoding));
        }

        if (defaultCharWidth is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(defaultCharWidth), defaultCharWidth, "Default character width must be positive.");
        }

        if (defaultFloatFormat is not null && string.IsNullOrWhiteSpace(defaultFloatFormat))
        {
            throw new ArgumentException("Default float format must be non-empty when specified.", nameof(defaultFloatFormat));
        }

        Name = name;
        TypeId = typeId;
        Type = type;
        IsComplex = isComplex;
        DefaultBigEndian = defaultBigEndian;
        DefaultStringEncoding = defaultStringEncoding;
        DefaultCharWidth = defaultCharWidth;
        DefaultFloatFormat = defaultFloatFormat;
        _fields = fields?.ToArray() ?? Array.Empty<TypeField>();
        Fields = Array.AsReadOnly(_fields);
    }

    /// <summary>
    /// Human-readable schema entry name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Stable type identifier used by DA complex-data properties.
    /// </summary>
    public string TypeId { get; }

    /// <summary>
    /// Top-level OPCBinary type kind.
    /// </summary>
    public TypeKind Type { get; }

    /// <summary>
    /// True when the type is composed from child fields.
    /// </summary>
    public bool IsComplex { get; }

    /// <summary>
    /// Optional byte-order override inherited by child fields.
    /// </summary>
    public bool? DefaultBigEndian { get; }

    /// <summary>
    /// Optional string-encoding override inherited by child fields.
    /// </summary>
    public string? DefaultStringEncoding { get; }

    /// <summary>
    /// Optional character-width override inherited by child fields.
    /// </summary>
    public int? DefaultCharWidth { get; }

    /// <summary>
    /// Optional floating-point format override inherited by child fields.
    /// </summary>
    public string? DefaultFloatFormat { get; }

    /// <summary>
    /// Fields that make up the type, in schema order.
    /// </summary>
    public IReadOnlyList<TypeField> Fields { get; }

    /// <inheritdoc />
    public bool Equals(TypeDescription? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(Name, other.Name)
        && StringComparer.Ordinal.Equals(TypeId, other.TypeId)
        && Type == other.Type
        && IsComplex == other.IsComplex
        && DefaultBigEndian == other.DefaultBigEndian
        && StringComparer.Ordinal.Equals(DefaultStringEncoding, other.DefaultStringEncoding)
        && DefaultCharWidth == other.DefaultCharWidth
        && StringComparer.Ordinal.Equals(DefaultFloatFormat, other.DefaultFloatFormat)
        && _fields.SequenceEqual(other._fields);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name, StringComparer.Ordinal);
        hash.Add(TypeId, StringComparer.Ordinal);
        hash.Add(Type);
        hash.Add(IsComplex);
        hash.Add(DefaultBigEndian);
        hash.Add(DefaultStringEncoding, StringComparer.Ordinal);
        hash.Add(DefaultCharWidth);
        hash.Add(DefaultFloatFormat, StringComparer.Ordinal);

        foreach (var field in _fields)
        {
            hash.Add(field);
        }

        return hash.ToHashCode();
    }
}
