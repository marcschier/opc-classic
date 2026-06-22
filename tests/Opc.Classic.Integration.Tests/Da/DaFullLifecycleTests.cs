// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Integration.Tests.Da;

public sealed class DaFullLifecycleTests
{
    private const int CacheDataSource = 1;
    private const int DeviceDataSource = 2;
    private const int ClientGroupHandle = 0xDA300;
    private const int LocaleId = 1033;

    [Test]
    [Category("Da.FullLifecycle")]
    public async Task DaFullLifecycle_managed_client_exercises_group_lifecycle_over_loopback_tcp()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        (InMemoryAddressSpace addressSpace, IReadOnlyDictionary<string, OpcVariant> values) = CreateAddressSpace();
        var callbackEndpoint = new CallbackEndpointAccessor();
        await using ServiceProvider provider = BuildServiceProvider(addressSpace, values, callbackEndpoint);
        OpcDaServerHost host = provider.GetRequiredService<OpcDaServerHost>();

        await host.StartAsync(cancellationToken);
        try
        {
            await using DcomCallChannel serverChannel = await ConnectRootAsync(host, cancellationToken);
            var server = new IOPCServerClientProxy(serverChannel);

            OpcServerStatus status = await server.GetStatusAsync(cancellationToken);
            await Assert.That(status.State).IsEqualTo(OpcServerState.Running);

            await server.AddGroupAsync(
                name: "lifecycle-group",
                active: true,
                requestedUpdateRate: 250,
                clientGroupHandle: ClientGroupHandle,
                timeBias: 0,
                percentDeadband: 0.0f,
                localeId: LocaleId,
                requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
                serverGroupHandle: out int serverGroupHandle,
                revisedUpdateRate: out int revisedUpdateRate,
                group: out IOpcInterfaceRef groupRef,
                cancellationToken);

            await Assert.That(serverGroupHandle).IsGreaterThan(0);
            await Assert.That(revisedUpdateRate).IsEqualTo(250);
            await Assert.That(groupRef.Iid).IsEqualTo(IOPCGroupStateMgt.InterfaceId);
            await Assert.That(groupRef.Ipid).IsNotEqualTo(Guid.Empty);

            await using DcomCallChannel groupStateChannel = await ConnectObjectAsync(host, groupRef.Ipid, cancellationToken);
            await using DcomCallChannel itemMgtChannel = await ConnectObjectAsync(host, groupRef.Ipid, cancellationToken);
            await using DcomCallChannel syncIoChannel = await ConnectObjectAsync(host, groupRef.Ipid, cancellationToken);
            await using DcomCallChannel containerChannel = await ConnectObjectAsync(host, groupRef.Ipid, cancellationToken);
            await using DcomCallChannel connectionPointChannel = await ConnectObjectAsync(host, groupRef.Ipid, cancellationToken);
            await using DcomCallChannel asyncIoChannel = await ConnectObjectAsync(host, groupRef.Ipid, cancellationToken);

            var groupState = new IOPCGroupStateMgtClientProxy(groupStateChannel);
            var itemMgt = new IOPCItemMgtClientProxy(itemMgtChannel);
            var syncIo = new IOPCSyncIOClientProxy(syncIoChannel);
            var connectionPoint = new IConnectionPointClientProxy(connectionPointChannel);
            var asyncIo = new IOPCAsyncIO2ClientProxy(asyncIoChannel);

            OpcGroupState state = await groupState.GetStateAsync(cancellationToken);
            await Assert.That(state.ServerHandle).IsEqualTo(serverGroupHandle);
            await Assert.That(state.ClientHandle).IsEqualTo(ClientGroupHandle);
            await Assert.That(state.Name).IsEqualTo("lifecycle-group");
            await Assert.That(state.Active).IsTrue();
            await Assert.That(state.UpdateRate).IsEqualTo(250);
            await Assert.That(state.TimeBias).IsEqualTo(0);
            await Assert.That(state.PercentDeadband).IsEqualTo(0.0f);
            await Assert.That(state.LocaleId).IsEqualTo(LocaleId);

            OpcItemDef[] itemDefinitions =
            [
                new(null, "Random.Int", Active: true, ClientHandle: 101, Blob: [], VarType.VT_EMPTY),
                new(null, "Random.Real", Active: true, ClientHandle: 102, Blob: [], VarType.VT_EMPTY),
                new(null, "Bucket.Int", Active: true, ClientHandle: 103, Blob: [], VarType.VT_EMPTY),
            ];
            await itemMgt.AddItemsAsync(itemDefinitions, out OpcItemResult[] addResults, out int[] addErrors, cancellationToken);

            await Assert.That(addResults.Length).IsEqualTo(3);
            await Assert.That(addErrors.All(IsSuccess)).IsTrue();
            await Assert.That(addResults.All(static result => result.ServerHandle > 0)).IsTrue();
            await Assert.That(addResults.Select(static result => result.CanonicalDataType).ToArray())
                .IsEquivalentTo([VarType.VT_I4, VarType.VT_R8, VarType.VT_I4]);

            int[] serverHandles = addResults.Select(static result => result.ServerHandle).ToArray();
            OpcItemState[] deviceStates = await syncIo.ReadAsync(DeviceDataSource, serverHandles, out int[] deviceReadErrors, cancellationToken);
            await Assert.That(deviceStates.Length).IsEqualTo(3);
            await Assert.That(deviceReadErrors.All(IsSuccess)).IsTrue();
            await Assert.That(deviceStates.All(static item => !item.Value.IsEmpty)).IsTrue();
            await Assert.That(deviceStates.All(static item => item.Quality == OpcQuality.Good)).IsTrue();

            int bucketHandle = addResults[2].ServerHandle;
            int[] writeErrors = await syncIo.WriteAsync([bucketHandle], [OpcVariant.FromInt32(5)], cancellationToken);
            await Assert.That(writeErrors.All(IsSuccess)).IsTrue();

            OpcItemState[] cacheStates = await syncIo.ReadAsync(CacheDataSource, [bucketHandle], out int[] cacheReadErrors, cancellationToken);
            await Assert.That(cacheReadErrors.All(IsSuccess)).IsTrue();
            await Assert.That(cacheStates[0].Value.AsInt32()).IsEqualTo(5);

            var callback = new RecordingDataCallback();
            await using OpcServerListener callbackListener = StartCallbackListener(callback, cancellationToken);
            callbackEndpoint.Endpoint = (IPEndPoint)callbackListener.LocalEndpoint;

            IOpcInterfaceRef connectionPointRef = await FindConnectionPointAsync(containerChannel, cancellationToken);
            await Assert.That(connectionPointRef.Iid).IsEqualTo(IConnectionPoint.InterfaceId);
            int cookie = await connectionPoint.AdviseAsync(CreateCallbackRef(), cancellationToken);
            await Assert.That(cookie).IsGreaterThan(0);

            await asyncIo.SetEnableAsync(enabled: true, cancellationToken);
            int cancelId = await asyncIo.Refresh2Async(DeviceDataSource, transactionId: 0x3005, cancellationToken);
            await Assert.That(cancelId).IsGreaterThan(0);

            DataChangeSnapshot dataChange = await callback.WaitForDataChangeAsync(cancellationToken);
            await Assert.That(dataChange.TransactionId).IsEqualTo(0x3005);
            await Assert.That(dataChange.GroupHandle).IsEqualTo(ClientGroupHandle);
            await Assert.That(dataChange.ClientHandles.Order().ToArray()).IsEquivalentTo([101, 102, 103]);
            await Assert.That(dataChange.Values.Length).IsEqualTo(3);
            await Assert.That(dataChange.Qualities.All(static quality => quality == OpcQuality.Good.RawValue)).IsTrue();
            await Assert.That(dataChange.Errors.All(IsSuccess)).IsTrue();

            await connectionPoint.UnadviseAsync(cookie, cancellationToken);

            int[] removeItemErrors = await itemMgt.RemoveItemsAsync(serverHandles, cancellationToken);
            await Assert.That(removeItemErrors.All(IsSuccess)).IsTrue();

            await server.RemoveGroupAsync(serverGroupHandle, force: true, cancellationToken);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    private static ServiceProvider BuildServiceProvider(
        InMemoryAddressSpace addressSpace,
        IReadOnlyDictionary<string, OpcVariant> values,
        CallbackEndpointAccessor callbackEndpoint)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOpcAddressSpace>(addressSpace);
        services.AddSingleton(new LifecycleItemCatalog(values));
        services.AddSingleton(callbackEndpoint);
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<LifecycleDaServer>();
        services.AddSingleton<IOpcDaServer>(static sp => sp.GetRequiredService<LifecycleDaServer>());
        services.AddSingleton<OpcDaServerHost>();
        services.AddSingleton<IOpcServerHost>(static sp => sp.GetRequiredService<OpcDaServerHost>());
        services.Configure<OpcDaServerOptions>(static options =>
        {
            options.Clsid = Guid.NewGuid();
            options.ProgId = "Opc.Classic.Integration.DaFullLifecycle.1";
            options.FriendlyName = "DA full lifecycle integration test server";
            options.ListenAddress = "127.0.0.1:0";
        });
        return services.BuildServiceProvider();
    }

    private static (InMemoryAddressSpace AddressSpace, IReadOnlyDictionary<string, OpcVariant> Values) CreateAddressSpace()
    {
        var addressSpace = new InMemoryAddressSpace("Random", "Bucket");
        addressSpace.AddItem("Random", "Int");
        addressSpace.AddItem("Random", "Real");
        addressSpace.AddItem("Bucket", "Int");

        IReadOnlyDictionary<string, OpcVariant> values = new Dictionary<string, OpcVariant>(StringComparer.Ordinal)
        {
            ["Random.Int"] = OpcVariant.FromInt32(17),
            ["Random.Real"] = OpcVariant.FromDouble(3.25d),
            ["Bucket.Int"] = OpcVariant.FromInt32(0),
        };
        return (addressSpace, values);
    }

    private static async Task<DcomCallChannel> ConnectRootAsync(OpcDaServerHost host, CancellationToken cancellationToken)
    {
        var endpoint = (IPEndPoint?)host.LocalEndpoint
            ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");
        return await DcomCallChannelFactory.ConnectTcpAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            NoOpAuthContext.Instance,
            cancellationToken);
    }

    private static async Task<DcomCallChannel> ConnectObjectAsync(
        OpcDaServerHost host,
        Guid objectIpid,
        CancellationToken cancellationToken)
    {
        var endpoint = (IPEndPoint?)host.LocalEndpoint
            ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            cancellationToken);
        return new DcomCallChannel(transport, NoOpAuthContext.Instance, objectIpid);
    }

    private static async Task<IOpcInterfaceRef> FindConnectionPointAsync(ICallChannel channel, CancellationToken cancellationToken)
    {
        var buffer = new byte[16];
        var writer = new NdrWriter(buffer);
        writer.WriteGuid(IOPCDataCallback.InterfaceId);
        NdrCallResult result = await channel.InvokeAsync(
            IConnectionPointContainer.InterfaceId,
            IConnectionPointContainer.Opnums.FindConnectionPointAsync,
            buffer.AsMemory(0, writer.Position).ToArray(),
            cancellationToken);
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }

        var reader = new NdrReader(result.ResponsePayload.Span);
        return OpcInterfaceRefCodec.Read(ref reader);
    }

    private static OpcServerListener StartCallbackListener(IOPCDataCallback callback, CancellationToken cancellationToken)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var dispatcher = new IOPCDataCallbackServerDispatcher(callback);
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [IOPCDataCallback.InterfaceId] = dispatcher });
        var listener = new OpcServerListener(endpoint, processor);
        listener.StartAsync(cancellationToken).GetAwaiter().GetResult();
        return listener;
    }

    private static IOpcInterfaceRef CreateCallbackRef() =>
        new OpcInterfaceRef(
            iid: IOPCDataCallback.InterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: 0,
            ipid: Guid.NewGuid(),
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());

    private static bool IsSuccess(int hresult) => new OpcResultId(hresult, null).IsSuccess;

    private sealed class CallbackEndpointAccessor
    {
        public IPEndPoint? Endpoint { get; set; }
    }

    private sealed class LifecycleItemCatalog
    {
        public LifecycleItemCatalog(IReadOnlyDictionary<string, OpcVariant> values) => Values = values;

        public IReadOnlyDictionary<string, OpcVariant> Values { get; }
    }

    private sealed class LifecycleDaServer : IOpcDaServer
    {
        private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
        private readonly object _gate = new();
        private readonly OpcObjectRegistry _registry;
        private readonly LifecycleItemCatalog _catalog;
        private readonly CallbackEndpointAccessor _callbackEndpoint;
        private readonly Dictionary<int, GroupEntry> _groups = new();
        private int _nextGroupHandle = 10_000;

        public LifecycleDaServer(
            OpcObjectRegistry registry,
            LifecycleItemCatalog catalog,
            CallbackEndpointAccessor callbackEndpoint)
        {
            _registry = registry;
            _catalog = catalog;
            _callbackEndpoint = callbackEndpoint;
        }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int groupCount;
            lock (_gate)
            {
                groupCount = _groups.Count;
            }

            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = StartTime,
                CurrentTime = now,
                LastUpdateTime = now,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "DA full lifecycle integration test server",
                GroupCount = groupCount,
                BandWidth = 0,
            });
        }

        public async Task<int> AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            CancellationToken cancellationToken = default)
        {
            GroupEntry entry = await AddGroupCoreAsync(
                name,
                active,
                requestedUpdateRate,
                clientHandle,
                timeBias: 0,
                percentDeadband: 0.0f,
                localeId,
                requestedInterfaceId: IOPCGroupStateMgt.InterfaceId,
                cancellationToken);
            return entry.ServerHandle;
        }

        Task IOPCServer.AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientGroupHandle,
            int timeBias,
            float percentDeadband,
            int localeId,
            Guid requestedInterfaceId,
            out int serverGroupHandle,
            out int revisedUpdateRate,
            out IOpcInterfaceRef group,
            CancellationToken cancellationToken)
        {
            GroupEntry entry = AddGroupCoreAsync(
                name,
                active,
                requestedUpdateRate,
                clientGroupHandle,
                timeBias,
                percentDeadband,
                localeId,
                requestedInterfaceId,
                cancellationToken).GetAwaiter().GetResult();

            serverGroupHandle = entry.ServerHandle;
            revisedUpdateRate = entry.Group.UpdateRate;
            group = CreateGroupRef(requestedInterfaceId, entry);
            return Task.CompletedTask;
        }

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
        {
            _ = force;
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                if (_groups.Remove(serverGroupHandle, out GroupEntry? entry))
                {
                    _registry.Unregister(entry.Ipid);
                }
            }

            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
        {
            _ = localeId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"0x{errorCode:X8}");
        }

        Task<IOpcInterfaceRef> IOPCServer.GetGroupByNameAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                foreach (GroupEntry entry in _groups.Values)
                {
                    if (string.Equals(entry.Group.Name, name, StringComparison.Ordinal))
                    {
                        return Task.FromResult(CreateGroupRef(requestedInterfaceId, entry));
                    }
                }
            }

            throw new OpcException(OpcResultId.UnknownPath);
        }

        Task<IOpcInterfaceRef> IOPCServer.CreateGroupEnumeratorAsync(int scope, Guid requestedInterfaceId, CancellationToken cancellationToken)
        {
            _ = scope;
            _ = requestedInterfaceId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromException<IOpcInterfaceRef>(new OpcException(OpcResultId.NotImplemented));
        }

        private Task<GroupEntry> AddGroupCoreAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientGroupHandle,
            int timeBias,
            float percentDeadband,
            int localeId,
            Guid requestedInterfaceId,
            CancellationToken cancellationToken)
        {
            _ = requestedInterfaceId;
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            cancellationToken.ThrowIfCancellationRequested();

            var group = new LifecycleGroup(
                name,
                Interlocked.Increment(ref _nextGroupHandle),
                clientGroupHandle,
                active,
                requestedUpdateRate,
                timeBias,
                percentDeadband,
                localeId,
                _catalog,
                _callbackEndpoint);

            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCGroupStateMgt.InterfaceId] = new IOPCGroupStateMgtServerDispatcher(group),
                [IOPCItemMgt.InterfaceId] = new IOPCItemMgtServerDispatcher(group),
                [IOPCSyncIO.InterfaceId] = new IOPCSyncIOServerDispatcher(group),
                [IOPCAsyncIO2.InterfaceId] = new IOPCAsyncIO2ServerDispatcher(group),
                [IConnectionPointContainer.InterfaceId] = new IConnectionPointContainerServerDispatcher(group),
                [IConnectionPoint.InterfaceId] = new IConnectionPointServerDispatcher(group),
            };
            Guid ipid = _registry.Register(dispatchers);
            group.SetIpid(ipid);
            var entry = new GroupEntry(group.ServerHandle, group, ipid);

            lock (_gate)
            {
                _groups[group.ServerHandle] = entry;
            }

            return Task.FromResult(entry);
        }

        private static IOpcInterfaceRef CreateGroupRef(Guid requestedInterfaceId, GroupEntry entry) =>
            new OpcInterfaceRef(
                iid: requestedInterfaceId,
                flags: 0,
                publicRefs: 1,
                oxid: 1,
                oid: unchecked((ulong)entry.ServerHandle),
                ipid: entry.Ipid,
                securityOffset: 0,
                resolverBindings: Array.Empty<ushort>());

        private sealed record GroupEntry(int ServerHandle, LifecycleGroup Group, Guid Ipid);
    }

    private sealed class LifecycleGroup : IOPCGroupStateMgt, IOPCItemMgt, IOPCSyncIO, IOPCAsyncIO2,
        IConnectionPointContainer, IConnectionPoint
    {
        private readonly object _gate = new();
        private readonly LifecycleItemCatalog _catalog;
        private readonly CallbackEndpointAccessor _callbackEndpoint;
        private readonly Dictionary<int, TrackedItem> _items = new();
        private readonly Dictionary<int, IOpcInterfaceRef> _sinks = new();
        private int _nextItemHandle = 20_000;
        private int _nextCancelId = 30_000;
        private int _nextCookie = 40_000;
        private bool _callbacksEnabled = true;
        private Guid _ipid;

        public LifecycleGroup(
            string name,
            int serverHandle,
            int clientHandle,
            bool active,
            int updateRate,
            int timeBias,
            float percentDeadband,
            int localeId,
            LifecycleItemCatalog catalog,
            CallbackEndpointAccessor callbackEndpoint)
        {
            Name = name;
            ServerHandle = serverHandle;
            ClientHandle = clientHandle;
            Active = active;
            UpdateRate = updateRate;
            TimeBias = timeBias;
            PercentDeadband = percentDeadband;
            LocaleId = localeId;
            _catalog = catalog;
            _callbackEndpoint = callbackEndpoint;
        }

        public string Name { get; private set; }
        public int ServerHandle { get; }
        public int ClientHandle { get; private set; }
        public bool Active { get; private set; }
        public int UpdateRate { get; private set; }
        public int TimeBias { get; private set; }
        public float PercentDeadband { get; private set; }
        public int LocaleId { get; private set; }

        public void SetIpid(Guid ipid) => _ipid = ipid;

        public Task<OpcGroupState> GetStateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new OpcGroupState(
                ClientHandle,
                ServerHandle,
                Name,
                Active,
                UpdateRate,
                TimeBias,
                PercentDeadband,
                LocaleId));
        }

        public Task SetStateAsync(
            int requestedUpdateRate,
            bool active,
            int timeBias,
            float percentDeadband,
            int localeId,
            int clientGroupHandle,
            out int revisedUpdateRate,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UpdateRate = requestedUpdateRate;
            Active = active;
            TimeBias = timeBias;
            PercentDeadband = percentDeadband;
            LocaleId = localeId;
            ClientHandle = clientGroupHandle;
            revisedUpdateRate = UpdateRate;
            return Task.CompletedTask;
        }

        public Task SetNameAsync(string name, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            cancellationToken.ThrowIfCancellationRequested();
            Name = name;
            return Task.CompletedTask;
        }

        public Task<IOpcInterfaceRef> CloneGroupAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken = default)
        {
            _ = name;
            _ = requestedInterfaceId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromException<IOpcInterfaceRef>(new OpcException(OpcResultId.NotImplemented));
        }

        public Task AddItemsAsync(
            OpcItemDef[] itemDefinitions,
            out OpcItemResult[] addResults,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(itemDefinitions);
            cancellationToken.ThrowIfCancellationRequested();
            addResults = new OpcItemResult[itemDefinitions.Length];
            errors = new int[itemDefinitions.Length];

            lock (_gate)
            {
                for (int i = 0; i < itemDefinitions.Length; i++)
                {
                    OpcItemDef definition = itemDefinitions[i];
                    if (definition.ItemId is null || !_catalog.Values.TryGetValue(definition.ItemId, out OpcVariant initialValue))
                    {
                        addResults[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
                        errors[i] = OpcResultId.UnknownItemId.Code;
                        continue;
                    }

                    int serverHandle = Interlocked.Increment(ref _nextItemHandle);
                    var item = new TrackedItem(
                        serverHandle,
                        definition.ItemId,
                        definition.ClientHandle,
                        definition.Active,
                        initialValue.Type,
                        initialValue,
                        DateTimeOffset.UtcNow);
                    _items[serverHandle] = item;
                    addResults[i] = new OpcItemResult(serverHandle, item.CanonicalDataType, 0x3, []);
                    errors[i] = OpcResultId.Ok.Code;
                }
            }

            return Task.CompletedTask;
        }

        public Task ValidateItemsAsync(
            OpcItemDef[] itemDefinitions,
            bool blobUpdate,
            out OpcItemResult[] validationResults,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            _ = blobUpdate;
            ArgumentNullException.ThrowIfNull(itemDefinitions);
            cancellationToken.ThrowIfCancellationRequested();
            validationResults = new OpcItemResult[itemDefinitions.Length];
            errors = new int[itemDefinitions.Length];

            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                OpcItemDef definition = itemDefinitions[i];
                if (definition.ItemId is null || !_catalog.Values.TryGetValue(definition.ItemId, out OpcVariant value))
                {
                    validationResults[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
                    errors[i] = OpcResultId.UnknownItemId.Code;
                    continue;
                }

                validationResults[i] = new OpcItemResult(0, value.Type, 0x3, []);
                errors[i] = OpcResultId.Ok.Code;
            }

            return Task.CompletedTask;
        }

        public Task<int[]> RemoveItemsAsync(int[] serverHandles, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serverHandles);
            cancellationToken.ThrowIfCancellationRequested();
            var errors = new int[serverHandles.Length];
            lock (_gate)
            {
                for (int i = 0; i < serverHandles.Length; i++)
                {
                    errors[i] = _items.Remove(serverHandles[i])
                        ? OpcResultId.Ok.Code
                        : OpcResultId.InvalidHandle.Code;
                }
            }

            return Task.FromResult(errors);
        }

        public Task<int[]> SetActiveStateAsync(int[] serverHandles, bool active, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serverHandles);
            cancellationToken.ThrowIfCancellationRequested();
            var errors = new int[serverHandles.Length];
            lock (_gate)
            {
                for (int i = 0; i < serverHandles.Length; i++)
                {
                    if (_items.TryGetValue(serverHandles[i], out TrackedItem? item))
                    {
                        item.Active = active;
                        errors[i] = OpcResultId.Ok.Code;
                    }
                    else
                    {
                        errors[i] = OpcResultId.InvalidHandle.Code;
                    }
                }
            }

            return Task.FromResult(errors);
        }

        public Task<int[]> SetClientHandlesAsync(int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serverHandles);
            ArgumentNullException.ThrowIfNull(clientHandles);
            cancellationToken.ThrowIfCancellationRequested();
            var errors = new int[serverHandles.Length];
            lock (_gate)
            {
                for (int i = 0; i < serverHandles.Length; i++)
                {
                    if (i < clientHandles.Length && _items.TryGetValue(serverHandles[i], out TrackedItem? item))
                    {
                        item.ClientHandle = clientHandles[i];
                        errors[i] = OpcResultId.Ok.Code;
                    }
                    else
                    {
                        errors[i] = OpcResultId.InvalidHandle.Code;
                    }
                }
            }

            return Task.FromResult(errors);
        }

        public Task<int[]> SetDatatypesAsync(int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serverHandles);
            ArgumentNullException.ThrowIfNull(requestedDataTypes);
            cancellationToken.ThrowIfCancellationRequested();
            int[] errors = serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray();
            return Task.FromResult(errors);
        }

        public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default)
        {
            _ = requestedInterfaceId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromException<IOpcInterfaceRef>(new OpcException(OpcResultId.NotImplemented));
        }

        public Task<OpcItemState[]> ReadAsync(
            int dataSource,
            int[] serverHandles,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            _ = dataSource;
            ArgumentNullException.ThrowIfNull(serverHandles);
            cancellationToken.ThrowIfCancellationRequested();
            var states = new OpcItemState[serverHandles.Length];
            errors = new int[serverHandles.Length];

            lock (_gate)
            {
                for (int i = 0; i < serverHandles.Length; i++)
                {
                    if (_items.TryGetValue(serverHandles[i], out TrackedItem? item))
                    {
                        states[i] = item.ToState();
                        errors[i] = OpcResultId.Ok.Code;
                    }
                    else
                    {
                        states[i] = new OpcItemState(0, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Empty);
                        errors[i] = OpcResultId.InvalidHandle.Code;
                    }
                }
            }

            return Task.FromResult(states);
        }

        public Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(serverHandles);
            ArgumentNullException.ThrowIfNull(values);
            cancellationToken.ThrowIfCancellationRequested();
            var errors = new int[serverHandles.Length];
            lock (_gate)
            {
                for (int i = 0; i < serverHandles.Length; i++)
                {
                    if (i < values.Length && _items.TryGetValue(serverHandles[i], out TrackedItem? item))
                    {
                        item.Value = values[i];
                        item.Timestamp = DateTimeOffset.UtcNow;
                        errors[i] = OpcResultId.Ok.Code;
                    }
                    else
                    {
                        errors[i] = OpcResultId.InvalidHandle.Code;
                    }
                }
            }

            return Task.FromResult(errors);
        }

        Task<int> IOPCAsyncIO2.ReadAsync(
            int[] serverHandles,
            int transactionId,
            out int[] errors,
            CancellationToken cancellationToken)
        {
            _ = transactionId;
            ArgumentNullException.ThrowIfNull(serverHandles);
            cancellationToken.ThrowIfCancellationRequested();
            errors = serverHandles.Select(handle => _items.ContainsKey(handle) ? OpcResultId.Ok.Code : OpcResultId.InvalidHandle.Code).ToArray();
            return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
        }

        Task<int> IOPCAsyncIO2.WriteAsync(
            int[] serverHandles,
            OpcVariant[] values,
            int transactionId,
            out int[] errors,
            CancellationToken cancellationToken)
        {
            _ = transactionId;
            errors = WriteAsync(serverHandles, values, cancellationToken).GetAwaiter().GetResult();
            return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
        }

        public async Task<int> Refresh2Async(int dataSource, int transactionId, CancellationToken cancellationToken = default)
        {
            _ = dataSource;
            cancellationToken.ThrowIfCancellationRequested();
            int cancelId = Interlocked.Increment(ref _nextCancelId);
            if (!_callbacksEnabled)
            {
                return cancelId;
            }

            IOpcInterfaceRef[] sinks;
            DataChangeSnapshot snapshot;
            lock (_gate)
            {
                sinks = _sinks.Values.ToArray();
                snapshot = BuildDataChangeSnapshot(transactionId);
            }

            foreach (IOpcInterfaceRef sink in sinks)
            {
                await PublishDataChangeAsync(sink, snapshot, cancellationToken);
            }

            return cancelId;
        }

        public Task Cancel2Async(int cancelId, CancellationToken cancellationToken = default)
        {
            _ = cancelId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task SetEnableAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _callbacksEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task<bool> GetEnableAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_callbacksEnabled);
        }

        public Task<IOpcInterfaceRef> EnumConnectionPointsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromException<IOpcInterfaceRef>(new OpcException(OpcResultId.NotImplemented));
        }

        public Task<IOpcInterfaceRef> FindConnectionPointAsync(Guid iid, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (iid != IOPCDataCallback.InterfaceId)
            {
                throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
            }

            return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
                iid: IConnectionPoint.InterfaceId,
                flags: 0,
                publicRefs: 1,
                oxid: 1,
                oid: unchecked((ulong)ServerHandle),
                ipid: _ipid,
                securityOffset: 0,
                resolverBindings: Array.Empty<ushort>()));
        }

        public Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(IOPCDataCallback.InterfaceId);
        }

        public Task<int> AdviseAsync(IOpcInterfaceRef sink, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(sink);
            cancellationToken.ThrowIfCancellationRequested();
            if (sink.Iid != IOPCDataCallback.InterfaceId)
            {
                throw new OpcException(new OpcResultId(unchecked((int)0x80040202), "CONNECT_E_CANNOTCONNECT"));
            }

            int cookie = Interlocked.Increment(ref _nextCookie);
            lock (_gate)
            {
                _sinks[cookie] = sink;
            }

            return Task.FromResult(cookie);
        }

        public Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (_gate)
            {
                if (_sinks.Remove(cookie))
                {
                    return Task.CompletedTask;
                }
            }

            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }

        private DataChangeSnapshot BuildDataChangeSnapshot(int transactionId)
        {
            TrackedItem[] activeItems = _items.Values
                .Where(static item => item.Active)
                .OrderBy(static item => item.ServerHandle)
                .ToArray();
            return new DataChangeSnapshot(
                transactionId,
                ClientGroupHandle,
                OpcQuality.Good.RawValue,
                OpcResultId.Ok.Code,
                activeItems.Select(static item => item.ClientHandle).ToArray(),
                activeItems.Select(static item => item.Value).ToArray(),
                activeItems.Select(static _ => OpcQuality.Good.RawValue).ToArray(),
                activeItems.Select(static item => item.Timestamp.ToFileTime()).ToArray(),
                activeItems.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }

        private async Task PublishDataChangeAsync(
            IOpcInterfaceRef sink,
            DataChangeSnapshot snapshot,
            CancellationToken cancellationToken)
        {
            if (sink.Iid != IOPCDataCallback.InterfaceId)
            {
                return;
            }

            IPEndPoint endpoint = _callbackEndpoint.Endpoint
                ?? throw new InvalidOperationException("Callback endpoint was not registered before Refresh2.");
            await using DcomCallChannel channel = await DcomCallChannelFactory.ConnectTcpAsync(
                endpoint.Address.ToString(),
                endpoint.Port,
                NoOpAuthContext.Instance,
                cancellationToken);
            var callback = new IOPCDataCallbackClientProxy(channel);
            await callback.OnDataChangeAsync(
                snapshot.TransactionId,
                snapshot.GroupHandle,
                snapshot.MasterQuality,
                snapshot.MasterError,
                snapshot.ClientHandles,
                snapshot.Values,
                snapshot.Qualities,
                snapshot.Timestamps,
                snapshot.Errors,
                cancellationToken);
        }
    }

    private sealed class TrackedItem
    {
        public TrackedItem(
            int serverHandle,
            string itemId,
            int clientHandle,
            bool active,
            VarType canonicalDataType,
            OpcVariant value,
            DateTimeOffset timestamp)
        {
            ServerHandle = serverHandle;
            ItemId = itemId;
            ClientHandle = clientHandle;
            Active = active;
            CanonicalDataType = canonicalDataType;
            Value = value;
            Timestamp = timestamp;
        }

        public int ServerHandle { get; }
        public string ItemId { get; }
        public int ClientHandle { get; set; }
        public bool Active { get; set; }
        public VarType CanonicalDataType { get; }
        public OpcVariant Value { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public OpcItemState ToState() => new(ClientHandle, Timestamp, OpcQuality.Good, Value);
    }

    private sealed class RecordingDataCallback : IOPCDataCallback
    {
        private readonly TaskCompletionSource<DataChangeSnapshot> _dataChange = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task<DataChangeSnapshot> WaitForDataChangeAsync(CancellationToken cancellationToken) =>
            _dataChange.Task.WaitAsync(cancellationToken);

        public Task OnDataChangeAsync(
            int transactionId,
            int groupHandle,
            int masterQuality,
            int masterError,
            int[] clientHandles,
            OpcVariant[] values,
            ushort[] qualities,
            long[] timestamps,
            int[] errors,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _dataChange.TrySetResult(new DataChangeSnapshot(
                transactionId,
                groupHandle,
                masterQuality,
                masterError,
                clientHandles,
                values,
                qualities,
                timestamps,
                errors));
            return Task.CompletedTask;
        }

        public Task OnReadCompleteAsync(
            int transactionId,
            int groupHandle,
            int masterQuality,
            int masterError,
            int[] clientHandles,
            OpcVariant[] values,
            ushort[] qualities,
            long[] timestamps,
            int[] errors,
            CancellationToken cancellationToken = default)
        {
            _ = transactionId;
            _ = groupHandle;
            _ = masterQuality;
            _ = masterError;
            _ = clientHandles;
            _ = values;
            _ = qualities;
            _ = timestamps;
            _ = errors;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task OnWriteCompleteAsync(
            int transactionId,
            int groupHandle,
            int masterError,
            int[] clientHandles,
            int[] errors,
            CancellationToken cancellationToken = default)
        {
            _ = transactionId;
            _ = groupHandle;
            _ = masterError;
            _ = clientHandles;
            _ = errors;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task OnCancelCompleteAsync(int transactionId, int groupHandle, CancellationToken cancellationToken = default)
        {
            _ = transactionId;
            _ = groupHandle;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed record DataChangeSnapshot(
        int TransactionId,
        int GroupHandle,
        int MasterQuality,
        int MasterError,
        int[] ClientHandles,
        OpcVariant[] Values,
        ushort[] Qualities,
        long[] Timestamps,
        int[] Errors);
}
