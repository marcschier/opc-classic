// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Dcom;
using Opc.Classic.Discovery;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Mcp.Tests;

public sealed class DiscoveryToolsTests
{
    [Test]
    public async Task Enumerate_servers_round_trips_via_mcp_client_and_synthetic_opcenum()
    {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-000000000401");
        var opcEnum = new SyntheticOpcEnumServer()
            .AddServer(OpcGuids.CATID_OPCDAServer20, classId, "Vendor.McpDa.1", "Vendor MCP DA", "Vendor.McpDa");
        await using McpTestServer server = await McpTestServer.CreateAsync(services =>
        {
            services.AddSingleton<IOpcDiscovery>(new OpcEnumClient("opc-host", opcEnum, new[] { OpcGuids.CATID_OPCDAServer20 }));
        }).ConfigureAwait(false);

        OpcServerDescriptorDto[] descriptors = await server.CallToolAsync<OpcServerDescriptorDto[]>(
            "opcclassic.discovery.enumerate_servers",
            new Dictionary<string, object>
            {
                ["host"] = "opc-host",
                ["categoryIds"] = new[] { OpcGuids.CATID_OPCDAServer20.ToString("D") },
            }).ConfigureAwait(false);

        await Assert.That(descriptors.Length).IsEqualTo(1);
        await Assert.That(descriptors[0].ClassId).IsEqualTo(classId);
        await Assert.That(descriptors[0].ProgId).IsEqualTo("Vendor.McpDa.1");
        await Assert.That(descriptors[0].UserType).IsEqualTo("Vendor MCP DA");
        await Assert.That(descriptors[0].VerIndProgId).IsEqualTo("Vendor.McpDa");
        await Assert.That(descriptors[0].Host).IsEqualTo("opc-host");
        await Assert.That(opcEnum.Calls.Count).IsGreaterThan(0);
    }
}

internal sealed class SyntheticOpcEnumServer : IOpcEnumCallChannelFactory
{
    private static readonly Guid RemoteScmActivatorInterfaceId = new("000001A0-0000-0000-C000-000000000046");
    private readonly Dictionary<Guid, List<Guid>> _categoryClasses = new();
    private readonly Dictionary<Guid, SyntheticOpcServerDetails> _details = new();
    private readonly Queue<IReadOnlyList<Guid>> _pendingEnums = new();
    private readonly InMemoryCallChannel _channel;
    private IReadOnlyList<Guid>? _currentEnum;
    private int _currentEnumIndex;

    public SyntheticOpcEnumServer() => _channel = new InMemoryCallChannel(HandleCallAsync);

    public IReadOnlyList<InMemoryCall> Calls => _channel.CallLog;

    public SyntheticOpcEnumServer AddServer(Guid categoryId, Guid classId, string progId, string userType, string? verIndProgId)
    {
        if (!_categoryClasses.TryGetValue(categoryId, out List<Guid>? classIds))
        {
            classIds = [];
            _categoryClasses.Add(categoryId, classIds);
        }

        if (!classIds.Contains(classId))
        {
            classIds.Add(classId);
        }

        _details[classId] = new SyntheticOpcServerDetails(progId, userType, verIndProgId);
        return this;
    }

    public ValueTask<ICallChannel> CreateActivationChannelAsync(string host, CancellationToken cancellationToken = default)
    {
        _ = host;
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<ICallChannel>(_channel);
    }

    public ValueTask<ICallChannel> CreateObjectChannelAsync(string host, IOpcInterfaceRef interfaceRef, Guid interfaceId, CancellationToken cancellationToken = default)
    {
        _ = host;
        _ = interfaceRef;
        _ = interfaceId;
        cancellationToken.ThrowIfCancellationRequested();
        return ValueTask.FromResult<ICallChannel>(_channel);
    }

    private Task<NdrCallResult> HandleCallAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (interfaceId == RemoteScmActivatorInterfaceId && opnum == 4)
        {
            return Task.FromResult(new NdrCallResult(0, EncodeObjRef(OpcGuids.IID_IOPCServerList2)));
        }

        if (interfaceId == OpcGuids.IID_IOPCServerList2 && opnum == 3)
        {
            Guid categoryId = DecodeFirstImplementedCategory(requestPayload);
            _pendingEnums.Enqueue(_categoryClasses.TryGetValue(categoryId, out List<Guid>? classIds) ? classIds.ToArray() : Array.Empty<Guid>());
            return Task.FromResult(new NdrCallResult(0, EncodeObjRef(OpcGuids.IID_IOPCEnumGUID)));
        }

        if (interfaceId == OpcGuids.IID_IOPCEnumGUID && opnum == 3)
        {
            return Task.FromResult(HandleNext(requestPayload));
        }

        if (interfaceId == OpcGuids.IID_IOPCServerList2 && opnum == 4)
        {
            Guid classId = DecodeClassId(requestPayload);
            return Task.FromResult(new NdrCallResult(0, EncodeClassDetails(_details[classId])));
        }

        return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
    }

    private NdrCallResult HandleNext(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        int requested = reader.ReadInt32();
        _currentEnum ??= _pendingEnums.Count == 0 ? Array.Empty<Guid>() : _pendingEnums.Dequeue();
        int remaining = Math.Max(0, _currentEnum.Count - _currentEnumIndex);
        int fetched = Math.Min(requested, remaining);
        var batch = new Guid[fetched];
        for (int i = 0; i < batch.Length; i++)
        {
            batch[i] = _currentEnum[_currentEnumIndex++];
        }

        if (_currentEnumIndex >= _currentEnum.Count)
        {
            _currentEnum = null;
            _currentEnumIndex = 0;
        }

        return new NdrCallResult(
            fetched < requested ? OpcResultId.False.Code : OpcResultId.Ok.Code,
            EncodeNext(batch, fetched, requested));
    }

    private static Guid DecodeFirstImplementedCategory(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        // IDL: [in] ULONG cImplemented, [in, size_is(cImplemented)] CATID rgcatidImpl[],
        //      [in] ULONG cRequired,    [in, size_is(cRequired)] CATID rgcatidReq[]
        _ = reader.ReadUInt32();
        Guid[] implementedCategories = reader.ReadConformantGuidArray();
        _ = reader.ReadUInt32();
        _ = reader.ReadConformantGuidArray();
        return implementedCategories.Length == 0 ? Guid.Empty : implementedCategories[0];
    }

    private static Guid DecodeClassId(ReadOnlyMemory<byte> requestPayload)
    {
        var reader = new NdrReader(requestPayload.Span);
        return reader.ReadGuid();
    }

    private static byte[] EncodeObjRef(Guid iid) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUInt32(0x574F454Du);
        writer.WriteUInt32(0x00000001u);
        writer.WriteGuid(iid);
        writer.WriteUInt32(0);
        writer.WriteUInt32(5);
        writer.WriteUInt64(1);
        writer.WriteUInt64(2);
        writer.WriteGuid(Guid.NewGuid());
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
    });

    private static byte[] EncodeClassDetails(SyntheticOpcServerDetails details) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUnicodeStringPtr(details.ProgId);
        writer.WriteUnicodeStringPtr(details.UserType);
        writer.WriteUnicodeStringPtr(details.VerIndProgId);
    });

    private static byte[] EncodeNext(Guid[] classIds, int fetched, int requested) => WritePayload((ref NdrWriter writer) =>
    {
        // IEnumGUID::Next response: varying-conformant GUID array (max + offset + length + elements)
        // followed by pceltFetched ULONG.
        writer.WriteUInt32((uint)requested);
        writer.WriteUInt32(0);
        writer.WriteUInt32((uint)classIds.Length);
        for (int i = 0; i < classIds.Length; i++)
        {
            writer.WriteGuid(classIds[i]);
        }

        writer.WriteInt32(fetched);
    });

    private static byte[] WritePayload(NdrWriteAction action)
    {
        var buffer = new byte[4096];
        var writer = new NdrWriter(buffer);
        action(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed record SyntheticOpcServerDetails(string ProgId, string UserType, string? VerIndProgId);
}
