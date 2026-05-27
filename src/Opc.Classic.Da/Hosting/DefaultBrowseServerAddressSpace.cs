//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Default managed implementation of <see cref="IOPCBrowseServerAddressSpace"/>
/// (DA 2.x). Returns <c>OPC_E_NOTSUPPORTED</c> for browse operations so a
/// server is conformant to "interface exists, behaviour declines" rather than
/// the previous "interface absent" state. Real address-space browsing is a
/// separate feature.
/// </summary>
public sealed class DefaultBrowseServerAddressSpace : IOPCBrowseServerAddressSpace
{
    /// <summary>Returns <c>OPCNS_FLAT</c> (1).</summary>
    public Task<int> QueryOrganizationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(1); // OPCNS_FLAT
    }

    public Task ChangeBrowsePositionAsync(int browseDirection, string browsePosition, CancellationToken cancellationToken = default)
    {
        _ = browseDirection; _ = browsePosition;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotSupported);
    }

    public Task<IOpcInterfaceRef> BrowseOpcItemIdsAsync(
        int browseFilterType,
        string filterCriteria,
        ushort dataTypeFilter,
        int accessRightsFilter,
        CancellationToken cancellationToken = default)
    {
        _ = browseFilterType; _ = filterCriteria; _ = dataTypeFilter; _ = accessRightsFilter;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotSupported);
    }

    public Task<string> GetItemIdAsync(string itemDataId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemDataId);
        cancellationToken.ThrowIfCancellationRequested();
        // For flat namespaces the item ID equals the browse data ID per OPC DA 2.05a §4.6.4.
        return Task.FromResult(itemDataId);
    }

    public Task<IOpcInterfaceRef> BrowseAccessPathsAsync(string itemId, CancellationToken cancellationToken = default)
    {
        _ = itemId;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotSupported);
    }
}
