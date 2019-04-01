//////////////////////////////////////////////////////////////////////
// OPCServer.cpp: implementation of the COPCServer class.
//

#include "stdafx.h"
#include "OPCServer.h"

#include "utility.h"
#include "AFXPRIV.H"          // conversions

OPCHANDLE Try_GetGroupState(IOPCGroupStateMgt * pGRP);



#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCServer::COPCServer()
{
   m_pIOPCServer  = NULL;
   m_pIMalloc     = NULL;
   m_pGroup       = NULL;

   m_pGRP1GSM     = NULL;
   m_pGRP1PGSM    = NULL;
   m_pGRP1SIO     = NULL;
   m_pGRP1ASIO    = NULL;
   m_pGRP1ASIO2   = NULL;
   m_pGRP1IM      = NULL;
   m_pGRP1DO      = NULL;
   m_pGRP1CPC     = NULL;

   m_pGRP2        = NULL;




}

COPCServer::~COPCServer()
{
   CleanUp();
}

void COPCServer::CleanUp()
{
   if (m_pIOPCServer)   m_pIOPCServer->Release();

   if (m_pIMalloc)      m_pIMalloc->Release();

   if (m_pGRP1GSM)      m_pGRP1GSM->Release();
   if (m_pGRP1PGSM)     m_pGRP1PGSM->Release();
   if (m_pGRP1SIO)      m_pGRP1SIO->Release();
   if (m_pGRP1ASIO)     m_pGRP1ASIO->Release();
   if (m_pGRP1ASIO2)    m_pGRP1ASIO2->Release();
   if (m_pGRP1IM)       m_pGRP1IM->Release();
   if (m_pGRP1DO)       m_pGRP1DO->Release();
   if (m_pGRP1CPC)      m_pGRP1CPC->Release();

   if (m_pGRP2)         m_pGRP2->Release();

};


HRESULT COPCServer::Initialize(IUnknown* m_pIOPCServerBatchServer)
{
   CleanUp();

   // initialize member pointer to IOPCServer interface.
   HRESULT hRes = m_pIOPCServerBatchServer->QueryInterface(IID_IOPCServer, (void**)&m_pIOPCServer);
   if (FAILED(hRes)) {
	   DisplayOutput(ERROR_OUT, _T("Unable to get IOPCServer interface. %s"), FormatHresult(hRes));
      m_pIOPCServer = NULL;   
   }
   else
   {
	   // Also get access to the COM memory manager
	   //
	   hRes = CoGetMalloc(MEMCTX_TASK, &m_pIMalloc);

	   if (FAILED(hRes))
	   {
		   DisplayOutput(ERROR_OUT, _T("GetMalloc failed. %s"), FormatHresult(hRes));
	   }
   }

   return hRes;
}




//////////////////////////////////////////////////////////////////////
// Method Test Functions
//////////////////////////////////////////////////////////////////////

HRESULT COPCServer::Try_CreateGroupEnumerator()
{

   DisplayOutput(VERBOSE_OUT, _T("COPCServer::Try_CreateGroupEnumerator."));


	HRESULT hRes1;
	IEnumUnknown * pEnum;

	hRes1 = m_pIOPCServer->CreateGroupEnumerator(OPC_ENUM_ALL, IID_IEnumUnknown, (IUnknown**)&pEnum);
	if (FAILED(hRes1))
	{
      DisplayOutput(ERROR_OUT, _T("Error from CreateGroupEnumerator for EnumUnknown () %s"), FormatHresult(hRes1));
	} 
	else
	{
		HRESULT hRes2 = S_OK;
		IUnknown *pUnk;
		IOPCGroupStateMgt * pGRP;
		ULONG actual;

		DisplayOutput(NORMAL_OUT, _T("Got an IEnumUnknown from CreateGroupEnumerator()."));

		while((hRes2 = pEnum->Next(1, &pUnk, &actual)) == S_OK)
		{
			HRESULT hRes3;

			hRes3 = pUnk->QueryInterface(IID_IOPCGroupStateMgt, (void**)&pGRP);
			
			if (FAILED(hRes3))
			{
				DisplayOutput(ERROR_OUT, _T("Error from QI after Next() %s"), FormatHresult(hRes3));
			}
			else
			{
            Try_GetGroupState(pGRP);
            pGRP->Release();
			}

			pUnk->Release();
		}
		if (FAILED(hRes2))
		{
			DisplayOutput(ERROR_OUT,_T("Error from Next() %s"), FormatHresult(hRes2));
		}

		// Release the interface
		//
		pEnum->Release();
	}


   return S_OK;
};



HRESULT COPCServer::Try_AddGroup()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCServer::Try_AddGroup."));

	HRESULT hRes;
	DWORD	RevisedRate1;
	DWORD	RevisedRate2;
	float	DeadBand1 = (float)0.0;
	float	DeadBand2 = (float)0.0;
	HRESULT r2, r3, r4, r5, r6, r8;
	HRESULT r9, r5a;

	// Try to create 2 groups 
	//
	if(m_pGroup == NULL)
	{
		DisplayOutput(VERBOSE_OUT, _T("First time through: Creating groups."));

		hRes = m_pIOPCServer->AddGroup( L"TestGroup1", TRUE, 0, 0, 0, &DeadBand1,
			0, &m_hServerGroup1, &RevisedRate1, IID_IUnknown, 
			(LPUNKNOWN*)&m_pGroup);

		if (FAILED(hRes))
		{
			DisplayOutput(ERROR_OUT, _T("Error from AddGroup (TestGroup1) () %s"), FormatHresult(hRes));
		} 
		else
		{
			DisplayOutput(VERBOSE_OUT, _T("Checking Interfaces on Group 1."));

			r2  = m_pGroup->QueryInterface(IID_IOPCGroupStateMgt, (void**)&m_pGRP1GSM);
			r3  = m_pGroup->QueryInterface(IID_IOPCPublicGroupStateMgt, (void**)&m_pGRP1PGSM);
			r4  = m_pGroup->QueryInterface(IID_IOPCSyncIO, (void**)&m_pGRP1SIO);
			r5  = m_pGroup->QueryInterface(IID_IOPCAsyncIO, (void**)&m_pGRP1ASIO);
			r5a = m_pGroup->QueryInterface(IID_IOPCAsyncIO2, (void**)&m_pGRP1ASIO2);
			r6  = m_pGroup->QueryInterface(IID_IOPCItemMgt, (void**)&m_pGRP1IM);
			r8  = m_pGroup->QueryInterface(IID_IDataObject, (void**)&m_pGRP1DO);
			r9  = m_pGroup->QueryInterface(IID_IConnectionPointContainer, (void**)&m_pGRP1CPC);

			Report(VERBOSE_OUT, "IUnknown               ", S_OK);
			Report(VERBOSE_OUT, "IOPCGroupStateMgt      ", r2);
			Report(VERBOSE_OUT, "IOPCPublicGroupStateMgt", r3);
			Report(VERBOSE_OUT, "IOPCSyncIO             ", r4);
			Report(VERBOSE_OUT, "IOPCAsyncIO            ", r5);
			Report(VERBOSE_OUT, "IOPCAsyncIO2           ", r5a);
			Report(VERBOSE_OUT, "IOPCItemMgt            ", r6);
			Report(VERBOSE_OUT, "IDataObject            ", r8);
			Report(VERBOSE_OUT, "IConnectionPointContainer", r9);
		}
	}
	else
	{
		DisplayOutput(VERBOSE_OUT, _T("AddGroup already done."));
		return S_OK;
	}

	if( m_pGRP2 == 0)
	{
		r2 = m_pIOPCServer->AddGroup( L"TestGroup2", TRUE, 0, 0, 0, &DeadBand2,
			0, &m_hServerGroup2, &RevisedRate2, IID_IOPCGroupStateMgt, 
			(LPUNKNOWN*)&m_pGRP2);

		if (FAILED(r2))
		{
			DisplayOutput(ERROR_OUT, _T("Error from AddGroup (TestGroup2) () %s"), FormatHresult(r2));
		} 
	}
	DisplayOutput(VERBOSE_OUT, _T("AddGroup test complete."));

   return S_OK;
};


HRESULT COPCServer::Try_GetErrorString(){

   DisplayOutput(VERBOSE_OUT, _T("COPCServer::Try_GetErrorString."));

	HRESULT hRes;
	LPWSTR string = 0;

	// Invoke a method on the OPCServer interface
	//
	hRes = m_pIOPCServer->GetErrorString(0, 0, &string);

	if (FAILED(hRes))
	{
		DisplayOutput(ERROR_OUT, _T("Error from GetErrorString() %s"), FormatHresult(hRes));
	} 
	else
	{
		// And print some of the results of the method
		//
		DisplayOutput(ERROR_OUT, _T("GetErrorString returned \"%ls\"\n"), string);

		// Dont forget to release the memory returned by the method
		//
		m_pIMalloc->Free(string);
	}

   return S_OK;
};


HRESULT COPCServer::Try_GetGroupByName(){

   DisplayOutput(VERBOSE_OUT, _T("COPCServer::Try_GetGroupByName."));

	HRESULT   hRes;
	IOPCGroupStateMgt *pGRP;

   CString strGroupName;
   strGroupName = "TestGroup1";      //$TODO get group name from user

   USES_CONVERSION;  

	hRes = m_pIOPCServer->GetGroupByName(T2COLE(strGroupName), IID_IOPCGroupStateMgt, (IUnknown**)&pGRP);
	if (FAILED(hRes))
	{
		DisplayOutput(ERROR_OUT, _T("Error from GetGroupByName() %s"), FormatHresult(hRes));
	} 
	else
	{
      Try_GetGroupState(pGRP);
		pGRP->Release();
	}
	DisplayOutput(VERBOSE_OUT, _T("GetGroupByName test complete"));



   return S_OK;
};


HRESULT COPCServer::Try_RemoveGroup(){

   DisplayOutput(VERBOSE_OUT, _T("COPCServer::Try_RemoveGroup."));


	HRESULT hRes;
	// When done
	// Release the Group interface 
	// and remove the group from the server
	// (Which should take care of deleting any items)
	//
	if(!m_pGroup)
	{
		DisplayOutput(ERROR_OUT, _T("Add needs to be done first"));
		return S_OK;
	}

	if(m_pGRP1GSM) m_pGRP1GSM->Release();
	if(m_pGRP1PGSM) m_pGRP1PGSM->Release();
	if(m_pGRP1SIO) m_pGRP1SIO->Release();
	if(m_pGRP1ASIO) m_pGRP1ASIO->Release();
	if(m_pGRP1ASIO2) m_pGRP1ASIO2->Release();
	if(m_pGRP1IM) m_pGRP1IM->Release();
	if(m_pGRP1DO) m_pGRP1DO->Release();
	if(m_pGRP1CPC) m_pGRP1CPC->Release();

	m_pGRP1GSM = 0;
	m_pGRP1PGSM = 0;
	m_pGRP1SIO = 0;
	m_pGRP1ASIO = 0;
	m_pGRP1ASIO2 = 0;
	m_pGRP1IM = 0;
	m_pGRP1DO = 0;
	m_pGRP1CPC = 0;

	if(m_pGroup)
	{
		m_pGroup->Release();
		hRes = m_pIOPCServer->RemoveGroup(m_hServerGroup1, FALSE);
		if (FAILED(hRes))
		{
			DisplayOutput(ERROR_OUT, _T("Error from RemoveGroup (TestGroup1) () %s"), FormatHresult(hRes));
		} else 
         DisplayOutput(NORMAL_OUT, _T("Group 1 removed")); 
		m_pGroup = 0;
	}
	if(m_pGRP2)
	{
		m_pGRP2->Release();
		hRes = m_pIOPCServer->RemoveGroup(m_hServerGroup2, FALSE);
		if (FAILED(hRes))
		{
			DisplayOutput(ERROR_OUT, _T("Error from RemoveGroup (TestGroup2) () %s"), FormatHresult(hRes));
		} else 
         DisplayOutput(NORMAL_OUT, _T("Group 2 (empty) removed")); 
		m_pGRP2 = 0;
	}

   printf("Remove Groups test complete\n");


   return S_OK;
};


HRESULT COPCServer::Try_CreateEnumerator(){

   DisplayOutput(NORMAL_OUT, _T("IOPCServer::CreateEnumerator Not Implemented.  This is a new Batch Server method."));

   return E_NOTIMPL;
};


HRESULT COPCServer::Try_GetStatus()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCServer::Try_GetStatus."));

   if (!m_pIOPCServer){
      DisplayOutput(NORMAL_OUT, _T("Can't do IOPCServer::GetStatus."));
      return E_FAIL;
   }


	OPCSERVERSTATUS	*pss;
	HRESULT hRes1;

	// Invoke a method on the OPCServer interface
	//
	hRes1 = m_pIOPCServer->GetStatus(&pss);

	if (FAILED(hRes1))
	{
		DisplayOutput(ERROR_OUT, _T("Error from GetStatus() %s"), FormatHresult(hRes1));
	} 
	else
	{
      // output the status returned.
      DisplayOutput(VERBOSE_OUT, _T("ServerState:%d"), pss->dwServerState);
      DisplayOutput(VERBOSE_OUT, _T("GroupCount:%d, BandWidth:%d"), pss->dwGroupCount, pss->dwBandWidth);
      DisplayOutput(VERBOSE_OUT, _T("MajorVersion:%d, MinorVersion:%d, BuildNumber:%d"),
                    pss->wMajorVersion, pss->wMinorVersion, pss->wBuildNumber);
      DisplayOutput(VERBOSE_OUT, _T("wReserved:%d"), pss->wReserved);
		DisplayOutput(VERBOSE_OUT, _T("VendorInfo:%ls"), pss->szVendorInfo);

		// Dont forget to release the memory returned by the method
		//
		m_pIMalloc->Free(pss->szVendorInfo);
		m_pIMalloc->Free(pss);
	}

   return S_OK;
}





//---------------------------------------------------------
// TryGetGroupState
// Perform some functions
OPCHANDLE COPCServer::Try_GetGroupState(IOPCGroupStateMgt * pGRP)
{
   DisplayOutput(VERBOSE_OUT, _T("COPCServer::Try_GetGroupState."));

   HRESULT   hRes;
	DWORD     UpdateRate; 
	BOOL      Active;
	LPWSTR    pName;
	LONG      TimeBias;
	FLOAT     PercentDeadband;
	DWORD     LCID;
	OPCHANDLE hClientGroup;
	OPCHANDLE hServerGroup;

	// Get it's status
	//
	hRes = pGRP->GetState(&UpdateRate, &Active, 
			&pName, &TimeBias, &PercentDeadband,
			&LCID, &hClientGroup, &hServerGroup);

	if (FAILED(hRes))
	{
		DisplayOutput(ERROR_OUT, _T("Error from GetState() %s"), FormatHresult(hRes));
	} 
	else
	{
		// Print the name (to verify it worked)
		// And don't forget to Free the returned string!
		//
		DisplayOutput(VERBOSE_OUT, _T("GetState Succeeded for %ls"), pName);
		DisplayOutput(VERBOSE_OUT, _T("UpdateRate = %d, Active = %d, TimeBias=%d, Deadband=%5.2f"),
			UpdateRate, (DWORD)Active, TimeBias, PercentDeadband);
		DisplayOutput(VERBOSE_OUT, _T("LCID=%d ClientHandle=%d, ServerHandle=%d"), 
			LCID, hClientGroup, hServerGroup);
		
		m_pIMalloc->Free(pName);
	}


	return hServerGroup;
}
