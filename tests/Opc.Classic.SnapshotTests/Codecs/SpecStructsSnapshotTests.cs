//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Threading.Tasks;
using Opc.Classic.Ndr;
using Opc.Classic.SnapshotTests.Support;
using TUnit.Core;
using AeNdr = Opc.Classic.Ae.Ndr;
using DaNdr = Opc.Classic.Da.Ndr;
using HdaNdr = Opc.Classic.Hda.Ndr;

namespace Opc.Classic.SnapshotTests.Codecs;

public sealed class SpecStructsSnapshotTests {
    [Test]
    public async Task OpcItemState_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCITEMSTATE", "canonical DA item state", static (ref NdrWriter writer) => DaNdr.NdrOpcItemStateCodec.Write(ref writer, CodecFixtures.ItemState()));

    [Test]
    public async Task OpcItemResult_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCITEMRESULT", "canonical DA add-item result", static (ref NdrWriter writer) => DaNdr.NdrOpcItemResultCodec.Write(ref writer, CodecFixtures.ItemResult()));

    [Test]
    public async Task OpcServerStatus_da_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCSERVERSTATUS", "canonical DA server status", static (ref NdrWriter writer) => DaNdr.NdrOpcServerStatusCodec.Write(ref writer, CodecFixtures.DaServerStatus()));

    [Test]
    public async Task OpcEventServerStatus_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCEVENTSERVERSTATUS", "canonical AE server status", static (ref NdrWriter writer) => AeNdr.NdrOpcEventServerStatusCodec.Write(ref writer, CodecFixtures.AeServerStatus()));

    [Test]
    public async Task OpcHdaServerStatus_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCHDA_SERVERSTATUS", "canonical HDA server status", static (ref NdrWriter writer) => HdaNdr.NdrOpcHdaServerStatusCodec.Write(ref writer, CodecFixtures.HdaServerStatus()));

    [Test]
    public async Task OpcHdaItem_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCHDA_ITEM", "canonical HDA item with mixed values", static (ref NdrWriter writer) => HdaNdr.NdrOpcHdaItemCodec.Write(ref writer, CodecFixtures.HdaItem()));

    [Test]
    public async Task OpcHdaAnnotation_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCHDA_ANNOTATION", "canonical HDA annotation", static (ref NdrWriter writer) => HdaNdr.NdrOpcHdaAnnotationCodec.Write(ref writer, CodecFixtures.HdaAnnotation()));

    [Test]
    public async Task OpcHdaAttribute_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCHDA_ATTRIBUTE", "canonical HDA attribute", static (ref NdrWriter writer) => HdaNdr.NdrOpcHdaAttributeCodec.Write(ref writer, CodecFixtures.HdaAttribute()));

    [Test]
    public async Task OpcHdaModifiedItem_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCHDA_MODIFIEDITEM", "canonical modified HDA item", static (ref NdrWriter writer) => HdaNdr.NdrOpcHdaModifiedItemCodec.Write(ref writer, CodecFixtures.HdaModifiedItem()));

    [Test]
    public async Task OpcHdaTime_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCHDA_TIME", "string time expression NOW-1H", static (ref NdrWriter writer) => HdaNdr.NdrOpcHdaTimeCodec.Write(ref writer, CodecFixtures.HdaTime()));

    [Test]
    public async Task OpcBrowseElement_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCBROWSEELEMENT", "canonical DA browse element", static (ref NdrWriter writer) => DaNdr.NdrOpcBrowseElementCodec.Write(ref writer, CodecFixtures.BrowseElement()));

    [Test]
    public async Task OpcItemProperties_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCITEMPROPERTIES", "two canonical item properties", static (ref NdrWriter writer) => DaNdr.NdrOpcItemPropertiesCodec.Write(ref writer, CodecFixtures.ItemProperties()));

    [Test]
    public async Task OpcItemProperty_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCITEMPROPERTY", "canonical item value property", static (ref NdrWriter writer) => DaNdr.NdrOpcItemPropertyCodec.Write(ref writer, CodecFixtures.ItemValueProperty()));

    [Test]
    public async Task OpcItemDef_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCITEMDEF", "canonical active item definition", static (ref NdrWriter writer) => DaNdr.NdrOpcItemDefCodec.Write(ref writer, CodecFixtures.ItemDef()));

    [Test]
    public async Task OpcItemAttributes_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCITEMATTRIBUTES", "canonical item attributes with EU range", static (ref NdrWriter writer) => DaNdr.NdrOpcItemAttributesCodec.Write(ref writer, CodecFixtures.ItemAttributes()));

    [Test]
    public async Task OpcItemVqt_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCITEMVQT", "value-quality-timestamp with BSTR value", static (ref NdrWriter writer) => DaNdr.NdrOpcItemVqtCodec.Write(ref writer, CodecFixtures.ItemVqt()));

    [Test]
    public async Task OpcGroupState_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCGROUPSTATE", "IOPCGroupStateMgt GetState response shape", static (ref NdrWriter writer) => DaNdr.NdrOpcGroupStateCodec.Write(ref writer, CodecFixtures.GroupState()));

    [Test]
    public async Task Oneventstruct_encodes_to_stable_bytes() =>
        await VerifyStruct("ONEVENTSTRUCT", "canonical AE event notification", static (ref NdrWriter writer) => AeNdr.NdrOpcEventNotificationCodec.Write(ref writer, CodecFixtures.EventNotification()), capacity: 4096);

    [Test]
    public async Task OpcConditionState_encodes_to_stable_bytes() =>
        await VerifyStruct("OPCCONDITIONSTATE", "canonical AE condition state", static (ref NdrWriter writer) => AeNdr.NdrOpcConditionStateCodec.Write(ref writer, CodecFixtures.ConditionState()), capacity: 8192);

    private static Task VerifyStruct(string codecName, string sampleDescription, NdrWriteAction write, int capacity = 4096) =>
        SnapshotVerifier.VerifyBytes(codecName, sampleDescription, NdrSnapshotWriter.Write(write, capacity));
}
