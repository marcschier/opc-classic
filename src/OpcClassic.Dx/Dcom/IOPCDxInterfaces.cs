//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC DX (Data eXchange) DCOM-projection interfaces. Each [OpcInterface]
// partial interface is extended by the OpcInterfaceGenerator to carry a
// compile-time-known InterfaceId. Per-method [OpcMethod(opnum)] declarations
// are deferred until the DX NDR codecs land in Phase 9A-followup.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCConfiguration)
#pragma warning disable MA0048 // Multiple trivial interface stubs grouped for readability

using OpcClassic.Generators;

namespace OpcClassic.Dx.Dcom;

/// <summary><c>IOPCConfiguration</c> — DX server-to-server configuration (IID_IOPCConfiguration).</summary>
[OpcInterface("C130D281-F4AA-4779-8846-C2C4CB444F2A")]
public partial interface IOPCConfiguration
{
}

/// <summary><c>IOPCDXServer</c> — managed DX server shim used by the OpcInterfaceGenerator pipeline.</summary>
[OpcInterface("D5D8F8E9-6F45-43F2-B19E-3FAE3DA88A7C")]
public partial interface IOPCDXServer
{
}