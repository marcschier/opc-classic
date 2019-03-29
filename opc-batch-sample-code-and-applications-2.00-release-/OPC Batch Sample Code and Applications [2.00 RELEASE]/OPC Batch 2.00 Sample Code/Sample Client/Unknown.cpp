// CUnknown.cpp: implementation of the CUnknown class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "OPCBClient.h"
#include "Unknown.h"

#include "utility.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

CUnknown::CUnknown()
{
   m_pIUnknown = NULL;
}

CUnknown::~CUnknown()
{
   CleanUp();
}

void CUnknown::CleanUp()
{
 if (m_pIUnknown)
    m_pIUnknown->Release();

};


HRESULT CUnknown::Initialize(IUnknown* pOPCBatchServer)
{
   CleanUp();

   // initialize member pointer to IOPCServer interface.
   HRESULT hRes = pOPCBatchServer->QueryInterface(IID_IUnknown, (void**)&m_pIUnknown);
   if (FAILED(hRes))
      m_pIUnknown = NULL;   

   return hRes;
}




//////////////////////////////////////////////////////////////////////
// Method Test Functions
//////////////////////////////////////////////////////////////////////

HRESULT CUnknown::Try_QueryInterface()
{
   DisplayOutput(VERBOSE_OUT, _T("CUnknown::Try_QueryInterface."));

   if (m_pIUnknown == NULL) {
      DisplayOutput(ERROR_OUT, _T("Failed calling AddRef. IUnknown pointer already released."));
      return E_FAIL;
   }

   HRESULT hRes = m_pIUnknown->QueryInterface(IID_IUnknown, (void**)&m_pIUnknown);
   if (FAILED(hRes)) {
      m_pIUnknown = NULL;   
      DisplayOutput(ERROR_OUT, _T("Failed calling QueryInterface. %s"), FormatHresult(hRes));
   }
   else
      DisplayOutput(NORMAL_OUT, _T("Successfully called QueryInterface."));

   return hRes;
};


HRESULT CUnknown::Try_AddRef(){

   DisplayOutput(VERBOSE_OUT, _T("CUnknown::Try_AddRef."));

   if (m_pIUnknown == NULL) {
      DisplayOutput(ERROR_OUT, _T("Failed calling AddRef. IUnknown pointer already released."));
      return E_FAIL;
   }


   ULONG count = m_pIUnknown->AddRef();
   if (count <= 0) {     // should never happen
      DisplayOutput(ERROR_OUT, _T("Failed calling AddRef."));
      return E_FAIL;
   }
   else
      DisplayOutput(NORMAL_OUT, _T("Successfully called AddRef. Count:%d"), count);

   return S_OK;
};


HRESULT CUnknown::Try_Release()
{
   DisplayOutput(VERBOSE_OUT, _T("CUnknown::Try_Release."));

   if (m_pIUnknown == NULL) {
      DisplayOutput(ERROR_OUT, _T("Failed calling Release. IUnknown pointer already released."));
      return E_FAIL;
   }


   ULONG count = m_pIUnknown->Release();
   if (count <= 0) {     
      m_pIUnknown = NULL;
   }
   else
      DisplayOutput(NORMAL_OUT, _T("Successfully called Release. Count:%d"), count);

   return S_OK;
};
