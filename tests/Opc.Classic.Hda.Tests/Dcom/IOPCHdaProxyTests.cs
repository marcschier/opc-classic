// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Hda.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Hda.Tests.Dcom;

public sealed class IOPCHdaProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task SyncRead_ReadRaw_invokes_channel_with_correct_metadata_and_decodes_array()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        int observedPayloadLength = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUInt32(0));
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCHDA_SyncReadClientProxy(channel);
        OpcHdaItem[] results = await proxy.ReadRawAsync(
            OpcHdaTime.FromString("NOW-1H"),
            OpcHdaTime.FromString("NOW"),
            100,
            bounds: false,
            new[] { 101, 202 },
            CancellationToken.None);

        int expectedOpnum = IOPCHDA_SyncRead.Opnums.ReadRawAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCHDA_SyncRead.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
        await Assert.That(results.Length).IsEqualTo(0);
    }

    [Test]
    public async Task AsyncRead_AdviseRaw_returns_cancel_id()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(77));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCHDA_AsyncReadClientProxy(channel);
        int cancelId = await proxy.AdviseRawAsync(
            12,
            OpcHdaTime.FromString("NOW"),
            10_000,
            new[] { 42 },
            CancellationToken.None);

        int expectedOpnum = IOPCHDA_AsyncRead.Opnums.AdviseRawAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCHDA_AsyncRead.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(cancelId).IsEqualTo(77);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 1024)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }
}
