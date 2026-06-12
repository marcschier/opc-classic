//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Cpx.Hosting;

/// <summary>
/// <see cref="IOpcAddressSpace"/> decorator that exposes the reserved CPX type-dictionary subtree.
/// </summary>
public sealed class OpcCpxAddressSpace : IOpcAddressSpace
{
    private readonly IOpcAddressSpace _inner;
    private readonly OpcCpxOptions _options;

    /// <summary>
    /// Creates a CPX address-space decorator.
    /// </summary>
    public OpcCpxAddressSpace(IOpcAddressSpace inner, OpcCpxOptions options)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public bool IsHierarchical => true;

    /// <inheritdoc />
    public async Task<OpcBrowseResult> BrowseAsync(
        string? branchPath,
        OpcBrowseElementKind kind,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsRootPath(branchPath))
        {
            var innerResult = await _inner.BrowseAsync(branchPath, kind, cancellationToken).ConfigureAwait(false);
            return MergeRoot(innerResult, kind);
        }

        if (!TryParseCpxPath(branchPath, out var path))
        {
            return await _inner.BrowseAsync(branchPath, kind, cancellationToken).ConfigureAwait(false);
        }

        return BrowseCpx(path, kind);
    }

    /// <inheritdoc />
    public Task<string> GetItemIdAsync(
        string? currentBranchPath,
        string itemDataId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemDataId);
        cancellationToken.ThrowIfCancellationRequested();

        if (currentBranchPath is not null && TryParseCpxPath(currentBranchPath, out var path))
        {
            return Task.FromResult(GetCpxItemId(path, itemDataId));
        }

        if (IsRootPath(currentBranchPath)
            && itemDataId.Equals(CpxNamespaceBuilder.RootSegment, StringComparison.Ordinal))
        {
            return Task.FromResult(CpxNamespaceBuilder.RootPath);
        }

        return _inner.GetItemIdAsync(currentBranchPath, itemDataId, cancellationToken);
    }

    private OpcBrowseResult MergeRoot(OpcBrowseResult innerResult, OpcBrowseElementKind kind)
    {
        var branches = kind == OpcBrowseElementKind.Items
            ? Array.Empty<string>()
            : MergeDistinct(innerResult.Branches, HasDictionaries() ? CpxNamespaceBuilder.RootSegment : null);
        var items = kind == OpcBrowseElementKind.Branches ? Array.Empty<string>() : innerResult.Items;
        return new OpcBrowseResult(branches, items);
    }

    private OpcBrowseResult BrowseCpx(CpxPath path, OpcBrowseElementKind kind)
    {
        if (path.TypeSystemId is null)
        {
            return new OpcBrowseResult(GetBranches(kind, GetTypeSystemIds()), Array.Empty<string>());
        }

        if (path.DictionarySegment is null)
        {
            return new OpcBrowseResult(GetBranches(kind, GetDictionarySegments(path.TypeSystemId)), Array.Empty<string>());
        }

        if (path.TypeId is null)
        {
            if (!_options.TryGetDictionaryBySegment(path.TypeSystemId, path.DictionarySegment, out var registration))
            {
                return OpcBrowseResult.Empty;
            }

            return new OpcBrowseResult(Array.Empty<string>(), GetItems(kind, registration.Dictionary.Types.Select(static type => type.TypeId)));
        }

        return OpcBrowseResult.Empty;
    }

    private static string GetCpxItemId(CpxPath path, string itemDataId)
    {
        if (path.TypeSystemId is null)
        {
            return CpxNamespaceBuilder.BuildTypeSystemPath(itemDataId);
        }

        if (path.DictionarySegment is null)
        {
            return CpxNamespaceBuilder.BuildDictionaryPath(path.TypeSystemId, itemDataId);
        }

        return CpxNamespaceBuilder.BuildTypePath(path.TypeSystemId, path.DictionarySegment, itemDataId);
    }

    private IReadOnlyList<string> GetTypeSystemIds()
    {
        var values = new List<string>();
        foreach (var dictionary in _options.Dictionaries)
        {
            AddIfMissing(values, dictionary.TypeSystemId);
        }

        return values;
    }

    private IReadOnlyList<string> GetDictionarySegments(string typeSystemId)
    {
        var values = new List<string>();
        foreach (var dictionary in _options.Dictionaries)
        {
            if (StringComparer.Ordinal.Equals(dictionary.TypeSystemId, typeSystemId))
            {
                AddIfMissing(values, dictionary.DictionarySegment);
            }
        }

        return values;
    }

    private bool HasDictionaries() => _options.Dictionaries.Count > 0;

    private static IReadOnlyList<string> GetBranches(OpcBrowseElementKind kind, IReadOnlyList<string> values) =>
        kind == OpcBrowseElementKind.Items ? Array.Empty<string>() : values;

    private static IReadOnlyList<string> GetItems(OpcBrowseElementKind kind, IEnumerable<string> values) =>
        kind == OpcBrowseElementKind.Branches ? Array.Empty<string>() : values.ToArray();

    private static string[] MergeDistinct(IReadOnlyList<string> existing, string? additional)
    {
        var merged = new List<string>(existing);
        if (additional is not null)
        {
            AddIfMissing(merged, additional);
        }

        return merged.ToArray();
    }

    private static void AddIfMissing(List<string> values, string value)
    {
        foreach (var existing in values)
        {
            if (StringComparer.Ordinal.Equals(existing, value))
            {
                return;
            }
        }

        values.Add(value);
    }

    private bool TryParseCpxPath(string? branchPath, out CpxPath path)
    {
        path = default;
        if (IsRootPath(branchPath))
        {
            return false;
        }

        var trimmed = branchPath!.Trim().Trim('/', '\\');
        if (trimmed.Length == 0)
        {
            return false;
        }

        if (trimmed.Contains('/', StringComparison.Ordinal) || trimmed.Contains('\\', StringComparison.Ordinal))
        {
            return TryParseSlashPath(trimmed.Replace('\\', '/'), out path);
        }

        return TryParseDotPath(trimmed, out path);
    }

    private static bool TryParseSlashPath(string trimmed, out CpxPath path)
    {
        path = default;
        var segments = trimmed.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || !segments[0].Equals(CpxNamespaceBuilder.RootSegment, StringComparison.Ordinal))
        {
            return false;
        }

        path = new CpxPath(
            segments.Length > 1 ? segments[1] : null,
            segments.Length > 2 ? segments[2] : null,
            segments.Length > 3 ? string.Join('/', segments.AsSpan(3)) : null);
        return true;
    }

    private bool TryParseDotPath(string trimmed, out CpxPath path)
    {
        path = default;
        var segments = trimmed.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || !segments[0].Equals(CpxNamespaceBuilder.RootSegment, StringComparison.Ordinal))
        {
            return false;
        }

        if (segments.Length <= 2)
        {
            path = new CpxPath(segments.Length == 2 ? segments[1] : null, null, null);
            return true;
        }

        var typeSystemId = segments[1];
        for (var end = segments.Length; end >= 3; end--)
        {
            var dictionarySegment = string.Join('.', segments.AsSpan(2, end - 2));
            if (_options.TryGetDictionaryBySegment(typeSystemId, dictionarySegment, out _))
            {
                path = new CpxPath(
                    typeSystemId,
                    dictionarySegment,
                    end < segments.Length ? string.Join('.', segments.AsSpan(end)) : null);
                return true;
            }
        }

        path = new CpxPath(typeSystemId, string.Join('.', segments.AsSpan(2)), null);
        return true;
    }

    private static bool IsRootPath(string? branchPath) =>
        string.IsNullOrWhiteSpace(branchPath)
        || branchPath.Equals("/", StringComparison.Ordinal)
        || branchPath.Equals("\\", StringComparison.Ordinal);

    private readonly record struct CpxPath(string? TypeSystemId, string? DictionarySegment, string? TypeId);
}
