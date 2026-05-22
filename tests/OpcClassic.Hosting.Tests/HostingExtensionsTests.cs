//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TUnit.Core;

namespace OpcClassic.Hosting.Tests;

public sealed class HostingExtensionsTests
{
    [Test]
    public async Task AddOpcClassicServer_registers_hosted_service()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<OpcClassicHostedService>>(NoopLogger<OpcClassicHostedService>.Instance);

        services.AddOpcClassicServer();

        var provider = new TestServiceProvider(services);
        var hostedService = provider.GetRequiredService<IHostedService>();

        await Assert.That(hostedService).IsTypeOf<OpcClassicHostedService>();
    }

    [Test]
    public async Task AddOpcAeServer_registers_IOpcServerHost()
    {
        var services = new ServiceCollection();

        services.AddOpcAeServer<TestOpcServerHost>();

        var provider = new TestServiceProvider(services);
        var host = provider.GetRequiredService<IOpcServerHost>();

        await Assert.That(host).IsTypeOf<TestOpcServerHost>();
    }

    [Test]
    public async Task OpcClassicHostedService_starts_and_stops_all_hosts()
    {
        var first = new TestOpcServerHost { SpecName = "DA", Registration = CreateRegistration("Vendor.First.1") };
        var second = new TestOpcServerHost { SpecName = "AE", Registration = CreateRegistration("Vendor.Second.1") };
        var service = new OpcClassicHostedService([first, second], NoopLogger<OpcClassicHostedService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await service.StopAsync(CancellationToken.None);

        await Assert.That(first.Started).IsTrue();
        await Assert.That(second.Started).IsTrue();
        await Assert.That(first.Stopped).IsTrue();
        await Assert.That(second.Stopped).IsTrue();
    }

    [Test]
    public async Task AddOpcAeServer_can_be_called_multiple_times()
    {
        var services = new ServiceCollection();

        services.AddOpcAeServer<TestOpcServerHost>();
        services.AddOpcAeServer<SecondTestOpcServerHost>();

        var provider = new TestServiceProvider(services);
        var hosts = provider.GetRequiredService<IEnumerable<IOpcServerHost>>().ToArray();

        await Assert.That(hosts.Length).IsEqualTo(2);
        await Assert.That(hosts[0]).IsTypeOf<TestOpcServerHost>();
        await Assert.That(hosts[1]).IsTypeOf<SecondTestOpcServerHost>();
    }

    private static OpcClsidRegistration CreateRegistration(string progId) =>
        new(
            Guid.Parse("10138C2C-0000-0000-0000-000000000010"),
            progId,
            "Vendor.Server",
            "Vendor.Server.ServerClass");

    private sealed class TestServiceProvider : IServiceProvider
    {
        private readonly IReadOnlyList<ServiceDescriptor> _descriptors;
        private readonly Dictionary<ServiceDescriptor, object?> _singletons = new();

        public TestServiceProvider(IEnumerable<ServiceDescriptor> descriptors)
        {
            _descriptors = descriptors.ToArray();
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return GetServices(serviceType.GetGenericArguments()[0]);
            }

            var descriptor = _descriptors.LastOrDefault(candidate => candidate.ServiceType == serviceType);
            return descriptor is null ? null : GetService(descriptor);
        }

        private Array GetServices(Type serviceType)
        {
            var descriptors = _descriptors.Where(candidate => candidate.ServiceType == serviceType).ToArray();
            var services = Array.CreateInstance(serviceType, descriptors.Length);

            for (var i = 0; i < descriptors.Length; i++)
            {
                services.SetValue(GetService(descriptors[i]), i);
            }

            return services;
        }

        private object? GetService(ServiceDescriptor descriptor)
        {
            if (descriptor.Lifetime != ServiceLifetime.Singleton)
            {
                return CreateService(descriptor);
            }

            if (_singletons.TryGetValue(descriptor, out var service))
            {
                return service;
            }

            service = CreateService(descriptor);
            _singletons.Add(descriptor, service);
            return service;
        }

        private object? CreateService(ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance is not null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory is not null)
            {
                return descriptor.ImplementationFactory(this);
            }

            return descriptor.ImplementationType is null ? null : CreateImplementation(descriptor.ImplementationType);
        }

        private object CreateImplementation(Type implementationType)
        {
            var constructor = implementationType.GetConstructors()
                .OrderByDescending(candidate => candidate.GetParameters().Length)
                .First();
            var parameters = constructor.GetParameters();
            var arguments = new object?[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                arguments[i] = GetService(parameters[i].ParameterType)
                    ?? throw new InvalidOperationException($"Could not resolve {parameters[i].ParameterType}.");
            }

            return constructor.Invoke(arguments);
        }
    }

    private sealed class TestOpcServerHost : IOpcServerHost
    {
        public string SpecName { get; init; } = "Test";

        public OpcClsidRegistration Registration { get; init; } = CreateRegistration("Vendor.Test.1");

        public bool Started { get; private set; }

        public bool Stopped { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Started = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Stopped = true;
            return Task.CompletedTask;
        }
    }

    private sealed class SecondTestOpcServerHost : IOpcServerHost
    {
        public string SpecName { get; init; } = "SecondTest";

        public OpcClsidRegistration Registration { get; init; } = CreateRegistration("Vendor.SecondTest.1");

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class NoopLogger<T> : ILogger<T>
    {
        public static NoopLogger<T> Instance { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull =>
            null;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}
