//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Commands.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Commands.Tests.Dcom;

public sealed class IOPCCommandsProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task CommandInformation_ListCommands_invokes_channel_and_decodes_string_array()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(2);
            writer.WriteUnicodeStringPtr("Start");
            writer.WriteUnicodeStringPtr("Stop");
        });
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCCommandInformationClientProxy(channel);
        string[] commands = await proxy.ListCommandsAsync(CancellationToken.None);

        int expectedOpnum = IOPCCommandInformation.Opnums.ListCommandsAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCCommandInformation.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(commands.Length).IsEqualTo(2);
        await Assert.That(commands[0]).IsEqualTo("Start");
    }

    [Test]
    public async Task CommandExecution_Control_encodes_payload_and_uses_opnum()
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

        var proxy = new IOPCCommandExecutionClientProxy(channel);
        await proxy.ControlAsync("invoke-1", "Cancel", CancellationToken.None);

        int expectedOpnum = IOPCCommandExecution.Opnums.ControlAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCCommandExecution.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 512)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }
}
