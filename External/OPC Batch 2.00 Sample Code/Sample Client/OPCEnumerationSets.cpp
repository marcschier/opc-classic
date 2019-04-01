// OPCEnumerationSets.cpp: implementation of the COPCEnumerationSets class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "OPCBClient.h"
#include "OPCEnumerationSets.h"

#include "utility.h"
#include "EditDlg.h"          // simple edit dialog to get name

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCEnumerationSets::COPCEnumerationSets()
{
   m_pIOPCEnumerationSets  = NULL;
   m_pIMalloc              = NULL;
}

COPCEnumerationSets::~COPCEnumerationSets()
{
   CleanUp();
}


void COPCEnumerationSets::CleanUp()
{
   if (m_pIOPCEnumerationSets) m_pIOPCEnumerationSets->Release();
   if (m_pIMalloc)            m_pIMalloc->Release();

};


HRESULT COPCEnumerationSets::Initialize(IUnknown* pOPCBatchServer)
{
   CleanUp();

   // initialize member pointer to IOPCServer interface.
   HRESULT hRes = pOPCBatchServer->QueryInterface(IID_IOPCEnumerationSets, (void**)&m_pIOPCEnumerationSets);
   if (FAILED(hRes)) {
      m_pIOPCEnumerationSets = NULL;   
   }
   else
   {
	   // Also get access to the COM memory manager
	   //
	   hRes = CoGetMalloc(MEMCTX_TASK, &m_pIMalloc);

	   if (FAILED(hRes))
	   {
		   DisplayOutput(ERROR_OUT, _T("GetMalloc failed. %s "), FormatHresult(hRes));
	   }
   }

   return hRes;
}



HRESULT COPCEnumerationSets::Try_QueryEnumerationSets() {

   DisplayOutput(NORMAL_OUT, _T("COPCEnumerationSets::QueryEnumerationSets."));

   DWORD dwCount;
   DWORD* pdwEnumSetID;
   LPWSTR* pszEnumSetName;
   HRESULT hRes;

   hRes = m_pIOPCEnumerationSets->QueryEnumerationSets(&dwCount, &pdwEnumSetID, &pszEnumSetName);
   if (FAILED(hRes)) {
      DisplayOutput(ERROR_OUT, _T("QueryEnumerationsSets Failed: %s"), FormatHresult(hRes));
   }
   else {
      DisplayOutput(VERBOSE_OUT, _T("Count:%d"), dwCount);

      for (ULONG i = 0; i < dwCount; i++) {
         DisplayOutput(VERBOSE_OUT, _T("SetID:%d, Name:%ls."), pdwEnumSetID[i], pszEnumSetName[i]);
      }
     

      for (DWORD j = 0; j < dwCount; j++)
      {
         m_pIMalloc->Free(pszEnumSetName[j]);
      }
      m_pIMalloc->Free(pszEnumSetName);
      m_pIMalloc->Free(pdwEnumSetID);

   }


   return hRes;
};



HRESULT COPCEnumerationSets::Try_QueryEnumeration(){

   DisplayOutput(NORMAL_OUT, _T("COPCEnumerationSets::QueryEnumeration."));

   
	DWORD dwEnumSetID;
	DWORD dwEnumValue;
   LPWSTR pszEnumName;


   // get Enum set ID 
   CEditDlg dlg(_T("Enum Set ID"), _T("Please enter Enum Set ID:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;

   dwEnumSetID = atol(dlg.m_strEdit);

   // get value to query for
   dlg.Init(_T("Query Value"), _T("Please enter Enum Value to Query:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;

   dwEnumValue = atol(dlg.m_strEdit);


   HRESULT hRes = m_pIOPCEnumerationSets->QueryEnumeration( dwEnumSetID,  dwEnumValue, &pszEnumName);
   if (FAILED(hRes))
   {
      DisplayOutput(ERROR_OUT, _T("QueryEnumeration Failed: %s"), FormatHresult(hRes));
   }
   else
   {
      DisplayOutput(NORMAL_OUT, _T("Set: %d, Value: %d, Name: %ls."), dwEnumSetID,  dwEnumValue, pszEnumName);

      m_pIMalloc->Free(pszEnumName);
   }


   return S_OK;
};


HRESULT COPCEnumerationSets::Try_QueryEnumerationList(){

   DisplayOutput(NORMAL_OUT, _T("COPCEnumerationSets::QueryEnumerationList."));

   
	DWORD dwEnumSetID;
	
   DWORD dwCount;
   DWORD* pdwEnumValue;
   LPWSTR* pszEnumString; //$TODO DO WE REALLY NEED "LPWSTR*" pszEnumString; LPWSTR pszEnumString s/b sufficient.


   // get Enum set ID 
   CEditDlg dlg(_T("Enum Set ID"), _T("Please enter Enum Set ID:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;

   dwEnumSetID = atol(dlg.m_strEdit);


   HRESULT hRes = m_pIOPCEnumerationSets->QueryEnumerationList( dwEnumSetID,  &dwCount, &pdwEnumValue, &pszEnumString);
   if (FAILED(hRes))
   {
      DisplayOutput(ERROR_OUT, _T("QueryEnumerationList Failed: %s"), FormatHresult(hRes));
   }
   else
   {
      DisplayOutput(NORMAL_OUT, _T("Found: %d enumerations."), dwCount);

      DisplayOutput(VERBOSE_OUT, _T("Value, EnumString"));
      for (DWORD i = 0; i < dwCount; i++)
         DisplayOutput(VERBOSE_OUT, _T("%d, %ls"), pdwEnumValue[i], pszEnumString[i]);


      for (DWORD j = 0; j < dwCount; j++)
      {
         m_pIMalloc->Free(pszEnumString[j]);
      }
      m_pIMalloc->Free(pdwEnumValue);
      m_pIMalloc->Free(pszEnumString);

   
   }




   return S_OK;
};
