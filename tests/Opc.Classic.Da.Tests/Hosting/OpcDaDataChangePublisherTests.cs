//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Hosting;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class OpcDaDataChangePublisherTests
{
    [Test]
    public async Task Advise_returns_distinct_cookies()
    {
        var publisher = CreatePublisher();

        var firstCookie = publisher.Advise((_, _) => ValueTask.CompletedTask);
        var secondCookie = publisher.Advise((_, _) => ValueTask.CompletedTask);

        await Assert.That(firstCookie).IsNotEqualTo(secondCookie);
    }

    [Test]
    public async Task PublishAsync_invokes_all_subscribers()
    {
        var publisher = CreatePublisher();
        var calls = new List<int>();

        publisher.Advise((_, _) =>
        {
            calls.Add(1);
            return ValueTask.CompletedTask;
        });
        publisher.Advise((_, _) =>
        {
            calls.Add(2);
            return ValueTask.CompletedTask;
        });
        publisher.Advise((_, _) =>
        {
            calls.Add(3);
            return ValueTask.CompletedTask;
        });

        await publisher.PublishAsync(CreateChange());

        await Assert.That(calls.Count).IsEqualTo(3);
        await Assert.That(calls).Contains(1);
        await Assert.That(calls).Contains(2);
        await Assert.That(calls).Contains(3);
    }

    [Test]
    public async Task Unadvise_removes_subscriber()
    {
        var publisher = CreatePublisher();
        var calls = 0;
        var cookie = publisher.Advise((_, _) =>
        {
            calls++;
            return ValueTask.CompletedTask;
        });

        publisher.Unadvise(cookie);
        await publisher.PublishAsync(CreateChange());

        await Assert.That(calls).IsEqualTo(ReadZero());
    }

    [Test]
    public async Task PublishAsync_continues_after_subscriber_throws()
    {
        var publisher = CreatePublisher();
        var calls = 0;
        publisher.Advise((_, _) => throw new InvalidOperationException("boom"));
        publisher.Advise((_, _) =>
        {
            calls++;
            return ValueTask.CompletedTask;
        });

        await publisher.PublishAsync(CreateChange());

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task PublishAsync_observes_cancellation()
    {
        var publisher = CreatePublisher();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var threw = false;
        try
        {
            await publisher.PublishAsync(CreateChange(), cts.Token);
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    private static OpcDaDataChangePublisher CreatePublisher() =>
        new(NoopLogger<OpcDaDataChangePublisher>.Instance);

    private static OpcDaDataChange CreateChange() =>
        new(1, 2, 0, 0, Array.Empty<OpcDaDataChangeItem>());

    // TUnitAssertions0005 workaround: use non-const indirection for literal assertions.
    private static int ReadZero() => 0;

    private sealed class NoopLogger<T> : ILogger<T>
    {
        public static NoopLogger<T> Instance { get; } = new();

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull =>
            null;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}
