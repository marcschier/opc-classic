//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Xml;
using TUnit.Core;

namespace Opc.Classic.Xml.Tests;

public sealed class XmlDaErrorCodesTests {
    [Test]
    [Arguments("S_CLAMP", XmlDaErrorCode.Clamp)]
    [Arguments("xmlDa:S_CLAMP", XmlDaErrorCode.Clamp)]
    [Arguments("S_DATAQUEUEOVERFLOW", XmlDaErrorCode.DataQueueOverflow)]
    [Arguments("xmlDa:S_DATAQUEUEOVERFLOW", XmlDaErrorCode.DataQueueOverflow)]
    [Arguments("S_UNSUPPORTEDRATE", XmlDaErrorCode.UnsupportedRate)]
    [Arguments("xmlDa:S_UNSUPPORTEDRATE", XmlDaErrorCode.UnsupportedRate)]
    public async Task ParseResultId_MapsSuccessResultIds(string resultId, XmlDaErrorCode expected) {
        await Assert.That(XmlDaErrorCodes.ParseResultId(resultId)).IsEqualTo(expected);
    }

    [Test]
    [Arguments(XmlDaErrorCode.Ok)]
    [Arguments(XmlDaErrorCode.Clamp)]
    [Arguments(XmlDaErrorCode.DataQueueOverflow)]
    [Arguments(XmlDaErrorCode.UnsupportedRate)]
    public async Task IsSuccess_ReturnsTrue_ForSuccessCodes(XmlDaErrorCode code) {
        await Assert.That(code.IsSuccess()).IsTrue();
    }

    [Test]
    [Arguments(XmlDaErrorCode.Unknown)]
    [Arguments(XmlDaErrorCode.AccessDenied)]
    [Arguments(XmlDaErrorCode.Busy)]
    [Arguments(XmlDaErrorCode.Fail)]
    [Arguments(XmlDaErrorCode.InvalidContinuationPoint)]
    [Arguments(XmlDaErrorCode.InvalidFilter)]
    [Arguments(XmlDaErrorCode.InvalidHoldTime)]
    [Arguments(XmlDaErrorCode.InvalidItemId)]
    [Arguments(XmlDaErrorCode.InvalidItemName)]
    [Arguments(XmlDaErrorCode.InvalidItemPath)]
    [Arguments(XmlDaErrorCode.InvalidPid)]
    [Arguments(XmlDaErrorCode.NoSubscription)]
    [Arguments(XmlDaErrorCode.NotSupported)]
    [Arguments(XmlDaErrorCode.OutOfMemory)]
    [Arguments(XmlDaErrorCode.Range)]
    [Arguments(XmlDaErrorCode.BadType)]
    [Arguments(XmlDaErrorCode.ReadOnly)]
    [Arguments(XmlDaErrorCode.ServerState)]
    [Arguments(XmlDaErrorCode.TimedOut)]
    [Arguments(XmlDaErrorCode.UnknownItemId)]
    [Arguments(XmlDaErrorCode.UnknownItemName)]
    [Arguments(XmlDaErrorCode.UnknownItemPath)]
    [Arguments(XmlDaErrorCode.WriteOnly)]
    [Arguments(XmlDaErrorCode.BadRights)]
    public async Task IsSuccess_ReturnsFalse_ForFaultCodes(XmlDaErrorCode code) {
        await Assert.That(code.IsSuccess()).IsFalse();
    }
}
