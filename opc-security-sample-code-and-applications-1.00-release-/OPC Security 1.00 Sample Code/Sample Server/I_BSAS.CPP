// I_BSAS.cpp
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
//  the IOPCBrowseServerAddressSpace interface for the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  1.0a  08/01/97 ACC
//  2.0   04/08/98 acc   make this a tear off i/f rather than separate obj
//
//
//
// NOTE: at present this code does not support any filtering
// and works only for a 'flat' address space.
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXBSAS()
//   Constructor for this Interface
///////////////////////////////////////
/*
 * IXXXBSAS::IXXXBSAS
 * IXXXBSAS::~IXXXBSAS
 *
 * Parameters (Constructor):
 *  m_Parent		'parent' LPUNKNOWN to use for reference counting.
 *  pmem       IMalloc memory allocator to use for returned data
 */

IXXXBSAS::IXXXBSAS(
	LPUNKNOWN parent, 
	IMalloc * pmem)
{
	m_Parent = (XXXServer*)parent;
	m_pmem = pmem;
}


///////////////////////////////////////
// ~IXXXBSAS()
//   Destructor for this Interface
///////////////////////////////////////
IXXXBSAS::~IXXXBSAS(void)
{
}






/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
/////////////////////////////////////////////////////////////////////////////


STDMETHODIMP_(ULONG) IXXXBSAS::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXBSAS::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXBSAS::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}


/////////////////////////////////////////////////////////////////////////////
// IOPCBrowseServerAddressSpace functions
/////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////
// IXXXBSAS::
///////////////////////////////////////
STDMETHODIMP IXXXBSAS::QueryOrganization( 
    /* [out] */ OPCNAMESPACETYPE  *pNameSpaceType)
{
	if(!pNameSpaceType) return E_INVALIDARG;
	*pNameSpaceType = OPC_NS_FLAT;
	return S_OK;
}


        
///////////////////////////////////////
// IXXXBSAS::
///////////////////////////////////////
STDMETHODIMP IXXXBSAS::ChangeBrowsePosition( 
    /* [in] */ OPCBROWSEDIRECTION dwBrowseDirection,
    /* [string][in] */ LPCWSTR szString)
{
	return E_FAIL;
}


        
///////////////////////////////////////
// IXXXBSAS::
///////////////////////////////////////
STDMETHODIMP IXXXBSAS::BrowseOPCItemIDs( 
    /* [in] */ OPCBROWSETYPE dwBrowseFilterType,
    /* [string][in] */ LPCWSTR szFilterCriteria,
    /* [in] */ VARTYPE vtDataTypeFilter,
    /* [in] */ DWORD dwAccessRightsFilter,
    /* [out] */ LPENUMSTRING  *ppIEnumString)
{
	IXXXEnumString *temp;
	LPOLESTR * AddressList;
	int	AddressCount;
	HRESULT hr;

	// Get a snapshot of the address list 
	// Note that the filtering will be server specific
	//
	m_Parent->GetAddressList(  
	 dwBrowseFilterType,
	 szFilterCriteria,
	 vtDataTypeFilter,
	 dwAccessRightsFilter,
	 &AddressList, 
	 &AddressCount);

	// Create the Enumerator using the snapshot
	// Note that the enumerator will AddRef the 'parent' server 
	// This is not actually required in this sample code but is good practice.
	//
	temp = new IXXXEnumString(m_Parent, AddressCount, AddressList, m_pmem);
	m_Parent->FreeAddressList(AddressList, AddressCount);

	if ( temp == NULL)
		return E_OUTOFMEMORY;

	// Then QI for the interface.
	// Note 'temp' actually is the interface
	// but QI is the 'proper' way to get it.
	// Note QI will do an AddRef of the Enum which will also do
	// an AddRef of the 'parent' - i.e. the 'this' pointer passed above.
	//
	hr = temp->QueryInterface( IID_IEnumString, (LPVOID*)ppIEnumString);
	if ( FAILED( hr))
	{
		delete temp;
		return hr;
	}

	return S_OK;
}


        
///////////////////////////////////////
// IXXXBSAS::
///////////////////////////////////////
STDMETHODIMP IXXXBSAS::GetItemID( 
    /* [in] */ LPWSTR szItemDataID,
    /* [string][out] */ LPWSTR  *szItemID)
{
	if(!szItemID) return E_INVALIDARG;
	*szItemID = WSTRClone(szItemDataID, m_pmem);
	return S_OK;
}


        
///////////////////////////////////////
// IXXXBSAS::
///////////////////////////////////////
STDMETHODIMP IXXXBSAS::BrowseAccessPaths( 
    /* [string][in] */ LPCWSTR szItemID,
    /* [out] */ LPENUMSTRING  *ppIEnumString)
{
	return E_NOTIMPL;
}
        


