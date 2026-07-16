// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dx;
using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Dx;

/// <summary>
/// Data eXchange (DX) feature-area module that hosts the bounded reference engine.
/// </summary>
public sealed class SimDxModule : ISimulationModule
{
    /// <summary>The live engine-backed DX server after registration.</summary>
    public SimDxClient? Client { get; private set; }

    /// <inheritdoc />
    public string Spec => "dx";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        string? configurationPath =
            Environment.GetEnvironmentVariable("OPC_CLASSIC_SIM_DX_CONFIG");
        JsonFileDxConfigurationStore? fileStore = string.IsNullOrWhiteSpace(configurationPath)
            ? null
            : new JsonFileDxConfigurationStore(configurationPath);
        try
        {
            Client = SimDxClient.CreateAsync(
                context.Model,
                fileStore,
                SystemDxScheduler.Instance).GetAwaiter().GetResult();
            IDisposable registry = InMemoryDxConnectionRegistry.Register(
                context.ChannelName(Spec),
                () => Client ?? throw new InvalidOperationException(
                    "The simulation DX server is not registered."));
            return new SimulationConnection(
                Spec,
                context.ConnectionString(Spec),
                new Registration(registry, Client, fileStore));
        }
        catch
        {
            fileStore?.Dispose();
            throw;
        }
    }

    private sealed class Registration(
        IDisposable registry,
        SimDxClient client,
        IDisposable? store) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            registry.Dispose();
            client.ShutdownAsync().AsTask().GetAwaiter().GetResult();
            store?.Dispose();
        }
    }
}
