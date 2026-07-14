// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Activation;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Activation;

public sealed class ActivationRetryPolicyTests
{
    private const int TransientFailure = unchecked((int)0x80080005u);

    [Test]
    public async Task ExecuteAsync_retries_transient_results_until_success()
    {
        var results = new Queue<int>([TransientFailure, TransientFailure, 0]);
        var delays = new List<TimeSpan>();
        var policy = new ActivationRetryPolicy(
            5,
            TimeSpan.FromMilliseconds(10),
            (delay, _) =>
            {
                delays.Add(delay);
                return Task.CompletedTask;
            });

        int result = await policy.ExecuteAsync(
            _ => Task.FromResult(results.Dequeue()),
            IsTransient);

        await Assert.That(result).IsEqualTo(0);
        await Assert.That(delays).IsEquivalentTo(
            new[] { TimeSpan.FromMilliseconds(10), TimeSpan.FromMilliseconds(20) });
    }

    [Test]
    public async Task ExecuteAsync_does_not_retry_non_transient_result()
    {
        int calls = 0;
        int delays = 0;
        var policy = new ActivationRetryPolicy(
            5,
            TimeSpan.FromMilliseconds(10),
            (_, _) =>
            {
                delays++;
                return Task.CompletedTask;
            });

        int result = await policy.ExecuteAsync(
            _ =>
            {
                calls++;
                return Task.FromResult(unchecked((int)0x80070005u));
            },
            IsTransient);

        await Assert.That(result).IsEqualTo(unchecked((int)0x80070005u));
        await Assert.That(calls).IsEqualTo(1);
        await Assert.That(delays).IsEqualTo(0);
    }

    [Test]
    public async Task ExecuteAsync_propagates_cancellation_during_retry_delay()
    {
        int calls = 0;
        using var cts = new CancellationTokenSource();
        var policy = new ActivationRetryPolicy(
            5,
            TimeSpan.FromMilliseconds(10),
            (_, _) =>
            {
                cts.Cancel();
                return Task.FromCanceled(cts.Token);
            });

        await Assert.That(async () =>
            await policy.ExecuteAsync(
                _ =>
                {
                    calls++;
                    return Task.FromResult(TransientFailure);
                },
                IsTransient,
                cts.Token)).Throws<OperationCanceledException>();

        await Assert.That(calls).IsEqualTo(1);
    }

    [Test]
    public async Task ExecuteAsync_returns_last_transient_result_after_exhaustion()
    {
        int calls = 0;
        int delays = 0;
        var policy = new ActivationRetryPolicy(
            3,
            TimeSpan.Zero,
            (_, _) =>
            {
                delays++;
                return Task.CompletedTask;
            });

        int result = await policy.ExecuteAsync(
            _ =>
            {
                calls++;
                return Task.FromResult(TransientFailure);
            },
            IsTransient);

        await Assert.That(result).IsEqualTo(TransientFailure);
        await Assert.That(calls).IsEqualTo(3);
        await Assert.That(delays).IsEqualTo(2);
    }

    private static bool IsTransient(int hresult) => hresult == TransientFailure;
}
