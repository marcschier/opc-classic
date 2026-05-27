//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Dispatcher contract and browse-filter DTO are grouped for API locality.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Hda.Hosting;

/// <summary>Dispatches NDR-encoded HDA DCOM calls to a managed HDA server implementation.</summary>
public interface IOpcHdaServerDispatcher
{
    /// <summary>Routes an incoming interface/opnum request and returns an HRESULT plus NDR response body.</summary>
    Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken);

    /// <summary>Validates HDA browse filters and returns one HRESULT per filter.</summary>
    Task<int[]> ValidateBrowseFiltersAsync(
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new int[filters.Count]);
    }

    /// <summary>Returns display strings for the requested HDA browse type at the supplied branch position.</summary>
    Task<IReadOnlyList<string>> BrowseAsync(
        string branchPosition,
        HdaBrowseType browseType,
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        _ = branchPosition;
        _ = browseType;
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    /// <summary>Returns the next branch position for an HDA browser cursor move.</summary>
    Task<string> ChangeBrowsePositionAsync(
        string currentBranchPosition,
        int browseDirection,
        string? browseString,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(browseDirection switch
        {
            1 => MoveUp(currentBranchPosition),
            2 when !string.IsNullOrEmpty(browseString) => string.IsNullOrEmpty(currentBranchPosition)
                ? browseString
                : currentBranchPosition + "." + browseString,
            3 => browseString ?? string.Empty,
            _ => throw new OpcException(OpcResultId.InvalidArg),
        });
    }

    /// <summary>Resolves an HDA browse node to a fully qualified item ID.</summary>
    Task<string> GetItemIdAsync(
        string branchPosition,
        string node,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(branchPosition) || string.IsNullOrEmpty(node))
        {
            return Task.FromResult(node);
        }

        return Task.FromResult(node.StartsWith(branchPosition + ".", StringComparison.Ordinal)
            ? node
            : branchPosition + "." + node);
    }

    /// <summary>Returns the browser's current branch position string.</summary>
    Task<string> GetBranchPositionAsync(
        string branchPosition,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(branchPosition);
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

/// <summary>One HDA browser attribute filter supplied to <c>IOPCHDA_Server::CreateBrowse</c>.</summary>
/// <param name="AttributeId">The HDA attribute ID being filtered.</param>
/// <param name="OperatorCode">The <c>OPCHDA_OPERATORCODES</c> comparison operator.</param>
/// <param name="Value">The native VARIANT filter value converted to a managed carrier.</param>
public sealed record OpcHdaBrowseFilter(int AttributeId, int OperatorCode, OpcVariant Value);
