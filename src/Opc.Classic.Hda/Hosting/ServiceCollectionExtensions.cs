//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Opc.Classic.Hda.Hosting;

#pragma warning disable MA0048

/// <summary>Service registration helpers for managed OPC HDA server hosting.</summary>
public static class OpcHdaHostingServiceCollectionExtensions
{
    /// <summary>Registers a managed OPC HDA server implementation and host.</summary>
    public static IServiceCollection AddOpcHdaServer<
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this IServiceCollection services,
        Action<OpcHdaServerOptions> configureOptions)
        where T : class, IOpcHdaServer
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);
        services.AddSingleton<IOpcHdaServer, T>();
        services.AddSingleton<Opc.Classic.Hosting.IOpcServerHost, OpcHdaServerHost>();
        return services;
    }
}

#pragma warning restore MA0048
