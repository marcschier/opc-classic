//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Testing;
using TUnit.Core;

namespace OpcClassic.Generators.Tests;

[OpcInterface("11111111-2222-3333-4444-555555555555")]
[GenerateOpcProxy]
public partial interface IDemoService
{
    [OpcMethod(3)]
    Task PingAsync(CancellationToken ct);

    [OpcMethod(4)]
    Task<int> ReadCountAsync(CancellationToken ct);

    [OpcMethod(5)]
    Task<string> GetLabelAsync();

    Task<bool> WithoutOpcMethodAsync();
}

public sealed class ProxyIntegrationTests
{
    [Test]
    public async Task Successful_call_returns_without_throwing()
    {
        var channel = new InMemoryCallChannel(static (_, _, _, _) =>
            Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));
        var proxy = new IDemoService_ClientProxy(channel);

        await proxy.PingAsync(CancellationToken.None);

        await Assert.That(channel.CallLog.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Failed_call_throws_OpcException()
    {
        const int OPC_E_UNKNOWNITEMID = unchecked((int)0xC0040007u);
        var channel = new InMemoryCallChannel(static (_, _, _, _) =>
            Task.FromResult(new NdrCallResult(OPC_E_UNKNOWNITEMID, ReadOnlyMemory<byte>.Empty)));
        var proxy = new IDemoService_ClientProxy(channel);

        var exception = await CaptureAsync<OpcException>(() => proxy.PingAsync(CancellationToken.None));

        await Assert.That(exception.ResultId.Code).IsEqualTo(OPC_E_UNKNOWNITEMID);
    }

    [Test]
    public async Task Channel_observes_correct_metadata()
    {
        Guid observedInterfaceId = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((interfaceId, opnum, _, _) =>
        {
            observedInterfaceId = interfaceId;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new IDemoService_ClientProxy(channel);

        _ = await proxy.ReadCountAsync(CancellationToken.None);

        await Assert.That(observedInterfaceId).IsEqualTo(IDemoService.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IDemoService.Opnums.ReadCountAsync);
    }

    [Test]
    public async Task CancellationToken_propagates()
    {
        using var cts = new CancellationTokenSource();
        CancellationToken observed = default;
        var channel = new InMemoryCallChannel((_, _, _, cancellationToken) =>
        {
            observed = cancellationToken;
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new IDemoService_ClientProxy(channel);

        await proxy.PingAsync(cts.Token);

        await Assert.That(observed).IsEqualTo(cts.Token);
    }

    [Test]
    public async Task Method_without_OpcMethod_still_throws_NotImplementedException()
    {
        var channel = new InMemoryCallChannel(static (_, _, _, _) =>
            Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));
        var proxy = new IDemoService_ClientProxy(channel);

        await CaptureAsync<NotImplementedException>(proxy.WithoutOpcMethodAsync);
    }

    [Test]
    public async Task Multiple_method_invocations_log_correctly()
    {
        var channel = new InMemoryCallChannel(static (_, _, _, _) =>
            Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));
        var proxy = new IDemoService_ClientProxy(channel);

        await proxy.PingAsync(CancellationToken.None);
        _ = await proxy.ReadCountAsync(CancellationToken.None);
        _ = await proxy.GetLabelAsync();

        var callLog = channel.CallLog;
        await Assert.That(callLog.Count).IsEqualTo(3);
        await Assert.That(callLog[0].Opnum).IsEqualTo(IDemoService.Opnums.PingAsync);
        await Assert.That(callLog[1].Opnum).IsEqualTo(IDemoService.Opnums.ReadCountAsync);
        await Assert.That(callLog[2].Opnum).IsEqualTo(IDemoService.Opnums.GetLabelAsync);
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
