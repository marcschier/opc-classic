// Copyright (c) 2026 marcschier. Licensed under the MIT License.

#pragma warning disable TUnitAssertions0005 // Constants are verified against the OPC Foundation header as a single table.

namespace Opc.Classic.Hda.Tests;

public sealed class OpcHdaErrorsTests
{
    [Test]
    public async Task Constants_MatchOpcFoundationHeader()
    {
        await Assert.That(OpcHdaErrors.OPCHDA_E_MAXEXCEEDED).IsEqualTo(unchecked((int)0xC0041001u));
        await Assert.That(OpcHdaErrors.OPCHDA_S_NODATA).IsEqualTo(0x40041002);
        await Assert.That(OpcHdaErrors.OPCHDA_S_MOREDATA).IsEqualTo(0x40041003);
        await Assert.That(OpcHdaErrors.OPCHDA_E_INVALIDAGGREGATE).IsEqualTo(unchecked((int)0xC0041004u));
        await Assert.That(OpcHdaErrors.OPCHDA_S_CURRENTVALUE).IsEqualTo(0x40041005);
        await Assert.That(OpcHdaErrors.OPCHDA_S_EXTRADATA).IsEqualTo(0x40041006);
        await Assert.That(OpcHdaErrors.OPCHDA_W_NOFILTER).IsEqualTo(unchecked((int)0x80041007u));
        await Assert.That(OpcHdaErrors.OPCHDA_E_UNKNOWNATTRID).IsEqualTo(unchecked((int)0xC0041008u));
        await Assert.That(OpcHdaErrors.OPCHDA_E_NOT_AVAIL).IsEqualTo(unchecked((int)0xC0041009u));
        await Assert.That(OpcHdaErrors.OPCHDA_E_INVALIDDATATYPE).IsEqualTo(unchecked((int)0xC004100Au));
        await Assert.That(OpcHdaErrors.OPCHDA_E_DATAEXISTS).IsEqualTo(unchecked((int)0xC004100Bu));
        await Assert.That(OpcHdaErrors.OPCHDA_E_INVALIDATTRID).IsEqualTo(unchecked((int)0xC004100Cu));
        await Assert.That(OpcHdaErrors.OPCHDA_E_NODATAEXISTS).IsEqualTo(unchecked((int)0xC004100Du));
        await Assert.That(OpcHdaErrors.OPCHDA_S_INSERTED).IsEqualTo(0x4004100E);
        await Assert.That(OpcHdaErrors.OPCHDA_S_REPLACED).IsEqualTo(0x4004100F);
    }

    [Test]
    public async Task HeaderNameAliases_MatchHdaScopedConstants()
    {
        await Assert.That(OpcHdaErrors.OPC_E_MAXEXCEEDED).IsEqualTo(OpcHdaErrors.OPCHDA_E_MAXEXCEEDED);
        await Assert.That(OpcHdaErrors.OPC_S_NODATA).IsEqualTo(OpcHdaErrors.OPCHDA_S_NODATA);
        await Assert.That(OpcHdaErrors.OPC_S_MOREDATA).IsEqualTo(OpcHdaErrors.OPCHDA_S_MOREDATA);
        await Assert.That(OpcHdaErrors.OPC_E_INVALIDAGGREGATE).IsEqualTo(OpcHdaErrors.OPCHDA_E_INVALIDAGGREGATE);
        await Assert.That(OpcHdaErrors.OPC_S_CURRENTVALUE).IsEqualTo(OpcHdaErrors.OPCHDA_S_CURRENTVALUE);
        await Assert.That(OpcHdaErrors.OPC_S_EXTRADATA).IsEqualTo(OpcHdaErrors.OPCHDA_S_EXTRADATA);
        await Assert.That(OpcHdaErrors.OPC_W_NOFILTER).IsEqualTo(OpcHdaErrors.OPCHDA_W_NOFILTER);
        await Assert.That(OpcHdaErrors.OPC_E_UNKNOWNATTRID).IsEqualTo(OpcHdaErrors.OPCHDA_E_UNKNOWNATTRID);
        await Assert.That(OpcHdaErrors.OPC_E_NOT_AVAIL).IsEqualTo(OpcHdaErrors.OPCHDA_E_NOT_AVAIL);
        await Assert.That(OpcHdaErrors.OPC_E_INVALIDDATATYPE).IsEqualTo(OpcHdaErrors.OPCHDA_E_INVALIDDATATYPE);
        await Assert.That(OpcHdaErrors.OPC_E_DATAEXISTS).IsEqualTo(OpcHdaErrors.OPCHDA_E_DATAEXISTS);
        await Assert.That(OpcHdaErrors.OPC_E_INVALIDATTRID).IsEqualTo(OpcHdaErrors.OPCHDA_E_INVALIDATTRID);
        await Assert.That(OpcHdaErrors.OPC_E_NODATAEXISTS).IsEqualTo(OpcHdaErrors.OPCHDA_E_NODATAEXISTS);
        await Assert.That(OpcHdaErrors.OPC_S_INSERTED).IsEqualTo(OpcHdaErrors.OPCHDA_S_INSERTED);
        await Assert.That(OpcHdaErrors.OPC_S_REPLACED).IsEqualTo(OpcHdaErrors.OPCHDA_S_REPLACED);
    }
}
