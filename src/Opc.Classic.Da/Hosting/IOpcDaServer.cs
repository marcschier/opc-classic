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
/// Contract implemented by user code to provide an in-process managed DA server.
/// The OpcDaServerHost (Phase 6F) marshals incoming IOPCServer + IOPCGroupStateMgt
/// + IOPCSyncIO/AsyncIO calls onto this interface; the user's implementation
/// returns server status, manages groups, and serves item values.
/// </summary>
public interface IOpcDaServer : IOPCServer
{
    /// <summary>Gets the server runtime status snapshot.</summary>
    new Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a DA group and returns its server handle.</summary>
    Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a DA group by server handle.</summary>
    new Task RemoveGroupAsync(
        int serverGroupHandle,
        bool force,
        CancellationToken cancellationToken = default);

    /// <summary>Gets a localized error string for an OPC HRESULT.</summary>
    new Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a server-tracked <see cref="OpcDaGroup"/> instance by its
    /// server handle. Used by the Windows CCW activation path
    /// (<c>OpcDaServerCcw</c>) to look up the managed group after
    /// <c>AddGroup</c>. Returns <see langword="null"/> when the handle is
    /// unknown or the implementation doesn't track groups in-process.
    /// </summary>
    /// <remarks>
    /// The default implementation returns <see langword="null"/>; only
    /// implementations that maintain an internal group dictionary
    /// (e.g. the reference <c>CttDaServer</c>) override it.
    /// </remarks>
    Task<OpcDaGroup?> ResolveGroupAsync(int serverHandle, CancellationToken cancellationToken = default) =>
        Task.FromResult<OpcDaGroup?>(null);

    /// <summary>
    /// Resolves a server-tracked <see cref="OpcDaGroup"/> instance by its
    /// name. Used by the Windows CCW activation path
    /// (<c>OpcDaServerCcw.GetGroupByName</c>) to look up a managed group.
    /// Returns <see langword="null"/> when the name is unknown or the
    /// implementation doesn't track groups in-process.
    /// </summary>
    /// <remarks>
    /// The default implementation returns <see langword="null"/>; only
    /// implementations that maintain an internal group dictionary
    /// (e.g. the reference <c>CttDaServer</c>) override it.
    /// </remarks>
    Task<OpcDaGroup?> ResolveGroupByNameAsync(string name, CancellationToken cancellationToken = default) =>
        Task.FromResult<OpcDaGroup?>(null);

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
        _ = name;
        _ = active;
        _ = requestedUpdateRate;
        _ = clientGroupHandle;
        _ = timeBias;
        _ = percentDeadband;
        _ = localeId;
        _ = requestedInterfaceId;
        _ = cancellationToken;
        serverGroupHandle = 0;
        revisedUpdateRate = 0;
        group = CreateSyntheticInterfaceRef(requestedInterfaceId, 0);
        return Task.FromException(new NotSupportedException("Override the full IOPCServer.AddGroupAsync signature to return an OPC group interface pointer."));
    }

    Task<IOpcInterfaceRef> IOPCServer.GetGroupByNameAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateSyntheticInterfaceRef(requestedInterfaceId, name.GetHashCode(StringComparison.Ordinal)));
    }

    Task<IOpcInterfaceRef> IOPCServer.CreateGroupEnumeratorAsync(int scope, Guid requestedInterfaceId, CancellationToken cancellationToken)
    {
        _ = scope;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateSyntheticInterfaceRef(requestedInterfaceId, 0));
    }

    private static IOpcInterfaceRef CreateSyntheticInterfaceRef(Guid iid, int seed) =>
        new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: unchecked((ulong)(uint)seed),
            ipid: Guid.CreateVersion7(),
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());

    // Future Phase 6F-followup adds item-level read/write and subscriptions.
}
