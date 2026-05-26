//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Per-item state inside an <see cref="OpcDaGroup"/>. Mutable so callers
/// can <c>SetActiveState</c>, <c>SetClientHandles</c>, <c>SetDatatypes</c>,
/// and <c>Write</c> through the IOPC interfaces; <c>Read</c> returns a
/// snapshot of the current value/quality/timestamp.
/// </summary>
public sealed class OpcDaItem
{
    private readonly Lock _lock = new();
    private OpcVariant _value;
    private ushort _quality;
    private DateTimeOffset _timestamp;
    private bool _active;
    private int _clientHandle;
    private ushort _requestedDatatype;

    /// <summary>Initializes a new item.</summary>
    public OpcDaItem(
        int serverHandle,
        string itemId,
        string? accessPath,
        int clientHandle,
        bool active,
        ushort requestedDatatype)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ServerHandle = serverHandle;
        ItemId = itemId;
        AccessPath = accessPath;
        _clientHandle = clientHandle;
        _active = active;
        _requestedDatatype = requestedDatatype;
        _value = OpcVariant.Empty;
        _quality = OpcDaItemQuality.UncertainNonSpecific;
        _timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>Server-assigned handle.</summary>
    public int ServerHandle { get; }

    /// <summary>OPC item ID (server's namespace path to the data point).</summary>
    public string ItemId { get; }

    /// <summary>Optional server-defined access path supplied at add time.</summary>
    public string? AccessPath { get; }

    /// <summary>Client-side handle to echo back in callbacks.</summary>
    public int ClientHandle
    {
        get { lock (_lock) { return _clientHandle; } }
        set { lock (_lock) { _clientHandle = value; } }
    }

    /// <summary>Whether this item is currently sampled/published.</summary>
    public bool Active
    {
        get { lock (_lock) { return _active; } }
        set { lock (_lock) { _active = value; } }
    }

    /// <summary>The client's requested VARTYPE for reads/writes.</summary>
    public ushort RequestedDatatype
    {
        get { lock (_lock) { return _requestedDatatype; } }
        set { lock (_lock) { _requestedDatatype = value; } }
    }

    /// <summary>Gets a snapshot of the current value+quality+timestamp.</summary>
    public OpcItemState GetSnapshot()
    {
        lock (_lock)
        {
            return new OpcItemState(
                ClientHandle: _clientHandle,
                Timestamp: _timestamp,
                Quality: new OpcQuality(_quality),
                Value: _value);
        }
    }

    /// <summary>Atomically updates the value+quality+timestamp (server writes from the simulator or from caller writes).</summary>
    public void Update(OpcVariant value, ushort quality, DateTimeOffset timestamp)
    {
        lock (_lock)
        {
            _value = value;
            _quality = quality;
            _timestamp = timestamp;
        }
    }
}
