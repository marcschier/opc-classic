//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Batch.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Batch.Tests.Dcom;

public sealed class IOPCBatchProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task BatchServer_GetDelimiter_invokes_channel_and_decodes_string()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("/"));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCBatchServerClientProxy(channel);
        string delimiter = await proxy.GetDelimiterAsync(CancellationToken.None);

        int expectedOpnum = IOPCBatchServer.Opnums.GetDelimiterAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCBatchServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(delimiter).IsEqualTo("/");
    }

    [Test]
    public async Task BatchSummaryEnumerator_Count_decodes_int32()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(3));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IEnumOPCBatchSummaryClientProxy(channel);
        int count = await proxy.CountAsync(CancellationToken.None);

        int expectedOpnum = IEnumOPCBatchSummary.Opnums.CountAsync;
        await Assert.That(observedIid).IsEqualTo(IEnumOPCBatchSummary.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(count).IsEqualTo(3);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 256)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }
}
