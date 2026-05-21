//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic;
using OpcClassic.Dx;
using TUnit.Core;

namespace OpcClassic.Dx.Tests;

public sealed class ConnectionStateTests
{
    private static int ValueOf(ConnectionState s) => (int)s;

    [Test]
    public async Task EnumValues_MatchOpcDxSpec()
    {
        // OPC DX 1.0 §4.1 CONNECTION_STATE values.
        await Assert.That(ValueOf(ConnectionState.Initial)).IsEqualTo(0);
        await Assert.That(ValueOf(ConnectionState.Connecting)).IsEqualTo(1);
        await Assert.That(ValueOf(ConnectionState.Subscribing)).IsEqualTo(2);
        await Assert.That(ValueOf(ConnectionState.Connected)).IsEqualTo(3);
        await Assert.That(ValueOf(ConnectionState.Disconnecting)).IsEqualTo(4);
        await Assert.That(ValueOf(ConnectionState.Disconnected)).IsEqualTo(5);
    }
}

public sealed class DxConnectionTests
{
    [Test]
    public async Task Default_HasDisabledOverride_InitialState()
    {
        var c = new DxConnection();
        await Assert.That(c.OverrideState).IsEqualTo(OverrideState.Disabled);
        await Assert.That(c.State).IsEqualTo(ConnectionState.Initial);
        await Assert.That(c.UpdateRateMs).IsEqualTo(0);
        await Assert.That(c.DeadbandPercent).IsEqualTo(0f);
    }

    [Test]
    public async Task Initializer_AssignsAllFields()
    {
        var c = new DxConnection
        {
            Name = "Tank1_to_HMI",
            SourceServerName = "PLC1",
            SourceItemId = "PLC1.Tank1.Level",
            TargetItemId = "HMI.Tank1.Level",
            UpdateRateMs = 1000,
            DeadbandPercent = 0.5f,
            OverrideState = OverrideState.Enabled,
            OverrideValue = 42.0,
            State = ConnectionState.Connected,
        };

        await Assert.That(c.Name).IsEqualTo("Tank1_to_HMI");
        await Assert.That(c.SourceItemId).IsEqualTo("PLC1.Tank1.Level");
        await Assert.That(c.TargetItemId).IsEqualTo("HMI.Tank1.Level");
        await Assert.That(c.UpdateRateMs).IsEqualTo(1000);
        await Assert.That(c.OverrideState).IsEqualTo(OverrideState.Enabled);
        await Assert.That(c.OverrideValue).IsEqualTo(42.0);
        await Assert.That(c.State).IsEqualTo(ConnectionState.Connected);
    }
}

public sealed class SourceServerTests
{
    [Test]
    public async Task Default_HasEmptyStrings()
    {
        var s = new SourceServer();
        await Assert.That(s.Name).IsEqualTo(string.Empty);
        await Assert.That(s.Url).IsEqualTo(string.Empty);
        await Assert.That(s.Description).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Initializer_AssignsAllFields()
    {
        var s = new SourceServer
        {
            Name = "PLC1",
            Url = "opcda://plc1.plant1/Vendor.OPC.1",
            Description = "Main process PLC",
        };
        await Assert.That(s.Name).IsEqualTo("PLC1");
        await Assert.That(s.Url).IsEqualTo("opcda://plc1.plant1/Vendor.OPC.1");
        await Assert.That(s.Description).IsEqualTo("Main process PLC");
    }
}

internal sealed class FakeDxServer : IDxServer
{
    private readonly Dictionary<string, SourceServer> _sources = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DxConnection> _connections = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<EventArgs>? ServerShutdown;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) =>
        Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Dx, State = OpcServerState.Running });

    public Task<IReadOnlyList<SourceServer>> GetSourceServersAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<SourceServer>>(_sources.Values.ToList());

    public Task<OpcResultId> AddOrUpdateSourceServerAsync(SourceServer source, CancellationToken ct = default)
    {
        _sources[source.Name] = source;
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> RemoveSourceServerAsync(string sourceServerName, CancellationToken ct = default)
    {
        var removed = _sources.Remove(sourceServerName);
        var toRemove = _connections.Where(kv => string.Equals(kv.Value.SourceServerName, sourceServerName, StringComparison.OrdinalIgnoreCase)).Select(kv => kv.Key).ToList();
        foreach (var k in toRemove)
        {
            _connections.Remove(k);
        }
        return Task.FromResult(removed ? OpcResultId.Ok : OpcResultId.NotFound);
    }

    public Task<IReadOnlyList<DxConnection>> GetConnectionsAsync(string? nameFilter = null, CancellationToken ct = default)
    {
        IEnumerable<DxConnection> result = _connections.Values;
        if (!string.IsNullOrEmpty(nameFilter))
        {
            result = result.Where(c => c.Name.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));
        }
        return Task.FromResult<IReadOnlyList<DxConnection>>(result.ToList());
    }

    public Task<OpcResultId> AddOrUpdateConnectionAsync(DxConnection connection, CancellationToken ct = default)
    {
        _connections[connection.Name] = connection;
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> RemoveConnectionAsync(string connectionName, CancellationToken ct = default) =>
        Task.FromResult(_connections.Remove(connectionName) ? OpcResultId.Ok : OpcResultId.NotFound);

    public Task<OpcResultId> ResetConfigurationAsync(CancellationToken ct = default)
    {
        _sources.Clear();
        _connections.Clear();
        return Task.FromResult(OpcResultId.Ok);
    }

    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }
}

public sealed class IDxServerContractTests
{
    [Test]
    public async Task GetStatusAsync_ReturnsDxStatus()
    {
        await using var server = new FakeDxServer();
        var status = await server.GetStatusAsync();
        await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Dx);
    }

    [Test]
    public async Task AddSourceServer_ThenGet_ReturnsIt()
    {
        await using var server = new FakeDxServer();
        await server.AddOrUpdateSourceServerAsync(new SourceServer { Name = "PLC1", Url = "opcda://h/x" });
        var sources = await server.GetSourceServersAsync();
        await Assert.That(sources.Count).IsEqualTo(1);
        await Assert.That(sources[0].Name).IsEqualTo("PLC1");
    }

    [Test]
    public async Task RemoveSourceServer_AlsoRemovesDependentConnections()
    {
        await using var server = new FakeDxServer();
        await server.AddOrUpdateSourceServerAsync(new SourceServer { Name = "PLC1", Url = "opcda://h/x" });
        await server.AddOrUpdateConnectionAsync(new DxConnection
        {
            Name = "C1",
            SourceServerName = "PLC1",
            SourceItemId = "A",
            TargetItemId = "B",
        });

        await server.RemoveSourceServerAsync("PLC1");
        var connections = await server.GetConnectionsAsync();
        await Assert.That(connections.Count).IsEqualTo(0);
    }

    [Test]
    public async Task GetConnections_WithNameFilter_FiltersResults()
    {
        await using var server = new FakeDxServer();
        await server.AddOrUpdateConnectionAsync(new DxConnection { Name = "Tank1_HMI", SourceItemId = "A", TargetItemId = "B" });
        await server.AddOrUpdateConnectionAsync(new DxConnection { Name = "Tank2_HMI", SourceItemId = "C", TargetItemId = "D" });
        await server.AddOrUpdateConnectionAsync(new DxConnection { Name = "Other", SourceItemId = "E", TargetItemId = "F" });

        var filtered = await server.GetConnectionsAsync("Tank");
        await Assert.That(filtered.Count).IsEqualTo(2);
    }

    [Test]
    public async Task RemoveConnection_NonExistent_ReturnsNotFound()
    {
        await using var server = new FakeDxServer();
        var result = await server.RemoveConnectionAsync("DoesNotExist");
        await Assert.That(result).IsEqualTo(OpcResultId.NotFound);
    }

    [Test]
    public async Task ResetConfiguration_ClearsAll()
    {
        await using var server = new FakeDxServer();
        await server.AddOrUpdateSourceServerAsync(new SourceServer { Name = "S1" });
        await server.AddOrUpdateConnectionAsync(new DxConnection { Name = "C1" });
        await server.ResetConfigurationAsync();
        await Assert.That((await server.GetSourceServersAsync()).Count).IsEqualTo(0);
        await Assert.That((await server.GetConnectionsAsync()).Count).IsEqualTo(0);
    }
}
