//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using V20AsyncIO = Opc.Classic.Da.V20.Dcom.IOPCAsyncIO;
using V20AsyncIOClientProxy = Opc.Classic.Da.V20.Dcom.IOPCAsyncIOClientProxy;
using V20SyncIO = Opc.Classic.Da.V20.Dcom.IOPCSyncIO;
using V20SyncIOClientProxy = Opc.Classic.Da.V20.Dcom.IOPCSyncIOClientProxy;

namespace Opc.Classic.Da.Tests.Dcom;

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

        var proxy = new IOPCGroupStateMgtClientProxy(channel);
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

        var proxy = new IOPCGroupStateMgtClientProxy(channel);
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

        var proxy = new IOPCGroupStateMgtClientProxy(channel);
        var exception = await CaptureAsync<OpcException>(() => proxy.SetNameAsync("BadName", CancellationToken.None));

        int actual = exception.ResultId.Code;
        await Assert.That(actual).IsEqualTo(eFail);
    }

    [Test]
    public async Task ItemIO_WriteVqt_invokes_channel_with_correct_metadata_and_decodes_errors()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeUniqueInt32Array(0)));
        });

        var proxy = new IOPCItemIOClientProxy(channel);
        int[] errors = await proxy.WriteVqtAsync(
            new[] { "Random.Int4" },
            new[] { new OpcItemVqt(OpcVariant.FromInt32(123)) },
            CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCItemIO.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCItemIO.Opnums.WriteVqtAsync);
        await Assert.That(errors[0]).IsEqualTo(0);
    }

    [Test]
    public async Task Browse_GetProperties_invokes_channel_with_correct_metadata_and_decodes_properties()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
        {
            NdrOpcBrowseResponseDecoder.WriteItemPropertiesConformantArray(
                ref writer,
                [new OpcItemProperties(0, [
                    new OpcItemPropertyResult(
                        VarType.VT_I4,
                        100,
                        "Random.Int4",
                        "Value",
                        OpcVariant.FromInt32(88),
                        0),
                ])]);
        });
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        var proxy = new IOPCBrowseClientProxy(channel);
        OpcItemProperties[] actual = await proxy.GetPropertiesAsync(
            new[] { "Random.Int4" },
            returnPropertyValues: true,
            propertyIds: new[] { 100 },
            CancellationToken.None);

        int actualLength = actual.Length;
        await Assert.That(observedIid).IsEqualTo(IOPCBrowse.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCBrowse.Opnums.GetPropertiesAsync);
        await Assert.That(actualLength).IsEqualTo(1);
        await Assert.That(actual[0].Properties.Length).IsEqualTo(1);
        await Assert.That(actual[0].Properties[0].PropertyId).IsEqualTo(100);
        await Assert.That(actual[0].Properties[0].Value.AsInt32()).IsEqualTo(88);
    }

    [Test]
    public async Task Browse_GetProperties_decodes_non_null_property_pointer_when_num_properties_is_zero()
    {
        OpcItemPropertyResult property = new(
            VarType.VT_I4,
            100,
            "Random.Int4",
            "Value",
            OpcVariant.FromInt32(88),
            0);
        ReadOnlyMemory<byte> responsePayload = WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteUniquePointerReferent(true);     // ppItemProperties outer referent
            writer.WriteUInt32(1);                        // max_count of OPCITEMPROPERTIES array
            writer.WriteInt32(0);                         // hrErrorId
            writer.WriteUInt32(0);                        // dwNumProperties = 0
            writer.WriteUniquePointerReferent(true);     // pItemProperties non-null pointer
            writer.WriteUInt32(0);                        // reserved
            // Deferred body: emit the conformant array of OPCITEMPROPERTY in the
            // C706 §14.3.12.3 deferred-pile shape (per-element inline parts with
            // string referents, then per-element deferred string/variant bodies).
            // This matches the live-Matrikon layout the reader now expects.
            NdrOpcBrowseResponseDecoder.WriteItemPropertyConformantArray(ref writer, [property]);
        });
        var channel = new InMemoryCallChannel((_, _, _, _) => Task.FromResult(new NdrCallResult(0, responsePayload)));
        var proxy = new IOPCBrowseClientProxy(channel);

        OpcItemProperties[] actual = await proxy.GetPropertiesAsync(
            new[] { "Random.Int4" },
            returnPropertyValues: true,
            propertyIds: [],
            CancellationToken.None);

        await Assert.That(actual[0].Properties.Length).IsEqualTo(1);
        await Assert.That(actual[0].Properties[0].PropertyId).IsEqualTo(100);
        await Assert.That(actual[0].Properties[0].Value.AsInt32()).IsEqualTo(88);
    }

    [Test]
    public async Task BrowseServerAddressSpace_GetItemId_invokes_channel_with_correct_metadata()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeString("Channel1.Device1.Tag1")));
        });

        var proxy = new IOPCBrowseServerAddressSpaceClientProxy(channel);
        string actual = await proxy.GetItemIdAsync("Tag1", CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCBrowseServerAddressSpace.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCBrowseServerAddressSpace.Opnums.GetItemIdAsync);
        await Assert.That(actual).IsEqualTo("Channel1.Device1.Tag1");
    }

    [Test]
    public async Task ItemMgt_SetActiveState_invokes_channel_with_correct_metadata_and_decodes_errors()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeUniqueInt32Array(0)));
        });

        var proxy = new IOPCItemMgtClientProxy(channel);
        int[] errors = await proxy.SetActiveStateAsync(new[] { 42 }, active: true, CancellationToken.None);

        int firstError = errors[0];
        await Assert.That(observedIid).IsEqualTo(IOPCItemMgt.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCItemMgt.Opnums.SetActiveStateAsync);
        await Assert.That(firstError).IsEqualTo(0);
    }

    [Test]
    public async Task SyncIO_Write_invokes_channel_with_correct_metadata_and_decodes_errors()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeUniqueInt32Array(0)));
        });

        var proxy = new IOPCSyncIOClientProxy(channel);
        int[] errors = await proxy.WriteAsync(Array.Empty<int>(), Array.Empty<OpcVariant>(), CancellationToken.None);

        int errorCount = errors.Length;
        await Assert.That(observedIid).IsEqualTo(IOPCSyncIO.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCSyncIO.Opnums.WriteAsync);
        await Assert.That(errorCount).IsEqualTo(1);
    }

    [Test]
    public async Task SyncIO2_WriteVqt_invokes_channel_with_correct_metadata_and_decodes_errors()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeUniqueInt32Array(0)));
        });

        var proxy = new IOPCSyncIO2ClientProxy(channel);
        int[] errors = await proxy.WriteVqtAsync(Array.Empty<int>(), Array.Empty<OpcItemVqt>(), CancellationToken.None);

        int errorCount = errors.Length;
        await Assert.That(observedIid).IsEqualTo(IOPCSyncIO2.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCSyncIO2.Opnums.WriteVqtAsync);
        await Assert.That(errorCount).IsEqualTo(1);
    }

    [Test]
    public async Task AsyncIO2_GetEnable_invokes_channel_with_correct_metadata_and_decodes_boolean()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeBoolean(true)));
        });

        var proxy = new IOPCAsyncIO2ClientProxy(channel);
        bool enabled = await proxy.GetEnableAsync(CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCAsyncIO2.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCAsyncIO2.Opnums.GetEnableAsync);
        await Assert.That(enabled).IsEqualTo(true);
    }

    [Test]
    public async Task AsyncIO3_RefreshMaxAge_invokes_channel_with_correct_metadata_and_decodes_cancel_id()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeInt32(0x1234)));
        });

        var proxy = new IOPCAsyncIO3ClientProxy(channel);
        int cancelId = await proxy.RefreshMaxAgeAsync(1000, 0x99, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCAsyncIO3.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCAsyncIO3.Opnums.RefreshMaxAgeAsync);
        await Assert.That(cancelId).IsEqualTo(0x1234);
    }

    [Test]
    public async Task DataCallback_OnCancelComplete_invokes_channel_with_correct_metadata()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        var proxy = new IOPCDataCallbackClientProxy(channel);
        await proxy.OnCancelCompleteAsync(0x22, 0x33, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCDataCallback.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCDataCallback.Opnums.OnCancelCompleteAsync);
    }

    [Test]
    public async Task ConnectionPoint_GetConnectionInterface_invokes_channel_with_correct_metadata()
    {
        Guid expected = IOPCDataCallback.InterfaceId;
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeGuid(expected)));
        });

        var proxy = new IConnectionPointClientProxy(channel);
        Guid actual = await proxy.GetConnectionInterfaceAsync(CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IConnectionPoint.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IConnectionPoint.Opnums.GetConnectionInterfaceAsync);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task ConnectionPoint_Advise_and_Unadvise_round_trip_sink_and_cookie()
    {
        var sink = new OpcInterfaceRef(
            IOPCDataCallback.InterfaceId,
            flags: 1,
            publicRefs: 2,
            oxid: 3,
            oid: 4,
            ipid: Guid.Parse("11111111-2222-3333-4444-555555555555"),
            securityOffset: 0,
            resolverBindings: new ushort[] { 7, 8, 0 });
        IOpcInterfaceRef? observedSink = null;
        int observedCookie = 0;
        int calls = 0;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            Ensure(iid == IConnectionPoint.InterfaceId);
            calls++;
            var reader = new NdrReader(payload.Span);
            if (opnum == IConnectionPoint.Opnums.AdviseAsync)
            {
                observedSink = OpcInterfaceRefCodec.Read(ref reader);
                return Task.FromResult(new NdrCallResult(0, EncodeInt32(0x1234)));
            }

            Ensure(opnum == IConnectionPoint.Opnums.UnadviseAsync);
            observedCookie = reader.ReadInt32();
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        var proxy = new IConnectionPointClientProxy(channel);
        int cookie = await proxy.AdviseAsync(sink, CancellationToken.None);
        await proxy.UnadviseAsync(cookie, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(2);
        await Assert.That(cookie).IsEqualTo(0x1234);
        await Assert.That(observedCookie).IsEqualTo(0x1234);
        await Assert.That(observedSink is not null).IsTrue();
        await Assert.That(observedSink!.Iid).IsEqualTo(IOPCDataCallback.InterfaceId);
        await Assert.That(observedSink!.Ipid).IsEqualTo(sink.Ipid);
    }

    [Test]
    public async Task Common_methods_round_trip_locale_error_text_and_client_name()
    {
        int calls = 0;
        bool setLocaleObserved = false;
        bool setClientNameObserved = false;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            Ensure(iid == IOPCCommon.InterfaceId);
            calls++;
            var reader = new NdrReader(payload.Span);
            if (opnum == IOPCCommon.Opnums.SetLocaleIdAsync)
            {
                setLocaleObserved = reader.ReadInt32() == 0x0407;
                return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
            }

            if (opnum == IOPCCommon.Opnums.GetLocaleIdAsync)
            {
                return Task.FromResult(new NdrCallResult(0, EncodeInt32(0x0409)));
            }

            if (opnum == IOPCCommon.Opnums.QueryAvailableLocaleIdsAsync)
            {
                return Task.FromResult(new NdrCallResult(0, EncodeInt32Array(0x0409, 0x0407)));
            }

            if (opnum == IOPCCommon.Opnums.GetErrorStringAsync)
            {
                Ensure(reader.ReadInt32() == unchecked((int)0x80004005u));
                return Task.FromResult(new NdrCallResult(0, EncodeString("Failure")));
            }

            Ensure(opnum == IOPCCommon.Opnums.SetClientNameAsync);
            setClientNameObserved = reader.ReadUnicodeStringPtr() == "opc-client";
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        var proxy = new IOPCCommonClientProxy(channel);
        await proxy.SetLocaleIdAsync(0x0407, CancellationToken.None);
        int localeId = await proxy.GetLocaleIdAsync(CancellationToken.None);
        int[] localeIds = await proxy.QueryAvailableLocaleIdsAsync(CancellationToken.None);
        string errorText = await proxy.GetErrorStringAsync(unchecked((int)0x80004005u), CancellationToken.None);
        await proxy.SetClientNameAsync("opc-client", CancellationToken.None);

        await Assert.That(calls).IsEqualTo(5);
        await Assert.That(setLocaleObserved).IsTrue();
        await Assert.That(setClientNameObserved).IsTrue();
        await Assert.That(localeId).IsEqualTo(0x0409);
        await Assert.That(localeIds[1]).IsEqualTo(0x0407);
        await Assert.That(errorText).IsEqualTo("Failure");
    }

    [Test]
    public async Task ShutdownRequest_invokes_channel_with_reason()
    {
        string? observedReason = null;
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            Ensure(iid == IOPCShutdown.InterfaceId);
            Ensure(opnum == IOPCShutdown.Opnums.ShutdownRequestAsync);
            var reader = new NdrReader(payload.Span);
            observedReason = reader.ReadUnicodeStringPtr();
            return Task.FromResult(new NdrCallResult(0, ReadOnlyMemory<byte>.Empty));
        });

        var proxy = new IOPCShutdownClientProxy(channel);
        await proxy.ShutdownRequestAsync("maintenance", CancellationToken.None);

        await Assert.That(observedReason).IsEqualTo("maintenance");
    }

    [Test]
    public async Task EnumGuid_Next_invokes_channel_with_correct_metadata_and_decodes_guids()
    {
        Guid expected = Guid.Parse("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeGuidArray(expected)));
        });

        var proxy = new IOPCEnumGUIDClientProxy(channel);
        Guid[] actual = await proxy.NextAsync(1, CancellationToken.None);

        Guid first = actual[0];
        await Assert.That(observedIid).IsEqualTo(IOPCEnumGUID.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCEnumGUID.Opnums.NextAsync);
        await Assert.That(first).IsEqualTo(expected);
    }

    [Test]
    public async Task ServerList_ClsidFromProgId_invokes_channel_with_correct_metadata()
    {
        Guid expected = Guid.Parse("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeGuid(expected)));
        });

        var proxy = new IOPCServerListClientProxy(channel);
        Guid actual = await proxy.ClsidFromProgIdAsync("Matrikon.OPC.Simulation.1", CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCServerList.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCServerList.Opnums.ClsidFromProgIdAsync);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task ServerList2_ClsidFromProgId_invokes_channel_with_correct_metadata()
    {
        Guid expected = Guid.Parse("39C13A4D-011E-11D0-9675-0020AFD8ADB3");
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeGuid(expected)));
        });

        var proxy = new IOPCServerList2ClientProxy(channel);
        Guid actual = await proxy.ClsidFromProgIdAsync("Matrikon.OPC.Simulation.1", CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(IOPCServerList2.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(IOPCServerList2.Opnums.ClsidFromProgIdAsync);
        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test]
    public async Task V20SyncIO_Read_invokes_channel_with_correct_metadata_and_decodes_states_and_errors()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var expected = new OpcItemState(10, DateTimeOffset.UnixEpoch, new OpcQuality(192), OpcVariant.FromInt32(42));
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            var reader = new NdrReader(payload.Span);
            Ensure(reader.ReadInt32() == 1);
            Ensure(reader.ReadConformantInt32Array()[0] == 100);
            return Task.FromResult(new NdrCallResult(0, EncodeItemStates(expected, 0)));
        });

        var proxy = new V20SyncIOClientProxy(channel);
        OpcItemState[] states = await proxy.ReadAsync(1, new[] { 100 }, out int[] errors, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(V20SyncIO.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(V20SyncIO.Opnums.ReadAsync);
        await Assert.That(states[0]).IsEqualTo(expected);
        await Assert.That(errors[0]).IsEqualTo(0);
    }

    [Test]
    public async Task V20SyncIO_Write_invokes_channel_with_correct_metadata_and_decodes_errors()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeInt32Array(0)));
        });

        var proxy = new V20SyncIOClientProxy(channel);
        int[] errors = await proxy.WriteAsync(Array.Empty<int>(), Array.Empty<OpcVariant>(), CancellationToken.None);

        int errorCount = errors.Length;
        await Assert.That(observedIid).IsEqualTo(V20SyncIO.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(V20SyncIO.Opnums.WriteAsync);
        await Assert.That(errorCount).IsEqualTo(1);
    }

    [Test]
    public async Task V20AsyncIO_Refresh_invokes_channel_with_correct_metadata_and_decodes_transaction_id()
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        var channel = new InMemoryCallChannel((iid, opnum, _, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            return Task.FromResult(new NdrCallResult(0, EncodeInt32(0x5678)));
        });

        var proxy = new V20AsyncIOClientProxy(channel);
        int transactionId = await proxy.RefreshAsync(1, 2, CancellationToken.None);

        await Assert.That(observedIid).IsEqualTo(V20AsyncIO.InterfaceId);
        await Assert.That(observedOpnum).IsEqualTo(V20AsyncIO.Opnums.RefreshAsync);
        await Assert.That(transactionId).IsEqualTo(0x5678);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 1024)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static ReadOnlyMemory<byte> EncodeBoolean(bool value) => EncodeInt32(value ? -1 : 0);

    private static ReadOnlyMemory<byte> EncodeGuid(Guid value) => WritePayload((ref NdrWriter writer) =>
        writer.WriteGuid(value));

    private static ReadOnlyMemory<byte> EncodeGuidArray(params Guid[] values) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUInt32((uint)values.Length);
        foreach (Guid value in values)
        {
            writer.WriteGuid(value);
        }
    });

    private static ReadOnlyMemory<byte> EncodeInt32(int value) => WritePayload((ref NdrWriter writer) =>
        writer.WriteInt32(value));

    private static ReadOnlyMemory<byte> EncodeInt32Array(params int[] values) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUInt32((uint)values.Length);
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    });

    /// <summary>
    /// Encodes a unique-pointer-prefixed conformant Int32 array for <c>[out, size_is(,N)] HRESULT**</c> wire shape.
    /// Use for response payloads where the C# proxy carries <c>[OpcUniquePointer]</c> on the array.
    /// </summary>
    private static ReadOnlyMemory<byte> EncodeUniqueInt32Array(params int[] values) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUniquePointerReferent(true);    // unique-pointer referent (non-null)
        writer.WriteUInt32((uint)values.Length);    // max_count
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    });

    private static ReadOnlyMemory<byte> EncodeItemStates(OpcItemState state, int error) => WritePayload((ref NdrWriter writer) =>
    {
        writer.WriteUInt32(1);
        NdrOpcItemStateCodec.Write(ref writer, state);
        writer.WriteUInt32(1);
        writer.WriteInt32(error);
    });

    private static ReadOnlyMemory<byte> EncodeString(string value) => WritePayload((ref NdrWriter writer) =>
        writer.WriteUnicodeStringPtr(value));

    private static void Ensure(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Unexpected NDR payload.");
        }
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
