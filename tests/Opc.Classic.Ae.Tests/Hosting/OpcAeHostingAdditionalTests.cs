//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Opc.Classic.Ae.Hosting;

namespace Opc.Classic.Ae.Tests.Hosting;

public sealed class OpcAeHostingAdditionalTests
{
    [Test]
    public async Task AddOpcAeServer_RegistersServerHostAndOptions_ResolvesExpectedConcreteTypes()
    {
        var clsid = Guid.Parse("aaaaaaaa-0000-0000-0000-0000000000ae");
        var options = new OpcAeServerOptions
        {
            Clsid = clsid,
            ProgId = "Opc.Classic.Tests.Ae.1",
            FriendlyName = "AE DI test server",
            ListenAddress = "127.0.0.1:0",
        };
        var services = new ServiceCollection();

        services.AddOpcAeServer<MinimalAeServer>(configured =>
        {
            configured.Clsid = options.Clsid;
            configured.ProgId = options.ProgId;
            configured.FriendlyName = options.FriendlyName;
            configured.ListenAddress = options.ListenAddress;
        });
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<ILogger<OpcAeServerHost>>(NullLogger<OpcAeServerHost>.Instance);
        var provider = new TestServiceProvider(services);

        var server = provider.GetRequiredService<IOpcAeServer>();
        var host = provider.GetRequiredService<Opc.Classic.Hosting.IOpcServerHost>();
        var resolvedOptions = provider.GetRequiredService<IOptions<OpcAeServerOptions>>().Value;

        await Assert.That(services.Any(static descriptor =>
            descriptor.ServiceType == typeof(IOpcAeServer)
            && descriptor.ImplementationType == typeof(MinimalAeServer))).IsTrue();
        await Assert.That(services.Any(static descriptor =>
            descriptor.ServiceType == typeof(Opc.Classic.Hosting.IOpcServerHost)
            && descriptor.ImplementationType == typeof(OpcAeServerHost))).IsTrue();
        await Assert.That(server).IsTypeOf<MinimalAeServer>();
        await Assert.That(host).IsTypeOf<OpcAeServerHost>();
        await Assert.That(host.SpecName).IsEqualTo("AE");
        await Assert.That(host.Registration.Clsid).IsEqualTo(clsid);
        await Assert.That(host.Registration.ProgId).IsEqualTo("Opc.Classic.Tests.Ae.1");
        await Assert.That(host.Registration.FriendlyName).IsEqualTo("AE DI test server");
        await Assert.That(resolvedOptions.Clsid).IsEqualTo(clsid);
    }

    [Test]
    public async Task OpcAeServerHost_RegistrationAndStopBeforeStart_AreCrossPlatformAndDoNotOpenListener()
    {
        var clsid = Guid.Parse("bbbbbbbb-0000-0000-0000-0000000000ae");
        var server = new MinimalAeServer();
        var host = new OpcAeServerHost(
            server,
            Options.Create(new OpcAeServerOptions
            {
                Clsid = clsid,
                ProgId = "Opc.Classic.Tests.Ae.Host.1",
                FriendlyName = "AE host lifecycle",
                ListenAddress = "127.0.0.1:0",
            }),
            NullLogger<OpcAeServerHost>.Instance);

        await host.StopAsync(CancellationToken.None);
        await host.DisposeAsync();

        await Assert.That(host.SpecName).IsEqualTo("AE");
        await Assert.That(host.LocalEndpoint).IsNull();
        await Assert.That(host.Registration.Clsid).IsEqualTo(clsid);
        await Assert.That(host.Registration.ProgId).IsEqualTo("Opc.Classic.Tests.Ae.Host.1");
        await Assert.That(host.Registration.AssemblyName).IsEqualTo("Opc.Classic.Ae");
        await Assert.That(host.Registration.TypeName).IsEqualTo(typeof(MinimalAeServer).FullName);
        await Assert.That(host.Registration.FriendlyName).IsEqualTo("AE host lifecycle");
    }

    [Test]
    public async Task OpcAeServerHost_ConstructorNullArguments_ThrowArgumentNullException()
    {
        var server = new MinimalAeServer();
        IOptions<OpcAeServerOptions> options = Options.Create(new OpcAeServerOptions
        {
            Clsid = Guid.Parse("cccccccc-0000-0000-0000-0000000000ae"),
            ProgId = "Opc.Classic.Tests.Ae.Null.1",
        });
        ILogger<OpcAeServerHost> logger = NullLogger<OpcAeServerHost>.Instance;

        ArgumentNullException serverException = Capture<ArgumentNullException>(() => CreateHost(null!, options, logger));
        ArgumentNullException optionsException = Capture<ArgumentNullException>(() => CreateHost(server, null!, logger));
        ArgumentNullException loggerException = Capture<ArgumentNullException>(() => CreateHost(server, options, null!));

        await Assert.That(serverException.ParamName).IsEqualTo("serverImpl");
        await Assert.That(optionsException.ParamName).IsEqualTo("options");
        await Assert.That(loggerException.ParamName).IsEqualTo("logger");
    }

    private static OpcAeServerHost CreateHost(
        IOpcAeServer server,
        IOptions<OpcAeServerOptions> options,
        ILogger<OpcAeServerHost> logger) =>
        new(server, options, logger);

    private static TException Capture<TException>(Func<object> action)
        where TException : Exception
    {
        try
        {
            _ = action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
    }

    private sealed class MinimalAeServer : IOpcAeServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Ae,
                State = OpcServerState.Running,
                VendorInfo = "Minimal AE server",
            });

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0);
    }

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
            ServiceDescriptor? descriptor = _descriptors.LastOrDefault(candidate => candidate.ServiceType == serviceType);
            return descriptor is null ? null : GetService(descriptor);
        }

        private object? GetService(ServiceDescriptor descriptor)
        {
            if (descriptor.Lifetime != ServiceLifetime.Singleton)
            {
                return CreateService(descriptor);
            }

            if (_singletons.TryGetValue(descriptor, out object? instance))
            {
                return instance;
            }

            instance = CreateService(descriptor);
            _singletons.Add(descriptor, instance);
            return instance;
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
            object?[] arguments = constructor.GetParameters()
                .Select(parameter => GetService(parameter.ParameterType)
                    ?? throw new InvalidOperationException($"Could not resolve {parameter.ParameterType}."))
                .ToArray();
            return constructor.Invoke(arguments);
        }
    }
}
