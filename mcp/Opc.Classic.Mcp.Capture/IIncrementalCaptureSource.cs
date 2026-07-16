// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

internal interface IIncrementalCaptureSource
{
    IAsyncEnumerable<CapturedPacket> ReadFromAsync(
        long packetIndex,
        CancellationToken cancellationToken);
}
