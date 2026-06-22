// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dx;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Mcp.Tests;

public sealed class DxToolsTests
{
    [Test]
    public async Task Dx_connect_status_and_source_servers_round_trip_via_mcp_client()
    {
        var dx = new SyntheticDxClient();
        string name = "dx-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryDxConnectionRegistry.Register(name, dx);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcSessionDto connected = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.dx.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + name,
            }).ConfigureAwait(false);
        OpcServerStatusDto status = await server.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.dx.get_status",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcResultDto added = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.add_source_server",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["sourceServer"] = new OpcDxSourceServerDto("PLC1", "opcda://plc1/Vendor.OPC.1", "Primary PLC"),
            }).ConfigureAwait(false);
        OpcDxSourceServerDto[] sources = await server.CallToolAsync<OpcDxSourceServerDto[]>(
            "opcclassic.dx.query_source_servers",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcResultDto modified = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.modify_source_server",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["sourceServer"] = new OpcDxSourceServerDto("PLC1", "opcda://plc1/Vendor.OPC.2", "Modified PLC"),
            }).ConfigureAwait(false);

        await Assert.That(connected.DaConnected).IsTrue();
        await Assert.That(status.VendorInfo).IsEqualTo("Synthetic MCP DX Server");
        await Assert.That(added.Succeeded).IsTrue();
        await Assert.That(sources.Any(static source => source.Name == "PLC1")).IsTrue();
        await Assert.That(modified.Succeeded).IsTrue();
    }

    [Test]
    public async Task Dx_connection_lifecycle_round_trips_via_mcp_client()
    {
        var dx = new SyntheticDxClient();
        string name = "dx-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryDxConnectionRegistry.Register(name, dx);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.dx.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + name,
            }).ConfigureAwait(false);

        var connection = new OpcDxConnectionDto(
            Name: "C1",
            Description: "Source to target",
            SourceServerName: "PLC1",
            SourceItemName: "ns=1;s=Source",
            TargetItemName: "ns=1;s=Target",
            UpdateRateMilliseconds: 1000);
        OpcResultDto added = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.add_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connection"] = connection,
            }).ConfigureAwait(false);
        string[] names = await server.CallToolAsync<string[]>(
            "opcclassic.dx.query_connections",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcResultDto modified = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.modify_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connection"] = connection with { Description = "Modified" },
            }).ConfigureAwait(false);
        OpcResultDto updated = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.update_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionName"] = "C1",
                ["connection"] = connection with { UpdateRateMilliseconds = 250 },
            }).ConfigureAwait(false);
        OpcResultDto deleted = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.delete_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionName"] = "C1",
            }).ConfigureAwait(false);

        await Assert.That(added.Succeeded).IsTrue();
        await Assert.That(names).Contains("C1");
        await Assert.That(modified.Succeeded).IsTrue();
        await Assert.That(updated.Succeeded).IsTrue();
        await Assert.That(deleted.Succeeded).IsTrue();
        await Assert.That(dx.ConnectionCount).IsEqualTo(0);
    }

    [Test]
    public async Task Dx_reset_and_disconnect_round_trip_via_mcp_client()
    {
        var dx = new SyntheticDxClient();
        string name = "dx-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryDxConnectionRegistry.Register(name, dx);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.dx.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + name,
            }).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.add_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connection"] = new OpcDxConnectionDto(Name: "C1", SourceServerName: "PLC1"),
            }).ConfigureAwait(false);

        OpcResultDto reset = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.reset_configuration",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["configurationVersion"] = "cfg-1",
            }).ConfigureAwait(false);
        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(reset.Succeeded).IsTrue();
        await Assert.That(dx.ConnectionCount).IsEqualTo(0);
        await Assert.That(disconnected.Succeeded).IsTrue();
        await Assert.That(dx.Disposed).IsTrue();
    }
}

internal sealed class SyntheticDxClient : IOpcDxClient
{
    private readonly Dictionary<string, DxConnection> _connections = new(StringComparer.Ordinal);
    private readonly Dictionary<string, DxSourceServer> _sources = new(StringComparer.Ordinal);

    public bool Disposed { get; private set; }
    public int ConnectionCount => _connections.Count;

    public ValueTask DisposeAsync()
    {
        Disposed = true;
        return ValueTask.CompletedTask;
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Dx,
            StartTime = DateTimeOffset.UnixEpoch,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0),
            VendorInfo = "Synthetic MCP DX Server",
            GroupCount = _connections.Count,
        });
    }

    public Task<IReadOnlyList<string>> QueryConnectionNamesAsync(string browsePath, IReadOnlyList<string> connectionMasks, bool recursive, CancellationToken cancellationToken = default)
    {
        _ = browsePath;
        _ = recursive;
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<string> names = _connections.Keys.Order(StringComparer.Ordinal);
        if (connectionMasks.Count > 0)
        {
            names = names.Where(name => connectionMasks.Contains(name, StringComparer.Ordinal));
        }

        return Task.FromResult<IReadOnlyList<string>>(names.ToArray());
    }

    public Task<IReadOnlyList<DxSourceServer>> QuerySourceServersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<DxSourceServer>>(_sources.Values.OrderBy(static source => source.Name, StringComparer.Ordinal).ToArray());
    }

    public Task<OpcResultId> AddConnectionAsync(DxConnection connection, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connections[connection.Name ?? string.Empty] = connection;
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> ModifyConnectionAsync(DxConnection connection, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string name = connection.Name ?? string.Empty;
        if (!_connections.ContainsKey(name))
        {
            return Task.FromResult(OpcResultId.UnknownItemId);
        }

        _connections[name] = connection;
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> UpdateConnectionAsync(string browsePath, string connectionName, bool recursive, DxConnection connectionDefinition, CancellationToken cancellationToken = default)
    {
        _ = browsePath;
        _ = recursive;
        cancellationToken.ThrowIfCancellationRequested();
        if (!_connections.ContainsKey(connectionName))
        {
            return Task.FromResult(OpcResultId.UnknownItemId);
        }

        _connections[connectionName] = connectionDefinition with { Name = connectionName };
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> DeleteConnectionAsync(string browsePath, string connectionName, bool recursive, CancellationToken cancellationToken = default)
    {
        _ = browsePath;
        _ = recursive;
        cancellationToken.ThrowIfCancellationRequested();
        _connections.Remove(connectionName);
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> AddSourceServerAsync(DxSourceServer sourceServer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _sources[sourceServer.Name ?? string.Empty] = sourceServer;
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> ModifySourceServerAsync(DxSourceServer sourceServer, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string name = sourceServer.Name ?? string.Empty;
        if (!_sources.ContainsKey(name))
        {
            return Task.FromResult(OpcResultId.UnknownItemId);
        }

        _sources[name] = sourceServer;
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<string> ResetConfigurationAsync(string configurationVersion, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connections.Clear();
        _sources.Clear();
        return Task.FromResult(configurationVersion + ":reset");
    }
}
