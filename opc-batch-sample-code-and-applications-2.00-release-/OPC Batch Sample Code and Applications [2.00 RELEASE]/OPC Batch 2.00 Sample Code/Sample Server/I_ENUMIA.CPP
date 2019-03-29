// I_enumia.cpp
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
//  This file contains an implementation of an IEnumOPCItemAttributes interface.
//
//  Since the list of elements is buffered in the object
//  It is appropriate for enumerations with a 'reasonable' number of elements
//
//  Note that new/delete is used for local buffer storage
//  while the global allocator is used for returned storage.
//
// See IEnumXXXX::Next in Win32SDK\OLE\Reference\Interfaces\IEnumXXXX
// for general guidelines for enumerators
//
//
// Modification Log:
//	Vers  Date     By    Notes
//	----  -------- ---   -----
//	0.00  12/27/96 ACC   Based on Brockschmidt (with improvements)
//        06/01/00 acc   fix to properly handle count (cIA) of 0 in constructor
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"


/*
 * IXXXEnumIA::IXXXEnumIA
 * IXXXEnumIA::~IXXXEnumIA
 *
 * Parameters (Constructor):
 *  pUnkRef		 LPUNKNOWN to use for reference counting.
 *  cstr			ULONG number of OPCITEMATTRIBTESs in pIA
 *  pIA		  ptr to IAs
 *  pmem       IMalloc memory allocator to use for returned data
 */

IXXXEnumIA::IXXXEnumIA(LPUNKNOWN pUnkRef, ULONG cIA
	, OPCITEMATTRIBUTES *pIA, IMalloc * pmem)
{
	UINT		i;

	m_cRef = 0;
	m_pUnkRef = pUnkRef;

	m_iCur = 0;
	m_cIA = cIA;
	if(cIA) m_pIA = new OPCITEMATTRIBUTES[cIA];	// use local memory
	else m_pIA = NULL;
	m_pmem = pmem;

	if (NULL != m_pIA)
	{
		// Make local copies of all the IAs
		//
		for (i=0; i < m_cIA; i++)
		{
			IAClone(&m_pIA[i], &pIA[i], NULL);	// use local memory
		}
	}
	return;
}


IXXXEnumIA::~IXXXEnumIA(void)
{
	unsigned int i;

	if (NULL!=m_pIA)
	{
		// Delete the local copies of all the IAs
		//
		for (i=0; i < m_cIA; i++)
		{
			IAFree(&m_pIA[i], NULL);
		}
		delete [] m_pIA;
	}
	return;
}






/*
 * IXXXEnumIA::QueryInterface
 * IXXXEnumIA::AddRef
 * IXXXEnumIA::Release
 *
 * Purpose:
 *  IUnknown members for IXXXEnumIA object.
 */

STDMETHODIMP IXXXEnumIA::QueryInterface(REFIID riid
	, LPVOID *ppv)
{
	*ppv=NULL;

	if (IID_IUnknown==riid || IID_IEnumOPCItemAttributes==riid)
		*ppv=(LPVOID)this;

	if (NULL!=*ppv)
	{
		((LPUNKNOWN)*ppv)->AddRef();
		return S_OK;
	}

	return E_NOINTERFACE;
}


STDMETHODIMP_(ULONG) IXXXEnumIA::AddRef(void)
{
	// Addref this object and also the 'parent' if any
	//
	++m_cRef;
	if(m_pUnkRef != NULL) 
		m_pUnkRef->AddRef();
	return m_cRef;
}

STDMETHODIMP_(ULONG) IXXXEnumIA::Release(void)
{
	// Release this object and also the 'parent' if any
	//
	if(m_pUnkRef != NULL) 
		m_pUnkRef->Release();

	if (0L!=--m_cRef)
		return m_cRef;

	delete this;
	return 0;
}



/*
 * IXXXEnumIA::Next
 *
 * Purpose:
 *  Returns the next element in the enumeration.
 *
 * Parameters:
 *  cIA			ULONG max number of OPCITEMATTRIBTESs to return.
 *  pIA			OPCITEMATTRIBTES(s) in which to store the returned
 *				  structures.
 *  pulstr		  ULONG * in which to return how many we
 *				  actually returned.
 *
 * Return Value:
 *  HRESULT		 S_OK if successful, S_FALSE otherwise,
 */

STDMETHODIMP IXXXEnumIA::Next(ULONG cIA, OPCITEMATTRIBUTES **pIA
	, ULONG *pActual)
{
	ULONG		cReturn = 0L;
	ULONG		maxcount = cIA;
	OPCITEMATTRIBUTES * temp;

	*pActual = 0L;	// default return count
	*pIA = NULL;

	// If this enumerator is empty - return FALSE (should never happen)
	// 
	if (NULL == m_pIA)
		return S_FALSE;

	// If user passed null for count of items returned
	// Then he is only allowed to ask for 1 item
	//
	if (NULL == pActual)
	{
		if (1L != cIA)
			return E_POINTER;
	}

	// If we are at end of list return FALSE
	//
	if (m_iCur >= m_cIA)
		return S_FALSE;

	// Allocate the items to be returned
	//
	temp = (OPCITEMATTRIBUTES*)pIMalloc->Alloc(cIA * sizeof(OPCITEMATTRIBUTES));
	if(temp == NULL) return S_FALSE;
	
	// Return as many as we have left in list up to request count
	//
	while (m_iCur < m_cIA && cIA > 0)
	{
		// Fill in an IA to return to caller
		IAClone(&temp[cReturn], &m_pIA[m_iCur], m_pmem);	// Use global memory for strings

		// And move on to the next one
		//
		m_iCur++;
		cReturn++;
		cIA--;
	}

	// Shorten the block to the size actually used
	// and return the final pointer to the caller
	//
	*pIA = (OPCITEMATTRIBUTES*)pIMalloc->Realloc(temp, cReturn * sizeof(OPCITEMATTRIBUTES));

	if (NULL != pActual)
		*pActual = cReturn;

	if (cReturn == maxcount) return S_OK;
	return S_FALSE;
}







/*
 * IXXXEnumIA::Skip
 *
 * Purpose:
 *  Skips the next n elements in the enumeration.
 *
 * Parameters:
 *  cSkip		   ULONG number of elements to skip.
 *
 * Return Value:
 *  HRESULT		 S_OK if successful, S_FALSE if we could not
 *				  skip the requested number.
 */

STDMETHODIMP IXXXEnumIA::Skip(ULONG cSkip)
{
	if (((m_iCur+cSkip) >= m_cIA) || NULL==m_pIA)
		return S_FALSE;

	m_iCur+=cSkip;
	return S_OK;
}






/*
 * IXXXEnumIA::Reset
 *
 * Purpose:
 *  Resets the current element index in the enumeration to zero.
 *
 * Parameters:
 *  None
 */

STDMETHODIMP IXXXEnumIA::Reset(void)
{
	m_iCur=0;
	return S_OK;
}






/*
 * IXXXEnumIA::Clone
 *
 * Purpose:
 *  Returns another IEnumIA with the same state as ourselves.
 *
 * Parameters:
 *  ppEnum		  IXXXEnumIA * in which to return the
 *				  new object.
 */

STDMETHODIMP IXXXEnumIA::Clone(IEnumOPCItemAttributes **ppEnum)
{
	IXXXEnumIA	*pNew;

	*ppEnum = NULL;

	//Create the clone
	//
	pNew = new IXXXEnumIA(m_pUnkRef, m_cIA, m_pIA, m_pmem);

	if (NULL == pNew)
		return E_OUTOFMEMORY;

	pNew->AddRef();

	// Set the 'state' of the clone to match the state if this
	//
	pNew->m_iCur = m_iCur;

	*ppEnum = pNew;
	return S_OK;
}




