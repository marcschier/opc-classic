//////////////////////////////////////////////////////////////////////
// OPCBatchServer2.cpp: implementation of the COPCBatchServer2 class.
//
/////////////////////////////////////////////////////////////////////////////
// (c) Copyright 2000-2001 The OPC Foundation
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
//  0.1   11/6/00  WGI   Copied with modifications from OPCBatchServer.cpp
//

#include "stdafx.h"
#include "OPCBClient.h"
#include "OPCBatchServer2.h"

#include "utility.h"
#include "AFXPRIV.H"          // conversions
#include "FilterDialog.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

// reference the global declared in OPCBatchServer.cpp
// Note the comment there.....
extern IEnumOPCBatchSummary * G_pIEnumOPCBatchSummary;

/////////////////////////////////////////////////////////////////////////////
// COPCBatchServer2

COPCBatchServer2::COPCBatchServer2()
{
   m_pIOPCBatchServer2  = NULL;
   m_pIMalloc     = NULL;
   m_pGroup       = NULL;

}

COPCBatchServer2::~COPCBatchServer2()
{
  CleanUp();
}

void COPCBatchServer2::CleanUp()
{
   if (m_pIOPCBatchServer2)       m_pIOPCBatchServer2->Release();

   if (m_pIMalloc)               m_pIMalloc->Release();

};


HRESULT COPCBatchServer2::Initialize(IUnknown* pIOPCBatchServer2)
{
   CleanUp();

   // initialize member pointer to IOPCBatchServer2 interface.
   HRESULT hRes = pIOPCBatchServer2->QueryInterface(IID_IOPCBatchServer2, (void**)&m_pIOPCBatchServer2);
   if (FAILED(hRes)) {
	   DisplayOutput(ERROR_OUT, _T("Unable to get IOPCBatchServer2 interface. %s"), FormatHresult(hRes));
      m_pIOPCBatchServer2 = NULL;  
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

HRESULT COPCBatchServer2::Try_CreateFilteredEnumerator(){

  USES_CONVERSION;

  DisplayOutput(VERBOSE_OUT, _T("COPCBatchServer2::Try_CreateFilteredEnumerator."));

  CFilterDialog filtDlg;
  OPCBATCHSUMMARYFILTER filter;
  LPWSTR pszModel;

  // Initialize the filter structure with some defaults.  Bad
  // things will happen (usually an access violation in the proxy/stub)
  // if the members are not initialized.
  filter.szID = A2W("");             // defaults to empty string => no filtering
  filter.szDescription = A2W("");
  filter.szOPCItemID = A2W("");
  filter.szMasterRecipeID = A2W("");
  filter.fMinBatchSize = -1e10f;
  filter.fMaxBatchSize = +1e10f;
  filter.szEU = A2W("");
  filter.szExecutionState = A2W("");
  filter.szExecutionMode  = A2W("");
  
  FILETIME ftMin = {0, 0};                  // the beginning of time
  FILETIME ftMax = {ULONG_MAX, ULONG_MAX};  // the end of time
  
  filter.ftMinStartTime = ftMin;
  filter.ftMaxStartTime = ftMax;
  filter.ftMinEndTime = ftMin;
  filter.ftMaxEndTime = ftMax;
  
	if( filtDlg.DoModal() == IDOK )
	{
    // copy the user's entries into the parameters that
    // will be passed to the server in CreateFilteredEnumerator
    filter.szID = A2W(filtDlg.m_sBatchIDFilter);
    filter.szDescription = A2W(filtDlg.m_sDescrFilter);
    filter.fMinBatchSize = filtDlg.m_fMinBatchSizeFilter;
    filter.fMaxBatchSize = filtDlg.m_fMaxBatchSizeFilter;

    pszModel = A2W(filtDlg.m_sModel);

    if (!m_pIOPCBatchServer2){
      DisplayOutput(NORMAL_OUT, _T("Can't do IOPCBatchServer2::CreateFilteredEnumerator."));
      return E_FAIL;
    }

    // if create already called then release previous enumeration and try again
    if (G_pIEnumOPCBatchSummary != NULL)
    {
      G_pIEnumOPCBatchSummary->Release();    //$REVIEW is this all that's necessary?
      G_pIEnumOPCBatchSummary = NULL;
    }

    IUnknown* pEnumUnknown;
    HRESULT hRes;

    // get the IUnknown pointer for the enumerator interface
    // Pass NULL as the filter pointer if the used specified Unfiltered
    if (filtDlg.m_bUnfiltered)
    {
      DisplayOutput(VERBOSE_OUT, _T("Try_CreateFilteredEnumerator with NULL pFilter parameter"));
      hRes = m_pIOPCBatchServer2->CreateFilteredEnumerator(IID_IEnumOPCBatchSummary, NULL, pszModel, &pEnumUnknown);
    }
    else
    {
      DisplayOutput(VERBOSE_OUT, _T("Try_CreateFilteredEnumerator with non-NULL pFilter parameter"));
      hRes = m_pIOPCBatchServer2->CreateFilteredEnumerator(IID_IEnumOPCBatchSummary, &filter, pszModel, &pEnumUnknown);
    }
      
    if (FAILED(hRes))
    {
      DisplayOutput(ERROR_OUT, _T("IOPCBatchServer2::CreateFilteredEnumerator Failed. %s"), FormatHresult(hRes));
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

  }
  return S_OK;
}

/////////////////////////////////////////////////////////////////////////////
// COPCBatchServer2 message handlers
