//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OpcClassic.Testing;
using OpcClassic.Transport;
using SharpInterop.Core;
using TUnit.Core;

namespace OpcClassic.Dcom.Tests;

public sealed class LocalCoClassModernizationTests
{
    [Test]
    public async Task BackgroundService_accepts_and_queues_connections()
    {
        var endpoint = new RecordingAsyncEndpoint();
        var releaseProcessor = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Func<IAsyncTransport, CancellationToken, ValueTask> processor = async (_, cancellationToken) =>
            await releaseProcessor.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        IHostedService service = CreateAcceptService(endpoint, processor, workerCount: 1);

        try
        {
            await service.StartAsync(CancellationToken.None).ConfigureAwait(false);
            await endpoint.EnqueueAsync(new InMemoryAsyncTransport()).ConfigureAwait(false);

            await WaitUntilAsync(() => GetInt32(service, nameof(QueuedConnectionCount)) == 1).ConfigureAwait(false);

            await Assert.That(GetInt32(service, nameof(AcceptedConnectionCount))).IsEqualTo(1);
            await Assert.That(GetInt32(service, nameof(QueuedConnectionCount))).IsEqualTo(1);
        }
        finally
        {
            releaseProcessor.TrySetResult();
            await service.StopAsync(CancellationToken.None).ConfigureAwait(false);
            await DisposeAcceptServiceAsync(service).ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Worker_pool_processes_queued_connections()
    {
        var endpoint = new RecordingAsyncEndpoint();
        var processedTwo = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processed = 0;
        Func<IAsyncTransport, CancellationToken, ValueTask> processor = (_, _) =>
        {
            if (Interlocked.Increment(ref processed) == 2)
            {
                processedTwo.TrySetResult();
            }

            return ValueTask.CompletedTask;
        };
        IHostedService service = CreateAcceptService(endpoint, processor, workerCount: 2);

        try
        {
            await service.StartAsync(CancellationToken.None).ConfigureAwait(false);
            await endpoint.EnqueueAsync(new InMemoryAsyncTransport()).ConfigureAwait(false);
            await endpoint.EnqueueAsync(new InMemoryAsyncTransport()).ConfigureAwait(false);

            await processedTwo.Task.WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false);
            await WaitUntilAsync(() => GetInt32(service, nameof(ProcessedConnectionCount)) == 2).ConfigureAwait(false);

            await Assert.That(GetInt32(service, nameof(WorkerCount))).IsEqualTo(2);
            await Assert.That(GetInt32(service, nameof(ProcessedConnectionCount))).IsEqualTo(2);
        }
        finally
        {
            await service.StopAsync(CancellationToken.None).ConfigureAwait(false);
            await DisposeAcceptServiceAsync(service).ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Disposal_stops_BackgroundService_cleanly()
    {
        var endpoint = new RecordingAsyncEndpoint();
        Func<IAsyncTransport, CancellationToken, ValueTask> processor = (_, _) => ValueTask.CompletedTask;
        IHostedService service = CreateAcceptService(endpoint, processor, workerCount: 1);

        await service.StartAsync(CancellationToken.None).ConfigureAwait(false);
        await DisposeAcceptServiceAsync(service).ConfigureAwait(false);

        await Assert.That(endpoint.IsDisposed).IsTrue();
    }

    private static IHostedService CreateAcceptService(
        IAsyncEndpoint endpoint,
        Func<IAsyncTransport, CancellationToken, ValueTask> processor,
        int workerCount)
    {
        var serviceType = typeof(LocalCoClass).Assembly.GetType(
            "SharpInterop.Core.ComOxidRuntimeAcceptService", throwOnError: true)!;
        return (IHostedService)Activator.CreateInstance(serviceType, endpoint, processor, workerCount)!;
    }

    private static int GetInt32(IHostedService service, string propertyName) =>
        (int)service.GetType().GetProperty(propertyName)!.GetValue(service)!;

    private static async ValueTask DisposeAcceptServiceAsync(IHostedService service)
    {
        if (service is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
            return;
        }

        if (service is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static async Task WaitUntilAsync(Func<bool> predicate)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        while (!predicate())
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeout.Token).ConfigureAwait(false);
        }
    }

    private const string AcceptedConnectionCount = "AcceptedConnectionCount";
    private const string QueuedConnectionCount = "QueuedConnectionCount";
    private const string ProcessedConnectionCount = "ProcessedConnectionCount";
    private const string WorkerCount = "WorkerCount";

    private sealed class RecordingAsyncEndpoint : IAsyncEndpoint
    {
        private readonly Channel<IAsyncTransport> _connections = Channel.CreateUnbounded<IAsyncTransport>();

        public EndPoint LocalEndpoint { get; } = new IPEndPoint(IPAddress.Loopback, 0);

        public bool IsDisposed { get; private set; }

        public async IAsyncEnumerable<IAsyncTransport> AcceptConnectionsAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var transport in _connections.Reader.ReadAllAsync(cancellationToken).WithCancellation(cancellationToken))
            {
                yield return transport;
            }
        }

        public async ValueTask EnqueueAsync(IAsyncTransport transport)
        {
            await _connections.Writer.WriteAsync(transport).ConfigureAwait(false);
        }

        public ValueTask DisposeAsync()
        {
            IsDisposed = true;
            _connections.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }
    }
}
