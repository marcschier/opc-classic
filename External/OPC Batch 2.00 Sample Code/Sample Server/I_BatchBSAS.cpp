// I_BatchBSAS.cpp
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
//  Will Irwin, Fisher-Rosemount Systems.
//
// CONTENTS:
//
//  This file contains the sample implementation of 
//  the IOPCBrowseServerAddressSpace interface for the XXX server.
//  It uses a set of sample data that is created to represent a 
//  batch system. 
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  1.0   11/17/99 WGI   Created by copying Al Chisholm's I_BSAS.CPP
//                        from the sample DA server, and changing it
//                        to deal with the batch-oriented OPCBObj
//                        rather than the TAGITEM structure. 
//
//
// NOTE: at present this code does not support any filtering
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"

extern OPCBObj root_tag;

void FreeAddressList( LPOLESTR*AddressList, int count);


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
	*pNameSpaceType = OPC_NS_HIERARCHIAL;   // An OPC batch server must support hierarchical
	return S_OK;
}


        
///////////////////////////////////////
// IXXXBSAS::
///////////////////////////////////////
STDMETHODIMP IXXXBSAS::ChangeBrowsePosition( 
    /* [in] */ OPCBROWSEDIRECTION dwBrowseDirection,
    /* [string][in] */ LPCWSTR szString)
{
  OPCBObj* pNew;
  OPCBObj* pCurr = getCurrentBrowsePosition();
  bool success = false;

  switch(dwBrowseDirection)
	{
	case OPC_BROWSE_UP:
		if((pNew = pCurr->getParent()) != NULL) 
		{
			setCurrentBrowsePosition(pNew);
			success = true;
		}
		break;

	case OPC_BROWSE_DOWN:
    if((pNew = pCurr->findChildNamed(szString)) != NULL) 
		{
			setCurrentBrowsePosition(pNew);
			success = true;
		}
		break;

	case OPC_BROWSE_TO:
    // szString must be a fully qualified itemID
    if ((pNew = FindSampleBatchObjectByItemID(szString)) != NULL)
    {
      setCurrentBrowsePosition(pNew);
			success = true;
    }
    else if (wcscmp(szString, L"") == 0) 
    {
      setCurrentBrowsePosition(&root_tag);
      success = true;
    }
		break;

	default:
		return E_INVALIDARG;
		break;
	}

  if (success)
    return S_OK;
  else
    return E_INVALIDARG;
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

  // Get a snapshot of the address list for the 
  // object at the current browse position
	// Note that the filtering will be server specific
	//
	OPCBObj* pCurr = getCurrentBrowsePosition();
  pCurr->GetAddressList( dwBrowseFilterType,
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
	FreeAddressList(AddressList, AddressCount);

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
	*szItemID = 0;

	if(!szItemDataID) return E_INVALIDARG;
	if(*szItemDataID == '+') return E_INVALIDARG;	// kludge for compliance test

  OPCBObj* pCurr = getCurrentBrowsePosition();
  if (pCurr == NULL)
  {
    // current browse position is invalid
    return E_FAIL;
  }
  
  if (wcscmp(szItemDataID, L"") == 0)   // empty string passed, caller wants
  {                                     // the id of the current browse position
    *szItemID = WSTRClone ( pCurr->GetItemID(), m_pmem );
    return S_OK;
  }

  // see if the object at the current browse position
  // has a child object named szItemDataID
  OPCBObj* pChild = pCurr->findChildNamed(szItemDataID);
  if (pChild != NULL)
  {
    *szItemID = WSTRClone( pChild->GetItemID(), m_pmem );
	  return S_OK;
  }
  // see if the object at the current browse position 
  // has a property with a descriptor matching szItemDataID
  PROPERTY* pPropList = pCurr->getPropertyList();
  while ((pPropList != NULL) && (pPropList->pid != 0))
  {
    if (wcscmp(pPropList->desc, szItemDataID) == 0)
    {
      // found the property; it must have a non-null
      // suffix otherwise we can't form an itemID for it
      if (pPropList->idsuffix != NULL)
      {
        // form a complete itemID by concatenating the current 
        // object's ID and the property suffix.
        WCHAR temp[200];
        wcscpy ( temp, pCurr->GetItemID() );
        wcscat ( temp, DELIMITER );
        wcscat ( temp, pPropList->idsuffix );
        *szItemID = WSTRClone( temp, m_pmem );
        return S_OK;
      }
      else
      {
        break;  // no point searching further
      }
    }
    pPropList++;     // iterate over property list
  }

  // If we get to here, no such child object or property was found
  return E_INVALIDARG;
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


///////////////////////////////////////
// FreeAddressList()
// Free a list of Item names
//
void FreeAddressList( LPOLESTR*AddressList, int count)
{
	int j;
	
	if(!count) return;

	for(j=0; j<count; j++)
	{
		delete [] AddressList[j];
	}
	delete [] AddressList;
}
