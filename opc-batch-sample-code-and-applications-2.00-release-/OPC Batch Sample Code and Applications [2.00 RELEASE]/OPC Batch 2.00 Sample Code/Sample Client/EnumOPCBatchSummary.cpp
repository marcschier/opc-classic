// EnumOPCBatchSummary.cpp: implementation of the CEnumOPCBatchSummary class.
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
#include "OPCBClient.h"
#include "EnumOPCBatchSummary.h"

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

CEnumOPCBatchSummary::CEnumOPCBatchSummary()
{
   m_pIMalloc              = NULL;

   Initialize();
}

CEnumOPCBatchSummary::~CEnumOPCBatchSummary()
{
   CleanUp();
}

void CEnumOPCBatchSummary::CleanUp()
{
   if (m_pIMalloc)            m_pIMalloc->Release();

};


HRESULT CEnumOPCBatchSummary::Initialize()
{
   CleanUp();

   // Get access to the COM memory manager
	//
	HRESULT hRes = CoGetMalloc(MEMCTX_TASK, &m_pIMalloc);

	if (FAILED(hRes))
	{
		DisplayOutput(ERROR_OUT, _T("GetMalloc failed %s"), FormatHresult(hRes));
	}

   return hRes;
}



HRESULT CEnumOPCBatchSummary::Try_Next(IEnumOPCBatchSummary* pIEnumOPCBatchSummary)
{
   DisplayOutput(VERBOSE_OUT, _T("CEnumOPCBatchSummary::Try_Next."));

   if (pIEnumOPCBatchSummary == NULL) {
      DisplayOutput(ERROR_OUT, _T("CEnumOPCBatchSummary::Try_Next. Error: IEnumOPCBatchSummary pointer is NULL."));
      return E_INVALIDARG;
   }


   OPCBATCHSUMMARY* pBSum;
   ULONG ulFetched = NULL;
   
   // Get number of items to fetch
   CEditDlg dlg(_T("Number of Items"), _T("Please enter the number of items to fetch:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;

   DisplayOutput (VERBOSE_OUT, _T("User entered: %s"), dlg.m_strEdit); 


	HRESULT hRes = pIEnumOPCBatchSummary->Next(atol(dlg.m_strEdit), &pBSum, &ulFetched);
   
	if (FAILED(hRes))	{
		DisplayOutput(ERROR_OUT, _T("Error from Next:%s"), FormatHresult(hRes));
		return E_FAIL;
	}
   else {

		DisplayOutput(NORMAL_OUT, _T("Next returned: %d"), ulFetched);
		DisplayOutput(VERBOSE_OUT, _T("ID, Description, OPCItemID, BatchSize, EU, ExecState, ExecMode"));

      for (ULONG i = 0; i < ulFetched; i++)
      {
         //display batch summary. 
		   DisplayOutput(VERBOSE_OUT, _T("%ls, %ls, %ls, %7.1f, %ls, %ls, %ls"), 
            pBSum[i].szID, pBSum[i].szDescription, pBSum[i].szOPCItemID, pBSum[i].fBatchSize,
            pBSum[i].szEU, pBSum[i].szExecutionState, pBSum[i].szExecutionMode);

         // release memory
         m_pIMalloc->Free(pBSum[i].szID);
         m_pIMalloc->Free(pBSum[i].szDescription);
         m_pIMalloc->Free(pBSum[i].szOPCItemID);
         m_pIMalloc->Free(pBSum[i].szEU);
         m_pIMalloc->Free(pBSum[i].szExecutionState);
         m_pIMalloc->Free(pBSum[i].szExecutionMode);

      }



   	if(pBSum) m_pIMalloc->Free(pBSum);
   	

   }

 
   return S_OK;
};

HRESULT CEnumOPCBatchSummary::Try_Skip(IEnumOPCBatchSummary* pIEnumOPCBatchSummary)
{
   DisplayOutput(VERBOSE_OUT, _T("CEnumOPCBatchSummary::Try_Skip."));

   if (pIEnumOPCBatchSummary == NULL) {
      DisplayOutput(ERROR_OUT, _T("CEnumOPCBatchSummary::Try_Skip. Error: IEnumOPCBatchSummary pointer is NULL."));
      return E_INVALIDARG;
   }
 
   // Get number of items to skip
   CEditDlg dlg(_T("Number of Items"), _T("Please enter the number of items to fetch:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;


	HRESULT hRes = pIEnumOPCBatchSummary->Skip(atol(dlg.m_strEdit));

	if (FAILED(hRes))	{
		DisplayOutput(ERROR_OUT, _T("Error from Skip: %s"), FormatHresult(hRes));
		return E_FAIL;
	}
 
   return S_OK;
};

HRESULT CEnumOPCBatchSummary::Try_Reset(IEnumOPCBatchSummary* pIEnumOPCBatchSummary)
{
   DisplayOutput(VERBOSE_OUT, _T("CEnumOPCBatchSummary::Try_Reset."));

   if (pIEnumOPCBatchSummary == NULL) {
      DisplayOutput(ERROR_OUT, _T("CEnumOPCBatchSummary::Try_Reset. Error: IEnumOPCBatchSummary pointer is NULL."));
      return E_INVALIDARG;
   }

	HRESULT hRes = pIEnumOPCBatchSummary->Reset();

	if (FAILED(hRes))	{
		DisplayOutput(ERROR_OUT, _T("Error from Reset: %s"), FormatHresult(hRes));
		return E_FAIL;
	}

 
   return S_OK;
};

HRESULT CEnumOPCBatchSummary::Try_Clone(IEnumOPCBatchSummary* pIEnumOPCBatchSummary)
{
   DisplayOutput(VERBOSE_OUT, _T("CEnumOPCBatchSummary::Try_Clone."));
 
   if (pIEnumOPCBatchSummary == NULL) {
      DisplayOutput(ERROR_OUT, _T("CEnumOPCBatchSummary::Try_Clone. Error: IEnumOPCBatchSummary pointer is NULL."));
      return E_INVALIDARG;
   }

   IEnumOPCBatchSummary* pEnumBatchSummary;


	HRESULT hRes = pIEnumOPCBatchSummary->Clone(&pEnumBatchSummary);

	if (FAILED(hRes))	{
		DisplayOutput(ERROR_OUT, _T("Error from Clone: %s"), FormatHresult(hRes));
		return E_FAIL;
	}
   else {

      Try_Next(pEnumBatchSummary);

   	if(pEnumBatchSummary) m_pIMalloc->Free(pEnumBatchSummary);

   }

   return S_OK;
};

