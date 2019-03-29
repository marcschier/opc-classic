// Group.cpp
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
//  the 'core' of the XXXGroup Object for the OPC Sample server.
//
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  0.00  11/18/96 ACC
//  0.90  04/02/97 acc   add Async support
//        08/25/97 acc   add ConnectionPoint Support.
//  2.00  01/15/98 acc   add 'final' ConnectionPoint Support.
//        06/15/98 acc   fix m_OnDataEnable to be TRUE by default
//
//


#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"

extern CRITICAL_SECTION	CritSec;

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// OPCGroup()
//   Constructor for the OPC Sample Group.
//
///////////////////////////////////////
XXXGroup::XXXGroup( LPUNKNOWN pOuter)
{
	int	j;

	mRefCount = 0;

	// Record Parent server
	//
	m_ParentServer = (XXXServer *)pOuter;

	// Initizlize member variables
	//
	m_ServerGroupHandle = 0;
	m_ClientGroupHandle = 0;
	m_dwRevisedRate = 0;
	m_bActive = FALSE;
	m_szName = 0;

	// Clear out 'tear-off' interface ptrs
	// The interfaces will be created as needed in QueryInterface
	//
	m_pIXXXGSM = 0;
	m_pIXXXPGSM = 0;	// optional
	m_pIXXXSIO = 0;
	m_pIXXXASIO = 0;
	m_pIXXXASIO2 = 0;
	m_pIXXXIM = 0;
	m_pIXXXDO = 0;
	m_pCPC = 0;

	// async state variables used by either DataObject
	//
	m_OnDataEnable = TRUE;
	m_AsyncReadActive = 0;
	m_AsyncReadCancel = 0;
	m_AsyncReadTID = 0;
	m_AsyncReadList = 0;
	m_AsyncReadSource = OPC_DS_CACHE;

	m_AsyncWriteActive = 0;
	m_AsyncWriteCancel = 0;
	m_AsyncWriteTID = 0;
	m_AsyncWriteList = 0;

	m_RefreshTID = 0;
	m_RefreshCancel = 0;
	m_RefreshSource = OPC_DS_CACHE;
	m_RefreshActive = 0;

	// async state variables used by ConnectionPoint
	//
	m_Async2ReadActive = 0;
	m_Async2ReadCancel = 0;
	m_Async2ReadTID = 0;
	m_Async2ReadList = 0;

	m_Async2WriteActive = 0;
	m_Async2WriteCancel = 0;
	m_Async2WriteTID = 0;
	m_Async2WriteList = 0;

	m_Refresh2TID = 0;
	m_Refresh2Cancel = 0;
	m_Refresh2Source = OPC_DS_CACHE;
	m_Refresh2Active = 0;

	m_scan = 0;

	// Clear out the 'items'
	// This simple example only holds 100 items
	// A real implementation would use some sort
	// of container or linked list
	//
	for (j=0; j<N_ITEMS; j++)
	{
		m_items[j].inuse = 0;
		m_items[j].pItem = NULL;
	}
}



///////////////////////////////////////
// ~XXXGroup()
//   Destructor for the OPC Sample Group.
//
///////////////////////////////////////
XXXGroup::~XXXGroup( void)
{
	int	j;

	// Enter critical section whenever deleting anything
	//
	EnterCriticalSection(&CritSec);

	// Free memory associated with group name
	//
	if(m_szName) WSTRFree(m_szName, NULL);

	// Delete all the Group level tear off interfaces
	//
	if(m_pIXXXGSM) delete m_pIXXXGSM;
//	if(m_pIXXXPGSM) delete m_pIXXXPGSM;	// optional 
	if(m_pIXXXSIO) delete m_pIXXXSIO;
	if(m_pIXXXASIO) delete m_pIXXXASIO;
	if(m_pIXXXASIO2) delete m_pIXXXASIO2;
	if(m_pIXXXIM) delete m_pIXXXIM;
	if(m_pIXXXDO) delete m_pIXXXDO;

	// Clean up any Async & Connection Point stuff
	//
	if(m_pCPC) delete m_pCPC;

	if(m_AsyncReadList) pIMalloc->Free(m_AsyncReadList); // should be 0 already
	if(m_AsyncWriteList) pIMalloc->Free(m_AsyncWriteList);

	if(m_Async2ReadList) pIMalloc->Free(m_Async2ReadList); // should be 0 already
	if(m_Async2WriteList) pIMalloc->Free(m_Async2WriteList);

	
	// Also free any memory associated with the ITEMS
	//
	for (j=0; j<N_ITEMS; j++)
	{
		// This release should delete the item
		// (unless some badly behaved program is holding another interface)
		//
		if(m_items[j].inuse)
		{
			m_items[j].inuse = 0;
			m_items[j].pItem->Release();
		}
	}
	LeaveCriticalSection(&CritSec);
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions
//

///////////////////////////////////////
// IUnknown::AddRef()
//   Standard IUnknown implementation
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) XXXGroup::AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
// IUnknown::Release()
//   Standard IUnknown implementation
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) XXXGroup::Release( void)
{
	ULONG currentCount = --mRefCount;

	// If no references left for this group
	if ( currentCount == 0)
	{
		// Then delete this group.
		delete this;
	}
	return currentCount;
}


///////////////////////////////////////
// IUnknown::QueryInterface()
//   Standard IUnknown implementation
//
///////////////////////////////////////
STDMETHODIMP XXXGroup::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown )
		*ppInterface = (IUnknown*) this;

	else if ( iid == IID_IOPCGroupStateMgt)
	{
		if(m_pIXXXGSM == 0) m_pIXXXGSM = new IXXXGSM(this);
		*ppInterface = m_pIXXXGSM;
	}
	else if ( iid == IID_IOPCPublicGroupStateMgt)
	{
		if(m_pIXXXPGSM == 0) m_pIXXXPGSM = 0;	// optional (notimplemented)
		*ppInterface = m_pIXXXPGSM;
	}
	else if ( iid == IID_IOPCSyncIO)
	{
		if(m_pIXXXSIO == 0) m_pIXXXSIO = new IXXXSIO(this) ;
		*ppInterface = m_pIXXXSIO;
	}
	else if ( iid == IID_IOPCAsyncIO)
	{
		if(m_pIXXXASIO == 0) m_pIXXXASIO = new IXXXASIO(this) ;
		*ppInterface = m_pIXXXASIO;
	}
	else if ( iid == IID_IOPCAsyncIO2)
	{
		if(m_pIXXXASIO2 == 0) m_pIXXXASIO2 = new IXXXASIO2(this) ;
		*ppInterface = m_pIXXXASIO2;
	}
	else if ( iid == IID_IOPCItemMgt)
	{
		if(m_pIXXXIM == 0) m_pIXXXIM = new IXXXIM(this) ;
		*ppInterface = m_pIXXXIM;
	}
	else if ( iid == IID_IDataObject)
	{
		if(m_pIXXXDO == 0) m_pIXXXDO = new IXXXDO(this);
		*ppInterface = m_pIXXXDO;
	}
	else if ( iid == IID_IConnectionPointContainer)		//V2.0
	{
		// Create the CP for IOPCDataCallback and the CPC to hold it
		// (if we have not already done so)
		// 
		if(m_pCPC == 0) 
		{
			IXXXCP * pCP;

			// Also Create the ConnectionPoint itself
			// Note the CP is a separate object, 
			// not a tear off interface
			//
			pCP = new IXXXCP(m_pCPC, IID_IOPCDataCallback);
			// later check for error/null...

			// Now create the CPC (which will addref the CP)
			// (this is the actual tear-off we will return)
			//  Note, I could probably move the 'new' for the CP (above) into the CPC constructor
			//
			m_pCPC = new IXXXCPC(this, IID_IOPCDataCallback , pCP);
			// later check for error/null...
		}
		*ppInterface = m_pCPC;
	}

	else
	{
		*ppInterface = NULL;
	}
	
	if ( *ppInterface == NULL)
		return E_NOINTERFACE;

	// Addref the group for each tear off interface created
	// (This is equivalent to ppInterface->AddRef() which delegates back here)
	//
	AddRef();
	return S_OK;
}


///////////////////////////////////////
// XXXGroup::GetItemList()
// Create a list of all the item attributes in Local Memory
//
// Note that local heap is used 
// and that caller must free the list using FreeItemList
//
void		XXXGroup::GetItemList( OPCITEMATTRIBUTES **AttrList, int *ItemCount)
{
	int	j, count = 0;
	OPCITEMATTRIBUTES *myAttr;

	*ItemCount = 0;

	// First pass - count items in use
	//
	for (j=0; j<ItemHandles(); j++)
	{
		if(ItemValid(j)) count++;
	}
	if(!count) 
	{
		return;
	}

	*AttrList = myAttr = new OPCITEMATTRIBUTES[count];
	if (myAttr == NULL)
	{
		return;
	}

	// Second pass - figure out what to return and do it
	//
	for (j=0; j<ItemHandles(); j++)
	{
		if(ItemValid(j)) 
		{
			// Copy the needed items...
			ItemPtr(j)->IAGet(&myAttr[*ItemCount]);
			(*ItemCount)++;
		}
	}
}



///////////////////////////////////////
// XXXGroup::FreeItemList()
// Free a list of item attributes
//
//		The strings in the ATTRs
//		The Attrs themselves 
//
void		XXXGroup::FreeItemList( OPCITEMATTRIBUTES *AttrList, int ItemCount)
{
	int	j;

	if(!ItemCount) return;

	for (j=0; j<ItemCount; j++)
	{
		// Free the contents of the IA
		IAFree(&AttrList[j], NULL);
	}
	delete [] AttrList;
}





///////////////////////////////////////
// XXXGroup::AsyncHelper
//
// This method handles all ASYNC logic for the group.
// Note that we respond to async reads & writes 
// at faster than the update rate.
//
// The DataObject and ConnectionPoint logic are very similar
//
void		XXXGroup::AsyncHelper( DWORD tics )
{
	// Check on Connection Points
	//
	IXXXCP *pCP = NULL;
	HRESULT r1;

	if(m_pCPC)
	{
		r1 = m_pCPC->FindConnectionPoint(IID_IOPCDataCallback, (IConnectionPoint**)&pCP);
	}

	// 'Scan' the values at the update rate
	// and check for OnDataChange
	//
	m_scan -= tics;
	if(m_scan <= 0) 
	{
		// 'Scan'the hardware (in this case execute the simulator)
		//
		Scan();

		// If IAdviseSink active then check for OnDataChange
		//
		if (m_pIXXXDO && ((m_pIXXXDO->m_datatimeCallback) || (m_pIXXXDO->m_dataCallback)))
		{
			CheckDOOnDataTimeChange();	//DataObject with time
			CheckDOOnDataChange();		//DataObject without time
		}

		// If IOPCCallback active and enabled then check for OnDataChange
		//
		if(m_OnDataEnable && pCP && pCP->m_Callback)
		{
			CheckCPOnDataChange(pCP);		//Cpoint
		}

		m_scan = m_dwRevisedRate;
	}

	// If IAdviseSink active then check for Async Operations
	//
		if (m_pIXXXDO && ((m_pIXXXDO->m_datatimeCallback) || (m_pIXXXDO->m_dataCallback)))
	{
		CheckDORefresh();
		CheckDOAsyncRead();
	}
	if (m_pIXXXDO && m_pIXXXDO->m_writeCallback)
	{
		CheckDOAsyncWrite();
	}

	// If ConnectionPoint active then check for Async Operations
	//
	if(pCP && pCP->m_Callback)
	{
		CheckCPRefresh(pCP);
		CheckCPAsyncRead(pCP);
		CheckCPAsyncWrite(pCP);
	}

	// Release the interface we got from FindInterface
	//
	if(pCP) pCP->Release();
}


///////////////////////////////////////
// XXXGroup::Scan
//
// Group Scanner - called at UpdateRate
// This keeps the local data cache up to date.
// It also marks items as changed for OnDataChange
//
void		XXXGroup::Scan( void )
{
	int j;
	// Normally we would fetch fresh data for the group here.
	// But in this sample we just simulate data...
	//
	// *** Add Server Specific Code ***
	//
	for (j=0; j<ItemHandles(); j++)
	{
		if(ItemValid(j))
		{
			ItemPtr(j)->Simulate();
		}
	}
}

