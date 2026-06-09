//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;
using TUnit.Core;

namespace Opc.Classic.Mcp.Tests;

public sealed class SecurityToolsTests {
    [Test]
    public async Task Security_availability_round_trips_via_mcp_client() {
        var security = new SyntheticSecurityClient(supportsNt: true, supportsPrivate: false);
        await using McpTestServer server = await McpTestServer.CreateAsync(services => {
            services.AddSingleton<IOpcSecurityClientFactory>(new SyntheticSecurityClientFactory(security));
        }).ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcSecurityInfoDto nt = await server.CallToolAsync<OpcSecurityInfoDto>(
            "opcclassic.security.is_available_nt",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcSecurityInfoDto priv = await server.CallToolAsync<OpcSecurityInfoDto>(
            "opcclassic.security.is_available_private",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(nt.SupportsWindowsAuthentication).IsTrue();
        await Assert.That(nt.SupportsPrivateAuthentication).IsFalse();
        await Assert.That(priv.SupportsPrivateAuthentication).IsFalse();
        await Assert.That(nt.IsAuthenticated).IsFalse();
    }

    [Test]
    public async Task Security_logon_and_logoff_round_trip_via_mcp_client() {
        var security = new SyntheticSecurityClient(supportsNt: false, supportsPrivate: true);
        await using McpTestServer server = await McpTestServer.CreateAsync(services => {
            services.AddSingleton<IOpcSecurityClientFactory>(new SyntheticSecurityClientFactory(security));
        }).ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcResultDto failed = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.security.logon",
            new Dictionary<string, object> {
                ["sessionId"] = session.SessionId,
                ["username"] = "operator",
                ["password"] = "wrong",
            }).ConfigureAwait(false);
        OpcResultDto loggedOn = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.security.logon",
            new Dictionary<string, object> {
                ["sessionId"] = session.SessionId,
                ["username"] = "operator",
                ["password"] = "correct",
            }).ConfigureAwait(false);
        OpcResultDto loggedOff = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.security.logoff",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(failed.Succeeded).IsFalse();
        await Assert.That(loggedOn.Succeeded).IsTrue();
        await Assert.That(loggedOn.ItemName).IsEqualTo("private:operator");
        await Assert.That(loggedOff.Succeeded).IsTrue();
        await Assert.That(security.IsAuthenticated).IsFalse();
    }
}

internal sealed class SyntheticSecurityClientFactory : IOpcSecurityClientFactory {
    private readonly SyntheticSecurityClient _client;

    public SyntheticSecurityClientFactory(SyntheticSecurityClient client) => _client = client;

    public Task<SecurityClientState> CreateAsync(OpcSession session, CancellationToken cancellationToken = default) {
        _ = session;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new SecurityClientState(_client));
    }
}

internal sealed class SyntheticSecurityClient : IOpcSecurityClient {
    private readonly bool _supportsNt;
    private readonly bool _supportsPrivate;

    public SyntheticSecurityClient(bool supportsNt, bool supportsPrivate) {
        _supportsNt = supportsNt;
        _supportsPrivate = supportsPrivate;
    }

    public bool IsAuthenticated { get; private set; }

    public string CurrentIdentity { get; private set; } = string.Empty;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task<bool> IsAvailableNtAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_supportsNt);
    }

    public Task<bool> IsAvailablePrivateAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_supportsPrivate);
    }

    public Task<bool> LogonPrivateAsync(string username, string password, CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_supportsPrivate || password != "correct") {
            return Task.FromResult(false);
        }

        IsAuthenticated = true;
        CurrentIdentity = "private:" + username;
        return Task.FromResult(true);
    }

    public Task LogoffAsync(CancellationToken cancellationToken = default) {
        cancellationToken.ThrowIfCancellationRequested();
        IsAuthenticated = false;
        CurrentIdentity = string.Empty;
        return Task.CompletedTask;
    }
}
