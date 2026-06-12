//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.Versioning;
using Opc.Classic.Ae.Dcom;

namespace Opc.Classic.Ae.Tests.Hosting.Windows;

[SupportedOSPlatform("windows")]
public sealed class OpcAeRefreshTests
{
    private const int S_OK = 0;

    [Test]
    public async Task Refresh_delivers_refresh_fragments_and_marks_final_fragment()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new FragmentingRefreshDispatcher();
        IntPtr subscription = OpcAeEventSinkTestHelpers.CreateSubscription(dispatcher);
        IntPtr connectionPoint = OpcAeEventSinkTestHelpers.FindEventConnectionPoint(subscription);
        IntPtr sink = OpcAeEventSinkTestHelpers.CreateSinkStub();
        try
        {
            (int adviseHr, uint cookie) = OpcAeEventSinkTestHelpers.Advise(connectionPoint, sink);
            await Assert.That(adviseHr).IsEqualTo(S_OK);

            Task<int> refreshTask = Task.Run(() => OpcAeEventSinkTestHelpers.InvokeRefresh(subscription, unchecked((int)cookie)));
            await dispatcher.FirstFragmentDelivered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            dispatcher.AllowFinalFragment();
            int refreshHr = await refreshTask.WaitAsync(TimeSpan.FromSeconds(5));

            OpcAeEventSinkTestHelpers.EventCallbackInvocation[] invocations = OpcAeEventSinkTestHelpers.GetInvocations(sink);
            await Assert.That(refreshHr).IsEqualTo(S_OK);
            await Assert.That(invocations.Length).IsEqualTo(2);
            await Assert.That(invocations[0].Refresh).IsTrue();
            await Assert.That(invocations[0].LastRefresh).IsFalse();
            await Assert.That(invocations[0].Events.Length).IsEqualTo(2);
            await Assert.That(invocations[1].Refresh).IsTrue();
            await Assert.That(invocations[1].LastRefresh).IsTrue();
            await Assert.That(invocations[1].Events.Length).IsEqualTo(1);
        }
        finally
        {
            OpcAeEventSinkTestHelpers.DestroySinkStub(sink);
        }
    }

    [Test]
    public async Task CancelRefresh_stops_midflight_before_last_refresh_fragment()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        var dispatcher = new FragmentingRefreshDispatcher();
        IntPtr subscription = OpcAeEventSinkTestHelpers.CreateSubscription(dispatcher);
        IntPtr connectionPoint = OpcAeEventSinkTestHelpers.FindEventConnectionPoint(subscription);
        IntPtr sink = OpcAeEventSinkTestHelpers.CreateSinkStub();
        try
        {
            (int adviseHr, uint cookie) = OpcAeEventSinkTestHelpers.Advise(connectionPoint, sink);
            await Assert.That(adviseHr).IsEqualTo(S_OK);

            Task<int> refreshTask = Task.Run(() => OpcAeEventSinkTestHelpers.InvokeRefresh(subscription, unchecked((int)cookie)));
            await dispatcher.FirstFragmentDelivered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            int cancelHr = OpcAeEventSinkTestHelpers.InvokeCancelRefresh(subscription, unchecked((int)cookie));
            int refreshHr = await refreshTask.WaitAsync(TimeSpan.FromSeconds(5));

            OpcAeEventSinkTestHelpers.EventCallbackInvocation[] invocations = OpcAeEventSinkTestHelpers.GetInvocations(sink);
            await Assert.That(cancelHr).IsEqualTo(S_OK);
            await Assert.That(refreshHr).IsEqualTo(S_OK);
            await Assert.That(invocations.Length).IsEqualTo(1);
            await Assert.That(invocations[0].Refresh).IsTrue();
            await Assert.That(invocations[0].LastRefresh).IsFalse();
        }
        finally
        {
            OpcAeEventSinkTestHelpers.DestroySinkStub(sink);
        }
    }

    private sealed class FragmentingRefreshDispatcher : RecordingAeDispatcher
    {
        private readonly TaskCompletionSource _releaseFinal = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private CancellationTokenSource? _refreshCts;

        public TaskCompletionSource FirstFragmentDelivered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void AllowFinalFragment() => _releaseFinal.TrySetResult();

        public override async Task RefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            if (!TryGetSink(connection, out IOPCEventSink? sink) || sink is null)
            {
                return;
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _refreshCts = cts;
            OpcEventNotification[] notifications = OpcAeEventSinkTestHelpers.CreateNotifications();
            try
            {
                await sink.OnEventAsync(ClientSubscription, refresh: true, lastRefresh: false, [notifications[0], notifications[1]], cts.Token).ConfigureAwait(false);
                FirstFragmentDelivered.TrySetResult();
                await _releaseFinal.Task.WaitAsync(cts.Token).ConfigureAwait(false);
                cts.Token.ThrowIfCancellationRequested();
                await sink.OnEventAsync(ClientSubscription, refresh: true, lastRefresh: true, [notifications[2]], cts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _refreshCts = null;
            }
        }

        public override async Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            _ = connection;
            cancellationToken.ThrowIfCancellationRequested();
            if (_refreshCts is not null)
            {
                await _refreshCts.CancelAsync().ConfigureAwait(false);
            }
            _releaseFinal.TrySetResult();
        }
    }
}
