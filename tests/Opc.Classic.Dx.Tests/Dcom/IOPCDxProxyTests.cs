//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dx.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Dx.Tests.Dcom;

public sealed class IOPCDxProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task DXServer_GetVersion_invokes_channel_and_decodes_string()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("1.0"));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCDXServerClientProxy(channel);
        string version = await proxy.GetVersionAsync(CancellationToken.None);

        int expectedOpnum = IOPCDXServer.Opnums.GetVersionAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCDXServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(version).IsEqualTo("1.0");
    }

    [Test]
    public async Task Configuration_ResetConfiguration_encodes_version_and_decodes_new_version()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        int observedPayloadLength = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteUnicodeStringPtr("v2"));
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCConfigurationClientProxy(channel);
        string newVersion = await proxy.ResetConfigurationAsync("v1", CancellationToken.None);

        int expectedOpnum = IOPCConfiguration.Opnums.ResetConfigurationAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCConfiguration.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
        await Assert.That(newVersion).IsEqualTo("v2");
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 512)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }
}
