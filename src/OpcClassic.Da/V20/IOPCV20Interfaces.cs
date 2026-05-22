// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC Data Access 2.05a backward-compatibility interfaces.
//
// New consumer code SHOULD prefer the DA 3.0 surface in
// OpcClassic.Da.Dcom (IOPCSyncIO2, IOPCAsyncIO2/AsyncIO3).
// These V20 declarations exist for connectivity to legacy servers
// that did not implement DA 3.0.

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCSyncIO not IOpcSyncIO)
#pragma warning disable MA0048 // Legacy DA 2.05a shims are clearer grouped than split across files

using OpcClassic.Generators;

namespace OpcClassic.Da.V20.Dcom;

/// <summary>
/// IOPCSyncIO (DA 2.05a back-compat). Superseded by IOPCSyncIO2 (DA 3.0).
/// Use IOPCSyncIO2 in new code; this exists for connectivity to legacy servers.
/// </summary>
[OpcInterface("39C13A52-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCSyncIO
{
}

/// <summary>
/// IOPCAsyncIO (DA 2.05a back-compat). Superseded by IOPCAsyncIO2/IOPCAsyncIO3.
/// </summary>
[OpcInterface("39C13A53-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCAsyncIO
{
}
