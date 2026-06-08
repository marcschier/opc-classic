//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using CsCheck;
using Opc.Classic.Tests.Fuzz;
using TUnit.Core;

namespace Opc.Classic.Cpx.Tests.Fuzz;

public sealed class CpxDictionaryParserFuzzTests
{
    private static readonly Type[] AllowedDictionaryParseExceptions =
    [
        typeof(InvalidDataException),
        typeof(XmlException),
        typeof(FormatException),
        typeof(ArgumentException),
        typeof(ArgumentOutOfRangeException),
        typeof(EndOfStreamException),
        typeof(NotSupportedException),
    ];

    [Test]
    [Category("Fuzz")]
    public async Task OpcBinaryDictionaryParser_Parse_RandomBytes_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.BytesEdgeWeighted.Sample(bytes =>
        {
            exercised++;
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                static input => OpcBinaryDictionaryParser.Parse(Encoding.UTF8.GetString(input.Span)),
                AllowedDictionaryParseExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    [Arguments(8)]
    [Arguments(16)]
    [Arguments(32)]
    [Arguments(64)]
    [Arguments(128)]
    [Arguments(256)]
    public async Task OpcBinaryDictionaryParser_Parse_DeepNestedTypes_BoundedOrRejected(int levels)
    {
        byte[] input = Encoding.UTF8.GetBytes(CreateDeepNestedDictionary(levels));

        FuzzHarness.AssertParseDoesNotCrash(
            input,
            static payload => OpcBinaryDictionaryParser.Parse(Encoding.UTF8.GetString(payload.Span)),
            AllowedDictionaryParseExceptions,
            resultInvariant: dictionary => AssertDictionaryContains(dictionary, $"Type{levels - 1}"));

        await Assert.That(input.Length).IsGreaterThan(0);
    }

    [Test]
    [Category("Fuzz")]
    public async Task OpcBinaryDictionaryParser_Parse_TypeRefCycle_BoundedOrRejected()
    {
        byte[] input = Encoding.UTF8.GetBytes("""
            <TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/">
              <TypeDescription TypeID="TypeA">
                <TypeReference Name="B" TypeID="TypeB" />
              </TypeDescription>
              <TypeDescription TypeID="TypeB">
                <TypeReference Name="A" TypeID="TypeA" />
              </TypeDescription>
            </TypeDictionary>
            """);

        FuzzHarness.AssertParseDoesNotCrash(
            input,
            static payload => OpcBinaryDictionaryParser.Parse(Encoding.UTF8.GetString(payload.Span)),
            AllowedDictionaryParseExceptions,
            resultInvariant: dictionary => AssertDictionaryContains(dictionary, "TypeA"));

        await Assert.That(input.Length).IsGreaterThan(0);
    }

    [Test]
    [Category("Fuzz")]
    public async Task OpcBinaryDictionaryParser_Parse_Corpus_DoesNotCrash()
    {
        int exercised = 0;
        foreach (object[] row in FuzzHarness.LoadCorpus("CpxDictionary"))
        {
            exercised++;
            var bytes = (byte[])row[0];
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                static payload => OpcBinaryDictionaryParser.Parse(Encoding.UTF8.GetString(payload.Span)),
                AllowedDictionaryParseExceptions);
        }

        await Assert.That(exercised).IsGreaterThanOrEqualTo(0);
    }

    private static void AssertDictionaryContains(TypeDictionary dictionary, string typeId)
    {
        if (dictionary.TryGetByTypeId(typeId) is null)
        {
            throw new InvalidDataException($"Dictionary did not contain expected TypeID '{typeId}'.");
        }
    }

    private static string CreateDeepNestedDictionary(int levels)
    {
        var builder = new StringBuilder();
        builder.AppendLine("""<TypeDictionary xmlns="http://opcfoundation.org/OPCBinary/1.0/">""");
        for (int i = 0; i < levels; i++)
        {
            builder.Append(CultureInvariant($"  <TypeDescription TypeID=\"Type{i}\">"));
            builder.AppendLine();
            if (i == levels - 1)
            {
                builder.AppendLine("""    <Integer Name="Value" xsi:type="Int32" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" />""");
            }
            else
            {
                builder.AppendLine(CultureInvariant($"    <TypeReference Name=\"Next\" TypeID=\"Type{i + 1}\" />"));
            }

            builder.AppendLine("  </TypeDescription>");
        }

        builder.AppendLine("</TypeDictionary>");
        return builder.ToString();
    }

    private static string CultureInvariant(FormattableString value) => FormattableString.Invariant(value);
}
