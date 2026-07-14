// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Bounded retry policy for transient activation failures.
/// </summary>
public sealed class ActivationRetryPolicy
{
    private static readonly TimeSpan DefaultBaseDelay = TimeSpan.FromMilliseconds(500);
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;

    public ActivationRetryPolicy(
        int maxAttempts,
        TimeSpan baseDelay,
        Func<TimeSpan, CancellationToken, Task>? delayAsync = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(baseDelay, TimeSpan.Zero);

        MaxAttempts = maxAttempts;
        BaseDelay = baseDelay;
        _delayAsync = delayAsync ?? Task.Delay;
    }

    public static ActivationRetryPolicy Default { get; } = new(5, DefaultBaseDelay);

    public int MaxAttempts { get; }

    public TimeSpan BaseDelay { get; }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        Func<T, bool> isTransient,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(isTransient);

        for (int attempt = 1; ; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            T result = await operation(cancellationToken).ConfigureAwait(false);
            if (attempt >= MaxAttempts || !isTransient(result))
            {
                return result;
            }

            await _delayAsync(Multiply(BaseDelay, attempt), cancellationToken).ConfigureAwait(false);
        }
    }

    private static TimeSpan Multiply(TimeSpan delay, int multiplier) =>
        TimeSpan.FromTicks(checked(delay.Ticks * multiplier));
}
