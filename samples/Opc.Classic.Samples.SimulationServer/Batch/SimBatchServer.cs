// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Batch;
using Opc.Classic.Batch.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.SimulationServer.Batch;

/// <summary>
/// Deterministic in-memory OPC Batch server used by the simulation sample.
/// </summary>
public sealed class SimBatchServer : IOPCBatchServer, IOPCBatchServer2, IEnumOPCBatchSummary, IOPCEnumerationSets
{
    private static readonly OpcBatchSummary[] Summaries =
    [
        new(
            "B-2026-001",
            "Starter culture fermentation",
            "Batch.B-2026-001",
            "MR-FERM-10",
            10.5f,
            "kg",
            "RUNNING",
            "AUTOMATIC",
            new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 1, 16, 0, 0, TimeSpan.Zero)),
        new(
            "B-2026-002",
            "CIP rinse cycle",
            "Batch.B-2026-002",
            "MR-CIP-20",
            12.0f,
            "kg",
            "COMPLETE",
            "MANUAL",
            new DateTimeOffset(2026, 1, 2, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 2, 10, 30, 0, TimeSpan.Zero)),
        new(
            "B-2026-003",
            "Packaging line warm-up",
            "Batch.B-2026-003",
            "MR-PACK-30",
            8.25f,
            "kg",
            "RUNNING",
            "AUTOMATIC",
            new DateTimeOffset(2026, 1, 3, 8, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 1, 3, 8, 0, 0, TimeSpan.Zero)),
    ];

    private static readonly SimEnumerationSet[] EnumerationSets =
    [
        new(0, "OPCB_ENUM_PHYS", [new(0, "PROCESS_CELL"), new(1, "UNIT"), new(2, "EQUIPMENT_MODULE")]),
        new(1, "OPCB_ENUM_MODE", [new(0, "AUTOMATIC"), new(1, "MANUAL")]),
        new(2, "OPCB_ENUM_STATE", [new(0, "IDLE"), new(1, "RUNNING"), new(2, "COMPLETE"), new(3, "HELD")]),
    ];

    private readonly object _gate = new();
    private readonly IOPCBatchServerServerDispatcher _batchServerDispatcher;
    private readonly IOPCBatchServer2ServerDispatcher _batchServer2Dispatcher;
    private readonly IEnumOPCBatchSummaryServerDispatcher _summaryDispatcher;
    private OpcBatchSummary[] _currentSummaries = Summaries;
    private int _position;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimBatchServer" /> class.
    /// </summary>
    public SimBatchServer()
        : this(NullLoggerFactory.Instance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SimBatchServer" /> class.
    /// </summary>
    /// <param name="loggerFactory">Logger factory supplied by the simulation host.</param>
    public SimBatchServer(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _batchServerDispatcher = new IOPCBatchServerServerDispatcher(this);
        _batchServer2Dispatcher = new IOPCBatchServer2ServerDispatcher(this);
        _summaryDispatcher = new IEnumOPCBatchSummaryServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    /// <summary>
    /// Gets the in-memory DCOM call channel exposed to the MCP Batch connection registry.
    /// </summary>
    public InMemoryCallChannel Channel { get; }

    /// <inheritdoc />
    public Task<string> GetDelimiterAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult("/");
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid riid, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ResetEnumeration(Summaries);
        return Task.FromResult(CreateInterfaceRef(riid));
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> CreateFilteredEnumeratorAsync(
        Guid riid,
        OpcBatchSummaryFilter filter,
        string model,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);
        cancellationToken.ThrowIfCancellationRequested();
        _ = model;

        ResetEnumeration(FilterSummaries(filter));
        return Task.FromResult(CreateInterfaceRef(riid));
    }

    /// <inheritdoc />
    public Task<OpcBatchSummary[]> NextAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (count <= 0)
        {
            return Task.FromResult(Array.Empty<OpcBatchSummary>());
        }

        lock (_gate)
        {
            int available = Math.Max(0, _currentSummaries.Length - _position);
            int take = Math.Min(count, available);
            var page = new OpcBatchSummary[take];
            Array.Copy(_currentSummaries, _position, page, 0, take);
            _position += take;
            return Task.FromResult(page);
        }
    }

    /// <inheritdoc />
    public Task SkipAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _position = Math.Min(_currentSummaries.Length, _position + Math.Max(0, count));
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _position = 0;
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateInterfaceRef(IEnumOPCBatchSummary.InterfaceId));
    }

    /// <inheritdoc />
    public Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            return Task.FromResult(_currentSummaries.Length);
        }
    }

    /// <inheritdoc />
    public Task QueryEnumerationSetsAsync(out int[] enumerationSetIds, out string[] enumerationSetNames, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReadEnumerationSets(out enumerationSetIds, out enumerationSetNames);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> QueryEnumerationAsync(int enumerationSetId, int enumerationValue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(ReadEnumerationName(enumerationSetId, enumerationValue));
    }

    /// <inheritdoc />
    public Task QueryEnumerationListAsync(
        int enumerationSetId,
        out int[] enumerationValues,
        out string[] enumerationNames,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReadEnumerationList(enumerationSetId, out enumerationValues, out enumerationNames);
        return Task.CompletedTask;
    }

    private async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (interfaceId == IOPCBatchServer.InterfaceId)
        {
            DispatchResult result = await _batchServerDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false);
            return result.ToNdrCallResult();
        }

        if (interfaceId == IOPCBatchServer2.InterfaceId)
        {
            DispatchResult result = await _batchServer2Dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false);
            return result.ToNdrCallResult();
        }

        if (interfaceId == IEnumOPCBatchSummary.InterfaceId)
        {
            DispatchResult result = await _summaryDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false);
            return result.ToNdrCallResult();
        }

        if (interfaceId == IOPCEnumerationSets.InterfaceId)
        {
            return DispatchEnumerationSets(opnum, requestPayload, cancellationToken);
        }

        return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
    }

    private static NdrCallResult DispatchEnumerationSets(int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (opnum == IOPCEnumerationSets.Opnums.QueryEnumerationSetsAsync)
        {
            ReadEnumerationSets(out int[] ids, out string[] names);
            return WriteResult((ref NdrWriter writer) =>
            {
                WriteInt32Array(ref writer, ids);
                WriteStringArray(ref writer, names);
            });
        }

        if (opnum == IOPCEnumerationSets.Opnums.QueryEnumerationAsync)
        {
            var reader = new NdrReader(requestPayload.Span);
            int setId = reader.ReadInt32();
            int value = reader.ReadInt32();
            string name = ReadEnumerationName(setId, value);
            return WriteResult((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(name));
        }

        if (opnum == IOPCEnumerationSets.Opnums.QueryEnumerationListAsync)
        {
            var reader = new NdrReader(requestPayload.Span);
            int setId = reader.ReadInt32();
            ReadEnumerationList(setId, out int[] values, out string[] names);
            return WriteResult((ref NdrWriter writer) =>
            {
                WriteInt32Array(ref writer, values);
                WriteStringArray(ref writer, names);
            });
        }

        return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
    }

    private static OpcBatchSummary[] FilterSummaries(OpcBatchSummaryFilter filter)
    {
        var matches = new List<OpcBatchSummary>(Summaries.Length);
        foreach (OpcBatchSummary summary in Summaries)
        {
            if (Matches(summary, filter))
            {
                matches.Add(summary);
            }
        }

        return matches.ToArray();
    }

    private static bool Matches(OpcBatchSummary summary, OpcBatchSummaryFilter filter) =>
        Contains(summary.Id, filter.Id)
        && Contains(summary.Description, filter.Description)
        && Contains(summary.OpcItemId, filter.OpcItemId)
        && Contains(summary.MasterRecipeId, filter.MasterRecipeId)
        && summary.BatchSize >= filter.MinBatchSize
        && summary.BatchSize <= filter.MaxBatchSize
        && Contains(summary.EngineeringUnits, filter.EngineeringUnits)
        && Contains(summary.ExecutionState, filter.ExecutionState)
        && Contains(summary.ExecutionMode, filter.ExecutionMode)
        && summary.ActualStartTime >= filter.MinStartTime
        && summary.ActualStartTime <= filter.MaxStartTime
        && summary.ActualEndTime >= filter.MinEndTime
        && summary.ActualEndTime <= filter.MaxEndTime;

    private static bool Contains(string? value, string? filter) =>
        string.IsNullOrWhiteSpace(filter)
        || (value is not null && value.Contains(filter, StringComparison.OrdinalIgnoreCase));

    private static void ReadEnumerationSets(out int[] enumerationSetIds, out string[] enumerationSetNames)
    {
        enumerationSetIds = new int[EnumerationSets.Length];
        enumerationSetNames = new string[EnumerationSets.Length];
        for (int i = 0; i < EnumerationSets.Length; i++)
        {
            enumerationSetIds[i] = EnumerationSets[i].Id;
            enumerationSetNames[i] = EnumerationSets[i].Name;
        }
    }

    private static string ReadEnumerationName(int enumerationSetId, int enumerationValue)
    {
        SimEnumerationSet? set = FindEnumerationSet(enumerationSetId);
        if (set is not null)
        {
            foreach (SimEnumerationValue value in set.Values)
            {
                if (value.Value == enumerationValue)
                {
                    return value.Name;
                }
            }
        }

        return "UNKNOWN";
    }

    private static void ReadEnumerationList(int enumerationSetId, out int[] enumerationValues, out string[] enumerationNames)
    {
        SimEnumerationSet? set = FindEnumerationSet(enumerationSetId);
        if (set is null)
        {
            enumerationValues = [];
            enumerationNames = [];
            return;
        }

        enumerationValues = new int[set.Values.Length];
        enumerationNames = new string[set.Values.Length];
        for (int i = 0; i < set.Values.Length; i++)
        {
            enumerationValues[i] = set.Values[i].Value;
            enumerationNames[i] = set.Values[i].Name;
        }
    }

    private static SimEnumerationSet? FindEnumerationSet(int id)
    {
        foreach (SimEnumerationSet set in EnumerationSets)
        {
            if (set.Id == id)
            {
                return set;
            }
        }

        return null;
    }

    private static IOpcInterfaceRef CreateInterfaceRef(Guid iid) =>
        new OpcInterfaceRef(iid, 0, 5, 1, 2, iid, 0, Array.Empty<ushort>());

    private void ResetEnumeration(OpcBatchSummary[] summaries)
    {
        lock (_gate)
        {
            _currentSummaries = summaries;
            _position = 0;
        }
    }

    private static NdrCallResult WriteResult(NdrWriteAction write) => new(0, WritePayload(write));

    private static ReadOnlyMemory<byte> WritePayload(NdrWriteAction write)
    {
        var buffer = new byte[4096];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position);
    }

    private static void WriteInt32Array(ref NdrWriter writer, IReadOnlyList<int> values)
    {
        writer.WriteUInt32((uint)values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            writer.WriteInt32(values[i]);
        }
    }

    private static void WriteStringArray(ref NdrWriter writer, IReadOnlyList<string> values)
    {
        writer.WriteUInt32((uint)values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            writer.WriteUnicodeStringPtr(values[i]);
        }
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed record SimEnumerationSet(int Id, string Name, SimEnumerationValue[] Values);

    private readonly record struct SimEnumerationValue(int Value, string Name);
}
