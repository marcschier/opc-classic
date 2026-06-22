// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Dx;

/// <summary>
/// Data eXchange (DX) feature-area module that registers an in-memory DX client.
/// </summary>
public sealed class SimDxModule : ISimulationModule
{
    /// <summary>
    /// Creates the DX simulation module.
    /// </summary>
    public SimDxModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "dx";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var client = new SimDxClient();
        IDisposable registration = InMemoryDxConnectionRegistry.Register(context.ChannelName(Spec), client);
        return new SimulationConnection(Spec, context.ConnectionString(Spec), registration);
    }
}
