// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;

namespace Opc.Classic.Transport;

/// <summary>
/// Dependency injection helpers for asynchronous DCE/RPC transports.
/// </summary>
public static class AsyncTransportFactoryExtensions
{
    /// <summary>
    /// Adds the async transport extension point to an OPC Classic service collection.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAsyncTransport(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // The default implementation will be registered by Opc.Classic.Dcom or a future
        // Opc.Classic.Transport.Tcp package; this method prepares consumer code to call it.
        return services;
    }
}
