// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Cpx;

/// <summary>
/// Complex Data feature-area module that exposes type metadata and dictionaries over a
/// dedicated DA in-memory channel.
/// </summary>
public sealed class SimCpxModule : ISimulationModule
{
    /// <summary>Creates the Complex Data feature-area module.</summary>
    public SimCpxModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "cpx";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var server = new SimCpxServer(context.LoggerFactory);
        IDisposable registration = InMemoryDaConnectionRegistry.Register(context.ChannelName("cpx"), server.Channel);
        return new SimulationConnection("cpx", context.ConnectionString("cpx"), registration);
    }
}
