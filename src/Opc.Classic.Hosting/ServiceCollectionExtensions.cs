//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Opc.Classic.Hosting;

#pragma warning disable MA0048

/// <summary>
/// Service registration helpers for OPC Classic hosting integration.
/// </summary>
public static class ClassicHostingServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="IClsidRegistry"/> as a singleton bound to the
    /// <c>Opc.Classic:Servers</c> configuration section.
    /// </summary>
    public static IServiceCollection AddClassicClsidRegistry(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddSingleton<IClsidRegistry>(_ => ConfigurationClsidRegistry.FromConfiguration(configuration));
        return services;
    }

    /// <summary>
    /// Registers the OPC Classic hosted service that drives lifecycle of all
    /// <see cref="IOpcServerHost"/> instances registered in the container.
    /// </summary>
    public static IServiceCollection AddClassicServer(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHostedService<ClassicHostedService>();
        return services;
    }

    /// <summary>
    /// Registers an AE server-host implementation.
    /// </summary>
    public static IServiceCollection AddOpcAeServer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceCollection services)
        where T : class, IOpcServerHost
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IOpcServerHost, T>();
        return services;
    }

    /// <summary>
    /// Registers an HDA server-host implementation.
    /// </summary>
    public static IServiceCollection AddOpcHdaServer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceCollection services)
        where T : class, IOpcServerHost
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IOpcServerHost, T>();
        return services;
    }
}

#pragma warning restore MA0048
