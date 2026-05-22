//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using OpcClassic;
using OpcClassic.Xml;
using TUnit.Core;

namespace OpcClassic.Xml.Tests;

public sealed class XmlDaQualityCompatTests
{
    [Test]
    public async Task ToWireByte_DropsHighByte()
    {
        // RawValue with vendor extension 0xAB in the high byte and quality
        // bits in the low byte. The wire byte must be just the low byte.
        var quality = new OpcQuality(0xAB_C0);  // good (0xC0) + vendor 0xAB
        await Assert.That(XmlDaQualityCompat.ToWireByte(quality)).IsEqualTo((byte)0xC0);
    }

    [Test]
    public async Task FromWireByte_PreservesLowByteSetsHighToZero()
    {
        var quality = XmlDaQualityCompat.FromWireByte(0xC0);
        await Assert.That((int)quality.RawValue).IsEqualTo(0xC0);
    }

    [Test]
    public async Task RoundTrip_NoVendorExtension_PreservesValue()
    {
        var input = new OpcQuality(0x44);  // bad quality, some sub-status
        var wire = XmlDaQualityCompat.ToWireByte(input);
        var back = XmlDaQualityCompat.FromWireByte(wire);
        await Assert.That(back).IsEqualTo(input);
    }
}
