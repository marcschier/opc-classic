//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic;
using OpcClassic.Da.Dcom;
using OpcClassic.Da.Ndr;
using OpcClassic.Ndr;
using OpcClassic.Testing;
using TUnit.Core;

namespace OpcClassic.Da.Tests.Dcom;

public sealed class IOPCAdditionalDaProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task GroupState_GetState_invokes_channel_with_correct_metadata_and_decodes_state()
    {
        var expected = new OpcGroupState(
            ClientHandle: 0x1234,
            ServerHandle: 0x5678,
            Name: "BatchLine",
            Active: true,
            UpdateRate: 1000,
            TimeBias: -60,
            PercentDeadband: 1.5f,
            LocaleId: 0x0409);
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
            NdrOpcGroupStateCodec.Write(ref writer, expected));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCGroupStateMgt_ClientProxy(channel);
        OpcGroupState actual = await proxy.GetStateAsync(CancellationToken.None);

        int expectedOpnum = IOPCGroupStateMgt.Opnums.GetStateAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCGroupStateMgt.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task GroupState_SetName_invokes_channel_with_correct_metadata_and_encodes_payload()
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

        var proxy = new IOPCGroupStateMgt_ClientProxy(channel);
        await proxy.SetNameAsync("Renamed", CancellationToken.None);

        int expectedOpnum = IOPCGroupStateMgt.Opnums.SetNameAsync;
        await Assert.That(observedIid).IsEqualTo(IOPCGroupStateMgt.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
    }

    [Test]
    public async Task GroupState_SetName_failure_throws_OpcException()
    {
        int eFail = EFail();
        var channel = new InMemoryCallChannel((_, _, _, _) =>
            Task.FromResult(new NdrCallResult(eFail, ReadOnlyMemory<byte>.Empty)));

        var proxy = new IOPCGroupStateMgt_ClientProxy(channel);
        var exception = await CaptureAsync<OpcException>(() => proxy.SetNameAsync("BadName", CancellationToken.None));

        int actual = exception.ResultId.Code;
        await Assert.That(actual).IsEqualTo(eFail);
    }

    [Test]
    public async Task ItemIO_GetProperties_invokes_channel_with_correct_metadata_and_decodes_properties()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
            NdrOpcItemPropertiesCodec.Write(ref writer, new OpcItemProperties(0, Array.Empty<OpcItemPropertyResult>())));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCItemIO_ClientProxy(channel);
        OpcItemProperties actual = await proxy.GetPropertiesAsync("Random.Int4", 100, CancellationToken.None);

        int expectedOpnum = IOPCItemIO.Opnums.GetPropertiesAsync;
        int propertyCount = actual.Properties.Length;
        await Assert.That(observedIid).IsEqualTo(IOPCItemIO.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(actual.ErrorId).IsEqualTo(0);
        await Assert.That(propertyCount).IsEqualTo(0);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 1024)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }

    private static int EFail() => unchecked((int)0x80004005u);

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
