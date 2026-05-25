// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// All logs go to stderr because stdio MCP transports use stdout for protocol traffic.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(static o =>
{
    o.LogToStandardErrorThreshold = LogLevel.Trace;
});

// MCP server hosted over stdio. Tool classes are wired in WithTools<T>() calls below
// (Wave MCP-2 onward — kept empty here intentionally to keep MCP-1 a pure bootstrap).
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport();

await builder.Build().RunAsync().ConfigureAwait(false);
