//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OpcClassic.Hosting;

#pragma warning disable MA0048

/// <summary>
/// Service registration helpers for OPC Classic hosting integration.
/// </summary>
public static class OpcClassicHostingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IClsidRegistry"/> as a singleton bound to the
    /// <c>OpcClassic:Servers</c> configuration section.
    /// </summary>
    public static IServiceCollection AddOpcClassicClsidRegistry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddSingleton<IClsidRegistry>(_ => ConfigurationClsidRegistry.FromConfiguration(configuration));
        return services;
    }
}

#pragma warning restore MA0048
