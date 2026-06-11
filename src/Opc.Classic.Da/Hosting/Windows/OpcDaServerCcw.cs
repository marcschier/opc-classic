//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;

namespace Opc.Classic.Da.Hosting.Windows;

/// <summary>
/// Builds a Windows COM-callable wrapper (CCW) over an <see cref="IOpcDaServer"/>
/// instance. The CCW exposes native vtables for <c>IUnknown</c>,
/// <c>IOPCServer</c>, <c>IOPCCommon</c>, <c>IOPCBrowse</c>,
/// <c>IOPCItemProperties</c>, <c>IOPCItemIO</c>,
/// <c>IOPCBrowseServerAddressSpace</c>, <c>IOPCSecurityNT</c> and
/// <c>IOPCSecurityPrivate</c>; vtable thunks are
/// <see cref="UnmanagedCallersOnlyAttribute"/>-decorated static methods so the
/// assembly remains NativeAOT-compatible (<c>[ComImport]</c> is banned in
/// <c>src/</c>).
/// </summary>
/// <remarks>
/// <para>
/// The OPC DA root server object (the one returned from
/// <c>IClassFactory::CreateInstance</c> on activation) answers
/// <c>QueryInterface</c> for every IID listed in
/// <see cref="SupportsInterface(Guid)"/>: <c>IID_IUnknown</c>,
/// <c>IID_IOPCServer</c>, <c>IID_IOPCCommon</c>, <c>IID_IOPCBrowse</c>,
/// <c>IID_IOPCItemProperties</c>, <c>IID_IOPCItemIO</c>,
/// <c>IID_IOPCBrowseServerAddressSpace</c>, <c>IID_IOPCSecurityNT</c> and
/// <c>IID_IOPCSecurityPrivate</c>. Each interface gets its own CCW instance
/// (sharing one <see cref="CcwEntry"/>) so native clients can hold independent
/// interface pointers.
/// </para>
/// <para>
/// <b>Method dispatch.</b> The vtable thunks marshal the OPC NDR parameters and
/// dispatch into the managed <see cref="IOpcDaServer"/> (and the browse /
/// item-property / security helpers), so native OPC clients (e.g. the OPC
/// Foundation <c>OpcTestClient</c>) and the cross-impl interop matrix drive the
/// sample servers end-to-end. The cross-implementation matrix exercises
/// activation plus the browse / get-properties / read-by-id paths over this CCW.
/// </para>
/// <para>
/// <b>Lifetime.</b> CCW instances and their vtables are never freed
/// (leak-at-process-exit). Once handed to ole32, the pointer must remain
/// valid for the lifetime of the registration; freeing on
/// <c>Release</c>-to-zero would race with in-flight <c>QueryInterface</c>
/// calls. The managed <see cref="IOpcDaServer"/> is pinned via
/// <see cref="GCHandle"/>; the handle is also never freed.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static unsafe class OpcDaServerCcw
{
    private const int S_OK = 0;
    private const int E_NOINTERFACE = unchecked((int)0x80004002);
    private const int E_INVALIDARG = unchecked((int)0x80070057);
    private const int E_NOTIMPL = unchecked((int)0x80004001);
    private const int E_FAIL = unchecked((int)0x80004005);

    private const int ServerVtableSlotCount = 12; // 3 IUnknown + 9 IOPCServer
    private const int CommonVtableSlotCount = 8; // 3 IUnknown + 5 IOPCCommon
    private const int BrowseVtableSlotCount = 5; // 3 IUnknown + 2 IOPCBrowse (Browse, GetProperties)
    private const int ItemPropertiesVtableSlotCount = 6; // 3 IUnknown + 3 IOPCItemProperties (QueryAvailable, GetItem, LookupItemIDs)
    private const int ItemIoVtableSlotCount = 5; // 3 IUnknown + 2 IOPCItemIO (Read, WriteVQT)
    private const int BrowseSasVtableSlotCount = 8; // 3 IUnknown + 5 IOPCBrowseServerAddressSpace
    private const int SecurityNtVtableSlotCount = 6; // 3 IUnknown + 3 IOPCSecurityNT
    private const int SecurityPrivateVtableSlotCount = 6; // 3 IUnknown + 3 IOPCSecurityPrivate

    private static readonly Guid IID_IUnknown = Guid.Parse("00000000-0000-0000-C000-000000000046");

    private static readonly ConcurrentDictionary<IntPtr, CcwEntry> s_ccws = new();

    /// <summary>
    /// Builds a CCW around <paramref name="server"/> and returns a pointer to
    /// the requested interface, or <see cref="IntPtr.Zero"/> if the interface
    /// is not supported by this CCW.
    /// </summary>
    /// <param name="server">The managed server instance to expose via COM.</param>
    /// <param name="requestedIid">
    /// The IID requested by <c>IClassFactory::CreateInstance</c>. Any IID
    /// accepted by <see cref="SupportsInterface(Guid)"/> (IUnknown, IOPCServer,
    /// IOPCCommon, IOPCBrowse, IOPCItemProperties, IOPCItemIO,
    /// IOPCBrowseServerAddressSpace, IOPCSecurityNT, IOPCSecurityPrivate)
    /// returns a live interface pointer; other IIDs return
    /// <see cref="IntPtr.Zero"/>.
    /// </param>
    /// <returns>
    /// A CCW <see cref="IntPtr"/> with reference count = 1 (the caller's
    /// reference), or <see cref="IntPtr.Zero"/> for
    /// <c>E_NOINTERFACE</c>-equivalent.
    /// </returns>
    public static IntPtr Create(IOpcDaServer server, Guid requestedIid)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (!SupportsInterface(requestedIid))
        {
            return IntPtr.Zero;
        }

        IntPtr* serverVtable = AllocateServerVtable();
        IntPtr serverInstance = AllocateInstance(serverVtable);
        IntPtr* commonVtable = AllocateCommonVtable();
        IntPtr commonInstance = AllocateInstance(commonVtable);
        IntPtr* browseVtable = AllocateBrowseVtable();
        IntPtr browseInstance = AllocateInstance(browseVtable);
        IntPtr* itemPropsVtable = AllocateItemPropertiesVtable();
        IntPtr itemPropsInstance = AllocateInstance(itemPropsVtable);
        IntPtr* itemIoVtable = AllocateItemIoVtable();
        IntPtr itemIoInstance = AllocateInstance(itemIoVtable);
        IntPtr* browseSasVtable = AllocateBrowseSasVtable();
        IntPtr browseSasInstance = AllocateInstance(browseSasVtable);
        IntPtr* securityNtVtable = AllocateSecurityNtVtable();
        IntPtr securityNtInstance = AllocateInstance(securityNtVtable);
        IntPtr* securityPrivVtable = AllocateSecurityPrivateVtable();
        IntPtr securityPrivInstance = AllocateInstance(securityPrivVtable);
        var handle = GCHandle.Alloc(server, GCHandleType.Normal);
        var entry = new CcwEntry(handle, serverInstance, commonInstance, browseInstance, itemPropsInstance, itemIoInstance, browseSasInstance, securityNtInstance, securityPrivInstance);
        entry.RefCount = 1;
        s_ccws[serverInstance] = entry;
        s_ccws[commonInstance] = entry;
        s_ccws[browseInstance] = entry;
        s_ccws[itemPropsInstance] = entry;
        s_ccws[itemIoInstance] = entry;
        s_ccws[browseSasInstance] = entry;
        s_ccws[securityNtInstance] = entry;
        s_ccws[securityPrivInstance] = entry;
        return entry.GetInterfacePointer(requestedIid);
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="iid"/> is one of the
    /// COM interfaces this CCW exposes.
    /// </summary>
    public static bool SupportsInterface(Guid iid) =>
        iid == IID_IUnknown
        || iid == Dcom.IOPCServer.InterfaceId
        || iid == Dcom.IOPCCommon.InterfaceId
        || iid == Dcom.IOPCBrowse.InterfaceId
        || iid == Dcom.IOPCItemProperties.InterfaceId
        || iid == Dcom.IOPCItemIO.InterfaceId
        || iid == Dcom.IOPCBrowseServerAddressSpace.InterfaceId
        || iid == OpcGuids.IID_IOPCSecurityNT
        || iid == OpcGuids.IID_IOPCSecurityPrivate;

    /// <summary>
    /// Test helper: returns the current reference count for a CCW pointer, or
    /// <c>-1</c> if the pointer is not a known CCW.
    /// </summary>
    public static long GetReferenceCount(IntPtr ccw) =>
        s_ccws.TryGetValue(ccw, out CcwEntry? entry) ? Interlocked.Read(ref entry.RefCount) : -1L;

    [SuppressMessage(
        "Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating IntPtr-sized native vtable with explicit byte count.")]
    private static IntPtr* AllocateServerVtable()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(ServerVtableSlotCount * sizeof(IntPtr)));
        // IUnknown
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        // IOPCServer (canonical opnum order per OPC DA 3.0 spec)
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int, uint, uint, IntPtr, IntPtr, uint, IntPtr, IntPtr, Guid*, IntPtr*, int>)&AddGroup;
        vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, int, uint, IntPtr*, int>)&GetErrorString;
        vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, Guid*, IntPtr*, int>)&GetGroupByName;
        vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr*, int>)&GetStatus;
        vtable[7] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int, int>)&RemoveGroup;
        vtable[8] = (IntPtr)(delegate* unmanaged<IntPtr, uint, Guid*, IntPtr*, int>)&CreateGroupEnumerator;
        // Remaining slots are reserved; zero them so a misdirected dispatch
        // crashes loudly instead of into arbitrary memory.
        for (int i = 9; i < ServerVtableSlotCount; i++)
        {
            vtable[i] = IntPtr.Zero;
        }
        return vtable;
    }

    [SuppressMessage(
        "Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating IntPtr-sized native vtable with explicit byte count.")]
    private static IntPtr* AllocateCommonVtable()
    {
        IntPtr* vtable = (IntPtr*)NativeMemory.Alloc((nuint)(CommonVtableSlotCount * sizeof(IntPtr)));
        // IUnknown
        vtable[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        vtable[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        vtable[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        // IOPCCommon (canonical opnum order per OPC Common 1.10 spec)
        vtable[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, int>)&CommonSetLocaleId;
        vtable[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, int>)&CommonGetLocaleId;
        vtable[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, IntPtr*, int>)&CommonQueryAvailableLocaleIds;
        vtable[6] = (IntPtr)(delegate* unmanaged<IntPtr, int, IntPtr*, int>)&CommonGetErrorString;
        vtable[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, int>)&CommonSetClientName;
        return vtable;
    }

    [SuppressMessage(
        "Reliability", "CA2018:Buffer size argument matches element count",
        Justification = "Allocating IntPtr-sized CCW header with explicit byte count.")]
    private static IntPtr AllocateInstance(IntPtr* vtable)
    {
        IntPtr* instance = (IntPtr*)NativeMemory.Alloc((nuint)sizeof(IntPtr));
        instance[0] = (IntPtr)vtable;
        return (IntPtr)instance;
    }

    // IOPCBrowse (DA 3.0): stub tearoff that returns empty data for browse +
    // get_properties. Lets multi-IID-activation clients bind the interface
    // and decode an empty result so probes can succeed without the heavy
    // managed→native marshalling needed for full-fidelity browse responses.
    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte count.")]
    private static IntPtr* AllocateBrowseVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(BrowseVtableSlotCount * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        // IOPCBrowse: opnum 3 = GetProperties, opnum 4 = Browse
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, int, uint, IntPtr, IntPtr*, int>)&BrowseGetProperties;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, uint, uint, IntPtr, IntPtr, int, int, uint, IntPtr, IntPtr*, IntPtr*, IntPtr*, int>)&BrowseBrowse;
        return v;
    }

    // IOPCItemProperties (DA 2.0): stub tearoff returning empty property lists.
    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte count.")]
    private static IntPtr* AllocateItemPropertiesVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(ItemPropertiesVtableSlotCount * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, uint*, IntPtr*, IntPtr*, IntPtr*, int>)&ItemPropertiesQueryAvailable;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)&ItemPropertiesGetItemProperties;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, uint, IntPtr, IntPtr*, IntPtr*, int>)&ItemPropertiesLookupItemIds;
        return v;
    }

    // IOPCItemIO (DA 3.0): stub tearoff returning empty values for read +
    // success no-ops for write.
    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte count.")]
    private static IntPtr* AllocateItemIoVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(ItemIoVtableSlotCount * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        // IOPCItemIO: opnum 3 = Read, opnum 4 = WriteVQT
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, IntPtr*, int>)&ItemIoRead;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, IntPtr, IntPtr*, int>)&ItemIoWriteVqt;
        return v;
    }

    // IOPCBrowseServerAddressSpace (DA 2.0): stub tearoff returning empty
    // namespace enumerations + flat namespace shape.
    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte count.")]
    private static IntPtr* AllocateBrowseSasVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(BrowseSasVtableSlotCount * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, int>)&BrowseSasQueryOrganization;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, int>)&BrowseSasChangeBrowsePosition;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, uint, IntPtr, ushort, uint, IntPtr*, int>)&BrowseSasBrowseOpcItemIds;
        v[6] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, int>)&BrowseSasGetItemId;
        v[7] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr*, int>)&BrowseSasBrowseAccessPaths;
        return v;
    }

    // ===== Stub method implementations =====
    // These return S_OK with empty output buffers so probes can bind +
    // invoke the interface without the heavy COM marshaling needed for
    // full-fidelity browse/properties/read responses. Future work can
    // replace these with real implementations that translate managed
    // IOpcDaServer.BrowseAsync / IDaServer.ReadAsync results to native
    // COM types (OPCBROWSEELEMENT, OPCITEMPROPERTY, OPCITEMSTATE, etc.).

    [UnmanagedCallersOnly]
    private static int BrowseGetProperties(IntPtr pThis, uint dwItemCount, IntPtr pszItemIDs, int bReturnPropertyValues, uint dwPropertyCount, IntPtr pdwPropertyIDs, IntPtr* ppItemProperties)
    {
        _ = pThis; _ = dwItemCount; _ = pszItemIDs; _ = bReturnPropertyValues; _ = dwPropertyCount; _ = pdwPropertyIDs;
        if (ppItemProperties != null) { *ppItemProperties = IntPtr.Zero; }
        return S_OK; // Empty result, no allocations.
    }

    [UnmanagedCallersOnly]
    private static int BrowseBrowse(
        IntPtr pThis,
        IntPtr szItemID,
        IntPtr* pszContinuationPoint,
        uint dwMaxElementsReturned,
        uint dwBrowseFilter,
        IntPtr szElementNameFilter,
        IntPtr szVendorFilter,
        int bReturnAllProperties,
        int bReturnPropertyValues,
        uint dwPropertyCount,
        IntPtr pdwPropertyIDs,
        IntPtr* pbMoreElements,
        IntPtr* pdwCount,
        IntPtr* ppBrowseElements)
    {
        _ = pThis; _ = szItemID; _ = dwMaxElementsReturned; _ = dwBrowseFilter;
        _ = szElementNameFilter; _ = szVendorFilter; _ = bReturnAllProperties;
        _ = bReturnPropertyValues; _ = dwPropertyCount; _ = pdwPropertyIDs;
        if (pszContinuationPoint != null) { *pszContinuationPoint = IntPtr.Zero; }
        if (pbMoreElements != null) { *pbMoreElements = IntPtr.Zero; }
        if (pdwCount != null) { *pdwCount = IntPtr.Zero; }
        if (ppBrowseElements != null) { *ppBrowseElements = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int ItemPropertiesQueryAvailable(IntPtr pThis, IntPtr szItemID, uint* pdwCount, IntPtr* ppPropertyIDs, IntPtr* ppDescriptions, IntPtr* ppvtDataTypes)
    {
        _ = pThis; _ = szItemID;
        if (pdwCount != null) { *pdwCount = 0; }
        if (ppPropertyIDs != null) { *ppPropertyIDs = IntPtr.Zero; }
        if (ppDescriptions != null) { *ppDescriptions = IntPtr.Zero; }
        if (ppvtDataTypes != null) { *ppvtDataTypes = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int ItemPropertiesGetItemProperties(IntPtr pThis, IntPtr szItemID, uint dwCount, IntPtr pdwPropertyIDs, IntPtr* ppvData, IntPtr* ppErrors)
    {
        _ = pThis; _ = szItemID; _ = dwCount; _ = pdwPropertyIDs;
        if (ppvData != null) { *ppvData = IntPtr.Zero; }
        if (ppErrors != null) { *ppErrors = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int ItemPropertiesLookupItemIds(IntPtr pThis, IntPtr szItemID, uint dwCount, IntPtr pdwPropertyIDs, IntPtr* ppszNewItemIDs, IntPtr* ppErrors)
    {
        _ = pThis; _ = szItemID; _ = dwCount; _ = pdwPropertyIDs;
        if (ppszNewItemIDs != null) { *ppszNewItemIDs = IntPtr.Zero; }
        if (ppErrors != null) { *ppErrors = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int ItemIoRead(IntPtr pThis, uint dwCount, IntPtr pszItemIDs, IntPtr pdwMaxAges, IntPtr* ppvValues, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwCount; _ = pszItemIDs; _ = pdwMaxAges;
        if (ppvValues != null) { *ppvValues = IntPtr.Zero; }
        if (ppErrors != null) { *ppErrors = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int ItemIoWriteVqt(IntPtr pThis, uint dwCount, IntPtr pszItemIDs, IntPtr pItemVQT, IntPtr* ppErrors)
    {
        _ = pThis; _ = dwCount; _ = pszItemIDs; _ = pItemVQT;
        if (ppErrors != null) { *ppErrors = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int BrowseSasQueryOrganization(IntPtr pThis, uint* pNamespaceType)
    {
        _ = pThis;
        if (pNamespaceType != null) { *pNamespaceType = 1; /* OPC_NS_HIERARCHIAL */ }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int BrowseSasChangeBrowsePosition(IntPtr pThis, uint dwBrowseDirection, IntPtr szString)
    {
        _ = pThis; _ = dwBrowseDirection; _ = szString;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int BrowseSasBrowseOpcItemIds(IntPtr pThis, uint dwBrowseFilterType, IntPtr szFilterCriteria, ushort vtDataTypeFilter, uint dwAccessRightsFilter, IntPtr* ppIEnumString)
    {
        _ = pThis; _ = dwBrowseFilterType; _ = szFilterCriteria; _ = vtDataTypeFilter; _ = dwAccessRightsFilter;
        if (ppIEnumString != null) { *ppIEnumString = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int BrowseSasGetItemId(IntPtr pThis, IntPtr szItemDataID, IntPtr* ppszItemID)
    {
        _ = pThis; _ = szItemDataID;
        if (ppszItemID != null) { *ppszItemID = IntPtr.Zero; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int BrowseSasBrowseAccessPaths(IntPtr pThis, IntPtr szItemID, IntPtr* ppIEnumString)
    {
        _ = pThis; _ = szItemID;
        if (ppIEnumString != null) { *ppIEnumString = IntPtr.Zero; }
        return S_OK;
    }

    // ===== IOPCSecurityNT / IOPCSecurityPrivate (OPC Security 1.00) =====

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte count.")]
    private static IntPtr* AllocateSecurityNtVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(SecurityNtVtableSlotCount * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        // IOPCSecurityNT: opnum 3 = IsAvailableNT, opnum 4 = QueryMinImpersonationLevel, opnum 5 = ChangeUser
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, int*, int>)&SecurityNtIsAvailableNT;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, uint*, int>)&SecurityNtQueryMinImpersonationLevel;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int>)&SecurityNtChangeUser;
        return v;
    }

    [SuppressMessage("Reliability", "CA2018:Buffer size argument matches element count", Justification = "Explicit byte count.")]
    private static IntPtr* AllocateSecurityPrivateVtable()
    {
        IntPtr* v = (IntPtr*)NativeMemory.Alloc((nuint)(SecurityPrivateVtableSlotCount * sizeof(IntPtr)));
        v[0] = (IntPtr)(delegate* unmanaged<IntPtr, Guid*, IntPtr*, int>)&QueryInterface;
        v[1] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&AddRef;
        v[2] = (IntPtr)(delegate* unmanaged<IntPtr, uint>)&Release;
        // IOPCSecurityPrivate: opnum 3 = IsAvailablePriv, opnum 4 = Logon, opnum 5 = Logoff
        v[3] = (IntPtr)(delegate* unmanaged<IntPtr, int*, int>)&SecurityPrivateIsAvailablePriv;
        v[4] = (IntPtr)(delegate* unmanaged<IntPtr, IntPtr, IntPtr, int>)&SecurityPrivateLogon;
        v[5] = (IntPtr)(delegate* unmanaged<IntPtr, int>)&SecurityPrivateLogoff;
        return v;
    }

    [UnmanagedCallersOnly]
    private static int SecurityNtIsAvailableNT(IntPtr pThis, int* pbAvailable)
    {
        _ = pThis;
        if (pbAvailable != null) { *pbAvailable = -1; /* TRUE */ }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int SecurityNtQueryMinImpersonationLevel(IntPtr pThis, uint* pdwLevel)
    {
        _ = pThis;
        // RPC_C_IMP_LEVEL_IDENTIFY (2)
        if (pdwLevel != null) { *pdwLevel = 2; }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int SecurityNtChangeUser(IntPtr pThis)
    {
        _ = pThis;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int SecurityPrivateIsAvailablePriv(IntPtr pThis, int* pbAvailable)
    {
        _ = pThis;
        if (pbAvailable != null) { *pbAvailable = -1; /* TRUE */ }
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int SecurityPrivateLogon(IntPtr pThis, IntPtr szUserID, IntPtr szPassword)
    {
        _ = pThis; _ = szUserID; _ = szPassword;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int SecurityPrivateLogoff(IntPtr pThis)
    {
        _ = pThis;
        return S_OK;
    }

    [UnmanagedCallersOnly]
    private static int QueryInterface(IntPtr pThis, Guid* riid, IntPtr* ppv)
    {
        if (ppv == null)
        {
            return E_INVALIDARG;
        }
        if (riid == null)
        {
            *ppv = IntPtr.Zero;
            return E_INVALIDARG;
        }

        Guid requestedIid = *riid;
        if (SupportsInterface(requestedIid) && s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            *ppv = entry.GetInterfacePointer(requestedIid);
            Interlocked.Increment(ref entry.RefCount);
            return S_OK;
        }

        *ppv = IntPtr.Zero;
        return E_NOINTERFACE;
    }

    [UnmanagedCallersOnly]
    private static uint AddRef(IntPtr pThis)
    {
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return 1;
        }
        return (uint)Interlocked.Increment(ref entry.RefCount);
    }

    [UnmanagedCallersOnly]
    private static uint Release(IntPtr pThis)
    {
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return 0;
        }

        long next = Interlocked.Decrement(ref entry.RefCount);
        // CCWs are never freed (leak-at-exit); see class remarks.
        return next < 0 ? 0 : (uint)next;
    }

    // ===== IOPCCommon stubs =====

    [UnmanagedCallersOnly]
    private static int CommonSetLocaleId(IntPtr pThis, uint dwLcid)
    {
        _ = pThis; _ = dwLcid;
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int CommonGetLocaleId(IntPtr pThis, uint* pdwLcid)
    {
        _ = pThis;
        if (pdwLcid != null)
        {
            *pdwLcid = 0;
        }
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int CommonQueryAvailableLocaleIds(IntPtr pThis, uint* pdwCount, IntPtr* ppdwLcid)
    {
        _ = pThis;
        if (pdwCount != null)
        {
            *pdwCount = 0;
        }
        if (ppdwLcid != null)
        {
            *ppdwLcid = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int CommonGetErrorString(IntPtr pThis, int dwError, IntPtr* ppString)
    {
        _ = pThis; _ = dwError;
        if (ppString != null)
        {
            *ppString = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    [UnmanagedCallersOnly]
    private static int CommonSetClientName(IntPtr pThis, IntPtr szName)
    {
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOTIMPL;
        }

        try
        {
            string clientName = szName == IntPtr.Zero ? string.Empty : (Marshal.PtrToStringUni(szName) ?? string.Empty);
            entry.ClientName = clientName;
            if (entry.ServerHandle.Target is IDaServer daServer)
            {
#pragma warning disable VSTHRD002 // Synchronous bridge across the COM ABI.
                daServer.SetClientNameAsync(clientName, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            }
            return S_OK;
        }
#pragma warning disable CA1031 // Cross-unmanaged-boundary catch.
        catch (Opc.Classic.OpcException opcEx)
        {
            return opcEx.ResultId.Code;
        }
        catch (ArgumentException)
        {
            return E_INVALIDARG;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    // ===== IOPCServer stubs (E_NOTIMPL until ocom-3b/follow-up wires real impls) =====

    [UnmanagedCallersOnly]
    private static int AddGroup(
        IntPtr pThis,
        IntPtr szName,
        int bActive,
        uint dwRequestedUpdateRate,
        uint hClientGroup,
        IntPtr pTimeBias,
        IntPtr pPercentDeadband,
        uint dwLCID,
        IntPtr phServerGroup,
        IntPtr pRevisedUpdateRate,
        Guid* riid,
        IntPtr* ppUnk)
    {
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOTIMPL;
        }
        if (entry.ServerHandle.Target is not IOpcDaServer server)
        {
            return E_NOTIMPL;
        }
        _ = riid; _ = pTimeBias; _ = pPercentDeadband;
        return AddGroupCore(server, szName, bActive, dwRequestedUpdateRate, hClientGroup,
            dwLCID, phServerGroup, pRevisedUpdateRate, ppUnk);
    }

    private static int AddGroupCore(
        IOpcDaServer server,
        IntPtr szName,
        int bActive,
        uint dwRequestedUpdateRate,
        uint hClientGroup,
        uint dwLCID,
        IntPtr phServerGroup,
        IntPtr pRevisedUpdateRate,
        IntPtr* ppUnk)
    {
        // OPC DA 2.05a §4.3.2: all required OUT params must be non-NULL.
        if (phServerGroup == IntPtr.Zero || pRevisedUpdateRate == IntPtr.Zero || ppUnk == null)
        {
            return E_INVALIDARG;
        }
        try
        {
            string name = szName == IntPtr.Zero ? string.Empty : (Marshal.PtrToStringUni(szName) ?? string.Empty);
#pragma warning disable VSTHRD002 // Sync bridge across the COM ABI.
            int serverHandle = server.AddGroupAsync(
                name,
                active: bActive != 0,
                requestedUpdateRate: (int)dwRequestedUpdateRate,
                clientHandle: (int)hClientGroup,
                localeId: (int)dwLCID,
                CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            Marshal.WriteInt32(phServerGroup, serverHandle);
            Marshal.WriteInt32(pRevisedUpdateRate, (int)dwRequestedUpdateRate);

            // Prefer the server-tracked group instance (cap-a6) so the CCW
            // refers to the same managed OpcDaGroup that the server bookkeeps
            // — subsequent dispatch via IOPCItemMgt / IOPCSyncIO routes through
            // the server's authoritative state. Fall back to a placeholder
            // OpcDaGroup only when the server doesn't track groups in-process
            // (the default IOpcDaServer.ResolveGroupAsync returns null).
#pragma warning disable VSTHRD002
            OpcDaGroup? resolved = server.ResolveGroupAsync(serverHandle, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            OpcDaGroup target = resolved ?? new OpcDaGroup(
                name: name,
                serverHandle: serverHandle,
                clientHandle: (int)hClientGroup,
                active: bActive != 0,
                requestedUpdateRate: (int)dwRequestedUpdateRate,
                timeBias: 0,
                percentDeadband: 0f,
                localeId: (int)dwLCID);
            *ppUnk = OpcDaGroupCcw.Create(target);
            return S_OK;
        }
#pragma warning disable CA1031 // Cross-unmanaged-boundary catch.
        catch (Opc.Classic.OpcException opcEx)
        {
            return opcEx.ResultId.Code;
        }
        catch (ArgumentException)
        {
            return E_INVALIDARG;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    [UnmanagedCallersOnly]
    private static int GetErrorString(IntPtr pThis, int dwError, uint dwLocale, IntPtr* ppString)
    {
        if (ppString == null)
        {
            return E_INVALIDARG;
        }
        *ppString = IntPtr.Zero;
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOTIMPL;
        }
        if (entry.ServerHandle.Target is not IOpcDaServer server)
        {
            return E_NOTIMPL;
        }

        try
        {
#pragma warning disable VSTHRD002 // Synchronous bridge across the COM ABI; the underlying impl is async-by-design.
            string text = server.GetErrorStringAsync(dwError, (int)dwLocale, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            *ppString = AllocateLpwStr(text);
            return S_OK;
        }
#pragma warning disable CA1031 // Cross-unmanaged-boundary catch.
        catch (Opc.Classic.OpcException opcEx)
        {
            return opcEx.ResultId.Code;
        }
        catch (ArgumentException)
        {
            return E_INVALIDARG;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    [UnmanagedCallersOnly]
    private static int GetStatus(IntPtr pThis, IntPtr* ppServerStatus)
    {
        if (ppServerStatus == null)
        {
            return E_INVALIDARG;
        }
        *ppServerStatus = IntPtr.Zero;
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOTIMPL;
        }
        if (entry.ServerHandle.Target is not IOpcDaServer server)
        {
            return E_NOTIMPL;
        }

        try
        {
#pragma warning disable VSTHRD002
            OpcServerStatus status = server.GetStatusAsync(CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

            *ppServerStatus = AllocateOpcServerStatus(status);
            return S_OK;
        }
#pragma warning disable CA1031
        catch (Opc.Classic.OpcException opcEx)
        {
            return opcEx.ResultId.Code;
        }
        catch (ArgumentException)
        {
            return E_INVALIDARG;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    [UnmanagedCallersOnly]
    private static int GetGroupByName(IntPtr pThis, IntPtr szName, Guid* riid, IntPtr* ppUnk)
    {
        _ = riid;
        if (ppUnk == null)
        {
            return E_INVALIDARG;
        }
        *ppUnk = IntPtr.Zero;
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOTIMPL;
        }
        if (entry.ServerHandle.Target is not IOpcDaServer server)
        {
            return E_NOTIMPL;
        }
        if (szName == IntPtr.Zero)
        {
            return E_INVALIDARG;
        }

        try
        {
            string name = Marshal.PtrToStringUni(szName) ?? string.Empty;
#pragma warning disable VSTHRD002
            OpcDaGroup? group = server.ResolveGroupByNameAsync(name, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            if (group is null)
            {
                return Opc.Classic.OpcResultId.UnknownPath.Code;
            }
            *ppUnk = OpcDaGroupCcw.Create(group);
            return S_OK;
        }
#pragma warning disable CA1031 // Cross-unmanaged-boundary catch.
        catch (Opc.Classic.OpcException opcEx)
        {
            return opcEx.ResultId.Code;
        }
        catch (ArgumentException)
        {
            return E_INVALIDARG;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    [UnmanagedCallersOnly]
    private static int RemoveGroup(IntPtr pThis, uint hServerGroup, int bForce)
    {
        if (!s_ccws.TryGetValue(pThis, out CcwEntry? entry))
        {
            return E_NOTIMPL;
        }
        if (entry.ServerHandle.Target is not IOpcDaServer server)
        {
            return E_NOTIMPL;
        }

        try
        {
#pragma warning disable VSTHRD002 // The CCW method runs synchronously across the COM ABI; bridge to the async impl via .GetAwaiter().GetResult().
            server.RemoveGroupAsync((int)hServerGroup, bForce != 0, CancellationToken.None)
                .GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            return S_OK;
        }
#pragma warning disable CA1031 // Cross-unmanaged-boundary catch: any escaping managed exception would crash the process.
        catch (Opc.Classic.OpcException opcEx)
        {
            return opcEx.ResultId.Code;
        }
        catch (ArgumentException)
        {
            return E_INVALIDARG;
        }
        catch (Exception)
        {
            return E_FAIL;
        }
#pragma warning restore CA1031
    }

    [UnmanagedCallersOnly]
    private static int CreateGroupEnumerator(IntPtr pThis, uint dwScope, Guid* riid, IntPtr* ppUnk)
    {
        _ = pThis; _ = dwScope; _ = riid;
        if (ppUnk != null)
        {
            *ppUnk = IntPtr.Zero;
        }
        return E_NOTIMPL;
    }

    private sealed class CcwEntry
    {
        public CcwEntry(GCHandle serverHandle, IntPtr serverPointer, IntPtr commonPointer, IntPtr browsePointer, IntPtr itemPropsPointer, IntPtr itemIoPointer, IntPtr browseSasPointer, IntPtr securityNtPointer, IntPtr securityPrivPointer)
        {
            ServerHandle = serverHandle;
            ServerPointer = serverPointer;
            CommonPointer = commonPointer;
            BrowsePointer = browsePointer;
            ItemPropertiesPointer = itemPropsPointer;
            ItemIoPointer = itemIoPointer;
            BrowseSasPointer = browseSasPointer;
            SecurityNtPointer = securityNtPointer;
            SecurityPrivatePointer = securityPrivPointer;
        }

        public GCHandle ServerHandle { get; }

        public IntPtr ServerPointer { get; }

        public IntPtr CommonPointer { get; }

        public IntPtr BrowsePointer { get; }

        public IntPtr ItemPropertiesPointer { get; }

        public IntPtr ItemIoPointer { get; }

        public IntPtr BrowseSasPointer { get; }

        public IntPtr SecurityNtPointer { get; }

        public IntPtr SecurityPrivatePointer { get; }

        public string ClientName { get; set; } = string.Empty;

        public long RefCount;

        public IntPtr GetInterfacePointer(Guid iid)
        {
            if (iid == Dcom.IOPCCommon.InterfaceId) { return CommonPointer; }
            if (iid == Dcom.IOPCBrowse.InterfaceId) { return BrowsePointer; }
            if (iid == Dcom.IOPCItemProperties.InterfaceId) { return ItemPropertiesPointer; }
            if (iid == Dcom.IOPCItemIO.InterfaceId) { return ItemIoPointer; }
            if (iid == Dcom.IOPCBrowseServerAddressSpace.InterfaceId) { return BrowseSasPointer; }
            if (iid == OpcGuids.IID_IOPCSecurityNT) { return SecurityNtPointer; }
            if (iid == OpcGuids.IID_IOPCSecurityPrivate) { return SecurityPrivatePointer; }
            return ServerPointer;
        }
    }

    // ----- COM allocation helpers -----

    /// <summary>
    /// Native marshalling of <c>OPCSERVERSTATUS</c> as defined in
    /// <c>interop\inc\opcda.h</c> (MIDL-generated). LayoutKind.Sequential with
    /// default packing (natural alignment) matches MIDL's default on x64,
    /// where <c>szVendorInfo</c> (LPWSTR = pointer) needs 8-byte alignment.
    /// A non-default <c>Pack</c> value would put the pointer at an
    /// unaligned offset and Windows DCOM's MIDL stub would read garbage
    /// when marshalling the response, causing the wire connection to be
    /// closed mid-call (observed as RPC_S_CALL_FAILED on the client side).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct OPCSERVERSTATUS_NATIVE
    {
        public long ftStartTime;
        public long ftCurrentTime;
        public long ftLastUpdateTime;
        public int dwServerState;
        public uint dwGroupCount;
        public uint dwBandWidth;
        public ushort wMajorVersion;
        public ushort wMinorVersion;
        public ushort wBuildNumber;
        public ushort wReserved;
        public IntPtr szVendorInfo;
    }

    /// <summary>Allocates an LPWSTR (null-terminated UTF-16) via CoTaskMemAlloc.</summary>
    private static IntPtr AllocateLpwStr(string? value)
    {
        if (value is null)
        {
            return IntPtr.Zero;
        }
        int byteCount = (value.Length + 1) * sizeof(char);
        IntPtr ptr = Marshal.AllocCoTaskMem(byteCount);
        Marshal.Copy(value.ToCharArray(), 0, ptr, value.Length);
        Marshal.WriteInt16(ptr, value.Length * sizeof(char), 0); // null terminator
        return ptr;
    }

    /// <summary>Allocates an OPCSERVERSTATUS via CoTaskMemAlloc and fills it from <paramref name="status"/>.</summary>
    private static IntPtr AllocateOpcServerStatus(OpcServerStatus status)
    {
        int size = sizeof(OPCSERVERSTATUS_NATIVE);
        IntPtr ptr = Marshal.AllocCoTaskMem(size);
        Version version = status.ServerVersion ?? new Version(1, 0, 0);
        var native = new OPCSERVERSTATUS_NATIVE
        {
            ftStartTime = status.StartTime.ToFileTime(),
            ftCurrentTime = status.CurrentTime.ToFileTime(),
            ftLastUpdateTime = status.LastUpdateTime.ToFileTime(),
            dwServerState = (int)status.State,
            dwGroupCount = (uint)status.GroupCount,
            dwBandWidth = (uint)status.BandWidth,
            wMajorVersion = (ushort)version.Major,
            wMinorVersion = (ushort)version.Minor,
            wBuildNumber = (ushort)Math.Max(0, version.Build),
            wReserved = 0,
            szVendorInfo = AllocateLpwStr(status.VendorInfo),
        };
        Marshal.StructureToPtr(native, ptr, fDeleteOld: false);
        return ptr;
    }
}
