// i_im.cpp
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
//  This file contains the implementation of 
//  the IOPCItemMgt interface for groups in the XXX server.
//
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  0.00  11/18/96 ACC
//  0.01  02/05/97 acc   Fix memory leaks (see acc001)
//                       Also fix some error handlers
//        10/20/99 acc   Misc fixes and additional error checks for compliance test
//        05/31/2000 acc Fill in defaults (esp pBlob) in Add and Validate 
//                       to prevent crash on Bad tag.
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCerror.h"

extern CRITICAL_SECTION	CritSec;

//zzz
extern HRESULT FindItem(LPWSTR ItemID, DWORD *Ihandle);
extern HRESULT ValidItem(LPWSTR ItemID);

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXIM()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXIM::IXXXIM( LPUNKNOWN parent )
{
	m_Parent = (XXXGroup *)parent;
}



///////////////////////////////////////
// ~IXXXIM()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXIM::~IXXXIM( void)
{
	m_Parent->m_pIXXXIM = 0;
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXIM::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXIM::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXIM::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXIM (IOPCItemMgt) interface functions
//


///////////////////////////////////////
// AddItems
///////////////////////////////////////
STDMETHODIMP IXXXIM::AddItems( 
	DWORD            dwNumItems,
	OPCITEMDEF     * pItemArray,
	OPCITEMRESULT ** ppAddResults,
	HRESULT       ** ppErrors
    )
{
	XXXGroup &g = *m_Parent;
	unsigned int	i;
	OPCHANDLE j;
	OPCITEMRESULT *ir;
	HRESULT *hr;
	BOOL	ok = TRUE;
	XXXItem	*newitem;

	// Defaults in case of error
	//
	*ppAddResults = ir = NULL;		//acc001
	*ppErrors = hr = NULL;		//acc001

	// Validate parameters
	//
	if (dwNumItems == 0) return E_INVALIDARG;

	// Allocate memory for the result array(s)
	//
	*ppAddResults = ir = (OPCITEMRESULT*)pIMalloc->Alloc(sizeof(OPCITEMRESULT) * dwNumItems);	//acc001
	if(ir == NULL) goto add_error;
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) goto add_error;

	// Now for each item... 
	//
	for(i= 0; i<dwNumItems; i++)
	{
		// Fill in sensible defaults so proxy won't crash if tag is bad.
		ir[i].hServer = 0;
		ir[i].vtCanonicalDataType = 0;
		ir[i].dwAccessRights = 0;
		ir[i].dwBlobSize = 0l;
		ir[i].pBlob = NULL;	// (this is probably the important one)

		// Validate the ItemTag
		// zzz actually 'newitem' should probably be doing this....
		//
		hr[i] = ValidItem(pItemArray[i].szItemID);
		if (FAILED(hr[i]))
		{
			ok = FALSE;
			continue;
		}

		// find a place to put the item
		//
		hr[i] = g.ItemAlloc(&j);
		if (FAILED(hr[i]))
		{
			ok = FALSE;
			continue;
		}

		// Then create an empty item ... make it a batch item 
		// if necessary...
    if (isBatchItemID(pItemArray[i].szItemID))
    {
      newitem = new XXXBatchItem(&g);
    }
    else
    {
      newitem = new XXXItem(&g);
    }
		if(newitem == NULL) 
		{
			hr[i] = E_OUTOFMEMORY;
			ok = FALSE;
			continue;
		}

		// Then populate it
		// and record it in this group
		//
		hr[i] = newitem->Init(j, &pItemArray[i], &ir[i]);
		if(FAILED(hr[i]))
		{
			delete newitem;
			ok = FALSE;
			continue;
		}
		g.ItemSet(j, newitem);
		newitem->AddRef();		// Record the fact the the group has a pointer to it
	}
	if(ok) return S_OK;
	return S_FALSE;

add_error:
	if(hr) pIMalloc->Free(hr);		//acc001
	if(ir) pIMalloc->Free(ir);		//acc001
	*ppErrors = NULL;
	*ppAddResults = NULL;
	return E_OUTOFMEMORY;
}


///////////////////////////////////////
// ValidateItems
// Note this sample server accepts any item definition
// So any input will be validated as 'OK'
///////////////////////////////////////
STDMETHODIMP IXXXIM::ValidateItems( 
	DWORD             dwNumItems,
	OPCITEMDEF      * pItemArray,
	BOOL              bBlobUpdate,
	OPCITEMRESULT  ** ppValidationResults,
	HRESULT        ** ppErrors
    )
{
	unsigned int	i;
	OPCITEMRESULT *ir;
	HRESULT *hr;
	BOOL	ok = TRUE;

	// Defaults in case of error
	//
	*ppValidationResults = ir = NULL;		//acc001
	*ppErrors = hr = NULL;		//acc001

	// Validate parameters
	//
	if (dwNumItems == 0) return E_INVALIDARG;

	// Allocate memory for the result array(s)
	//
	*ppValidationResults = ir = (OPCITEMRESULT*) pIMalloc->Alloc(sizeof(OPCITEMRESULT) * dwNumItems);	//acc001
	if(ir == NULL) goto val_error;
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) goto val_error;

	// Now for each item... 
	//
	for(i= 0; i<dwNumItems; i++)
	{
		// Fill in sensible defaults so proxy won't crash if tag is bad.
		ir[i].hServer = 0;
		ir[i].vtCanonicalDataType = 0;		// Sample is always Float
		ir[i].dwAccessRights = 0;
		ir[i].dwBlobSize = 0l;
		ir[i].pBlob = NULL;	// (this is probably the important one)

		// Validate the ItemTag
		// (actually we are supposed to find the real item...)
		//
		hr[i] = ValidItem(pItemArray[i].szItemID);
		if (FAILED(hr[i]))
		{
			ok = FALSE;
			continue;
		}

		// Otherwise Item is valid
		//
		hr[i] = S_OK;

		ir[i].hServer = 0;
		ir[i].vtCanonicalDataType = VT_R4;		// Sample is always Float
		ir[i].dwAccessRights = OPC_READABLE | OPC_WRITEABLE;
		ir[i].dwBlobSize = 0l;
		ir[i].pBlob = NULL;
	}
	if(ok) return S_OK;
	return S_FALSE;

val_error:
	if(ir) pIMalloc->Free(hr);		//acc001
	if(ir) pIMalloc->Free(ir);		//acc001
	*ppErrors = NULL;
	*ppValidationResults = NULL;
	return E_OUTOFMEMORY;
}

///////////////////////////////////////
// RemoveItems
///////////////////////////////////////
STDMETHODIMP IXXXIM::RemoveItems( 
	DWORD        dwNumItems,
	OPCHANDLE  * phServer,
	HRESULT   ** ppErrors
    )
{
	XXXGroup &g = *m_Parent;
	unsigned int	i;
	OPCHANDLE j;
	HRESULT *hr, r1;
	BOOL ok = TRUE;

	// Defaults in case of error
	//
	*ppErrors = hr = NULL;

	// Validate parameters
	//
	if (dwNumItems == 0) return E_INVALIDARG;

	// First - allocate memory for the result array(s)
	//
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) return E_OUTOFMEMORY;

	// Enter critical section whenever deleting anything
	//
	EnterCriticalSection(&CritSec);

	// Now for each item handle... 
	//
	for(i= 0; i<dwNumItems; i++)
	{
		// Insure handle is valid
		//
		j = phServer[i];
		if(!g.ItemValid(j))
		{
			hr[i] = OPC_E_INVALIDHANDLE;
			ok = FALSE;
			continue;
		}

		// And if so then delete the item
		// The 'release' indicates that the group no longer has a pointer to it
		// This will delete the item if all other interfaces are free
		// (Note that there are no Custom interfaces to the item)
		//
		g.ItemPtr(j)->Release();	
		g.ItemFree(j);

		hr[i] = S_OK;
	}
	if(ok) r1 = S_OK;
	else r1 = S_FALSE;

	LeaveCriticalSection(&CritSec);
	return r1;
}


///////////////////////////////////////
// SetActiveState
///////////////////////////////////////
STDMETHODIMP IXXXIM::SetActiveState(
	DWORD        dwNumItems,
	OPCHANDLE  * phServer,
	BOOL         bActive, 
	HRESULT   ** ppErrors
    )
{
	XXXGroup &g = *m_Parent;
	unsigned int	i;
	OPCHANDLE j;
	HRESULT *hr;
	BOOL ok = TRUE;

	// Defaults in case of error
	//
	*ppErrors = hr = NULL;

	// Validate parameters
	//
	if (dwNumItems == 0) return E_INVALIDARG;

	// Allocate memory for the result array(s)
	//
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) return E_OUTOFMEMORY;

	// Now for each item handle... 
	//
	for(i= 0; i<dwNumItems; i++)
	{
		// Insure handle is valid
		//
		j = phServer[i];
		if(!g.ItemValid(j))
		{
			hr[i] = OPC_E_INVALIDHANDLE;
			ok = FALSE;
			continue;
		}

		// And if so then activate the item
		//
		g.ItemPtr(j)->SetActive(bActive);

		hr[i] = S_OK;
	}
	if(ok) return S_OK;
	return S_FALSE;
}

///////////////////////////////////////
// SetClientHandles
///////////////////////////////////////
STDMETHODIMP IXXXIM::SetClientHandles(
	DWORD        dwNumItems,
	OPCHANDLE  * phServer,
	OPCHANDLE  * phClient,
	HRESULT   ** ppErrors
    )
{
	XXXGroup &g = *m_Parent;
	unsigned int	i;
	OPCHANDLE j;
	HRESULT *hr;
	BOOL ok = TRUE;

	// Defaults in case of error
	//
	*ppErrors = hr = NULL;

	// Validate parameters
	//
	if (dwNumItems == 0) return E_INVALIDARG;

	// Allocate memory for the result array(s)
	//
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) return E_OUTOFMEMORY;

	// Now for each item handle... 
	//
	for(i= 0; i<dwNumItems; i++)
	{
		// Insure handle is valid
		//
		j = phServer[i];
		if(!g.ItemValid(j))
		{
			hr[i] = OPC_E_INVALIDHANDLE;
			ok = FALSE;
			continue;
		}

		// And if so then set client handle
		//
		g.ItemPtr(j)->SetHandle(phClient[i]);

		hr[i] = S_OK;
	}
	if(ok) return S_OK;
	return S_FALSE;
}

///////////////////////////////////////
// SetDataTypes
///////////////////////////////////////
STDMETHODIMP IXXXIM::SetDatatypes(
	DWORD        dwNumItems,
	OPCHANDLE  * phServer,
	VARTYPE    * pRequestedDatatypes,
	HRESULT   ** ppErrors
    )
{
	XXXGroup &g = *m_Parent;
	unsigned int	i;
	OPCHANDLE j;
	HRESULT *hr;
	BOOL ok = TRUE;

	// Defaults in case of error
	//
	*ppErrors = hr = NULL;

	// Validate parameters
	//
	if (dwNumItems == 0) return E_INVALIDARG;

	// Allocate memory for the result array(s)
	//
	*ppErrors = hr = (HRESULT*) pIMalloc->Alloc(sizeof(HRESULT) *dwNumItems);		//acc001
	if(hr == NULL) return E_OUTOFMEMORY;

	// Now for each item handle... 
	//
	for(i= 0; i<dwNumItems; i++)
	{
		// Insure handle is valid
		//
		j = phServer[i];
		if(!g.ItemValid(j))
		{
			hr[i] = OPC_E_INVALIDHANDLE;
			ok = FALSE;
			continue;
		}

		// And if so then set data type
		//
		hr[i] = g.ItemPtr(j)->SetDatatype(pRequestedDatatypes[i]);
		if(FAILED(hr[i]))
		{
			ok = FALSE;
			continue;
		}

	}
	if(ok) return S_OK;
	return S_FALSE;
}
 
///////////////////////////////////////
// CreateEnumerator
///////////////////////////////////////
STDMETHODIMP IXXXIM::CreateEnumerator(
	REFIID      riid,
	LPUNKNOWN * ppUnk
    )
{
	// default in case of error
	//
	*ppUnk = NULL;		//acc001

	if ( riid == IID_IEnumOPCItemAttributes)
	{
		IXXXEnumIA *temp;
		OPCITEMATTRIBUTES* AttrList;
		int	ItemCount;
		HRESULT hr;

		// Get a 'local' snapshot of the item list 
		//
		m_Parent->GetItemList( &AttrList, &ItemCount);

		// Create the Enumerator which will contain a copy of the snapshot
		// Note that the enumerator is not affected by 
		// subsequent changes to the item list.
		// The group will get an 'addref' via m_Parent below and will thus
		// stay around until the enumerator is released 
		// (although this is probably unnecessary)
		//
		temp = new IXXXEnumIA(m_Parent, ItemCount, AttrList, pIMalloc);
		m_Parent->FreeItemList( AttrList, ItemCount);		//acc001

		// Then QI for the interface. ('temp' actually is the interface
		// but QI is the 'proper' way to get it.)
		// Note QI will do an AddRef of the Enum which will also do
		// an AddRef of the 'parent' - i.e. the 'this' pointer passed above.
		//
		hr = temp->QueryInterface( riid, (LPVOID*)ppUnk);
		if ( FAILED( hr))
		{
			delete temp;
			return hr;
		}

		return S_OK;
	}
	return E_INVALIDARG;
}


