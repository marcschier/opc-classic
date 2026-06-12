//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;

namespace Opc.Classic.Cpx;

/// <summary>
/// Builds OPC Complex Data namespace paths for type discovery, conversion, and data filters.
/// </summary>
public static class CpxNamespaceBuilder
{
    /// <summary>Reserved CPX branch name.</summary>
    public const string RootSegment = "CPX";

    /// <summary>Reserved data-filter branch name.</summary>
    public const string DataFiltersSegment = "DataFilters";

    /// <summary>Root path for CPX type dictionaries.</summary>
    public const string RootPath = "/CPX";

    /// <summary>Build <c>/CPX/{TypeSystem}</c>.</summary>
    public static string BuildTypeSystemPath(string typeSystemId) =>
        CombineAbsolute(RootSegment, RequireSegment(typeSystemId, nameof(typeSystemId)));

    /// <summary>Build <c>/CPX/{TypeSystem}/{Dictionary}</c>.</summary>
    public static string BuildDictionaryPath(string typeSystemId, string dictionary) =>
        CombineAbsolute(
            RootSegment,
            RequireSegment(typeSystemId, nameof(typeSystemId)),
            RequireSegment(dictionary, nameof(dictionary)));

    /// <summary>Build <c>/CPX/{TypeSystem}/{Dictionary}/{TypeID}</c>.</summary>
    public static string BuildTypePath(string typeSystemId, string dictionary, string typeId) =>
        CombineAbsolute(
            RootSegment,
            RequireSegment(typeSystemId, nameof(typeSystemId)),
            RequireSegment(dictionary, nameof(dictionary)),
            RequirePathSuffix(typeId, nameof(typeId)));

    /// <summary>Build <c>{ItemID}/CPX/{Format}</c> for type conversions.</summary>
    public static string BuildConversionPath(string itemId, string format) =>
        CombineItemPath(
            RequireItemId(itemId, nameof(itemId)),
            RootSegment,
            RequireSegment(format, nameof(format)));

    /// <summary>Build <c>{ItemID}/CPX/{Format}/DataFilters</c>.</summary>
    public static string BuildDataFiltersPath(string itemId, string format) =>
        CombineItemPath(
            RequireItemId(itemId, nameof(itemId)),
            RootSegment,
            RequireSegment(format, nameof(format)),
            DataFiltersSegment);

    /// <summary>Build <c>{ItemID}/CPX/{Format}/DataFilters/{FilterName}</c>.</summary>
    public static string BuildDataFilterPath(string itemId, string format, string filterName) =>
        CombineItemPath(
            RequireItemId(itemId, nameof(itemId)),
            RootSegment,
            RequireSegment(format, nameof(format)),
            DataFiltersSegment,
            RequireSegment(filterName, nameof(filterName)));

    /// <summary>Build a stable dictionary branch name from a dictionary identifier.</summary>
    public static string GetDictionarySegment(string dictionaryId)
    {
        var normalized = RequirePathSuffix(dictionaryId, nameof(dictionaryId));
        var lastSlash = normalized.LastIndexOf('/');
        if (lastSlash >= 0 && lastSlash < normalized.Length - 1)
        {
            return normalized[(lastSlash + 1)..];
        }

        var lastBackslash = normalized.LastIndexOf('\\');
        if (lastBackslash >= 0 && lastBackslash < normalized.Length - 1)
        {
            return normalized[(lastBackslash + 1)..];
        }

        var lastColon = normalized.LastIndexOf(':');
        if (lastColon >= 0 && lastColon < normalized.Length - 1)
        {
            return normalized[(lastColon + 1)..];
        }

        return normalized;
    }

    private static string CombineAbsolute(params string[] segments) =>
        string.Create(CultureInfo.InvariantCulture, $"/{string.Join('/', segments)}");

    private static string CombineItemPath(string itemId, params string[] segments) =>
        string.Create(CultureInfo.InvariantCulture, $"{itemId.TrimEnd('/', '\\')}/{string.Join('/', segments)}");

    private static string RequireSegment(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var trimmed = value.Trim('/', '\\');
        if (trimmed.Length == 0 || trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
        {
            throw new ArgumentException("A CPX namespace segment cannot contain path separators.", parameterName);
        }

        return trimmed;
    }

    private static string RequirePathSuffix(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var trimmed = value.Trim('/', '\\');
        if (trimmed.Length == 0)
        {
            throw new ArgumentException("A CPX namespace path suffix cannot be empty.", parameterName);
        }

        return trimmed;
    }

    private static string RequireItemId(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        return value.TrimEnd('/', '\\');
    }
}
