// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Dispatcher tests assert protocol constants and captured call values.

using System.Runtime.CompilerServices;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Hda.Tests.Hosting;

public sealed class OpcHdaServerDispatcherRoundTripAdditionalTests
{
    [Test]
    public async Task DispatchAsync_GeneratedProxyRoundTripsServerMethods_ReturnsConcreteResults()
    {
        var server = new RecordingHdaServer();
        IOpcHdaServerDispatcher dispatcher = new OpcHdaServerDispatcher(server);
        InMemoryCallChannel channel = CreateChannel(dispatcher);
        var proxy = new IOPCHDA_ServerClientProxy(channel);

        await proxy.GetItemAttributesAsync(
            out int[] attributeIds,
            out string[] attributeNames,
            out string[] attributeDescriptions,
            out int[] attributeTypes,
            CancellationToken.None);
        await proxy.GetAggregatesAsync(out int[] aggregateIds, out string[] aggregateNames, out string[] aggregateDescriptions, CancellationToken.None);
        OpcServerStatus status = await proxy.GetStatusAsync(CancellationToken.None);
        int[] handles = await proxy.GetItemHandlesAsync(["Plant.Area.Tag1", "Plant.Area.Tag2"], [101, 102], CancellationToken.None);
        int[] releaseErrors = await proxy.ReleaseItemHandlesAsync(handles, CancellationToken.None);
        int[] validateErrors = await proxy.ValidateItemIDsAsync(["Plant.Area.Tag1", "Missing"], CancellationToken.None);

        await Assert.That(attributeIds).IsEquivalentTo([1, 2]);
        await Assert.That(attributeNames).IsEquivalentTo(["DataType", "Description"]);
        await Assert.That(attributeDescriptions).IsEquivalentTo(["Variant type", "Human text"]);
        await Assert.That(attributeTypes).IsEquivalentTo([(int)VarType.VT_I4, (int)VarType.VT_BSTR]);
        await Assert.That(aggregateIds).IsEquivalentTo([1, 4]);
        await Assert.That(aggregateNames).IsEquivalentTo(["Interpolative", "Average"]);
        await Assert.That(aggregateDescriptions).IsEquivalentTo(["Interpolated value", "Time average"]);
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Hda);
        await Assert.That(status.MaxReturnValues).IsEqualTo(500);
        await Assert.That(handles).IsEquivalentTo([501, 502]);
        await Assert.That(releaseErrors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.InvalidHandle.Code]);
        await Assert.That(validateErrors).IsEquivalentTo([OpcResultId.Ok.Code, OpcResultId.UnknownItemId.Code]);
        await Assert.That(server.LastItemIds).IsEquivalentTo(["Plant.Area.Tag1", "Missing"]);
        await Assert.That(server.LastClientHandles).IsEquivalentTo([101, 102]);
        await Assert.That(channel.CallLog.Select(static call => call.Opnum).ToArray()).IsEquivalentTo([
            IOPCHDA_Server.Opnums.GetItemAttributesAsync,
            IOPCHDA_Server.Opnums.GetAggregatesAsync,
            IOPCHDA_Server.Opnums.GetStatusAsync,
            IOPCHDA_Server.Opnums.GetItemHandlesAsync,
            IOPCHDA_Server.Opnums.ReleaseItemHandlesAsync,
            IOPCHDA_Server.Opnums.ValidateItemIDsAsync]);
    }

    [Test]
    public async Task DispatcherBrowseHelpers_ValidateFiltersBrowseAndMove_ReturnConcreteValues()
    {
        var server = new RecordingHdaServer();
        IOpcHdaServerDispatcher dispatcher = new OpcHdaServerDispatcher(server);

        int[] filterErrors = await dispatcher.ValidateBrowseFiltersAsync([
            new OpcHdaBrowseFilter(1, 1, OpcVariant.FromString("Plant")),
            new OpcHdaBrowseFilter(99, 1, OpcVariant.FromString("Missing")),
            new OpcHdaBrowseFilter(0, 1, OpcVariant.Empty),
            new OpcHdaBrowseFilter(1, 99, OpcVariant.Empty)], CancellationToken.None);
        IReadOnlyList<string> branches = await dispatcher.BrowseAsync("Plant", HdaBrowseType.Branch, [], CancellationToken.None);
        IReadOnlyList<string> items = await dispatcher.BrowseAsync("Plant", HdaBrowseType.Items, [], CancellationToken.None);
        string down = await dispatcher.ChangeBrowsePositionAsync("Plant", 2, "Area", CancellationToken.None);
        string up = await dispatcher.ChangeBrowsePositionAsync("Plant.Area", 1, null, CancellationToken.None);
        string itemId = await dispatcher.GetItemIdAsync("Plant.Area", "Tag1", CancellationToken.None);
        string current = await dispatcher.GetBranchPositionAsync("Plant.Area", CancellationToken.None);

        await Assert.That(filterErrors).IsEquivalentTo([
            OpcResultId.Ok.Code,
            OpcHdaErrors.OPCHDA_E_UNKNOWNATTRID,
            OpcHdaErrors.OPCHDA_E_INVALIDATTRID,
            OpcResultId.InvalidArg.Code]);
        await Assert.That(branches).IsEquivalentTo(["Area"]);
        await Assert.That(items).IsEquivalentTo(["Plant.Area.Tag1", "Flat.Tag"]);
        await Assert.That(down).IsEqualTo("Plant.Area");
        await Assert.That(up).IsEqualTo("Plant");
        await Assert.That(itemId).IsEqualTo("Plant.Area.Tag1");
        await Assert.That(current).IsEqualTo("Plant.Area");
        await Assert.That(server.LastBrowseBranch).IsEqualTo("Plant");
    }

    [Test]
    public async Task DispatcherUpdateHelpers_DelegateToSyncUpdateAndAsyncFallback_ReturnConcreteErrors()
    {
        var server = new RecordingHdaServer();
        IOpcHdaServerDispatcher dispatcher = new OpcHdaServerDispatcher(server);
        long firstTime = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero).ToFileTime();
        OpcVariant[] values = [OpcVariant.FromDouble(12.5), OpcVariant.FromString("Manual")];

        int capabilities = await dispatcher.UpdateCapabilitiesAsync(CancellationToken.None);
        int[] insertErrors = await dispatcher.InsertAsync([501, 502], [firstTime, firstTime + 1], values, [192, 216], CancellationToken.None);
        int[] replaceErrors = await dispatcher.ReplaceAsync([501], [firstTime], [OpcVariant.FromInt32(42)], [192], CancellationToken.None);
        int[] deleteErrors = await dispatcher.DeleteAtTimeAsync([501, 502], [firstTime, firstTime + 1], CancellationToken.None);
        OpcHdaAsyncUpdateResult asyncInsert = await dispatcher.BeginAsyncInsertAsync(
            77,
            [501, 502],
            [firstTime, firstTime + 1],
            values,
            [192, 216],
            CancellationToken.None);

        await Assert.That(capabilities).IsEqualTo(0x1F);
        await Assert.That(insertErrors).IsEquivalentTo([OpcHdaErrors.OPCHDA_S_INSERTED, OpcHdaErrors.OPCHDA_E_DATAEXISTS]);
        await Assert.That(replaceErrors).IsEquivalentTo([OpcHdaErrors.OPCHDA_S_REPLACED]);
        await Assert.That(deleteErrors).IsEquivalentTo([OpcResultId.Ok.Code, OpcHdaErrors.OPCHDA_E_NODATAEXISTS]);
        await Assert.That(asyncInsert.CancelId).IsEqualTo(0);
        await Assert.That(asyncInsert.ClientHandles).IsEquivalentTo([501, 502]);
        await Assert.That(asyncInsert.Errors).IsEquivalentTo([OpcHdaErrors.OPCHDA_S_INSERTED, OpcHdaErrors.OPCHDA_E_DATAEXISTS]);
        await Assert.That(server.LastInsertHandles).IsEquivalentTo([501, 502]);
        await Assert.That(server.LastInsertValues[0].AsDouble()).IsEqualTo(12.5d);
        await Assert.That(server.LastInsertQualities).IsEquivalentTo([192, 216]);
    }

    [Test]
    public async Task DispatchAsync_UnknownInterface_ReturnsNotImplementedWithoutCallingServer()
    {
        var server = new RecordingHdaServer();
        IOpcHdaServerDispatcher dispatcher = new OpcHdaServerDispatcher(server);

        NdrCallResult result = await dispatcher.DispatchAsync(
            Guid.Parse("99999999-8888-7777-6666-555555555555"),
            IOPCHDA_Server.Opnums.GetStatusAsync,
            ReadOnlyMemory<byte>.Empty,
            CancellationToken.None);

        await Assert.That(result.Hresult).IsEqualTo(OpcResultId.NotImplemented.Code);
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
        await Assert.That(server.GetStatusCallCount).IsEqualTo(0);
    }

    private static InMemoryCallChannel CreateChannel(IOpcHdaServerDispatcher dispatcher) =>
        new((iid, opnum, payload, cancellationToken) =>
            dispatcher.DispatchAsync(iid, opnum, payload, cancellationToken));

    private static OpcServerStatus CreateStatus() => new()
    {
        Spec = OpcStatusSpec.Hda,
        StartTime = DateTimeOffset.UnixEpoch,
        CurrentTime = DateTimeOffset.UnixEpoch.AddSeconds(10),
        LastUpdateTime = DateTimeOffset.UnixEpoch.AddSeconds(11),
        State = OpcServerState.Running,
        ServerVersion = new Version(1, 20, 7),
        MaxReturnValues = 500,
        VendorInfo = "HDA adapter round-trip",
    };

    private sealed class RecordingHdaServer : IOpcHdaServer, IOPCHDA_SyncUpdate
    {
        public int GetStatusCallCount { get; private set; }
        public string[] LastItemIds { get; private set; } = [];
        public int[] LastClientHandles { get; private set; } = [];
        public int[] LastReleasedHandles { get; private set; } = [];
        public string? LastBrowseBranch { get; private set; }
        public int[] LastInsertHandles { get; private set; } = [];
        public OpcVariant[] LastInsertValues { get; private set; } = [];
        public int[] LastInsertQualities { get; private set; } = [];

        public Task GetItemAttributesAsync(
            out int[] attributeIds,
            out string[] attributeNames,
            out string[] attributeDescriptions,
            out int[] attributeDataTypes,
            CancellationToken cancellationToken = default)
        {
            attributeIds = [1, 2];
            attributeNames = ["DataType", "Description"];
            attributeDescriptions = ["Variant type", "Human text"];
            attributeDataTypes = [(int)VarType.VT_I4, (int)VarType.VT_BSTR];
            return Task.CompletedTask;
        }

        public Task GetAggregatesAsync(
            out int[] aggregateIds,
            out string[] aggregateNames,
            out string[] aggregateDescriptions,
            CancellationToken cancellationToken = default)
        {
            aggregateIds = [1, 4];
            aggregateNames = ["Interpolative", "Average"];
            aggregateDescriptions = ["Interpolated value", "Time average"];
            return Task.CompletedTask;
        }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            GetStatusCallCount++;
            return Task.FromResult(CreateStatus());
        }

        public Task<int[]> GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken = default)
        {
            LastItemIds = itemIds;
            LastClientHandles = clientHandles;
            return Task.FromResult(new[] { 501, 502 });
        }

        public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
        {
            LastReleasedHandles = serverHandles;
            return Task.FromResult(new[] { OpcResultId.Ok.Code, OpcResultId.InvalidHandle.Code });
        }

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
        {
            LastItemIds = itemIds;
            return Task.FromResult(itemIds
                .Select(static itemId => itemId == "Missing" ? OpcResultId.UnknownItemId.Code : OpcResultId.Ok.Code)
                .ToArray());
        }

        public async IAsyncEnumerable<HdaBrowseElement> BrowseAsync(
            string branchPosition,
            HdaBrowseType browseType,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            LastBrowseBranch = branchPosition;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            yield return new HdaBrowseElement { Name = "Area", ItemId = "Plant.Area", BrowseType = HdaBrowseType.Branch };
            yield return new HdaBrowseElement { Name = "Tag1", ItemId = "Plant.Area.Tag1", BrowseType = HdaBrowseType.Leaf };
            yield return new HdaBrowseElement { Name = "Flat.Tag", ItemId = "Flat.Tag", BrowseType = HdaBrowseType.Flat };
        }

        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(0x1F);

        public Task<int[]> InsertAsync(
            int[] serverHandles,
            long[] timestampFileTimes,
            OpcVariant[] dataValues,
            int[] qualities,
            CancellationToken cancellationToken = default)
        {
            _ = timestampFileTimes;
            LastInsertHandles = serverHandles;
            LastInsertValues = dataValues;
            LastInsertQualities = qualities;
            return Task.FromResult(new[] { OpcHdaErrors.OPCHDA_S_INSERTED, OpcHdaErrors.OPCHDA_E_DATAEXISTS });
        }

        public Task<int[]> ReplaceAsync(
            int[] serverHandles,
            long[] timestampFileTimes,
            OpcVariant[] dataValues,
            int[] qualities,
            CancellationToken cancellationToken = default)
        {
            _ = serverHandles;
            _ = timestampFileTimes;
            _ = dataValues;
            _ = qualities;
            return Task.FromResult(new[] { OpcHdaErrors.OPCHDA_S_REPLACED });
        }

        public Task<int[]> InsertReplaceAsync(
            int[] serverHandles,
            long[] timestampFileTimes,
            OpcVariant[] dataValues,
            int[] qualities,
            CancellationToken cancellationToken = default)
        {
            _ = serverHandles;
            _ = timestampFileTimes;
            _ = dataValues;
            _ = qualities;
            return Task.FromResult(new[] { OpcResultId.Ok.Code });
        }

        public Task<int[]> DeleteRawAsync(
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            int[] serverHandles,
            CancellationToken cancellationToken = default)
        {
            _ = startTime;
            _ = endTime;
            _ = serverHandles;
            return Task.FromResult(new[] { OpcResultId.Ok.Code });
        }

        public Task<int[]> DeleteAtTimeAsync(
            int[] serverHandles,
            long[] timestampFileTimes,
            CancellationToken cancellationToken = default)
        {
            _ = serverHandles;
            _ = timestampFileTimes;
            return Task.FromResult(new[] { OpcResultId.Ok.Code, OpcHdaErrors.OPCHDA_E_NODATAEXISTS });
        }
    }
}
