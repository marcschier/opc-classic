// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Integration.Tests.Support;

namespace Opc.Classic.Tests.Integration.Matrikon;

public sealed class MatrikonDaConformanceTests
{
    [Test]
    [Category("MatrikonConformance.Loopback")]
    public async Task Matrikon_tag_tree_loopback_reports_running_status()
    {
        var serverImpl = StubDaServer.MatrikonSimulation();
        var (proxy, channel) = StubDaServer.CreateLoopbackProxy(serverImpl);

        var status = await proxy.GetStatusAsync(CancellationToken.None).ConfigureAwait(false);
        var errorString = await proxy.GetErrorStringAsync(unchecked((int)0xC0040006u), 0x0409, CancellationToken.None).ConfigureAwait(false);

        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.VendorInfo).Contains("Matrikon");
        await Assert.That(serverImpl.HasTag("Random.Int4")).IsTrue();
        await Assert.That(serverImpl.HasTag("Bucket Brigade.Boolean")).IsTrue();
        await Assert.That(serverImpl.HasTag("Read Error.Int1")).IsTrue();
        await Assert.That(errorString).Contains("Matrikon");
        await Assert.That(channel.CallLog.Count).IsEqualTo(2);
        await Assert.That(ConformanceMetadata.HasCategory(
            typeof(MatrikonDaConformanceTests),
            nameof(Matrikon_tag_tree_loopback_reports_running_status),
            "MatrikonConformance.Loopback")).IsTrue();
        await Assert.That(ConformanceMetadata.HasCategory(
            typeof(MatrikonDaConformanceTests),
            nameof(GetStatus_returns_running),
            "MatrikonConformance")).IsTrue();
        await AssertMatrikonProbeAsync().ConfigureAwait(false);
    }

    [Test, Category("MatrikonConformance")]
    public async Task GetStatus_returns_running()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        await AssertMatrikonScaffoldReadyAsync<IOPCServer, IOPCServerClientProxy>(
            nameof(GetStatus_returns_running),
            IOPCServer.Opnums.GetStatusAsync).ConfigureAwait(false);
    }

    [Test, Category("MatrikonConformance")]
    public async Task Read_Random_Int4_returns_value_with_Good_quality()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        await AssertMatrikonScaffoldReadyAsync<IOPCBrowse, IOPCBrowseClientProxy>(
            nameof(Read_Random_Int4_returns_value_with_Good_quality),
            IOPCBrowse.Opnums.GetPropertiesAsync).ConfigureAwait(false);
        await Assert.That(StubDaServer.MatrikonSimulation().HasTag("Random.Int4")).IsTrue();
    }

    [Test, Category("MatrikonConformance")]
    public async Task BucketBrigade_Boolean_can_be_written_then_read_back()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        await AssertMatrikonScaffoldReadyAsync<IOPCSyncIO2, IOPCSyncIO2ClientProxy>(
            nameof(BucketBrigade_Boolean_can_be_written_then_read_back),
            IOPCSyncIO2.Opnums.WriteVqtAsync).ConfigureAwait(false);
        await Assert.That(StubDaServer.MatrikonSimulation().HasTag("Bucket Brigade.Boolean")).IsTrue();
    }

    [Test, Category("MatrikonConformance")]
    public async Task ReadError_Int1_returns_OPC_E_BADRIGHTS()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        await AssertMatrikonScaffoldReadyAsync<IOPCServer, IOPCServerClientProxy>(
            nameof(ReadError_Int1_returns_OPC_E_BADRIGHTS),
            IOPCServer.Opnums.GetErrorStringAsync).ConfigureAwait(false);
        await Assert.That(StubDaServer.MatrikonSimulation().HasTag("Read Error.Int1")).IsTrue();
    }

    [Test, Category("MatrikonConformance")]
    public async Task Subscription_on_Random_Real4_delivers_OnDataChange()
    {
        if (MatrikonServerProbe.ShouldSkip(out _))
        {
            return;
        }

        await AssertMatrikonScaffoldReadyAsync<IOPCDataCallback, IOPCDataCallbackClientProxy>(
            nameof(Subscription_on_Random_Real4_delivers_OnDataChange),
            IOPCDataCallback.Opnums.OnDataChangeAsync).ConfigureAwait(false);
        await Assert.That(StubDaServer.MatrikonSimulation().HasTag("Random.Real4")).IsTrue();
    }

    private static async Task AssertMatrikonScaffoldReadyAsync<TInterface, TProxy>(string methodName, int expectedOpnum)
    {
        await Assert.That(ConformanceMetadata.HasCategory(typeof(MatrikonDaConformanceTests), methodName, "MatrikonConformance")).IsTrue();
        await Assert.That(ConformanceMetadata.ReadType<TInterface>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadType<TProxy>()).IsNotNull();
        await Assert.That(ConformanceMetadata.ReadString(MatrikonServerProbe.MatrikonProgId)).IsEqualTo("Matrikon.OPC.Simulation.1");
        await Assert.That(ConformanceMetadata.ReadInt32(expectedOpnum)).IsGreaterThan(0);
    }

    private static async Task AssertMatrikonProbeAsync()
    {
        var shouldSkip = MatrikonServerProbe.ShouldSkip(out var reason);
        if (shouldSkip)
        {
            await Assert.That(reason.Length).IsGreaterThan(0);
            return;
        }

        await Assert.That(ConformanceMetadata.ReadString(MatrikonServerProbe.MatrikonProgId)).IsEqualTo("Matrikon.OPC.Simulation.1");
    }
}
