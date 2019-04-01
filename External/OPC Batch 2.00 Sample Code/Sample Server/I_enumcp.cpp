// I_enumcp.cpp
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
//  This file contains an implementation of an IEnumConnectionPoints interface.
//  Note this is a fully 'generic' enumerator implementation.
//
//  This code is virtually identical to IEnumUnknown (I_ENUMUN.CPP)
//
//
// Modification Log:
//	Vers  Date     By    Notes
//	----  -------- ---   -----
//	2.00  03/17/97 ACC
//        06/01/00 acc   fix to properly handle count of 0 in constructor
//

#define WIN32_LEAN_AND_MEAN
#include "windows.h"
#include "OLE2.h"

#include "OLECTL.h"
#include "ENUMCP.h"

/*
 * IXXXEnumConnectionPoints::IXXXEnumConnectionPoints
 * IXXXEnumConnectionPoints::~IXXXEnumConnectionPoints
 *
 * Parameters (Constructor):
 *  pUnkRef LPUNKNOWN to use for reference counting (the 'parent').
 *  cUnk    ULONG number of LPUNKNOWNs in prgUnk
 *  prgUnk  LPConnectionPoint to the array to enumerate (we will make a local copy).
 */

IXXXEnumConnectionPoints::IXXXEnumConnectionPoints(LPUNKNOWN pUnkRef, ULONG cUnk
	, IConnectionPoint **prgUnk)
{
	UINT		i;

	m_cRef=0;
	m_pUnkRef=pUnkRef;

	m_iCur=0;
	m_cUnk=cUnk;
	if(cUnk) m_prgUnk=new LPCONNECTIONPOINT[(UINT)cUnk];
	else m_prgUnk = NULL;

	if (NULL!=m_prgUnk)
	{
		for (i=0; i < cUnk; i++)
		{
			// Create a local copy of the IConnectionPoint List
			// And AddRef to insure the objects are not deleted 
			// while the ENUM is using them!
			//
			m_prgUnk[i] = prgUnk[i];
			m_prgUnk[i]->AddRef();	
		}
	}

	return;
}


IXXXEnumConnectionPoints::~IXXXEnumConnectionPoints(void)
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
 * IXXXEnumConnectionPoints::QueryInterface
 * IXXXEnumConnectionPoints::AddRef
 * IXXXEnumConnectionPoints::Release
 *
 * Purpose:
 *  IENUMConnectionPoints members for IXXXEnumConnectionPoints object.
 */

STDMETHODIMP IXXXEnumConnectionPoints::QueryInterface(REFIID riid
	, LPVOID *ppv)
{
	*ppv=NULL;

	/*
	 * Enumerators are separate objects with their own
	 * QueryInterface behavior.
	 */
	if (IID_IUnknown==riid || IID_IEnumConnectionPoints==riid)
		*ppv=(LPVOID)this;

	if (NULL!=*ppv)
	{
		((LPUNKNOWN)*ppv)->AddRef();
		return S_OK;
	}

	return E_NOINTERFACE;
}


STDMETHODIMP_(ULONG) IXXXEnumConnectionPoints::AddRef(void)
{
	// Addref this object and also the 'parent' if any
	//
	++m_cRef;
	if(m_pUnkRef != NULL) 
		m_pUnkRef->AddRef();
	return m_cRef;
}

STDMETHODIMP_(ULONG) IXXXEnumConnectionPoints::Release(void)
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
 * IXXXEnumConnectionPoints::Next
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

STDMETHODIMP IXXXEnumConnectionPoints::Next(ULONG cRequested, IConnectionPoint **ppUnk
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
 * IXXXEnumConnectionPoints::Skip
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

STDMETHODIMP IXXXEnumConnectionPoints::Skip(ULONG cSkip)
{
	if (((m_iCur+cSkip) >= m_cUnk) || NULL==m_prgUnk)
		return S_FALSE;

	m_iCur+=cSkip;
	return S_OK;
}






/*
 * IXXXEnumConnectionPoints::Reset
 *
 * Purpose:
 *  Resets the current element index in the enumeration to zero.
 *
 * Parameters:
 *  None
 */

STDMETHODIMP IXXXEnumConnectionPoints::Reset(void)
{
	m_iCur=0;
	return S_OK;
}






/*
 * IXXXEnumConnectionPoints::Clone
 *
 * Purpose:
 *  Returns another IEnumConnectionPoints with the same state as ourselves.
 *
 * Parameters:
 *  ppEnum		  LPENUMUNKNOWN * in which to return the
 *				  new object.
 */

STDMETHODIMP IXXXEnumConnectionPoints::Clone(LPENUMCONNECTIONPOINTS *ppEnum)
{
	IXXXEnumConnectionPoints   *pNew;

	*ppEnum=NULL;

	//Create the clone
	//
	pNew=new IXXXEnumConnectionPoints(m_pUnkRef, m_cUnk, m_prgUnk);

	if (NULL==pNew)
		return E_OUTOFMEMORY;

	pNew->AddRef();

	// Set the 'state' of the clone to match the state if this
	//
	pNew->m_iCur=m_iCur;

	*ppEnum=pNew;
	return S_OK;
}
