// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Globalization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Opc.Classic.Hosting.Tests;

public sealed class HostingAdditionalTests
{
    [Test]
    public async Task AddClassicClsidRegistry_RegistersConfigurationRegistry_ResolvesConcreteRegistration()
    {
        var registration = CreateRegistration(
            clsid: Guid.Parse("10138c2c-0000-0000-0000-000000000101"),
            progId: "Vendor.Hosting.AddRegistry.1",
            friendlyName: "Hosting registry extension",
            implementedCategories: [OpcComponentCategories.OpcAeServer10.CategoryId]);
        var services = new ServiceCollection();
        IServiceCollection returned = services.AddClassicClsidRegistry(CreateConfiguration(registration));
        var provider = new TestServiceProvider(services);

        var registry = provider.GetRequiredService<IClsidRegistry>();
        bool resolvedByClsid = registry.TryResolve(registration.Clsid, out OpcClsidRegistration? byClsid);
        bool resolvedByProgId = registry.TryResolveProgId("vendor.hosting.addregistry.1", out OpcClsidRegistration? byProgId);

        await Assert.That(returned).IsEqualTo(services);
        await Assert.That(registry).IsTypeOf<InMemoryClsidRegistry>();
        await Assert.That(resolvedByClsid).IsTrue();
        await Assert.That(resolvedByProgId).IsTrue();
        await Assert.That(byClsid).IsNotNull();
        await Assert.That(byClsid!.FriendlyName).IsEqualTo("Hosting registry extension");
        await Assert.That(byClsid.ImplementedCategories).IsNotNull();
        await Assert.That(byClsid.ImplementedCategories![0]).IsEqualTo(OpcComponentCategories.OpcAeServer10.CategoryId);
        await Assert.That(byProgId).IsEqualTo(byClsid);
    }

    [Test]
    public async Task ServiceCollectionExtensions_NullArguments_ThrowExpectedParameterNames()
    {
        IConfiguration configuration = CreateConfiguration();

        ArgumentNullException classicServer = Capture<ArgumentNullException>(() => ClassicHostingServiceCollectionExtensions.AddClassicServer(null!));
        ArgumentNullException registryServices = Capture<ArgumentNullException>(() => ClassicHostingServiceCollectionExtensions.AddClassicClsidRegistry(null!, configuration));
        ArgumentNullException registryConfiguration = Capture<ArgumentNullException>(() => new ServiceCollection().AddClassicClsidRegistry(null!));
        ArgumentNullException aeServices = Capture<ArgumentNullException>(() => ClassicHostingServiceCollectionExtensions.AddOpcAeServer<TestHost>(null!));
        ArgumentNullException hdaServices = Capture<ArgumentNullException>(() => ClassicHostingServiceCollectionExtensions.AddOpcHdaServer<TestHost>(null!));

        await Assert.That(classicServer.ParamName).IsEqualTo("services");
        await Assert.That(registryServices.ParamName).IsEqualTo("services");
        await Assert.That(registryConfiguration.ParamName).IsEqualTo("configuration");
        await Assert.That(aeServices.ParamName).IsEqualTo("services");
        await Assert.That(hdaServices.ParamName).IsEqualTo("services");
    }

    [Test]
    public async Task ClassicHostedService_StartAsync_WhenHostThrows_PropagatesAndDoesNotStartLaterHosts()
    {
        var order = new List<string>();
        var first = new TestHost("DA", "Vendor.First.1", order);
        var second = new TestHost("AE", "Vendor.Second.1", order) { StartException = new InvalidOperationException("boom") };
        var third = new TestHost("HDA", "Vendor.Third.1", order);
        var service = new ClassicHostedService([first, second, third], NoopLogger<ClassicHostedService>.Instance);

        InvalidOperationException exception = await CaptureAsync<InvalidOperationException>(() => service.StartAsync(CancellationToken.None));

        await Assert.That(exception.Message).IsEqualTo("boom");
        await Assert.That(order).IsEquivalentTo(["start:Vendor.First.1", "start:Vendor.Second.1"]);
        await Assert.That(first.StartCount).IsEqualTo(1);
        await Assert.That(second.StartCount).IsEqualTo(1);
        await Assert.That(third.StartCount).IsEqualTo(0);
    }

    [Test]
    public async Task ClassicHostedService_StopAsync_StopsHostsInRegistrationOrder()
    {
        var order = new List<string>();
        var first = new TestHost("AE", "Vendor.First.1", order);
        var second = new TestHost("HDA", "Vendor.Second.1", order);
        var service = new ClassicHostedService([first, second], NoopLogger<ClassicHostedService>.Instance);

        await service.StopAsync(CancellationToken.None);

        await Assert.That(order).IsEquivalentTo(["stop:Vendor.First.1", "stop:Vendor.Second.1"]);
        await Assert.That(first.StopCount).IsEqualTo(1);
        await Assert.That(second.StopCount).IsEqualTo(1);
    }

    [Test]
    public async Task ClassicHostedService_ConstructorNullArguments_ThrowExpectedParameterNames()
    {
        IEnumerable<IOpcServerHost> hosts = [new TestHost("AE", "Vendor.Host.1", [])];
        ILogger<ClassicHostedService> logger = NoopLogger<ClassicHostedService>.Instance;

        ArgumentNullException hostsException = Capture<ArgumentNullException>(() => CreateHostedService(null!, logger));
        ArgumentNullException loggerException = Capture<ArgumentNullException>(() => CreateHostedService(hosts, null!));

        await Assert.That(hostsException.ParamName).IsEqualTo("hosts");
        await Assert.That(loggerException.ParamName).IsEqualTo("logger");
    }

    [Test]
    public async Task OpcComponentCategories_KnownAeAndHdaCategories_ExposeSpecIdsAndDescriptions()
    {
        await Assert.That(OpcComponentCategories.OpcAeServer10.CategoryId)
            .IsEqualTo(Guid.Parse("58E13251-AC87-11d1-84D5-00608CB8A7E9"));
        await Assert.That(OpcComponentCategories.OpcAeServer10.Description)
            .IsEqualTo("OPC Alarm & Event Server Version 1.0");
        await Assert.That(OpcComponentCategories.OpcHdaServer10.CategoryId)
            .IsEqualTo(Guid.Parse("7DE5B060-E089-11d2-A5E6-000086339399"));
        await Assert.That(OpcComponentCategories.OpcHdaServer10.Description)
            .IsEqualTo("OPC Historical Data Access Servers Version 1.0");
    }

    private static ClassicHostedService CreateHostedService(
        IEnumerable<IOpcServerHost> hosts,
        ILogger<ClassicHostedService> logger) =>
        new(hosts, logger);

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

    private static async Task<TException> CaptureAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
    }

    private static OpcClsidRegistration CreateRegistration(
        Guid? clsid = null,
        string progId = "Vendor.Hosting.1",
        string assemblyName = "Vendor.Hosting",
        string typeName = "Vendor.Hosting.ServerClass",
        string? friendlyName = null,
        IReadOnlyList<Guid>? implementedCategories = null) =>
        new(
            clsid ?? Guid.Parse("10138c2c-0000-0000-0000-000000000100"),
            progId,
            assemblyName,
            typeName,
            friendlyName,
            implementedCategories);

    private static InMemoryConfigurationSection CreateConfiguration(params OpcClsidRegistration[] registrations)
    {
        InMemoryConfigurationSection root = new(string.Empty, string.Empty);
        InMemoryConfigurationSection servers = root.GetOrAddSection("Opc.Classic").GetOrAddSection("Servers");

        for (int i = 0; i < registrations.Length; i++)
        {
            OpcClsidRegistration registration = registrations[i];
            InMemoryConfigurationSection section = servers.GetOrAddSection(i.ToString(CultureInfo.InvariantCulture));
            section.GetOrAddSection("Clsid").Value = registration.Clsid.ToString("D");
            section.GetOrAddSection("ProgId").Value = registration.ProgId;
            section.GetOrAddSection("AssemblyName").Value = registration.AssemblyName;
            section.GetOrAddSection("TypeName").Value = registration.TypeName;
            section.GetOrAddSection("FriendlyName").Value = registration.FriendlyName;

            if (registration.ImplementedCategories is not null)
            {
                InMemoryConfigurationSection categories = section.GetOrAddSection("ImplementedCategories");
                for (int categoryIndex = 0; categoryIndex < registration.ImplementedCategories.Count; categoryIndex++)
                {
                    categories.GetOrAddSection(categoryIndex.ToString(CultureInfo.InvariantCulture)).Value =
                        registration.ImplementedCategories[categoryIndex].ToString("D");
                }
            }
        }

        return root;
    }

    private sealed class TestHost : IOpcServerHost
    {
        private readonly List<string> _order;

        public TestHost()
            : this("Test", "Vendor.Test.1", [])
        {
        }

        public TestHost(string specName, string progId, List<string> order)
        {
            SpecName = specName;
            Registration = CreateRegistration(progId: progId);
            _order = order;
        }

        public string SpecName { get; }
        public OpcClsidRegistration Registration { get; }
        public Exception? StartException { get; init; }
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartCount++;
            _order.Add("start:" + Registration.ProgId);
            if (StartException is not null)
            {
                throw StartException;
            }

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            StopCount++;
            _order.Add("stop:" + Registration.ProgId);
            return Task.CompletedTask;
        }
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
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return GetServices(serviceType.GetGenericArguments()[0]);
            }

            ServiceDescriptor? descriptor = _descriptors.LastOrDefault(candidate => candidate.ServiceType == serviceType);
            return descriptor is null ? null : GetService(descriptor);
        }

        private Array GetServices(Type serviceType)
        {
            ServiceDescriptor[] descriptors = _descriptors.Where(candidate => candidate.ServiceType == serviceType).ToArray();
            Array services = Array.CreateInstance(serviceType, descriptors.Length);
            for (int i = 0; i < descriptors.Length; i++)
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

    private sealed class InMemoryConfigurationSection : IConfigurationSection
    {
        private readonly List<InMemoryConfigurationSection> _children = new();

        public InMemoryConfigurationSection(string key, string path)
        {
            Key = key;
            Path = path;
        }

        public string Key { get; }
        public string Path { get; }
        public string? Value { get; set; }

        public string? this[string key]
        {
            get => GetSection(key).Value;
            set => GetOrAddSection(key).Value = value;
        }

        public IEnumerable<IConfigurationSection> GetChildren() => _children;
        public IChangeToken GetReloadToken() => NoopChangeToken.Instance;

        public IConfigurationSection GetSection(string key)
        {
            InMemoryConfigurationSection current = this;
            foreach (string segment in key.Split(':', StringSplitOptions.RemoveEmptyEntries))
            {
                InMemoryConfigurationSection? next = current._children.FirstOrDefault(child =>
                    string.Equals(child.Key, segment, StringComparison.OrdinalIgnoreCase));
                if (next is null)
                {
                    return new InMemoryConfigurationSection(segment, CreateChildPath(current.Path, segment));
                }

                current = next;
            }

            return current;
        }

        public InMemoryConfigurationSection GetOrAddSection(string key)
        {
            InMemoryConfigurationSection current = this;
            foreach (string segment in key.Split(':', StringSplitOptions.RemoveEmptyEntries))
            {
                InMemoryConfigurationSection? next = current._children.FirstOrDefault(child =>
                    string.Equals(child.Key, segment, StringComparison.OrdinalIgnoreCase));
                if (next is null)
                {
                    next = new InMemoryConfigurationSection(segment, CreateChildPath(current.Path, segment));
                    current._children.Add(next);
                }

                current = next;
            }

            return current;
        }

        private static string CreateChildPath(string parentPath, string key) =>
            string.IsNullOrEmpty(parentPath) ? key : string.Concat(parentPath, ":", key);
    }

    private sealed class NoopChangeToken : IChangeToken
    {
        public static NoopChangeToken Instance { get; } = new();
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => NoopDisposable.Instance;
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
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
