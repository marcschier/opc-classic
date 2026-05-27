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
/// Default managed implementation of <see cref="IOPCItemProperties"/>
/// (DA 2.x). Returns empty property lists / <c>OPC_E_NOTSUPPORTED</c> so
/// the interface is reachable on the wire even when the server doesn't
/// expose per-item properties.
/// </summary>
public sealed class DefaultItemProperties : IOPCItemProperties
{
    /// <inheritdoc />
    public Task QueryAvailablePropertiesAsync(
        string itemId,
        out int[] propertyIds,
        out string[] descriptions,
        out ushort[] dataTypes,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemId);
        cancellationToken.ThrowIfCancellationRequested();
        propertyIds = Array.Empty<int>();
        descriptions = Array.Empty<string>();
        dataTypes = Array.Empty<ushort>();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task GetItemPropertiesAsync(
        string itemId,
        int[] propertyIds,
        out OpcVariant[] data,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemId);
        ArgumentNullException.ThrowIfNull(propertyIds);
        cancellationToken.ThrowIfCancellationRequested();
        data = new OpcVariant[propertyIds.Length];
        errors = new int[propertyIds.Length];
        for (int i = 0; i < propertyIds.Length; i++)
        {
            data[i] = OpcVariant.Empty;
            errors[i] = OpcResultId.InvalidPid.Code;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task LookupItemIdsAsync(
        string itemId,
        int[] propertyIds,
        out string[] newItemIds,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(itemId);
        ArgumentNullException.ThrowIfNull(propertyIds);
        cancellationToken.ThrowIfCancellationRequested();
        newItemIds = new string[propertyIds.Length];
        errors = new int[propertyIds.Length];
        for (int i = 0; i < propertyIds.Length; i++)
        {
            newItemIds[i] = string.Empty;
            errors[i] = OpcResultId.InvalidPid.Code;
        }
        return Task.CompletedTask;
    }
}
