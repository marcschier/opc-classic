// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class CommandsIntegrationTests
{
    [Test]
    public async Task Commands_tools_get_status_descriptions_and_disconnect_against_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);

        OpcResultDto connected = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("commands"),
            }).ConfigureAwait(false);
        OpcResultDto status = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.get_status",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcCommandDescriptionDto[] descriptions = await host.CallToolAsync<OpcCommandDescriptionDto[]>(
            "opcclassic.commands.get_command_descriptions",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["commandNamespace"] = "simulation",
            }).ConfigureAwait(false);
        OpcResultDto disconnected = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.disconnect",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Message).Contains("commands=3");
        await Assert.That(descriptions.Select(static description => description.CommandName))
            .IsEquivalentTo(new[] { "Start", "Stop", "Reset" });
        await Assert.That(descriptions.All(static description => description.CommandNamespace == "simulation")).IsTrue();
        await Assert.That(descriptions.Single(static description => description.CommandName == "Start").Description)
            .Contains("Starts a simulated unit");
        await Assert.That(descriptions.Single(static description => description.CommandName == "Stop").Description)
            .Contains("Stops a simulated unit");
        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    [Test]
    public async Task Commands_tools_sync_invoke_and_async_poll_to_completion_against_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);
        _ = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("commands"),
            }).ConfigureAwait(false);

        OpcCommandInvocationDto syncInvocation = await host.CallToolAsync<OpcCommandInvocationDto>(
            "opcclassic.commands.invoke_command",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["commandName"] = "Start",
                ["targetId"] = "Unit1",
                ["arguments"] = new[] { "fast" },
                ["asynchronous"] = false,
            }).ConfigureAwait(false);
        OpcCommandInvocationDto asyncInvocation = await host.CallToolAsync<OpcCommandInvocationDto>(
            "opcclassic.commands.invoke_command",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["commandName"] = "Stop",
                ["targetId"] = "Unit1",
                ["arguments"] = new[] { "operator" },
                ["asynchronous"] = true,
                ["updateFrequencyMs"] = 125,
            }).ConfigureAwait(false);
        OpcCommandStateDto runningState = await host.CallToolAsync<OpcCommandStateDto>(
            "opcclassic.commands.poll_command_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["invocationId"] = asyncInvocation.InvocationId!,
            }).ConfigureAwait(false);
        OpcCommandStateDto completedState = await host.CallToolAsync<OpcCommandStateDto>(
            "opcclassic.commands.poll_command_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["invocationId"] = asyncInvocation.InvocationId!,
            }).ConfigureAwait(false);
        OpcCommandStateDto unchangedState = await host.CallToolAsync<OpcCommandStateDto>(
            "opcclassic.commands.poll_command_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["invocationId"] = asyncInvocation.InvocationId!,
            }).ConfigureAwait(false);

        await Assert.That(syncInvocation.Succeeded).IsTrue();
        await Assert.That(syncInvocation.Asynchronous).IsFalse();
        await Assert.That(syncInvocation.InvocationId).IsNull();
        await Assert.That(syncInvocation.Results)
            .IsEquivalentTo(new[] { "command=Start", "target=Unit1", "state=Running", "arg0=fast" });
        await Assert.That(asyncInvocation.Succeeded).IsTrue();
        await Assert.That(asyncInvocation.InvocationId).IsEqualTo("cmd-1");
        await Assert.That(asyncInvocation.RevisedUpdateFrequencyMs).IsEqualTo(125);
        await Assert.That(runningState.InvocationId).IsEqualTo("cmd-1");
        await Assert.That(runningState.EventCount).IsEqualTo(1);
        await Assert.That(runningState.PermittedControls).IsEquivalentTo(new[] { "Cancel" });
        await Assert.That(runningState.NoStateChange).IsFalse();
        await Assert.That(completedState.EventCount).IsEqualTo(2);
        await Assert.That(completedState.PermittedControls).IsEquivalentTo(Array.Empty<string>());
        await Assert.That(completedState.NoStateChange).IsFalse();
        await Assert.That(unchangedState.EventCount).IsEqualTo(2);
        await Assert.That(unchangedState.NoStateChange).IsTrue();
        await Assert.That(unchangedState.Message).IsEqualTo("No state change.");
    }

    [Test]
    public async Task Commands_tools_cancel_async_invocation_after_control_state_against_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);
        _ = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("commands"),
            }).ConfigureAwait(false);

        OpcCommandInvocationDto invocation = await host.CallToolAsync<OpcCommandInvocationDto>(
            "opcclassic.commands.invoke_command",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["commandName"] = "Start",
                ["targetId"] = "MixerA",
                ["arguments"] = new[] { "normal" },
                ["asynchronous"] = true,
                ["updateFrequencyMs"] = 250,
            }).ConfigureAwait(false);
        OpcCommandStateDto state = await host.CallToolAsync<OpcCommandStateDto>(
            "opcclassic.commands.poll_command_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["invocationId"] = invocation.InvocationId!,
            }).ConfigureAwait(false);
        OpcResultDto cancelled = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.cancel_command",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["invocationId"] = invocation.InvocationId!,
            }).ConfigureAwait(false);

        await Assert.That(invocation.InvocationId).IsEqualTo("cmd-1");
        await Assert.That(invocation.TargetId).IsEqualTo("MixerA");
        await Assert.That(invocation.RevisedUpdateFrequencyMs).IsEqualTo(250);
        await Assert.That(state.PermittedControls).IsEquivalentTo(new[] { "Cancel", "Hold" });
        await Assert.That(state.NoStateChange).IsFalse();
        await Assert.That(cancelled.Succeeded).IsTrue();
        await Assert.That(cancelled.ItemName).IsEqualTo("cmd-1");
        await Assert.That(cancelled.Message).Contains("cancelled");
    }
}
