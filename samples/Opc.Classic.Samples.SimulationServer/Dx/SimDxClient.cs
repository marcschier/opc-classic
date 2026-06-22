// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Opc.Classic.Dx;
using Opc.Classic.Mcp.Sessions;

namespace Opc.Classic.Samples.SimulationServer.Dx;

/// <summary>
/// In-memory OPC DX client used by the simulation server's MCP DX endpoint.
/// </summary>
public sealed class SimDxClient : IOpcDxClient
{
    private const string RootPath = "";
    private readonly Dictionary<string, DxConnection> _connections = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DxSourceServer> _sources = new(StringComparer.Ordinal);
    private readonly DateTimeOffset _startTime;
    private bool _disposed;

    /// <summary>
    /// Creates a DX client with a small deterministic source-server and connection configuration.
    /// </summary>
    public SimDxClient()
    {
        _startTime = DateTimeOffset.UtcNow;
        SeedConfiguration();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Dx,
            StartTime = _startTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = _disposed ? OpcServerState.Failed : OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Opc.Classic Simulation DX Client",
            GroupCount = _connections.Count,
        });
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> QueryConnectionNamesAsync(
        string browsePath,
        IReadOnlyList<string> connectionMasks,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browsePath);
        ArgumentNullException.ThrowIfNull(connectionMasks);
        cancellationToken.ThrowIfCancellationRequested();

        IEnumerable<DxConnection> connections = _connections.Values.Where(connection => IsInBrowsePath(connection, browsePath, recursive));
        if (connectionMasks.Count > 0)
        {
            connections = connections.Where(connection => MatchesAnyMask(connection.Name, connectionMasks));
        }

        return Task.FromResult<IReadOnlyList<string>>(connections
            .Select(static connection => connection.Name ?? string.Empty)
            .Where(static name => name.Length > 0)
            .Order(StringComparer.Ordinal)
            .ToArray());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<DxSourceServer>> QuerySourceServersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<DxSourceServer>>(_sources.Values
            .OrderBy(static source => source.Name, StringComparer.Ordinal)
            .ToArray());
    }

    /// <inheritdoc />
    public Task<OpcResultId> AddConnectionAsync(DxConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        string name = connection.Name ?? string.Empty;
        if (name.Length == 0)
        {
            return Task.FromResult(OpcResultId.InvalidArg);
        }

        _connections[name] = NormalizeConnection(connection);
        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task<OpcResultId> ModifyConnectionAsync(DxConnection connection, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        cancellationToken.ThrowIfCancellationRequested();
        string name = connection.Name ?? string.Empty;
        if (!_connections.ContainsKey(name))
        {
            return Task.FromResult(OpcResultId.UnknownItemId);
        }

        _connections[name] = NormalizeConnection(connection);
        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task<OpcResultId> UpdateConnectionAsync(
        string browsePath,
        string connectionName,
        bool recursive,
        DxConnection connectionDefinition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browsePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        ArgumentNullException.ThrowIfNull(connectionDefinition);
        cancellationToken.ThrowIfCancellationRequested();

        string[] matchedNames = _connections.Values
            .Where(connection => IsInBrowsePath(connection, browsePath, recursive) && MatchesMask(connection.Name, connectionName))
            .Select(static connection => connection.Name ?? string.Empty)
            .Where(static name => name.Length > 0)
            .ToArray();
        if (matchedNames.Length == 0)
        {
            return Task.FromResult(OpcResultId.UnknownItemId);
        }

        foreach (string name in matchedNames)
        {
            _connections[name] = NormalizeConnection(connectionDefinition with { Name = name });
        }

        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task<OpcResultId> DeleteConnectionAsync(
        string browsePath,
        string connectionName,
        bool recursive,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browsePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        cancellationToken.ThrowIfCancellationRequested();

        string[] matchedNames = _connections.Values
            .Where(connection => IsInBrowsePath(connection, browsePath, recursive) && MatchesMask(connection.Name, connectionName))
            .Select(static connection => connection.Name ?? string.Empty)
            .Where(static name => name.Length > 0)
            .ToArray();
        foreach (string name in matchedNames)
        {
            _connections.Remove(name);
        }

        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task<OpcResultId> AddSourceServerAsync(DxSourceServer sourceServer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceServer);
        cancellationToken.ThrowIfCancellationRequested();
        string name = sourceServer.Name ?? string.Empty;
        if (name.Length == 0)
        {
            return Task.FromResult(OpcResultId.InvalidArg);
        }

        _sources[name] = sourceServer;
        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task<OpcResultId> ModifySourceServerAsync(DxSourceServer sourceServer, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceServer);
        cancellationToken.ThrowIfCancellationRequested();
        string name = sourceServer.Name ?? string.Empty;
        if (!_sources.ContainsKey(name))
        {
            return Task.FromResult(OpcResultId.UnknownItemId);
        }

        _sources[name] = sourceServer;
        return Task.FromResult(OpcResultId.Ok);
    }

    /// <inheritdoc />
    public Task<string> ResetConfigurationAsync(string configurationVersion, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(configurationVersion);
        cancellationToken.ThrowIfCancellationRequested();
        _connections.Clear();
        _sources.Clear();
        return Task.FromResult(configurationVersion + ":reset");
    }

    private void SeedConfiguration()
    {
        AddSeedSource(new DxSourceServer(
            name: "ReactorPLC",
            serverUrl: "opcda://reactor-plc/Opc.Classic.Samples.Reactor",
            description: "Primary reactor unit PLC",
            serverType: "OPC DA 3.0",
            itemPath: "Plant.Reactor1",
            itemName: "ReactorPLC",
            version: "cfg-1",
            defaultConnected: true));
        AddSeedSource(new DxSourceServer(
            name: "PackagingPLC",
            serverUrl: "opcda://packaging-plc/Opc.Classic.Samples.Packaging",
            description: "Packaging line PLC",
            serverType: "OPC DA 3.0",
            itemPath: "Plant.Packaging",
            itemName: "PackagingPLC",
            version: "cfg-1",
            defaultConnected: true));

        AddSeedConnection(new DxConnection(
            name: "ReactorTemperatureToHistorian",
            description: "Mirrors reactor temperature into the DX target namespace.",
            itemPath: "Plant.Reactor1",
            itemName: "TemperatureMirror",
            version: "cfg-1",
            browsePaths: ["Plant", "Plant.Reactor1"],
            keyword: "temperature",
            defaultSourceItemConnected: true,
            defaultTargetItemConnected: true,
            sourceServerName: "ReactorPLC",
            sourceItemPath: "Plant.Reactor1",
            sourceItemName: "Temperature",
            sourceItemQueueSize: 4,
            targetItemPath: "Dx.Targets.Reactor1",
            targetItemName: "Temperature",
            updateRateMilliseconds: 1000,
            deadbandPercent: 0.5f,
            vendorData: "sim:reactor-temperature"));
        AddSeedConnection(new DxConnection(
            name: "PackagingRateToLineDashboard",
            description: "Publishes packaging rate into the simulated dashboard target.",
            itemPath: "Plant.Packaging",
            itemName: "RateMirror",
            version: "cfg-1",
            browsePaths: ["Plant", "Plant.Packaging"],
            keyword: "rate",
            defaultSourceItemConnected: true,
            defaultTargetItemConnected: true,
            sourceServerName: "PackagingPLC",
            sourceItemPath: "Plant.Packaging",
            sourceItemName: "Rate",
            sourceItemQueueSize: 2,
            targetItemPath: "Dx.Targets.Packaging",
            targetItemName: "Rate",
            updateRateMilliseconds: 500,
            deadbandPercent: 1.0f,
            vendorData: "sim:packaging-rate"));
    }

    private void AddSeedSource(DxSourceServer source)
    {
        _sources[source.Name ?? string.Empty] = source;
    }

    private void AddSeedConnection(DxConnection connection)
    {
        _connections[connection.Name ?? string.Empty] = connection;
    }

    private static DxConnection NormalizeConnection(DxConnection connection)
    {
        if (connection.BrowsePaths.Length > 0)
        {
            return connection;
        }

        string itemPath = connection.ItemPath ?? RootPath;
        return connection with { BrowsePaths = itemPath.Length == 0 ? [RootPath] : [itemPath] };
    }

    private static bool IsInBrowsePath(DxConnection connection, string browsePath, bool recursive)
    {
        string normalizedPath = browsePath.Trim('.');
        if (normalizedPath.Length == 0)
        {
            return true;
        }

        string[] paths = connection.BrowsePaths.Length == 0 ? [connection.ItemPath ?? RootPath] : connection.BrowsePaths;
        foreach (string path in paths)
        {
            string normalizedConnectionPath = path.Trim('.');
            if (string.Equals(normalizedConnectionPath, normalizedPath, StringComparison.Ordinal))
            {
                return true;
            }

            if (recursive &&
                normalizedConnectionPath.StartsWith(normalizedPath + ".", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesAnyMask(string? name, IReadOnlyList<string> masks)
    {
        foreach (string mask in masks)
        {
            if (MatchesMask(name, mask))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesMask(string? name, string mask)
    {
        if (string.IsNullOrEmpty(name))
        {
            return false;
        }

        if (mask.Length == 0 || mask == "*")
        {
            return true;
        }

        return MatchWildcard(name, 0, mask, 0);
    }

    private static bool MatchWildcard(string value, int valueIndex, string pattern, int patternIndex)
    {
        while (patternIndex < pattern.Length)
        {
            char patternChar = pattern[patternIndex];
            if (patternChar == '*')
            {
                while (patternIndex + 1 < pattern.Length && pattern[patternIndex + 1] == '*')
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

            if (valueIndex >= value.Length)
            {
                return false;
            }

            if (patternChar != '?' && patternChar != value[valueIndex])
            {
                return false;
            }

            valueIndex++;
            patternIndex++;
        }

        return valueIndex == value.Length;
    }
}
