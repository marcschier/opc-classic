// I_enumun.cpp
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
//  This file contains an implementation of an IEnumUnknown interface.
//  Note this is a fully 'generic' enumerator implementation.
//
//  Since the list of elements is buffered in the object
//  It is appropriate for enumerations with a 'reasonable' number of elements
//
//  There is no OPC or Server specific code here.
//
// See IEnumXXXX::Next in Win32SDK\OLE\Reference\Interfaces\IEnumXXXX
// for general guidelines for enumerators
//
//
// Modification Log:
//	Vers  Date     By    Notes
//	----  -------- ---   -----
//	0.00  12/10/96 ACC	Based on Brockschmidt (with improvements)
//

#define WIN32_LEAN_AND_MEAN
#include "windows.h"
#include "OLE2.h"

#include "ENUMHELP.h"


/*
 * IXXXEnumUnknown::IXXXEnumUnknown
 * IXXXEnumUnknown::~IXXXEnumUnknown
 *
 * Parameters (Constructor):
 *  pUnkRef LPUNKNOWN to use for reference counting (the 'parent').
 *  cUnk    ULONG number of LPUNKNOWNs in prgUnk
 *  prgUnk  LPUNKNOWN to the array to enumerate (we will make a local copy).
 */

IXXXEnumUnknown::IXXXEnumUnknown(LPUNKNOWN pUnkRef, ULONG cUnk
	, LPUNKNOWN *prgUnk, IMalloc * pmem)
{
	UINT		i;

	m_cRef=0;
	m_pUnkRef=pUnkRef;

	m_iCur=0;
	m_cUnk=cUnk;
	m_prgUnk=new LPUNKNOWN[(UINT)cUnk];
	m_pmem = pmem;

	if (NULL!=m_prgUnk)
	{
		for (i=0; i < cUnk; i++)
		{
			// Create a local copy of the IUnknown List
			// And AddRef to insure the objects are not deleted 
			// while the ENUM is using them!
			//
			m_prgUnk[i] = prgUnk[i];
			m_prgUnk[i]->AddRef();	
		}
	}

	return;
}


IXXXEnumUnknown::~IXXXEnumUnknown(void)
{
	unsigned int	i;

	if (NULL != m_prgUnk)
	{
		for (i=0; i < m_cUnk; i++)
		{
			// Release each object since this ENUM is done with it
			// 
			m_prgUnk[i]->Release();	
		}
		delete [] m_prgUnk;
	}
	return;
}






/*
 * IXXXEnumUnknown::QueryInterface
 * IXXXEnumUnknown::AddRef
 * IXXXEnumUnknown::Release
 *
 * Purpose:
 *  IUnknown members for IXXXEnumUnknown object.
 */

STDMETHODIMP IXXXEnumUnknown::QueryInterface(REFIID riid
	, LPVOID *ppv)
{
	*ppv=NULL;

	/*
	 * Enumerators are separate objects with their own
	 * QueryInterface behavior.
	 */
	if (IID_IUnknown==riid || IID_IEnumUnknown==riid)
		*ppv=(LPVOID)this;

	if (NULL!=*ppv)
	{
		((LPUNKNOWN)*ppv)->AddRef();
		return S_OK;
	}

	return E_NOINTERFACE;
}


STDMETHODIMP_(ULONG) IXXXEnumUnknown::AddRef(void)
{
	// Addref this object and also the 'parent' if any
	//
	++m_cRef;
	if(m_pUnkRef != NULL) 
		m_pUnkRef->AddRef();
	return m_cRef;
}

STDMETHODIMP_(ULONG) IXXXEnumUnknown::Release(void)
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
 * IXXXEnumUnknown::Next
 *
 * Purpose:
 *  Returns the next element in the enumeration.
 *
 * Parameters:
 *  cRequested		ULONG max number of LPUNKNOWNs to return.
 *  ppUnk		   LPUNKNOWN * in which to store the returned
 *				  pointer.
 *  pActual		  ULONG * in which to return how many we actually
 *				  enumerated.
 *
 * Return Value:
 *  HRESULT		 S_OK if successful, S_FALSE otherwise,
 */

STDMETHODIMP IXXXEnumUnknown::Next(ULONG cRequested, IUnknown **ppUnk
	, ULONG *pActual)
{
	ULONG			cReturn=0L;
	ULONG			maxcount = cRequested;

	*pActual = 0L;		// default
	*ppUnk = NULL;		// default

	// If this enumerator is empty - return FALSE
	//
	if (NULL==m_prgUnk)
		return S_FALSE;

	// If user passed null for count of items returned
	// Then he is only allowed to ask for 1 item
	//
	if (NULL == pActual)	
	{
		if (1L != cRequested)
			return E_POINTER;
	}

	// If we are at end of list return FALSE
	//
	if (m_iCur >= m_cUnk)
		return S_FALSE;


	// Return as many as we have left in list up to request count
	//
	while (m_iCur < m_cUnk && cRequested > 0)
	{
		ppUnk[cReturn] = m_prgUnk[m_iCur];

		if (NULL != ppUnk[cReturn])	// (should never be null)
			ppUnk[cReturn]->AddRef();

		// And move on to the next one
		//
		m_iCur++;
		cReturn++;
		cRequested--;
	}

	if (NULL != pActual)
		*pActual = cReturn;

	if (cReturn == maxcount) return S_OK;
	return S_FALSE;

}







/*
 * IXXXEnumUnknown::Skip
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

STDMETHODIMP IXXXEnumUnknown::Skip(ULONG cSkip)
{
	if (((m_iCur+cSkip) >= m_cUnk) || NULL==m_prgUnk)
		return S_FALSE;

	m_iCur+=cSkip;
	return S_OK;
}






/*
 * IXXXEnumUnknown::Reset
 *
 * Purpose:
 *  Resets the current element index in the enumeration to zero.
 *
 * Parameters:
 *  None
 */

STDMETHODIMP IXXXEnumUnknown::Reset(void)
{
	m_iCur=0;
	return S_OK;
}






/*
 * IXXXEnumUnknown::Clone
 *
 * Purpose:
 *  Returns another IEnumUnknown with the same state as ourselves.
 *
 * Parameters:
 *  ppEnum		  LPENUMUNKNOWN * in which to return the
 *				  new object.
 */

STDMETHODIMP IXXXEnumUnknown::Clone(LPENUMUNKNOWN *ppEnum)
{
	IXXXEnumUnknown   *pNew;

	*ppEnum=NULL;

	//Create the clone
	//
	pNew=new IXXXEnumUnknown(m_pUnkRef, m_cUnk, m_prgUnk, m_pmem);

	if (NULL==pNew)
		return E_OUTOFMEMORY;

	pNew->AddRef();

	// Set the 'state' of the clone to match the state if this
	//
	pNew->m_iCur=m_iCur;

	*ppEnum=pNew;
	return S_OK;
}
