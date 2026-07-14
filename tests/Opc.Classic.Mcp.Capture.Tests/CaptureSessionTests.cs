// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Reflection;
using System.Buffers.Binary;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;

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

    [Test]
    public async Task Constructor_ClonesAndSanitizesNtlmKey_AndDisposeZerosOwnedClone()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        byte[] callerKey = Enumerable.Range(1, 16).Select(i => (byte)i).ToArray();
        byte[] original = callerKey.ToArray();
        var source = new FakeCaptureSource();
        source.Packets.Add(NewTcpPacket(PduCodec.EncodePdu(
            new BindPdu
            {
                CallId = 80,
                MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
                MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
                ContextList = [],
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE)));
        var session = new CaptureSession(
            "session-key",
            "fake",
            source,
            directory,
            new CaptureStartRequest(NtlmSessionKey: callerKey));
        byte[] ownedKey = GetOwnedSessionKey(session)!;

        callerKey[0] = 0xEE;
        await session.StartAsync(TestContext.Current!.CancellationToken);

        await Assert.That(ReferenceEquals(callerKey, ownedKey)).IsFalse();
        await Assert.That(ownedKey[0]).IsEqualTo(original[0]);
        await Assert.That(session.Request.NtlmSessionKey).IsNull();
        await Assert.That(source.LastStartRequest!.NtlmSessionKey).IsNull();

        await session.DisposeAsync();

        await Assert.That(ownedKey.All(b => b == 0)).IsTrue();
        await Assert.That(callerKey[0]).IsEqualTo((byte)0xEE);
        await Assert.That(callerKey[1]).IsEqualTo(original[1]);
    }

    [Test]
    public async Task StartAsync_FailureZerosOwnedNtlmKeyWithoutMutatingCaller()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        byte[] callerKey = Enumerable.Range(0x40, 16).Select(i => (byte)i).ToArray();
        byte[] original = callerKey.ToArray();
        var source = new FakeCaptureSource
        {
            StartException = new CaptureException("start failed"),
        };
        var session = new CaptureSession(
            "session-key-fail",
            "fake",
            source,
            directory,
            new CaptureStartRequest(NtlmSessionKey: callerKey));
        byte[] ownedKey = GetOwnedSessionKey(session)!;

        await Assert.That(async () => await session.StartAsync(TestContext.Current!.CancellationToken))
            .Throws<CaptureException>();

        await Assert.That(ownedKey.All(b => b == 0)).IsTrue();
        await Assert.That(callerKey).IsEquivalentTo(original);
        await session.DisposeAsync();
    }

    [Test]
    public async Task StopAsync_FailureZerosOwnedNtlmKeyAndDisposesCursorUnwrapper()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        byte[] callerKey = Enumerable.Range(0x20, 16).Select(i => (byte)i).ToArray();
        byte[] original = callerKey.ToArray();
        var source = new FakeCaptureSource();
        var session = new CaptureSession(
            "session-key-stop-fail",
            "fake",
            source,
            directory,
            new CaptureStartRequest(NtlmSessionKey: callerKey));
        byte[] ownedKey = GetOwnedSessionKey(session)!;

        await session.StartAsync(TestContext.Current!.CancellationToken);
        DrainTailResult initial = await session.DrainTailAsync(0, 10, TestContext.Current.CancellationToken);
        NtlmPassiveUnwrapper unwrapper = GetCursorUnwrapper(session)!;
        source.StopException = new CaptureException("stop failed");

        await Assert.That(async () => await session.StopAsync(TestContext.Current.CancellationToken))
            .Throws<CaptureException>();

        await Assert.That(session.State).IsEqualTo(CaptureSessionState.Failed);
        await Assert.That(ownedKey.All(b => b == 0)).IsTrue();
        await Assert.That(IsDisposed(unwrapper)).IsTrue();
        await Assert.That(callerKey).IsEquivalentTo(original);
        source.Packets.Add(NewTcpPacket(AppendBogusNtlmTrailer(PduCodec.EncodePdu(
            new RequestCoPdu
            {
                CallId = 81,
                ContextId = 0,
                Opnum = 3,
                AllocationHint = 4,
                Stub = [0x01, 0x02, 0x03, 0x04],
            },
            ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE))));

        DrainTailResult afterFailure = await session.DrainTailAsync(
            initial.NextIndex,
            10,
            TestContext.Current.CancellationToken);

        await Assert.That(afterFailure.Pdus.Count).IsEqualTo(0);
        await Assert.That(afterFailure.SessionState).IsEqualTo(CaptureSessionState.Failed);
        await Assert.That(afterFailure.Done).IsTrue();
        await Assert.That(GetCursorUnwrapper(session)).IsNull();
        await Assert.That(GetDecoderUnwrapper(session)).IsNull();
        await session.DisposeAsync();
    }

    [Test]
    public async Task SecretCleanup_PreventsConcurrentTailFromCreatingKeyedCursor()
    {
        string directory = TestDirectories.CreateUniqueTempDirectory();
        byte[] callerKey = Enumerable.Range(0x30, 16).Select(i => (byte)i).ToArray();
        var source = new FakeCaptureSource
        {
            StopException = new CaptureException("stop failed"),
        };
        var session = new CaptureSession(
            "session-key-cleanup-race",
            "fake",
            source,
            directory,
            new CaptureStartRequest(NtlmSessionKey: callerKey));
        byte[] ownedKey = GetOwnedSessionKey(session)!;
        await session.StartAsync(TestContext.Current!.CancellationToken);

        var cleanupObserved = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var releaseCleanup = new ManualResetEventSlim(false);
        session.SecretCleanupObserved = () =>
        {
            cleanupObserved.TrySetResult();
            releaseCleanup.Wait();
        };

        Task stopTask = Task.Run(
            async () => await session.StopAsync(TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);
        await cleanupObserved.Task.WaitAsync(TestContext.Current.CancellationToken);

        var tailStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task<DrainTailResult> tailTask = Task.Run(async () =>
        {
            tailStarted.TrySetResult();
            return await session.DrainTailAsync(0, 10, TestContext.Current.CancellationToken);
        }, TestContext.Current.CancellationToken);
        await tailStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        await Task.Delay(20, TestContext.Current.CancellationToken);
        await Assert.That(tailTask.IsCompleted).IsFalse();

        releaseCleanup.Set();
        await Assert.That(async () => await stopTask).Throws<CaptureException>();
        DrainTailResult tail = await tailTask;
        session.SecretCleanupObserved = null;

        await Assert.That(ownedKey.All(b => b == 0)).IsTrue();
        await Assert.That(tail.SessionState).IsEqualTo(CaptureSessionState.Failed);
        await Assert.That(GetCursorUnwrapper(session)).IsNull();
        await Assert.That(GetDecoderUnwrapper(session)).IsNull();
        await session.DisposeAsync();
    }

    [Test]
    public async Task Constructor_FailureDoesNotMutateCallerNtlmKey()
    {
        byte[] callerKey = Enumerable.Range(0x70, 16).Select(i => (byte)i).ToArray();
        byte[] original = callerKey.ToArray();

        await Assert.That(() => new CaptureSession(
            string.Empty,
            "fake",
            new FakeCaptureSource(),
            "folder",
            new CaptureStartRequest(NtlmSessionKey: callerKey))).Throws<ArgumentException>();

        await Assert.That(callerKey).IsEquivalentTo(original);
    }

    private static byte[]? GetOwnedSessionKey(CaptureSession session)
        => (byte[]?)typeof(CaptureSession)
            .GetField("_ntlmSessionKey", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(session);

    private static NtlmPassiveUnwrapper? GetCursorUnwrapper(CaptureSession session)
    {
        object? cursor = typeof(CaptureSession)
            .GetField("_cursor", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(session);
        return cursor is null
            ? null
            : (NtlmPassiveUnwrapper?)cursor.GetType()
                .GetProperty("Unwrapper", BindingFlags.Instance | BindingFlags.Public)!
                .GetValue(cursor);
    }

    private static bool IsDisposed(NtlmPassiveUnwrapper unwrapper)
        => (bool)typeof(NtlmPassiveUnwrapper)
            .GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(unwrapper)!;

    private static NtlmPassiveUnwrapper? GetDecoderUnwrapper(CaptureSession session)
    {
        object? cursor = typeof(CaptureSession)
            .GetField("_cursor", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(session);
        object? decoder = cursor?.GetType()
            .GetProperty("Decoder", BindingFlags.Instance | BindingFlags.Public)!
            .GetValue(cursor);
        return decoder is null
            ? null
            : (NtlmPassiveUnwrapper?)typeof(OpcDcomDecoder)
                .GetField("_unwrapper", BindingFlags.Instance | BindingFlags.NonPublic)!
                .GetValue(decoder);
    }

    private static byte[] AppendBogusNtlmTrailer(byte[] pdu)
    {
        const int verifierHeaderLength = 8;
        const int verifierLength = 16;
        int padding = (4 - (pdu.Length % 4)) % 4;
        int verifierStart = pdu.Length + padding;
        byte[] frame = new byte[verifierStart + verifierHeaderLength + verifierLength];
        pdu.CopyTo(frame, 0);
        frame[verifierStart] = 0x0A;
        frame[verifierStart + 1] = 0x06;
        frame[verifierStart + 2] = (byte)padding;
        BinaryPrimitives.WriteUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, 2),
            (ushort)frame.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(
            frame.AsSpan(ConnectionOrientedPdu.AUTH_LENGTH_OFFSET, 2),
            verifierLength);
        return frame;
    }

    private static CapturedPacket NewTcpPacket(byte[] tcpPayload)
    {
        byte[] frame = new byte[14 + 20 + 20 + tcpPayload.Length];
        frame[12] = 0x08;
        frame[13] = 0x00;
        int ipOffset = 14;
        frame[ipOffset] = 0x45;
        BinaryPrimitives.WriteUInt16BigEndian(
            frame.AsSpan(ipOffset + 2, 2),
            (ushort)(20 + 20 + tcpPayload.Length));
        frame[ipOffset + 8] = 64;
        frame[ipOffset + 9] = 6;
        frame[ipOffset + 12] = 10;
        frame[ipOffset + 15] = 1;
        frame[ipOffset + 16] = 10;
        frame[ipOffset + 19] = 2;
        int tcpOffset = ipOffset + 20;
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset, 2), 50000);
        BinaryPrimitives.WriteUInt16BigEndian(frame.AsSpan(tcpOffset + 2, 2), 135);
        frame[tcpOffset + 12] = 0x50;
        frame[tcpOffset + 13] = 0x18;
        tcpPayload.CopyTo(frame.AsSpan(tcpOffset + 20));
        return new CapturedPacket(
            DateTimeOffset.UnixEpoch,
            frame.Length,
            frame,
            LinkType: 1,
            Annotations: new Dictionary<string, string?>());
    }
}
