// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureSessionManagerTests
{
    [Test]
    public async Task Constructor_InvalidArguments_Throw()
    {
        await Assert.That(() => new CaptureSessionManager(null!)).Throws<ArgumentNullException>();
        await Assert.That(() => new CaptureSessionManager(string.Empty)).Throws<ArgumentException>();
        await Assert.That(() => new CaptureSessionManager("scratch", maxActiveSessions: 0)).Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => new CaptureSessionManager("scratch", maxActiveSessions: 2, maxRetainedSessions: 1))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task CreateAndStartAsync_RegistersRunningSessionAndTryGetTouchesIt()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root, maxActiveSessions: 2, maxRetainedSessions: 4);
            var source = new FakeCaptureSource();
            var request = new CaptureStartRequest(InterfaceName: "eth0");

            CaptureSession session = await manager.CreateAndStartAsync(
                "fake",
                _ => source,
                request,
                TestContext.Current!.CancellationToken);
            DateTimeOffset beforeTouch = session.LastTouchedAt;
            bool found = manager.TryGet(session.Id, out CaptureSession foundSession);

            await Assert.That(found).IsTrue();
            await Assert.That(foundSession).IsEqualTo(session);
            await Assert.That(foundSession.State).IsEqualTo(CaptureSessionState.Running);
            await Assert.That(foundSession.SourceName).IsEqualTo("fake");
            await Assert.That(foundSession.Request).IsEqualTo(request);
            await Assert.That(manager.Count).IsEqualTo(1);
            await Assert.That(manager.ActiveCount).IsEqualTo(1);
            await Assert.That(source.StartCallCount).IsEqualTo(1);
            await Assert.That(foundSession.LastTouchedAt >= beforeTouch).IsTrue();
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task CreateAndStartAsync_ArgumentValidation_Throws()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root);
            Func<string, ICaptureSource> factory = _ => new FakeCaptureSource();
            var request = new CaptureStartRequest();

            await Assert.That(async () => await manager.CreateAndStartAsync(null!, factory, request, CancellationToken.None))
                .Throws<ArgumentNullException>();
            await Assert.That(async () => await manager.CreateAndStartAsync(string.Empty, factory, request, CancellationToken.None))
                .Throws<ArgumentException>();
            await Assert.That(async () => await manager.CreateAndStartAsync("fake", null!, request, CancellationToken.None))
                .Throws<ArgumentNullException>();
            await Assert.That(async () => await manager.CreateAndStartAsync("fake", factory, null!, CancellationToken.None))
                .Throws<ArgumentNullException>();
            await Assert.That(() => manager.TryGet(null!, out _)).Throws<ArgumentNullException>();
            await Assert.That(() => manager.TryGet(string.Empty, out _)).Throws<ArgumentException>();
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task CreateAndStartAsync_ActiveCapExceeded_ThrowsCaptureException()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root, maxActiveSessions: 1, maxRetainedSessions: 2);
            await manager.CreateAndStartAsync("fake", _ => new FakeCaptureSource(), new CaptureStartRequest(), CancellationToken.None);

            await Assert.That(async () => await manager.CreateAndStartAsync(
                    "fake",
                    _ => new FakeCaptureSource(),
                    new CaptureStartRequest(),
                    CancellationToken.None))
                .Throws<CaptureException>();
            await Assert.That(manager.Count).IsEqualTo(1);
            await Assert.That(manager.ActiveCount).IsEqualTo(1);
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task NaturalCompletion_ReleasesActiveCapacity()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(
                root,
                maxActiveSessions: 1,
                maxRetainedSessions: 2);
            var firstSource = new FakeCaptureSource();
            CaptureSession first = await manager.CreateAndStartAsync(
                "fake",
                _ => firstSource,
                new CaptureStartRequest(),
                CancellationToken.None);

            firstSource.CompleteNaturally();
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            while (first.State != CaptureSessionState.Completed)
            {
                await Task.Delay(10, timeout.Token);
            }

            CaptureSession second = await manager.CreateAndStartAsync(
                "fake",
                _ => new FakeCaptureSource(),
                new CaptureStartRequest(),
                CancellationToken.None);

            await Assert.That(manager.ActiveCount).IsEqualTo(1);
            await Assert.That(second.State).IsEqualTo(CaptureSessionState.Running);
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task RemoveAsync_ExistingSession_StopsDisposesAndRemovesIt()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root, maxActiveSessions: 2, maxRetainedSessions: 4);
            var source = new FakeCaptureSource();
            CaptureSession session = await manager.CreateAndStartAsync("fake", _ => source, new CaptureStartRequest(), CancellationToken.None);

            bool removed = await manager.RemoveAsync(session.Id, CancellationToken.None);
            bool removedAgain = await manager.RemoveAsync(session.Id, CancellationToken.None);

            await Assert.That(removed).IsTrue();
            await Assert.That(removedAgain).IsFalse();
            await Assert.That(manager.Count).IsEqualTo(0);
            await Assert.That(source.StopCallCount).IsEqualTo(1);
            await Assert.That(source.DisposeCallCount).IsEqualTo(1);
            await Assert.That(session.State).IsEqualTo(CaptureSessionState.Disposed);
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task CreateAndStartAsync_StartFailure_RemovesAndDisposesSession()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root, maxActiveSessions: 2, maxRetainedSessions: 4);
            var source = new FakeCaptureSource
            {
                StartException = new CaptureException("start failed"),
            };

            await Assert.That(async () => await manager.CreateAndStartAsync(
                    "fake",
                    _ => source,
                    new CaptureStartRequest(),
                    CancellationToken.None))
                .Throws<CaptureException>();

            await Assert.That(manager.Count).IsEqualTo(0);
            await Assert.That(source.DisposeCallCount).IsEqualTo(1);
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task CreateAndStartAsync_RetentionCapEvictsOldestCompletedSession()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root, maxActiveSessions: 2, maxRetainedSessions: 2);
            var sources = new Queue<FakeCaptureSource>([
                new FakeCaptureSource(),
                new FakeCaptureSource(),
                new FakeCaptureSource(),
            ]);
            CaptureSession first = await manager.CreateAndStartAsync("fake", _ => sources.Dequeue(), new CaptureStartRequest(), CancellationToken.None);
            await first.StopAsync(CancellationToken.None);
            CaptureSession second = await manager.CreateAndStartAsync("fake", _ => sources.Dequeue(), new CaptureStartRequest(), CancellationToken.None);
            await second.StopAsync(CancellationToken.None);

            CaptureSession third = await manager.CreateAndStartAsync("fake", _ => sources.Dequeue(), new CaptureStartRequest(), CancellationToken.None);

            await Assert.That(manager.Count).IsEqualTo(2);
            await Assert.That(manager.TryGet(first.Id, out _)).IsFalse();
            await Assert.That(manager.TryGet(second.Id, out _)).IsTrue();
            await Assert.That(manager.TryGet(third.Id, out _)).IsTrue();
            await Assert.That(first.State).IsEqualTo(CaptureSessionState.Disposed);
            await Assert.That(third.State).IsEqualTo(CaptureSessionState.Running);
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task List_StateFilter_ReturnsOnlyMatchingSessions()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            await using var manager = new CaptureSessionManager(root, maxActiveSessions: 3, maxRetainedSessions: 4);
            CaptureSession completed = await manager.CreateAndStartAsync("fake", _ => new FakeCaptureSource(), new CaptureStartRequest(), CancellationToken.None);
            await completed.StopAsync(CancellationToken.None);
            CaptureSession running = await manager.CreateAndStartAsync("fake", _ => new FakeCaptureSource(), new CaptureStartRequest(), CancellationToken.None);

            IReadOnlyList<CaptureSession> completedSessions = manager.List(CaptureSessionState.Completed);
            IReadOnlyList<CaptureSession> runningSessions = manager.List(CaptureSessionState.Running);
            IReadOnlyList<CaptureSession> allSessions = manager.List();

            await Assert.That(completedSessions.Count).IsEqualTo(1);
            await Assert.That(completedSessions[0].Id).IsEqualTo(completed.Id);
            await Assert.That(runningSessions.Count).IsEqualTo(1);
            await Assert.That(runningSessions[0].Id).IsEqualTo(running.Id);
            await Assert.That(allSessions.Select(s => s.Id).Contains(completed.Id)).IsTrue();
            await Assert.That(allSessions.Select(s => s.Id).Contains(running.Id)).IsTrue();
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }

    [Test]
    public async Task DisposeAsync_StopsDisposesAllSessionsClearsRegistryAndRejectsNewWork()
    {
        string root = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var manager = new CaptureSessionManager(root, maxActiveSessions: 2, maxRetainedSessions: 4);
            var firstSource = new FakeCaptureSource();
            var secondSource = new FakeCaptureSource();
            await manager.CreateAndStartAsync("fake", _ => firstSource, new CaptureStartRequest(), CancellationToken.None);
            await manager.CreateAndStartAsync("fake", _ => secondSource, new CaptureStartRequest(), CancellationToken.None);

            await manager.DisposeAsync();
            await manager.DisposeAsync();

            await Assert.That(manager.Count).IsEqualTo(0);
            await Assert.That(firstSource.StopCallCount).IsEqualTo(1);
            await Assert.That(firstSource.DisposeCallCount).IsEqualTo(1);
            await Assert.That(secondSource.StopCallCount).IsEqualTo(1);
            await Assert.That(secondSource.DisposeCallCount).IsEqualTo(1);
            await Assert.That(async () => await manager.CreateAndStartAsync(
                    "fake",
                    _ => new FakeCaptureSource(),
                    new CaptureStartRequest(),
                    CancellationToken.None))
                .Throws<ObjectDisposedException>();
        }
        finally
        {
            TestDirectories.DeleteIfExists(root);
        }
    }
}
