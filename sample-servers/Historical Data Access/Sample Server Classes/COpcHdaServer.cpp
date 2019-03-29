//============================================================================
// TITLE: COpcHdaServer.cpp
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

#include "StdAfx.h"
#include "COpcHdaServer.h"
#include "COpcHdaHistorian.h"
#include "COpcHdaAttribute.h"
#include "COpcHdaAggregate.h"
#include "COpcHdaBrowser.h"
#include "COpcHdaTime.h"

//============================================================================
// COpcHdaServer

// Constructor
COpcHdaServer::COpcHdaServer()
{
}

// Destructor
COpcHdaServer::~COpcHdaServer()
{
}

// FinalConstruct
HRESULT COpcHdaServer::FinalConstruct()
{
	COpcLock cLock(*this);

	// register callback interfaces.
    RegisterInterface(IID_IOPCShutdown);
    RegisterInterface(IID_IOPCHDA_DataCallback);

	// create callback object.
	m_pCallback = new COpcHdaServer::COpcHdaCallback(this);

	return S_OK;
}

// FinalRelease
bool COpcHdaServer::FinalRelease()
{
	COpcLock cLock(*this);

	// release all item handles.
	OPC_POS pos = m_cItems.GetStartPosition();

	while (pos != NULL)
	{
		OPCHANDLE hServer = NULL;
		OpcHdaItem* pItem = NULL; 
		m_cItems.GetNextAssoc(pos, hServer, pItem);

		delete pItem;
	}

	m_cItems.RemoveAll();

	// release all active subscriptions.
	while (m_cSubscriptions.GetCount() > 0)
	{
		GetHistorian().CancelTransaction(m_cSubscriptions.RemoveHead());
	}
	
	// will delete the server when all transactions have completed.
	m_pCallback->Release();
	m_pCallback = NULL;

	// caller should not delete server.
	return false;
}

//=========================================================================
// COpcCommon

// GetErrorString
HRESULT COpcHdaServer::GetErrorString( 
    HRESULT dwError,
    LCID    dwLocale,
    LPWSTR* ppString
)
{
    return COpcCommon::GetErrorString(OPC_MESSAGE_MODULE_NAME_HDA, dwError, dwLocale, ppString);
}

//=========================================================================
// IOPCHDA_Server

// GetItemAttributes
HRESULT COpcHdaServer::GetItemAttributes( 
	DWORD*    pdwCount,
	DWORD**   ppdwAttrID,
	LPWSTR**  ppszAttrName,
	LPWSTR**  ppszAttrDesc,
	VARTYPE** ppvtAttrDataType
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (pdwCount == NULL || ppdwAttrID == NULL || ppszAttrName == NULL || ppszAttrDesc == NULL ||  ppvtAttrDataType == NULL)
	{
		return E_INVALIDARG;
	}

	// the set of possible attributes is fixed at compile time.
	COpcHdaAttribute::Create(
		*pdwCount,
		*ppdwAttrID,
		*ppszAttrName,
		*ppszAttrDesc,
		*ppvtAttrDataType
	);

    return S_OK;
}

// GetAggregates
HRESULT COpcHdaServer::GetAggregates(
	DWORD*   pdwCount,
	DWORD**  ppdwAggrID,
	LPWSTR** ppszAggrName,
	LPWSTR** ppszAggrDesc
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (pdwCount == NULL || ppdwAggrID == NULL || ppszAggrName == NULL || ppszAggrDesc == NULL)
	{
		return E_INVALIDARG;
	}

	// the set of possible attributes is fixed at compile time.
	COpcHdaAggregate::Create(
		*pdwCount,
		*ppdwAggrID,
		*ppszAggrName,
		*ppszAggrDesc
	);

    return S_OK;
}

// GetHistorianStatus
HRESULT COpcHdaServer::GetHistorianStatus(
	OPCHDA_SERVERSTATUS* pwStatus,
	FILETIME**           pftCurrentTime,
	FILETIME**           pftStartTime,
	WORD*                pwMajorVersion,
	WORD*                pwMinorVersion,
	WORD*                pwBuildNumber,
	DWORD*               pdwMaxReturnValues,
	LPWSTR*              ppszStatusString,
	LPWSTR*              ppszVendorInfo
)
{
	return GetHistorian().GetHistorianStatus(
		pwStatus,
		pftCurrentTime,
		pftStartTime,
		pwMajorVersion,
		pwMinorVersion,
		pwBuildNumber,
		pdwMaxReturnValues,
		ppszStatusString,
		ppszVendorInfo
	);
}

// GetItemHandles
HRESULT COpcHdaServer::GetItemHandles(
	DWORD		dwCount,
	LPWSTR*     pszItemID,
	OPCHANDLE*  phClient,
	OPCHANDLE** pphServer,
	HRESULT**   ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwCount == 0 || pszItemID == NULL || phClient == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// lookup handles.
	*pphServer = GetHistorian().GetItemHandles(dwCount, pszItemID);

	// populate errors array.
	*ppErrors = OpcArrayAlloc(HRESULT, dwCount);

	bool bError = false;

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		if ((*pphServer)[ii] == NULL)
		{
			(*ppErrors)[ii] = OPC_E_UNKNOWNITEMID;
			bError = true;
		}
		else
		{
			OpcHdaItem* pItem = new OpcHdaItem();

			pItem->hClient = phClient[ii];
			pItem->hServer = (*pphServer)[ii];

			m_cItems[(OPCHANDLE)pItem] = pItem;

			(*pphServer)[ii] = (OPCHANDLE)pItem;		
			(*ppErrors)[ii]  = S_OK;
		}
	}

	// indicate whether an error occurred with the return code.
	return (bError)?S_FALSE:S_OK;
}

// ReleaseItemHandles
HRESULT COpcHdaServer::ReleaseItemHandles(
	DWORD	   dwCount,
	OPCHANDLE* phServer,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwCount == 0 || phServer == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// populate errors array.
	*ppErrors = OpcArrayAlloc(HRESULT, dwCount);

	bool bError = false;

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		(*ppErrors)[ii] = S_OK;
	
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
			bError = true;
		}
		else
		{
			m_cItems.RemoveKey(phServer[ii]);
			delete pItem;
		}
	}

	// indicate whether an error occurred with the return code.
	return (bError)?S_FALSE:S_OK;
}

// ValidateItemIDs
HRESULT COpcHdaServer::ValidateItemIDs(
	DWORD	  dwCount,
	LPWSTR*   pszItemID,
	HRESULT** ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwCount == 0 || pszItemID == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// lookup handles.
	OPCHANDLE* phHandles = GetHistorian().GetItemHandles(dwCount, pszItemID);

	// populate errors array.
	*ppErrors = OpcArrayAlloc(HRESULT, dwCount);

	bool bError = false;

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		(*ppErrors)[ii] = S_OK;

		if (phHandles[ii] == NULL)
		{
			(*ppErrors)[ii] = OPC_E_UNKNOWNITEMID;
			bError = true;
		}
	}

	// free memory.
	OpcFree(phHandles);

	// indicate whether an error occurred with the return code.
	return (bError)?S_FALSE:S_OK;
}

// CreateBrowse
HRESULT COpcHdaServer::CreateBrowse(
	DWORD			      dwCount,
	DWORD*                pdwAttrID,
	OPCHDA_OPERATORCODES* pOperator,
	VARIANT*              vFilter,
	IOPCHDA_Browser**     pphBrowser,
	HRESULT**             ppErrors
)
{
	COpcLock cLock(*this);
	
	// check arguments.
	if (pphBrowser == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	if (dwCount > 0)
	{
		if (pdwAttrID == NULL || pOperator == NULL || vFilter == NULL)
		{
			return E_INVALIDARG;
		}
	}

	// create filter list.
	COpcHdaBrowseFilterList cFilters;
	COpcHdaBrowseFilter::Create(dwCount, pdwAttrID, pOperator, vFilter, cFilters);

	// return error codes for individual filters.
	bool bError = false;

	*ppErrors = OpcArrayAlloc(HRESULT, dwCount);

	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		(*ppErrors)[ii] = cFilters[ii]->GetError();

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// create browser object (takes ownership of filter objects).
	COpcHdaBrowser* pBrowser = new COpcHdaBrowser(cFilters);

	HRESULT hResult = pBrowser->QueryInterface(IID_IOPCHDA_Browser, (void**)pphBrowser);

	if (FAILED(hResult))
	{
		*pphBrowser = NULL;
	}

	// release local reference.
	pBrowser->Release();

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

//=========================================================================
// IOPCHDA_SyncRead

// ReadRaw
HRESULT COpcHdaServer::ReadRaw(
	OPCHDA_TIME*  htStartTime,
	OPCHDA_TIME*  htEndTime,
	DWORD	      dwNumValues,
	BOOL	      bBounds,
	DWORD	      dwNumItems,
	OPCHANDLE*    phServer, 
	OPCHDA_ITEM** ppItemValues,
	HRESULT**     ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || phServer == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppItemValues = NULL;
	*ppErrors     = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, dwNumValues);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// allocate return parameters.
	*ppItemValues = OpcArrayAlloc(OPCHDA_ITEM, dwNumItems);
	*ppErrors     = OpcArrayAlloc(HRESULT, dwNumItems);

	memset(*ppItemValues, 0, sizeof(OPCHDA_ITEM)*dwNumItems);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumItems);

	// read data for each item.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
		}

		else
		{
			(*ppErrors)[ii] = GetHistorian().ReadRaw(
				llStartTime,
				llEndTime,
				dwNumValues,
				(bBounds)?true:false,
				0,
				pItem->hServer,
				(*ppItemValues)[ii]
			);
		}

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

// ReadProcessed
HRESULT COpcHdaServer::ReadProcessed(
	OPCHDA_TIME*  htStartTime,
	OPCHDA_TIME*  htEndTime,
	FILETIME      ftResampleInterval,
	DWORD         dwNumItems,
	OPCHANDLE*    phServer, 
	DWORD*        haAggregate, 
	OPCHDA_ITEM** ppItemValues,
	HRESULT**     ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || phServer == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppItemValues = NULL;
	*ppErrors     = NULL;

	LONGLONG llStartTime        = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime          = OpcHdaResolveTime(*htEndTime);
	LONGLONG llResampleInterval = OpcHdaInt64FromFILETIME(ftResampleInterval);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, llResampleInterval);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// allocate return parameters.
	*ppItemValues = OpcArrayAlloc(OPCHDA_ITEM, dwNumItems);
	*ppErrors     = OpcArrayAlloc(HRESULT, dwNumItems);

	memset(*ppItemValues, 0, sizeof(OPCHDA_ITEM)*dwNumItems);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumItems);

	// read data for each item.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
		}

		else
		{
			(*ppErrors)[ii] = GetHistorian().ReadProcessed(
				llStartTime,
				llEndTime,
				llResampleInterval,
				haAggregate[ii],
				pItem->hServer,
				(*ppItemValues)[ii]
			);
		}

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

// ReadAtTime
HRESULT COpcHdaServer::ReadAtTime(
	DWORD         dwNumTimeStamps,
	FILETIME*     ftTimeStamps,
	DWORD         dwNumItems,
	OPCHANDLE*    phServer, 
	OPCHDA_ITEM** ppItemValues,
	HRESULT**     ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumTimeStamps == NULL || ftTimeStamps == NULL || dwNumItems == 0 || phServer == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppItemValues = NULL;
	*ppErrors     = NULL;

	// convert FILETIMEs to Int64s
	COpcArray<LONGLONG> cTimestamps(dwNumTimeStamps);

	for (DWORD ii = 0; ii < dwNumTimeStamps; ii++)
	{
		cTimestamps[ii] = OpcHdaInt64FromFILETIME(ftTimeStamps[ii]);
	}

	// allocate return parameters.
	*ppItemValues = OpcArrayAlloc(OPCHDA_ITEM, dwNumItems);
	*ppErrors     = OpcArrayAlloc(HRESULT, dwNumItems);

	memset(*ppItemValues, 0, sizeof(OPCHDA_ITEM)*dwNumItems);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumItems);

	// read data for each item.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
		}

		else
		{
			(*ppErrors)[ii] = GetHistorian().ReadAtTime(
				cTimestamps,
				pItem->hServer,
				(*ppItemValues)[ii]
			);
		}

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

// ReadModified
HRESULT COpcHdaServer::ReadModified(
	OPCHDA_TIME*          htStartTime,
	OPCHDA_TIME*          htEndTime,
	DWORD                 dwNumValues,
	DWORD                 dwNumItems,
	OPCHANDLE*            phServer, 
	OPCHDA_MODIFIEDITEM** ppItemValues,
	HRESULT**             ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || phServer == NULL || ppItemValues == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppItemValues = NULL;
	*ppErrors     = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, dwNumValues);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// allocate return parameters.
	*ppItemValues = OpcArrayAlloc(OPCHDA_MODIFIEDITEM, dwNumItems);
	*ppErrors     = OpcArrayAlloc(HRESULT, dwNumItems);

	memset(*ppItemValues, 0, sizeof(OPCHDA_MODIFIEDITEM)*dwNumItems);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumItems);

	// read data for each item.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
		}

		else
		{
			(*ppErrors)[ii] = GetHistorian().ReadModified(
				llStartTime,
				llEndTime,
				dwNumValues,
				pItem->hServer,
				(*ppItemValues)[ii]
			);
		}

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

// ReadAttribute
HRESULT COpcHdaServer::ReadAttribute(
	OPCHDA_TIME*       htStartTime,
	OPCHDA_TIME*       htEndTime,
	OPCHANDLE          hServer, 
	DWORD              dwNumAttributes,
	DWORD*             pdwAttributeIDs, 
	OPCHDA_ATTRIBUTE** ppAttributeValues,
	HRESULT**          ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumAttributes == 0 || pdwAttributeIDs == NULL || ppAttributeValues == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppAttributeValues = NULL;
	*ppErrors          = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	if (llStartTime == 0 && llEndTime == 0)
	{
		return E_INVALIDARG;
	}

	// lookup item handle.
	OpcHdaItem* pItem = NULL;

	if (!m_cItems.Lookup(hServer, pItem))
	{
		return OPC_E_INVALIDHANDLE;
	}

	// allocate return parameters.
	*ppAttributeValues = OpcArrayAlloc(OPCHDA_ATTRIBUTE, dwNumAttributes);
	*ppErrors          = OpcArrayAlloc(HRESULT, dwNumAttributes);

	memset(*ppAttributeValues, 0, sizeof(OPCHDA_ATTRIBUTE)*dwNumAttributes);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumAttributes);

	// read data for each attribute.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumAttributes; ii++)
	{
		(*ppErrors)[ii] = GetHistorian().ReadAttribute(
			llStartTime,
			llEndTime,
			pItem->hServer,
			pdwAttributeIDs[ii],
			(*ppAttributeValues)[ii]
		);

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

//=========================================================================
// IOPCHDA_SyncUpdate

// QueryCapabilities
HRESULT COpcHdaServer::QueryCapabilities(
		OPCHDA_UPDATECAPABILITIES* pCapabilities
)
{
	COpcLock cLock(*this);

	if (pCapabilities == NULL)
	{
		return E_INVALIDARG;
	}
	
	*pCapabilities = (OPCHDA_UPDATECAPABILITIES)(OPCHDA_INSERTCAP | OPCHDA_REPLACECAP | OPCHDA_INSERTREPLACECAP | OPCHDA_DELETERAWCAP | OPCHDA_DELETEATTIMECAP);

	return S_OK;
}

// Insert
HRESULT COpcHdaServer::Insert(
	DWORD      dwNumItems, 
	OPCHANDLE* phServer, 
	FILETIME*  ftTimeStamps,
	VARIANT*   vDataValues,
	DWORD*     pdwQualities,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || phServer == NULL || ftTimeStamps == NULL || vDataValues == NULL || pdwQualities == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	*ppErrors = NULL;

	// convert external handles to internal handles.
	OPCHANDLE* phInternal = OpcArrayAlloc(OPCHANDLE, dwNumItems);

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			phInternal[ii] = NULL;
			continue;
		}

		phInternal[ii] = pItem->hServer;	
	}

	HRESULT hResult = GetHistorian().Update(
		OPCHDA_INSERT,
		dwNumItems,
		phInternal,
		ftTimeStamps,
		vDataValues,
		pdwQualities,
		ppErrors
	);

	OpcFree(phInternal);

	return hResult;
}

// Replace
HRESULT COpcHdaServer::Replace(
	DWORD      dwNumItems, 
	OPCHANDLE* phServer, 
	FILETIME*  ftTimeStamps,
	VARIANT*   vDataValues,
	DWORD*     pdwQualities,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || phServer == NULL || ftTimeStamps == NULL || vDataValues == NULL || pdwQualities == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	*ppErrors = NULL;

	// convert external handles to internal handles.
	OPCHANDLE* phInternal = OpcArrayAlloc(OPCHANDLE, dwNumItems);

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			phInternal[ii] = NULL;
			continue;
		}

		phInternal[ii] = pItem->hServer;	
	}

	HRESULT hResult = GetHistorian().Update(
		OPCHDA_REPLACE,
		dwNumItems,
		phInternal,
		ftTimeStamps,
		vDataValues,
		pdwQualities,
		ppErrors
	);

	OpcFree(phInternal);

	return hResult;
}

// InsertReplace
HRESULT COpcHdaServer::InsertReplace(
	DWORD      dwNumItems, 
	OPCHANDLE* phServer, 
	FILETIME*  ftTimeStamps,
	VARIANT*   vDataValues,
	DWORD*     pdwQualities,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || phServer == NULL || ftTimeStamps == NULL || vDataValues == NULL || pdwQualities == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	*ppErrors = NULL;

	// convert external handles to internal handles.
	OPCHANDLE* phInternal = OpcArrayAlloc(OPCHANDLE, dwNumItems);

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			phInternal[ii] = NULL;
			continue;
		}

		phInternal[ii] = pItem->hServer;	
	}

	HRESULT hResult = GetHistorian().Update(
		OPCHDA_INSERTREPLACE,
		dwNumItems,
		phInternal,
		ftTimeStamps,
		vDataValues,
		pdwQualities,
		ppErrors
	);

	OpcFree(phInternal);

	return hResult;
}

// DeleteRaw
HRESULT COpcHdaServer::DeleteRaw(
	OPCHDA_TIME* htStartTime,
	OPCHDA_TIME* htEndTime,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || phServer == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppErrors = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// allocate return parameters.
	*ppErrors = OpcArrayAlloc(HRESULT, dwNumItems);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumItems);

	// read data for each item.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
		}

		else
		{
			(*ppErrors)[ii] = GetHistorian().Delete(
				llStartTime,
				llEndTime,
				pItem->hServer
			);
		}

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

// DeleteAtTime
HRESULT COpcHdaServer::DeleteAtTime(
	DWORD      dwNumItems,
	OPCHANDLE* phServer,
	FILETIME*  ftTimeStamps,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || phServer == NULL || ftTimeStamps == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	*ppErrors = NULL;

	// convert external handles to internal handles.
	OPCHANDLE* phInternal = OpcArrayAlloc(OPCHANDLE, dwNumItems);

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			phInternal[ii] = NULL;
			continue;
		}

		phInternal[ii] = pItem->hServer;	
	}

	HRESULT hResult = GetHistorian().Update(
		OPCHDA_DELETE,
		dwNumItems,
		phInternal,
		ftTimeStamps,
		NULL,
		NULL,
		ppErrors
	);

	OpcFree(phInternal);

	return hResult;
}

//=========================================================================
// IOPCHDA_SyncAnnotations

// QueryCapabilities
HRESULT COpcHdaServer::QueryCapabilities(
		OPCHDA_ANNOTATIONCAPABILITIES* pCapabilities
)
{
	COpcLock cLock(*this);

	if (pCapabilities == NULL)
	{
		return E_INVALIDARG;
	}
	
	*pCapabilities = (OPCHDA_ANNOTATIONCAPABILITIES)(OPCHDA_READANNOTATIONCAP | OPCHDA_INSERTANNOTATIONCAP);

	return S_OK;
}

// Read
HRESULT COpcHdaServer::Read(
	OPCHDA_TIME*        htStartTime,
	OPCHDA_TIME*        htEndTime,
	DWORD	            dwNumItems,
	OPCHANDLE*          phServer, 
	OPCHDA_ANNOTATION** ppAnnotationValues,
	HRESULT**           ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || phServer == NULL || ppAnnotationValues == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppAnnotationValues = NULL;
	*ppErrors           = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// allocate return parameters.
	*ppAnnotationValues = OpcArrayAlloc(OPCHDA_ANNOTATION, dwNumItems);
	*ppErrors           = OpcArrayAlloc(HRESULT, dwNumItems);

	memset(*ppAnnotationValues, 0, sizeof(OPCHDA_ANNOTATION)*dwNumItems);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumItems);

	// read data for each item.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;
		}

		else
		{
			(*ppErrors)[ii] = GetHistorian().ReadAnnotations(
				llStartTime,
				llEndTime,
				pItem->hServer,
				(*ppAnnotationValues)[ii]
			);
		}

		if ((*ppErrors)[ii] != S_OK)
		{
			bError = true;
		}
	}

	// return flag indicating whether an error exists.
	return (bError)?S_FALSE:S_OK;
}

// Insert
HRESULT COpcHdaServer::Insert(
	DWORD              dwNumItems, 
	OPCHANDLE*         phServer, 
	FILETIME*          ftTimeStamps,
	OPCHDA_ANNOTATION* pAnnotationValues,
	HRESULT**          ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || phServer == NULL || ftTimeStamps == NULL ||  pAnnotationValues == NULL || ppErrors == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*ppErrors = NULL;

	// convert external handles to internal handles.
	OPCHANDLE* phInternal = OpcArrayAlloc(OPCHANDLE, dwNumItems);

	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		OpcHdaItem* pItem = NULL;

		if (!m_cItems.Lookup(phServer[ii], pItem))
		{
			phInternal[ii] = NULL;
			continue;
		}

		phInternal[ii] = pItem->hServer;	
	}

	// update the database.
	HRESULT hResult = GetHistorian().InsertAnnotations(
		dwNumItems,
		phInternal,
		ftTimeStamps,
		pAnnotationValues,
		ppErrors
	);

	OpcFree(phInternal);

	// all done.
	return hResult;
}

//=========================================================================
// IOPCHDA_AsyncRead

// ReadRaw
HRESULT COpcHdaServer::ReadRaw(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	OPCHDA_TIME* htEndTime,
	DWORD        dwNumValues,
	BOOL         bBounds,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer,
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, dwNumValues);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaReadTransaction::ReadRaw(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		dwNumValues,
		(bBounds)?true:false,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// AdviseRaw
HRESULT COpcHdaServer::AdviseRaw(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	FILETIME     ftUpdateInterval,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer,
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || dwNumItems == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime      = OpcHdaResolveTime(*htStartTime);
	LONGLONG llUpdateInterval = OpcHdaInt64FromFILETIME(ftUpdateInterval);

	// check that the start time was provided.
	if (llStartTime == 0)
	{
		return E_INVALIDARG;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// adjust update rate.
	bool bUnsupportedRate = false;

	if (llUpdateInterval%OPCHDA_MAX_UPDATE_INTERVAL != 0)
	{
		llUpdateInterval += OPCHDA_MAX_UPDATE_INTERVAL - llUpdateInterval%OPCHDA_MAX_UPDATE_INTERVAL;
		bUnsupportedRate  = true;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaReadTransaction::AdviseRaw(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llUpdateInterval,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// unsupported update rate.
	if (bUnsupportedRate)
	{
		return OPC_S_UNSUPPORTEDRATE;
	}

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;
}

// ReadProcessed
HRESULT COpcHdaServer::ReadProcessed(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	OPCHDA_TIME* htEndTime,
	FILETIME     ftResampleInterval,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer,
	DWORD*       haAggregate,
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || pdwCancelID == NULL || haAggregate == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime        = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime          = OpcHdaResolveTime(*htEndTime);
	LONGLONG llResampleInterval = OpcHdaInt64FromFILETIME(ftResampleInterval);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, llResampleInterval);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);
	COpcArray<DWORD>     cAggregates(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cAggregates[uIndex]    = haAggregate[ii];
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaReadTransaction::ReadProcessed(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		llResampleInterval,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cAggregates.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;
}

// AdviseProcessed
HRESULT COpcHdaServer::AdviseProcessed(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	FILETIME     ftResampleInterval,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer,
	DWORD*       haAggregate,
	DWORD        dwNumIntervals,
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || dwNumItems == 0 || pdwCancelID == NULL || haAggregate == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime        = OpcHdaResolveTime(*htStartTime);
	LONGLONG llResampleInterval = OpcHdaInt64FromFILETIME(ftResampleInterval);
	LONGLONG llEndTime          = llStartTime + dwNumIntervals*llResampleInterval;

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, llResampleInterval);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);
	COpcArray<DWORD>     cAggregates(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cAggregates[uIndex]    = haAggregate[ii];
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaReadTransaction::AdviseProcessed(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		llResampleInterval,
		dwNumIntervals*llResampleInterval,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cAggregates.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;
}

// ReadAtTime
HRESULT COpcHdaServer::ReadAtTime(
	DWORD      dwTransactionID,
	DWORD      dwNumTimeStamps,
	FILETIME*  ftTimeStamps,
	DWORD      dwNumItems,
	OPCHANDLE* phServer, 
	DWORD*     pdwCancelID,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumTimeStamps == 0 || dwNumItems == 0 || ftTimeStamps == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaReadTransaction::ReadAtTime(
		m_pCallback,
		dwTransactionID,
		dwNumTimeStamps,
		ftTimeStamps,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// ReadModified
HRESULT COpcHdaServer::ReadModified(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	OPCHDA_TIME* htEndTime,
	DWORD        dwNumValues,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer, 
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, dwNumValues);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaModifiedTransaction::ReadModified(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		dwNumValues,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// ReadAttribute
HRESULT COpcHdaServer::ReadAttribute(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	OPCHDA_TIME* htEndTime,
	OPCHANDLE    hServer, 
	DWORD        dwNumAttributes,
	DWORD*       dwAttributeIDs, 
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumAttributes == 0 || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	if (llStartTime == 0 && llEndTime == 0)
	{
		return E_INVALIDARG;
	}

	// lookup item handle.
	OpcHdaItem* pItem = NULL;

	if (!m_cItems.Lookup(hServer, pItem))
	{
		return OPC_E_INVALIDHANDLE;
	}

	// allocate return parameters.
	*ppErrors = OpcArrayAlloc(HRESULT, dwNumAttributes);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumAttributes);

	// read data for each attribute.
	bool bError = false;

	for (DWORD ii = 0; ii < dwNumAttributes; ii++)
	{
		(*ppErrors)[ii] = S_OK;
	}
	
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaAttributeTransaction::ReadAttributes(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		pItem->hServer,
		pItem->hClient,
		dwNumAttributes,
		dwAttributeIDs,
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return S_OK;
}

// Cancel
HRESULT COpcHdaServer::Cancel(DWORD dwCancelID)
{
	COpcLock cLock(*this);

	// may be a subscription - search and remove.
	OPC_POS pos = m_cSubscriptions.GetHeadPosition();

	while (pos != NULL)
	{
		if (dwCancelID == m_cSubscriptions[pos])
		{
			m_cSubscriptions.RemoveAt(pos);
			break;
		}

		m_cSubscriptions.GetNext(pos);
	}

	// cancel transaction.
	if (!GetHistorian().CancelTransaction(dwCancelID))
	{
		return E_FAIL;
	}

	// all done.
    return S_OK;
}

//=========================================================================
// IOPCHDA_AsyncUpdate

// Insert
HRESULT COpcHdaServer::Insert(
	DWORD      dwTransactionID,
	DWORD      dwNumItems,
	OPCHANDLE* phServer,
	FILETIME*  ftTimeStamps,
	VARIANT*   vDataValues,
	DWORD*     pdwQualities,
	DWORD*     pdwCancelID,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || ftTimeStamps == NULL || vDataValues == NULL || pdwQualities == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);
	COpcArray<VARIANT>   cValues(uCount);
	COpcArray<DWORD>     cQualities(uCount);
	COpcArray<LONGLONG>  cTimestamps(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cQualities[uIndex]     = pdwQualities[ii];
			cTimestamps[uIndex]    = OpcHdaInt64FromFILETIME(ftTimeStamps[ii]);

			VariantInit(&(cValues[uIndex]));
			VariantCopy(&(cValues[uIndex]), &(vDataValues[ii]));
			
			uIndex++;
		}
	}
		
	// create transaction (passed ownership of contents of VARIANTs).
	COpcHdaTransaction* pTransaction = COpcHdaUpdateTransaction::Update(
		m_pCallback,
		OPCHDA_INSERT,
		dwTransactionID,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cValues.GetData(),
		cQualities.GetData(),
		cTimestamps.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// Replace
HRESULT COpcHdaServer::Replace(
	DWORD      dwTransactionID,
	DWORD      dwNumItems,
	OPCHANDLE* phServer,
	FILETIME*  ftTimeStamps,
	VARIANT*   vDataValues,
	DWORD*     pdwQualities,
	DWORD*     pdwCancelID,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || ftTimeStamps == NULL || vDataValues == NULL || pdwQualities == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);
	COpcArray<VARIANT>   cValues(uCount);
	COpcArray<DWORD>     cQualities(uCount);
	COpcArray<LONGLONG>  cTimestamps(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cQualities[uIndex]     = pdwQualities[ii];
			cTimestamps[uIndex]    = OpcHdaInt64FromFILETIME(ftTimeStamps[ii]);
			
			VariantInit(&(cValues[uIndex]));
			VariantCopy(&(cValues[uIndex]), &(vDataValues[ii]));
			
			uIndex++;
		}
	}
		
	// create transaction (passed ownership of contents of VARIANTs).
	COpcHdaTransaction* pTransaction = COpcHdaUpdateTransaction::Update(
		m_pCallback,
		OPCHDA_REPLACE,
		dwTransactionID,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cValues.GetData(),
		cQualities.GetData(),
		cTimestamps.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// InsertReplace
HRESULT COpcHdaServer::InsertReplace(
	DWORD      dwTransactionID,
	DWORD      dwNumItems,
	OPCHANDLE* phServer,
	FILETIME*  ftTimeStamps,
	VARIANT*   vDataValues,
	DWORD*     pdwQualities,
	DWORD*     pdwCancelID,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || ftTimeStamps == NULL || vDataValues == NULL || pdwQualities == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);
	COpcArray<VARIANT>   cValues(uCount);
	COpcArray<DWORD>     cQualities(uCount);
	COpcArray<LONGLONG>  cTimestamps(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cQualities[uIndex]     = pdwQualities[ii];
			cTimestamps[uIndex]    = OpcHdaInt64FromFILETIME(ftTimeStamps[ii]);
			
			VariantInit(&(cValues[uIndex]));
			VariantCopy(&(cValues[uIndex]), &(vDataValues[ii]));
			
			uIndex++;
		}
	}
		
	// create transaction (passed ownership of contents of VARIANTs).
	COpcHdaTransaction* pTransaction = COpcHdaUpdateTransaction::Update(
		m_pCallback,
		OPCHDA_INSERTREPLACE,
		dwTransactionID,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cValues.GetData(),
		cQualities.GetData(),
		cTimestamps.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// DeleteRaw
HRESULT COpcHdaServer::DeleteRaw(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	OPCHDA_TIME* htEndTime,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer,
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || htStartTime == NULL || htEndTime == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaUpdateTransaction::Delete(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// DeleteAtTime
HRESULT COpcHdaServer::DeleteAtTime(
	DWORD      dwTransactionID,
	DWORD      dwNumItems,
	OPCHANDLE* phServer,
	FILETIME*  ftTimeStamps,
	DWORD*     pdwCancelID,
	HRESULT**  ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);
	COpcArray<LONGLONG>  cTimestamps(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cTimestamps[uIndex]    = OpcHdaInt64FromFILETIME(ftTimeStamps[ii]);
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaUpdateTransaction::DeleteAtTime(
		m_pCallback,
		dwTransactionID,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cTimestamps.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

//=========================================================================
// IOPCHDA_AsyncAnnotations

HRESULT COpcHdaServer::Read(
	DWORD        dwTransactionID,
	OPCHDA_TIME* htStartTime,
	OPCHDA_TIME* htEndTime,
	DWORD        dwNumItems,
	OPCHANDLE*   phServer, 
	DWORD*       pdwCancelID,
	HRESULT**    ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || htStartTime == NULL || htEndTime == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime   = OpcHdaResolveTime(*htEndTime);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaAnnotationTransaction::ReadAnnotations(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

// Insert
HRESULT COpcHdaServer::Insert(
	DWORD              dwTransactionID,
	DWORD              dwNumItems, 
	OPCHANDLE*         phServer, 
	FILETIME*          ftTimeStamps,
	OPCHDA_ANNOTATION* pAnnotationValues,
	DWORD*             pdwCancelID,
	HRESULT**          ppErrors
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (dwNumItems == 0 || phServer == NULL || ftTimeStamps == NULL || pAnnotationValues == NULL || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE>         cServerHandles(uCount);
	COpcArray<OPCHANDLE>         cClientHandles(uCount);
	COpcArray<LONGLONG>          cTimestamps(uCount);
	COpcArray<OPCHDA_ANNOTATION> cAnnotations(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cTimestamps[uIndex]    = OpcHdaInt64FromFILETIME(ftTimeStamps[ii]);

			OPCHDA_ANNOTATION& cAnnotation = cAnnotations[uIndex];

			cAnnotation.dwNumValues = pAnnotationValues[ii].dwNumValues;

			cAnnotation.ftTimeStamps     = OpcArrayAlloc(FILETIME, cAnnotation.dwNumValues);
			cAnnotation.szAnnotation     = OpcArrayAlloc(LPWSTR, cAnnotation.dwNumValues);
			cAnnotation.ftAnnotationTime = OpcArrayAlloc(FILETIME, cAnnotation.dwNumValues);
			cAnnotation.szUser           = OpcArrayAlloc(LPWSTR, cAnnotation.dwNumValues);

			for (DWORD jj = 0; jj < cAnnotation.dwNumValues; jj++)
			{
				cAnnotation.ftTimeStamps[jj]     = pAnnotationValues[ii].ftTimeStamps[jj];
				cAnnotation.szAnnotation[jj]     = OpcStrDup(pAnnotationValues[ii].szAnnotation[jj]);
				cAnnotation.ftAnnotationTime[jj] = pAnnotationValues[ii].ftAnnotationTime[jj];
				cAnnotation.szUser[jj]           = OpcStrDup(pAnnotationValues[ii].szUser[jj]);
			}

			uIndex++;
		}
	}
		
	// create transaction (memory ownership of ANNOTATION structures is passed to the transaction).
	COpcHdaTransaction* pTransaction = COpcHdaAnnotationTransaction::InsertAnnotations(
		m_pCallback,
		dwTransactionID,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cTimestamps.GetData(),
		cAnnotations.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;	
}

//=========================================================================
// IOPCHDA_Playback

// ReadRawWithUpdate
HRESULT COpcHdaServer::ReadRawWithUpdate(
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
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime      = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime        = OpcHdaResolveTime(*htEndTime);
	LONGLONG llUpdateInterval = OpcHdaInt64FromFILETIME(ftUpdateInterval);
	LONGLONG llUpdateDuration = OpcHdaInt64FromFILETIME(ftUpdateDuration);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// reverse time series not allowed for playback.
	if (llStartTime == 0  || llEndTime != 0 && llEndTime < llStartTime)
	{
		return E_INVALIDARG;
	}

	// update duration must non zero.
	if (llUpdateDuration == 0)
	{
		return E_INVALIDARG;
	}

	// adjust update rate.
	bool bUnsupportedRate = false;

	if (llUpdateInterval%OPCHDA_MAX_UPDATE_INTERVAL != 0)
	{
		llUpdateInterval += OPCHDA_MAX_UPDATE_INTERVAL - llUpdateInterval%OPCHDA_MAX_UPDATE_INTERVAL;
		bUnsupportedRate  = true;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaReadTransaction::PlaybackRaw(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		dwNumValues,
		llUpdateDuration,
		llUpdateInterval,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// unsupported update rate.
	if (bUnsupportedRate)
	{
		return OPC_S_UNSUPPORTEDRATE;
	}

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;
}

// ReadProcessedWithUpdate
HRESULT COpcHdaServer::ReadProcessedWithUpdate(
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
)
{
	COpcLock cLock(*this);

	// check arguments.
	if (htStartTime == NULL || htEndTime == NULL || dwNumItems == 0 || pdwCancelID == NULL)
	{
		return E_INVALIDARG;
	}

	// initialize return parameters.
	*pdwCancelID = NULL;
	*ppErrors    = NULL;

	LONGLONG llStartTime        = OpcHdaResolveTime(*htStartTime);
	LONGLONG llEndTime          = OpcHdaResolveTime(*htEndTime);
	LONGLONG llResampleInterval = OpcHdaInt64FromFILETIME(ftResampleInterval);
	LONGLONG llUpdateInterval   = OpcHdaInt64FromFILETIME(ftUpdateInterval);

	// validate time domain parameters.
	HRESULT hResult = ValidateTimeDomain(llStartTime, llEndTime, llResampleInterval);

	if (FAILED(hResult))
	{
		return hResult;
	}

	// reverse time series not allowed for playback.
	if (llStartTime == 0  || llEndTime != 0 && llEndTime < llStartTime)
	{
		return E_INVALIDARG;
	}

	// playback duration and resample interval must non zero.
	if (dwNumIntervals == 0 || llResampleInterval == 0)
	{
		return E_INVALIDARG;
	}

	// adjust update rate.
	bool bUnsupportedRate = false;

	if (llUpdateInterval%OPCHDA_MAX_UPDATE_INTERVAL != 0)
	{
		llUpdateInterval += OPCHDA_MAX_UPDATE_INTERVAL - llUpdateInterval%OPCHDA_MAX_UPDATE_INTERVAL;
		bUnsupportedRate  = true;
	}

	// validate server handles.
	COpcArray<OpcHdaItem*> cItems(dwNumItems);

	UINT uCount = ValidateServerHandles(dwNumItems, phServer, ppErrors, cItems);

	// no valid handles found.
	if (uCount == 0)
	{
		return S_FALSE;
	}

	// keep only the valid handles.
	COpcArray<OPCHANDLE> cServerHandles(uCount);
	COpcArray<OPCHANDLE> cClientHandles(uCount);
	COpcArray<DWORD>     cAggregates(uCount);

	UINT uIndex = 0;

	for (UINT ii = 0; uIndex < uCount && ii < dwNumItems; ii++)
	{
		if (cItems[ii] != NULL)
		{
			cServerHandles[uIndex] = cItems[ii]->hServer;
			cClientHandles[uIndex] = cItems[ii]->hClient;
			cAggregates[uIndex]    = haAggregate[ii];
			uIndex++;
		}
	}
		
	// create transaction.
	COpcHdaTransaction* pTransaction = COpcHdaReadTransaction::PlaybackProcessed(
		m_pCallback,
		dwTransactionID,
		llStartTime,
		llEndTime,
		llResampleInterval,
		dwNumIntervals,
		llUpdateInterval,
		uCount,
		cServerHandles.GetData(),
		cClientHandles.GetData(),
		cAggregates.GetData(),
		pdwCancelID
	);

	// queue transaction.
	GetHistorian().QueueTransaction(pTransaction);

	// unsupported update rate.
	if (bUnsupportedRate)
	{
		return OPC_S_UNSUPPORTEDRATE;
	}

	// all done.
	return (uCount == dwNumItems)?S_OK:S_FALSE;
}

// ValidateTimeDomain
HRESULT COpcHdaServer::ValidateTimeDomain(
	LONGLONG llStartTime,
	LONGLONG llEndTime,
	DWORD    dwNumValues
)
{
	// check that at least 2 of start time, end time and num values have been provided.
	if (llStartTime == 0 && llEndTime == 0)
	{
		return E_INVALIDARG;
	}
	else if (dwNumValues == 0 && (llStartTime == 0 || llEndTime == 0)) 
	{
		return E_INVALIDARG;
	}

	// check if max number of values exceeded.
	if (OPCHDA_MAX_VALUES > 0 && dwNumValues > OPCHDA_MAX_VALUES)
	{
		return OPC_E_MAXEXCEEDED;
	}

	// time domain valid.
	return S_OK;
}

// ValidateTimeDomain
HRESULT COpcHdaServer::ValidateTimeDomain(
	LONGLONG llStartTime,
	LONGLONG llEndTime,
	LONGLONG llResampleInterval
)
{
	// check that the start time, end time and resample interval are valid.
	if (llStartTime == 0 || llEndTime == 0 || llStartTime == llEndTime || llResampleInterval < 0)
	{
		return E_INVALIDARG;
	}

	// calculate the number of values.
	UINT uNumValues = 1;

	if (llResampleInterval > 0)
	{
		LONGLONG llUpperBound = (llEndTime > llStartTime)?llEndTime:llStartTime;
		LONGLONG llLowerBound = (llEndTime < llStartTime)?llEndTime:llStartTime;

		LONGLONG llRange = (llUpperBound - llLowerBound);

		uNumValues = (UINT)(((double)llRange)/llResampleInterval);

		// add a partial interval at the if range is not evenly divisible by the interval.
		if (llRange%llResampleInterval != 0)
		{
			uNumValues++;
		}
	}

	// check if max number of values exceeded.
	if (uNumValues > OPCHDA_MAX_VALUES)
	{
		return OPC_E_MAXEXCEEDED;
	}

	// all done.
	return S_OK;
}

// ValidateServerHandles
UINT COpcHdaServer::ValidateServerHandles(
	DWORD                   dwNumItems,
	OPCHANDLE*              phServer,
	HRESULT**               ppErrors,
	COpcArray<OpcHdaItem*>& cItems
)
{
	// check arguments.
	if (dwNumItems == 0 || phServer == NULL || ppErrors == NULL)
	{
		return 0;
	}

	// allocate return parameters.
	*ppErrors = OpcArrayAlloc(HRESULT, dwNumItems);
	memset(*ppErrors, 0, sizeof(HRESULT)*dwNumItems);

	UINT uValidCount = 0;

	// lookup server handles for each item.
	for (DWORD ii = 0; ii < dwNumItems; ii++)
	{
		cItems[ii] = NULL;
		(*ppErrors)[ii] = OPC_E_INVALIDHANDLE;

		OpcHdaItem* pItem = NULL;

		if (m_cItems.Lookup(phServer[ii], pItem))
		{
			cItems[ii] = pItem;
			(*ppErrors)[ii] = S_OK;
			
			uValidCount++;
		}
	}
	
	// all done.
	return uValidCount;
}


//==============================================================================
// IOpcMessageCallback

// ProcessMessage
void COpcHdaServer::ProcessMessage(COpcMessage& cMsg)
{
	COpcLock cLock(*this);

	// check if server has been released by the client.
	if (m_pCallback == NULL)
	{
		GetHistorian().TransactionComplete(cMsg.GetID(), false);
		return;
	}
		    
    // lookup callback - may have been released since transaction was posted.
    IOPCHDA_DataCallback* ipCallback = NULL;

    HRESULT hResult = GetCallback(IID_IOPCHDA_DataCallback, (IUnknown**)&ipCallback);
    
    if (FAILED(hResult))
    {
		GetHistorian().TransactionComplete(cMsg.GetID(), false);
        return;
    }

	// count the number of items with more data to return.
	UINT uNoOfMoreDataItems = 0;

	for (UINT ii = 0; ii < ((COpcHdaTransaction&)cMsg).GetNoOfItems(); ii++)
	{
		if (((COpcHdaTransaction&)cMsg).cErrors[ii] == OPC_S_MOREDATA)
		{
			uNoOfMoreDataItems++;
		}
	}

	// remove tranaction from historian.
	bool bResult = GetHistorian().TransactionComplete(cMsg.GetID(), (uNoOfMoreDataItems > 0));
	
	cLock.Unlock();

	// send cancel complete update.
	if (!bResult)
	{
		ipCallback->OnCancelComplete(((COpcHdaTransaction&)cMsg).dwClientID);
		ipCallback->Release();
		return;
    }

	// whether transaction has to be added back into the queue. 
    switch (cMsg.GetType())
    {
		// read raw values.
		case OPC_TRANSACTION_READ_RAW:
		case OPC_TRANSACTION_READ_PROCESSED:
		case OPC_TRANSACTION_READ_AT_TIME:
		{			
			COpcHdaReadTransaction& cTransaction = (COpcHdaReadTransaction&)cMsg;

			ipCallback->OnReadComplete(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.GetNoOfItems(),
				cTransaction.cItems.GetData(),
				cTransaction.cErrors.GetData()
			);

			// duplicate and queue transaction to fetch the remaining values.
			if (uNoOfMoreDataItems > 0)
			{	
				CopyTransaction(cTransaction, uNoOfMoreDataItems);
			}

			break;
		}

		// update raw values.
		case OPC_TRANSACTION_ADVISE_RAW:
		case OPC_TRANSACTION_ADVISE_PROCESSED:
		{			
			COpcHdaReadTransaction& cTransaction = (COpcHdaReadTransaction&)cMsg;

			ipCallback->OnDataChange(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.GetNoOfItems(),
				cTransaction.cItems.GetData(),
				cTransaction.cErrors.GetData()
			);

			break;
		}

		// playback raw values.
		case OPC_TRANSACTION_PLAYBACK_RAW:
		case OPC_TRANSACTION_PLAYBACK_PROCESSED:
		{			
			COpcHdaReadTransaction& cTransaction = (COpcHdaReadTransaction&)cMsg;

			// must convert for array items to a array of pointers to an item because 
			// the IDL incorrectly specifies OPCHDA_ITEM** as the argument data type. 
			UINT uCount = cTransaction.GetNoOfItems();

			OPCHDA_ITEM** ppItems = OpcArrayAlloc(OPCHDA_ITEM*, uCount);
			memset(ppItems, 0, uCount*sizeof(OPCHDA_ITEM*));

			for (UINT ii = 0; ii < uCount; ii++)
			{
				ppItems[ii] = &(cTransaction.cItems[ii]);
			}

			ipCallback->OnPlayback(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.GetNoOfItems(),
				ppItems,
				cTransaction.cErrors.GetData()
			);

			OpcFree(ppItems);

			break;
		}

		// read modified values.
		case OPC_TRANSACTION_READ_MODIFIED:
		{			
			COpcHdaModifiedTransaction& cTransaction = (COpcHdaModifiedTransaction&)cMsg;

			ipCallback->OnReadModifiedComplete(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.GetNoOfItems(),
				cTransaction.cItems.GetData(),
				cTransaction.cErrors.GetData()
			);

			// duplicate and queue transaction to fetch the remaining values.
			if (uNoOfMoreDataItems > 0)
			{	
				CopyTransaction(cTransaction, uNoOfMoreDataItems);
			}

			break;
		}

		// read attribute values.
		case OPC_TRANSACTION_READ_ATTRIBUTE:
		{			
			COpcHdaAttributeTransaction& cTransaction = (COpcHdaAttributeTransaction&)cMsg;

			ipCallback->OnReadAttributeComplete(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.cClientHandles[0],
				cTransaction.cAttributes.GetSize(),
				cTransaction.cAttributes.GetData(),
				cTransaction.cErrors.GetData()
			);

			break;
		}

		// read annotation values.
		case OPC_TRANSACTION_READ_ANNOTATION:
		{			
			COpcHdaAnnotationTransaction& cTransaction = (COpcHdaAnnotationTransaction&)cMsg;

			ipCallback->OnReadAnnotations(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.cAnnotations.GetSize(),
				cTransaction.cAnnotations.GetData(),
				cTransaction.cErrors.GetData()
			);

			break;
		}

		// insert annotation values.
		case OPC_TRANSACTION_INSERT_ANNOTATION:
		{			
			COpcHdaAnnotationTransaction& cTransaction = (COpcHdaAnnotationTransaction&)cMsg;

			ipCallback->OnInsertAnnotations(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.cClientHandles.GetSize(),
				cTransaction.cClientHandles.GetData(),
				cTransaction.cErrors.GetData()
			);

			break;
		}

		// update values.
		case OPC_TRANSACTION_INSERT:
		case OPC_TRANSACTION_REPLACE:
		case OPC_TRANSACTION_INSERT_REPLACE:
		case OPC_TRANSACTION_DELETE:
		case OPC_TRANSACTION_DELETE_AT_TIME:
		{			
			COpcHdaUpdateTransaction& cTransaction = (COpcHdaUpdateTransaction&)cMsg;

			ipCallback->OnUpdateComplete(
				cTransaction.dwClientID,
				cTransaction.hResult,
				cTransaction.cClientHandles.GetSize(),
				cTransaction.cClientHandles.GetData(),
				cTransaction.cErrors.GetData()
			);

			break;
		}

		default:
		{
			break;
		}
    }

    // release reference to callback.
    ipCallback->Release();
	return;
}

// CopyTransaction
void COpcHdaServer::CopyTransaction(COpcHdaReadTransaction& cTransaction, UINT uNoOfItems)
{
	// create a new transaction to return the rest of the data.
	COpcHdaReadTransaction* pCopy = new COpcHdaReadTransaction(cTransaction);

	// copy information only from items which have more data left.
	pCopy->SetNoOfItems(uNoOfItems);

	UINT uIndex = 0;

	for (UINT ii = 0; ii < cTransaction.GetNoOfItems(); ii++)
	{
		if (cTransaction.cErrors[ii] == OPC_S_MOREDATA)
		{
			// should never happen.
			if (uIndex >= uNoOfItems)
			{
				OPC_ASSERT(false);
				break;
			}

			pCopy->cServerHandles[uIndex] = cTransaction.cServerHandles[ii];
			pCopy->cClientHandles[uIndex] = cTransaction.cClientHandles[ii];
			pCopy->cAggregates[uIndex]    = cTransaction.cAggregates[ii];

			uIndex++;
		}
	}

	// update the total values sent.
	pCopy->uValuesSent += cTransaction.uNumValues;

	// put the transaction back in the queue to fetch additional data.
	if (!GetHistorian().QueueTransaction(pCopy, true))
	{
		delete pCopy;
	}
}

// CopyTransaction
void COpcHdaServer::CopyTransaction(COpcHdaModifiedTransaction& cTransaction, UINT uNoOfItems)
{
	// create a new transaction to return the rest of the data.
	COpcHdaModifiedTransaction* pCopy = new COpcHdaModifiedTransaction(cTransaction);

	// copy information only from items which have more data left.
	pCopy->SetNoOfItems(uNoOfItems);

	UINT uIndex = 0;

	for (UINT ii = 0; ii < cTransaction.GetNoOfItems(); ii++)
	{
		if (cTransaction.cErrors[ii] == OPC_S_MOREDATA)
		{
			// should never happen.
			if (uIndex >= uNoOfItems)
			{
				OPC_ASSERT(false);
				break;
			}

			pCopy->cServerHandles[uIndex] = cTransaction.cServerHandles[ii];
			pCopy->cClientHandles[uIndex] = cTransaction.cClientHandles[ii];

			uIndex++;
		}
	}

	// update the total values sent.
	pCopy->uValuesSent += cTransaction.uNumValues;

	// put the transaction back in the queue to fetch additional data.
	if (!GetHistorian().QueueTransaction(pCopy, true))
	{
		delete pCopy;
	}
}
