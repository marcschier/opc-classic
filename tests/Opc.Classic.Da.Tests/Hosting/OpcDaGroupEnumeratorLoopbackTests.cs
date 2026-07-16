// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class OpcDaGroupEnumeratorLoopbackTests
{
    private static readonly Guid IidIUnknown =
        new("00000000-0000-0000-C000-000000000046");

    [Test]
    [Arguments(1, true, 2)]
    [Arguments(2, true, 0)]
    [Arguments(3, true, 2)]
    [Arguments(4, false, 2)]
    [Arguments(5, false, 0)]
    [Arguments(6, false, 2)]
    public async Task All_scopes_have_windows_compatible_private_public_all_semantics(
        int scope,
        bool connections,
        int expectedCount)
    {
        var registry = new OpcObjectRegistry();
        var server = new SnapshotServer([Group("P1", 1), Group("P2", 2)]);
        var root = new OpcDaServerDispatcher(server, objectRegistry: registry);
        Guid iid = connections ? IEnumUnknown.InterfaceId : IEnumString.InterfaceId;

        (DispatchResult create, IOpcInterfaceRef enumerator) =
            await CreateEnumeratorAsync(root.ServerDispatcher, scope, iid);
        IOpcServerDispatcher enumDispatcher = GetDispatcher(registry, enumerator);
        try
        {
            if (connections)
            {
                (DispatchResult next, IOpcInterfaceRef[] values) =
                    await NextUnknownAsync(enumDispatcher, 8);
                await Assert.That(next.Hresult).IsEqualTo(OpcResultId.False.Code);
                await Assert.That(values.Length).IsEqualTo(expectedCount);
                foreach (IOpcInterfaceRef value in values)
                {
                    await Assert.That(value.Iid).IsEqualTo(IidIUnknown);
                    await Assert.That(registry.TryGetDispatcher(
                        value.Ipid,
                        IOPCGroupStateMgt.InterfaceId,
                        out _)).IsTrue();
                    await ReleaseAsync(registry, value);
                }
            }
            else
            {
                (DispatchResult next, string[] values) =
                    await NextStringsAsync(enumDispatcher, 8);
                await Assert.That(next.Hresult).IsEqualTo(OpcResultId.False.Code);
                await Assert.That(values.Length).IsEqualTo(expectedCount);
                if (expectedCount != 0)
                {
                    await Assert.That(values).IsEquivalentTo(["P1", "P2"]);
                }
            }

            await Assert.That(create.Hresult).IsEqualTo(
                expectedCount == 0 ? OpcResultId.False.Code : OpcResultId.Ok.Code);
        }
        finally
        {
            await ReleaseAsync(registry, enumerator);
        }
    }

    [Test]
    public async Task Name_enumerator_is_point_in_time_and_supports_partial_skip_reset_clone()
    {
        OpcDaGroup first = Group("P1", 1);
        OpcDaGroup second = Group("P2", 2);
        var registry = new OpcObjectRegistry();
        var server = new SnapshotServer([first, second]);
        var root = new OpcDaServerDispatcher(server, objectRegistry: registry);
        (_, IOpcInterfaceRef enumerator) =
            await CreateEnumeratorAsync(root.ServerDispatcher, 4, IEnumString.InterfaceId);
        IOpcServerDispatcher dispatcher = GetDispatcher(registry, enumerator);

        (DispatchResult firstNext, string[] firstValues) =
            await NextStringsAsync(dispatcher, 1);
        IOpcInterfaceRef clone = await CloneAsync(dispatcher);
        IOpcServerDispatcher cloneDispatcher = GetDispatcher(registry, clone);

        server.ReplacePrivate([Group("P3", 3)]);
        await first.SetNameAsync("Changed");
        await second.SetNameAsync("Changed2");

        DispatchResult skipped = await dispatcher.DispatchAsync(4, EncodeCount(10));
        _ = await dispatcher.DispatchAsync(5, ReadOnlyMemory<byte>.Empty);
        (DispatchResult resetNext, string[] resetValues) =
            await NextStringsAsync(dispatcher, 3);
        (DispatchResult cloneNext, string[] cloneValues) =
            await NextStringsAsync(cloneDispatcher, 5);

        await Assert.That(firstNext.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(firstValues).IsEquivalentTo(["P1"]);
        await Assert.That(skipped.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(resetNext.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(resetValues).IsEquivalentTo(["P1", "P2"]);
        await Assert.That(cloneNext.Hresult).IsEqualTo(OpcResultId.False.Code);
        await Assert.That(cloneValues).IsEquivalentTo(["P2"]);

        await ReleaseAsync(registry, enumerator);
        await ReleaseAsync(registry, clone);
    }

    [Test]
    public async Task Unknown_clone_and_returned_group_refs_follow_IRemUnknown_release_lifetime()
    {
        var registry = new OpcObjectRegistry();
        var root = new OpcDaServerDispatcher(
            new SnapshotServer([Group("P1", 1)]),
            objectRegistry: registry);
        (_, IOpcInterfaceRef enumerator) =
            await CreateEnumeratorAsync(root.ServerDispatcher, 1, IEnumUnknown.InterfaceId);
        IOpcServerDispatcher dispatcher = GetDispatcher(registry, enumerator);
        IOpcInterfaceRef clone = await CloneAsync(dispatcher);
        (DispatchResult next, IOpcInterfaceRef[] groups) =
            await NextUnknownAsync(dispatcher, 1);
        IOpcInterfaceRef group = groups.Single();

        await Assert.That(next.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await ReleaseAsync(registry, enumerator);
        await Assert.That(registry.Contains(enumerator.Ipid)).IsFalse();
        await Assert.That(registry.Contains(clone.Ipid)).IsTrue();
        await Assert.That(registry.Contains(group.Ipid)).IsTrue();

        await ReleaseAsync(registry, clone);
        await Assert.That(registry.Contains(clone.Ipid)).IsFalse();
        await Assert.That(registry.Contains(group.Ipid)).IsTrue();

        await ReleaseAsync(registry, group);
        await Assert.That(registry.Contains(group.Ipid)).IsFalse();
    }

    [Test]
    public async Task Unknown_enumerator_reuses_existing_group_identity_and_reference_lifetime()
    {
        var registry = new OpcObjectRegistry();
        var server = new StableIdentityServer(Group("P1", 1), registry);
        var root = new OpcDaServerDispatcher(
            server,
            objectRegistry: registry);
        IOpcInterfaceRef direct = await ((IOPCServer)server)
            .GetGroupByNameAsync(
                "P1",
                IidIUnknown,
                TestContext.Current!.CancellationToken);
        (_, IOpcInterfaceRef enumerator) =
            await CreateEnumeratorAsync(
                root.ServerDispatcher,
                1,
                IEnumUnknown.InterfaceId);
        IOpcServerDispatcher dispatcher = GetDispatcher(registry, enumerator);
        (_, IOpcInterfaceRef[] groups) = await NextUnknownAsync(dispatcher, 1);
        IOpcInterfaceRef enumerated = groups.Single();

        await Assert.That(enumerated.Ipid).IsEqualTo(direct.Ipid);
        await Assert.That(enumerated.Oxid).IsEqualTo(direct.Oxid);
        await Assert.That(enumerated.Oid).IsEqualTo(direct.Oid);

        await ReleaseAsync(registry, direct);
        await ReleaseAsync(registry, enumerated);
        await ReleaseAsync(registry, enumerator);
        await Assert.That(registry.Contains(server.InitialReference.Ipid)).IsTrue();

        await ReleaseAsync(registry, server.InitialReference);
        await Assert.That(registry.Contains(server.InitialReference.Ipid)).IsFalse();
    }

    [Test]
    public async Task Separate_enumerators_keep_stable_unique_identity_through_query_and_release()
    {
        var registry = new OpcObjectRegistry();
        var root = new OpcDaServerDispatcher(
            new SnapshotServer([Group("P1", 1)]),
            objectRegistry: registry);
        (_, IOpcInterfaceRef first) =
            await CreateEnumeratorAsync(root.ServerDispatcher, 4, IEnumString.InterfaceId);
        (_, IOpcInterfaceRef second) =
            await CreateEnumeratorAsync(root.ServerDispatcher, 4, IEnumString.InterfaceId);

        OpcRemQIResult firstQi = await QueryInterfaceAsync(registry, first);
        OpcRemQIResult secondQi = await QueryInterfaceAsync(registry, second);

        await Assert.That(firstQi.Ipid).IsEqualTo(first.Ipid);
        await Assert.That(firstQi.Oxid).IsEqualTo(first.Oxid);
        await Assert.That(firstQi.Oid).IsEqualTo(first.Oid);
        await Assert.That(secondQi.Ipid).IsEqualTo(second.Ipid);
        await Assert.That(secondQi.Oxid).IsEqualTo(second.Oxid);
        await Assert.That(secondQi.Oid).IsEqualTo(second.Oid);
        await Assert.That(first.Oxid).IsEqualTo(second.Oxid);
        await Assert.That(first.Oid).IsNotEqualTo(second.Oid);

        await ReleaseAsync(registry, first);
        await Assert.That(registry.TryGetObjectMetadata(first.Ipid, out _)).IsFalse();
        await Assert.That(registry.TryGetObjectMetadata(second.Ipid, out _)).IsTrue();

        await ReleaseAsync(registry, second);
        await Assert.That(registry.TryGetObjectMetadata(second.Ipid, out _)).IsFalse();
    }

    [Test]
    public async Task Generated_proxies_decode_varying_string_and_interface_pointer_arrays()
    {
        var registry = new OpcObjectRegistry();
        var root = new OpcDaServerDispatcher(
            new SnapshotServer([Group("P1", 1), Group("P2", 2)]),
            objectRegistry: registry);
        (_, IOpcInterfaceRef stringsRef) =
            await CreateEnumeratorAsync(root.ServerDispatcher, 4, IEnumString.InterfaceId);
        (_, IOpcInterfaceRef unknownsRef) =
            await CreateEnumeratorAsync(root.ServerDispatcher, 1, IEnumUnknown.InterfaceId);

        var strings = new IEnumStringClientProxy(
            new DispatcherChannel(GetDispatcher(registry, stringsRef)));
        var unknowns = new IEnumUnknownClientProxy(
            new DispatcherChannel(GetDispatcher(registry, unknownsRef)));

        await strings.NextStringsAsync(
            3,
            out string[] names,
            out int namesFetched,
            TestContext.Current!.CancellationToken);
        await unknowns.NextUnknownsAsync(
            3,
            out IOpcInterfaceRef[] groups,
            out int groupsFetched,
            TestContext.Current!.CancellationToken);

        await Assert.That(names).IsEquivalentTo(["P1", "P2"]);
        await Assert.That(namesFetched).IsEqualTo(2);
        await Assert.That(groups.Length).IsEqualTo(2);
        await Assert.That(groupsFetched).IsEqualTo(2);

        foreach (IOpcInterfaceRef group in groups)
        {
            await ReleaseAsync(registry, group);
        }
        await ReleaseAsync(registry, stringsRef);
        await ReleaseAsync(registry, unknownsRef);
    }

    [Test]
    public async Task Concurrent_group_mutation_never_changes_an_issued_all_names_snapshot()
    {
        var registry = new OpcObjectRegistry();
        var server = new AtomicPairServer();
        var root = new OpcDaServerDispatcher(server, objectRegistry: registry);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current!.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        using var start = new Barrier(2);

        Task writer = Task.Run(() =>
        {
            start.SignalAndWait(cts.Token);
            for (int generation = 1; generation <= 500; generation++)
            {
                server.ReplacePair(generation);
            }
        }, cts.Token);

        Task reader = Task.Run(async () =>
        {
            start.SignalAndWait(cts.Token);
            for (int i = 0; i < 200; i++)
            {
                (_, IOpcInterfaceRef enumerator) = await CreateEnumeratorAsync(
                    root.ServerDispatcher,
                    6,
                    IEnumString.InterfaceId);
                IOpcServerDispatcher dispatcher = GetDispatcher(registry, enumerator);
                (_, string[] names) = await NextStringsAsync(dispatcher, 2);
                string privateGeneration = names[0]["private-a-".Length..];
                string secondGeneration = names[1]["private-b-".Length..];
                if (!string.Equals(
                    privateGeneration,
                    secondGeneration,
                    StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("The all-groups snapshot mixed generations.");
                }
                await ReleaseAsync(registry, enumerator);
            }
        }, cts.Token);

        await Task.WhenAll(writer, reader);
    }

    private static async Task<(DispatchResult Result, IOpcInterfaceRef Enumerator)>
        CreateEnumeratorAsync(
            IOpcServerDispatcher dispatcher,
            int scope,
            Guid iid)
    {
        DispatchResult result = await dispatcher.DispatchAsync(
            IOPCServer.Opnums.CreateGroupEnumeratorAsync,
            WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteInt32(scope);
                writer.WriteGuid(iid);
            }));
        var reader = new NdrReader(result.Payload.Span);
        return (result, OpcMInterfacePointerCodec.Read(ref reader)
            ?? throw new InvalidOperationException("CreateGroupEnumerator returned a null interface."));
    }

    private static async Task<(DispatchResult Result, string[] Values)>
        NextStringsAsync(IOpcServerDispatcher dispatcher, int count)
    {
        DispatchResult result = await dispatcher.DispatchAsync(3, EncodeCount(count));
        var reader = new NdrReader(result.Payload.Span);
        int maximum = reader.ReadConformanceHeader();
        int offset = reader.ReadInt32();
        int actual = reader.ReadInt32();
        if (maximum != count || offset != 0 || actual < 0 || actual > maximum)
        {
            throw new InvalidOperationException("Invalid IEnumString varying-array header.");
        }
        for (int i = 0; i < actual; i++)
        {
            if (!reader.TryReadReferentId(out _))
            {
                throw new InvalidOperationException("IEnumString returned a null string.");
            }
        }
        var values = new string[actual];
        for (int i = 0; i < actual; i++)
        {
            values[i] = reader.ReadUnicodeString();
        }
        int fetched = reader.ReadInt32();
        if (fetched != actual)
        {
            throw new InvalidOperationException("IEnumString fetched count did not match actual_count.");
        }
        return (result, values);
    }

    private static async Task<(DispatchResult Result, IOpcInterfaceRef[] Values)>
        NextUnknownAsync(IOpcServerDispatcher dispatcher, int count)
    {
        DispatchResult result = await dispatcher.DispatchAsync(3, EncodeCount(count));
        var reader = new NdrReader(result.Payload.Span);
        int maximum = reader.ReadConformanceHeader();
        int offset = reader.ReadInt32();
        int actual = reader.ReadInt32();
        if (maximum != count || offset != 0 || actual < 0 || actual > maximum)
        {
            throw new InvalidOperationException("Invalid IEnumUnknown varying-array header.");
        }
        for (int i = 0; i < actual; i++)
        {
            if (!reader.TryReadReferentId(out _))
            {
                throw new InvalidOperationException("IEnumUnknown returned a null interface.");
            }
        }
        var values = new IOpcInterfaceRef[actual];
        for (int i = 0; i < actual; i++)
        {
            _ = reader.ReadUInt32();
            int byteCount = reader.ReadInt32();
            var inner = new NdrReader(reader.ReadRawBytes(byteCount));
            values[i] = OpcInterfaceRefCodec.Read(ref inner);
        }
        int fetched = reader.ReadInt32();
        if (fetched != actual)
        {
            throw new InvalidOperationException("IEnumUnknown fetched count did not match actual_count.");
        }
        return (result, values);
    }

    private static async Task<IOpcInterfaceRef> CloneAsync(
        IOpcServerDispatcher dispatcher)
    {
        DispatchResult result = await dispatcher.DispatchAsync(
            6,
            ReadOnlyMemory<byte>.Empty);
        var reader = new NdrReader(result.Payload.Span);
        return OpcMInterfacePointerCodec.Read(ref reader)
            ?? throw new InvalidOperationException("Clone returned a null interface.");
    }

    private static async Task<OpcRemQIResult> QueryInterfaceAsync(
        OpcObjectRegistry registry,
        IOpcInterfaceRef interfaceRef)
    {
        var dispatcher = new RemUnknownServerDispatcher(registry);
        DispatchResult result = await dispatcher.DispatchAsync(
            3,
            WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteGuid(interfaceRef.Ipid);
                writer.WriteUInt32(0);
                writer.WriteUInt16(1);
                writer.WriteConformanceHeader(1);
                writer.WriteGuid(interfaceRef.Iid);
            }));
        if (result.Hresult != OpcResultId.Ok.Code)
        {
            throw new InvalidOperationException("IRemUnknown::RemQueryInterface failed.");
        }

        var reader = new NdrReader(result.Payload.Span);
        if (!reader.TryReadReferentId(out _) || reader.ReadConformanceHeader() != 1)
        {
            throw new InvalidOperationException("IRemUnknown::RemQueryInterface returned an invalid result array.");
        }
        return NdrRemQIResultCodec.Read(ref reader);
    }

    private static IOpcServerDispatcher GetDispatcher(
        OpcObjectRegistry registry,
        IOpcInterfaceRef interfaceRef)
    {
        if (!registry.TryGetDispatcher(
            interfaceRef.Ipid,
            interfaceRef.Iid,
            out IOpcServerDispatcher dispatcher))
        {
            throw new InvalidOperationException("The returned interface was not registered.");
        }
        return dispatcher;
    }

    private static async Task ReleaseAsync(
        OpcObjectRegistry registry,
        IOpcInterfaceRef interfaceRef)
    {
        var dispatcher = new RemUnknownServerDispatcher(registry);
        DispatchResult result = await dispatcher.DispatchAsync(
            5,
            WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteUInt16(1);
                writer.WriteConformanceHeader(1);
                writer.WriteGuid(interfaceRef.Ipid);
                writer.WriteUInt32(interfaceRef.PublicRefs);
                writer.WriteUInt32(0);
            }));
        if (result.Hresult != OpcResultId.Ok.Code)
        {
            throw new InvalidOperationException("IRemUnknown::RemRelease failed.");
        }
    }

    private static byte[] EncodeCount(int count) =>
        WritePayload((ref NdrWriter writer) => writer.WriteUInt32((uint)count));

    private static byte[] WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[4096];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static OpcDaGroup Group(string name, int handle) =>
        new(name, handle, handle, true, 1000, 0, 0, 1033);

    private sealed class SnapshotServer : ServerBase
    {
        private readonly Lock _gate = new();
        private OpcDaGroup[] _privateGroups;

        public SnapshotServer(OpcDaGroup[] privateGroups) =>
            _privateGroups = privateGroups;

        public void ReplacePrivate(OpcDaGroup[] groups)
        {
            lock (_gate)
            {
                _privateGroups = groups;
            }
        }

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                return Task.FromResult<IReadOnlyList<OpcDaGroup>>(
                    [.. _privateGroups]);
            }
        }
    }

    private sealed class StableIdentityServer : ServerBase, IOPCServer
    {
        private readonly OpcDaGroup _group;
        private readonly OpcObjectRegistry _registry;

        public StableIdentityServer(
            OpcDaGroup group,
            OpcObjectRegistry registry)
        {
            _group = group;
            _registry = registry;
            Guid ipid = registry.Register(
                new Dictionary<Guid, IOpcServerDispatcher>
                {
                    [IOPCGroupStateMgt.InterfaceId] =
                        new IOPCGroupStateMgtServerDispatcher(group),
                },
                publicRefs: 1);
            InitialReference = CreateReference(IidIUnknown, ipid);
        }

        public IOpcInterfaceRef InitialReference { get; }

        Task<IOpcInterfaceRef> IOPCServer.GetGroupByNameAsync(
            string name,
            Guid requestedInterfaceId,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!string.Equals(name, _group.Name, StringComparison.Ordinal))
            {
                throw new OpcException(OpcResultId.UnknownPath);
            }
            if (!_registry.AddPublicRefs(InitialReference.Ipid, 1))
            {
                throw new OpcException(new OpcResultId(
                    unchecked((int)0x80010108),
                    "RPC_E_DISCONNECTED"));
            }

            return Task.FromResult<IOpcInterfaceRef>(
                CreateReference(requestedInterfaceId, InitialReference.Ipid));
        }

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult<IReadOnlyList<OpcDaGroup>>([_group]);
        }

        private IOpcInterfaceRef CreateReference(Guid iid, Guid ipid)
        {
            if (!_registry.TryGetObjectMetadata(
                    ipid,
                    out OpcObjectMetadata metadata))
            {
                throw new InvalidOperationException(
                    "The stable test group is not registered.");
            }

            return new OpcInterfaceRef(
                iid,
                flags: 0,
                publicRefs: 1,
                metadata.Oxid,
                metadata.Oid,
                ipid,
                securityOffset: 0,
                resolverBindings: []);
        }
    }

    private sealed class AtomicPairServer : ServerBase
    {
        private readonly Lock _gate = new();
        private OpcDaGroup[] _privateGroups =
            [Group("private-a-0", 1), Group("private-b-0", 2)];

        public void ReplacePair(int generation)
        {
            lock (_gate)
            {
                _privateGroups =
                    [Group($"private-a-{generation}", 1), Group($"private-b-{generation}", 2)];
            }
        }

        public override Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                return Task.FromResult<IReadOnlyList<OpcDaGroup>>(
                    [.. _privateGroups]);
            }
        }
    }

    private abstract class ServerBase : IOpcDaServer
    {
        public Task<OpcServerStatus> GetStatusAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus());

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
            Task.FromResult(string.Empty);

        public abstract Task<IReadOnlyList<OpcDaGroup>> SnapshotPrivateGroupsAsync(
            CancellationToken cancellationToken = default);
    }

    private sealed class DispatcherChannel : ICallChannel
    {
        private readonly IOpcServerDispatcher _dispatcher;

        public DispatcherChannel(IOpcServerDispatcher dispatcher) =>
            _dispatcher = dispatcher;

        public async Task<NdrCallResult> InvokeAsync(
            Guid interfaceId,
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken = default)
        {
            _ = interfaceId;
            DispatchResult result = await _dispatcher
                .DispatchAsync(opnum, requestPayload, cancellationToken)
                .ConfigureAwait(false);
            return result.ToNdrCallResult();
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);
}
