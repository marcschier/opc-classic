// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Hosting;

namespace Opc.Classic.Integration.Tests.Support;

internal sealed class GatedDaServer : IOpcDaServer
{
    private readonly TaskCompletionSource _entered = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task Entered => _entered.Task;

    public async Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _entered.TrySetResult();
        await _release.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = DateTimeOffset.UnixEpoch,
            CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(1),
            LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(2),
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Gated DA Stub Server",
        };
    }

    public void Release() => _release.TrySetResult();

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
        _ = cancellationToken;
        throw new NotSupportedException("Group creation is not exercised by the gated DA cancellation test.");
    }

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        _ = serverGroupHandle;
        _ = force;
        _ = cancellationToken;
        throw new NotSupportedException("Group removal is not exercised by the gated DA cancellation test.");
    }

    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        _ = errorCode;
        _ = localeId;
        _ = cancellationToken;
        throw new NotSupportedException("Error strings are not exercised by the gated DA cancellation test.");
    }
}
