// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Xml.Tests;

public sealed class XmlDaConstantsTests
{
    private static string ReadXmlDaNamespace() => XmlDaConstants.XmlDaNamespace;
    private static string ReadSoapEnvelopeNamespace() => XmlDaConstants.SoapEnvelopeNamespace;
    private static string ReadGetStatusAction() => XmlDaConstants.SoapActionGetStatus;
    private static string ReadReadAction() => XmlDaConstants.SoapActionRead;
    private static string ReadWriteAction() => XmlDaConstants.SoapActionWrite;
    private static string ReadSubscribeAction() => XmlDaConstants.SoapActionSubscribe;
    private static string ReadBrowseAction() => XmlDaConstants.SoapActionBrowse;

    [Test]
    public async Task XmlDaNamespace_MatchesSpec()
    {
        await Assert.That(ReadXmlDaNamespace())
            .IsEqualTo("http://opcfoundation.org/webservices/XMLDA/1.0/");
    }

    [Test]
    public async Task SoapEnvelope_IsStandardSoap11()
    {
        await Assert.That(ReadSoapEnvelopeNamespace())
            .IsEqualTo("http://schemas.xmlsoap.org/soap/envelope/");
    }

    [Test]
    public async Task SoapActionGetStatus_BuildsCorrectly()
    {
        await Assert.That(ReadGetStatusAction())
            .IsEqualTo("http://opcfoundation.org/webservices/XMLDA/1.0/GetStatus");
    }

    [Test]
    public async Task SoapAction_AllOperations_StartWithNamespace()
    {
        await Assert.That(ReadReadAction()).StartsWith(ReadXmlDaNamespace());
        await Assert.That(ReadWriteAction()).StartsWith(ReadXmlDaNamespace());
        await Assert.That(ReadSubscribeAction()).StartsWith(ReadXmlDaNamespace());
        await Assert.That(ReadBrowseAction()).StartsWith(ReadXmlDaNamespace());
    }
}
