//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Da.Dcom;
using OpcClassic.Testing;
using TUnit.Core;

namespace OpcClassic.Integration.Tests.Loopback;

public sealed class F7_Cancellation
{
    [Test]
    public async Task Already_cancelled_token_propagates_as_OperationCanceledException()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var channel = new InMemoryCallChannel(static (_, _, _, _) =>
            throw new InvalidOperationException("The channel handler should not run for an already-cancelled token."));
        var proxy = new IOPCServer_ClientProxy(channel);

        var exception = await CaptureAsync<OperationCanceledException>(() => proxy.GetStatusAsync(cts.Token));

        await Assert.That(exception.CancellationToken).IsEqualTo(cts.Token);
        var expectedCallCount = 0;
        await Assert.That(channel.CallLog.Count).IsEqualTo(expectedCallCount);
    }

    private static async Task<TException> CaptureAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Expected {typeof(TException).Name}, but caught {exception.GetType().Name}.", exception);
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}, but no exception was thrown.");
    }
}
