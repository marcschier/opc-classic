//============================================================================
// TITLE: COpcDaGroupWrapper.h
//
// CONTENTS:
// 
// A group object for an OPC Data Access server.
//
// (c) Copyright 2002-2003 The OPC Foundation
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

#ifndef _COpcDaGroupWrapper_H_
#define _COpcDaGroupWrapper_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

class COpcDaServerWrapper;

//============================================================================
// CLASS:   COpcDaGroupWrapper
// PURPOSE: A class that implements the IOPCGroup interface.

class COpcDaGroupWrapper :
    public COpcComObject,
    public COpcCPContainer,
    public IOPCItemMgt,
    // public IOPCGroupStateMgt,
    // public IOPCSyncIO,
    // public IOPCAsyncIO2,
    public IOPCItemDeadbandMgt,
    public IOPCItemSamplingMgt,
    public IOPCSyncIO2,
    public IOPCAsyncIO3,
    public IOPCGroupStateMgt2,
    public IOPCDataCallback,
    public COpcSynchObject
{
    OPC_CLASS_NEW_DELETE()

    OPC_BEGIN_INTERFACE_TABLE(COpcDaGroupWrapper)
        OPC_INTERFACE_ENTRY(IConnectionPointContainer)
        OPC_INTERFACE_ENTRY(IOPCItemMgt)
        OPC_INTERFACE_ENTRY(IOPCItemDeadbandMgt)
        OPC_INTERFACE_ENTRY(IOPCItemSamplingMgt)
        OPC_INTERFACE_ENTRY(IOPCSyncIO)
        OPC_INTERFACE_ENTRY(IOPCSyncIO2)
        OPC_INTERFACE_ENTRY(IOPCAsyncIO2)
        OPC_INTERFACE_ENTRY(IOPCAsyncIO3)
        OPC_INTERFACE_ENTRY(IOPCGroupStateMgt)
        OPC_INTERFACE_ENTRY(IOPCGroupStateMgt2)
        OPC_INTERFACE_ENTRY(IOPCDataCallback)
    OPC_END_INTERFACE_TABLE()

public:

    //=========================================================================
    // Operators

    // Constructor
    COpcDaGroupWrapper();
    COpcDaGroupWrapper(COpcDaServerWrapper* pServer, IUnknown* ipUnknown);

    // Destructor 
    ~COpcDaGroupWrapper();

    //=========================================================================
    // Public Methods

	// Delete
	void Delete();

	// OnAdvise
	virtual void OnAdvise(REFIID riid, DWORD dwCookie);

	// OnUnadvise
	virtual void OnUnadvise(REFIID riid, DWORD dwCookie);

	//=========================================================================
    // IOPCGroupStateMgt

    // GetState
    STDMETHODIMP  GetState(
        DWORD     * pUpdateRate, 
        BOOL      * pActive, 
        LPWSTR    * ppName,
        LONG      * pTimeBias,
        FLOAT     * pPercentDeadband,
        DWORD     * pLCID,
        OPCHANDLE * phClientGroup,
        OPCHANDLE * phServerGroup
    );

    // SetState
    STDMETHODIMP  SetState( 
        DWORD     * pRequestedUpdateRate, 
        DWORD     * pRevisedUpdateRate, 
        BOOL      * pActive, 
        LONG      * pTimeBias,
        FLOAT     * pPercentDeadband,
        DWORD     * pLCID,
        OPCHANDLE * phClientGroup
    );

    // SetName
    STDMETHODIMP  SetName( 
        LPCWSTR szName
    );

    // CloneGroup
    STDMETHODIMP  CloneGroup(
        LPCWSTR     szName,
        REFIID      riid,
        LPUNKNOWN * ppUnk
    );
    
	//=========================================================================
    // IOPCGroupStateMgt2

	// SetKeepAlive
    STDMETHODIMP SetKeepAlive( 
        DWORD   dwKeepAliveTime,
        DWORD * pdwRevisedKeepAliveTime 
    );

	// GetKeepAlive
    STDMETHODIMP GetKeepAlive( 
        DWORD * pdwKeepAliveTime 
    );

    //=========================================================================
    // IOPCItemMgt

    // AddItems
    STDMETHODIMP  AddItems( 
        DWORD            dwCount,
        OPCITEMDEF     * pItemArray,
        OPCITEMRESULT ** ppAddResults,
        HRESULT       ** ppErrors
    );

    // ValidateItems
    STDMETHODIMP  ValidateItems( 
        DWORD             dwCount,
        OPCITEMDEF      * pItemArray,
        BOOL              bBlobUpdate,
        OPCITEMRESULT  ** ppValidationResults,
        HRESULT        ** ppErrors
    );

    // RemoveItems
    STDMETHODIMP  RemoveItems( 
        DWORD        dwCount,
        OPCHANDLE  * phServer,
        HRESULT   ** ppErrors
    );

    // SetActiveState
    STDMETHODIMP  SetActiveState(
        DWORD        dwCount,
        OPCHANDLE  * phServer,
        BOOL         bActive, 
        HRESULT   ** ppErrors
    );

    // SetClientHandles
    STDMETHODIMP  SetClientHandles(
        DWORD        dwCount,
        OPCHANDLE  * phServer,
        OPCHANDLE  * phClient,
        HRESULT   ** ppErrors
    );

    // SetDatatypes
    STDMETHODIMP  SetDatatypes(
        DWORD        dwCount,
        OPCHANDLE  * phServer,
        VARTYPE    * pRequestedDatatypes,
        HRESULT   ** ppErrors
    );

    // CreateEnumerator
    STDMETHODIMP  CreateEnumerator(
        REFIID      riid,
        LPUNKNOWN * ppUnk
    );

    //=========================================================================
    // IOPCAsyncIO2

    // Read
    STDMETHODIMP  Read(
        DWORD           dwCount,
        OPCHANDLE     * phServer,
        DWORD           dwTransactionID,
        DWORD         * pdwCancelID,
        HRESULT      ** ppErrors
    );

    // Write
    STDMETHODIMP  Write(
        DWORD           dwCount, 
        OPCHANDLE     * phServer,
        VARIANT       * pItemValues, 
        DWORD           dwTransactionID,
        DWORD         * pdwCancelID,
        HRESULT      ** ppErrors
    );

    // Refresh2
    STDMETHODIMP  Refresh2(
        OPCDATASOURCE   dwSource,
        DWORD           dwTransactionID,
        DWORD         * pdwCancelID
    );

    // Cancel2
    STDMETHODIMP  Cancel2(DWORD dwCancelID);

    // SetEnable
    STDMETHODIMP  SetEnable(BOOL bEnable);

    // GetEnable
    STDMETHODIMP  GetEnable(BOOL* pbEnable);

    //=========================================================================
    // IOPCItemDeadbandMgt

    // SetItemDeadband
    STDMETHODIMP SetItemDeadband( 
        DWORD 	    dwCount,
        OPCHANDLE * phServer,
        FLOAT     * pPercentDeadband,
        HRESULT  ** ppErrors
    );

    // GetItemDeadband
    STDMETHODIMP GetItemDeadband( 
        DWORD 	    dwCount,
        OPCHANDLE * phServer,
        FLOAT    ** ppPercentDeadband,
        HRESULT  ** ppErrors
    );

    // ClearItemDeadband
    STDMETHODIMP ClearItemDeadband(
        DWORD       dwCount,
        OPCHANDLE * phServer,
        HRESULT  ** ppErrors
    );

    //=========================================================================
    // IOPCItemSamplingMgt

    // SetItemSamplingRate
    STDMETHODIMP SetItemSamplingRate(
        DWORD 	    dwCount,
        OPCHANDLE * phServer,
        DWORD     * pdwRequestedSamplingRate,
        DWORD    ** ppdwRevisedSamplingRate,
        HRESULT  ** ppErrors
    );

    // GetItemSamplingRate
    STDMETHODIMP GetItemSamplingRate(
        DWORD 	    dwCount,
        OPCHANDLE * phServer,
        DWORD    ** ppdwSamplingRate,
        HRESULT  ** ppErrors
    );

    // ClearItemSamplingRate
    STDMETHODIMP ClearItemSamplingRate(
        DWORD 	    dwCount,
        OPCHANDLE * phServer,
        HRESULT  ** ppErrors
    );

    // SetItemBufferEnable
    STDMETHODIMP SetItemBufferEnable(
        DWORD       dwCount, 
        OPCHANDLE * phServer, 
        BOOL      * pbEnable,
        HRESULT  ** ppErrors
    );

    // GetItemBufferEnable
    STDMETHODIMP GetItemBufferEnable(
        DWORD       dwCount, 
        OPCHANDLE * phServer, 
        BOOL     ** ppbEnable,
        HRESULT  ** ppErrors
    );

    //=========================================================================
    // IOPCSyncIO2

    // Read
    STDMETHODIMP  Read(
        OPCDATASOURCE   dwSource,
        DWORD           dwCount, 
        OPCHANDLE     * phServer, 
        OPCITEMSTATE ** ppItemValues,
        HRESULT      ** ppErrors
    );

    // Write
    STDMETHODIMP  Write(
        DWORD        dwCount, 
        OPCHANDLE  * phServer, 
        VARIANT    * pItemValues, 
        HRESULT   ** ppErrors
    );

    // ReadMaxAge
    STDMETHODIMP ReadMaxAge(
        DWORD       dwCount, 
        OPCHANDLE * phServer, 
        DWORD     * pdwMaxAge,
        VARIANT  ** ppvValues,
        WORD     ** ppwQualities,
        FILETIME ** ppftTimeStamps,
        HRESULT  ** ppErrors
    );

    // WriteVQT
    STDMETHODIMP WriteVQT(
        DWORD         dwCount, 
        OPCHANDLE  *  phServer, 
        OPCITEMVQT *  pItemVQT,
        HRESULT    ** ppErrors
    );
    
	//=========================================================================
    // IOPCAsyncIO3

    STDMETHODIMP ReadMaxAge(
        DWORD       dwCount, 
        OPCHANDLE * phServer,
        DWORD     * pdwMaxAge,
        DWORD       dwTransactionID,
        DWORD     * pdwCancelID,
        HRESULT  ** ppErrors
    );

    STDMETHODIMP WriteVQT(
        DWORD        dwCount, 
        OPCHANDLE  * phServer,
        OPCITEMVQT * pItemVQT,
        DWORD        dwTransactionID,
        DWORD      * pdwCancelID,
        HRESULT   ** ppErrors
    );

    STDMETHODIMP RefreshMaxAge(
        DWORD   dwMaxAge,
        DWORD   dwTransactionID,
        DWORD * pdwCancelID
    );

	//=========================================================================
    // IOPCDataCallback

	// OnDataChange
    STDMETHODIMP OnDataChange(
        DWORD      dwTransid, 
        OPCHANDLE  hGroup, 
        HRESULT    hrMasterquality,
        HRESULT    hrMastererror,
        DWORD      dwCount, 
        OPCHANDLE* phClientItems, 
        VARIANT*   pvValues, 
        WORD*      pwQualities,
        FILETIME*  pftTimeStamps,
        HRESULT*   pErrors
    );

	// OnReadComplete
    STDMETHODIMP OnReadComplete(
        DWORD      dwTransid, 
        OPCHANDLE  hGroup, 
        HRESULT    hrMasterquality,
        HRESULT    hrMastererror,
        DWORD      dwCount, 
        OPCHANDLE* phClientItems, 
        VARIANT*   pvValues, 
        WORD*      pwQualities,
        FILETIME*  pftTimeStamps,
        HRESULT*   pErrors
    );

	// OnWriteComplete
    STDMETHODIMP OnWriteComplete(
        DWORD      dwTransid, 
        OPCHANDLE  hGroup, 
        HRESULT    hrMastererr, 
        DWORD      dwCount, 
        OPCHANDLE* pClienthandles, 
        HRESULT*   pErrors
    );

	// OnCancelComplete
    STDMETHODIMP OnCancelComplete(
        DWORD     dwTransid, 
        OPCHANDLE hGroup
    );

private:

	COpcDaServerWrapper* m_pServer;
	IUnknown*            m_ipUnknown;
	DWORD                m_dwConnection;
};

//============================================================================
// MACRO:   RETURN_SFALSE
// PURPOSE: Returns SFALSE if any elements in the array are not S_OK

#define RETURN_SFALSE(xResult, xCount, xErrors) \
if ((xResult) == S_OK && (*(xErrors)) != NULL)\
{ \
	for (DWORD xx = 0; xx < (DWORD)(xCount); xx++) \
	{ \
		if ((*(xErrors))[xx] != S_OK) \
		{ \
			return S_FALSE; \
		} \
	} \
} \
return xResult;

#endif // _COpcDaGroupWrapper_H_