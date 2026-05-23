//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC DX (Data eXchange) DCOM-projection interfaces. Proxy generation is
// enabled for both DX projections; method coverage is limited to string,
// scalar, and string-array shapes until DX connection codecs are registered.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCConfiguration)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Generators;

namespace Opc.Classic.Dx.Dcom;

/// <summary><c>IOPCConfiguration</c> — DX server-to-server configuration (IID_IOPCConfiguration).</summary>
[OpcInterface("C130D281-F4AA-4779-8846-C2C4CB444F2A")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCConfiguration
{
    /// <summary><c>IOPCConfiguration::QueryDXConnections</c> (opnum 8) projected as connection-name discovery.</summary>
    [OpcMethod(8)]
    Task<string[]> QueryDXConnectionNamesAsync(string browsePath, string[] connectionMasks, bool recursive, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::DeleteDXConnections</c> (opnum 12) projected as per-connection HRESULTs.</summary>
    [OpcMethod(12)]
    Task<int[]> DeleteDXConnectionsAsync(string browsePath, string[] connectionNames, bool recursive, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCConfiguration::ResetConfiguration</c> (opnum 14).</summary>
    [OpcMethod(14)]
    Task<string> ResetConfigurationAsync(string configurationVersion, CancellationToken cancellationToken = default);

    // Add/modify/copy methods use OpcDxConnection/OpcDxGeneralResponse records not registered in the proxy codec table yet.
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