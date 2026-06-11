//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// synthetic Matrikon-shaped fixtures for IOPCItemProperties::GetItemProperties.
//
// The blocker (ag-get-properties-decode) is that against a live Matrikon Simulation
// Server the response-side VARIANT[] decode reportedly hits an offset issue on certain
// VARTYPEs. The request side is in place; the failing surface is the
// per-element wireVARIANT envelope that our generator-emitted proxy reads via
// [OpcVariantElements] (NdrVariantExtensions.ReadVariantElement).
//
// These fixtures don't have a live capture to compare against; they instead pin every
// canonical OPC standard property (PropertyId 1..7) round-trips correctly through
// WriteVariantElement -> ReadVariantElement. If a future Matrikon capture lands and
// produces different bytes, the diff against these fixtures will pinpoint the
// vendor-specific padding / VARTYPE encoding the codec doesn't yet model.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using TUnit.Core;

namespace Opc.Classic.Da.Tests;

public sealed class GetItemPropertiesStandardSetFixtureTests
{
    /// <summary>
    /// OPC DA 3.00 §6.5 standard property set (subset used by simulation servers).
    /// Each property id is paired with the VARTYPE the spec mandates for the value.
    /// </summary>
    /// <remarks>
    /// Reference: OPC Foundation, "Data Access Custom Interface Standard", §6.5 +
    /// Appendix A1 (Properties).
    /// </remarks>
    public sealed record StandardProperty(int Id, string Name, VarType DataType, OpcVariant SampleValue);

    private static readonly StandardProperty[] s_standardSet =
    [
        new(1, "Item Canonical DataType", VarType.VT_I2, OpcVariant.FromInt16((short)VarType.VT_R4)),
        new(2, "Item Value", VarType.VT_R4, OpcVariant.FromSingle(42.5f)),
        new(3, "Item Quality", VarType.VT_I2, OpcVariant.FromInt16(192 /* Good */)),
        new(4, "Item Timestamp", VarType.VT_FILETIME, OpcVariant.FromFileTime(new DateTimeOffset(2026, 6, 3, 12, 0, 0, TimeSpan.Zero).ToFileTime())),
        new(5, "Item Access Rights", VarType.VT_I4, OpcVariant.FromInt32(0x3 /* READABLE | WRITEABLE */)),
        new(6, "Server Scan Rate", VarType.VT_R4, OpcVariant.FromSingle(100.0f)),
        new(7, "Item EU Type", VarType.VT_I4, OpcVariant.FromInt32(0 /* OPC_NO_ENUM */)),
    ];

    [Test]
    public async Task EachStandardProperty_RoundTrips_ThroughVariantElementCodec()
    {
        foreach (StandardProperty prop in s_standardSet)
        {
            // 1) Confirm the sample value has the spec-mandated VARTYPE.
            await Assert.That(prop.SampleValue.Type).IsEqualTo(prop.DataType);

            // 2) Round-trip through the per-element VARIANT codec (the path that
            //    GetItemProperties decodes its [out, size_is(,N)] VARIANT** ppvData
            //    through). Allocate a generously-sized buffer to absorb the
            //    pad-to-8 trailing alignment.
            byte[] buf = new byte[256];
            var writer = new NdrWriter(buf);
            writer.WriteVariantElement(prop.SampleValue);
            int written = writer.Position;

            var reader = new NdrReader(buf.AsMemory(0, written).Span);
            OpcVariant decoded = reader.ReadVariantElement();

            await Assert.That(decoded.Type).IsEqualTo(prop.DataType);
            await Assert.That(decoded).IsEqualTo(prop.SampleValue);
        }
    }

    [Test]
    public async Task MixedVariantArray_RoundTrips_PreservingOrderAndTypes()
    {
        // GetItemProperties response is the array variant of the per-element layout —
        // exercise the same codec used by [OpcVariantElements] with a typical 3-element
        // simulation server payload: scan rate (R4), access rights (I4), EU type (I4).
        OpcVariant[] inputs =
        [
            OpcVariant.FromSingle(1000.0f),
            OpcVariant.FromInt32(0x3),
            OpcVariant.FromInt32(0),
        ];

        byte[] buf = new byte[512];
        var writer = new NdrWriter(buf);
        foreach (OpcVariant v in inputs)
        {
            writer.WriteVariantElement(v);
        }

        int written = writer.Position;
        var reader = new NdrReader(buf.AsMemory(0, written).Span);
        OpcVariant[] decoded = new OpcVariant[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            decoded[i] = reader.ReadVariantElement();
        }

        for (int i = 0; i < inputs.Length; i++)
        {
            await Assert.That(decoded[i].Type).IsEqualTo(inputs[i].Type);
            await Assert.That(decoded[i]).IsEqualTo(inputs[i]);
        }
    }

    [Test]
    public async Task BstrEuInfo_VariantElementRoundTrip()
    {
        // OPC property #7 (EU Type) can be OPC_ANALOG (engineering range stored
        // separately), OPC_ENUMERATED (string array), or OPC_NO_ENUM. The
        // companion property "EU Info" carries a BSTR or BSTR[] payload. Pin the
        // singleton BSTR case (the path most likely to trip on alignment).
        var input = OpcVariant.FromString("degree Celsius");
        byte[] buf = new byte[256];
        var writer = new NdrWriter(buf);
        writer.WriteVariantElement(input);

        var reader = new NdrReader(buf.AsMemory(0, writer.Position).Span);
        OpcVariant decoded = reader.ReadVariantElement();

        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(decoded.AsString()).IsEqualTo("degree Celsius");
    }

    [Test]
    public async Task EmptyVariantElement_RoundTripsWithoutOffsetDrift()
    {
        // OPC servers commonly return VT_EMPTY for unsupported properties.
        // Verifying that the empty arm consumes the correct number of bytes
        // protects against the offset-drift class of bugs the live blocker
        // is suspected to fall into.
        var input = OpcVariant.Empty;
        byte[] buf = new byte[64];
        var writer = new NdrWriter(buf);
        writer.WriteVariantElement(input);
        int firstSize = writer.Position;
        writer.WriteVariantElement(input);
        int twoSize = writer.Position;

        // Two empty VARIANT elements must consume an integer multiple of the
        // per-element pad-to-8 stride. If the pad accounting drifts, the
        // second write will start at the wrong offset.
        await Assert.That(twoSize - firstSize).IsEqualTo(firstSize);

        var reader = new NdrReader(buf.AsMemory(0, twoSize).Span);
        OpcVariant first = reader.ReadVariantElement();
        OpcVariant second = reader.ReadVariantElement();
        int readerPosition = reader.Position;
        await Assert.That(first.Type).IsEqualTo(VarType.VT_EMPTY);
        await Assert.That(second.Type).IsEqualTo(VarType.VT_EMPTY);
        await Assert.That(readerPosition).IsEqualTo(twoSize);
    }

    [Test]
    public async Task FiletimeElement_PadsToEightByteStride()
    {
        // FILETIME is the variant arm with strictest alignment requirements
        // (AlignTo(8) before and pad to 8 after). Verifies pad accounting for
        // the property-#4 (Timestamp) path that Matrikon Simulation Server
        // returns on every readable item.
        var input = OpcVariant.FromFileTime(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero).ToFileTime());
        byte[] buf = new byte[64];
        var writer = new NdrWriter(buf);
        writer.WriteVariantElement(input);

        int written = writer.Position;
        await Assert.That(written % 8).IsEqualTo(0);

        var reader = new NdrReader(buf.AsMemory(0, written).Span);
        OpcVariant decoded = reader.ReadVariantElement();
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_FILETIME);
    }
}
