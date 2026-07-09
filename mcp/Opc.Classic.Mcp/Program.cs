// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Mcp.Capture;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;

var builder = Host.CreateApplicationBuilder(args);

// All logs go to stderr because stdio MCP transports use stdout for protocol traffic.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(static o =>
{
    o.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddSingleton<IOpcSessionManager, OpcSessionManager>();
builder.Services.AddSingleton(sp => new CaptureSessionManager(
    scratchRoot: Path.Combine(Path.GetTempPath(), "opc.classic.mcp.capture"),
    logger: sp.GetService<ILoggerFactory>()?.CreateLogger<CaptureSessionManager>()));

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

await builder.Build().RunAsync().ConfigureAwait(false);
