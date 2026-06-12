//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.ObjectModel;

namespace Opc.Classic.Cpx;

/// <summary>
/// Managed description of an OPC Complex Data item instance.
/// </summary>
/// <remarks>
/// OPC DA exposes complex metadata via item properties such as
/// <c>typeSystemID</c>, <c>dictionaryID</c>, and <c>typeID</c>. This record
/// gathers those identifiers with field values decoded by a CPX type-system codec.
/// </remarks>
public sealed record InstanceDescription
{
    private readonly Dictionary<string, object?> _fieldValues;

    /// <summary>Create an item instance description.</summary>
    public InstanceDescription(
        string itemId,
        string typeId,
        bool isComplex,
        IReadOnlyDictionary<string, object?>? fieldValues = null,
        string? dictionaryId = null,
        string typeSystemId = TypeDictionary.OpcBinaryTypeSystemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("An instance description must have a non-empty item identifier.", nameof(itemId));
        }

        if (string.IsNullOrWhiteSpace(typeId))
        {
            throw new ArgumentException("An instance description must have a non-empty type identifier.", nameof(typeId));
        }

        if (string.IsNullOrWhiteSpace(typeSystemId))
        {
            throw new ArgumentException("An instance description must have a non-empty type system identifier.", nameof(typeSystemId));
        }

        ItemId = itemId;
        TypeId = typeId;
        IsComplex = isComplex;
        DictionaryId = string.IsNullOrWhiteSpace(dictionaryId) ? null : dictionaryId;
        TypeSystemId = typeSystemId;
        _fieldValues = CopyFieldValues(fieldValues);
        FieldValues = new ReadOnlyDictionary<string, object?>(_fieldValues);
    }

    /// <summary>OPC item identifier for this complex item instance.</summary>
    public string ItemId { get; }

    /// <summary>Type identifier selected for this instance.</summary>
    public string TypeId { get; }

    /// <summary>Type-system identifier, usually <c>OPCBinary</c>.</summary>
    public string TypeSystemId { get; }

    /// <summary>Dictionary identifier that supplied <see cref="TypeId"/>, if known.</summary>
    public string? DictionaryId { get; }

    /// <summary>True when this instance represents a structured complex value.</summary>
    public bool IsComplex { get; }

    /// <summary>Decoded field values, keyed by type-description field name.</summary>
    public IReadOnlyDictionary<string, object?> FieldValues { get; }

    /// <summary>Get a decoded field value by name.</summary>
    public object? this[string fieldName] => FieldValues[fieldName];

    /// <summary>Try to read a field value with a strongly typed cast.</summary>
    public bool TryGet<T>(string fieldName, out T value)
    {
        if (FieldValues.TryGetValue(fieldName, out var raw) && raw is T typed)
        {
            value = typed;
            return true;
        }

        value = default!;
        return false;
    }

    /// <inheritdoc />
    public bool Equals(InstanceDescription? other) =>
        other is not null
        && StringComparer.Ordinal.Equals(ItemId, other.ItemId)
        && StringComparer.Ordinal.Equals(TypeId, other.TypeId)
        && StringComparer.Ordinal.Equals(TypeSystemId, other.TypeSystemId)
        && StringComparer.Ordinal.Equals(DictionaryId, other.DictionaryId)
        && IsComplex == other.IsComplex
        && FieldValuesEqual(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(ItemId, StringComparer.Ordinal);
        hash.Add(TypeId, StringComparer.Ordinal);
        hash.Add(TypeSystemId, StringComparer.Ordinal);
        hash.Add(DictionaryId, StringComparer.Ordinal);
        hash.Add(IsComplex);

        foreach (var pair in _fieldValues.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            hash.Add(pair.Key, StringComparer.Ordinal);
            hash.Add(pair.Value);
        }

        return hash.ToHashCode();
    }

    private bool FieldValuesEqual(InstanceDescription other)
    {
        if (_fieldValues.Count != other._fieldValues.Count)
        {
            return false;
        }

        foreach (var pair in _fieldValues)
        {
            if (!other._fieldValues.TryGetValue(pair.Key, out var otherValue)
                || !EqualityComparer<object?>.Default.Equals(pair.Value, otherValue))
            {
                return false;
            }
        }

        return true;
    }

    private static Dictionary<string, object?> CopyFieldValues(IReadOnlyDictionary<string, object?>? fieldValues)
    {
        var copy = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (fieldValues is null)
        {
            return copy;
        }

        foreach (var pair in fieldValues)
        {
            if (string.IsNullOrWhiteSpace(pair.Key))
            {
                throw new ArgumentException("Field value keys must be non-empty field names.", nameof(fieldValues));
            }

            copy.Add(pair.Key, pair.Value);
        }

        return copy;
    }
}
