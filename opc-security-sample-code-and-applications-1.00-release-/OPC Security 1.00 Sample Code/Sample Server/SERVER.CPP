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
// 2.02  05/28/99 dj    enhance with OPC security
// 1.00  08/03/00 dj    security spec release
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCTHRD.h"
#include "objbase.h"
// %OPC_SEC_MODIFIED%
#include <stdio.h>
extern HINSTANCE	serverInstance;
#define showError(pszText)  ::MessageBox(NULL, pszText, _T("OPC Sample Server"), MB_ICONERROR | MB_OK)
// %END_OPC_SEC_MODIFIED%

extern CRITICAL_SECTION	CritSec;

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
// %OPC_SEC_MODIFIED%
	m_pISecurityPrivate = NULL;
	m_pISecurityNT = NULL;
    HRESULT hr;
    LPCTSTR pszWho;
    TCHAR   szError[200];
    TCHAR   szBase[_MAX_PATH + 1];
    TCHAR   szConfig[_MAX_PATH + 1];
    ::GetModuleFileName(serverInstance, szBase, sizeof(szBase)/sizeof(TCHAR));
    LPTSTR psz = _tcsrchr(szBase, _T('\\'));
    if (psz != NULL) {
        *psz = _T('\0');
    }
    _tcscpy(szConfig, szBase);
    _tcscat(szConfig, _T("\\acl.ini"));

    // init and build an ACL for secured objects:
    hr = m_accessCheck.Initialize();
							// mfr
	m_UsePrivSec = FALSE;	// set active security to NT

    if (FAILED(hr)) {
        _stprintf(szError, _T("Cannot initialize access check: 0x%08X\n"), hr);
        showError(szError);
    }

    FILE *hACLs = _tfopen(szConfig, _T("r"));
    if (hACLs != NULL)
    {
        // either read ACL from server's config file ...
        TCHAR   szLine[200];
        while (!feof(hACLs))
        {
            if (_fgetts(szLine, sizeof(szLine)/sizeof(TCHAR), hACLs) == NULL) {
                break;
            }
            // trim whitespaces at beginning of line
            LPTSTR pszLine = szLine;
            while ((*pszLine != 0) && _istspace(*pszLine)) {
                pszLine++;
            }
            if ((*pszLine == 0) || (*pszLine == _T('#')) || (*pszLine == _T(';'))) {
                // commented line, skip it
                continue;
            }
            // got ACL line, split and decode...
            LPCTSTR pszVerb = _tcstok(pszLine, _T(" \t="));
            if (pszVerb == NULL) {
                // bad config line, skip to next
                continue;
            }
            pszWho = _tcstok(NULL, _T(" \t="));
            if (pszWho == NULL) {
                // bad config line, skip to next
                continue;
            }
            DWORD dwAccess = 0;
            LPCTSTR pszAccess = _tcstok(NULL, _T(" \t=\n"));
            if (pszAccess == NULL) {
                dwAccess = SEC_ACCESS_READ | SEC_ACCESS_WRITE;
            }
            else
            {
                for (int i = 0; (*pszAccess != 0) && i < 2; i++)
                {
                    if ((*pszAccess == _T('R')) || (*pszAccess == _T('r'))) {
                        dwAccess |= SEC_ACCESS_READ;
                    }
                    if ((*pszAccess == _T('W')) || (*pszAccess == _T('w'))) {
                        dwAccess |= SEC_ACCESS_WRITE;
                    }
                    pszAccess++;
                }
            }
            // .. and add as deny or allow entry to ACL
            if (_tcsicmp(pszVerb, _T("ALLOW")) == NULL)
            {
                hr = m_accessCheck.AllowAccess(pszWho, dwAccess);
                if (FAILED(hr)) {
                    _stprintf(szError, _T("Cannot allow access for '%s': 0x%08X\n"), pszWho, hr); 
                    showError(szError);
                }
            }
            else if (_tcsicmp(pszVerb, _T("DENY")) == NULL)
            {
                hr = m_accessCheck.DenyAccess(pszWho, SEC_ACCESS_READ);
                if (FAILED(hr)) {
                    _stprintf(szError, _T("Cannot allow access for '%s': 0x%08X\n"), pszWho, hr); 
                    showError(szError);
                }
            }
            else {
                // unknown verb!!
                _stprintf(szError, _T("Unknown verb in ACL.ini: '%s'\n"), pszVerb); 
                showError(szError);
            }
        }
        fclose(hACLs);
    }
    else
    {
        // ... or initialize ACL with hard-coded ACL
        // add default user to access check list
        pszWho = _T("opc1");
        hr = m_accessCheck.AllowAccess(pszWho, SEC_ACCESS_READ);
        if (FAILED(hr)) {
            _stprintf(szError, _T("Cannot allow access for '%s': 0x%08X\n"), pszWho, hr);
            showError(szError);
        }
    }
// %END_OPC_SEC_MODIFIED%

	GetSystemTime(&SystemTime);		// Get current UTC Time
	SystemTimeToFileTime(&SystemTime, &mLastUpdate); // and store it

	m_pfnDestroy = pfn;

	m_tid = 0;
	m_LCID = 0;		//This sample code does not support LCID

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
// %OPC_SEC_MODIFIED%
	if(m_pISecurityNT) delete m_pISecurityNT;
	if(m_pISecurityPrivate) delete m_pISecurityPrivate;
// %END_OPC_SEC_MODIFIED%

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
// %OPC_SEC_MODIFIED%
	else if (iid == IID_IOPCSecurityNT)
	{
		if(m_pISecurityNT == 0) m_pISecurityNT = new IXXXSecurityNT(this);
		*ppInterface = m_pISecurityNT;
	}
	else if (iid == IID_IOPCSecurityPrivate)
	{
		if(m_pISecurityPrivate == 0) m_pISecurityPrivate = new IXXXSecurityPrivate(this);
		*ppInterface = m_pISecurityPrivate;
	}
// %END_OPC_SEC_MODIFIED%
	else if (iid == IID_IOPCBrowseServerAddressSpace)
	{
		if(m_pIBSAS == 0) m_pIBSAS = new IXXXBSAS(this, pIMalloc);
		*ppInterface = m_pIBSAS;
	}
	else if ( iid == IID_IConnectionPointContainer)		//V2.0
	{
		// Create the CP for IOPCShutdown and the CPC to hold it
		// (if we have not already done so)
		//
		if(m_pCPC == 0) 
		{
			IXXXCP * pCP;

			// Also Create the ConnectionPoint itself
			// Note the CP is a separate object, 
			// not a tear off interface
			//
			pCP = new IXXXCP(m_pCPC, IID_IOPCShutdown);
			// later check for error/null...

			// Now create the CPC (which will addref the CP)
			// (this is the actual tear-off we will return)
			//
			m_pCPC = new IXXXCPC(this, IID_IOPCShutdown , pCP);
			// later check for error/null...
		}
		*ppInterface = m_pCPC;

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
// Define a sample list of available ITEMIDs for GetAddressList/Browse.
// Note that in this sample code, AddItems and ValidateItems do NOT
// check against this list. In a 'real' server they would need to do so.
//
#define N_TAGS	9
static LPOLESTR tags[N_TAGS] = 
{
	L"FIC101",
	L"FIC102",
	L"FIC103",
	L"FIC104",
	L"FIC105",
	L"FIC106",
	L"FIC107",
	L"FIC108",
	L"FIC109"
};

///////////////////////////////////////
// XXXServer::GetAddressList()
// Create a list of all the group names
//
void XXXServer::GetAddressList( 
	OPCBROWSETYPE dwBrowseFilterType,
	LPCWSTR szFilterCriteria,
	VARTYPE vtDataTypeFilter,
	DWORD dwAccessRightsFilter,
	LPOLESTR**AddressList, 
	int *AddressCount)
{
	int	j, count = N_TAGS;
	LPOLESTR *mylist;

	// Defaults...
	*AddressList = NULL;
	*AddressCount = 0;

	*AddressList = mylist = new LPOLESTR[count];	// Caller must free this!
	if (mylist == NULL)		//acc082598
	{
		return;
	}
	for (j=0; j<count; j++)
	{
		// Note that the string is cloned to keep the enumerator 'stable'
		mylist[j] = WSTRClone( tags[j], NULL);
	}
	*AddressCount = count;
}

///////////////////////////////////////
// XXXServer::FreeAddressList()
// Free a list of Item names
//
void XXXServer::FreeAddressList( LPOLESTR*AddressList, int count)
{
	int j;
	
	if(!count) return;

	for(j=0; j<count; j++)
	{
		delete [] AddressList[j];
	}
	delete [] AddressList;
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


// %OPC_SEC_MODIFIED%

///////////////////////////////////////
// OPC security: update cache with new user credentials
HRESULT XXXServer::ChangeUser(void)
{
    return m_accessCheck.ChangeUser();
}

// OPC security: check caller's credentials for requested access
HRESULT XXXServer::CheckAccessRights(enum reqAccess access)
{
    HRESULT hr = S_FALSE;
    HRESULT hrRet = E_ACCESSDENIED;
    DWORD dwRequestedAccess;
	BOOL bAccessOK;

 
	if ( m_UsePrivSec ) // use NT or private security 
	{
	    if (access == ReadItem )
			dwRequestedAccess = SEC_ACCESS_READ;
		else
			dwRequestedAccess = SEC_ACCESS_WRITE;

		hr = m_PrivAccessCheck.IsAccessOK(dwRequestedAccess, bAccessOK);
	}
	else
	{
	    switch (access) 
		{
			case ReadItem:
				dwRequestedAccess = SEC_ACCESS_READ;
				break;

			default:
				hr = E_ACCESSDENIED;
				break;
		}

	    if (FAILED(hr))
			return hr;  // unknown access or denied for everybody
		
		// currently we only have one SD or accessCheck instance for all read accesses!!!
		// could very well be a array of access checks for different secured objects
		hr = m_accessCheck.IsAccessOK(dwRequestedAccess, bAccessOK);
	}


	if (FAILED(hr)) 
		return hr;
	
    if (bAccessOK) 
	{
        hrRet = S_OK;
    }
    else 
	{
        hrRet = E_ACCESSDENIED;
    }
    return FAILED(hr) ? hr : hrRet;
}



//mfr
// OPC security: check caller's credentials for requested access
BOOL XXXServer::SecPrivIsAvailable ()
{
	return m_PrivAccessCheck.GetSecurityEnable();
}

STDMETHODIMP XXXServer::SecPrivLogon(LPCWSTR szUserID, LPCWSTR szPassword)
{

    if ( m_PrivAccessCheck.LogOnUser (szUserID, szPassword) )
	{
		return S_OK;
	}
	return E_FAIL;
}

//mfr
// OPC security: check caller's credentials for requested access
STDMETHODIMP XXXServer::SecPrivLogoff()
{
    m_PrivAccessCheck.LogOffUser ();
	return S_OK;
}

// %END_OPC_SEC_MODIFIED%
