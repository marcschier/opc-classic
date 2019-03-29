// ParamsResults.cpp: Implementation code showing how to access 
//    parameters and results via the discovery and anonymous methods.
//
//////////////////////////////////////////////////////////////////////
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
//       01/14/00 jb    delimiter support, fixed anonymous loop counter.

#include "stdafx.h"
#include "OPCBClient.h"
#include "OPCItemProperties.h"
#include "OPCBatchProps.h"    // well known item IDs like OPC_PROP_B_PARAMETER_COUNT
#include "opcbc.h"
#include "ParamsResults.h"
#include "ParamsResultsDlg.h"

#include "utility.h"
#include "AFXPRIV.H"          // conversions
#include "EditDlg.h"          // simple edit dialog to get item ID

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif



HRESULT Try_ParametersResults(IUnknown* pOPCBatchServer)
{

   if (pOPCBatchServer == NULL)
      return E_FAIL;



   CParamsResultsDlg dlg;
   if (dlg.DoModal() != IDOK)
      return S_FALSE;


   // for item ID . ParameterCount
   if (dlg.m_bShowParameters)
      UseAnonymous(pOPCBatchServer, dlg.m_strItemID, TRUE);

   // for item ID . ResultCount
   if (dlg.m_bShowResults)
      UseAnonymous(pOPCBatchServer, dlg.m_strItemID, FALSE);



   return S_OK;
}



HRESULT UseAnonymous(IUnknown* pOPCBatchServer, const char* szItem, BOOL bParams)
{
   USES_CONVERSION;

	HRESULT *erhRes = 0;
	DWORD n = 0;
	LPWSTR *descs = 0;
	VARTYPE *vts = 0;
	VARIANT *v = 0;
	DWORD *ids = 0;
   CString strVar;
   CString strItem;
   DWORD dwCount;
   DWORD dwParamCountID    = OPC_PROP_B_PARAMETER_COUNT;
   DWORD dwResultCountID   = OPC_PROP_B_RESULT_COUNT;

   HRESULT hRes;
   IMalloc*                      pIMalloc;
   IOPCItemProperties*           pIOPCItemProperties;    
   IOPCBatchServer*              pIOPCBatchServer;    
   CString                       strOut;
   LPWSTR pszDelimiter;
   DWORD i;

   // display output for either parameter or results data display
   if (bParams)
      DisplayOutput(NORMAL_OUT, _T("Trying Anonymous Parameter access."));
   else
      DisplayOutput(NORMAL_OUT, _T("Trying Anonymous Results access."));

   // show entered RPE Item ID
   DisplayOutput(NORMAL_OUT, _T("RPE = %s"), szItem);

   // get delimiter to use in item ID construction
   // first get a pointer to the interface 
   hRes = pOPCBatchServer->QueryInterface(IID_IOPCBatchServer, (void**)&pIOPCBatchServer);
   if (FAILED(hRes)) {
	   DisplayOutput(ERROR_OUT, _T("Unable to get IOPCBatchServer interface."));
      return E_FAIL;
   }
   
   // get the delimiter
   hRes = pIOPCBatchServer->GetDelimiter(&pszDelimiter);
   pIOPCBatchServer->Release();    // no longer need batch server interface pointer
   if (FAILED(hRes)) {
		DisplayOutput(ERROR_OUT, _T("Unable to determine delimiter. GetDelimiter() %s"), FormatHresult(hRes));
      return E_FAIL;
   }

   // get memory allocation interface
   hRes = CoGetMalloc(MEMCTX_TASK, &pIMalloc);
   if (FAILED(hRes)) {
      DisplayOutput(ERROR_OUT, _T("Unable to get IMalloc interface. %s"), FormatHresult(hRes));
      return E_FAIL;
   }

   
   // try to get the properties interface
   hRes = pOPCBatchServer->QueryInterface(IID_IOPCItemProperties, (void**)&pIOPCItemProperties);
   if (FAILED(hRes)) {
      pIMalloc->Release();
      DisplayOutput(ERROR_OUT, _T("Unable to get IOPCItemProperties interface. %s"), FormatHresult(hRes));
      return E_FAIL;
   }

   // get item count
   if (bParams)
   {
	   hRes = pIOPCItemProperties->GetItemProperties(T2OLE(szItem), 1, &dwParamCountID, &v, &erhRes);
	   if (FAILED(hRes)) {
		   DisplayOutput(ERROR_OUT, _T("Error from GetItemProperties: %s"), FormatHresult(hRes));
         return E_FAIL;
	   } 
      else
	   {
         ASSERT(v->vt == VT_I4);
         dwCount = v->lVal;
		   DisplayOutput(VERBOSE_OUT, _T("GetItemProperties for %s.ParameterCount = %d:"), szItem, dwCount);
      }

   }
   else
   {
	   hRes = pIOPCItemProperties->GetItemProperties(T2OLE(szItem), 1, &dwResultCountID, &v, &erhRes);
	   if (FAILED(hRes)) {
		   DisplayOutput(ERROR_OUT, _T("Error from GetItemProperties: %s"), FormatHresult(hRes));
         return E_FAIL;
	   } 
      else
	   {
         ASSERT(v->vt == VT_I4);
         dwCount = v->lVal;
		   DisplayOutput(VERBOSE_OUT, _T("GetItemProperties for %s.ResultCount = %d:"), szItem, dwCount);
      }
   }

   // for each parameter/result show its item properties
   for (DWORD j = 1; j <= dwCount; j++) {

      // create anonymous item id
      if (bParams)
        strItem.Format("%s.OPCBParameters%lsOPCBP%d", szItem, pszDelimiter, j);
      else
        strItem.Format("%s.OPCBResults%lsOPCBR%d", szItem, pszDelimiter, j);

	   hRes = pIOPCItemProperties->QueryAvailableProperties(T2OLE(strItem), &n, &ids, &descs, &vts);

	   if (FAILED(hRes))	{
		   DisplayOutput(ERROR_OUT, _T("Error from QueryAvailableProperties: %s"), FormatHresult(hRes));
		   return E_FAIL;
	   }

      // Test GetItemProperties
	   //
	   hRes = pIOPCItemProperties->GetItemProperties(T2OLE(strItem), n, ids, &v, &erhRes);
	   if (FAILED(hRes)) {
		   DisplayOutput(ERROR_OUT, _T("Error from GetItemProperties: %s"), FormatHresult(hRes));
	   } 
      else
	   {
		   DisplayOutput(VERBOSE_OUT, _T("GetItemProperties for %s returns:"), strItem);
		   DisplayOutput(VERBOSE_OUT, _T(" ID, ERR, DESC, VALUE"));
		   for(i=0; i<n; i++)
		   {
			   if(erhRes[i] == S_OK)
			   {
               FormatVariant(strVar, &v[i]);
				   DisplayOutput(VERBOSE_OUT, _T("  %ld, %8lx, %ls, %s"), ids[i], erhRes[i], descs[i], strVar);
			   }
			   else DisplayOutput(VERBOSE_OUT, _T("  %ld, %8lx"), ids[i], erhRes[i]);
		   }
	   }

	   // Free all of the memory - Careful! this is trickey!
	   //
      // Free memory from QueryAvailableProperties call
      for(i=0; i<n; i++) {
		   if(descs) if(descs[i]) pIMalloc->Free(descs[i]);
	   }

	   if(descs) pIMalloc->Free(descs);
	   if(vts) pIMalloc->Free(vts);
	   if(ids) pIMalloc->Free(ids);

      // Free memory from GetItemProperties call
      if (v) {

         for(i=0; i<n; i++)
	      {
   		   if(&v[i])   VariantClear(&v[i]);
	      }
	      pIMalloc->Free(v);
      }
      if(erhRes) pIMalloc->Free(erhRes);

   }

   // release the interfaces
   pIOPCItemProperties->Release();
   pIMalloc->Release();
   
   return S_OK;
};



