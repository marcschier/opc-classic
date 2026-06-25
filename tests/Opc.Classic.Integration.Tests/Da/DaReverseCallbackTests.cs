// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Integration.Tests.Da;

public sealed class DaReverseCallbackTests
{
    private const string User = "opcuser";
    private const string Password = "P@ssw0rd!42";
    private const string Domain = "OPC";
    private const int ClientGroupHandle = 0x7007;
    private const int ClientItemHandle = 0x7070;

    [Test]
    [Category("Da.ReverseCallbacks")]
    public async Task Authenticated_server_as_client_delivers_OnDataChange_and_Unadvise_stops_it()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        CancellationToken cancellationToken = timeout.Token;

        var callback = new RecordingDataCallback();
        await using CallbackFixture callbackFixture = await StartCallbackListenerAsync(callback, cancellationToken);
        IOpcInterfaceRef callbackRef = callbackFixture.CreateSinkRef();

        var serverRegistry = new OpcObjectRegistry();
        var server = new ReverseCallbackDaServer(serverRegistry, CreateSinkFactory(callbackFixture.Endpoint));
        await using OpcServerListener serverListener = await StartDaListenerAsync(server, serverRegistry, cancellationToken);
        IPEndPoint serverEndpoint = (IPEndPoint)serverListener.LocalEndpoint;

        await using DcomCallChannel rootChannel = await ConnectAuthenticatedAsync(serverEndpoint, Guid.Empty, cancellationToken);
        var opcServer = new IOPCServerClientProxy(rootChannel);
        await opcServer.AddGroupAsync(
            name: "reverse-callbacks",
            active: true,
            requestedUpdateRate: 100,
            clientGroupHandle: ClientGroupHandle,
            timeBias: 0,
            percentDeadband: 0,
            localeId: 1033,
            requestedInterfaceId: IOPCItemMgt.InterfaceId,
            serverGroupHandle: out int serverGroupHandle,
            revisedUpdateRate: out int revisedUpdateRate,
            group: out IOpcInterfaceRef groupRef,
            cancellationToken);

        await Assert.That(revisedUpdateRate).IsEqualTo(100);
        await using DcomCallChannel groupChannel = await ConnectAuthenticatedAsync(serverEndpoint, groupRef.Ipid, cancellationToken);
        await using DcomCallChannel connectionPointChannel = await ConnectAuthenticatedAsync(serverEndpoint, groupRef.Ipid, cancellationToken);
        var itemMgt = new IOPCItemMgtClientProxy(groupChannel);
        var syncIo = new IOPCSyncIOClientProxy(groupChannel);
        var connectionPoint = new IConnectionPointClientProxy(connectionPointChannel);

        await itemMgt.AddItemsAsync(
            [new OpcItemDef(null, "Channel.Value", Active: true, ClientHandle: ClientItemHandle, Blob: [], VarType.VT_I4)],
            out OpcItemResult[] addResults,
            out int[] addErrors,
            cancellationToken);
        await Assert.That(addErrors[0]).IsEqualTo(OpcResultId.Ok.Code);

        int cookie = await connectionPoint.AdviseAsync(callbackRef, cancellationToken);
        await syncIo.WriteAsync([addResults[0].ServerHandle], [OpcVariant.FromInt32(42)], cancellationToken);
        await server.TriggerAsync(serverGroupHandle, [addResults[0].ServerHandle], transactionId: 0x7100, cancellationToken);

        DataChangeSnapshot first = await callback.WaitForCountAsync(1, cancellationToken);
        await Assert.That(first.TransactionId).IsEqualTo(0x7100);
        await Assert.That(first.GroupHandle).IsEqualTo(ClientGroupHandle);
        await Assert.That(first.ClientHandles).IsEquivalentTo([ClientItemHandle]);
        await Assert.That(first.Values[0].AsInt32()).IsEqualTo(42);

        await connectionPoint.UnadviseAsync(cookie, cancellationToken);
        await syncIo.WriteAsync([addResults[0].ServerHandle], [OpcVariant.FromInt32(99)], cancellationToken);
        await server.TriggerAsync(serverGroupHandle, [addResults[0].ServerHandle], transactionId: 0x7101, cancellationToken);

        await Assert.That(callback.CallCount).IsEqualTo(1);
    }

    [Test]
    [Category("Da.ReverseCallbacks")]
    public async Task TriggerDataChange_drops_faulting_direct_sink_and_continues_delivery()
    {
        var group = new OpcDaGroup(
            name: "fault-isolation",
            serverHandle: 0x7200,
            clientHandle: ClientGroupHandle,
            active: true,
            requestedUpdateRate: 100,
            timeBias: 0,
            percentDeadband: 0,
            localeId: 1033);
        using var faultingSink = new FaultingDataCallbackSink();
        using var healthySink = new RecordingDataCallbackSink();

        await group.AdviseAsync(faultingSink, TestContext.Current!.CancellationToken);
        await group.AdviseAsync(healthySink, TestContext.Current.CancellationToken);

        await group.TriggerDataChangeAsync(
            transactionId: 0x7201,
            serverHandles: [],
            static (_, _, _) => Task.CompletedTask,
            TestContext.Current.CancellationToken);
        await group.TriggerDataChangeAsync(
            transactionId: 0x7202,
            serverHandles: [],
            static (_, _, _) => Task.CompletedTask,
            TestContext.Current.CancellationToken);

        await Assert.That(faultingSink.DataChangeCount).IsEqualTo(1);
        await Assert.That(healthySink.DataChangeCount).IsEqualTo(2);
        await group.DisposeAsync();
    }

    private static IOpcDataCallbackSinkFactory CreateSinkFactory(IPEndPoint endpoint)
    {
        var credentials = new NetworkCredential(User, Password, Domain);
        OpcConnectData connectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse($"opcda://{endpoint.Address}:{endpoint.Port}/Callback"),
            credentials,
            OpcProtectionLevel.Integrity);
        return DcomOpcDataCallbackSinkFactory.CreateTcpOnly(connectData, endpoint.Address.ToString());
    }

    private static async Task<OpcServerListener> StartDaListenerAsync(
        ReverseCallbackDaServer server,
        OpcObjectRegistry registry,
        CancellationToken cancellationToken)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCServer.InterfaceId] = new IOPCServerServerDispatcher(server),
            },
            registry,
            new ConfiguredAuthenticationSource(User, Password, Domain));
        var listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(cancellationToken);
        return listener;
    }

    private static async Task<CallbackFixture> StartCallbackListenerAsync(
        IOPCDataCallback callback,
        CancellationToken cancellationToken)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var registry = new OpcObjectRegistry();
        OpcServerListener? listener = null;
        var exporter = new IObjectExporterDispatcher(
            () => listener?.LocalEndpoint as IPEndPoint,
            registry);
        Guid callbackIpid = registry.Register(new Dictionary<Guid, IOpcServerDispatcher>
        {
            [IOPCDataCallback.InterfaceId] = new IOPCDataCallbackServerDispatcher(callback),
        });
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IObjectExporterDispatcher.InterfaceId] = exporter,
            },
            registry,
            new ConfiguredAuthenticationSource(User, Password, Domain));
        listener = new OpcServerListener(endpoint, processor);
        await listener.StartAsync(cancellationToken);
        return new CallbackFixture(listener, callbackIpid);
    }

    private static async Task<DcomCallChannel> ConnectAuthenticatedAsync(
        IPEndPoint endpoint,
        Guid objectIpid,
        CancellationToken cancellationToken)
    {
        var credentials = new NetworkCredential(User, Password, Domain);
        OpcConnectData connectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse($"opcda://{endpoint.Address}:{endpoint.Port}/Reverse.Callbacks"),
            credentials,
            OpcProtectionLevel.Connect);
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            cancellationToken);
        IAuthContext authContext = NtlmAuthentication.CreateAuthContext(connectData);
        return objectIpid == Guid.Empty
            ? new DcomCallChannel(transport, authContext)
            : new DcomCallChannel(transport, authContext, objectIpid);
    }

    private sealed class ReverseCallbackDaServer : IOpcDaServer
    {
        private readonly OpcObjectRegistry _registry;
        private readonly IOpcDataCallbackSinkFactory _sinkFactory;
        private readonly Dictionary<int, (OpcDaGroup Group, Guid Ipid)> _groups = new();
        private int _nextGroupHandle = 10_000;

        public ReverseCallbackDaServer(OpcObjectRegistry registry, IOpcDataCallbackSinkFactory sinkFactory)
        {
            _registry = registry;
            _sinkFactory = sinkFactory;
        }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UtcNow,
                CurrentTime = DateTimeOffset.UtcNow,
                LastUpdateTime = DateTimeOffset.UtcNow,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "reverse callback test",
            });

        public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default)
        {
            AddGroupCore(name, active, requestedUpdateRate, clientHandle, localeId, out int serverHandle, out _, out _);
            return Task.FromResult(serverHandle);
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
            _ = timeBias;
            _ = percentDeadband;
            _ = requestedInterfaceId;
            AddGroupCore(name, active, requestedUpdateRate, clientGroupHandle, localeId, out serverGroupHandle, out revisedUpdateRate, out group);
            return Task.CompletedTask;
        }

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
        {
            _ = force;
            if (_groups.Remove(serverGroupHandle, out var entry))
            {
                _registry.Unregister(entry.Ipid);
            }

            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default) =>
            Task.FromResult($"0x{errorCode:X8}");

        public Task TriggerAsync(int serverGroupHandle, int[] serverHandles, int transactionId, CancellationToken cancellationToken)
        {
            OpcDaGroup group = _groups[serverGroupHandle].Group;
            return group.TriggerDataChangeAsync(
                transactionId,
                serverHandles,
                (sink, payload, _) =>
                {
                    using IOpcDataCallbackSink callbackSink = _sinkFactory.Create(sink);
                    callbackSink.OnDataChange(payload);
                    return Task.CompletedTask;
                },
                cancellationToken);
        }

        private void AddGroupCore(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            out int serverGroupHandle,
            out int revisedUpdateRate,
            out IOpcInterfaceRef groupRef)
        {
            var group = new OpcDaGroup(
                name,
                Interlocked.Increment(ref _nextGroupHandle),
                clientHandle,
                active,
                requestedUpdateRate,
                0,
                0,
                localeId,
                _registry);
            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCItemMgt.InterfaceId] = new IOPCItemMgtServerDispatcher(group),
                [IOPCSyncIO.InterfaceId] = new IOPCSyncIOServerDispatcher(group),
                [IConnectionPoint.InterfaceId] = new IConnectionPointServerDispatcher(group),
            };
            Guid ipid = _registry.Register(dispatchers);
            _groups[group.ServerHandle] = (group, ipid);
            serverGroupHandle = group.ServerHandle;
            revisedUpdateRate = group.UpdateRate;
            groupRef = new OpcInterfaceRef(IOPCItemMgt.InterfaceId, 0, 1, 1, (ulong)serverGroupHandle, ipid, 0, []);
        }
    }

    private sealed class CallbackFixture : IAsyncDisposable
    {
        private readonly OpcServerListener _listener;
        private readonly Guid _callbackIpid;

        public CallbackFixture(OpcServerListener listener, Guid callbackIpid)
        {
            _listener = listener;
            _callbackIpid = callbackIpid;
            Endpoint = (IPEndPoint)listener.LocalEndpoint;
        }

        public IPEndPoint Endpoint { get; }

        public IOpcInterfaceRef CreateSinkRef()
        {
            (ushort[] bindings, ushort securityOffset) = IObjectExporterDispatcher.BuildResolverBindings(Endpoint);
            return new OpcInterfaceRef(IOPCDataCallback.InterfaceId, 0, 1, 0xCA11, 0xDA7A, _callbackIpid, securityOffset, bindings);
        }

        public ValueTask DisposeAsync() => _listener.DisposeAsync();
    }

    private sealed class RecordingDataCallback : IOPCDataCallback
    {
        private readonly List<DataChangeSnapshot> _changes = new();
        private TaskCompletionSource<DataChangeSnapshot> _next = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount
        {
            get
            {
                lock (_changes)
                {
                    return _changes.Count;
                }
            }
        }

        public Task<DataChangeSnapshot> WaitForCountAsync(int count, CancellationToken cancellationToken)
        {
            lock (_changes)
            {
                if (_changes.Count >= count)
                {
                    return Task.FromResult(_changes[^1]);
                }

                return _next.Task.WaitAsync(cancellationToken);
            }
        }

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
            var snapshot = new DataChangeSnapshot(transactionId, groupHandle, masterQuality, masterError, clientHandles, values, qualities, timestamps, errors);
            lock (_changes)
            {
                _changes.Add(snapshot);
                _next.TrySetResult(snapshot);
                _next = new TaskCompletionSource<DataChangeSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            return Task.CompletedTask;
        }

        public Task OnReadCompleteAsync(int transactionId, int groupHandle, int masterQuality, int masterError, int[] clientHandles, OpcVariant[] values, ushort[] qualities, long[] timestamps, int[] errors, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task OnWriteCompleteAsync(int transactionId, int groupHandle, int masterError, int[] clientHandles, int[] errors, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task OnCancelCompleteAsync(int transactionId, int groupHandle, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
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

    private sealed class FaultingDataCallbackSink : IOpcDataCallbackSink
    {
        public int DataChangeCount { get; private set; }

        public void OnDataChange(OpcDaGroup.DataChangePayload payload)
        {
            _ = payload;
            DataChangeCount++;
            throw new InvalidOperationException("Simulated faulting DA callback sink.");
        }

        public void OnReadComplete(OpcDaGroup.DataChangePayload payload) => _ = payload;

        public void OnWriteComplete(int transactionId, int groupHandle, int masterError, int[] clientHandles, int[] errors)
        {
            _ = transactionId;
            _ = groupHandle;
            _ = masterError;
            _ = clientHandles;
            _ = errors;
        }

        public void OnCancelComplete(OpcDaGroup.CancelCompletePayload payload) => _ = payload;

        public void Dispose()
        {
        }
    }

    private sealed class RecordingDataCallbackSink : IOpcDataCallbackSink
    {
        public int DataChangeCount { get; private set; }

        public void OnDataChange(OpcDaGroup.DataChangePayload payload)
        {
            _ = payload;
            DataChangeCount++;
        }

        public void OnReadComplete(OpcDaGroup.DataChangePayload payload) => _ = payload;

        public void OnWriteComplete(int transactionId, int groupHandle, int masterError, int[] clientHandles, int[] errors)
        {
            _ = transactionId;
            _ = groupHandle;
            _ = masterError;
            _ = clientHandles;
            _ = errors;
        }

        public void OnCancelComplete(OpcDaGroup.CancelCompletePayload payload) => _ = payload;

        public void Dispose()
        {
        }
    }
}
