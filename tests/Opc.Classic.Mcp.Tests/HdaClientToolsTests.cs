//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.CompilerServices;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Testing;

namespace Opc.Classic.Mcp.Tests;

public sealed class HdaClientToolsTests
{
    [Test]
    public async Task Hda_connect_status_browse_and_disconnect_round_trip_via_mcp_client()
    {
        var syntheticHda = new SyntheticHdaServer();
        string channelName = "hda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryHdaConnectionRegistry.Register(channelName, syntheticHda.Channel, syntheticHda);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcResultDto connected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.hda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);
        OpcServerStatusDto status = await server.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.hda.get_status",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcHdaBrowseElementDto[] browse = await server.CallToolAsync<OpcHdaBrowseElementDto[]>(
            "opcclassic.hda.browse",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId, ["browseType"] = "flat" }).ConfigureAwait(false);
        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.hda.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Spec).IsEqualTo("Hda");
        await Assert.That(status.VendorInfo).IsEqualTo("Synthetic MCP HDA Server");
        await Assert.That(browse.Select(static element => element.ItemId)).Contains("Sensor.Temperature");
        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    [Test]
    public async Task Hda_validate_get_and_release_item_handles_via_mcp_client()
    {
        var syntheticHda = new SyntheticHdaServer();
        string channelName = "hda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryHdaConnectionRegistry.Register(channelName, syntheticHda.Channel, syntheticHda);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await CreateConnectedSessionAsync(server, channelName).ConfigureAwait(false);

        OpcHdaItemHandleDto[] validated = await server.CallToolAsync<OpcHdaItemHandleDto[]>(
            "opcclassic.hda.validate_items",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["itemIds"] = new[] { "Sensor.Temperature", "Missing.Tag" },
            }).ConfigureAwait(false);
        OpcHdaItemHandleDto[] handles = await server.CallToolAsync<OpcHdaItemHandleDto[]>(
            "opcclassic.hda.get_item_handles",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["itemIds"] = new[] { "Sensor.Temperature", "Sensor.Pressure" },
                ["clientHandles"] = new[] { 41, 42 },
            }).ConfigureAwait(false);
        OpcResultDto[] released = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.release_item_handles",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = handles.Select(static handle => handle.ServerHandle).ToArray(),
            }).ConfigureAwait(false);

        await Assert.That(validated[0].Succeeded).IsTrue();
        await Assert.That(validated[1].Succeeded).IsFalse();
        await Assert.That(handles.Length).IsEqualTo(2);
        await Assert.That(handles.All(static handle => handle.ServerHandle > 0)).IsTrue();
        await Assert.That(released.All(static result => result.Succeeded)).IsTrue();
    }

    [Test]
    public async Task Hda_read_raw_processed_and_at_time_via_mcp_client()
    {
        var syntheticHda = new SyntheticHdaServer();
        string channelName = "hda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryHdaConnectionRegistry.Register(channelName, syntheticHda.Channel, syntheticHda);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await CreateConnectedSessionAsync(server, channelName).ConfigureAwait(false);
        int[] handles = await GetHandlesAsync(server, session.SessionId).ConfigureAwait(false);

        OpcHdaReadResultDto[] raw = await server.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_raw",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = handles,
                ["startTime"] = "NOW-1H",
                ["endTime"] = "NOW",
                ["maxValuesPerItem"] = 5,
            }).ConfigureAwait(false);
        OpcHdaReadResultDto[] processed = await server.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_processed",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = handles,
                ["startTime"] = "NOW-1H",
                ["endTime"] = "NOW",
                ["resampleIntervalSeconds"] = 60,
                ["aggregate"] = "Average",
            }).ConfigureAwait(false);
        OpcHdaReadResultDto[] atTime = await server.CallToolAsync<OpcHdaReadResultDto[]>(
            "opcclassic.hda.read_at_time",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = handles,
                ["timestamps"] = new[] { DateTimeOffset.UtcNow.AddMinutes(-5) },
            }).ConfigureAwait(false);

        await Assert.That(raw.Length).IsEqualTo(2);
        await Assert.That(raw[0].Values.Count).IsGreaterThan(0);
        await Assert.That(processed[0].Aggregate).IsEqualTo("Average");
        await Assert.That(atTime[0].Values.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Hda_read_modified_attributes_annotations_and_aggregates_via_mcp_client()
    {
        var syntheticHda = new SyntheticHdaServer();
        string channelName = "hda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryHdaConnectionRegistry.Register(channelName, syntheticHda.Channel, syntheticHda);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await CreateConnectedSessionAsync(server, channelName).ConfigureAwait(false);
        int[] handles = await GetHandlesAsync(server, session.SessionId).ConfigureAwait(false);

        OpcHdaModifiedReadResultDto[] modified = await server.CallToolAsync<OpcHdaModifiedReadResultDto[]>(
            "opcclassic.hda.read_modified",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = handles,
                ["startTime"] = "NOW-1H",
                ["endTime"] = "NOW",
            }).ConfigureAwait(false);
        OpcHdaAttributeResultDto[] attributes = await server.CallToolAsync<OpcHdaAttributeResultDto[]>(
            "opcclassic.hda.read_attribute",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandle"] = handles[0],
                ["attributeIds"] = new[] { 3 },
                ["startTime"] = "NOW-1H",
                ["endTime"] = "NOW",
            }).ConfigureAwait(false);
        OpcHdaAnnotationResultDto[] annotations = await server.CallToolAsync<OpcHdaAnnotationResultDto[]>(
            "opcclassic.hda.read_annotations",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = handles,
                ["startTime"] = "NOW-1H",
                ["endTime"] = "NOW",
            }).ConfigureAwait(false);
        OpcHdaAggregateDto[] aggregates = await server.CallToolAsync<OpcHdaAggregateDto[]>(
            "opcclassic.hda.get_aggregates",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(modified[0].Values[0].User).IsEqualTo("historian");
        await Assert.That(attributes[0].AttributeId).IsEqualTo(3);
        await Assert.That(annotations[0].Annotations[0].AnnotationText).Contains("Calibration");
        await Assert.That(aggregates.Select(static aggregate => aggregate.Name)).Contains("Average");
    }

    [Test]
    public async Task Hda_update_delete_and_insert_annotations_via_mcp_client()
    {
        var syntheticHda = new SyntheticHdaServer();
        string channelName = "hda-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryHdaConnectionRegistry.Register(channelName, syntheticHda.Channel, syntheticHda);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await CreateConnectedSessionAsync(server, channelName).ConfigureAwait(false);
        int[] handles = await GetHandlesAsync(server, session.SessionId).ConfigureAwait(false);
        DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddMinutes(-1);

        OpcResultDto[] inserted = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.insert_data",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = new[] { handles[0] },
                ["timestamps"] = new[] { timestamp },
                ["values"] = new[] { 42.5 },
            }).ConfigureAwait(false);
        OpcResultDto[] replaced = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.replace_data",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = new[] { handles[0] },
                ["timestamps"] = new[] { timestamp },
                ["values"] = new[] { 43.5 },
            }).ConfigureAwait(false);
        OpcResultDto[] insertReplaced = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.insert_replace_data",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = new[] { handles[0] },
                ["timestamps"] = new[] { timestamp },
                ["values"] = new[] { 44.5 },
            }).ConfigureAwait(false);
        OpcResultDto[] deleteRaw = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.delete_raw",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = new[] { handles[0] },
                ["startTime"] = "NOW-1H",
                ["endTime"] = "NOW",
            }).ConfigureAwait(false);
        OpcResultDto[] deleteAtTime = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.delete_at_time",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = new[] { handles[0] },
                ["timestamps"] = new[] { timestamp },
            }).ConfigureAwait(false);
        OpcResultDto[] annotationInsert = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.hda.insert_annotations",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["serverHandles"] = new[] { handles[0] },
                ["timestamps"] = new[] { timestamp },
                ["annotationTexts"] = new[] { "operator note" },
                ["users"] = new[] { "tester" },
            }).ConfigureAwait(false);

        await Assert.That(inserted[0].Succeeded).IsTrue();
        await Assert.That(replaced[0].Succeeded).IsTrue();
        await Assert.That(insertReplaced[0].Succeeded).IsTrue();
        await Assert.That(deleteRaw[0].Succeeded).IsTrue();
        await Assert.That(deleteAtTime[0].Succeeded).IsTrue();
        await Assert.That(annotationInsert[0].Succeeded).IsTrue();
    }

    private static async Task<OpcSessionDto> CreateConnectedSessionAsync(McpAeHdaTestServer server, string channelName)
    {
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.hda.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);
        return session;
    }

    private static async Task<int[]> GetHandlesAsync(McpAeHdaTestServer server, string sessionId)
    {
        OpcHdaItemHandleDto[] handles = await server.CallToolAsync<OpcHdaItemHandleDto[]>(
            "opcclassic.hda.get_item_handles",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemIds"] = new[] { "Sensor.Temperature", "Sensor.Pressure" },
            }).ConfigureAwait(false);
        return handles.Select(static handle => handle.ServerHandle).ToArray();
    }
}

internal sealed class SyntheticHdaServer : IOpcHdaServer, IHdaServer, IOPCHDA_SyncRead, IOPCHDA_SyncUpdate, IOPCHDA_SyncAnnotations
{
    private static readonly DateTimeOffset Startup = DateTimeOffset.UtcNow.AddHours(-1);
    private readonly Dictionary<string, double> _values = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Sensor.Temperature"] = 21.5,
        ["Sensor.Pressure"] = 101.25,
    };
    private readonly Dictionary<int, HandleRegistration> _handles = new();
    private readonly OpcHdaServerDispatcher _serverDispatcher;
    private readonly IOPCHDA_SyncReadServerDispatcher _syncReadDispatcher;
    private readonly IOPCHDA_SyncUpdateServerDispatcher _syncUpdateDispatcher;
    private readonly IOPCHDA_SyncAnnotationsServerDispatcher _syncAnnotationsDispatcher;
    private int _nextHandle = 2000;

    public SyntheticHdaServer()
    {
        _serverDispatcher = new OpcHdaServerDispatcher(this);
        _syncReadDispatcher = new IOPCHDA_SyncReadServerDispatcher(this);
        _syncUpdateDispatcher = new IOPCHDA_SyncUpdateServerDispatcher(this);
        _syncAnnotationsDispatcher = new IOPCHDA_SyncAnnotationsServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    public event EventHandler<EventArgs>? ServerShutdown;

    public InMemoryCallChannel Channel { get; }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = Startup,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 2, 3),
            VendorInfo = "Synthetic MCP HDA Server",
            MaxReturnValues = 1000,
        });
    }

    public async IAsyncEnumerable<HdaBrowseElement> BrowseAsync(string itemIdPrefix, HdaBrowseType browseType, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(itemIdPrefix) && browseType is HdaBrowseType.Branch or HdaBrowseType.Flat)
        {
            yield return new HdaBrowseElement { Name = "Sensor", ItemId = "Sensor", BrowseType = HdaBrowseType.Branch };
        }

        if (browseType is HdaBrowseType.Leaf or HdaBrowseType.Flat)
        {
            foreach (string itemId in _values.Keys.Order(StringComparer.OrdinalIgnoreCase))
            {
                yield return new HdaBrowseElement { Name = itemId[(itemId.LastIndexOf('.') + 1)..], ItemId = itemId, BrowseType = HdaBrowseType.Leaf };
            }
        }
    }

    public Task<IReadOnlyList<HdaAggregate>> GetSupportedAggregatesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<HdaAggregate>>([HdaAggregate.Average, HdaAggregate.Minimum, HdaAggregate.Maximum]);
    }

    public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(itemIds.Select(itemId => _values.ContainsKey(itemId) ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code).ToArray());
    }

    public Task<int[]> GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        cancellationToken.ThrowIfCancellationRequested();
        var handles = new int[itemIds.Length];
        for (int i = 0; i < itemIds.Length; i++)
        {
            if (!_values.ContainsKey(itemIds[i]))
            {
                continue;
            }

            int handle = Interlocked.Increment(ref _nextHandle);
            int clientHandle = i < clientHandles.Length ? clientHandles[i] : i + 1;
            _handles[handle] = new HandleRegistration(itemIds[i], clientHandle);
            handles[i] = handle;
        }

        return Task.FromResult(handles);
    }

    public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        var errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            errors[i] = _handles.Remove(serverHandles[i]) ? OpcResultId.Ok.Code : OpcResultId.InvalidHandle.Code;
        }

        return Task.FromResult(errors);
    }

    public Task GetItemAttributesAsync(out int[] attributeIds, out string[] attributeNames, out string[] attributeDescriptions, out int[] attributeDataTypes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        attributeIds = [1, 2, 3];
        attributeNames = ["DataType", "Description", "EngUnits"];
        attributeDescriptions = ["Canonical data type", "Description", "Engineering units"];
        attributeDataTypes = [(int)VarType.VT_I2, (int)VarType.VT_BSTR, (int)VarType.VT_BSTR];
        return Task.CompletedTask;
    }

    public Task GetAggregatesAsync(out int[] aggregateIds, out string[] aggregateNames, out string[] aggregateDescriptions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        aggregateIds = [(int)HdaAggregate.Average, (int)HdaAggregate.Minimum, (int)HdaAggregate.Maximum];
        aggregateNames = ["Average", "Minimum", "Maximum"];
        aggregateDescriptions = ["Time-weighted average", "Minimum value", "Maximum value"];
        return Task.CompletedTask;
    }

    public Task<OpcHdaItem[]> ReadRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        _ = maxValues;
        _ = bounds;
        return Task.FromResult(serverHandles.Select(handle => CreateItem(handle, 0, ReadValue(handle))).ToArray());
    }

    public Task<OpcHdaItem[]> ReadProcessedAsync(OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        _ = resampleIntervalFileTime;
        return Task.FromResult(serverHandles.Select((handle, index) => CreateItem(handle, index < aggregateIds.Length ? aggregateIds[index] : (int)HdaAggregate.Average, ReadValue(handle) + 1.0)).ToArray());
    }

    public Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset[] timestamps = timestampFileTimes.Select(DateTimeOffset.FromFileTime).ToArray();
        return Task.FromResult(serverHandles.Select(handle => CreateItem(handle, 0, ReadValue(handle), timestamps)).ToArray());
    }

    public Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        _ = maxValues;
        return Task.FromResult(serverHandles.Select(handle => new OpcHdaModifiedItem(
            ClientHandle(handle),
            [DateTimeOffset.UtcNow.AddMinutes(-10)],
            [(uint)OpcQuality.Good.RawValue],
            [OpcVariant.FromDouble(ReadValue(handle))],
            [DateTimeOffset.UtcNow.AddMinutes(-5)],
            [1u],
            ["historian"])).ToArray());
    }

    public Task<OpcHdaAttribute[]> ReadAttributeAsync(OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, int[] attributeIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        return Task.FromResult(attributeIds.Select(attributeId => new OpcHdaAttribute(
            ClientHandle(serverHandle),
            attributeId,
            [DateTimeOffset.UtcNow],
            [OpcVariant.FromString(attributeId == 3 ? "degC" : "attribute")])).ToArray());
    }

    public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(0x1F);
    }

    public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = timestampFileTimes;
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<OpcHdaAnnotation[]> ReadAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        return Task.FromResult(serverHandles.Select(handle => new OpcHdaAnnotation(
            ClientHandle(handle),
            [DateTimeOffset.UtcNow.AddMinutes(-30)],
            [$"Calibration note for {ItemId(handle)}"],
            [DateTimeOffset.UtcNow.AddMinutes(-29)],
            ["historian"])).ToArray());
    }

    public Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = timestampFileTimes;
        _ = annotationValues;
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<IReadOnlyList<HdaReadResult>> ReadRawAsync(IReadOnlyList<string> itemIds, HdaTime startTime, HdaTime endTime, int maxValuesPerItem, bool includeBounds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        _ = maxValuesPerItem;
        _ = includeBounds;
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(itemId => new HdaReadResult
        {
            ItemId = itemId,
            Values = [new HdaItemValue { Timestamp = DateTimeOffset.UtcNow, Value = _values.GetValueOrDefault(itemId), Quality = OpcQuality.Good }],
        }).ToArray());
    }

    public Task<IReadOnlyList<HdaReadResult>> ReadProcessedAsync(IReadOnlyList<AggregateRequest> requests, HdaTime startTime, HdaTime endTime, TimeSpan resampleInterval, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        _ = resampleInterval;
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(requests.Select(request => new HdaReadResult
        {
            ItemId = request.ItemId,
            Values = [new HdaItemValue { Timestamp = DateTimeOffset.UtcNow, Value = _values.GetValueOrDefault(request.ItemId), Quality = OpcQuality.Good }],
        }).ToArray());
    }

    public Task<IReadOnlyList<HdaReadResult>> ReadAtTimeAsync(IReadOnlyList<string> itemIds, IReadOnlyList<DateTimeOffset> timestamps, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(itemId => new HdaReadResult
        {
            ItemId = itemId,
            Values = timestamps.Select(timestamp => new HdaItemValue { Timestamp = timestamp, Value = _values.GetValueOrDefault(itemId), Quality = OpcQuality.Good }).ToArray(),
        }).ToArray());
    }

    public Task<IReadOnlyList<HdaAnnotationResult>> ReadAnnotationsAsync(IReadOnlyList<string> itemIds, HdaTime startTime, HdaTime endTime, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        return Task.FromResult<IReadOnlyList<HdaAnnotationResult>>(itemIds.Select(itemId => new HdaAnnotationResult
        {
            ItemId = itemId,
            Annotations = [new HdaAnnotation { Timestamp = DateTimeOffset.UtcNow, AnnotationTime = DateTimeOffset.UtcNow, AnnotationText = "Calibration", User = "historian" }],
        }).ToArray());
    }

    public Task<IReadOnlyList<HdaReadResult>> ReadNextAsync(IReadOnlyList<string> itemIds, IReadOnlyList<int> continuationHandles, int maxValuesPerItem, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = continuationHandles;
        _ = maxValuesPerItem;
        return Task.FromResult<IReadOnlyList<HdaReadResult>>(itemIds.Select(static itemId => new HdaReadResult { ItemId = itemId }).ToArray());
    }

    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        if (interfaceId == IOPCHDA_Server.InterfaceId)
        {
            return _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCHDA_SyncRead.InterfaceId)
        {
            return ToCallResultAsync(_syncReadDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCHDA_SyncUpdate.InterfaceId)
        {
            return ToCallResultAsync(_syncUpdateDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCHDA_SyncAnnotations.InterfaceId)
        {
            return ToCallResultAsync(_syncAnnotationsDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
    }

    private static async Task<NdrCallResult> ToCallResultAsync(ValueTask<DispatchResult> dispatch) =>
        (await dispatch.ConfigureAwait(false)).ToNdrCallResult();

    private OpcHdaItem CreateItem(int serverHandle, int aggregateHandle, double value, DateTimeOffset[]? timestamps = null)
    {
        timestamps ??= [DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow];
        return new OpcHdaItem(
            ClientHandle(serverHandle),
            aggregateHandle,
            timestamps,
            timestamps.Select(static _ => (uint)OpcQuality.Good.RawValue).ToArray(),
            timestamps.Select((_, index) => OpcVariant.FromDouble(value + index)).ToArray());
    }

    private double ReadValue(int serverHandle) => _values.GetValueOrDefault(ItemId(serverHandle));

    private int ClientHandle(int serverHandle) => _handles.TryGetValue(serverHandle, out HandleRegistration registration) ? registration.ClientHandle : serverHandle;

    private string ItemId(int serverHandle) => _handles.TryGetValue(serverHandle, out HandleRegistration registration) ? registration.ItemId : string.Empty;

    private readonly record struct HandleRegistration(string ItemId, int ClientHandle);
}
