// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Ae;

/// <summary>
/// Alarms &amp; Events (AE) feature-area module for the simulation server.
/// </summary>
public sealed class SimAeModule : ISimulationModule
{
    /// <summary>Initializes a new instance of the <see cref="SimAeModule" /> class.</summary>
    public SimAeModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "ae";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var server = new SimAeServer(context.Model, context.LoggerFactory);
        IDisposable registration = InMemoryAeConnectionRegistry.Register(context.ChannelName("ae"), server.Channel, server);
        return new SimulationConnection("ae", context.ConnectionString("ae"), registration);
    }
}
