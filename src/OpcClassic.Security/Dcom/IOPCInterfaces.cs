//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC Security DCOM-projection interfaces. Each [OpcInterface] partial
// interface is extended by the OpcInterfaceGenerator to carry a
// compile-time-known InterfaceId. Methods will be added with Phase 9E.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCSecurityNT)
#pragma warning disable MA0048 // Two trivial interface stubs grouped for readability

using OpcClassic.Generators;

namespace OpcClassic.Security.Dcom;

/// <summary><c>IOPCSecurityNT</c> — Windows-integrated security via SSPI (IID_IOPCSecurityNT).</summary>
[OpcInterface("7AA83A01-6C77-11D3-84F9-00008630A38B")]
public partial interface IOPCSecurityNT
{
}

/// <summary><c>IOPCSecurityPrivate</c> — server-private credential security (IID_IOPCSecurityPrivate).</summary>
[OpcInterface("7AA83A02-6C77-11D3-84F9-00008630A38B")]
public partial interface IOPCSecurityPrivate
{
}
