// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Commands.Dcom;

namespace Opc.Classic.Commands.Tests.Dcom;

public sealed class OpcCommandsSpecCatalogTests
{
    [Test]
    public async Task Commands_returns_prebind_iids_in_expected_order()
    {
        Guid[] expected =
        {
            IOPCCommandInformation.InterfaceId,
            IOPCCommandExecution.InterfaceId,
        };

        await Assert.That(OpcCommandsSpecCatalog.Commands.Count).IsEqualTo(expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            await Assert.That(OpcCommandsSpecCatalog.Commands[i]).IsEqualTo(expected[i]);
        }
    }
}
