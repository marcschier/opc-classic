//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Opc.Classic.Hda.Hosting;

namespace Opc.Classic.Hda.Tests.Hosting;

public sealed class OpcHdaHostingAdditionalTests
{
    [Test]
    public async Task AddOpcHdaServer_RegistersServerHostAndOptions_ResolvesExpectedConcreteTypes()
    {
        var clsid = Guid.Parse("aaaaaaaa-0000-0000-0000-0000000000da");
        var options = new OpcHdaServerOptions
        {
            Clsid = clsid,
            ProgId = "Opc.Classic.Tests.Hda.1",
            FriendlyName = "HDA DI test server",
            ListenAddress = "127.0.0.1:0",
        };
        var services = new ServiceCollection();

        services.AddOpcHdaServer<MinimalHdaServer>(configured =>
        {
            configured.Clsid = options.Clsid;
            configured.ProgId = options.ProgId;
            configured.FriendlyName = options.FriendlyName;
            configured.ListenAddress = options.ListenAddress;
        });
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<ILogger<OpcHdaServerHost>>(NullLogger<OpcHdaServerHost>.Instance);
        var provider = new TestServiceProvider(services);

        var server = provider.GetRequiredService<IOpcHdaServer>();
        var host = provider.GetRequiredService<Opc.Classic.Hosting.IOpcServerHost>();
        var resolvedOptions = provider.GetRequiredService<IOptions<OpcHdaServerOptions>>().Value;

        await Assert.That(services.Any(static descriptor =>
            descriptor.ServiceType == typeof(IOpcHdaServer)
            && descriptor.ImplementationType == typeof(MinimalHdaServer))).IsTrue();
        await Assert.That(services.Any(static descriptor =>
            descriptor.ServiceType == typeof(Opc.Classic.Hosting.IOpcServerHost)
            && descriptor.ImplementationType == typeof(OpcHdaServerHost))).IsTrue();
        await Assert.That(server).IsTypeOf<MinimalHdaServer>();
        await Assert.That(host).IsTypeOf<OpcHdaServerHost>();
        await Assert.That(host.SpecName).IsEqualTo("HDA");
        await Assert.That(host.Registration.Clsid).IsEqualTo(clsid);
        await Assert.That(host.Registration.ProgId).IsEqualTo("Opc.Classic.Tests.Hda.1");
        await Assert.That(host.Registration.FriendlyName).IsEqualTo("HDA DI test server");
        await Assert.That(resolvedOptions.Clsid).IsEqualTo(clsid);
    }

    [Test]
    public async Task OpcHdaServerHost_RegistrationAndStopBeforeStart_AreCrossPlatformAndDoNotOpenListener()
    {
        var clsid = Guid.Parse("bbbbbbbb-0000-0000-0000-0000000000da");
        var host = new OpcHdaServerHost(
            new MinimalHdaServer(),
            Options.Create(new OpcHdaServerOptions
            {
                Clsid = clsid,
                ProgId = "Opc.Classic.Tests.Hda.Host.1",
                FriendlyName = "HDA host lifecycle",
                ListenAddress = "127.0.0.1:0",
            }),
            NullLogger<OpcHdaServerHost>.Instance);

        await host.StopAsync(CancellationToken.None);
        await host.DisposeAsync();

        await Assert.That(host.SpecName).IsEqualTo("HDA");
        await Assert.That(host.LocalEndpoint).IsNull();
        await Assert.That(host.Registration.Clsid).IsEqualTo(clsid);
        await Assert.That(host.Registration.ProgId).IsEqualTo("Opc.Classic.Tests.Hda.Host.1");
        await Assert.That(host.Registration.AssemblyName).IsEqualTo("Opc.Classic.Hda");
        await Assert.That(host.Registration.TypeName).IsEqualTo(typeof(MinimalHdaServer).FullName);
        await Assert.That(host.Registration.FriendlyName).IsEqualTo("HDA host lifecycle");
    }

    [Test]
    public async Task OpcHdaServerHost_ConstructorNullArguments_ThrowArgumentNullException()
    {
        var server = new MinimalHdaServer();
        IOptions<OpcHdaServerOptions> options = Options.Create(new OpcHdaServerOptions
        {
            Clsid = Guid.Parse("cccccccc-0000-0000-0000-0000000000da"),
            ProgId = "Opc.Classic.Tests.Hda.Null.1",
        });
        ILogger<OpcHdaServerHost> logger = NullLogger<OpcHdaServerHost>.Instance;

        ArgumentNullException serverException = Capture<ArgumentNullException>(() => CreateHost(null!, options, logger));
        ArgumentNullException optionsException = Capture<ArgumentNullException>(() => CreateHost(server, null!, logger));
        ArgumentNullException loggerException = Capture<ArgumentNullException>(() => CreateHost(server, options, null!));

        await Assert.That(serverException.ParamName).IsEqualTo("serverImpl");
        await Assert.That(optionsException.ParamName).IsEqualTo("options");
        await Assert.That(loggerException.ParamName).IsEqualTo("logger");
    }

    private static OpcHdaServerHost CreateHost(
        IOpcHdaServer server,
        IOptions<OpcHdaServerOptions> options,
        ILogger<OpcHdaServerHost> logger) =>
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

    private sealed class MinimalHdaServer : IOpcHdaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Hda,
                State = OpcServerState.Running,
                MaxReturnValues = 100,
                VendorInfo = "Minimal HDA server",
            });

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult(itemIds.Select(static _ => OpcResultId.Ok.Code).ToArray());
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
