// I_BSVR.cpp
//
// (c) Copyright 1999 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Batch Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Will Irwin, Fisher-Rosemount Systems Inc.  The 'tear-off'
//  implementation of the interface is based on code found elsewhere
//  in the OPC sample server, originally provided by Al Chisholm, 
//  Intellution Inc.
//
// CONTENTS:
//
//  This file contains the sample implementation of 
//  the IOPCBatchServer interface for the XXX server.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
//  1.0  10/15/99 WGI   Initial implementation for OPC Batch sample server
//
//
//
// The addref of the 'parent' is not actually required by this sample code
// but is good practice.
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXBSV()
//   Constructor for this Interface
///////////////////////////////////////
/*
 * IXXXBSVR::IXXXBSVR
 * IXXXBSVR::~IXXXBSVR
 *
 * Parameters (Constructor):
 *  m_Parent		'parent' LPUNKNOWN to use for reference counting.
 *  pmem       IMalloc memory allocator to use for returned data
 */

IXXXBSVR::IXXXBSVR(
	LPUNKNOWN parent, 
	IMalloc * pMem)
{
	m_Parent = (XXXServer*)parent;
	m_pMem = pMem;
}


///////////////////////////////////////
// ~IXXXBSVR()
//   Destructor for this Interface
///////////////////////////////////////
IXXXBSVR::~IXXXBSVR(void)
{

}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
/////////////////////////////////////////////////////////////////////////////


STDMETHODIMP_(ULONG) IXXXBSVR::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXBSVR::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXBSVR::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}

STDMETHODIMP IXXXBSVR::GetDelimiter( LPWSTR * pszDelimiter )
{
  *pszDelimiter = (WCHAR*)m_pMem->Alloc(sizeof(WCHAR) *
                                          (wcslen(DELIMITER) + 1));
  if (*pszDelimiter != NULL)
  {
    wcscpy (*pszDelimiter, DELIMITER);
    return S_OK;
  }
  else
  {
    return E_OUTOFMEMORY;
  }
}

///////////////////////////////////////
// CreateEnumerator
//  This is heavily based on Al Chisholm's
//  IXXXIA::CreateEnumerator
///////////////////////////////////////
STDMETHODIMP IXXXBSVR::CreateEnumerator(
	REFIID      riid,
	LPUNKNOWN * ppUnk )
{
	// default in case of error
	//
	*ppUnk = NULL;		//acc001

	if ( riid == IID_IEnumOPCBatchSummary)
	{
		IXXXEnumBatchSummary *temp;
		HRESULT hr;

    // Create the Enumerator which will create its own copy
    // of the batch list data it needs.
		// Note that the enumerator is not affected by 
		// subsequent changes to the item list.
		// The group will get an 'addref' via m_Parent below and will thus
		// stay around until the enumerator is released 
		// (although this is probably unnecessary)
		//
		temp = new IXXXEnumBatchSummary(m_Parent, pIMalloc);

		// Then QI for the interface. ('temp' actually is the interface
		// but QI is the 'proper' way to get it.)
		// Note QI will do an AddRef of the Enum which will also do
		// an AddRef of the 'parent' - i.e. the 'this' pointer passed above.
		//
		hr = temp->QueryInterface( riid, (LPVOID*)ppUnk);
		if ( FAILED( hr))
		{
			delete temp;
			return hr;
		}

		return S_OK;
	}
	return E_INVALIDARG;
}
