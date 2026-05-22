// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors

using OpcClassic;
using OpcClassic.Da;
using OpcClassic.Da.Ndr;
using OpcClassic.Ndr;

var url = OpcUrl.Parse("opcda://localhost/Matrikon.OPC.Simulation.1");
Console.WriteLine($"Parsed URL: scheme={url.Scheme}, host={url.Host}, server={url.ServerId}");

var variant = OpcVariant.FromInt32(42);
Console.WriteLine($"Variant: type={variant.Type}, value={variant.AsInt32()}");

var state = new OpcItemState(
    ClientHandle: 7,
    Timestamp: DateTimeOffset.UtcNow,
    Quality: OpcQuality.Good,
    Value: OpcVariant.FromDouble(3.14));

Span<byte> buffer = stackalloc byte[256];
var writer = new NdrWriter(buffer);
NdrOpcItemStateCodec.Write(ref writer, state);
int wrote = writer.Position;

var reader = new NdrReader(buffer[..wrote]);
var roundTripped = NdrOpcItemStateCodec.Read(ref reader);
Console.WriteLine($"Round-tripped OpcItemState: clientHandle={roundTripped.ClientHandle}, value={roundTripped.Value.AsDouble()}");

Console.WriteLine("AOT canary OK");
return 0;
