// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Formats.Asn1;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Decodes RFC 4178 SPNEGO negotiation tokens.
/// </summary>
public static class SpnegoDecoder
{
    /// <summary>
    /// Decodes a NegTokenInit initial context token and preserves the exact MechTypeList bytes.
    /// </summary>
    /// <param name="data">DER-encoded NegTokenInit data.</param>
    /// <returns>The parsed NegTokenInit fields.</returns>
    public static SpnegoNegTokenInit DecodeNegTokenInit(ReadOnlyMemory<byte> data)
    {
        var reader = new AsnReader(data, AsnEncodingRules.DER);
        var bodyReader = ReadNegTokenInitBody(reader);
        reader.ThrowIfNotEmpty();

        IReadOnlyList<string> mechTypes = [];
        ReadOnlyMemory<byte> mechToken = ReadOnlyMemory<byte>.Empty;
        ReadOnlyMemory<byte>? mechListMic = null;
        ReadOnlyMemory<byte> mechListBytes = ReadOnlyMemory<byte>.Empty;

        while (bodyReader.HasData)
        {
            var tag = bodyReader.PeekTag();
            if (tag.TagClass != TagClass.ContextSpecific)
            {
                _ = bodyReader.ReadEncodedValue();
                continue;
            }

            switch (tag.TagValue)
            {
                case 0:
                    mechTypes = ReadMechTypes(bodyReader, tag, out mechListBytes);
                    break;
                case 2:
                    mechToken = ReadOctetStringField(bodyReader, tag);
                    break;
                case 3:
                    mechListMic = ReadOctetStringField(bodyReader, tag);
                    break;
                default:
                    _ = bodyReader.ReadEncodedValue();
                    break;
            }
        }

        return new SpnegoNegTokenInit(mechTypes, mechToken, mechListMic, mechListBytes);
    }

    /// <summary>
    /// Decodes a NegTokenResp continuation token.
    /// </summary>
    /// <param name="data">DER-encoded NegTokenResp data.</param>
    /// <returns>The parsed NegTokenResp fields.</returns>
    public static SpnegoNegTokenResp DecodeNegTokenResp(ReadOnlyMemory<byte> data)
    {
        var reader = new AsnReader(data, AsnEncodingRules.DER);
        var bodyReader = ReadNegTokenRespBody(reader);
        reader.ThrowIfNotEmpty();

        SpnegoNegState? negState = null;
        string? supportedMech = null;
        ReadOnlyMemory<byte>? responseToken = null;
        ReadOnlyMemory<byte>? mechListMic = null;

        while (bodyReader.HasData)
        {
            var tag = bodyReader.PeekTag();
            if (tag.TagClass != TagClass.ContextSpecific)
            {
                _ = bodyReader.ReadEncodedValue();
                continue;
            }

            switch (tag.TagValue)
            {
                case 0:
                    negState = ReadNegState(bodyReader, tag);
                    break;
                case 1:
                    supportedMech = ReadSupportedMech(bodyReader, tag);
                    break;
                case 2:
                    responseToken = ReadOctetStringField(bodyReader, tag);
                    break;
                case 3:
                    mechListMic = ReadOctetStringField(bodyReader, tag);
                    break;
                default:
                    _ = bodyReader.ReadEncodedValue();
                    break;
            }
        }

        return new SpnegoNegTokenResp(negState, supportedMech, responseToken, mechListMic);
    }

    private static AsnReader ReadNegTokenInitBody(AsnReader reader)
    {
        var tag = reader.PeekTag();
        var negTokenInitTag = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        if (tag.Equals(negTokenInitTag))
        {
            var tokenReader = reader.ReadSequence(negTokenInitTag);
            var bodyReader = tokenReader.ReadSequence();
            tokenReader.ThrowIfNotEmpty();
            return bodyReader;
        }

        var initialContextTokenTag = new Asn1Tag(TagClass.Application, 0, isConstructed: true);
        if (tag.Equals(initialContextTokenTag))
        {
            var initialContextTokenReader = reader.ReadSequence(initialContextTokenTag);
            var oid = initialContextTokenReader.ReadObjectIdentifier();
            if (!StringComparer.Ordinal.Equals(oid, SpnegoOids.Spnego))
            {
                throw new AsnContentException();
            }

            var bodyReader = ReadNegTokenInitBody(initialContextTokenReader);
            initialContextTokenReader.ThrowIfNotEmpty();
            return bodyReader;
        }

        if (tag.Equals(Asn1Tag.Sequence))
        {
            return reader.ReadSequence();
        }

        throw new AsnContentException();
    }

    private static AsnReader ReadNegTokenRespBody(AsnReader reader)
    {
        var tag = reader.PeekTag();
        var negTokenRespTag = new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true);
        if (tag.Equals(negTokenRespTag))
        {
            var tokenReader = reader.ReadSequence(negTokenRespTag);
            var bodyReader = tokenReader.ReadSequence();
            tokenReader.ThrowIfNotEmpty();
            return bodyReader;
        }

        var initialContextTokenTag = new Asn1Tag(TagClass.Application, 0, isConstructed: true);
        if (tag.Equals(initialContextTokenTag))
        {
            var initialContextTokenReader = reader.ReadSequence(initialContextTokenTag);
            var oid = initialContextTokenReader.ReadObjectIdentifier();
            if (!StringComparer.Ordinal.Equals(oid, SpnegoOids.Spnego))
            {
                throw new AsnContentException();
            }

            var bodyReader = ReadNegTokenRespBody(initialContextTokenReader);
            initialContextTokenReader.ThrowIfNotEmpty();
            return bodyReader;
        }

        if (tag.Equals(Asn1Tag.Sequence))
        {
            return reader.ReadSequence();
        }

        throw new AsnContentException();
    }

    private static SpnegoNegState ReadNegState(AsnReader bodyReader, Asn1Tag tag)
    {
        var innerReader = bodyReader.ReadSequence(tag);
        var negState = innerReader.ReadEnumeratedValue<SpnegoNegState>();
        innerReader.ThrowIfNotEmpty();
        return negState;
    }

    private static List<string> ReadMechTypes(
        AsnReader bodyReader,
        Asn1Tag tag,
        out ReadOnlyMemory<byte> mechListBytes)
    {
        var innerReader = bodyReader.ReadSequence(tag);
        mechListBytes = innerReader.ReadEncodedValue().ToArray();
        innerReader.ThrowIfNotEmpty();

        var mechListReader = new AsnReader(mechListBytes, AsnEncodingRules.DER);
        var sequenceReader = mechListReader.ReadSequence();
        var mechTypes = new List<string>();
        while (sequenceReader.HasData)
        {
            mechTypes.Add(sequenceReader.ReadObjectIdentifier());
        }

        mechListReader.ThrowIfNotEmpty();
        return mechTypes;
    }

    private static string ReadSupportedMech(AsnReader bodyReader, Asn1Tag tag)
    {
        var innerReader = bodyReader.ReadSequence(tag);
        var supportedMech = innerReader.ReadObjectIdentifier();
        innerReader.ThrowIfNotEmpty();
        return supportedMech;
    }

    private static ReadOnlyMemory<byte> ReadOctetStringField(AsnReader bodyReader, Asn1Tag tag)
    {
        var innerReader = bodyReader.ReadSequence(tag);
        var value = new ReadOnlyMemory<byte>(innerReader.ReadOctetString());
        innerReader.ThrowIfNotEmpty();
        return value;
    }
}
