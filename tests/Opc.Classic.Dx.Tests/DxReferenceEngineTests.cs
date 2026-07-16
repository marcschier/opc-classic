// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dx.Tests;

public sealed class DxReferenceEngineTests
{
    [Test]
    public async Task StartAsync_returns_when_synchronous_transfers_exceed_update_rate()
    {
        var scheduler = new ImmediateDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration(
                connection: CreateConnection(updateRateMilliseconds: 1))),
            source,
            target,
            scheduler);

        Task start = Task.Run(() => engine.StartAsync());
        await start.WaitAsync(
            TimeSpan.FromSeconds(2),
            TestContext.Current!.CancellationToken);
        await WaitUntilAsync(() => target.WriteCount > 1);

        await Assert.That(start.IsCompletedSuccessfully).IsTrue();
    }

    [Test]
    public async Task Transfer_PreservesValueTimestampAndBadQuality()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source")
        {
            ReadValue = new DxDataValue(
                OpcVariant.FromDouble(12.75),
                scheduler.UtcNow.AddSeconds(-1),
                new DxQuality(DxQualityStatus.BadSensorFailure, DxLimitStatus.High),
                OpcResultId.Ok),
        };
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => target.WriteCount == 1);

        var request = target.Writes.Single();
        var status = engine.GetStatusSnapshot().Connections.Single();
        await Assert.That(request.Value).IsEqualTo(source.ReadValue.Value);
        await Assert.That(request.Timestamp).IsEqualTo(source.ReadValue.Timestamp);
        await Assert.That(request.Quality).IsEqualTo(source.ReadValue.Quality);
        await Assert.That(status.State).IsEqualTo(DxTransferState.Running);
        await Assert.That(status.ReadCount).IsEqualTo(1);
        await Assert.That(status.WriteCount).IsEqualTo(1);
        await Assert.That(status.QueueDepth).IsEqualTo(0);
    }

    [Test]
    public async Task SourceFailure_PropagatesErrorReconnectsAndRetries()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        source.ReadValues.Enqueue(new DxDataValue(
            OpcVariant.Empty,
            scheduler.UtcNow,
            new DxQuality(DxQualityStatus.BadCommFailure),
            OpcResultId.Fail,
            "source unavailable"));
        source.ReadValues.Enqueue(FakeEndpoint.GoodValue);
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            source,
            target,
            scheduler,
            retryDelay: TimeSpan.FromMilliseconds(50));

        await engine.StartAsync();
        await WaitUntilAsync(() =>
            source.ReconnectCount == 1
            && engine.GetStatusSnapshot().Connections.Single().State
                == DxTransferState.RetryDelay);

        var failed = engine.GetStatusSnapshot().Connections.Single();
        await Assert.That(failed.State).IsEqualTo(DxTransferState.RetryDelay);
        await Assert.That(failed.LastSourceValue!.ErrorId).IsEqualTo(OpcResultId.Fail);
        await Assert.That(failed.ConsecutiveFailures).IsEqualTo(1);
        await Assert.That(target.WriteCount).IsEqualTo(0);

        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(50));
        await WaitUntilAsync(() => target.WriteCount == 1);

        var recovered = engine.GetStatusSnapshot().Connections.Single();
        await Assert.That(recovered.State).IsEqualTo(DxTransferState.Running);
        await Assert.That(recovered.ConsecutiveFailures).IsEqualTo(0);
        await Assert.That(source.HealthCount).IsEqualTo(1);
    }

    [Test]
    public async Task TargetFailure_PropagatesErrorAndReconnectsTarget()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        target.WriteResults.Enqueue(new DxWriteResult(
            new DxDataItem("Target"),
            OpcResultId.Fail,
            "target rejected value"));
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() =>
            target.ReconnectCount == 1
            && engine.GetStatusSnapshot().Connections.Single().State
                == DxTransferState.RetryDelay);

        var status = engine.GetStatusSnapshot().Connections.Single();
        await Assert.That(status.State).IsEqualTo(DxTransferState.RetryDelay);
        await Assert.That(status.LastWriteResult!.ErrorId).IsEqualTo(OpcResultId.Fail);
        await Assert.That(status.LastWriteResult.ErrorDiagnostic)
            .IsEqualTo("target rejected value");
        await Assert.That(source.ReconnectCount).IsEqualTo(0);
        await Assert.That(target.HealthCount).IsEqualTo(1);
    }

    [Test]
    public async Task DisabledConnection_DoesNotTransferAndCanBeEnabled()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        var configuration = CreateConfiguration(
            connection: CreateConnection(
                defaultSourceItemConnected: false,
                defaultTargetItemConnected: false));
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(configuration),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await Task.Yield();

        await Assert.That(source.ReadCount).IsEqualTo(0);
        await Assert.That(engine.GetStatusSnapshot().Connections.Single().State)
            .IsEqualTo(DxTransferState.Disabled);

        var enabled = await engine.SetConnectionEnabledAsync("C1", true, expectedRevision: 0);
        await WaitUntilAsync(() => target.WriteCount == 1);
        await engine.SetConnectionEnabledAsync("C1", false, enabled.Version);
        await WaitUntilAsync(() =>
            engine.GetStatusSnapshot().Connections.Single().State == DxTransferState.Disabled);

        scheduler.AdvanceBy(TimeSpan.FromSeconds(10));
        await Task.Yield();
        await Assert.That(target.WriteCount).IsEqualTo(1);
    }

    [Test]
    public async Task RevisedRate_WakesDelayAndUsesNewSchedule()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration(
                connection: CreateConnection(updateRateMilliseconds: 1000))),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => target.WriteCount == 1 && scheduler.PendingCount == 1);

        await engine.UpsertConnectionAsync(
            CreateConnection(updateRateMilliseconds: 100),
            expectedRevision: 0);
        await WaitUntilAsync(() => target.WriteCount == 2 && scheduler.PendingCount == 1);

        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(99));
        await Task.Yield();
        await Assert.That(target.WriteCount).IsEqualTo(2);

        scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1));
        await WaitUntilAsync(() => target.WriteCount == 3);
    }

    [Test]
    public async Task ConfigurationMutation_DoesNotOverlapConnectionTransfer()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target")
        {
            WriteGate = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously),
        };
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => target.ActiveWrites == 1);

        await engine.UpsertConnectionAsync(
            CreateConnection(updateRateMilliseconds: 10),
            expectedRevision: 0);
        await WaitUntilAsync(() => target.ActiveWrites == 1);

        await Assert.That(target.MaximumActiveWrites).IsEqualTo(1);
        await Assert.That(target.WriteCount).IsEqualTo(2);

        target.WriteGate.SetResult();
        await WaitUntilAsync(() =>
            engine.GetStatusSnapshot().Connections.Single().State == DxTransferState.Running);
        await Assert.That(target.MaximumActiveWrites).IsEqualTo(1);
    }

    [Test]
    public async Task TargetReconfiguration_CancelsAndAwaitsOldIterationBeforeCommit()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source")
        {
            ReadGate = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously),
            IgnoreReadCancellation = true,
        };
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration(
                connection: CreateConnection(targetItemName: "OldTarget"))),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => source.ActiveReads == 1);
        var mutation = engine.UpsertConnectionAsync(
            CreateConnection(targetItemName: "NewTarget"),
            expectedRevision: 0).AsTask();
        await WaitUntilAsync(() => source.ReadCancellationObserved);

        await Assert.That(mutation.IsCompleted).IsFalse();
        source.ReadGate.SetResult();
        await mutation;
        await WaitUntilAsync(() => target.WriteCount == 1);

        await Assert.That(target.Writes.All(write =>
                write.Item.ItemName == "NewTarget"))
            .IsTrue();
    }

    [Test]
    public async Task Disable_CancelsAndAwaitsOldIterationWithoutStaleWrite()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source")
        {
            ReadGate = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously),
            IgnoreReadCancellation = true,
        };
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => source.ActiveReads == 1);
        var disable = engine.SetConnectionEnabledAsync(
            "C1",
            false,
            expectedRevision: 0).AsTask();
        await WaitUntilAsync(() => source.ReadCancellationObserved);

        await Assert.That(disable.IsCompleted).IsFalse();
        source.ReadGate.SetResult();
        var saved = await disable;

        await Assert.That(target.WriteCount).IsEqualTo(0);
        await Assert.That(engine.GetStatusSnapshot().Connections.Single().State)
            .IsEqualTo(DxTransferState.Disabled);
        await Assert.That(saved.Version).IsEqualTo(1);
    }

    [Test]
    public async Task ResolverFailures_AreAttributedAndReconnectCorrectEndpoint()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        var sourceResolver = new FakeResolver(source, target)
        {
            SourceFailuresRemaining = 1,
        };
        await using (var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            sourceResolver,
            scheduler))
        {
            await engine.StartAsync();
            await WaitUntilAsync(() => source.ReconnectCount == 1);

            await Assert.That(sourceResolver.TargetResolutionCount).IsEqualTo(0);
            await Assert.That(target.ReconnectCount).IsEqualTo(0);
            await Assert.That(engine.GetDiagnostics().Any(diagnostic =>
                    diagnostic.Code == "DX_SOURCE_FAILURE"))
                .IsTrue();
        }

        source = new FakeEndpoint("source");
        target = new FakeEndpoint("target");
        var targetResolver = new FakeResolver(source, target)
        {
            TargetFailuresRemaining = 1,
        };
        await using (var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            targetResolver,
            scheduler))
        {
            await engine.StartAsync();
            await WaitUntilAsync(() => target.ReconnectCount == 1);

            await Assert.That(source.ReconnectCount).IsEqualTo(0);
            await Assert.That(engine.GetDiagnostics().Any(diagnostic =>
                    diagnostic.Code == "DX_TARGET_FAILURE"))
                .IsTrue();
        }
    }

    [Test]
    public async Task Override_WritesConfiguredValueWithLocalOverrideQuality()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        var connection = CreateConnection(
            defaultOverridden: true,
            defaultOverrideValue: OpcVariant.FromInt32(99));
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration(connection)),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => target.WriteCount == 1);

        var write = target.Writes.Single();
        await Assert.That(source.ReadCount).IsEqualTo(1);
        await Assert.That(write.Value).IsEqualTo(OpcVariant.FromInt32(99));
        await Assert.That(write.Quality.Quality)
            .IsEqualTo(DxQualityStatus.GoodLocalOverride);
    }

    [Test]
    public async Task Substitute_WritesConfiguredValueOnSourceFailureAndRetriesSource()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        source.ReadValues.Enqueue(new DxDataValue(
            OpcVariant.Empty,
            scheduler.UtcNow,
            new DxQuality(DxQualityStatus.BadCommFailure),
            OpcResultId.Fail,
            "offline"));
        var target = new FakeEndpoint("target");
        var connection = CreateConnection(
            enableSubstituteValue: true,
            substituteValue: OpcVariant.FromString("fallback"));
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration(connection)),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() =>
            target.WriteCount == 1
            && source.ReconnectCount == 1
            && engine.GetStatusSnapshot().Connections.Single().State
                == DxTransferState.RetryDelay);

        var write = target.Writes.Single();
        await Assert.That(write.Value).IsEqualTo(OpcVariant.FromString("fallback"));
        await Assert.That(write.Quality.Quality)
            .IsEqualTo(DxQualityStatus.UncertainLastUsableValue);
        await Assert.That(target.ReconnectCount).IsEqualTo(0);
        await Assert.That(engine.GetStatusSnapshot().Connections.Single().State)
            .IsEqualTo(DxTransferState.RetryDelay);
    }

    [Test]
    public async Task CommittedMutation_ReconcilesAfterCallerCancellation()
    {
        var scheduler = new ManualDxScheduler();
        using var callerCancellation = new CancellationTokenSource();
        using var store = new CancelAfterCommitStore(
            CreateConfiguration(),
            callerCancellation);
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(store, source, target, scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => target.WriteCount == 1 && scheduler.PendingCount == 1);

        var saved = await engine.RemoveConnectionAsync(
            "C1",
            expectedRevision: 0,
            callerCancellation.Token);

        await Assert.That(callerCancellation.IsCancellationRequested).IsTrue();
        await Assert.That(saved.Version).IsEqualTo(1);
        await Assert.That(engine.GetStatusSnapshot().Connections).IsEmpty();
        await Assert.That(scheduler.PendingCount).IsEqualTo(0);
    }

    [Test]
    public async Task Stop_CancelsInFlightEndpointOperation()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source")
        {
            ReadGate = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously),
        };
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            source,
            target,
            scheduler);

        await engine.StartAsync();
        await WaitUntilAsync(() => source.ActiveReads == 1);
        await engine.StopAsync();

        await Assert.That(source.CanceledReadCount).IsEqualTo(1);
        await Assert.That(engine.GetStatusSnapshot().IsRunning).IsFalse();
        await Assert.That(engine.GetStatusSnapshot().Connections.Single().State)
            .IsEqualTo(DxTransferState.Stopped);
    }

    [Test]
    public async Task Stop_CallerCancellationAfterEntry_CompletesCleanupAndCanRestart()
    {
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source")
        {
            ReadGate = new TaskCompletionSource(
                TaskCreationOptions.RunContinuationsAsynchronously),
            IgnoreReadCancellation = true,
        };
        var target = new FakeEndpoint("target");
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration()),
            source,
            target,
            scheduler);
        using var callerCancellation = new CancellationTokenSource();

        await engine.StartAsync();
        await WaitUntilAsync(() => source.ActiveReads == 1);
        var stop = engine.StopAsync(callerCancellation.Token).AsTask();
        await WaitUntilAsync(() => source.ReadCancellationObserved);

        await Assert.That(engine.GetStatusSnapshot().IsRunning).IsTrue();
        callerCancellation.Cancel();
        source.ReadGate.SetResult();
        await stop;

        await Assert.That(engine.GetStatusSnapshot().IsRunning).IsFalse();
        var writesBeforeRestart = target.WriteCount;
        await engine.StartAsync();
        await WaitUntilAsync(() => target.WriteCount > writesBeforeRestart);
        await Assert.That(engine.GetStatusSnapshot().IsRunning).IsTrue();
    }

    [Test]
    public async Task JsonPersistence_RecoversRevisionAndConfigurationAfterRestart()
    {
        using var directory = new TestDirectory();
        var path = directory.File("engine.json");
        var scheduler = new ManualDxScheduler();
        var source = new FakeEndpoint("source");
        var target = new FakeEndpoint("target");
        long revision;
        await using (var first = CreateEngine(
            new JsonFileDxConfigurationStore(path),
            source,
            target,
            scheduler))
        {
            var sourceRevision = await first.UpsertSourceServerAsync(
                CreateSource(),
                expectedRevision: 0);
            var connectionRevision = await first.UpsertConnectionAsync(
                CreateConnection(),
                sourceRevision.Version);
            revision = connectionRevision.Version;
        }

        await using var second = CreateEngine(
            new JsonFileDxConfigurationStore(path),
            source,
            target,
            scheduler);
        await second.StartAsync();
        await WaitUntilAsync(() => target.WriteCount == 1);

        var recovered = await second.GetConfigurationAsync();
        await Assert.That(recovered.Version).IsEqualTo(revision);
        await Assert.That(recovered.Configuration.SourceServers.Single().Name)
            .IsEqualTo("S1");
        await Assert.That(recovered.Configuration.Connections.Single().Name)
            .IsEqualTo("C1");
    }

    [Test]
    public async Task CorruptPersistence_PreventsEngineStartup()
    {
        using var directory = new TestDirectory();
        var path = directory.File("engine.json");
        await File.WriteAllTextAsync(path, "{not-json");
        await using var engine = CreateEngine(
            new JsonFileDxConfigurationStore(path),
            new FakeEndpoint("source"),
            new FakeEndpoint("target"),
            new ManualDxScheduler());

        await Assert.That(async () => await engine.StartAsync())
            .Throws<DxConfigurationCorruptException>();
        await Assert.That(engine.GetStatusSnapshot().IsRunning).IsFalse();
    }

    [Test]
    public async Task ConcurrentConfigurationMutation_CommitsExactlyOneRevision()
    {
        var store = new InMemoryDxConfigurationStore(
            new DxConfiguration(new[] { CreateSource() }));
        await using var engine = CreateEngine(
            store,
            new FakeEndpoint("source"),
            new FakeEndpoint("target"),
            new ManualDxScheduler());
        var first = CaptureAsync(engine.UpsertConnectionAsync(
            CreateConnection(name: "C1"),
            expectedRevision: 0));
        var second = CaptureAsync(engine.UpsertConnectionAsync(
            CreateConnection(name: "C2"),
            expectedRevision: 0));

        var results = await Task.WhenAll(first, second);
        var configuration = await engine.GetConfigurationAsync();

        await Assert.That(results.Count(result => result.Exception is null)).IsEqualTo(1);
        await Assert.That(results.Count(result =>
                result.Exception is DxConfigurationVersionException))
            .IsEqualTo(1);
        await Assert.That(configuration.Version).IsEqualTo(1);
        await Assert.That(configuration.Configuration.Connections.Length).IsEqualTo(1);
    }

    [Test]
    public async Task CrudValidationAndReset_AreAtomic()
    {
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(),
            new FakeEndpoint("source"),
            new FakeEndpoint("target"),
            new ManualDxScheduler());

        await Assert.That(async () =>
                await engine.UpsertConnectionAsync(CreateConnection(), expectedRevision: 0))
            .Throws<DxConfigurationValidationException>();

        var source = await engine.UpsertSourceServerAsync(CreateSource(), expectedRevision: 0);
        var connection = await engine.UpsertConnectionAsync(CreateConnection(), source.Version);
        await Assert.That(async () =>
                await engine.RemoveSourceServerAsync("S1", connection.Version))
            .Throws<DxConfigurationValidationException>();

        var reset = await engine.ResetAsync(connection.Version);
        await Assert.That(reset.Configuration.SourceServers).IsEmpty();
        await Assert.That(reset.Configuration.Connections).IsEmpty();
        await Assert.That(reset.Version).IsEqualTo(3);
    }

    [Test]
    public async Task SetConnectionEnabled_RecomputesConnectivityMask()
    {
        var connection = CreateConnection(
            defaultSourceItemConnected: null,
            defaultTargetItemConnected: null);
        await using var engine = CreateEngine(
            new InMemoryDxConfigurationStore(CreateConfiguration(connection)),
            new FakeEndpoint("source"),
            new FakeEndpoint("target"),
            new ManualDxScheduler());

        var disabled = await engine.SetConnectionEnabledAsync(
            "C1",
            false,
            expectedRevision: 0);
        var disabledMask = (DxMask)disabled.Configuration.Connections.Single().Mask;
        await Assert.That(disabledMask.HasFlag(DxMask.DefaultSourceItemConnected)).IsTrue();
        await Assert.That(disabledMask.HasFlag(DxMask.DefaultTargetItemConnected)).IsTrue();
        await Assert.That(disabledMask.HasFlag(DxMask.SourceItemName)).IsTrue();
        await Assert.That(disabledMask.HasFlag(DxMask.TargetItemName)).IsTrue();

        var enabled = await engine.SetConnectionEnabledAsync(
            "C1",
            true,
            disabled.Version);
        var enabledConnection = enabled.Configuration.Connections.Single();
        var enabledMask = (DxMask)enabledConnection.Mask;
        await Assert.That(enabledConnection.DefaultSourceItemConnected).IsTrue();
        await Assert.That(enabledConnection.DefaultTargetItemConnected).IsTrue();
        await Assert.That(enabledMask.HasFlag(DxMask.DefaultSourceItemConnected)).IsTrue();
        await Assert.That(enabledMask.HasFlag(DxMask.DefaultTargetItemConnected)).IsTrue();
    }

    private static DxReferenceEngine CreateEngine(
        IDxConfigurationStore store,
        FakeEndpoint source,
        FakeEndpoint target,
        IDxScheduler scheduler,
        TimeSpan? retryDelay = null) =>
        new(
            store,
            new FakeResolver(source, target),
            scheduler,
            new DxReferenceEngineOptions
            {
                InitialRetryDelay = retryDelay ?? TimeSpan.FromMilliseconds(100),
                MaximumRetryDelay = TimeSpan.FromSeconds(1),
            });

    private static DxReferenceEngine CreateEngine(
        IDxConfigurationStore store,
        IDxEndpointResolver resolver,
        IDxScheduler scheduler) =>
        new(
            store,
            resolver,
            scheduler,
            new DxReferenceEngineOptions
            {
                InitialRetryDelay = TimeSpan.FromMilliseconds(100),
                MaximumRetryDelay = TimeSpan.FromSeconds(1),
            });

    private static DxConfiguration CreateConfiguration(
        DxConnection? connection = null) =>
        new(
            new[] { CreateSource() },
            new[] { connection ?? CreateConnection() });

    private static DxSourceServer CreateSource() =>
        new("S1", "opcda://source/Vendor.Server", defaultConnected: true);

    private static DxConnection CreateConnection(
        string name = "C1",
        int updateRateMilliseconds = 1000,
        bool? defaultSourceItemConnected = true,
        bool? defaultTargetItemConnected = true,
        bool? defaultOverridden = null,
        OpcVariant? defaultOverrideValue = null,
        OpcVariant? substituteValue = null,
        bool? enableSubstituteValue = null,
        string targetItemName = "Target") =>
        new(
            name,
            defaultOverridden: defaultOverridden,
            defaultOverrideValue: defaultOverrideValue,
            substituteValue: substituteValue,
            enableSubstituteValue: enableSubstituteValue,
            sourceServerName: "S1",
            sourceItemName: "Source",
            targetItemName: targetItemName,
            sourceItemQueueSize: 1,
            updateRateMilliseconds: updateRateMilliseconds,
            defaultSourceItemConnected: defaultSourceItemConnected,
            defaultTargetItemConnected: defaultTargetItemConnected);

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 1000; attempt++)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(1);
        }

        throw new TimeoutException("The deterministic engine condition was not reached.");
    }

    private static async Task<MutationResult> CaptureAsync(
        ValueTask<DxConfigurationSnapshot> mutation)
    {
        try
        {
            return new MutationResult(await mutation, null);
        }
        catch (Exception exception)
        {
            return new MutationResult(null, exception);
        }
    }

    private sealed record MutationResult(
        DxConfigurationSnapshot? Snapshot,
        Exception? Exception);

    private sealed class FakeResolver : IDxEndpointResolver
    {
        private readonly FakeEndpoint _source;
        private readonly FakeEndpoint _target;
        private int _sourceResolutionCount;
        private int _targetResolutionCount;

        public FakeResolver(FakeEndpoint source, FakeEndpoint target)
        {
            _source = source;
            _target = target;
        }

        public int SourceFailuresRemaining { get; set; }

        public int TargetFailuresRemaining { get; set; }

        public int SourceResolutionCount => Volatile.Read(ref _sourceResolutionCount);

        public int TargetResolutionCount => Volatile.Read(ref _targetResolutionCount);

        public IDxDaAdapter ResolveSource(DxSourceServer sourceServer)
        {
            Interlocked.Increment(ref _sourceResolutionCount);
            if (SourceFailuresRemaining > 0)
            {
                SourceFailuresRemaining--;
                throw new InvalidOperationException("source resolver failed");
            }

            return _source;
        }

        public IDxDaAdapter ResolveTarget(DxConnection connection)
        {
            Interlocked.Increment(ref _targetResolutionCount);
            if (TargetFailuresRemaining > 0)
            {
                TargetFailuresRemaining--;
                throw new InvalidOperationException("target resolver failed");
            }

            return _target;
        }
    }

    private sealed class FakeEndpoint(string name) : IDxDaAdapter
    {
        private int _activeReads;
        private int _activeWrites;
        private int _canceledReadCount;
        private int _healthCount;
        private int _maximumActiveWrites;
        private int _readCount;
        private int _readCancellationObserved;
        private int _reconnectCount;
        private int _writeCount;

        public static DxDataValue GoodValue { get; } = new(
            OpcVariant.FromInt32(42),
            DateTimeOffset.UnixEpoch,
            new DxQuality(DxQualityStatus.Good),
            OpcResultId.Ok);

        public string Name { get; } = name;

        public ConcurrentQueue<DxDataValue> ReadValues { get; } = new();

        public ConcurrentQueue<DxWriteResult> WriteResults { get; } = new();

        public ConcurrentQueue<DxWriteRequest> Writes { get; } = new();

        public DxDataValue ReadValue { get; set; } = GoodValue;

        public TaskCompletionSource? ReadGate { get; init; }

        public bool IgnoreReadCancellation { get; init; }

        public TaskCompletionSource? WriteGate { get; init; }

        public int ActiveReads => Volatile.Read(ref _activeReads);

        public int ActiveWrites => Volatile.Read(ref _activeWrites);

        public int CanceledReadCount => Volatile.Read(ref _canceledReadCount);

        public int HealthCount => Volatile.Read(ref _healthCount);

        public int MaximumActiveWrites => Volatile.Read(ref _maximumActiveWrites);

        public int ReadCount => Volatile.Read(ref _readCount);

        public bool ReadCancellationObserved =>
            Volatile.Read(ref _readCancellationObserved) != 0;

        public int ReconnectCount => Volatile.Read(ref _reconnectCount);

        public int WriteCount => Volatile.Read(ref _writeCount);

        public ValueTask<DxEndpointHealth> GetHealthAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _healthCount);
            return ValueTask.FromResult(new DxEndpointHealth(
                DxEndpointHealthState.Disconnected,
                DateTimeOffset.UnixEpoch,
                OpcResultId.Fail));
        }

        public ValueTask ReconnectAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Interlocked.Increment(ref _reconnectCount);
            return ValueTask.CompletedTask;
        }

        public async ValueTask<IReadOnlyList<DxReadResult>> ReadAsync(
            IReadOnlyList<DxDataItem> items,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _readCount);
            Interlocked.Increment(ref _activeReads);
            try
            {
                if (ReadGate is not null)
                {
                    try
                    {
                        if (IgnoreReadCancellation)
                        {
                            using var registration = cancellationToken.UnsafeRegister(
                                static state =>
                                    Interlocked.Exchange(ref ((FakeEndpoint)state!)._readCancellationObserved, 1),
                                this);
                            await ReadGate.Task;
                        }
                        else
                        {
                            await ReadGate.Task.WaitAsync(cancellationToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Interlocked.Increment(ref _canceledReadCount);
                        throw;
                    }
                }

                var value = ReadValues.TryDequeue(out var queued) ? queued : ReadValue;
                return new[] { new DxReadResult(items[0], value) };
            }
            finally
            {
                Interlocked.Decrement(ref _activeReads);
            }
        }

        public async ValueTask<IReadOnlyList<DxWriteResult>> WriteAsync(
            IReadOnlyList<DxWriteRequest> requests,
            CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _writeCount);
            var active = Interlocked.Increment(ref _activeWrites);
            UpdateMaximum(ref _maximumActiveWrites, active);
            Writes.Enqueue(requests[0]);
            try
            {
                if (WriteGate is not null)
                {
                    await WriteGate.Task.WaitAsync(cancellationToken);
                }

                var result = WriteResults.TryDequeue(out var queued)
                    ? queued
                    : new DxWriteResult(requests[0].Item, OpcResultId.Ok);
                return new[] { result };
            }
            finally
            {
                Interlocked.Decrement(ref _activeWrites);
            }
        }

        private static void UpdateMaximum(ref int location, int value)
        {
            while (true)
            {
                var current = Volatile.Read(ref location);
                if (current >= value ||
                    Interlocked.CompareExchange(ref location, value, current) == current)
                {
                    return;
                }
            }
        }
    }

    private sealed class ImmediateDxScheduler : IDxScheduler, IDxClock
    {
        private long _timestamp;

        public IDxClock Clock => this;

        public DateTimeOffset UtcNow => DateTimeOffset.UnixEpoch;

        public long GetTimestamp() => Interlocked.Increment(ref _timestamp);

        public TimeSpan GetElapsedTime(
            long startingTimestamp,
            long endingTimestamp)
        {
            _ = startingTimestamp;
            _ = endingTimestamp;
            return TimeSpan.FromMilliseconds(2);
        }

        public ValueTask DelayAsync(
            TimeSpan delay,
            CancellationToken cancellationToken = default)
        {
            _ = delay;
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class CancelAfterCommitStore(
        DxConfiguration initialConfiguration,
        CancellationTokenSource cancellation) : IDxConfigurationStore, IDisposable
    {
        private readonly InMemoryDxConfigurationStore _inner =
            new(initialConfiguration);

        public ValueTask<DxConfigurationSnapshot> LoadAsync(
            CancellationToken cancellationToken = default) =>
            _inner.LoadAsync(cancellationToken);

        public async ValueTask<DxConfigurationSnapshot> SaveAsync(
            DxConfiguration configuration,
            long expectedVersion,
            CancellationToken cancellationToken = default)
        {
            var saved = await _inner.SaveAsync(
                configuration,
                expectedVersion,
                cancellationToken);
            cancellation.Cancel();
            return saved;
        }

        public void Dispose() => _inner.Dispose();
    }

    private sealed class TestDirectory : IDisposable
    {
        public TestDirectory()
        {
            Path = System.IO.Path.Combine(
                AppContext.BaseDirectory,
                "dx-engine-tests",
                Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string File(string name) => System.IO.Path.Combine(Path, name);

        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
