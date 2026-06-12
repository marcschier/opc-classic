//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class OpcDaServerHostTests
{
    [Test]
    public async Task OpcDaServerHost_starts_and_stops_without_error()
    {
        var host = CreateHost(CreateOptions());

        await host.StartAsync(CancellationToken.None);
        await host.StopAsync(CancellationToken.None);

        await Assert.That(host.SpecName).IsEqualTo(ReadDaSpecName());
    }

    [Test]
    public async Task Registration_carries_options_values()
    {
        var clsid = Guid.Parse("10138C2C-0000-0000-0000-000000000061");
        var options = CreateOptions(clsid, "Vendor.Da.1", "Vendor DA Server");
        var host = CreateHost(options);

        var registration = host.Registration;

        await Assert.That(registration.Clsid).IsEqualTo(clsid);
        await Assert.That(registration.ProgId).IsEqualTo("Vendor.Da.1");
        await Assert.That(registration.FriendlyName).IsEqualTo("Vendor DA Server");
        await Assert.That(registration.AssemblyName).IsEqualTo("Opc.Classic.Da");
        await Assert.That(registration.TypeName).IsEqualTo(typeof(StubDaServer).FullName);
    }

    [Test]
    public async Task AddOpcDaServer_registers_both_IOpcDaServer_and_IOpcServerHost()
    {
        using var provider = CreateProvider(CreateOptions());

        var server = provider.GetRequiredService<IOpcDaServer>();
        var host = provider.GetRequiredService<IOpcServerHost>();

        await Assert.That(server).IsTypeOf<StubDaServer>();
        await Assert.That(host).IsTypeOf<OpcDaServerHost>();
    }

    [Test]
    public async Task AddOpcDaServer_passes_options_to_host()
    {
        var clsid = Guid.Parse("10138C2C-0000-0000-0000-000000000062");
        using var provider = CreateProvider(CreateOptions(clsid, "Vendor.Configured.1", "Configured DA Server"));
        var host = provider.GetRequiredService<IOpcServerHost>();

        var registration = host.Registration;

        await Assert.That(registration.Clsid).IsEqualTo(clsid);
        await Assert.That(registration.ProgId).IsEqualTo("Vendor.Configured.1");
        await Assert.That(registration.FriendlyName).IsEqualTo("Configured DA Server");
    }

    [Test]
    public async Task OpcDaServerOptions_record_equality()
    {
        var clsid = Guid.Parse("10138C2C-0000-0000-0000-000000000063");
        var first = CreateOptions(clsid, "Vendor.Equal.1", "Equal DA Server");
        var second = CreateOptions(clsid, "Vendor.Equal.1", "Equal DA Server");
        var equalOperatorResult = first == second;

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(equalOperatorResult).IsTrue();
    }

    [Test]
    public async Task LocalEndpoint_is_bound_after_StartAsync()
    {
        var host = CreateHost(CreateOptions());

        await host.StartAsync(TestContext.Current!.CancellationToken);

        try
        {
            await Assert.That(host.LocalEndpoint).IsNotNull();
            var bound = host.LocalEndpoint as System.Net.IPEndPoint;
            await Assert.That(bound).IsNotNull();
            await Assert.That(bound!.Port).IsGreaterThan(0);
        }
        finally
        {
            await host.StopAsync(TestContext.Current!.CancellationToken);
        }

        await Assert.That(host.LocalEndpoint).IsNull();
    }

    [Test]
    public async Task Real_TCP_client_can_connect_after_StartAsync()
    {
        var host = CreateHost(CreateOptions());
        await host.StartAsync(TestContext.Current!.CancellationToken);

        try
        {
            var bound = (System.Net.IPEndPoint)host.LocalEndpoint!;
            using var client = new System.Net.Sockets.TcpClient();
            await client.ConnectAsync(bound.Address, bound.Port, TestContext.Current!.CancellationToken);
            await Assert.That(client.Connected).IsTrue();
        }
        finally
        {
            await host.StopAsync(TestContext.Current!.CancellationToken);
        }
    }

    private static OpcDaServerHost CreateHost(OpcDaServerOptions options) =>
        new(new StubDaServer(), Options.Create(options), new OpcObjectRegistry(), NoopLogger<OpcDaServerHost>.Instance);

    private static ServiceProvider CreateProvider(OpcDaServerOptions options)
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<OpcDaServerHost>>(NoopLogger<OpcDaServerHost>.Instance);
        services.AddOpcDaServer<StubDaServer>(configured =>
        {
            configured.Clsid = options.Clsid;
            configured.ProgId = options.ProgId;
            configured.FriendlyName = options.FriendlyName;
            configured.ListenAddress = options.ListenAddress;
        });

        return services.BuildServiceProvider();
    }

    private static OpcDaServerOptions CreateOptions(
        Guid? clsid = null,
        string progId = "Vendor.Da.1",
        string? friendlyName = null) =>
        new()
        {
            Clsid = clsid ?? Guid.Parse("10138C2C-0000-0000-0000-000000000060"),
            ProgId = progId,
            FriendlyName = friendlyName,
        };

    // TUnitAssertions0005 workaround: use non-const indirection for literal assertions.
    private static string ReadDaSpecName() => "DA";

    private sealed class StubDaServer : IOpcDaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "Stub",
            });

        public Task<int> AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(1);

        public Task RemoveGroupAsync(
            int serverGroupHandle,
            bool force,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<string> GetErrorStringAsync(
            int errorCode,
            int localeId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult("ok");
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
