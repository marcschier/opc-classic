//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.ComponentModel;
using ModelContextProtocol.Server;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// MCP tools for OPC Classic session lifecycle management.
/// </summary>
public sealed class SessionTools
{
    private readonly IOpcSessionManager _sessionManager;

    /// <summary>
    /// Creates the session tool set.
    /// </summary>
    public SessionTools(IOpcSessionManager sessionManager) =>
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

    /// <summary>
    /// Creates a new OPC Classic session and returns its session identifier.
    /// </summary>
    [McpServerTool(Name = "opcclassic.session.create", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = false)]
    [Description("Creates an OPC Classic MCP session and returns the sessionId used by discovery and DA tools.")]
    public OpcSessionDto CreateSession(
        [Description("Optional idle timeout in seconds. If omitted, the session expires after 30 minutes of inactivity.")]
        int? idleExpirySeconds = null)
    {
        TimeSpan? expiry = idleExpirySeconds is null ? null : TimeSpan.FromSeconds(idleExpirySeconds.Value);
        OpcSession session = _sessionManager.CreateSession(expiry);
        return ToDto(session);
    }

    /// <summary>
    /// Closes an OPC Classic session and releases all associated clients and channels.
    /// </summary>
    [McpServerTool(Name = "opcclassic.session.close", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = false)]
    [Description("Closes an OPC Classic MCP session, releasing all DA groups, subscriptions, clients, and channels.")]
    public OpcResultDto CloseSession(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId)
    {
        bool closed = _sessionManager.CloseSession(sessionId);
        return closed
            ? new OpcResultDto(0, $"Session '{sessionId}' closed.", Succeeded: true)
            : new OpcResultDto(1, $"Session '{sessionId}' was not active.", Succeeded: false);
    }

    /// <summary>
    /// Lists all active OPC Classic sessions.
    /// </summary>
    [McpServerTool(Name = "opcclassic.session.list", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = false)]
    [Description("Lists active OPC Classic MCP sessions, including expiry and DA connection state.")]
    public IReadOnlyList<OpcSessionDto> ListSessions() => _sessionManager.ListSessions();

    private static OpcSessionDto ToDto(OpcSession session)
    {
        DaClientState? da = session.DaClient;
        return new OpcSessionDto(
            session.SessionId,
            session.CreatedAt,
            session.LastUsedAt,
            session.LastUsedAt.Add(session.IdleExpiry),
            checked((int)Math.Ceiling(session.IdleExpiry.TotalSeconds)),
            da is not null,
            da?.Host,
            da?.ProgId,
            da?.Clsid);
    }
}
