//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace OpcClassic.Hosting;

/// <summary>
/// Creates CLSID registries from Microsoft.Extensions.Configuration data.
/// </summary>
public static class ConfigurationClsidRegistry
{
    /// <summary>
    /// Binds a list of <see cref="OpcClsidRegistration"/> entries from the configuration
    /// section <c>"OpcClassic:Servers"</c>:
    /// <code>
    /// "OpcClassic": {
    ///   "Servers": [
    ///     { "Clsid": "10138C2C-...", "ProgId": "Matrikon.OPC.Simulation.1", "AssemblyName": "...", "TypeName": "..." }
    ///   ]
    /// }
    /// </code>
    /// </summary>
    public static InMemoryClsidRegistry FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var registrations = configuration.GetSection("OpcClassic:Servers")
            .Get<List<OpcClsidRegistration>>()
            ?? new List<OpcClsidRegistration>();
        return new InMemoryClsidRegistry(registrations);
    }
}
