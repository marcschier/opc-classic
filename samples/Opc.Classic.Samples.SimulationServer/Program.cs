// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Hosting.Windows;
using Opc.Classic.Mcp.Capture;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Transports;

// Handle Windows DCOM registration (--register / --unregister) before standing up anything
// else, so the simulation DA/AE/HDA servers appear in OpcEnum and can be activated by native
// DCOM clients such as Matrikon OPC Explorer. No-op (with a message) off Windows.
(OpcClsidRegistration Registration, OpcComponentCategory[] Categories)[] simServerRegistrations =
[
    (new OpcClsidRegistration(
        Clsid: new Guid("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0001"),
        ProgId: "Opc.Classic.Simulation.DA.1",
        AssemblyName: "Opc.Classic.Samples.SimulationServer",
        TypeName: typeof(SimDaHostServer).FullName!,
        FriendlyName: "Opc.Classic Full-Feature Simulation Server (DA)"),
        [OpcComponentCategories.OpcDaServer20, OpcComponentCategories.OpcDaServer30]),
    (new OpcClsidRegistration(
        Clsid: new Guid("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0002"),
        ProgId: "Opc.Classic.Simulation.AE.1",
        AssemblyName: "Opc.Classic.Samples.SimulationServer",
        TypeName: typeof(SimAeHostServer).FullName!,
        FriendlyName: "Opc.Classic Full-Feature Simulation Server (AE)"),
        [OpcComponentCategories.OpcAeServer10]),
    (new OpcClsidRegistration(
        Clsid: new Guid("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0003"),
        ProgId: "Opc.Classic.Simulation.HDA.1",
        AssemblyName: "Opc.Classic.Samples.SimulationServer",
        TypeName: typeof(SimHdaHostServer).FullName!,
        FriendlyName: "Opc.Classic Full-Feature Simulation Server (HDA)"),
        [OpcComponentCategories.OpcHdaServer10]),
];
bool registrationHandled = false;
foreach ((OpcClsidRegistration registration, OpcComponentCategory[] categories) in simServerRegistrations)
{
    if (SampleServerRegistrationCommand.TryHandle(args, registration, categories, out int registrationExit))
    {
        registrationHandled = true;
        if (registrationExit != 0)
        {
            return registrationExit;
        }
    }
}

if (registrationHandled)
{
    return 0;
}

using ILoggerFactory loggerFactory = LoggerFactory.Create(b =>
{
    b.ClearProviders();
    b.AddConsole(static o => o.LogToStandardErrorThreshold = LogLevel.Trace);
});

// Run-mode split:
//   default                 -> MCP server (stdio) + in-memory simulation only
//   --listen                -> MCP server + DA/AE/HDA real TCP listeners
//   -Embedding / --opc-only -> OPC servers only (no MCP stdio); used by Windows SCM
//                              activation so a native explorer (Matrikon) can launch us.
bool embedding = SampleServerRegistrationCommand.HasEmbeddingFlag(args);
bool opcOnly = embedding || args.Contains("--opc-only", StringComparer.OrdinalIgnoreCase);
if (opcOnly)
{
    return await RunOpcServersOnlyAsync(args, embedding, loggerFactory).ConfigureAwait(false);
}

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

// All logs go to stderr because stdio MCP transports use stdout for protocol traffic.
builder.Logging.ClearProviders();
builder.Logging.AddConsole(static o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Stand up the full feature-area simulation and register every inmemory:// endpoint.
SimulationServerHandle simulation = SimulationServerRegistration.RegisterAll(
    loggerFactory,
    namePrefix: "sim");

builder.Services.AddSingleton(simulation);
builder.Services.AddSingleton<IOpcSessionManager, OpcSessionManager>();
builder.Services.AddSingleton(sp => new CaptureSessionManager(
    scratchRoot: Path.Combine(Path.GetTempPath(), "opc.classic.simulation.capture"),
    logger: sp.GetService<ILoggerFactory>()?.CreateLogger<CaptureSessionManager>()));

// Contribute Discovery / Security services that are resolved from DI rather than a registry.
simulation.ConfigureMcpHost(builder.Services);

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<SessionTools>()
    .WithTools<DiscoveryTools>()
    .WithTools<DaClientTools>()
    .WithTools<AeClientTools>()
    .WithTools<HdaClientTools>()
    .WithTools<BatchTools>()
    .WithTools<CommandsTools>()
    .WithTools<CpxTools>()
    .WithTools<DxTools>()
    .WithTools<SecurityTools>()
    .WithTools<XmlDaTools>()
    .WithTools<CaptureTools>();

ILogger startupLogger = loggerFactory.CreateLogger("Opc.Classic.Samples.SimulationServer");
startupLogger.LogInformation("Simulation server starting.");

Console.Error.WriteLine(string.Create(
    CultureInfo.InvariantCulture,
    $"Simulation server ready: {simulation.Model.Tags.Count} tags, {simulation.ConnectionStrings.Count} feature-area endpoints."));
foreach (KeyValuePair<string, string> endpoint in simulation.ConnectionStrings)
{
    Console.Error.WriteLine(string.Create(
        CultureInfo.InvariantCulture,
        $"  {endpoint.Key,-9} -> {endpoint.Value}"));
}

Console.Error.WriteLine(string.Create(
    CultureInfo.InvariantCulture,
    $"Discovery host: {Opc.Classic.Samples.SimulationServer.Discovery.SimDiscoveryModule.DiscoveryHost}. Connect MCP sessions with the connection strings above."));

// Optional: also expose the simulation over a real transport (managed ncacn_ip_tcp,
// and native DCOM when registered on Windows) so external OPC clients (the Opc.Classic
// MCP server via tcp:// / dcom://, or Matrikon OPC Explorer) can connect. Opt in with
// --listen; the bind address/port can be overridden with OPC_CLASSIC_SIM_DA_LISTEN.
SimulationTransportHost? transportHost = null;
if (args.Contains("--listen", StringComparer.OrdinalIgnoreCase))
{
    var transportOptions = new SimulationTransportOptions
    {
        DaListenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_SIM_DA_LISTEN") ?? "0.0.0.0:0",
    };
    transportHost = SimulationTransportHost.Create(simulation.Model, transportOptions, loggerFactory);
    await transportHost.StartAsync().ConfigureAwait(false);
    Console.Error.WriteLine(string.Create(
        CultureInfo.InvariantCulture,
        $"DA  transport listening: tcp://{transportHost.DaEndpoint} (ProgID {transportHost.Options.DaProgId})."));
    Console.Error.WriteLine(string.Create(
        CultureInfo.InvariantCulture,
        $"AE  transport listening: tcp://{transportHost.AeEndpoint} (ProgID {transportHost.Options.AeProgId})."));
    Console.Error.WriteLine(string.Create(
        CultureInfo.InvariantCulture,
        $"HDA transport listening: tcp://{transportHost.HdaEndpoint} (ProgID {transportHost.Options.HdaProgId})."));
}

try
{
    await builder.Build().RunAsync().ConfigureAwait(false);
}
finally
{
    if (transportHost is not null)
    {
        await transportHost.DisposeAsync().ConfigureAwait(false);
    }

    simulation.Dispose();
}

return 0;

// ----- OPC-servers-only run mode (no MCP stdio) -----

async Task<int> RunOpcServersOnlyAsync(string[] commandArgs, bool isEmbedding, ILoggerFactory factory)
{
    var model = new SimulatedPlantModel();
    var options = new SimulationTransportOptions
    {
        DaListenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_SIM_DA_LISTEN") ?? "0.0.0.0:0",
    };
    SimulationTransportHost transport = SimulationTransportHost.Create(model, options, factory);
    await transport.StartAsync().ConfigureAwait(false);

    Console.Error.WriteLine(string.Create(
        CultureInfo.InvariantCulture,
        $"OPC servers only. DA tcp://{transport.DaEndpoint}, AE tcp://{transport.AeEndpoint}, HDA tcp://{transport.HdaEndpoint}."));

    uint classObjectCookie = 0;
    if (isEmbedding && OperatingSystem.IsWindows())
    {
        classObjectCookie = RegisterScmDaFactory(transport, options);
    }

    try
    {
        await Host.CreateApplicationBuilder(commandArgs).Build().RunAsync().ConfigureAwait(false);
    }
    finally
    {
        if (classObjectCookie != 0 && OperatingSystem.IsWindows())
        {
            ComClassObjectRegistrar.RevokeClassObject(classObjectCookie);
            ComClassObjectRegistrar.Uninitialize();
        }

        await transport.DisposeAsync().ConfigureAwait(false);
    }

    return 0;
}

[SupportedOSPlatform("windows")]
static uint RegisterScmDaFactory(SimulationTransportHost transport, SimulationTransportOptions options)
{
    IOpcDaServer serverImpl = transport.DaServer;
    ComClassObjectRegistrar.InitializeMultithreaded();
    uint cookie = ComClassObjectRegistrar.RegisterClassObject(
        options.DaClsid,
        createInstanceCallback: requestedIid =>
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException();
            }

            return Opc.Classic.Da.Hosting.Windows.OpcDaServerCcw.Create(serverImpl, requestedIid);
        });
    ComClassObjectRegistrar.ResumeClassObjects();
    return cookie;
}
