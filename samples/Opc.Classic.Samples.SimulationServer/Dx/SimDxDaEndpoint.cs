// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using Opc.Classic.Dx;

namespace Opc.Classic.Samples.SimulationServer.Dx;

/// <summary>
/// Deterministic, model-backed DA endpoint used by the simulation DX reference engine.
/// </summary>
public sealed class SimDxDaEndpoint : IDxDaAdapter
{
    private readonly SimulatedPlantModel _model;
    private readonly IDxClock _clock;
    private readonly ConcurrentQueue<DxDataValue> _readFailures = new();
    private readonly ConcurrentQueue<DxWriteResult> _writeFailures = new();
    private readonly Lock _gate = new();
    private TaskCompletionSource? _readGate;
    private DxEndpointHealthState _healthState = DxEndpointHealthState.Healthy;
    private OpcResultId _healthError = OpcResultId.Ok;
    private string? _healthDiagnostic;
    private int _reconnectFailures;
    private int _canceledReadCount;
    private int _healthCount;
    private int _readCount;
    private int _reconnectCount;
    private int _writeCount;

    /// <summary>Creates a deterministic DA endpoint over the shared plant model.</summary>
    public SimDxDaEndpoint(string name, SimulatedPlantModel model, IDxClock clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(clock);
        Name = name;
        _model = model;
        _clock = clock;
    }

    /// <inheritdoc />
    public string Name { get; }

    /// <summary>Number of canceled reads observed by the endpoint.</summary>
    public int CanceledReadCount => Volatile.Read(ref _canceledReadCount);

    /// <summary>Number of health checks performed by the engine.</summary>
    public int HealthCount => Volatile.Read(ref _healthCount);

    /// <summary>Number of read batches requested by the engine.</summary>
    public int ReadCount => Volatile.Read(ref _readCount);

    /// <summary>Number of reconnect attempts requested by the engine.</summary>
    public int ReconnectCount => Volatile.Read(ref _reconnectCount);

    /// <summary>Number of write batches requested by the engine.</summary>
    public int WriteCount => Volatile.Read(ref _writeCount);

    /// <summary>Queues a deterministic source read failure.</summary>
    public void FailNextRead(
        OpcResultId? errorId = null,
        string diagnostic = "Simulated source endpoint failure.",
        DxQualityStatus quality = DxQualityStatus.BadCommFailure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnostic);
        _readFailures.Enqueue(new DxDataValue(
            OpcVariant.Empty,
            _clock.UtcNow,
            new DxQuality(quality),
            errorId ?? OpcResultId.Fail,
            diagnostic));
        SetHealth(DxEndpointHealthState.Disconnected, errorId ?? OpcResultId.Fail, diagnostic);
    }

    /// <summary>Queues a deterministic target write failure.</summary>
    public void FailNextWrite(
        OpcResultId? errorId = null,
        string diagnostic = "Simulated target endpoint failure.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(diagnostic);
        _writeFailures.Enqueue(new DxWriteResult(
            new DxDataItem("pending"),
            errorId ?? OpcResultId.Fail,
            diagnostic));
        SetHealth(DxEndpointHealthState.Disconnected, errorId ?? OpcResultId.Fail, diagnostic);
    }

    /// <summary>Causes the next reconnect attempts to fail before recovery succeeds.</summary>
    public void FailReconnectAttempts(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Interlocked.Exchange(ref _reconnectFailures, count);
    }

    /// <summary>Blocks future reads until the returned gate is disposed.</summary>
    public IDisposable BlockReads()
    {
        lock (_gate)
        {
            if (_readGate is not null)
            {
                throw new InvalidOperationException("DX reads are already blocked.");
            }

            _readGate = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously);
            return new ReadBlock(this, _readGate);
        }
    }

    /// <inheritdoc />
    public ValueTask<DxEndpointHealth> GetHealthAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _healthCount);
        lock (_gate)
        {
            return ValueTask.FromResult(new DxEndpointHealth(
                _healthState,
                _clock.UtcNow,
                _healthError,
                _healthDiagnostic));
        }
    }

    /// <inheritdoc />
    public ValueTask ReconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _reconnectCount);
        while (true)
        {
            int remaining = Volatile.Read(ref _reconnectFailures);
            if (remaining == 0)
            {
                SetHealth(DxEndpointHealthState.Healthy, OpcResultId.Ok, null);
                return ValueTask.CompletedTask;
            }

            if (Interlocked.CompareExchange(ref _reconnectFailures, remaining - 1, remaining) == remaining)
            {
                throw new InvalidOperationException("Simulated endpoint reconnect failure.");
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<DxReadResult>> ReadAsync(
        IReadOnlyList<DxDataItem> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        Interlocked.Increment(ref _readCount);
        Task? wait;
        lock (_gate)
        {
            wait = _readGate?.Task;
        }

        if (wait is not null)
        {
            try
            {
                await wait.WaitAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Interlocked.Increment(ref _canceledReadCount);
                throw;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (_readFailures.TryDequeue(out DxDataValue? failure))
        {
            return items.Select(item => new DxReadResult(item, failure)).ToArray();
        }

        var results = new DxReadResult[items.Count];
        for (var i = 0; i < results.Length; i++)
        {
            DxDataItem item = items[i];
            string itemId = CombineItemId(item);
            if (!_model.TryGetTag(itemId, out SimulatedTag tag))
            {
                results[i] = new DxReadResult(
                    item,
                    new DxDataValue(
                        OpcVariant.Empty,
                        _clock.UtcNow,
                        new DxQuality(DxQualityStatus.BadConfigurationError),
                        OpcResultId.UnknownItemId,
                        $"Simulation item '{itemId}' was not found."));
                continue;
            }

            results[i] = new DxReadResult(
                item,
                new DxDataValue(
                    OpcVariantConverter.FromObject(_model.CurrentValue(tag, _clock.UtcNow)),
                    _clock.UtcNow,
                    new DxQuality(DxQualityStatus.Good),
                    OpcResultId.Ok));
        }

        return results;
    }

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<DxWriteResult>> WriteAsync(
        IReadOnlyList<DxWriteRequest> requests,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);
        cancellationToken.ThrowIfCancellationRequested();
        Interlocked.Increment(ref _writeCount);
        var results = new DxWriteResult[requests.Count];
        for (var i = 0; i < results.Length; i++)
        {
            DxWriteRequest request = requests[i];
            if (_writeFailures.TryDequeue(out DxWriteResult? failure))
            {
                results[i] = failure with { Item = request.Item };
                continue;
            }

            string itemId = CombineItemId(request.Item);
            object? value = OpcVariantConverter.ToObject(request.Value);
            results[i] = value is not null && _model.TryWrite(itemId, value)
                ? new DxWriteResult(request.Item, OpcResultId.Ok)
                : new DxWriteResult(
                    request.Item,
                    OpcResultId.UnknownItemId,
                    $"Simulation target item '{itemId}' was not found or is read-only.");
        }

        return ValueTask.FromResult<IReadOnlyList<DxWriteResult>>(results);
    }

    private void SetHealth(
        DxEndpointHealthState state,
        OpcResultId error,
        string? diagnostic)
    {
        lock (_gate)
        {
            _healthState = state;
            _healthError = error;
            _healthDiagnostic = diagnostic;
        }
    }

    private void ReleaseReadBlock(TaskCompletionSource gate)
    {
        lock (_gate)
        {
            if (!ReferenceEquals(_readGate, gate))
            {
                return;
            }

            _readGate = null;
        }

        gate.TrySetResult();
    }

    private static string CombineItemId(DxDataItem item) =>
        string.IsNullOrWhiteSpace(item.ItemPath)
            ? item.ItemName
            : item.ItemPath + "." + item.ItemName;

    private sealed class ReadBlock(
        SimDxDaEndpoint endpoint,
        TaskCompletionSource gate) : IDisposable
    {
        private SimDxDaEndpoint? _endpoint = endpoint;

        public void Dispose()
        {
            SimDxDaEndpoint? owner = Interlocked.Exchange(ref _endpoint, null);
            owner?.ReleaseReadBlock(gate);
        }
    }
}

/// <summary>
/// Resolves the two deterministic managed DA endpoints used by the DX sample.
/// </summary>
public sealed class SimDxEndpointResolver : IDxEndpointResolver
{
    /// <summary>Creates a resolver for the source and target DA endpoints.</summary>
    public SimDxEndpointResolver(SimDxDaEndpoint source, SimDxDaEndpoint target)
    {
        Source = source ?? throw new ArgumentNullException(nameof(source));
        Target = target ?? throw new ArgumentNullException(nameof(target));
    }

    /// <summary>The deterministic source DA endpoint.</summary>
    public SimDxDaEndpoint Source { get; }

    /// <summary>The deterministic target DA endpoint.</summary>
    public SimDxDaEndpoint Target { get; }

    /// <inheritdoc />
    public IDxDaAdapter ResolveSource(DxSourceServer sourceServer)
    {
        ArgumentNullException.ThrowIfNull(sourceServer);
        return Source;
    }

    /// <inheritdoc />
    public IDxDaAdapter ResolveTarget(DxConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return Target;
    }
}
