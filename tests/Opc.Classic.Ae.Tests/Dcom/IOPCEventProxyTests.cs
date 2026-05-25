//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // Generated proxy metadata assertions intentionally observe captured locals.

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Ae.Tests.Dcom;

public sealed class IOPCEventProxyTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task EventServer_GetStatus_invokes_channel_with_metadata_and_decodes_status()
    {
        var expected = new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
            CurrentTime = new DateTimeOffset(2024, 1, 2, 3, 4, 6, TimeSpan.Zero),
            LastUpdateTime = new DateTimeOffset(2024, 1, 2, 3, 4, 7, TimeSpan.Zero),
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 10, 42),
            VendorInfo = "AE Test Server",
        };
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
            NdrOpcEventServerStatusCodec.Write(ref writer, expected));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEventServerClientProxy(channel);
        OpcServerStatus actual = await proxy.GetStatusAsync(CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventServer.Opnums.GetStatusAsync);
        await Assert.That(actual.Spec).IsEqualTo(OpcStatusSpec.Ae);
        await Assert.That(actual.VendorInfo).IsEqualTo(expected.VendorInfo);
        await Assert.That(actual.ServerVersion).IsEqualTo(expected.ServerVersion);
    }

    [Test]
    public async Task EventServer_QueryConditionNames_invokes_channel_with_metadata_and_decodes_names()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WriteStringArray("High", "Low");
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEventServerClientProxy(channel);
        string[] actual = await proxy.QueryConditionNamesAsync(7, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEventServer.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventServer.Opnums.QueryConditionNamesAsync);
        await Assert.That(actual).IsEquivalentTo(["High", "Low"]);
    }

    [Test]
    public async Task EventSubscriptionMgt_SetFilter_invokes_channel_with_metadata_and_encodes_payload()
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

        var proxy = new IOPCEventSubscriptionMgtClientProxy(channel);
        await proxy.SetFilterAsync(7, [1, 2], 100, 900, ["Area1"], ["Source1"], CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEventSubscriptionMgt.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.SetFilterAsync);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
    }

    [Test]
    public async Task EventSubscriptionMgt_GetReturnedAttributes_invokes_channel_with_metadata_and_decodes_ids()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WriteIntArray(10, 20, 30);
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEventSubscriptionMgtClientProxy(channel);
        int[] actual = await proxy.GetReturnedAttributesAsync(2, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEventSubscriptionMgt.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventSubscriptionMgt.Opnums.GetReturnedAttributesAsync);
        await Assert.That(actual).IsEquivalentTo([10, 20, 30]);
    }

    [Test]
    public async Task EventAreaBrowser_GetQualifiedAreaName_invokes_channel_with_metadata_and_decodes_name()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
            writer.WriteUnicodeStringPtr("Plant1.Area1"));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEventAreaBrowserClientProxy(channel);
        string actual = await proxy.GetQualifiedAreaNameAsync("Area1", CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEventAreaBrowser.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventAreaBrowser.Opnums.GetQualifiedAreaNameAsync);
        await Assert.That(actual).IsEqualTo("Plant1.Area1");
    }

    [Test]
    public async Task EventSink_OnEvent_invokes_channel_with_metadata_and_encodes_notifications()
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

        var proxy = new IOPCEventSinkClientProxy(channel);
        await proxy.OnEventAsync(123, refresh: true, lastRefresh: false, [CreateNotification()], CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEventSink.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventSink.Opnums.OnEventAsync);
        await Assert.That(observedPayloadLength).IsGreaterThan(0);
    }

    [Test]
    public async Task EventSubscriptionMgt2_GetKeepAlive_invokes_channel_with_metadata_and_decodes_value()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) => writer.WriteInt32(5000));
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCEventSubscriptionMgt2ClientProxy(channel);
        int actual = await proxy.GetKeepAliveAsync(CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCEventSubscriptionMgt2.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEventSubscriptionMgt2.Opnums.GetKeepAliveAsync);
        await Assert.That(actual).IsEqualTo(5000);
    }

    private static ReadOnlyMemory<byte> WriteIntArray(params int[] values) =>
        WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(unchecked((uint)values.Length));
            foreach (int value in values)
            {
                writer.WriteInt32(value);
            }
        });

    private static ReadOnlyMemory<byte> WriteStringArray(params string[] values) =>
        WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(unchecked((uint)values.Length));
            foreach (string value in values)
            {
                writer.WriteUnicodeStringPtr(value);
            }
        });

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 2048)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static OpcEventNotification CreateNotification() =>
        new(
            changeMask: 1,
            newState: 2,
            source: "Source1",
            time: new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
            message: "Alarm",
            eventType: 4,
            eventCategory: 7,
            severity: 500,
            conditionName: "Level",
            subconditionName: "High",
            quality: new OpcQuality(192),
            ackRequired: true,
            activeTime: new DateTimeOffset(2024, 1, 2, 3, 4, 6, TimeSpan.Zero),
            cookie: 42,
            eventAttributes: [],
            actorId: "operator");
}
