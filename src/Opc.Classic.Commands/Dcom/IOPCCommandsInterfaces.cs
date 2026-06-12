//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC Commands DCOM-projection interfaces. Generated proxy coverage starts
// with listing, invocation-status, and control methods that can be represented
// with primitive/string payloads.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCCommandInformation)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using Opc.Classic.Generators;

namespace Opc.Classic.Commands.Dcom;

/// <summary>
/// <c>IOPCCommandInformation</c> — Commands metadata interface (IID_IOPCCommandInformation).
/// </summary>
[OpcInterface("3104B525-2016-442D-9696-1275DE978778")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCCommandInformation
{
    /// <summary>
    /// <c>IOPCCommandInformation::QueryCapabilities</c> (opnum 3) projected as maximum storage time.
    /// </summary>
    [OpcMethod(3)]
    Task<double> QueryMaxStorageTimeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandInformation::QueryComands</c> (opnum 4) projected as command names.
    /// </summary>
    [OpcMethod(4)]
    Task<string[]> ListCommandsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandInformation::BrowseCommandTargets</c> (opnum 5) projected as target identifiers.
    /// </summary>
    [OpcMethod(5)]
    Task<string[]> BrowseCommandTargetsAsync(string targetId, string commandNamespace, int browseFilter, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandInformation::GetCommandDescription</c> (opnum 6) projected as descriptive text.
    /// </summary>
    [OpcMethod(6)]
    Task<string> GetCommandDescriptionAsync(string commandName, string commandNamespace, CancellationToken cancellationToken = default);
}

/// <summary>
/// <c>IOPCCommandExecution</c> — Commands execution interface (IID_IOPCCommandExecution).
/// </summary>
[OpcInterface("3104B526-2016-442D-9696-1275DE978778")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCCommandExecution
{
    /// <summary>
    /// <c>IOPCCommandExecution::SyncInvoke</c> (opnum 3) projected as string result arguments.
    /// </summary>
    [OpcMethod(3)]
    Task<string[]> SyncInvokeAsync(string commandName, string commandNamespace, string targetId, string[] arguments, string[] filters, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandExecution::AsyncInvoke</c> (opnum 4) projected as invocation UUID.
    /// </summary>
    [OpcMethod(4)]
    Task<string> AsyncInvokeAsync(string commandName, string commandNamespace, string targetId, string[] arguments, string[] filters, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandExecution::Connect</c> (opnum 5).
    /// </summary>
    [OpcMethod(5)]
    Task<int> ConnectAsync(string invokeUuid, int updateFrequency, int keepAliveTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandExecution::Disconnect</c> (opnum 6).
    /// </summary>
    [OpcMethod(6)]
    Task DisconnectAsync(string invokeUuid, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandExecution::QueryState</c> (opnum 7) projected as permitted controls.
    /// </summary>
    [OpcMethod(7)]
    Task<string[]> QueryStateAsync(string invokeUuid, int waitTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommandExecution::Control</c> (opnum 8).
    /// </summary>
    [OpcMethod(8)]
    Task ControlAsync(string invokeUuid, string control, CancellationToken cancellationToken = default);
}

/// <summary>
/// <c>IOPCCommandCallback</c> — Commands progress / completion sink (IID_IOPCCommandCallback).
/// </summary>
[OpcInterface("3104B527-2016-442D-9696-1275DE978778")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCCommandCallback
{
    /// <summary>
    /// <c>IOPCCommandCallback::OnStateChange</c> (opnum 3) projected as a compact callback notification.
    /// </summary>
    [OpcMethod(3)]
    Task OnStateChangeAsync(int eventCount, string[] permittedControls, bool noStateChange, CancellationToken cancellationToken = default);
}
