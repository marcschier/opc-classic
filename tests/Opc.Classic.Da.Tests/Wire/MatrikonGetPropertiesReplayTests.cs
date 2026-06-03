// SPDX-License-Identifier: MIT
// Replay test for the captured Matrikon GetProperties response.

using System;
using System.IO;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Da.Tests.Wire.Replay;
using Opc.Classic.Ndr;
using TUnit.Core;

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
    /// Pinned regression marker for <c>ag-get-properties-decode</c>: replays the
    /// live Matrikon Simulation Server <c>IOPCBrowse::GetProperties</c> response
    /// through our spec-compliant decoder and asserts that it currently throws
    /// the expected NDR drift exception. Matrikon's proxy emits a non-standard
    /// 28-byte inline OPCITEMPROPERTY layout (see Track AY notes in
    /// <c>docs/interop/unblocking-get-properties-decode.md</c>) instead of the
    /// 40+ byte spec layout MIDL would produce.
    /// </summary>
    /// <remarks>
    /// When the Matrikon-shape decoder lands, this test MUST be flipped to
    /// assert on the decoded property values (count = 14, propIds 1..8 + 101 +
    /// the five vendor-private IDs). Until then, keeping the assertion on the
    /// EXPECTED failure keeps CI green while documenting that the fixture is
    /// being exercised.
    /// </remarks>
    [Test]
    public async Task Replay_throws_expected_drift_until_matrikon_shape_decoder_lands()
    {
        WireCaptureFile capture = WireCaptureFile.Load(FixturePath());

        var ex = Assert.Throws<InvalidDataException>(() => DecodeCaptured(capture.ResponsePayload));
        await Assert.That(ex.Message).Contains("NDR VARIANT wire decoding is not supported for type");
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
