// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Opc.Classic.Commands.Dcom;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.SimulationServer.Commands;

/// <summary>
/// Managed in-memory OPC Commands server with deterministic command metadata, invocation, and state polling.
/// </summary>
public sealed class SimCommandsServer : IOPCCommandInformation, IOPCCommandExecution
{
    private const double MaxStorageTimeSeconds = 300;
    private const int DefaultUpdateFrequencyMilliseconds = 250;

    private static readonly CommandDefinition[] Commands =
    [
        new(
            "Start",
            "Starts a simulated unit. Arguments: mode=normal|fast. Results: command, target, state, mode.",
            ["Cancel", "Hold"]),
        new(
            "Stop",
            "Stops a simulated unit. Arguments: reason=operator|interlock|maintenance. Results: command, target, state, reason.",
            ["Cancel"]),
        new(
            "Reset",
            "Resets a stopped or faulted simulated unit. Arguments: authorization token. Results: command, target, state.",
            ["Cancel"]),
    ];

    private readonly object _gate = new();
    private readonly IOPCCommandInformationServerDispatcher _informationDispatcher;
    private readonly IOPCCommandExecutionServerDispatcher _executionDispatcher;
    private readonly Dictionary<string, CommandDefinition> _commandsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, UnitState> _targetsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, InvocationState> _invocationsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<SimCommandsServer> _logger;
    private int _nextInvocationId;

    /// <summary>Initializes a new instance of the <see cref="SimCommandsServer" /> class.</summary>
    /// <param name="loggerFactory">Logger factory used for Commands diagnostics.</param>
    public SimCommandsServer(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _logger = loggerFactory.CreateLogger<SimCommandsServer>();
        foreach (CommandDefinition command in Commands)
        {
            _commandsByName.Add(command.Name, command);
        }

        _targetsById.Add("Unit1", UnitState.Idle);
        _targetsById.Add("Unit2", UnitState.Running);
        _targetsById.Add("MixerA", UnitState.Held);

        _informationDispatcher = new IOPCCommandInformationServerDispatcher(this);
        _executionDispatcher = new IOPCCommandExecutionServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    /// <summary>Gets the in-memory call channel used by generated Commands proxies.</summary>
    public InMemoryCallChannel Channel { get; }

    Task<double> IOPCCommandInformation.QueryMaxStorageTimeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(MaxStorageTimeSeconds);
    }

    Task<string[]> IOPCCommandInformation.ListCommandsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Commands.Select(static command => command.Name).ToArray());
    }

    Task<string[]> IOPCCommandInformation.BrowseCommandTargetsAsync(
        string targetId,
        string commandNamespace,
        int browseFilter,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = commandNamespace;
        _ = browseFilter;

        string prefix = targetId ?? string.Empty;
        lock (_gate)
        {
            return Task.FromResult(_targetsById.Keys
                .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .Order(StringComparer.OrdinalIgnoreCase)
                .ToArray());
        }
    }

    Task<string> IOPCCommandInformation.GetCommandDescriptionAsync(
        string commandName,
        string commandNamespace,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = commandNamespace;

        CommandDefinition command = GetCommand(commandName);
        return Task.FromResult(command.Description);
    }

    Task<string[]> IOPCCommandExecution.SyncInvokeAsync(
        string commandName,
        string commandNamespace,
        string targetId,
        string[] arguments,
        string[] filters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = commandNamespace;
        _ = filters;

        string normalizedTargetId = NormalizeTargetId(targetId);
        string[] actualArguments = arguments ?? [];
        CommandDefinition command = GetCommand(commandName);
        UnitState newState = ApplyCommand(command, normalizedTargetId, actualArguments);
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Commands sync invoke {Command} on {Target} -> {State}.", command.Name, normalizedTargetId, newState);
        }

        return Task.FromResult(BuildResults(command.Name, normalizedTargetId, newState, actualArguments));
    }

    Task<string> IOPCCommandExecution.AsyncInvokeAsync(
        string commandName,
        string commandNamespace,
        string targetId,
        string[] arguments,
        string[] filters,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = commandNamespace;
        _ = filters;

        string normalizedTargetId = NormalizeTargetId(targetId);
        string[] actualArguments = arguments ?? [];
        CommandDefinition command = GetCommand(commandName);
        string invocationId;
        lock (_gate)
        {
            _nextInvocationId++;
            invocationId = "cmd-" + _nextInvocationId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            _invocationsById.Add(invocationId, new InvocationState(invocationId, command, normalizedTargetId, actualArguments));
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Commands async invoke {InvocationId}: {Command} on {Target}.", invocationId, command.Name, normalizedTargetId);
        }

        return Task.FromResult(invocationId);
    }

    Task<int> IOPCCommandExecution.ConnectAsync(
        string invokeUuid,
        int updateFrequency,
        int keepAliveTime,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = keepAliveTime;

        _ = GetInvocation(invokeUuid);

        return Task.FromResult(updateFrequency > 0 ? updateFrequency : DefaultUpdateFrequencyMilliseconds);
    }

    Task IOPCCommandExecution.DisconnectAsync(string invokeUuid, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _invocationsById.Remove(invokeUuid ?? string.Empty);
        }

        return Task.CompletedTask;
    }

    Task<string[]> IOPCCommandExecution.QueryStateAsync(string invokeUuid, int waitTime, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = waitTime;

        InvocationState invocation = GetInvocation(invokeUuid);
        lock (_gate)
        {
            if (invocation.Stage == InvocationStage.Pending)
            {
                invocation.Stage = InvocationStage.Running;
                return Task.FromResult(invocation.Command.PermittedControls.ToArray());
            }

            if (invocation.Stage == InvocationStage.Running)
            {
                _ = ApplyCommandCore(invocation.Command, invocation.TargetId, invocation.Arguments);
                invocation.Stage = InvocationStage.Completed;
                return Task.FromResult(Array.Empty<string>());
            }

            if (invocation.Stage == InvocationStage.Cancelled)
            {
                return Task.FromResult(Array.Empty<string>());
            }

            return Task.FromResult(Array.Empty<string>());
        }
    }

    Task IOPCCommandExecution.ControlAsync(string invokeUuid, string control, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        InvocationState invocation = GetInvocation(invokeUuid);
        string normalizedControl = control ?? string.Empty;
        lock (_gate)
        {
            if (string.Equals(normalizedControl, "Cancel", StringComparison.OrdinalIgnoreCase))
            {
                invocation.Stage = InvocationStage.Cancelled;
            }
            else if (string.Equals(normalizedControl, "Hold", StringComparison.OrdinalIgnoreCase))
            {
                _targetsById[invocation.TargetId] = UnitState.Held;
                invocation.Stage = InvocationStage.Completed;
            }
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation("Commands control {Control} for {InvocationId}.", normalizedControl, invocation.Id);
        }

        return Task.CompletedTask;
    }

    private async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (interfaceId == IOPCCommandInformation.InterfaceId)
        {
            return (await _informationDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false)).ToNdrCallResult();
        }

        if (interfaceId == IOPCCommandExecution.InterfaceId)
        {
            return (await _executionDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false)).ToNdrCallResult();
        }

        return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
    }

    private CommandDefinition GetCommand(string commandName)
    {
        string normalizedCommandName = commandName ?? string.Empty;
        if (_commandsByName.TryGetValue(normalizedCommandName, out CommandDefinition? command))
        {
            return command;
        }

        throw new OpcException(OpcResultId.InvalidArg, "Unknown command: " + normalizedCommandName);
    }

    private InvocationState GetInvocation(string? invocationId)
    {
        string normalizedInvocationId = invocationId ?? string.Empty;
        lock (_gate)
        {
            if (_invocationsById.TryGetValue(normalizedInvocationId, out InvocationState? invocation))
            {
                return invocation;
            }
        }

        throw new OpcException(OpcResultId.InvalidArg, "Unknown command invocation: " + normalizedInvocationId);
    }

    private UnitState ApplyCommand(CommandDefinition command, string targetId, string[] arguments)
    {
        lock (_gate)
        {
            return ApplyCommandCore(command, targetId, arguments);
        }
    }

    private UnitState ApplyCommandCore(CommandDefinition command, string targetId, string[] arguments)
    {
        UnitState newState = command.Name switch
        {
            "Start" => UnitState.Running,
            "Stop" => UnitState.Stopped,
            "Reset" => UnitState.Idle,
            _ => UnitState.Idle,
        };
        _ = arguments;
        _targetsById[targetId] = newState;
        return newState;
    }

    private static string NormalizeTargetId(string? targetId) =>
        string.IsNullOrWhiteSpace(targetId) ? "Unit1" : targetId;

    private static string[] BuildResults(string commandName, string targetId, UnitState state, string[] arguments)
    {
        if (arguments.Length > 0)
        {
            return ["command=" + commandName, "target=" + targetId, "state=" + state, "arg0=" + arguments[0]];
        }

        return ["command=" + commandName, "target=" + targetId, "state=" + state];
    }

    private sealed record CommandDefinition(string Name, string Description, string[] PermittedControls);

    private enum UnitState
    {
        Idle,
        Running,
        Held,
        Stopped,
    }

    private enum InvocationStage
    {
        Pending,
        Running,
        Completed,
        Cancelled,
    }

    private sealed class InvocationState
    {
        public InvocationState(string id, CommandDefinition command, string targetId, string[] arguments)
        {
            Id = id;
            Command = command;
            TargetId = targetId;
            Arguments = arguments;
        }

        public string Id { get; }
        public CommandDefinition Command { get; }
        public string TargetId { get; }
        public string[] Arguments { get; }
        public InvocationStage Stage { get; set; }
    }
}
