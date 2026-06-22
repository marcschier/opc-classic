// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Xml.Tests;

public sealed class XmlDaErrorCodesMappingTests
{
    [Test]
    [Arguments(XmlDaErrorCode.Ok, "S_OK")]
    [Arguments(XmlDaErrorCode.Clamp, "S_CLAMP")]
    [Arguments(XmlDaErrorCode.DataQueueOverflow, "S_DATAQUEUEOVERFLOW")]
    [Arguments(XmlDaErrorCode.UnsupportedRate, "S_UNSUPPORTEDRATE")]
    [Arguments(XmlDaErrorCode.AccessDenied, "E_ACCESS_DENIED")]
    [Arguments(XmlDaErrorCode.Busy, "E_BUSY")]
    [Arguments(XmlDaErrorCode.Fail, "E_FAIL")]
    [Arguments(XmlDaErrorCode.InvalidContinuationPoint, "E_INVALIDCONTINUATIONPOINT")]
    [Arguments(XmlDaErrorCode.InvalidFilter, "E_INVALIDFILTER")]
    [Arguments(XmlDaErrorCode.InvalidHoldTime, "E_INVALIDHOLDTIME")]
    [Arguments(XmlDaErrorCode.InvalidItemId, "E_INVALIDITEMID")]
    [Arguments(XmlDaErrorCode.InvalidItemName, "E_INVALIDITEMNAME")]
    [Arguments(XmlDaErrorCode.InvalidItemPath, "E_INVALIDITEMPATH")]
    [Arguments(XmlDaErrorCode.InvalidPid, "E_INVALIDPID")]
    [Arguments(XmlDaErrorCode.NoSubscription, "E_NOSUBSCRIPTION")]
    [Arguments(XmlDaErrorCode.NotSupported, "E_NOTSUPPORTED")]
    [Arguments(XmlDaErrorCode.OutOfMemory, "E_OUTOFMEMORY")]
    [Arguments(XmlDaErrorCode.Range, "E_RANGE")]
    [Arguments(XmlDaErrorCode.BadType, "E_BADTYPE")]
    [Arguments(XmlDaErrorCode.ReadOnly, "E_READONLY")]
    [Arguments(XmlDaErrorCode.ServerState, "E_SERVERSTATE")]
    [Arguments(XmlDaErrorCode.TimedOut, "E_TIMEDOUT")]
    [Arguments(XmlDaErrorCode.UnknownItemId, "E_UNKNOWNITEMID")]
    [Arguments(XmlDaErrorCode.UnknownItemName, "E_UNKNOWNITEMNAME")]
    [Arguments(XmlDaErrorCode.UnknownItemPath, "E_UNKNOWNITEMPATH")]
    [Arguments(XmlDaErrorCode.WriteOnly, "E_WRITEONLY")]
    [Arguments(XmlDaErrorCode.BadRights, "E_BADRIGHTS")]
    public async Task KnownCodes_ToResultIdAndParse_RoundTrip(XmlDaErrorCode code, string resultId)
    {
        await Assert.That(XmlDaErrorCodes.ToResultId(code)).IsEqualTo(resultId);
        await Assert.That(XmlDaErrorCodes.Parse(resultId)).IsEqualTo(code);
        await Assert.That(XmlDaErrorCodes.Parse("xmlDa:" + resultId)).IsEqualTo(code);
        await Assert.That(XmlDaErrorCodes.ParseResultId("  xmlDa:" + resultId + "  ")).IsEqualTo(code);
    }

    [Test]
    public async Task ParseResultId_NullOrWhitespace_MapsToOk()
    {
        await Assert.That(XmlDaErrorCodes.ParseResultId(null)).IsEqualTo(XmlDaErrorCode.Ok);
        await Assert.That(XmlDaErrorCodes.ParseResultId(string.Empty)).IsEqualTo(XmlDaErrorCode.Ok);
        await Assert.That(XmlDaErrorCodes.ParseResultId("   ")).IsEqualTo(XmlDaErrorCode.Ok);
    }

    [Test]
    public async Task Parse_NullWhitespaceMalformedOrUnknown_MapsToUnknown()
    {
        await Assert.That(XmlDaErrorCodes.Parse(null)).IsEqualTo(XmlDaErrorCode.Unknown);
        await Assert.That(XmlDaErrorCodes.Parse(string.Empty)).IsEqualTo(XmlDaErrorCode.Unknown);
        await Assert.That(XmlDaErrorCodes.Parse("   ")).IsEqualTo(XmlDaErrorCode.Unknown);
        await Assert.That(XmlDaErrorCodes.Parse("xmlDa:E_VENDOR_SPECIFIC")).IsEqualTo(XmlDaErrorCode.Unknown);
        await Assert.That(XmlDaErrorCodes.Parse("xmlDa:")).IsEqualTo(XmlDaErrorCode.Unknown);
    }

    [Test]
    public async Task Parse_UsesTextAfterLastColonAsLocalName()
    {
        await Assert.That(XmlDaErrorCodes.Parse("soap:xmlDa:E_FAIL")).IsEqualTo(XmlDaErrorCode.Fail);
        await Assert.That(XmlDaErrorCodes.Parse("vendor:xmlDa:S_OK")).IsEqualTo(XmlDaErrorCode.Ok);
    }

    [Test]
    public async Task ToResultId_UnknownOrUndefinedCode_ReturnsEmptyString()
    {
        await Assert.That(XmlDaErrorCodes.ToResultId(XmlDaErrorCode.Unknown)).IsEqualTo(string.Empty);
        await Assert.That(XmlDaErrorCodes.ToResultId((XmlDaErrorCode)9999)).IsEqualTo(string.Empty);
    }
}
