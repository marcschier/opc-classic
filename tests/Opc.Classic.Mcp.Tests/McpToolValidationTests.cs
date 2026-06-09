//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;
using System.Text.Json;
using ModelContextProtocol;
using Opc.Classic.Discovery;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Mcp.Tests;

public sealed class McpToolValidationTests {
    [Test]
    public async Task DiscoveryTools_EnumerateServers_Uses_trimmed_host_and_projects_entries() {
        var classId = Guid.Parse("10138C2C-0000-0000-0000-00000000C001");
        var entry = new OpcServerEntry(
            classId,
            "Vendor.Projected.1",
            "Vendor Projected",
            "resolved-host",
            [OpcGuids.CATID_OPCDAServer20]);
        var discovery = new RecordingDiscovery(entry);
        var tools = new DiscoveryTools([discovery]);

        IReadOnlyList<OpcServerDescriptorDto> descriptors = await tools.EnumerateServers(
            "  opc-host  ",
            [OpcGuids.CATID_OPCDAServer20.ToString("D", CultureInfo.InvariantCulture)],
            cancellationToken: CancellationToken.None);

        await Assert.That(discovery.LastHost).IsEqualTo("opc-host");
        await Assert.That(descriptors.Count).IsEqualTo(1);
        await Assert.That(descriptors[0].ClassId).IsEqualTo(classId);
        await Assert.That(descriptors[0].ProgId).IsEqualTo("Vendor.Projected.1");
        await Assert.That(descriptors[0].UserType).IsEqualTo("Vendor Projected");
        await Assert.That(descriptors[0].VerIndProgId).IsNull();
        await Assert.That(descriptors[0].Categories.Count).IsEqualTo(1);
        await Assert.That(descriptors[0].Categories[0]).IsEqualTo(OpcGuids.CATID_OPCDAServer20);
        await Assert.That(descriptors[0].Host).IsEqualTo("resolved-host");
    }

    [Test]
    public async Task DiscoveryTools_EnumerateServers_Invalid_category_id_throws_argument_exception() {
        var tools = new DiscoveryTools([new RecordingDiscovery()]);

        await Assert.That(async () => await tools.EnumerateServers(
            categoryIds: ["not-a-guid"],
            cancellationToken: CancellationToken.None)).Throws<ArgumentException>();
    }

    [Test]
    public async Task SecurityTools_IsAvailableNt_Without_da_or_factory_reports_required_connection() {
        using var sessionManager = new OpcSessionManager();
        OpcSession session = sessionManager.CreateSession();
        var tools = new SecurityTools(sessionManager, []);

        Exception exception = await CaptureAsync(async () => await tools.IsAvailableNt(session.SessionId, CancellationToken.None));

        await Assert.That(exception is McpException).IsTrue();
        await Assert.That(exception.Message).IsEqualTo("OPC Security tools require an existing DCOM client in the session. Connect DA first or register IOpcSecurityClientFactory.");
    }

    [Test]
    public async Task DaClientTools_Connect_Uses_injected_factory_and_shapes_session_dto() {
        using var sessionManager = new OpcSessionManager();
        OpcSession session = sessionManager.CreateSession();
        var factory = new CapturingDaConnectionFactory();
        var tools = new DaClientTools(sessionManager, [factory]);

        OpcSessionDto dto = await tools.Connect(
            session.SessionId,
            host: "  requested-host  ",
            progId: "Requested.Prog.1",
            clsid: "10138C2C-0000-0000-0000-00000000C002",
            username: "DOMAIN\\operator",
            password: "secret",
            useKerberos: true,
            connectionString: "dcom://wire-host/Wire.Prog.1",
            useSso: true,
            authLevel: "pkt_privacy",
            cancellationToken: CancellationToken.None);

        await Assert.That(factory.LastRequest).IsNotNull();
        await Assert.That(factory.LastRequest!.Host).IsEqualTo("  requested-host  ");
        await Assert.That(factory.LastRequest.ProgId).IsEqualTo("Requested.Prog.1");
        await Assert.That(factory.LastRequest.Clsid).IsEqualTo("10138C2C-0000-0000-0000-00000000C002");
        await Assert.That(factory.LastRequest.Username).IsEqualTo("DOMAIN\\operator");
        await Assert.That(factory.LastRequest.Password).IsEqualTo("secret");
        await Assert.That(factory.LastRequest.UseKerberos).IsTrue();
        await Assert.That(factory.LastRequest.ConnectionString).IsEqualTo("dcom://wire-host/Wire.Prog.1");
        await Assert.That(factory.LastRequest.UseSso).IsTrue();
        await Assert.That(factory.LastRequest.AuthLevel).IsEqualTo("pkt_privacy");
        await Assert.That(dto.SessionId).IsEqualTo(session.SessionId);
        await Assert.That(dto.DaConnected).IsTrue();
        await Assert.That(dto.DaHost).IsEqualTo("factory-host");
        await Assert.That(dto.DaProgId).IsEqualTo("Factory.Prog.1");
        await Assert.That(dto.DaClsid).IsEqualTo(CapturingDaConnectionFactory.FactoryClassId);
    }

    [Test]
    public async Task DaClientTools_WriteSync_Mismatched_value_count_throws_argument_exception_before_session_lookup() {
        using var sessionManager = new OpcSessionManager();
        var tools = new DaClientTools(sessionManager, []);
        JsonElement[] values =
        [
            JsonSerializer.SerializeToElement(1),
            JsonSerializer.SerializeToElement(2),
        ];

        await Assert.That(async () => await tools.WriteSync(
            "missing-session",
            groupHandle: 10,
            serverHandles: [100],
            values,
            CancellationToken.None)).Throws<ArgumentException>();
    }

    private static async Task<Exception> CaptureAsync(Func<Task> action) {
        try {
            await action();
        }
        catch (Exception ex) {
            return ex;
        }

        throw new InvalidOperationException("Expected an exception.");
    }

    private sealed class RecordingDiscovery : IOpcDiscovery {
        private readonly IReadOnlyList<OpcServerEntry> _entries;

        public RecordingDiscovery(params OpcServerEntry[] entries) => _entries = entries;

        public string? LastHost { get; private set; }

        public async IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
            string? host = null,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default) {
            LastHost = host;
            await Task.CompletedTask;
            foreach (OpcServerEntry entry in _entries) {
                cancellationToken.ThrowIfCancellationRequested();
                yield return entry;
            }
        }
    }

    private sealed class CapturingDaConnectionFactory : IOpcDaConnectionFactory {
        public static readonly Guid FactoryClassId = Guid.Parse("10138C2C-0000-0000-0000-00000000C003");

        private readonly SyntheticDaServer _server = new();

        public DaConnectionRequest? LastRequest { get; private set; }

        public Task<DaClientState> ConnectAsync(DaConnectionRequest request, CancellationToken cancellationToken = default) {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;
            return Task.FromResult(new DaClientState("factory-host", "Factory.Prog.1", FactoryClassId, _server.Channel, ownsChannel: false));
        }
    }
}
