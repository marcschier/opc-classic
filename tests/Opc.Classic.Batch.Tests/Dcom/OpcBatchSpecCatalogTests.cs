// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Batch.Dcom;

namespace Opc.Classic.Batch.Tests.Dcom;

public sealed class OpcBatchSpecCatalogTests
{
    [Test]
    public async Task Batch_returns_prebind_iids_in_expected_order()
    {
        Guid[] expected =
        {
            IOPCBatchServer.InterfaceId,
            IOPCBatchServer2.InterfaceId,
            IOPCEnumerationSets.InterfaceId,
        };

        await Assert.That(OpcBatchSpecCatalog.Batch.Count).IsEqualTo(expected.Length);
        for (int i = 0; i < expected.Length; i++)
        {
            await Assert.That(OpcBatchSpecCatalog.Batch[i]).IsEqualTo(expected[i]);
        }
    }
}
