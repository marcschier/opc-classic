//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Linq;
using System.Threading.Tasks;
using Opc.Classic.Da.Hosting;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting;

/// <summary>
/// Tests for the address-space abstractions (cap-b1/b2/b3):
/// FlatHierarchicalNamespace, InMemoryAddressSpace, the
/// DefaultBrowseServerAddressSpace (DA 2.x), DefaultBrowse (DA 3.0), and
/// DefaultItemProperties with the canonical OPC-standard property bag.
/// </summary>
public sealed class OpcAddressSpaceTests
{
    [Test]
    public async Task FlatHierarchicalNamespace_IsHierarchical_is_false()
    {
        var space = new FlatHierarchicalNamespace();
        OpcBrowseResult result = await space.BrowseAsync(null, OpcBrowseElementKind.All, TestContext.Current!.CancellationToken);

        await Assert.That(space.IsHierarchical).IsFalse();
        await Assert.That(result.Branches.Count).IsEqualTo(0);
        await Assert.That(result.Items.Count).IsEqualTo(0);
    }

    [Test]
    public async Task FlatHierarchicalNamespace_GetItemId_echoes_input()
    {
        var space = new FlatHierarchicalNamespace();
        string id = await space.GetItemIdAsync("any", "Tag.A", TestContext.Current!.CancellationToken);
        await Assert.That(id).IsEqualTo("Tag.A");
    }

    [Test]
    public async Task InMemoryAddressSpace_returns_root_branches()
    {
        var space = new InMemoryAddressSpace("Plant", "Diagnostics");
        OpcBrowseResult result = await space.BrowseAsync(null, OpcBrowseElementKind.All, TestContext.Current!.CancellationToken);

        await Assert.That(space.IsHierarchical).IsTrue();
        await Assert.That(result.Branches).Contains("Plant");
        await Assert.That(result.Branches).Contains("Diagnostics");
    }

    [Test]
    public async Task InMemoryAddressSpace_AddItem_under_root_appears_in_browse()
    {
        var space = new InMemoryAddressSpace();
        space.AddItem(string.Empty, "Tag.A");

        OpcBrowseResult result = await space.BrowseAsync(null, OpcBrowseElementKind.Items, TestContext.Current!.CancellationToken);

        await Assert.That(result.Items).Contains("Tag.A");
    }

    [Test]
    public async Task InMemoryAddressSpace_AddItem_under_nested_branch_creates_hierarchy()
    {
        var space = new InMemoryAddressSpace();
        space.AddBranch("Plant.Cooling.Pumps");
        space.AddItem("Plant.Cooling.Pumps", "Pump1.Status");

        OpcBrowseResult rootBranches = await space.BrowseAsync(null, OpcBrowseElementKind.Branches, TestContext.Current!.CancellationToken);
        OpcBrowseResult pumpsItems = await space.BrowseAsync("Plant.Cooling.Pumps", OpcBrowseElementKind.Items, TestContext.Current.CancellationToken);

        await Assert.That(rootBranches.Branches).Contains("Plant");
        await Assert.That(pumpsItems.Items).Contains("Pump1.Status");
    }

    [Test]
    public async Task InMemoryAddressSpace_GetItemId_concats_branch_path()
    {
        var space = new InMemoryAddressSpace();

        string id = await space.GetItemIdAsync("Plant.Cooling", "Pump1", TestContext.Current!.CancellationToken);

        await Assert.That(id).IsEqualTo("Plant.Cooling.Pump1");
    }

    [Test]
    public async Task DefaultBrowseServerAddressSpace_with_hierarchical_space_reports_OPCNS_HIERARCHIAL()
    {
        var space = new InMemoryAddressSpace("Plant");
        var browse = new DefaultBrowseServerAddressSpace(space);

        int org = await browse.QueryOrganizationAsync(TestContext.Current!.CancellationToken);

        await Assert.That(org).IsEqualTo(2);
    }

    [Test]
    public async Task DefaultBrowseServerAddressSpace_ChangeBrowsePosition_DOWN_navigates_into_branch()
    {
        var space = new InMemoryAddressSpace("Plant");
        space.AddItem("Plant", "Tag.A");
        var browse = new DefaultBrowseServerAddressSpace(space);

        await browse.ChangeBrowsePositionAsync(browseDirection: 1, "Plant", TestContext.Current!.CancellationToken);

        await Assert.That(browse.CurrentBrowsePosition).IsEqualTo("Plant");
        OpcBrowseResult snapshot = await browse.SnapshotCurrentBranchAsync(OpcBrowseElementKind.Items, TestContext.Current.CancellationToken);
        await Assert.That(snapshot.Items).Contains("Tag.A");
    }

    [Test]
    public async Task DefaultBrowseServerAddressSpace_ChangeBrowsePosition_UP_pops_branch()
    {
        var space = new InMemoryAddressSpace("Plant.Cooling");
        var browse = new DefaultBrowseServerAddressSpace(space);
        await browse.ChangeBrowsePositionAsync(browseDirection: 1, "Plant", TestContext.Current!.CancellationToken);
        await browse.ChangeBrowsePositionAsync(browseDirection: 1, "Cooling", TestContext.Current.CancellationToken);

        await browse.ChangeBrowsePositionAsync(browseDirection: 0, string.Empty, TestContext.Current.CancellationToken);

        await Assert.That(browse.CurrentBrowsePosition).IsEqualTo("Plant");
    }

    [Test]
    public async Task DefaultBrowseServerAddressSpace_ChangeBrowsePosition_TO_absolute_position()
    {
        var space = new InMemoryAddressSpace("Plant.Cooling");
        var browse = new DefaultBrowseServerAddressSpace(space);

        await browse.ChangeBrowsePositionAsync(browseDirection: 2, "Plant.Cooling", TestContext.Current!.CancellationToken);

        await Assert.That(browse.CurrentBrowsePosition).IsEqualTo("Plant.Cooling");
    }

    [Test]
    public async Task DefaultBrowseServerAddressSpace_GetItemId_uses_current_browse_position()
    {
        var space = new InMemoryAddressSpace("Plant");
        var browse = new DefaultBrowseServerAddressSpace(space);
        await browse.ChangeBrowsePositionAsync(browseDirection: 2, "Plant", TestContext.Current!.CancellationToken);

        string id = await browse.GetItemIdAsync("Tag.A", TestContext.Current.CancellationToken);

        await Assert.That(id).IsEqualTo("Plant.Tag.A");
    }

    [Test]
    public async Task DefaultBrowse_returns_branches_and_items_from_address_space()
    {
        var space = new InMemoryAddressSpace("Plant");
        space.AddItem("Plant", "Tag.A");
        space.AddItem("Plant", "Tag.B");
        var browse = new DefaultBrowse(space);
        string? continuation = null;

        await browse.BrowseAsync(
            "Plant",
            ref continuation,
            maxElementsReturned: 0,
            browseFilter: 1, // ALL
            elementNameFilter: "*",
            vendorFilter: "",
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: System.Array.Empty<int>(),
            out bool more,
            out OpcBrowseElementResult[] elements,
            TestContext.Current!.CancellationToken);

        await Assert.That(elements.Length).IsEqualTo(2);
        await Assert.That(more).IsFalse();
        await Assert.That(elements.All(e => e.IsItem)).IsTrue();
        string[] names = elements.Select(e => e.Name ?? string.Empty).ToArray();
        await Assert.That(names).Contains("Tag.A");
    }

    [Test]
    public async Task DefaultBrowse_truncates_to_maxElementsReturned_and_sets_moreElements()
    {
        var space = new InMemoryAddressSpace("Plant");
        for (int i = 0; i < 10; i++)
        {
            space.AddItem("Plant", $"Tag.{i}");
        }
        var browse = new DefaultBrowse(space);
        string? continuation = null;

        await browse.BrowseAsync(
            "Plant",
            ref continuation,
            maxElementsReturned: 5,
            browseFilter: 3, // ITEM
            elementNameFilter: "*",
            vendorFilter: "",
            returnAllProperties: false,
            returnPropertyValues: false,
            propertyIds: System.Array.Empty<int>(),
            out bool more,
            out OpcBrowseElementResult[] elements,
            TestContext.Current!.CancellationToken);

        await Assert.That(elements.Length).IsEqualTo(5);
        await Assert.That(more).IsTrue();
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

        await Assert.That(ids).Contains(OpcStandardProperties.CanonicalDataType);
        await Assert.That(ids).Contains(OpcStandardProperties.Value);
        await Assert.That(ids).Contains(OpcStandardProperties.Quality);
        await Assert.That(ids).Contains(OpcStandardProperties.Timestamp);
        await Assert.That(ids.Length).IsEqualTo(8);
        await Assert.That(descriptions.Length).IsEqualTo(ids.Length);
        await Assert.That(types.Length).IsEqualTo(ids.Length);
    }

    [Test]
    public async Task DefaultItemProperties_with_custom_provider_returns_provider_values()
    {
        var provider = new InMemoryPropertyProvider();
        provider.Set("Tag.A", OpcStandardProperties.CanonicalDataType, new OpcVariant(VarType.VT_I2, (short)VarType.VT_R8));
        var props = new DefaultItemProperties(provider);

        await props.GetItemPropertiesAsync(
            "Tag.A",
            new[] { OpcStandardProperties.CanonicalDataType, 999 },
            out OpcVariant[] data,
            out int[] errors,
            TestContext.Current!.CancellationToken);

        await Assert.That(errors[0]).IsEqualTo(OpcResultId.Ok.Code);
        await Assert.That(data[0].AsInt16()).IsEqualTo((short)VarType.VT_R8);
        await Assert.That(errors[1]).IsEqualTo(OpcResultId.InvalidPid.Code);
    }

    private sealed class InMemoryPropertyProvider : IOpcItemPropertyProvider
    {
        private readonly System.Collections.Generic.Dictionary<(string ItemId, int Pid), OpcVariant> _values = new();

        public void Set(string itemId, int propertyId, OpcVariant value) =>
            _values[(itemId, propertyId)] = value;

        public (OpcVariant Value, int Error) TryGetPropertyValue(string itemId, int propertyId) =>
            _values.TryGetValue((itemId, propertyId), out OpcVariant v)
                ? (v, OpcResultId.Ok.Code)
                : (OpcVariant.Empty, OpcResultId.InvalidPid.Code);
    }
}
