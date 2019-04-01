// OPCCommon.cpp: implementation of the COPCCommon class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "OPCBClient.h"
#include "OPCCommon.h"

#include "utility.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCCommon::COPCCommon()
{
   m_pIOPCCommon = NULL;
}


COPCCommon::~COPCCommon()
{
   if (m_pIOPCCommon)
      m_pIOPCCommon->Release();
}


HRESULT COPCCommon::Initialize(IUnknown* pOPCBatchServer)
{
   // initialize member pointer to IOPCServer interface.
   HRESULT hr = pOPCBatchServer->QueryInterface(IID_IOPCCommon, (void**)&m_pIOPCCommon);
   if (FAILED(hr))
	{
      m_pIOPCCommon = NULL;   
	   DisplayOutput(ERROR_OUT, _T("Unable to get IOPCCommon interface. Error = %s"), FormatHresult(hr));
	}
   return hr;
}




//////////////////////////////////////////////////////////////////////
// Method Test Functions
//////////////////////////////////////////////////////////////////////



HRESULT COPCCommon::Try_SetLocaleID()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCommon::Try_SetLocaleID."));

   HRESULT hr;
	LCID	lcid;

	lcid = MAKELCID( MAKELANGID(LANG_ENGLISH, SUBLANG_DEFAULT) , SORT_DEFAULT );
	hr = m_pIOPCCommon->SetLocaleID( lcid );
	if (FAILED(hr)) 
		DisplayOutput(ERROR_OUT, _T("IOPCCommon::SetLocaleID() Error = %s"), FormatHresult(hr));
	else 
		DisplayOutput(NORMAL_OUT, _T("IOPCCommon::SetLocaleID() to %ld - OK"), lcid);	
   return hr;
}


HRESULT COPCCommon::Try_GetLocaleID()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCommon::Try_GetLocaleID."));

   HRESULT hr;
	LCID	lcid;

	hr = m_pIOPCCommon->GetLocaleID(&lcid);
	if (FAILED(hr)) 
		DisplayOutput(ERROR_OUT, _T("Error from IOPCCommon::GetLocaleID Error = %s"), FormatHresult(hr));
	else 
		DisplayOutput(NORMAL_OUT, _T("IOPCCommon::GetLocaleID - OK, ID = %d"), (long)lcid );

   return hr;
}


HRESULT COPCCommon::Try_QueryAvailableLocaleIDs()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCommon::Try_QueryAvailableLocaleIDs."));

   HRESULT hr;
	DWORD dwCount, *pList;

	hr = m_pIOPCCommon->QueryAvailableLocaleIDs( &dwCount, &pList );
	if (FAILED(hr)) 
		DisplayOutput(ERROR_OUT, _T("IOPCCommon::QueryAvailableLocaleIDs() Error = %s"), FormatHresult(hr));
	else 
		DisplayOutput(NORMAL_OUT, _T("IOPCCommon::QueryAvailableLocaleIDs() returned %ld IDs"), dwCount);

	for( DWORD i = 0; i < dwCount; i++ )
		DisplayOutput(NORMAL_OUT, _T("IOPCCommon::QueryAvailableLocaleIDs() ID[%ld] = %ld"),
		i, pList[i]);


   return hr;
}

HRESULT COPCCommon::Try_GetErrorString()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCommon::Try_GetErrorString."));

   HRESULT hr;
	LPWSTR s = 0;
	TCHAR	s_keep[256];
	
	s_keep[0] = 0;
	int index = 0;
	while( index >= 0 )
	{
		hr = m_pIOPCCommon->GetErrorString(index, &s);
		if (FAILED(hr)) 
		{
			if( index == 0)
				DisplayOutput(ERROR_OUT, _T("IOPCCommon::GetErrorString() Error = %s"), FormatHresult(hr));
			index = -1;
		}
		else 
		{
			if( _tcscmp( (TCHAR *) s, s_keep ) == 0 )
			{
				index = -1;
			}
			else
			{
				DisplayOutput(NORMAL_OUT, _T("IOPCCommon::GetErrorString[%d] = \"%ls\""), index, s);
				index++;
				_tcscpy( s_keep, (TCHAR *) s );
			}
			((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(s);
		}
	}
   return hr;
}

HRESULT COPCCommon::Try_SetClientName()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCommon::Try_SetClientName."));

   HRESULT hr;
	
	hr = m_pIOPCCommon->SetClientName((LPCWSTR)_T("OPCBTestClient"));
	if (FAILED(hr)) 
		DisplayOutput(ERROR_OUT, _T("IOPCCommon::SetClientName() Error = %s"), FormatHresult(hr));
	else 
		DisplayOutput(NORMAL_OUT, _T("IOPCCommon::SetClientName() to \"OPCBTestClient\" - OK"));

   return hr;
}


BOOL COPCCommon::IsAvailable()
{
	return (m_pIOPCCommon != NULL );
}
