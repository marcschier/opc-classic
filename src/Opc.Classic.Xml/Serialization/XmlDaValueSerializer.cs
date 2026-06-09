//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace Opc.Classic.Xml.Serialization;

internal static class XmlDaValueSerializer {
    public static void WriteValueElement(XmlWriter writer, XmlDaValue value) {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartElement("Value", XmlDaConstants.XmlDaNamespace);
        WriteValueType(writer, value.Type);
        WriteValueContent(writer, value);
        writer.WriteEndElement();
    }

    public static XmlDaValue ReadValue(XmlReader reader) {
        ArgumentNullException.ThrowIfNull(reader);

        string xsiType = reader.GetAttribute("type", XmlDaConstants.XsiNamespace) ?? string.Empty;
        string localType = GetLocalType(xsiType);

        return localType switch {
            "ArrayOfByte" => XmlDaValue.OfArrayOfByte(ReadArray(reader, XmlConvert.ToSByte)),
            "ArrayOfShort" => XmlDaValue.OfArrayOfShort(ReadArray(reader, XmlConvert.ToInt16)),
            "ArrayOfInt" => XmlDaValue.OfArrayOfInt(ReadArray(reader, XmlConvert.ToInt32)),
            "ArrayOfLong" => XmlDaValue.OfArrayOfLong(ReadArray(reader, XmlConvert.ToInt64)),
            "ArrayOfFloat" => XmlDaValue.OfArrayOfFloat(ReadArray(reader, XmlConvert.ToSingle)),
            "ArrayOfDouble" => XmlDaValue.OfArrayOfDouble(ReadArray(reader, XmlConvert.ToDouble)),
            "ArrayOfString" => XmlDaValue.OfArrayOfString(ReadStringArray(reader)),
            "ArrayOfBool" or "ArrayOfBoolean" => XmlDaValue.OfArrayOfBoolean(ReadArray(reader, XmlConvert.ToBoolean)),
            "ArrayOfDateTime" => XmlDaValue.OfArrayOfDateTime(ReadArray(reader, static text =>
                DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind))),
            _ => ReadScalarValue(reader, localType),
        };
    }

    private static XmlDaValue ReadScalarValue(XmlReader reader, string localType) {
        string content = reader.ReadElementContentAsString();
        return localType switch {
            "string" => XmlDaValue.OfString(content),
            "byte" => XmlDaValue.OfInt8(XmlConvert.ToSByte(content)),
            "unsignedByte" => XmlDaValue.OfUInt8(XmlConvert.ToByte(content)),
            "short" => XmlDaValue.OfInt16(XmlConvert.ToInt16(content)),
            "unsignedShort" => XmlDaValue.OfUInt16(XmlConvert.ToUInt16(content)),
            "int" or "integer" => XmlDaValue.OfInt32(XmlConvert.ToInt32(content)),
            "unsignedInt" => XmlDaValue.OfUInt32(XmlConvert.ToUInt32(content)),
            "long" => XmlDaValue.OfInt64(XmlConvert.ToInt64(content)),
            "unsignedLong" => XmlDaValue.OfUInt64(XmlConvert.ToUInt64(content)),
            "float" => XmlDaValue.OfSingle(XmlConvert.ToSingle(content)),
            "double" => XmlDaValue.OfDouble(XmlConvert.ToDouble(content)),
            "decimal" => XmlDaValue.OfDecimal(XmlConvert.ToDecimal(content)),
            "boolean" => XmlDaValue.OfBoolean(XmlConvert.ToBoolean(content)),
            "dateTime" => XmlDaValue.OfDateTime(
                DateTimeOffset.Parse(content, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)),
            "time" => XmlDaValue.OfParsedTime(content, ParseTime(content)),
            "date" => XmlDaValue.OfParsedDate(content, ParseDate(content)),
            "duration" => XmlDaValue.OfDuration(XmlConvert.ToTimeSpan(content)),
            "QName" => XmlDaValue.OfParsedQName(content, ResolveQNameNamespace(reader, content)),
            "base64Binary" => XmlDaValue.OfBase64Binary(Convert.FromBase64String(content)),
            _ => XmlDaValue.Unknown(content),
        };
    }

    private static void WriteValueType(XmlWriter writer, XmlDaValueType type) {
        string? xsiType = type switch {
            XmlDaValueType.String => "xsd:string",
            XmlDaValueType.Int8 => "xsd:byte",
            XmlDaValueType.UInt8 => "xsd:unsignedByte",
            XmlDaValueType.Int16 => "xsd:short",
            XmlDaValueType.UInt16 => "xsd:unsignedShort",
            XmlDaValueType.Int32 => "xsd:int",
            XmlDaValueType.UInt32 => "xsd:unsignedInt",
            XmlDaValueType.Int64 => "xsd:long",
            XmlDaValueType.UInt64 => "xsd:unsignedLong",
            XmlDaValueType.Single => "xsd:float",
            XmlDaValueType.Double => "xsd:double",
            XmlDaValueType.Boolean => "xsd:boolean",
            XmlDaValueType.DateTime => "xsd:dateTime",
            XmlDaValueType.Decimal => "xsd:decimal",
            XmlDaValueType.Time => "xsd:time",
            XmlDaValueType.Date => "xsd:date",
            XmlDaValueType.Duration => "xsd:duration",
            XmlDaValueType.QName => "xsd:QName",
            XmlDaValueType.ArrayOfByte => "ArrayOfByte",
            XmlDaValueType.ArrayOfShort => "ArrayOfShort",
            XmlDaValueType.ArrayOfInt => "ArrayOfInt",
            XmlDaValueType.ArrayOfLong => "ArrayOfLong",
            XmlDaValueType.ArrayOfFloat => "ArrayOfFloat",
            XmlDaValueType.ArrayOfDouble => "ArrayOfDouble",
            XmlDaValueType.ArrayOfString => "ArrayOfString",
            XmlDaValueType.ArrayOfBoolean => "ArrayOfBoolean",
            XmlDaValueType.ArrayOfDateTime => "ArrayOfDateTime",
            XmlDaValueType.Base64Binary => "xsd:base64Binary",
            _ => null,
        };

        if (!string.IsNullOrEmpty(xsiType)) {
            writer.WriteAttributeString("type", XmlDaConstants.XsiNamespace, xsiType);
        }
    }

    private static void WriteValueContent(XmlWriter writer, XmlDaValue value) {
        switch (value.Type) {
            case XmlDaValueType.ArrayOfByte:
                WriteArray(writer, "byte", (sbyte[])value.Boxed!, static v => v.ToString(CultureInfo.InvariantCulture));
                break;
            case XmlDaValueType.ArrayOfShort:
                WriteArray(writer, "short", (short[])value.Boxed!, static v => v.ToString(CultureInfo.InvariantCulture));
                break;
            case XmlDaValueType.ArrayOfInt:
                WriteArray(writer, "int", (int[])value.Boxed!, static v => v.ToString(CultureInfo.InvariantCulture));
                break;
            case XmlDaValueType.ArrayOfLong:
                WriteArray(writer, "long", (long[])value.Boxed!, static v => v.ToString(CultureInfo.InvariantCulture));
                break;
            case XmlDaValueType.ArrayOfFloat:
                WriteArray(writer, "float", (float[])value.Boxed!, XmlConvert.ToString);
                break;
            case XmlDaValueType.ArrayOfDouble:
                WriteArray(writer, "double", (double[])value.Boxed!, XmlConvert.ToString);
                break;
            case XmlDaValueType.ArrayOfString:
                WriteStringArray(writer, (string?[])value.Boxed!);
                break;
            case XmlDaValueType.ArrayOfBoolean:
                WriteArray(writer, "boolean", (bool[])value.Boxed!, static v => v ? "true" : "false");
                break;
            case XmlDaValueType.ArrayOfDateTime:
                WriteArray(writer, "dateTime", (DateTimeOffset[])value.Boxed!, static v =>
                    v.UtcDateTime.ToString("o", CultureInfo.InvariantCulture));
                break;
            case XmlDaValueType.Base64Binary:
                writer.WriteString(Convert.ToBase64String((byte[])value.Boxed!));
                break;
            default:
                writer.WriteString(value.RawText);
                break;
        }
    }

    private static void WriteArray<T>(XmlWriter writer, string elementName, T[] values, Func<T, string> format) {
        foreach (T value in values) {
            writer.WriteElementString(elementName, XmlDaConstants.XmlDaNamespace, format(value));
        }
    }

    private static void WriteStringArray(XmlWriter writer, string?[] values) {
        foreach (string? value in values) {
            writer.WriteStartElement("string", XmlDaConstants.XmlDaNamespace);
            if (value is null) {
                writer.WriteAttributeString("nil", XmlDaConstants.XsiNamespace, "true");
            }
            else {
                writer.WriteString(value);
            }
            writer.WriteEndElement();
        }
    }

    private static T[] ReadArray<T>(XmlReader reader, Func<string, T> parse) {
        var values = new List<T>();
        if (reader.IsEmptyElement) {
            reader.Read();
            return values.ToArray();
        }

        int valueDepth = reader.Depth;
        bool alreadyAdvanced = false;
        while (true) {
            if (!alreadyAdvanced && !reader.Read()) {
                break;
            }
            alreadyAdvanced = false;
            if (reader.Depth <= valueDepth) {
                break;
            }
            if (reader.NodeType != XmlNodeType.Element) {
                continue;
            }

            if (IsNil(reader)) {
                reader.Skip();
                alreadyAdvanced = true;
                continue;
            }

            values.Add(parse(reader.ReadElementContentAsString()));
            alreadyAdvanced = true;
        }

        return values.ToArray();
    }

    private static string?[] ReadStringArray(XmlReader reader) {
        var values = new List<string?>();
        if (reader.IsEmptyElement) {
            reader.Read();
            return values.ToArray();
        }

        int valueDepth = reader.Depth;
        bool alreadyAdvanced = false;
        while (true) {
            if (!alreadyAdvanced && !reader.Read()) {
                break;
            }
            alreadyAdvanced = false;
            if (reader.Depth <= valueDepth) {
                break;
            }
            if (reader.NodeType != XmlNodeType.Element) {
                continue;
            }

            if (IsNil(reader)) {
                values.Add(null);
                reader.Skip();
                alreadyAdvanced = true;
                continue;
            }

            values.Add(reader.ReadElementContentAsString());
            alreadyAdvanced = true;
        }

        return values.ToArray();
    }

    private static bool IsNil(XmlReader reader) {
        string? nil = reader.GetAttribute("nil", XmlDaConstants.XsiNamespace);
        return string.Equals(nil, "true", StringComparison.OrdinalIgnoreCase) || string.Equals(nil, "1", StringComparison.Ordinal);
    }

    private static string GetLocalType(string xsiType) {
        int colon = xsiType.LastIndexOf(':');
        return colon >= 0 ? xsiType[(colon + 1)..] : xsiType;
    }

    private static string ResolveQNameNamespace(XmlReader reader, string value) {
        int colon = value.IndexOf(':', StringComparison.Ordinal);
        if (colon <= 0) {
            return string.Empty;
        }

        return reader.LookupNamespace(value[..colon]) ?? string.Empty;
    }

    private static TimeOnly ParseTime(string value) {
        string text = StripTimeZone(value, dateLength: 0);
        return TimeOnly.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    private static DateOnly ParseDate(string value) {
        string text = StripTimeZone(value, dateLength: 10);
        return DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    private static string StripTimeZone(string value, int dateLength) {
        if (value.EndsWith('Z')) {
            return value[..^1];
        }

        int plus = value.LastIndexOf('+');
        int minus = value.LastIndexOf('-');
        int offset = Math.Max(plus, minus);
        return offset > dateLength ? value[..offset] : value;
    }
}
