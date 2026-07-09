// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Da;

/// <summary>
/// Data Access (DA) feature-area module backed by the shared simulation plant model.
/// </summary>
public sealed class SimDaModule : ISimulationModule
{
    /// <summary>Initializes a new instance of the <see cref="SimDaModule" /> class.</summary>
    public SimDaModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "da";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var server = new SimDaServer(context.Model, context.LoggerFactory);
        string channelName = context.ChannelName("da");
        IDisposable registration = InMemoryDaConnectionRegistry.Register(channelName, server.Channel);
        return new SimulationConnection("da", context.ConnectionString("da"), registration);
    }
}
