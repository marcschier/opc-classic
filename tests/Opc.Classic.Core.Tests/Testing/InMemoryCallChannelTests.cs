//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class InMemoryCallChannelTests
{
    [Test]
    public async Task InvokeAsync_ForwardsToHandler()
    {
        var expectedInterfaceId = new Guid("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        byte[] request = [0x01, 0x02, 0x03];
        using var cts = new CancellationTokenSource();

        bool called = false;
        Guid actualInterfaceId = Guid.Empty;
        int actualOpnum = -1;
        byte[] actualPayload = [];
        CancellationToken actualToken = default;

        var channel = new InMemoryCallChannel((interfaceId, opnum, requestPayload, cancellationToken) =>
        {
            called = true;
            actualInterfaceId = interfaceId;
            actualOpnum = opnum;
            actualPayload = requestPayload.ToArray();
            actualToken = cancellationToken;
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        await channel.InvokeAsync(expectedInterfaceId, 4, request, cts.Token);

        await Assert.That(called).IsTrue();
        await Assert.That(actualInterfaceId).IsEqualTo(expectedInterfaceId);
        await Assert.That(actualOpnum).IsEqualTo(4);
        await Assert.That(actualPayload.Length).IsEqualTo(3);
        await Assert.That(actualPayload[0]).IsEqualTo((byte)0x01);
        await Assert.That(actualPayload[2]).IsEqualTo((byte)0x03);
        await Assert.That(actualToken).IsEqualTo(cts.Token);
    }

    [Test]
    public async Task InvokeAsync_ReturnsHandlerResult()
    {
        byte[] responsePayload = [0x10, 0x20, 0x30];
        var expected = new NdrCallResult(unchecked((int)0xC0040007u), responsePayload);
        var channel = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(expected));

        NdrCallResult result = await channel.InvokeAsync(Guid.Empty, 1, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result).IsEqualTo(expected);
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(3);
        await Assert.That(result.ResponsePayload.Span[1]).IsEqualTo((byte)0x20);
    }

    [Test]
    public async Task InvokeAsync_PropagatesHandlerException()
    {
        var channel = new InMemoryCallChannel((_, _, _, _) =>
        {
            throw new TimeoutException("simulated transport timeout");
        });

        bool threw = false;
        try
        {
            await channel.InvokeAsync(Guid.Empty, 1, ReadOnlyMemory<byte>.Empty);
        }
        catch (TimeoutException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task CallLog_RecordsEachCall()
    {
        var channel = new InMemoryCallChannel((_, _, _, _) =>
            Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));
        var firstInterfaceId = new Guid("00000000-0000-0000-0000-000000000001");
        var secondInterfaceId = new Guid("00000000-0000-0000-0000-000000000002");

        await channel.InvokeAsync(firstInterfaceId, 3, new byte[] { 0x01 });
        await channel.InvokeAsync(secondInterfaceId, 4, new byte[] { 0x02, 0x03 });
        await channel.InvokeAsync(firstInterfaceId, 5, ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<InMemoryCall> log = channel.CallLog;

        await Assert.That(log.Count).IsEqualTo(3);
        await Assert.That(log[0].InterfaceId).IsEqualTo(firstInterfaceId);
        await Assert.That(log[0].Opnum).IsEqualTo(3);
        await Assert.That(log[0].PayloadLength).IsEqualTo(1);
        await Assert.That(log[1].InterfaceId).IsEqualTo(secondInterfaceId);
        await Assert.That(log[1].Opnum).IsEqualTo(4);
        await Assert.That(log[1].PayloadLength).IsEqualTo(2);
        await Assert.That(log[2].InterfaceId).IsEqualTo(firstInterfaceId);
        await Assert.That(log[2].Opnum).IsEqualTo(5);
        await Assert.That(log[2].PayloadLength).IsEqualTo(0);
    }

    [Test]
    public async Task CancellationToken_IsObserved()
    {
        bool handlerCalled = false;
        var channel = new InMemoryCallChannel((_, _, _, _) =>
        {
            handlerCalled = true;
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        bool threw = false;
        try
        {
            await channel.InvokeAsync(Guid.Empty, 1, ReadOnlyMemory<byte>.Empty, cts.Token);
        }
        catch (OperationCanceledException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
        await Assert.That(handlerCalled).IsFalse();
        await Assert.That(channel.CallLog.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Builder_RegistersPerIidOpnumHandler()
    {
        var interfaceId = new Guid("10000000-0000-0000-0000-000000000001");
        int registeredCalls = 0;
        int fallbackCalls = 0;
        var channel = new InMemoryCallChannelBuilder()
            .Register(interfaceId, 3, (_, _, _, _) =>
            {
                registeredCalls++;
                return Task.FromResult(new NdrCallResult(123, new byte[] { 0xAA }));
            })
            .WithFallback((_, _, _, _) =>
            {
                fallbackCalls++;
                return Task.FromResult(new NdrCallResult(456, new byte[] { 0xBB }));
            })
            .Build();

        NdrCallResult registered = await channel.InvokeAsync(interfaceId, 3, ReadOnlyMemory<byte>.Empty);
        NdrCallResult fallback = await channel.InvokeAsync(interfaceId, 4, ReadOnlyMemory<byte>.Empty);

        await Assert.That(registered.Hresult).IsEqualTo(123);
        await Assert.That(registered.ResponsePayload.Span[0]).IsEqualTo((byte)0xAA);
        await Assert.That(fallback.Hresult).IsEqualTo(456);
        await Assert.That(fallback.ResponsePayload.Span[0]).IsEqualTo((byte)0xBB);
        await Assert.That(registeredCalls).IsEqualTo(1);
        await Assert.That(fallbackCalls).IsEqualTo(1);
    }

    [Test]
    public async Task Builder_FallbackReturnsENotImplByDefault()
    {
        const int eNotImpl = unchecked((int)0x80004001u);
        var channel = new InMemoryCallChannelBuilder().Build();

        NdrCallResult result = await channel.InvokeAsync(Guid.Empty, 9, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(eNotImpl);
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
    }

    [Test]
    public async Task InMemoryCallChannel_SatisfiesICallChannelContract()
    {
        InMemoryCallChannel channel = new((_, _, _, _) =>
            Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));

        NdrCallResult result = await ((ICallChannel)channel).InvokeAsync(Guid.Empty, 3, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(0);
    }
}
