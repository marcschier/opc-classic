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
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

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
public sealed class OpcDaGroup : IOPCGroupStateMgt, IOPCGroupStateMgt2, IOPCItemMgt, IOPCSyncIO,
    IOPCSyncIO2, IOPCAsyncIO2, IOPCAsyncIO3, IConnectionPoint, IConnectionPointContainer,
    IOPCItemDeadbandMgt, IOPCItemSamplingMgt
{
    private readonly OpcObjectRegistry? _objectRegistry;
    private readonly ConcurrentDictionary<int, OpcDaItem> _items = new();
    private readonly ConcurrentDictionary<int, IOpcInterfaceRef> _sinks = new();
    private int _nextItemHandle = 1;
    private int _nextCancelId = 1;
    private int _nextSubscriptionCookie = 1;
    private int _lastCancel2Id;

    /// <summary>Async I/O callbacks enabled (the GetEnable/SetEnable state).</summary>
    private bool _callbacksEnabled = true;

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
        : this(name, serverHandle, clientHandle, active, requestedUpdateRate, timeBias, percentDeadband, localeId, objectRegistry: null)
    {
    }

    /// <summary>
    /// Initializes a new group with an attached <see cref="OpcObjectRegistry"/>
    /// for registering per-cursor enumerators created via
    /// <see cref="CreateEnumeratorAsync"/>.
    /// </summary>
    public OpcDaGroup(
        string name,
        int serverHandle,
        int clientHandle,
        bool active,
        int requestedUpdateRate,
        int timeBias,
        float percentDeadband,
        int localeId,
        OpcObjectRegistry? objectRegistry)
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
        _objectRegistry = objectRegistry;
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

    /// <summary>Read-only view of the data-callback subscriptions registered via <see cref="IConnectionPoint.AdviseAsync"/>.</summary>
    public IReadOnlyDictionary<int, IOpcInterfaceRef> Subscriptions => _sinks;

    /// <summary>Test helper: returns the number of active <c>IOPCDataCallback</c> subscriptions.</summary>
    public int SubscriptionCount => _sinks.Count;

    /// <summary>Test helper: the most recent cancel ID passed to <see cref="Cancel2Async"/>.</summary>
    public int LastCancel2Id => _lastCancel2Id;

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

        OpcItemAttributes[] snapshot = BuildItemAttributesSnapshot();
        var enumerator = new OpcDaItemAttributesEnumerator(snapshot, _objectRegistry);

        Guid ipid;
        if (_objectRegistry is not null)
        {
            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IEnumOPCItemAttributes.InterfaceId] = new IEnumOPCItemAttributesServerDispatcher(enumerator),
            };
            ipid = _objectRegistry.Register(dispatchers);
        }
        else
        {
            ipid = Guid.CreateVersion7();
        }

        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
            iid: requestedInterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: unchecked((ulong)ServerHandle),
            ipid: ipid,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>()));
    }

    internal OpcItemAttributes[] BuildItemAttributesSnapshot()
    {
        var snapshot = new List<OpcItemAttributes>(_items.Count);
        foreach (OpcDaItem item in _items.Values)
        {
            snapshot.Add(new OpcItemAttributes(
                AccessPath: item.AccessPath,
                ItemId: item.ItemId,
                Active: item.Active,
                ClientHandle: item.ClientHandle,
                ServerHandle: item.ServerHandle,
                AccessRights: 0x3,
                Blob: Array.Empty<byte>(),
                RequestedDataType: (VarType)item.RequestedDatatype,
                CanonicalDataType: (VarType)item.RequestedDatatype,
                EUType: 0,
                EUInfo: OpcVariant.Empty));
        }
        return snapshot.ToArray();
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

    // ----- IOPCSyncIO2 -----

    /// <inheritdoc />
    public Task ReadMaxAgeAsync(
        int[] serverHandles,
        int[] maxAges,
        out OpcVariant[] values,
        out ushort[] qualities,
        out long[] timestamps,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(maxAges);
        cancellationToken.ThrowIfCancellationRequested();
        _ = maxAges; // in-memory: max-age is satisfied immediately

        int count = serverHandles.Length;
        values = new OpcVariant[count];
        qualities = new ushort[count];
        timestamps = new long[count];
        errors = new int[count];
        for (int i = 0; i < count; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                OpcItemState snapshot = item.GetSnapshot();
                values[i] = snapshot.Value;
                qualities[i] = (ushort)snapshot.Quality.RawValue;
                timestamps[i] = snapshot.Timestamp.ToFileTime();
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                values[i] = OpcVariant.Empty;
                qualities[i] = OpcDaItemQuality.BadNonSpecific;
                timestamps[i] = 0;
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int[]> WriteVqtAsync(int[] serverHandles, OpcItemVqt[] values, CancellationToken cancellationToken = default)
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
                OpcItemVqt vqt = values[i];
                ushort quality = vqt.Quality is { } q ? (ushort)q.RawValue : OpcDaItemQuality.GoodNonSpecific;
                DateTimeOffset ts = vqt.Timestamp ?? now;
                item.Update(vqt.Value, quality, ts);
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(errors);
    }

    // ----- IOPCAsyncIO2 -----

    /// <inheritdoc />
    Task<int> IOPCAsyncIO2.ReadAsync(int[] serverHandles, int transactionId, out int[] errors, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        _ = transactionId;
        // Allocate a cancel id; per-item error reporting is identical to sync read.
        // The real ocom-7b path would fire-and-forget a callback delivering the
        // read results to the subscribed sink; here we just produce the per-item
        // errors synchronously so the client gets a consistent dispatch contract.
        int cancelId = Interlocked.Increment(ref _nextCancelId);
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            errors[i] = _items.ContainsKey(serverHandles[i])
                ? OpcResultId.Ok.Code
                : OpcResultId.InvalidHandle.Code;
        }
        return Task.FromResult(cancelId);
    }

    /// <inheritdoc />
    Task<int> IOPCAsyncIO2.WriteAsync(int[] serverHandles, OpcVariant[] values, int transactionId, out int[] errors, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);
        if (serverHandles.Length != values.Length)
        {
            throw new ArgumentException("serverHandles and values must have the same length.", nameof(values));
        }
        cancellationToken.ThrowIfCancellationRequested();
        _ = transactionId;
        int cancelId = Interlocked.Increment(ref _nextCancelId);
        errors = new int[serverHandles.Length];
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
        return Task.FromResult(cancelId);
    }

    /// <inheritdoc />
    public Task<int> Refresh2Async(int dataSource, int transactionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = dataSource;
        _ = transactionId;
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    /// <inheritdoc />
    public Task Cancel2Async(int cancelId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _lastCancel2Id = cancelId;
        // OPC DA spec: the server confirms by raising OnCancelComplete on each
        // subscribed sink. Callers wire that delivery via
        // <see cref="TriggerCancelCompleteAsync"/>.
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetEnableAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _callbacksEnabled = enabled;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> GetEnableAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_callbacksEnabled);
    }

    // ----- IOPCAsyncIO3 -----

    /// <inheritdoc />
    public Task<int> ReadMaxAgeAsync(int[] serverHandles, int[] maxAges, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(maxAges);
        cancellationToken.ThrowIfCancellationRequested();
        _ = transactionId;
        _ = maxAges;
        int cancelId = Interlocked.Increment(ref _nextCancelId);
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            errors[i] = _items.ContainsKey(serverHandles[i])
                ? OpcResultId.Ok.Code
                : OpcResultId.InvalidHandle.Code;
        }
        return Task.FromResult(cancelId);
    }

    /// <inheritdoc />
    public Task<int> WriteVqtAsync(int[] serverHandles, OpcItemVqt[] values, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);
        if (serverHandles.Length != values.Length)
        {
            throw new ArgumentException("serverHandles and values must have the same length.", nameof(values));
        }
        cancellationToken.ThrowIfCancellationRequested();
        _ = transactionId;
        int cancelId = Interlocked.Increment(ref _nextCancelId);
        errors = new int[serverHandles.Length];
        DateTimeOffset now = DateTimeOffset.UtcNow;
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                OpcItemVqt vqt = values[i];
                ushort quality = vqt.Quality is { } q ? (ushort)q.RawValue : OpcDaItemQuality.GoodNonSpecific;
                DateTimeOffset ts = vqt.Timestamp ?? now;
                item.Update(vqt.Value, quality, ts);
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(cancelId);
    }

    /// <inheritdoc />
    public Task<int> RefreshMaxAgeAsync(int maxAge, int transactionId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = maxAge;
        _ = transactionId;
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    // IOPCSyncIO2 has the same ReadAsync/WriteAsync as IOPCSyncIO -- the
    // generator binds them to the same interface methods. Explicit interface
    // implementation routes both to the shared in-memory paths.
    Task<OpcItemState[]> IOPCSyncIO2.ReadAsync(int dataSource, int[] serverHandles, out int[] errors, CancellationToken cancellationToken) =>
        ReadAsync(dataSource, serverHandles, out errors, cancellationToken);

    Task<int[]> IOPCSyncIO2.WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken) =>
        WriteAsync(serverHandles, values, cancellationToken);

    // ----- IOPCItemDeadbandMgt (DA 3.0 per-item deadband) -----

    /// <inheritdoc />
    public Task<int[]> SetItemDeadbandAsync(int[] serverHandles, float[] percentDeadbands, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(percentDeadbands);
        if (serverHandles.Length != percentDeadbands.Length)
        {
            throw new ArgumentException("serverHandles and percentDeadbands must have the same length.", nameof(percentDeadbands));
        }
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                float pd = percentDeadbands[i];
                if (pd < 0f || pd > 100f)
                {
                    errors[i] = OpcResultId.Range.Code;
                    continue;
                }
                item.PercentDeadband = pd;
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
    public Task GetItemDeadbandAsync(int[] serverHandles, out float[] percentDeadbands, out int[] errors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        percentDeadbands = new float[serverHandles.Length];
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                if (item.PercentDeadband is { } pd)
                {
                    percentDeadbands[i] = pd;
                    errors[i] = OpcResultId.Ok.Code;
                }
                else
                {
                    errors[i] = OpcResultId.DeadbandNotSet.Code;
                }
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int[]> ClearItemDeadbandAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                if (item.PercentDeadband is null)
                {
                    errors[i] = OpcResultId.DeadbandNotSet.Code;
                }
                else
                {
                    item.PercentDeadband = null;
                    errors[i] = OpcResultId.Ok.Code;
                }
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(errors);
    }

    // ----- IOPCItemSamplingMgt (DA 3.0 per-item sampling rate + buffering) -----

    /// <inheritdoc />
    public Task SetItemSamplingRateAsync(
        int[] serverHandles,
        int[] requestedSamplingRates,
        out int[] revisedSamplingRates,
        out int[] errors,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(requestedSamplingRates);
        if (serverHandles.Length != requestedSamplingRates.Length)
        {
            throw new ArgumentException("serverHandles and requestedSamplingRates must have the same length.", nameof(requestedSamplingRates));
        }
        cancellationToken.ThrowIfCancellationRequested();
        revisedSamplingRates = new int[serverHandles.Length];
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                int rate = Math.Max(0, requestedSamplingRates[i]);
                item.SamplingRate = rate;
                revisedSamplingRates[i] = rate;
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task GetItemSamplingRateAsync(int[] serverHandles, out int[] samplingRates, out int[] errors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        samplingRates = new int[serverHandles.Length];
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                if (item.SamplingRate is { } rate)
                {
                    samplingRates[i] = rate;
                    errors[i] = OpcResultId.Ok.Code;
                }
                else
                {
                    errors[i] = OpcResultId.RateNotSet.Code;
                }
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int[]> ClearItemSamplingRateAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                if (item.SamplingRate is null)
                {
                    errors[i] = OpcResultId.RateNotSet.Code;
                }
                else
                {
                    item.SamplingRate = null;
                    errors[i] = OpcResultId.Ok.Code;
                }
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int[]> SetItemBufferEnableAsync(int[] serverHandles, bool[] enabled, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(enabled);
        if (serverHandles.Length != enabled.Length)
        {
            throw new ArgumentException("serverHandles and enabled must have the same length.", nameof(enabled));
        }
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                item.BufferEnabled = enabled[i];
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
    public Task GetItemBufferEnableAsync(int[] serverHandles, out bool[] enabled, out int[] errors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        enabled = new bool[serverHandles.Length];
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (_items.TryGetValue(serverHandles[i], out OpcDaItem? item))
            {
                enabled[i] = item.BufferEnabled;
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                errors[i] = OpcResultId.InvalidHandle.Code;
            }
        }
        return Task.CompletedTask;
    }

    // ----- IConnectionPoint (subscription sink-binding) -----

    /// <inheritdoc />
    public Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(IOPCDataCallback.InterfaceId);
    }

    /// <inheritdoc />
    public Task<int> AdviseAsync(IOpcInterfaceRef sink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);
        cancellationToken.ThrowIfCancellationRequested();
        int cookie = Interlocked.Increment(ref _nextSubscriptionCookie);
        _sinks[cookie] = sink;
        return Task.FromResult(cookie);
    }

    /// <inheritdoc />
    public Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // OLE / COM IConnectionPoint convention: unknown cookies return CONNECT_E_NOCONNECTION.
        if (!_sinks.TryRemove(cookie, out _))
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }
        return Task.CompletedTask;
    }

    // ----- IConnectionPointContainer -----

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> EnumConnectionPointsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
            iid: Guid.Parse("B196B285-BAB4-101A-B69C-00AA00341D07"), // IID_IEnumConnectionPoints
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: unchecked((ulong)ServerHandle),
            ipid: Guid.CreateVersion7(),
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>()));
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> FindConnectionPointAsync(Guid iid, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // OpcDaGroup only supports IOPCDataCallback subscriptions. IID_IDataObject
        // (OPC DA 2.05a Appendix B, optional) is intentionally NOT supported;
        // clients that QI for IDataObject see a clean CONNECT_E_NOCONNECTION
        // failure rather than a malformed sink.
        if (iid != IOPCDataCallback.InterfaceId)
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }

        // OpcDaGroup IS the IConnectionPoint for IOPCDataCallback. Return a
        // synthetic interface ref carrying this group's identity; in the real
        // wire path the IPID would be the one assigned at AddGroup time.
        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
            iid: IConnectionPoint.InterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: unchecked((ulong)ServerHandle),
            ipid: Guid.CreateVersion7(),
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>()));
    }

    /// <summary>
    /// Fans out an <c>OnDataChange</c> callback for all active subscriptions.
    /// Delivers via the caller-supplied <paramref name="sender"/> delegate;
    /// the OpcDaGroup itself stays transport-agnostic (it doesn't construct
    /// outbound channels). Callers (typically the host) supply a sender that
    /// resolves each sink's <see cref="IOpcInterfaceRef"/> into a real
    /// <c>DcomCallChannel</c> + <c>IOPCDataCallbackClientProxy</c> and invokes
    /// <c>OnDataChangeAsync</c> on it.
    /// </summary>
    /// <param name="transactionId">Transaction id echoed in the callback payload.</param>
    /// <param name="serverHandles">Server handles for the items whose values changed; unknown handles are skipped.</param>
    /// <param name="sender">Delivery callback invoked once per active subscription with the change payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task TriggerDataChangeAsync(
        int transactionId,
        int[] serverHandles,
        Func<IOpcInterfaceRef, DataChangePayload, CancellationToken, Task> sender,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(sender);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_callbacksEnabled || _sinks.IsEmpty)
        {
            return;
        }

        DataChangePayload payload = BuildDataChangePayload(transactionId, serverHandles);
        foreach (KeyValuePair<int, IOpcInterfaceRef> entry in _sinks)
        {
            await sender(entry.Value, payload, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Fans out an <c>OnCancelComplete</c> callback for all active subscriptions
    /// confirming completion of a prior <see cref="Cancel2Async"/> request.
    /// Honors <c>SetEnable</c> just like data-change callbacks: when callbacks
    /// are disabled the trigger short-circuits.
    /// </summary>
    /// <param name="transactionId">Transaction id originally cancelled.</param>
    /// <param name="sender">Delivery callback invoked once per active subscription with the cancel-complete payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task TriggerCancelCompleteAsync(
        int transactionId,
        Func<IOpcInterfaceRef, CancelCompletePayload, CancellationToken, Task> sender,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sender);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_callbacksEnabled || _sinks.IsEmpty)
        {
            return;
        }

        var payload = new CancelCompletePayload(transactionId, ClientHandle);
        foreach (KeyValuePair<int, IOpcInterfaceRef> entry in _sinks)
        {
            await sender(entry.Value, payload, cancellationToken).ConfigureAwait(false);
        }
    }

    private DataChangePayload BuildDataChangePayload(int transactionId, int[] serverHandles)
    {
        var clientHandles = new List<int>(serverHandles.Length);
        var values = new List<OpcVariant>(serverHandles.Length);
        var qualities = new List<ushort>(serverHandles.Length);
        var timestamps = new List<long>(serverHandles.Length);
        var errors = new List<int>(serverHandles.Length);

        foreach (int serverHandle in serverHandles)
        {
            if (!_items.TryGetValue(serverHandle, out OpcDaItem? item))
            {
                continue;
            }
            OpcItemState snapshot = item.GetSnapshot();
            clientHandles.Add(snapshot.ClientHandle);
            values.Add(snapshot.Value);
            qualities.Add((ushort)snapshot.Quality.RawValue);
            timestamps.Add(snapshot.Timestamp.ToFileTime());
            errors.Add(0);
        }

        return new DataChangePayload(
            TransactionId: transactionId,
            GroupHandle: ClientHandle,
            MasterQuality: OpcDaItemQuality.GoodNonSpecific,
            MasterError: 0,
            ClientHandles: clientHandles.ToArray(),
            Values: values.ToArray(),
            Qualities: qualities.ToArray(),
            Timestamps: timestamps.ToArray(),
            Errors: errors.ToArray());
    }

    /// <summary>
    /// Immutable snapshot of an <c>OnDataChange</c> payload delivered to a
    /// subscribed <c>IOPCDataCallback</c> sink.
    /// </summary>
    public sealed record DataChangePayload(
        int TransactionId,
        int GroupHandle,
        int MasterQuality,
        int MasterError,
        int[] ClientHandles,
        OpcVariant[] Values,
        ushort[] Qualities,
        long[] Timestamps,
        int[] Errors);

    /// <summary>
    /// Immutable snapshot of an <c>OnCancelComplete</c> payload delivered to a
    /// subscribed <c>IOPCDataCallback</c> sink after <see cref="Cancel2Async"/>.
    /// </summary>
    public sealed record CancelCompletePayload(int TransactionId, int GroupHandle);

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

