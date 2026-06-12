//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Tools;

internal static class OpcMcpAuthLevel
{
    public const string Description = "Optional DCOM RPC authentication level: default, connect, call, packet, pkt_integrity, or pkt_privacy. Use pkt_integrity for hardened Windows DCOM.";

    public static bool IsSpecified(string? authLevel) => !string.IsNullOrWhiteSpace(authLevel);

    public static OpcProtectionLevel ParseOrDefault(string? authLevel)
    {
        if (string.IsNullOrWhiteSpace(authLevel))
        {
            return OpcProtectionLevel.Integrity;
        }

        string normalized = authLevel.Trim().Replace("-", string.Empty, StringComparison.Ordinal).Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        if (int.TryParse(normalized, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int numeric)
            && Enum.IsDefined(typeof(OpcProtectionLevel), numeric))
        {
            OpcProtectionLevel numericLevel = (OpcProtectionLevel)numeric;
            return numericLevel == OpcProtectionLevel.Default ? OpcProtectionLevel.Integrity : numericLevel;
        }

        return normalized switch
        {
            "default" => OpcProtectionLevel.Integrity,
            "none" => OpcProtectionLevel.None,
            "connect" => OpcProtectionLevel.Connect,
            "call" => OpcProtectionLevel.Call,
            "packet" or "pkt" => OpcProtectionLevel.Packet,
            "integrity" or "pktintegrity" or "packetintegrity" => OpcProtectionLevel.Integrity,
            "privacy" or "pktprivacy" or "packetprivacy" => OpcProtectionLevel.Privacy,
            _ => throw new ArgumentException($"Unsupported DCOM auth level '{authLevel}'. Use default, connect, call, packet, pkt_integrity, or pkt_privacy.", nameof(authLevel)),
        };
    }
}
