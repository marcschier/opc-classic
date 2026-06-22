// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.ComponentModel;
using System.Xml;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Cpx;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// MCP tools for OPC Complex Data (CPX) metadata operations.
/// </summary>
public sealed class CpxTools
{
    private static readonly string[] SupportedTypeSystemIds = [TypeDictionary.OpcBinaryTypeSystemId, TypeDictionary.XmlSchemaTypeSystemId];
    private readonly IOpcSessionManager _sessionManager;

    /// <summary>
    /// Creates the CPX tool set.
    /// </summary>
    public CpxTools(IOpcSessionManager sessionManager) =>
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));

    /// <summary>
    /// Gets the complex-type description for a DA item.
    /// </summary>
    [McpServerTool(Name = "opcclassic.cpx.get_complex_type", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets OPC Complex Data metadata for a DA item, including type ID, dictionary ID, type item ID, unconverted item ID, and available filters.")]
    public async Task<OpcComplexTypeDto> GetComplexType(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect.")]
        string sessionId,
        [Description("The DA item ID whose complex-data metadata should be queried.")]
        string itemId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        CpxClientState client = GetCpxClient(sessionId);
        Guid typeId = await client.ComplexDataItem2.GetTypeIDAsync(itemId, cancellationToken).ConfigureAwait(false);
        string dictionaryId = await client.ComplexDataItem2.GetDictionaryIDAsync(itemId, cancellationToken).ConfigureAwait(false);
        string? typeItemId = await GetOptionalStringAsync(
            token => client.ComplexDataItem.GetTypeItemIDAsync(itemId, token),
            cancellationToken).ConfigureAwait(false);
        string? unconvertedItemId = await GetOptionalStringAsync(
            token => client.ComplexDataItem.GetUnconvertedItemIDAsync(itemId, token),
            cancellationToken).ConfigureAwait(false);
        string? dataFilter = await GetOptionalStringAsync(
            token => client.ComplexDataItem.GetDataFilterAsync(itemId, token),
            cancellationToken).ConfigureAwait(false);
        string[] availableFilters = await GetOptionalStringArrayAsync(
            token => client.ComplexDataItem2.GetAvailableFiltersAsync(itemId, token),
            cancellationToken).ConfigureAwait(false);

        return new OpcComplexTypeDto(
            itemId,
            typeId,
            dictionaryId,
            typeItemId,
            unconvertedItemId,
            dataFilter,
            availableFilters);
    }

    /// <summary>
    /// Gets an OPC Complex Data type-system descriptor.
    /// </summary>
    [McpServerTool(Name = "opcclassic.cpx.get_type_system", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets the OPC Complex Data namespace descriptor for a supported type system: OPCBinary or XMLSchema.")]
    public OpcTypeSystemDto GetTypeSystem(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect.")]
        string sessionId,
        [Description("Type system identifier. Accepted values include OPCBinary, binary, XMLSchema, xml, and schema.")]
        string typeSystemId = TypeDictionary.OpcBinaryTypeSystemId)
    {
        _ = GetCpxClient(sessionId);
        string normalized = NormalizeTypeSystemId(typeSystemId);
        bool supported = SupportedTypeSystemIds.Contains(normalized, StringComparer.Ordinal);
        string namespacePath = CpxNamespaceBuilder.BuildTypeSystemPath(normalized);
        return new OpcTypeSystemDto(normalized, supported, namespacePath, SupportedTypeSystemIds);
    }

    /// <summary>
    /// Gets a CPX type dictionary.
    /// </summary>
    [McpServerTool(Name = "opcclassic.cpx.get_dictionary", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets a Complex Data type dictionary by dictionary ID and parses OPCBinary or XMLSchema dictionaries when possible.")]
    public async Task<OpcTypeDictionaryDto> GetDictionary(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect.")]
        string sessionId,
        [Description("Dictionary identifier returned by opcclassic.cpx.get_complex_type.")]
        string dictionaryId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dictionaryId);
        CpxClientState client = GetCpxClient(sessionId);
        string dictionary = await client.TypeLibrary.GetDictionaryAsync(dictionaryId, cancellationToken).ConfigureAwait(false);
        ParsedDictionary parsed = ParseDictionary(dictionary);
        return new OpcTypeDictionaryDto(
            dictionaryId,
            parsed.TypeSystemId,
            parsed.Name,
            dictionary,
            parsed.Types,
            parsed.ParseError);
    }

    private CpxClientState GetCpxClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        if (session.CpxClient is { } existing)
        {
            return existing;
        }

        DaClientState daClient = session.DaClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC DA server. Call opcclassic.da.connect before using CPX tools.");
        var cpxClient = new CpxClientState(daClient.Host, daClient.ProgId, daClient.Clsid, daClient.CallChannel, ownsChannel: false);
        session.CpxClient = cpxClient;
        session.Touch();
        return cpxClient;
    }

    private static async Task<string?> GetOptionalStringAsync(Func<CancellationToken, Task<string>> action, CancellationToken cancellationToken)
    {
        try
        {
            return await action(cancellationToken).ConfigureAwait(false);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return null;
        }
    }

    private static async Task<string[]> GetOptionalStringArrayAsync(Func<CancellationToken, Task<string[]>> action, CancellationToken cancellationToken)
    {
        try
        {
            return await action(cancellationToken).ConfigureAwait(false);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            return [];
        }
    }

    private static string NormalizeTypeSystemId(string? typeSystemId)
    {
        string value = string.IsNullOrWhiteSpace(typeSystemId) ? TypeDictionary.OpcBinaryTypeSystemId : typeSystemId.Trim();
        return value.ToLowerInvariant() switch
        {
            "binary" or "opc-binary" or "opc_binary" or "opcbinary" => TypeDictionary.OpcBinaryTypeSystemId,
            "xml" or "schema" or "xml-schema" or "xml_schema" or "xmlschema" => TypeDictionary.XmlSchemaTypeSystemId,
            _ => value,
        };
    }

    private static ParsedDictionary ParseDictionary(string dictionary)
    {
        if (string.IsNullOrWhiteSpace(dictionary))
        {
            return new ParsedDictionary(TypeDictionary.OpcBinaryTypeSystemId, null, [], "Dictionary is empty.");
        }

        try
        {
            TypeDictionary parsed = OpcBinaryDictionaryParser.Parse(dictionary);
            return new ParsedDictionary(TypeDictionary.OpcBinaryTypeSystemId, parsed.Name, ToTypeDtos(parsed), null);
        }
        catch (ArgumentException ex)
        {
            return ParseXmlSchemaDictionary(dictionary, ex.Message);
        }
        catch (FormatException ex)
        {
            return ParseXmlSchemaDictionary(dictionary, ex.Message);
        }
        catch (XmlException ex)
        {
            return ParseXmlSchemaDictionary(dictionary, ex.Message);
        }
    }

    private static ParsedDictionary ParseXmlSchemaDictionary(string dictionary, string binaryParseError)
    {
        try
        {
            TypeDictionary parsed = XmlSchemaParser.Parse(dictionary);
            return new ParsedDictionary(TypeDictionary.XmlSchemaTypeSystemId, parsed.Name, ToTypeDtos(parsed), null);
        }
        catch (ArgumentException ex)
        {
            return new ParsedDictionary(TypeDictionary.XmlSchemaTypeSystemId, null, [], $"OPCBinary parse failed: {binaryParseError}; XMLSchema parse failed: {ex.Message}");
        }
        catch (FormatException ex)
        {
            return new ParsedDictionary(TypeDictionary.XmlSchemaTypeSystemId, null, [], $"OPCBinary parse failed: {binaryParseError}; XMLSchema parse failed: {ex.Message}");
        }
        catch (XmlException ex)
        {
            return new ParsedDictionary(TypeDictionary.XmlSchemaTypeSystemId, null, [], $"OPCBinary parse failed: {binaryParseError}; XMLSchema parse failed: {ex.Message}");
        }
    }

    private static IReadOnlyList<OpcComplexTypeDescriptionDto> ToTypeDtos(TypeDictionary dictionary) =>
        dictionary.Types.Select(static type => new OpcComplexTypeDescriptionDto(
            type.Name,
            type.TypeId,
            type.Type.ToString(),
            type.IsComplex,
            type.Fields.Select(static field => new OpcComplexTypeFieldDto(
                field.Name,
                field.Kind.ToString(),
                field.TypeId,
                field.Length,
                field.ElementCount,
                field.ElementCountFieldName,
                field.FieldTerminator,
                field.ByteOrder?.ToString(),
                field.StringEncoding,
                field.CharWidth,
                field.Format)).ToArray())).ToArray();

    private sealed record ParsedDictionary(
        string TypeSystemId,
        string? Name,
        IReadOnlyList<OpcComplexTypeDescriptionDto> Types,
        string? ParseError);
}
