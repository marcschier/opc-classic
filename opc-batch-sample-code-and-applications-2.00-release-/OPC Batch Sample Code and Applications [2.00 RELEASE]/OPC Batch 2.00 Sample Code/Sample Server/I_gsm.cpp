// i_gsm.cpp
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
//  the IOPCGroupStateMgt interface for groups in the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
// 0.02  04/08/97 acc   fix SetName to call proper 'Free' Function
//                      fix Clone to copy LCID, TimeBias, Deadband
//        02/22/00 acc  improved error checking in SetState, SetName
//                      fix CloneGroup for proper auto naming.
//        07/14/00 acc  fix problem in requested update rate validation.
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCERROR.h"

// local function
//
HRESULT CheckName(XXXServer &s, LPCWSTR szName);

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXGSM()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXGSM::IXXXGSM( LPUNKNOWN parent )
{
	m_Parent = (XXXGroup *)parent;
}



///////////////////////////////////////
// ~IXXXGSM()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXGSM::~IXXXGSM( void)
{
	m_Parent->m_pIXXXGSM = 0;
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXGSM::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXGSM::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXGSM::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXGSM (IOPCGroupStateMgt) interface functions
//


///////////////////////////////////////
// GetState
///////////////////////////////////////
STDMETHODIMP IXXXGSM::GetState(
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
	*pUpdateRate = m_Parent->m_dwRevisedRate;
	*pActive = m_Parent->m_bActive;
	*ppName = WSTRClone( m_Parent->m_szName, pIMalloc);
	*pTimeBias = m_Parent->m_TimeBias;
	*pPercentDeadband = m_Parent->m_Deadband;
	*pLCID = m_Parent->m_LCID;
	*phClientGroup = m_Parent->m_ClientGroupHandle;
	*phServerGroup = m_Parent->m_ServerGroupHandle;

	return S_OK;
}


///////////////////////////////////////
// SetState
///////////////////////////////////////
STDMETHODIMP IXXXGSM::SetState( 
	DWORD     * pRequestedUpdateRate, 
	DWORD     * pRevisedUpdateRate, 
	BOOL      * pActive, 
	LONG      * pTimeBias,
	FLOAT     * pPercentDeadband,
	DWORD     * pLCID,
	OPCHANDLE * phClientGroup
    )
{
	HRESULT result = S_OK;

	if(pPercentDeadband) 
	{
		// Validate sensible deadband	acc022200
		if(*pPercentDeadband < 0.0) return E_INVALIDARG;
		if(*pPercentDeadband > 100.0) return E_INVALIDARG;
	}

	if(pRequestedUpdateRate) 
	{
		// Validate the UpdateRate
		//
		result = m_Parent->SetUpdateRate(*pRequestedUpdateRate);

	}
	if(pActive) m_Parent->m_bActive = *pActive;
	if(phClientGroup) m_Parent->m_ClientGroupHandle = *phClientGroup;
	if(pPercentDeadband) m_Parent->m_Deadband = *pPercentDeadband;
	if(pLCID) m_Parent->m_LCID = *pLCID;
	if(pTimeBias) m_Parent->m_TimeBias = *pTimeBias;

	*pRevisedUpdateRate = m_Parent->m_dwRevisedRate;

	return result;
}


///////////////////////////////////////
// SetName
///////////////////////////////////////
STDMETHODIMP IXXXGSM::SetName( 
	LPCWSTR szName
    )
{
	XXXServer &s = *m_Parent->m_ParentServer;
	HRESULT r1;

	// Sanity check in name acc022200
	//
	if(!szName) return E_INVALIDARG;
	if(!*szName) return E_INVALIDARG;

	// Verify that the name provided is Unique		acc022200
	//
	r1 = CheckName(s, szName);
	if(r1 != S_OK) return r1;

	// Free old name and store new name
	//
	WSTRFree(m_Parent->m_szName, NULL);
	m_Parent->m_szName = WSTRClone( szName, NULL);

	return S_OK;
}


///////////////////////////////////////
// CloneGroup
///////////////////////////////////////
STDMETHODIMP IXXXGSM::CloneGroup(
	LPCWSTR     szName,
	REFIID      riid,
	LPUNKNOWN * ppUnk
    )
{
	int	i;
	int	j;
	XXXGroup *oldgroup = m_Parent;
	XXXGroup *newgroup;
	XXXServer &s = *m_Parent->m_ParentServer;
	HRESULT	r1;
	char nbuf[10];
	WCHAR Wnbuf[20];

	// find a place in the server to put the new group (similar to i_server::AddGroup)
	//
	for(j=0; j<N_GRPS; j++)
	{
		if(s.m_groups[j].inuse == 0) break;
	}
	if(j >= N_GRPS)
		return E_OUTOFMEMORY;

	// Generate Unique Name (WCHAR) if not supplied by caller		acc022200
	// Note the algorithm below is a somewhat 'weak' example
	//
	if(!szName || !*szName)
	{
		char *p = itoa(j, nbuf, 10);
		WCHAR *sc = Wnbuf;
		while(*p)
		{
			*sc = *p;	// convert SBCS to WCHAR
			sc++;p++;
		}
		*sc = 0;
		szName = Wnbuf;
	}

	// Verify that name provided by caller is Unique		acc022200
	//
	r1 = CheckName(s, szName);
	if(r1 != S_OK) return r1;

	// Create the new (cloned) group (returns IUnknown)
	// and do an 'AddRef' since we will hold this IUnknown
	// in the Server
	//
	newgroup = new XXXGroup(m_Parent);
	if(newgroup == NULL)
	{
		return E_OUTOFMEMORY;
	}
	newgroup->AddRef();

	// And request a 2nd interface for the caller
	//
	r1 = newgroup->QueryInterface(riid, (LPVOID*) ppUnk);
	if(FAILED(r1))
	{
		// If error - delete group and return
		delete newgroup;
		return r1;
	}

	// If OK then record the group in the server for future use
	//
	s.m_groups[j].inuse = 1;
	s.m_groups[j].pGroup = newgroup;

	// If OK then Copy info from the old group to the new group
	// (except name and server handle)
	//
	newgroup->m_ServerGroupHandle = j;
	newgroup->m_ClientGroupHandle = oldgroup->m_ClientGroupHandle;
	newgroup->m_dwRevisedRate = oldgroup->m_dwRevisedRate;
	newgroup->m_bActive = FALSE;
	newgroup->m_szName = WSTRClone(szName, NULL);
	newgroup->m_TimeBias = oldgroup->m_TimeBias;
	newgroup->m_Deadband = oldgroup->m_Deadband;
	newgroup->m_LCID = oldgroup->m_LCID;

	// And then copy the ITEMS from the old group to the new group
	// Note that the Item Handles are preserved (per the OPC specification).
	//
	for(i=0; i<oldgroup->ItemHandles(); i++)
	{
		// Allocate a specific slot/handle in the new group
		// (Initially empty)
		//
		newgroup->ItemReAlloc(i);

		// And if old item is in use then copy it
		//
		if(oldgroup->ItemValid(i))
		{
			newgroup->ItemSet(i, oldgroup->ItemPtr(i)->Clone(newgroup, i));
		}
	}
	return S_OK;
}

///////////////////////////////////////
// CheckName for uniqueness
///////////////////////////////////////
HRESULT CheckName(XXXServer &s, LPCWSTR szName)
{
	// Verify that the name provided is Unique		//acc022200
	// (Note that its a bit trickey to call GetGroupByName from here...)
	//
	IXXXServer *iS;
	s.QueryInterface(IID_IOPCServer, (LPVOID*) &iS);	// does AddRef

	LPUNKNOWN test;
	iS->GetGroupByName(szName, IID_IUnknown, &test);
	if(test)
	{
		test->Release();
		iS->Release();
		return OPC_E_DUPLICATENAME;
	}
	iS->Release();
	iS = NULL;
	return S_OK;
}
