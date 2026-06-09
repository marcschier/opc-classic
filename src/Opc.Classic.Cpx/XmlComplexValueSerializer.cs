//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Opc.Classic.Cpx;

/// <summary>
/// Serializes and deserializes CPX XMLSchema complex values.
/// </summary>
public static class XmlComplexValueSerializer {
    private static readonly XNamespace XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>Serialize a complex value as an XML document.</summary>
    public static string Serialize(ComplexValue value, TypeDescription type, TypeDictionary? dictionary = null) {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(type);

        var root = SerializeElement(type.Name, value, type, dictionary, isRoot: true);
        return new XDocument(root).ToString(SaveOptions.DisableFormatting);
    }

    /// <summary>Deserialize an XML document into a complex value.</summary>
    public static ComplexValue Deserialize(string xml, TypeDescription type, TypeDictionary? dictionary = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);
        ArgumentNullException.ThrowIfNull(type);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, CreateReaderSettings());
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        return DeserializeElement(document.Root ?? throw new FormatException("XML complex value document is empty."), type, dictionary);
    }

    private static XElement SerializeElement(string elementName, ComplexValue value, TypeDescription type, TypeDictionary? dictionary, bool isRoot) {
        var element = new XElement(CreateName(dictionary, elementName));
        if (isRoot) {
            element.SetAttributeValue(XNamespace.Xmlns + "xsi", XsiNamespace.NamespaceName);
            element.SetAttributeValue(XsiNamespace + "type", type.TypeId);
        }

        foreach (var field in type.Fields) {
            if (!value.Fields.TryGetValue(field.Name, out var rawValue)) {
                throw new KeyNotFoundException($"Complex value is missing XML field '{field.Name}'.");
            }

            AppendField(element, field, rawValue, dictionary);
        }

        return element;
    }

    private static void AppendField(XElement parent, TypeField field, object? rawValue, TypeDictionary? dictionary) {
        if (field.ElementCount is { } count && field.Kind != TypeKind.String && rawValue is IEnumerable enumerable and not string) {
            var written = 0;
            foreach (var elementValue in enumerable) {
                parent.Add(CreateFieldElement(field, elementValue, dictionary));
                written++;
            }

            if (written != count) {
                throw new InvalidOperationException($"XML field '{field.Name}' contains {written.ToString(CultureInfo.InvariantCulture)} elements; expected {count.ToString(CultureInfo.InvariantCulture)}.");
            }

            return;
        }

        parent.Add(CreateFieldElement(field, rawValue, dictionary));
    }

    private static XElement CreateFieldElement(TypeField field, object? rawValue, TypeDictionary? dictionary) {
        if (field.Kind == TypeKind.StructReference) {
            var nestedType = OpcBinaryCodecUtilities.ResolveType(field, dictionary);
            var nestedValue = rawValue as ComplexValue
                ?? throw new InvalidCastException($"XML field '{field.Name}' must contain a ComplexValue.");
            return SerializeElement(field.Name, nestedValue, nestedType, dictionary, isRoot: false);
        }

        return new XElement(CreateName(dictionary, field.Name), FormatPrimitive(field.Kind, rawValue));
    }

    private static ComplexValue DeserializeElement(XElement element, TypeDescription type, TypeDictionary? dictionary) {
        var fields = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var field in type.Fields) {
            var children = FindChildren(element, field.Name);
            if (field.ElementCount is { } count && field.Kind != TypeKind.String) {
                if (children.Count != count) {
                    throw new FormatException($"XML field '{field.Name}' has {children.Count.ToString(CultureInfo.InvariantCulture)} elements; expected {count.ToString(CultureInfo.InvariantCulture)}.");
                }

                var values = new object?[count];
                for (var i = 0; i < count; i++) {
                    values[i] = ParseField(children[i], field, dictionary);
                }

                fields[field.Name] = values;
                continue;
            }

            if (children.Count == 0) {
                throw new FormatException($"XML complex value is missing field '{field.Name}'.");
            }

            fields[field.Name] = ParseField(children[0], field, dictionary);
        }

        return new ComplexValue {
            Type = OpcBinaryCodecUtilities.CreateStructType(type, dictionary),
            Fields = fields,
        };
    }

    private static object? ParseField(XElement element, TypeField field, TypeDictionary? dictionary) {
        if (field.Kind == TypeKind.StructReference) {
            var nestedType = OpcBinaryCodecUtilities.ResolveType(field, dictionary);
            return DeserializeElement(element, nestedType, dictionary);
        }

        return ParsePrimitive(field.Kind, element.Value);
    }

    private static string FormatPrimitive(TypeKind kind, object? value) =>
        kind switch {
            TypeKind.Boolean => XmlConvert.ToString(Convert.ToBoolean(value, CultureInfo.InvariantCulture)),
            TypeKind.Int8 => XmlConvert.ToString(Convert.ToSByte(value, CultureInfo.InvariantCulture)),
            TypeKind.UInt8 => XmlConvert.ToString(Convert.ToByte(value, CultureInfo.InvariantCulture)),
            TypeKind.Int16 => XmlConvert.ToString(Convert.ToInt16(value, CultureInfo.InvariantCulture)),
            TypeKind.UInt16 => XmlConvert.ToString(Convert.ToUInt16(value, CultureInfo.InvariantCulture)),
            TypeKind.Int32 => XmlConvert.ToString(Convert.ToInt32(value, CultureInfo.InvariantCulture)),
            TypeKind.UInt32 => XmlConvert.ToString(Convert.ToUInt32(value, CultureInfo.InvariantCulture)),
            TypeKind.Int64 => XmlConvert.ToString(Convert.ToInt64(value, CultureInfo.InvariantCulture)),
            TypeKind.UInt64 => XmlConvert.ToString(Convert.ToUInt64(value, CultureInfo.InvariantCulture)),
            TypeKind.Single => XmlConvert.ToString(Convert.ToSingle(value, CultureInfo.InvariantCulture)),
            TypeKind.Double => XmlConvert.ToString(Convert.ToDouble(value, CultureInfo.InvariantCulture)),
            TypeKind.FileTime => XmlConvert.ToString(ToDateTime(value), XmlDateTimeSerializationMode.Utc),
            TypeKind.Guid => ToGuid(value).ToString("D"),
            TypeKind.Blob => Convert.ToBase64String(ToByteArray(value)),
            TypeKind.String => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
            _ => throw new NotSupportedException($"Type kind '{kind}' is not supported by the XML complex value serializer."),
        };

    private static object ParsePrimitive(TypeKind kind, string value) =>
        kind switch {
            TypeKind.Boolean => XmlConvert.ToBoolean(value),
            TypeKind.Int8 => sbyte.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.UInt8 => byte.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.Int16 => short.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.UInt16 => ushort.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.Int32 => int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.UInt32 => uint.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.Int64 => long.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.UInt64 => ulong.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture),
            TypeKind.Single => XmlConvert.ToSingle(value),
            TypeKind.Double => XmlConvert.ToDouble(value),
            TypeKind.FileTime => XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Utc),
            TypeKind.Guid => Guid.Parse(value),
            TypeKind.Blob => Convert.FromBase64String(value),
            TypeKind.String => value,
            _ => throw new NotSupportedException($"Type kind '{kind}' is not supported by the XML complex value serializer."),
        };

    private static XName CreateName(TypeDictionary? dictionary, string localName) {
        if (dictionary?.Name is { Length: > 0 } namespaceName && Uri.TryCreate(namespaceName, UriKind.Absolute, out _)) {
            return XNamespace.Get(namespaceName) + localName;
        }

        return localName;
    }

    private static List<XElement> FindChildren(XElement element, string localName) {
        var children = new List<XElement>();
        foreach (var child in element.Elements()) {
            if (child.Name.LocalName.Equals(localName, StringComparison.Ordinal)) {
                children.Add(child);
            }
        }

        return children;
    }

    private static DateTime ToDateTime(object? value) =>
        value switch {
            DateTime typed => typed.ToUniversalTime(),
            DateTimeOffset typed => typed.UtcDateTime,
            string typed => XmlConvert.ToDateTime(typed, XmlDateTimeSerializationMode.Utc),
            long typed => DateTime.FromFileTimeUtc(typed),
            _ => Convert.ToDateTime(value, CultureInfo.InvariantCulture).ToUniversalTime(),
        };

    private static Guid ToGuid(object? value) =>
        value switch {
            Guid typed => typed,
            string typed => Guid.Parse(typed),
            _ => throw new InvalidCastException("Value cannot be converted to a GUID."),
        };

    private static byte[] ToByteArray(object? value) =>
        value switch {
            byte[] typed => typed,
            ReadOnlyMemory<byte> typed => typed.ToArray(),
            Memory<byte> typed => typed.ToArray(),
            _ => throw new InvalidCastException("Value cannot be converted to a byte array."),
        };

    private static XmlReaderSettings CreateReaderSettings() =>
        new() {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };
}
