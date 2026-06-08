//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using System.Threading;
using Opc.Classic.Mcp.Capture;
using TUnit.Core;

namespace Opc.Classic.Mcp.Capture.Tests;

public sealed class CaptureSessionTests
{
    [Test]
    public async Task Constructor_InvalidArguments_Throw()
    {
        var source = new FakeCaptureSource();
        var request = new CaptureStartRequest();

        await Assert.That(() => new CaptureSession(null!, "fake", source, "folder", request)).Throws<ArgumentNullException>();
        await Assert.That(() => new CaptureSession(string.Empty, "fake", source, "folder", request)).Throws<ArgumentException>();
        await Assert.That(() => new CaptureSession("id", null!, source, "folder", request)).Throws<ArgumentNullException>();
        await Assert.That(() => new CaptureSession("id", string.Empty, source, "folder", request)).Throws<ArgumentException>();
        await Assert.That(() => new CaptureSession("id", "fake", null!, "folder", request)).Throws<ArgumentNullException>();
        await Assert.That(() => new CaptureSession("id", "fake", source, null!, request)).Throws<ArgumentNullException>();
        await Assert.That(() => new CaptureSession("id", "fake", source, string.Empty, request)).Throws<ArgumentException>();
        await Assert.That(() => new CaptureSession("id", "fake", source, "folder", null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task StartAsync_SourceSucceeds_TransitionsToRunningAndRecordsMetadata()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var request = new CaptureStartRequest(InterfaceName: "lo");
            var source = new FakeCaptureSource();
            var session = new CaptureSession("session-id", "fake", source, directory, request);
            DateTimeOffset before = session.LastTouchedAt;

            await session.StartAsync(TestContext.Current!.CancellationToken);

            await Assert.That(session.State).IsEqualTo(CaptureSessionState.Running);
            await Assert.That(session.StartedAt.HasValue).IsTrue();
            await Assert.That(session.StoppedAt).IsNull();
            await Assert.That(session.Error).IsNull();
            await Assert.That(source.StartCallCount).IsEqualTo(1);
            await Assert.That(source.LastStartRequest).IsEqualTo(request);
            await Assert.That(session.LastTouchedAt >= before).IsTrue();
            await session.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task StopAsync_RunningSession_TransitionsToCompletedAndIsIdempotent()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new FakeCaptureSource();
            var session = new CaptureSession("session-id", "fake", source, directory, new CaptureStartRequest());
            await session.StartAsync(TestContext.Current!.CancellationToken);

            await session.StopAsync(TestContext.Current.CancellationToken);
            await session.StopAsync(TestContext.Current.CancellationToken);

            await Assert.That(session.State).IsEqualTo(CaptureSessionState.Completed);
            await Assert.That(session.StoppedAt.HasValue).IsTrue();
            await Assert.That(source.StopCallCount).IsEqualTo(1);
            await session.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task StartAsync_SourceThrows_TransitionsToFailedAndStoresError()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new FakeCaptureSource
            {
                StartException = new CaptureException("start failed"),
            };
            var session = new CaptureSession("session-id", "fake", source, directory, new CaptureStartRequest());

            await Assert.That(async () => await session.StartAsync(TestContext.Current!.CancellationToken))
                .Throws<CaptureException>();

            await Assert.That(session.State).IsEqualTo(CaptureSessionState.Failed);
            await Assert.That(session.Error).IsEqualTo("start failed");
            await Assert.That(source.StartCallCount).IsEqualTo(1);
            await session.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task StopAsync_SourceThrows_TransitionsToFailedAndStoresError()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new FakeCaptureSource();
            var session = new CaptureSession("session-id", "fake", source, directory, new CaptureStartRequest());
            await session.StartAsync(TestContext.Current!.CancellationToken);
            source.StopException = new CaptureException("stop failed");

            await Assert.That(async () => await session.StopAsync(TestContext.Current.CancellationToken))
                .Throws<CaptureException>();

            await Assert.That(session.State).IsEqualTo(CaptureSessionState.Failed);
            await Assert.That(session.Error).IsEqualTo("stop failed");
            await Assert.That(source.StopCallCount).IsEqualTo(1);
            await session.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }

    [Test]
    public async Task DisposeAsync_DisposesSourceDeletesFolderAndMarksDisposed()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        string marker = Path.Combine(directory, "marker.txt");
        await File.WriteAllTextAsync(marker, "delete me", TestContext.Current!.CancellationToken);
        var source = new FakeCaptureSource();
        var session = new CaptureSession("session-id", "fake", source, directory, new CaptureStartRequest());

        await session.DisposeAsync();
        await session.DisposeAsync();

        await Assert.That(source.DisposeCallCount).IsEqualTo(1);
        await Assert.That(session.State).IsEqualTo(CaptureSessionState.Disposed);
        await Assert.That(Directory.Exists(directory)).IsFalse();
    }

    [Test]
    public async Task StopAsync_CompletedOrFailedSession_DoesNotCallSourceAgain()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        try
        {
            var source = new FakeCaptureSource();
            var session = new CaptureSession("session-id", "fake", source, directory, new CaptureStartRequest());
            await session.StartAsync(TestContext.Current!.CancellationToken);
            await session.StopAsync(CancellationToken.None);

            await session.StopAsync(CancellationToken.None);

            await Assert.That(source.StopCallCount).IsEqualTo(1);
            await Assert.That(session.State).IsEqualTo(CaptureSessionState.Completed);
            await session.DisposeAsync();
        }
        finally
        {
            TestDirectories.DeleteIfExists(directory);
        }
    }
}
