//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da.Dcom;

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

    // Future Phase 6F-followup adds: GetGroupByName, CreateGroupEnumerator,
    // item-level read/write, subscriptions.
}
