// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Dcom.Kerberos;

namespace Opc.Classic.Dcom.Rpc.Auth;

/// <summary>
/// AOT-safe explicit and dependency-injection registration helpers for Kerberos server policy.
/// </summary>
public static class KerberosServerAuthenticationRegistration
{
    /// <summary>
    /// Registers Kerberos policy directly in an RPC server authentication registry.
    /// </summary>
    public static KerberosServerAuthenticationProvider RegisterKerberos(
        this RpcServerAuthenticationProviderRegistry registry,
        KerberosServerOptions options)
    {
        ArgumentNullException.ThrowIfNull(registry);
        var provider = new KerberosServerAuthenticationProvider(options);
        registry.Register(provider);
        return provider;
    }

    /// <summary>
    /// Registers Kerberos policy, its provider, and a registry selector with dependency injection.
    /// </summary>
    public static IServiceCollection AddKerberosRpcServerAuthentication(
        this IServiceCollection services,
        KerberosServerOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);
        if (services.Any(static descriptor => descriptor.ServiceType == typeof(KerberosServerOptions)))
        {
            throw new InvalidOperationException("Kerberos RPC server authentication is already registered.");
        }

        ServiceDescriptor? existingRegistry = TakeExistingRegistryRegistration(services);
        services.AddSingleton(options);
        services.AddSingleton<IKerberosServerCredentialProvider>(
            static provider => provider.GetRequiredService<KerberosServerOptions>().CredentialProvider);
        services.AddSingleton<KerberosServerAuthenticationProvider>();
        services.AddSingleton<IRpcServerAuthenticationProvider>(
            static provider => provider.GetRequiredService<KerberosServerAuthenticationProvider>());
        services.AddSingleton(
            provider => ComposeRegistry(provider, existingRegistry));

        RemoveRegistrations<IRpcServerAuthenticationProviderSelector>(services);
        services.AddSingleton<IRpcServerAuthenticationProviderSelector>(
            static provider => provider.GetRequiredService<RpcServerAuthenticationProviderRegistry>());
        return services;
    }

    private static RpcServerAuthenticationProviderRegistry ComposeRegistry(
        IServiceProvider services,
        ServiceDescriptor? existingRegistration)
    {
        RpcServerAuthenticationProviderRegistry registry = existingRegistration is null
            ? new RpcServerAuthenticationProviderRegistry(
                services.GetServices<IRpcServerAuthenticationProvider>())
            : CreateExistingRegistry(services, existingRegistration);
        foreach (IRpcServerAuthenticationProvider provider in
            services.GetServices<IRpcServerAuthenticationProvider>())
        {
            if (registry.TryGetProvider(
                    provider.AuthenticationService,
                    out IRpcServerAuthenticationProvider? registered))
            {
                if (!ReferenceEquals(registered, provider))
                {
                    throw new InvalidOperationException(
                        $"The existing registry already contains a different provider for authentication service {provider.AuthenticationService}.");
                }
            }
            else
            {
                registry.Register(provider);
            }
        }

        if (!registry.TryGetProvider(
                KerberosServerAuthenticationProvider.KerberosAuthenticationService,
                out _))
        {
            throw new InvalidOperationException(
                "Kerberos authentication registration did not compose into the server provider registry.");
        }

        return registry;
    }

    private static RpcServerAuthenticationProviderRegistry CreateExistingRegistry(
        IServiceProvider services,
        ServiceDescriptor registration)
    {
        object? instance;
        if (registration.ImplementationInstance is not null)
        {
            instance = registration.ImplementationInstance;
        }
        else if (registration.ImplementationFactory is not null)
        {
            instance = registration.ImplementationFactory(services);
        }
        else if (registration.ImplementationType == typeof(RpcServerAuthenticationProviderRegistry))
        {
            instance = new RpcServerAuthenticationProviderRegistry();
        }
        else
        {
            throw new InvalidOperationException(
                "The existing registry registration cannot be composed without runtime activation.");
        }

        return instance as RpcServerAuthenticationProviderRegistry
            ?? throw new InvalidOperationException(
                "The existing registry registration returned an incompatible service.");
    }

    private static ServiceDescriptor? TakeExistingRegistryRegistration(IServiceCollection services)
    {
        ServiceDescriptor[] registrations = services
            .Where(static descriptor =>
                !descriptor.IsKeyedService
                && descriptor.ServiceType == typeof(RpcServerAuthenticationProviderRegistry))
            .ToArray();
        if (registrations.Length > 1)
        {
            throw new InvalidOperationException(
                "Multiple RpcServerAuthenticationProviderRegistry registrations are ambiguous.");
        }

        ServiceDescriptor? registration = registrations.SingleOrDefault();
        if (registration is null)
        {
            return null;
        }
        if (registration.Lifetime != ServiceLifetime.Singleton)
        {
            throw new InvalidOperationException(
                "RpcServerAuthenticationProviderRegistry must be registered as a singleton.");
        }

        services.Remove(registration);
        return registration;
    }

    private static void RemoveRegistrations<TService>(IServiceCollection services)
    {
        for (int index = services.Count - 1; index >= 0; index--)
        {
            ServiceDescriptor descriptor = services[index];
            if (!descriptor.IsKeyedService && descriptor.ServiceType == typeof(TService))
            {
                services.RemoveAt(index);
            }
        }
    }
}
