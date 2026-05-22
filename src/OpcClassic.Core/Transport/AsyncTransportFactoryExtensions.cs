//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using Microsoft.Extensions.DependencyInjection;

namespace OpcClassic.Transport;

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

        // The default implementation will be registered by OpcClassic.Dcom or a future
        // OpcClassic.Transport.Tcp package; this method prepares consumer code to call it.
        return services;
    }
}
