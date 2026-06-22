// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.ComponentModel;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Commands.Dcom;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// Creates Commands client state for a session.
/// </summary>
public interface IOpcCommandsConnectionFactory
{
    /// <summary>
    /// Connects to a Commands server and returns a client state object.
    /// </summary>
    Task<CommandsClientState> ConnectAsync(CommandsConnectionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Connection request used by Commands tools.
/// </summary>
public sealed record CommandsConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString,
    string? AuthLevel = null);

/// <summary>
/// Registers in-memory Commands call channels for MCP tests and loopback scenarios.
/// </summary>
public static class InMemoryCommandsConnectionRegistry
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ICallChannel> Channels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers an in-memory Commands call channel by name.
    /// </summary>
    public static IDisposable Register(string name, ICallChannel channel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(channel);

        Channels[name] = channel;
        return new Registration(name);
    }

    internal static bool TryGet(string name, out ICallChannel channel) => Channels.TryGetValue(name, out channel!);

    private sealed class Registration : IDisposable
    {
        private readonly string _name;
        private bool _disposed;

        public Registration(string name) => _name = name;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Channels.TryRemove(_name, out _);
        }
    }
}

/// <summary>
/// MCP tools for OPC Commands client operations.
/// </summary>
public sealed class CommandsTools
{
    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcCommandsConnectionFactory _connectionFactory;

    /// <summary>
    /// Creates the Commands tool set.
    /// </summary>
    public CommandsTools(IOpcSessionManager sessionManager, IEnumerable<IOpcCommandsConnectionFactory> connectionFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(connectionFactories);
        _connectionFactory = connectionFactories.FirstOrDefault() ?? new DefaultOpcCommandsConnectionFactory();
    }

    /// <summary>
    /// Connects a session to an OPC Commands server.
    /// </summary>
    [McpServerTool(Name = "opcclassic.commands.connect", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Connects an existing MCP session to an OPC Commands server using DCOM or an in-memory test channel.")]
    public async Task<OpcResultDto> Connect(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId,
        [Description("OPC Commands server host name or IP address. Ignored when connectionString uses inmemory://.")]
        string host = "localhost",
        [Description("OPC Commands server ProgID. Optional when clsid or connectionString is supplied.")]
        string? progId = null,
        [Description("OPC Commands server CLSID as a GUID string. Optional when progId or connectionString is supplied.")]
        string? clsid = null,
        [Description("Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\\user when a Windows domain is required.")]
        string? username = null,
        [Description("Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections.")]
        string? password = null,
        [Description("True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied.")]
        bool useKerberos = false,
        [Description("Optional connection string. Use inmemory://name for a registered InMemoryCommandsConnectionRegistry channel, or dcom://host/ProgID for DCOM.")]
        string? connectionString = null,
        [Description(OpcMcpAuthLevel.Description)]
        string? authLevel = null,
        CancellationToken cancellationToken = default)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        CommandsClientState client = await _connectionFactory.ConnectAsync(
            new CommandsConnectionRequest(host, progId, clsid, username, password, useKerberos, connectionString, authLevel),
            cancellationToken).ConfigureAwait(false);

        CommandsClientState? existing = session.CommandsClient;
        session.CommandsClient = client;
        if (existing is not null)
        {
            await existing.DisposeAsync().ConfigureAwait(false);
        }

        session.Touch();
        return new OpcResultDto(0, "Commands client connected.", Succeeded: true, ValueType: "Commands");
    }

    /// <summary>
    /// Gets OPC Commands connection status for a connected session.
    /// </summary>
    [McpServerTool(Name = "opcclassic.commands.get_status", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets OPC Commands connection status, maximum storage time, and command count.")]
    public async Task<OpcResultDto> GetStatus(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.commands.connect.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        CommandsClientState client = GetCommandsClient(sessionId);
        double maxStorageTime = await client.CommandInformation.QueryMaxStorageTimeAsync(cancellationToken).ConfigureAwait(false);
        string[] commandNames = await client.CommandInformation.ListCommandsAsync(cancellationToken).ConfigureAwait(false);
        return new OpcResultDto(0, $"Commands client connected to {client.Host}; maxStorageTime={maxStorageTime}; commands={commandNames.Length}.", Succeeded: true, ValueType: "Commands");
    }

    /// <summary>
    /// Disconnects a session from its OPC Commands server.
    /// </summary>
    [McpServerTool(Name = "opcclassic.commands.disconnect", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Disconnects the session from its OPC Commands server and releases the Commands channel.")]
    public async Task<OpcResultDto> Disconnect(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        CommandsClientState? client = session.CommandsClient;
        session.CommandsClient = null;
        if (client is not null)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            return new OpcResultDto(0, "Commands client disconnected.", Succeeded: true, ValueType: "Commands");
        }

        return new OpcResultDto(1, "Commands client was not connected.", Succeeded: false, ValueType: "Commands");
    }

    /// <summary>
    /// Gets OPC Commands command descriptions.
    /// </summary>
    [McpServerTool(Name = "opcclassic.commands.get_command_descriptions", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Lists command names and retrieves the server description text for each command.")]
    public async Task<IReadOnlyList<OpcCommandDescriptionDto>> GetCommandDescriptions(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Optional command namespace. Use an empty string for the server default namespace.")]
        string commandNamespace = "",
        [Description("Optional command names to describe. Omit or pass an empty array to describe all commands returned by the server.")]
        string[]? commandNames = null,
        CancellationToken cancellationToken = default)
    {
        CommandsClientState client = GetCommandsClient(sessionId);
        string[] names = commandNames is { Length: > 0 }
            ? commandNames
            : await client.CommandInformation.ListCommandsAsync(cancellationToken).ConfigureAwait(false);

        var descriptions = new List<OpcCommandDescriptionDto>(names.Length);
        foreach (string name in names)
        {
            string description = await client.CommandInformation.GetCommandDescriptionAsync(
                name,
                commandNamespace ?? string.Empty,
                cancellationToken).ConfigureAwait(false);
            descriptions.Add(new OpcCommandDescriptionDto(name, commandNamespace ?? string.Empty, description));
        }

        return descriptions;
    }

    /// <summary>
    /// Invokes an OPC Commands command.
    /// </summary>
    [McpServerTool(Name = "opcclassic.commands.invoke_command", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Invokes an OPC Commands command. Asynchronous invocations return an invocationId for polling and cancellation.")]
    public async Task<OpcCommandInvocationDto> InvokeCommand(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Command name returned by opcclassic.commands.get_command_descriptions.")]
        string commandName,
        [Description("Optional command namespace. Use an empty string for the server default namespace.")]
        string commandNamespace = "",
        [Description("Command target identifier. Use an empty string for server-level commands.")]
        string targetId = "",
        [Description("Command arguments, in server-defined order.")]
        string[]? arguments = null,
        [Description("Optional command result filters, in server-defined order.")]
        string[]? filters = null,
        [Description("True to use async invocation and return an invocationId; false to block for synchronous results.")]
        bool asynchronous = true,
        [Description("Requested async state-update frequency in milliseconds.")]
        int updateFrequencyMs = 1000,
        [Description("Requested async keep-alive time in milliseconds.")]
        int keepAliveTimeMs = 30000,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        CommandsClientState client = GetCommandsClient(sessionId);
        string[] actualArguments = arguments ?? [];
        string[] actualFilters = filters ?? [];
        string actualNamespace = commandNamespace ?? string.Empty;
        string actualTargetId = targetId ?? string.Empty;

        if (!asynchronous)
        {
            string[] results = await client.CommandExecution.SyncInvokeAsync(
                commandName,
                actualNamespace,
                actualTargetId,
                actualArguments,
                actualFilters,
                cancellationToken).ConfigureAwait(false);
            return new OpcCommandInvocationDto(
                null,
                commandName,
                actualNamespace,
                actualTargetId,
                Asynchronous: false,
                results,
                0,
                "Command completed synchronously.",
                Succeeded: true);
        }

        string invocationId = await client.CommandExecution.AsyncInvokeAsync(
            commandName,
            actualNamespace,
            actualTargetId,
            actualArguments,
            actualFilters,
            cancellationToken).ConfigureAwait(false);
        int revisedUpdateFrequency = await client.CommandExecution.ConnectAsync(
            invocationId,
            updateFrequencyMs,
            keepAliveTimeMs,
            cancellationToken).ConfigureAwait(false);
        client.Invocations[invocationId] = new CommandsInvocationContext(invocationId, commandName, actualNamespace, actualTargetId, DateTimeOffset.UtcNow);

        return new OpcCommandInvocationDto(
            invocationId,
            commandName,
            actualNamespace,
            actualTargetId,
            Asynchronous: true,
            Array.Empty<string>(),
            0,
            "Command invoked asynchronously. Poll with opcclassic.commands.poll_command_state.",
            Succeeded: true,
            revisedUpdateFrequency,
            Array.Empty<string>());
    }

    /// <summary>
    /// Polls OPC Commands state-change notifications.
    /// </summary>
    [McpServerTool(Name = "opcclassic.commands.poll_command_state", ReadOnly = true, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Polls a command invocation for state-change notifications using IOPCCommandExecution::QueryState.")]
    public async Task<OpcCommandStateDto> PollCommandState(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Invocation identifier returned by opcclassic.commands.invoke_command.")]
        string invocationId,
        [Description("Server wait time in milliseconds for QueryState. Use 0 for a non-blocking poll.")]
        int waitTimeMs = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invocationId);
        CommandsClientState client = GetCommandsClient(sessionId);
        string[] permittedControls = await client.CommandExecution.QueryStateAsync(invocationId, waitTimeMs, cancellationToken).ConfigureAwait(false);
        CommandsInvocationContext context = client.Invocations.GetOrAdd(
            invocationId,
            static id => new CommandsInvocationContext(id, string.Empty, string.Empty, string.Empty, DateTimeOffset.UtcNow));

        bool noStateChange = context.LastPermittedControls.SequenceEqual(permittedControls, StringComparer.Ordinal);
        if (!noStateChange)
        {
            context.EventCount++;
            context.LastPermittedControls = permittedControls;
        }

        string state = noStateChange ? "Unchanged" : "Changed";
        return new OpcCommandStateDto(
            invocationId,
            context.EventCount,
            permittedControls,
            noStateChange,
            state,
            0,
            noStateChange ? "No state change." : "Command state changed.",
            Succeeded: true,
            DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Cancels an OPC Commands invocation.
    /// </summary>
    [McpServerTool(Name = "opcclassic.commands.cancel_command", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Sends the Cancel control to an OPC Commands invocation and disconnects the poll connection.")]
    public async Task<OpcResultDto> CancelCommand(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Invocation identifier returned by opcclassic.commands.invoke_command.")]
        string invocationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(invocationId);
        CommandsClientState client = GetCommandsClient(sessionId);
        await client.CommandExecution.ControlAsync(invocationId, "Cancel", cancellationToken).ConfigureAwait(false);
        await client.CommandExecution.DisconnectAsync(invocationId, cancellationToken).ConfigureAwait(false);
        client.Invocations.TryRemove(invocationId, out _);
        return new OpcResultDto(0, $"Command invocation '{invocationId}' cancelled.", Succeeded: true, ItemName: invocationId, ValueType: "Commands");
    }

    private CommandsClientState GetCommandsClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        return session.CommandsClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC Commands server. Call opcclassic.commands.connect first.");
    }

    private sealed class DefaultOpcCommandsConnectionFactory : IOpcCommandsConnectionFactory
    {
        public Task<CommandsClientState> ConnectAsync(CommandsConnectionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            return OpcClassicDcomConnectionFactory.ConnectAsync(
                new OpcClassicConnectionRequest(request.Host, request.ProgId, request.Clsid, request.Username, request.Password, request.UseKerberos, request.ConnectionString, request.AuthLevel),
                IOPCCommandInformation.InterfaceId,
                OpcGuids.CommandsCategoryIds,
                static (host, progId, clsid, channel, ownsChannel) => new CommandsClientState(host, progId, clsid, channel, ownsChannel),
                InMemoryCommandsConnectionRegistry.TryGet,
                "Commands",
                cancellationToken);
        }
    }
}
