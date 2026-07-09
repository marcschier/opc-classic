// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Da.Tests.Dcom;

public sealed class IOPCMissingDaMethodRoundTripTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    public async Task Browse_round_trips_continuation_more_and_elements()
    {
        bool serverObservedArgs = false;
        var channel = Channel(IOPCBrowse.InterfaceId, IOPCBrowse.Opnums.BrowseAsync, (ref NdrReader reader) =>
        {
            string? itemId = reader.ReadUnicodeString();
            string? continuation = reader.ReadUnicodeStringPtr();
            int maxElements = reader.ReadInt32();
            int browseFilter = reader.ReadInt32();
            string? elementFilter = reader.ReadUnicodeString();
            string? vendorFilter = reader.ReadUnicodeString();
            bool allProperties = reader.ReadInt32() != 0;
            bool propertyValues = reader.ReadInt32() != 0;
            _ = reader.ReadInt32();   // dwPropertyCount sibling (consumed; the conformant array carries its own max_count)
            int[] propertyIds = ReadInt32Array(ref reader);

            serverObservedArgs = itemId == "Area" &&
                continuation == "start" &&
                maxElements == 10 &&
                browseFilter == 2 &&
                elementFilter == "*" &&
                vendorFilter == "vendor" &&
                allProperties &&
                !propertyValues &&
                propertyIds.Length == 1 &&
                propertyIds[0] == 100;

            var element = new OpcBrowseElementResult(
                "Tag1",
                "Area.Tag1",
                2,
                new OpcItemProperties(0, Array.Empty<OpcItemPropertyResult>()));
            return WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteUnicodeStringPtr("next");
                writer.WriteInt32(-1);
                NdrOpcBrowseResponseDecoder.WriteConformantArrayWithReferent(ref writer, new[] { element });
            });
        });

        var proxy = new IOPCBrowseClientProxy(channel);
        string? continuationPoint = "start";
        await proxy.BrowseAsync(
            "Area",
            ref continuationPoint,
            10,
            2,
            "*",
            "vendor",
            returnAllProperties: true,
            returnPropertyValues: false,
            propertyIds: new[] { 100 },
            out bool moreElements,
            out OpcBrowseElementResult[] browseElements,
            CancellationToken.None);

        await Assert.That(serverObservedArgs).IsTrue();
        await Assert.That(continuationPoint).IsEqualTo("next");
        await Assert.That(moreElements).IsTrue();
        await Assert.That(browseElements[0].ItemId).IsEqualTo("Area.Tag1");
    }

    [Test]
    public async Task GroupState_SetState_round_trips_revised_update_rate()
    {
        bool serverObservedArgs = false;
        var channel = Channel(IOPCGroupStateMgt.InterfaceId, IOPCGroupStateMgt.Opnums.SetStateAsync, (ref NdrReader reader) =>
        {
            serverObservedArgs = reader.ReadInt32() == 1000 &&
                (reader.ReadInt32() != 0) &&
                reader.ReadInt32() == -60 &&
                Math.Abs(reader.ReadSingle() - 1.25f) < 0.001f &&
                reader.ReadInt32() == 0x0409 &&
                reader.ReadInt32() == 0x1234;
            return EncodeInt32(950);
        });

        var proxy = new IOPCGroupStateMgtClientProxy(channel);
        await proxy.SetStateAsync(1000, true, -60, 1.25f, 0x0409, 0x1234, out int revised, CancellationToken.None);

        await Assert.That(serverObservedArgs).IsTrue();
        await Assert.That(revised).IsEqualTo(950);
    }

    [Test]
    public async Task ItemMgt_AddItems_and_ValidateItems_round_trip_results_and_errors()
    {
        int addCalls = 0;
        int validateCalls = 0;
        var channel = Channel(IOPCItemMgt.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            // AddItems/ValidateItems IDL: [in] DWORD dwCount, [in, size_is(dwCount)] OPCITEMDEF*.
            // After AF1, proxy emits dwCount before the array's max_count.
            _ = reader.ReadUInt32();
            OpcItemDef[] definitions = NdrOpcItemDefCodec.ReadConformantArray(ref reader);
            if (opnum == IOPCItemMgt.Opnums.AddItemsAsync)
            {
                addCalls++;
                Ensure(definitions[0].ItemId == "Bucket.Level");
                return EncodeItemResults(101, 0);
            }

            Ensure(opnum == IOPCItemMgt.Opnums.ValidateItemsAsync);
            validateCalls++;
            bool blobUpdate = reader.ReadInt32() != 0;
            Ensure(blobUpdate);
            Ensure(definitions[0].ClientHandle == 77);
            return EncodeItemResults(202, unchecked((int)0x80004005u));
        });

        var proxy = new IOPCItemMgtClientProxy(channel);
        var item = new OpcItemDef(null, "Bucket.Level", true, 77, Array.Empty<byte>(), VarType.VT_I4);
        await proxy.AddItemsAsync(new[] { item }, out OpcItemResult[] addResults, out int[] addErrors, CancellationToken.None);
        await proxy.ValidateItemsAsync(new[] { item }, blobUpdate: true, out OpcItemResult[] validateResults, out int[] validateErrors, CancellationToken.None);

        await Assert.That(addCalls).IsEqualTo(1);
        await Assert.That(validateCalls).IsEqualTo(1);
        await Assert.That(addResults[0].ServerHandle).IsEqualTo(101);
        await Assert.That(addErrors[0]).IsEqualTo(0);
        await Assert.That(validateResults[0].ServerHandle).IsEqualTo(202);
        await Assert.That(validateErrors[0]).IsEqualTo(unchecked((int)0x80004005u));
    }

    [Test]
    public async Task ItemMgt_CreateEnumerator_and_GroupState_Clone_round_trip_objrefs()
    {
        var itemChannel = Channel(IOPCItemMgt.InterfaceId, IOPCItemMgt.Opnums.CreateEnumeratorAsync, (ref NdrReader reader) =>
        {
            Ensure(reader.ReadGuid() == OpcGuids.IID_IEnumOPCItemAttributes);
            return EncodeInterfaceRef(OpcGuids.IID_IEnumOPCItemAttributes, 0x55);
        });
        var groupChannel = Channel(IOPCGroupStateMgt.InterfaceId, IOPCGroupStateMgt.Opnums.CloneGroupAsync, (ref NdrReader reader) =>
        {
            Ensure(reader.ReadUnicodeStringPtr() == "Clone.A");
            Ensure(reader.ReadGuid() == IOPCItemMgt.InterfaceId);
            return EncodeInterfaceRef(IOPCItemMgt.InterfaceId, 0x56);
        });

        IOpcInterfaceRef itemEnumerator = await new IOPCItemMgtClientProxy(itemChannel)
            .CreateEnumeratorAsync(OpcGuids.IID_IEnumOPCItemAttributes, CancellationToken.None);
        IOpcInterfaceRef clone = await new IOPCGroupStateMgtClientProxy(groupChannel)
            .CloneGroupAsync("Clone.A", IOPCItemMgt.InterfaceId, CancellationToken.None);

        await Assert.That(itemEnumerator.Iid).IsEqualTo(OpcGuids.IID_IEnumOPCItemAttributes);
        await Assert.That(clone.Iid).IsEqualTo(IOPCItemMgt.InterfaceId);
    }

    [Test]
    public async Task SyncIO_Read_round_trips_for_DA2_and_DA3_interfaces()
    {
        var state = new OpcItemState(11, DateTimeOffset.UnixEpoch, new OpcQuality(192), OpcVariant.FromInt32(42));

        var syncChannel = Channel(IOPCSyncIO.InterfaceId, IOPCSyncIO.Opnums.ReadAsync, (ref NdrReader reader) =>
        {
            Ensure(reader.ReadInt32() == 1);
            _ = reader.ReadUInt32();  // [OpcEmitArrayCount] dwCount sibling for serverHandles
            Ensure(ReadInt32Array(ref reader)[0] == 500);
            return EncodeItemStates(state, 0);
        });
        var syncProxy = new IOPCSyncIOClientProxy(syncChannel);
        OpcItemState[] states = await syncProxy.ReadAsync(1, new[] { 500 }, out int[] errors, CancellationToken.None);

        var sync2Channel = Channel(IOPCSyncIO2.InterfaceId, IOPCSyncIO2.Opnums.ReadAsync, (ref NdrReader reader) =>
        {
            Ensure(reader.ReadInt32() == 2);
            _ = reader.ReadUInt32();  // [OpcEmitArrayCount] dwCount sibling for serverHandles
            Ensure(ReadInt32Array(ref reader)[0] == 501);
            return EncodeItemStates(state with { ClientHandle = 12 }, 1);
        });
        var sync2Proxy = new IOPCSyncIO2ClientProxy(sync2Channel);
        OpcItemState[] states2 = await sync2Proxy.ReadAsync(2, new[] { 501 }, out int[] errors2, CancellationToken.None);

        await Assert.That(states[0].Value).IsEqualTo(OpcVariant.FromInt32(42));
        await Assert.That(errors[0]).IsEqualTo(0);
        await Assert.That(states2[0].ClientHandle).IsEqualTo(12);
        await Assert.That(errors2[0]).IsEqualTo(1);
    }

    [Test]
    public async Task SyncIO2_ReadMaxAge_round_trips_values_qualities_timestamps_and_errors()
    {
        var channel = Channel(IOPCSyncIO2.InterfaceId, IOPCSyncIO2.Opnums.ReadMaxAgeAsync, (ref NdrReader reader) =>
        {
            _ = reader.ReadUInt32();  // [OpcEmitArrayCount] dwCount sibling
            Ensure(ReadInt32Array(ref reader)[0] == 777);
            Ensure(ReadInt32Array(ref reader)[0] == 250);
            return WritePayload((ref NdrWriter writer) =>
            {
                // [out, size_is(,N)] VARIANT** values — [OpcUniquePointer, OpcVariantElements] layout
                writer.WriteUniquePointerReferent(true);
                WriteVariantElementsArray(ref writer, OpcVariant.FromString("fresh"));
                WriteUInt16Array(ref writer, 192);
                // [out, size_is(,N)] FILETIME** timestamps — [OpcUniquePointer, OpcFileTimeElements]
                writer.WriteUniquePointerReferent(true);
                writer.WriteConformantFileTimeArray(new[] { 1234L });
                WriteInt32Array(ref writer, 0);
            });
        });

        var proxy = new IOPCSyncIO2ClientProxy(channel);
        await proxy.ReadMaxAgeAsync(new[] { 777 }, new[] { 250 }, out OpcVariant[] values, out ushort[] qualities, out long[] timestamps, out int[] errors, CancellationToken.None);

        await Assert.That(values[0].AsString()).IsEqualTo("fresh");
        await Assert.That(qualities[0]).IsEqualTo((ushort)192);
        await Assert.That(timestamps[0]).IsEqualTo(1234L);
        await Assert.That(errors[0]).IsEqualTo(0);
    }

    [Test]
    public async Task AsyncIO2_Read_and_Write_round_trip_cancel_id_and_errors()
    {
        int readCalls = 0;
        int writeCalls = 0;
        var channel = Channel(IOPCAsyncIO2.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            _ = reader.ReadUInt32();  // [OpcEmitArrayCount] dwCount sibling for serverHandles
            int[] handles = ReadInt32Array(ref reader);
            if (opnum == IOPCAsyncIO2.Opnums.ReadAsync)
            {
                readCalls++;
                Ensure(handles[0] == 100);
                Ensure(reader.ReadInt32() == 0x10);
                return EncodeCancelAndErrors(0xCA, 0);
            }

            Ensure(opnum == IOPCAsyncIO2.Opnums.WriteAsync);
            writeCalls++;
            Ensure(handles[0] == 101);
            // After AF4, IOPCAsyncIO2.Write values is [OpcVariantElements]; proxy emits
            // canonical wireVARIANT elements (with duplicated discriminator). Read accordingly.
            int valuesCount = (int)reader.ReadUInt32();
            Ensure(valuesCount == 1);
            OpcVariant value = NdrVariantExtensions.ReadVariantElement(ref reader);
            Ensure(value.Equals(OpcVariant.FromInt32(7)));
            Ensure(reader.ReadInt32() == 0x11);
            return EncodeCancelAndErrors(0xCB, 1);
        });

        var proxy = new IOPCAsyncIO2ClientProxy(channel);
        int readCancel = await proxy.ReadAsync(new[] { 100 }, 0x10, out int[] readErrors, CancellationToken.None);
        int writeCancel = await proxy.WriteAsync(new[] { 101 }, new[] { OpcVariant.FromInt32(7) }, 0x11, out int[] writeErrors, CancellationToken.None);

        await Assert.That(readCalls).IsEqualTo(1);
        await Assert.That(writeCalls).IsEqualTo(1);
        await Assert.That(readCancel).IsEqualTo(0xCA);
        await Assert.That(readErrors[0]).IsEqualTo(0);
        await Assert.That(writeCancel).IsEqualTo(0xCB);
        await Assert.That(writeErrors[0]).IsEqualTo(1);
    }

    [Test]
    public async Task AsyncIO3_ReadMaxAge_and_WriteVqt_round_trip_cancel_id_and_errors()
    {
        int readCalls = 0;
        int writeCalls = 0;
        var channel = Channel(IOPCAsyncIO3.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            _ = reader.ReadUInt32();  // [OpcEmitArrayCount] dwCount sibling for serverHandles
            int[] handles = ReadInt32Array(ref reader);
            if (opnum == IOPCAsyncIO3.Opnums.ReadMaxAgeAsync)
            {
                readCalls++;
                Ensure(handles[0] == 200);
                Ensure(ReadInt32Array(ref reader)[0] == 1000);
                Ensure(reader.ReadInt32() == 0x20);
                return EncodeCancelAndErrors(0xDA, 0);
            }

            Ensure(opnum == IOPCAsyncIO3.Opnums.WriteVqtAsync);
            writeCalls++;
            Ensure(handles[0] == 201);
            OpcItemVqt[] values = ReadArray(ref reader, NdrOpcItemVqtCodec.Read);
            Ensure(values[0].Value.Equals(OpcVariant.FromInt32(9)));
            Ensure(reader.ReadInt32() == 0x21);
            return EncodeCancelAndErrors(0xDB, 2);
        });

        var proxy = new IOPCAsyncIO3ClientProxy(channel);
        int readCancel = await proxy.ReadMaxAgeAsync(new[] { 200 }, new[] { 1000 }, 0x20, out int[] readErrors, CancellationToken.None);
        int writeCancel = await proxy.WriteVqtAsync(new[] { 201 }, new[] { new OpcItemVqt(OpcVariant.FromInt32(9)) }, 0x21, out int[] writeErrors, CancellationToken.None);

        await Assert.That(readCalls).IsEqualTo(1);
        await Assert.That(writeCalls).IsEqualTo(1);
        await Assert.That(readCancel).IsEqualTo(0xDA);
        await Assert.That(readErrors[0]).IsEqualTo(0);
        await Assert.That(writeCancel).IsEqualTo(0xDB);
        await Assert.That(writeErrors[0]).IsEqualTo(2);
    }

    [Test]
    public async Task BrowseServerAddressSpace_enumstring_methods_round_trip_objrefs()
    {
        int calls = 0;
        var channel = Channel(IOPCBrowseServerAddressSpace.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            calls++;
            if (opnum == IOPCBrowseServerAddressSpace.Opnums.BrowseOpcItemIdsAsync)
            {
                Ensure(reader.ReadInt32() == 2);
                Ensure(reader.ReadUnicodeStringPtr() == "*");
                Ensure(reader.ReadUInt16() == (ushort)VarType.VT_EMPTY);
                Ensure(reader.ReadInt32() == 0x3);
                return EncodeInterfaceRef(OpcGuids.IID_IEnumString, 0x71);
            }

            Ensure(opnum == IOPCBrowseServerAddressSpace.Opnums.BrowseAccessPathsAsync);
            Ensure(reader.ReadUnicodeStringPtr() == "Bucket.Level");
            return EncodeInterfaceRef(OpcGuids.IID_IEnumString, 0x72);
        });

        var proxy = new IOPCBrowseServerAddressSpaceClientProxy(channel);
        IOpcInterfaceRef itemIds = await proxy.BrowseOpcItemIdsAsync(2, "*", (ushort)VarType.VT_EMPTY, 0x3, CancellationToken.None);
        IOpcInterfaceRef accessPaths = await proxy.BrowseAccessPathsAsync("Bucket.Level", CancellationToken.None);

        await Assert.That(calls).IsEqualTo(2);
        await Assert.That(itemIds.Iid).IsEqualTo(OpcGuids.IID_IEnumString);
        await Assert.That(accessPaths.Oid).IsEqualTo((ulong)0x72);
    }

    [Test]
    public async Task GroupStateMgt2_keep_alive_round_trips_revised_and_current_values()
    {
        var channel = Channel(IOPCGroupStateMgt2.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            if (opnum == IOPCGroupStateMgt2.Opnums.SetKeepAliveAsync)
            {
                Ensure(reader.ReadInt32() == 30_000);
                return EncodeInt32(29_000);
            }

            Ensure(opnum == IOPCGroupStateMgt2.Opnums.GetKeepAliveAsync);
            return EncodeInt32(29_000);
        });

        var proxy = new IOPCGroupStateMgt2ClientProxy(channel);
        int revised = await proxy.SetKeepAliveAsync(30_000, CancellationToken.None);
        int current = await proxy.GetKeepAliveAsync(CancellationToken.None);

        await Assert.That(revised).IsEqualTo(29_000);
        await Assert.That(current).IsEqualTo(29_000);
    }

    [Test]
    public async Task ItemProperties_methods_round_trip_arrays_and_variants()
    {
        int calls = 0;
        var channel = Channel(IOPCItemProperties.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            string? itemId = reader.ReadUnicodeStringPtr();
            Ensure(itemId == "Random.Int4");
            calls++;
            if (opnum == IOPCItemProperties.Opnums.QueryAvailablePropertiesAsync)
            {
                return WritePayload((ref NdrWriter writer) =>
                {
                    WriteInt32Array(ref writer, 1);
                    WriteStringArray(ref writer, "Value");
                    WriteUInt16Array(ref writer, (ushort)VarType.VT_I4);
                });
            }

            // GetItemProperties / LookupItemIDs: [OpcEmitArrayCount] on propertyIds emits dwCount sibling
            _ = reader.ReadUInt32();
            int[] ids = ReadInt32Array(ref reader);
            Ensure(ids[0] == 1);
            if (opnum == IOPCItemProperties.Opnums.GetItemPropertiesAsync)
            {
                return WritePayload((ref NdrWriter writer) =>
                {
                    // [out, size_is(,N)] VARIANT** ppvData — [OpcUniquePointer, OpcVariantElements] layout
                    writer.WriteUniquePointerReferent(true);
                    WriteVariantElementsArray(ref writer, OpcVariant.FromInt32(88));
                    WriteInt32Array(ref writer, 0);
                });
            }

            Ensure(opnum == IOPCItemProperties.Opnums.LookupItemIdsAsync);
            return WritePayload((ref NdrWriter writer) =>
            {
                WriteStringArray(ref writer, "Random.Int4.Value");
                WriteInt32Array(ref writer, 0);
            });
        });

        var proxy = new IOPCItemPropertiesClientProxy(channel);
        await proxy.QueryAvailablePropertiesAsync("Random.Int4", out int[] propertyIds, out string[] descriptions, out ushort[] dataTypes, CancellationToken.None);
        await proxy.GetItemPropertiesAsync("Random.Int4", new[] { 1 }, out OpcVariant[] data, out int[] getErrors, CancellationToken.None);
        await proxy.LookupItemIdsAsync("Random.Int4", new[] { 1 }, out string[] newItemIds, out int[] lookupErrors, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(3);
        await Assert.That(propertyIds[0]).IsEqualTo(1);
        await Assert.That(descriptions[0]).IsEqualTo("Value");
        await Assert.That(dataTypes[0]).IsEqualTo((ushort)VarType.VT_I4);
        await Assert.That(data[0]).IsEqualTo(OpcVariant.FromInt32(88));
        await Assert.That(getErrors[0]).IsEqualTo(0);
        await Assert.That(newItemIds[0]).IsEqualTo("Random.Int4.Value");
        await Assert.That(lookupErrors[0]).IsEqualTo(0);
    }

    [Test]
    public async Task ItemDeadband_methods_round_trip_arrays()
    {
        int calls = 0;
        var channel = Channel(IOPCItemDeadbandMgt.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            _ = reader.ReadUInt32();  // [OpcEmitArrayCount] dwCount sibling for serverHandles
            int[] handles = ReadInt32Array(ref reader);
            Ensure(handles[0] == 300);
            calls++;
            if (opnum == IOPCItemDeadbandMgt.Opnums.SetItemDeadbandAsync)
            {
                Ensure(Math.Abs(ReadSingleArray(ref reader)[0] - 1.5f) < 0.001f);
                return EncodeErrors(0);
            }

            if (opnum == IOPCItemDeadbandMgt.Opnums.GetItemDeadbandAsync)
            {
                return WritePayload((ref NdrWriter writer) =>
                {
                    WriteSingleArray(ref writer, 2.5f);
                    WriteInt32Array(ref writer, 0);
                });
            }

            Ensure(opnum == IOPCItemDeadbandMgt.Opnums.ClearItemDeadbandAsync);
            return EncodeErrors(1);
        });

        var proxy = new IOPCItemDeadbandMgtClientProxy(channel);
        int[] setErrors = await proxy.SetItemDeadbandAsync(new[] { 300 }, new[] { 1.5f }, CancellationToken.None);
        await proxy.GetItemDeadbandAsync(new[] { 300 }, out float[] deadbands, out int[] getErrors, CancellationToken.None);
        int[] clearErrors = await proxy.ClearItemDeadbandAsync(new[] { 300 }, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(3);
        await Assert.That(setErrors[0]).IsEqualTo(0);
        await Assert.That(deadbands[0]).IsEqualTo(2.5f);
        await Assert.That(getErrors[0]).IsEqualTo(0);
        await Assert.That(clearErrors[0]).IsEqualTo(1);
    }

    [Test]
    public async Task ItemSampling_methods_round_trip_rates_buffers_and_errors()
    {
        int calls = 0;
        var channel = Channel(IOPCItemSamplingMgt.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            _ = reader.ReadUInt32();  // [OpcEmitArrayCount] dwCount sibling for serverHandles
            int[] handles = ReadInt32Array(ref reader);
            Ensure(handles[0] == 400);
            calls++;
            if (opnum == IOPCItemSamplingMgt.Opnums.SetItemSamplingRateAsync)
            {
                Ensure(ReadInt32Array(ref reader)[0] == 1000);
                return WritePayload((ref NdrWriter writer) =>
                {
                    WriteInt32Array(ref writer, 900);
                    WriteInt32Array(ref writer, 0);
                });
            }

            if (opnum == IOPCItemSamplingMgt.Opnums.GetItemSamplingRateAsync)
            {
                return WritePayload((ref NdrWriter writer) =>
                {
                    WriteInt32Array(ref writer, 750);
                    WriteInt32Array(ref writer, 0);
                });
            }

            if (opnum == IOPCItemSamplingMgt.Opnums.ClearItemSamplingRateAsync)
            {
                return EncodeErrors(0);
            }

            if (opnum == IOPCItemSamplingMgt.Opnums.SetItemBufferEnableAsync)
            {
                Ensure(ReadBooleanArray(ref reader)[0]);
                return EncodeErrors(0);
            }

            Ensure(opnum == IOPCItemSamplingMgt.Opnums.GetItemBufferEnableAsync);
            return WritePayload((ref NdrWriter writer) =>
            {
                WriteBooleanArray(ref writer, true);
                WriteInt32Array(ref writer, 0);
            });
        });

        var proxy = new IOPCItemSamplingMgtClientProxy(channel);
        await proxy.SetItemSamplingRateAsync(new[] { 400 }, new[] { 1000 }, out int[] revised, out int[] setErrors, CancellationToken.None);
        await proxy.GetItemSamplingRateAsync(new[] { 400 }, out int[] rates, out int[] getErrors, CancellationToken.None);
        int[] clearErrors = await proxy.ClearItemSamplingRateAsync(new[] { 400 }, CancellationToken.None);
        int[] bufferSetErrors = await proxy.SetItemBufferEnableAsync(new[] { 400 }, new[] { true }, CancellationToken.None);
        await proxy.GetItemBufferEnableAsync(new[] { 400 }, out bool[] enabled, out int[] bufferGetErrors, CancellationToken.None);

        await Assert.That(calls).IsEqualTo(5);
        await Assert.That(revised[0]).IsEqualTo(900);
        await Assert.That(setErrors[0]).IsEqualTo(0);
        await Assert.That(rates[0]).IsEqualTo(750);
        await Assert.That(getErrors[0]).IsEqualTo(0);
        await Assert.That(clearErrors[0]).IsEqualTo(0);
        await Assert.That(bufferSetErrors[0]).IsEqualTo(0);
        await Assert.That(enabled[0]).IsTrue();
        await Assert.That(bufferGetErrors[0]).IsEqualTo(0);
    }

    [Test]
    public async Task ItemIO_Read_and_WriteVqt_round_trip_values_qualities_timestamps_and_errors()
    {
        int readCalls = 0;
        int writeCalls = 0;
        var channel = Channel(IOPCItemIO.InterfaceId, (int opnum, ref NdrReader reader) =>
        {
            // IOPCItemIO::Read/WriteVQT have [OpcEmitArrayCount] on itemIds — the
            // proxy emits a standalone dwCount before the conformant array header
            // per the IDL [in] DWORD dwCount, [in, size_is(dwCount)] LPCWSTR*
            // shape. Skip the sibling count before reading the array body.
            _ = reader.ReadUInt32();
            string?[] itemIds = ReadStringArray(ref reader);
            Ensure(itemIds[0] == "Random.Int4");
            if (opnum == IOPCItemIO.Opnums.ReadAsync)
            {
                readCalls++;
                Ensure(ReadInt32Array(ref reader)[0] == 0);
                return WritePayload((ref NdrWriter writer) =>
                {
                    // [out, size_is(,N)] VARIANT** values — [OpcUniquePointer, OpcVariantElements]
                    writer.WriteUniquePointerReferent(true);  // outer referent for values
                    writer.WriteUInt32(1);                     // max_count
                    writer.WriteUniquePointerReferent(true);  // per-element referent
                    writer.AlignTo(8);
                    NdrVariantExtensions.WriteVariantElement(ref writer, OpcVariant.FromInt32(12));
                    // qualities, timestamps, errors — helpers emit their own outer referent
                    WriteUInt16Array(ref writer, 192);
                    // [out, size_is(,N)] FILETIME** timestamps — [OpcUniquePointer, OpcFileTimeElements]
                    writer.WriteUniquePointerReferent(true);
                    writer.WriteConformantFileTimeArray(new[] { 123456789L });
                    WriteInt32Array(ref writer, 0);
                });
            }

            Ensure(opnum == IOPCItemIO.Opnums.WriteVqtAsync);
            writeCalls++;
            OpcItemVqt[] values = ReadArray(ref reader, NdrOpcItemVqtCodec.Read);
            Ensure(values[0].Value.Equals(OpcVariant.FromInt32(13)));
            return EncodeErrors(0);
        });

        var proxy = new IOPCItemIOClientProxy(channel);
        await proxy.ReadAsync(new[] { "Random.Int4" }, new[] { 0 }, out OpcVariant[] values, out ushort[] qualities, out long[] timestamps, out int[] readErrors, CancellationToken.None);
        int[] writeErrors = await proxy.WriteVqtAsync(new[] { "Random.Int4" }, new[] { new OpcItemVqt(OpcVariant.FromInt32(13)) }, CancellationToken.None);

        await Assert.That(readCalls).IsEqualTo(1);
        await Assert.That(writeCalls).IsEqualTo(1);
        await Assert.That(values[0]).IsEqualTo(OpcVariant.FromInt32(12));
        await Assert.That(qualities[0]).IsEqualTo((ushort)192);
        await Assert.That(timestamps[0]).IsEqualTo(123456789L);
        await Assert.That(readErrors[0]).IsEqualTo(0);
        await Assert.That(writeErrors[0]).IsEqualTo(0);
    }

    private static InMemoryCallChannel Channel(Guid expectedIid, int expectedOpnum, NdrHandler handler) =>
        new((iid, opnum, payload, _) =>
        {
            Ensure(iid == expectedIid);
            Ensure(opnum == expectedOpnum);
            var reader = new NdrReader(payload.Span);
            return Task.FromResult(new NdrCallResult(0, handler(ref reader)));
        });

    private static InMemoryCallChannel Channel(Guid expectedIid, NdrOpnumHandler handler) =>
        new((iid, opnum, payload, _) =>
        {
            Ensure(iid == expectedIid);
            var reader = new NdrReader(payload.Span);
            return Task.FromResult(new NdrCallResult(0, handler(opnum, ref reader)));
        });

    private static ReadOnlyMemory<byte> EncodeCancelAndErrors(int cancelId, params int[] errors) =>
        WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteInt32(cancelId);
            WriteInt32Array(ref writer, errors);
        });

    private static ReadOnlyMemory<byte> EncodeErrors(params int[] errors) =>
        WritePayload((ref NdrWriter writer) => WriteInt32Array(ref writer, errors));

    private static ReadOnlyMemory<byte> EncodeInt32(int value) =>
        WritePayload((ref NdrWriter writer) => writer.WriteInt32(value));

    private static ReadOnlyMemory<byte> EncodeInterfaceRef(Guid iid, int seed) =>
        WritePayload((ref NdrWriter writer) => OpcInterfaceRefCodec.Write(ref writer, InterfaceRef(iid, seed)));

    private static OpcInterfaceRef InterfaceRef(Guid iid, int seed) =>
        new(iid, 0, 1, 1, unchecked((ulong)(uint)seed), Guid.CreateVersion7(), 0, Array.Empty<ushort>());

    private static ReadOnlyMemory<byte> EncodeItemResults(int serverHandle, int error) =>
        WritePayload((ref NdrWriter writer) =>
        {
            // [out, size_is(,N)] OPCITEMRESULT** ppAddResults — self-contained helper emits
            // outer unique-pointer referent + max_count + inline + deferred.
            NdrOpcItemResultCodec.WriteConformantArray(ref writer, [new OpcItemResult(serverHandle, VarType.VT_I4, 3, Array.Empty<byte>())]);
            WriteInt32Array(ref writer, error);
        });

    private static ReadOnlyMemory<byte> EncodeItemStates(OpcItemState state, int error) =>
        WritePayload((ref NdrWriter writer) =>
        {
            // [out, size_is(,N)] OPCITEMSTATE** ppItemValues — unique-pointer referent + max_count + inline.
            writer.WriteUniquePointerReferent(true);
            WriteArray(ref writer, new[] { state }, NdrOpcItemStateCodec.Write);
            WriteInt32Array(ref writer, error);
        });

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write, int capacity = 2048)
    {
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static T[] ReadArray<T>(ref NdrReader reader, NdrReadFunc<T> read)
    {
        int count = reader.ReadInt32();
        var values = new T[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = read(ref reader);
        }

        return values;
    }

    private static bool[] ReadBooleanArray(ref NdrReader reader)
    {
        int count = reader.ReadInt32();
        var values = new bool[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = reader.ReadInt32() != 0;
        }

        return values;
    }

    private static int[] ReadInt32Array(ref NdrReader reader) => reader.ReadConformantInt32Array();
    private static float[] ReadSingleArray(ref NdrReader reader) => reader.ReadConformantSingleArray();

    private static string?[] ReadStringArray(ref NdrReader reader)
    {
        int count = reader.ReadInt32();
        var values = new string?[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = reader.ReadUnicodeStringPtr();
        }

        return values;
    }

    private static void WriteArray<T>(ref NdrWriter writer, T[] values, NdrWriteFunc<T> write)
    {
        writer.WriteUInt32((uint)values.Length);
        foreach (T value in values)
        {
            write(ref writer, value);
        }
    }

    private static void WriteBooleanArray(ref NdrWriter writer, params bool[] values)
    {
        writer.WriteUniquePointerReferent(true);  // [out, size_is(,N)] BOOL** outer referent
        writer.WriteUInt32((uint)values.Length);
        foreach (bool value in values)
        {
            writer.WriteInt32(value ? -1 : 0);
        }
    }

    private static void WriteInt32Array(ref NdrWriter writer, params int[] values)
    {
        writer.WriteUniquePointerReferent(true);  // [out, size_is(,N)] HRESULT** / DWORD** outer referent
        writer.WriteUInt32((uint)values.Length);
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    private static void WriteInt64Array(ref NdrWriter writer, params long[] values)
    {
        writer.WriteUInt32((uint)values.Length);
        foreach (long value in values)
        {
            writer.WriteInt64(value);
        }
    }

    private static void WriteSingleArray(ref NdrWriter writer, params float[] values)
    {
        writer.WriteUniquePointerReferent(true);  // [out, size_is(,N)] float** outer referent
        writer.WriteUInt32((uint)values.Length);
        foreach (float value in values)
        {
            writer.WriteSingle(value);
        }
    }

    private static void WriteStringArray(ref NdrWriter writer, params string[] values)
    {
        writer.WriteUniquePointerReferent(true);  // [out, size_is(,N)] LPWSTR** outer referent
        writer.WriteUInt32((uint)values.Length);
        foreach (string value in values)
        {
            writer.WriteUnicodeStringPtr(value);
        }
    }

    private static void WriteUInt16Array(ref NdrWriter writer, params ushort[] values)
    {
        writer.WriteUniquePointerReferent(true);  // [out, size_is(,N)] WORD** / VARTYPE** outer referent
        writer.WriteUInt32((uint)values.Length);
        foreach (ushort value in values)
        {
            writer.WriteUInt16(value);
        }
    }

    /// <summary>
    /// Writes a VARIANT conformant array in the C706 §14.3.12.3 deferred-pointer pile layout
    /// expected by the proxy decoder under <c>[OpcVariantElements]</c>: max_count, N per-element
    /// referents, AlignTo(8), then N VARIANT bodies. Caller must have already emitted the outer
    /// unique-pointer referent when the parameter carries <c>[OpcUniquePointer]</c>.
    /// </summary>
    private static void WriteVariantElementsArray(ref NdrWriter writer, params OpcVariant[] values)
    {
        writer.WriteUInt32((uint)values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            writer.WriteUniquePointerReferent(true);
        }
        writer.AlignTo(8);
        foreach (OpcVariant value in values)
        {
            NdrVariantExtensions.WriteVariantElement(ref writer, value);
        }
    }

    private static void Ensure(bool condition)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Unexpected round-trip payload.");
        }
    }

    private delegate ReadOnlyMemory<byte> NdrHandler(ref NdrReader reader);
    private delegate ReadOnlyMemory<byte> NdrOpnumHandler(int opnum, ref NdrReader reader);
    private delegate T NdrReadFunc<T>(ref NdrReader reader);
    private delegate void NdrWriteFunc<T>(ref NdrWriter writer, T value);
}
