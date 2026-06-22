// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Discovery.Tests;

public sealed class OpcDiscoveryAdditionalTests
{
    [Test]
    public async Task OpcDiscovery_EnumerateAsync_Empty_categories_returns_empty_without_activation()
    {
        OpcServerDescriptor[] descriptors = await OpcDiscovery.EnumerateAsync(
            "  opc-host  ",
            Array.Empty<Guid>(),
            CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task OpcDiscovery_EnumerateAsync_With_connect_data_and_empty_categories_returns_empty_without_activation()
    {
        var connectData = OpcConnectData.WithNtlmV2(
            OpcUrl.Parse("opcda://opc-host/OPC.ServerList.1"),
            new NetworkCredential("operator", "password"),
            OpcProtectionLevel.Privacy);

        OpcServerDescriptor[] descriptors = await OpcDiscovery.EnumerateAsync(
            "opc-host",
            connectData,
            Array.Empty<Guid>(),
            CancellationToken.None);

        await Assert.That(descriptors.Length).IsEqualTo(0);
    }

    [Test]
    public async Task OpcDiscovery_EnumerateAsync_Blank_host_throws_argument_exception()
    {
        await Assert.That(async () => await OpcDiscovery.EnumerateAsync(
            "   ",
            Array.Empty<Guid>(),
            CancellationToken.None)).Throws<ArgumentException>();
    }

    [Test]
    public async Task OpcDiscovery_EnumerateAsync_Null_connect_data_throws_argument_null_exception()
    {
        await Assert.That(async () => await OpcDiscovery.EnumerateAsync(
            "opc-host",
            null!,
            Array.Empty<Guid>(),
            CancellationToken.None)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task OpcServerDescriptor_Construction_and_equality_preserve_category_values()
    {
        Guid[] categories =
        [
            OpcGuids.CATID_OPCDAServer20,
            OpcGuids.CATID_OPCDAServer30,
        ];
        var descriptor = new OpcServerDescriptor(
            Guid.Parse("10138C2C-0000-0000-0000-00000000D101"),
            "Vendor.Discovery.1",
            "Vendor Discovery",
            "Vendor.Discovery",
            categories);
        var same = new OpcServerDescriptor(
            Guid.Parse("10138C2C-0000-0000-0000-00000000D101"),
            "Vendor.Discovery.1",
            "Vendor Discovery",
            "Vendor.Discovery",
            categories);
        var differentProgId = descriptor with { ProgId = "Vendor.Other.1" };

        await Assert.That(descriptor.ClassId).IsEqualTo(Guid.Parse("10138C2C-0000-0000-0000-00000000D101"));
        await Assert.That(descriptor.ProgId).IsEqualTo("Vendor.Discovery.1");
        await Assert.That(descriptor.UserType).IsEqualTo("Vendor Discovery");
        await Assert.That(descriptor.VerIndProgId).IsEqualTo("Vendor.Discovery");
        await Assert.That(descriptor.Categories.Count).IsEqualTo(2);
        await Assert.That(descriptor.Categories[0]).IsEqualTo(OpcGuids.CATID_OPCDAServer20);
        await Assert.That(descriptor.Categories[1]).IsEqualTo(OpcGuids.CATID_OPCDAServer30);
        await Assert.That(descriptor).IsEqualTo(same);
        await Assert.That(descriptor == differentProgId).IsFalse();
    }

    [Test]
    public async Task OpcServerEntry_Construction_and_equality_preserve_host_and_categories()
    {
        Guid[] categories =
        [
            OpcGuids.CATID_OPCAEServer10,
            OpcGuids.CATID_OPCHDAServer10,
        ];
        var entry = new OpcServerEntry(
            Guid.Parse("10138C2C-0000-0000-0000-00000000D102"),
            "Vendor.Entry.1",
            "Vendor Entry",
            "opc-entry-host",
            categories);
        var same = new OpcServerEntry(
            Guid.Parse("10138C2C-0000-0000-0000-00000000D102"),
            "Vendor.Entry.1",
            "Vendor Entry",
            "opc-entry-host",
            categories);
        var differentHost = entry with { Host = "other-host" };

        await Assert.That(entry.Clsid).IsEqualTo(Guid.Parse("10138C2C-0000-0000-0000-00000000D102"));
        await Assert.That(entry.ProgId).IsEqualTo("Vendor.Entry.1");
        await Assert.That(entry.FriendlyName).IsEqualTo("Vendor Entry");
        await Assert.That(entry.Host).IsEqualTo("opc-entry-host");
        await Assert.That(entry.SupportedCategories.Count).IsEqualTo(2);
        await Assert.That(entry.SupportedCategories[0]).IsEqualTo(OpcGuids.CATID_OPCAEServer10);
        await Assert.That(entry.SupportedCategories[1]).IsEqualTo(OpcGuids.CATID_OPCHDAServer10);
        await Assert.That(entry).IsEqualTo(same);
        await Assert.That(entry == differentHost).IsFalse();
    }
}
