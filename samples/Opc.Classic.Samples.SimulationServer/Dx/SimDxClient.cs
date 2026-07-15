// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using Opc.Classic.Dx;
using Opc.Classic.Dx.Dcom;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.SimulationServer.Dx;

/// <summary>
/// Engine-backed DX reference server exposed through the simulation MCP and DCOM surfaces.
/// </summary>
public sealed class SimDxClient : IOpcDxClient, IOPCConfiguration
{
    private const string RootPath = "";
    private readonly SemaphoreSlim _mutationGate = new(1, 1);
    private readonly DateTimeOffset _startTime;
    private readonly IDisposable? _ownedStore;
    private bool _shutdown;

    private SimDxClient(
        SimulatedPlantModel model,
        IDxConfigurationStore store,
        IDxScheduler scheduler,
        DxReferenceEngineOptions? options,
        IDisposable? ownedStore)
    {
        ArgumentNullException.ThrowIfNull(model);
        _startTime = model.StartTimeUtc;
        _ownedStore = ownedStore;
        SourceEndpoint = new SimDxDaEndpoint("simulation-da-source", model, scheduler.Clock);
        TargetEndpoint = new SimDxDaEndpoint("simulation-da-target", model, scheduler.Clock);
        EndpointResolver = new SimDxEndpointResolver(SourceEndpoint, TargetEndpoint);
        Engine = new DxReferenceEngine(store, EndpointResolver, scheduler, options);
        Channel = new InMemoryCallChannel(new SimDxDcomDispatcher(this).DispatchAsync);
    }

    /// <summary>The bounded reference transfer engine used by all exposed surfaces.</summary>
    public DxReferenceEngine Engine { get; }

    /// <summary>The deterministic source endpoint used by the engine.</summary>
    public SimDxDaEndpoint SourceEndpoint { get; }

    /// <summary>The deterministic target endpoint used by the engine.</summary>
    public SimDxDaEndpoint TargetEndpoint { get; }

    /// <summary>The endpoint resolver used by the engine.</summary>
    public SimDxEndpointResolver EndpointResolver { get; }

    /// <summary>In-process NDR channel for the OPC DX <c>IOPCConfiguration</c> surface.</summary>
    public InMemoryCallChannel Channel { get; }

    /// <summary>
    /// Creates, seeds when necessary, and starts a deterministic DX reference server.
    /// </summary>
    public static async Task<SimDxClient> CreateAsync(
        SimulatedPlantModel model,
        IDxConfigurationStore? store = null,
        IDxScheduler? scheduler = null,
        DxReferenceEngineOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model);
        scheduler ??= SystemDxScheduler.Instance;

        IDisposable? ownedStore = null;
        if (store is null)
        {
            var memoryStore = new InMemoryDxConfigurationStore(CreateSeedConfiguration());
            store = memoryStore;
            ownedStore = memoryStore;
        }
        else
        {
            DxConfigurationSnapshot persisted =
                await store.LoadAsync(cancellationToken).ConfigureAwait(false);
            if (persisted.Version == 0 &&
                persisted.Configuration.SourceServers.Length == 0 &&
                persisted.Configuration.Connections.Length == 0)
            {
                _ = await store.SaveAsync(
                    CreateSeedConfiguration(),
                    expectedVersion: 0,
                    cancellationToken).ConfigureAwait(false);
            }
        }

        var client = new SimDxClient(model, store, scheduler, options, ownedStore);
        try
        {
            await client.Engine.StartAsync(cancellationToken).ConfigureAwait(false);
            return client;
        }
        catch
        {
            await client.ShutdownAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>Creates the deterministic source-server and connection configuration.</summary>
    public static DxConfiguration CreateSeedConfiguration() =>
        new(
            sourceServers:
            [
                new DxSourceServer(
                    name: "SimulationDA",
                    serverUrl: "opcda://simulation/Opc.Classic.Simulation.DA.1",
                    description: "Deterministic managed DA source endpoint",
                    serverType: "OPC DA 3.0",
                    itemPath: "Simulation",
                    itemName: "SimulationDA",
                    version: "sim-dx-1",
                    defaultConnected: true),
            ],
            connections:
            [
                new DxConnection(
                    name: "ReactorTemperatureToBucket",
                    description: "Mirrors reactor temperature to a writable DA bucket item.",
                    itemPath: "Plant.Reactor1",
                    itemName: "TemperatureMirror",
                    version: "sim-dx-1",
                    browsePaths: ["Plant", "Plant.Reactor1"],
                    keyword: "temperature",
                    defaultSourceItemConnected: true,
                    defaultTargetItemConnected: true,
                    sourceServerName: "SimulationDA",
                    sourceItemPath: "Plant.Reactor1",
                    sourceItemName: "Temperature",
                    sourceItemQueueSize: 4,
                    targetItemPath: "Bucket Brigade",
                    targetItemName: "Real8",
                    updateRateMilliseconds: 1000,
                    deadbandPercent: 0.5f,
                    vendorData: "sim:reactor-temperature"),
                new DxConnection(
                    name: "ReactorPressureDisabled",
                    description: "Disabled-by-default pressure transfer used to demonstrate enable/disable.",
                    itemPath: "Plant.Reactor1",
                    itemName: "PressureMirror",
                    version: "sim-dx-1",
                    browsePaths: ["Plant", "Plant.Reactor1"],
                    keyword: "pressure",
                    defaultSourceItemConnected: false,
                    defaultTargetItemConnected: false,
                    sourceServerName: "SimulationDA",
                    sourceItemPath: "Plant.Reactor1",
                    sourceItemName: "Pressure",
                    sourceItemQueueSize: 2,
                    targetItemPath: "Bucket Brigade",
                    targetItemName: "Int4",
                    updateRateMilliseconds: 500,
                    deadbandPercent: 1.0f,
                    vendorData: "sim:reactor-pressure"),
            ]);

    /// <summary>Stops the transfer engine and releases owned resources.</summary>
    public async ValueTask ShutdownAsync()
    {
        if (_shutdown)
        {
            return;
        }

        _shutdown = true;
        await Engine.DisposeAsync().ConfigureAwait(false);
        _mutationGate.Dispose();
        _ownedStore?.Dispose();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <inheritdoc />
    public async Task<OpcServerStatus> GetStatusAsync(
        CancellationToken cancellationToken = default)
    {
        DxConfigurationSnapshot configuration =
            await Engine.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        DxReferenceEngineSnapshot status = Engine.GetStatusSnapshot();
        return new OpcServerStatus
        {
            Spec = OpcStatusSpec.Dx,
            StartTime = _startTime,
            CurrentTime = status.Timestamp,
            LastUpdateTime = status.Timestamp,
            State = status.IsRunning ? OpcServerState.Running : OpcServerState.Suspended,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Opc.Classic Simulation DX Reference Engine",
            GroupCount = configuration.Configuration.Connections.Length,
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> QueryConnectionNamesAsync(
        string browsePath,
        IReadOnlyList<string> connectionMasks,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionMasks);
        DxConnection[] connections =
            await QueryConnectionsCoreAsync(
                browsePath,
                connectionMasks.Select(static name => new DxConnection(name: name)).ToArray(),
                recursive,
                cancellationToken).ConfigureAwait(false);
        return connections
            .Select(static connection => connection.Name ?? string.Empty)
            .Where(static name => name.Length > 0)
            .ToArray();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<DxSourceServer>> QuerySourceServersAsync(
        CancellationToken cancellationToken = default) =>
        await QuerySourceServersCoreAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<OpcResultId> AddConnectionAsync(
        DxConnection connection,
        CancellationToken cancellationToken = default) =>
        await UpsertConnectionCoreAsync(connection, requireExisting: false, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<OpcResultId> ModifyConnectionAsync(
        DxConnection connection,
        CancellationToken cancellationToken = default) =>
        await UpsertConnectionCoreAsync(connection, requireExisting: true, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<OpcResultId> UpdateConnectionAsync(
        string browsePath,
        string connectionName,
        bool recursive,
        DxConnection connectionDefinition,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        ArgumentNullException.ThrowIfNull(connectionDefinition);
        DxConnection[] matches = await QueryConnectionsCoreAsync(
            browsePath,
            [new DxConnection(name: connectionName)],
            recursive,
            cancellationToken).ConfigureAwait(false);
        if (matches.Length == 0)
        {
            return OpcResultId.UnknownItemId;
        }

        foreach (DxConnection match in matches)
        {
            OpcResultId result = await UpsertConnectionCoreAsync(
                connectionDefinition with { Name = match.Name },
                requireExisting: true,
                cancellationToken).ConfigureAwait(false);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return OpcResultId.Ok;
    }

    /// <inheritdoc />
    public async Task<OpcResultId> DeleteConnectionAsync(
        string browsePath,
        string connectionName,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        DxConnection[] matches = await QueryConnectionsCoreAsync(
            browsePath,
            [new DxConnection(name: connectionName)],
            recursive,
            cancellationToken).ConfigureAwait(false);
        foreach (DxConnection match in matches)
        {
            await MutateAsync(
                (revision, token) => Engine.RemoveConnectionAsync(
                    match.Name!,
                    revision,
                    token),
                cancellationToken).ConfigureAwait(false);
        }

        return OpcResultId.Ok;
    }

    /// <inheritdoc />
    public async Task<OpcResultId> AddSourceServerAsync(
        DxSourceServer sourceServer,
        CancellationToken cancellationToken = default) =>
        await UpsertSourceServerCoreAsync(sourceServer, requireExisting: false, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<OpcResultId> ModifySourceServerAsync(
        DxSourceServer sourceServer,
        CancellationToken cancellationToken = default) =>
        await UpsertSourceServerCoreAsync(sourceServer, requireExisting: true, cancellationToken)
            .ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<string> ResetConfigurationAsync(
        string configurationVersion,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configurationVersion);
        _ = await MutateAsync(
            (revision, token) => Engine.ResetAsync(revision, token),
            cancellationToken).ConfigureAwait(false);
        return configurationVersion + ":reset";
    }

    /// <inheritdoc />
    async Task<DxSourceServer[]> IOPCConfiguration.QuerySourceServersAsync(
        CancellationToken cancellationToken) =>
        await QuerySourceServersCoreAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public Task<DxGeneralResponse> AddSourceServersAsync(
        DxSourceServer[] sourceServers,
        CancellationToken cancellationToken = default) =>
        MutateSourceServersAsync(sourceServers, requireExisting: false, cancellationToken);

    /// <inheritdoc />
    public Task<DxGeneralResponse> ModifySourceServersAsync(
        DxSourceServer[] sourceServers,
        CancellationToken cancellationToken = default) =>
        MutateSourceServersAsync(sourceServers, requireExisting: true, cancellationToken);

    /// <inheritdoc />
    public async Task<DxGeneralResponse> DeleteSourceServersAsync(
        DxItemIdentifier[] sourceServers,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceServers);
        var results = new List<DxIdentifiedResult>(sourceServers.Length);
        foreach (DxItemIdentifier identifier in sourceServers)
        {
            string name = identifier.ItemName ?? string.Empty;
            OpcResultId result = await RemoveSourceServerCoreAsync(name, cancellationToken)
                .ConfigureAwait(false);
            results.Add(ToIdentifiedResult(identifier.ItemPath, name, result));
        }

        return await CreateResponseAsync(results, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DxGeneralResponse> CopyDefaultServerAttributesAsync(
        bool configToStatus,
        DxItemIdentifier[] sourceServers,
        CancellationToken cancellationToken = default)
    {
        _ = configToStatus;
        ArgumentNullException.ThrowIfNull(sourceServers);
        return await CreateResponseAsync(
            sourceServers.Select(identifier =>
                ToIdentifiedResult(identifier.ItemPath, identifier.ItemName, OpcResultId.Ok)),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<DxConnectionQueryResult> QueryDXConnectionsAsync(
        string browsePath,
        DxConnection[] connectionMasks,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionMasks);
        DxConnection[] connections = await QueryConnectionsCoreAsync(
            browsePath,
            connectionMasks,
            recursive,
            cancellationToken).ConfigureAwait(false);
        return new DxConnectionQueryResult(new int[connectionMasks.Length], connections);
    }

    /// <inheritdoc />
    public async Task<string[]> QueryDXConnectionNamesAsync(
        string browsePath,
        string[] connectionMasks,
        bool recursive,
        CancellationToken cancellationToken = default) =>
        (await QueryConnectionNamesAsync(
            browsePath,
            connectionMasks,
            recursive,
            cancellationToken).ConfigureAwait(false)).ToArray();

    /// <inheritdoc />
    public Task<DxGeneralResponse> AddDXConnectionsAsync(
        DxConnection[] connections,
        CancellationToken cancellationToken = default) =>
        MutateConnectionsAsync(connections, requireExisting: false, cancellationToken);

    /// <inheritdoc />
    public async Task<DxUpdateConnectionsResult> UpdateDXConnectionsAsync(
        string browsePath,
        DxConnection[] connectionMasks,
        bool recursive,
        DxConnection connectionDefinition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionMasks);
        var errors = new int[connectionMasks.Length];
        for (var i = 0; i < connectionMasks.Length; i++)
        {
            string name = connectionMasks[i].Name ?? "*";
            errors[i] = (await UpdateConnectionAsync(
                browsePath,
                name,
                recursive,
                connectionDefinition,
                cancellationToken).ConfigureAwait(false)).Code;
        }

        return new DxUpdateConnectionsResult(
            errors,
            await CreateResponseAsync([], cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public Task<DxGeneralResponse> ModifyDXConnectionsAsync(
        DxConnection[] connections,
        CancellationToken cancellationToken = default) =>
        MutateConnectionsAsync(connections, requireExisting: true, cancellationToken);

    /// <inheritdoc />
    public async Task<DxDeleteConnectionsResult> DeleteDXConnectionsAsync(
        string browsePath,
        DxConnection[] connectionMasks,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectionMasks);
        var errors = new int[connectionMasks.Length];
        var results = new List<DxIdentifiedResult>();
        for (var i = 0; i < connectionMasks.Length; i++)
        {
            string name = connectionMasks[i].Name ?? "*";
            DxConnection[] matches = await QueryConnectionsCoreAsync(
                browsePath,
                [connectionMasks[i]],
                recursive,
                cancellationToken).ConfigureAwait(false);
            errors[i] = OpcResultId.Ok.Code;
            foreach (DxConnection match in matches)
            {
                OpcResultId result = await DeleteConnectionAsync(
                    browsePath,
                    match.Name ?? name,
                    recursive,
                    cancellationToken).ConfigureAwait(false);
                results.Add(ToIdentifiedResult(match.ItemPath, match.Name, result));
            }
        }

        return new DxDeleteConnectionsResult(
            errors,
            await CreateResponseAsync(results, cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<DxUpdateConnectionsResult> CopyDefaultDXConnectionAttributesAsync(
        bool configToStatus,
        string browsePath,
        DxConnection[] connectionMasks,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        _ = configToStatus;
        ArgumentNullException.ThrowIfNull(connectionMasks);
        _ = await QueryConnectionsCoreAsync(
            browsePath,
            connectionMasks,
            recursive,
            cancellationToken).ConfigureAwait(false);
        return new DxUpdateConnectionsResult(
            new int[connectionMasks.Length],
            await CreateResponseAsync([], cancellationToken).ConfigureAwait(false));
    }

    private async Task<DxSourceServer[]> QuerySourceServersCoreAsync(
        CancellationToken cancellationToken)
    {
        DxConfigurationSnapshot snapshot =
            await Engine.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        return snapshot.Configuration.SourceServers
            .OrderBy(static source => source.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private async Task<DxConnection[]> QueryConnectionsCoreAsync(
        string browsePath,
        IReadOnlyList<DxConnection> masks,
        bool recursive,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(browsePath);
        DxConfigurationSnapshot snapshot =
            await Engine.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        IEnumerable<DxConnection> connections = snapshot.Configuration.Connections
            .Where(connection => IsInBrowsePath(connection, browsePath, recursive));
        if (masks.Count > 0)
        {
            connections = connections.Where(connection =>
                masks.Any(mask => MatchesConnection(connection, mask)));
        }

        return connections
            .OrderBy(static connection => connection.Name, StringComparer.Ordinal)
            .ToArray();
    }

    private async Task<OpcResultId> UpsertConnectionCoreAsync(
        DxConnection connection,
        bool requireExisting,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connection);
        string name = connection.Name ?? string.Empty;
        if (name.Length == 0)
        {
            return OpcResultId.InvalidArg;
        }

        DxConfigurationSnapshot current =
            await Engine.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        bool exists = current.Configuration.Connections.Any(candidate =>
            string.Equals(candidate.Name, name, StringComparison.Ordinal));
        if (requireExisting && !exists)
        {
            return OpcResultId.UnknownItemId;
        }

        try
        {
            _ = await MutateAsync(
                (revision, token) => Engine.UpsertConnectionAsync(
                    NormalizeConnection(connection),
                    revision,
                    token),
                cancellationToken).ConfigureAwait(false);
            return OpcResultId.Ok;
        }
        catch (DxConfigurationValidationException)
        {
            return OpcResultId.InvalidArg;
        }
    }

    private async Task<OpcResultId> UpsertSourceServerCoreAsync(
        DxSourceServer sourceServer,
        bool requireExisting,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceServer);
        string name = sourceServer.Name ?? string.Empty;
        if (name.Length == 0)
        {
            return OpcResultId.InvalidArg;
        }

        DxConfigurationSnapshot current =
            await Engine.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        bool exists = current.Configuration.SourceServers.Any(candidate =>
            string.Equals(candidate.Name, name, StringComparison.Ordinal));
        if (requireExisting && !exists)
        {
            return OpcResultId.UnknownItemId;
        }

        try
        {
            _ = await MutateAsync(
                (revision, token) => Engine.UpsertSourceServerAsync(
                    sourceServer,
                    revision,
                    token),
                cancellationToken).ConfigureAwait(false);
            return OpcResultId.Ok;
        }
        catch (DxConfigurationValidationException)
        {
            return OpcResultId.InvalidArg;
        }
    }

    private async Task<OpcResultId> RemoveSourceServerCoreAsync(
        string name,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OpcResultId.InvalidArg;
        }

        try
        {
            _ = await MutateAsync(
                (revision, token) => Engine.RemoveSourceServerAsync(name, revision, token),
                cancellationToken).ConfigureAwait(false);
            return OpcResultId.Ok;
        }
        catch (DxConfigurationValidationException)
        {
            return OpcResultId.Fail;
        }
    }

    private async Task<DxGeneralResponse> MutateSourceServersAsync(
        DxSourceServer[] sourceServers,
        bool requireExisting,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceServers);
        var results = new List<DxIdentifiedResult>(sourceServers.Length);
        foreach (DxSourceServer source in sourceServers)
        {
            OpcResultId result = await UpsertSourceServerCoreAsync(
                source,
                requireExisting,
                cancellationToken).ConfigureAwait(false);
            results.Add(ToIdentifiedResult(source.ItemPath, source.Name, result));
        }

        return await CreateResponseAsync(results, cancellationToken).ConfigureAwait(false);
    }

    private async Task<DxGeneralResponse> MutateConnectionsAsync(
        DxConnection[] connections,
        bool requireExisting,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(connections);
        var results = new List<DxIdentifiedResult>(connections.Length);
        foreach (DxConnection connection in connections)
        {
            OpcResultId result = await UpsertConnectionCoreAsync(
                connection,
                requireExisting,
                cancellationToken).ConfigureAwait(false);
            results.Add(ToIdentifiedResult(connection.ItemPath, connection.Name, result));
        }

        return await CreateResponseAsync(results, cancellationToken).ConfigureAwait(false);
    }

    private async Task<DxConfigurationSnapshot> MutateAsync(
        Func<long, CancellationToken, ValueTask<DxConfigurationSnapshot>> mutation,
        CancellationToken cancellationToken)
    {
        await _mutationGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            DxConfigurationSnapshot current =
                await Engine.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            return await mutation(current.Version, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _mutationGate.Release();
        }
    }

    private async Task<DxGeneralResponse> CreateResponseAsync(
        IEnumerable<DxIdentifiedResult> results,
        CancellationToken cancellationToken)
    {
        DxConfigurationSnapshot snapshot =
            await Engine.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        return new DxGeneralResponse(
            snapshot.Version.ToString(CultureInfo.InvariantCulture),
            results.ToArray());
    }

    private static DxIdentifiedResult ToIdentifiedResult(
        string? itemPath,
        string? itemName,
        OpcResultId result) =>
        new(itemPath, itemName, null, result, result.IsSuccess ? null : result.ToString());

    private static DxConnection NormalizeConnection(DxConnection connection)
    {
        if (connection.BrowsePaths.Length > 0)
        {
            return connection;
        }

        string itemPath = connection.ItemPath ?? RootPath;
        return connection with
        {
            BrowsePaths = itemPath.Length == 0 ? [RootPath] : [itemPath],
        };
    }

    private static bool IsInBrowsePath(
        DxConnection connection,
        string browsePath,
        bool recursive)
    {
        string normalizedPath = browsePath.Trim('.');
        if (normalizedPath.Length == 0)
        {
            return true;
        }

        string[] paths = connection.BrowsePaths.Length == 0
            ? [connection.ItemPath ?? RootPath]
            : connection.BrowsePaths;
        return paths.Any(path =>
        {
            string candidate = path.Trim('.');
            return string.Equals(candidate, normalizedPath, StringComparison.Ordinal) ||
                (recursive &&
                 candidate.StartsWith(normalizedPath + ".", StringComparison.Ordinal));
        });
    }

    private static bool MatchesConnection(DxConnection connection, DxConnection mask)
    {
        if (!MatchesMask(connection.Name, mask.Name))
        {
            return false;
        }

        return string.IsNullOrEmpty(mask.SourceServerName) ||
            string.Equals(
                connection.SourceServerName,
                mask.SourceServerName,
                StringComparison.Ordinal);
    }

    private static bool MatchesMask(string? value, string? mask)
    {
        if (string.IsNullOrEmpty(mask) || mask == "*")
        {
            return true;
        }

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return MatchWildcard(value, 0, mask, 0);
    }

    private static bool MatchWildcard(
        string value,
        int valueIndex,
        string pattern,
        int patternIndex)
    {
        while (patternIndex < pattern.Length)
        {
            char patternChar = pattern[patternIndex];
            if (patternChar == '*')
            {
                while (patternIndex + 1 < pattern.Length &&
                       pattern[patternIndex + 1] == '*')
                {
                    patternIndex++;
                }

                if (patternIndex + 1 == pattern.Length)
                {
                    return true;
                }

                for (int i = valueIndex; i <= value.Length; i++)
                {
                    if (MatchWildcard(value, i, pattern, patternIndex + 1))
                    {
                        return true;
                    }
                }

                return false;
            }

            if (valueIndex >= value.Length ||
                (patternChar != '?' && patternChar != value[valueIndex]))
            {
                return false;
            }

            valueIndex++;
            patternIndex++;
        }

        return valueIndex == value.Length;
    }
}
