// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using ModelContextProtocol.Client;

namespace Opc.Classic.Mcp.Integration.Tests;

/// <summary>
/// Baseline smoke tests that verify the MCP host boots against the full simulation,
/// exposes the expected tool surface, and that a session can be created. Per-spec
/// end-to-end behavior is covered by the feature-area test classes.
/// </summary>
public sealed class SimulationHostSmokeTests
{
    [Test]
    public async Task Host_starts_and_creates_a_session()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);

        string sessionId = await host.CreateSessionAsync().ConfigureAwait(false);

        await Assert.That(sessionId).IsNotNull();
    }

    [Test]
    public async Task Host_exposes_the_full_mcp_tool_surface()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);

        IList<McpClientTool> tools = await host.Client.ListToolsAsync().ConfigureAwait(false);

        await Assert.That(tools.Any(static t => t.Name == "opcclassic.session.create")).IsTrue();
        await Assert.That(tools.Any(static t => t.Name == "opcclassic.da.browse")).IsTrue();
    }

    [Test]
    public async Task Simulation_model_seeds_a_non_empty_address_space()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync().ConfigureAwait(false);

        await Assert.That(host.Model.Tags.Count).IsGreaterThan(0);
    }
}
