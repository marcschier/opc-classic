// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Testing;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F5ErrorInjection
{
    [Test]
    public async Task GetStatus_hresult_failure_from_InMemoryCallChannel_surfaces_as_OpcException()
    {
        var expectedHresult = unchecked((int)0xC0040007u);
        var channel = new InMemoryCallChannel((_, _, _, _) =>
            Task.FromResult(new NdrCallResult(expectedHresult, ReadOnlyMemory<byte>.Empty)));
        var proxy = new IOPCServerClientProxy(channel);

        var exception = await CaptureAsync<OpcException>(() => proxy.GetStatusAsync(CancellationToken.None));

        await Assert.That(exception.ResultId.Code).IsEqualTo(expectedHresult);
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
