// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Mcp.Capture;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Samples.SimulationServer;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// All logs go to stderr because stdio MCP transports use stdout for protocol traffic.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(static o => o.LogToStandardErrorThreshold = LogLevel.Trace);

using ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
{
    b.ClearProviders();
    b.AddConsole(static o => o.LogToStandardErrorThreshold = LogLevel.Trace);
});

// Stand up the full feature-area simulation and register every inmemory:// endpoint.
SimulationServerHandle simulation = SimulationServerRegistration.RegisterAll(
    loggerFactory,
    namePrefix: "sim");

builder.Services.AddSingleton(simulation);
builder.Services.AddSingleton<IOpcSessionManager, OpcSessionManager>();
builder.Services.AddSingleton(sp => new CaptureSessionManager(
    scratchRoot: Path.Combine(Path.GetTempPath(), "opc.classic.simulation.capture"),
    logger: sp.GetService<ILoggerFactory>()?.CreateLogger<CaptureSessionManager>()));

// Contribute Discovery / Security services that are resolved from DI rather than a registry.
simulation.ConfigureMcpHost(builder.Services);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
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
    .WithTools<XmlDaTools>()
    .WithTools<CaptureTools>();

ILogger startupLogger = loggerFactory.CreateLogger("Opc.Classic.Samples.SimulationServer");
startupLogger.LogInformation("Simulation server starting.");

Console.Error.WriteLine(string.Create(
    CultureInfo.InvariantCulture,
    $"Simulation server ready: {simulation.Model.Tags.Count} tags, {simulation.ConnectionStrings.Count} feature-area endpoints."));
foreach (KeyValuePair<string, string> endpoint in simulation.ConnectionStrings)
{
    Console.Error.WriteLine(string.Create(
        CultureInfo.InvariantCulture,
        $"  {endpoint.Key,-9} -> {endpoint.Value}"));
}

Console.Error.WriteLine(string.Create(
    CultureInfo.InvariantCulture,
    $"Discovery host: {Opc.Classic.Samples.SimulationServer.Discovery.SimDiscoveryModule.DiscoveryHost}. Connect MCP sessions with the connection strings above."));

await builder.Build().RunAsync().ConfigureAwait(false);

simulation.Dispose();
