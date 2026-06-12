//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Hosting;

/// <summary>
/// Standard OPC Foundation component-category identifiers (CATIDs) for the OPC
/// Classic server specifications. Values are verified against the published OPC
/// Foundation IDL definitions in <c>interop\inc\opcda_i.c</c>,
/// <c>opc_ae_i.c</c>, <c>opchda_i.c</c>, <c>OpcDx_i.c</c>, and <c>opcbc_i.c</c>.
/// </summary>
public static class OpcComponentCategories
{
    /// <summary><c>CATID_OPCDAServer10</c> — OPC Data Access Servers Version 1.0.</summary>
    public static readonly OpcComponentCategory OpcDaServer10 = new(
        Guid.Parse("63D5F430-CFE4-11d1-B2C8-0060083BA1FB"),
        "OPC Data Access Servers Version 1.0");

    /// <summary><c>CATID_OPCDAServer20</c> — OPC Data Access Servers Version 2.0.</summary>
    public static readonly OpcComponentCategory OpcDaServer20 = new(
        Guid.Parse("63D5F432-CFE4-11d1-B2C8-0060083BA1FB"),
        "OPC Data Access Servers Version 2.0");

    /// <summary><c>CATID_OPCDAServer30</c> — OPC Data Access Servers Version 3.0.</summary>
    public static readonly OpcComponentCategory OpcDaServer30 = new(
        Guid.Parse("CC603642-66D7-48f1-B69A-B625E73652D7"),
        "OPC Data Access Servers Version 3.0");

    /// <summary><c>CATID_OPCAEServer10</c> — OPC Alarm &amp; Event Server Version 1.0.</summary>
    public static readonly OpcComponentCategory OpcAeServer10 = new(
        Guid.Parse("58E13251-AC87-11d1-84D5-00608CB8A7E9"),
        "OPC Alarm & Event Server Version 1.0");

    /// <summary><c>CATID_OPCHDAServer10</c> — OPC Historical Data Access Servers Version 1.0.</summary>
    public static readonly OpcComponentCategory OpcHdaServer10 = new(
        Guid.Parse("7DE5B060-E089-11d2-A5E6-000086339399"),
        "OPC Historical Data Access Servers Version 1.0");

    /// <summary><c>CATID_XMLDAServer10</c> — OPC XML Data Access Servers Version 1.0.</summary>
    public static readonly OpcComponentCategory OpcXmlDaServer10 = new(
        Guid.Parse("3098EDA4-A006-48b2-A27F-247453959408"),
        "OPC XML Data Access Servers Version 1.0");

    /// <summary><c>CATID_OPCDXServer10</c> — OPC Data eXchange Server Version 1.0.</summary>
    public static readonly OpcComponentCategory OpcDxServer10 = new(
        Guid.Parse("A0C85BB8-4161-4fd6-8655-BB584601C9E0"),
        "OPC Data eXchange Server Version 1.0");

    /// <summary><c>CATID_OPCBatchServer10</c> — OPC Batch Server Version 1.0.</summary>
    public static readonly OpcComponentCategory OpcBatchServer10 = new(
        Guid.Parse("A8080DA0-E23E-11D2-AFA7-00C04F539421"),
        "OPC Batch Server Version 1.0");

    /// <summary><c>CATID_OPCBatchServer20</c> — OPC Batch Server Version 2.0.</summary>
    public static readonly OpcComponentCategory OpcBatchServer20 = new(
        Guid.Parse("843DE67B-B0C9-11d4-A0B7-000102A980B1"),
        "OPC Batch Server Version 2.0");
}
