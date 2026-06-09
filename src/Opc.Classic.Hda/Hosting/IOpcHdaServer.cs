//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Hda.Hosting;

/// <summary>Contract implemented by user code to provide an in-process managed HDA server.</summary>
public interface IOpcHdaServer : IOPCHDA_Server {
    /// <summary>Gets the HDA historian runtime status snapshot.</summary>
    new Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Validates HDA item IDs and returns per-item HRESULTs.</summary>
    Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default);

    Task<int[]> IOPCHDA_Server.GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<int[]> IOPCHDA_Server.ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken) =>
        throw NotImplemented();

    Task<int[]> IOPCHDA_Server.ValidateItemIDsAsync(string[] itemIds, CancellationToken cancellationToken) =>
        ValidateItemIdsAsync(itemIds, cancellationToken);

    Task IOPCHDA_Server.GetItemAttributesAsync(out int[] attributeIds, out string[] attributeNames, out string[] attributeDescriptions, out int[] attributeDataTypes, CancellationToken cancellationToken) {
        attributeIds = [];
        attributeNames = [];
        attributeDescriptions = [];
        attributeDataTypes = [];
        throw NotImplemented();
    }

    Task IOPCHDA_Server.GetAggregatesAsync(out int[] aggregateIds, out string[] aggregateNames, out string[] aggregateDescriptions, CancellationToken cancellationToken) {
        aggregateIds = [];
        aggregateNames = [];
        aggregateDescriptions = [];
        throw NotImplemented();
    }

    /// <summary>Browses the HDA address space at the supplied branch position.</summary>
    async IAsyncEnumerable<HdaBrowseElement> BrowseAsync(
        string branchPosition,
        HdaBrowseType browseType,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        _ = branchPosition;
        _ = browseType;
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    private static OpcException NotImplemented() => new(OpcResultId.NotImplemented);

    /// <summary>Reads raw historical values for each item in the requested time range.</summary>
    Task<OpcHdaItem[]> ReadRawAsync(
        string[] itemIds,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        CancellationToken ct = default) {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        _ = maxValues;
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Array.Empty<OpcHdaItem>());
    }

    /// <summary>Reads processed historical values for each item in fixed resample intervals.</summary>
    Task<OpcHdaItem[]> ReadProcessedAsync(
        string[] itemIds,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        TimeSpan resampleInterval,
        HdaAggregate aggregate,
        CancellationToken ct = default) {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        _ = resampleInterval;
        _ = aggregate;
        ct.ThrowIfCancellationRequested();
        return Task.FromResult(Array.Empty<OpcHdaItem>());
    }
}
