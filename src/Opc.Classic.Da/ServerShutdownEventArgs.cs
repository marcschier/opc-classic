//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Da;

/// <summary>
/// Payload of <see cref="IDaServer.ServerShutdown"/>. Servers MAY supply a
/// human-readable reason via <c>IOPCShutdown::ShutdownRequest</c>.
/// </summary>
public sealed class ServerShutdownEventArgs : EventArgs
{
    /// <summary>Server-supplied reason text (may be empty).</summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>UTC time the shutdown notification was received.</summary>
    public DateTimeOffset Time { get; init; } = DateTimeOffset.UtcNow;
}
