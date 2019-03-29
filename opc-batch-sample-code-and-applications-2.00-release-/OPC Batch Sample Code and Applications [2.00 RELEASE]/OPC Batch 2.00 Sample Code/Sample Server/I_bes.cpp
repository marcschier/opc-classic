// I_BES.cpp
//
// (c) Copyright 1999 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Batch Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Will Irwin, Fisher-Rosemount Systems Inc.  The 'tear-off'
//  implementation of the interface is based on code found elsewhere
//  in the OPC sample server, originally provided by Al Chisholm, 
//  Intellution Inc.
//
// CONTENTS:
//
//  This file contains the sample implementation of 
//  the IOPCEnumerationSets interface for the XXX server.
//
//
// Modification Log:
// Vers    Date   By    Notes
// ----  -------- ---   -----
//  1.0  09/08/99 WGI   Initial implementation for OPC Batch sample server
//
//
//
// The addref of the 'parent' is not actually required by this sample code
// but is good practice.
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"

// Some definitions and declarations that give us some sample
// enumeration sets to work with.  A real server would do this
// in some vendor-specific way.
#include "enumsamp.h"
extern EnumerationSet * allEnumSets[];
extern void createEnumSets();
extern void destroyEnumSets();

/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXBES()
//   Constructor for this Interface
///////////////////////////////////////
/*
 * IXXXBES::IXXXBES
 * IXXXBES::~IXXXBES
 *
 * Parameters (Constructor):
 *  m_Parent		'parent' LPUNKNOWN to use for reference counting.
 *  pmem       IMalloc memory allocator to use for returned data
 */

IXXXBES::IXXXBES(
	LPUNKNOWN parent, 
	IMalloc * pMem)
{
	m_Parent = (XXXServer*)parent;
	m_pMem = pMem;
	createEnumSets();   // set up some sample data (see enumsamp.cpp)
}


///////////////////////////////////////
// ~IXXXBES()
//   Destructor for this Interface
///////////////////////////////////////
IXXXBES::~IXXXBES(void)
{
  destroyEnumSets();
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
/////////////////////////////////////////////////////////////////////////////


STDMETHODIMP_(ULONG) IXXXBES::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXBES::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXBES::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}


STDMETHODIMP IXXXBES::QueryEnumerationSets(
		DWORD* pdwCount,          // [out]
    DWORD** ppdwEnumSetId,    // [out, size_is(,*pdwCount)]
    LPWSTR** ppszEnumSetName  // [out, string, size_is(,*pdwCount)]
		)
{
  DWORD cSets = EnumerationSet::M_cEnumSets;

  *pdwCount = cSets;

  // Allocate space for the array of set IDs
  // The client must free this
  DWORD * pSetIds;
  pSetIds = *ppdwEnumSetId = (DWORD*) m_pMem->Alloc(sizeof(DWORD) * cSets);
  
  // Allocate space for the array of set name pointers
  // The client must free this
  LPWSTR * pSetNames;
  pSetNames = *ppszEnumSetName = (LPWSTR*) m_pMem->Alloc(sizeof(LPWSTR) * cSets);

  // Iterate through the sets and copy the IDs and names into
  // the arrays that we will return to the client.  We have to 
  // allocate (and the client must free) memory for each of
  // the names.
	for(DWORD i=0; i < cSets; i++)
	{
    EnumerationSet* pSet = allEnumSets[i];

    // Copy the set id to the array we will return to client
    pSetIds[i] = pSet->GetEnumSetId();

    // Copy the set name to the array we will return to client
    // If we were being rigorous about error checking we ought
    // to detect any Alloc failure, free anything we had previously
    // allocated, and return E_OUTOFMEMORY.
		pSetNames[i] = (WCHAR*) m_pMem->Alloc(sizeof(WCHAR) * 
                                          (wcslen(pSet->GetEnumSetName())+1));
    if (pSetNames[i] != NULL)
    {
  		wcscpy(pSetNames[i], pSet->GetEnumSetName());
    }
  }

  return S_OK;
}

STDMETHODIMP IXXXBES::QueryEnumeration(
		DWORD dwEnumSetId,        // [in]
    DWORD dwEnumValue,        // [in]
    LPWSTR* pszEnumName       // [out, string]
		)
{
  // Get a pointer to the requested Set
  EnumerationSet * pSet = getEnumSetWithId(dwEnumSetId);
  if (pSet == NULL) 
  {
    // Client asked for an enum set that doesn't exist.
    return E_INVALIDARG;
  }

  if (dwEnumValue < 0 || dwEnumValue > pSet->GetEnumMaxSize())
  {
    // Out of range
    return E_INVALIDARG;
  }

  LPWSTR name = pSet->GetEnumNameAt(dwEnumValue);
  if (name == NULL)
  {
    // No such enum in set
    return E_INVALIDARG;
  }

  *pszEnumName = (WCHAR*)m_pMem->Alloc(sizeof(WCHAR) * 
                                        (wcslen(name) + 1));
  if (*pszEnumName == NULL)
  {
    return E_OUTOFMEMORY;
  }
  else
  {
    wcscpy ( *pszEnumName, name );
  }

  return S_OK;
}


STDMETHODIMP IXXXBES::QueryEnumerationList(
    DWORD dwEnumSetId,        // [in]
    DWORD* pdwCount,          // [out]
    DWORD** ppdwEnumValue,    // [out, size_is(,*pdwCount)]
    LPWSTR** ppszEnumString   // [out, string, size_is(,*pdwCount)]
    )
{
  // Get a pointer to the requested Set
  EnumerationSet * pSet = getEnumSetWithId(dwEnumSetId);
  if (pSet == NULL) 
  {
    // Client asked for an enum set that doesn't exist.
    return E_INVALIDARG;
  }

  DWORD count = pSet->GetEnumCount();
  *pdwCount = count;

  // Allocate space for the array of values
  // Client must free this.
  DWORD* pValues;
  pValues = *ppdwEnumValue = (DWORD*)m_pMem->Alloc(sizeof(DWORD) * count);
  if (pValues == NULL)
  {
    return E_OUTOFMEMORY;
  }

  // Allocate space for the array of string pointers
  // Client must free this.
  LPWSTR* pStrings;
  pStrings = *ppszEnumString = 
    (LPWSTR*)m_pMem->Alloc(sizeof(LPWSTR) * count);
  if (pStrings == NULL)
  {
    return E_OUTOFMEMORY;
  }

  // Iterate through the enums in the set, allocating memory and 
  // copying the strings for return to the client.  Also copy the
  // values and fill in the array of pointers to the strings.
  // In this sample server, the enums within a set are stored
  // in an array; unused values are indicated by a NULL pointer
  // for the name.
  DWORD iEnum = 0; // index to the arrays we will return to client
  for (DWORD i = 0; i < pSet->GetEnumMaxSize(); i++)
  {
    LPWSTR name = pSet->GetEnumNameAt(i);
    if (name != NULL)   // if this enum is used, copy its details
    {
      pValues[iEnum] = i;  

      // allocate memory to hold the name
      pStrings[iEnum] = (WCHAR*)m_pMem->Alloc(sizeof(WCHAR) * 
                                               (wcslen(name) + 1));
      // copy the name into the buffer we will return to client
      wcscpy(pStrings[iEnum], name);
      iEnum++;
    }
  }

  return S_OK;
}
