// OPCItemProperties.cpp: implementation of the COPCItemProperties class.
//
//////////////////////////////////////////////////////////////////////

#include "stdafx.h"
#include "OPCBClient.h"
#include "OPCItemProperties.h"

#include "utility.h"
#include "AFXPRIV.H"          // conversions
#include "EditDlg.h"          // simple edit dialog to get name

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif


//void TryParams(IOPCItemProperties * pPRM);   




//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCItemProperties::COPCItemProperties()
{
   m_pIOPCItemProperties   = NULL;
   m_pIMalloc              = NULL;

}

COPCItemProperties::~COPCItemProperties()
{
   CleanUp();
}

void COPCItemProperties::CleanUp(){
   if (m_pIOPCItemProperties) m_pIOPCItemProperties->Release();
   if (m_pIMalloc)            m_pIMalloc->Release();
}


HRESULT COPCItemProperties::Initialize(IUnknown* pOPCBatchServer)
{
   CleanUp();

   // initialize member pointer to IOPCServer interface.
   HRESULT hRes = pOPCBatchServer->QueryInterface(IID_IOPCItemProperties, (void**)&m_pIOPCItemProperties);
   if (FAILED(hRes)) {
      m_pIOPCItemProperties = NULL;  
	   DisplayOutput(ERROR_OUT, _T("Unable to get IOPCItemProperties interface. %s"), FormatHresult(hRes));
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











HRESULT COPCItemProperties::Try_QueryAvailableProperties()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCItemProperties::Try_QueryAvailableProperties."));

   USES_CONVERSION;

	HRESULT hRes;
	DWORD n;
	LPWSTR *descs = 0;
	VARTYPE *vts = 0;
	DWORD *ids = 0;
	unsigned int i;


   // Get ItemID from user
   CEditDlg dlg(_T("Item ID"), _T("Please enter Item ID:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;


	// Test QueryAvailableProperties
	//
	hRes = m_pIOPCItemProperties->QueryAvailableProperties(T2OLE(dlg.m_strEdit), &n, &ids, &descs, &vts);

	if (FAILED(hRes))	{
		DisplayOutput(ERROR_OUT, _T("Error from QueryAvailableProperties: %s"), FormatHresult(hRes));
		return E_FAIL;
	}

	DisplayOutput(VERBOSE_OUT, _T("Query returns: %ld properties:"), n);
	DisplayOutput(VERBOSE_OUT, _T(" ID, DESC, TYPE"));
	for(i=0; i<n; i++) {
		DisplayOutput(VERBOSE_OUT, _T("  %ld, %ls, %4lx"), ids[i], descs[i], vts[i]);
	}



	// Free all of the memory - Careful! this is trickey!
	//
   for(i=0; i<n; i++) {
		if(descs) if(descs[i]) m_pIMalloc->Free(descs[i]);
	}


	if(descs) m_pIMalloc->Free(descs);
	if(vts) m_pIMalloc->Free(vts);
	if(ids) m_pIMalloc->Free(ids);


   return S_OK;
};


HRESULT COPCItemProperties::Try_GetItemProperties()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCItemProperties::Try_GetItemProperties."));

   USES_CONVERSION;

	HRESULT hRes;
	HRESULT *erhRes = 0;
	DWORD n = 0;
	LPWSTR *descs = 0;
	LPWSTR *newids = 0;
	VARTYPE *vts = 0;
	VARIANT *v = 0;
	DWORD *ids = 0;
	unsigned int i;
   CString strVar;

   // Get ItemID from user
   CEditDlg dlg(_T("Item ID"), _T("Please enter Item ID:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;

   // get available properties
   // we need to make this call to initialize ids for the GetItemProperties call.
	hRes = m_pIOPCItemProperties->QueryAvailableProperties(T2OLE(dlg.m_strEdit), &n, &ids, &descs, &vts);

	if (FAILED(hRes))	{
		DisplayOutput(ERROR_OUT, _T("Error from QueryAvailableProperties: %s"), FormatHresult(hRes));
		return E_FAIL;
	}

   // Test GetItemProperties
	//
	hRes = m_pIOPCItemProperties->GetItemProperties(T2OLE(dlg.m_strEdit), n, ids, &v, &erhRes);
	if (FAILED(hRes)) {
		DisplayOutput(ERROR_OUT, _T("Error from GetItemProperties: %s"), FormatHresult(hRes));
	} 
   else
	{
		DisplayOutput(VERBOSE_OUT, _T("Get returns:"));
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
		if(descs) if(descs[i]) m_pIMalloc->Free(descs[i]);
	}

	if(descs) m_pIMalloc->Free(descs);
	if(vts) m_pIMalloc->Free(vts);
	if(ids) m_pIMalloc->Free(ids);

   // Free memory from GetItemProperties call
   if (v) {

      for(i=0; i<n; i++)
	   {
   		if(&v[i])   VariantClear(&v[i]);
	   }
	   m_pIMalloc->Free(v);
   }
	if(erhRes) m_pIMalloc->Free(erhRes);

   return S_OK;
};


HRESULT COPCItemProperties::Try_LookupItemIDs()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCItemProperties::Try_LookupItemIDs."));

   USES_CONVERSION;

   HRESULT hRes;
	DWORD n = 0;
	LPWSTR *descs = 0;
	LPWSTR *newids = 0;
	VARTYPE *vts = 0;
	DWORD *ids = 0;
	VARIANT *v = 0;
	HRESULT *erhRes = 0;
	HRESULT *err2 = 0;
	unsigned int i;



   // Get ItemID from user
   CEditDlg dlg(_T("Item ID"), _T("Please enter Item ID:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;

   // get avialable properties
   // we need to make this call to initialize ids for the GetItemProperties call.
	hRes = m_pIOPCItemProperties->QueryAvailableProperties(T2OLE(dlg.m_strEdit), &n, &ids, &descs, &vts);

	if (FAILED(hRes))	{
		DisplayOutput(ERROR_OUT, _T("Error from QueryAvailableProperties: %s"), FormatHresult(hRes));
		return E_FAIL;
	}

   // Test LookupItemIDs
	//
	hRes = m_pIOPCItemProperties->LookupItemIDs(T2OLE(dlg.m_strEdit), n, ids, &newids, &err2);
	if (FAILED(hRes))	{
	   DisplayOutput(ERROR_OUT, _T("Error from LookupItemIDs: %s"), FormatHresult(hRes));
	}  
   else
	{
		DisplayOutput(VERBOSE_OUT, _T("Lookup returns:"));
		DisplayOutput(VERBOSE_OUT, _T(" ID, NEWID, ERR"));
		for(i=0; i<n; i++)
		{
			if(err2[i] == S_OK) 
            DisplayOutput(VERBOSE_OUT, _T("  %ld, %ls, %8lx"), ids[i], newids[i], err2[i]);
			else 
            DisplayOutput(VERBOSE_OUT, _T("  %ld, %8lx"), ids[i], err2[i]);
		}
	}


	// Free all of the memory - Careful! this is trickey!
	//
   // Free memory from QueryAvailableProperties call
   for(i=0; i<n; i++) {
		if(descs) if(descs[i]) m_pIMalloc->Free(descs[i]);
	}

	if(descs) m_pIMalloc->Free(descs);
	if(vts) m_pIMalloc->Free(vts);
	if(ids) m_pIMalloc->Free(ids);

   // Free memory from LookupItemIDs call
   for(i=0; i<n; i++)
	{
		if(newids) if(newids[i]) m_pIMalloc->Free(newids[i]);
	}
	if(newids) m_pIMalloc->Free(newids);
	if(err2) m_pIMalloc->Free(err2);

   return S_OK;
};




