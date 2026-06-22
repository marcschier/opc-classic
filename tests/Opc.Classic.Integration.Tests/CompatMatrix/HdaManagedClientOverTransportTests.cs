// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Integration.Tests.Support;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

public sealed class HdaManagedClientOverTransportTests
{
    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Hda_managed_client_round_trips_GetStatus_over_real_listener()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        var stub = new StubHdaServer();
        await using ServiceProvider provider = BuildServiceProvider(stub);
        OpcHdaServerHost host = ResolveHost(provider);

        await host.StartAsync(cancellationToken);
        try
        {
            await using DcomCallChannel channel = await ConnectClientAsync(host, cancellationToken);
            var proxy = new IOPCHDA_ServerClientProxy(channel);

            OpcServerStatus status = await proxy.GetStatusAsync(cancellationToken);

            await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
            await Assert.That(status.VendorInfo).IsEqualTo("Loopback HDA Stub Server");
            await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Hda);
            await Assert.That(status.MaxReturnValues).IsEqualTo(500);
            await Assert.That(stub.StatusCallCount).IsEqualTo(1);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Hda_managed_client_round_trips_metadata_and_validate_item_ids_over_real_listener()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        var stub = new StubHdaServer();
        await using ServiceProvider provider = BuildServiceProvider(stub);
        OpcHdaServerHost host = ResolveHost(provider);

        await host.StartAsync(cancellationToken);
        try
        {
            await using DcomCallChannel channel = await ConnectClientAsync(host, cancellationToken);
            var proxy = new IOPCHDA_ServerClientProxy(channel);

            await proxy.GetItemAttributesAsync(
                out int[] attributeIds,
                out string[] attributeNames,
                out string[] attributeDescriptions,
                out int[] attributeDataTypes,
                cancellationToken);
            int[] validation = await proxy.ValidateItemIDsAsync(
                ["Plant.Hda.Tag1", "Missing.Hda.Tag"],
                cancellationToken);

            await Assert.That(attributeIds).IsEquivalentTo([1, 2]);
            await Assert.That(attributeNames).IsEquivalentTo(["DataType", "Description"]);
            await Assert.That(attributeDescriptions).IsEquivalentTo(["Variant type", "Human text"]);
            await Assert.That(attributeDataTypes).IsEquivalentTo([(int)VarType.VT_I4, (int)VarType.VT_BSTR]);
            await Assert.That(validation).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.UnknownItemId.Code]);
            await Assert.That(stub.LastItemIds).IsEquivalentTo(["Plant.Hda.Tag1", "Missing.Hda.Tag"]);
            await Assert.That(stub.AttributeCallCount).IsEqualTo(1);
            await Assert.That(stub.ValidateCallCount).IsEqualTo(1);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Hda_managed_client_two_back_to_back_calls_share_connection()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        var stub = new StubHdaServer();
        await using ServiceProvider provider = BuildServiceProvider(stub);
        OpcHdaServerHost host = ResolveHost(provider);

        await host.StartAsync(cancellationToken);
        try
        {
            await using DcomCallChannel channel = await ConnectClientAsync(host, cancellationToken);
            var proxy = new IOPCHDA_ServerClientProxy(channel);

            OpcServerStatus status = await proxy.GetStatusAsync(cancellationToken);
            int[] validation = await proxy.ValidateItemIDsAsync(["Plant.Hda.Tag1"], cancellationToken);

            await Assert.That(status.VendorInfo).IsEqualTo("Loopback HDA Stub Server");
            await Assert.That(validation).IsEquivalentTo([OpcResultId.Ok.Code]);
            await Assert.That(stub.StatusCallCount).IsEqualTo(1);
            await Assert.That(stub.ValidateCallCount).IsEqualTo(1);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    private static ServiceProvider BuildServiceProvider(StubHdaServer server)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOpcHdaServer>(server);
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<OpcHdaServerHost>();
        services.AddSingleton<IOpcServerHost>(static sp => sp.GetRequiredService<OpcHdaServerHost>());
        services.Configure<OpcHdaServerOptions>(static options =>
        {
            options.Clsid = Guid.NewGuid();
            options.ProgId = "Managed.Hda.Compat.1";
            options.FriendlyName = "Managed HDA transport test server";
            options.ListenAddress = "127.0.0.1:0";
        });
        return services.BuildServiceProvider();
    }

    private static OpcHdaServerHost ResolveHost(ServiceProvider provider) =>
        (OpcHdaServerHost)provider.GetRequiredService<IOpcServerHost>();

    private static async Task<DcomCallChannel> ConnectClientAsync(
        OpcHdaServerHost host,
        CancellationToken cancellationToken)
    {
        var endpoint = (IPEndPoint?)host.LocalEndpoint
            ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            cancellationToken);
        return new DcomCallChannel(transport, NoOpAuthContext.Instance);
    }
}
