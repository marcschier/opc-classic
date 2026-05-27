//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default managed implementation of <see cref="IOPCBrowse"/> (DA 3.0
/// unified browse). Returns empty browse results so the interface is
/// reachable on the wire without exposing a real address-space.
/// </summary>
public sealed class DefaultBrowse : IOPCBrowse
{
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
        _ = itemId; _ = maxElementsReturned; _ = browseFilter; _ = elementNameFilter;
        _ = vendorFilter; _ = returnAllProperties; _ = returnPropertyValues; _ = propertyIds;
        cancellationToken.ThrowIfCancellationRequested();
        continuationPoint = null;
        moreElements = false;
        browseElements = Array.Empty<OpcBrowseElementResult>();
        return Task.CompletedTask;
    }
}
