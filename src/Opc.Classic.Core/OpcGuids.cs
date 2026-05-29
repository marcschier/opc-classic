//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Comprehensive registry of OPC Foundation Classic IIDs, CLSIDs, and CATIDs.
// Values are sourced from the OPC Foundation interface IDL headers (verified
// against the [Guid] attributes that previously lived in DotNet/Rcw/*.cs).
//
// Identifier naming preserves the OPC Foundation IDL convention exactly
// (IID_*, CLSID_*, CATID_*) — consumers grep / cross-reference these against
// the canonical OPC specs and the native C++ samples preserved under ext/samples/.
// CA1707 (no underscores) is suppressed for this file only.
//

#pragma warning disable CA1707 // OPC IDL naming convention preserves underscores
#pragma warning disable IDE1006 // CA1707 covers the same naming concern

using System;

namespace Opc.Classic;

/// <summary>
/// Static registry of every OPC Classic interface identifier (IID), class
/// identifier (CLSID), and component category identifier (CATID) used by the
/// OPC Foundation specifications.
/// </summary>
/// <remarks>
/// <para>
/// Naming follows the OPC Foundation IDL conventions:
/// <list type="bullet">
///   <item><description><c>IID_*</c> — RPC interface identifier (UUID for a vtable-based interface)</description></item>
///   <item><description><c>CLSID_*</c> — class identifier (UUID for an instantiable COM class)</description></item>
///   <item><description><c>CATID_*</c> — component category identifier (UUID a server registers under to advertise capability)</description></item>
/// </list>
/// </para>
/// <para>
/// This class is <see langword="static"/> and AOT-clean — each constant is a
/// pre-computed <see cref="Guid"/> with no runtime parsing.
/// </para>
/// </remarks>
public static class OpcGuids
{
    // ============================================================
    // Common COM infrastructure (interfaces used across specs)
    // ============================================================

    /// <summary><c>IUnknown</c> — root COM interface.</summary>
    public static readonly Guid IID_IUnknown =
        new("00000000-0000-0000-C000-000000000046");

    /// <summary><c>IActivation</c> — legacy DCOM remote activation RPC interface.</summary>
    public static readonly Guid IID_IActivation =
        new("4D9F4AB8-7D1C-11CF-861E-0020AF6E7C57");

    /// <summary><c>winreg</c> — MS-RRP remote registry RPC interface.</summary>
    public static readonly Guid IID_WINREG =
        new("338CD001-2244-31F1-AAAA-900038001003");

    /// <summary><c>IDispatch</c> — Automation late-binding interface.</summary>
    public static readonly Guid IID_IDispatch =
        new("00020400-0000-0000-C000-000000000046");

    /// <summary><c>IEnumUnknown</c> — generic enumeration over IUnknowns.</summary>
    public static readonly Guid IID_IEnumUnknown =
        new("00000100-0000-0000-C000-000000000046");

    /// <summary><c>IEnumString</c> — string enumeration.</summary>
    public static readonly Guid IID_IEnumString =
        new("00000101-0000-0000-C000-000000000046");

    /// <summary><c>IEnumGUID</c> — GUID enumeration (used by category manager).</summary>
    public static readonly Guid IID_IEnumGUID =
        new("0002E000-0000-0000-C000-000000000046");

    /// <summary><c>IConnectionPointContainer</c> — exposes connection-point sinks.</summary>
    public static readonly Guid IID_IConnectionPointContainer =
        new("B196B284-BAB4-101A-B69C-00AA00341D07");

    /// <summary><c>IEnumConnectionPoints</c> — connection-point enumeration.</summary>
    public static readonly Guid IID_IEnumConnectionPoints =
        new("B196B285-BAB4-101A-B69C-00AA00341D07");

    /// <summary><c>IConnectionPoint</c> — single connection-point endpoint.</summary>
    public static readonly Guid IID_IConnectionPoint =
        new("B196B286-BAB4-101A-B69C-00AA00341D07");

    /// <summary><c>IEnumConnections</c> — connection enumeration.</summary>
    public static readonly Guid IID_IEnumConnections =
        new("B196B287-BAB4-101A-B69C-00AA00341D07");

    /// <summary><c>IOPCCommon</c> — common server interface (locale, error text, ...).</summary>
    public static readonly Guid IID_IOPCCommon =
        new("F31DFDE2-07B6-11D2-B2D8-0060083BA1FB");

    /// <summary><c>IOPCShutdown</c> — server-to-client shutdown notification sink.</summary>
    public static readonly Guid IID_IOPCShutdown =
        new("F31DFDE1-07B6-11D2-B2D8-0060083BA1FB");

    // ============================================================
    // OpcEnum — server discovery (the well-known component-category manager)
    // ============================================================

    /// <summary><c>OpcEnum</c> class — the cross-spec server discovery component.</summary>
    public static readonly Guid CLSID_OpcEnum =
        new("13486D51-4821-11D2-A494-3CB306C10000");

    /// <summary><c>IOPCServerList</c> — original server-discovery interface (deprecated).</summary>
    public static readonly Guid IID_IOPCServerList =
        new("13486D50-4821-11D2-A494-3CB306C10000");

    /// <summary><c>IOPCServerList2</c> — extended server-discovery interface (use this).</summary>
    public static readonly Guid IID_IOPCServerList2 =
        new("9DD0B56C-AD9E-43EE-8305-487F3188BF7A");

    /// <summary><c>IOPCEnumGUID</c> — class-of-category enumeration.</summary>
    public static readonly Guid IID_IOPCEnumGUID =
        new("55C382C8-21C7-4E88-96C1-BECFB1E3F483");

    // ============================================================
    // OPC Data Access (DA 2.0 / 2.05a / 3.0)
    // ============================================================

    /// <summary><c>CATID_OPCDAServer10</c> — registers a DA 1.0 server.</summary>
    public static readonly Guid CATID_OPCDAServer10 =
        new("63D5F430-CFE4-11D1-B2C8-0060083BA1FB");

    /// <summary><c>CATID_OPCDAServer20</c> — registers a DA 2.x server.</summary>
    public static readonly Guid CATID_OPCDAServer20 =
        new("63D5F432-CFE4-11D1-B2C8-0060083BA1FB");

    /// <summary><c>CATID_OPCDAServer30</c> — registers a DA 3.0 server.</summary>
    public static readonly Guid CATID_OPCDAServer30 =
        new("CC603642-66D7-48F1-B69A-B625E73652D7");

    /// <summary><c>IOPCServer</c> — top-level DA server interface.</summary>
    public static readonly Guid IID_IOPCServer =
        new("39C13A4D-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCServerPublicGroups</c> — deprecated public-group surface.</summary>
    public static readonly Guid IID_IOPCServerPublicGroups =
        new("39C13A4E-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCBrowseServerAddressSpace</c> — DA 2.x browse interface.</summary>
    public static readonly Guid IID_IOPCBrowseServerAddressSpace =
        new("39C13A4F-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCGroupStateMgt</c> — group state (active, rate, deadband, ...).</summary>
    public static readonly Guid IID_IOPCGroupStateMgt =
        new("39C13A50-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCPublicGroupStateMgt</c> — deprecated public-group state mgmt.</summary>
    public static readonly Guid IID_IOPCPublicGroupStateMgt =
        new("39C13A51-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCSyncIO</c> — DA 2.x synchronous read/write.</summary>
    public static readonly Guid IID_IOPCSyncIO =
        new("39C13A52-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCAsyncIO</c> — original DA asynchronous I/O (deprecated, use IOPCAsyncIO2).</summary>
    public static readonly Guid IID_IOPCAsyncIO =
        new("39C13A53-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCItemMgt</c> — group item management (add/validate/remove/SetActive).</summary>
    public static readonly Guid IID_IOPCItemMgt =
        new("39C13A54-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IEnumOPCItemAttributes</c> — enumeration of group item attributes.</summary>
    public static readonly Guid IID_IEnumOPCItemAttributes =
        new("39C13A55-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCDataCallback</c> — DA subscription callback sink.</summary>
    public static readonly Guid IID_IOPCDataCallback =
        new("39C13A70-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCAsyncIO2</c> — DA 2.05a asynchronous I/O (current).</summary>
    public static readonly Guid IID_IOPCAsyncIO2 =
        new("39C13A71-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCItemProperties</c> — DA 2.x item-property interface.</summary>
    public static readonly Guid IID_IOPCItemProperties =
        new("39C13A72-011E-11D0-9675-0020AFD8ADB3");

    /// <summary><c>IOPCItemDeadbandMgt</c> — DA 3.0 per-item deadband management.</summary>
    public static readonly Guid IID_IOPCItemDeadbandMgt =
        new("5946DA93-8B39-4EC8-AB3D-AA73DF5BC86F");

    /// <summary><c>IOPCItemSamplingMgt</c> — DA 3.0 per-item sampling rate.</summary>
    public static readonly Guid IID_IOPCItemSamplingMgt =
        new("3E22D313-F08B-41A5-86C8-95E95CB49FFC");

    /// <summary><c>IOPCBrowse</c> — DA 3.0 unified browse interface.</summary>
    public static readonly Guid IID_IOPCBrowse =
        new("39227004-A18F-4B57-8B0A-5235670F4468");

    /// <summary><c>IOPCItemIO</c> — DA 3.0 stateless item I/O.</summary>
    public static readonly Guid IID_IOPCItemIO =
        new("85C0B427-2893-4CBC-BD78-E5FC5146F08F");

    /// <summary><c>IOPCSyncIO2</c> — DA 3.0 max-age synchronous I/O.</summary>
    public static readonly Guid IID_IOPCSyncIO2 =
        new("730F5F0F-55B1-4C81-9E18-FF8A0904E1FA");

    /// <summary><c>IOPCAsyncIO3</c> — DA 3.0 max-age asynchronous I/O.</summary>
    public static readonly Guid IID_IOPCAsyncIO3 =
        new("0967B97B-36EF-423E-B6F8-6BFF1E40D39D");

    /// <summary><c>IOPCGroupStateMgt2</c> — DA 3.0 keepalive support.</summary>
    public static readonly Guid IID_IOPCGroupStateMgt2 =
        new("8E368666-D72E-4F78-87ED-647611C61C9F");

    // ============================================================
    // OPC Alarms & Events (AE 1.0 / 1.10)
    // ============================================================

    /// <summary><c>CATID_OPCAEServer10</c> — registers an AE 1.0 / 1.10 server.</summary>
    public static readonly Guid CATID_OPCAEServer10 =
        new("58E13251-AC87-11D1-84D5-00608CB8A7E9");

    /// <summary><c>IOPCEventServer</c> — top-level AE server interface.</summary>
    public static readonly Guid IID_IOPCEventServer =
        new("65168851-5783-11D1-84A0-00608CB8A7E9");

    /// <summary><c>IOPCEventSubscriptionMgt</c> — AE event subscription management.</summary>
    public static readonly Guid IID_IOPCEventSubscriptionMgt =
        new("65168855-5783-11D1-84A0-00608CB8A7E9");

    /// <summary><c>IOPCEventAreaBrowser</c> — AE area-namespace browser.</summary>
    public static readonly Guid IID_IOPCEventAreaBrowser =
        new("65168857-5783-11D1-84A0-00608CB8A7E9");

    /// <summary><c>IOPCEventSink</c> — AE event-delivery callback sink.</summary>
    public static readonly Guid IID_IOPCEventSink =
        new("6516885F-5783-11D1-84A0-00608CB8A7E9");

    /// <summary><c>IOPCEventServer2</c> — AE 1.10 enable-/disable-conditions extensions.</summary>
    public static readonly Guid IID_IOPCEventServer2 =
        new("71BBE88E-9564-4BCD-BCFC-71C558D94F2D");

    /// <summary><c>IOPCEventSubscriptionMgt2</c> — AE 1.10 keep-alive extensions.</summary>
    public static readonly Guid IID_IOPCEventSubscriptionMgt2 =
        new("94C955DC-3684-4CCB-AFAB-F898CE19AAC3");

    // ============================================================
    // OPC Historical Data Access (HDA 1.x)
    // ============================================================

    /// <summary><c>CATID_OPCHDAServer10</c> — registers an HDA 1.x server.</summary>
    public static readonly Guid CATID_OPCHDAServer10 =
        new("7DE5B060-E089-11D2-A5E6-000086339399");

    /// <summary><c>IOPCHDA_Server</c> — top-level HDA server interface.</summary>
    public static readonly Guid IID_IOPCHDA_Server =
        new("1F1217B0-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_Browser</c> — HDA address-space browse.</summary>
    public static readonly Guid IID_IOPCHDA_Browser =
        new("1F1217B1-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_SyncRead</c> — synchronous HDA read.</summary>
    public static readonly Guid IID_IOPCHDA_SyncRead =
        new("1F1217B2-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_SyncUpdate</c> — synchronous HDA insert/replace/delete.</summary>
    public static readonly Guid IID_IOPCHDA_SyncUpdate =
        new("1F1217B3-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_SyncAnnotations</c> — synchronous HDA annotation management.</summary>
    public static readonly Guid IID_IOPCHDA_SyncAnnotations =
        new("1F1217B4-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_AsyncRead</c> — asynchronous HDA read.</summary>
    public static readonly Guid IID_IOPCHDA_AsyncRead =
        new("1F1217B5-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_AsyncUpdate</c> — asynchronous HDA insert/replace/delete.</summary>
    public static readonly Guid IID_IOPCHDA_AsyncUpdate =
        new("1F1217B6-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_AsyncAnnotations</c> — asynchronous HDA annotation management.</summary>
    public static readonly Guid IID_IOPCHDA_AsyncAnnotations =
        new("1F1217B7-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_Playback</c> — HDA playback (server pushes history at rate).</summary>
    public static readonly Guid IID_IOPCHDA_Playback =
        new("1F1217B8-DEE0-11D2-A5E5-000086339399");

    /// <summary><c>IOPCHDA_DataCallback</c> — HDA async-read / playback callback sink.</summary>
    public static readonly Guid IID_IOPCHDA_DataCallback =
        new("1F1217B9-DEE0-11D2-A5E5-000086339399");

    // ============================================================
    // OPC Data eXchange (DX 1.0)
    // ============================================================

    /// <summary><c>CATID_OPCDXServer10</c> — registers a DX 1.0 server.</summary>
    public static readonly Guid CATID_OPCDXServer10 =
        new("A0C85BB8-4161-4FD6-8655-BB584601C9E0");

    /// <summary><c>IOPCConfiguration</c> — DX server-to-server configuration.</summary>
    public static readonly Guid IID_IOPCConfiguration =
        new("C130D281-F4AA-4779-8846-C2C4CB444F2A");

    // ============================================================
    // OPC Batch (Batch 1.0 / 2.0)
    // ============================================================

    /// <summary><c>CATID_OPCBatchServer10</c> — registers a Batch 1.0 server.</summary>
    public static readonly Guid CATID_OPCBatchServer10 =
        new("A8080DA0-E23E-11D2-AFA7-00C04F539421");

    /// <summary><c>CATID_OPCBatchServer20</c> — registers a Batch 2.0 server.</summary>
    public static readonly Guid CATID_OPCBatchServer20 =
        new("843DE67B-B0C9-11D4-A0B7-000102A980B1");

    /// <summary><c>IOPCBatchServer</c> — Batch 1.0 top-level server interface.</summary>
    public static readonly Guid IID_IOPCBatchServer =
        new("8BB4ED50-B314-11D3-B3EA-00C04F8ECEAA");

    /// <summary><c>IOPCBatchServer2</c> — Batch 2.0 top-level server interface.</summary>
    public static readonly Guid IID_IOPCBatchServer2 =
        new("895A78CF-B0C5-11D4-A0B7-000102A980B1");

    /// <summary><c>IEnumOPCBatchSummary</c> — enumeration of batch summaries.</summary>
    public static readonly Guid IID_IEnumOPCBatchSummary =
        new("A8080DA2-E23E-11D2-AFA7-00C04F539421");

    /// <summary><c>IOPCEnumerationSets</c> — Batch enumeration sets.</summary>
    public static readonly Guid IID_IOPCEnumerationSets =
        new("A8080DA3-E23E-11D2-AFA7-00C04F539421");

    // ============================================================
    // OPC Commands (Commands 1.0)
    // ============================================================

    /// <summary><c>CATID_OPCCMDServer10</c> — registers a Commands 1.0 server.</summary>
    public static readonly Guid CATID_OPCCMDServer10 =
        new("2D869D5C-3B05-41FB-851A-642FB2B801A0");

    /// <summary><c>IOPCCommandInformation</c> — Commands metadata interface.</summary>
    public static readonly Guid IID_IOPCCommandInformation =
        new("3104B525-2016-442D-9696-1275DE978778");

    /// <summary><c>IOPCCommandExecution</c> — Commands execution interface.</summary>
    public static readonly Guid IID_IOPCCommandExecution =
        new("3104B526-2016-442D-9696-1275DE978778");

    /// <summary><c>IOPCCommandCallback</c> — Commands progress / completion sink.</summary>
    public static readonly Guid IID_IOPCCommandCallback =
        new("3104B527-2016-442D-9696-1275DE978778");

    // ============================================================
    // OPC Security
    // ============================================================

    /// <summary><c>IOPCSecurityNT</c> — Windows-integrated security.</summary>
    public static readonly Guid IID_IOPCSecurityNT =
        new("7AA83A01-6C77-11D3-84F9-00008630A38B");

    /// <summary><c>IOPCSecurityPrivate</c> — server-private credential security.</summary>
    public static readonly Guid IID_IOPCSecurityPrivate =
        new("7AA83A02-6C77-11D3-84F9-00008630A38B");

    // ============================================================
    // OPC XML-DA (HTTP/SOAP, not DCOM)
    // ============================================================

    /// <summary><c>CATID_XMLDAServer10</c> — registers an XML-DA 1.0 server.</summary>
    public static readonly Guid CATID_XMLDAServer10 =
        new("3098EDA4-A006-48B2-A27F-247453959408");

    // ============================================================
    // Spec-set lookup helpers
    // ============================================================

    /// <summary>
    /// The category IDs a client typically passes to
    /// <c>IOPCServerList2::EnumClassesOfCategories</c> to discover OPC DA
    /// servers (all versions).
    /// </summary>
    public static readonly Guid[] DaCategoryIds =
    {
        CATID_OPCDAServer10,
        CATID_OPCDAServer20,
        CATID_OPCDAServer30,
    };

    /// <summary>Category IDs for OPC AE server discovery.</summary>
    public static readonly Guid[] AeCategoryIds =
    {
        CATID_OPCAEServer10,
    };

    /// <summary>Category IDs for OPC HDA server discovery.</summary>
    public static readonly Guid[] HdaCategoryIds =
    {
        CATID_OPCHDAServer10,
    };

    /// <summary>Category IDs for OPC DX server discovery.</summary>
    public static readonly Guid[] DxCategoryIds =
    {
        CATID_OPCDXServer10,
    };

    /// <summary>Category IDs for OPC Batch server discovery (both Batch 1.0 and 2.0).</summary>
    public static readonly Guid[] BatchCategoryIds =
    {
        CATID_OPCBatchServer10,
        CATID_OPCBatchServer20,
    };

    /// <summary>Category IDs for OPC Commands server discovery.</summary>
    public static readonly Guid[] CommandsCategoryIds =
    {
        CATID_OPCCMDServer10,
    };

    /// <summary>Category IDs for OPC XML-DA server discovery.</summary>
    public static readonly Guid[] XmlDaCategoryIds =
    {
        CATID_XMLDAServer10,
    };
}
