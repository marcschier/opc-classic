// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Security;

/// <summary>
/// Security feature-area module that contributes an OPC Security client factory to the MCP host.
/// </summary>
public sealed class SimSecurityModule : ISimulationModule
{
    /// <summary>
    /// Creates the Security simulation module.
    /// </summary>
    public SimSecurityModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "security";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return null;
    }

    /// <inheritdoc />
    public void ConfigureMcpHost(SimulationContext context, IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IOpcSecurityClientFactory>(new SimSecurityClientFactory());
    }
}
