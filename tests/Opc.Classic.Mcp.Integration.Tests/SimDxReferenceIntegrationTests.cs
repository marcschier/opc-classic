// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using Opc.Classic.Dx;
using Opc.Classic.Dx.Dcom;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Samples.SimulationServer;
using Opc.Classic.Samples.SimulationServer.Dx;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class SimDxReferenceIntegrationTests
{
    [Test]
    public async Task Iopcconfiguration_proxy_round_trips_crud_and_reset_over_channel()
    {
        SimDxClient client = await CreateClientAsync().ConfigureAwait(false);
        try
        {
            var proxy = new IOPCConfigurationClientProxy(client.Channel);
            DxConnection connection = CreateDxConnection("ProxyCrudMirror", "Int4", updateRateMilliseconds: 100);

            DxGeneralResponse add = await proxy.AddDXConnectionsAsync([connection]).ConfigureAwait(false);
            await Assert.That(add.ConfigurationVersion).IsEqualTo("1");
            await Assert.That(add.IdentifiedResults.Single().ResultId.Code).IsEqualTo(OpcResultId.Ok.Code);

            string[] namesAfterAdd = await proxy.QueryDXConnectionNamesAsync(
                "Plant.Reactor1",
                [connection.Name!],
                true).ConfigureAwait(false);
            await Assert.That(namesAfterAdd.SequenceEqual(new[] { connection.Name! })).IsTrue();

            DxConfigurationSnapshot afterAdd = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
            await Assert.That(afterAdd.Version).IsEqualTo(1);
            DxConnection added = afterAdd.Configuration.Connections.Single(c => c.Name == connection.Name);
            await Assert.That(added.DefaultSourceItemConnected).IsTrue();
            await Assert.That(added.UpdateRateMilliseconds).IsEqualTo(100);

            DxConnection modified = connection with
            {
                Description = "disabled for CRUD test",
                DefaultSourceItemConnected = false,
                DefaultTargetItemConnected = false,
                UpdateRateMilliseconds = 150,
            };
            DxGeneralResponse modify = await proxy.ModifyDXConnectionsAsync([modified]).ConfigureAwait(false);
            await Assert.That(modify.ConfigurationVersion).IsEqualTo("2");
            await Assert.That(modify.IdentifiedResults.Single().ResultId.Code).IsEqualTo(OpcResultId.Ok.Code);

            DxConfigurationSnapshot afterModify = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
            await Assert.That(afterModify.Version).IsEqualTo(2);
            DxConnection disabled = afterModify.Configuration.Connections.Single(c => c.Name == connection.Name);
            await Assert.That(disabled.DefaultSourceItemConnected).IsFalse();
            await Assert.That(disabled.DefaultTargetItemConnected).IsFalse();
            await Assert.That(disabled.UpdateRateMilliseconds).IsEqualTo(150);

            DxConnection updatedDefinition = new(
                description: "must not replace stored fields",
                updateRateMilliseconds: 200,
                deadbandPercent: 99,
                mask: (int)DxMask.UpdateRate);
            DxUpdateConnectionsResult update = await proxy.UpdateDXConnectionsAsync(
                "Plant.Reactor1",
                [new DxConnection(name: connection.Name)],
                true,
                updatedDefinition).ConfigureAwait(false);
            await Assert.That(update.Errors.Single()).IsEqualTo(OpcResultId.Ok.Code);
            await Assert.That(update.Response.ConfigurationVersion).IsEqualTo("3");

            DxConfigurationSnapshot afterUpdate = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
            await Assert.That(afterUpdate.Version).IsEqualTo(3);
            DxConnection enabled = afterUpdate.Configuration.Connections.Single(c => c.Name == connection.Name);
            await Assert.That(enabled.Description).IsEqualTo("disabled for CRUD test");
            await Assert.That(enabled.DefaultSourceItemConnected).IsFalse();
            await Assert.That(enabled.DefaultTargetItemConnected).IsFalse();
            await Assert.That(enabled.UpdateRateMilliseconds).IsEqualTo(200);
            await Assert.That(enabled.DeadbandPercent).IsEqualTo(0.5f);
            await Assert.That(enabled.Mask).IsEqualTo(disabled.Mask);

            DxDeleteConnectionsResult delete = await proxy.DeleteDXConnectionsAsync(
                "Plant.Reactor1",
                [new DxConnection(name: connection.Name)],
                true).ConfigureAwait(false);
            await Assert.That(delete.ConfigurationVersion).IsEqualTo("4");
            await Assert.That(delete.IdentifiedResults.Single().ResultId.Code).IsEqualTo(OpcResultId.Ok.Code);

            DxConfigurationSnapshot afterDelete = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
            await Assert.That(afterDelete.Version).IsEqualTo(4);
            await Assert.That(afterDelete.Configuration.Connections.Any(c => c.Name == connection.Name)).IsFalse();

            OpcException? staleError = null;
            try
            {
                _ = await proxy.ResetConfigurationAsync("3").ConfigureAwait(false);
            }
            catch (OpcException ex)
            {
                staleError = ex;
            }

            ArgumentNullException.ThrowIfNull(staleError);
            await Assert.That(staleError.ResultId.Code).IsEqualTo(OpcDxError.OPCDX_E_VERSION_MISMATCH.Code);

            DxConfigurationSnapshot afterStaleReset =
                await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
            await Assert.That(afterStaleReset.Version).IsEqualTo(4);
            await Assert.That(afterStaleReset.Configuration.SourceServers.Length).IsEqualTo(1);

            string reset = await proxy.ResetConfigurationAsync("4").ConfigureAwait(false);
            await Assert.That(reset).IsEqualTo("5");

            DxConfigurationSnapshot afterReset = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
            await Assert.That(afterReset.Version).IsEqualTo(5);
            await Assert.That(afterReset.Configuration.Connections.Length).IsEqualTo(0);
        }
        finally
        {
            await client.ShutdownAsync().ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Iopcconfiguration_query_honors_every_selected_connection_mask_field()
    {
        SimDxClient client = await CreateClientAsync().ConfigureAwait(false);
        try
        {
            var proxy = new IOPCConfigurationClientProxy(client.Channel);
            var cases = new[]
            {
                (
                    Mask: new DxConnection(
                        name: "ignored-name",
                        sourceItemName: "Temperature",
                        mask: (int)DxMask.SourceItemName),
                    Expected: new[] { "ReactorTemperatureToBucket" }),
                (
                    Mask: new DxConnection(
                        targetItemName: "Int4",
                        mask: (int)DxMask.TargetItemName),
                    Expected: new[] { "ReactorPressureDisabled" }),
                (
                    Mask: new DxConnection(
                        sourceItemName: "Temperature",
                        targetItemName: "Int4",
                        mask: (int)(DxMask.SourceItemName | DxMask.TargetItemName)),
                    Expected: Array.Empty<string>()),
                (
                    Mask: new DxConnection(
                        itemName: "Pressure*",
                        mask: (int)DxMask.ItemName),
                    Expected: new[] { "ReactorPressureDisabled" }),
                (
                    Mask: new DxConnection(
                        name: "Reactor*",
                        keyword: "pressure",
                        mask: (int)(DxMask.Name | DxMask.Keyword)),
                    Expected: new[] { "ReactorPressureDisabled" }),
                (
                    Mask: new DxConnection(
                        sourceServerName: "SimulationDA",
                        deadbandPercent: 0.5f,
                        mask: (int)(DxMask.SourceServerName | DxMask.DeadBand)),
                    Expected: new[] { "ReactorTemperatureToBucket" }),
            };

            foreach ((DxConnection mask, string[] expected) in cases)
            {
                DxConnectionQueryResult result = await proxy.QueryDXConnectionsAsync(
                    "",
                    [mask],
                    recursive: true).ConfigureAwait(false);
                string[] actual = result.Connections
                    .Select(static connection => connection.Name ?? string.Empty)
                    .ToArray();
                await Assert.That(actual.SequenceEqual(expected)).IsTrue();
            }
        }
        finally
        {
            await client.ShutdownAsync().ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Json_persistence_recovers_restart_and_actual_transfer_after_proxy_add()
    {
        string root = Path.Combine(
            AppContext.BaseDirectory,
            "simdx-reference-tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        string storePath = Path.Combine(root, "dx-config.json");

        try
        {
            var model1 = new SimulatedPlantModel();
            var scheduler1 = new ManualDxScheduler(model1.StartTimeUtc);
            using var store1 = new JsonFileDxConfigurationStore(storePath);

            SimDxClient client1 = await CreateClientAsync(model1, store1, scheduler1).ConfigureAwait(false);
            try
            {
                var proxy1 = new IOPCConfigurationClientProxy(client1.Channel);
                DxConnection connection = CreateDxConnection("RestartRecoveryMirror", "Int4", updateRateMilliseconds: 100);

                DxGeneralResponse add = await proxy1.AddDXConnectionsAsync([connection]).ConfigureAwait(false);
                await Assert.That(add.ConfigurationVersion).IsEqualTo("2");
            }
            finally
            {
                await client1.ShutdownAsync().ConfigureAwait(false);
            }

            var model2 = new SimulatedPlantModel();
            var scheduler2 = new ManualDxScheduler(model2.StartTimeUtc);
            using var store2 = new JsonFileDxConfigurationStore(storePath);

            SimDxClient client2 = await CreateClientAsync(model2, store2, scheduler2).ConfigureAwait(false);
            try
            {
                DxConfigurationSnapshot recovered = await client2.Engine.GetConfigurationAsync().ConfigureAwait(false);
                await Assert.That(recovered.Version).IsEqualTo(2);

                DxConnection recoveredConnection = recovered.Configuration.Connections.Single(
                    connection => connection.Name == "RestartRecoveryMirror");
                await Assert.That(recoveredConnection.DefaultSourceItemConnected).IsTrue();
                await Assert.That(recoveredConnection.UpdateRateMilliseconds).IsEqualTo(100);

                DxTransferSnapshot transfer = await WaitForConnectionSnapshotAsync(
                    client2,
                    "RestartRecoveryMirror",
                    snapshot => snapshot.ReadCount >= 1 && snapshot.WriteCount >= 1).ConfigureAwait(false);
                await Assert.That(transfer.State).IsEqualTo(DxTransferState.Running);
                await Assert.That(transfer.LastSourceValue!.ErrorId).IsEqualTo(OpcResultId.Ok);
                await Assert.That(transfer.LastWriteResult!.ErrorId).IsEqualTo(OpcResultId.Ok);

                model2.TryGetTag("Plant.Reactor1.Temperature", out SimulatedTag? sourceTag);
                model2.TryGetTag("Bucket Brigade.Int4", out SimulatedTag? targetTag);
                ArgumentNullException.ThrowIfNull(sourceTag);
                ArgumentNullException.ThrowIfNull(targetTag);
                int sourceValue = Convert.ToInt32(
                    model2.CurrentValue(sourceTag, scheduler2.UtcNow),
                    CultureInfo.InvariantCulture);
                int targetValue = Convert.ToInt32(
                    model2.CurrentValue(targetTag, scheduler2.UtcNow),
                    CultureInfo.InvariantCulture);
                await Assert.That(targetValue).IsEqualTo(sourceValue);
            }
            finally
            {
                await client2.ShutdownAsync().ConfigureAwait(false);
            }
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Test]
    public async Task Enabled_disabled_and_update_rate_round_trip_through_runtime_snapshot()
    {
        var model = new SimulatedPlantModel();
        var scheduler = new ManualDxScheduler(model.StartTimeUtc);
        SimDxClient client = await CreateClientAsync(model, scheduler: scheduler).ConfigureAwait(false);
        try
        {
            var proxy = new IOPCConfigurationClientProxy(client.Channel);
            const string connectionName = "ReactorTemperatureToBucket";

            DxTransferSnapshot initial = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.ReadCount >= 1).ConfigureAwait(false);
            await Assert.That(initial.State).IsEqualTo(DxTransferState.Running);

            DxConnection seed = (await client.Engine.GetConfigurationAsync().ConfigureAwait(false))
                .Configuration.Connections.Single(connection => connection.Name == connectionName);

            DxConnection disabledDefinition = seed with
            {
                DefaultSourceItemConnected = false,
                DefaultTargetItemConnected = false,
            };
            await proxy.ModifyDXConnectionsAsync([disabledDefinition]).ConfigureAwait(false);
            DxTransferSnapshot disabled = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.Disabled).ConfigureAwait(false);
            long disabledReads = disabled.ReadCount;

            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(200));
            DxTransferSnapshot stillDisabled = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.Disabled &&
                    snapshot.ReadCount == disabledReads).ConfigureAwait(false);
            await Assert.That(stillDisabled.ReadCount).IsEqualTo(disabledReads);

            DxConnection enabledDefinition = seed with
            {
                DefaultSourceItemConnected = true,
                DefaultTargetItemConnected = true,
                UpdateRateMilliseconds = 100,
            };
            await proxy.UpdateDXConnectionsAsync(
                "Plant.Reactor1",
                [new DxConnection(name: connectionName)],
                true,
                enabledDefinition).ConfigureAwait(false);

            DxTransferSnapshot running = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.Running &&
                    snapshot.ReadCount >= 1).ConfigureAwait(false);
            long readsBeforeAdvance = running.ReadCount;

            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1000));
            DxTransferSnapshot afterAdvance = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.Running &&
                    snapshot.ReadCount > readsBeforeAdvance).ConfigureAwait(false);
            await Assert.That(afterAdvance.ReadCount > readsBeforeAdvance).IsTrue();
            await Assert.That(afterAdvance.LastWriteResult!.ErrorId).IsEqualTo(OpcResultId.Ok);
        }
        finally
        {
            await client.ShutdownAsync().ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Source_failure_triggers_reconnect_backoff_then_recovers()
    {
        var model = new SimulatedPlantModel();
        var scheduler = new ManualDxScheduler(model.StartTimeUtc);
        SimDxClient client = await CreateClientAsync(model, scheduler: scheduler).ConfigureAwait(false);
        try
        {
            const string connectionName = "ReactorTemperatureToBucket";
            await WaitForConnectionSnapshotAsync(client, connectionName, snapshot => snapshot.ReadCount >= 1)
                .ConfigureAwait(false);

            client.SourceEndpoint.FailNextRead();
            client.SourceEndpoint.FailReconnectAttempts(1);

            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1000));
            DxTransferSnapshot retry = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.RetryDelay &&
                    snapshot.ConsecutiveFailures == 1).ConfigureAwait(false);

            await Assert.That(retry.LastSourceValue!.ErrorId).IsEqualTo(OpcResultId.Fail);
            await Assert.That(retry.LastSourceValue.Quality.Quality).IsEqualTo(DxQualityStatus.BadCommFailure);
            await Assert.That(retry.NextRetryTimestamp).IsNotNull();
            await Assert.That(client.SourceEndpoint.ReconnectCount).IsEqualTo(1);
            await Assert.That(client.Engine.GetDiagnostics().Select(diagnostic => diagnostic.Code))
                .Contains("DX_SOURCE_FAILURE");
            await Assert.That(client.Engine.GetDiagnostics().Select(diagnostic => diagnostic.Code))
                .Contains("DX_RECONNECT_FAILED");

            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1000));
            DxTransferSnapshot recovered = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.Running &&
                    snapshot.ReadCount >= 2).ConfigureAwait(false);
            await Assert.That(recovered.ReadCount >= 2).IsTrue();
        }
        finally
        {
            await client.ShutdownAsync().ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Target_failure_triggers_reconnect_backoff_then_recovers()
    {
        var model = new SimulatedPlantModel();
        var scheduler = new ManualDxScheduler(model.StartTimeUtc);
        SimDxClient client = await CreateClientAsync(model, scheduler: scheduler).ConfigureAwait(false);
        try
        {
            const string connectionName = "ReactorTemperatureToBucket";
            await WaitForConnectionSnapshotAsync(client, connectionName, snapshot => snapshot.ReadCount >= 1)
                .ConfigureAwait(false);

            client.TargetEndpoint.FailNextWrite();
            client.TargetEndpoint.FailReconnectAttempts(1);

            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1000));
            DxTransferSnapshot retry = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.RetryDelay &&
                    snapshot.ConsecutiveFailures == 1).ConfigureAwait(false);

            await Assert.That(retry.LastWriteResult!.ErrorId).IsEqualTo(OpcResultId.Fail);
            await Assert.That(retry.LastSourceValue!.ErrorId).IsEqualTo(OpcResultId.Ok);
            await Assert.That(client.TargetEndpoint.ReconnectCount).IsEqualTo(1);
            await Assert.That(client.Engine.GetDiagnostics().Select(diagnostic => diagnostic.Code))
                .Contains("DX_TARGET_FAILURE");
            await Assert.That(client.Engine.GetDiagnostics().Select(diagnostic => diagnostic.Code))
                .Contains("DX_RECONNECT_FAILED");

            scheduler.AdvanceBy(TimeSpan.FromMilliseconds(1000));
            DxTransferSnapshot recovered = await WaitForConnectionSnapshotAsync(
                client,
                connectionName,
                snapshot => snapshot.State == DxTransferState.Running &&
                    snapshot.WriteCount >= 2).ConfigureAwait(false);
            await Assert.That(recovered.WriteCount >= 2).IsTrue();
        }
        finally
        {
            await client.ShutdownAsync().ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Blocked_read_is_cancelled_on_shutdown_and_reset_clears_configuration()
    {
        var cancelModel = new SimulatedPlantModel();
        var cancelScheduler = new ManualDxScheduler(cancelModel.StartTimeUtc);
        SimDxClient cancelClient = await CreateClientAsync(
            cancelModel,
            scheduler: cancelScheduler).ConfigureAwait(false);
        try
        {
            using IDisposable readBlock = cancelClient.SourceEndpoint.BlockReads();
            await WaitForConnectionSnapshotAsync(
                cancelClient,
                "ReactorTemperatureToBucket",
                snapshot => snapshot.ReadCount >= 1).ConfigureAwait(false);
            cancelScheduler.AdvanceBy(TimeSpan.FromMilliseconds(1000));
            await WaitForConnectionSnapshotAsync(
                cancelClient,
                "ReactorTemperatureToBucket",
                _ => cancelClient.SourceEndpoint.ReadCount >= 2).ConfigureAwait(false);

            await cancelClient.ShutdownAsync().ConfigureAwait(false);
            await Assert.That(cancelClient.SourceEndpoint.CanceledReadCount).IsEqualTo(1);
            readBlock.Dispose();
        }
        finally
        {
            await cancelClient.ShutdownAsync().ConfigureAwait(false);
        }

        SimDxClient resetClient = await CreateClientAsync().ConfigureAwait(false);
        try
        {
            var proxy = new IOPCConfigurationClientProxy(resetClient.Channel);
            await proxy.AddDXConnectionsAsync([CreateDxConnection("ResetMirror", "Int4", 100)])
                .ConfigureAwait(false);

            string reset = await proxy.ResetConfigurationAsync("1").ConfigureAwait(false);
            await Assert.That(reset).IsEqualTo("2");

            DxConfigurationSnapshot afterReset = await resetClient.Engine.GetConfigurationAsync().ConfigureAwait(false);
            await Assert.That(afterReset.Configuration.Connections.Length).IsEqualTo(0);
            await Assert.That(resetClient.Engine.GetStatusSnapshot().Connections.Length).IsEqualTo(0);
        }
        finally
        {
            await resetClient.ShutdownAsync().ConfigureAwait(false);
        }
    }

    [Test]
    public async Task Mcp_crud_round_trip_updates_shared_dx_engine_via_simulation_module()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        SimDxClient client = host.Simulation.GetModule<SimDxModule>().Client
            ?? throw new InvalidOperationException("Simulation DX module was not registered.");

        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);
        OpcSessionDto connected = await host.CallToolAsync<OpcSessionDto>(
            "opcclassic.dx.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("dx"),
            }).ConfigureAwait(false);
        await Assert.That(connected.DaConnected).IsTrue();

        const string connectionName = "McpSharedMirror";
        OpcDxConnectionDto addConnection = CreateMcpConnection(connectionName, updateRateMilliseconds: 100);
        OpcResultDto added = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.add_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connection"] = addConnection,
            }).ConfigureAwait(false);
        await Assert.That(added.Succeeded).IsTrue();
        await Assert.That(added.ItemName).IsEqualTo(connectionName);

        DxTransferSnapshot addedRuntime = await WaitForConnectionSnapshotAsync(
            client,
            connectionName,
            snapshot => snapshot.ReadCount >= 1).ConfigureAwait(false);
        await Assert.That(addedRuntime.State).IsEqualTo(DxTransferState.Running);

        DxConfigurationSnapshot afterAdd = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
        await Assert.That(afterAdd.Configuration.Connections.Any(connection => connection.Name == connectionName))
            .IsTrue();

        OpcDxConnectionDto disabledConnection = addConnection with
        {
            Description = "disabled by MCP",
            DefaultSourceItemConnected = false,
            DefaultTargetItemConnected = false,
        };
        OpcResultDto modified = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.modify_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connection"] = disabledConnection,
            }).ConfigureAwait(false);
        await Assert.That(modified.Succeeded).IsTrue();

        DxTransferSnapshot disabledRuntime = await WaitForConnectionSnapshotAsync(
            client,
            connectionName,
            snapshot => snapshot.State == DxTransferState.Disabled).ConfigureAwait(false);
        await Assert.That(disabledRuntime.State).IsEqualTo(DxTransferState.Disabled);

        int maskBeforePartialUpdate = (await client.Engine.GetConfigurationAsync().ConfigureAwait(false))
            .Configuration.Connections.Single(connection => connection.Name == connectionName).Mask;
        var enabledConnection = new OpcDxConnectionDto(
            Description: "must not replace stored fields",
            DefaultSourceItemConnected: true,
            DefaultTargetItemConnected: true,
            UpdateRateMilliseconds: 150,
            DeadbandPercent: 99,
            Mask: (int)(
                DxMask.DefaultSourceItemConnected |
                DxMask.DefaultTargetItemConnected |
                DxMask.UpdateRate));
        OpcResultDto updated = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.update_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["browsePath"] = "Plant.Reactor1",
                ["connectionName"] = connectionName,
                ["connection"] = enabledConnection,
            }).ConfigureAwait(false);
        await Assert.That(updated.Succeeded).IsTrue();

        DxTransferSnapshot runningRuntime = await WaitForConnectionSnapshotAsync(
            client,
            connectionName,
            snapshot => snapshot.State == DxTransferState.Running &&
                snapshot.ReadCount >= 1).ConfigureAwait(false);
        await Assert.That(runningRuntime.State).IsEqualTo(DxTransferState.Running);

        DxConnection afterPartialUpdate = (await client.Engine.GetConfigurationAsync().ConfigureAwait(false))
            .Configuration.Connections.Single(connection => connection.Name == connectionName);
        await Assert.That(afterPartialUpdate.Description).IsEqualTo("disabled by MCP");
        await Assert.That(afterPartialUpdate.DeadbandPercent).IsEqualTo(0.5f);
        await Assert.That(afterPartialUpdate.TargetItemName).IsEqualTo("Int4");
        await Assert.That(afterPartialUpdate.Mask).IsEqualTo(maskBeforePartialUpdate);

        OpcResultDto deleted = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.delete_connection",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["browsePath"] = "Plant.Reactor1",
                ["connectionName"] = connectionName,
            }).ConfigureAwait(false);
        await Assert.That(deleted.Succeeded).IsTrue();

        DxConfigurationSnapshot afterDelete = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
        await Assert.That(afterDelete.Configuration.Connections.Any(connection => connection.Name == connectionName))
            .IsFalse();

        InvalidOperationException? staleResetError = null;
        try
        {
            _ = await host.CallToolAsync<OpcResultDto>(
                "opcclassic.dx.reset_configuration",
                new Dictionary<string, object>
                {
                    ["sessionId"] = sessionId,
                    ["configurationVersion"] = "3",
                }).ConfigureAwait(false);
        }
        catch (InvalidOperationException ex)
        {
            staleResetError = ex;
        }

        ArgumentNullException.ThrowIfNull(staleResetError);
        await Assert.That(staleResetError.Message)
            .Contains("opcclassic.dx.reset_configuration");

        DxConfigurationSnapshot afterStaleReset =
            await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
        await Assert.That(afterStaleReset.Version).IsEqualTo(4);
        await Assert.That(afterStaleReset.Configuration.SourceServers.Length).IsEqualTo(1);

        OpcResultDto reset = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.dx.reset_configuration",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["configurationVersion"] = "4",
            }).ConfigureAwait(false);
        await Assert.That(reset.Succeeded).IsTrue();
        await Assert.That(reset.ValueType).IsEqualTo("5");

        DxConfigurationSnapshot afterReset = await client.Engine.GetConfigurationAsync().ConfigureAwait(false);
        await Assert.That(afterReset.Version).IsEqualTo(5);
        await Assert.That(afterReset.Configuration.Connections.Length).IsEqualTo(0);
        await Assert.That(client.Engine.GetStatusSnapshot().Connections.Length).IsEqualTo(0);
    }

    private static async Task<SimDxClient> CreateClientAsync(
        SimulatedPlantModel? model = null,
        IDxConfigurationStore? store = null,
        ManualDxScheduler? scheduler = null)
    {
        model ??= new SimulatedPlantModel();
        scheduler ??= new ManualDxScheduler(model.StartTimeUtc);
        return await SimDxClient.CreateAsync(model, store, scheduler, CreateOptions()).ConfigureAwait(false);
    }

    private static DxReferenceEngineOptions CreateOptions() => new()
    {
        DefaultUpdateRate = TimeSpan.FromMilliseconds(50),
        MaximumUpdateRate = TimeSpan.FromSeconds(2),
        InitialRetryDelay = TimeSpan.FromMilliseconds(50),
        MaximumRetryDelay = TimeSpan.FromMilliseconds(200),
    };

    private static DxConnection CreateDxConnection(
        string name,
        string targetItemName,
        int updateRateMilliseconds,
        bool connected = true) =>
        new(
            name: name,
            description: name + " integration mirror",
            itemPath: "Plant.Reactor1",
            itemName: name,
            version: "simdx-integration",
            browsePaths: ["Plant", "Plant.Reactor1"],
            keyword: "temperature",
            defaultSourceItemConnected: connected,
            defaultTargetItemConnected: connected,
            targetItemPath: "Bucket Brigade",
            targetItemName: targetItemName,
            sourceServerName: "SimulationDA",
            sourceItemPath: "Plant.Reactor1",
            sourceItemName: "Temperature",
            sourceItemQueueSize: 4,
            updateRateMilliseconds: updateRateMilliseconds,
            deadbandPercent: 0.5f,
            vendorData: "simdx-integration");

    private static OpcDxConnectionDto CreateMcpConnection(
        string name,
        int updateRateMilliseconds,
        bool connected = true) =>
        new(
            Name: name,
            Description: name + " integration mirror",
            ItemPath: "Plant.Reactor1",
            ItemName: name,
            Version: "simdx-integration",
            BrowsePaths: ["Plant", "Plant.Reactor1"],
            Keyword: "temperature",
            DefaultSourceItemConnected: connected,
            DefaultTargetItemConnected: connected,
            TargetItemPath: "Bucket Brigade",
            TargetItemName: "Int4",
            SourceServerName: "SimulationDA",
            SourceItemPath: "Plant.Reactor1",
            SourceItemName: "Temperature",
            SourceItemQueueSize: 4,
            UpdateRateMilliseconds: updateRateMilliseconds,
            DeadbandPercent: 0.5f,
            VendorData: "simdx-integration");

    private static async Task<DxTransferSnapshot> WaitForConnectionSnapshotAsync(
        SimDxClient client,
        string connectionName,
        Func<DxTransferSnapshot, bool> predicate)
    {
        for (int attempt = 0; attempt < 200; attempt++)
        {
            DxTransferSnapshot snapshot = client.Engine.GetStatusSnapshot().Connections.Single(
                connection => connection.ConnectionName == connectionName);
            if (predicate(snapshot))
            {
                return snapshot;
            }

            await Task.Delay(10).ConfigureAwait(false);
        }

        throw new TimeoutException($"Timed out waiting for DX connection '{connectionName}'.");
    }
}
