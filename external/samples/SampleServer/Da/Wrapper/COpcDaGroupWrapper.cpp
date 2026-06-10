//============================================================================
// TITLE: COpcDaGroup.cpp
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

#include "StdAfx.h"
#include "COpcDaGroupWrapper.h"
#include "COpcDaEnumItemWrapper.h"
#include "COpcDaServerWrapper.h"

//==========================================================================
// Local Functions

// FixupDecimalArray
void FixupDecimalArray(VARIANT& vValue)
{
	VARIANT vDst;
	VariantInit(&vDst);

	COpcSafeArray cSrc(vValue);

	UINT uLength = cSrc.GetLength();

	COpcSafeArray cDst(vDst);
	cDst.Alloc(VT_CY, cSrc.GetLength());

	for (UINT jj = 0; jj < uLength; jj++)
	{
		DECIMAL decVal;
		
		if (SUCCEEDED(SafeArrayGetElement(vValue.parray, (LONG*)&jj, (void*)&decVal)))
		{		
			CY cyVal;

			if (FAILED(VarCyFromDec(&decVal, &cyVal)))
			{
				cyVal.int64 = 0;
			}

			SafeArrayPutElement(vDst.parray, (LONG*)&jj, (void*)&cyVal);
		}
	}

	VariantClear(&vValue);
	vValue = vDst;
}

// FixupOutputVariant
void FixupOutputVariant(VARIANT& vValue)
{
	switch (vValue.vt)
	{
		case VT_ARRAY | VT_DECIMAL:
		{
			FixupDecimalArray(vValue);
			break;
		}

		case VT_DECIMAL:
		{
			VARIANT cyVal; 
			VariantInit(&cyVal);

			if (SUCCEEDED(VariantChangeType(&cyVal, &vValue, NULL, VT_CY)))
			{
				vValue.vt    = VT_CY;
				vValue.cyVal = cyVal.cyVal;
			}

			break;
		}
	}
}

// FixupOutputVariants
void FixupOutputVariants(DWORD dwCount, OPCITEMSTATE* pItemValues)
{
	if (pItemValues != NULL)
	{
		for (DWORD ii = 0; ii < dwCount; ii++)
		{
			FixupOutputVariant(pItemValues[ii].vDataValue);
		}
	}
}

// FixupOutputVariants
void FixupOutputVariants(DWORD dwCount, VARIANT* pItemValues)
{
	if (pItemValues != NULL)
	{
		for (DWORD ii = 0; ii < dwCount; ii++)
		{
			FixupOutputVariant(pItemValues[ii]);
		}
	}
}

// FixupInputVariants
void FixupInputVariants(DWORD dwCount, VARIANT* pValues)
{
	for (DWORD ii = 0; ii < dwCount; ii++)
	{
		if (pValues[ii].vt == VT_DATE)
		{
			if (pValues[ii].dblVal > 2e6)
			{
				pValues[ii].vt = VT_R8;
			}
		}
	}
}

//==========================================================================
// COpcDaGroupWrapper

// Constructor
COpcDaGroupWrapper::COpcDaGroupWrapper()
{
	RegisterInterface(IID_IOPCDataCallback);

	m_pServer      = NULL;
	m_ipUnknown    = NULL;
	m_dwConnection = NULL;
}

// Constructor
COpcDaGroupWrapper::COpcDaGroupWrapper(COpcDaServerWrapper* pServer, IUnknown* ipUnknown)
{
	RegisterInterface(IID_IOPCDataCallback);

	m_pServer      = pServer;
	m_ipUnknown    = ipUnknown;
	m_dwConnection = NULL;

	if (ipUnknown != NULL)
	{
		ipUnknown->AddRef();
	}
}

// Destructor 
COpcDaGroupWrapper::~COpcDaGroupWrapper()
{
	if (m_ipUnknown != NULL)
	{
		m_ipUnknown->Release();
		m_ipUnknown = NULL;
	}
}

// Delete
void COpcDaGroupWrapper::Delete()
{
    COpcLock cLock(*this);
	m_pServer = NULL;
}

// OnAdvise
void COpcDaGroupWrapper::OnAdvise(REFIID riid, DWORD dwCookie)
{
    COpcLock cLock(*this);

	if (riid == IID_IOPCDataCallback)
	{
		if (FAILED(OpcConnect(m_ipUnknown, (IOPCDataCallback*)this, riid, &m_dwConnection)))
		{
			m_dwConnection = NULL;
		}
	}
}

// OnUnadvise
void COpcDaGroupWrapper::OnUnadvise(REFIID riid, DWORD dwCookie)
{
	if (riid == IID_IOPCDataCallback)
	{
		OpcDisconnect(m_ipUnknown, riid, m_dwConnection);
		m_dwConnection = NULL;
	}
}

//=========================================================================
// IOPCGroupStateMgt

// GetState
HRESULT COpcDaGroupWrapper::GetState(
    DWORD     * pUpdateRate, 
    BOOL      * pActive, 
    LPWSTR    * ppName,
    LONG      * pTimeBias,
    FLOAT     * pPercentDeadband,
    DWORD     * pLCID,
    OPCHANDLE * phClientGroup,
    OPCHANDLE * phServerGroup
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCGroupStateMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCGroupStateMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetState(
		pUpdateRate, 
		pActive, 
		ppName,
		pTimeBias,
		pPercentDeadband,
		pLCID,
		phClientGroup,
		phServerGroup
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// SetState
HRESULT COpcDaGroupWrapper::SetState( 
    DWORD     * pRequestedUpdateRate, 
    DWORD     * pRevisedUpdateRate, 
    BOOL      * pActive, 
    LONG      * pTimeBias,
    FLOAT     * pPercentDeadband,
    DWORD     * pLCID,
    OPCHANDLE * phClientGroup
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCGroupStateMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCGroupStateMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetState(
		pRequestedUpdateRate, 
		pRevisedUpdateRate, 
		pActive, 
		pTimeBias,
		pPercentDeadband,
		pLCID,
		phClientGroup
	);

	// release interface.
	ipInterface->Release();

	// insert necessary success code.
	if (pRequestedUpdateRate != NULL && hResult == S_OK)
	{
		return (*pRevisedUpdateRate == *pRequestedUpdateRate)?S_OK:OPC_S_UNSUPPORTEDRATE;
	}

	return hResult;
}

// SetName
HRESULT COpcDaGroupWrapper::SetName( 
    LPCWSTR szName
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCGroupStateMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCGroupStateMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetName(szName);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// CloneGroup
HRESULT COpcDaGroupWrapper::CloneGroup(
    LPCWSTR     szName,
    REFIID      riid,
    LPUNKNOWN * ppUnk
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCGroupStateMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCGroupStateMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->CloneGroup(
		szName,
		riid,
		ppUnk
	);

	// release interface.
	ipInterface->Release();

	COpcDaServerWrapper* pServer = m_pServer;

	cLock.Unlock();

	// wrap group object.
	if (SUCCEEDED(hResult))
	{
		OPCHANDLE hGroup = NULL;

		hResult = pServer->WrapGroup(riid, ppUnk, &hGroup);

		if (FAILED(hResult))
		{
			// remove group.
			IOPCServer* ipServer = NULL;

			if (SUCCEEDED(pServer->QueryInterface(IID_IOPCServer, (void**)&ipServer)))
			{
				ipServer->RemoveGroup(hGroup, FALSE);
				ipServer->Release();
			}

			// release unknown.
			if ((*ppUnk) != NULL)
			{
				(*ppUnk)->Release();
				*ppUnk = NULL;
			}

			// return failure.
			return hResult;
		}
	}

	return hResult;
}

//=========================================================================
// IOPCGroupStateMgt2

// SetKeepAlive
HRESULT COpcDaGroupWrapper::SetKeepAlive( 
    DWORD   dwKeepAliveTime,
    DWORD * pdwRevisedKeepAliveTime 
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCGroupStateMgt2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCGroupStateMgt2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetKeepAlive(
		dwKeepAliveTime,
		pdwRevisedKeepAliveTime 
	);

	// release interface.
	ipInterface->Release();

	// insert necessary success code.
	if (hResult == S_OK)
	{
		return (*pdwRevisedKeepAliveTime == dwKeepAliveTime)?S_OK:OPC_S_UNSUPPORTEDRATE;
	}

	return hResult;
}

// GetKeepAlive
HRESULT COpcDaGroupWrapper::GetKeepAlive( 
    DWORD * pdwKeepAliveTime 
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCGroupStateMgt2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCGroupStateMgt2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetKeepAlive(
		pdwKeepAliveTime
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

//=========================================================================
// IOPCItemMgt

// AddItems
HRESULT COpcDaGroupWrapper::AddItems( 
    DWORD            dwCount,
    OPCITEMDEF     * pItemArray,
    OPCITEMRESULT ** ppAddResults,
    HRESULT       ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->AddItems(
		dwCount,
		pItemArray,
		ppAddResults,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// ValidateItems
HRESULT COpcDaGroupWrapper::ValidateItems( 
    DWORD             dwCount,
    OPCITEMDEF      * pItemArray,
    BOOL              bBlobUpdate,
    OPCITEMRESULT  ** ppValidationResults,
    HRESULT        ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->ValidateItems(
        dwCount,
        pItemArray,
        bBlobUpdate,
		ppValidationResults,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// RemoveItems
HRESULT COpcDaGroupWrapper::RemoveItems( 
    DWORD        dwCount,
    OPCHANDLE  * phServer,
    HRESULT   ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->RemoveItems(
		dwCount,
		phServer,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// SetActiveState
HRESULT COpcDaGroupWrapper::SetActiveState(
    DWORD        dwCount,
    OPCHANDLE  * phServer,
    BOOL         bActive, 
    HRESULT   ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetActiveState(
		dwCount,
		phServer,
		bActive, 
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// SetClientHandles
HRESULT COpcDaGroupWrapper::SetClientHandles(
    DWORD        dwCount,
    OPCHANDLE  * phServer,
    OPCHANDLE  * phClient,
    HRESULT   ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetClientHandles(
		dwCount,
		phServer,
		phClient,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// SetDatatypes
HRESULT COpcDaGroupWrapper::SetDatatypes(
    DWORD        dwCount,
    OPCHANDLE  * phServer,
    VARTYPE    * pRequestedDatatypes,
    HRESULT   ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetDatatypes(
		dwCount,
		phServer,
		pRequestedDatatypes,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// CreateEnumerator
HRESULT COpcDaGroupWrapper::CreateEnumerator(
    REFIID      riid,
    LPUNKNOWN * ppUnk
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->CreateEnumerator(
		riid,
		ppUnk
	);

	// release interface.
	ipInterface->Release();

	if (SUCCEEDED(hResult))
	{
		// create wrapper.
		COpcDaEnumItemWrapper* pEnum = new COpcDaEnumItemWrapper(*ppUnk);

		// release local reference.
		(*ppUnk)->Release();
		*ppUnk = NULL;

		// query for interface.
		hResult = pEnum->QueryInterface(riid, (void**)ppUnk);

		// release local reference.
		pEnum->Release();
	}

	return hResult;
}

//=========================================================================
// IOPCAsyncIO2

// Read
HRESULT COpcDaGroupWrapper::Read(
    DWORD           dwCount,
    OPCHANDLE     * phServer,
    DWORD           dwTransactionID,
    DWORD         * pdwCancelID,
    HRESULT      ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->Read(
		dwCount,
		phServer,
		dwTransactionID,
		pdwCancelID,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// Write
HRESULT COpcDaGroupWrapper::Write(
    DWORD           dwCount, 
    OPCHANDLE     * phServer,
    VARIANT       * pItemValues, 
    DWORD           dwTransactionID,
    DWORD         * pdwCancelID,
    HRESULT      ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// fixup conversion problems with input variants.
	FixupInputVariants(dwCount, pItemValues);

	// invoke method.
	HRESULT hResult = ipInterface->Write(
		dwCount, 
		phServer,
		pItemValues, 
		dwTransactionID,
		pdwCancelID,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// Refresh2
HRESULT COpcDaGroupWrapper::Refresh2(
    OPCDATASOURCE   dwSource,
    DWORD           dwTransactionID,
    DWORD         * pdwCancelID
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->Refresh2(
		dwSource,
		dwTransactionID,
		pdwCancelID
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// Cancel2
HRESULT COpcDaGroupWrapper::Cancel2(DWORD dwCancelID)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->Cancel2(
		dwCancelID
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// SetEnable
HRESULT COpcDaGroupWrapper::SetEnable(BOOL bEnable)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetEnable(
		bEnable
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// GetEnable
HRESULT COpcDaGroupWrapper::GetEnable(BOOL* pbEnable)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetEnable(
		pbEnable
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

//=========================================================================
// IOPCItemDeadbandMgt

// SetItemDeadband
HRESULT COpcDaGroupWrapper::SetItemDeadband( 
    DWORD 	    dwCount,
    OPCHANDLE * phServer,
    FLOAT     * pPercentDeadband,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemDeadbandMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemDeadbandMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetItemDeadband(
		dwCount,
		phServer,
		pPercentDeadband,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// GetItemDeadband
HRESULT COpcDaGroupWrapper::GetItemDeadband( 
    DWORD 	    dwCount,
    OPCHANDLE * phServer,
    FLOAT    ** ppPercentDeadband,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemDeadbandMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemDeadbandMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetItemDeadband(
		dwCount,
		phServer,
		ppPercentDeadband,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// ClearItemDeadband
HRESULT COpcDaGroupWrapper::ClearItemDeadband(
    DWORD       dwCount,
    OPCHANDLE * phServer,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemDeadbandMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemDeadbandMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->ClearItemDeadband(
		dwCount,
		phServer,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

//=========================================================================
// IOPCItemSamplingMgt

// SetItemSamplingRate
HRESULT COpcDaGroupWrapper::SetItemSamplingRate(
    DWORD 	    dwCount,
    OPCHANDLE * phServer,
    DWORD     * pdwRequestedSamplingRate,
    DWORD    ** ppdwRevisedSamplingRate,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemSamplingMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemSamplingMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetItemSamplingRate(
		dwCount,
		phServer,
		pdwRequestedSamplingRate,
		ppdwRevisedSamplingRate,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// GetItemSamplingRate
HRESULT COpcDaGroupWrapper::GetItemSamplingRate(
    DWORD 	    dwCount,
    OPCHANDLE * phServer,
    DWORD    ** ppdwSamplingRate,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemSamplingMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemSamplingMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetItemSamplingRate(
		dwCount,
		phServer,
		ppdwSamplingRate,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// ClearItemSamplingRate
HRESULT COpcDaGroupWrapper::ClearItemSamplingRate(
    DWORD 	    dwCount,
    OPCHANDLE * phServer,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemSamplingMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemSamplingMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->ClearItemSamplingRate(
		dwCount,
		phServer,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// SetItemBufferEnable
HRESULT COpcDaGroupWrapper::SetItemBufferEnable(
    DWORD       dwCount, 
    OPCHANDLE * phServer, 
    BOOL      * pbEnable,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemSamplingMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemSamplingMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetItemBufferEnable(
		dwCount, 
		phServer, 
		pbEnable,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// GetItemBufferEnable
HRESULT COpcDaGroupWrapper::GetItemBufferEnable(
    DWORD       dwCount, 
    OPCHANDLE * phServer, 
    BOOL     ** ppbEnable,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemSamplingMgt* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemSamplingMgt, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetItemBufferEnable(
		dwCount, 
		phServer, 
		ppbEnable,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

//=========================================================================
// IOPCSyncIO2

// Read
HRESULT COpcDaGroupWrapper::Read(
    OPCDATASOURCE   dwSource,
    DWORD           dwCount, 
    OPCHANDLE     * phServer, 
    OPCITEMSTATE ** ppItemValues,
    HRESULT      ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCSyncIO* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCSyncIO, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->Read(
		dwSource,
		dwCount, 
		phServer, 
		ppItemValues,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	// fix any conversion issues in variants.
	FixupOutputVariants(dwCount, *ppItemValues);

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// Write
HRESULT COpcDaGroupWrapper::Write(
    DWORD        dwCount, 
    OPCHANDLE  * phServer, 
    VARIANT    * pItemValues, 
    HRESULT   ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCSyncIO* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCSyncIO, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}
	
	// fixup conversion problems with input variants.
	FixupInputVariants(dwCount, pItemValues);
	
	// invoke method.
	HRESULT hResult = ipInterface->Write(
		dwCount, 
		phServer, 
		pItemValues, 
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// ReadMaxAge
HRESULT COpcDaGroupWrapper::ReadMaxAge(
    DWORD       dwCount, 
    OPCHANDLE * phServer, 
    DWORD     * pdwMaxAge,
    VARIANT  ** ppvValues,
    WORD     ** ppwQualities,
    FILETIME ** ppftTimeStamps,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCSyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCSyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->ReadMaxAge(
		dwCount, 
		phServer, 
		pdwMaxAge,
		ppvValues,
		ppwQualities,
		ppftTimeStamps,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	// fix any conversion issues in variants.
	FixupOutputVariants(dwCount, *ppvValues);

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// WriteVQT
HRESULT COpcDaGroupWrapper::WriteVQT(
    DWORD         dwCount, 
    OPCHANDLE  *  phServer, 
    OPCITEMVQT *  pItemVQT,
    HRESULT    ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCSyncIO2* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCSyncIO2, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->WriteVQT(
		dwCount, 
		phServer, 
		pItemVQT,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

//=========================================================================
// IOPCAsyncIO3

HRESULT COpcDaGroupWrapper::ReadMaxAge(
    DWORD       dwCount, 
    OPCHANDLE * phServer,
    DWORD     * pdwMaxAge,
    DWORD       dwTransactionID,
    DWORD     * pdwCancelID,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO3* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO3, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->ReadMaxAge(
		dwCount, 
		phServer,
		pdwMaxAge,
		dwTransactionID,
		pdwCancelID,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

HRESULT COpcDaGroupWrapper::WriteVQT(
    DWORD        dwCount, 
    OPCHANDLE  * phServer,
    OPCITEMVQT * pItemVQT,
    DWORD        dwTransactionID,
    DWORD      * pdwCancelID,
    HRESULT   ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO3* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO3, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->WriteVQT(
		dwCount, 
		phServer,
		pItemVQT,
		dwTransactionID,
		pdwCancelID,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

HRESULT COpcDaGroupWrapper::RefreshMaxAge(
    DWORD   dwMaxAge,
    DWORD   dwTransactionID,
    DWORD * pdwCancelID
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL || m_pServer == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCAsyncIO3* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCAsyncIO3, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->RefreshMaxAge(
		dwMaxAge,
		dwTransactionID,
		pdwCancelID
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

//==============================================================================
// IOPCDataCallback

// OnDataChange
HRESULT COpcDaGroupWrapper::OnDataChange(
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
)
{
	// get callback object.
	IOPCDataCallback* ipCallback = NULL;

	HRESULT hResult = GetCallback(IID_IOPCDataCallback, (IUnknown**)&ipCallback);

	if (FAILED(hResult) || ipCallback == NULL)
	{
		return S_OK;
	}

	// fix any conversion issues in variants.
	FixupOutputVariants(dwCount, pvValues);

	// invoke callback.
	ipCallback->OnDataChange(
		dwTransid, 
		hGroup, 
		hrMasterquality,
		hrMastererror,
		dwCount, 
		phClientItems, 
		pvValues, 
		pwQualities,
		pftTimeStamps,
		pErrors);

	// release callback.
	ipCallback->Release();

	return S_OK;
}

// OnReadComplete
HRESULT COpcDaGroupWrapper::OnReadComplete(
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
)
{
	// get callback object.
	IOPCDataCallback* ipCallback = NULL;

	HRESULT hResult = GetCallback(IID_IOPCDataCallback, (IUnknown**)&ipCallback);

	if (FAILED(hResult) || ipCallback == NULL)
	{
		return S_OK;
	}

	// fix any conversion issues in variants.
	FixupOutputVariants(dwCount, pvValues);

	// invoke callback.
	ipCallback->OnReadComplete(
		dwTransid, 
		hGroup, 
		hrMasterquality,
		hrMastererror,
		dwCount, 
		phClientItems, 
		pvValues, 
		pwQualities,
		pftTimeStamps,
		pErrors);

	// release callback.
	ipCallback->Release();

	return S_OK;
}

// OnWriteComplete
HRESULT COpcDaGroupWrapper::OnWriteComplete(
    DWORD      dwTransid, 
    OPCHANDLE  hGroup, 
    HRESULT    hrMastererr, 
    DWORD      dwCount, 
    OPCHANDLE* pClienthandles, 
    HRESULT*   pErrors
)
{
	// get callback object.
	IOPCDataCallback* ipCallback = NULL;

	HRESULT hResult = GetCallback(IID_IOPCDataCallback, (IUnknown**)&ipCallback);

	if (FAILED(hResult) || ipCallback == NULL)
	{
		return S_OK;
	}

	// invoke callback.
	ipCallback->OnWriteComplete(
		dwTransid, 
		hGroup, 
		hrMastererr,
		dwCount, 
		pClienthandles, 
		pErrors);

	// release callback.
	ipCallback->Release();

	return S_OK;
}

// OnCancelComplete
HRESULT COpcDaGroupWrapper::OnCancelComplete(
    DWORD     dwTransid, 
    OPCHANDLE hGroup
)
{
	// get callback object.
	IOPCDataCallback* ipCallback = NULL;

	HRESULT hResult = GetCallback(IID_IOPCDataCallback, (IUnknown**)&ipCallback);

	if (FAILED(hResult) || ipCallback == NULL)
	{
		return S_OK;
	}

	// invoke callback.
	ipCallback->OnCancelComplete(
		dwTransid, 
		hGroup);

	// release callback.
	ipCallback->Release();

	return S_OK;
}