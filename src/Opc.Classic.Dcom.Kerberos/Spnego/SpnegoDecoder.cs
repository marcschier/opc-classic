//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Formats.Asn1;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Decodes RFC 4178 SPNEGO negotiation tokens.
/// </summary>
public static class SpnegoDecoder
{
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
