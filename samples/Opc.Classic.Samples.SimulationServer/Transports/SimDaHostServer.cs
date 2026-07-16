// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.SimulationServer.Transports;

/// <summary>
/// A managed OPC DA server, backed by the shared <see cref="SimulatedPlantModel" />, that the
/// <see cref="OpcDaServerHost" /> serves over the real cross-platform transport (and, on Windows,
/// native DCOM). It supports the full DA group lifecycle an OPC explorer uses — AddGroup,
/// AddItems, sync read/write, and subscription tearoffs — by creating real
/// <see cref="OpcDaGroup" /> objects and registering their per-interface dispatchers in the
/// host's <see cref="OpcObjectRegistry" /> (mirroring the reference <c>CttDaServer</c>). Group
/// item values are kept live by <see cref="RefreshFromModel" /> (driven by the transport host's
/// value ticker). It also implements the DA 3.0 stateless <c>IOPCItemIO</c> read/write surface.
/// </summary>
public sealed class SimDaHostServer : IOpcDaServer, IDisposable
{
    private const ushort GoodQuality = 0x00C0;
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
    private readonly SimulatedPlantModel _model;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly IOpcDataCallbackSinkFactory? _callbackSinkFactory;
    private readonly ConcurrentDictionary<int, GroupEntry> _groups = new();
    private int _nextGroupHandle = 0x4000;

    /// <summary>Initializes a new instance of the <see cref="SimDaHostServer" /> class.</summary>
    /// <param name="model">The shared deterministic plant model to serve.</param>
    /// <param name="objectRegistry">The host's per-CLSID object/IPID registry for group tearoffs.</param>
    public SimDaHostServer(
        SimulatedPlantModel model,
        OpcObjectRegistry objectRegistry,
        IOpcDataCallbackSinkFactory? callbackSinkFactory = null)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _callbackSinkFactory = callbackSinkFactory;
    }

    /// <summary>Builds the hierarchical address space exposed for browsing, from the model.</summary>
    public IOpcAddressSpace BuildAddressSpace()
    {
        var space = new InMemoryAddressSpace();
        foreach (SimulatedTag tag in _model.Tags)
        {
            space.AddItem(tag.BranchPath, tag.Name);
        }

        return space;
    }

    /// <summary>
    /// Reconciles every active group's items with the model. Read-only/generated tags are pushed
    /// model -> item so reads return live values; writable tags persist the explorer's group write
    /// item -> model (after seeding an initial value), so write+readback stays consistent across the
    /// group cache and the DA 3.0 <c>IOPCItemIO</c> surface. Called by the transport host's ticker.
    /// </summary>
    public void RefreshFromModel()
    {
        RefreshFromModelAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Reconciles every active group's items with the model and publishes DA callbacks.
    /// </summary>
    public async Task RefreshFromModelAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        foreach (GroupEntry entry in _groups.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!entry.Group.Active)
            {
                continue;
            }

            var changedHandles = new List<int>();
            foreach (OpcDaItem item in entry.Group.Items)
            {
                if (!_model.TryGetTag(item.ItemId, out SimulatedTag tag))
                {
                    continue;
                }

                if (!tag.Writable)
                {
                    // Live generated value.
                    item.Update(ToVariant(_model.CurrentValue(tag, now)), GoodQuality, now);
                    changedHandles.Add(item.ServerHandle);
                    continue;
                }

                // Writable: seed once, then let client writes flow item -> model so they stick.
                object? cached = item.GetSnapshot().Value.Boxed;
                if (cached is null)
                {
                    item.Update(ToVariant(_model.CurrentValue(tag, now)), GoodQuality, now);
                    changedHandles.Add(item.ServerHandle);
                }
                else
                {
                    _model.TryWrite(item.ItemId, cached);
                }
            }

            if (changedHandles.Count > 0)
            {
                await entry.Group.TriggerDataChangeAsync(
                    transactionId: 0,
                    changedHandles.ToArray(),
                    static (_, _, _) => Task.CompletedTask,
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = _groups.Count,
            BandWidth = 0,
            ServerVersion = _model.ServerVersion,
            VendorInfo = _model.VendorInfo + " (DA)",
        });
    }

    /// <inheritdoc />
    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();
        OpcDaGroup group = CreateGroup(name, clientHandle, active, requestedUpdateRate, 0, 0f, localeId);
        return Task.FromResult(group.ServerHandle);
    }

    /// <inheritdoc />
    Task IOPCServer.AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientGroupHandle,
        int timeBias,
        float percentDeadband,
        int localeId,
        Guid requestedInterfaceId,
        out int serverGroupHandle,
        out int revisedUpdateRate,
        out IOpcInterfaceRef group,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();

        OpcDaGroup managedGroup = CreateGroup(name, clientGroupHandle, active, requestedUpdateRate, timeBias, percentDeadband, localeId);
        serverGroupHandle = managedGroup.ServerHandle;
        revisedUpdateRate = managedGroup.UpdateRate;

        var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [IOPCGroupStateMgt.InterfaceId] = new IOPCGroupStateMgtServerDispatcher(managedGroup),
            [IOPCGroupStateMgt2.InterfaceId] = new IOPCGroupStateMgt2ServerDispatcher(managedGroup),
            [IOPCItemMgt.InterfaceId] = new IOPCItemMgtServerDispatcher(managedGroup),
            [IOPCSyncIO.InterfaceId] = new IOPCSyncIOServerDispatcher(managedGroup),
            [IOPCSyncIO2.InterfaceId] = new IOPCSyncIO2ServerDispatcher(managedGroup),
            [IOPCAsyncIO2.InterfaceId] = new IOPCAsyncIO2ServerDispatcher(managedGroup),
            [IOPCAsyncIO3.InterfaceId] = new IOPCAsyncIO3ServerDispatcher(managedGroup),
            [IConnectionPoint.InterfaceId] = new IConnectionPointServerDispatcher(managedGroup),
            [IConnectionPointContainer.InterfaceId] = new IConnectionPointContainerServerDispatcher(managedGroup),
            [IOPCItemDeadbandMgt.InterfaceId] = new IOPCItemDeadbandMgtServerDispatcher(managedGroup),
            [IOPCItemSamplingMgt.InterfaceId] = new IOPCItemSamplingMgtServerDispatcher(managedGroup),
        };
        Guid ipid = _objectRegistry.Register(dispatchers, publicRefs: 1);
        var entry = new GroupEntry(managedGroup, ipid);
        _groups[managedGroup.ServerHandle] = entry;

        group = CreateGroupReference(
            entry,
            requestedInterfaceId,
            addPublicRef: false);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        _ = force;
        cancellationToken.ThrowIfCancellationRequested();
        if (_groups.TryRemove(serverGroupHandle, out GroupEntry? entry))
        {
            _objectRegistry.Unregister(entry.Ipid);
            entry.Group.Dispose();
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (GroupEntry entry in _groups.Values)
        {
            entry.Group.Dispose();
        }

        _groups.Clear();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"Opc.Classic simulation DA error 0x{errorCode:X8} locale=0x{localeId:X4}");
    }

    /// <inheritdoc />
    Task<IOpcInterfaceRef> IOPCServer.GetGroupByNameAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();
        foreach (GroupEntry entry in _groups.Values)
        {
            if (string.Equals(entry.Group.Name, name, StringComparison.Ordinal))
            {
                return Task.FromResult(
                    CreateGroupReference(
                        entry,
                        requestedInterfaceId,
                        addPublicRef: true));
            }
        }

        throw new OpcException(OpcResultId.UnknownPath);
    }

    /// <inheritdoc />
    public Task<OpcDaGroup?> ResolveGroupAsync(int serverHandle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_groups.TryGetValue(serverHandle, out GroupEntry? entry) ? entry.Group : null);
    }

    /// <inheritdoc />
    public Task<OpcDaGroup?> ResolveGroupByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();
        foreach (GroupEntry entry in _groups.Values)
        {
            if (string.Equals(entry.Group.Name, name, StringComparison.Ordinal))
            {
                return Task.FromResult<OpcDaGroup?>(entry.Group);
            }
        }

        return Task.FromResult<OpcDaGroup?>(null);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<OpcDaGroup>>(CreatePrivateGroupSnapshot());
    }

    /// <inheritdoc />
    public Task<OpcDaGroupSetSnapshot> SnapshotAllGroupsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new OpcDaGroupSetSnapshot(
            CreatePrivateGroupSnapshot(),
            Array.Empty<OpcDaGroup>()));
    }

    private OpcDaGroup[] CreatePrivateGroupSnapshot()
    {
        KeyValuePair<int, GroupEntry>[] entries = _groups.ToArray();
        return [.. entries.OrderBy(static pair => pair.Key).Select(static pair => pair.Value.Group)];
    }

    private IOpcInterfaceRef CreateGroupReference(
        GroupEntry entry,
        Guid requestedInterfaceId,
        bool addPublicRef)
    {
        if (addPublicRef
            && !_objectRegistry.AddPublicRefs(entry.Ipid, 1))
        {
            throw new OpcException(new OpcResultId(
                unchecked((int)0x80010108),
                "RPC_E_DISCONNECTED"));
        }
        if (!_objectRegistry.TryGetObjectMetadata(
                entry.Ipid,
                out OpcObjectMetadata metadata))
        {
            if (addPublicRef)
            {
                _objectRegistry.ReleasePublicRefs(entry.Ipid, 1);
            }
            throw new OpcException(new OpcResultId(
                unchecked((int)0x80010108),
                "RPC_E_DISCONNECTED"));
        }

        return new OpcInterfaceRef(
            iid: requestedInterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: metadata.Oxid,
            oid: metadata.Oid,
            ipid: entry.Ipid,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(
        IReadOnlyList<Item> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var results = new ItemValueResult[items.Count];
        for (int i = 0; i < items.Count; i++)
        {
            string itemId = items[i].ItemName;
            if (_model.TryGetTag(itemId, out SimulatedTag tag))
            {
                results[i] = new ItemValueResult(itemId)
                {
                    Value = _model.CurrentValue(tag, now),
                    Quality = OpcQuality.Good,
                    Timestamp = now,
                    ResultId = OpcResultId.Ok,
                };
            }
            else
            {
                results[i] = new ItemValueResult(itemId)
                {
                    Quality = OpcQuality.Bad,
                    Timestamp = now,
                    ResultId = OpcResultId.Fail,
                };
            }
        }

        return Task.FromResult<IReadOnlyList<ItemValueResult>>(results);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<IdentifiedResult>> WriteVQTAsync(
        IReadOnlyList<ItemValue> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        cancellationToken.ThrowIfCancellationRequested();
        var results = new IdentifiedResult[values.Count];
        for (int i = 0; i < values.Count; i++)
        {
            ItemValue write = values[i];
            bool ok = write.Value is not null && _model.TryWrite(write.ItemName, write.Value);
            results[i] = new IdentifiedResult(write.ItemName)
            {
                ResultId = ok ? OpcResultId.Ok : OpcResultId.Fail,
            };
        }

        return Task.FromResult<IReadOnlyList<IdentifiedResult>>(results);
    }

    private OpcDaGroup CreateGroup(
        string name,
        int clientHandle,
        bool active,
        int requestedUpdateRate,
        int timeBias,
        float percentDeadband,
        int localeId)
    {
        int serverHandle = Interlocked.Increment(ref _nextGroupHandle);
        return new OpcDaGroup(
            name: name,
            serverHandle: serverHandle,
            clientHandle: clientHandle,
            active: active,
            requestedUpdateRate: requestedUpdateRate,
            timeBias: timeBias,
            percentDeadband: percentDeadband,
            localeId: localeId,
            objectRegistry: _objectRegistry,
            callbackSinkFactory: _callbackSinkFactory);
    }

    private static OpcVariant ToVariant(object? value) =>
        value switch
        {
            null => OpcVariant.Empty,
            OpcVariant variant => variant,
            bool b => OpcVariant.FromBoolean(b),
            short s => OpcVariant.FromInt16(s),
            int i => OpcVariant.FromInt32(i),
            float f => OpcVariant.FromSingle(f),
            double d => OpcVariant.FromDouble(d),
            string s => OpcVariant.FromString(s),
            _ => OpcVariant.Empty,
        };

    private sealed class GroupEntry(OpcDaGroup group, Guid ipid)
    {
        public OpcDaGroup Group { get; } = group;

        public Guid Ipid { get; } = ipid;
    }
}
