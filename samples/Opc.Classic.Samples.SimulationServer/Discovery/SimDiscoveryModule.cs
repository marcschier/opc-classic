// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Discovery;

namespace Opc.Classic.Samples.SimulationServer.Discovery;

/// <summary>
/// Discovery (OpcEnum) feature-area module. Implemented by the <c>sim-discovery</c> task:
/// contributes an <c>IOpcDiscovery</c> backed by an in-memory OpcEnum factory that
/// enumerates the simulated servers to the MCP host.
/// </summary>
public sealed class SimDiscoveryModule : ISimulationModule
{
    /// <summary>The simulated discovery host name the OpcEnum factory answers for.</summary>
    public const string DiscoveryHost = "sim-host";

    private static readonly Guid[] DiscoveryCategoryIds =
    [
        OpcGuids.CATID_OPCDAServer20,
        OpcGuids.CATID_OPCDAServer30,
        OpcGuids.CATID_OPCAEServer10,
        OpcGuids.CATID_OPCHDAServer10,
    ];

    /// <summary>
    /// Initializes a discovery simulation module.
    /// </summary>
    public SimDiscoveryModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "discovery";

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

        var factory = new SimOpcEnumFactory();
        services.AddSingleton<IOpcDiscovery>(new OpcEnumClient(
            DiscoveryHost,
            factory,
            DiscoveryCategoryIds));
    }
}
