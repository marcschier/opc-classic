//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC DX (Data eXchange) DCOM-projection interfaces.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCConfiguration)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using Opc.Classic.Generators;

namespace Opc.Classic.Dx.Dcom;

/// <summary><c>IOPCConfiguration</c> — DX server-to-server configuration (IID_IOPCConfiguration).</summary>
[OpcInterface("C130D281-F4AA-4779-8846-C2C4CB444F2A")]
public partial interface IOPCConfiguration
{
    /// <summary><c>IOPCConfiguration::GetServers</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<DxSourceServer[]> QuerySourceServersAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::AddServers</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<DxGeneralResponse> AddSourceServersAsync(DxSourceServer[] sourceServers, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::ModifyServers</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<DxGeneralResponse> ModifySourceServersAsync(DxSourceServer[] sourceServers, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::DeleteServers</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task<DxGeneralResponse> DeleteSourceServersAsync(DxItemIdentifier[] sourceServers, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::CopyDefaultServerAttributes</c> (opnum 7).</summary>
    [OpcMethod(7)]
    Task<DxGeneralResponse> CopyDefaultServerAttributesAsync(bool configToStatus, DxItemIdentifier[] sourceServers, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::QueryDXConnections</c> (opnum 8).</summary>
    [OpcMethod(8)]
    Task<DxConnectionQueryResult> QueryDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive, CancellationToken cancellationToken = default);

    /// <summary>Convenience projection that returns connection names from <c>QueryDXConnections</c>.</summary>
    Task<string[]> QueryDXConnectionNamesAsync(string browsePath, string[] connectionMasks, bool recursive, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::AddDXConnections</c> (opnum 9).</summary>
    [OpcMethod(9)]
    Task<DxGeneralResponse> AddDXConnectionsAsync(DxConnection[] connections, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::UpdateDXConnections</c> (opnum 10).</summary>
    [OpcMethod(10)]
    Task<DxUpdateConnectionsResult> UpdateDXConnectionsAsync(string browsePath, DxConnection[] connectionMasks, bool recursive, DxConnection connectionDefinition, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::ModifyDXConnections</c> (opnum 11).</summary>
    [OpcMethod(11)]
    Task<DxGeneralResponse> ModifyDXConnectionsAsync(DxConnection[] connections, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::DeleteDXConnections</c> (opnum 12) projected as per-connection HRESULTs.</summary>
    [OpcMethod(12)]
    Task<int[]> DeleteDXConnectionsAsync(string browsePath, string[] connectionNames, bool recursive, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::CopyDXConnectionDefaultAttributes</c> (opnum 13).</summary>
    [OpcMethod(13)]
    Task<DxUpdateConnectionsResult> CopyDefaultDXConnectionAttributesAsync(bool configToStatus, string browsePath, DxConnection[] connectionMasks, bool recursive, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::ResetConfiguration</c> (opnum 14).</summary>
    [OpcMethod(14)]
    Task<string> ResetConfigurationAsync(string configurationVersion, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCDXServer</c> — managed DX server shim used by the OpcInterfaceGenerator pipeline.</summary>
[OpcInterface("D5D8F8E9-6F45-43F2-B19E-3FAE3DA88A7C")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCDXServer
{
    /// <summary><c>IOPCDXServer::GetServerName</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<string> GetServerNameAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCDXServer::GetVersion</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<string> GetVersionAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCDXServer::GetConfigurationVersion</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<string> GetConfigurationVersionAsync(CancellationToken cancellationToken = default);
}
