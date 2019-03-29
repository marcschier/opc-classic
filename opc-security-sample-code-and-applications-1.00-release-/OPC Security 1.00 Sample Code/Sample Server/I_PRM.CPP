// I_PRM.cpp
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
//  This file contains the sample implementation of 
//  the IOPCItemProperties interface for the XXX server.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
// 2.00  04/08/98 ACC   new 2.0 Interface
//       06/15/98 ACC   update sample properties to match spec
//
//
//
// The addref of the 'parent' is not actually required by this sample code
// but is good practice.
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"

/////////////////////////////////////////////////////////////////////////////

struct  pidlist {
	DWORD	pid;
	VARTYPE	vt;
	LPWSTR	idsuffix;
	LPWSTR	desc;
};
extern HRESULT FindItem(LPWSTR ItemID, DWORD *Ihandle);
extern HRESULT ReadParameterValue(DWORD Ihandle, DWORD pid, VARIANT * v);
extern int GetParameterTable(DWORD Ihandle, struct pidlist **pidlist);


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXPRM()
//   Constructor for this Interface
///////////////////////////////////////
/*
 * IXXXPRM::IXXXPRM
 * IXXXPRM::~IXXXPRM
 *
 * Parameters (Constructor):
 *  m_Parent		'parent' LPUNKNOWN to use for reference counting.
 *  pmem       IMalloc memory allocator to use for returned data
 */

IXXXPRM::IXXXPRM(
	LPUNKNOWN parent, 
	IMalloc * pmem)
{
	m_Parent = (XXXServer*)parent;
	m_pmem = pmem;
}


///////////////////////////////////////
// ~IXXXPRM()
//   Destructor for this Interface
///////////////////////////////////////
IXXXPRM::~IXXXPRM(void)
{
}





/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
/////////////////////////////////////////////////////////////////////////////


STDMETHODIMP_(ULONG) IXXXPRM::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXPRM::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXPRM::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IOPCParameters functions
/////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////
// IXXXPRM::QueryAvailableParameters
// Return a list of ID codes and descriptions for the available parameters for this ITEMID. 
///////////////////////////////////////

STDMETHODIMP IXXXPRM::QueryAvailableProperties( 
	LPWSTR     ItemID,				//   [in]                        
	DWORD     *pdwNumItems,			//   [out]                       
	DWORD   ** ppParameterIDs,		//   [out, size_is(,*dwNumItems)] 
	LPWSTR  ** ppDescriptions,		//   [out, size_is(,*dwNumItems)] 
	VARTYPE ** ppDataTypes			//   [out, size_is(,*dwNumItems)] 
)
{
	if( !ItemID) return E_INVALIDARG;
	if( !pdwNumItems) return E_INVALIDARG;
	if( !ppParameterIDs) return E_INVALIDARG;
	if( !ppDescriptions) return E_INVALIDARG;
	if( !ppDataTypes) return E_INVALIDARG;

	// Defaults in case of error
	//
	*pdwNumItems = 0;
	*ppParameterIDs = 0;
	*ppDescriptions = 0;
	*ppDataTypes = 0;
	
	// Find the Object in the underlying system
	//
	DWORD Ihandle;
	HRESULT hr;
	int n;
	struct pidlist * pt;

	// Call some simulated low lever functions to find the information
	//
	hr = FindItem(ItemID, &Ihandle);
	if(FAILED(hr)) return hr;
	n = GetParameterTable(Ihandle, &pt);

	// Allocate memory (should add error checks here)
	//
	DWORD *pid;
	LPWSTR *pdesc;
	VARTYPE *ptype;

	pid = *ppParameterIDs = 
		(DWORD*) m_pmem->Alloc(sizeof(DWORD) * n);
	pdesc = *ppDescriptions =
		(LPWSTR*) m_pmem->Alloc(sizeof(LPWSTR) * n);
	ptype = *ppDataTypes = 
		(VARTYPE*) m_pmem->Alloc(sizeof(VARTYPE) * n);

	// Fill in results
	//
	int i;
	for(i=0; i<n; i++)
	{
		WCHAR *temp;
		temp = (WCHAR*) m_pmem->Alloc(sizeof(WCHAR) * (wcslen(pt[i].desc)+1));
		wcscpy(temp, pt[i].desc);
		pdesc[i] = temp;
		ptype[i] = pt[i].vt;
		pid[i] = pt[i].pid;
	}

	*pdwNumItems = n;

	return S_OK;
}

///////////////////////////////////////
// IXXXPRM::
///////////////////////////////////////

STDMETHODIMP IXXXPRM::LookupItemIDs( 
	LPWSTR     ItemID,		    //	[in]                        
	DWORD      dwNumItems,		//	[in]                        
	DWORD    * pParameterIDs,	//	[in]                        
	LPWSTR  ** ppNewItemIDs,	//	[out, string, size_is(,dwNumItems)] 
	HRESULT ** ppErrors		    //	[out, size_is(,dwNumItems)] 
)
{
	if( !ItemID) return E_INVALIDARG;
	if( !dwNumItems) return E_INVALIDARG;
	if( !pParameterIDs) return E_INVALIDARG;
	if( !ppNewItemIDs) return E_INVALIDARG;
	if( !ppErrors) return E_INVALIDARG;

	// Defaults in case of error
	//
	*ppNewItemIDs = 0;
	*ppErrors = 0;
	
	// Find the Object in the underlying system
	//
	DWORD Ihandle;
	HRESULT hr;
	int n;
	struct pidlist * pt;

	// Call some simulated low lever functions to find the information
	//
	hr = FindItem(ItemID, &Ihandle);
	if(FAILED(hr)) return hr;
	n = GetParameterTable(Ihandle, &pt);

	// Allocate memory (should add error checks here)
	//
	LPWSTR *pitemids;
	HRESULT *perr;

	pitemids = *ppNewItemIDs =
		(LPWSTR*) m_pmem->Alloc(sizeof(LPWSTR) * n);
	perr = *ppErrors =
		(HRESULT*) m_pmem->Alloc(sizeof(HRESULT) * dwNumItems);

	// Fill in results
	// For each ID
	//
	int i;
	for(i=0; i<n; i++)
	{
		// If there is a suffix available
		//
		if(pt[i].idsuffix)
		{
			// Then return it
			// (this means that you can do 'real' reads for this parameter)
			//
			WCHAR *temp;
			temp = (WCHAR*) m_pmem->Alloc(sizeof(WCHAR) * 
				(wcslen(ItemID) + wcslen(pt[i].idsuffix) + 1)
				);
			wcscpy(temp, ItemID);
			wcscat(temp, pt[i].idsuffix);
			pitemids[i] = temp;
			perr[i] = S_OK;
		}
		else
		{
			// Else if no corresponding Item ID
			// Then return an error
			// (this means that you can only do GetItemProperties for this parameter)
			//
			pitemids[i] = 0;
			perr[i] = E_FAIL;
		}
	}

	return S_OK;
}


///////////////////////////////////////
// IXXXPRM::
///////////////////////////////////////

STDMETHODIMP IXXXPRM::GetItemProperties( 
	LPWSTR     ItemID,			//	[in]
	DWORD      dwNumItems,		//	[in]
	DWORD    * pParameterIDs,	//	[in]
	VARIANT ** ppData,			//	[out, size_is(,dwNumItems)]
	HRESULT ** ppErrors			//	[out, size_is(,dwNumItems)]
)
{
	if( !ItemID) return E_INVALIDARG;
	if( !dwNumItems) return E_INVALIDARG;
	if( !pParameterIDs) return E_INVALIDARG;
	if( !ppData) return E_INVALIDARG;
	if( !ppErrors) return E_INVALIDARG;

	// Defaults in case of error

	*ppData = 0;
	*ppErrors = 0;

	// Pretend to Find the Object in the underlying system
	//
	DWORD Ihandle;
	HRESULT hr;

	hr = FindItem(ItemID, &Ihandle);
	if(FAILED(hr)) return hr;

	// Allocate memory
	// ( should add error checks)
	//
	VARIANT *pv;
	HRESULT *perr;

	pv = *ppData = 
		(VARIANT*) m_pmem->Alloc(sizeof(VARIANT) * dwNumItems);
	perr = *ppErrors =
		(HRESULT*) m_pmem->Alloc(sizeof(HRESULT) * dwNumItems);

	// And go get the Data
	//
	unsigned int i;

	for(i=0; i<dwNumItems; i++)
	{
		VariantInit(&pv[i]);
		perr[i] = ReadParameterValue(Ihandle, pParameterIDs[i], &pv[i]);
	}

	return S_OK;
}


//////////////////////////////////////////////////////////////////////////////
// Stubs for Demo code...

// Define some sample parameters
// The details of this will be a function of the 'real' server
//
struct pidlist pidlist[] = {
	// OPC Parameters
	1, VT_I2, 0, L"Item Canonical Data Type",
	2, VT_VARIANT, L".CV", L"Item Value",
	3, VT_I2, L".QUALITY", L"Item Quality",
	4, VT_DATE, L".TIMESTAMP", L"Item TimeStamp",
	5, VT_I2, 0, L"Item Access Rights",
	6, VT_R4, L".SCANRATE", L"Server Scan Rate",

	// Recommended Parameters
	100, VT_BSTR, L".EULABEL", L"EU Units",
	101, VT_BSTR, L".DESCRIPTOR", L"Item Description"

	// Vendor Specific Parameters

	// End of list
};


HRESULT FindItem(LPWSTR ItemID, DWORD *Ihandle)
{
	return S_OK;
}

int GetParameterTable(DWORD Ihandle, struct pidlist **pplist)
{
	*pplist = pidlist;
	return sizeof(pidlist) / sizeof(struct pidlist);		// # elements in structure
}


HRESULT ReadParameterValue(DWORD Ihandle, DWORD pid, VARIANT * v)
{
	FILETIME lclTime;
	DATE date;
	WORD dosDate=0, dosTime=0;

	switch(pid)
	{
	case 1:
		//we want the canonical VARTYPE of this items 'value'
		//
		v->vt = VT_I2;			// the type of a VARTYPE (should be UI2 but there is no such thing)
		v->iVal = VT_R4;		// at the moment all items are arbitrarily R4 
									//(this would be more complex in a real server)
		return S_OK;
		break;

	case 2:
		//we want the current value
		//
		v->vt = VT_R4;			// arbitrary canonical data type
		v->fltVal = (float) 12.345;
		return S_OK;
		break;

	case 3:
		//we want the Quality
		//
		v->vt = VT_I2;
		v->iVal = OPC_QUALITY_GOOD;
		return S_OK;
		break;

	case 4:
		//we want timestamp
		// This code shows how to convert a FILETIME to a DATE (magic... Thanks Jim Hansen)
		//
		CoFileTimeNow(&lclTime);		// Real code would get this from the ITEM...
		FileTimeToDosDateTime( &lclTime, &dosDate, &dosTime);
		DosDateTimeToVariantTime( dosDate, dosTime, &date);
		v->vt = VT_DATE;
		v->date = date;
		return S_OK;
		break;

	case 5:
		//we want the Access Rights of this item
		//
		v->vt = VT_I2;
		v->iVal = OPC_READABLE | OPC_WRITEABLE;
		return S_OK;
		break;

	case 6:
		//we want the server scan time
		//
		v->vt = VT_R4;
		v->fltVal = (float) 250.0;
		return S_OK;
		break;


//----------------
	case 100:
		//we want the EU units
		//
		v->vt = VT_BSTR;
		v->bstrVal = SysAllocString(L"Percent");
		return S_OK;
		break;

	case 101:
		//we want the item description
		//
		v->vt = VT_BSTR;
		v->bstrVal = SysAllocString(L"Boiler 5 Water Level");
		return S_OK;
		break;

	default:
		return E_FAIL;
	}
}


