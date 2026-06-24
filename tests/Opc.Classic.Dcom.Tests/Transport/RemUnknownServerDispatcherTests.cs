// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class RemUnknownServerDispatcherTests
{
    private static readonly Guid Iid1 = new("11111111-2222-3333-4444-555555555555");
    private static readonly Guid Iid2 = new("22222222-3333-4444-5555-666666666666");
    private static readonly Guid UnsupportedIid = new("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

    [Test]
    public async Task RemQueryInterface_returns_stdobjrefs_for_supported_iids()
    {
        var registry = new OpcObjectRegistry();
        Guid ipid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher>
        {
            [Iid1] = new StubDispatcher(),
            [Iid2] = new StubDispatcher(),
        });
        var dispatcher = new RemUnknownServerDispatcher(registry);

        DispatchResult result = await dispatcher.DispatchAsync(
            3,
            WriteRemQueryInterfaceRequest(ipid, 3, new[] { Iid1, Iid2, UnsupportedIid }),
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(0);
        OpcRemQIResult[] qiResults = ReadRemQueryInterfaceResponse(result.Payload.Span);
        await Assert.That(qiResults.Length).IsEqualTo(3);
        await Assert.That(qiResults[0].Hresult).IsEqualTo(0);
        await Assert.That(qiResults[1].Hresult).IsEqualTo(0);
        await Assert.That(qiResults[2].Hresult).IsEqualTo(unchecked((int)0x80004002u));
        await Assert.That(qiResults[0].Ipid).IsEqualTo(ipid);
        await Assert.That(qiResults[1].Ipid).IsEqualTo(ipid);
        await Assert.That(registry.Contains(qiResults[0].Ipid)).IsTrue();

        IOpcInterfaceRef decoded = DecodeAsObjRef(Iid1, qiResults[0]);
        await Assert.That(decoded.Iid).IsEqualTo(Iid1);
        await Assert.That(decoded.Ipid).IsEqualTo(ipid);
    }

    [Test]
    public async Task RemAddRef_and_RemRelease_update_lifetime()
    {
        var registry = new OpcObjectRegistry();
        Guid ipid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher> { [Iid1] = new StubDispatcher() });
        var dispatcher = new RemUnknownServerDispatcher(registry);

        DispatchResult addResult = await dispatcher.DispatchAsync(
            4,
            WriteInterfaceRefRequest(ipid, publicRefs: 2),
            CancellationToken.None);

        await Assert.That(addResult.Hresult).IsEqualTo(0);
        int[] addHresults = ReadAddRefResponse(addResult.Payload.Span);
        await Assert.That(addHresults.Length).IsEqualTo(1);
        await Assert.That(addHresults[0]).IsEqualTo(0);
        await Assert.That(registry.Contains(ipid)).IsTrue();

        DispatchResult releaseOne = await dispatcher.DispatchAsync(
            5,
            WriteInterfaceRefRequest(ipid, publicRefs: 1),
            CancellationToken.None);
        await Assert.That(releaseOne.Hresult).IsEqualTo(0);
        await Assert.That(registry.Contains(ipid)).IsTrue();

        DispatchResult releaseLast = await dispatcher.DispatchAsync(
            5,
            WriteInterfaceRefRequest(ipid, publicRefs: 1),
            CancellationToken.None);
        await Assert.That(releaseLast.Hresult).IsEqualTo(0);
        await Assert.That(registry.Contains(ipid)).IsFalse();
    }

    [Test]
    public async Task ObjectExporter_registers_routable_remunknown_ipid()
    {
        var registry = new OpcObjectRegistry();
        var exporter = new IObjectExporterDispatcher(
            static () => null,
            registry,
            new Guid("12345678-1234-1234-1234-1234567890ab"));

        await Assert.That(registry.TryGetDispatcher(exporter.IRemUnknownIpid, RemUnknownServerDispatcher.InterfaceId, out _)).IsTrue();
        await Assert.That(registry.TryGetDispatcher(exporter.IRemUnknownIpid, RemUnknownServerDispatcher.InterfaceId2, out _)).IsTrue();
    }

    [Test]
    public async Task ResolveOxid2_returns_non_empty_tcp_dualstringarray_and_remunknown_ipid()
    {
        var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13579);
        var registry = new OpcObjectRegistry();
        var remUnknownIpid = new Guid("12345678-1234-1234-1234-1234567890ab");
        var exporter = new IObjectExporterDispatcher(
            () => endpoint,
            registry,
            remUnknownIpid);

        DispatchResult result = await exporter.DispatchAsync(4, Array.Empty<byte>(), CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(0);
        (Guid actualRemUnknownIpid, ushort[] bindings, ushort securityOffset) = ReadResolveOxid2Response(result.Payload.Span);
        await Assert.That(actualRemUnknownIpid).IsEqualTo(remUnknownIpid);
        await Assert.That(bindings.Length).IsGreaterThan(0);
        await Assert.That(bindings[0]).IsEqualTo((ushort)0x07);
        await Assert.That(securityOffset).IsGreaterThan((ushort)0);
        await Assert.That(ReadStringBinding(bindings)).IsEqualTo("127.0.0.1[13579]");
    }

    private static byte[] WriteRemQueryInterfaceRequest(Guid ripid, uint cRefs, Guid[] iids)
    {
        var buffer = new byte[256];
        var writer = new NdrWriter(buffer);
        writer.WriteGuid(ripid);
        writer.WriteUInt32(cRefs);
        writer.WriteUInt16((ushort)iids.Length);
        writer.WriteConformanceHeader(iids.Length);
        for (int i = 0; i < iids.Length; i++)
        {
            writer.WriteGuid(iids[i]);
        }

        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static OpcRemQIResult[] ReadRemQueryInterfaceResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        bool hasResults = reader.TryReadReferentId(out _);
        if (!hasResults)
        {
            return Array.Empty<OpcRemQIResult>();
        }

        int count = reader.ReadConformanceHeader();
        var results = new OpcRemQIResult[count];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = NdrRemQIResultCodec.Read(ref reader);
        }

        return results;
    }

    private static byte[] WriteInterfaceRefRequest(Guid ipid, uint publicRefs)
    {
        var buffer = new byte[64];
        var writer = new NdrWriter(buffer);
        writer.WriteUInt16(1);
        writer.WriteConformanceHeader(1);
        writer.WriteGuid(ipid);
        writer.WriteUInt32(publicRefs);
        writer.WriteUInt32(0);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static int[] ReadAddRefResponse(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        int count = reader.ReadConformanceHeader();
        var results = new int[count];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = reader.ReadInt32();
        }

        return results;
    }

    private static IOpcInterfaceRef DecodeAsObjRef(Guid iid, OpcRemQIResult result)
    {
        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        OpcInterfaceRefCodec.Write(
            ref writer,
            new OpcInterfaceRef(
                iid,
                result.Flags,
                result.PublicRefs,
                result.Oxid,
                result.Oid,
                result.Ipid,
                securityOffset: 0,
                resolverBindings: Array.Empty<ushort>()));
        var reader = new NdrReader(buffer.AsSpan(0, writer.Position));
        return OpcInterfaceRefCodec.Read(ref reader);
    }

    private static (Guid RemUnknownIpid, ushort[] Bindings, ushort SecurityOffset) ReadResolveOxid2Response(ReadOnlySpan<byte> payload)
    {
        var reader = new NdrReader(payload);
        _ = reader.TryReadReferentId(out _);
        _ = reader.TryReadReferentId(out _);
        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        var bindings = new ushort[entryCount];
        for (int i = 0; i < bindings.Length; i++)
        {
            bindings[i] = reader.ReadUInt16();
        }

        reader.AlignTo(4);
        Guid remUnknownIpid = reader.ReadGuid();
        return (remUnknownIpid, bindings, securityOffset);
    }

    private static string ReadStringBinding(ushort[] bindings)
    {
        var chars = new char[bindings.Length - 1];
        int count = 0;
        for (int i = 1; i < bindings.Length && bindings[i] != 0; i++)
        {
            chars[count++] = (char)bindings[i];
        }

        return new string(chars, 0, count);
    }

    private sealed class StubDispatcher : IOpcServerDispatcher
    {
        public ValueTask<DispatchResult> DispatchAsync(int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
            ValueTask.FromResult(DispatchResult.Success(ReadOnlyMemory<byte>.Empty));
    }
}
