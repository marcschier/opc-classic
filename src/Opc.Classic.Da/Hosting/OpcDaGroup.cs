//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// In-memory managed implementation of an OPC DA group exposing the group-level
/// COM interfaces (<c>IOPCGroupStateMgt</c>, <c>IOPCGroupStateMgt2</c>). The
/// host wraps the group in source-generated <c>*ServerDispatcher</c> classes
/// and registers them in the <see cref="Opc.Classic.Dcom.Transport.OpcObjectRegistry"/>
/// so per-call <c>RequestCoPdu.Object</c> UUIDs route to the correct group
/// instance.
/// </summary>
/// <remarks>
/// <para>
/// ocom-3b focuses on the AddGroup → IPID-registration → group-state-routing
/// loop using only the lightest interface set
/// (<c>IOPCGroupStateMgt</c> + <c>IOPCGroupStateMgt2</c>). Item management
/// (<c>IOPCItemMgt</c>), synchronous IO (<c>IOPCSyncIO</c> /
/// <c>IOPCSyncIO2</c>), asynchronous IO (<c>IOPCAsyncIO2</c> /
/// <c>IOPCAsyncIO3</c>), and data callbacks (<c>IOPCDataCallback</c>) are
/// follow-up commits — the additional interfaces plug into the existing
/// registration shape by adding more dispatchers to the per-object map.
/// </para>
/// <para>
/// State is mutated from a single connection thread per call (the
/// <see cref="Opc.Classic.Dcom.Transport.RpcServerConnectionProcessor"/>'s
/// request loop). For multi-connection clients sharing a group, callers
/// must externally synchronize; future work may add a per-group lock.
/// </para>
/// </remarks>
public sealed class OpcDaGroup : IOPCGroupStateMgt, IOPCGroupStateMgt2
{
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
        // Clone is not yet supported - returns an interface ref pointing back
        // to this group (callers see a usable IPID but the clone is not a
        // separate object). Real clone semantics arrive in a follow-up.
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
}
