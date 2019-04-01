// i_asio2.cpp
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
//  the IOPCASyncIO2 interface for groups in the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
// 2.00  08/25/97 acc   add ConnectionPoint Support.
//       01/14/98 acc   add GetState, SetState
//       06/15/98 acc   reject read/write/refresh if no callback registered (per spec)
//       06/17/98 acc   rename some methods; cancel2,setenable,getenable
//       10/14/98 acc   rename Refresh to Refresh2
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCERROR.h"

extern CRITICAL_SECTION	CritSec;

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXASIO2()
//   Constructor for this Interface
///////////////////////////////////////
IXXXASIO2::IXXXASIO2( LPUNKNOWN parent )
{
	m_Parent = (XXXGroup *)parent;
}



///////////////////////////////////////
// ~IXXXASIO()
//   Destructor for this Interface
///////////////////////////////////////
IXXXASIO2::~IXXXASIO2( void)
{
	m_Parent->m_pIXXXASIO2 = 0;
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXASIO2::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXASIO2::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXASIO2::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXASIO (IOPCAsyncIO) interface functions
//


///////////////////////////////////////
// Read
// Queue up an Async read.
// 
///////////////////////////////////////
STDMETHODIMP IXXXASIO2::Read(
	DWORD           dwNumItems,
	OPCHANDLE     * phServer,
	DWORD           TransactionID,
	DWORD         * pCancelID,
	HRESULT      ** ppErrors
    )
{

	XXXGroup &g = *m_Parent;
	XXXServer &s = *g.m_ParentServer;
	unsigned int j;
	int reject = 0;
	HRESULT *pe = 0;

	// Sanity Check
	//
	if(dwNumItems == 0) return E_INVALIDARG;
	if(phServer == NULL) return E_INVALIDARG;
	if(pCancelID == NULL) return E_INVALIDARG;
	if(ppErrors == NULL) return E_INVALIDARG;

	// See if there is a callback registered
	//
	reject = 1;
	if(g.m_pCPC)
	{
		HRESULT r1;
		IXXXCP *pCP = NULL;
		r1 = g.m_pCPC->FindConnectionPoint(IID_IOPCDataCallback, (IConnectionPoint**)&pCP);
		if(SUCCEEDED(r1)) 
			if(pCP && pCP->m_Callback) reject = 0;
		if(pCP) pCP->Release();
	}
	if(reject) return CONNECT_E_NOCONNECTION;	// no callback available

	// This sample supports only 1 outstanding async read per group
	//
	if(g.m_Async2ReadActive) 
	{
		return CONNECT_E_ADVISELIMIT;
	}

	// Alloc memory for returned error list
	// and the stored handle list (to be returned later in the callback)
	//
	pe = *ppErrors = (HRESULT*)pIMalloc->Alloc(dwNumItems * sizeof( HRESULT ));
	if(pe == NULL) 
	{
		return E_OUTOFMEMORY;
	}

	// Store the list of items to be read
	// so that the background thread knows which ones to 'watch'
	// Note that many different implementations of this are possible.
	//
	g.m_Async2ReadList = (OPCHANDLE*)pIMalloc->Alloc(dwNumItems * sizeof( OPCHANDLE ));
	if(g.m_Async2ReadList == NULL) 
	{
		pIMalloc->Free(pe);
		return E_OUTOFMEMORY;
	}


	EnterCriticalSection(&CritSec);

	// Next, validate ALL of the handles
	// The spec implies we should proceed with any of the handles that are GOOD
	// even if some of them are bad. However this server is not quite smart enough to do that
	// (and clients should not be passing bad handles in any case)
	//
	reject = 0;
	for (j=0; j<dwNumItems; j++)
	{
		if (!g.ItemValid(phServer[j]))
		{
			pe[j] = OPC_E_INVALIDHANDLE;
			reject = 1;
			continue;
		}

		pe[j] = S_OK;
	}

	// If any handles are bad
	// then we are finished
	//
	if (reject)
	{
		pIMalloc->Free(g.m_Async2ReadList);
		LeaveCriticalSection(&CritSec);
		return OPC_E_INVALIDHANDLE;
	}


	// If all handles are good, then que up async reads of the items
	// (exact implementation of this will be server specific)
	//
	for (j=0; j<dwNumItems; j++)
	{
		int I;
		I = phServer[j];	// Get ServerItem Handle
		g.m_Async2ReadList[j] = I;


		// Queue a device read of this item
		//
		g.ItemPtr(I)->QueDeviceRead(DEVICE_READ);
	}

	// it all worked - the read is in progress
	//

	s.GenerateTransaction(pCancelID);		// Generate Cancel ID
	g.m_Async2ReadCanID = *pCancelID;
	g.m_Async2ReadTID = TransactionID;		// Record ID passed by Client
	g.m_Async2ReadCancel = 0;
	g.m_Async2ReadActive = dwNumItems;		// record number of items

	LeaveCriticalSection(&CritSec);
	return S_OK;
}



///////////////////////////////////////
// Write
// Queue up an Async Write.
//
///////////////////////////////////////
STDMETHODIMP IXXXASIO2::Write(
	DWORD       dwNumItems, 
	OPCHANDLE * phServer,
	VARIANT   * pItemValues, 
	DWORD       TransactionID,
	DWORD     * pCancelID,
	HRESULT ** ppErrors
    )
{

	XXXGroup &g = *m_Parent;
	XXXServer &s = *g.m_ParentServer;
	unsigned int j;
	int reject = 0;
	HRESULT *pe = 0;

	// Sanity Check
	//
	if(dwNumItems == 0) return E_INVALIDARG;
	if(phServer == NULL) return E_INVALIDARG;
	if(pCancelID == NULL) return E_INVALIDARG;
	if(pItemValues == NULL) return E_INVALIDARG;
	if(ppErrors == NULL) return E_INVALIDARG;

	// See if there is a callback registered
	//
	reject = 1;
	if(g.m_pCPC)
	{
		HRESULT r1;
		IXXXCP *pCP = NULL;
		r1 = g.m_pCPC->FindConnectionPoint(IID_IOPCDataCallback, (IConnectionPoint**)&pCP);
		if(SUCCEEDED(r1)) 
			if(pCP && pCP->m_Callback) reject = 0;
		if(pCP) pCP->Release();
	}
	if(reject) return CONNECT_E_NOCONNECTION;	// no callback available

	// This sample supports only 1 outstanding async write per group
	//
	if(g.m_Async2WriteActive) 
	{
		return CONNECT_E_ADVISELIMIT;
	}

	// Alloc memory for returned error list
	// and the stored handle list (to be returned later in the callback)
	//
	pe = *ppErrors = (HRESULT*)pIMalloc->Alloc(dwNumItems * sizeof( HRESULT ));
	if(pe == NULL) 
	{
		return E_OUTOFMEMORY;
	}

	// Store the list of items to be written
	// so that the background thread knows which ones to 'watch'
	// Note that many different implementations of this are possible.
	//
	g.m_Async2WriteList = (OPCHANDLE*)pIMalloc->Alloc(dwNumItems * sizeof( OPCHANDLE ));
	if(g.m_Async2WriteList == NULL) 
	{
		pIMalloc->Free(pe);
		return E_OUTOFMEMORY;
	}

	EnterCriticalSection(&CritSec);

	// First, validate ALL of the handles
	// The spec implies we should proceed with any of the handles that are GOOD
	// even if some of them are bad. However this server is not quite smart enough to do that
	// (and clients should not be passing bad handles in any case)
	//
	reject = 0;
	for (j=0; j<dwNumItems; j++)
	{
		if (!g.ItemValid(phServer[j]))
		{
			pe[j] = OPC_E_INVALIDHANDLE;
			reject = 1;
			continue;
		}

		pe[j] = S_OK;
	}

	// If any handles are bad
	// then we are finished
	//
	if (reject)
	{
		pIMalloc->Free(g.m_Async2WriteList);
		LeaveCriticalSection(&CritSec);
		return OPC_E_INVALIDHANDLE;
	}


	// If all handles are good, then que up async writes of the items
	// (exact implementation of this will be server specific)
	//
	for (j=0; j<dwNumItems; j++)
	{
		int I;
		I = phServer[j];	// Get ServerItem Handle
		g.m_Async2WriteList[j] = I;

		// Queue a write of this item
		// Per the Spec - errors are returned only in the callback
		//
		g.ItemPtr(I)->QueDeviceWrite( &pItemValues[j] );
	}

	// it all worked - the write is in progress
	//

	s.GenerateTransaction(pCancelID);		// Generate Cancel ID
	g.m_Async2WriteCanID = *pCancelID;		
	g.m_Async2WriteTID = TransactionID;		// Record Client passed ID
	g.m_Async2WriteCancel = 0;
	g.m_Async2WriteActive = dwNumItems;		// record number of items

	LeaveCriticalSection(&CritSec);

	return S_OK;
}



///////////////////////////////////////
// Refresh
// Queue up an Async refresh 
// (which is a cross between async read and ondatachange).
//
///////////////////////////////////////
STDMETHODIMP IXXXASIO2::Refresh2(
	OPCDATASOURCE   dwSource,
	DWORD           TransactionID,
	DWORD         * pCancelID
    )
{
	int reject = 0;
	int j;
	XXXGroup &g = *m_Parent;
	XXXServer &s = *g.m_ParentServer;
	int Count;

	// See if there is a callback registered
	//
	reject = 1;
	if(g.m_pCPC)
	{
		HRESULT r1;
		IXXXCP *pCP = NULL;
		r1 = g.m_pCPC->FindConnectionPoint(IID_IOPCDataCallback, (IConnectionPoint**)&pCP);
		if(SUCCEEDED(r1)) 
			if(pCP && pCP->m_Callback) reject = 0;
		if(pCP) pCP->Release();
	}
	if(reject) return CONNECT_E_NOCONNECTION;	// no callback available

	EnterCriticalSection(&CritSec);

	// Count active items in group
	//
	Count = 0;
	if (g.m_bActive) for (j=0; j<g.ItemHandles(); j++)
	{
		// If item is in use
		//
		if(g.ItemValid(j))
		{
			if(g.ItemPtr(j)->GetActive())
				Count++;
		}
	}

	// If nothing active, return FAIL (per spec)
	//
	if(Count == 0) 
	{
		LeaveCriticalSection(&CritSec);
		return E_FAIL;
	}

	// Store the list of items to be returned (all of them)
	// so that the background thread knows which ones to 'watch'.
	// This is treated somewhat like an async read of 'all'
	// Note that many different implementations of this are possible.
	//
	// In this case since we have created an explicit list of items to return,
	// we do not need to artificialy mark them as changed.
	//
	g.m_Refresh2List = (OPCHANDLE*)pIMalloc->Alloc(Count * sizeof( OPCHANDLE ));
	if(g.m_Refresh2List == NULL) 
	{
		LeaveCriticalSection(&CritSec);
		return E_OUTOFMEMORY;
	}

	// For each item in group
	//
	Count = 0;
	for (j=0; j<g.ItemHandles(); j++)
	{
		// If item is in use
		//
		if(g.ItemValid(j))
		{
			// Then add to list to be returned.
			//
			g.m_Refresh2List[Count] = j;	// Copy Item Handle

			// If device read
			// Queue a read of this item
			//
			if(dwSource == OPC_DS_DEVICE)
				g.ItemPtr(j)->QueDeviceRead(DEVICE_REFRESH);

			Count++;
		}
	}


	// the refresh is in progress
	//

	s.GenerateTransaction(pCancelID);		// Generate Cancel ID
	g.m_Refresh2CanID = *pCancelID;
	g.m_Refresh2TID = TransactionID;			// Record Client passed ID
	g.m_Refresh2Cancel = 0;
	g.m_Refresh2Source = dwSource;
	g.m_Refresh2Active = Count;

	LeaveCriticalSection(&CritSec);
	return S_OK;
}

///////////////////////////////////////
// Cancel
///////////////////////////////////////
STDMETHODIMP IXXXASIO2::Cancel2(
	DWORD dwTransactionID
    )
{
	XXXGroup &g = *m_Parent;

	// search for this transaction
	//
	if(g.m_Async2ReadActive &&
		(g.m_Async2ReadCanID == dwTransactionID))
	{
			g.m_Async2ReadCancel = 1;
			return S_OK;
	}
	if(g.m_Async2WriteActive &&
		(g.m_Async2WriteCanID == dwTransactionID))
	{
			g.m_Async2WriteCancel = 1;
			return S_OK;
	}
	if(g.m_Refresh2Active &&
		(g.m_Refresh2CanID == dwTransactionID))
	{
			g.m_Refresh2Cancel = 1;
			return S_OK;
	}

	return E_FAIL;
}

///////////////////////////////////////
// SetState
///////////////////////////////////////
STDMETHODIMP IXXXASIO2::SetEnable(
	BOOL bState
    )
{
	XXXGroup &g = *m_Parent;

	g.m_OnDataEnable = bState;
	
	return S_OK;
}

///////////////////////////////////////
// SetState
///////////////////////////////////////
STDMETHODIMP IXXXASIO2::GetEnable(
	BOOL *bState
    )
{
	XXXGroup &g = *m_Parent;

	*bState = g.m_OnDataEnable;
	return S_OK;
}

