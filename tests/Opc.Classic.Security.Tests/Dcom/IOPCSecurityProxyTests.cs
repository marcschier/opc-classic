//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ndr;
using Opc.Classic.Security.Dcom;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Security.Tests.Dcom;

public sealed class IOPCSecurityProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task SecurityNT_IsAvailable_decodes_boolean()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(-1));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCSecurityNTClientProxy(channel);
        bool available = await proxy.IsAvailableNTAsync(CancellationToken.None);

        int expectedOpnum = IOPCSecurityNT.Opnums.IsAvailableNTAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCSecurityNT.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(available).IsTrue();
    }

    [Test]
    public async Task SecurityPrivate_Logon_encodes_credentials()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        int observedPayloadLength = -1;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        var proxy = new IOPCSecurityPrivateClientProxy(channel);
        await proxy.LogonAsync("operator", "password", CancellationToken.None);

        int expectedOpnum = IOPCSecurityPrivate.Opnums.LogonAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCSecurityPrivate.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 128)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }
}
