//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC DA DCOM-projection interfaces. Each [OpcInterface] partial interface is
// extended by the OpcInterfaceGenerator to carry a compile-time-known InterfaceId.
//
// Methods will be added in Phase 6B/6C with [OpcMethod(opnum)] driving call-shim
// emission. Today these are pure markers — the IID surface alone is the
// foundation the upcoming generators bind against.
//
// IID values match Opc.Classic.Core.OpcGuids — duplication is acceptable for
// now; a future refactor may collapse OpcGuids constants to delegate to these
// generator-emitted InterfaceId values as the single source of truth.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCServer not IOpcServer)
#pragma warning disable MA0048 // 14 trivial 4-line interface stubs are clearer grouped than fragmented across files

using System;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Generators;

namespace Opc.Classic.Da.Dcom;

/// <summary><c>IOPCServer</c> — top-level OPC DA server interface (IID_IOPCServer).</summary>
[OpcInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCServer
{
    /// <summary>
    /// <c>IOPCServer::GetErrorString</c> (opnum 4). Returns a localized human-readable string for the given HRESULT.
    /// </summary>
    [OpcMethod(4)]
    Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::GetStatus</c> (opnum 6). Returns the server's run-state snapshot.
    /// </summary>
    [OpcMethod(6)]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::RemoveGroup</c> (opnum 7). Removes the named group from the server.
    /// </summary>
    [OpcMethod(7)]
    Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default);

    // AddGroup, GetGroupByName, and CreateGroupEnumerator require out COM interface pointers
    // or multi-return shapes, so they are deferred until those codecs exist.
}

/// <summary><c>IOPCBrowse</c> — DA 3.0 unified browse interface (IID_IOPCBrowse).</summary>
[OpcInterface("39227004-A18F-4B57-8B0A-5235670F4468")]
[GenerateOpcProxy]
public partial interface IOPCBrowse
{
    /// <summary>
    /// <c>IOPCBrowse::GetProperties</c> (opnum 3). Returns DA 3.0 property bags for the requested items.
    /// </summary>
    [OpcMethod(3)]
    Task<OpcItemProperties[]> GetPropertiesAsync(
        string[] itemIds,
        bool returnPropertyValues,
        int[] propertyIds,
        CancellationToken cancellationToken = default);

    // Browse has an in/out continuation point plus more/count outputs; deferred until
    // the generator supports explicit multi-out records for those semantics.
}

/// <summary><c>IOPCBrowseServerAddressSpace</c> — DA 2.x browse interface (IID_IOPCBrowseServerAddressSpace).</summary>
[OpcInterface("39C13A4F-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCBrowseServerAddressSpace
{
    /// <summary>
    /// <c>IOPCBrowseServerAddressSpace::QueryOrganization</c> (opnum 3). Returns flat or hierarchical namespace shape.
    /// </summary>
    [OpcMethod(3)]
    Task<int> QueryOrganizationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCBrowseServerAddressSpace::ChangeBrowsePosition</c> (opnum 4). Moves the server-side browse cursor.
    /// </summary>
    [OpcMethod(4)]
    Task ChangeBrowsePositionAsync(int browseDirection, string browsePosition, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCBrowseServerAddressSpace::GetItemID</c> (opnum 6). Resolves a browse data ID to a fully qualified item ID.
    /// </summary>
    [OpcMethod(6)]
    Task<string> GetItemIdAsync(string itemDataId, CancellationToken cancellationToken = default);

    // BrowseOPCItemIDs and BrowseAccessPaths return IEnumString interface pointers.
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
[GenerateOpcProxy]
public partial interface IOPCItemMgt
{
    /// <summary>
    /// <c>IOPCItemMgt::RemoveItems</c> (opnum 5). Removes server handles and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(5)]
    Task<int[]> RemoveItemsAsync(int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::SetActiveState</c> (opnum 6). Sets active state and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(6)]
    Task<int[]> SetActiveStateAsync(int[] serverHandles, bool active, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::SetClientHandles</c> (opnum 7). Rebinds client handles and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(7)]
    Task<int[]> SetClientHandlesAsync(int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::SetDatatypes</c> (opnum 8). Sets requested VARTYPEs and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(8)]
    Task<int[]> SetDatatypesAsync(int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default);

    // AddItems and ValidateItems return OPCITEMRESULT[] plus HRESULT[]; CreateEnumerator returns an interface pointer.
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
    /// <c>IOPCGroupStateMgt::SetName</c> (opnum 5). Renames the group.
    /// </summary>
    [OpcMethod(5)]
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
[GenerateOpcProxy]
public partial interface IOPCSyncIO
{
    /// <summary>
    /// <c>IOPCSyncIO::Write</c> (opnum 4). Writes item values and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(4)]
    Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default);

    // Read returns OPCITEMSTATE[] plus HRESULT[] and needs a multi-out result record codec.
}

/// <summary><c>IOPCSyncIO2</c> — DA 3.0 max-age synchronous I/O (IID_IOPCSyncIO2).</summary>
[OpcInterface("730F5F0F-55B1-4C81-9E18-FF8A0904E1FA")]
[GenerateOpcProxy]
public partial interface IOPCSyncIO2
{
    /// <summary>
    /// <c>IOPCSyncIO2::Write</c> (opnum 4). Writes item values and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(4)]
    Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCSyncIO2::WriteVQT</c> (opnum 6). Writes value/quality/timestamp tuples and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(6)]
    Task<int[]> WriteVqtAsync(int[] serverHandles, OpcItemVqt[] values, CancellationToken cancellationToken = default);

    // Read and ReadMaxAge have parallel value/quality/timestamp/error outputs.
}

/// <summary><c>IOPCAsyncIO2</c> — DA 2.05a asynchronous I/O (IID_IOPCAsyncIO2).</summary>
[OpcInterface("39C13A71-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCAsyncIO2
{
    /// <summary>
    /// <c>IOPCAsyncIO2::Refresh2</c> (opnum 5). Starts an async refresh and returns the cancel ID.
    /// </summary>
    [OpcMethod(5)]
    Task<int> Refresh2Async(int dataSource, int transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO2::Cancel2</c> (opnum 6). Cancels a pending async transaction.
    /// </summary>
    [OpcMethod(6)]
    Task Cancel2Async(int cancelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO2::SetEnable</c> (opnum 7). Enables or disables callbacks.
    /// </summary>
    [OpcMethod(7)]
    Task SetEnableAsync(bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO2::GetEnable</c> (opnum 8). Returns whether callbacks are enabled.
    /// </summary>
    [OpcMethod(8)]
    Task<bool> GetEnableAsync(CancellationToken cancellationToken = default);

    // Read and Write return cancel IDs plus per-item HRESULT arrays.
}

/// <summary><c>IOPCAsyncIO3</c> — DA 3.0 asynchronous I/O with max-age/VQT methods (IID_IOPCAsyncIO3).</summary>
[OpcInterface("0967B97B-36EF-423E-B6F8-6BFF1E40D39D")]
[GenerateOpcProxy]
public partial interface IOPCAsyncIO3
{
    /// <summary>
    /// <c>IOPCAsyncIO3::Refresh2</c> (opnum 5). Starts an async refresh and returns the cancel ID.
    /// </summary>
    [OpcMethod(5)]
    Task<int> Refresh2Async(int dataSource, int transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO3::Cancel2</c> (opnum 6). Cancels a pending async transaction.
    /// </summary>
    [OpcMethod(6)]
    Task Cancel2Async(int cancelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO3::SetEnable</c> (opnum 7). Enables or disables callbacks.
    /// </summary>
    [OpcMethod(7)]
    Task SetEnableAsync(bool enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO3::GetEnable</c> (opnum 8). Returns whether callbacks are enabled.
    /// </summary>
    [OpcMethod(8)]
    Task<bool> GetEnableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO3::RefreshMaxAge</c> (opnum 11). Starts a max-age refresh and returns the cancel ID.
    /// </summary>
    [OpcMethod(11)]
    Task<int> RefreshMaxAgeAsync(int maxAge, int transactionId, CancellationToken cancellationToken = default);

    // Read/Write and max-age/VQT write methods also return per-item HRESULT arrays.
}

/// <summary><c>IConnectionPointContainer</c> — enumerates connection points (IID_IConnectionPointContainer).</summary>
[OpcInterface("B196B284-BAB4-101A-B69C-00AA00341D07")]
public partial interface IConnectionPointContainer
{
    // EnumConnectionPoints and FindConnectionPoint return COM interface pointers.
}

/// <summary><c>IConnectionPoint</c> — the subscription sink-binding interface (IID_IConnectionPoint).</summary>
[OpcInterface("B196B286-BAB4-101A-B69C-00AA00341D07")]
[GenerateOpcProxy]
public partial interface IConnectionPoint
{
    /// <summary>
    /// <c>IConnectionPoint::GetConnectionInterface</c> (opnum 3). Returns the outbound callback IID for this point.
    /// </summary>
    [OpcMethod(3)]
    Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default);

    // GetConnectionPointContainer, Advise, and EnumConnections use interface pointers; Unadvise is deferred with the rest.
}

/// <summary><c>IOPCDataCallback</c> — server -&gt; client OnDataChange/OnReadComplete/OnWriteComplete/OnCancelComplete (IID_IOPCDataCallback).</summary>
[OpcInterface("39C13A70-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
public partial interface IOPCDataCallback
{
    /// <summary>
    /// <c>IOPCDataCallback::OnDataChange</c> (opnum 3). Delivers sampled values from an active subscription.
    /// </summary>
    [OpcMethod(3)]
    Task OnDataChangeAsync(
        int transactionId,
        int groupHandle,
        int masterQuality,
        int masterError,
        int[] clientHandles,
        OpcVariant[] values,
        ushort[] qualities,
        long[] timestamps,
        int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCDataCallback::OnReadComplete</c> (opnum 4). Delivers completion values for an async read.
    /// </summary>
    [OpcMethod(4)]
    Task OnReadCompleteAsync(
        int transactionId,
        int groupHandle,
        int masterQuality,
        int masterError,
        int[] clientHandles,
        OpcVariant[] values,
        ushort[] qualities,
        long[] timestamps,
        int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCDataCallback::OnWriteComplete</c> (opnum 5). Delivers per-item status for an async write.
    /// </summary>
    [OpcMethod(5)]
    Task OnWriteCompleteAsync(
        int transactionId,
        int groupHandle,
        int masterError,
        int[] clientHandles,
        int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCDataCallback::OnCancelComplete</c> (opnum 6). Confirms an async transaction was canceled.
    /// </summary>
    [OpcMethod(6)]
    Task OnCancelCompleteAsync(int transactionId, int groupHandle, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCEnumGUID</c> — enumerates OPC category/server class IDs (IID_IOPCEnumGUID).</summary>
[OpcInterface("55C382C8-21C7-4E88-96C1-BECFB1E3F483")]
[GenerateOpcProxy]
public partial interface IOPCEnumGUID
{
    /// <summary>
    /// <c>IOPCEnumGUID::Next</c> (opnum 3). Returns up to the requested number of GUIDs.
    /// </summary>
    [OpcMethod(3)]
    Task<Guid[]> NextAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCEnumGUID::Skip</c> (opnum 4). Skips the requested number of GUIDs.
    /// </summary>
    [OpcMethod(4)]
    Task SkipAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCEnumGUID::Reset</c> (opnum 5). Resets enumeration to the first GUID.
    /// </summary>
    [OpcMethod(5)]
    Task ResetAsync(CancellationToken cancellationToken = default);

    // Clone returns another enumerator interface pointer.
}

/// <summary><c>IOPCServerList</c> — OPC Discovery 1.0 server list (IID_IOPCServerList).</summary>
[OpcInterface("13486D50-4821-11D2-A494-3CB306C10000")]
[GenerateOpcProxy]
public partial interface IOPCServerList
{
    /// <summary>
    /// <c>IOPCServerList::CLSIDFromProgID</c> (opnum 5). Resolves a ProgID to a class ID.
    /// </summary>
    [OpcMethod(5)]
    Task<Guid> ClsidFromProgIdAsync(string progId, CancellationToken cancellationToken = default);

    // EnumClassesOfCategories returns an enumerator interface; GetClassDetails returns multiple strings.
}

/// <summary><c>IOPCServerList2</c> — OPC Discovery 2.0 server list (IID_IOPCServerList2).</summary>
[OpcInterface("9DD0B56C-AD9E-43EE-8305-487F3188BF7A")]
[GenerateOpcProxy]
public partial interface IOPCServerList2
{
    /// <summary>
    /// <c>IOPCServerList2::CLSIDFromProgID</c> (opnum 5). Resolves a ProgID to a class ID.
    /// </summary>
    [OpcMethod(5)]
    Task<Guid> ClsidFromProgIdAsync(string progId, CancellationToken cancellationToken = default);

    // EnumClassesOfCategories returns an OPC enumerator interface; GetClassDetails returns multiple strings.
}
