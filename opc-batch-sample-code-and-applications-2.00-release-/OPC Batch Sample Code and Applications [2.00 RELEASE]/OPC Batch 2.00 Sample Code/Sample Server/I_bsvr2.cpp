// I_BSVR2.cpp
//
// (c) Copyright 2001 The OPC Foundation
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
//  the IOPCBatchServer2 interface for the XXX server.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
//  1.0  11/02/00 WGI   Initial implementation for OPC Batch 2.0 sample server
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
// IXXXBSVR2()
//   Constructor for this Interface
///////////////////////////////////////
/*
 * IXXXBSVR2::IXXXBSVR2
 * IXXXBSVR2::~IXXXBSVR2
 *
 * Parameters (Constructor):
 *  m_Parent		'parent' LPUNKNOWN to use for reference counting.
 *  pmem       IMalloc memory allocator to use for returned data
 */

IXXXBSVR2::IXXXBSVR2(
	LPUNKNOWN parent, 
	IMalloc * pMem)
{
	m_Parent = (XXXServer*)parent;
	m_pMem = pMem;
}


///////////////////////////////////////
// ~IXXXBSVR2()
//   Destructor for this Interface
///////////////////////////////////////
IXXXBSVR2::~IXXXBSVR2(void)
{

}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
/////////////////////////////////////////////////////////////////////////////


STDMETHODIMP_(ULONG) IXXXBSVR2::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXBSVR2::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXBSVR2::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}

///////////////////////////////////////
// CreateEnumerator
//  This is heavily based on Al Chisholm's
//  IXXXIA::CreateEnumerator
///////////////////////////////////////
STDMETHODIMP IXXXBSVR2::CreateFilteredEnumerator(
	REFIID      riid,
  OPCBATCHSUMMARYFILTER *pFilter,    // [in]
  LPCWSTR szModel,                    // [in, string] 

	LPUNKNOWN * ppUnk )
{
  int iModel = 0;  // use 1 for BatchModel, 2 for BatchArchiveModel 
  if (wcscmp(szModel, L"OPCBBatchModel") == 0)
  {
    iModel = 1;
  }
  else if (wcscmp(szModel, L"OPCBBatchArchiveModel") == 0)
  {
    iModel = 2;
  }

  if (iModel == 0)
    return E_INVALIDARG;

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
		temp = new IXXXEnumBatchSummary(m_Parent, pFilter, iModel, pIMalloc);

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
