//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC Commands DCOM-projection interfaces. Each [OpcInterface] partial
// interface is extended by the OpcInterfaceGenerator to carry a
// compile-time-known InterfaceId. Methods + supporting types will be added
// in Phase 9D with spec-derived API design (no managed API existed prior
// to this rewrite).
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCCommandInformation)
#pragma warning disable MA0048 // Multiple trivial interface stubs grouped for readability

using OpcClassic.Generators;

namespace OpcClassic.Commands.Dcom;

/// <summary><c>IOPCCommandInformation</c> — Commands metadata interface (IID_IOPCCommandInformation).</summary>
[OpcInterface("3104B525-2016-442D-9696-1275DE978778")]
public partial interface IOPCCommandInformation
{
}

/// <summary><c>IOPCCommandExecution</c> — Commands execution interface (IID_IOPCCommandExecution).</summary>
[OpcInterface("3104B526-2016-442D-9696-1275DE978778")]
public partial interface IOPCCommandExecution
{
}

/// <summary><c>IOPCCommandCallback</c> — Commands progress / completion sink (IID_IOPCCommandCallback).</summary>
[OpcInterface("3104B527-2016-442D-9696-1275DE978778")]
public partial interface IOPCCommandCallback
{
}
