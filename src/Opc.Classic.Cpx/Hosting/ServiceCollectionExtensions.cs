//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Cpx.Hosting;

/// <summary>Service registration helpers for CPX-enabled managed DA hosts.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers CPX address-space and item-property decorators.</summary>
    public static IServiceCollection AddOpcCpxAddressSpace(
        this IServiceCollection services,
        Action<OpcCpxOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new OpcCpxOptions();
        configure(options);
        services.AddSingleton(options);
        DecorateAddressSpace(services);
        DecorateItemPropertyProvider(services);
        services.TryAddSingleton<IOPCItemProperties>(static provider =>
            new DefaultItemProperties(provider.GetRequiredService<IOpcItemPropertyProvider>()));
        return services;
    }

    private static void DecorateAddressSpace(IServiceCollection services)
    {
        var index = FindLast(services, typeof(IOpcAddressSpace));
        if (index < 0)
        {
            services.AddSingleton<IOpcAddressSpace>(static provider =>
                new OpcCpxAddressSpace(new FlatHierarchicalNamespace(), provider.GetRequiredService<OpcCpxOptions>()));
            return;
        }

        var descriptor = services[index];
        services[index] = ServiceDescriptor.Describe(
            typeof(IOpcAddressSpace),
            provider => new OpcCpxAddressSpace(
                (IOpcAddressSpace)CreateOriginalService(provider, descriptor),
                provider.GetRequiredService<OpcCpxOptions>()),
            descriptor.Lifetime);
    }

    private static void DecorateItemPropertyProvider(IServiceCollection services)
    {
        var index = FindLast(services, typeof(IOpcItemPropertyProvider));
        if (index < 0)
        {
            services.AddSingleton<IOpcItemPropertyProvider>(static provider =>
                new OpcCpxItemProperties(provider.GetRequiredService<OpcCpxOptions>()));
            return;
        }

        var descriptor = services[index];
        services[index] = ServiceDescriptor.Describe(
            typeof(IOpcItemPropertyProvider),
            provider => new OpcCpxItemProperties(
                provider.GetRequiredService<OpcCpxOptions>(),
                (IOpcItemPropertyProvider)CreateOriginalService(provider, descriptor)),
            descriptor.Lifetime);
    }

    private static int FindLast(IServiceCollection services, Type serviceType)
    {
        for (var i = services.Count - 1; i >= 0; i--)
        {
            if (services[i].ServiceType == serviceType)
            {
                return i;
            }
        }

        return -1;
    }

    private static object CreateOriginalService(IServiceProvider provider, ServiceDescriptor descriptor)
    {
        if (descriptor.ImplementationInstance is not null)
        {
            return descriptor.ImplementationInstance;
        }

        if (descriptor.ImplementationFactory is not null)
        {
            return descriptor.ImplementationFactory(provider)
                ?? throw new InvalidOperationException("The decorated service factory returned null.");
        }

        if (descriptor.ImplementationType is not null)
        {
            return ActivatorUtilities.CreateInstance(provider, descriptor.ImplementationType);
        }

        throw new InvalidOperationException("The decorated service descriptor cannot be activated.");
    }
}
