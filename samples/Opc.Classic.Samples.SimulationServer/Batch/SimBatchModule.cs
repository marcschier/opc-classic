// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Batch;

/// <summary>
/// Batch feature-area module that hosts an in-memory Batch server and registers
/// it with <see cref="InMemoryBatchConnectionRegistry" />.
/// </summary>
public sealed class SimBatchModule : ISimulationModule
{
    /// <summary>Initializes a new instance of the <see cref="SimBatchModule" /> class.</summary>
    public SimBatchModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "batch";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var server = new SimBatchServer(context.LoggerFactory);
        IDisposable registration = InMemoryBatchConnectionRegistry.Register(context.ChannelName("batch"), server.Channel);
        return new SimulationConnection("batch", context.ConnectionString("batch"), registration);
    }
}
