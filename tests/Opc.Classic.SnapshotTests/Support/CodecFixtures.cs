//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ae;
using Opc.Classic.Da;
using Opc.Classic.Hda;

namespace Opc.Classic.SnapshotTests.Support;

internal static class CodecFixtures {
    public static DateTimeOffset BaseTime { get; } = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public static OpcRecordInfo SampleRecordInfo { get; } = new(
        new Guid("6d88a608-407a-4f1f-a8f0-ae2b10bba875"),
        "SampleRecord",
        new[]
        {
            new OpcRecordField("Id", VarType.VT_I4),
            new OpcRecordField("Name", VarType.VT_BSTR),
            new OpcRecordField("Value", VarType.VT_R8),
        });

    public static OpcRecordValue SampleRecordValue() =>
        new(SampleRecordInfo, new object?[] { 42, "Pump", 9.75d });

    public static OpcItemPropertyResult ItemValueProperty() =>
        new(
            DataType: VarType.VT_R8,
            PropertyId: 100,
            ItemId: null,
            Description: "Item Value",
            Value: OpcVariant.FromDouble(42.5),
            ErrorId: 0);

    public static OpcItemPropertyResult QualityProperty() =>
        new(
            DataType: VarType.VT_I4,
            PropertyId: 101,
            ItemId: "Factory.Line1.Temperature.Quality",
            Description: "Quality Code",
            Value: OpcVariant.FromInt32(192),
            ErrorId: 0);

    public static OpcItemProperties ItemProperties() =>
        new(ErrorId: 0, Properties: [ItemValueProperty(), QualityProperty()]);

    public static OpcBrowseElementResult BrowseElement() =>
        new(
            Name: "Temperature",
            ItemId: "Factory.Line1.Temperature",
            FlagValue: 2,
            Properties: ItemProperties());

    public static OpcItemState ItemState() =>
        new(
            ClientHandle: 0x12345678,
            Timestamp: BaseTime.AddHours(1),
            Quality: OpcQuality.Good,
            Value: OpcVariant.FromDouble(3.14));

    public static OpcItemResult ItemResult() =>
        new(
            ServerHandle: 0x01020304,
            CanonicalDataType: VarType.VT_R8,
            AccessRights: 3,
            Blob: new byte[] { 0xDE, 0xAD, 0xBE, 0xEF });

    public static OpcItemDef ItemDef() =>
        new(
            AccessPath: "Root",
            ItemId: "Factory.Line1.Temperature",
            Active: true,
            ClientHandle: 42,
            Blob: new byte[] { 0x01, 0x02, 0x03 },
            RequestedDataType: VarType.VT_R8);

    public static OpcItemAttributes ItemAttributes() =>
        new(
            AccessPath: "Root",
            ItemId: "Factory.Line1.Temperature",
            Active: true,
            ClientHandle: 42,
            ServerHandle: 0x01020304,
            AccessRights: 3,
            Blob: new byte[] { 0xAA, 0x55 },
            RequestedDataType: VarType.VT_EMPTY,
            CanonicalDataType: VarType.VT_R8,
            EUType: 1,
            EUInfo: OpcVariant.FromSafeArray(OpcSafeArray.OfDouble([0.0d, 100.0d])));

    public static OpcItemVqt ItemVqt() =>
        new(
            OpcVariant.FromString("snapshot"),
            Quality: OpcQuality.Good,
            Timestamp: BaseTime.AddHours(2));

    public static OpcGroupState GroupState() =>
        new(
            ClientHandle: 7,
            ServerHandle: 11,
            Name: "SnapshotGroup",
            Active: true,
            UpdateRate: 1000,
            TimeBias: -60,
            PercentDeadband: 0.25f,
            LocaleId: 0x0409);

    public static OpcServerStatus DaServerStatus() => new() {
        Spec = OpcStatusSpec.Da,
        StartTime = BaseTime,
        CurrentTime = BaseTime.AddHours(3),
        LastUpdateTime = BaseTime.AddHours(2).AddMinutes(59),
        State = OpcServerState.Running,
        ServerVersion = new Version(2, 5, 1),
        GroupCount = 17,
        BandWidth = 4500,
        VendorInfo = "Acme DA Server",
    };

    public static OpcHdaItem HdaItem() =>
        new(
            clientHandle: 42,
            aggregateHandle: 1,
            timestamps:
            [
                BaseTime,
                BaseTime.AddMinutes(1),
                BaseTime.AddMinutes(2),
            ],
            qualities: [192u, 216u, 0u],
            values:
            [
                OpcVariant.FromDouble(100.0),
                OpcVariant.FromInt32(101),
                OpcVariant.FromString("102"),
            ]);

    public static OpcHdaAnnotation HdaAnnotation() =>
        new(
            clientHandle: 42,
            timestamps: [BaseTime, BaseTime.AddMinutes(1)],
            annotations: ["Started", "Stable"],
            annotationTimes: [BaseTime.AddSeconds(5), BaseTime.AddMinutes(1).AddSeconds(5)],
            users: ["operator-a", "operator-b"]);

    public static OpcHdaAttribute HdaAttribute() =>
        new(
            clientHandle: 42,
            attributeId: 1,
            timestamps: [BaseTime, BaseTime.AddMinutes(1)],
            values: [OpcVariant.FromInt32(100), OpcVariant.FromString("running")]);

    public static OpcHdaModifiedItem HdaModifiedItem() =>
        new(
            clientHandle: 42,
            timestamps: [BaseTime, BaseTime.AddMinutes(1)],
            qualities: [192u, 216u],
            values: [OpcVariant.FromDouble(12.5), OpcVariant.FromDouble(13.75)],
            modificationTimes: [BaseTime.AddDays(1), BaseTime.AddDays(1).AddMinutes(5)],
            editTypes: [1u, 2u],
            users: ["operator-a", "operator-b"]);

    public static OpcHdaTime HdaTime() =>
        OpcHdaTime.FromString("NOW-1H");

    public static OpcServerStatus HdaServerStatus() => new() {
        Spec = OpcStatusSpec.Hda,
        StartTime = BaseTime,
        CurrentTime = BaseTime.AddHours(4),
        State = OpcServerState.Running,
        ServerVersion = new Version(3, 1, 4),
        MaxReturnValues = 1000,
        VendorInfo = "Acme HDA Server",
    };

    public static OpcServerStatus AeServerStatus() => new() {
        Spec = OpcStatusSpec.Ae,
        StartTime = BaseTime,
        CurrentTime = BaseTime.AddHours(5),
        LastUpdateTime = BaseTime.AddHours(4).AddMinutes(58),
        State = OpcServerState.Running,
        ServerVersion = new Version(1, 10, 42),
        VendorInfo = "Acme AE Server",
    };

    public static OpcEventNotification EventNotification() =>
        new(
            changeMask: 0x0003,
            newState: 0x0005,
            source: "Boiler.Area1.Pump7",
            time: BaseTime.AddHours(6),
            message: "Pump discharge pressure high",
            eventType: 0x0004,
            eventCategory: 17,
            severity: 900,
            conditionName: "PressureHigh",
            subconditionName: "HighHigh",
            quality: OpcQuality.Good,
            ackRequired: true,
            activeTime: BaseTime.AddHours(5).AddMinutes(55),
            cookie: 0x12345678,
            eventAttributes:
            [
                OpcVariant.FromInt32(42),
                OpcVariant.FromString("high-high"),
                OpcVariant.FromBoolean(true),
            ],
            actorId: "operator1");

    public static OpcConditionState ConditionState() =>
        new(
            state: 0x0007,
            activeSubCondition: "HighHigh",
            activeSubConditionDefinition: "Pressure above trip threshold",
            activeSubConditionSeverity: 900,
            activeSubConditionDescription: "Discharge pressure critically high",
            quality: OpcQuality.Good,
            lastAckTime: BaseTime.AddHours(6).AddMinutes(1),
            subConditionLastActive: BaseTime.AddHours(5).AddMinutes(55),
            conditionLastActive: BaseTime.AddHours(5).AddMinutes(50),
            conditionLastInactive: BaseTime.AddHours(5),
            acknowledgerId: "operator1",
            comment: "Acknowledged during shift handoff",
            subConditionNames: ["High", "HighHigh"],
            subConditionDefinitions: ["Above limit", "Above trip"],
            subConditionSeverities: [700u, 900u],
            subConditionDescriptions: ["Pressure high", "Pressure critically high"],
            eventAttributes:
            [
                OpcVariant.FromInt32(42),
                OpcVariant.FromString("pump-7"),
                OpcVariant.FromBoolean(true),
            ],
            errors: [0, unchecked((int)0x80004005u), 7]);
}
