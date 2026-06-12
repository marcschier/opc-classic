//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Tests;

public sealed class OpcCommonClientProxyTests
{
    [Test]
    public async Task SetClientNameAsync_InvokesIOPCCommonSetClientName()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        string? observedClientName = null;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            observedClientName = reader.ReadUnicodeStringPtr();
            return Task.FromResult(new NdrCallResult(OpcResultId.Ok.Code, ReadOnlyMemory<byte>.Empty));
        });
        var proxy = new OpcCommonClientProxy(channel);

        await proxy.SetClientNameAsync("opc-client", CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(OpcCommonClientProxy.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(OpcCommonClientProxy.Opnums.SetClientName);
        await Assert.That(observedClientName).IsEqualTo("opc-client");
    }

    [Test]
    public async Task SetClientNameAsync_FailureHresultThrowsOpcException()
    {
        var channel = new InMemoryCallChannel((_, _, _, _) =>
            Task.FromResult(new NdrCallResult(OpcResultId.Fail.Code, ReadOnlyMemory<byte>.Empty)));
        var proxy = new OpcCommonClientProxy(channel);

        Exception exception = await CaptureAsync(() => proxy.SetClientNameAsync("bad", CancellationToken.None));

        await Assert.That(exception is OpcException).IsTrue();
        await Assert.That(((OpcException)exception).ResultId.Code).IsEqualTo(OpcResultId.Fail.Code);
    }

    [Test]
    public async Task Dispatcher_DecodesSetClientName()
    {
        var server = new StubCommonServer();
        var dispatcher = new OpcCommonServerDispatcher(server);
        byte[] request = WritePayload((ref NdrWriter writer) =>
            writer.WriteUnicodeStringPtr("diagnostic-client"));

        DispatchResult result = await dispatcher.DispatchAsync(
            OpcCommonClientProxy.Opnums.SetClientName,
            request,
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(server.ClientName).IsEqualTo("diagnostic-client");
    }

    [Test]
    public async Task Dispatcher_UnknownOpnumReturnsNotImplemented()
    {
        var dispatcher = new OpcCommonServerDispatcher(new StubCommonServer());

        DispatchResult result = await dispatcher.DispatchAsync(999, ReadOnlyMemory<byte>.Empty, CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(result.Payload.IsEmpty).IsTrue();
    }

    private static byte[] WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[512];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static async Task<Exception> CaptureAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return ex;
        }

        throw new InvalidOperationException("Expected an exception.");
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed class StubCommonServer : IOpcCommonServer
    {
        public string? ClientName { get; private set; }

        public Task SetClientNameAsync(string clientName, CancellationToken cancellationToken = default)
        {
            ClientName = clientName;
            return Task.CompletedTask;
        }
    }
}
