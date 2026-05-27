//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting;

/// <summary>HDA dispatcher adapter that delegates to the source-generated IOPCHDA_Server dispatcher.</summary>
public sealed class OpcHdaServerDispatcher : IOpcHdaServerDispatcher
{
    private const int OpchdaEqual = 1;
    private const int OpchdaNotEqual = 6;

    private readonly IOpcHdaServer _server;
    private readonly IOPCHDA_ServerServerDispatcher _serverDispatcher;

    /// <summary>Initializes a new instance of the <see cref="OpcHdaServerDispatcher" /> class.</summary>
    public OpcHdaServerDispatcher(IOpcHdaServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _serverDispatcher = new IOPCHDA_ServerServerDispatcher(_server);
    }

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId != IOPCHDA_Server.InterfaceId)
        {
            return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
        }

        return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
            .ToNdrCallResult();
    }

    /// <inheritdoc />
    public async Task<int[]> ValidateBrowseFiltersAsync(
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();

        HashSet<int>? supportedAttributeIds = await TryGetSupportedAttributeIdsAsync(cancellationToken).ConfigureAwait(false);
        var errors = new int[filters.Count];
        for (int i = 0; i < filters.Count; i++)
        {
            errors[i] = ValidateBrowseFilter(filters[i], supportedAttributeIds);
        }

        return errors;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> BrowseAsync(
        string branchPosition,
        HdaBrowseType browseType,
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();

        var values = new List<string>();
        await foreach (HdaBrowseElement element in _server.BrowseAsync(branchPosition, browseType, cancellationToken).ConfigureAwait(false))
        {
            if (ShouldInclude(element, browseType))
            {
                values.Add(ToBrowseString(element, browseType));
            }
        }

        return values;
    }

    /// <inheritdoc />
    public Task<string> ChangeBrowsePositionAsync(
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

    /// <inheritdoc />
    public Task<string> GetItemIdAsync(
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

    /// <inheritdoc />
    public Task<string> GetBranchPositionAsync(
        string branchPosition,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(branchPosition);
    }

    private async Task<HashSet<int>?> TryGetSupportedAttributeIdsAsync(CancellationToken cancellationToken)
    {
        try
        {
            await ((IOPCHDA_Server)_server).GetItemAttributesAsync(
                out int[] attributeIds,
                out string[] attributeNames,
                out string[] attributeDescriptions,
                out int[] attributeDataTypes,
                cancellationToken).ConfigureAwait(false);
            _ = attributeNames;
            _ = attributeDescriptions;
            _ = attributeDataTypes;
            return attributeIds.Length == 0 ? null : new HashSet<int>(attributeIds);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return null;
        }
    }

    private static int ValidateBrowseFilter(OpcHdaBrowseFilter filter, HashSet<int>? supportedAttributeIds)
    {
        if (filter.AttributeId <= 0)
        {
            return OpcHdaErrors.OPCHDA_E_INVALIDATTRID;
        }
        if (filter.OperatorCode is < OpchdaEqual or > OpchdaNotEqual)
        {
            return OpcResultId.InvalidArg.Code;
        }
        if (supportedAttributeIds is not null && !supportedAttributeIds.Contains(filter.AttributeId))
        {
            return OpcHdaErrors.OPCHDA_E_UNKNOWNATTRID;
        }

        return OpcResultId.Ok.Code;
    }

    private static bool ShouldInclude(HdaBrowseElement element, HdaBrowseType browseType) => browseType switch
    {
        HdaBrowseType.Branch => element.BrowseType == HdaBrowseType.Branch,
        HdaBrowseType.Leaf => element.BrowseType == HdaBrowseType.Leaf,
        HdaBrowseType.Flat => element.BrowseType == HdaBrowseType.Flat,
        HdaBrowseType.Items => element.BrowseType is HdaBrowseType.Leaf or HdaBrowseType.Flat,
        _ => false,
    };

    private static string ToBrowseString(HdaBrowseElement element, HdaBrowseType browseType) =>
        browseType is HdaBrowseType.Flat or HdaBrowseType.Items
            ? string.IsNullOrEmpty(element.ItemId) ? element.Name : element.ItemId
            : element.Name;

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
