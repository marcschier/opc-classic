// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
// Replay test for the captured Matrikon GetProperties response.

using Opc.Classic.Da.Ndr;
using Opc.Classic.Da.Tests.Wire.Replay;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Tests.Wire;

public sealed class MatrikonGetPropertiesReplayTests
{
    private static string FixturePath()
    {
        string baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, "Wire", "Fixtures", "matrikon-getproperties-random-int4.hex");
    }

    [Test]
    public async Task Fixture_loads_with_expected_metadata_and_response_size()
    {
        WireCaptureFile capture = WireCaptureFile.Load(FixturePath());
        await Assert.That(capture.Metadata["iid"]).IsEqualTo("39227004-a18f-4b57-8b0a-5235670f4468");
        await Assert.That(capture.Metadata["opnum"]).IsEqualTo("3");
        await Assert.That(capture.ResponsePayload.Length).IsEqualTo(1932);
    }

    /// <summary>
    /// Replays the live Matrikon Simulation Server <c>IOPCBrowse::GetProperties</c>
    /// response through the decoder and verifies it produces the OPC DA 3.00 §A.1
    /// standard + recommended property set for <c>Random.Int4</c>.
    /// </summary>
    [Test]
    public async Task Replay_decodes_response_through_browse_decoder()
    {
        WireCaptureFile capture = WireCaptureFile.Load(FixturePath());
        OpcItemProperties[] items = DecodeCaptured(capture.ResponsePayload);

        await Assert.That(items.Length).IsEqualTo(1);
        await Assert.That(items[0].ErrorId).IsEqualTo(0);
        await Assert.That(items[0].Properties.Length).IsEqualTo(14);

        // Standard property set 1..8 + recommended Item Description (101) +
        // five Matrikon-private wave-form properties (-5..-1).
        int[] expectedIds = [1, 2, 3, 4, 5, 6, 7, 8, 101, -5, -4, -3, -2, -1];
        for (int i = 0; i < expectedIds.Length; i++)
        {
            await Assert.That(items[0].Properties[i].PropertyId).IsEqualTo(expectedIds[i]);
            await Assert.That(items[0].Properties[i].ItemId).IsEqualTo("Random.Int4");
            await Assert.That(items[0].Properties[i].ErrorId).IsEqualTo(0);
        }

        // Spec-mandated vtDataType for the standard set (OPC DA 3.00 §A.1).
        await Assert.That(items[0].Properties[0].DataType).IsEqualTo(VarType.VT_I2);          // canonical type
        await Assert.That(items[0].Properties[2].DataType).IsEqualTo(VarType.VT_I2);          // quality
        await Assert.That(items[0].Properties[4].DataType).IsEqualTo(VarType.VT_I4);          // access rights
        await Assert.That(items[0].Properties[5].DataType).IsEqualTo(VarType.VT_R4);          // scan rate
        await Assert.That(items[0].Properties[6].DataType).IsEqualTo(VarType.VT_I4);          // EU type

        // Standard property descriptions (proves itemId + description pile alignment).
        await Assert.That(items[0].Properties[0].Description).IsEqualTo("Item Canonical DataType");
        await Assert.That(items[0].Properties[1].Description).IsEqualTo("Item Value");
        await Assert.That(items[0].Properties[2].Description).IsEqualTo("Item Quality");
        await Assert.That(items[0].Properties[3].Description).IsEqualTo("Item Timestamp");
        await Assert.That(items[0].Properties[4].Description).IsEqualTo("Item Access Rights");
        await Assert.That(items[0].Properties[5].Description).IsEqualTo("Server Scan Rate");
        await Assert.That(items[0].Properties[6].Description).IsEqualTo("Item EU Type");
        await Assert.That(items[0].Properties[7].Description).IsEqualTo("Item EUInfo");
    }

    private static OpcItemProperties[] DecodeCaptured(byte[] response)
    {
        // Match the generated proxy code path verbatim — ReadItemPropertiesConformantArray
        // consumes the outer unique-pointer referent internally before reading the
        // array max_count, so the caller does NOT pre-read it.
        var reader = new NdrReader(response);
        return NdrOpcBrowseResponseDecoder.ReadItemPropertiesConformantArray(ref reader);
    }
}
