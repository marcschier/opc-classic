//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Shared per-item reader for Read / Subscribe / SubscriptionPolledRefresh
// responses — they all carry the same <Items> shape (Name + Handle +
// Timestamp + Value + Quality + ResultID).
//

using System;
using System.Globalization;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

internal static class ItemValueReader {
    public static XmlDaItemValueResult ReadOneItem(XmlReader r) {
        string itemName = r.GetAttribute("ItemName") ?? string.Empty;
        string? clientHandle = r.GetAttribute("ClientItemHandle");
        string? timestampAttr = r.GetAttribute("Timestamp");
        string? resultId = r.GetAttribute("ResultID");

        DateTimeOffset? timestamp = null;
        if (!string.IsNullOrEmpty(timestampAttr) &&
            DateTimeOffset.TryParse(timestampAttr, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var parsedTs)) {
            timestamp = parsedTs;
        }

        XmlDaValue? value = null;
        var quality = new OpcQuality((ushort)OpcQualityKind.Good);

        if (r.IsEmptyElement) {
            return new XmlDaItemValueResult(itemName, clientHandle, value, quality, timestamp, resultId);
        }

        ReadItemChildren(r, ref value, ref quality);
        return new XmlDaItemValueResult(itemName, clientHandle, value, quality, timestamp, resultId);
    }

    private static void ReadItemChildren(XmlReader r, ref XmlDaValue? value, ref OpcQuality quality) {
        int itemDepth = r.Depth;
        bool alreadyAdvanced = false;
        while (true) {
            if (!alreadyAdvanced) {
                if (!r.Read()) { break; }
            }
            alreadyAdvanced = false;
            if (r.Depth <= itemDepth) { break; }
            if (r.NodeType != XmlNodeType.Element) { continue; }

            if (string.Equals(r.LocalName, "Value", StringComparison.Ordinal)) {
                value = ReadValue(r);
                alreadyAdvanced = true;
            }
            else if (string.Equals(r.LocalName, "Quality", StringComparison.Ordinal)) {
                quality = ReadQuality(r);
            }
            else {
                r.Skip();
                alreadyAdvanced = true;
            }
        }
    }

    private static XmlDaValue ReadValue(XmlReader r) => XmlDaValueSerializer.ReadValue(r);

    private static OpcQuality ReadQuality(XmlReader r) {
        string? field = r.GetAttribute("QualityField");
        string? limit = r.GetAttribute("LimitField");
        ushort raw = 0;
        if (!string.IsNullOrEmpty(field)) {
            raw |= ParseQualityKind(field);
        }
        if (!string.IsNullOrEmpty(limit)) {
            raw |= ParseQualityLimit(limit);
        }
        if (!r.IsEmptyElement) { r.Skip(); }
        return new OpcQuality(raw);
    }

    private static ushort ParseQualityKind(string value) => value switch {
        "good" or "goodNonSpecific" => (ushort)OpcQualityKind.Good,
        "uncertain" => (ushort)OpcQualityKind.Uncertain,
        "bad" => (ushort)OpcQualityKind.Bad,
        _ => (ushort)OpcQualityKind.Bad,
    };

    private static ushort ParseQualityLimit(string value) => value switch {
        "none" => (ushort)((int)OpcQualityLimit.NotLimited << 6),
        "low" => (ushort)((int)OpcQualityLimit.Low << 6),
        "high" => (ushort)((int)OpcQualityLimit.High << 6),
        "constant" => (ushort)((int)OpcQualityLimit.Constant << 6),
        _ => 0,
    };
}
