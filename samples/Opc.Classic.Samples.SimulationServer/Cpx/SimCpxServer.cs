// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Cpx.Dcom;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.SimulationServer.Cpx;

/// <summary>
/// In-memory OPC Complex Data server surfaced on a DA channel for the simulation server.
/// </summary>
public sealed class SimCpxServer : IOpcDaServer, IOPCComplexDataItem, IOPCComplexDataItem2, IOPCTypeLibrary
{
    private const string DictionaryId = "SampleDictionary";
    private const string DictionaryXml = """
        <?xml version="1.0" encoding="utf-8" ?>
        <TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/"
                        xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                        Name="SimulationComplexTypes"
                        DefaultBigEndian="true">
          <TypeDescription TypeID="ReactorSnapshot">
            <CharString Name="Unit" xsi:type="Ascii" Length="16" />
            <Integer Name="Sequence" xsi:type="UInt32" />
            <FloatingPoint Name="Temperature" Length="8" />
            <FloatingPoint Name="Pressure" Length="8" />
            <Integer Name="Quality" xsi:type="UInt16" />
          </TypeDescription>
          <TypeDescription TypeID="BatchTransferRecord">
            <CharString Name="Batch Id" xsi:type="Ascii" Length="12" />
            <CharString Name="Recipe" xsi:type="Ascii" Length="24" />
            <FloatingPoint Name="Net Weight" Length="8" />
            <Integer Name="Phase" xsi:type="UInt16" />
          </TypeDescription>
        </TypeDictionary>
        """;

    private static readonly ComplexItemDescription[] ItemDescriptions =
    [
        new(
            "Plant.Reactor1.Packet",
            new Guid("f1ca2a57-9f4d-4c6f-b761-a8f8fbbef101"),
            "ReactorSnapshot",
            "Types.ReactorSnapshot",
            "Plant.Reactor1.Packet.Raw",
            "Engineering",
            new[] { "Raw", "Engineering", "Compact" }),
        new(
            "Plant.Batch.TransferRecord",
            new Guid("6f8718c5-3348-4f68-8bf7-28f307f3d210"),
            "BatchTransferRecord",
            "Types.BatchTransferRecord",
            "Plant.Batch.TransferRecord.Raw",
            "Engineering",
            new[] { "Raw", "Engineering" })
    ];

    private readonly Dictionary<string, ComplexItemDescription> _itemsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ComplexItemDescription> _typesByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _dataFilters = new(StringComparer.OrdinalIgnoreCase);
    private readonly OpcDaServerDispatcher _serverDispatcher;
    private readonly IOPCComplexDataItemServerDispatcher _complexDataItemDispatcher;
    private readonly IOPCComplexDataItem2ServerDispatcher _complexDataItem2Dispatcher;
    private readonly IOPCTypeLibraryServerDispatcher _typeLibraryDispatcher;

    /// <summary>Creates a Complex Data simulation server with a null logger.</summary>
    public SimCpxServer()
        : this(NullLoggerFactory.Instance)
    {
    }

    /// <summary>Creates a Complex Data simulation server.</summary>
    /// <param name="loggerFactory">Logger factory used by the DA dispatcher.</param>
    public SimCpxServer(ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(loggerFactory);

        foreach (ComplexItemDescription description in ItemDescriptions)
        {
            _itemsById[description.ItemId] = description;
            _typesByName[description.TypeName] = description;
            _dataFilters[description.ItemId] = description.DefaultFilter;
        }

        _serverDispatcher = new OpcDaServerDispatcher(this, loggerFactory.CreateLogger<SimCpxServer>());
        _complexDataItemDispatcher = new IOPCComplexDataItemServerDispatcher(this);
        _complexDataItem2Dispatcher = new IOPCComplexDataItem2ServerDispatcher(this);
        _typeLibraryDispatcher = new IOPCTypeLibraryServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    /// <summary>Gets the in-memory DA call channel that also carries CPX interfaces.</summary>
    public InMemoryCallChannel Channel { get; }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Opc.Classic Simulation CPX Server",
            GroupCount = 0,
            BandWidth = 0,
        });
    }

    /// <inheritdoc />
    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        _ = name;
        _ = active;
        _ = requestedUpdateRate;
        _ = clientHandle;
        _ = localeId;
        _ = cancellationToken;
        throw new OpcException(OpcResultId.NotSupported);
    }

    /// <inheritdoc />
    public Task AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientGroupHandle,
        int timeBias,
        float percentDeadband,
        int localeId,
        Guid requestedInterfaceId,
        out int serverGroupHandle,
        out int revisedUpdateRate,
        out IOpcInterfaceRef group,
        CancellationToken cancellationToken = default)
    {
        _ = name;
        _ = active;
        _ = requestedUpdateRate;
        _ = clientGroupHandle;
        _ = timeBias;
        _ = percentDeadband;
        _ = localeId;
        _ = cancellationToken;
        serverGroupHandle = 0;
        revisedUpdateRate = 0;
        group = new OpcInterfaceRef(requestedInterfaceId, 0, 1, 1, 1, Guid.CreateVersion7(), 0, []);
        throw new OpcException(OpcResultId.NotSupported);
    }

    /// <inheritdoc />
    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        _ = serverGroupHandle;
        _ = force;
        _ = cancellationToken;
        throw new OpcException(OpcResultId.NotSupported);
    }

    /// <inheritdoc />
    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        _ = errorCode;
        _ = localeId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult("Simulation CPX server error");
    }

    /// <inheritdoc />
    public Task<string> GetTypeItemIDAsync(string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ComplexItemDescription description = FindItem(itemId);
        return Task.FromResult(description.TypeItemId);
    }

    /// <inheritdoc />
    public Task<string> GetUnconvertedItemIDAsync(string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ComplexItemDescription description = FindItem(itemId);
        return Task.FromResult(description.UnconvertedItemId);
    }

    /// <inheritdoc />
    public Task<string> GetDataFilterAsync(string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = FindItem(itemId);
        return Task.FromResult(_dataFilters[itemId]);
    }

    /// <inheritdoc />
    public Task SetDataFilterAsync(string itemId, string dataFilter, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ComplexItemDescription description = FindItem(itemId);
        if (!description.AvailableFilters.Contains(dataFilter, StringComparer.OrdinalIgnoreCase))
        {
            throw new OpcException(OpcResultId.InvalidArg);
        }

        _dataFilters[itemId] = dataFilter;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<Guid> GetTypeIDAsync(string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ComplexItemDescription description = FindItem(itemId);
        return Task.FromResult(description.TypeId);
    }

    /// <inheritdoc />
    public Task<string> GetDictionaryIDAsync(string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = FindItem(itemId);
        return Task.FromResult(DictionaryId);
    }

    /// <inheritdoc />
    public Task<string[]> GetAvailableFiltersAsync(string itemId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ComplexItemDescription description = FindItem(itemId);
        return Task.FromResult(description.AvailableFilters.ToArray());
    }

    /// <inheritdoc />
    public Task<string> GetDictionaryAsync(string dictionaryId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!string.Equals(dictionaryId, DictionaryId, StringComparison.OrdinalIgnoreCase))
        {
            throw new OpcException(OpcResultId.NotFound);
        }

        return Task.FromResult(DictionaryXml);
    }

    Task<string> IOPCTypeLibrary.GetTypeIDAsync(string typeName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ComplexItemDescription description = FindType(typeName);
        return Task.FromResult(description.TypeName);
    }

    Task<string> IOPCTypeLibrary.GetTypeItemIDAsync(string typeName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ComplexItemDescription description = FindType(typeName);
        return Task.FromResult(description.TypeItemId);
    }

    private async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (interfaceId == IOPCServer.InterfaceId)
        {
            return await _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken).ConfigureAwait(false);
        }

        if (interfaceId == IOPCComplexDataItem.InterfaceId)
        {
            return (await _complexDataItemDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false)).ToNdrCallResult();
        }

        if (interfaceId == IOPCComplexDataItem2.InterfaceId)
        {
            return (await _complexDataItem2Dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false)).ToNdrCallResult();
        }

        if (interfaceId == IOPCTypeLibrary.InterfaceId)
        {
            return (await _typeLibraryDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false)).ToNdrCallResult();
        }

        return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
    }

    private ComplexItemDescription FindItem(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId) || !_itemsById.TryGetValue(itemId, out ComplexItemDescription? description))
        {
            throw new OpcException(OpcResultId.UnknownItemId);
        }

        return description;
    }

    private ComplexItemDescription FindType(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName) || !_typesByName.TryGetValue(typeName, out ComplexItemDescription? description))
        {
            throw new OpcException(OpcResultId.NotFound);
        }

        return description;
    }

    private sealed class ComplexItemDescription
    {
        internal ComplexItemDescription(
            string itemId,
            Guid typeId,
            string typeName,
            string typeItemId,
            string unconvertedItemId,
            string defaultFilter,
            IReadOnlyList<string> availableFilters)
        {
            ItemId = itemId;
            TypeId = typeId;
            TypeName = typeName;
            TypeItemId = typeItemId;
            UnconvertedItemId = unconvertedItemId;
            DefaultFilter = defaultFilter;
            AvailableFilters = availableFilters;
        }

        internal string ItemId { get; }

        internal Guid TypeId { get; }

        internal string TypeName { get; }

        internal string TypeItemId { get; }

        internal string UnconvertedItemId { get; }

        internal string DefaultFilter { get; }

        internal IReadOnlyList<string> AvailableFilters { get; }
    }
}
