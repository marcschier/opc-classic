//============================================================================
// TITLE: COpcHdaServer.h
//
// CONTENTS:
// 
// Implements an OPC Historical Data Access server.
//
// (c) Copyright 2003-2004 The OPC Foundation
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
// 2003/12/17 RSA   Initial implementation.

#ifndef _COpcHdaServer_H_
#define _COpcHdaServer_H_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000

#include "COpcHdaTransaction.h"

//============================================================================
// CLASS:   COpcHdaServer
// PURPOSE: A class that implements the IOPCServer interface.

class COpcHdaServer :
    public COpcCommon,
    public COpcCPContainer,
    public IOPCHDA_Server,
    public IOPCHDA_SyncRead,
    public IOPCHDA_SyncUpdate,
	public IOPCHDA_SyncAnnotations,
    public IOPCHDA_AsyncRead,
    public IOPCHDA_AsyncUpdate,
    public IOPCHDA_AsyncAnnotations,
    public IOPCHDA_Playback,
    public COpcSynchObject
{
    OPC_CLASS_NEW_DELETE()

public:

    //=========================================================================
    // Operators

    // Constructor
    COpcHdaServer();

    // Destructor 
    ~COpcHdaServer();

    //=========================================================================
    // Public Methods

    // FinalConstruct
    virtual HRESULT FinalConstruct();

    // FinalRelease
    virtual bool FinalRelease();

	//=========================================================================
    // COpcCommon

    // GetErrorString
    STDMETHODIMP GetErrorString( 
        HRESULT dwError,
        LCID    dwLocale,
        LPWSTR* ppString
    );

	//=========================================================================
    // IOpcMessageCallback

	// ProcessMessage
	virtual void ProcessMessage(COpcMessage& cMsg);

    //=========================================================================
    // IOPCHDA_Server

	STDMETHODIMP GetItemAttributes( 
		DWORD*    pdwCount,
		DWORD**   ppdwAttrID,
		LPWSTR**  ppszAttrName,
		LPWSTR**  ppszAttrDesc,
		VARTYPE** ppvtAttrDataType
	);

	STDMETHODIMP GetAggregates(
		DWORD*   pdwCount,
		DWORD**  ppdwAggrID,
		LPWSTR** ppszAggrName,
		LPWSTR** ppszAggrDesc
	);

	STDMETHODIMP GetHistorianStatus(
		OPCHDA_SERVERSTATUS* pwStatus,
		FILETIME**           pftCurrentTime,
		FILETIME**           pftStartTime,
		WORD*                pwMajorVersion,
		WORD*                pwMinorVersion,
		WORD*                pwBuildNumber,
		DWORD*               pdwMaxReturnValues,
		LPWSTR*              ppszStatusString,
		LPWSTR*              ppszVendorInfo
	);

	STDMETHODIMP GetItemHandles(
		DWORD		dwCount,
		LPWSTR*     pszItemID,
		OPCHANDLE*  phClient,
		OPCHANDLE** pphServer,
		HRESULT**   ppErrors
	);

	STDMETHODIMP ReleaseItemHandles(
		DWORD		dwCount,
		OPCHANDLE* phServer,
		HRESULT**  ppErrors
	);

	STDMETHODIMP ValidateItemIDs(
		DWORD	   dwCount,
		LPWSTR*   pszItemID,
		HRESULT** ppErrors
	);

	STDMETHODIMP CreateBrowse(
		DWORD			      dwCount,
		DWORD*                pdwAttrID,
		OPCHDA_OPERATORCODES* pOperator,
		VARIANT*              vFilter,
		IOPCHDA_Browser**     pphBrowser,
		HRESULT**             ppErrors
	);

    //=========================================================================
    // IOPCHDA_SyncRead

	STDMETHODIMP ReadRaw(
		OPCHDA_TIME*  htStartTime,
		OPCHDA_TIME*  htEndTime,
		DWORD	      dwNumValues,
		BOOL	      bBounds,
		DWORD	      dwNumItems,
		OPCHANDLE*    phServer, 
		OPCHDA_ITEM** ppItemValues,
		HRESULT**     ppErrors
	);

	STDMETHODIMP ReadProcessed(
		OPCHDA_TIME*  htStartTime,
		OPCHDA_TIME*  htEndTime,
		FILETIME      ftResampleInterval,
		DWORD         dwNumItems,
		OPCHANDLE*    phServer, 
		DWORD*        haAggregate, 
		OPCHDA_ITEM** ppItemValues,
		HRESULT**     ppErrors
	);

	STDMETHODIMP ReadAtTime(
		DWORD         dwNumTimeStamps,
		FILETIME*     ftTimeStamps,
		DWORD         dwNumItems,
		OPCHANDLE*    phServer, 
		OPCHDA_ITEM** ppItemValues,
		HRESULT**     ppErrors
	);

	STDMETHODIMP ReadModified(
		OPCHDA_TIME*          htStartTime,
		OPCHDA_TIME*          htEndTime,
		DWORD                 dwNumValues,
		DWORD                 dwNumItems,
		OPCHANDLE*            phServer, 
		OPCHDA_MODIFIEDITEM** ppItemValues,
		HRESULT**             ppErrors
	);

	STDMETHODIMP ReadAttribute(
		OPCHDA_TIME*       htStartTime,
		OPCHDA_TIME*       htEndTime,
		OPCHANDLE          hServer, 
		DWORD              dwNumAttributes,
		DWORD*             pdwAttributeIDs, 
		OPCHDA_ATTRIBUTE** ppAttributeValues,
		HRESULT**          ppErrors
	);

    //=========================================================================
    // IOPCHDA_SyncUpdate

	STDMETHODIMP QueryCapabilities(
		 OPCHDA_UPDATECAPABILITIES* pCapabilities
	);

	STDMETHODIMP Insert(
		DWORD      dwNumItems, 
		OPCHANDLE* phServer, 
		FILETIME*  ftTimeStamps,
		VARIANT*   vDataValues,
		DWORD*     pdwQualities,
		HRESULT**  ppErrors
	);

	STDMETHODIMP Replace(
		DWORD      dwNumItems, 
		OPCHANDLE* phServer, 
		FILETIME*  ftTimeStamps,
		VARIANT*   vDataValues,
		DWORD*     pdwQualities,
		HRESULT**  ppErrors
	);

	STDMETHODIMP InsertReplace(
		DWORD      dwNumItems, 
		OPCHANDLE* phServer, 
		FILETIME*  ftTimeStamps,
		VARIANT*   vDataValues,
		DWORD*     pdwQualities,
		HRESULT**  ppErrors
	);

	STDMETHODIMP DeleteRaw(
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		HRESULT**    ppErrors
	);

	STDMETHODIMP DeleteAtTime(
		DWORD      dwNumItems,
		OPCHANDLE* phServer,
		FILETIME*  ftTimeStamps,
		HRESULT**  ppErrors
	);

    //=========================================================================
    // IOPCHDA_SyncAnnotations

	STDMETHODIMP QueryCapabilities(
		 OPCHDA_ANNOTATIONCAPABILITIES* pCapabilities
	);

	STDMETHODIMP Read(
		OPCHDA_TIME*        htStartTime,
		OPCHDA_TIME*        htEndTime,
		DWORD	            dwNumItems,
		OPCHANDLE*          phServer, 
		OPCHDA_ANNOTATION** ppAnnotationValues,
		HRESULT**           ppErrors
	);

	STDMETHODIMP Insert(
		DWORD              dwNumItems, 
		OPCHANDLE*         phServer, 
		FILETIME*          ftTimeStamps,
		OPCHDA_ANNOTATION* pAnnotationValues,
		HRESULT**          ppErrors
	);

    //=========================================================================
    // IOPCHDA_AsyncRead

	STDMETHODIMP ReadRaw(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		DWORD        dwNumValues,
		BOOL         bBounds,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP AdviseRaw(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		FILETIME     ftUpdateInterval,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP ReadProcessed(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		FILETIME     ftResampleInterval,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		DWORD*       haAggregate,
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP AdviseProcessed(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		FILETIME     ftResampleInterval,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		DWORD*       haAggregate,
		DWORD        dwNumIntervals,
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP ReadAtTime(
		DWORD      dwTransactionID,
		DWORD      dwNumTimeStamps,
		FILETIME*  ftTimeStamps,
		DWORD      dwNumItems,
		OPCHANDLE* phServer, 
		DWORD*     pdwCancelID,
		HRESULT**  ppErrors
	);

	STDMETHODIMP ReadModified(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		DWORD        dwNumValues,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer, 
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP ReadAttribute(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		OPCHANDLE    hServer, 
		DWORD        dwNumAttributes,
		DWORD*       dwAttributeIDs, 
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP Cancel(
		 DWORD dwCancelID
	);

	//=========================================================================
    // IOPCHDA_AsyncUpdate

	STDMETHODIMP Insert(
		DWORD      dwTransactionID,
		DWORD      dwNumItems,
		OPCHANDLE* phServer,
		FILETIME*  ftTimeStamps,
		VARIANT*   vDataValues,
		DWORD*     pdwQualities,
		DWORD*     pdwCancelID,
		HRESULT**  ppErrors
	);

	STDMETHODIMP Replace(
		DWORD      dwTransactionID,
		DWORD      dwNumItems,
		OPCHANDLE* phServer,
		FILETIME*  ftTimeStamps,
		VARIANT*   vDataValues,
		DWORD*     pdwQualities,
		DWORD*     pdwCancelID,
		HRESULT**  ppErrors
	);

	STDMETHODIMP InsertReplace(
		DWORD      dwTransactionID,
		DWORD      dwNumItems,
		OPCHANDLE* phServer,
		FILETIME*  ftTimeStamps,
		VARIANT*   vDataValues,
		DWORD*     pdwQualities,
		DWORD*     pdwCancelID,
		HRESULT**  ppErrors
	);

	STDMETHODIMP DeleteRaw(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP DeleteAtTime(
		DWORD      dwTransactionID,
		DWORD      dwNumItems,
		OPCHANDLE* phServer,
		FILETIME*  ftTimeStamps,
		DWORD*     pdwCancelID,
		HRESULT**  ppErrors
	);

	//=========================================================================
    // IOPCHDA_AsyncAnnotations

	STDMETHODIMP Read(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer, 
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP Insert(
		DWORD              dwTransactionID,
		DWORD              dwNumItems, 
		OPCHANDLE*         phServer, 
		FILETIME*          ftTimeStamps,
		OPCHDA_ANNOTATION* pAnnotationValues,
		DWORD*             pdwCancelID,
		HRESULT**          ppErrors
	);

	//=========================================================================
    // IOPCHDA_Playback

	STDMETHODIMP ReadRawWithUpdate(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		DWORD        dwNumValues,
		FILETIME     ftUpdateDuration,
		FILETIME     ftUpdateInterval,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

	STDMETHODIMP ReadProcessedWithUpdate(
		DWORD        dwTransactionID,
		OPCHDA_TIME* htStartTime,
		OPCHDA_TIME* htEndTime,
		FILETIME     ftResampleInterval,
		DWORD        dwNumIntervals,
		FILETIME     ftUpdateInterval,
		DWORD        dwNumItems,
		OPCHANDLE*   phServer,
		DWORD*       haAggregate,
		DWORD*       pdwCancelID,
		HRESULT**    ppErrors
	);

private:

	// a class used to associate a client assigned handle with an item handle.
	struct OpcHdaItem
	{
		OPCHANDLE hClient;
		OPCHANDLE hServer;
	};

	// a class used to allow the server to shutdown when the client releases the last reference.
	class COpcHdaCallback : public IOpcMessageCallback
	{
	public:

		// QueryInterface
		STDMETHODIMP QueryInterface(REFIID iid, LPVOID* ppInterface) 
		{
			return E_NOINTERFACE; 
		} 

		// AddRef
		STDMETHODIMP_(ULONG) AddRef() 
		{
			return InterlockedIncrement((LONG*)&m_ulRefs); 
		}
		
		// Release
		STDMETHODIMP_(ULONG) Release() 
		{
			ULONG ulRefs = InterlockedIncrement((LONG*)&m_ulRefs); 

			if (ulRefs == 0)
			{
				delete m_pServer;
				delete this;
			}

			return ulRefs;
		} 

		// Constructor
		COpcHdaCallback(COpcHdaServer* pServer)
		{
			m_pServer = pServer;
			m_ulRefs  = 1;
		}

		// ProcessMessage
		virtual void ProcessMessage(COpcMessage& cMsg)
		{
			if (m_pServer != NULL)
			{
				m_pServer->ProcessMessage(cMsg);
			}
		}

	private:

		COpcHdaServer* m_pServer;
		UINT           m_ulRefs;
	};
        
	//==========================================================================
    // Private Methods

	// creates a copy of a transction if more data has to be sent,
	void CopyTransaction(COpcHdaReadTransaction& cTransaction, UINT uNoOfItems);
	void CopyTransaction(COpcHdaModifiedTransaction& cTransaction, UINT uNoOfItems);
    
	// validates the time domain for processed data and returns the requested number of values.
	HRESULT ValidateTimeDomain(
		LONGLONG llStartTime,
		LONGLONG llEndTime,
		DWORD    dwNumValues = 0
	);

	// validates the time domain for processed data and returns the requested number of values.
	HRESULT ValidateTimeDomain(
		LONGLONG llStartTime,
		LONGLONG llEndTime,
		LONGLONG llResampleInterval
	);

	// validates the server handles and returns the number of valid handles.
	UINT ValidateServerHandles(
		DWORD                   dwNumItems,
		OPCHANDLE*              phServer,
		HRESULT**               ppErrors,
		COpcArray<OpcHdaItem*>& cItems
	);

	//==========================================================================
    // Private Members

	COpcMap<OPCHANDLE,OpcHdaItem*> m_cItems;
	COpcHdaCallback*               m_pCallback;
	COpcList<DWORD>                m_cSubscriptions;
};

#endif // _COpcHdaServer_H_