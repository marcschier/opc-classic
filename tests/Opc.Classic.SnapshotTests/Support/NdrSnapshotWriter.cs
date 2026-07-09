// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ndr;

namespace Opc.Classic.SnapshotTests.Support;

internal delegate void NdrWriteAction(ref NdrWriter writer);

internal static class NdrSnapshotWriter
{
    public static byte[] Write(NdrWriteAction write, int capacity = 4096)
    {
        ArgumentNullException.ThrowIfNull(write);

        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer[..writer.Position];
    }
}
