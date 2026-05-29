//============================================================================
// TITLE: COpcDaServerWrapper.cpp
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

#include "StdAfx.h"

#include "OpcDaWrapper.h"
#include "OpcDaWrapper_i.c"

#include "COpcDaServerWrapper.h"
#include "COpcDaGroupWrapper.h"
#include "COpcEnumStringWrapper.h"

//============================================================================
// Local Definitions

#define TAG_WRAPPED_SERVER_ID _T("WrappedServerID")

//==========================================================================
// Local Functions

// FixupOutputVariant
extern void FixupOutputVariant(VARIANT& vValue);

// FixupOutputVariant
extern void FixupOutputVariants(DWORD dwCount, VARIANT* pItemValues);

// FixupOutputVariants
HRESULT FixupOutputVariants(DWORD dwCount, OPCITEMPROPERTIES* pItemProperties)
{
	HRESULT hResult = S_OK;

	if (pItemProperties != NULL)
	{
		for (DWORD ii = 0; ii < dwCount; ii++)
		{
			for (DWORD jj = 0; jj < pItemProperties[ii].dwNumProperties; jj++)
			{
				FixupOutputVariant(pItemProperties[ii].pItemProperties[jj].vValue);
			}            
			
			// check if individual item has an issue 
			if (pItemProperties[ii].hrErrorID != S_OK) hResult = S_FALSE; 
		}
	}

	return hResult;
}

// FixupOutputVariants
HRESULT FixupOutputVariants(DWORD dwCount, OPCBROWSEELEMENT* ppBrowseElements)
{
	HRESULT hResult = S_OK;

	if (ppBrowseElements != NULL)
	{
		for (DWORD ii = 0; ii < dwCount; ii++)
		{
			if (FixupOutputVariants(1, &(ppBrowseElements[ii].ItemProperties)) != S_OK)
			{
				hResult = S_FALSE; 
			}
		}
	}

	return hResult;
}

//==============================================================================
// Static Data

static COpcCriticalSection g_cLock;
static UINT                g_uRefs = 0;

//==============================================================================
// Static Functions

// Initialize
static bool Initialize(REFCLSID cClsid, IUnknown** ippUnknown)
{
    COpcLock cLock(g_cLock);

	g_uRefs++;
		
	// initialize server security info.
	COSERVERINFO cInfo; 
	memset(&cInfo, 0, sizeof(cInfo));

    cInfo.pwszName    = NULL;
    cInfo.pAuthInfo   = NULL;
    cInfo.dwReserved1 = NULL;
    cInfo.dwReserved2 = NULL;

	// setup requested interfaces.
    MULTI_QI cResults;
	memset(&cResults, 0, sizeof(cResults));

    cResults.pIID = &IID_IUnknown;
    cResults.pItf = NULL;
    cResults.hr   = S_OK;

    // call create instance.
    HRESULT hResult = CoCreateInstanceEx(
        cClsid,
        NULL,
        CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
        &cInfo,
        1,
        &cResults);

    if (FAILED(hResult))
    {
		g_uRefs--;
		return false;
    }

    // check that interface is supported.
    if (FAILED(cResults.hr))
    {
		g_uRefs--;
		return false;
    }

	// return reference (AddRef() was done by CoCreateInstanceEx()).
	*ippUnknown = cResults.pItf;

	return true;
}

// Uninitialize
static void Uninitialize()
{
    COpcLock cLock(g_cLock);

    g_uRefs--;
    
    if (g_uRefs > 0)
    {
		return;
    }

    cLock.Unlock();

    COpcComModule::ExitProcess(S_OK);
}

//============================================================================
// COpcDaServerWrapper

// Constructor
COpcDaServerWrapper::COpcDaServerWrapper()
{
    m_ipUnknown     = NULL;
	m_ipSelfRegInfo = NULL;
	m_dwConnection  = NULL;
}

// Destructor
COpcDaServerWrapper::~COpcDaServerWrapper()
{
}

// FinalConstruct
HRESULT COpcDaServerWrapper::FinalConstruct()
{
	COpcLock cLock(*this);

	HRESULT hResult = S_OK;

	CLSID cClsid = GetCLSID();

	// lookup wrapped server clsid in registry.
	COpcString cSubKey;

	cSubKey += "CLSID\\";
	cSubKey += cClsid;
	cSubKey += "\\WrappedServer";

	LPWSTR tsWrappedClsid = NULL;
	
	bool bResult = OpcRegGetValue(HKEY_CLASSES_ROOT, cSubKey, NULL, &tsWrappedClsid);

	if (bResult)
	{
		// remove the enclosing curly brackets.
		tsWrappedClsid[_tcslen(tsWrappedClsid)-1] = '\0';

		RPC_STATUS status = UuidFromString((RPC_WSTR)tsWrappedClsid+1, &cClsid);

		if (status != RPC_S_OK)
		{
			bResult = false;
		} 

		OpcFree(tsWrappedClsid);
	}

	// use default wrapped server from config file if registry server not found.
	if (!bResult)
	{
		// construct configuration file name.
		COpcString cFileName;
		
		cFileName += OpcGetModulePath();
		cFileName += _T("\\");
		cFileName += OpcGetModuleName();
		cFileName += _T(".config.xml");

		// load the configuration file.
		COpcXmlDocument cConfigFile;

		if (!cConfigFile.Load(cFileName))
		{
			return OPC_E_INVALIDCONFIGFILE;
		}

		COpcXmlElement cRoot = cConfigFile.GetRoot();

		// release existing server self registration information.
		if (m_ipSelfRegInfo != NULL)
		{
			m_ipSelfRegInfo->Release();
			m_ipSelfRegInfo = NULL;
		}

		// extract server self registration information.
		hResult = OpcExtractSelfRegInfo((IXMLDOMElement*)cRoot, &m_ipSelfRegInfo);

		if (FAILED(hResult))
		{
			return hResult;
		}

		// extract the wrapper server prog id/clsid from the registration info.
		COpcXmlElement cSelfRegInfo(m_ipSelfRegInfo);

		COpcXmlElement cElement = cSelfRegInfo.GetChild(TAG_WRAPPED_SERVER_ID);

		if (cElement == NULL)
		{
			return OPC_E_INVALIDCONFIGFILE;
		}

		COpcString cProgID = cElement.GetValue();

		if (cProgID.IsEmpty())
		{
			return OPC_E_INVALIDCONFIGFILE;
		}

		// convert prog id to CLSID.
		if (FAILED(CLSIDFromProgID((LPCWSTR)cProgID, &cClsid)))
		{
			// try to convert a clsid string instead.
			if (UuidFromString((RPC_WSTR)(LPCWSTR)cProgID, &cClsid) != RPC_S_OK)
			{
				return E_FAIL;
			}
		}
	}

	// get server object.
	if (!Initialize(cClsid, &m_ipUnknown))
	{
		return E_FAIL;
	}

	// Tell the wrapped object about the CLSID/ProgID used to activate it.
	IOPCWrappedServer* ipObject = NULL;

	hResult = m_ipUnknown->QueryInterface(IID_IOPCWrappedServer, (void**)&ipObject);

	if (SUCCEEDED(hResult))
	{
		ipObject->Load(GetCLSID());
		ipObject->Release();
	}

	// register shutdown interface.
	RegisterInterface(IID_IOPCShutdown);

    return S_OK;
}

// FinalRelease
bool COpcDaServerWrapper::FinalRelease()
{
	COpcLock cLock(*this);
	
	// Tell the wrapped object that it is being shutdown.
	IOPCWrappedServer* ipObject = NULL;

	HRESULT hResult = m_ipUnknown->QueryInterface(IID_IOPCWrappedServer, (void**)&ipObject);

	if (SUCCEEDED(hResult))
	{
		ipObject->Unload();
		ipObject->Release();
	}

	UnregisterInterface(IID_IOPCShutdown);

	// release server.
	if (m_ipUnknown != NULL)
	{
		m_ipUnknown->Release();
		m_ipUnknown = NULL;
	}
	
	// release self reg info.
	if (m_ipSelfRegInfo != NULL)
	{
		m_ipSelfRegInfo->Release();
		m_ipSelfRegInfo = NULL;
	}

	// release groups.
	OPC_POS pos = m_cGroups.GetStartPosition();

	while (pos != NULL)
	{
		OPCHANDLE           hServer = NULL;
		COpcDaGroupWrapper* pGroup  = NULL;

		m_cGroups.GetNextAssoc(pos, hServer, pGroup);

		pGroup->Delete();
		pGroup->Release();
	}

	m_cGroups.RemoveAll();

	cLock.Unlock();

	// decrement global reference count.
	Uninitialize();

	return true;
}

// OnAdvise
void COpcDaServerWrapper::OnAdvise(REFIID riid, DWORD dwCookie)
{
    COpcLock cLock(*this);

	if (riid == IID_IOPCShutdown)
	{
		if (FAILED(OpcConnect(m_ipUnknown, (IOPCShutdown*)this, riid, &m_dwConnection)))
		{
			m_dwConnection = NULL;
		}
	}
}

// OnUnadvise
void COpcDaServerWrapper::OnUnadvise(REFIID riid, DWORD dwCookie)
{
	if (riid == IID_IOPCShutdown)
	{
		OpcDisconnect(m_ipUnknown, riid, m_dwConnection);
		m_dwConnection = NULL;
	}
}

//==============================================================================
// IOPCCommon

// SetLocaleID
HRESULT COpcDaServerWrapper::SetLocaleID(LCID dwLcid)
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCCommon* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCCommon, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetLocaleID(
		dwLcid
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// GetLocaleID
HRESULT COpcDaServerWrapper::GetLocaleID(LCID *pdwLcid)
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCCommon* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCCommon, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetLocaleID(
		pdwLcid
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// QueryAvailableLocaleIDs
HRESULT COpcDaServerWrapper::QueryAvailableLocaleIDs(DWORD* pdwCount, LCID** pdwLcid)
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCCommon* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCCommon, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->QueryAvailableLocaleIDs(
		pdwCount,
		pdwLcid
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// GetErrorString
HRESULT COpcDaServerWrapper::GetErrorString(HRESULT dwError, LPWSTR* ppString)
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCCommon* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCCommon, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->GetErrorString(
		dwError,
		ppString
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// SetClientName
HRESULT COpcDaServerWrapper::SetClientName(LPCWSTR szName)
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCCommon* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCCommon, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->SetClientName(
		szName
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

//============================================================================
// IOPCServer

// AddGroup
HRESULT COpcDaServerWrapper::AddGroup(
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
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCServer* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCServer, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->AddGroup(
		szName,
		bActive,
		dwRequestedUpdateRate,
		hClientGroup,
		pTimeBias,
		pPercentDeadband,
		dwLCID,
		phServerGroup,
		pRevisedUpdateRate,
		riid,
		ppUnk
	);

	// release interface.
	ipInterface->Release();
	ipInterface = NULL;

	// wrap group object.
	if (SUCCEEDED(hResult))
	{
		OPCHANDLE hGroup = NULL;

		hResult = WrapGroup(riid, ppUnk, &hGroup);

		if (FAILED(hResult))
		{
			// remove group.
			if (SUCCEEDED(m_ipUnknown->QueryInterface(IID_IOPCServer, (void**)&ipInterface)))
			{
				ipInterface->RemoveGroup(hGroup, FALSE);
				ipInterface->Release();
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

	// insert necessary success code.
	if (hResult == S_OK)
	{
		return (*pRevisedUpdateRate == dwRequestedUpdateRate)?S_OK:OPC_S_UNSUPPORTEDRATE;
	}

	return hResult;
}

// WrapGroup
HRESULT COpcDaServerWrapper::WrapGroup(REFIID riid, IUnknown** ippUnknown, OPCHANDLE* phGroup)
{
	// query for group state interface.
	IOPCGroupStateMgt* ipGroup = NULL;

	HRESULT hResult = (*ippUnknown)->QueryInterface(IID_IOPCGroupStateMgt, (void**)&ipGroup);
	
	if (FAILED(hResult))
	{
		return hResult;
	}

	// lookup group server handle.
	DWORD     dwUpdateRate = 0;
	BOOL      bActive      = NULL;
	LPWSTR    szName       = NULL;
	LONG      lTimeBias    = 0;
	FLOAT     fltDeadband  = 0;
	LCID      dwLocaleID   = 0;
	OPCHANDLE hClient      = NULL;
	OPCHANDLE hServer      = NULL;

	hResult = ipGroup->GetState(
		&dwUpdateRate,
		&bActive,
		&szName,
		&lTimeBias,
		&fltDeadband,
		&dwLocaleID,
		&hClient,
		&hServer
	);

	if (FAILED(hResult))
	{
		return hResult;
	}

	*phGroup = hServer;

	// wrap group.
	COpcDaGroupWrapper* pGroup = new COpcDaGroupWrapper(this, *ippUnknown);

	// release reference.
	(*ippUnknown)->Release();
	*ippUnknown = NULL;

	// query for desired interface,
	hResult = pGroup->QueryInterface(riid, (void**)ippUnknown);
		
	if (FAILED(hResult))
	{
		pGroup->Release();
		return hResult;
	}

	// save reference to group locally.
	COpcLock cLock(*this);
	m_cGroups[hServer] = pGroup;
	cLock.Unlock();

	return S_OK;
}

// GetErrorString
HRESULT COpcDaServerWrapper::GetErrorString( 
    HRESULT dwError,
    LCID    dwLocale,
    LPWSTR* ppString
)
{  
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCServer* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCServer, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->GetErrorString(
		dwError,
		dwLocale,
		ppString
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// GetGroupByName
HRESULT COpcDaServerWrapper::GetGroupByName(
    LPCWSTR    szName,
    REFIID     riid,
    LPUNKNOWN* ppUnk
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}
	
	// fetch required interface.
	IOPCServer* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCServer, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	IUnknown* ipUnknown = NULL;

	HRESULT hResult = ipInterface->GetGroupByName(
		szName,
		IID_IOPCGroupStateMgt,
		(IUnknown**)&ipUnknown
	);

	// release interface.
	ipInterface->Release();

	if (SUCCEEDED(hResult))
	{		
		IOPCGroupStateMgt* ipGroup = NULL;

		hResult = ipUnknown->QueryInterface(IID_IOPCGroupStateMgt, (void**)&ipGroup);

		if (SUCCEEDED(hResult))
		{
			// lookup group server handle.
			DWORD     dwUpdateRate = 0;
			BOOL      bActive      = NULL;
			LPWSTR    szName       = NULL;
			LONG      lTimeBias    = 0;
			FLOAT     fltDeadband  = 0;
			LCID      dwLocaleID   = 0;
			OPCHANDLE hClient      = NULL;
			OPCHANDLE hServer      = NULL;
	 
			hResult = ipGroup->GetState(
				&dwUpdateRate,
				&bActive,
				&szName,
				&lTimeBias,
				&fltDeadband,
				&dwLocaleID,
				&hClient,
				&hServer
			);

			if (SUCCEEDED(hResult))
			{
				// lookup wrapper.
				COpcDaGroupWrapper* pWrapper = NULL;

				if (m_cGroups.Lookup(hServer, pWrapper))
				{				
					// query for desired interface.
					hResult = pWrapper->QueryInterface(riid, (void**)ppUnk);
				}
			}
			
			ipGroup->Release();
		}

		ipUnknown->Release();
	}

	return hResult;
}

// GetStatus
HRESULT COpcDaServerWrapper::GetStatus( 
    OPCSERVERSTATUS** ppServerStatus
)
{   
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCServer* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCServer, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->GetStatus(ppServerStatus);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// RemoveGroup
HRESULT COpcDaServerWrapper::RemoveGroup(
    OPCHANDLE hServerGroup,
    BOOL      bForce
)
{    
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCServer* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCServer, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// remove group from inner server.
	HRESULT hResult = ipInterface->RemoveGroup(
		hServerGroup,
		bForce
	);

	// release interface.
	ipInterface->Release();

	ULONG ulRefs = 0;

	// remove wrapper.
	COpcDaGroupWrapper* pWrapper = NULL;

	if (m_cGroups.Lookup(hServerGroup, pWrapper))
	{
		pWrapper->Delete();
		m_cGroups.RemoveKey(hServerGroup);
		ulRefs = pWrapper->Release();
	}

	// check if group still in use.
	if (hResult == S_OK)
	{
		return (ulRefs == 0 || bForce)?S_OK:OPC_S_INUSE;
	}

	return hResult;
}

// CreateGroupEnumerator
HRESULT COpcDaServerWrapper::CreateGroupEnumerator(
    OPCENUMSCOPE dwScope, 
    REFIID       riid, 
    LPUNKNOWN*   ppUnk
)
{    
    COpcLock cLock(*this);

	HRESULT hResult = S_OK;

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	if (riid != IID_IEnumUnknown)
	{
		// fetch required interface.
		IOPCServer* ipInterface = NULL;

		if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCServer, (void**)&ipInterface)))
		{
			return E_NOTIMPL;
		}

		IUnknown* ipEnum = NULL;

		hResult = ipInterface->CreateGroupEnumerator(
			dwScope, 
			riid, 
			&ipEnum
		);

		// release interface.
		ipInterface->Release();

		if (SUCCEEDED(hResult))
		{
			// create enumerator wrapper.
			IUnknown* ipWrapper = new COpcEnumStringWrapper(ipEnum);
		           
			// release local reference.
			ipEnum->Release();

			// query for desired interface.
			hResult = ipWrapper->QueryInterface(riid, (void**)ppUnk);

			// release local reference.
			ipWrapper->Release();
		}
	}

	else
	{
		// check for public groups.
		switch (dwScope)
		{
			case OPC_ENUM_PUBLIC_CONNECTIONS:
			case OPC_ENUM_PUBLIC:
			{
				IUnknown* ipEnum = new COpcEnumUnknown();

				// return requested interface.
				HRESULT hResult = ipEnum->QueryInterface(riid, (void**)ppUnk);

				ipEnum->Release();

				if (FAILED(hResult))
				{
					return hResult;
				}

				return S_FALSE;
			}
		}

		// create enumerator that contains the group wrapper objects.
		COpcEnumUnknown* ipEnum = NULL;

		if (m_cGroups.GetCount() > 0)
		{
			COpcSortedArray<OPCHANDLE,IUnknown*> cList;

			OPC_POS pos = m_cGroups.GetStartPosition();

			while (pos != NULL)
			{
				OPCHANDLE           hServer = NULL;
				COpcDaGroupWrapper* pGroup  = NULL;

				m_cGroups.GetNextAssoc(pos, hServer, pGroup);

				cList.Insert(hServer, (IOPCGroupStateMgt*)pGroup);
			}

			UINT uCount = cList.GetCount();
			
			IUnknown** ippUnknown = OpcArrayAlloc(IUnknown*, uCount);

			for (UINT ii = 0; ii < uCount; ii++)
			{
				ippUnknown[ii] = cList[ii];
				ippUnknown[ii]->AddRef();
			}

			ipEnum = new COpcEnumUnknown(uCount, ippUnknown);
		}
		else
		{
			ipEnum = new COpcEnumUnknown();
		}
		
		// request desired interface.
		hResult = ipEnum->QueryInterface(IID_IEnumUnknown, (void**)ppUnk);

		// release local reference.
		ipEnum->Release();
	}

	if (hResult == S_OK)
	{
		return (m_cGroups.GetCount() > 0)?S_OK:S_FALSE;
	}

	return hResult;
}

//============================================================================
// IOPCItemIO

// Read
HRESULT COpcDaServerWrapper::Read(
    DWORD       dwCount, 
    LPCWSTR   * pszItemIDs,
    DWORD     * pdwMaxAge,
    VARIANT  ** ppvValues,
    WORD     ** ppwQualities,
    FILETIME ** ppftTimeStamps,
    HRESULT  ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemIO* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemIO, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->Read(
		dwCount, 
		pszItemIDs,
		pdwMaxAge,
		ppvValues,
		ppwQualities,
		ppftTimeStamps,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	// fix any conversion issues in variants.
	if (SUCCEEDED(hResult))
	{	
		FixupOutputVariants(dwCount, *ppvValues);
	}

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// WriteVQT
HRESULT COpcDaServerWrapper::WriteVQT(
    DWORD         dwCount, 
    LPCWSTR    *  pszItemIDs,
    OPCITEMVQT *  pItemVQT,
    HRESULT    ** ppErrors
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemIO* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemIO, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->WriteVQT(
		dwCount, 
		pszItemIDs,
		pItemVQT,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}


//=============================================================================
// IOPCBrowseServerAddressSpace

// QueryOrganization
HRESULT COpcDaServerWrapper::QueryOrganization(OPCNAMESPACETYPE* pNameSpaceType)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCBrowseServerAddressSpace* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCBrowseServerAddressSpace, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->QueryOrganization(pNameSpaceType);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// ChangeBrowsePosition
HRESULT COpcDaServerWrapper::ChangeBrowsePosition(
    OPCBROWSEDIRECTION dwBrowseDirection,  
    LPCWSTR            szString
)
{    
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCBrowseServerAddressSpace* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCBrowseServerAddressSpace, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->ChangeBrowsePosition(
		dwBrowseDirection,  
		szString
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// BrowseOPCItemIDs
HRESULT COpcDaServerWrapper::BrowseOPCItemIDs(
    OPCBROWSETYPE   dwBrowseFilterType,
    LPCWSTR         szFilterCriteria,  
    VARTYPE         vtDataTypeFilter,     
    DWORD           dwAccessRightsFilter,
    LPENUMSTRING*   ppIEnumString
)
{   
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCBrowseServerAddressSpace* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCBrowseServerAddressSpace, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	IEnumString* ipEnum = NULL;

	HRESULT hResult = ipInterface->BrowseOPCItemIDs(
		dwBrowseFilterType,
		szFilterCriteria,  
		vtDataTypeFilter,     
		dwAccessRightsFilter,
		&ipEnum
	);

	// release interface.
	ipInterface->Release();

	bool bEmpty = false;

	if (SUCCEEDED(hResult))
	{
		// check if enumerator has any entries.
		ULONG  ulFetched = 0;
		LPWSTR szName    = NULL;

		hResult = ipEnum->Next(1, &szName, &ulFetched);

		if (SUCCEEDED(hResult))
		{
			ipEnum->Reset();

			if (ulFetched == 0)
			{
				bEmpty = true;
			}
			else
			{
				OpcFree(szName);
			}
		}

		// create enumerator wrapper.
		IUnknown* ipWrapper = new COpcEnumStringWrapper(ipEnum);
		        
		// release local reference.
		ipEnum->Release();

		// query for desired interface.
		hResult = ipWrapper->QueryInterface(IID_IEnumString, (void**)ppIEnumString);

		// release local reference.
		ipWrapper->Release();
	}

	if (hResult == S_OK)
	{
		return (bEmpty)?S_FALSE:S_OK;
	}

	return hResult;
}

// GetItemID
HRESULT COpcDaServerWrapper::GetItemID(
    LPWSTR  wszItemName,
    LPWSTR* pszItemID
)
{   
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCBrowseServerAddressSpace* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCBrowseServerAddressSpace, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->GetItemID(
		wszItemName,
		pszItemID
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// BrowseAccessPaths
HRESULT COpcDaServerWrapper::BrowseAccessPaths(
    LPCWSTR       szItemID,  
    LPENUMSTRING* ppIEnumString
)
{
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCBrowseServerAddressSpace* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCBrowseServerAddressSpace, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	IEnumString* ipEnum = NULL;

	HRESULT hResult = ipInterface->BrowseAccessPaths(
		szItemID,  
		&ipEnum
	);

	// release interface.
	ipInterface->Release();

	if (SUCCEEDED(hResult))
	{
		// create enumerator wrapper.
		IUnknown* ipWrapper = new COpcEnumStringWrapper(ipEnum);
		        
		// release local reference.
		ipEnum->Release();

		// query for desired interface.
		hResult = ipWrapper->QueryInterface(IID_IEnumString, (void**)ppIEnumString);

		// release local reference.
		ipWrapper->Release();
	}

	return hResult;
}

//============================================================================
// IOPCItemProperties

// QueryAvailableProperties
HRESULT COpcDaServerWrapper::QueryAvailableProperties( 
    LPWSTR     szItemID,
    DWORD    * pdwCount,
    DWORD   ** ppPropertyIDs,
    LPWSTR  ** ppDescriptions,
    VARTYPE ** ppvtDataTypes
)
{   
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemProperties* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemProperties, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->QueryAvailableProperties(
		szItemID,
		pdwCount,
		ppPropertyIDs,
		ppDescriptions,
		ppvtDataTypes
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// GetItemProperties
HRESULT COpcDaServerWrapper::GetItemProperties( 
    LPWSTR     szItemID,
    DWORD      dwCount,
    DWORD    * pdwPropertyIDs,
    VARIANT ** ppvData,
    HRESULT ** ppErrors
)
{    
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemProperties* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemProperties, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->GetItemProperties(
		szItemID,
		dwCount,
		pdwPropertyIDs,
		ppvData,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	// fix any conversion issues in variants.
	if (SUCCEEDED(hResult))
	{
		FixupOutputVariants(dwCount, *ppvData);
	}

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

// LookupItemIDs
HRESULT COpcDaServerWrapper::LookupItemIDs( 
    LPWSTR     szItemID,
    DWORD      dwCount,
    DWORD    * pdwPropertyIDs,
    LPWSTR  ** ppszNewItemIDs,
    HRESULT ** ppErrors
)
{    
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCItemProperties* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCItemProperties, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->LookupItemIDs(
		szItemID,
		dwCount,
		pdwPropertyIDs,
		ppszNewItemIDs,
		ppErrors
	);

	// release interface.
	ipInterface->Release();

	RETURN_SFALSE(hResult, dwCount, ppErrors);
}

//=============================================================================
// IOPCBrowse

// GetProperties
HRESULT COpcDaServerWrapper::GetProperties( 
    DWORD		        dwItemCount,
    LPWSTR*             pszItemIDs,
    BOOL		        bReturnPropertyValues,
    DWORD		        dwPropertyCount,
    DWORD*              pdwPropertyIDs,
    OPCITEMPROPERTIES** ppItemProperties 
)
{    
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCBrowse* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCBrowse, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->GetProperties(
		dwItemCount,
		pszItemIDs,
		bReturnPropertyValues,
		dwPropertyCount,
		pdwPropertyIDs,
		ppItemProperties 
	);

	// release interface.
	ipInterface->Release();

	// fix any conversion issues in variants.
	if (SUCCEEDED(hResult))
	{
		hResult = FixupOutputVariants(dwItemCount, *ppItemProperties);
	}

	return hResult;
}

// Browse
HRESULT COpcDaServerWrapper::Browse(
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
)
{    
    COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IOPCBrowse* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IOPCBrowse, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	HRESULT hResult = ipInterface->Browse(
		szItemName,
		pszContinuationPoint,
		dwMaxElementsReturned,
		dwFilter,
		szElementNameFilter,
		szVendorFilter,
		bReturnAllProperties,
		bReturnPropertyValues,
		dwPropertyCount,
		pdwPropertyIDs,
		pbMoreElements,
		pdwCount,
		ppBrowseElements
	);

	// release interface.
	ipInterface->Release();

	// fix any conversion issues in variants.
	if (SUCCEEDED(hResult))
	{
		hResult = FixupOutputVariants(*pdwCount, *ppBrowseElements);
	}

	return hResult;
}

//==============================================================================
// IOPCShutdown

// ShutdownRequest
HRESULT COpcDaServerWrapper::ShutdownRequest(
    LPCWSTR szReason
)
{
	// get callback object.
	IOPCShutdown* ipCallback = NULL;

	HRESULT hResult = GetCallback(IID_IOPCShutdown, (IUnknown**)&ipCallback);

	if (FAILED(hResult) || ipCallback == NULL)
	{
		return S_OK;
	}

	// invoke callback.
	ipCallback->ShutdownRequest(szReason);

	// release callback.
	ipCallback->Release();

	return S_OK;
}