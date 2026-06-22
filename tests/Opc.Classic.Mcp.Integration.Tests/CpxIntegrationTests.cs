// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Cpx;
using Opc.Classic.Mcp.Dtos;

namespace Opc.Classic.Mcp.Integration.Tests;

public sealed class CpxIntegrationTests
{
    [Test]
    public async Task Cpx_tools_get_complex_type_for_simulation_da_item()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync();
        string sessionId = await host.CreateSessionAsync();
        _ = await host.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("cpx"),
            });

        OpcComplexTypeDto complexType = await host.CallToolAsync<OpcComplexTypeDto>(
            "opcclassic.cpx.get_complex_type",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["itemId"] = "Plant.Reactor1.Packet",
            });

        await Assert.That(complexType.ItemId).IsEqualTo("Plant.Reactor1.Packet");
        await Assert.That(complexType.TypeId).IsEqualTo(new Guid("f1ca2a57-9f4d-4c6f-b761-a8f8fbbef101"));
        await Assert.That(complexType.DictionaryId).IsEqualTo("SampleDictionary");
        await Assert.That(complexType.TypeItemId).IsEqualTo("Types.ReactorSnapshot");
        await Assert.That(complexType.UnconvertedItemId).IsEqualTo("Plant.Reactor1.Packet.Raw");
        await Assert.That(complexType.DataFilter).IsEqualTo("Engineering");
        await Assert.That(complexType.AvailableFilters).Contains("Raw");
        await Assert.That(complexType.AvailableFilters).Contains("Engineering");
        await Assert.That(complexType.AvailableFilters).Contains("Compact");
    }

    [Test]
    public async Task Cpx_tools_get_type_system_and_dictionary_from_simulation_server()
    {
        await using SimulationMcpHost host = await SimulationMcpHost.CreateAsync();
        string sessionId = await host.CreateSessionAsync();
        _ = await host.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["connectionString"] = host.ConnectionString("cpx"),
            });

        OpcTypeSystemDto typeSystem = await host.CallToolAsync<OpcTypeSystemDto>(
            "opcclassic.cpx.get_type_system",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["typeSystemId"] = "binary",
            });
        OpcTypeDictionaryDto dictionary = await host.CallToolAsync<OpcTypeDictionaryDto>(
            "opcclassic.cpx.get_dictionary",
            new Dictionary<string, object>
            {
                ["sessionId"] = sessionId,
                ["dictionaryId"] = "SampleDictionary",
            });

        await Assert.That(typeSystem.TypeSystemId).IsEqualTo(TypeDictionary.OpcBinaryTypeSystemId);
        await Assert.That(typeSystem.Supported).IsTrue();
        await Assert.That(typeSystem.NamespacePath).IsEqualTo("/CPX/OPCBinary");
        await Assert.That(dictionary.DictionaryId).IsEqualTo("SampleDictionary");
        await Assert.That(dictionary.TypeSystemId).IsEqualTo(TypeDictionary.OpcBinaryTypeSystemId);
        await Assert.That(dictionary.Name).IsEqualTo("SimulationComplexTypes");
        await Assert.That(dictionary.ParseError).IsNull();
        await Assert.That(dictionary.Types.Select(static type => type.TypeId)).Contains("ReactorSnapshot");
        await Assert.That(dictionary.Types.Select(static type => type.TypeId)).Contains("BatchTransferRecord");

        OpcComplexTypeDescriptionDto reactor = dictionary.Types.Single(static type => type.TypeId == "ReactorSnapshot");
        await Assert.That(reactor.Fields.Select(static field => field.Name)).Contains("Unit");
        await Assert.That(reactor.Fields.Select(static field => field.Name)).Contains("Sequence");
        await Assert.That(reactor.Fields.Select(static field => field.Name)).Contains("Temperature");
        await Assert.That(reactor.Fields.Select(static field => field.Name)).Contains("Pressure");
        await Assert.That(reactor.Fields.Select(static field => field.Name)).Contains("Quality");
    }
}
