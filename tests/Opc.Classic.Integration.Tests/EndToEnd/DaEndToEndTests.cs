//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable TUnitAssertions0005 // End-to-end tests assert captured pipeline state.

using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.EndToEnd;

public sealed class DaEndToEndTests
{
    [Test, Category("EndToEnd")]
    public async Task ConnectAndGetStatus_Then_ServerStatusFlowsBack()
    {
        var pipeline = new DaEndToEndPipeline();

        OpcServerStatus status = await pipeline.Server.GetStatusAsync(CancellationToken.None);

        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Da);
        await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
        await Assert.That(status.ServerVersion).IsEqualTo(new Version(1, 0, 0));
        await Assert.That(status.GroupCount).IsEqualTo(0);
        await Assert.That(status.BandWidth).IsEqualTo(0u);
        await Assert.That(status.VendorInfo).Contains("Opc.Classic Sample DA Server");
        await Assert.That(status.VendorInfo).Contains("30 tags");
        await Assert.That(status.StartTime).IsNotEqualTo(default(DateTimeOffset));
        await Assert.That(status.CurrentTime).IsGreaterThanOrEqualTo(status.StartTime);
        await Assert.That(status.LastUpdateTime).IsGreaterThanOrEqualTo(status.StartTime);
        await Assert.That(pipeline.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog[0].Opnum).IsEqualTo(IOPCServer.Opnums.GetStatusAsync);
    }

    [Test, Category("EndToEnd")]
    public async Task AddGroupWithDefaultState_Then_GroupExistsWithMatchingState()
    {
        var pipeline = new DaEndToEndPipeline();
        const string groupName = "E2E.DA.Group-A";
        const int updateRate = 1_000;
        const int clientHandle = 0x4401;
        const int localeId = 0x0409;

        int serverHandle = await pipeline.AddGroupViaWireAsync(groupName, active: true, updateRate, clientHandle, localeId, CancellationToken.None);
        Opc.Classic.Da.OpcGroupState state = await pipeline.GroupState.GetStateAsync(CancellationToken.None);

        await Assert.That(serverHandle).IsEqualTo(clientHandle + 0x1000);
        await Assert.That(pipeline.GroupExists(serverHandle)).IsTrue();
        await Assert.That(state.ClientHandle).IsEqualTo(clientHandle);
        await Assert.That(state.ServerHandle).IsEqualTo(serverHandle);
        await Assert.That(state.Name).IsEqualTo(groupName);
        await Assert.That(state.Active).IsTrue();
        await Assert.That(state.UpdateRate).IsEqualTo(updateRate);
        await Assert.That(state.TimeBias).IsEqualTo(0);
        await Assert.That(state.PercentDeadband).IsEqualTo(0.0f);
        await Assert.That(state.LocaleId).IsEqualTo(localeId);
        await Assert.That(pipeline.Channel.CallLog.Select(static call => call.InterfaceId))
            .IsEquivalentTo([IOPCServer.InterfaceId, IOPCGroupStateMgt.InterfaceId]);
    }

    [Test, Category("EndToEnd")]
    public async Task AddItemsAndReadSync_Then_ValuesRoundTripThroughCodecs()
    {
        var pipeline = new DaEndToEndPipeline();
        int groupHandle = await AddDefaultGroupAsync(pipeline);

        DaAddItemResult[] items = await pipeline.AddItemsViaWireAsync(groupHandle, WritableItemRequests(), CancellationToken.None);
        DaReadResult[] readResults = await pipeline.ReadViaWireAsync(items, CancellationToken.None);

        await Assert.That(items.Length).IsEqualTo(3);
        await Assert.That(items[0].ItemId).IsEqualTo("Bucket Brigade.Int4");
        await Assert.That(items[0].ClientHandle).IsEqualTo(0x501);
        await Assert.That(items[0].ServerHandle).IsGreaterThan(0);
        await Assert.That(items[0].CanonicalDataType).IsEqualTo(VarType.VT_I4);
        await Assert.That(items[0].AccessRights).IsEqualTo(0x3);
        await Assert.That(items[0].Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(items[1].CanonicalDataType).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(items[2].CanonicalDataType).IsEqualTo(VarType.VT_BOOL);
        await Assert.That(readResults.Length).IsEqualTo(3);
        await Assert.That(readResults[0].ClientHandle).IsEqualTo(0x501);
        await Assert.That(readResults[0].ServerHandle).IsEqualTo(items[0].ServerHandle);
        await Assert.That(readResults[0].Value.Type).IsEqualTo(VarType.VT_I4);
        await Assert.That(readResults[0].Value.AsInt32()).IsEqualTo(0);
        await Assert.That(readResults[0].Quality).IsEqualTo(OpcQuality.Good);
        await Assert.That(readResults[0].Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(readResults[0].Timestamp).IsNotEqualTo(default(DateTimeOffset));
        await Assert.That(readResults[1].Value.AsString()).IsEqualTo(string.Empty);
        await Assert.That(readResults[2].Value.AsBoolean()).IsEqualTo(false);
    }

    [Test, Category("EndToEnd")]
    public async Task WriteSync_Then_ServerReceivesValuesAndReadReflectsThem()
    {
        var pipeline = new DaEndToEndPipeline();
        int groupHandle = await AddDefaultGroupAsync(pipeline);
        DaAddItemResult[] items = await pipeline.AddItemsViaWireAsync(groupHandle, WritableItemRequests(), CancellationToken.None);
        OpcVariant[] values =
        [
            OpcVariant.FromInt32(42_424),
            OpcVariant.FromString("batch-β-17"),
            OpcVariant.FromBoolean(true),
        ];

        int[] errors = await pipeline.SyncIo.WriteAsync(items.Select(static item => item.ServerHandle).ToArray(), values, CancellationToken.None);
        DaReadResult[] readResults = await pipeline.ReadViaWireAsync(items, CancellationToken.None);

        await Assert.That(errors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.Ok.Code, OpcResultId.Ok.Code]);
        await Assert.That(pipeline.LastWrites.Count).IsEqualTo(3);
        await Assert.That(pipeline.LastWrites[0].ServerHandle).IsEqualTo(items[0].ServerHandle);
        await Assert.That(pipeline.LastWrites[0].Value).IsEqualTo(values[0]);
        await Assert.That(pipeline.LastWrites[0].Error).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(pipeline.LastWrites[1].Value).IsEqualTo(values[1]);
        await Assert.That(pipeline.LastWrites[2].Value).IsEqualTo(values[2]);
        await Assert.That(readResults[0].Value.AsInt32()).IsEqualTo(42_424);
        await Assert.That(readResults[1].Value.AsString()).IsEqualTo("batch-β-17");
        await Assert.That(readResults[2].Value.AsBoolean()).IsEqualTo(true);
        await Assert.That(readResults.All(static result => result.Quality == OpcQuality.Good)).IsTrue();
        await Assert.That(readResults.All(static result => result.Error == OpcResultId.Ok.Code)).IsTrue();
    }

    [Test, Category("EndToEnd")]
    public async Task SetActiveState_Then_ItemActiveStateToggles()
    {
        var pipeline = new DaEndToEndPipeline();
        int groupHandle = await AddDefaultGroupAsync(pipeline);
        DaAddItemResult[] items = await pipeline.AddItemsViaWireAsync(groupHandle, WritableItemRequests(), CancellationToken.None);
        int firstServerHandle = items[0].ServerHandle;

        int[] deactivateErrors = await pipeline.ItemMgt.SetActiveStateAsync([firstServerHandle], active: false, CancellationToken.None);
        bool inactive = pipeline.IsItemActive(firstServerHandle);
        int[] activateErrors = await pipeline.ItemMgt.SetActiveStateAsync([firstServerHandle], active: true, CancellationToken.None);
        bool active = pipeline.IsItemActive(firstServerHandle);

        await Assert.That(deactivateErrors).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(inactive).IsFalse();
        await Assert.That(activateErrors).IsEquivalentTo([OpcResultId.Ok.Code]);
        await Assert.That(active).IsTrue();
        await Assert.That(pipeline.Channel.CallLog.Count(static call => call.InterfaceId == IOPCItemMgt.InterfaceId)).IsEqualTo(3);
    }

    [Test, Category("EndToEnd")]
    public async Task BrowseAddressSpace_Then_HierarchicalResultsAreReturned()
    {
        var pipeline = new DaEndToEndPipeline();

        int organization = await pipeline.Browse.QueryOrganizationAsync(CancellationToken.None);
        await pipeline.Browse.ChangeBrowsePositionAsync(1, "Bucket Brigade", CancellationToken.None);
        string itemId = await pipeline.Browse.GetItemIdAsync("Int4", CancellationToken.None);
        string[] browsed = await pipeline.BrowseItemsViaWireAsync(CancellationToken.None);

        await Assert.That(organization).IsEqualTo(1);
        await Assert.That(itemId).IsEqualTo("Bucket Brigade.Int4");
        await Assert.That(browsed.Length).IsEqualTo(30);
        await Assert.That(browsed).Contains("Bucket Brigade.Int4");
        await Assert.That(browsed).Contains("Random.Real8");
        await Assert.That(browsed).Contains("Triangle Waves.Real4");
        await Assert.That(browsed.Where(static id => id.StartsWith("Bucket Brigade.", StringComparison.Ordinal)).Count()).IsEqualTo(10);
    }


    [Test, Category("EndToEnd")]
    public async Task InterfacePointerMethods_Then_ObjrefsFlowBack()
    {
        var pipeline = new DaEndToEndPipeline();

        await pipeline.Server.AddGroupAsync(
            "E2E.DA.InterfaceRefs",
            active: true,
            requestedUpdateRate: 1_000,
            clientGroupHandle: 0x6101,
            timeBias: 0,
            percentDeadband: 0.0f,
            localeId: 0x0409,
            requestedInterfaceId: IOPCItemMgt.InterfaceId,
            out int serverHandle,
            out int revisedRate,
            out IOpcInterfaceRef addGroupRef,
            CancellationToken.None);
        IOpcInterfaceRef byNameRef = await pipeline.Server.GetGroupByNameAsync("E2E.DA.InterfaceRefs", IOPCItemMgt.InterfaceId, CancellationToken.None);
        IOpcInterfaceRef groupEnumRef = await pipeline.Server.CreateGroupEnumeratorAsync(1, OpcGuids.IID_IEnumUnknown, CancellationToken.None);
        IOpcInterfaceRef itemEnumRef = await pipeline.ItemMgt.CreateEnumeratorAsync(OpcGuids.IID_IEnumOPCItemAttributes, CancellationToken.None);
        IOpcInterfaceRef cloneRef = await pipeline.GroupState.CloneGroupAsync("E2E.DA.InterfaceRefs.Clone", IOPCItemMgt.InterfaceId, CancellationToken.None);

        await Assert.That(serverHandle).IsEqualTo(0x6101 + 0x1000);
        await Assert.That(revisedRate).IsEqualTo(1_000);
        await Assert.That(addGroupRef.Iid).IsEqualTo(IOPCItemMgt.InterfaceId);
        await Assert.That(byNameRef.Iid).IsEqualTo(IOPCItemMgt.InterfaceId);
        await Assert.That(groupEnumRef.Iid).IsEqualTo(OpcGuids.IID_IEnumUnknown);
        await Assert.That(itemEnumRef.Iid).IsEqualTo(OpcGuids.IID_IEnumOPCItemAttributes);
        await Assert.That(cloneRef.Iid).IsEqualTo(IOPCItemMgt.InterfaceId);
        await Assert.That(pipeline.GroupCount).IsEqualTo(2);
    }

    [Test, Category("EndToEnd")]
    public async Task MultiOutMethods_Then_ValuesAndErrorsFlowBack()
    {
        var pipeline = new DaEndToEndPipeline();
        int groupHandle = await AddDefaultGroupAsync(pipeline);
        var definitions = WritableItemRequests()
            .Select(static item => new OpcItemDef(null, item.ItemId, Active: true, item.ClientHandle, Array.Empty<byte>(), VarType.VT_EMPTY))
            .ToArray();

        await pipeline.ItemMgt.ValidateItemsAsync(definitions, blobUpdate: false, out OpcItemResult[] validationResults, out int[] validationErrors, CancellationToken.None);
        await pipeline.GroupState.SetStateAsync(750, active: false, timeBias: -30, percentDeadband: 2.5f, localeId: 0x0407, clientGroupHandle: 0x6202, out int revisedRate, CancellationToken.None);
        int revisedKeepAlive = await pipeline.GroupState2.SetKeepAliveAsync(30_000, CancellationToken.None);
        int currentKeepAlive = await pipeline.GroupState2.GetKeepAliveAsync(CancellationToken.None);
        DaAddItemResult[] items = await pipeline.AddItemsViaWireAsync(groupHandle, WritableItemRequests(), CancellationToken.None);
        await pipeline.SyncIo2.ReadMaxAgeAsync(
            items.Select(static item => item.ServerHandle).ToArray(),
            items.Select(static _ => 1_000).ToArray(),
            out OpcVariant[] values,
            out ushort[] qualities,
            out long[] timestamps,
            out int[] readErrors,
            CancellationToken.None);
        Opc.Classic.Da.OpcGroupState state = await pipeline.GroupState.GetStateAsync(CancellationToken.None);

        await Assert.That(validationResults.Length).IsEqualTo(3);
        await Assert.That(validationErrors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.Ok.Code, OpcResultId.Ok.Code]);
        await Assert.That(revisedRate).IsEqualTo(750);
        await Assert.That(revisedKeepAlive).IsEqualTo(30_000);
        await Assert.That(currentKeepAlive).IsEqualTo(30_000);
        await Assert.That(state.Active).IsFalse();
        await Assert.That(state.UpdateRate).IsEqualTo(750);
        await Assert.That(state.TimeBias).IsEqualTo(-30);
        await Assert.That(state.PercentDeadband).IsEqualTo(2.5f);
        await Assert.That(values[0].AsInt32()).IsEqualTo(0);
        await Assert.That(qualities.All(static quality => quality == OpcQuality.Good.RawValue)).IsTrue();
        await Assert.That(timestamps.All(static timestamp => timestamp != 0)).IsTrue();
        await Assert.That(readErrors.All(static error => error == OpcResultId.Ok.Code)).IsTrue();
    }

    [Test, Category("EndToEnd")]
    public async Task AsyncIO2ReadWrite_Then_CancelIdsAndErrorsFlowBack()
    {
        var pipeline = new DaEndToEndPipeline();
        int groupHandle = await AddDefaultGroupAsync(pipeline);
        DaAddItemResult[] items = await pipeline.AddItemsViaWireAsync(groupHandle, WritableItemRequests(), CancellationToken.None);
        int[] handles = items.Select(static item => item.ServerHandle).ToArray();

        int readCancelId = await pipeline.AsyncIo2.ReadAsync(handles, transactionId: 0x7001, out int[] readErrors, CancellationToken.None);
        int writeCancelId = await pipeline.AsyncIo2.WriteAsync(handles, [OpcVariant.FromInt32(9), OpcVariant.FromString("async"), OpcVariant.FromBoolean(true)], transactionId: 0x7002, out int[] writeErrors, CancellationToken.None);
        DaReadResult[] readBack = await pipeline.ReadViaWireAsync(items, CancellationToken.None);

        await Assert.That(readCancelId).IsGreaterThan(0);
        await Assert.That(writeCancelId).IsGreaterThan(readCancelId);
        await Assert.That(readErrors.All(static error => error == OpcResultId.Ok.Code)).IsTrue();
        await Assert.That(writeErrors.All(static error => error == OpcResultId.Ok.Code)).IsTrue();
        await Assert.That(readBack[0].Value.AsInt32()).IsEqualTo(9);
        await Assert.That(readBack[1].Value.AsString()).IsEqualTo("async");
        await Assert.That(readBack[2].Value.AsBoolean()).IsTrue();
    }

    [Test, Category("EndToEnd")]
    public async Task GetErrorString_Then_KnownHresultMapsToMessage()
    {
        var pipeline = new DaEndToEndPipeline();
        int hresult = OpcResultId.BadRights.Code;

        string message = await pipeline.Server.GetErrorStringAsync(hresult, 0x0409, CancellationToken.None);

        await Assert.That(message).IsEqualTo($"Opc.Classic Sample DA error: 0x{hresult:X8}");
        await Assert.That(pipeline.Channel.CallLog[0].InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog[0].Opnum).IsEqualTo(IOPCServer.Opnums.GetErrorStringAsync);
        await Assert.That(pipeline.Channel.CallLog[0].PayloadLength).IsGreaterThan(0);
    }

    [Test, Category("EndToEnd")]
    public async Task RemoveGroup_Then_GroupIsTornDown()
    {
        var pipeline = new DaEndToEndPipeline();
        int groupHandle = await AddDefaultGroupAsync(pipeline);
        await Assert.That(pipeline.GroupExists(groupHandle)).IsTrue();

        await pipeline.Server.RemoveGroupAsync(groupHandle, force: true, CancellationToken.None);

        await Assert.That(pipeline.GroupExists(groupHandle)).IsFalse();
        await Assert.That(pipeline.GroupCount).IsEqualTo(0);
        await Assert.That(pipeline.Channel.CallLog.Last().InterfaceId).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(pipeline.Channel.CallLog.Last().Opnum).IsEqualTo(IOPCServer.Opnums.RemoveGroupAsync);
        await Assert.That(pipeline.Channel.CallLog.Last().PayloadLength).IsGreaterThan(0);
    }

    private static Task<int> AddDefaultGroupAsync(DaEndToEndPipeline pipeline) =>
        pipeline.AddGroupViaWireAsync("E2E.DA.Values", active: true, 500, 0x5201, 0x0409, CancellationToken.None);

    private static (string ItemId, int ClientHandle)[] WritableItemRequests() =>
    [
        ("Bucket Brigade.Int4", 0x501),
        ("Bucket Brigade.String", 0x502),
        ("Bucket Brigade.Boolean", 0x503),
    ];
}
