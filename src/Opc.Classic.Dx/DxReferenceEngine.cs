// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable MA0048 // The bounded reference engine and its public contracts are cohesive.
#pragma warning disable MA0051 // The transfer loop is intentionally kept as one explicit state machine.
#pragma warning disable VSTHRD003 // ConfigurationSignal tasks are owned and completed by this engine.
#pragma warning disable CA1064, RCS1194 // Private exception carries transfer control-flow metadata only.
#pragma warning disable CA1031 // Endpoint failures are isolated and converted into retry diagnostics.

using System.Collections.Concurrent;

namespace Opc.Classic.Dx;

/// <summary>
/// Resolves configured DX source and target endpoints.
/// </summary>
public interface IDxEndpointResolver
{
    /// <summary>Resolves the adapter for a registered source server.</summary>
    IDxDaAdapter ResolveSource(DxSourceServer sourceServer);

    /// <summary>Resolves the adapter that hosts a connection's target item.</summary>
    IDxDaAdapter ResolveTarget(DxConnection connection);
}

/// <summary>
/// Bounds and timing defaults for <see cref="DxReferenceEngine"/>.
/// </summary>
public sealed record DxReferenceEngineOptions
{
    /// <summary>Default source update rate.</summary>
    public TimeSpan DefaultUpdateRate { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Maximum accepted update rate.</summary>
    public TimeSpan MaximumUpdateRate { get; init; } = TimeSpan.FromHours(1);

    /// <summary>Initial reconnect delay after a failed transfer.</summary>
    public TimeSpan InitialRetryDelay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Maximum reconnect delay.</summary>
    public TimeSpan MaximumRetryDelay { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>Maximum number of registered source servers.</summary>
    public int MaximumSourceServers { get; init; } = 256;

    /// <summary>Maximum number of configured connections.</summary>
    public int MaximumConnections { get; init; } = 1024;

    /// <summary>Maximum per-connection queue capacity.</summary>
    public int MaximumQueueCapacity { get; init; } = 1024;

    /// <summary>Maximum retained diagnostic records.</summary>
    public int DiagnosticCapacity { get; init; } = 1024;

    internal void Validate()
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(DefaultUpdateRate, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaximumUpdateRate, DefaultUpdateRate);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(InitialRetryDelay, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThan(MaximumRetryDelay, InitialRetryDelay);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumSourceServers);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumConnections);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(MaximumQueueCapacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(DiagnosticCapacity);
    }
}

/// <summary>
/// Indicates that a DX engine configuration is incomplete, inconsistent, or exceeds its bounds.
/// </summary>
public sealed class DxConfigurationValidationException : Exception
{
    /// <summary>Creates a configuration validation exception.</summary>
    public DxConfigurationValidationException() { }

    /// <summary>Creates a configuration validation exception.</summary>
    public DxConfigurationValidationException(string message) : base(message) { }

    /// <summary>Creates a configuration validation exception with an inner exception.</summary>
    public DxConfigurationValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Immutable engine-wide configuration and transfer status.
/// </summary>
public sealed record DxReferenceEngineSnapshot(
    long ConfigurationRevision,
    bool IsRunning,
    DateTimeOffset Timestamp,
    DxTransferSnapshot[] Connections);

/// <summary>
/// Single-process, bounded OPC DX reference transfer engine.
/// </summary>
public sealed class DxReferenceEngine : IAsyncDisposable
{
    private readonly Lock _syncRoot = new();
    private readonly SemaphoreSlim _configurationGate = new(1, 1);
    private readonly IDxConfigurationStore _store;
    private readonly IDxEndpointResolver _endpointResolver;
    private readonly IDxScheduler _scheduler;
    private readonly DxReferenceEngineOptions _options;
    private readonly ConcurrentDictionary<string, ConnectionRuntime> _runtimes =
        new(StringComparer.Ordinal);
    private readonly Queue<DxTransferDiagnostic> _diagnostics = new();
    private DxConfigurationSnapshot _configuration = DxConfigurationSnapshot.Empty;
    private bool _initialized;
    private bool _running;
    private bool _disposed;

    /// <summary>Creates a bounded reference engine.</summary>
    public DxReferenceEngine(
        IDxConfigurationStore store,
        IDxEndpointResolver endpointResolver,
        IDxScheduler? scheduler = null,
        DxReferenceEngineOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(endpointResolver);
        _store = store;
        _endpointResolver = endpointResolver;
        _scheduler = scheduler ?? SystemDxScheduler.Instance;
        _options = options ?? new DxReferenceEngineOptions();
        _options.Validate();
    }

    /// <summary>Loads the latest atomic revision and starts configured transfer loops.</summary>
    public async ValueTask StartAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await _configurationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_running)
            {
                return;
            }

            if (!_initialized)
            {
                var loaded = await _store.LoadAsync(cancellationToken).ConfigureAwait(false);
                ValidateConfiguration(loaded.Configuration);
                lock (_syncRoot)
                {
                    _configuration = InMemoryDxConfigurationStore.CloneSnapshot(loaded);
                    _initialized = true;
                }
            }

            lock (_syncRoot)
            {
                _running = true;
            }

            CreateRuntimes();
        }
        finally
        {
            _configurationGate.Release();
        }
    }

    /// <summary>
    /// Stops all transfer loops and waits for in-flight endpoint calls to cancel.
    /// Cancellation is honored while waiting to enter the stop operation; once stopping
    /// begins, cleanup completes so a later start cannot inherit stale runtimes.
    /// </summary>
    public async ValueTask StopAsync(CancellationToken cancellationToken = default)
    {
        await _configurationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_running)
            {
                return;
            }

            var runtimes = _runtimes.Values.ToArray();
            foreach (var runtime in runtimes)
            {
                runtime.RequestStop();
            }

            await WaitForRuntimesAsync(runtimes).ConfigureAwait(false);
            foreach (var runtime in runtimes)
            {
                runtime.Dispose();
            }

            _runtimes.Clear();
            lock (_syncRoot)
            {
                _running = false;
            }
        }
        finally
        {
            _configurationGate.Release();
        }
    }

    /// <summary>Gets a defensive copy of the current atomic configuration revision.</summary>
    public async ValueTask<DxConfigurationSnapshot> GetConfigurationAsync(
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        await EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
        lock (_syncRoot)
        {
            return InMemoryDxConfigurationStore.CloneSnapshot(_configuration);
        }
    }

    /// <summary>Adds or replaces a source-server definition.</summary>
    public ValueTask<DxConfigurationSnapshot> UpsertSourceServerAsync(
        DxSourceServer sourceServer,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceServer);
        return MutateAsync(
            configuration =>
            {
                var name = RequireName(sourceServer.Name, "Source server");
                var sources = configuration.SourceServers
                    .Where(candidate => !string.Equals(candidate.Name, name, StringComparison.Ordinal))
                    .Append(sourceServer)
                    .ToArray();
                return new DxConfiguration(sources, configuration.Connections);
            },
            expectedRevision,
            cancellationToken);
    }

    /// <summary>Deletes a source server that is not referenced by a connection.</summary>
    public ValueTask<DxConfigurationSnapshot> RemoveSourceServerAsync(
        string name,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return MutateAsync(
            configuration =>
            {
                if (configuration.Connections.Any(connection =>
                    string.Equals(connection.SourceServerName, name, StringComparison.Ordinal)))
                {
                    throw new DxConfigurationValidationException(
                        $"Source server '{name}' is referenced by a DX connection.");
                }

                var sources = configuration.SourceServers
                    .Where(candidate => !string.Equals(candidate.Name, name, StringComparison.Ordinal))
                    .ToArray();
                return new DxConfiguration(sources, configuration.Connections);
            },
            expectedRevision,
            cancellationToken);
    }

    /// <summary>Adds or replaces a source-to-target connection.</summary>
    public ValueTask<DxConfigurationSnapshot> UpsertConnectionAsync(
        DxConnection connection,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return MutateAsync(
            configuration =>
            {
                var name = RequireName(connection.Name, "Connection");
                var connections = configuration.Connections
                    .Where(candidate => !string.Equals(candidate.Name, name, StringComparison.Ordinal))
                    .Append(connection)
                    .ToArray();
                return new DxConfiguration(configuration.SourceServers, connections);
            },
            expectedRevision,
            cancellationToken);
    }

    /// <summary>Deletes a connection.</summary>
    public ValueTask<DxConfigurationSnapshot> RemoveConnectionAsync(
        string name,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return MutateAsync(
            configuration => new DxConfiguration(
                configuration.SourceServers,
                configuration.Connections
                    .Where(candidate => !string.Equals(candidate.Name, name, StringComparison.Ordinal))
                    .ToArray()),
            expectedRevision,
            cancellationToken);
    }

    /// <summary>Atomically enables or disables a configured connection.</summary>
    public ValueTask<DxConfigurationSnapshot> SetConnectionEnabledAsync(
        string name,
        bool enabled,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return MutateAsync(
            configuration =>
            {
                var found = false;
                var connections = new DxConnection[configuration.Connections.Length];
                for (var i = 0; i < connections.Length; i++)
                {
                    var connection = configuration.Connections[i];
                    if (string.Equals(connection.Name, name, StringComparison.Ordinal))
                    {
                        connection = connection.WithConnectivity(enabled);
                        found = true;
                    }

                    connections[i] = connection;
                }

                if (!found)
                {
                    throw new KeyNotFoundException($"DX connection '{name}' was not found.");
                }

                return new DxConfiguration(configuration.SourceServers, connections);
            },
            expectedRevision,
            cancellationToken);
    }

    /// <summary>Atomically clears all source servers and connections.</summary>
    public ValueTask<DxConfigurationSnapshot> ResetAsync(
        long expectedRevision,
        CancellationToken cancellationToken = default) =>
        MutateAsync(
            static _ => DxConfiguration.Empty,
            expectedRevision,
            cancellationToken);

    /// <summary>Gets an immutable point-in-time status snapshot.</summary>
    public DxReferenceEngineSnapshot GetStatusSnapshot()
    {
        ThrowIfDisposed();
        DxConfigurationSnapshot configuration;
        KeyValuePair<string, ConnectionRuntime>[] runtimes;
        bool running;
        lock (_syncRoot)
        {
            configuration = InMemoryDxConfigurationStore.CloneSnapshot(_configuration);
            runtimes = _runtimes.ToArray();
            running = _running;
        }

        var runtimeMap = runtimes.ToDictionary(
            static pair => pair.Key,
            static pair => pair.Value,
            StringComparer.Ordinal);
        var snapshots = new DxTransferSnapshot[configuration.Configuration.Connections.Length];
        for (var i = 0; i < snapshots.Length; i++)
        {
            var connection = configuration.Configuration.Connections[i];
            var name = RequireName(connection.Name, "Connection");
            snapshots[i] = runtimeMap.TryGetValue(name, out var runtime)
                ? runtime.GetSnapshot()
                : CreateInactiveSnapshot(
                    connection,
                    FindSource(configuration.Configuration, connection));
        }

        return new(
            configuration.Version,
            running,
            _scheduler.Clock.UtcNow,
            snapshots);
    }

    /// <summary>Gets retained diagnostics in emission order.</summary>
    public DxTransferDiagnostic[] GetDiagnostics()
    {
        ThrowIfDisposed();
        lock (_syncRoot)
        {
            return _diagnostics.ToArray();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await StopAsync().ConfigureAwait(false);
        _disposed = true;
        _configurationGate.Dispose();
    }

    private async ValueTask<DxConfigurationSnapshot> MutateAsync(
        Func<DxConfiguration, DxConfiguration> mutation,
        long expectedRevision,
        CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(mutation);
        ArgumentOutOfRangeException.ThrowIfNegative(expectedRevision);
        await _configurationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
            if (_configuration.Version != expectedRevision)
            {
                throw new DxConfigurationVersionException(
                    expectedRevision,
                    _configuration.Version);
            }

            var updated = mutation(_configuration.Configuration.Copy());
            ValidateConfiguration(updated);
            var wasRunning = _running;
            if (wasRunning)
            {
                await QuiesceRuntimesAsync().ConfigureAwait(false);
            }

            DxConfigurationSnapshot saved;
            try
            {
                saved = await _store.SaveAsync(
                    updated,
                    expectedRevision,
                    cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                if (wasRunning)
                {
                    CreateRuntimes();
                }

                throw;
            }

            lock (_syncRoot)
            {
                _configuration = InMemoryDxConfigurationStore.CloneSnapshot(saved);
            }

            if (wasRunning)
            {
                // Runtimes were quiesced before persistence, so no operation using
                // the old revision can write after this commit.
                CreateRuntimes();
            }

            return InMemoryDxConfigurationStore.CloneSnapshot(saved);
        }
        finally
        {
            _configurationGate.Release();
        }
    }

    private async ValueTask EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        await _configurationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await EnsureInitializedCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _configurationGate.Release();
        }
    }

    private async ValueTask EnsureInitializedCoreAsync(CancellationToken cancellationToken)
    {
        lock (_syncRoot)
        {
            if (_initialized)
            {
                return;
            }
        }

        var loaded = await _store.LoadAsync(cancellationToken).ConfigureAwait(false);
        ValidateConfiguration(loaded.Configuration);
        lock (_syncRoot)
        {
            _configuration = InMemoryDxConfigurationStore.CloneSnapshot(loaded);
            _initialized = true;
        }
    }

    private void ValidateConfiguration(DxConfiguration configuration)
    {
        if (configuration.SourceServers.Length > _options.MaximumSourceServers)
        {
            throw new DxConfigurationValidationException(
                $"DX configuration exceeds the {_options.MaximumSourceServers} source-server limit.");
        }

        if (configuration.Connections.Length > _options.MaximumConnections)
        {
            throw new DxConfigurationValidationException(
                $"DX configuration exceeds the {_options.MaximumConnections} connection limit.");
        }

        var sources = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in configuration.SourceServers)
        {
            ArgumentNullException.ThrowIfNull(source);
            var name = RequireName(source.Name, "Source server");
            if (!sources.Add(name))
            {
                throw new DxConfigurationValidationException(
                    $"Source server name '{name}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(source.ServerUrl))
            {
                throw new DxConfigurationValidationException(
                    $"Source server '{name}' does not define a server URL.");
            }
        }

        var connections = new HashSet<string>(StringComparer.Ordinal);
        foreach (var connection in configuration.Connections)
        {
            ArgumentNullException.ThrowIfNull(connection);
            var name = RequireName(connection.Name, "Connection");
            if (!connections.Add(name))
            {
                throw new DxConfigurationValidationException(
                    $"Connection name '{name}' is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(connection.SourceServerName) ||
                !sources.Contains(connection.SourceServerName))
            {
                throw new DxConfigurationValidationException(
                    $"Connection '{name}' references an unknown source server.");
            }

            if (string.IsNullOrWhiteSpace(connection.SourceItemName) ||
                string.IsNullOrWhiteSpace(connection.TargetItemName))
            {
                throw new DxConfigurationValidationException(
                    $"Connection '{name}' must define source and target item names.");
            }
            if (connection.DefaultOverridden == true &&
                !connection.DefaultOverrideValue.HasValue)
            {
                throw new DxConfigurationValidationException(
                    $"Connection '{name}' enables override without an override value.");
            }
            if (connection.EnableSubstituteValue == true &&
                !connection.SubstituteValue.HasValue)
            {
                throw new DxConfigurationValidationException(
                    $"Connection '{name}' enables substitution without a substitute value.");
            }

            _ = GetUpdateRate(connection);
            _ = GetQueueCapacity(connection);
        }
    }

    private TimeSpan GetUpdateRate(DxConnection connection)
    {
        var rate = connection.UpdateRateMilliseconds.HasValue
            ? TimeSpan.FromMilliseconds(connection.UpdateRateMilliseconds.Value)
            : _options.DefaultUpdateRate;
        if (rate <= TimeSpan.Zero || rate > _options.MaximumUpdateRate)
        {
            throw new DxConfigurationValidationException(
                $"Connection '{connection.Name}' update rate must be greater than zero and no more than {_options.MaximumUpdateRate}.");
        }

        return rate;
    }

    private int GetQueueCapacity(DxConnection connection)
    {
        var capacity = connection.SourceItemQueueSize ?? 1;
        if (capacity <= 0 || capacity > _options.MaximumQueueCapacity)
        {
            throw new DxConfigurationValidationException(
                $"Connection '{connection.Name}' queue capacity must be between 1 and {_options.MaximumQueueCapacity}.");
        }

        return capacity;
    }

    private void CreateRuntimes()
    {
        var configuration = _configuration.Configuration;
        foreach (var connection in configuration.Connections)
        {
            var name = RequireName(connection.Name, "Connection");
            var source = FindSource(configuration, connection);
            var enabled = IsEnabled(source, connection);
            var runtime = new ConnectionRuntime(
                connection,
                source,
                enabled,
                GetUpdateRate(connection),
                GetQueueCapacity(connection),
                _endpointResolver,
                _scheduler,
                _options,
                EmitDiagnostic);
            if (!_runtimes.TryAdd(name, runtime))
            {
                runtime.Dispose();
                throw new InvalidOperationException($"DX runtime '{name}' already exists.");
            }

            runtime.Start();
        }
    }

    private async ValueTask QuiesceRuntimesAsync()
    {
        var runtimes = _runtimes.Values.ToArray();
        foreach (var runtime in runtimes)
        {
            runtime.RequestStop();
        }

        await WaitForRuntimesAsync(runtimes).ConfigureAwait(false);
        foreach (var runtime in runtimes)
        {
            if (_runtimes.TryRemove(runtime.Name, out var removedRuntime))
            {
                removedRuntime.Dispose();
            }
        }
    }

    private async ValueTask WaitForRuntimesAsync(
        IReadOnlyCollection<ConnectionRuntime> runtimes)
    {
        foreach (var runtime in runtimes)
        {
            try
            {
                await runtime.Completion.ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                EmitDiagnostic(new DxTransferDiagnostic(
                    _scheduler.Clock.UtcNow,
                    DxDiagnosticSeverity.Critical,
                    DxTransferOperation.Lifecycle,
                    "DX_RUNTIME_CLEANUP_FAILED",
                    exception.Message,
                    runtime.Name,
                    OpcResultId.Fail));
            }
        }
    }

    private void EmitDiagnostic(DxTransferDiagnostic diagnostic)
    {
        lock (_syncRoot)
        {
            while (_diagnostics.Count >= _options.DiagnosticCapacity)
            {
                _diagnostics.Dequeue();
            }

            _diagnostics.Enqueue(diagnostic);
        }
    }

    private static DxSourceServer FindSource(
        DxConfiguration configuration,
        DxConnection connection) =>
        configuration.SourceServers.Single(source =>
            string.Equals(source.Name, connection.SourceServerName, StringComparison.Ordinal));

    private static bool IsEnabled(DxSourceServer source, DxConnection connection) =>
        source.DefaultConnected != false &&
        connection.DefaultSourceItemConnected != false &&
        connection.DefaultTargetItemConnected != false;

    private static DxTransferSnapshot CreateInactiveSnapshot(
        DxConnection connection,
        DxSourceServer source) =>
        new(
            RequireName(connection.Name, "Connection"),
            IsEnabled(source, connection) ? DxTransferState.Stopped : DxTransferState.Disabled,
            queueDepth: 0,
            queueCapacity: connection.SourceItemQueueSize ?? 1);

    private static string RequireName(string? name, string kind)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DxConfigurationValidationException($"{kind} name is required.");
        }

        return name;
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private sealed class ConnectionRuntime : IDisposable
    {
        private readonly Lock _syncRoot = new();
        private readonly IDxEndpointResolver _endpointResolver;
        private readonly IDxScheduler _scheduler;
        private readonly DxReferenceEngineOptions _options;
        private readonly Action<DxTransferDiagnostic> _emitDiagnostic;
        private readonly CancellationTokenSource _stop = new();
        private readonly MutableStatus _status;
        private readonly ConfigurationSignal _configurationSignal = new();
        private readonly DxConnection _connection;
        private readonly DxSourceServer _source;
        private readonly TimeSpan _updateRate;
        private readonly bool _enabled;
        private Task? _completion;

        public ConnectionRuntime(
            DxConnection connection,
            DxSourceServer source,
            bool enabled,
            TimeSpan updateRate,
            int queueCapacity,
            IDxEndpointResolver endpointResolver,
            IDxScheduler scheduler,
            DxReferenceEngineOptions options,
            Action<DxTransferDiagnostic> emitDiagnostic)
        {
            _connection = connection;
            _source = source;
            _enabled = enabled;
            _updateRate = updateRate;
            _endpointResolver = endpointResolver;
            _scheduler = scheduler;
            _options = options;
            _emitDiagnostic = emitDiagnostic;
            Name = RequireName(connection.Name, "Connection");
            _status = new MutableStatus(
                Name,
                enabled ? DxTransferState.Stopped : DxTransferState.Disabled,
                queueCapacity);
        }

        public string Name { get; }

        public Task Completion => _completion ?? Task.CompletedTask;

        public void Start()
        {
            lock (_syncRoot)
            {
                _completion ??= RunAsync(_stop.Token);
            }
        }

        public void RequestStop()
        {
            _stop.Cancel();
            lock (_syncRoot)
            {
                _configurationSignal.Pulse();
            }
        }

        public DxTransferSnapshot GetSnapshot() => _status.GetSnapshot();

        public void Dispose() => _stop.Dispose();

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            Emit(
                DxDiagnosticSeverity.Information,
                DxTransferOperation.Lifecycle,
                "DX_STARTED",
                "DX connection transfer started.");
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var configuration = CaptureConfiguration();
                    if (!configuration.Enabled)
                    {
                        _status.SetState(DxTransferState.Disabled);
                        await configuration.Signal.Task
                            .WaitAsync(cancellationToken)
                            .ConfigureAwait(false);
                        continue;
                    }

                    _status.SetState(DxTransferState.Starting);
                    var startingTimestamp = _scheduler.Clock.GetTimestamp();
                    try
                    {
                        await TransferAsync(configuration, cancellationToken).ConfigureAwait(false);
                        _status.TransferSucceeded();
                        var elapsed = _scheduler.Clock.GetElapsedTime(
                            startingTimestamp,
                            _scheduler.Clock.GetTimestamp());
                        var delay = elapsed >= configuration.UpdateRate
                            ? TimeSpan.Zero
                            : configuration.UpdateRate - elapsed;
                        _ = await DelayOrConfigurationChangeAsync(
                            delay,
                            configuration.Signal,
                            cancellationToken).ConfigureAwait(false);
                    }
                    catch (EndpointTransferException exception)
                    {
                        await HandleTransferFailureAsync(
                            configuration,
                            exception,
                            cancellationToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        var failure = new EndpointTransferException(
                            DxTransferOperation.Lifecycle,
                            EndpointRole.Source,
                            OpcResultId.Fail,
                            exception.Message,
                            exception);
                        await HandleTransferFailureAsync(
                            configuration,
                            failure,
                            cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _status.SetState(DxTransferState.Stopped);
            }
            finally
            {
                Emit(
                    DxDiagnosticSeverity.Information,
                    DxTransferOperation.Lifecycle,
                    "DX_STOPPED",
                    "DX connection transfer stopped.");
            }
        }

        private async ValueTask TransferAsync(
            RuntimeConfiguration configuration,
            CancellationToken cancellationToken)
        {
            DxDataValue sourceData;
            try
            {
                sourceData = await ReadSourceAsync(configuration, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (EndpointTransferException exception)
                when (exception.Role == EndpointRole.Source &&
                    TryCreateFallbackValue(configuration.Connection, out var fallback))
            {
                await WriteTargetAsync(configuration.Connection, fallback, cancellationToken)
                    .ConfigureAwait(false);
                throw;
            }

            var writeData = configuration.Connection.DefaultOverridden == true
                ? CreateOverrideValue(configuration.Connection, sourceData.Timestamp)
                : sourceData;
            await WriteTargetAsync(configuration.Connection, writeData, cancellationToken)
                .ConfigureAwait(false);
        }

        private async ValueTask<DxDataValue> ReadSourceAsync(
            RuntimeConfiguration configuration,
            CancellationToken cancellationToken)
        {
            IDxDaAdapter sourceAdapter;
            try
            {
                sourceAdapter = _endpointResolver.ResolveSource(configuration.Source) ??
                    throw new InvalidOperationException(
                        "The source endpoint resolver returned null.");
            }
            catch (Exception exception)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Read,
                    EndpointRole.Source,
                    OpcResultId.Fail,
                    exception.Message,
                    exception);
            }

            var sourceItem = new DxDataItem(
                RequireName(configuration.Connection.SourceItemName, "Source item"),
                configuration.Connection.SourceItemPath);
            IReadOnlyList<DxReadResult> reads;
            try
            {
                reads = await sourceAdapter.ReadAsync(
                    new[] { sourceItem },
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Read,
                    EndpointRole.Source,
                    OpcResultId.Fail,
                    exception.Message,
                    exception);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (reads.Count != 1)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Read,
                    EndpointRole.Source,
                    OpcResultId.Fail,
                    $"Source endpoint returned {reads.Count} results for one item.");
            }

            var sourceData = reads[0].Data;
            _status.ReadCompleted(sourceData, _scheduler.Clock.UtcNow);
            if (!sourceData.IsSuccess)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Read,
                    EndpointRole.Source,
                    sourceData.ErrorId,
                    sourceData.ErrorDiagnostic ?? "Source read failed.");
            }

            return sourceData;
        }

        private async ValueTask WriteTargetAsync(
            DxConnection connection,
            DxDataValue data,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            IDxDaAdapter targetAdapter;
            try
            {
                targetAdapter = _endpointResolver.ResolveTarget(connection) ??
                    throw new InvalidOperationException(
                        "The target endpoint resolver returned null.");
            }
            catch (Exception exception)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Write,
                    EndpointRole.Target,
                    OpcResultId.Fail,
                    exception.Message,
                    exception);
            }

            _status.SetQueueDepth(1);
            var targetItem = new DxDataItem(
                RequireName(connection.TargetItemName, "Target item"),
                connection.TargetItemPath);
            IReadOnlyList<DxWriteResult> writes;
            try
            {
                writes = await targetAdapter.WriteAsync(
                    new[]
                    {
                        new DxWriteRequest(
                            targetItem,
                            data.Value,
                            data.Quality,
                            data.Timestamp),
                    },
                    cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Write,
                    EndpointRole.Target,
                    OpcResultId.Fail,
                    exception.Message,
                    exception);
            }
            finally
            {
                _status.SetQueueDepth(0);
            }

            if (writes.Count != 1)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Write,
                    EndpointRole.Target,
                    OpcResultId.Fail,
                    $"Target endpoint returned {writes.Count} results for one item.");
            }

            var write = writes[0];
            _status.WriteCompleted(write, _scheduler.Clock.UtcNow);
            if (!write.IsSuccess)
            {
                throw new EndpointTransferException(
                    DxTransferOperation.Write,
                    EndpointRole.Target,
                    write.ErrorId,
                    write.ErrorDiagnostic ?? "Target write failed.");
            }
        }

        private static DxDataValue CreateOverrideValue(
            DxConnection connection,
            DateTimeOffset timestamp) =>
            new(
                connection.DefaultOverrideValue!.Value,
                timestamp,
                new DxQuality(DxQualityStatus.GoodLocalOverride),
                OpcResultId.Ok);

        private bool TryCreateFallbackValue(
            DxConnection connection,
            out DxDataValue value)
        {
            if (connection.DefaultOverridden == true &&
                connection.DefaultOverrideValue.HasValue)
            {
                value = CreateOverrideValue(connection, _scheduler.Clock.UtcNow);
                return true;
            }
            if (connection.EnableSubstituteValue == true &&
                connection.SubstituteValue.HasValue)
            {
                value = new DxDataValue(
                    connection.SubstituteValue.Value,
                    _scheduler.Clock.UtcNow,
                    new DxQuality(DxQualityStatus.UncertainLastUsableValue),
                    OpcResultId.Ok);
                return true;
            }

            value = default!;
            return false;
        }

        private async ValueTask HandleTransferFailureAsync(
            RuntimeConfiguration configuration,
            EndpointTransferException exception,
            CancellationToken cancellationToken)
        {
            var diagnostic = Emit(
                DxDiagnosticSeverity.Error,
                exception.Operation,
                exception.Role == EndpointRole.Source
                    ? "DX_SOURCE_FAILURE"
                    : "DX_TARGET_FAILURE",
                exception.Message,
                exception.ErrorId);
            var failures = _status.TransferFailed(diagnostic);
            await TryReconnectAsync(configuration, exception.Role, cancellationToken)
                .ConfigureAwait(false);
            var delay = GetRetryDelay(failures);
            _status.SetRetry(
                _scheduler.Clock.UtcNow.Add(delay),
                Emit(
                    DxDiagnosticSeverity.Warning,
                    DxTransferOperation.Retry,
                    "DX_RETRY_SCHEDULED",
                    $"Retry scheduled after {delay}.",
                    exception.ErrorId));
            _ = await DelayOrConfigurationChangeAsync(
                delay,
                configuration.Signal,
                cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask TryReconnectAsync(
            RuntimeConfiguration configuration,
            EndpointRole role,
            CancellationToken cancellationToken)
        {
            try
            {
                var adapter = role == EndpointRole.Source
                    ? _endpointResolver.ResolveSource(configuration.Source)
                    : _endpointResolver.ResolveTarget(configuration.Connection);
                _ = await adapter.GetHealthAsync(cancellationToken).ConfigureAwait(false);
                await adapter.ReconnectAsync(cancellationToken).ConfigureAwait(false);
                Emit(
                    DxDiagnosticSeverity.Information,
                    DxTransferOperation.Reconnect,
                    "DX_RECONNECTED",
                    $"{role} endpoint reconnect completed.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                Emit(
                    DxDiagnosticSeverity.Warning,
                    DxTransferOperation.Reconnect,
                    "DX_RECONNECT_FAILED",
                    exception.Message,
                    OpcResultId.Fail);
            }
        }

        private async ValueTask<bool> DelayOrConfigurationChangeAsync(
            TimeSpan delay,
            ConfigurationSignal signal,
            CancellationToken cancellationToken)
        {
            if (delay == TimeSpan.Zero)
            {
                await Task.Yield();
                return true;
            }

            using var delayCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var delayTask = _scheduler.DelayAsync(delay, delayCancellation.Token).AsTask();
            var completed = await Task.WhenAny(delayTask, signal.Task).ConfigureAwait(false);
            if (ReferenceEquals(completed, delayTask))
            {
                await delayTask.ConfigureAwait(false);
                return true;
            }

            await delayCancellation.CancelAsync().ConfigureAwait(false);
            try
            {
                await delayTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();
            return false;
        }

        private TimeSpan GetRetryDelay(int consecutiveFailures)
        {
            var exponent = Math.Min(consecutiveFailures - 1, 30);
            var multiplier = 1L << exponent;
            var ticks = _options.InitialRetryDelay.Ticks > _options.MaximumRetryDelay.Ticks / multiplier
                ? _options.MaximumRetryDelay.Ticks
                : _options.InitialRetryDelay.Ticks * multiplier;
            return TimeSpan.FromTicks(Math.Min(ticks, _options.MaximumRetryDelay.Ticks));
        }

        private RuntimeConfiguration CaptureConfiguration()
        {
            lock (_syncRoot)
            {
                return new(
                    _connection,
                    _source,
                    _enabled,
                    _updateRate,
                    _configurationSignal);
            }
        }

        private DxTransferDiagnostic Emit(
            DxDiagnosticSeverity severity,
            DxTransferOperation operation,
            string code,
            string message,
            OpcResultId? errorId = null)
        {
            var diagnostic = new DxTransferDiagnostic(
                _scheduler.Clock.UtcNow,
                severity,
                operation,
                code,
                message,
                Name,
                errorId);
            _status.SetDiagnostic(diagnostic);
            _emitDiagnostic(diagnostic);
            return diagnostic;
        }
    }

    private sealed record RuntimeConfiguration(
        DxConnection Connection,
        DxSourceServer Source,
        bool Enabled,
        TimeSpan UpdateRate,
        ConfigurationSignal Signal);

    private sealed class ConfigurationSignal
    {
        private readonly TaskCompletionSource _completion =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Task => _completion.Task;

        public void Pulse() => _completion.TrySetResult();
    }

    private sealed class MutableStatus
    {
        private readonly Lock _syncRoot = new();
        private readonly string _name;
        private DxTransferState _state;
        private int _queueDepth;
        private readonly int _queueCapacity;
        private long _readCount;
        private long _writeCount;
        private int _consecutiveFailures;
        private DateTimeOffset? _lastReadTimestamp;
        private DateTimeOffset? _lastWriteTimestamp;
        private DateTimeOffset? _nextRetryTimestamp;
        private DxDataValue? _lastSourceValue;
        private DxWriteResult? _lastWriteResult;
        private DxTransferDiagnostic? _lastDiagnostic;

        public MutableStatus(
            string name,
            DxTransferState state,
            int queueCapacity)
        {
            _name = name;
            _state = state;
            _queueCapacity = queueCapacity;
        }

        public void SetState(DxTransferState state)
        {
            lock (_syncRoot)
            {
                _state = state;
                if (state is not DxTransferState.RetryDelay)
                {
                    _nextRetryTimestamp = null;
                }
            }
        }

        public void SetQueueDepth(int depth)
        {
            lock (_syncRoot)
            {
                _queueDepth = depth;
            }
        }

        public void ReadCompleted(DxDataValue value, DateTimeOffset timestamp)
        {
            lock (_syncRoot)
            {
                _readCount++;
                _lastReadTimestamp = timestamp;
                _lastSourceValue = value;
            }
        }

        public void WriteCompleted(DxWriteResult result, DateTimeOffset timestamp)
        {
            lock (_syncRoot)
            {
                _writeCount++;
                _lastWriteTimestamp = timestamp;
                _lastWriteResult = result;
            }
        }

        public void TransferSucceeded()
        {
            lock (_syncRoot)
            {
                _state = DxTransferState.Running;
                _consecutiveFailures = 0;
                _nextRetryTimestamp = null;
            }
        }

        public int TransferFailed(DxTransferDiagnostic diagnostic)
        {
            lock (_syncRoot)
            {
                _consecutiveFailures++;
                _lastDiagnostic = diagnostic;
                return _consecutiveFailures;
            }
        }

        public void SetRetry(
            DateTimeOffset nextRetryTimestamp,
            DxTransferDiagnostic diagnostic)
        {
            lock (_syncRoot)
            {
                _state = DxTransferState.RetryDelay;
                _nextRetryTimestamp = nextRetryTimestamp;
                _lastDiagnostic = diagnostic;
            }
        }

        public void SetDiagnostic(DxTransferDiagnostic diagnostic)
        {
            lock (_syncRoot)
            {
                _lastDiagnostic = diagnostic;
            }
        }

        public DxTransferSnapshot GetSnapshot()
        {
            lock (_syncRoot)
            {
                return new(
                    _name,
                    _state,
                    _queueDepth,
                    _queueCapacity,
                    _readCount,
                    _writeCount,
                    consecutiveFailures: _consecutiveFailures,
                    lastReadTimestamp: _lastReadTimestamp,
                    lastWriteTimestamp: _lastWriteTimestamp,
                    nextRetryTimestamp: _nextRetryTimestamp,
                    lastSourceValue: _lastSourceValue,
                    lastWriteResult: _lastWriteResult,
                    lastDiagnostic: _lastDiagnostic);
            }
        }
    }

    private sealed class EndpointTransferException : Exception
    {
        public EndpointTransferException(
            DxTransferOperation operation,
            EndpointRole role,
            OpcResultId errorId,
            string message,
            Exception? innerException = null)
            : base(message, innerException)
        {
            Operation = operation;
            Role = role;
            ErrorId = errorId;
        }

        public DxTransferOperation Operation { get; }

        public EndpointRole Role { get; }

        public OpcResultId ErrorId { get; }
    }

    private enum EndpointRole
    {
        Source,
        Target,
    }
}
