// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Hda;

/// <summary>
/// Historical Data Access (HDA) feature-area module for the simulation server.
/// </summary>
public sealed class SimHdaModule : ISimulationModule
{
    /// <inheritdoc />
    public string Spec => "hda";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var server = new SimHdaServer(context.Model, context.LoggerFactory);
        IDisposable registration = InMemoryHdaConnectionRegistry.Register(context.ChannelName(Spec), server.Channel, server);
        return new SimulationConnection(Spec, context.ConnectionString(Spec), registration);
    }
}
