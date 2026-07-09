// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.Configuration;

namespace Opc.Classic.Hosting;

/// <summary>
/// Creates CLSID registries from Microsoft.Extensions.Configuration data.
/// </summary>
public static class ConfigurationClsidRegistry
{
    /// <summary>
    /// Binds a list of <see cref="OpcClsidRegistration"/> entries from the configuration
    /// section <c>"Opc.Classic:Servers"</c>:
    /// <code>
    /// "Opc.Classic": {
    ///   "Servers": [
    ///     { "Clsid": "10138C2C-...", "ProgId": "Matrikon.OPC.Simulation.1", "AssemblyName": "...", "TypeName": "..." }
    ///   ]
    /// }
    /// </code>
    /// </summary>
    public static InMemoryClsidRegistry FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var registrations = configuration.GetSection("Opc.Classic:Servers")
            .Get<List<OpcClsidRegistration>>()
            ?? new List<OpcClsidRegistration>();
        return new InMemoryClsidRegistry(registrations);
    }
}
