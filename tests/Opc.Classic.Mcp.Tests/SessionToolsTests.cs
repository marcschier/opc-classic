//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.IO.Pipelines;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;
using TUnit.Core;

namespace Opc.Classic.Mcp.Tests;

public sealed class SessionToolsTests
{
    [Test]
    public async Task Session_tools_create_list_and_close_session_via_mcp_client()
    {
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);

        OpcSessionDto created = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.session.create",
            new Dictionary<string, object> { ["idleExpirySeconds"] = 30 }).ConfigureAwait(false);
        IReadOnlyList<OpcSessionDto> sessions = await server.CallToolAsync<IReadOnlyList<OpcSessionDto>>(
            "opcclassic.session.list",
            []).ConfigureAwait(false);
        OpcResultDto closed = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.session.close",
            new Dictionary<string, object> { ["sessionId"] = created.SessionId }).ConfigureAwait(false);
        IReadOnlyList<OpcSessionDto> afterClose = await server.CallToolAsync<IReadOnlyList<OpcSessionDto>>(
            "opcclassic.session.list",
            []).ConfigureAwait(false);

        await Assert.That(created.SessionId).IsNotNull();
        await Assert.That(sessions.Any(session => session.SessionId == created.SessionId)).IsTrue();
        await Assert.That(closed.Succeeded).IsTrue();
        await Assert.That(afterClose.Any(session => session.SessionId == created.SessionId)).IsFalse();
    }

    [Test]
    public async Task Session_manager_expires_idle_sessions()
    {
        using var manager = new OpcSessionManager();
        OpcSession session = manager.CreateSession(TimeSpan.FromMilliseconds(25));

        await Task.Delay(TimeSpan.FromMilliseconds(75)).ConfigureAwait(false);

        bool found = manager.TryGetSession(session.SessionId, out _);
        await Assert.That(found).IsFalse();
        await Assert.That(manager.ListSessions().Count).IsEqualTo(0);
    }
}

internal sealed class McpTestServer : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHost _host;
    private readonly Pipe _clientToServer;
    private readonly Pipe _serverToClient;

    private McpTestServer(IHost host, McpClient client, Pipe clientToServer, Pipe serverToClient)
    {
        _host = host;
        Client = client;
        _clientToServer = clientToServer;
        _serverToClient = serverToClient;
    }

    public McpClient Client { get; }

    public static async Task<McpTestServer> CreateAsync(Action<IServiceCollection>? configureServices = null)
    {
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();
        HostApplicationBuilder builder = Host.CreateApplicationBuilder([]);
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<IOpcSessionManager, OpcSessionManager>();
        configureServices?.Invoke(builder.Services);
        builder.Services
            .AddMcpServer()
            .WithStreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream())
            .WithTools<SessionTools>()
            .WithTools<DiscoveryTools>()
            .WithTools<DaClientTools>()
            .WithTools<DxTools>()
            .WithTools<SecurityTools>()
            .WithTools<XmlDaTools>();

        IHost host = builder.Build();
        await host.StartAsync().ConfigureAwait(false);

        var transport = new StreamClientTransport(
            clientToServer.Writer.AsStream(),
            serverToClient.Reader.AsStream(),
            NullLoggerFactory.Instance);
        McpClient client = await McpClient.CreateAsync(
            transport,
            loggerFactory: NullLoggerFactory.Instance).ConfigureAwait(false);

        return new McpTestServer(host, client, clientToServer, serverToClient);
    }

    public async Task<T> CallToolAsync<T>(string toolName, Dictionary<string, object> arguments)
    {
        Dictionary<string, object?> nullableArguments = arguments.ToDictionary(static pair => pair.Key, static pair => (object?)pair.Value);
        CallToolResult result = await Client.CallToolAsync(toolName, nullableArguments).ConfigureAwait(false);
        if (result.IsError == true)
        {
            string error = string.Join("\n", result.Content.OfType<TextContentBlock>().Select(static content => content.Text));
            throw new InvalidOperationException(error);
        }

        TextContentBlock text = result.Content.OfType<TextContentBlock>().Single();
        T? value = JsonSerializer.Deserialize<T>(text.Text, JsonOptions);
        return value ?? throw new InvalidOperationException($"Tool '{toolName}' returned null JSON.");
    }

    public async ValueTask DisposeAsync()
    {
        await Client.DisposeAsync().ConfigureAwait(false);
        await _host.StopAsync().ConfigureAwait(false);
        _host.Dispose();
        await _clientToServer.Reader.CompleteAsync().ConfigureAwait(false);
        await _clientToServer.Writer.CompleteAsync().ConfigureAwait(false);
        await _serverToClient.Reader.CompleteAsync().ConfigureAwait(false);
        await _serverToClient.Writer.CompleteAsync().ConfigureAwait(false);
    }
}
