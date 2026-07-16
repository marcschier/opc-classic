// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom;
using Opc.Classic.Discovery.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Discovery.Tests;

public sealed class OpcEnumGuidProxyAndDispatcherTests
{
    private static readonly Guid FirstGuid = Guid.Parse("10138C2C-0000-0000-0000-000000000101");
    private static readonly Guid SecondGuid = Guid.Parse("10138C2C-0000-0000-0000-000000000102");

    [Test]
    public async Task ClientProxy_SkipAsync_InvokesSkipOpnumAndEncodesCount()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        int observedCount = -1;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            observedCount = reader.ReadInt32();
            return Task.FromResult(new NdrCallResult(OpcResultId.Ok.Code, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new IOPCEnumGUIDClientProxy(channel);

        await proxy.SkipAsync(3, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEnumGUIDClientProxy.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEnumGUIDClientProxy.Opnums.Skip);
        await Assert.That(observedCount).IsEqualTo(3);
    }

    [Test]
    public async Task ClientProxy_ResetAsync_InvokesResetOpnum()
    {
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((_, opnum, payload, _) =>
        {
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(OpcResultId.Ok.Code, payload));
        });
        var proxy = new IOPCEnumGUIDClientProxy(channel);

        await proxy.ResetAsync(CancellationToken.None);

        await Assert.That(observedOpnum).IsEqualTo(IOPCEnumGUIDClientProxy.Opnums.Reset);
    }

    [Test]
    public async Task ClientProxy_CloneAsync_DecodesInterfaceReference()
    {
        IOpcInterfaceRef expected = CreateInterfaceRef();
        var channel = new InMemoryCallChannel((_, opnum, _, _) =>
        {
            ReadOnlyMemory<byte> response = opnum == IOPCEnumGUIDClientProxy.Opnums.Clone
                ? EncodeInterfaceRef(expected)
                : ReadOnlyMemory<byte>.Empty;
            return Task.FromResult(new NdrCallResult(OpcResultId.Ok.Code, response));
        });
        var proxy = new IOPCEnumGUIDClientProxy(channel);

        IOpcInterfaceRef actual = await proxy.CloneAsync(CancellationToken.None);

        await Assert.That(actual.Iid).IsEqualTo(expected.Iid);
        await Assert.That(actual.Oid).IsEqualTo(expected.Oid);
        await Assert.That(actual.Ipid).IsEqualTo(expected.Ipid);
    }

    [Test]
    public async Task ClientProxy_Next_rejects_mismatched_native_max_count()
    {
        byte[] response = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(2);
            writer.WriteUInt32(0);
            writer.WriteUInt32(1);
            writer.WriteGuid(FirstGuid);
            writer.WriteUInt32(1);
        });
        var proxy = new IOPCEnumGUIDClientProxy(
            new InMemoryCallChannel((_, _, _, _) =>
                Task.FromResult(new NdrCallResult(OpcResultId.Ok.Code, response))));

        await Assert.That(async () => { _ = await proxy.NextAsync(3); })
            .Throws<InvalidDataException>();
    }

    [Test]
    public async Task Dispatcher_Next_EncodesFetchedGuidsAndPartialHresult()
    {
        var server = new StubEnumGuidServer([FirstGuid, SecondGuid]);
        var dispatcher = new IOPCEnumGUIDServerDispatcher(server);

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCEnumGUIDClientProxy.Opnums.Next,
            EncodeInt32(3),
            CancellationToken.None);

        var reader = new NdrReader(result.Payload.Span);
        Guid[] classIds = reader.ReadVaryingConformantGuidArray();
        int fetched = reader.ReadInt32();
        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(fetched).IsEqualTo(2);
        await Assert.That(classIds).IsEquivalentTo(new[] { FirstGuid, SecondGuid });
    }

    [Test]
    public async Task Dispatcher_Next_matches_native_conformant_varying_fixture()
    {
        var dispatcher = new IOPCEnumGUIDServerDispatcher(
            new StubEnumGuidServer([FirstGuid, SecondGuid]));
        byte[] expected =
        [
            0x03, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x02, 0x00, 0x00, 0x00,
            0x2C, 0x8C, 0x13, 0x10, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01,
            0x2C, 0x8C, 0x13, 0x10, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x02,
            0x02, 0x00, 0x00, 0x00,
        ];

        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCEnumGUIDClientProxy.Opnums.Next,
            EncodeInt32(3),
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(result.Payload.ToArray()).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Dispatcher_SkipResetAndClone_DelegateToServer()
    {
        var server = new StubEnumGuidServer([FirstGuid, SecondGuid]) { CloneRef = CreateInterfaceRef() };
        var dispatcher = new IOPCEnumGUIDServerDispatcher(server);

        DispatchResult skip = await dispatcher.DispatchAsync(
            IOPCEnumGUIDClientProxy.Opnums.Skip,
            EncodeInt32(1),
            CancellationToken.None);
        DispatchResult reset = await dispatcher.DispatchAsync(
            IOPCEnumGUIDClientProxy.Opnums.Reset,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);
        DispatchResult clone = await dispatcher.DispatchAsync(
            IOPCEnumGUIDClientProxy.Opnums.Clone,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        IOpcInterfaceRef cloneRef = DecodeInterfaceRef(clone.Payload);
        await Assert.That(skip.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(reset.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.ResetCount).IsEqualTo(1);
        await Assert.That(cloneRef.Iid).IsEqualTo(server.CloneRef.Iid);
    }

    [Test]
    public async Task Dispatcher_UnknownOpnum_ReturnsNotImplemented()
    {
        var dispatcher = new IOPCEnumGUIDServerDispatcher(new StubEnumGuidServer([]));

        DispatchResult result = await dispatcher.DispatchAsync(999, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(result.Payload.IsEmpty).IsTrue();
    }

    private static byte[] EncodeInt32(int value) => WritePayload((ref NdrWriter writer) => writer.WriteInt32(value));

    private static ReadOnlyMemory<byte> EncodeInterfaceRef(IOpcInterfaceRef interfaceRef) =>
        WritePayload((ref NdrWriter writer) => OpcMInterfacePointerCodec.Write(ref writer, interfaceRef));

    private static IOpcInterfaceRef DecodeInterfaceRef(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        return OpcMInterfacePointerCodec.Read(ref reader)
            ?? throw new InvalidDataException("Expected a non-null IOPCEnumGUID clone.");
    }

    private static byte[] WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[512];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static IOpcInterfaceRef CreateInterfaceRef() => new OpcInterfaceRef(
        OpcGuids.IID_IOPCEnumGUID,
        flags: 0,
        publicRefs: 5,
        oxid: 0x100,
        oid: 0x200,
        ipid: Guid.Parse("10138C2C-0000-0000-0000-0000000001FF"),
        securityOffset: 0,
        resolverBindings: Array.Empty<ushort>());

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed class StubEnumGuidServer : IOPCEnumGUIDServer
    {
        private readonly IReadOnlyList<Guid> _classIds;
        private int _index;

        public StubEnumGuidServer(IReadOnlyList<Guid> classIds) =>
            _classIds = classIds;

        public IOpcInterfaceRef CloneRef { get; init; } = CreateInterfaceRef();
        public int ResetCount { get; private set; }

        public Task<OpcEnumGuidNextResult> NextAsync(int count, CancellationToken cancellationToken = default)
        {
            int fetched = Math.Min(count, _classIds.Count - _index);
            var result = new Guid[fetched];
            for (int i = 0; i < fetched; i++)
            {
                result[i] = _classIds[_index++];
            }

            return Task.FromResult(new OpcEnumGuidNextResult(result, fetched));
        }

        public Task<int> SkipAsync(int count, CancellationToken cancellationToken = default)
        {
            int skipped = Math.Min(count, _classIds.Count - _index);
            _index += skipped;
            return Task.FromResult(skipped);
        }

        public Task ResetAsync(CancellationToken cancellationToken = default)
        {
            ResetCount++;
            _index = 0;
            return Task.CompletedTask;
        }

        public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CloneRef);
    }
}
