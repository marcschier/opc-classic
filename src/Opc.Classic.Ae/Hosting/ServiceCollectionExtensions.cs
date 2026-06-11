//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Opc.Classic.Ae.Hosting;

#pragma warning disable MA0048

/// <summary>Service registration helpers for managed OPC AE server hosting.</summary>
public static class OpcAeHostingServiceCollectionExtensions
{
    /// <summary>Registers a managed OPC AE server implementation and host.</summary>
    public static IServiceCollection AddOpcAeServer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceCollection services,
        Action<OpcAeServerOptions> configureOptions)
        where T : class, IOpcAeServer
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IOpcAeServer, T>();
        services.AddSingleton<Opc.Classic.Hosting.IOpcServerHost, OpcAeServerHost>();
        return services;
    }
}

#pragma warning restore MA0048
