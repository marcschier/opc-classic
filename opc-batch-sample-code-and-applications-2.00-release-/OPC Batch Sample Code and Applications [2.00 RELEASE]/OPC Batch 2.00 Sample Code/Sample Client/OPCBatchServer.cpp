//////////////////////////////////////////////////////////////////////
// OPCBatchServer.cpp: implementation of the COPCBatchServer class.
//
/////////////////////////////////////////////////////////////////////////////
// (c) Copyright 2000 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist
//  in understanding the OPC Specifications and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Dave Emerson and Jack Beaudry, Yokogawa Electric Company.
//  This application is based on the code for the sample DA client
//  generously provided by Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
// .4    01/12/00 jb    preliminary release for testing.
//

#include "stdafx.h"
#include "OPCBatchServer.h"

#include "utility.h"
#include "AFXPRIV.H"          // conversions


OPCHANDLE Try_GetGroupState(IOPCGroupStateMgt * pGRP);



#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

// Declare a global pointer to a batch summary object.
// There are two ways to get a pointer to this interface:
// IOPCBatchServer::CreateEnumerator and IOPCBatchServer2::CreateFilteredEnumerator.
// For the purposes of this sample code each of these
// will store the pointer it gets in this global, and
// then the test methods in CEnumOPCBatchSummary will
// refer to the interface using it.  This is adequate
// for the sample client but a real client might want
// to be able to support holding on to several distinct
// enumerators at once.
IEnumOPCBatchSummary * G_pIEnumOPCBatchSummary = NULL;

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCBatchServer::COPCBatchServer()
{
   m_pIOPCBatchServer  = NULL;
   m_pIMalloc     = NULL;
   m_pGroup       = NULL;

}

COPCBatchServer::~COPCBatchServer()
{
   CleanUp();
}

void COPCBatchServer::CleanUp()
{
   if (m_pIOPCBatchServer)       m_pIOPCBatchServer->Release();
   if (G_pIEnumOPCBatchSummary)  G_pIEnumOPCBatchSummary->Release(); //$REVIEW is this all that's necessary for an enumeration?

   if (m_pIMalloc)               m_pIMalloc->Release();

};


HRESULT COPCBatchServer::Initialize(IUnknown* pIOPCBatchServer)
{
   CleanUp();

   // initialize member pointer to IOPCBatchServer interface.
   HRESULT hRes = pIOPCBatchServer->QueryInterface(IID_IOPCBatchServer, (void**)&m_pIOPCBatchServer);
   if (FAILED(hRes)) {
	   DisplayOutput(ERROR_OUT, _T("Unable to get IOPCBatchServer interface. %s"), FormatHresult(hRes));
      m_pIOPCBatchServer = NULL;  
   }
   else
   {
	   // Also get access to the COM memory manager
	   //
	   hRes = CoGetMalloc(MEMCTX_TASK, &m_pIMalloc);

	   if (FAILED(hRes))
	   {
		   DisplayOutput(ERROR_OUT, _T("GetMalloc failed %s"), FormatHresult(hRes));
	   }
   }

   return hRes;
}



//////////////////////////////////////////////////////////////////////
// Method Test Functions
//////////////////////////////////////////////////////////////////////



HRESULT COPCBatchServer::Try_CreateEnumerator(){

   DisplayOutput(VERBOSE_OUT, _T("COPCBatchServer::Try_CreateEnumerator."));

   if (!m_pIOPCBatchServer){
      DisplayOutput(NORMAL_OUT, _T("Can't do IOPCBatchServer::CreateEnumerator."));
      return E_FAIL;
   }

   // if create already called then release previous enumeration and try again
   if (G_pIEnumOPCBatchSummary != NULL)
   {
      G_pIEnumOPCBatchSummary->Release();    //$REVIEW is this all that's necessary?
      G_pIEnumOPCBatchSummary = NULL;
   }

   IUnknown* pEnumUnknown;


   // get the IUnknown pointer for the enumerator interface
   HRESULT hRes = m_pIOPCBatchServer->CreateEnumerator(IID_IEnumOPCBatchSummary, &pEnumUnknown);

   if (FAILED(hRes))
   {
      DisplayOutput(ERROR_OUT, _T("IOPCBatchServer::CreateEnumerator Failed. %s"), FormatHresult(hRes));
      return E_FAIL;
   }

   // get the actual enumerator interface
   hRes = pEnumUnknown->QueryInterface(IID_IEnumOPCBatchSummary, (void**)&G_pIEnumOPCBatchSummary);
   if (FAILED(hRes))
   {
      DisplayOutput(ERROR_OUT, _T("Couldn't get IID_IEnumOPCBatchSummary. %s"), FormatHresult(hRes));
      pEnumUnknown->Release();
      return E_FAIL;
   }

   pEnumUnknown->Release();   // don't need the unknown pointer any longer


   return S_OK;
};


HRESULT COPCBatchServer::Try_GetDelimiter()
{

   DisplayOutput(VERBOSE_OUT, _T("COPCBatchServer::Try_GetDelimiter."));

   if (!m_pIOPCBatchServer){
      DisplayOutput(NORMAL_OUT, _T("Can't do IOPCBatchServer::GetDelimiter."));
      return E_FAIL;
   }

   LPWSTR pszDelimiter;

	HRESULT hRes1;

	// Invoke a method on the OPCBatchServer interface
	//
	hRes1 = m_pIOPCBatchServer->GetDelimiter(&pszDelimiter);

	if (FAILED(hRes1))
	{
		DisplayOutput(ERROR_OUT, _T("Error from GetDelimiter() %s"), FormatHresult(hRes1));
	} 
	else
	{
   	DisplayOutput(VERBOSE_OUT, _T("Delimiter:%ls"), pszDelimiter);

		// Dont forget to release the memory returned by the method
		//
		m_pIMalloc->Free(pszDelimiter);
	}

   return S_OK;
}


