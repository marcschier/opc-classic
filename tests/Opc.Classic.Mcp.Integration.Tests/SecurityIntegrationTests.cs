// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class SecurityIntegrationTests
{
    [Test]
    public async Task Security_authentication_round_trips_against_full_simulation()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);
        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);

        OpcSecurityInfoDto nt = await host.CallToolAsync<OpcSecurityInfoDto>(
            "opcclassic.security.is_available_nt",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcSecurityInfoDto priv = await host.CallToolAsync<OpcSecurityInfoDto>(
            "opcclassic.security.is_available_private",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcResultDto failed = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.security.logon",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["username"] = "operator",
                ["password"] = "wrong",
            }).ConfigureAwait(false);
        OpcResultDto loggedOn = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.security.logon",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["username"] = "operator",
                ["password"] = "correct",
            }).ConfigureAwait(false);
        OpcSecurityInfoDto authenticated = await host.CallToolAsync<OpcSecurityInfoDto>(
            "opcclassic.security.is_available_nt",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcResultDto loggedOff = await host.CallToolAsync<OpcResultDto>(
            "opcclassic.security.logoff",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);
        OpcSecurityInfoDto afterLogoff = await host.CallToolAsync<OpcSecurityInfoDto>(
            "opcclassic.security.is_available_nt",
            new Dictionary<string, object> { ["sessionId"] = sessionId }).ConfigureAwait(false);

        await Assert.That(nt.SupportsWindowsAuthentication).IsTrue();
        await Assert.That(nt.SupportsPrivateAuthentication).IsTrue();
        await Assert.That(nt.IsAuthenticated).IsFalse();
        await Assert.That(priv.SupportsPrivateAuthentication).IsTrue();
        await Assert.That(failed.Succeeded).IsFalse();
        await Assert.That(loggedOn.Succeeded).IsTrue();
        await Assert.That(loggedOn.ItemName).IsEqualTo("private:operator");
        await Assert.That(authenticated.IsAuthenticated).IsTrue();
        await Assert.That(authenticated.CurrentIdentity).IsEqualTo("private:operator");
        await Assert.That(loggedOff.Succeeded).IsTrue();
        await Assert.That(afterLogoff.IsAuthenticated).IsFalse();
        await Assert.That(afterLogoff.CurrentIdentity).IsEqualTo(string.Empty);
    }
}
