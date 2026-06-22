// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// OPC DA quality constants used as default starting quality.
/// </summary>
internal static class OpcDaItemQuality
{
    public const ushort GoodNonSpecific = 0x00C0;
    public const ushort UncertainNonSpecific = 0x0040;
    public const ushort BadNonSpecific = 0x0000;
}
