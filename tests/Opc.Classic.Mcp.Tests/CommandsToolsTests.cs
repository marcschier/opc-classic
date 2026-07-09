// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Commands.Dcom;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Mcp.Tests;

public sealed class CommandsToolsTests
{
    [Test]
    public async Task Commands_tools_get_status_descriptions_and_disconnect_via_mcp_client()
    {
        var syntheticCommands = new SyntheticCommandsServer();
        string channelName = "commands-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryCommandsConnectionRegistry.Register(channelName, syntheticCommands.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcResultDto connected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);
        OpcResultDto status = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.get_status",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcCommandDescriptionDto[] descriptions = await server.CallToolAsync<OpcCommandDescriptionDto[]>(
            "opcclassic.commands.get_command_descriptions",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["commandNamespace"] = "unit-test",
            }).ConfigureAwait(false);
        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Message).Contains("commands=2");
        await Assert.That(descriptions.Select(static description => description.CommandName)).Contains("Start");
        await Assert.That(descriptions[0].CommandNamespace).IsEqualTo("unit-test");
        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    [Test]
    public async Task Commands_tools_invoke_poll_and_cancel_via_mcp_client()
    {
        var syntheticCommands = new SyntheticCommandsServer();
        string channelName = "commands-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryCommandsConnectionRegistry.Register(channelName, syntheticCommands.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcCommandInvocationDto invocation = await server.CallToolAsync<OpcCommandInvocationDto>(
            "opcclassic.commands.invoke_command",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["commandName"] = "Start",
                ["targetId"] = "Unit1",
                ["arguments"] = new[] { "fast" },
                ["asynchronous"] = true,
                ["updateFrequencyMs"] = 250,
            }).ConfigureAwait(false);
        OpcCommandStateDto state = await server.CallToolAsync<OpcCommandStateDto>(
            "opcclassic.commands.poll_command_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["invocationId"] = invocation.InvocationId!,
            }).ConfigureAwait(false);
        OpcResultDto cancelled = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.commands.cancel_command",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["invocationId"] = invocation.InvocationId!,
            }).ConfigureAwait(false);

        await Assert.That(invocation.InvocationId).IsEqualTo("invoke-1");
        await Assert.That(invocation.RevisedUpdateFrequencyMs).IsEqualTo(250);
        await Assert.That(state.PermittedControls).Contains("Cancel");
        await Assert.That(state.NoStateChange).IsFalse();
        await Assert.That(cancelled.Succeeded).IsTrue();
        await Assert.That(syntheticCommands.LastControl).IsEqualTo("Cancel");
        await Assert.That(syntheticCommands.DisconnectedInvocationId).IsEqualTo("invoke-1");
    }

    private sealed class SyntheticCommandsServer
    {
        public SyntheticCommandsServer() => Channel = new InMemoryCallChannel(DispatchAsync);

        public InMemoryCallChannel Channel { get; }
        public string? LastControl { get; private set; }
        public string? DisconnectedInvocationId { get; private set; }

        private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (interfaceId == IOPCCommandInformation.InterfaceId)
            {
                return DispatchCommandInformation(opnum, requestPayload);
            }

            if (interfaceId == IOPCCommandExecution.InterfaceId)
            {
                return DispatchCommandExecution(opnum, requestPayload);
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> DispatchCommandInformation(int opnum, ReadOnlyMemory<byte> requestPayload)
        {
            if (opnum == IOPCCommandInformation.Opnums.QueryMaxStorageTimeAsync)
            {
                return Result((ref NdrWriter writer) => writer.WriteDouble(60));
            }

            if (opnum == IOPCCommandInformation.Opnums.ListCommandsAsync)
            {
                return Result((ref NdrWriter writer) => WriteStringArray(ref writer, "Start", "Stop"));
            }

            if (opnum == IOPCCommandInformation.Opnums.GetCommandDescriptionAsync)
            {
                var reader = new NdrReader(requestPayload.Span);
                string commandName = reader.ReadUnicodeStringPtr() ?? string.Empty;
                _ = reader.ReadUnicodeStringPtr();
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Description for " + commandName));
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private Task<NdrCallResult> DispatchCommandExecution(int opnum, ReadOnlyMemory<byte> requestPayload)
        {
            if (opnum == IOPCCommandExecution.Opnums.AsyncInvokeAsync)
            {
                return Result((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("invoke-1"));
            }

            if (opnum == IOPCCommandExecution.Opnums.ConnectAsync)
            {
                return Result((ref NdrWriter writer) => writer.WriteInt32(250));
            }

            if (opnum == IOPCCommandExecution.Opnums.QueryStateAsync)
            {
                return Result((ref NdrWriter writer) => WriteStringArray(ref writer, "Cancel"));
            }

            if (opnum == IOPCCommandExecution.Opnums.ControlAsync)
            {
                var reader = new NdrReader(requestPayload.Span);
                _ = reader.ReadUnicodeStringPtr();
                LastControl = reader.ReadUnicodeStringPtr();
                return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
            }

            if (opnum == IOPCCommandExecution.Opnums.DisconnectAsync)
            {
                var reader = new NdrReader(requestPayload.Span);
                DisconnectedInvocationId = reader.ReadUnicodeStringPtr();
                return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
            }

            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        private static Task<NdrCallResult> Result(NdrWriteAction write) => Task.FromResult(new NdrCallResult(0, WritePayload(write)));
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 1024)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static void WriteStringArray(ref NdrWriter writer, params string[] values)
    {
        writer.WriteUInt32((uint)values.Length);
        foreach (string value in values)
        {
            writer.WriteUnicodeStringPtr(value);
        }
    }
}
