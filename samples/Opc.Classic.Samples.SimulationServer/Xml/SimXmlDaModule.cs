// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Mcp.Tools;

namespace Opc.Classic.Samples.SimulationServer.Xml;

/// <summary>
/// XML-DA feature-area module that registers an in-memory client over the shared simulation model.
/// </summary>
public sealed class SimXmlDaModule : ISimulationModule
{
    /// <summary>
    /// Creates the XML-DA simulation module.
    /// </summary>
    public SimXmlDaModule()
    {
    }

    /// <inheritdoc />
    public string Spec => "xmlda";

    /// <inheritdoc />
    public SimulationConnection? Register(SimulationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var client = new SimXmlDaClient(context.Model);
        IDisposable registration = InMemoryXmlDaConnectionRegistry.Register(context.ChannelName("xmlda"), client);
        return new SimulationConnection("xmlda", context.ConnectionString("xmlda"), registration);
    }
}
