// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Protocol round-trip tests assert several fields per RPC case.

using Opc.Classic.Dcom;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Hda.Tests.Dcom;

public sealed class HdaMissingMethodProxyRoundTripTests
{
    private delegate void NdrWriteAction(ref NdrWriter writer);

    [Test]
    [Arguments("Server.GetItemAttributes")]
    [Arguments("Server.GetAggregates")]
    [Arguments("Browser.GetEnum")]
    [Arguments("Browser.ChangeBrowsePosition")]
    [Arguments("Browser.GetItemID")]
    [Arguments("Browser.GetBranchPosition")]
    [Arguments("SyncRead.ReadAttribute")]
    [Arguments("SyncUpdate.Insert")]
    [Arguments("SyncUpdate.Replace")]
    [Arguments("SyncUpdate.InsertReplace")]
    [Arguments("SyncUpdate.DeleteRaw")]
    [Arguments("SyncUpdate.DeleteAtTime")]
    [Arguments("SyncAnnotations.Read")]
    [Arguments("SyncAnnotations.Insert")]
    [Arguments("AsyncRead.ReadAtTime")]
    [Arguments("AsyncRead.ReadModified")]
    [Arguments("AsyncRead.ReadAttribute")]
    [Arguments("AsyncUpdate.Insert")]
    [Arguments("AsyncUpdate.Replace")]
    [Arguments("AsyncUpdate.InsertReplace")]
    [Arguments("AsyncUpdate.DeleteRaw")]
    [Arguments("AsyncUpdate.DeleteAtTime")]
    [Arguments("AsyncAnnotations.Read")]
    [Arguments("AsyncAnnotations.Insert")]
    [Arguments("DataCallback.OnDataChange")]
    [Arguments("DataCallback.OnReadComplete")]
    [Arguments("DataCallback.OnReadModifiedComplete")]
    [Arguments("DataCallback.OnReadAttributeComplete")]
    [Arguments("DataCallback.OnReadAnnotations")]
    [Arguments("DataCallback.OnInsertAnnotations")]
    [Arguments("DataCallback.OnPlayback")]
    [Arguments("DataCallback.OnUpdateComplete")]
    [Arguments("DataCallback.OnCancelComplete")]
    public async Task MissingHdaMethod_RoundTripsThroughInMemoryCallChannel(string method)
    {
        switch (method)
        {
            case "Server.GetItemAttributes":
                await AssertRoundTripAsync(
                    IOPCHDA_Server.InterfaceId,
                    3,
                    (ref NdrWriter w) =>
                    {
                        WriteIntArray(ref w, [1, 2]);
                        WriteStringArray(ref w, ["Data Type", "Description"]);
                        WriteStringArray(ref w, ["VT", "Text"]);
                        WriteIntArray(ref w, [5, 8]);
                    },
                    async channel =>
                    {
                        var proxy = new IOPCHDA_ServerClientProxy(channel);
                        await proxy.GetItemAttributesAsync(out int[] ids, out string[] names, out string[] descriptions, out int[] dataTypes, CancellationToken.None);
                        await Assert.That(ids[1]).IsEqualTo(2);
                        await Assert.That(names[0]).IsEqualTo("Data Type");
                        await Assert.That(descriptions[1]).IsEqualTo("Text");
                        await Assert.That(dataTypes[0]).IsEqualTo(5);
                    });
                return;

            case "Server.GetAggregates":
                await AssertRoundTripAsync(
                    IOPCHDA_Server.InterfaceId,
                    4,
                    (ref NdrWriter w) =>
                    {
                        WriteIntArray(ref w, [1, 4]);
                        WriteStringArray(ref w, ["Interpolative", "Average"]);
                        WriteStringArray(ref w, ["Interpolated value", "Time average"]);
                    },
                    async channel =>
                    {
                        var proxy = new IOPCHDA_ServerClientProxy(channel);
                        await proxy.GetAggregatesAsync(out int[] ids, out string[] names, out string[] descriptions, CancellationToken.None);
                        await Assert.That(ids[0]).IsEqualTo(1);
                        await Assert.That(names[1]).IsEqualTo("Average");
                        await Assert.That(descriptions[0]).IsEqualTo("Interpolated value");
                    });
                return;

            case "Browser.GetEnum":
                await AssertRoundTripAsync(
                    IOPCHDA_Browser.InterfaceId,
                    3,
                    WriteEnumStringRef,
                    async channel =>
                    {
                        var proxy = new IOPCHDA_BrowserClientProxy(channel);
                        IOpcInterfaceRef enumRef = await proxy.GetEnumAsync(3, CancellationToken.None);
                        await Assert.That(enumRef.Iid).IsEqualTo(OpcGuids.IID_IEnumString);
                    });
                return;

            case "Browser.ChangeBrowsePosition":
                await AssertRoundTripAsync(
                    IOPCHDA_Browser.InterfaceId,
                    4,
                    null,
                    async channel => await new IOPCHDA_BrowserClientProxy(channel).ChangeBrowsePositionAsync(2, "Area", CancellationToken.None));
                return;

            case "Browser.GetItemID":
                await AssertRoundTripAsync(
                    IOPCHDA_Browser.InterfaceId,
                    5,
                    (ref NdrWriter w) => w.WriteUnicodeStringPtr("Plant.Area.Tag"),
                    async channel =>
                    {
                        string itemId = await new IOPCHDA_BrowserClientProxy(channel).GetItemIDAsync("Tag", CancellationToken.None);
                        await Assert.That(itemId).IsEqualTo("Plant.Area.Tag");
                    });
                return;

            case "Browser.GetBranchPosition":
                await AssertRoundTripAsync(
                    IOPCHDA_Browser.InterfaceId,
                    6,
                    (ref NdrWriter w) => w.WriteUnicodeStringPtr("Plant.Area"),
                    async channel =>
                    {
                        string branch = await new IOPCHDA_BrowserClientProxy(channel).GetBranchPositionAsync(CancellationToken.None);
                        await Assert.That(branch).IsEqualTo("Plant.Area");
                    });
                return;

            case "SyncRead.ReadAttribute":
                await AssertRoundTripAsync(
                    IOPCHDA_SyncRead.InterfaceId,
                    7,
                    (ref NdrWriter w) =>
                    {
                        // [return: OpcUniquePointer] emits outer referent before conformance.
                        w.WriteUInt32(0x00020000u);
                        WriteAttributeArray(ref w, [SampleAttribute()]);
                    },
                    async channel =>
                    {
                        OpcHdaAttribute[] attributes = await new IOPCHDA_SyncReadClientProxy(channel).ReadAttributeAsync(SampleStart(), SampleEnd(), 100, [1], CancellationToken.None);
                        await Assert.That(attributes[0].AttributeId).IsEqualTo(1);
                        await Assert.That(attributes[0].Values[0].AsString()).IsEqualTo("kg/s");
                    });
                return;

            case "SyncUpdate.Insert":
                await AssertIntArrayReturnAsync(IOPCHDA_SyncUpdate.InterfaceId, 4, method,
                    channel => new IOPCHDA_SyncUpdateClientProxy(channel).InsertAsync([10, 11], [1L, 2L], [OpcVariant.FromDouble(1.0), OpcVariant.FromDouble(2.0)], [192, 192], CancellationToken.None));
                return;

            case "SyncUpdate.Replace":
                await AssertIntArrayReturnAsync(IOPCHDA_SyncUpdate.InterfaceId, 5, method,
                    channel => new IOPCHDA_SyncUpdateClientProxy(channel).ReplaceAsync([10, 11], [1L, 2L], [OpcVariant.FromDouble(1.0), OpcVariant.FromDouble(2.0)], [192, 192], CancellationToken.None));
                return;

            case "SyncUpdate.InsertReplace":
                await AssertIntArrayReturnAsync(IOPCHDA_SyncUpdate.InterfaceId, 6, method,
                    channel => new IOPCHDA_SyncUpdateClientProxy(channel).InsertReplaceAsync([10, 11], [1L, 2L], [OpcVariant.FromDouble(1.0), OpcVariant.FromDouble(2.0)], [192, 192], CancellationToken.None));
                return;

            case "SyncUpdate.DeleteRaw":
                await AssertIntArrayReturnAsync(IOPCHDA_SyncUpdate.InterfaceId, 7, method,
                    channel => new IOPCHDA_SyncUpdateClientProxy(channel).DeleteRawAsync(SampleStart(), SampleEnd(), [10, 11], CancellationToken.None));
                return;

            case "SyncUpdate.DeleteAtTime":
                await AssertIntArrayReturnAsync(IOPCHDA_SyncUpdate.InterfaceId, 8, method,
                    channel => new IOPCHDA_SyncUpdateClientProxy(channel).DeleteAtTimeAsync([10, 11], [1L, 2L], CancellationToken.None));
                return;

            case "SyncAnnotations.Read":
                await AssertRoundTripAsync(
                    IOPCHDA_SyncAnnotations.InterfaceId,
                    4,
                    (ref NdrWriter w) =>
                    {
                        // [return: OpcUniquePointer] emits outer referent before conformance.
                        w.WriteUInt32(0x00020000u);
                        WriteAnnotationArray(ref w, [SampleAnnotation()]);
                    },
                    async channel =>
                    {
                        OpcHdaAnnotation[] annotations = await new IOPCHDA_SyncAnnotationsClientProxy(channel).ReadAsync(SampleStart(), SampleEnd(), [10], CancellationToken.None);
                        await Assert.That(annotations[0].ClientHandle).IsEqualTo(77);
                        await Assert.That(annotations[0].Annotations[0]).IsEqualTo("checked by operator");
                        await Assert.That(annotations[0].AnnotationTimes[0]).IsEqualTo(SampleAnnotation().AnnotationTimes[0]);
                        await Assert.That(annotations[0].Users[0]).IsEqualTo("alice");
                    });
                return;

            case "SyncAnnotations.Insert":
                await AssertIntArrayReturnAsync(IOPCHDA_SyncAnnotations.InterfaceId, 5, method,
                    channel => new IOPCHDA_SyncAnnotationsClientProxy(channel).InsertAsync([10], [1L], [SampleAnnotation()], CancellationToken.None));
                return;

            case "AsyncRead.ReadAtTime":
                await AssertIntReturnAsync(IOPCHDA_AsyncRead.InterfaceId, 7,
                    channel => new IOPCHDA_AsyncReadClientProxy(channel).ReadAtTimeAsync(1, [1L, 2L], [10], CancellationToken.None));
                return;

            case "AsyncRead.ReadModified":
                await AssertIntReturnAsync(IOPCHDA_AsyncRead.InterfaceId, 8,
                    channel => new IOPCHDA_AsyncReadClientProxy(channel).ReadModifiedAsync(1, SampleStart(), SampleEnd(), 10, [10], CancellationToken.None));
                return;

            case "AsyncRead.ReadAttribute":
                await AssertIntReturnAsync(IOPCHDA_AsyncRead.InterfaceId, 9,
                    channel => new IOPCHDA_AsyncReadClientProxy(channel).ReadAttributeAsync(1, SampleStart(), SampleEnd(), 10, [1], CancellationToken.None));
                return;

            case "AsyncUpdate.Insert":
                await AssertIntReturnAsync(IOPCHDA_AsyncUpdate.InterfaceId, 4,
                    channel => new IOPCHDA_AsyncUpdateClientProxy(channel).InsertAsync(1, [10], [1L], [OpcVariant.FromDouble(1.0)], [192], CancellationToken.None));
                return;

            case "AsyncUpdate.Replace":
                await AssertIntReturnAsync(IOPCHDA_AsyncUpdate.InterfaceId, 5,
                    channel => new IOPCHDA_AsyncUpdateClientProxy(channel).ReplaceAsync(1, [10], [1L], [OpcVariant.FromDouble(1.0)], [192], CancellationToken.None));
                return;

            case "AsyncUpdate.InsertReplace":
                await AssertIntReturnAsync(IOPCHDA_AsyncUpdate.InterfaceId, 6,
                    channel => new IOPCHDA_AsyncUpdateClientProxy(channel).InsertReplaceAsync(1, [10], [1L], [OpcVariant.FromDouble(1.0)], [192], CancellationToken.None));
                return;

            case "AsyncUpdate.DeleteRaw":
                await AssertIntReturnAsync(IOPCHDA_AsyncUpdate.InterfaceId, 7,
                    channel => new IOPCHDA_AsyncUpdateClientProxy(channel).DeleteRawAsync(1, SampleStart(), SampleEnd(), [10], CancellationToken.None));
                return;

            case "AsyncUpdate.DeleteAtTime":
                await AssertIntReturnAsync(IOPCHDA_AsyncUpdate.InterfaceId, 8,
                    channel => new IOPCHDA_AsyncUpdateClientProxy(channel).DeleteAtTimeAsync(1, [10], [1L], CancellationToken.None));
                return;

            case "AsyncAnnotations.Read":
                await AssertIntReturnAsync(IOPCHDA_AsyncAnnotations.InterfaceId, 4,
                    channel => new IOPCHDA_AsyncAnnotationsClientProxy(channel).ReadAsync(1, SampleStart(), SampleEnd(), [10], CancellationToken.None));
                return;

            case "AsyncAnnotations.Insert":
                await AssertIntReturnAsync(IOPCHDA_AsyncAnnotations.InterfaceId, 5,
                    channel => new IOPCHDA_AsyncAnnotationsClientProxy(channel).InsertAsync(1, [10], [1L], [SampleAnnotation()], CancellationToken.None));
                return;

            case "DataCallback.OnDataChange":
                await AssertCallbackAsync(3,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnDataChangeAsync(1, 0, [SampleItem()], [0], CancellationToken.None));
                return;

            case "DataCallback.OnReadComplete":
                await AssertCallbackAsync(4,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnReadCompleteAsync(1, 0, [SampleItem()], [0], CancellationToken.None));
                return;

            case "DataCallback.OnReadModifiedComplete":
                await AssertCallbackAsync(5,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnReadModifiedCompleteAsync(1, 0, [SampleModifiedItem()], [0], CancellationToken.None));
                return;

            case "DataCallback.OnReadAttributeComplete":
                await AssertCallbackAsync(6,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnReadAttributeCompleteAsync(1, 0, 99, [SampleAttribute()], [0], CancellationToken.None));
                return;

            case "DataCallback.OnReadAnnotations":
                await AssertCallbackAsync(7,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnReadAnnotationsAsync(1, 0, [SampleAnnotation()], [0], CancellationToken.None));
                return;

            case "DataCallback.OnInsertAnnotations":
                await AssertCallbackAsync(8,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnInsertAnnotationsAsync(1, 0, [77], [0], CancellationToken.None));
                return;

            case "DataCallback.OnPlayback":
                await AssertCallbackAsync(9,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnPlaybackAsync(1, 0, [SampleItem()], [0], CancellationToken.None));
                return;

            case "DataCallback.OnUpdateComplete":
                await AssertCallbackAsync(10,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnUpdateCompleteAsync(1, 0, [77], [0], CancellationToken.None));
                return;

            case "DataCallback.OnCancelComplete":
                await AssertCallbackAsync(11,
                    channel => new IOPCHDA_DataCallbackClientProxy(channel).OnCancelCompleteAsync(123, CancellationToken.None));
                return;

            default:
                throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown HDA method test case.");
        }
    }

    private static async Task AssertIntReturnAsync(Guid expectedIid, int expectedOpnum, Func<InMemoryCallChannel, Task<int>> invoke)
    {
        await AssertRoundTripAsync(
            expectedIid,
            expectedOpnum,
            (ref NdrWriter w) => w.WriteInt32(1234),
            async channel =>
            {
                int value = await invoke(channel);
                await Assert.That(value).IsEqualTo(1234);
            });
    }

    private static async Task AssertIntArrayReturnAsync(Guid expectedIid, int expectedOpnum, string method, Func<InMemoryCallChannel, Task<int[]>> invoke)
    {
        _ = method;
        await AssertRoundTripAsync(
            expectedIid,
            expectedOpnum,
            (ref NdrWriter w) =>
            {
                // [return: OpcUniquePointer] emits outer referent before conformance.
                w.WriteUInt32(0x00020000u);
                WriteIntArray(ref w, [0, OpcResultId.UnknownItemId.Code]);
            },
            async channel =>
            {
                int[] values = await invoke(channel);
                await Assert.That(values.Length).IsEqualTo(2);
                await Assert.That(values[1]).IsEqualTo(OpcResultId.UnknownItemId.Code);
            });
    }

    private static Task AssertCallbackAsync(int expectedOpnum, Func<InMemoryCallChannel, Task> invoke) =>
        AssertRoundTripAsync(IOPCHDA_DataCallback.InterfaceId, expectedOpnum, null, invoke);

    private static async Task AssertRoundTripAsync(Guid expectedIid, int expectedOpnum, NdrWriteAction? writeResponse, Func<InMemoryCallChannel, Task> invoke)
    {
        Guid observedIid = Guid.Empty;
        int observedOpnum = -1;
        int observedPayloadLength = -1;
        ReadOnlyMemory<byte> responsePayload = WritePayload(writeResponse);
        var channel = new InMemoryCallChannel((iid, opnum, payload, _) =>
        {
            observedIid = iid;
            observedOpnum = opnum;
            observedPayloadLength = payload.Length;
            return Task.FromResult(new NdrCallResult(0, responsePayload));
        });

        await invoke(channel);

        await Assert.That(observedIid).IsEqualTo(expectedIid);
        await Assert.That(observedOpnum).IsEqualTo(expectedOpnum);
        await Assert.That(observedPayloadLength).IsGreaterThanOrEqualTo(0);
    }

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction? write, int capacity = 8192)
    {
        if (write is null)
        {
            return ReadOnlyMemory<byte>.Empty;
        }

        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static void WriteIntArray(ref NdrWriter writer, int[] values)
    {
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    private static void WriteStringArray(ref NdrWriter writer, string[] values)
    {
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (string value in values)
        {
            writer.WriteUnicodeStringPtr(value);
        }
    }

    private static void WriteAttributeArray(ref NdrWriter writer, OpcHdaAttribute[] values)
    {
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (OpcHdaAttribute value in values)
        {
            NdrOpcHdaAttributeCodec.Write(ref writer, value);
        }
    }

    private static void WriteAnnotationArray(ref NdrWriter writer, OpcHdaAnnotation[] values)
    {
        writer.WriteUInt32(unchecked((uint)values.Length));
        foreach (OpcHdaAnnotation value in values)
        {
            NdrOpcHdaAnnotationCodec.Write(ref writer, value);
        }
    }

    private static void WriteEnumStringRef(ref NdrWriter writer)
    {
        writer.WriteUInt32(0x574F454Du);
        writer.WriteUInt32(1u);
        writer.WriteGuid(OpcGuids.IID_IEnumString);
        writer.WriteUInt32(0u);
        writer.WriteUInt32(5u);
        writer.WriteUInt64(0x1122334455667788UL);
        writer.WriteUInt64(0x8877665544332211UL);
        writer.WriteGuid(new Guid("12345678-1234-5678-9ABC-DEF012345678"));
        writer.WriteUInt16(1);
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
    }

    private static OpcHdaTime SampleStart() => OpcHdaTime.FromString("NOW-1H");
    private static OpcHdaTime SampleEnd() => OpcHdaTime.FromString("NOW");

    private static OpcHdaItem SampleItem() => new(
        clientHandle: 77,
        aggregateHandle: 0,
        timestamps: [new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero)],
        qualities: [192u],
        values: [OpcVariant.FromDouble(42.5)]);

    private static OpcHdaModifiedItem SampleModifiedItem() => new(
        clientHandle: 77,
        timestamps: [new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero)],
        qualities: [192u],
        values: [OpcVariant.FromDouble(42.5)],
        modificationTimes: [new DateTimeOffset(2026, 5, 22, 10, 1, 0, TimeSpan.Zero)],
        editTypes: [1u],
        users: ["alice"]);

    private static OpcHdaAttribute SampleAttribute() => new(
        clientHandle: 77,
        attributeId: 1,
        timestamps: [new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero)],
        values: [OpcVariant.FromString("kg/s")]);

    private static OpcHdaAnnotation SampleAnnotation() => new(
        clientHandle: 77,
        timestamps: [new DateTimeOffset(2026, 5, 22, 10, 0, 0, TimeSpan.Zero)],
        annotations: ["checked by operator"],
        annotationTimes: [new DateTimeOffset(2026, 5, 22, 10, 1, 0, TimeSpan.Zero)],
        users: ["alice"]);
}
