// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Tests;

/// <summary>
/// Spot-check the <see cref="OpcGuids"/> registry: known-canonical IIDs/CLSIDs/CATIDs
/// match their on-the-wire byte values, and the spec-grouping arrays contain
/// exactly the expected category IDs.
/// </summary>
public sealed class OpcGuidsTests
{
    [Test]
    [Arguments("IID_IUnknown", "00000000-0000-0000-C000-000000000046")]
    [Arguments("IID_IDispatch", "00020400-0000-0000-C000-000000000046")]
    [Arguments("IID_IOPCCommon", "F31DFDE2-07B6-11D2-B2D8-0060083BA1FB")]
    [Arguments("IID_IConnectionPointContainer", "B196B284-BAB4-101A-B69C-00AA00341D07")]
    [Arguments("CLSID_OpcEnum", "13486D51-4821-11D2-A494-3CB306C10000")]
    [Arguments("IID_IOPCServerList2", "9DD0B56C-AD9E-43EE-8305-487F3188BF7A")]
    public async Task CommonInfrastructureGuids_MatchOpcFoundationValues(string name, string expected)
    {
        var actual = LookupGuid(name);
        await Assert.That(actual).IsEqualTo(Guid.Parse(expected));
    }

    [Test]
    [Arguments("IID_IOPCServer", "39C13A4D-011E-11D0-9675-0020AFD8ADB3")]
    [Arguments("IID_IOPCBrowse", "39227004-A18F-4B57-8B0A-5235670F4468")]
    [Arguments("IID_IOPCItemMgt", "39C13A54-011E-11D0-9675-0020AFD8ADB3")]
    [Arguments("IID_IOPCSyncIO", "39C13A52-011E-11D0-9675-0020AFD8ADB3")]
    [Arguments("IID_IOPCAsyncIO2", "39C13A71-011E-11D0-9675-0020AFD8ADB3")]
    [Arguments("IID_IOPCDataCallback", "39C13A70-011E-11D0-9675-0020AFD8ADB3")]
    [Arguments("IID_IOPCItemIO", "85C0B427-2893-4CBC-BD78-E5FC5146F08F")]
    [Arguments("CATID_OPCDAServer10", "63D5F430-CFE4-11D1-B2C8-0060083BA1FB")]
    [Arguments("CATID_OPCDAServer20", "63D5F432-CFE4-11D1-B2C8-0060083BA1FB")]
    [Arguments("CATID_OPCDAServer30", "CC603642-66D7-48F1-B69A-B625E73652D7")]
    public async Task DaGuids_MatchOpcFoundationValues(string name, string expected)
    {
        var actual = LookupGuid(name);
        await Assert.That(actual).IsEqualTo(Guid.Parse(expected));
    }

    [Test]
    [Arguments("IID_IOPCEventServer", "65168851-5783-11D1-84A0-00608CB8A7E9")]
    [Arguments("IID_IOPCEventSubscriptionMgt", "65168855-5783-11D1-84A0-00608CB8A7E9")]
    [Arguments("IID_IOPCEventAreaBrowser", "65168857-5783-11D1-84A0-00608CB8A7E9")]
    [Arguments("IID_IOPCEventSink", "6516885F-5783-11D1-84A0-00608CB8A7E9")]
    [Arguments("CATID_OPCAEServer10", "58E13251-AC87-11D1-84D5-00608CB8A7E9")]
    public async Task AeGuids_MatchOpcFoundationValues(string name, string expected)
    {
        var actual = LookupGuid(name);
        await Assert.That(actual).IsEqualTo(Guid.Parse(expected));
    }

    [Test]
    [Arguments("IID_IOPCHDA_Server", "1F1217B0-DEE0-11D2-A5E5-000086339399")]
    [Arguments("IID_IOPCHDA_Browser", "1F1217B1-DEE0-11D2-A5E5-000086339399")]
    [Arguments("IID_IOPCHDA_SyncRead", "1F1217B2-DEE0-11D2-A5E5-000086339399")]
    [Arguments("IID_IOPCHDA_AsyncRead", "1F1217B5-DEE0-11D2-A5E5-000086339399")]
    [Arguments("IID_IOPCHDA_DataCallback", "1F1217B9-DEE0-11D2-A5E5-000086339399")]
    [Arguments("CATID_OPCHDAServer10", "7DE5B060-E089-11D2-A5E6-000086339399")]
    public async Task HdaGuids_MatchOpcFoundationValues(string name, string expected)
    {
        var actual = LookupGuid(name);
        await Assert.That(actual).IsEqualTo(Guid.Parse(expected));
    }

    [Test]
    [Arguments("CATID_OPCDXServer10", "A0C85BB8-4161-4FD6-8655-BB584601C9E0")]
    [Arguments("IID_IOPCConfiguration", "C130D281-F4AA-4779-8846-C2C4CB444F2A")]
    [Arguments("CATID_OPCBatchServer10", "A8080DA0-E23E-11D2-AFA7-00C04F539421")]
    [Arguments("CATID_OPCBatchServer20", "843DE67B-B0C9-11D4-A0B7-000102A980B1")]
    [Arguments("CATID_OPCCMDServer10", "2D869D5C-3B05-41FB-851A-642FB2B801A0")]
    [Arguments("IID_IOPCSecurityNT", "7AA83A01-6C77-11D3-84F9-00008630A38B")]
    [Arguments("IID_IOPCSecurityPrivate", "7AA83A02-6C77-11D3-84F9-00008630A38B")]
    [Arguments("CATID_XMLDAServer10", "3098EDA4-A006-48B2-A27F-247453959408")]
    public async Task OtherSpecsGuids_MatchOpcFoundationValues(string name, string expected)
    {
        var actual = LookupGuid(name);
        await Assert.That(actual).IsEqualTo(Guid.Parse(expected));
    }

    [Test]
    public async Task DaCategoryIds_ContainAllThreeVersions()
    {
        await Assert.That(OpcGuids.DaCategoryIds.Length).IsEqualTo(3);
        await Assert.That(OpcGuids.DaCategoryIds.Contains(OpcGuids.CATID_OPCDAServer10)).IsTrue();
        await Assert.That(OpcGuids.DaCategoryIds.Contains(OpcGuids.CATID_OPCDAServer20)).IsTrue();
        await Assert.That(OpcGuids.DaCategoryIds.Contains(OpcGuids.CATID_OPCDAServer30)).IsTrue();
    }

    [Test]
    public async Task BatchCategoryIds_ContainBothVersions()
    {
        await Assert.That(OpcGuids.BatchCategoryIds.Length).IsEqualTo(2);
        await Assert.That(OpcGuids.BatchCategoryIds.Contains(OpcGuids.CATID_OPCBatchServer10)).IsTrue();
        await Assert.That(OpcGuids.BatchCategoryIds.Contains(OpcGuids.CATID_OPCBatchServer20)).IsTrue();
    }

    [Test]
    public async Task AllSpecArrays_AreNonEmpty()
    {
        await Assert.That(OpcGuids.DaCategoryIds.Length).IsGreaterThan(0);
        await Assert.That(OpcGuids.AeCategoryIds.Length).IsGreaterThan(0);
        await Assert.That(OpcGuids.HdaCategoryIds.Length).IsGreaterThan(0);
        await Assert.That(OpcGuids.DxCategoryIds.Length).IsGreaterThan(0);
        await Assert.That(OpcGuids.BatchCategoryIds.Length).IsGreaterThan(0);
        await Assert.That(OpcGuids.CommandsCategoryIds.Length).IsGreaterThan(0);
        await Assert.That(OpcGuids.XmlDaCategoryIds.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task NoDuplicateGuids_AcrossEntireRegistry()
    {
        // Reflection on a static class with no instance state and no open generics
        // is AOT-safe (only the test exercises it; the library doesn't reflect on
        // OpcGuids at runtime). This catches copy-paste errors in IID definitions.
        var fields = typeof(OpcGuids).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(f => f.FieldType == typeof(Guid))
            .ToList();

        await Assert.That(fields.Count).IsGreaterThan(20);

        var values = fields.Select(f => (Guid)f.GetValue(null)!).ToList();
        var unique = values.Distinct().Count();

        // CATID_OPCAEServer10 and IOPCEventSink share a vendor namespace but are
        // distinct GUIDs. If a copy/paste error lands two fields on the same GUID,
        // this catches it.
        await Assert.That(unique).IsEqualTo(values.Count);
    }

    /// <summary>
    /// Look up a public static Guid field on <see cref="OpcGuids"/> by name.
    /// Test-only helper — uses reflection but the test project is not AOT-strict.
    /// </summary>
    private static Guid LookupGuid(string name)
    {
        var field = typeof(OpcGuids).GetField(name,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        if (field is null)
        {
            throw new InvalidOperationException($"OpcGuids has no public static field named '{name}'.");
        }
        return (Guid)field.GetValue(null)!;
    }
}
