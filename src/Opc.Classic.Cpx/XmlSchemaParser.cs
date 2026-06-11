//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Opc.Classic.Cpx;

/// <summary>
/// Parses XML Schema dictionaries used by the CPX <c>XMLSchema</c> type system.
/// </summary>
public static class XmlSchemaParser
{
    private const string XmlSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

    /// <summary>Parse an XML Schema document into a CPX type dictionary.</summary>
    public static TypeDictionary Parse(string schemaXml)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaXml);

        using var stringReader = new StringReader(schemaXml);
        using var xmlReader = XmlReader.Create(stringReader, CreateReaderSettings());
        var document = XDocument.Load(xmlReader, LoadOptions.None);
        return Parse(document.Root ?? throw new FormatException("XML Schema document is empty."));
    }

    /// <summary>Parse an XML Schema root element into a CPX type dictionary.</summary>
    public static TypeDictionary Parse(XElement schema)
    {
        ArgumentNullException.ThrowIfNull(schema);
        if (!IsSchemaElement(schema, "schema"))
        {
            throw new FormatException($"Expected XML Schema root element, found '{schema.Name.LocalName}'.");
        }

        var targetNamespace = ReadString(schema, "targetNamespace") ?? string.Empty;
        var namedComplexTypes = new Dictionary<string, XElement>(StringComparer.Ordinal);
        foreach (var child in schema.Elements())
        {
            if (IsSchemaElement(child, "complexType") && ReadString(child, "name") is { } name)
            {
                namedComplexTypes[name] = child;
            }
        }

        var types = new Dictionary<string, TypeDescription>(StringComparer.Ordinal);
        foreach (var child in schema.Elements())
        {
            if (IsSchemaElement(child, "element"))
            {
                AddTopLevelElement(types, namedComplexTypes, child);
            }
        }

        foreach (var (name, complexType) in namedComplexTypes)
        {
            if (!types.ContainsKey(name))
            {
                AddComplexType(types, namedComplexTypes, name, complexType);
            }
        }

        if (types.Count == 0)
        {
            throw new FormatException("XML Schema dictionary must contain at least one element or complexType.");
        }

        return new TypeDictionary(targetNamespace, types.Values);
    }

    private static void AddTopLevelElement(
        Dictionary<string, TypeDescription> types,
        Dictionary<string, XElement> namedComplexTypes,
        XElement element)
    {
        var name = ReadRequiredString(element, "name");
        if (FindInlineComplexType(element) is { } inlineComplexType)
        {
            AddComplexType(types, namedComplexTypes, name, inlineComplexType);
            return;
        }

        if (ReadString(element, "type") is { } typeName && namedComplexTypes.TryGetValue(StripPrefix(typeName), out var referencedComplexType))
        {
            AddComplexType(types, namedComplexTypes, name, referencedComplexType);
            return;
        }

        var kind = MapXmlSchemaType(ReadString(element, "type"));
        types.TryAdd(name, new TypeDescription(name, name, kind, isComplex: false));
    }

    private static void AddComplexType(
        Dictionary<string, TypeDescription> types,
        Dictionary<string, XElement> namedComplexTypes,
        string typeId,
        XElement complexType)
    {
        if (types.ContainsKey(typeId))
        {
            return;
        }

        var fields = new List<TypeField>();
        foreach (var element in EnumerateChildElements(complexType))
        {
            fields.Add(ParseElementField(types, namedComplexTypes, typeId, element));
        }

        types[typeId] = new TypeDescription(typeId, typeId, TypeKind.StructReference, isComplex: true, fields);
    }

    private static TypeField ParseElementField(
        Dictionary<string, TypeDescription> types,
        Dictionary<string, XElement> namedComplexTypes,
        string parentTypeId,
        XElement element)
    {
        var name = ReadRequiredString(element, "name");
        var elementCount = ReadElementCount(element);

        if (FindInlineComplexType(element) is { } inlineComplexType)
        {
            var nestedTypeId = string.Create(CultureInfo.InvariantCulture, $"{parentTypeId}/{name}");
            AddComplexType(types, namedComplexTypes, nestedTypeId, inlineComplexType);
            return new TypeField(name, TypeKind.StructReference, nestedTypeId, ElementCount: elementCount);
        }

        if (ReadString(element, "type") is { } typeName)
        {
            var localTypeName = StripPrefix(typeName);
            if (namedComplexTypes.TryGetValue(localTypeName, out var referencedComplexType))
            {
                AddComplexType(types, namedComplexTypes, localTypeName, referencedComplexType);
                return new TypeField(name, TypeKind.StructReference, localTypeName, ElementCount: elementCount);
            }

            return new TypeField(name, MapXmlSchemaType(typeName), ElementCount: elementCount);
        }

        return new TypeField(name, TypeKind.String, ElementCount: elementCount);
    }

    private static IEnumerable<XElement> EnumerateChildElements(XElement complexType)
    {
        foreach (var child in complexType.Elements())
        {
            if (IsSchemaElement(child, "sequence") || IsSchemaElement(child, "all") || IsSchemaElement(child, "choice"))
            {
                foreach (var element in child.Elements())
                {
                    if (IsSchemaElement(element, "element"))
                    {
                        yield return element;
                    }
                }
            }
            else if (IsSchemaElement(child, "element"))
            {
                yield return child;
            }
        }
    }

    private static XElement? FindInlineComplexType(XElement element)
    {
        foreach (var child in element.Elements())
        {
            if (IsSchemaElement(child, "complexType"))
            {
                return child;
            }
        }

        return null;
    }

    private static TypeKind MapXmlSchemaType(string? typeName)
    {
        var localName = StripPrefix(typeName);
        return localName switch
        {
            "boolean" => TypeKind.Boolean,
            "byte" => TypeKind.Int8,
            "unsignedByte" => TypeKind.UInt8,
            "short" => TypeKind.Int16,
            "unsignedShort" => TypeKind.UInt16,
            "int" => TypeKind.Int32,
            "integer" => TypeKind.Int32,
            "nonNegativeInteger" => TypeKind.UInt32,
            "positiveInteger" => TypeKind.UInt32,
            "unsignedInt" => TypeKind.UInt32,
            "long" => TypeKind.Int64,
            "unsignedLong" => TypeKind.UInt64,
            "float" => TypeKind.Single,
            "double" => TypeKind.Double,
            "decimal" => TypeKind.Double,
            "dateTime" => TypeKind.FileTime,
            "date" => TypeKind.FileTime,
            "base64Binary" => TypeKind.Blob,
            "hexBinary" => TypeKind.Blob,
            "guid" => TypeKind.Guid,
            "string" => TypeKind.String,
            "normalizedString" => TypeKind.String,
            "token" => TypeKind.String,
            "anyURI" => TypeKind.String,
            _ => TypeKind.String,
        };
    }

    private static int? ReadElementCount(XElement element)
    {
        var maxOccurs = ReadString(element, "maxOccurs");
        if (maxOccurs is null || maxOccurs.Equals("1", StringComparison.Ordinal))
        {
            return null;
        }

        if (maxOccurs.Equals("unbounded", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return int.Parse(maxOccurs, NumberStyles.Integer, CultureInfo.InvariantCulture);
    }

    private static XmlReaderSettings CreateReaderSettings() =>
        new()
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

    private static bool IsSchemaElement(XElement element, string localName) =>
        element.Name.LocalName.Equals(localName, StringComparison.Ordinal)
        && (element.Name.NamespaceName.Length == 0 || element.Name.NamespaceName.Equals(XmlSchemaNamespace, StringComparison.Ordinal));

    private static string ReadRequiredString(XElement element, string attributeName) =>
        ReadString(element, attributeName) ?? throw new FormatException($"Element '{element.Name.LocalName}' is missing required attribute '{attributeName}'.");

    private static string? ReadString(XElement element, string attributeName) =>
        element.Attribute(attributeName)?.Value;

    private static string StripPrefix(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var separator = value.IndexOf(':', StringComparison.Ordinal);
        return separator >= 0 ? value[(separator + 1)..] : value;
    }
}
