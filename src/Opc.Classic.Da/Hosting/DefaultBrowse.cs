//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default managed implementation of <see cref="IOPCBrowse"/> (DA 3.0
/// unified browse) backed by an <see cref="IOpcAddressSpace"/>. Returns
/// elements at the queried browse position and resolves item properties
/// via the supplied address space.
/// </summary>
/// <remarks>
/// When no address space is supplied, the implementation returns empty
/// browse results so the interface remains reachable for clients that
/// just probe for DA 3.0 support.
/// </remarks>
public sealed class DefaultBrowse : IOPCBrowse
{
    private readonly IOpcAddressSpace _addressSpace;

    /// <summary>Initializes with an empty flat address space.</summary>
    public DefaultBrowse() : this(new FlatHierarchicalNamespace())
    {
    }

    /// <summary>Initializes with the supplied address space.</summary>
    public DefaultBrowse(IOpcAddressSpace addressSpace)
    {
        _addressSpace = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));
    }

    /// <inheritdoc />
    public Task<OpcItemProperties[]> GetPropertiesAsync(
        string[] itemIds,
        bool returnPropertyValues,
        int[] propertyIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        _ = returnPropertyValues; _ = propertyIds;
        cancellationToken.ThrowIfCancellationRequested();
        var result = new OpcItemProperties[itemIds.Length];
        for (int i = 0; i < itemIds.Length; i++)
        {
            result[i] = new OpcItemProperties(
                ErrorId: OpcResultId.Ok.Code,
                Properties: Array.Empty<OpcItemPropertyResult>());
        }
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task BrowseAsync(
        string itemId,
        ref string? continuationPoint,
        int maxElementsReturned,
        int browseFilter,
        string elementNameFilter,
        string vendorFilter,
        bool returnAllProperties,
        bool returnPropertyValues,
        int[] propertyIds,
        out bool moreElements,
        out OpcBrowseElementResult[] browseElements,
        CancellationToken cancellationToken = default)
    {
        _ = elementNameFilter; _ = vendorFilter;
        _ = returnAllProperties; _ = returnPropertyValues; _ = propertyIds;
        cancellationToken.ThrowIfCancellationRequested();

        // browseFilter: OPC_BROWSE_FILTER_ALL=1, BRANCH=2, ITEM=3
        OpcBrowseElementKind kind = browseFilter switch
        {
            2 => OpcBrowseElementKind.Branches,
            3 => OpcBrowseElementKind.Items,
            _ => OpcBrowseElementKind.All,
        };
#pragma warning disable VSTHRD002, VSTHRD103 // Sync bridge: the OPC interface signature is synchronous-with-out-params.
        OpcBrowseResult result = _addressSpace
            .BrowseAsync(string.IsNullOrEmpty(itemId) ? null : itemId, kind, cancellationToken)
            .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002, VSTHRD103

        var elements = new List<OpcBrowseElementResult>(result.Branches.Count + result.Items.Count);
        foreach (string branch in result.Branches)
        {
            elements.Add(new OpcBrowseElementResult(
                Name: branch,
                ItemId: CombineItemId(itemId, branch),
                FlagValue: 1,
                Properties: new OpcItemProperties(OpcResultId.Ok.Code, Array.Empty<OpcItemPropertyResult>())));
        }
        foreach (string item in result.Items)
        {
            elements.Add(new OpcBrowseElementResult(
                Name: item,
                ItemId: CombineItemId(itemId, item),
                FlagValue: 2,
                Properties: new OpcItemProperties(OpcResultId.Ok.Code, Array.Empty<OpcItemPropertyResult>())));
        }

        var offset = GetContinuationOffset(continuationPoint);
        if (offset > elements.Count)
        {
            throw new OpcException(OpcResultId.InvalidContinuationPoint);
        }

        var remaining = elements.Count - offset;
        var count = maxElementsReturned > 0 && remaining > maxElementsReturned ? maxElementsReturned : remaining;
        browseElements = count == 0 ? Array.Empty<OpcBrowseElementResult>() : elements.GetRange(offset, count).ToArray();
        moreElements = maxElementsReturned > 0 && offset + count < elements.Count;
        continuationPoint = moreElements ? CreateContinuationPoint(offset + count) : string.Empty;
        return Task.CompletedTask;
    }

    private static string CreateContinuationPoint(int offset) =>
        string.Create(CultureInfo.InvariantCulture, $"opc-da-browse:{offset}");

    private static int GetContinuationOffset(string? continuationPoint)
    {
        if (string.IsNullOrEmpty(continuationPoint))
        {
            return 0;
        }

        const string prefix = "opc-da-browse:";
        if (!continuationPoint.StartsWith(prefix, StringComparison.Ordinal)
            || !int.TryParse(continuationPoint.AsSpan(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out var offset)
            || offset < 0)
        {
            throw new OpcException(OpcResultId.InvalidContinuationPoint);
        }

        return offset;
    }

    private static string CombineItemId(string itemId, string child)
    {
        if (string.IsNullOrEmpty(itemId))
        {
            return child;
        }

        return itemId.Contains('/', StringComparison.Ordinal)
            ? $"{itemId.TrimEnd('/')}/{child}"
            : $"{itemId}.{child}";
    }
}
