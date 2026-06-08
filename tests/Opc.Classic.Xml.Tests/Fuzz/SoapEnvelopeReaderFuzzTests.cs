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
using Opc.Classic.Xml.Serialization;
using TUnit.Core;

namespace Opc.Classic.Xml.Tests.Fuzz;

public sealed class SoapEnvelopeReaderFuzzTests
{
    private static readonly Type[] AllowedSoapReadExceptions =
    [
        typeof(XmlException),
        typeof(InvalidDataException),
        typeof(FormatException),
        typeof(ArgumentException),
        typeof(EndOfStreamException),
        typeof(NotSupportedException),
    ];

    private static readonly byte[] ValidEnvelope = Encoding.UTF8.GetBytes("""
        <?xml version="1.0"?>
        <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <BrowseResponse xmlns="http://opcfoundation.org/webservices/XMLDA/1.0/" />
          </soap:Body>
        </soap:Envelope>
        """);

    [Test]
    [Category("Fuzz")]
    public async Task SoapEnvelopeReader_Read_RandomBytes_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.BytesEdgeWeighted.Sample(bytes =>
        {
            exercised++;
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                ReadOperationResponse,
                AllowedSoapReadExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    public async Task SoapEnvelopeReader_Read_RandomUtf8Text_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.BytesEdgeWeighted.Sample(bytes =>
        {
            exercised++;
            byte[] printable = ToPrintableUtf8(bytes);
            FuzzHarness.AssertParseDoesNotCrash(
                printable,
                ReadOperationResponse,
                AllowedSoapReadExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    public async Task SoapEnvelopeReader_Read_XmlBomb_BoundedTimeOrRejected()
    {
        byte[] input = Encoding.UTF8.GetBytes("""
            <?xml version="1.0"?>
            <!DOCTYPE lolz [
             <!ENTITY lol "lol">
             <!ENTITY lol1 "&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;&lol;">
             <!ENTITY lol2 "&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;&lol1;">
            ]>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body><BrowseResponse>&lol2;</BrowseResponse></soap:Body>
            </soap:Envelope>
            """);

        FuzzHarness.AssertParseDoesNotCrash(
            input,
            ReadOperationResponse,
            [typeof(XmlException)],
            timeoutMs: 1_000);

        await Assert.That(input.Length).IsGreaterThan(0);
    }

    [Test]
    [Category("Fuzz")]
    public async Task SoapEnvelopeReader_Read_XxeExternalEntity_RejectedNoDtd()
    {
        byte[] input = Encoding.UTF8.GetBytes("""
            <?xml version="1.0"?>
            <!DOCTYPE xxe [
              <!ENTITY xxe SYSTEM "file:///C:/Windows/win.ini">
            ]>
            <soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
              <soap:Body><BrowseResponse>&xxe;</BrowseResponse></soap:Body>
            </soap:Envelope>
            """);

        try
        {
            _ = ReadOperationResponse(input);
            throw new InvalidOperationException("XXE payload was accepted.");
        }
        catch (XmlException ex)
        {
            await Assert.That(ex.Message).Contains("DTD");
        }
    }

    [Test]
    [Category("Fuzz")]
    public async Task SoapEnvelopeReader_Read_MutatedValid_DoesNotCrash()
    {
        int exercised = 0;
        FuzzHarness.MutateValid(ValidEnvelope).Sample(bytes =>
        {
            exercised++;
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                ReadOperationResponse,
                AllowedSoapReadExceptions);
        }, iter: FuzzHarness.Iterations, threads: 1);

        await Assert.That(exercised).IsEqualTo(FuzzHarness.Iterations);
    }

    [Test]
    [Category("Fuzz")]
    public async Task SoapEnvelopeReader_Read_Corpus_DoesNotCrash()
    {
        int exercised = 0;
        foreach (object[] row in FuzzHarness.LoadCorpus("SoapEnvelope"))
        {
            exercised++;
            var bytes = (byte[])row[0];
            FuzzHarness.AssertParseDoesNotCrash(
                bytes,
                ReadOperationResponse,
                AllowedSoapReadExceptions);
        }

        await Assert.That(exercised).IsGreaterThanOrEqualTo(0);
    }

    private static string ReadOperationResponse(ReadOnlyMemory<byte> input)
    {
        using var stream = new MemoryStream(input.ToArray());
        using var reader = new SoapEnvelopeReader(stream);
        return reader.AdvanceToOperationResponse();
    }

    private static byte[] ToPrintableUtf8(byte[] bytes)
    {
        var printable = new byte[bytes.Length];
        for (int i = 0; i < bytes.Length; i++)
        {
            printable[i] = bytes[i] switch
            {
                0x09 or 0x0a or 0x0d => bytes[i],
                >= 0x20 and <= 0x7e => bytes[i],
                _ => (byte)('a' + (bytes[i] % 26)),
            };
        }

        return printable;
    }
}
