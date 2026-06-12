//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Opc.Classic.Cpx;

/// <summary>
/// Parses OPCBinary type dictionaries defined by OPC Complex Data 1.00 §6.
/// </summary>
public static class OpcBinaryDictionaryParser
{
    private static readonly XNamespace XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>Parse a complete <c>TypeDictionary</c> XML document.</summary>
    public static TypeDictionary Parse(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, CreateReaderSettings());
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        return Parse(document.Root ?? throw new FormatException("OPCBinary XML is empty."));
    }

    /// <summary>Parse a complete <c>TypeDictionary</c> element or a single <c>TypeDescription</c> element.</summary>
    public static TypeDictionary Parse(XElement root)
    {
        ArgumentNullException.ThrowIfNull(root);

        if (IsElement(root, "TypeDictionary"))
        {
            var defaultBigEndian = ReadBoolean(root, "DefaultBigEndian") ?? true;
            var defaultStringEncoding = ReadString(root, "DefaultStringEncoding") ?? TypeDictionary.DefaultOpcBinaryStringEncoding;
            var defaultCharWidth = ReadInt32(root, "DefaultCharWidth") ?? 2;
            var defaultFloatFormat = ReadString(root, "DefaultFloatFormat") ?? TypeDictionary.DefaultOpcBinaryFloatFormat;
            var name = ReadString(root, "Name") ?? ReadString(root, "TargetNamespace") ?? string.Empty;

            var types = new List<TypeDescription>();
            foreach (var element in root.Elements())
            {
                if (IsElement(element, "TypeDescription"))
                {
                    types.Add(ParseTypeDescriptionElement(element));
                }
            }

            if (types.Count == 0)
            {
                throw new FormatException("OPCBinary TypeDictionary must contain at least one TypeDescription.");
            }

            return new TypeDictionary(name, types, defaultBigEndian, defaultStringEncoding, defaultCharWidth, defaultFloatFormat);
        }

        if (IsElement(root, "TypeDescription"))
        {
            var type = ParseTypeDescriptionElement(root);
            return new TypeDictionary(
                string.Empty,
                new[] { type },
                type.DefaultBigEndian ?? true,
                type.DefaultStringEncoding ?? TypeDictionary.DefaultOpcBinaryStringEncoding,
                type.DefaultCharWidth ?? 2,
                type.DefaultFloatFormat ?? TypeDictionary.DefaultOpcBinaryFloatFormat);
        }

        throw new FormatException($"Unexpected OPCBinary root element '{root.Name.LocalName}'.");
    }

    /// <summary>Parse a single OPCBinary <c>TypeDescription</c> XML document.</summary>
    public static TypeDescription ParseTypeDescription(string xml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, CreateReaderSettings());
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        var root = document.Root ?? throw new FormatException("OPCBinary TypeDescription XML is empty.");
        if (!IsElement(root, "TypeDescription"))
        {
            throw new FormatException($"Expected TypeDescription root element, found '{root.Name.LocalName}'.");
        }

        return ParseTypeDescriptionElement(root);
    }

    private static TypeDescription ParseTypeDescriptionElement(XElement element)
    {
        var typeId = ReadRequiredString(element, "TypeID");
        var fields = new List<TypeField>();
        foreach (var child in element.Elements())
        {
            fields.Add(ParseField(child));
        }

        if (fields.Count == 0)
        {
            throw new FormatException($"TypeDescription '{typeId}' must contain at least one field.");
        }

        return new TypeDescription(
            typeId,
            typeId,
            TypeKind.StructReference,
            isComplex: true,
            fields,
            ReadBoolean(element, "DefaultBigEndian"),
            ReadString(element, "DefaultStringEncoding"),
            ReadInt32(element, "DefaultCharWidth"),
            ReadString(element, "DefaultFloatFormat"));
    }

    private static TypeField ParseField(XElement element)
    {
        var xsiType = ReadString(element, XsiNamespace + "type");
        var fieldTypeName = StripPrefix(xsiType) ?? element.Name.LocalName;
        var name = ReadString(element, "Name") ?? string.Empty;
        var length = ReadInt32(element, "Length");
        var elementCount = ReadInt32(element, "ElementCount");
        var elementCountRef = ReadString(element, "ElementCountRef") ?? ReadString(element, "CharCountRef");
        var fieldTerminator = ReadString(element, "FieldTerminator");
        var format = ReadString(element, "Format") ?? ReadString(element, "FloatFormat");
        var byteOrder = ReadBoolean(element, "DefaultBigEndian") switch
        {
            true => ByteOrder.BigEndian,
            false => ByteOrder.LittleEndian,
            null => (ByteOrder?)null,
        };

        var kind = ResolveKind(fieldTypeName, element, length);
        var typeId = kind == TypeKind.StructReference ? ReadRequiredString(element, "TypeID") : null;
        var stringEncoding = ReadString(element, "StringEncoding");
        var charWidth = ReadInt32(element, "CharWidth");

        if (fieldTypeName.Equals("Ascii", StringComparison.OrdinalIgnoreCase))
        {
            stringEncoding ??= "ASCII";
            charWidth ??= 1;
        }
        else if (fieldTypeName.Equals("Unicode", StringComparison.OrdinalIgnoreCase))
        {
            stringEncoding ??= TypeDictionary.DefaultOpcBinaryStringEncoding;
            charWidth ??= 2;
        }

        return new TypeField(
            name,
            kind,
            typeId,
            length,
            elementCount,
            elementCountRef,
            fieldTerminator,
            byteOrder,
            stringEncoding,
            charWidth,
            format);
    }

    private static TypeKind ResolveKind(string fieldTypeName, XElement element, int? length) =>
        fieldTypeName switch
        {
            "TypeReference" => TypeKind.StructReference,
            "CharString" => TypeKind.String,
            "Ascii" => TypeKind.String,
            "Unicode" => TypeKind.String,
            "Boolean" => TypeKind.Boolean,
            "BitString" => TypeKind.BitString,
            "Blob" => TypeKind.Blob,
            "Guid" => TypeKind.Guid,
            "GUID" => TypeKind.Guid,
            "FileTime" => TypeKind.FileTime,
            "FILETIME" => TypeKind.FileTime,
            "Single" => TypeKind.Single,
            "Float" => TypeKind.Single,
            "Double" => TypeKind.Double,
            "FloatingPoint" => ResolveFloatingPointKind(length),
            "Int8" => TypeKind.Int8,
            "UInt8" => TypeKind.UInt8,
            "Uint8" => TypeKind.UInt8,
            "Int16" => TypeKind.Int16,
            "UInt16" => TypeKind.UInt16,
            "Uint16" => TypeKind.UInt16,
            "Int32" => TypeKind.Int32,
            "UInt32" => TypeKind.UInt32,
            "Uint32" => TypeKind.UInt32,
            "Int64" => TypeKind.Int64,
            "UInt64" => TypeKind.UInt64,
            "Uint64" => TypeKind.UInt64,
            "Integer" => ResolveIntegerKind(element, length),
            _ => throw new FormatException($"Unsupported OPCBinary field type '{fieldTypeName}'."),
        };

    private static TypeKind ResolveIntegerKind(XElement element, int? length)
    {
        var signed = ReadBoolean(element, "Signed") ?? true;
        return (length ?? 4, signed) switch
        {
            (1, true) => TypeKind.Int8,
            (1, false) => TypeKind.UInt8,
            (2, true) => TypeKind.Int16,
            (2, false) => TypeKind.UInt16,
            (4, true) => TypeKind.Int32,
            (4, false) => TypeKind.UInt32,
            (8, true) => TypeKind.Int64,
            (8, false) => TypeKind.UInt64,
            _ => throw new FormatException("Integer fields must have a supported Length of 1, 2, 4, or 8 bytes."),
        };
    }

    private static TypeKind ResolveFloatingPointKind(int? length) =>
        (length ?? 8) switch
        {
            4 => TypeKind.Single,
            8 => TypeKind.Double,
            _ => throw new FormatException("FloatingPoint fields must have a supported Length of 4 or 8 bytes."),
        };

    private static XmlReaderSettings CreateReaderSettings() =>
        new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

    private static bool IsElement(XElement element, string localName) =>
        element.Name.LocalName.Equals(localName, StringComparison.Ordinal);

    private static string ReadRequiredString(XElement element, string attributeName) =>
        ReadString(element, attributeName) ?? throw new FormatException($"Element '{element.Name.LocalName}' is missing required attribute '{attributeName}'.");

    private static string? ReadString(XElement element, string attributeName) =>
        element.Attribute(attributeName)?.Value;

    private static string? ReadString(XElement element, XName attributeName) =>
        element.Attribute(attributeName)?.Value;

    private static int? ReadInt32(XElement element, string attributeName) =>
        ReadString(element, attributeName) is { } value
            ? int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture)
            : null;

    private static bool? ReadBoolean(XElement element, string attributeName) =>
        ReadString(element, attributeName) is { } value ? XmlConvert.ToBoolean(value) : null;

    private static string? StripPrefix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var separator = value.IndexOf(':', StringComparison.Ordinal);
        return separator >= 0 ? value[(separator + 1)..] : value;
    }
}
