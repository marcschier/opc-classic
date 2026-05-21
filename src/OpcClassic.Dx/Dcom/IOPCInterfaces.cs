//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC DX (Data eXchange) DCOM-projection interfaces. Each [OpcInterface]
// partial interface is extended by the OpcInterfaceGenerator to carry a
// compile-time-known InterfaceId. Methods will be added in Phase 9A.
//
// Note: DX is a layered spec on top of DA — the same DA IIDs apply to a
// DX server's underlying DA surface. Here we project only the DX-specific
// configuration interface; the DA interfaces live in OpcClassic.Da.Dcom.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCConfiguration)
#pragma warning disable MA0048 // File grouped under DCOM-projection convention (IOPCInterfaces.cs)

using OpcClassic.Generators;

namespace OpcClassic.Dx.Dcom;

/// <summary><c>IOPCConfiguration</c> — DX server-to-server configuration (IID_IOPCConfiguration).</summary>
[OpcInterface("C130D281-F4AA-4779-8846-C2C4CB444F2A")]
public partial interface IOPCConfiguration
{
}
