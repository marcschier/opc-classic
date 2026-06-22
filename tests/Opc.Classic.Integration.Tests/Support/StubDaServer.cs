// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Integration.Tests.Support;

internal sealed class StubDaServer : IOpcDaServer
{
    private readonly HashSet<string> _knownItemIds;
    private readonly string _errorPrefix;

    public StubDaServer(string vendorInfo, IEnumerable<string>? knownItemIds = null, string errorPrefix = "Stub error")
    {
        VendorInfo = vendorInfo;
        _knownItemIds = new HashSet<string>(knownItemIds ?? [], StringComparer.OrdinalIgnoreCase);
        _errorPrefix = errorPrefix;
    }

    public string VendorInfo { get; }
    public IReadOnlyCollection<string> KnownItemIds => _knownItemIds;
    public List<RemovedGroup> RemovedGroups { get; } = [];

    public static StubDaServer NativeSample() => new(
        "OPC Foundation Sample Stub server",
        ["Random.Int4", "Bucket Brigade.Boolean"],
        "Native sample stub error");

    public static StubDaServer MatrikonSimulation() => new(
        "Matrikon OPC Simulation Stub server",
        ["Random.Int4", "Random.Real4", "Bucket Brigade.Boolean", "Read Error.Int1"],
        "Matrikon simulation stub error");

    public static StubDaServer CompatMatrixNet10Server() => new(
        "Compat matrix net10 Stub server",
        ["Random.Int4", "Bucket Brigade.Boolean"],
        "Compat matrix stub error");

    public bool HasTag(string itemId) => _knownItemIds.Contains(itemId);

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = DateTimeOffset.UnixEpoch,
            CurrentTime = DateTimeOffset.UtcNow,
            LastUpdateTime = DateTimeOffset.UtcNow,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = VendorInfo,
            GroupCount = RemovedGroups.Count,
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
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(clientHandle + 1000);
    }

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RemovedGroups.Add(new RemovedGroup(serverGroupHandle, force));
        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"{_errorPrefix} 0x{errorCode:X8} locale=0x{localeId:X4}");
    }

    public static (IOPCServerClientProxy Proxy, InMemoryCallChannel Channel) CreateLoopbackProxy(IOpcDaServer server)
    {
        var dispatcher = new OpcDaServerDispatcher(server);
        var channel = new InMemoryCallChannel((iid, opnum, payload, ct) =>
            dispatcher.DispatchAsync(iid, opnum, payload, ct));
        return (new IOPCServerClientProxy(channel), channel);
    }

    public readonly record struct RemovedGroup(int ServerGroupHandle, bool Force);
}
