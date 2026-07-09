// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Samples.SimulationServer.Ae;
using Opc.Classic.Samples.SimulationServer.Batch;
using Opc.Classic.Samples.SimulationServer.Commands;
using Opc.Classic.Samples.SimulationServer.Cpx;
using Opc.Classic.Samples.SimulationServer.Da;
using Opc.Classic.Samples.SimulationServer.Discovery;
using Opc.Classic.Samples.SimulationServer.Dx;
using Opc.Classic.Samples.SimulationServer.Hda;
using Opc.Classic.Samples.SimulationServer.Security;
using Opc.Classic.Samples.SimulationServer.Xml;

namespace Opc.Classic.Samples.SimulationServer;

/// <summary>
/// Entry point that assembles every OPC Classic feature-area module over one shared
/// <see cref="SimulatedPlantModel" /> and registers their <c>inmemory://</c> endpoints
/// with the MCP connection registries. Reused by the runnable host and the integration tests.
/// </summary>
public static class SimulationServerRegistration
{
    /// <summary>
    /// Creates the full set of feature-area modules. The order is deterministic and
    /// covers all ten OPC Classic specifications.
    /// </summary>
    public static IReadOnlyList<ISimulationModule> CreateModules() =>
    [
        new SimDaModule(),
        new SimAeModule(),
        new SimHdaModule(),
        new SimBatchModule(),
        new SimCommandsModule(),
        new SimCpxModule(),
        new SimDxModule(),
        new SimSecurityModule(),
        new SimDiscoveryModule(),
        new SimXmlDaModule(),
    ];

    /// <summary>
    /// Builds a fresh simulation instance, registers every feature area, and returns a
    /// handle exposing the per-spec connection strings and DI contributions.
    /// </summary>
    /// <param name="loggerFactory">Optional logger factory; defaults to a null factory.</param>
    /// <param name="namePrefix">Optional channel-name prefix; defaults to a unique value.</param>
    public static SimulationServerHandle RegisterAll(
        ILoggerFactory? loggerFactory = null,
        string? namePrefix = null)
    {
        loggerFactory ??= NullLoggerFactory.Instance;
        namePrefix ??= "sim-" + Guid.NewGuid().ToString("N");

        var model = new SimulatedPlantModel();
        var context = new SimulationContext(model, loggerFactory, namePrefix);
        IReadOnlyList<ISimulationModule> modules = CreateModules();

        var registrations = new List<IDisposable>();
        var connectionStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (ISimulationModule module in modules)
        {
            SimulationConnection? connection = module.Register(context);
            if (connection is null)
            {
                continue;
            }

            registrations.Add(connection.Registration);
            connectionStrings[connection.Spec] = connection.ConnectionString;
        }

        return new SimulationServerHandle(context, modules, registrations, connectionStrings);
    }
}
