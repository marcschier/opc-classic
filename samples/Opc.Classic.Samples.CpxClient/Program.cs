// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Samples.CpxClient;
using Opc.Classic.Samples.CpxServer;

var report = await CpxClientDemo.RunAsync(new CpxSampleServer(), Console.Out).ConfigureAwait(false);
Console.WriteLine(
    $"CPX sample complete: browsed={report.BrowsedItems.Count}, decoded={report.DecodedItems.Count}, "
    + $"invalid={report.InvalidPayloads.Count}, unsupported={report.UnsupportedTypeSystems.Count}");
return 0;
