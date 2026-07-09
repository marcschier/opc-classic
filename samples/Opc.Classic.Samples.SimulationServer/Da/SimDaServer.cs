// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.SimulationServer.Da;

/// <summary>
/// Public in-memory OPC DA server projection for the full-feature simulation sample.
/// </summary>
public sealed class SimDaServer : IOpcDaServer, IOPCBrowse, IOPCItemMgt, IOPCSyncIO, IOPCAsyncIO2
{
    private const int BranchFlag = 1;
    private const int ItemFlag = 2;
    private const int Readable = 1;
    private const int Writable = 2;

    private static readonly int[] s_defaultPropertyIds = [1, 2, 3, 4, 5, 6, 7, 8, 100, 101, 102, 103];

    private readonly object _gate = new();
    private readonly SimulatedPlantModel _model;
    private readonly ILogger<SimDaServer> _logger;
    private readonly Dictionary<int, SimDaGroup> _groups = new();
    private readonly OpcDaServerDispatcher _serverDispatcher;
    private readonly IOPCBrowseServerDispatcher _browseDispatcher;
    private readonly IOPCItemMgtServerDispatcher _itemMgtDispatcher;
    private readonly IOPCSyncIOServerDispatcher _syncIoDispatcher;
    private readonly IOPCAsyncIO2ServerDispatcher _asyncIoDispatcher;
    private int _nextGroupHandle = 1000;
    private int _nextItemHandle = 2000;
    private int _nextCancelId = 3000;
    private int _currentGroupHandle;
    private bool _asyncEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimDaServer" /> class.
    /// </summary>
    /// <param name="model">The shared deterministic plant model used for browse, reads, and writes.</param>
    /// <param name="loggerFactory">Logger factory used for DA endpoint diagnostics.</param>
    public SimDaServer(SimulatedPlantModel model, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _model = model;
        _logger = loggerFactory.CreateLogger<SimDaServer>();
        _serverDispatcher = new OpcDaServerDispatcher(this);
        _browseDispatcher = new IOPCBrowseServerDispatcher(this);
        _itemMgtDispatcher = new IOPCItemMgtServerDispatcher(this);
        _syncIoDispatcher = new IOPCSyncIOServerDispatcher(this);
        _asyncIoDispatcher = new IOPCAsyncIO2ServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    /// <summary>
    /// Gets the call channel registered with the MCP in-memory DA connection registry.
    /// </summary>
    public InMemoryCallChannel Channel { get; }

    /// <summary>
    /// Gets the current number of server-side DA groups.
    /// </summary>
    public int GroupCount
    {
        get
        {
            lock (_gate)
            {
                return _groups.Count;
            }
        }
    }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = _model.StartTimeUtc,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = _model.ServerVersion,
            VendorInfo = _model.VendorInfo,
            GroupCount = GroupCount,
            BandWidth = 0,
        });
    }

    /// <inheritdoc />
    public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();
        int handle = Interlocked.Increment(ref _nextGroupHandle);
        lock (_gate)
        {
            _groups[handle] = new SimDaGroup(handle, name, active, requestedUpdateRate, clientHandle, localeId);
            _currentGroupHandle = handle;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Added DA group {GroupHandle} ({GroupName}).", handle, name);
        }

        return Task.FromResult(handle);
    }

    /// <inheritdoc />
    public Task AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientGroupHandle, int timeBias, float percentDeadband, int localeId, Guid requestedInterfaceId, out int serverGroupHandle, out int revisedUpdateRate, out IOpcInterfaceRef group, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();
        serverGroupHandle = Interlocked.Increment(ref _nextGroupHandle);
        revisedUpdateRate = requestedUpdateRate;
        group = CreateInterfaceRef(requestedInterfaceId, serverGroupHandle);
        lock (_gate)
        {
            _groups[serverGroupHandle] = new SimDaGroup(serverGroupHandle, name, active, requestedUpdateRate, clientGroupHandle, localeId)
            {
                TimeBias = timeBias,
                PercentDeadband = percentDeadband,
            };
            _currentGroupHandle = serverGroupHandle;
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Added DA group {GroupHandle} ({GroupName}) with IID {InterfaceId}.", serverGroupHandle, name, requestedInterfaceId);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        _ = force;
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _groups.Remove(serverGroupHandle);
            if (_currentGroupHandle == serverGroupHandle)
            {
                _currentGroupHandle = 0;
            }
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Removed DA group {GroupHandle}.", serverGroupHandle);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(string.Create(CultureInfo.InvariantCulture, $"Simulation DA error 0x{errorCode:X8} locale={localeId}"));
    }

    /// <inheritdoc />
    public Task<OpcItemProperties[]> GetPropertiesAsync(string[] itemIds, bool returnPropertyValues, int[] propertyIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(propertyIds);
        cancellationToken.ThrowIfCancellationRequested();

        int[] requested = propertyIds.Length == 0 ? s_defaultPropertyIds : propertyIds;
        return Task.FromResult(itemIds.Select(itemId => CreateProperties(itemId, requested, returnPropertyValues)).ToArray());
    }

    /// <inheritdoc />
    public Task BrowseAsync(string itemId, ref string? continuationPoint, int maxElementsReturned, int browseFilter, string elementNameFilter, string vendorFilter, bool returnAllProperties, bool returnPropertyValues, int[] propertyIds, out bool moreElements, out OpcBrowseElementResult[] browseElements, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemId);
        ArgumentNullException.ThrowIfNull(elementNameFilter);
        ArgumentNullException.ThrowIfNull(propertyIds);
        _ = vendorFilter;
        cancellationToken.ThrowIfCancellationRequested();

        string branchPath = NormalizeBranchPath(itemId);
        var elements = new List<OpcBrowseElementResult>();
        var filter = (BrowseFilters)browseFilter;
        bool includeBranches = filter is BrowseFilters.All or BrowseFilters.Branch;
        bool includeLeaves = filter is BrowseFilters.All or BrowseFilters.Leaf;
        int[] requestedProperties = propertyIds.Length == 0 && returnAllProperties ? s_defaultPropertyIds : propertyIds;

        if (includeBranches)
        {
            foreach (string branch in _model.BrowseBranches(branchPath))
            {
                string childPath = CombinePath(branchPath, branch);
                if (MatchesFilter(branch, elementNameFilter))
                {
                    elements.Add(new OpcBrowseElementResult(
                        branch,
                        childPath,
                        BranchFlag,
                        new OpcItemProperties(0, [])));
                }
            }
        }

        if (includeLeaves)
        {
            foreach (SimulatedTag tag in _model.BrowseLeaves(branchPath))
            {
                if (MatchesFilter(tag.Name, elementNameFilter))
                {
                    elements.Add(new OpcBrowseElementResult(
                        tag.Name,
                        tag.ItemId,
                        ItemFlag,
                        requestedProperties.Length == 0 ? new OpcItemProperties(0, []) : CreateProperties(tag.ItemId, requestedProperties, returnPropertyValues)));
                }
            }
        }

        elements.Sort(static (left, right) => string.Compare(left.ItemId, right.ItemId, StringComparison.OrdinalIgnoreCase));

        int offset = ParseContinuation(continuationPoint);
        if (offset >= elements.Count)
        {
            continuationPoint = null;
            moreElements = false;
            browseElements = [];
            return Task.CompletedTask;
        }

        int take = maxElementsReturned <= 0 ? elements.Count - offset : Math.Min(maxElementsReturned, elements.Count - offset);
        browseElements = elements.GetRange(offset, take).ToArray();
        int nextOffset = offset + take;
        moreElements = nextOffset < elements.Count;
        continuationPoint = moreElements ? nextOffset.ToString(CultureInfo.InvariantCulture) : null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task AddItemsAsync(OpcItemDef[] itemDefinitions, out OpcItemResult[] addResults, out int[] errors, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemDefinitions);
        cancellationToken.ThrowIfCancellationRequested();

        addResults = new OpcItemResult[itemDefinitions.Length];
        errors = new int[itemDefinitions.Length];
        lock (_gate)
        {
            SimDaGroup? group = null;
            bool hasGroup = _currentGroupHandle != 0 && _groups.TryGetValue(_currentGroupHandle, out group);
            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                string itemId = itemDefinitions[i].ItemId ?? string.Empty;
                if (!hasGroup || group is null)
                {
                    addResults[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
                    errors[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                if (!_model.TryGetTag(itemId, out SimulatedTag tag))
                {
                    addResults[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
                    errors[i] = OpcResultId.UnknownItemId.Code;
                    continue;
                }

                int serverHandle = Interlocked.Increment(ref _nextItemHandle);
                group.Items[serverHandle] = new SimDaItem(serverHandle, itemId, itemDefinitions[i].ClientHandle);
                addResults[i] = new OpcItemResult(serverHandle, ToVarType(tag.DataType), AccessRights(tag), []);
                errors[i] = OpcResultId.Ok.Code;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ValidateItemsAsync(OpcItemDef[] itemDefinitions, bool blobUpdate, out OpcItemResult[] validationResults, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = blobUpdate;
        ArgumentNullException.ThrowIfNull(itemDefinitions);
        cancellationToken.ThrowIfCancellationRequested();

        validationResults = new OpcItemResult[itemDefinitions.Length];
        errors = new int[itemDefinitions.Length];
        for (int i = 0; i < itemDefinitions.Length; i++)
        {
            string itemId = itemDefinitions[i].ItemId ?? string.Empty;
            if (_model.TryGetTag(itemId, out SimulatedTag tag))
            {
                validationResults[i] = new OpcItemResult(0, ToVarType(tag.DataType), AccessRights(tag), []);
                errors[i] = OpcResultId.Ok.Code;
            }
            else
            {
                validationResults[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
                errors[i] = OpcResultId.UnknownItemId.Code;
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<int[]> RemoveItemsAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        int[] errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                errors[i] = RemoveItem(serverHandles[i]) ? OpcResultId.Ok.Code : OpcResultId.InvalidHandle.Code;
            }
        }

        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int[]> SetActiveStateAsync(int[] serverHandles, bool active, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        int[] errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (TryFindItem(serverHandles[i], out SimDaItem? item) && item is not null)
                {
                    item.Active = active;
                    errors[i] = OpcResultId.Ok.Code;
                }
                else
                {
                    errors[i] = OpcResultId.InvalidHandle.Code;
                }
            }
        }

        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int[]> SetClientHandlesAsync(int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(clientHandles);
        cancellationToken.ThrowIfCancellationRequested();

        int[] errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (TryFindItem(serverHandles[i], out SimDaItem? item) && item is not null && i < clientHandles.Length)
                {
                    item.ClientHandle = clientHandles[i];
                    errors[i] = OpcResultId.Ok.Code;
                }
                else
                {
                    errors[i] = OpcResultId.InvalidHandle.Code;
                }
            }
        }

        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int[]> SetDatatypesAsync(int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(requestedDataTypes);
        cancellationToken.ThrowIfCancellationRequested();

        int[] errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                errors[i] = TryFindItem(serverHandles[i], out _) ? OpcResultId.Ok.Code : OpcResultId.InvalidHandle.Code;
            }
        }

        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateInterfaceRef(requestedInterfaceId, _currentGroupHandle));
    }

    /// <inheritdoc />
    public Task<OpcItemState[]> ReadAsync(int dataSource, int[] serverHandles, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = dataSource;
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        OpcItemState[] states = new OpcItemState[serverHandles.Length];
        errors = new int[serverHandles.Length];
        DateTimeOffset now = DateTimeOffset.UtcNow;
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (TryFindItem(serverHandles[i], out SimDaItem? item) && item is not null && _model.TryGetTag(item.ItemId, out SimulatedTag tag))
                {
                    states[i] = new OpcItemState(item.ClientHandle, now, OpcQuality.Good, ToVariant(_model.ValueAt(tag, now)));
                    errors[i] = OpcResultId.Ok.Code;
                }
                else
                {
                    states[i] = new OpcItemState(0, now, OpcQuality.Bad, OpcVariant.Empty);
                    errors[i] = OpcResultId.InvalidHandle.Code;
                }
            }
        }

        return Task.FromResult(states);
    }

    /// <inheritdoc />
    public Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);
        cancellationToken.ThrowIfCancellationRequested();

        int[] errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (!TryFindItem(serverHandles[i], out SimDaItem? item) || item is null || i >= values.Length)
                {
                    errors[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                if (!_model.TryGetTag(item.ItemId, out SimulatedTag tag))
                {
                    errors[i] = OpcResultId.UnknownItemId.Code;
                    continue;
                }

                if (!tag.Writable)
                {
                    errors[i] = OpcResultId.BadRights.Code;
                    continue;
                }

                errors[i] = _model.TryWrite(item.ItemId, OpcVariantConverter.ToObject(values[i]) ?? string.Empty)
                    ? OpcResultId.Ok.Code
                    : OpcResultId.BadRights.Code;
            }
        }

        return Task.FromResult(errors);
    }

    /// <inheritdoc />
    public Task<int> ReadAsync(int[] serverHandles, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = ReadAsync(1, serverHandles, out errors, cancellationToken);
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    /// <inheritdoc />
    public Task<int> WriteAsync(int[] serverHandles, OpcVariant[] values, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        errors = WriteAsync(serverHandles, values, cancellationToken).GetAwaiter().GetResult();
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    /// <inheritdoc />
    public Task<int> Refresh2Async(int dataSource, int transactionId, CancellationToken cancellationToken = default)
    {
        _ = dataSource;
        _ = transactionId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    /// <inheritdoc />
    public Task Cancel2Async(int cancelId, CancellationToken cancellationToken = default)
    {
        _ = cancelId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SetEnableAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _asyncEnabled = enabled;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> GetEnableAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_asyncEnabled);
    }

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        if (interfaceId == IOPCServer.InterfaceId)
        {
            return _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCBrowse.InterfaceId)
        {
            return ToCallResultAsync(_browseDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCItemMgt.InterfaceId)
        {
            return ToCallResultAsync(_itemMgtDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCSyncIO.InterfaceId)
        {
            return ToCallResultAsync(_syncIoDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCAsyncIO2.InterfaceId)
        {
            return ToCallResultAsync(_asyncIoDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
    }

    private OpcItemProperties CreateProperties(string itemId, int[] propertyIds, bool returnPropertyValues)
    {
        if (!_model.TryGetTag(itemId, out SimulatedTag tag))
        {
            return new OpcItemProperties(OpcResultId.UnknownItemId.Code, []);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;
        object currentValue = _model.ValueAt(tag, now);
        return new OpcItemProperties(0, propertyIds.Select(id => CreateProperty(tag, id, now, currentValue, returnPropertyValues)).ToArray());
    }

    private static OpcItemPropertyResult CreateProperty(SimulatedTag tag, int propertyId, DateTimeOffset now, object currentValue, bool returnPropertyValue)
    {
        VarType canonicalType = ToVarType(tag.DataType);
        OpcVariant value = returnPropertyValue ? ToVariant(currentValue) : OpcVariant.Empty;
        return propertyId switch
        {
            1 => Property(propertyId, VarType.VT_I2, "Item Canonical DataType", returnPropertyValue ? OpcVariant.FromInt16((short)canonicalType) : OpcVariant.Empty),
            2 => Property(propertyId, canonicalType, "Item Value", value),
            3 => Property(propertyId, VarType.VT_I2, "Item Quality", returnPropertyValue ? OpcVariant.FromInt16((short)OpcQuality.Good.RawValue) : OpcVariant.Empty),
            4 => Property(propertyId, VarType.VT_DATE, "Item Timestamp", returnPropertyValue ? OpcVariant.FromDate(now.UtcDateTime) : OpcVariant.Empty),
            5 => Property(propertyId, VarType.VT_I4, "Item Access Rights", returnPropertyValue ? OpcVariant.FromInt32(AccessRights(tag)) : OpcVariant.Empty),
            6 => Property(propertyId, VarType.VT_R4, "Server Scan Rate", returnPropertyValue ? OpcVariant.FromSingle(1000.0f) : OpcVariant.Empty),
            7 => Property(propertyId, VarType.VT_I2, "Item EU Type", returnPropertyValue ? OpcVariant.FromInt16((short)(IsAnalog(tag) ? 1 : 0)) : OpcVariant.Empty),
            8 => Property(propertyId, VarType.VT_EMPTY, "Item EU Info", OpcVariant.Empty),
            100 => Property(propertyId, VarType.VT_BSTR, "Item EU Units", returnPropertyValue ? OpcVariant.FromString(tag.Units ?? string.Empty) : OpcVariant.Empty),
            101 => Property(propertyId, VarType.VT_BSTR, "Item Description", returnPropertyValue ? OpcVariant.FromString(tag.ItemId) : OpcVariant.Empty),
            102 => Property(propertyId, VarType.VT_R8, "High EU", returnPropertyValue ? OpcVariant.FromDouble(tag.Maximum) : OpcVariant.Empty),
            103 => Property(propertyId, VarType.VT_R8, "Low EU", returnPropertyValue ? OpcVariant.FromDouble(tag.Minimum) : OpcVariant.Empty),
            _ => new OpcItemPropertyResult(VarType.VT_EMPTY, propertyId, null, "Unknown property", OpcVariant.Empty, OpcResultId.InvalidPid.Code),
        };
    }

    private static OpcItemPropertyResult Property(int propertyId, VarType dataType, string description, OpcVariant value) =>
        new(dataType, propertyId, ItemId: null, description, value, OpcResultId.Ok.Code);

    private static VarType ToVarType(SimulatedDataType dataType) => dataType switch
    {
        SimulatedDataType.Boolean => VarType.VT_BOOL,
        SimulatedDataType.Int16 => VarType.VT_I2,
        SimulatedDataType.Int32 => VarType.VT_I4,
        SimulatedDataType.Single => VarType.VT_R4,
        SimulatedDataType.Double => VarType.VT_R8,
        SimulatedDataType.String => VarType.VT_BSTR,
        _ => VarType.VT_EMPTY,
    };

    private static OpcVariant ToVariant(object value) => OpcVariantConverter.FromObject(value);

    private static int AccessRights(SimulatedTag tag) => tag.Writable ? Readable | Writable : Readable;

    private static bool IsAnalog(SimulatedTag tag) => tag.DataType is SimulatedDataType.Int16 or SimulatedDataType.Int32 or SimulatedDataType.Single or SimulatedDataType.Double;

    private bool TryFindItem(int serverHandle, out SimDaItem? item)
    {
        foreach (SimDaGroup group in _groups.Values)
        {
            if (group.Items.TryGetValue(serverHandle, out item))
            {
                return true;
            }
        }

        item = null;
        return false;
    }

    private bool RemoveItem(int serverHandle)
    {
        foreach (SimDaGroup group in _groups.Values)
        {
            if (group.Items.Remove(serverHandle))
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeBranchPath(string itemId) => itemId ?? string.Empty;

    private static string CombinePath(string branchPath, string childName) =>
        branchPath.Length == 0 ? childName : branchPath + "." + childName;

    private static int ParseContinuation(string? continuationPoint) =>
        int.TryParse(continuationPoint, NumberStyles.Integer, CultureInfo.InvariantCulture, out int offset) && offset > 0 ? offset : 0;

    private static bool MatchesFilter(string name, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter == "*")
        {
            return true;
        }

        return MatchWildcard(name, filter, nameIndex: 0, filterIndex: 0);
    }

    private static bool MatchWildcard(string value, string pattern, int nameIndex, int filterIndex)
    {
        while (filterIndex < pattern.Length)
        {
            char p = pattern[filterIndex];
            if (p == '*')
            {
                while (filterIndex + 1 < pattern.Length && pattern[filterIndex + 1] == '*')
                {
                    filterIndex++;
                }

                if (filterIndex + 1 == pattern.Length)
                {
                    return true;
                }

                for (int i = nameIndex; i <= value.Length; i++)
                {
                    if (MatchWildcard(value, pattern, i, filterIndex + 1))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (nameIndex >= value.Length)
            {
                return false;
            }

            if (p != '?' && char.ToUpperInvariant(p) != char.ToUpperInvariant(value[nameIndex]))
            {
                return false;
            }

            nameIndex++;
            filterIndex++;
        }

        return nameIndex == value.Length;
    }

    private static async Task<NdrCallResult> ToCallResultAsync(ValueTask<DispatchResult> dispatch) =>
        (await dispatch.ConfigureAwait(false)).ToNdrCallResult();

    private static IOpcInterfaceRef CreateInterfaceRef(Guid iid, int discriminator) =>
        new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: 0x1000,
            oid: unchecked((ulong)discriminator),
            ipid: Guid.Empty,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());

    private sealed class SimDaGroup
    {
        public SimDaGroup(int serverHandle, string name, bool active, int updateRate, int clientHandle, int localeId)
        {
            ServerHandle = serverHandle;
            Name = name;
            Active = active;
            UpdateRate = updateRate;
            ClientHandle = clientHandle;
            LocaleId = localeId;
        }

        public int ServerHandle { get; }
        public string Name { get; }
        public bool Active { get; }
        public int UpdateRate { get; }
        public int ClientHandle { get; }
        public int LocaleId { get; }
        public int TimeBias { get; init; }
        public float PercentDeadband { get; init; }
        public Dictionary<int, SimDaItem> Items { get; } = new();
    }

    private sealed class SimDaItem
    {
        public SimDaItem(int serverHandle, string itemId, int clientHandle)
        {
            ServerHandle = serverHandle;
            ItemId = itemId;
            ClientHandle = clientHandle;
            Active = true;
        }

        public int ServerHandle { get; }
        public string ItemId { get; }
        public int ClientHandle { get; set; }
        public bool Active { get; set; }
    }
}
