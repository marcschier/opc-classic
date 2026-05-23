//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Dx.Dcom;
using OpcClassic.Ndr;
using OpcClassic.Testing;
using TUnit.Core;

namespace OpcClassic.Dx.Tests.Dcom;

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

        var proxy = new IOPCDXServer_ClientProxy(channel);
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

        var proxy = new IOPCConfiguration_ClientProxy(channel);
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
