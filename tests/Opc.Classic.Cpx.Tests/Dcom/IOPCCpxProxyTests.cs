//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Cpx.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Cpx.Tests.Dcom;

public sealed class IOPCCpxProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task ComplexDataItem_GetTypeItemID_invokes_channel_and_decodes_string()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        int observedPayloadLength = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("Types.Motor"));
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCComplexDataItemClientProxy(channel);
        string typeItemId = await proxy.GetTypeItemIDAsync("Device.Motor", CancellationToken.None);

        int expectedOpnum = IOPCComplexDataItem.Opnums.GetTypeItemIDAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCComplexDataItem.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
        await Assert.That(typeItemId).IsEqualTo("Types.Motor");
    }

    [Test]
    public async Task ComplexDataItem2_GetTypeID_decodes_guid()
    {
        Guid expectedTypeId = Guid.NewGuid();
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteGuid(expectedTypeId));
        var channel = new InMemoryCallChannel(static (_, _, _, _) => Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty)));
        channel = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, responsePayload)));

        var proxy = new IOPCComplexDataItem2ClientProxy(channel);
        Guid actualTypeId = await proxy.GetTypeIDAsync("Device.Motor", CancellationToken.None);

        await Assert.That(actualTypeId).IsEqualTo(expectedTypeId);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 512)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }
}
