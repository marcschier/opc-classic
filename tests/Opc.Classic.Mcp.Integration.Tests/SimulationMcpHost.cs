// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.IO.Pipelines;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Opc.Classic.Mcp.Capture;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Samples.SimulationServer;

namespace Opc.Classic.Mcp.Integration.Tests;

/// <summary>
/// Boots the Opc.Classic MCP server in-process over a stream transport and wires it to a
/// live <see cref="SimulationServerHandle" /> so every MCP tool family can be driven
/// end-to-end against the full feature-area simulation. Each instance owns its own
/// simulation (unique channel-name prefix), so tests run isolated and in parallel.
/// </summary>
internal sealed class SimulationMcpHost : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHost _host;
    private readonly SimulationServerHandle _simulation;
    private readonly Pipe _clientToServer;
    private readonly Pipe _serverToClient;

    private SimulationMcpHost(
        IHost host,
        SimulationServerHandle simulation,
        McpClient client,
        Pipe clientToServer,
        Pipe serverToClient)
    {
        _host = host;
        _simulation = simulation;
        Client = client;
        _clientToServer = clientToServer;
        _serverToClient = serverToClient;
    }

    /// <summary>The connected MCP client.</summary>
    public McpClient Client { get; }

    /// <summary>The shared deterministic plant model behind every feature area.</summary>
    public SimulatedPlantModel Model => _simulation.Model;

    /// <summary>Maps each feature-area key (e.g. <c>da</c>) to its <c>inmemory://</c> connection string.</summary>
    public IReadOnlyDictionary<string, string> ConnectionStrings => _simulation.ConnectionStrings;

    /// <summary>The connection string for a feature area; throws if the area is not registered.</summary>
    public string ConnectionString(string spec) => _simulation.ConnectionStrings[spec];

    /// <summary>Creates and starts a host wired to a fresh full simulation instance.</summary>
    public static async Task<SimulationMcpHost> CreateAsync()
    {
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();

        SimulationServerHandle simulation = SimulationServerRegistration.RegisterAll(
            NullLoggerFactory.Instance,
            namePrefix: "simtest-" + Guid.NewGuid().ToString("N"));

        HostApplicationBuilder builder = Host.CreateApplicationBuilder([]);
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<IOpcSessionManager, OpcSessionManager>();
        builder.Services.AddSingleton(sp => new CaptureSessionManager(
            scratchRoot: Path.Combine(Path.GetTempPath(), "opc.classic.simtest.capture", Guid.NewGuid().ToString("N")),
            logger: null));

        // Contribute Discovery / Security services resolved from DI.
        simulation.ConfigureMcpHost(builder.Services);

        builder.Services
            .AddMcpServer()
            .WithStreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream())
            .WithTools<SessionTools>()
            .WithTools<DiscoveryTools>()
            .WithTools<DaClientTools>()
            .WithTools<AeClientTools>()
            .WithTools<HdaClientTools>()
            .WithTools<BatchTools>()
            .WithTools<CommandsTools>()
            .WithTools<CpxTools>()
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

        return new SimulationMcpHost(host, simulation, client, clientToServer, serverToClient);
    }

    /// <summary>Invokes an MCP tool and deserializes its JSON text result into <typeparamref name="T" />.</summary>
    public async Task<T> CallToolAsync<T>(string toolName, Dictionary<string, object> arguments)
    {
        Dictionary<string, object?> nullableArguments = arguments.ToDictionary(
            static pair => pair.Key,
            static pair => (object?)pair.Value);
        CallToolResult result = await Client.CallToolAsync(toolName, nullableArguments).ConfigureAwait(false);
        if (result.IsError == true)
        {
            string error = string.Join(
                "\n",
                result.Content.OfType<TextContentBlock>().Select(static content => content.Text));
            throw new InvalidOperationException("Tool '" + toolName + "' returned an error: " + error);
        }

        TextContentBlock text = result.Content.OfType<TextContentBlock>().Single();
        T? value = JsonSerializer.Deserialize<T>(text.Text, JsonOptions);
        return value ?? throw new InvalidOperationException($"Tool '{toolName}' returned null JSON.");
    }

    /// <summary>Creates a session and returns its id.</summary>
    public async Task<string> CreateSessionAsync()
    {
        OpcSessionDto session = await CallToolAsync<OpcSessionDto>(
            "opcclassic.session.create",
            []).ConfigureAwait(false);
        return session.SessionId;
    }

    public async ValueTask DisposeAsync()
    {
        await Client.DisposeAsync().ConfigureAwait(false);
        await _host.StopAsync().ConfigureAwait(false);
        _host.Dispose();
        _simulation.Dispose();
        await _clientToServer.Reader.CompleteAsync().ConfigureAwait(false);
        await _clientToServer.Writer.CompleteAsync().ConfigureAwait(false);
        await _serverToClient.Reader.CompleteAsync().ConfigureAwait(false);
        await _serverToClient.Writer.CompleteAsync().ConfigureAwait(false);
    }
}
