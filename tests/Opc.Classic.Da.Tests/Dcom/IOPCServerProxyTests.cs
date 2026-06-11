//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Dcom;

public sealed class IOPCServerProxyTests
{
    [Test]
    public async Task GetStatus_invokes_channel_with_correct_metadata()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        var proxy = new IOPCServerClientProxy(channel);
        try
        {
            _ = await proxy.GetStatusAsync(CancellationToken.None);
        }
        catch
        {
            // Empty payloads may not decode to OpcServerStatus; this test verifies channel wiring.
        }

        await Assert.That(observedIid).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
    }

    [Test]
    public async Task RemoveGroup_failure_throws_OpcException()
    {
        const int E_FAIL = unchecked((int)0x80004005u);
        var channel = new InMemoryCallChannel(static (_, _, _, _) =>
            Task.FromResult(new NdrCallResult(E_FAIL, ReadOnlyMemory<byte>.Empty)));

        var proxy = new IOPCServerClientProxy(channel);
        var exception = await CaptureAsync<OpcException>(() => proxy.RemoveGroupAsync(42, force: true, CancellationToken.None));

        await Assert.That(exception.ResultId.Code).IsEqualTo(E_FAIL);
    }

    [Test]
    public async Task GetErrorString_encodes_two_int_params()
    {
        ReadOnlyMemory<byte> capturedPayload = ReadOnlyMemory<byte>.Empty;
        var channel = new InMemoryCallChannel((_, _, payload, _) =>
        {
            capturedPayload = payload.ToArray();
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        var proxy = new IOPCServerClientProxy(channel);
        try
        {
            _ = await proxy.GetErrorStringAsync(0x12345678, 0x0409, CancellationToken.None);
        }
        catch
        {
            // Empty payloads may not decode to string; this test verifies request encoding.
        }

        await Assert.That(capturedPayload.Length).IsGreaterThanOrEqualTo(8);
        var errorCodeBytes = capturedPayload.Slice(0, 4).ToArray();
        await Assert.That(errorCodeBytes[0]).IsEqualTo((byte)0x78);
        await Assert.That(errorCodeBytes[1]).IsEqualTo((byte)0x56);
        await Assert.That(errorCodeBytes[2]).IsEqualTo((byte)0x34);
        await Assert.That(errorCodeBytes[3]).IsEqualTo((byte)0x12);
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
