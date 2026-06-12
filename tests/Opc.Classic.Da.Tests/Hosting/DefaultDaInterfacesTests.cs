//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Da.Hosting;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for the DA default implementations of the DA 2.x/3.0 browse,
/// property, deadband and sampling-rate interfaces. These guarantee
/// every conformant DA server presents the interface set even when the
/// user's IOpcDaServer doesn't explicitly implement them.
/// </summary>
public sealed class DefaultDaInterfacesTests
{
    [Test]
    public async Task DefaultBrowseServerAddressSpace_QueryOrganization_returns_flat()
    {
        var browse = new DefaultBrowseServerAddressSpace();

        int organization = await browse.QueryOrganizationAsync(TestContext.Current!.CancellationToken);

        await Assert.That(organization).IsEqualTo(1); // OPCNS_FLAT
    }

    [Test]
    public async Task DefaultBrowseServerAddressSpace_GetItemID_returns_input()
    {
        var browse = new DefaultBrowseServerAddressSpace();

        string itemId = await browse.GetItemIdAsync("Tag.A", TestContext.Current!.CancellationToken);

        await Assert.That(itemId).IsEqualTo("Tag.A");
    }

    [Test]
    public async Task DefaultBrowseServerAddressSpace_ChangeBrowsePosition_throws_NotSupported()
    {
        var browse = new DefaultBrowseServerAddressSpace();

        OpcException? ex = await Assert.That(async () =>
            await browse.ChangeBrowsePositionAsync(0, "x", TestContext.Current!.CancellationToken))
            .Throws<OpcException>();

        await Assert.That(ex).IsNotNull();
        await Assert.That(ex!.ResultId.Code).IsEqualTo(OpcResultId.NotSupported.Code);
    }

    [Test]
    public async Task DefaultItemProperties_QueryAvailableProperties_returns_OPC_standard_set()
    {
        var props = new DefaultItemProperties();

        await props.QueryAvailablePropertiesAsync(
            "Tag.A",
            out int[] ids,
            out string[] descriptions,
            out ushort[] types,
            TestContext.Current!.CancellationToken);

        // cap-b2 now publishes the OPC-standard property set (IDs 1-8).
        await Assert.That(ids.Length).IsEqualTo(8);
        await Assert.That(descriptions.Length).IsEqualTo(8);
        await Assert.That(types.Length).IsEqualTo(8);
    }

    [Test]
    public async Task DefaultItemProperties_GetItemProperties_returns_OPC_E_INVALID_PID()
    {
        var props = new DefaultItemProperties();
        int[] requestedIds = new[] { 100, 101, 102 };

        await props.GetItemPropertiesAsync(
            "Tag.A",
            requestedIds,
            out OpcVariant[] data,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(data.Length).IsEqualTo(3);
        await Assert.That(errors.Length).IsEqualTo(3);
        await Assert.That(errors[0]).IsEqualTo(OpcResultId.InvalidPid.Code);
    }

    [Test]
    public async Task DefaultBrowse_GetProperties_returns_one_entry_per_item()
    {
        var browse = new DefaultBrowse();
        string[] itemIds = new[] { "Tag.A", "Tag.B" };

        OpcItemProperties[] result = await browse.GetPropertiesAsync(
            itemIds,
            returnPropertyValues: false,
            propertyIds: Array.Empty<int>(),
            TestContext.Current!.CancellationToken);

        await Assert.That(result.Length).IsEqualTo(2);
        await Assert.That(result[0].Properties.Length).IsEqualTo(0);
    }

    [Test]
    public async Task DefaultBrowse_Browse_returns_no_elements()
    {
        var browse = new DefaultBrowse();
        string? continuation = null;

        await browse.BrowseAsync(
            "",
            ref continuation,
            maxElementsReturned: 10,
            browseFilter: 0,
            elementNameFilter: "*",
            vendorFilter: "",
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: Array.Empty<int>(),
            out bool moreElements,
            out OpcBrowseElementResult[] elements,
            TestContext.Current!.CancellationToken);

        await Assert.That(moreElements).IsFalse();
        await Assert.That(elements.Length).IsEqualTo(0);
    }

    [Test]
    public async Task DefaultItemDeadbandMgt_SetItemDeadband_returns_DEADBANDNOTSUPPORTED_per_handle()
    {
        var d = new DefaultItemDeadbandMgt();
        int[] handles = new[] { 1, 2, 3 };
        float[] deadbands = new[] { 0.5f, 1.0f, 2.0f };

        int[] errors = await d.SetItemDeadbandAsync(handles, deadbands, TestContext.Current!.CancellationToken);

        await Assert.That(errors.Length).IsEqualTo(3);
        await Assert.That(errors[0]).IsEqualTo(OpcResultId.DeadbandNotSupported.Code);
    }

    [Test]
    public async Task DefaultItemSamplingMgt_SetItemSamplingRate_returns_RATENOTSET_per_handle()
    {
        var s = new DefaultItemSamplingMgt();
        int[] handles = new[] { 1, 2 };
        int[] rates = new[] { 100, 200 };

        await s.SetItemSamplingRateAsync(
            handles,
            rates,
            out int[] revised,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(revised.Length).IsEqualTo(2);
        await Assert.That(errors[0]).IsEqualTo(OpcResultId.RateNotSet.Code);
    }

    [Test]
    public async Task DefaultItemSamplingMgt_SetItemBufferEnable_returns_NOBUFFERING_per_handle()
    {
        var s = new DefaultItemSamplingMgt();
        int[] handles = new[] { 1 };
        bool[] enabled = new[] { true };

        int[] errors = await s.SetItemBufferEnableAsync(handles, enabled, TestContext.Current!.CancellationToken);

        await Assert.That(errors.Length).IsEqualTo(1);
        await Assert.That(errors[0]).IsEqualTo(OpcResultId.NoBuffering.Code);
    }
}
