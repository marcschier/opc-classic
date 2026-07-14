// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.CttServer;

public sealed class CttDaServer : IOpcDaServer, IDisposable
{
    private static readonly Action<ILogger, Exception?> GetStatusMessage = LoggerMessage.Define(
        LogLevel.Information,
        new EventId(1, nameof(GetStatusAsync)),
        "GetStatus");

    private static readonly Action<ILogger, string, bool, int, Exception?> AddGroupMessage = LoggerMessage.Define<string, bool, int>(
        LogLevel.Information,
        new EventId(2, nameof(AddGroupAsync)),
        "AddGroup: name={Name}, active={Active}, rate={Rate}");

    private static readonly Action<ILogger, int, Exception?> RemoveGroupMessage = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(3, nameof(RemoveGroupAsync)),
        "RemoveGroup: handle={Handle}");

    private static readonly DateTimeOffset StartupTime = DateTimeOffset.UtcNow;
    private readonly ILogger<CttDaServer> _logger;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly IOpcDataCallbackSinkFactory? _callbackSinkFactory;
    // Per-instance group state. Keyed by server-assigned handle. The same
    // entry is tracked in _groupIpids (handle -> IPID) so RemoveGroup can
    // unregister from the OpcObjectRegistry without an extra lookup.
    private readonly ConcurrentDictionary<int, GroupEntry> _groups = new();
    // Atomically-allocated server-handle counter. Starts above the legacy
    // "clientHandle + 1000" sentinel range to make handles easy to recognize
    // in logs while still being correct.
    private int _nextServerHandle = 1_000_000;

    public CttDaServer(
        OpcObjectRegistry objectRegistry,
        ILogger<CttDaServer> logger,
        IOpcDataCallbackSinkFactory? callbackSinkFactory = null)
    {
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _callbackSinkFactory = callbackSinkFactory;
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        GetStatusMessage(_logger, null);
        var now = DateTimeOffset.UtcNow;
        var status = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = StartupTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = _groups.Count,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Opc.Classic .NET DA Sample",
        };

        return Task.FromResult(status);
    }

    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        AddGroupMessage(_logger, name, active, requestedUpdateRate, null);

        OpcDaGroup group = CreateGroup(name, clientHandle, active, requestedUpdateRate, timeBias: 0, percentDeadband: 0f, localeId);
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
        AddGroupMessage(_logger, name, active, requestedUpdateRate, null);

        OpcDaGroup managedGroup = CreateGroup(
            name, clientGroupHandle, active, requestedUpdateRate, timeBias, percentDeadband, localeId);
        serverGroupHandle = managedGroup.ServerHandle;
        revisedUpdateRate = managedGroup.UpdateRate;
        // Register the managed group in the IPID registry so subsequent calls
        // (IOPCGroupStateMgt etc.) carrying the assigned IPID route to this
        // group instance. The dispatcher set is built from the source-generated
        // *ServerDispatcher wrappers around the OpcDaGroup type; additional
        // interfaces (IOPCItemMgt, IOPCSyncIO, ...) plug in here in follow-up
        // commits.
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
        Guid ipid = _objectRegistry.Register(dispatchers);

        if (_groups.TryGetValue(managedGroup.ServerHandle, out GroupEntry? existing))
        {
            // Won't happen with a monotonic counter, but defend against rollover.
            existing.Ipid = ipid;
            existing.Group = managedGroup;
        }
        else
        {
            _groups[managedGroup.ServerHandle] = new GroupEntry(managedGroup, ipid);
        }

        group = new OpcInterfaceRef(
            iid: requestedInterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: unchecked((ulong)managedGroup.ServerHandle),
            ipid: ipid,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());
        return Task.CompletedTask;
    }

    public Task RemoveGroupAsync(
        int serverGroupHandle,
        bool force,
        CancellationToken cancellationToken = default)
    {
        RemoveGroupMessage(_logger, serverGroupHandle, null);
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

    public Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Opc.Classic DA sample error: 0x{errorCode:X8}");
    }

    Task<IOpcInterfaceRef> IOPCServer.GetGroupByNameAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();
        foreach (GroupEntry entry in _groups.Values)
        {
            if (string.Equals(entry.Group.Name, name, StringComparison.Ordinal))
            {
                return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
                    iid: requestedInterfaceId,
                    flags: 0,
                    publicRefs: 1,
                    oxid: 1,
                    oid: unchecked((ulong)entry.Group.ServerHandle),
                    ipid: entry.Ipid,
                    securityOffset: 0,
                    resolverBindings: Array.Empty<ushort>()));
            }
        }
        throw new OpcException(OpcResultId.UnknownPath);
    }

    Task<IOpcInterfaceRef> IOPCServer.CreateGroupEnumeratorAsync(int scope, Guid requestedInterfaceId, CancellationToken cancellationToken)
    {
        _ = OpcDaGroupEnumerationScopeExtensions.FromWireValue(scope);
        cancellationToken.ThrowIfCancellationRequested();
        // Register a fresh IEnumUnknown-like enumerator IPID for the snapshot of groups.
        Guid ipid = _objectRegistry.Register(new Dictionary<Guid, IOpcServerDispatcher>());
        return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
            iid: requestedInterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: 0,
            ipid: ipid,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>()));
    }

    /// <inheritdoc />
    public Task<OpcDaGroup?> ResolveGroupAsync(int serverHandle, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<OpcDaGroup?>(
            _groups.TryGetValue(serverHandle, out GroupEntry? entry) ? entry.Group : null);
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

    /// <summary>
    /// Test helper: returns the number of currently tracked groups.
    /// </summary>
    public int GroupCount => _groups.Count;

    /// <summary>
    /// Test helper: returns the IPID assigned to a registered group.
    /// </summary>
    public Guid? GetIpidForGroup(int serverGroupHandle) =>
        _groups.TryGetValue(serverGroupHandle, out GroupEntry? entry) ? entry.Ipid : null;

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
        return entries
            .OrderBy(static pair => pair.Key)
            .Select(static pair => pair.Value.Group)
            .ToArray();
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
        int serverHandle = Interlocked.Increment(ref _nextServerHandle);
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

    private sealed class GroupEntry
    {
        public GroupEntry(OpcDaGroup group, Guid ipid)
        {
            Group = group;
            Ipid = ipid;
        }

        public OpcDaGroup Group { get; set; }
        public Guid Ipid { get; set; }
    }
}
