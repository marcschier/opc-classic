//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Sessions;

/// <summary>Manages long-lived OPC Classic sessions for MCP tools.</summary>
public interface IOpcSessionManager
{
    /// <summary>Creates a new OPC Classic session.</summary>
    OpcSession CreateSession(TimeSpan? idleExpiry = null);

    /// <summary>Gets an active session or throws if the session is missing or expired.</summary>
    OpcSession GetSession(string sessionId);

    /// <summary>Attempts to get an active session.</summary>
    bool TryGetSession(string sessionId, out OpcSession session);

    /// <summary>Closes and removes a session.</summary>
    bool CloseSession(string sessionId);

    /// <summary>Lists active sessions.</summary>
    IReadOnlyList<OpcSessionDto> ListSessions();
}
