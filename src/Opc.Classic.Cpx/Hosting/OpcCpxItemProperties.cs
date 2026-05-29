//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Cpx.Hosting;

/// <summary>
/// Publishes OPC Complex Data item properties 600-609 for registered complex DA items.
/// </summary>
public sealed class OpcCpxItemProperties : IOpcItemPropertyProvider, IOpcItemPropertyMetadataProvider
{
    private static readonly OpcStandardProperty[] CpxPropertyDescriptors =
    [
        new(OpcComplexDataProperty.TypeSystemId, VarType.VT_BSTR, "Type System ID"),
        new(OpcComplexDataProperty.DictionaryId, VarType.VT_BSTR, "Dictionary ID"),
        new(OpcComplexDataProperty.TypeId, VarType.VT_BSTR, "Type ID"),
        new(OpcComplexDataProperty.Dictionary, VarType.VT_BSTR, "Dictionary"),
        new(OpcComplexDataProperty.TypeDescription, VarType.VT_BSTR, "Type Description"),
        new(OpcComplexDataProperty.ConsistencyWindow, VarType.VT_BSTR, "Consistency Window"),
        new(OpcComplexDataProperty.WriteBehavior, VarType.VT_BSTR, "Write Behavior"),
        new(OpcComplexDataProperty.UnconvertedItemId, VarType.VT_BSTR, "Unconverted Item ID"),
        new(OpcComplexDataProperty.UnfilteredItemId, VarType.VT_BSTR, "Unfiltered Item ID"),
        new(OpcComplexDataProperty.DataFilterValue, VarType.VT_BSTR, "Data Filter Value"),
    ];

    private readonly OpcCpxOptions _options;
    private readonly IOpcItemPropertyProvider _fallbackProvider;
    private readonly IOpcItemPropertyMetadataProvider? _fallbackMetadataProvider;

    /// <summary>Creates a CPX property provider.</summary>
    public OpcCpxItemProperties(OpcCpxOptions options)
        : this(options, NullItemPropertyProvider.Instance)
    {
    }

    /// <summary>Creates a CPX property provider with a fallback provider for non-CPX properties.</summary>
    public OpcCpxItemProperties(OpcCpxOptions options, IOpcItemPropertyProvider fallbackProvider)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _fallbackProvider = fallbackProvider ?? throw new ArgumentNullException(nameof(fallbackProvider));
        _fallbackMetadataProvider = fallbackProvider as IOpcItemPropertyMetadataProvider;
    }

    /// <summary>Descriptors for CPX properties 600-609.</summary>
    public static IReadOnlyList<OpcStandardProperty> Properties => CpxPropertyDescriptors;

    /// <inheritdoc />
    public (OpcVariant Value, int Error) TryGetPropertyValue(string itemId, int propertyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        if (TryGetCpxPropertyValue(itemId, propertyId, out var value, out var error))
        {
            return (value, error);
        }

        return _fallbackProvider.TryGetPropertyValue(itemId, propertyId);
    }

    /// <inheritdoc />
    public IReadOnlyList<OpcStandardProperty> GetAvailableProperties(string itemId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        var properties = new List<OpcStandardProperty>();
        if (_fallbackMetadataProvider is not null)
        {
            properties.AddRange(_fallbackMetadataProvider.GetAvailableProperties(itemId));
        }

        foreach (var descriptor in CpxPropertyDescriptors)
        {
            if (TryGetCpxPropertyValue(itemId, descriptor.Id, out _, out var error)
                && error == OpcResultId.Ok.Code)
            {
                AddIfMissing(properties, descriptor);
            }
        }

        return properties;
    }

    /// <inheritdoc />
    public (string ItemId, int Error) TryGetPropertyItemId(string itemId, int propertyId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        if (_options.TryGetComplexItem(itemId, out var complexItem))
        {
            return propertyId switch
            {
                OpcComplexDataProperty.DictionaryId => OkItemId(GetDictionaryItemId(complexItem.TypeSystemId, complexItem.DictionaryId)),
                OpcComplexDataProperty.TypeId => OkItemId(GetTypeItemId(complexItem.TypeSystemId, complexItem.DictionaryId, complexItem.TypeId)),
                OpcComplexDataProperty.UnconvertedItemId when complexItem.UnconvertedItemId is not null => OkItemId(complexItem.UnconvertedItemId),
                OpcComplexDataProperty.UnfilteredItemId when complexItem.UnfilteredItemId is not null => OkItemId(complexItem.UnfilteredItemId),
                _ => InvalidItemId(),
            };
        }

        return _fallbackMetadataProvider?.TryGetPropertyItemId(itemId, propertyId) ?? InvalidItemId();
    }

    private bool TryGetCpxPropertyValue(string itemId, int propertyId, out OpcVariant value, out int error)
    {
        if (_options.TryGetComplexItem(itemId, out var complexItem))
        {
            return TryGetComplexItemProperty(complexItem, propertyId, out value, out error);
        }

        if (TryGetDictionaryItem(itemId, out var dictionary))
        {
            return TryGetDictionaryItemProperty(dictionary, propertyId, out value, out error);
        }

        if (TryGetTypeItem(itemId, out dictionary, out var typeId))
        {
            return TryGetTypeItemProperty(dictionary, typeId, propertyId, out value, out error);
        }

        value = OpcVariant.Empty;
        error = OpcResultId.InvalidPid.Code;
        return false;
    }

    private bool TryGetComplexItemProperty(
        OpcCpxOptions.ComplexItemRegistration item,
        int propertyId,
        out OpcVariant value,
        out int error)
    {
        switch (propertyId)
        {
            case OpcComplexDataProperty.TypeSystemId:
                return OkString(item.TypeSystemId, out value, out error);
            case OpcComplexDataProperty.DictionaryId:
                return OkString(item.DictionaryId, out value, out error);
            case OpcComplexDataProperty.TypeId:
                return OkString(item.TypeId, out value, out error);
            case OpcComplexDataProperty.Dictionary:
                return TryGetDictionaryValue(item.TypeSystemId, item.DictionaryId, out value, out error);
            case OpcComplexDataProperty.TypeDescription:
                return TryGetTypeDescriptionValue(item.TypeSystemId, item.DictionaryId, item.TypeId, out value, out error);
            case OpcComplexDataProperty.ConsistencyWindow when item.ConsistencyWindow is not null:
                return OkString(item.ConsistencyWindow, out value, out error);
            case OpcComplexDataProperty.WriteBehavior when item.WriteBehavior is not null:
                return OkString(item.WriteBehavior, out value, out error);
            case OpcComplexDataProperty.UnconvertedItemId when item.UnconvertedItemId is not null:
                return OkString(item.UnconvertedItemId, out value, out error);
            case OpcComplexDataProperty.UnfilteredItemId when item.UnfilteredItemId is not null:
                return OkString(item.UnfilteredItemId, out value, out error);
            case OpcComplexDataProperty.DataFilterValue when item.DataFilterValue is not null:
                return OkString(item.DataFilterValue, out value, out error);
            default:
                value = OpcVariant.Empty;
                error = OpcResultId.InvalidPid.Code;
                return true;
        }
    }

    private static bool TryGetDictionaryItemProperty(
        OpcCpxOptions.DictionaryRegistration dictionary,
        int propertyId,
        out OpcVariant value,
        out int error)
    {
        switch (propertyId)
        {
            case OpcComplexDataProperty.TypeSystemId:
                return OkString(dictionary.TypeSystemId, out value, out error);
            case OpcComplexDataProperty.DictionaryId:
                return OkString(dictionary.DictionaryId, out value, out error);
            case OpcComplexDataProperty.Dictionary when dictionary.DictionaryValue is not null:
                return OkString(dictionary.DictionaryValue, out value, out error);
            default:
                value = OpcVariant.Empty;
                error = OpcResultId.InvalidPid.Code;
                return true;
        }
    }

    private static bool TryGetTypeItemProperty(
        OpcCpxOptions.DictionaryRegistration dictionary,
        string typeId,
        int propertyId,
        out OpcVariant value,
        out int error)
    {
        switch (propertyId)
        {
            case OpcComplexDataProperty.TypeSystemId:
                return OkString(dictionary.TypeSystemId, out value, out error);
            case OpcComplexDataProperty.DictionaryId:
                return OkString(dictionary.DictionaryId, out value, out error);
            case OpcComplexDataProperty.TypeId:
                return OkString(typeId, out value, out error);
            case OpcComplexDataProperty.TypeDescription when dictionary.TryGetTypeDescriptionValue(typeId, out var typeDescription):
                return OkString(typeDescription, out value, out error);
            default:
                value = OpcVariant.Empty;
                error = OpcResultId.InvalidPid.Code;
                return true;
        }
    }

    private bool TryGetDictionaryValue(string typeSystemId, string dictionaryId, out OpcVariant value, out int error)
    {
        if (_options.TryGetDictionary(typeSystemId, dictionaryId, out var dictionary)
            && dictionary.DictionaryValue is not null)
        {
            return OkString(dictionary.DictionaryValue, out value, out error);
        }

        value = OpcVariant.Empty;
        error = OpcResultId.InvalidPid.Code;
        return true;
    }

    private bool TryGetTypeDescriptionValue(string typeSystemId, string dictionaryId, string typeId, out OpcVariant value, out int error)
    {
        if (_options.TryGetDictionary(typeSystemId, dictionaryId, out var dictionary)
            && dictionary.TryGetTypeDescriptionValue(typeId, out var typeDescription))
        {
            return OkString(typeDescription, out value, out error);
        }

        value = OpcVariant.Empty;
        error = OpcResultId.InvalidPid.Code;
        return true;
    }

    private bool TryGetDictionaryItem(string itemId, out OpcCpxOptions.DictionaryRegistration dictionary)
    {
        foreach (var candidate in _options.Dictionaries)
        {
            if (PathsEqual(itemId, CpxNamespaceBuilder.BuildDictionaryPath(candidate.TypeSystemId, candidate.DictionarySegment)))
            {
                dictionary = candidate;
                return true;
            }
        }

        dictionary = null!;
        return false;
    }

    private bool TryGetTypeItem(string itemId, out OpcCpxOptions.DictionaryRegistration dictionary, out string typeId)
    {
        foreach (var candidate in _options.Dictionaries)
        {
            foreach (var type in candidate.Dictionary.Types)
            {
                if (PathsEqual(itemId, CpxNamespaceBuilder.BuildTypePath(candidate.TypeSystemId, candidate.DictionarySegment, type.TypeId)))
                {
                    dictionary = candidate;
                    typeId = type.TypeId;
                    return true;
                }
            }
        }

        dictionary = null!;
        typeId = string.Empty;
        return false;
    }

    private string GetDictionaryItemId(string typeSystemId, string dictionaryId) =>
        CpxNamespaceBuilder.BuildDictionaryPath(typeSystemId, GetDictionarySegment(typeSystemId, dictionaryId));

    private string GetTypeItemId(string typeSystemId, string dictionaryId, string typeId) =>
        CpxNamespaceBuilder.BuildTypePath(typeSystemId, GetDictionarySegment(typeSystemId, dictionaryId), typeId);

    private string GetDictionarySegment(string typeSystemId, string dictionaryId) =>
        _options.TryGetDictionary(typeSystemId, dictionaryId, out var dictionary)
            ? dictionary.DictionarySegment
            : CpxNamespaceBuilder.GetDictionarySegment(dictionaryId);

    private static bool OkString(string text, out OpcVariant value, out int error)
    {
        value = OpcVariant.FromString(text);
        error = OpcResultId.Ok.Code;
        return true;
    }

    private static (string ItemId, int Error) OkItemId(string itemId) => (itemId, OpcResultId.Ok.Code);

    private static (string ItemId, int Error) InvalidItemId() => (string.Empty, OpcResultId.InvalidPid.Code);

    private static void AddIfMissing(List<OpcStandardProperty> properties, OpcStandardProperty property)
    {
        foreach (var existing in properties)
        {
            if (existing.Id == property.Id)
            {
                return;
            }
        }

        properties.Add(property);
    }

    private static bool PathsEqual(string left, string right) =>
        NormalizePath(left).Equals(NormalizePath(right), StringComparison.Ordinal);

    private static string NormalizePath(string value) => value.Trim().Replace('\\', '/').Trim('/');
}
