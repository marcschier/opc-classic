//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// In-memory managed implementation of an OPC DA group exposing the
/// group-level COM interfaces. Items live in an internal collection so
/// callers can <c>AddItems</c>/<c>RemoveItems</c>/<c>Read</c>/<c>Write</c>
/// through the OPC item-management + sync I/O interfaces.
/// </summary>
/// <remarks>
/// <para>
/// Implements: <c>IOPCGroupStateMgt</c>, <c>IOPCGroupStateMgt2</c>,
/// <c>IOPCItemMgt</c>, <c>IOPCSyncIO</c>.
/// </para>
/// <para>
/// Async I/O interfaces (<c>IOPCAsyncIO2</c>, <c>IOPCAsyncIO3</c>) and the
/// max-age sync interface (<c>IOPCSyncIO2</c>) are a follow-up
/// (ocom-3d). Data-callback subscription (<c>IConnectionPoint</c> +
/// outbound <c>IOPCDataCallback</c>) is ocom-7b.
/// </para>
/// </remarks>
public sealed class OpcDaGroup : IOPCGroupStateMgt, IOPCGroupStateMgt2, IOPCItemMgt, IOPCSyncIO
{
    private readonly ConcurrentDictionary<int, OpcDaItem> _items = new();
    private int _nextItemHandle = 1;

    /// <summary>Initializes a new group with the supplied creation parameters.</summary>
    public OpcDaGroup(
        string name,
        int serverHandle,
        int clientHandle,
        bool active,
        int requestedUpdateRate,
        int timeBias,
        float percentDeadband,
        int localeId)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
        ServerHandle = serverHandle;
        ClientHandle = clientHandle;
        Active = active;
        UpdateRate = requestedUpdateRate;
        TimeBias = timeBias;
        PercentDeadband = percentDeadband;
        LocaleId = localeId;
        KeepAliveTime = 0;
    }

    /// <summary>Server-assigned group handle.</summary>
    public int ServerHandle { get; }

    /// <summary>Current group name (settable via SetName).</summary>
    public string Name { get; private set; }

    /// <summary>Client-supplied handle echoed back to the client in callbacks.</summary>
    public int ClientHandle { get; private set; }

    /// <summary>Whether the group is currently active (publishes updates).</summary>
    public bool Active { get; private set; }

    /// <summary>The negotiated update rate in milliseconds.</summary>
    public int UpdateRate { get; private set; }

    /// <summary>Group time bias in minutes from UTC.</summary>
    public int TimeBias { get; private set; }

    /// <summary>Analog deadband percentage (0..100).</summary>
    public float PercentDeadband { get; private set; }

    /// <summary>LCID used for server-supplied text.</summary>
    public int LocaleId { get; private set; }

    /// <summary>Keep-alive period in milliseconds (0 = disabled).</summary>
    public int KeepAliveTime { get; private set; }

    /// <summary>Read-only view of the items currently in the group.</summary>
    public IReadOnlyCollection<OpcDaItem> Items => (IReadOnlyCollection<OpcDaItem>)_items.Values;

    /// <summary>Test helper: returns the number of items currently in the group.</summary>
    public int ItemCount => _items.Count;

    /// <summary>Returns the item for a given server handle, or <see langword="null"/> if unknown.</summary>
    public OpcDaItem? GetItem(int serverHandle) =>
        _items.TryGetValue(serverHandle, out OpcDaItem? item) ? item : null;

    // ----- IOPCGroupStateMgt -----

    /// <inheritdoc />
    public Task<OpcGroupState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new OpcGroupState(
            ClientHandle: ClientHandle,
            ServerHandle: ServerHandle,
            Name: Name,
            Active: Active,
            UpdateRate: UpdateRate,
            TimeBias: TimeBias,
            PercentDeadband: PercentDeadband,
            LocaleId: LocaleId));
    }

    /// <inheritdoc />
    public Task SetStateAsync(
        int requestedUpdateRate,
        bool active,
        int timeBias,
        float percentDeadband,
        int localeId,
        int clientGroupHandle,
        out int revisedUpdateRate,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        UpdateRate = requestedUpdateRate;
        Active = active;
        TimeBias = timeBias;
        PercentDeadband = percentDeadband;
        LocaleId = localeId;
        ClientHandle = clientGroupHandle;
        revisedUpdateRate = requestedUpdateRate;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();
        Name = name;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> CloneGroupAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
            iid: requestedInterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: unchecked((ulong)ServerHandle),
            ipid: Guid.CreateVersion7(),
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>()));
    }

    // ----- IOPCGroupStateMgt2 -----

    /// <inheritdoc />
    public Task<int> SetKeepAliveAsync(int keepAliveTime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int previous = KeepAliveTime;
        KeepAliveTime = keepAliveTime;
        return Task.FromResult(previous);
    }

    /// <inheritdoc />
    public Task<int> GetKeepAliveAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(KeepAliveTime);
    }

    // ----- IOPCItemMgt -----

    /// <inheritdoc />
    public Task AddItemsAsync(
        OpcItemDef[] itemDefinitions,
        out OpcItemResult[] addResults,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemDefinitions);
        cancellationToken.ThrowIfCancellationRequested();

        addResults = new OpcItemResult[itemDefinitions.Length];
        errors = new int[itemDefinitions.Length];

        for (int i = 0; i < itemDefinitions.Length; i++)
        {
            (OpcItemResult result, int hr) = TryAddItem(itemDefinitions[i]);
            addResults[i] = result;
            errors[i] = hr;
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ValidateItemsAsync(
        OpcItemDef[] itemDefinitions,
        bool blobUpdate,
        out OpcItemResult[] validationResults,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemDefinitions);
        cancellationToken.ThrowIfCancellationRequested();
        _ = blobUpdate;

        validationResults = new OpcItemResult[itemDefinitions.Length];
        errors = new int[itemDefinitions.Length];

        // Validation does not add; produce a result shape but don't allocate
        // a real handle. A zero handle means "validation only".
        for (int i = 0; i < itemDefinitions.Length; i++)
        {
            OpcItemDef def = itemDefinitions[i];
            errors[i] = string.IsNullOrWhiteSpace(def?.ItemId)
                ? OpcResultId.UnknownItemId.Code
                : OpcResultId.Ok.Code;
            validationResults[i] = new OpcItemResult(
                ServerHandle: 0,
                CanonicalDataType: def?.RequestedDataType ?? VarType.VT_EMPTY,
                AccessRights: 0x3,
                Blob: Array.Empty<byte>());
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int[]> RemoveItemsAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            errors[i] = _items.TryRemove(serverHandles[i], out _)
                ? OpcResultId.Ok.Code
                : OpcResultId.InvalidHandle.Code;
        }
        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int[]> SetActiveStateAsync(int[] serverHandles, bool active, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                item.Active = active;
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int[]> SetClientHandlesAsync(int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(clientHandles);
        if (serverHandles.Length != clientHandles.Length)
        {
            throw new ArgumentException("serverHandles and clientHandles must have the same length.", nameof(clientHandles));
        }
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                item.ClientHandle = clientHandles[i];
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int[]> SetDatatypesAsync(int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(requestedDataTypes);
        if (serverHandles.Length != requestedDataTypes.Length)
        {
            throw new ArgumentException("serverHandles and requestedDataTypes must have the same length.", nameof(requestedDataTypes));
        }
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                item.RequestedDatatype = requestedDataTypes[i];
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Real IEnumOPCItemAttributes implementation is ocom-8b; return a
        // synthetic ref so the dispatch returns something usable.
        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
            iid: requestedInterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: unchecked((ulong)ServerHandle),
            ipid: Guid.CreateVersion7(),
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>()));
    }

    // ----- IOPCSyncIO -----

    /// <inheritdoc />
    public Task<OpcItemState[]> ReadAsync(
        int dataSource,
        int[] serverHandles,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        _ = dataSource; // OPC_DS_CACHE / OPC_DS_DEVICE -- ignored; in-memory snapshot serves both

        OpcItemState[] states = new OpcItemState[serverHandles.Length];
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                states[i] = item.GetSnapshot();
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                states[i] = new OpcItemState(0, DateTimeOffset.UnixEpoch, default, OpcVariant.Empty);
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(states);
    }

    /// <inheritdoc />
    public Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);
        if (serverHandles.Length != values.Length)
        {
            throw new ArgumentException("serverHandles and values must have the same length.", nameof(values));
        }
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        DateTimeOffset now = DateTimeOffset.UtcNow;
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                item.Update(values[i], OpcDaItemQuality.GoodNonSpecific, now);
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(errors);
    }

    private (OpcItemResult Result, int Hresult) TryAddItem(OpcItemDef? def)
    {
        if (def is null || string.IsNullOrWhiteSpace(def.ItemId))
        {
            return (
                new OpcItemResult(0, def?.RequestedDataType ?? VarType.VT_EMPTY, 0, Array.Empty<byte>()),
                OpcResultId.UnknownItemId.Code);
        }

        int handle = Interlocked.Increment(ref _nextItemHandle);
        var item = new OpcDaItem(
            serverHandle: handle,
            itemId: def.ItemId,
            accessPath: def.AccessPath,
            clientHandle: def.ClientHandle,
            active: def.Active,
            requestedDatatype: (ushort)def.RequestedDataType);
        _items[handle] = item;

        return (
            new OpcItemResult(
                ServerHandle: handle,
                CanonicalDataType: def.RequestedDataType,
                AccessRights: 0x3, // OPC_READABLE | OPC_WRITEABLE
                Blob: Array.Empty<byte>()),
            OpcResultId.Ok.Code);
    }
}
