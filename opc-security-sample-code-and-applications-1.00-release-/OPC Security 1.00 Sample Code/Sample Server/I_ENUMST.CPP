// I_enumst.cpp
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
//  This file contains an implementation of an IEnumString interface.
//  Note this is a fully 'generic' enumerator implementation.
//
//  Since the list of elements is buffered in the object
//  It is appropriate for enumerations with a 'reasonable' number of elements
//
//  There is no OPC or Server specific code here.
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
//	0.00  12/10/96 ACC   Based on Brockschmidt (with improvements)
//

#define WIN32_LEAN_AND_MEAN
#include "windows.h"
#include "OLE2.h"

#include "ENUMHELP.h"


/*
 * IXXXEnumString::IXXXEnumString
 * IXXXEnumString::~IXXXEnumString
 *
 * Parameters (Constructor):
 *  pUnkRef		 LPUNKNOWN to use for reference counting.
 *  cstr			ULONG number of LPOLESTRs in pstr
 *  prgstr		  LPOLESTR to the array to enumerate. (note OLESTR == WSTR)
 *  pmem       IMalloc memory allocator to use for returned strings
 */

IXXXEnumString::IXXXEnumString(LPUNKNOWN pUnkRef, ULONG cstr
	, LPOLESTR *prgstr, IMalloc * pmem)
{
	UINT		i;

	m_cRef = 0;
	m_pUnkRef = pUnkRef;

	m_iCur = 0;
	m_cstr = cstr;
	m_prgstr = new LPOLESTR[cstr];
	m_pmem = pmem;

	if (NULL != m_prgstr)
	{
		// Make local copies of all the strings
		//
		for (i=0; i < m_cstr; i++)
		{
			m_prgstr[i] = new WCHAR[wcslen(prgstr[i]) + 1];
			wcscpy(m_prgstr[i], prgstr[i]);
		}
	}
	return;
}


IXXXEnumString::~IXXXEnumString(void)
{
	unsigned int i;

	if (NULL!=m_prgstr)
	{
		// Delete the local copies of all the strings
		//
		for (i=0; i < m_cstr; i++)
		{
			delete [] m_prgstr[i];
		}
		delete [] m_prgstr;
	}
	return;
}






/*
 * IXXXEnumString::QueryInterface
 * IXXXEnumString::AddRef
 * IXXXEnumString::Release
 *
 * Purpose:
 *  IUnknown members for IXXXEnumString object.
 */

STDMETHODIMP IXXXEnumString::QueryInterface(REFIID riid
	, LPVOID *ppv)
{
	*ppv=NULL;

	if (IID_IUnknown==riid || IID_IEnumString==riid)
		*ppv=(LPVOID)this;

	if (NULL!=*ppv)
	{
		((LPUNKNOWN)*ppv)->AddRef();
		return S_OK;
	}

	return E_NOINTERFACE;
}


STDMETHODIMP_(ULONG) IXXXEnumString::AddRef(void)
{
	// Addref this object and also the 'parent' if any
	//
	++m_cRef;
	if(m_pUnkRef != NULL) 
		m_pUnkRef->AddRef();
	return m_cRef;
}

STDMETHODIMP_(ULONG) IXXXEnumString::Release(void)
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
 * IXXXEnumString::Next
 *
 * Purpose:
 *  Returns the next element in the enumeration.
 *
 * Parameters:
 *  cstr			ULONG max number of LPOLESTRs to return.
 *  pstr			LPOLESTR(s) in which to store the returned
 *				  structures.
 *  pulstr		  ULONG * in which to return how many we
 *				  actually returned.
 *
 * Return Value:
 *  HRESULT		 S_OK if successful, S_FALSE otherwise,
 */

STDMETHODIMP IXXXEnumString::Next(ULONG cstr, LPOLESTR *pstr
	, ULONG *pulstr)
{
	ULONG		cReturn = 0L;
	ULONG		maxcount = cstr;

	*pulstr = 0L;	// default return count
	*pstr = NULL;	// default return pointer

	// If this enumerator is empty - return FALSE (should never happen)
	// 
	if (NULL == m_prgstr)
		return S_FALSE;

	// If user passed null for count of items returned
	// Then he is only allowed to ask for 1 item
	//
	if (NULL == pulstr)
	{
		if (1L != cstr)
			return E_POINTER;
	}

	// If we are at end of list return FALSE
	//
	if (m_iCur >= m_cstr)
		return S_FALSE;

	// Return as many as we have left in list up to request count
	//
	while (m_iCur < m_cstr && cstr > 0)
	{
		int	size;

		// Compute WCHARs in string (will be at least 1 for nul).
		//
		size = (wcslen(m_prgstr[m_iCur])+1);

		pstr[cReturn] = (WCHAR*)m_pmem->Alloc(size * sizeof(WCHAR));
		if(pstr[cReturn])
		{
			wcscpy(pstr[cReturn], m_prgstr[m_iCur]);
		}

		// And move on to the next one
		//
		m_iCur++;
		cReturn++;
		cstr--;
	}

	if (NULL != pulstr)
		*pulstr = cReturn;

	if (cReturn == maxcount) return S_OK;
	return S_FALSE;
}







/*
 * IXXXEnumString::Skip
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

STDMETHODIMP IXXXEnumString::Skip(ULONG cSkip)
{
	if (((m_iCur+cSkip) >= m_cstr) || NULL==m_prgstr)
		return S_FALSE;

	m_iCur+=cSkip;
	return S_OK;
}






/*
 * IXXXEnumString::Reset
 *
 * Purpose:
 *  Resets the current element index in the enumeration to zero.
 *
 * Parameters:
 *  None
 */

STDMETHODIMP IXXXEnumString::Reset(void)
{
	m_iCur=0;
	return S_OK;
}






/*
 * IXXXEnumString::Clone
 *
 * Purpose:
 *  Returns another IEnumString with the same state as ourselves.
 *
 * Parameters:
 *  ppEnum		  LPENUMSTRING * in which to return the
 *				  new object.
 */

STDMETHODIMP IXXXEnumString::Clone(LPENUMSTRING *ppEnum)
{
	IXXXEnumString	*pNew;

	*ppEnum=NULL;

	//Create the clone
	//
	pNew=new IXXXEnumString(m_pUnkRef, m_cstr, m_prgstr, m_pmem);

	if (NULL==pNew)
		return E_OUTOFMEMORY;

	pNew->AddRef();

	// Set the 'state' of the clone to match the state if this
	//
	pNew->m_iCur=m_iCur;

	*ppEnum=pNew;
	return S_OK;
}
