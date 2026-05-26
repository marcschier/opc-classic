// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.CttServer;

public sealed class CttDaServer : IOpcDaServer
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

    // Per-CTT-instance group state. Keyed by server-assigned handle. The same
    // entry is tracked in _groupIpids (handle -> IPID) so RemoveGroup can
    // unregister from the OpcObjectRegistry without an extra lookup.
    private readonly ConcurrentDictionary<int, GroupEntry> _groups = new();

    // Atomically-allocated server-handle counter. Starts above the legacy
    // "clientHandle + 1000" sentinel range to make handles easy to recognize
    // in logs while still being correct.
    private int _nextServerHandle = 1_000_000;

    public CttDaServer(OpcObjectRegistry objectRegistry, ILogger<CttDaServer> logger)
    {
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            VendorInfo = "Opc.Classic .NET CTT Sample",
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
        }
        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult($"Opc.Classic CTT sample error: 0x{errorCode:X8}");
    }

    /// <summary>Test helper: returns the number of currently tracked groups.</summary>
    public int GroupCount => _groups.Count;

    /// <summary>Test helper: returns the IPID assigned to a registered group.</summary>
    public Guid? GetIpidForGroup(int serverGroupHandle) =>
        _groups.TryGetValue(serverGroupHandle, out GroupEntry? entry) ? entry.Ipid : null;

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
            localeId: localeId);
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
