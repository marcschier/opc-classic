// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Integration.Tests.Da;

public sealed class DaEnumOpcItemAttributesVeuInfoTests
{
    private const int OpcEuTypeNone = 0;
    private const int OpcEuTypeAnalog = 1;
    private const int OpcEuTypeEnumerated = 2;

    [Test]
    [Category("Da.Loopback")]
    public async Task CreateEnumerator_Next_round_trips_vEUInfo_variant_arrays_over_managed_transport()
    {
        await using ServiceProvider provider = BuildServiceProvider();
        OpcDaServerHost host = ResolveHost(provider);

        await host.StartAsync(TestContext.Current!.CancellationToken);
        try
        {
            await using DcomCallChannel serverChannel = await ConnectRootClientAsync(host);
            var server = new IOPCServerClientProxy(serverChannel);

            await server.AddGroupAsync(
                name: "DA vEUInfo attributes",
                active: true,
                requestedUpdateRate: 1000,
                clientGroupHandle: 100,
                timeBias: 0,
                percentDeadband: 0f,
                localeId: 1033,
                requestedInterfaceId: IOPCItemMgt.InterfaceId,
                out _,
                out _,
                out IOpcInterfaceRef groupRef,
                TestContext.Current.CancellationToken);

            await using DcomCallChannel itemMgtChannel = await ConnectObjectClientAsync(host, groupRef.Ipid);
            var itemMgt = new IOPCItemMgtClientProxy(itemMgtChannel);
            IOpcInterfaceRef enumRef = await itemMgt.CreateEnumeratorAsync(
                IEnumOPCItemAttributes.InterfaceId,
                TestContext.Current.CancellationToken);

            await using DcomCallChannel enumChannel = await ConnectObjectClientAsync(host, enumRef.Ipid);
            var enumerator = new IEnumOPCItemAttributesClientProxy(enumChannel);
            IReadOnlyList<OpcItemAttributes> attributes = await ReadAllAsync(enumerator, TestContext.Current.CancellationToken);

            await Assert.That(attributes.Count).IsEqualTo(3);
            OpcItemAttributes discrete = attributes.Single(static item => item.ItemId == "Discrete.Signal");
            OpcItemAttributes analog = attributes.Single(static item => item.ItemId == "Analog.Scale");
            OpcItemAttributes noEuInfo = attributes.Single(static item => item.ItemId == "NoEuInfo.Int4");

            await Assert.That(discrete.EUType).IsEqualTo(OpcEuTypeEnumerated);
            await AssertStringSafeArrayAsync(discrete.EUInfo, ["Low", "Medium", "High"]);

            await Assert.That(analog.EUType).IsEqualTo(OpcEuTypeAnalog);
            await AssertDoubleSafeArrayAsync(analog.EUInfo, [0.0d, 100.0d]);

            await Assert.That(noEuInfo.EUType).IsEqualTo(OpcEuTypeNone);
            await Assert.That(noEuInfo.EUInfo.Type).IsEqualTo(VarType.VT_EMPTY);
            await Assert.That(noEuInfo.EUInfo).IsEqualTo(OpcVariant.Empty);
        }
        finally
        {
            await host.StopAsync(TestContext.Current!.CancellationToken);
        }
    }

    private static async Task<IReadOnlyList<OpcItemAttributes>> ReadAllAsync(
        IEnumOPCItemAttributesClientProxy enumerator,
        CancellationToken cancellationToken)
    {
        var attributes = new List<OpcItemAttributes>();
        while (true)
        {
            await enumerator.NextAsync(2, out OpcItemAttributes[] batch, out int fetched, cancellationToken);
            if (fetched == 0)
            {
                return attributes;
            }

            // Only include the actual fetched slice — the enumerator surfaces pceltFetched so
            // a server pre-allocating to `celt` length doesn't confuse the consumer.
            for (int i = 0; i < fetched && i < batch.Length; i++)
            {
                attributes.Add(batch[i]);
            }
        }
    }

    private static async Task AssertStringSafeArrayAsync(OpcVariant variant, string[] expected)
    {
        await Assert.That(variant.Type).IsEqualTo((VarType)((ushort)VarType.VT_ARRAY | (ushort)VarType.VT_BSTR));
        OpcSafeArray? array = variant.AsSafeArray();
        await Assert.That(array is not null).IsTrue();
        await Assert.That(array!.ElementType).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(array.TotalElements).IsEqualTo(expected.Length);
        await Assert.That(array.Data.Cast<string?>().SequenceEqual(expected)).IsTrue();
    }

    private static async Task AssertDoubleSafeArrayAsync(OpcVariant variant, double[] expected)
    {
        await Assert.That(variant.Type).IsEqualTo((VarType)((ushort)VarType.VT_ARRAY | (ushort)VarType.VT_R8));
        OpcSafeArray? array = variant.AsSafeArray();
        await Assert.That(array is not null).IsTrue();
        await Assert.That(array!.ElementType).IsEqualTo(VarType.VT_R8);
        await Assert.That(array.TotalElements).IsEqualTo(expected.Length);
        await Assert.That(((double[])array.Data).SequenceEqual(expected)).IsTrue();
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<IOpcDaServer>(sp => new VeuInfoDaServer(sp.GetRequiredService<OpcObjectRegistry>()));
        services.AddSingleton<IOpcDaDataChangePublisher, OpcDaDataChangePublisher>();
        services.AddSingleton<OpcDaServerHost>();
        services.AddSingleton<IOpcServerHost>(sp => sp.GetRequiredService<OpcDaServerHost>());
        services.Configure<OpcDaServerOptions>(o =>
        {
            o.Clsid = Guid.NewGuid();
            o.ProgId = "Managed.Da.VEuInfo.1";
            o.FriendlyName = "Managed DA vEUInfo test server";
            o.ListenAddress = "127.0.0.1:0";
        });
        return services.BuildServiceProvider();
    }

    private static OpcDaServerHost ResolveHost(ServiceProvider provider) =>
        (OpcDaServerHost)provider.GetRequiredService<IOpcServerHost>();

    private static async Task<DcomCallChannel> ConnectRootClientAsync(OpcDaServerHost host)
    {
        TcpClientTransport transport = await ConnectTransportAsync(host);
        return new DcomCallChannel(transport, NoOpAuthContext.Instance);
    }

    private static async Task<DcomCallChannel> ConnectObjectClientAsync(OpcDaServerHost host, Guid objectIpid)
    {
        TcpClientTransport transport = await ConnectTransportAsync(host);
        return new DcomCallChannel(transport, NoOpAuthContext.Instance, objectIpid);
    }

    private static async Task<TcpClientTransport> ConnectTransportAsync(OpcDaServerHost host)
    {
        var bound = (IPEndPoint?)host.LocalEndpoint
            ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");

        return await TcpClientTransport.ConnectAsync(
            bound.Address.ToString(),
            bound.Port,
            TestContext.Current!.CancellationToken);
    }

    private sealed class VeuInfoDaServer : IOpcDaServer
    {
        private readonly OpcObjectRegistry _registry;
        private readonly Dictionary<int, Guid> _groupIpids = new();
        private int _nextServerHandle = 2000;

        public VeuInfoDaServer(OpcObjectRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new OpcServerStatus
            {
                Spec = OpcStatusSpec.Da,
                StartTime = DateTimeOffset.UnixEpoch,
                CurrentTime = DateTimeOffset.UnixEpoch,
                LastUpdateTime = DateTimeOffset.UnixEpoch,
                State = OpcServerState.Running,
                ServerVersion = new Version(1, 0, 0),
                VendorInfo = "Managed DA vEUInfo test server",
                GroupCount = _groupIpids.Count,
                BandWidth = 0,
            });
        }

        public Task<int> AddGroupAsync(
            string name,
            bool active,
            int requestedUpdateRate,
            int clientHandle,
            int localeId,
            CancellationToken cancellationToken = default)
        {
            _ = name;
            _ = active;
            _ = requestedUpdateRate;
            _ = clientHandle;
            _ = localeId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Interlocked.Increment(ref _nextServerHandle));
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
            _ = name;
            _ = active;
            _ = timeBias;
            _ = percentDeadband;
            _ = localeId;
            _ = clientGroupHandle;
            cancellationToken.ThrowIfCancellationRequested();

            serverGroupHandle = Interlocked.Increment(ref _nextServerHandle);
            revisedUpdateRate = requestedUpdateRate;
            var itemMgt = new VeuInfoItemMgt(_registry);
            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCItemMgt.InterfaceId] = new IOPCItemMgtServerDispatcher(itemMgt),
            };
            Guid ipid = _registry.Register(dispatchers);
            _groupIpids[serverGroupHandle] = ipid;
            group = new OpcInterfaceRef(
                iid: requestedInterfaceId,
                flags: 0,
                publicRefs: 1,
                oxid: 1,
                oid: unchecked((ulong)serverGroupHandle),
                ipid: ipid,
                securityOffset: 0,
                resolverBindings: Array.Empty<ushort>());
            return Task.CompletedTask;
        }

        public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
        {
            _ = force;
            cancellationToken.ThrowIfCancellationRequested();
            if (_groupIpids.Remove(serverGroupHandle, out Guid ipid))
            {
                _registry.Unregister(ipid);
            }

            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
        {
            _ = localeId;
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult($"Managed DA vEUInfo error 0x{errorCode:X8}");
        }
    }

    private sealed class VeuInfoItemMgt : IOPCItemMgt
    {
        private static readonly OpcItemAttributes[] Attributes =
        [
            new(
                AccessPath: string.Empty,
                ItemId: "Discrete.Signal",
                Active: true,
                ClientHandle: 1,
                ServerHandle: 101,
                AccessRights: 0x1,
                Blob: Array.Empty<byte>(),
                RequestedDataType: VarType.VT_I4,
                CanonicalDataType: VarType.VT_I4,
                EUType: OpcEuTypeEnumerated,
                EUInfo: OpcVariant.FromSafeArray(OpcSafeArray.OfString(["Low", "Medium", "High"]))),
            new(
                AccessPath: string.Empty,
                ItemId: "Analog.Scale",
                Active: true,
                ClientHandle: 2,
                ServerHandle: 102,
                AccessRights: 0x1,
                Blob: Array.Empty<byte>(),
                RequestedDataType: VarType.VT_R8,
                CanonicalDataType: VarType.VT_R8,
                EUType: OpcEuTypeAnalog,
                EUInfo: OpcVariant.FromSafeArray(OpcSafeArray.OfDouble([0.0d, 100.0d]))),
            new(
                AccessPath: string.Empty,
                ItemId: "NoEuInfo.Int4",
                Active: true,
                ClientHandle: 3,
                ServerHandle: 103,
                AccessRights: 0x1,
                Blob: Array.Empty<byte>(),
                RequestedDataType: VarType.VT_I4,
                CanonicalDataType: VarType.VT_I4,
                EUType: OpcEuTypeNone,
                EUInfo: OpcVariant.Empty),
        ];

        private readonly OpcObjectRegistry _registry;

        public VeuInfoItemMgt(OpcObjectRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        }

        public Task AddItemsAsync(
            OpcItemDef[] itemDefinitions,
            out OpcItemResult[] addResults,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(itemDefinitions);
            cancellationToken.ThrowIfCancellationRequested();
            addResults = Array.Empty<OpcItemResult>();
            errors = Array.Empty<int>();
            return Task.CompletedTask;
        }

        public Task ValidateItemsAsync(
            OpcItemDef[] itemDefinitions,
            bool blobUpdate,
            out OpcItemResult[] validationResults,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(itemDefinitions);
            _ = blobUpdate;
            cancellationToken.ThrowIfCancellationRequested();
            validationResults = Array.Empty<OpcItemResult>();
            errors = Array.Empty<int>();
            return Task.CompletedTask;
        }

        public Task<int[]> RemoveItemsAsync(int[] serverHandles, CancellationToken cancellationToken = default) =>
            ErrorsForAsync(serverHandles, cancellationToken);

        public Task<int[]> SetActiveStateAsync(int[] serverHandles, bool active, CancellationToken cancellationToken = default)
        {
            _ = active;
            return ErrorsForAsync(serverHandles, cancellationToken);
        }

        public Task<int[]> SetClientHandlesAsync(int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(clientHandles);
            return ErrorsForAsync(serverHandles, cancellationToken);
        }

        public Task<int[]> SetDatatypesAsync(int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(requestedDataTypes);
            return ErrorsForAsync(serverHandles, cancellationToken);
        }

        public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var enumerator = new OpcDaItemAttributesEnumerator(Attributes, _registry);
            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IEnumOPCItemAttributes.InterfaceId] = new IEnumOPCItemAttributesServerDispatcher(enumerator),
            };
            Guid ipid = _registry.Register(dispatchers);
            return Task.FromResult<IOpcInterfaceRef>(new OpcInterfaceRef(
                iid: requestedInterfaceId,
                flags: 0,
                publicRefs: 1,
                oxid: 1,
                oid: 0,
                ipid: ipid,
                securityOffset: 0,
                resolverBindings: Array.Empty<ushort>()));
        }

        private static Task<int[]> ErrorsForAsync(int[] serverHandles, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(serverHandles);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(serverHandles.Select(static _ => OpcResultId.InvalidHandle.Code).ToArray());
        }
    }
}
