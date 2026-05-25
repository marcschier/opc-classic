// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SessionTools>()
    .WithTools<DiscoveryTools>()
    .WithTools<DaClientTools>()
    .WithTools<BatchTools>()
    .WithTools<CommandsTools>()
    .WithTools<CpxTools>()
    .WithTools<AeClientTools>()
    .WithTools<HdaClientTools>();

await builder.Build().RunAsync().ConfigureAwait(false);

