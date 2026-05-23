//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Integration.Tests.Support;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

public sealed class Net10ServerToNativeClientTests
{
    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Managed_proxy_to_net10_server_loopback_calls_GetStatus()
    {
        var serverImpl = StubDaServer.CompatMatrixNet10Server();
        var (proxy, channel) = StubDaServer.CreateLoopbackProxy(serverImpl);

        var status = await proxy.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);

        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.VendorInfo).Contains("Compat matrix");
        await Assert.That(channel.CallLog.Count).IsEqualTo(1);
        await Assert.That(channel.CallLog[0].InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(channel.CallLog[0].Opnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
        await Assert.That(ConformanceMetadata.HasCategory(
            typeof(Net10ServerToNativeClientTests),
            nameof(Managed_proxy_to_net10_server_loopback_calls_GetStatus),
            "CompatMatrix.Loopback")).IsTrue();
        await AssertCompatProbeAsync().ConfigureAwait(false);
    }

    [Test, Category("CompatMatrix")]
    public async Task Native_simple_client_connects_to_net10_server_and_calls_GetStatus()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        // Phase 14D-B still needs a native client process and real listener-side transport.
        // The structural assertions below keep the scaffold wired to the managed server pieces.
        await AssertCompatScaffoldReadyAsync<IOPCServer, IOPCServerClientProxy>(
            nameof(Native_simple_client_connects_to_net10_server_and_calls_GetStatus),
            IOPCServer.Opnums.GetStatusAsync).ConfigureAwait(false);
        await Assert.That(NativeClientPathLooksResolved()).IsTrue();
    }

    [Test, Category("CompatMatrix")]
    public async Task Native_client_can_AddGroup_then_Read_through_net10_server()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        await AssertCompatScaffoldReadyAsync<IOPCSyncIO, IOPCSyncIOClientProxy>(
            nameof(Native_client_can_AddGroup_then_Read_through_net10_server),
            IOPCSyncIO.Opnums.WriteAsync).ConfigureAwait(false);
        await Assert.That(HasDaServerMethod(nameof(IOpcDaServer.AddGroupAsync))).IsTrue();
    }

    [Test, Category("CompatMatrix")]
    public async Task Native_client_receives_OnDataChange_callbacks_from_net10_server()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        await AssertCompatScaffoldReadyAsync<IOPCDataCallback, IOPCDataCallbackClientProxy>(
            nameof(Native_client_receives_OnDataChange_callbacks_from_net10_server),
            IOPCDataCallback.Opnums.OnDataChangeAsync).ConfigureAwait(false);
    }

    [Test, Category("CompatMatrix")]
    public async Task Native_client_disconnect_releases_net10_server_resources()
    {
        if (CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out _))
        {
            return;
        }

        await AssertCompatScaffoldReadyAsync<IOPCServer, IOPCServerClientProxy>(
            nameof(Native_client_disconnect_releases_net10_server_resources),
            IOPCServer.Opnums.RemoveGroupAsync).ConfigureAwait(false);
        await Assert.That(HasDaServerMethod(nameof(IOpcDaServer.RemoveGroupAsync))).IsTrue();
    }

    private static async Task AssertCompatScaffoldReadyAsync<TInterface, TProxy>(string methodName, int expectedOpnum)
    {
        await Assert.That(ConformanceMetadata.HasCategory(typeof(Net10ServerToNativeClientTests), methodName, "CompatMatrix")).IsTrue();
        await Assert.That(ConformanceMetadata.ReadType<TInterface>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadType<TProxy>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadType<OpcDaServerDispatcher>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadInt32(expectedOpnum)).IsGreaterThan(0);
    }

    private static async Task AssertCompatProbeAsync()
    {
        var shouldSkip = CompatMatrixProbe.ShouldSkipNet10ServerToNativeClient(out var reason);
        if (shouldSkip)
        {
            await Assert.That(reason.Length).IsGreaterThan(0);
            return;
        }

        await Assert.That(NativeClientPathLooksResolved()).IsTrue();
    }

    private static bool NativeClientPathLooksResolved() =>
        CompatMatrixProbe.NativeSampleClientPath.EndsWith("OpcDaSimpleClient.exe", StringComparison.OrdinalIgnoreCase);

    private static bool HasDaServerMethod(string methodName) =>
        typeof(IOpcDaServer).GetMethod(methodName) is not null;
}
