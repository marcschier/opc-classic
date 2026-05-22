//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC DA DCOM-projection interfaces. Each [OpcInterface] partial interface is
// extended by the OpcInterfaceGenerator to carry a compile-time-known InterfaceId.
//
// Methods will be added in Phase 6B/6C with [OpcMethod(opnum)] driving call-shim
// emission. Today these are pure markers — the IID surface alone is the
// foundation the upcoming generators bind against.
//
// IID values match OpcClassic.Core.OpcGuids — duplication is acceptable for
// now; a future refactor may collapse OpcGuids constants to delegate to these
// generator-emitted InterfaceId values as the single source of truth.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCServer not IOpcServer)
#pragma warning disable MA0048 // 12 trivial 4-line interface stubs are clearer grouped than fragmented across files

using System.Threading;
using System.Threading.Tasks;
using OpcClassic;
using OpcClassic.Da;
using OpcClassic.Generators;

namespace OpcClassic.Da.Dcom;

/// <summary><c>IOPCServer</c> — top-level OPC DA server interface (IID_IOPCServer).</summary>
[OpcInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCServer
{
    /// <summary>
    /// <c>IOPCServer::GetStatus</c> (opnum 3). Returns the server's run-state snapshot.
    /// </summary>
    [OpcMethod(3)]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::RemoveGroup</c> (opnum 5). Removes the named group from the server.
    /// </summary>
    [OpcMethod(5)]
    Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::GetErrorString</c> (opnum 8). Returns a localized human-readable string for the given HRESULT.
    /// </summary>
    [OpcMethod(8)]
    Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default);

    // AddGroup, GetGroupByName, and CreateGroupEnumerator require out COM interface pointers
    // or multi-return shapes, so they are deferred until those codecs exist.
}

/// <summary><c>IOPCBrowse</c> — DA 3.0 unified browse interface (IID_IOPCBrowse).</summary>
[OpcInterface("39227004-A18F-4B57-8B0A-5235670F4468")]
public partial interface IOPCBrowse
{
    // GetProperties and Browse require conformant arrays of strings, property IDs,
    // OPCITEMPROPERTIES, or OPCBROWSEELEMENT values; defer until array codecs exist.
}

/// <summary><c>IOPCBrowseServerAddressSpace</c> — DA 2.x browse interface (IID_IOPCBrowseServerAddressSpace).</summary>
[OpcInterface("39C13A4F-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCBrowseServerAddressSpace
{
}

/// <summary><c>IOPCItemProperties</c> — DA 2.x item-property interface (IID_IOPCItemProperties).</summary>
[OpcInterface("39C13A72-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCItemProperties
{
    // QueryAvailableProperties, GetItemProperties, and LookupItemIDs require
    // conformant arrays of property IDs, variants, strings, and HRESULTs.
}

/// <summary><c>IOPCItemIO</c> — DA 3.0 stateless item I/O (IID_IOPCItemIO).</summary>
[OpcInterface("85C0B427-2893-4CBC-BD78-E5FC5146F08F")]
[GenerateOpcProxy]
public partial interface IOPCItemIO
{
    /// <summary>
    /// <c>IOPCItemIO::GetProperties</c> (opnum 4). Returns property values for a single item/property pair.
    /// </summary>
    [OpcMethod(4)]
    Task<OpcItemProperties> GetPropertiesAsync(string itemId, int propertyId, CancellationToken cancellationToken = default);

    // Read and WriteVQT remain deferred because their IDL shapes require
    // conformant arrays of item IDs, max ages, VQTs, values, qualities, and HRESULTs.
}

/// <summary><c>IOPCItemMgt</c> — group item management (IID_IOPCItemMgt).</summary>
[OpcInterface("39C13A54-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCItemMgt
{
    // SetActiveState, SetClientHandles, SetDatatypes, and the item add/validate
    // methods are array-heavy and wait on conformant-array codec support.
}

/// <summary><c>IOPCGroupStateMgt</c> — group state (active, rate, deadband, ...) (IID_IOPCGroupStateMgt).</summary>
[OpcInterface("39C13A50-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCGroupStateMgt
{
    /// <summary>
    /// <c>IOPCGroupStateMgt::GetState</c> (opnum 3). Returns the group's current state snapshot.
    /// </summary>
    [OpcMethod(3)]
    Task<OpcGroupState> GetStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCGroupStateMgt::SetName</c> (opnum 4). Renames the group.
    /// </summary>
    [OpcMethod(4)]
    Task SetNameAsync(string name, CancellationToken cancellationToken = default);

    // SetState has optional pointer inputs and a revised-rate output; CloneGroup
    // returns a COM interface pointer. Both are deferred to later call-shim work.
}

/// <summary><c>IOPCGroupStateMgt2</c> — DA 3.0 group state with keep-alive (IID_IOPCGroupStateMgt2).</summary>
[OpcInterface("8E368666-D72E-4F78-87ED-647611C61C9F")]
public partial interface IOPCGroupStateMgt2
{
}

/// <summary><c>IOPCSyncIO</c> — DA 2.x synchronous read/write (IID_IOPCSyncIO).</summary>
[OpcInterface("39C13A52-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCSyncIO
{
}

/// <summary><c>IOPCSyncIO2</c> — DA 3.0 max-age synchronous I/O (IID_IOPCSyncIO2).</summary>
[OpcInterface("730F5F0F-55B1-4C81-9E18-FF8A0904E1FA")]
public partial interface IOPCSyncIO2
{
}

/// <summary><c>IOPCAsyncIO2</c> — DA 2.05a asynchronous I/O (IID_IOPCAsyncIO2).</summary>
[OpcInterface("39C13A71-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCAsyncIO2
{
}

/// <summary><c>IOPCDataCallback</c> — DA subscription callback sink (IID_IOPCDataCallback).</summary>
[OpcInterface("39C13A70-011E-11D0-9675-0020AFD8ADB3")]
public partial interface IOPCDataCallback
{
}
