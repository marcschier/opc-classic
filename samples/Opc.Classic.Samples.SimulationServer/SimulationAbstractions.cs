// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Opc.Classic.Samples.SimulationServer;

/// <summary>
/// Shared context handed to every <see cref="ISimulationModule" />. Carries the
/// single deterministic <see cref="SimulatedPlantModel" /> that all feature areas
/// project, a logger factory, and a per-instance name prefix used to derive the
/// <c>inmemory://</c> channel names registered with the MCP connection registries.
/// </summary>
public sealed class SimulationContext
{
    /// <summary>Creates a simulation context.</summary>
    public SimulationContext(SimulatedPlantModel model, ILoggerFactory loggerFactory, string namePrefix)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(namePrefix);

        Model = model;
        LoggerFactory = loggerFactory;
        NamePrefix = namePrefix;
    }

    /// <summary>The shared, deterministic plant model.</summary>
    public SimulatedPlantModel Model { get; }

    /// <summary>Logger factory for module-level diagnostics.</summary>
    public ILoggerFactory LoggerFactory { get; }

    /// <summary>Per-instance prefix that makes channel names unique across simulation instances.</summary>
    public string NamePrefix { get; }

    /// <summary>Derives the registry channel name for a feature area (e.g. <c>sim-1234-da</c>).</summary>
    public string ChannelName(string spec)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(spec);
        return NamePrefix + "-" + spec;
    }

    /// <summary>Derives the MCP connection string for a feature area (e.g. <c>inmemory://sim-1234-da</c>).</summary>
    public string ConnectionString(string spec) => "inmemory://" + ChannelName(spec);
}

/// <summary>
/// Describes a registered feature-area endpoint: its spec key, the MCP connection
/// string a session uses to reach it, and the registry registration to dispose.
/// </summary>
public sealed record SimulationConnection(string Spec, string ConnectionString, IDisposable Registration);

/// <summary>
/// One OPC Classic feature area of the simulation server. A module either registers
/// an <c>inmemory://</c> endpoint with the matching MCP connection registry (returning
/// a <see cref="SimulationConnection" />), contributes services to the MCP host via
/// <see cref="ConfigureMcpHost" /> (used by Discovery and Security, which are resolved
/// from DI rather than a static registry), or both.
/// </summary>
public interface ISimulationModule
{
    /// <summary>Stable feature-area key, e.g. <c>da</c>, <c>ae</c>, <c>hda</c>.</summary>
    string Spec { get; }

    /// <summary>
    /// Registers the feature area's <c>inmemory://</c> endpoint. Returns <see langword="null" />
    /// for modules that are wired purely through <see cref="ConfigureMcpHost" />.
    /// </summary>
    SimulationConnection? Register(SimulationContext context);

    /// <summary>
    /// Contributes services to the in-process MCP host (e.g. <c>IOpcDiscovery</c>,
    /// <c>IOpcSecurityClientFactory</c>). Default no-op for registry-based modules.
    /// </summary>
    void ConfigureMcpHost(SimulationContext context, IServiceCollection services)
    {
        _ = context;
        _ = services;
    }
}

/// <summary>
/// Owns a live simulation instance: the shared model, the per-spec connection strings,
/// the set of registry registrations to dispose, and the DI contributions to apply to
/// an MCP host. Returned by <see cref="SimulationServerRegistration.RegisterAll" />.
/// </summary>
public sealed class SimulationServerHandle : IDisposable
{
    private readonly List<IDisposable> _registrations;
    private readonly IReadOnlyList<ISimulationModule> _modules;
    private readonly SimulationContext _context;
    private bool _disposed;

    internal SimulationServerHandle(
        SimulationContext context,
        IReadOnlyList<ISimulationModule> modules,
        List<IDisposable> registrations,
        IReadOnlyDictionary<string, string> connectionStrings)
    {
        _context = context;
        _modules = modules;
        _registrations = registrations;
        ConnectionStrings = connectionStrings;
    }

    /// <summary>The shared, deterministic plant model behind every feature area.</summary>
    public SimulatedPlantModel Model => _context.Model;

    /// <summary>Per-instance name prefix used to derive channel names.</summary>
    public string NamePrefix => _context.NamePrefix;

    /// <summary>Maps each registered feature-area key to its <c>inmemory://</c> connection string.</summary>
    public IReadOnlyDictionary<string, string> ConnectionStrings { get; }

    /// <summary>
    /// Applies every module's DI contributions to an MCP host's service collection
    /// (used for Discovery and Security wiring).
    /// </summary>
    public void ConfigureMcpHost(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        foreach (ISimulationModule module in _modules)
        {
            module.ConfigureMcpHost(_context, services);
        }
    }

    /// <summary>Disposes every registry registration, removing the channels.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        foreach (IDisposable registration in _registrations)
        {
            registration.Dispose();
        }
    }
}
