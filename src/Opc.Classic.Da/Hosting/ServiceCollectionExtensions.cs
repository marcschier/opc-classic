//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Dcom.Transport;

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
        // Per-CLSID IPID registry: shared across the host + the user's IOpcDaServer
        // implementation so AddGroup/RemoveGroup can register and unregister
        // managed group objects whose per-call interfaces (IOPCGroupStateMgt etc.)
        // dispatch through the cross-platform listener path.
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<Opc.Classic.Hosting.IOpcServerHost, OpcDaServerHost>();
        return services;
    }
}

#pragma warning restore MA0048
