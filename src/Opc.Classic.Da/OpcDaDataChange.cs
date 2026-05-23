//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Keep the OnDataChange batch and item records together.

using System;
using System.Collections.Generic;

namespace Opc.Classic.Da;

/// <summary>
/// Managed projection of an <c>IOPCDataCallback::OnDataChange</c> delivery.
/// </summary>
/// <param name="TransactionId">Transaction ID supplied by the server.</param>
/// <param name="GroupServerHandle">Server handle of the group that produced the callback.</param>
/// <param name="MasterQuality">Batch-level master quality HRESULT from the callback.</param>
/// <param name="MasterError">Batch-level master error HRESULT from the callback.</param>
/// <param name="Items">Per-item values carried by the callback.</param>
public sealed record OpcDaDataChange(
    int TransactionId,
    int GroupServerHandle,
    short MasterQuality,
    int MasterError,
    IReadOnlyList<OpcDaDataChangeItem> Items)
{
    private IReadOnlyList<OpcDaDataChangeItem> _items = Items ?? throw new ArgumentNullException(nameof(Items));

    /// <summary>Per-item values carried by the callback.</summary>
    public IReadOnlyList<OpcDaDataChangeItem> Items
    {
        get => _items;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _items = value;
        }
    }
}

/// <summary>
/// Per-item value carried in an <see cref="OpcDaDataChange" /> callback batch.
/// </summary>
/// <param name="ClientHandle">Client item handle supplied when the item was added.</param>
/// <param name="Value">OPC VARIANT value for the item.</param>
/// <param name="Quality">OPC DA quality word for the item.</param>
/// <param name="Timestamp">Item timestamp converted from OPC FILETIME.</param>
/// <param name="Error">Per-item HRESULT.</param>
public sealed record OpcDaDataChangeItem(
    int ClientHandle,
    OpcVariant Value,
    OpcQuality Quality,
    DateTimeOffset Timestamp,
    int Error);
