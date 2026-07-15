// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Hosting;
using Opc.Classic.Samples.CpxServer;

var server = new CpxSampleServer();
OpcBrowseResult root = await server.BrowseAsync(null, OpcBrowseElementKind.All).ConfigureAwait(false);

Console.WriteLine("Opc.Classic managed CPX sample server");
Console.WriteLine($"Branches: {string.Join(", ", root.Branches)}");
foreach (string itemId in server.ItemIds)
{
    Console.WriteLine($"Item: {itemId}");
}

Console.WriteLine("Read-only DA groups are intentionally unsupported; use the CPX client sample for discovery and decode.");
return 0;
