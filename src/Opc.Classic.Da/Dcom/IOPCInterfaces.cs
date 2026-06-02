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
using Opc.Classic.Dcom;
using Opc.Classic.Generators;

namespace Opc.Classic.Da.Dcom;

/// <summary><c>IOPCServer</c> — top-level OPC DA server interface (IID_IOPCServer).</summary>
[OpcInterface("39C13A4D-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCServer
{
    /// <summary>
    /// <c>IOPCServer::AddGroup</c> (opnum 3). Creates a group and returns the requested group interface pointer.
    /// </summary>
    /// <remarks>
    /// IDL: <c>[out, iid_is(riid)] LPUNKNOWN *ppUnk</c>. The double-star with
    /// <c>iid_is</c> is wire-encoded as a unique pointer to an MInterfacePointer
    /// (MS-DCOM §2.2.1.10) wrapping the OBJREF. <see cref="OpcUniquePointerAttribute"/>
    /// on the out parameter directs the generator to use
    /// <c>OpcMInterfacePointerCodec</c> (referent + cbData + OBJREF) instead of
    /// the bare <c>OpcInterfaceRefCodec</c>.
    /// </remarks>
    [OpcMethod(3)]
    [OpcGenerateMultiOutRecord]
    Task AddGroupAsync(
        [OpcRefString] string name,
        bool active,
        int requestedUpdateRate,
        int clientGroupHandle,
        [OpcUniquePointer] int timeBias,
        [OpcUniquePointer] float percentDeadband,
        int localeId,
        Guid requestedInterfaceId,
        out int serverGroupHandle,
        out int revisedUpdateRate,
        [OpcUniquePointer] out IOpcInterfaceRef group,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::GetErrorString</c> (opnum 4). Returns a localized human-readable string for the given HRESULT.
    /// </summary>
    [OpcMethod(4)]
    Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::GetGroupByName</c> (opnum 5). Returns the requested interface for a named group.
    /// </summary>
    /// <remarks>
    /// IDL: <c>[out, iid_is(riid)] LPUNKNOWN *ppUnk</c>; wrapped in MInterfacePointer
    /// on the wire (see <see cref="AddGroupAsync"/> for the same shape).
    /// </remarks>
    [OpcMethod(5)]
    [return: OpcUniquePointer]
    Task<IOpcInterfaceRef> GetGroupByNameAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::GetStatus</c> (opnum 6). Returns the server's run-state snapshot.
    /// </summary>
    /// <remarks>
    /// IDL signature: <c>[out] OPCSERVERSTATUS **ppServerStatus</c>. The double-star
    /// shape is a NDR unique pointer (DCE 1.1 §14.3.10): a 4-byte referent ID
    /// precedes the struct on the wire. <see cref="OpcUniquePointerAttribute"/>
    /// instructs the proxy decoder to skip the referent before invoking the
    /// struct codec.
    /// </remarks>
    [OpcMethod(6)]
    [return: OpcUniquePointer]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::RemoveGroup</c> (opnum 7). Removes the named group from the server.
    /// </summary>
    [OpcMethod(7)]
    Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServer::CreateGroupEnumerator</c> (opnum 8). Returns an <c>IEnumUnknown</c> group enumerator.
    /// </summary>
    [OpcMethod(8)]
    Task<IOpcInterfaceRef> CreateGroupEnumeratorAsync(int scope, Guid requestedInterfaceId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCCommon</c> — common DA locale, error-text, and client-name interface (IID_IOPCCommon).</summary>
[OpcInterface("F31DFDE2-07B6-11D2-B2D8-0060083BA1FB")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCCommon
{
    /// <summary>
    /// <c>IOPCCommon::SetLocaleID</c> (opnum 3). Sets the locale used for localized server strings.
    /// </summary>
    [OpcMethod(3)]
    Task SetLocaleIdAsync(int localeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommon::GetLocaleID</c> (opnum 4). Returns the current locale identifier.
    /// </summary>
    [OpcMethod(4)]
    Task<int> GetLocaleIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommon::QueryAvailableLocaleIDs</c> (opnum 5). Lists supported locale identifiers.
    /// </summary>
    [OpcMethod(5)]
    Task<int[]> QueryAvailableLocaleIdsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommon::GetErrorString</c> (opnum 6). Returns localized text for the given HRESULT.
    /// </summary>
    [OpcMethod(6)]
    Task<string> GetErrorStringAsync(int errorCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCCommon::SetClientName</c> (opnum 7). Supplies a client name for server logging and diagnostics.
    /// </summary>
    [OpcMethod(7)]
    Task SetClientNameAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCBrowse</c> — DA 3.0 unified browse interface (IID_IOPCBrowse).</summary>
[OpcInterface("39227004-A18F-4B57-8B0A-5235670F4468")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCBrowse
{
    /// <summary>
    /// <c>IOPCBrowse::GetProperties</c> (opnum 3). Returns DA 3.0 property bags for the requested items.
    /// </summary>
    [OpcMethod(3)]
    Task<OpcItemProperties[]> GetPropertiesAsync(
        [OpcEmitArrayCount, OpcDeferredElements] string[] itemIds,
        bool returnPropertyValues,
        [OpcEmitArrayCount] int[] propertyIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCBrowse::Browse</c> (opnum 4). Browses DA 3.0 elements below an item ID.
    /// </summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task BrowseAsync(
        [OpcRefString] string itemId,
        ref string? continuationPoint,
        int maxElementsReturned,
        int browseFilter,
        [OpcRefString] string elementNameFilter,
        [OpcRefString] string vendorFilter,
        bool returnAllProperties,
        bool returnPropertyValues,
        [OpcEmitArrayCount] int[] propertyIds,
        out bool moreElements,
        out OpcBrowseElementResult[] browseElements,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCBrowseServerAddressSpace</c> — DA 2.x browse interface (IID_IOPCBrowseServerAddressSpace).</summary>
[OpcInterface("39C13A4F-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
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
    /// <c>IOPCBrowseServerAddressSpace::BrowseOPCItemIDs</c> (opnum 5). Returns an <c>IEnumString</c> item-ID enumerator.
    /// </summary>
    [OpcMethod(5)]
    Task<IOpcInterfaceRef> BrowseOpcItemIdsAsync(
        int browseFilterType,
        string filterCriteria,
        ushort dataTypeFilter,
        int accessRightsFilter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCBrowseServerAddressSpace::GetItemID</c> (opnum 6). Resolves a browse data ID to a fully qualified item ID.
    /// </summary>
    [OpcMethod(6)]
    Task<string> GetItemIdAsync(string itemDataId, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCBrowseServerAddressSpace::BrowseAccessPaths</c> (opnum 7). Returns an <c>IEnumString</c> access-path enumerator.
    /// </summary>
    [OpcMethod(7)]
    Task<IOpcInterfaceRef> BrowseAccessPathsAsync(string itemId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCItemProperties</c> — DA 2.x item-property interface (IID_IOPCItemProperties).</summary>
[OpcInterface("39C13A72-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCItemProperties
{
    /// <summary>
    /// <c>IOPCItemProperties::QueryAvailableProperties</c> (opnum 3). Lists properties available for an item.
    /// </summary>
    [OpcMethod(3)]
    [OpcGenerateMultiOutRecord]
    Task QueryAvailablePropertiesAsync(
        string itemId,
        [OpcUniquePointer] out int[] propertyIds,
        [OpcUniquePointer] out string[] descriptions,
        [OpcUniquePointer] out ushort[] dataTypes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemProperties::GetItemProperties</c> (opnum 4). Reads property values for an item.
    /// </summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task GetItemPropertiesAsync(
        string itemId,
        [OpcEmitArrayCount] int[] propertyIds,
        [OpcUniquePointer, OpcVariantElements] out OpcVariant[] data,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemProperties::LookupItemIDs</c> (opnum 5). Resolves item IDs for indirect properties.
    /// </summary>
    [OpcMethod(5)]
    [OpcGenerateMultiOutRecord]
    Task LookupItemIdsAsync(
        string itemId,
        [OpcEmitArrayCount] int[] propertyIds,
        [OpcUniquePointer] out string[] newItemIds,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCItemDeadbandMgt</c> — per-item deadband management (IID_IOPCItemDeadbandMgt).</summary>
[OpcInterface("5946DA93-8B39-4EC8-AB3D-AA73DF5BC86F")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCItemDeadbandMgt
{
    /// <summary>
    /// <c>IOPCItemDeadbandMgt::SetItemDeadband</c> (opnum 3). Sets per-item percent deadbands.
    /// </summary>
    [OpcMethod(3)]
    [return: OpcUniquePointer]
    Task<int[]> SetItemDeadbandAsync([OpcEmitArrayCount] int[] serverHandles, float[] percentDeadbands, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemDeadbandMgt::GetItemDeadband</c> (opnum 4). Reads per-item percent deadbands.
    /// </summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task GetItemDeadbandAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        [OpcUniquePointer] out float[] percentDeadbands,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemDeadbandMgt::ClearItemDeadband</c> (opnum 5). Clears per-item percent deadbands.
    /// </summary>
    [OpcMethod(5)]
    [return: OpcUniquePointer]
    Task<int[]> ClearItemDeadbandAsync([OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCItemSamplingMgt</c> — per-item sampling-rate/buffer management (IID_IOPCItemSamplingMgt).</summary>
[OpcInterface("3E22D313-F08B-41A5-86C8-95E95CB49FFC")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCItemSamplingMgt
{
    /// <summary>
    /// <c>IOPCItemSamplingMgt::SetItemSamplingRate</c> (opnum 3). Sets requested per-item sampling rates.
    /// </summary>
    [OpcMethod(3)]
    [OpcGenerateMultiOutRecord]
    Task SetItemSamplingRateAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        int[] requestedSamplingRates,
        [OpcUniquePointer] out int[] revisedSamplingRates,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemSamplingMgt::GetItemSamplingRate</c> (opnum 4). Reads per-item sampling rates.
    /// </summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task GetItemSamplingRateAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        [OpcUniquePointer] out int[] samplingRates,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemSamplingMgt::ClearItemSamplingRate</c> (opnum 5). Clears per-item sampling rates.
    /// </summary>
    [OpcMethod(5)]
    [return: OpcUniquePointer]
    Task<int[]> ClearItemSamplingRateAsync([OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemSamplingMgt::SetItemBufferEnable</c> (opnum 6). Enables or disables per-item buffering.
    /// </summary>
    [OpcMethod(6)]
    [return: OpcUniquePointer]
    Task<int[]> SetItemBufferEnableAsync([OpcEmitArrayCount] int[] serverHandles, bool[] enabled, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemSamplingMgt::GetItemBufferEnable</c> (opnum 7). Reads per-item buffering flags.
    /// </summary>
    [OpcMethod(7)]
    [OpcGenerateMultiOutRecord]
    Task GetItemBufferEnableAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        [OpcUniquePointer] out bool[] enabled,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCItemIO</c> — DA 3.0 stateless item I/O (IID_IOPCItemIO).</summary>
[OpcInterface("85C0B427-2893-4CBC-BD78-E5FC5146F08F")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCItemIO
{
    /// <summary>
    /// <c>IOPCItemIO::Read</c> (opnum 3). Reads item values by item ID and max age.
    /// </summary>
    /// <remarks>
    /// IDL signature: <c>[in] DWORD dwCount, [in, size_is(dwCount)] LPCWSTR *pszItemIDs,
    /// [in, size_is(dwCount)] DWORD *pdwMaxAge,
    /// [out, size_is(,dwCount)] VARIANT **ppvValues,
    /// [out, size_is(,dwCount)] WORD **ppwQualities,
    /// [out, size_is(,dwCount)] FILETIME **ppftTimeStamps,
    /// [out, size_is(,dwCount)] HRESULT **ppErrors</c>.
    /// Request inputs: <see cref="OpcEmitArrayCountAttribute"/> emits the
    /// standalone <c>dwCount</c>; <see cref="OpcDeferredElementsAttribute"/>
    /// on the LPCWSTR array uses the C706 §14.3.12.3 deferred-pointer layout
    /// (per-element referents first, then per-element string bodies).
    /// Response outputs: each <c>[out] T**</c> is a unique pointer to a
    /// conformant array — <see cref="OpcUniquePointerAttribute"/> on each
    /// directs the decoder to skip the 4-byte referent before reading the
    /// array max_count and elements.
    /// </remarks>
    [OpcMethod(3)]
    [OpcGenerateMultiOutRecord]
    Task ReadAsync(
        [OpcEmitArrayCount, OpcDeferredElements] string[] itemIds,
        int[] maxAges,
        [OpcUniquePointer, OpcVariantElements] out OpcVariant[] values,
        [OpcUniquePointer] out ushort[] qualities,
        [OpcUniquePointer, OpcFileTimeElements] out long[] timestamps,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemIO::WriteVQT</c> (opnum 4). Writes value/quality/timestamp tuples by item ID.
    /// </summary>
    /// <remarks>
    /// IDL: <c>[in] DWORD dwCount, [in, size_is(dwCount)] LPCWSTR *pszItemIDs,
    /// [in, size_is(dwCount)] OPCITEMVQT *pItemVQT, [out, size_is(,dwCount)]
    /// HRESULT **ppErrors</c>. The standalone <c>dwCount</c> field is emitted
    /// before the arrays via <see cref="OpcEmitArrayCountAttribute"/> on
    /// <paramref name="itemIds"/>; the LPCWSTR* elements use the deferred-pointer
    /// layout via <see cref="OpcDeferredElementsAttribute"/>. The response
    /// HRESULT array is a unique pointer to a conformant array — the return
    /// value carries <see cref="OpcUniquePointerAttribute"/> so the proxy
    /// consumes the outer referent before reading max_count.
    /// </remarks>
    [OpcMethod(4)]
    [return: OpcUniquePointer]
    Task<int[]> WriteVqtAsync([OpcEmitArrayCount, OpcDeferredElements] string[] itemIds, OpcItemVqt[] values, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCItemMgt</c> — group item management (IID_IOPCItemMgt).</summary>
[OpcInterface("39C13A54-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCItemMgt
{
    /// <summary>
    /// <c>IOPCItemMgt::AddItems</c> (opnum 3). Adds items to a group.
    /// </summary>
    [OpcMethod(3)]
    [OpcGenerateMultiOutRecord]
    Task AddItemsAsync(
        [OpcEmitArrayCount] OpcItemDef[] itemDefinitions,
        [OpcUniquePointer] out OpcItemResult[] addResults,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::ValidateItems</c> (opnum 4). Validates prospective items without adding them.
    /// </summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task ValidateItemsAsync(
        [OpcEmitArrayCount] OpcItemDef[] itemDefinitions,
        bool blobUpdate,
        [OpcUniquePointer] out OpcItemResult[] validationResults,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::RemoveItems</c> (opnum 5). Removes server handles and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(5)]
    [return: OpcUniquePointer]
    Task<int[]> RemoveItemsAsync([OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::SetActiveState</c> (opnum 6). Sets active state and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(6)]
    [return: OpcUniquePointer]
    Task<int[]> SetActiveStateAsync([OpcEmitArrayCount] int[] serverHandles, bool active, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::SetClientHandles</c> (opnum 7). Rebinds client handles and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(7)]
    [return: OpcUniquePointer]
    Task<int[]> SetClientHandlesAsync([OpcEmitArrayCount] int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::SetDatatypes</c> (opnum 8). Sets requested VARTYPEs and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(8)]
    [return: OpcUniquePointer]
    Task<int[]> SetDatatypesAsync([OpcEmitArrayCount] int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCItemMgt::CreateEnumerator</c> (opnum 9). Returns an <c>IEnumOPCItemAttributes</c> enumerator.
    /// </summary>
    [OpcMethod(9)]
    Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCGroupStateMgt</c> — group state (active, rate, deadband, ...) (IID_IOPCGroupStateMgt).</summary>
[OpcInterface("39C13A50-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCGroupStateMgt
{
    /// <summary>
    /// <c>IOPCGroupStateMgt::GetState</c> (opnum 3). Returns the group's current state snapshot.
    /// </summary>
    [OpcMethod(3)]
    Task<OpcGroupState> GetStateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCGroupStateMgt::SetState</c> (opnum 4). Updates group state and returns the revised update rate.
    /// </summary>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task SetStateAsync(
        int requestedUpdateRate,
        bool active,
        int timeBias,
        float percentDeadband,
        int localeId,
        int clientGroupHandle,
        out int revisedUpdateRate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCGroupStateMgt::SetName</c> (opnum 5). Renames the group.
    /// </summary>
    [OpcMethod(5)]
    Task SetNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCGroupStateMgt::CloneGroup</c> (opnum 6). Clones the group and returns the requested interface.
    /// </summary>
    [OpcMethod(6)]
    Task<IOpcInterfaceRef> CloneGroupAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCGroupStateMgt2</c> — DA 3.0 group state with keep-alive (IID_IOPCGroupStateMgt2).</summary>
[OpcInterface("8E368666-D72E-4F78-87ED-647611C61C9F")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCGroupStateMgt2
{
    /// <summary>
    /// <c>IOPCGroupStateMgt2::SetKeepAlive</c> (opnum 7). Sets the group keep-alive period.
    /// </summary>
    [OpcMethod(7)]
    Task<int> SetKeepAliveAsync(int keepAliveTime, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCGroupStateMgt2::GetKeepAlive</c> (opnum 8). Returns the group keep-alive period.
    /// </summary>
    [OpcMethod(8)]
    Task<int> GetKeepAliveAsync(CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCSyncIO</c> — DA 2.x synchronous read/write (IID_IOPCSyncIO).</summary>
[OpcInterface("39C13A52-011E-11D0-9675-0020AFD8ADB3")]
[OpcGenerateServerDispatch]
public partial interface IOPCSyncIO
{
    /// <summary>
    /// <c>IOPCSyncIO::Read</c> (opnum 3). Reads item states and per-item HRESULTs.
    /// </summary>
    [OpcMethod(3)]
    [OpcGenerateMultiOutRecord]
    [return: OpcUniquePointer]
    Task<OpcItemState[]> ReadAsync(
        int dataSource,
        [OpcEmitArrayCount] int[] serverHandles,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCSyncIO::Write</c> (opnum 4). Writes item values and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(4)]
    [return: OpcUniquePointer]
    Task<int[]> WriteAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        [OpcVariantElements] OpcVariant[] values,
        CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCSyncIO2</c> — DA 3.0 max-age synchronous I/O (IID_IOPCSyncIO2).</summary>
[OpcInterface("730F5F0F-55B1-4C81-9E18-FF8A0904E1FA")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCSyncIO2
{
    /// <summary>
    /// <c>IOPCSyncIO2::Read</c> (opnum 3). Reads item states and per-item HRESULTs.
    /// </summary>
    [OpcMethod(3)]
    [return: OpcUniquePointer]
    Task<OpcItemState[]> ReadAsync(
        int dataSource,
        [OpcEmitArrayCount] int[] serverHandles,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCSyncIO2::Write</c> (opnum 4). Writes item values and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(4)]
    [return: OpcUniquePointer]
    Task<int[]> WriteAsync([OpcEmitArrayCount] int[] serverHandles, [OpcVariantElements] OpcVariant[] values, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCSyncIO2::ReadMaxAge</c> (opnum 5). Reads values with per-item max-age semantics.
    /// </summary>
    [OpcMethod(5)]
    [OpcGenerateMultiOutRecord]
    Task ReadMaxAgeAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        int[] maxAges,
        [OpcUniquePointer, OpcVariantElements] out OpcVariant[] values,
        [OpcUniquePointer] out ushort[] qualities,
        [OpcUniquePointer, OpcFileTimeElements] out long[] timestamps,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCSyncIO2::WriteVQT</c> (opnum 6). Writes value/quality/timestamp tuples and returns one HRESULT per item.
    /// </summary>
    [OpcMethod(6)]
    [return: OpcUniquePointer]
    Task<int[]> WriteVqtAsync([OpcEmitArrayCount] int[] serverHandles, OpcItemVqt[] values, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCAsyncIO2</c> — DA 2.05a asynchronous I/O (IID_IOPCAsyncIO2).</summary>
[OpcInterface("39C13A71-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCAsyncIO2
{
    /// <summary>
    /// <c>IOPCAsyncIO2::Read</c> (opnum 3). Starts an async read and returns cancel ID plus per-item HRESULTs.
    /// </summary>
    [OpcMethod(3)]
    Task<int> ReadAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        int transactionId,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO2::Write</c> (opnum 4). Starts an async write and returns cancel ID plus per-item HRESULTs.
    /// </summary>
    [OpcMethod(4)]
    Task<int> WriteAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        [OpcVariantElements] OpcVariant[] values,
        int transactionId,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

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

}

/// <summary><c>IOPCAsyncIO3</c> — DA 3.0 asynchronous I/O with max-age/VQT methods (IID_IOPCAsyncIO3).</summary>
[OpcInterface("0967B97B-36EF-423E-B6F8-6BFF1E40D39D")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
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
    /// <c>IOPCAsyncIO3::ReadMaxAge</c> (opnum 9). Starts a max-age async read.
    /// </summary>
    [OpcMethod(9)]
    Task<int> ReadMaxAgeAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        int[] maxAges,
        int transactionId,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO3::WriteVQT</c> (opnum 10). Starts an async VQT write.
    /// </summary>
    [OpcMethod(10)]
    Task<int> WriteVqtAsync(
        [OpcEmitArrayCount] int[] serverHandles,
        OpcItemVqt[] values,
        int transactionId,
        [OpcUniquePointer] out int[] errors,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCAsyncIO3::RefreshMaxAge</c> (opnum 11). Starts a max-age refresh and returns the cancel ID.
    /// </summary>
    [OpcMethod(11)]
    Task<int> RefreshMaxAgeAsync(int maxAge, int transactionId, CancellationToken cancellationToken = default);
}

/// <summary><c>IEnumOPCItemAttributes</c> — enumerates items in a DA group with their full attribute set (IID_IEnumOPCItemAttributes).</summary>
[OpcInterface("39C13A55-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IEnumOPCItemAttributes
{
    /// <summary>
    /// <c>IEnumOPCItemAttributes::Next</c> (opnum 3). Returns up to <paramref name="count"/> item attributes; an empty array signals end of enumeration.
    /// </summary>
    /// <remarks>
    /// IDL: <c>[in] ULONG celt, [out, size_is(,*pceltFetched)] OPCITEMATTRIBUTES **ppItemArray, [out] ULONG *pceltFetched</c>.
    /// <see cref="OpcUniquePointerAttribute"/> on the return value directs the decoder to consume the
    /// outer unique-pointer referent and treat a null referent as an empty array (end of enumeration).
    /// The trailing <c>pceltFetched</c> out parameter is not surfaced — the array's own max_count is authoritative.
    /// </remarks>
    [OpcMethod(3)]
    [return: OpcUniquePointer]
    Task<OpcItemAttributes[]> NextAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IEnumOPCItemAttributes::Skip</c> (opnum 4). Skips <paramref name="count"/> items in the enumeration.
    /// </summary>
    [OpcMethod(4)]
    Task SkipAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IEnumOPCItemAttributes::Reset</c> (opnum 5). Resets the cursor to the start of the enumeration.
    /// </summary>
    [OpcMethod(5)]
    Task ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IEnumOPCItemAttributes::Clone</c> (opnum 6). Returns a new enumerator initialized to the current cursor position.
    /// </summary>
    [OpcMethod(6)]
    Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default);
}

/// <summary><c>IConnectionPointContainer</c> — enumerates connection points (IID_IConnectionPointContainer).</summary>
[OpcInterface("B196B284-BAB4-101A-B69C-00AA00341D07")]
[OpcGenerateServerDispatch]
public partial interface IConnectionPointContainer
{
    /// <summary>
    /// <c>IConnectionPointContainer::EnumConnectionPoints</c> (opnum 3).
    /// Returns an enumerator (<c>IEnumConnectionPoints</c>) over all connection
    /// points this container exposes.
    /// </summary>
    [OpcMethod(3)]
    Task<IOpcInterfaceRef> EnumConnectionPointsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IConnectionPointContainer::FindConnectionPoint</c> (opnum 4).
    /// Returns the <c>IConnectionPoint</c> for a specific outbound IID
    /// (e.g. <see cref="IOPCDataCallback.InterfaceId"/>).
    /// </summary>
    [OpcMethod(4)]
    Task<IOpcInterfaceRef> FindConnectionPointAsync(Guid iid, CancellationToken cancellationToken = default);
}

/// <summary><c>IConnectionPoint</c> — the subscription sink-binding interface (IID_IConnectionPoint).</summary>
[OpcInterface("B196B286-BAB4-101A-B69C-00AA00341D07")]
[OpcGenerateServerDispatch]
public partial interface IConnectionPoint
{
    /// <summary>
    /// <c>IConnectionPoint::GetConnectionInterface</c> (opnum 3). Returns the outbound callback IID for this point.
    /// </summary>
    [OpcMethod(3)]
    Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IConnectionPoint::Advise</c> (opnum 5). Registers a callback sink and returns its connection cookie.
    /// </summary>
    [OpcMethod(5)]
    Task<int> AdviseAsync(IOpcInterfaceRef sink, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IConnectionPoint::Unadvise</c> (opnum 6). Unregisters a callback sink by connection cookie.
    /// </summary>
    [OpcMethod(6)]
    Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default);

    // GetConnectionPointContainer and EnumConnections return interface pointers.
}

/// <summary><c>IOPCShutdown</c> — server -&gt; client shutdown notification sink (IID_IOPCShutdown).</summary>
[OpcInterface("F31DFDE1-07B6-11D2-B2D8-0060083BA1FB")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCShutdown
{
    /// <summary>
    /// <c>IOPCShutdown::ShutdownRequest</c> (opnum 3). Notifies the client that the server is shutting down.
    /// </summary>
    [OpcMethod(3)]
    Task ShutdownRequestAsync(string reason, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCDataCallback</c> — server -&gt; client OnDataChange/OnReadComplete/OnWriteComplete/OnCancelComplete (IID_IOPCDataCallback).</summary>
[OpcInterface("39C13A70-011E-11D0-9675-0020AFD8ADB3")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
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
[OpcGenerateServerDispatch]
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
[OpcGenerateServerDispatch]
public partial interface IOPCServerList
{
    /// <summary>
    /// <c>IOPCServerList::EnumClassesOfCategories</c> (opnum 3). Returns an enumerator
    /// (<c>IEnumGUID</c>) over server CLSIDs that implement <paramref name="implementedCategories"/>
    /// and (optionally) require <paramref name="requiredCategories"/>.
    /// </summary>
    /// <remarks>
    /// IDL: <c>[in] ULONG cImplemented, [in, size_is(cImplemented)] CATID rgcatidImpl[],
    /// [in] ULONG cRequired, [in, size_is(cRequired)] CATID rgcatidReq[],
    /// [out] IEnumGUID **ppenumClsid</c>. Each <c>ULONG</c> count is the sibling for the
    /// conformant CATID array that follows; the proxy emits the standalone count via
    /// <see cref="OpcEmitArrayCountAttribute"/> on the first such array, then a bare
    /// <c>max_count</c> for the second.
    /// </remarks>
    [OpcMethod(3)]
    Task<IOpcInterfaceRef> EnumClassesOfCategoriesAsync(
        [OpcEmitArrayCount] Guid[] implementedCategories,
        [OpcEmitArrayCount] Guid[] requiredCategories,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServerList::GetClassDetails</c> (opnum 4). Returns ProgID and friendly name
    /// for a registered OPC server CLSID.
    /// </summary>
    /// <remarks>
    /// IDL: <c>[in] REFCLSID clsid, [out] LPOLESTR *ppszProgID, [out] LPOLESTR *ppszUserType</c>.
    /// Each <c>LPOLESTR*</c> out parameter is a unique pointer to a conformant Unicode string;
    /// the generator emits the referent read + string body using the codec for <c>string</c>.
    /// </remarks>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task GetClassDetailsAsync(
        Guid clsid,
        out string progId,
        out string userType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServerList::CLSIDFromProgID</c> (opnum 5). Resolves a ProgID to a class ID.
    /// </summary>
    [OpcMethod(5)]
    Task<Guid> ClsidFromProgIdAsync(string progId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCServerList2</c> — OPC Discovery 2.0 server list (IID_IOPCServerList2).</summary>
[OpcInterface("9DD0B56C-AD9E-43EE-8305-487F3188BF7A")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCServerList2
{
    /// <summary>
    /// <c>IOPCServerList2::EnumClassesOfCategories</c> (opnum 3). Returns an enumerator
    /// (<c>IOPCEnumGUID</c>) over server CLSIDs that implement
    /// <paramref name="implementedCategories"/> and (optionally) require
    /// <paramref name="requiredCategories"/>.
    /// </summary>
    [OpcMethod(3)]
    Task<IOpcInterfaceRef> EnumClassesOfCategoriesAsync(
        [OpcEmitArrayCount] Guid[] implementedCategories,
        [OpcEmitArrayCount] Guid[] requiredCategories,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServerList2::GetClassDetails</c> (opnum 4). Returns ProgID, friendly name,
    /// and version-independent ProgID for a registered OPC server CLSID.
    /// </summary>
    /// <remarks>
    /// Named with the V2 suffix to keep the auto-generated multi-out record type
    /// (<c>GetClassDetailsV2AsyncResult</c>) distinct from the V1 version emitted for
    /// <see cref="IOPCServerList"/>; the wire opnum (4) and IDL semantics still match
    /// <c>IOPCServerList2::GetClassDetails</c>.
    /// </remarks>
    [OpcMethod(4)]
    [OpcGenerateMultiOutRecord]
    Task GetClassDetailsV2Async(
        Guid clsid,
        out string progId,
        out string userType,
        out string versionIndependentProgId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCServerList2::CLSIDFromProgID</c> (opnum 5). Resolves a ProgID to a class ID.
    /// </summary>
    [OpcMethod(5)]
    Task<Guid> ClsidFromProgIdAsync(string progId, CancellationToken cancellationToken = default);
}
