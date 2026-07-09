// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Commands;

/// <summary>
/// Commands feature-area module for the simulation server.
/// </summary>
public sealed class SimCommandsModule : ISimulationModule
{
    /// <summary>Initializes a new instance of the <see cref="SimCommandsModule" /> class.</summary>
    public SimCommandsModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "commands";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var server = new SimCommandsServer(context.LoggerFactory);
        IDisposable registration = InMemoryCommandsConnectionRegistry.Register(context.ChannelName("commands"), server.Channel);
        return new SimulationConnection("commands", context.ConnectionString("commands"), registration);
    }
}
