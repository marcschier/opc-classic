// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Hosting.Windows;
using Opc.Classic.Security;
using Opc.Classic.Security.Dcom;

namespace Opc.Classic.Samples.OpcSecurityServer;

internal static class Program
{
    private static readonly Guid SampleClsid = new("5A0DA9C7-56D2-4768-9CB3-6FC5E57B6D51");
    private const string SampleProgId = "Opc.Classic.Samples.OpcSecurityServer.1";
    private const string SampleFriendlyName = "Opc.Classic Sample OPC Security Server";
    private const string SampleAssemblyName = "Opc.Classic.Samples.OpcSecurityServer";
    private const string SampleTypeName = "Opc.Classic.Samples.OpcSecurityServer.Program+SecuritySampleDaServer";

    public static async Task<int> Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var registration = new OpcClsidRegistration(
            Clsid: SampleClsid,
            ProgId: SampleProgId,
            AssemblyName: SampleAssemblyName,
            TypeName: SampleTypeName,
            FriendlyName: SampleFriendlyName);
        IReadOnlyList<OpcComponentCategory> implementedCategories =
        [
            OpcComponentCategories.OpcDaServer20,
        ];

        if (SampleServerRegistrationCommand.TryHandle(args, registration, implementedCategories, out int registrationExitCode))
        {
            return registrationExitCode;
        }

        bool embedded = SampleServerRegistrationCommand.HasEmbeddingFlag(args);
        int port = int.TryParse(
            Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
            out int parsed) && parsed > 0 ? parsed : 51304;
        // When SCM activates the sample (-Embedding), bind an ephemeral
        // port to avoid EADDRINUSE if a previous SCM-launched instance
        // is still alive. DCOM activation doesn't depend on the sample's
        // TCP listener -- it routes through CoRegisterClassObject.
        string defaultBind = embedded ? "127.0.0.1:0" : $"0.0.0.0:{port}";
        string listenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_LISTEN_ADDRESS")
            ?? defaultBind;
        Console.WriteLine($"Listening on {listenAddress}");

        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(static opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddClassicServer();
        builder.Services.AddClassicClsidRegistry(builder.Configuration);
        builder.Services.Configure<OpcDaServerOptions>(options =>
        {
            options.Clsid = SampleClsid;
            options.ProgId = SampleProgId;
            options.FriendlyName = SampleFriendlyName;
            options.ListenAddress = listenAddress;
        });
        builder.Services.AddSingleton<OpcObjectRegistry>();
        builder.Services.AddSingleton<IOpcDaServer>(_ => new SecuritySampleDaServer());
        builder.Services.AddSingleton<StubOpcSecurityServer>();
        builder.Services.AddSingleton<IOpcSecurity>(static services => services.GetRequiredService<StubOpcSecurityServer>());
        builder.Services.AddSingleton<IOPCSecurityNT>(static services => services.GetRequiredService<StubOpcSecurityServer>());
        builder.Services.AddSingleton<IOPCSecurityPrivate>(static services => services.GetRequiredService<StubOpcSecurityServer>());
        builder.Services.AddSingleton<IOpcServerHost>(static services => new OpcSecuritySampleHost(
            services.GetRequiredService<IOpcDaServer>(),
            services.GetRequiredService<IOPCSecurityNT>(),
            services.GetRequiredService<IOPCSecurityPrivate>(),
            services.GetRequiredService<IOptions<OpcDaServerOptions>>(),
            services.GetRequiredService<OpcObjectRegistry>(),
            services.GetRequiredService<ILogger<OpcSecuritySampleHost>>()));

        var host = builder.Build();

        uint comClassObjectCookie = 0;
        if (embedded && OperatingSystem.IsWindows())
        {
            comClassObjectCookie = RegisterScmFactory(host.Services);
        }

        try
        {
            await host.RunAsync().ConfigureAwait(false);
        }
        finally
        {
            if (embedded && OperatingSystem.IsWindows() && comClassObjectCookie != 0)
            {
                ComClassObjectRegistrar.RevokeClassObject(comClassObjectCookie);
                ComClassObjectRegistrar.Uninitialize();
            }
        }
        return 0;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static uint RegisterScmFactory(IServiceProvider services)
    {
        // The DA-with-Security sample uses the standard OpcDaServerCcw for
        // the IOPCServer CCW; the IOPCSecurityNT / IOPCSecurityPrivate
        // interfaces are exposed through the listening transport endpoint
        // (registered via OpcSecuritySampleHost) and aren't part of the
        // direct CoCreateInstance return — so the SCM factory just hands
        // back the DA CCW. Clients that want IOPCSecurityNT QueryInterface
        // it from the IOPCServer object.
        var serverImpl = services.GetRequiredService<IOpcDaServer>();
        ComClassObjectRegistrar.InitializeMultithreaded();
        uint cookie = ComClassObjectRegistrar.RegisterClassObject(
            SampleClsid,
            createInstanceCallback: requestedIid =>
                Opc.Classic.Da.Hosting.Windows.OpcDaServerCcw.Create(serverImpl, requestedIid));
        ComClassObjectRegistrar.ResumeClassObjects();
        return cookie;
    }

    private sealed class OpcSecuritySampleHost : IOpcServerHost, IAsyncDisposable
    {
        private static readonly Action<ILogger, Guid, string, Exception?> StartingHost = LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(1, nameof(StartingHost)),
            "OpcSecuritySampleHost starting: CLSID={Clsid}, ProgId={ProgId}");

        private static readonly Action<ILogger, Guid, System.Net.EndPoint, Exception?> HostListeningOn = LoggerMessage.Define<Guid, System.Net.EndPoint>(
            LogLevel.Information,
            new EventId(2, nameof(HostListeningOn)),
            "OpcSecuritySampleHost listening: CLSID={Clsid}, endpoint={Endpoint}");

        private static readonly Action<ILogger, Guid, Exception?> StoppingHost = LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(3, nameof(StoppingHost)),
            "OpcSecuritySampleHost stopping: CLSID={Clsid}");

        private readonly IOpcDaServer _daServer;
        private readonly IOPCSecurityNT _securityNt;
        private readonly IOPCSecurityPrivate _securityPrivate;
        private readonly OpcDaServerOptions _options;
        private readonly OpcObjectRegistry _objectRegistry;
        private readonly ILogger<OpcSecuritySampleHost> _logger;
        private OpcServerListener? _listener;

        public OpcSecuritySampleHost(
            IOpcDaServer daServer,
            IOPCSecurityNT securityNt,
            IOPCSecurityPrivate securityPrivate,
            IOptions<OpcDaServerOptions> options,
            OpcObjectRegistry objectRegistry,
            ILogger<OpcSecuritySampleHost> logger)
        {
            _daServer = daServer ?? throw new ArgumentNullException(nameof(daServer));
            _securityNt = securityNt ?? throw new ArgumentNullException(nameof(securityNt));
            _securityPrivate = securityPrivate ?? throw new ArgumentNullException(nameof(securityPrivate));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public string SpecName => "DA+Security";

        public OpcClsidRegistration Registration => new(
            Clsid: _options.Clsid,
            ProgId: _options.ProgId,
            AssemblyName: typeof(SecuritySampleDaServer).Assembly.GetName().Name ?? "Opc.Classic.Samples.OpcSecurityServer",
            TypeName: typeof(SecuritySampleDaServer).FullName ?? "OpcSecuritySampleServer",
            FriendlyName: _options.FriendlyName);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StartingHost(_logger, _options.Clsid, _options.ProgId, null);

            var endpoint = new TcpServerEndpoint(ListenAddressParser.Parse(_options.ListenAddress ?? "127.0.0.1:0"));
            var processor = new RpcServerConnectionProcessor(
                BuildServerDispatchers(),
                _objectRegistry,
                _logger);
            _listener = new OpcServerListener(endpoint, processor, _logger);

            Task started = _listener.StartAsync(cancellationToken);
            HostListeningOn(_logger, _options.Clsid, _listener.LocalEndpoint, null);
            return started;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            StoppingHost(_logger, _options.Clsid, null);

            OpcServerListener? listener = _listener;
            _listener = null;
            if (listener is not null)
            {
                await listener.DisposeAsync().ConfigureAwait(false);
            }
        }

        public ValueTask DisposeAsync() => new(StopAsync(CancellationToken.None));

        private Dictionary<Guid, IOpcServerDispatcher> BuildServerDispatchers()
        {
            var addressSpace = new FlatHierarchicalNamespace();
            return new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCServer.InterfaceId] = new IOPCServerServerDispatcher(_daServer),
                [IOPCCommon.InterfaceId] = new OpcCommonServerDispatcher(new SampleCommonServer()),
                [IOPCBrowseServerAddressSpace.InterfaceId] = new IOPCBrowseServerAddressSpaceServerDispatcher(new DefaultBrowseServerAddressSpace(addressSpace)),
                [IOPCBrowse.InterfaceId] = new IOPCBrowseServerDispatcher(new DefaultBrowse(addressSpace)),
                [IOPCItemProperties.InterfaceId] = new IOPCItemPropertiesServerDispatcher(new DefaultItemProperties()),
                [IOPCItemDeadbandMgt.InterfaceId] = new IOPCItemDeadbandMgtServerDispatcher(new DefaultItemDeadbandMgt()),
                [IOPCItemSamplingMgt.InterfaceId] = new IOPCItemSamplingMgtServerDispatcher(new DefaultItemSamplingMgt()),
                [IOPCSecurityNT.InterfaceId] = new IOPCSecurityNTServerDispatcher(_securityNt),
                [IOPCSecurityPrivate.InterfaceId] = new IOPCSecurityPrivateServerDispatcher(_securityPrivate),
            };
        }
    }

    private sealed class SampleCommonServer : IOpcCommonServer
    {
        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(clientName);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class SecuritySampleDaServer : IOpcDaServer
    {
        private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = StartTime,
                CurrentTime = now,
                LastUpdateTime = now,
                State = OpcServerState.Running,
                GroupCount = 0,
                BandWidth = 0,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "Opc.Classic OPC Security sample DA loopback",
            });
        }

        public Task<int> AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            _ = active;
            _ = requestedUpdateRate;
            _ = localeId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(unchecked(clientHandle + 0x2000));
        }

        public Task AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientGroupHandle,
            int timeBias,
            float percentDeadband,
            int localeId,
            Guid requestedInterfaceId,
            out int serverGroupHandle,
            out int revisedUpdateRate,
            out IOpcInterfaceRef group,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            _ = active;
            _ = timeBias;
            _ = percentDeadband;
            _ = localeId;
            cancellationToken.ThrowIfCancellationRequested();
            serverGroupHandle = unchecked(clientGroupHandle + 0x2000);
            revisedUpdateRate = requestedUpdateRate;
            group = CreateInterfaceRef(requestedInterfaceId, serverGroupHandle);
            return Task.CompletedTask;
        }

        public Task RemoveGroupAsync(
            int serverGroupHandle,
            bool force,
            CancellationToken cancellationToken = default)
        {
            _ = serverGroupHandle;
            _ = force;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(
            int errorCode,
            int localeId,
            CancellationToken cancellationToken = default)
        {
            _ = localeId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"Opc.Classic OPC Security sample error: 0x{errorCode:X8}");
        }

        private static OpcInterfaceRef CreateInterfaceRef(Guid iid, int seed) =>
            new(iid, 0, 1, 1, unchecked((ulong)(uint)seed), Guid.CreateVersion7(), 0, Array.Empty<ushort>());
    }
}
