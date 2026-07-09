//============================================================================
// TITLE: COpcDaServerWrapper.h
//
// CONTENTS:
// 
// Implements a wrapper for a Data Access in-proc COM server. 
//
// (c) Copyright 2004 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This code is provided by the OPC Foundation solely to assist in 
//  understanding and use of the appropriate OPC Specification(s) and may be 
//  used as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// MODIFICATION LOG:
//
// Date       By    Notes
// ---------- ---   -----
// 2004/04/22 RSA   Initial implementation.

#ifndef _COpcDaServerWrapper_H_
#define _COpcDaServerWrapper_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "OpcDaWrapper.h"

class COpcDaGroupWrapper;

//============================================================================
// CLASS:   COpcDaServerWrapper
// PURPOSE: A class that implements the IOPCServer interface.

class COpcDaServerWrapper :
    public COpcComObject,
    public COpcCPContainer,
    public IOPCCommon,
    public IOPCBrowseServerAddressSpace,
    public IOPCItemProperties,
    public IOPCServer,
    public IOPCBrowse,
    public IOPCItemIO,
    public COpcSynchObject
{
    OPC_CLASS_NEW_DELETE()

    OPC_BEGIN_INTERFACE_TABLE(COpcDaServerWrapper)
        OPC_INTERFACE_ENTRY(IOPCCommon)
        OPC_INTERFACE_ENTRY(IConnectionPointContainer)
        OPC_INTERFACE_ENTRY(IOPCServer)
        OPC_INTERFACE_ENTRY(IOPCBrowseServerAddressSpace)
        OPC_INTERFACE_ENTRY(IOPCItemProperties)
        OPC_INTERFACE_ENTRY(IOPCBrowse)
        OPC_INTERFACE_ENTRY(IOPCItemIO)
    OPC_END_INTERFACE_TABLE()

public:

    //=========================================================================
    // Operators

    // Constructor
    COpcDaServerWrapper();

    // Destructor 
    ~COpcDaServerWrapper();

    //=========================================================================
    // Public Methods

    // FinalConstruct
    virtual HRESULT FinalConstruct();

    // FinalRelease
    virtual bool FinalRelease();
	
	// WrapGroup
	HRESULT WrapGroup(REFIID riid, IUnknown** ippUnknown, OPCHANDLE* phGroup);
	
	// OnAdvise
	virtual void OnAdvise(REFIID riid, DWORD dwCookie);

	// OnUnadvise
	virtual void OnUnadvise(REFIID riid, DWORD dwCookie);

	//=========================================================================
	// IOPCShutdown

	// ShutdownRequest
    STDMETHODIMP ShutdownRequest(
        LPCWSTR szReason
    );

	//==========================================================================
    // IOPCCommon

    // SetLocaleID
    STDMETHODIMP SetLocaleID(LCID dwLcid);

    // GetLocaleID
    STDMETHODIMP GetLocaleID(LCID *pdwLcid);

    // QueryAvailableLocaleIDs
    STDMETHODIMP QueryAvailableLocaleIDs(DWORD* pdwCount, LCID** pdwLcid);

    // GetErrorString
    STDMETHODIMP GetErrorString(HRESULT dwError, LPWSTR* ppString);

    // SetClientName
    STDMETHODIMP SetClientName(LPCWSTR szName);

    //=========================================================================
    // IOPCServer

    // AddGroup
    STDMETHODIMP AddGroup(
        LPCWSTR    szName,
        BOOL       bActive,
        DWORD      dwRequestedUpdateRate,
        OPCHANDLE  hClientGroup,
        LONG*      pTimeBias,
        FLOAT*     pPercentDeadband,
        DWORD      dwLCID,
        OPCHANDLE* phServerGroup,
        DWORD*     pRevisedUpdateRate,
        REFIID     riid,
        LPUNKNOWN* ppUnk
    );

    // GetErrorString
    STDMETHODIMP GetErrorString( 
        HRESULT dwError,
        LCID    dwLocale,
        LPWSTR* ppString
    );

    // GetGroupByName
    STDMETHODIMP GetGroupByName(
        LPCWSTR    szName,
        REFIID     riid,
        LPUNKNOWN* ppUnk
    );

    // GetStatus
    STDMETHODIMP GetStatus( 
        OPCSERVERSTATUS** ppServerStatus
    );

    // RemoveGroup
    STDMETHODIMP RemoveGroup(
        OPCHANDLE hServerGroup,
        BOOL      bForce
    );

    // CreateGroupEnumerator
    STDMETHODIMP CreateGroupEnumerator(
        OPCENUMSCOPE dwScope, 
        REFIID       riid, 
        LPUNKNOWN*   ppUnk
    );

    //=========================================================================
    // IOPCBrowseServerAddressSpace
    
    // QueryOrganization
    STDMETHODIMP QueryOrganization(OPCNAMESPACETYPE* pNameSpaceType);
    
    // ChangeBrowsePosition
    STDMETHODIMP ChangeBrowsePosition(
        OPCBROWSEDIRECTION dwBrowseDirection,  
        LPCWSTR            szString
    );

    // BrowseOPCItemIDs
    STDMETHODIMP BrowseOPCItemIDs(
        OPCBROWSETYPE   dwBrowseFilterType,
        LPCWSTR         szFilterCriteria,  
        VARTYPE         vtDataTypeFilter,     
        DWORD           dwAccessRightsFilter,
        LPENUMSTRING*   ppIEnumString
    );

    // GetItemID
    STDMETHODIMP GetItemID(
        LPWSTR  wszItemName,
        LPWSTR* pszItemID
    );

    // BrowseAccessPaths
    STDMETHODIMP BrowseAccessPaths(
        LPCWSTR       szItemID,  
        LPENUMSTRING* ppIEnumString
    );

    //=========================================================================
    // IOPCItemProperties

    // QueryAvailableProperties
    STDMETHODIMP QueryAvailableProperties( 
        LPWSTR     szItemID,
        DWORD    * pdwCount,
        DWORD   ** ppPropertyIDs,
        LPWSTR  ** ppDescriptions,
        VARTYPE ** ppvtDataTypes
    );

    // GetItemProperties
    STDMETHODIMP GetItemProperties ( 
        LPWSTR     szItemID,
        DWORD      dwCount,
        DWORD    * pdwPropertyIDs,
        VARIANT ** ppvData,
        HRESULT ** ppErrors
    );

    // LookupItemIDs
    STDMETHODIMP LookupItemIDs( 
        LPWSTR     szItemID,
        DWORD      dwCount,
        DWORD    * pdwPropertyIDs,
        LPWSTR  ** ppszNewItemIDs,
        HRESULT ** ppErrors
    );

    //=========================================================================
    // IOPCBrowse

    // GetProperties
    STDMETHODIMP GetProperties( 
        DWORD		        dwItemCount,
        LPWSTR*             pszItemIDs,
        BOOL		        bReturnPropertyValues,
        DWORD		        dwPropertyCount,
        DWORD*              pdwPropertyIDs,
        OPCITEMPROPERTIES** ppItemProperties 
    );

    // Browse
    STDMETHODIMP Browse(
	    LPWSTR	           szItemName,
	    LPWSTR*	           pszContinuationPoint,
	    DWORD              dwMaxElementsReturned,
		OPCBROWSEFILTER    dwFilter,
	    LPWSTR             szElementNameFilter,
	    LPWSTR             szVendorFilter,
	    BOOL               bReturnAllProperties,
	    BOOL               bReturnPropertyValues,
	    DWORD              dwPropertyCount,
	    DWORD*             pdwPropertyIDs,
	    BOOL*              pbMoreElements,
	    DWORD*	           pdwCount,
	    OPCBROWSEELEMENT** ppBrowseElements
    );
    
    //=========================================================================
    // IOPCItemIO

    // Read
    STDMETHODIMP Read(
        DWORD       dwCount, 
        LPCWSTR   * pszItemIDs,
        DWORD     * pdwMaxAge,
        VARIANT  ** ppvValues,
        WORD     ** ppwQualities,
        FILETIME ** ppftTimeStamps,
        HRESULT  ** ppErrors
    );

    // WriteVQT
    STDMETHODIMP WriteVQT(
        DWORD         dwCount, 
        LPCWSTR    *  pszItemIDs,
        OPCITEMVQT *  pItemVQT,
        HRESULT    ** ppErrors
    );

private:
        
    //==========================================================================
    // Private Members

	IXMLDOMElement*                        m_ipSelfRegInfo;
	IUnknown*                              m_ipUnknown;
	DWORD                                  m_dwConnection;
	COpcMap<OPCHANDLE,COpcDaGroupWrapper*> m_cGroups;
};

#endif // _COpcDaServerWrapper_H_