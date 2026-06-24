// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Integration.Tests.Support;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F4Auth
{
    private const string KerberosSkipReason =
        "Authenticated calls over the managed TCP listener are not yet supported: Kerberos requires the KDC fixture covered by KerberosKdcFixtureTests plus server-side Kerberos acceptor wiring on the listener.";

    private const string SpnegoSkipReason =
        "Authenticated calls over the managed TCP listener are not yet supported: SPNEGO requires server-side negotiation wiring on the listener before it can select NTLMv2 or Kerberos.";

    private const string Domain = "LOOPBACK";
    private const string User = "phase1-user";
    private const string Password = "phase1-password";

    [Test]
    public async Task Ntlmv2_authenticates_the_managed_loopback_call_path()
    {
        StubDaServer server = StubDaServer.CompatMatrixNet10Server();
        await using OpcServerListener listener = await StartListenerAsync(server);
        await using DcomCallChannel channel = await ConnectAsync(listener, Password, OpcProtectionLevel.Integrity);
        var proxy = new IOPCServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(TestContext.Current!.CancellationToken);

        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.VendorInfo).IsEqualTo(server.VendorInfo);
    }

    [Test]
    public async Task Ntlmv2_rejects_wrong_password_on_managed_loopback_call_path()
    {
        await using OpcServerListener listener = await StartListenerAsync(StubDaServer.CompatMatrixNet10Server());
        await using DcomCallChannel channel = await ConnectAsync(listener, "wrong-password", OpcProtectionLevel.Integrity);
        var proxy = new IOPCServerClientProxy(channel);

        await Assert.That(async () => await proxy.GetStatusAsync(TestContext.Current!.CancellationToken))
            .Throws<Exception>();
    }

    [Test]
    public async Task Auth_required_listener_rejects_anonymous_bind_and_plain_request()
    {
        await using OpcServerListener listener = await StartListenerAsync(StubDaServer.CompatMatrixNet10Server());
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            TestContext.Current!.CancellationToken);
        await using var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);
        var proxy = new IOPCServerClientProxy(channel);

        await Assert.That(async () => await proxy.GetStatusAsync(TestContext.Current!.CancellationToken))
            .Throws<Exception>();
    }

    [Test]
    public async Task Ntlmv2_privacy_authenticates_and_seals_the_managed_loopback_call_path()
    {
        StubDaServer server = StubDaServer.CompatMatrixNet10Server();
        await using OpcServerListener listener = await StartListenerAsync(server);
        await using DcomCallChannel channel = await ConnectAsync(listener, Password, OpcProtectionLevel.Privacy);
        var proxy = new IOPCServerClientProxy(channel);

        OpcServerStatus status = await proxy.GetStatusAsync(TestContext.Current!.CancellationToken);

        await Assert.That(status.VendorInfo).IsEqualTo(server.VendorInfo);
    }

    [Test, Skip(KerberosSkipReason)]
    public void Kerberos_authenticates_the_managed_loopback_call_path()
    {
        // TODO: use the Testcontainers KDC fixture to issue tickets and authenticate the loopback channel.
    }

    [Test, Skip(SpnegoSkipReason)]
    public void Spnego_negotiates_ntlmv2_or_kerberos_for_the_managed_loopback_call_path()
    {
        // TODO: exercise SPNEGO negotiation and assert the selected NTLMv2/Kerberos mechanism is enforced.
    }

    private static async Task<OpcServerListener> StartListenerAsync(StubDaServer server)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCServer.InterfaceId] = new IOPCServerServerDispatcher(server),
            },
            new ConfiguredAuthenticationSource(User, Password, Domain));
        var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(TestContext.Current!.CancellationToken);
        return listener;
    }

    private static async Task<DcomCallChannel> ConnectAsync(
        OpcServerListener listener,
        string password,
        OpcProtectionLevel protectionLevel)
    {
        var endpoint = (IPEndPoint)listener.LocalEndpoint;
        var credentials = new NetworkCredential(User, password, Domain);
        var connectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse($"opcda://{endpoint.Address}:{endpoint.Port}/Loopback.Auth"),
            credentials,
            protectionLevel);
        IAuthContext authContext = NtlmAuthentication.CreateAuthContext(connectData);
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            TestContext.Current!.CancellationToken);
        return new DcomCallChannel(transport, authContext);
    }
}
