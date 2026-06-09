//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Commands;

/// <summary>
/// Snapshot of an OPC Commands invocation returned by a server.
/// </summary>
public sealed record CommandInvocation(
    Guid InvocationId,
    Guid ClientHandle,
    string ServerName,
    CommandState State,
    int Hresult,
    DateTimeOffset StateTimestamp) {
    /// <summary>Server identifier associated with the command invocation.</summary>
    public string ServerName { get; init; } = ServerName ?? throw new ArgumentNullException(nameof(ServerName));
}
