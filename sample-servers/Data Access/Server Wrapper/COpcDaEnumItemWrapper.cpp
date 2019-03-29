//============================================================================
// TITLE: COpcDaEnumItemWrapper.cpp
//
// CONTENTS:
// 
// An wrapper implementation for the IEnumOPCItemAttributes interface.
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
// 2004/04/27 RSA   Initial implementation.

#include "StdAfx.h"
#include "COpcDaEnumItemWrapper.h"

//============================================================================
// COpcDaEnumItemWrapper

COpcDaEnumItemWrapper::COpcDaEnumItemWrapper()
{
	m_ipUnknown = NULL;
}

// Constructor
COpcDaEnumItemWrapper::COpcDaEnumItemWrapper(IUnknown* ipUnknown)
{
	m_ipUnknown = ipUnknown;
	m_ipUnknown->AddRef();
}

// Destructor 
COpcDaEnumItemWrapper::~COpcDaEnumItemWrapper()
{
	if (m_ipUnknown != NULL)
	{
		m_ipUnknown->Release();
		m_ipUnknown = NULL;
	}
}

//============================================================================
// IEnumOPCItemAttributes
   
// Next
HRESULT COpcDaEnumItemWrapper::Next(
	ULONG               celt,
	OPCITEMATTRIBUTES** ppItemArray,
	ULONG*              pceltFetched
)
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IEnumOPCItemAttributes* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IEnumOPCItemAttributes, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->Next(
		celt,
		ppItemArray,
		pceltFetched
	);

	// release interface.
	ipInterface->Release();

	if (hResult == S_OK)
	{
		if (celt > 0 && (*ppItemArray == NULL || *pceltFetched == 0))
		{
			return S_FALSE;
		}
	}

	return hResult;
}

// Skip
HRESULT COpcDaEnumItemWrapper::Skip(ULONG celt)
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IEnumOPCItemAttributes* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IEnumOPCItemAttributes, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->Skip(
		celt
	);

	// release interface.
	ipInterface->Release();

	return hResult;
}

// Reset
HRESULT COpcDaEnumItemWrapper::Reset()
{
	COpcLock cLock(*this);

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IEnumOPCItemAttributes* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IEnumOPCItemAttributes, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	HRESULT hResult = ipInterface->Reset();

	// release interface.
	ipInterface->Release();

	return hResult;
}

// Clone
HRESULT COpcDaEnumItemWrapper::Clone(IEnumOPCItemAttributes** ppEnum)
{
	COpcLock cLock(*this);

    // check for invalid arguments.
    if (ppEnum == NULL)
    {
        return E_INVALIDARG;
    }

	// check inner server.
	if (m_ipUnknown == NULL)
	{
		return E_FAIL;
	}

	// fetch required interface.
	IEnumOPCItemAttributes* ipInterface = NULL;

	if (FAILED(m_ipUnknown->QueryInterface(IID_IEnumOPCItemAttributes, (void**)&ipInterface)))
	{
		return E_NOTIMPL;
	}

	// invoke method.
	IEnumOPCItemAttributes* ipEnum = NULL;

	HRESULT hResult = ipInterface->Clone(&ipEnum);

	// release interface.
	ipInterface->Release();

	// create wrapper.
	COpcDaEnumItemWrapper* pEnum = new COpcDaEnumItemWrapper(ipEnum);

	// release local reference.
	ipEnum->Release();

	// query for interface.
    hResult = pEnum->QueryInterface(IID_IEnumOPCItemAttributes, (void**)ppEnum);

    // release local reference.
    pEnum->Release();

    return hResult;
}
