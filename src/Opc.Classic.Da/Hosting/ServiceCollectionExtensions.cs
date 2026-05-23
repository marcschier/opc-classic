//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Opc.Classic.Da.Hosting;

#pragma warning disable MA0048

/// <summary>Service registration helpers for managed OPC DA server hosting.</summary>
public static class OpcDaHostingServiceCollectionExtensions
{
    /// <summary>Registers a managed OPC DA server implementation and host.</summary>
    public static IServiceCollection AddOpcDaServer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceCollection services,
        Action<OpcDaServerOptions> configureOptions)
        where T : class, IOpcDaServer
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IOpcDaServer, T>();
        services.AddSingleton<IOpcDaDataChangePublisher, OpcDaDataChangePublisher>();
        services.AddSingleton<Opc.Classic.Hosting.IOpcServerHost, OpcDaServerHost>();
        return services;
    }
}

#pragma warning restore MA0048
