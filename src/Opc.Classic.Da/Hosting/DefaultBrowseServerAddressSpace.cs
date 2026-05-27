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
/// Managed implementation of <see cref="IOPCBrowseServerAddressSpace"/>
/// (DA 2.x) backed by an <see cref="IOpcAddressSpace"/> instance. Tracks
/// the current browse position per-server and resolves item IDs via the
/// supplied address space.
/// </summary>
/// <remarks>
/// When no address space is supplied, the implementation reports OPCNS_FLAT
/// and returns no items, preserving the previous "conformant but empty"
/// behaviour. Server authors that want real browse should construct
/// <see cref="DefaultBrowseServerAddressSpace"/> with an
/// <see cref="InMemoryAddressSpace"/> or a custom <see cref="IOpcAddressSpace"/>.
/// </remarks>
public sealed class DefaultBrowseServerAddressSpace : IOPCBrowseServerAddressSpace
{
    private readonly IOpcAddressSpace _addressSpace;
    private readonly Lock _lock = new();
    private string _browsePosition = string.Empty;

    /// <summary>Initializes with an empty flat address space.</summary>
    public DefaultBrowseServerAddressSpace()
        : this(new FlatHierarchicalNamespace())
    {
    }

    /// <summary>Initializes with the supplied address space.</summary>
    public DefaultBrowseServerAddressSpace(IOpcAddressSpace addressSpace)
    {
        _addressSpace = addressSpace ?? throw new ArgumentNullException(nameof(addressSpace));
    }

    /// <summary>Test helper: the current browse position.</summary>
    public string CurrentBrowsePosition
    {
        get
        {
            lock (_lock)
            {
                return _browsePosition;
            }
        }
    }

    /// <inheritdoc />
    public Task<int> QueryOrganizationAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_addressSpace.IsHierarchical ? 2 : 1);
    }

    /// <inheritdoc />
    public Task ChangeBrowsePositionAsync(int browseDirection, string browsePosition, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_addressSpace.IsHierarchical)
        {
            throw new OpcException(OpcResultId.NotSupported);
        }
        lock (_lock)
        {
            switch (browseDirection)
            {
                case 0: // OPC_BROWSE_UP
                    _browsePosition = MoveUp(_browsePosition);
                    break;
                case 1: // OPC_BROWSE_DOWN
                    if (string.IsNullOrEmpty(browsePosition))
                    {
                        throw new OpcException(OpcResultId.InvalidArg);
                    }
                    _browsePosition = string.IsNullOrEmpty(_browsePosition)
                        ? browsePosition
                        : $"{_browsePosition}.{browsePosition}";
                    break;
                case 2: // OPC_BROWSE_TO
                    _browsePosition = browsePosition ?? string.Empty;
                    break;
                default:
                    throw new OpcException(OpcResultId.InvalidArg);
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> BrowseOpcItemIdsAsync(
        int browseFilterType,
        string filterCriteria,
        ushort dataTypeFilter,
        int accessRightsFilter,
        CancellationToken cancellationToken = default)
    {
        _ = browseFilterType; _ = filterCriteria; _ = dataTypeFilter; _ = accessRightsFilter;
        cancellationToken.ThrowIfCancellationRequested();
        // The result drives an IEnumString enumerator on the wire. The address
        // space is consulted via SnapshotCurrentBranchAsync (test helper) when
        // needed; here we return a synthetic interface ref carrying the
        // enumerator's IPID. Full IEnumString stateful enumerator wiring is
        // dispatcher work.
        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
            iid: Guid.Parse("00000101-0000-0000-C000-000000000046"), // IID_IEnumString
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: 0,
            ipid: Guid.CreateVersion7(),
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>()));
    }

    /// <inheritdoc />
    public Task<string> GetItemIdAsync(string itemDataId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemDataId);
        string position;
        lock (_lock)
        {
            position = _browsePosition;
        }
        return _addressSpace.GetItemIdAsync(position, itemDataId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> BrowseAccessPathsAsync(string itemId, CancellationToken cancellationToken = default)
    {
        _ = itemId;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotSupported);
    }

    /// <summary>
    /// Test helper: snapshots the current branch's items at the current
    /// browse position (not part of the OPC wire interface).
    /// </summary>
    public Task<OpcBrowseResult> SnapshotCurrentBranchAsync(OpcBrowseElementKind kind, CancellationToken cancellationToken = default)
    {
        string position;
        lock (_lock)
        {
            position = _browsePosition;
        }
        return _addressSpace.BrowseAsync(position, kind, cancellationToken);
    }

    private static string MoveUp(string position)
    {
        if (string.IsNullOrEmpty(position))
        {
            return string.Empty;
        }
        int lastDot = position.LastIndexOf('.');
        return lastDot < 0 ? string.Empty : position[..lastDot];
    }
}
