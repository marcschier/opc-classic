// Server.cpp
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
//  the XXX Server Object for the OPC Sample server.
//
// This implementation uses 'tear off' interfaces
// (I.e. the interfaces are separate from the object 
//  which supports only IUnknown)
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
// 0.00  11/18/96 ACC
// 0.01  02/05/97 acc   Fix memory leaks (see acc001)
// 0.90  04/08/97 ACC   add async logic/UpdateData
// 1.0a  08/01/97 ACC   remove IEnumUnknown, add BrowseAddressSpace
//                      and Get/FreeAddressList()
//       09/11/97 ACC   add IOPCParameters
// 2.00  01/15/98 acc   add ConnectionPoint Support for IOPCShutdown.
//       04/08/98 acc   add szReason to shutdown
//                      make BSAS a tearoff i/f rather than separate obj
//       06/12/98 acc   cosmetic cleanup
//       08/25/98 acc   fix bug in GetAddressList which caused random failure
//       08/18/99 acc   add heirarchical browse 
//       09/09/99 WGI   add batch Enumerations interface
//       10/13/99 acc	modifications to connection points to fix initialization problem
//       07/14/00 acc   user proper details LocaleID.
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCTHRD.h"

extern CRITICAL_SECTION	CritSec;
extern OPCBObj root_tag;

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// OPCServer()
//   Constructor for the OPC Sample Server.
//
///////////////////////////////////////
XXXServer::XXXServer( LPUNKNOWN pOuter, void (*pfn)(void))
{
	int	j;
	SYSTEMTIME SystemTime;

	mRefCount = 0;

	// Clear out 'tear-off' interface ptrs
	// The interfaces will be created as needed in QueryInterface
	//
	m_pIServer = 0;
	m_pISPG = 0;
	m_pIPF = 0;
	m_pIPRM = 0;
	m_pCPC = 0;
	m_pICOMN = 0;
	m_pIBSAS = 0;
  m_pIBES = 0;
  m_pIBSVR = 0;
  m_pIBSVR2 = 0;
	m_curptr = &root_tag;

	GetSystemTime(&SystemTime);		// Get current UTC Time
	SystemTimeToFileTime(&SystemTime, &mLastUpdate); // and store it

	m_pfnDestroy = pfn;

	m_tid = 0;
	m_LCID = LOCALE_SYSTEM_DEFAULT;		//This sample supports only 1 LCID

	// Clear out the 'Groups'
	// This simple example only holds 10 groups
	// A real implementation would use some sort
	// of container or linked list
	//
	for (j=0; j<N_GRPS; j++)
	{
		m_groups[j].inuse = 0;
		m_groups[j].pGroup = NULL;
	}

  // Create the sample batch data.  We have to do
  // this here, not in one of the tear-offs, because
  // it is used by more than one of the interfaces.
  CreateSampleObjects();

	// Finally, plug this server into the Async helper
	//
	m_Slot = 0;
	FindServerSlot(&m_Slot, this);
	// later add error check!


}



///////////////////////////////////////
// ~XXXServer()
//   Destructor for the OPC Sample Server.
//
///////////////////////////////////////
XXXServer::~XXXServer( void)
{
	int	j;

	// Enter critical section whenever deleting anything
	//
	EnterCriticalSection(&CritSec);

	// First, turn off background processing	//acc003
	//
	if(m_Slot)ClearServerSlot(m_Slot);

	// If necessary free any memory associated with the GROUPS
	// (e.g. CStrings)
	//
	for (j=0; j<N_GRPS; j++)
	{
		// Your code here...
		//
		if(m_groups[j].inuse)
		{
			m_groups[j].pGroup->Release();
		}
	}

	// Then delete any tear off interfaces
	//
	if(m_pIServer) delete m_pIServer;
//	if(m_pISPG) delete m_pISPG;
	if(m_pIPF) delete m_pIPF;
	if(m_pIPRM) delete m_pIPRM;
	if(m_pICOMN) delete m_pICOMN;
	if(m_pIBSAS) delete m_pIBSAS;
	if(m_pCPC) delete m_pCPC;
  if(m_pIBES) delete m_pIBES;
  if(m_pIBSVR) delete m_pIBSVR;
  if(m_pIBSVR2) delete m_pIBSVR2;

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
STDMETHODIMP_(ULONG) XXXServer::AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
// IUnknown::Release()
//   Standard IUnknown implementation
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) XXXServer::Release( void)
{
	ULONG currentCount = --mRefCount;

	// If no references left for this server
	if ( currentCount == 0)
	{
		// Then delete this server.
		//
		if(m_pfnDestroy) (*m_pfnDestroy)();	// DLL or EXE can unload if last one
		delete this;
	}
	return currentCount;
}


///////////////////////////////////////
// IUnknown::QueryInterface()
//   Standard IUnknown implementation using
//   'tear off' interfaces
//
///////////////////////////////////////
STDMETHODIMP XXXServer::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	// check valid pointer
	//
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	// default return is NULL in case of error
	//
	*ppInterface = NULL;	

	if ( iid == IID_IUnknown )
	{
		*ppInterface = (IUnknown*) this;
	}

	else if ( iid == IID_IOPCServer)
	{
		if(m_pIServer == 0) m_pIServer = new IXXXServer(this);
		*ppInterface = m_pIServer;
	}
//	else if (iid == IID_IOPCServerPublicGroups)
//	{
//		if(m_pISPG == 0) m_pISPG = new IXXXOPCServerPublicGroups(this);
//		*ppInterface = m_pISPG;
//	}
	else if ((iid == IID_IPersistFile) || (iid == IID_IPersist))
	{
		if(m_pIPF == 0) m_pIPF = new IXXXPF(this);
		*ppInterface = m_pIPF;
	}
	else if (iid == IID_IOPCItemProperties)
	{
		if(m_pIPRM == 0) m_pIPRM = new IXXXPRM(this, pIMalloc);
		*ppInterface = m_pIPRM;
	}
	else if (iid == IID_IOPCCommon)
	{
		if(m_pICOMN == 0) m_pICOMN = new IXXXCOMN(this);
		*ppInterface = m_pICOMN;
	}
	else if (iid == IID_IOPCBrowseServerAddressSpace)
	{
		if(m_pIBSAS == 0) m_pIBSAS = new IXXXBSAS(this, pIMalloc);
		*ppInterface = m_pIBSAS;
	}
	else if ( iid == IID_IConnectionPointContainer)		//V2.0
	{
		// Create the CP for IOPCShutdown and the CPC to hold it
		// (if we have not already done so)
    // and hook them up to each other.
		//
		if(m_pCPC == 0) 
		{
      IXXXCP * pCP;

			// Create the ConnectionPoint itself
			// Note the CP is a separate object, 
			// not a tear off interface
			//
			pCP = new IXXXCP(IID_IOPCShutdown);
			// later check for error/null...

			// Create the CPC 
			// (this is the actual tear-off we will AddRef and return)
			//
			m_pCPC = new IXXXCPC(this, IID_IOPCShutdown);
			// later check for error/null...

			// tell the CP who his parent CPC is...
			//
			pCP->SetParent(m_pCPC);

			// and tell the CPC who his children are...
			// (which will addref the CP)
			//
			m_pCPC->AddChild(pCP);
		}
		*ppInterface = m_pCPC;

	}
	else if (iid == IID_IOPCBatchServer)
	{
		if(m_pIBSVR == 0) m_pIBSVR = new IXXXBSVR(this, pIMalloc);
		*ppInterface = m_pIBSVR;
	}
	else if (iid == IID_IOPCBatchServer2)
	{
		if(m_pIBSVR2 == 0) m_pIBSVR2 = new IXXXBSVR2(this, pIMalloc);
		*ppInterface = m_pIBSVR2;
	}
	else if (iid == IID_IOPCEnumerationSets)
	{
		if(m_pIBES == 0) m_pIBES = new IXXXBES(this, pIMalloc);
		*ppInterface = m_pIBES;
	}

	
	if ( *ppInterface == NULL)
		return E_NOINTERFACE;

	AddRef();
	return S_OK;
}


/////////////////////////////////////////////////////////////////////////////
// Private (non-OPC) Utility Functions
//


///////////////////////////////////////
// XXXServer::GetNameList()
// Create a list of all the group names
//
void XXXServer::GetNameList( OPCENUMSCOPE scope, LPOLESTR**GroupList, int *GroupCount)
{
	int	j, count = 0;
	LPOLESTR *mylist;

	// Defaults...
	*GroupList = NULL;
	*GroupCount = 0;

	// First pass - count private groups in use
	//
	for (j=0; j<N_GRPS; j++)
	{
		if(m_groups[j].inuse) count++;
	}
	if(!count) 
	{
		return;
	}

	// Second pass - figure out what to return and do it
	//
	switch(scope)
	{
	case OPC_ENUM_PRIVATE_CONNECTIONS:
	case OPC_ENUM_PRIVATE:
	case OPC_ENUM_ALL_CONNECTIONS:
	case OPC_ENUM_ALL:
		*GroupList = mylist = new LPOLESTR[count];	// Caller must free this!
		if (*mylist == NULL)
		{
			return;
		}
		for (j=0; j<N_GRPS; j++)
		{
			if(m_groups[j].inuse) 
			{
				// Note that the string is cloned to keep the enumerator 'stable'
				mylist[*GroupCount] = WSTRClone(m_groups[j].pGroup->m_szName, NULL);
				(*GroupCount)++;
			}
		}
		break;

	case OPC_ENUM_PUBLIC:
	case OPC_ENUM_PUBLIC_CONNECTIONS:
	default:
		break;
	}
}

///////////////////////////////////////
// XXXServer::FreeNameList()
// Free a list of group names
//
void XXXServer::FreeNameList( LPOLESTR*GroupList, int count)
{
	int j;
	
	if(!count) return;

	for(j=0; j<count; j++)
	{
		delete [] GroupList[j];
	}
	delete [] GroupList;
}

///////////////////////////////////////
// XXXServer::GetUnkList()
// Create a list of all the group Unknown Pointers
// Note that we do NOT AddRef the pointers here - the caller must do that
//
void XXXServer::GetUnkList( OPCENUMSCOPE scope, LPUNKNOWN ** GroupList, int *GroupCount)
{
	int	j, count = 0;
	LPUNKNOWN *mylist;

	// Defaults...
	*GroupList = NULL;
	*GroupCount = 0;

	// First pass - count private groups in use
	//
	for (j=0; j<N_GRPS; j++)
	{
		if(m_groups[j].inuse) count++;
	}
	if(!count) 
	{
		return;
	}

	// Second pass - figure out what to return and do it
	//
	switch(scope)
	{
	case OPC_ENUM_PRIVATE_CONNECTIONS:
	case OPC_ENUM_PRIVATE:
	case OPC_ENUM_ALL_CONNECTIONS:
	case OPC_ENUM_ALL:
		*GroupList = mylist = new LPUNKNOWN[count];
		if (*mylist == NULL)
		{
			return;
		}
		for (j=0; j<N_GRPS; j++)
		{
			if(m_groups[j].inuse) 
			{
				mylist[*GroupCount] = (LPUNKNOWN)(m_groups[j].pGroup);
				(*GroupCount)++;
			}
		}
		break;

	case OPC_ENUM_PUBLIC:
	case OPC_ENUM_PUBLIC_CONNECTIONS:
	default:
		break;
	}
}

///////////////////////////////////////
// XXXServer::FreeUnkList()
// Free a list of group pointers
// Note that since GetUnkList did not addref, we do not release
//
void XXXServer::FreeUnkList( LPUNKNOWN*GroupList, int count)
{
	
	if(!count) return;

	delete [] GroupList;
}




///////////////////////////////////////
// XXXServer::UpdateData
//
// Handle any async operations
// This includes Data Refresh
// 
//
void XXXServer::UpdateData(DWORD tics)
{
	int j;

	EnterCriticalSection(&CritSec);

	// For each possible group in server...
	//
	for(j=0; j<N_GRPS; j++)
	{
		// if group in this slot 
		//
		if(m_groups[j].inuse) 
		{
			XXXGroup *group;

			// get local group pointer (XXXGroup)
			// and handle any Asnyc events for the group
			//
			group = m_groups[j].pGroup;

			group->AsyncHelper(tics);
		}
	}

	LeaveCriticalSection(&CritSec);
}


///////////////////////////////////////
// XXXServer::GenerateTransactionID
//
void XXXServer::GenerateTransaction(DWORD *pTID)
{
	m_tid++;
	if(m_tid == 0) m_tid++;	// 0 is not a legal TID
	*pTID = m_tid;
}


///////////////////////////////////////
// XXXServer::ShutdownClients
//
void XXXServer::ShutdownClients(void)
{
	IXXXCP *pCP = NULL;

	// If there is a ConnectionPointContainer
	//
	if(m_pCPC)
	{
		// then get the CP for Shutdown
		//
		m_pCPC->FindConnectionPoint(IID_IOPCShutdown, (IConnectionPoint**)&pCP);
	}
	// And if the CP for Shutdown is active
	//
	if(pCP)
	{
		IOPCShutdown * pSH;

		// Then call the client function.
		//
		pSH = (IOPCShutdown *) pCP->m_Callback;
		if(pSH) pSH->ShutdownRequest(L"Server Shutdown by User");
	}
}

