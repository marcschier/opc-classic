// i_asio.cpp
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
//  the IOPCASyncIO interface for groups in the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC   stubs
//  0.90  04/08/97 ACC   add async logic
//  1.0a  08/01/97 acc   use 'GetActive()'
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
// IXXXASIO()
//   Constructor for this Interface
///////////////////////////////////////
IXXXASIO::IXXXASIO( LPUNKNOWN parent )
{
	m_Parent = (XXXGroup *)parent;
}



///////////////////////////////////////
// ~IXXXASIO()
//   Destructor for this Interface
///////////////////////////////////////
IXXXASIO::~IXXXASIO( void)
{
	m_Parent->m_pIXXXASIO = 0;
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXASIO::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXASIO::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXASIO::QueryInterface( REFIID iid, LPVOID* ppInterface)
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
STDMETHODIMP IXXXASIO::Read(
	DWORD           dwConnection,
	OPCDATASOURCE   dwSource,
	DWORD           dwNumItems,
	OPCHANDLE     * phServer,
	DWORD         * pTransactionID,
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
	if(pTransactionID == NULL) return E_INVALIDARG;
	if(ppErrors == NULL) return E_INVALIDARG;

	// This sample supports only 1 outstanding async read per group
	//
	if(g.m_AsyncReadActive) return CONNECT_E_ADVISELIMIT;

	// Alloc memory for returned error list
	// and the stored handle list (to be returned later in the callback)
	//
	pe = *ppErrors = (HRESULT*)pIMalloc->Alloc(dwNumItems * sizeof( HRESULT ));
	if(pe == NULL) return E_OUTOFMEMORY;

	// Store the list of items to be read
	// so that the background thread knows which ones to 'watch'
	// Note that many different implementations of this are possible.
	//
	g.m_AsyncReadList = (OPCHANDLE*)pIMalloc->Alloc(dwNumItems * sizeof( OPCHANDLE ));
	if(g.m_AsyncReadList == NULL) 
	{
		pIMalloc->Free(pe);
		return E_OUTOFMEMORY;
	}


	EnterCriticalSection(&CritSec);

	// First, validate ALL of the handles
	//
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
		pIMalloc->Free(g.m_AsyncReadList);
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
		g.m_AsyncReadList[j] = I;


		// If device read
		// Queue a read of this item
		//
		if(dwSource == OPC_DS_DEVICE)
			g.ItemPtr(I)->QueDeviceRead(DEVICE_READ);
	}

	// it all worked - the read is in progress
	//

	s.GenerateTransaction(pTransactionID);
	g.m_AsyncReadTID = *pTransactionID;
	g.m_AsyncReadCancel = 0;
	g.m_AsyncReadSource = dwSource;
	g.m_AsyncReadActive = dwNumItems;		// record number of items

	LeaveCriticalSection(&CritSec);
	return S_OK;
}



///////////////////////////////////////
// Write
// Queue up an Async Write.
//
///////////////////////////////////////
STDMETHODIMP IXXXASIO::Write(
	DWORD       dwConnection,
	DWORD       dwNumItems, 
	OPCHANDLE * phServer,
	VARIANT   * pItemValues, 
	DWORD     * pTransactionID,
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
	if(pTransactionID == NULL) return E_INVALIDARG;
	if(pItemValues == NULL) return E_INVALIDARG;
	if(ppErrors == NULL) return E_INVALIDARG;

	// This sample supports only 1 outstanding async write per group
	//
	if(g.m_AsyncWriteActive) return CONNECT_E_ADVISELIMIT;

	// Alloc memory for returned error list
	// and the stored handle list (to be returned later in the callback)
	//
	pe = *ppErrors = (HRESULT*)pIMalloc->Alloc(dwNumItems * sizeof( HRESULT ));
	if(pe == NULL) return E_OUTOFMEMORY;

	// Store the list of items to be written
	// so that the background thread knows which ones to 'watch'
	// Note that many different implementations of this are possible.
	//
	g.m_AsyncWriteList = (OPCHANDLE*)pIMalloc->Alloc(dwNumItems * sizeof( OPCHANDLE ));
	if(g.m_AsyncWriteList == NULL) 
	{
		pIMalloc->Free(pe);
		return E_OUTOFMEMORY;
	}

	EnterCriticalSection(&CritSec);

	// First, validate ALL of the handles
	//
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
		pIMalloc->Free(g.m_AsyncWriteList);
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
		g.m_AsyncWriteList[j] = I;

		// Queue a write of this item
		// Per the Spec - errors are returned only in the callback
		//
		g.ItemPtr(I)->QueDeviceWrite( &pItemValues[j] );
	}

	// it all worked - the write is in progress
	//

	s.GenerateTransaction(pTransactionID);
	g.m_AsyncWriteTID = *pTransactionID;
	g.m_AsyncWriteCancel = 0;
	g.m_AsyncWriteActive = dwNumItems;		// record number of items

	LeaveCriticalSection(&CritSec);

	return S_OK;
}



///////////////////////////////////////
// Refresh
// Queue up an Async refresh 
// (which is a cross between async read and ondatachange).
//
///////////////////////////////////////
STDMETHODIMP IXXXASIO::Refresh(
	DWORD           dwConnection,
	OPCDATASOURCE   dwSource,
	DWORD         * pTransactionID
    )
{
	int j;
	XXXGroup &g = *m_Parent;
	XXXServer &s = *g.m_ParentServer;
	int Count;

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
	// In this case since we have created an explicit list of items to watch,
	// we do not need to artificaly mark them as changed.
	//
	g.m_RefreshList = (OPCHANDLE*)pIMalloc->Alloc(Count * sizeof( OPCHANDLE ));
	if(g.m_RefreshList == NULL) 
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
			g.m_RefreshList[Count] = j;	// Copy Item Handle

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

	s.GenerateTransaction(pTransactionID);
	g.m_RefreshTID = *pTransactionID;
	g.m_RefreshCancel = 0;
	g.m_RefreshSource = dwSource;
	g.m_RefreshActive = Count;

	LeaveCriticalSection(&CritSec);
	return S_OK;
}

///////////////////////////////////////
// Cancel
///////////////////////////////////////
STDMETHODIMP IXXXASIO::Cancel(
	DWORD dwTransactionID
    )
{
	XXXGroup &g = *m_Parent;

	// search for this transaction
	//
	if(g.m_AsyncReadActive &&
		(g.m_AsyncReadTID == dwTransactionID))
	{
			g.m_AsyncReadCancel = 1;
			return S_OK;
	}
	if(g.m_AsyncWriteActive &&
		(g.m_AsyncWriteTID == dwTransactionID))
	{
			g.m_AsyncWriteCancel = 1;
			return S_OK;
	}
	if(g.m_RefreshActive &&
		(g.m_RefreshTID == dwTransactionID))
	{
			g.m_RefreshCancel = 1;
			return S_OK;
	}

	return E_FAIL;
}

