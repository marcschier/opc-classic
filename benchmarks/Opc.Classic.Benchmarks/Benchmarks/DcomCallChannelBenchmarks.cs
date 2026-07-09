// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using BenchmarkDotNet.Attributes;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Benchmarks.Benchmarks;

[MemoryDiagnoser]
public class DcomCallChannelBenchmarks
{
    private IOPCServerClientProxy _proxy = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var server = new ConstantDaServer(BuildStatus());
        var dispatcher = new OpcDaServerDispatcher(server);
        var channel = new InMemoryCallChannel(dispatcher.DispatchAsync);
        _proxy = new IOPCServerClientProxy(channel);
    }

    [Benchmark]
    public Task<OpcServerStatus> GetStatusAsync() => _proxy.GetStatusAsync(CancellationToken.None);

    private static OpcServerStatus BuildStatus() => new()
    {
        Spec = OpcStatusSpec.Da,
        StartTime = DateTimeOffset.UnixEpoch,
        CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(1),
        LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(2),
        State = OpcServerState.Running,
        ServerVersion = new Version(1, 2, 3),
        GroupCount = 7,
        BandWidth = 99,
        VendorInfo = "Benchmark InMemory DA Server",
    };

    private sealed class ConstantDaServer : IOpcDaServer
    {
        private readonly Task<OpcServerStatus> _statusTask;

        public ConstantDaServer(OpcServerStatus status)
        {
            _statusTask = Task.FromResult(status);
        }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return _statusTask;
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
            return Task.FromResult(0);
        }

        public Task RemoveGroupAsync(
            int serverGroupHandle,
            bool force,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task<string> GetErrorStringAsync(
            int errorCode,
            int localeId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult("Benchmark error string");
        }
    }
}
