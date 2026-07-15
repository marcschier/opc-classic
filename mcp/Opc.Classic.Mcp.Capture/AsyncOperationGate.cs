// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

internal sealed class AsyncOperationGate : IAsyncDisposable
{
    private readonly object _sync = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _objectName;
    private Task? _disposeTask;
    private TaskCompletionSource? _drained;
    private int _active;
    private bool _closing;

    public AsyncOperationGate(string objectName) => _objectName = objectName;

    public async ValueTask<Lease> EnterAsync(CancellationToken cancellationToken)
    {
        lock (_sync)
        {
            if (_closing)
            {
                throw new ObjectDisposedException(_objectName);
            }
            _active++;
        }
        try
        {
            await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Lease(this);
        }
        catch
        {
            Exit(acquired: false);
            throw;
        }
    }

    private void Exit(bool acquired)
    {
        if (acquired)
        {
            _semaphore.Release();
        }
        TaskCompletionSource? drained = null;
        lock (_sync)
        {
            _active--;
            if (_closing && _active == 0)
            {
                drained = _drained;
            }
        }
        drained?.TrySetResult();
    }

    public ValueTask DisposeAsync()
    {
        lock (_sync)
        {
            if (_disposeTask is not null)
            {
                return new ValueTask(_disposeTask);
            }
            _closing = true;
            if (_active == 0)
            {
                _semaphore.Dispose();
                _disposeTask = Task.CompletedTask;
                return ValueTask.CompletedTask;
            }
            _drained = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _disposeTask = FinishDisposeAsync(_drained.Task);
            return new ValueTask(_disposeTask);
        }
    }

    private async Task FinishDisposeAsync(Task drained)
    {
        await drained.ConfigureAwait(false);
        _semaphore.Dispose();
    }

    internal sealed class Lease : IAsyncDisposable, IDisposable
    {
        private AsyncOperationGate? _owner;

        public Lease(AsyncOperationGate owner) => _owner = owner;

        public void Dispose()
        {
            AsyncOperationGate? owner = Interlocked.Exchange(ref _owner, null);
            owner?.Exit(acquired: true);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
