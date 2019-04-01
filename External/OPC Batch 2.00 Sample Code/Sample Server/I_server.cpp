// I_Server.cpp
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
//  the IOPCServer interface for the XXX server.
//
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  0.00  11/18/96 ACC
//        12/06/96 acc	add GetGroupByName
//  0.02  05/05/97 acc  (acc002) generate unique group name	
//                      if none provided by caller.
//                      and verify that caller provided name is locally unique
//  2.00  01/19/98 acc  add check for null string to AddGroup
//        01/09/99 acc  add missing check for nul deadband ptr to AddGroup
//        10/14/99 acc  fix CreateGropEnumerator for proper error return E_NOINTERFACE, 
//                      add bForce support to RemoveGroup
//                      fix bug in auto groupname generation (missing nul at end)
//        02/22/00 acc  add parameter sanity check to GetGroupByName, fix bForce error return to be correct
//        07/13/00 acc  add call to CoDisconnectObject on Force of Remove Group.
//                      fix for proper initialization of TimeZoneBias.
//                      tweek error return of GetGroupByName to pass compliance test (spec is ambiguous)
//                      tweek handling of UpdateRate in AddGroup
//  2.00  08/11/00 ACC/MD implement GetOPCError()
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCERROR.h"

extern CRITICAL_SECTION	CritSec;
extern	HRESULT GetOPCError( HRESULT hr, LPWSTR *ppString); //opc_err.cpp

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXServer()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXServer::IXXXServer( LPUNKNOWN parent )
{
	m_Parent = (XXXServer *)parent;
}



///////////////////////////////////////
// ~IXXXServer()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXServer::~IXXXServer( void)
{
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXServer::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXServer::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXServer::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXServer (IOPCServer) interface functions
//

static	WCHAR	VendorInfo[] = L"OPC Batch Sample Server";

///////////////////////////////////////
// IXXXServer::GetStatus()
//   This function fills in the OPCSERVERSTATUS structure that was passed in.
///////////////////////////////////////
STDMETHODIMP IXXXServer::GetStatus( OPCSERVERSTATUS** ppServerStatus)
{
	OPCSERVERSTATUS* pServerStatus;

	if ( ppServerStatus == NULL)
		return E_INVALIDARG;

	// allocate some memory for the struct
	pServerStatus = (OPCSERVERSTATUS*) pIMalloc->Alloc( sizeof(OPCSERVERSTATUS) );
	if(pServerStatus)
	{
		// and for the serverinfo string
		pServerStatus->szVendorInfo = WSTRClone( VendorInfo, pIMalloc);

		pServerStatus->ftStartTime = serverStartTime;
		CoFileTimeNow( &pServerStatus->ftCurrentTime);
		pServerStatus->ftLastUpdateTime = m_Parent->mLastUpdate;
		
		pServerStatus->dwServerState = OPC_STATUS_RUNNING;
		pServerStatus->dwGroupCount = 0;
		pServerStatus->dwBandWidth = 0;
		
		pServerStatus->wMajorVersion = 2;
		pServerStatus->wMinorVersion = 0;
		pServerStatus->wBuildNumber = 0;
		pServerStatus->wReserved = 0;
	}
	else
	{
		// else the struct alloc failed
	}

	// return the result (if any) and the error
	*ppServerStatus = pServerStatus;

	if(pServerStatus) return S_OK;
	return E_OUTOFMEMORY;
}



///////////////////////////////////////
// IXXXServer::GetErrorString()
//   For server specific error codes we need to return a user displayable string 
//   in the user's language. 
// The easiest way to do this is to put the strings in the RC file and use LoadString
//
///////////////////////////////////////
STDMETHODIMP IXXXServer::GetErrorString( HRESULT hr, LCID locale, LPWSTR *ppString)
{
	// validate the LocaleID and pointer
	//
	if ( (locale != MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), SORT_DEFAULT)) &&
		 (locale != LOCALE_SYSTEM_DEFAULT) &&
		 (locale != 0)) 
		return E_INVALIDARG;

	if (ppString == NULL)
		return E_INVALIDARG;

	// Then get the string from a common function (shared with i_comn.cpp)
	//
	return GetOPCError( hr, ppString);

}




///////////////////////////////////////
// IXXXServer::AddGroup()
//   This function creates a new group of the specified name on this server.  
//   Note TimeBias, Deadband and LCID are not implemented
///////////////////////////////////////
STDMETHODIMP         IXXXServer::AddGroup( 
		LPCWSTR szName,
		BOOL bActive,
		DWORD dwRequestedUpdateRate,
		OPCHANDLE hClientGroup,
		LONG *pTimeBias,
		FLOAT *pPercentDeadband,
		DWORD dwLCID,
		OPCHANDLE *phServerGroup,
		DWORD *pRevisedUpdateRate,
		REFIID riid,
		LPUNKNOWN *ppUnk
		)
{
	int	j;
	XXXGroup *newgroup;
	XXXServer &s = *m_Parent;
	HRESULT	r1, result = S_OK;
	char nbuf[10];
	WCHAR Wnbuf[20];

	// Default return in case of error
	//
	*ppUnk = 0;

	// Tentatively find a place to put the group
	//
	for(j=0; j<N_GRPS; j++)
	{
		if(s.m_groups[j].inuse == 0) break;
	}
	if(j >= N_GRPS)
		return E_OUTOFMEMORY;

	// Generate Unique Name (WCHAR) if not supplied by caller		acc002
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
	
	// Verify that the name provided is Unique		acc002
	//
	LPUNKNOWN test;
	GetGroupByName(szName, IID_IUnknown, &test);
	if(test)
	{
		test->Release();
		return OPC_E_DUPLICATENAME;
	}

	// Create the group (returns IUnknown)
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

	// If OK then Record the group in the server for future use
	//
	s.m_groups[j].inuse = 1;
	s.m_groups[j].pGroup = newgroup;

	newgroup->m_ServerGroupHandle = j;
	newgroup->m_ClientGroupHandle = hClientGroup;
	result = newgroup->SetUpdateRate(dwRequestedUpdateRate);
	newgroup->m_bActive = bActive;
	newgroup->m_szName = WSTRClone(szName, NULL);
	newgroup->m_LCID = dwLCID;

	if(pPercentDeadband)
		newgroup->m_Deadband = *pPercentDeadband;
	else
		newgroup->m_Deadband = 0.0;

	if(pTimeBias) 
		newgroup->m_TimeBias = *pTimeBias;
	else
	{
		// Fetch current system TimeZoneBias TIME_ZONE_INFORMATION
		//
		TIME_ZONE_INFORMATION tzi;
		DWORD err;
		err = GetTimeZoneInformation(&tzi);
		if( err != TIME_ZONE_ID_INVALID)
			newgroup->m_TimeBias = tzi.Bias;
		else
			newgroup->m_TimeBias = 0L;
	}
	// Return handle and updaterate to the caller
	//
	*phServerGroup = j;
	if(pRevisedUpdateRate) *pRevisedUpdateRate = newgroup->m_dwRevisedRate;

	return result;
}


///////////////////////////////////////
// IXXXServer::GetGroupByName()
//   This function scans the set of groups known to this OPC server and returns a pointer to the
//   IOPCGroup interface for the specified group.
///////////////////////////////////////
STDMETHODIMP IXXXServer::GetGroupByName( LPCWSTR szGroupName, 
		REFIID riid, LPUNKNOWN *ppUnk)
{
	int	j;
	XXXGroup *group;
	XXXServer &s = *m_Parent;
	HRESULT r1;

	// Sanity Check
	//
	if(!szGroupName || !*szGroupName)
		return E_INVALIDARG;

	*ppUnk = 0;

	// For each 'slot' in this server...
	//
	for(j=0; j<N_GRPS; j++)
	{
		// if it contains a group and if the name matches...
		//
		if(s.m_groups[j].inuse == 0) continue;
		if (wcscmp(s.m_groups[j].pGroup->m_szName, szGroupName) == 0)
		{
			// Then query for and return the requested interface
			//
			group = s.m_groups[j].pGroup;
			r1 = group->QueryInterface(riid, (LPVOID*) ppUnk);
			if(FAILED(r1))
			{
				// If error - return
				return r1;
			}
			return S_OK;
		}
	}
	return E_INVALIDARG; 
}





///////////////////////////////////////
// IXXXServer::RemoveGroup()
//		This function removes the specified group from the server.  
//		Note that the group doesn't
//		go away until the last reference to it is removed.
//
///////////////////////////////////////
STDMETHODIMP IXXXServer::RemoveGroup( OPCHANDLE groupHandleID, BOOL bForce)
{
	int	j;
	XXXServer &s = *m_Parent;
	XXXGroup *group;
	ULONG ref;

	// find the group with the passed ServerHandle
	//
	for(j=0; j<N_GRPS; j++)
	{
		if(s.m_groups[j].inuse && 
			(groupHandleID == s.m_groups[j].pGroup->m_ServerGroupHandle)) 
			break;
	}
	if(j >= N_GRPS)
		return E_FAIL;
	group = s.m_groups[j].pGroup;


	EnterCriticalSection(&CritSec);

	// Do the bForce logic (which is bad COM style...)
	//
	if(bForce) CoDisconnectObject(group, 0);

	// release our own reference to it
	// (which will delete it if the reference goes to 0)
	// and mark the slot unused
	//

	ref = group->Release();

	// and then make sure it gets deleted
	// (theorectically the CoDisconnect plus the local release should have taken care of it),
	//
	if(bForce && ref)
	{
		ULONG temp = ref;
		while (temp) 
			temp = group->Release();
	}

	// In any case the group is no longer available for use.
	//
	s.m_groups[j].inuse = 0;
	s.m_groups[j].pGroup = 0;

	LeaveCriticalSection(&CritSec);

	// We return OPC_S_INUSE if group is (or was) still referenced
	// we return this even if bForce is set as a 'warning' to the caller.
	// (Note the spec is ambiguous on this)
	//
	if (ref == 0) return S_OK;
	return OPC_S_INUSE;
}


///////////////////////////////////////
// IXXXServer::CreateGroupEnumerator()
//
///////////////////////////////////////
STDMETHODIMP IXXXServer::CreateGroupEnumerator( 
			OPCENUMSCOPE dwScope,
			REFIID riid,
			LPUNKNOWN *ppUnk
			)
{
	if ( riid == IID_IEnumUnknown)
	{
		// Note this logic is also used in XXXServer::QueryInterface
		IXXXEnumUnknown *temp;
		LPUNKNOWN * GroupList;
		int	GroupCount;
		HRESULT hr;

		// Get a snapshot of the group list 
		// (Note this does NOT do AddRefs to the groups - but see IXXXEnum below)
		//
		m_Parent->GetUnkList( dwScope, &GroupList, &GroupCount);

		// Create the Enumerator using the snapshot
		// Note that the enumerator will AddRef the server 
		// and also all of the groups.
		//
		temp = new IXXXEnumUnknown(m_Parent, GroupCount, GroupList, pIMalloc);
		m_Parent->FreeUnkList(GroupList, GroupCount);

		// Then QI for the interface ('temp') actually is the interface
		// but QI is the 'proper' way to get it.
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
	else if ( riid == IID_IEnumString)
	{
		IXXXEnumString *temp;
		LPOLESTR * GroupList;
		int	GroupCount;
		HRESULT hr;

		// Get a snapshot of the group list 
		//
		m_Parent->GetNameList( dwScope, &GroupList, &GroupCount);

		// Create the Enumerator using the snapshot
		// Note that the enumerator will AddRef the server 
		//
		temp = new IXXXEnumString(m_Parent, GroupCount, GroupList, pIMalloc);
		m_Parent->FreeNameList(GroupList, GroupCount);

		if ( temp == NULL)
			return E_OUTOFMEMORY;

		// Then QI for the interface ('temp') actually is the interface
		// but QI is the 'proper' way to get it.
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

	return E_NOINTERFACE;		//acc1099
}



