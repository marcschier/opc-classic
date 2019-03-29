// i_sio.cpp
//
// (c) Copyright 1997 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Data Access Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//  This file contains the implementation of 
//  the IOPCSyncIO interface for groups in the XXX server.
//
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  0.00  11/18/96 ACC
//        12/27/96 acc   add more sample code
//  0.01  02/05/97 acc   See 'acc001'
//                       Fix memory leaks
//                       Fix handle validation in Read and Write (missing 'continue')
//                       Be sure to null all 'out' pointers in case of error
//  0.02  03/01/97 acc   Add support for Variant Conversions (acc002)
//                       (changes to read and write)
//  0.90  04/02/97 acc   Changes to GetValue()
//  1.0a  08/01/97 acc   use 'GetHandle()'
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCerror.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXSIO()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXSIO::IXXXSIO( LPUNKNOWN parent )
{
	m_Parent = (XXXGroup *)parent;
}



///////////////////////////////////////
// ~IXXXSIO()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXSIO::~IXXXSIO( void)
{
	m_Parent->m_pIXXXSIO = 0;
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXSIO::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXSIO::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXSIO::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXSIO (IOPCSyncIO) interface functions
//


///////////////////////////////////////
// Read
///////////////////////////////////////
STDMETHODIMP IXXXSIO::Read(
	OPCDATASOURCE   dwSource,
	DWORD           dwNumItems, 
	OPCHANDLE     * phServer, 
	OPCITEMSTATE ** ppItemValues,
	HRESULT      ** ppErrors
    )
{
	XXXGroup &g = *m_Parent;
	DWORD	i;
	HRESULT *hr;
	OPCITEMSTATE *is;

	// Defaults in case of error
	//
	*ppErrors = hr = NULL;			//acc001
	*ppItemValues = is = NULL;		//acc001

	// Allocate memory for the item values and hresults
	//
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) return E_OUTOFMEMORY;
	*ppItemValues = is = (OPCITEMSTATE*) pIMalloc->Alloc(sizeof(OPCITEMSTATE)*dwNumItems);		//acc001
	if(is == NULL) 
	{
		pIMalloc->Free(hr);			//acc001
		return E_OUTOFMEMORY;
	}

	// For each item
	//
	for (i=0; i< dwNumItems; i++)
	{
		// Validate the handle
		//
		int j = phServer[i];
		if (!g.ItemValid(j))
		{
			hr[i] = OPC_E_INVALIDHANDLE;
			continue;			//acc001
		}

		// Find the item that matches the server handle
		//
		XXXItem &item = *g.ItemPtr(j);

		// and extract the requested data...
		// GetValue attempts the conversion from Canonical to Requested
		// According the the OPC Spec, AddItems and/or SetDataTypes
		// will have already verified that this is possible.
		//
		is[i].hClient = item.GetHandle();
		VariantInit(&is[i].vDataValue);
		hr[i] = item.GetValue(dwSource, &is[i].vDataValue, &is[i].wQuality, &is[i].ftTimeStamp);	// acc002

		// For CACHE, If Group not active then quality is bad no matter what
		//
		if(( dwSource == OPC_DS_CACHE) && !g.m_bActive) 
			is[i].wQuality = QUAL_NOTACTIVE;

		// Note that doing a read does not affect OnDataChange
		// so we do NOT clear Changed flag
		//

	}
	return S_OK;
}

///////////////////////////////////////
// Write
///////////////////////////////////////
STDMETHODIMP IXXXSIO::Write(
	DWORD        dwNumItems, 
	OPCHANDLE  * phServer, 
	VARIANT    * pItemValues, 
	HRESULT   ** ppErrors
    )
{
	XXXGroup &g = *m_Parent;
	DWORD	i;
	HRESULT *hr;

	// Allocate memory for the hresults
	//
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) return E_OUTOFMEMORY;

	// For each item
	//
	for (i=0; i< dwNumItems; i++)
	{
		// Validate the handle
		//
		int j = phServer[i];
		if (!g.ItemValid(j))
		{
			hr[i] = OPC_E_INVALIDHANDLE;
			continue;			//acc001
		}

		// Find the item that matches the server handle
		//
		XXXItem &item = *g.ItemPtr(j);

		// and write the requested data...
		//
		HRESULT r1 = item.SetValue(&pItemValues[i]);		//acc002
		if(FAILED(r1))
			hr[i] = OPC_E_BADTYPE;
		else
			hr[i] = S_OK;

	}
	return S_OK;
}

