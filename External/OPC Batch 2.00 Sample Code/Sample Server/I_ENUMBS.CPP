// I_ENUMBS.cpp
//
// (c) Copyright 1999 The OPC Foundation
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
//  Will Irwin, Fisher-Rosemount, based on the earlier code by 
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//  This file contains an implementation of an IEnumOPCBatchSummary interface.
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
//  1.00  10/15/99 WGI   Adapted from ACC's IEnumOPCItemAttributes,
//                        (just copied his code and changed names)
//  2.00  11/02/00 WGI   Added a new constructor that takes an
//                         OPCBATCHSUMMARYFILTER parameter and
//                         produces a filtered summary of the batches.


#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include <assert.h>

extern OPCBBatchObj* BatchList[];
extern OPCBBatchObj* BatchArchiveList[];

/*
 * IXXXEnumBatchSummary::IXXXEnumBatchSummary
 * IXXXEnumBatchSummary::~IXXXEnumBatchSummary
 *
 * Parameters (Constructor):
 *  pUnkRef		 LPUNKNOWN to use for reference counting.
 *  pmem       IMalloc memory allocator to use for returned data
 */

IXXXEnumBatchSummary::IXXXEnumBatchSummary(LPUNKNOWN pUnkRef, IMalloc * pmem)
{
	UINT		i;

	m_cRef = 0;
	m_pUnkRef = pUnkRef;

	m_iCur = 0;
  m_cBS = 0;

  // Count the batches on the batch list
  OPCBBatchObj** ppBatch = &BatchList[0];
  while (*ppBatch++ != NULL)
  {
    m_cBS++;
  }

	m_pBS = new OPCBATCHSUMMARY[m_cBS];	// use local memory
	m_pmem = pmem;

	if (NULL != m_pBS)
	{
		// Create a OPCBATCHSUMMARY for each batch
		//
		for (i=0; i < m_cBS; i++)
		{
			CreateBatchSummary(&m_pBS[i], BatchList[i]);
		}
	}
	return;
}
/*
 * Parameters (Constructor):
 *  pUnkRef		 LPUNKNOWN to use for reference counting.
 *  pFilter    pointer to a filter structure
 *  szModel    name of model to base the summary on
 *  pmem       IMalloc memory allocator to use for returned data
 */
 
IXXXEnumBatchSummary::IXXXEnumBatchSummary(LPUNKNOWN pUnkRef, 
                                           OPCBATCHSUMMARYFILTER* pFilter,
                                           int iModel,
                                           IMalloc * pmem)
{
  UINT		i;
	m_cRef = 0;
	m_pUnkRef = pUnkRef;

	m_iCur = 0;
  m_cBS = 0;

  // Set up to look at the appropriate batch list based on iModel
  // BatchList and BatchArchiveList are arrays of pointers to 
  // OPCBBatchObj*.
  OPCBBatchObj** pList;
  switch (iModel)
  {
  case 1:
    pList = BatchList;
    break;  
  case 2:
    pList = BatchArchiveList;
    break;
  default:
    // This is a constructor so we can't return an error: 
    // anyway this should be impossible unless there is
    // coding error in the caller.
    assert(false);  // bad parameter iModel
  }

  // Count the batches on the batch list.  This will count
  // them all regardless of the filter, so we may allocate
  // more memory than we really need.  The m_cBS member will
  // count only those that pass the filter, so the Next, Count
  // etc methods will be correct.   A real server might
  // want to do something more intelligent here, especially
  // if very large numbers of batches are expected.
  UINT cAllBatches = 0;
  OPCBBatchObj** ppBatch = &pList[0];
  while (*ppBatch++ != NULL)
  {
    cAllBatches++;
  }

	m_pBS = new OPCBATCHSUMMARY[cAllBatches];	// use local memory
	m_pmem = pmem;

	if (NULL != m_pBS)
	{
		// Create a OPCBATCHSUMMARY for each batch
		// that passes the filter criteria.
		for (i=0; i < cAllBatches; i++)
		{
      if (pList[i]->PassesFilter(pFilter))
      {
  			CreateBatchSummary(&m_pBS[m_cBS], pList[i]);
        m_cBS++;
      }
		}
	}
	return;
}



IXXXEnumBatchSummary::~IXXXEnumBatchSummary(void)
{
	unsigned int i;

	if (NULL!=m_pBS)
	{
		// Delete the local copies of all the BSs
		//
		for (i=0; i < m_cBS; i++)
		{
			BSFree(&m_pBS[i], NULL);
		}
		delete [] m_pBS;
	}
	return;
}

/*
 * IXXXEnumBatchSummary::QueryInterface
 * IXXXEnumBatchSummary::AddRef
 * IXXXEnumBatchSummary::Release
 *
 * Purpose:
 *  IUnknown members for IXXXEnumBatchSummary object.
 */

STDMETHODIMP IXXXEnumBatchSummary::QueryInterface(REFIID riid
	, LPVOID *ppv)
{
	*ppv=NULL;

	if (IID_IUnknown==riid || IID_IEnumOPCBatchSummary==riid)
		*ppv=(LPVOID)this;

	if (NULL!=*ppv)
	{
		((LPUNKNOWN)*ppv)->AddRef();
		return S_OK;
	}

	return E_NOINTERFACE;
}


STDMETHODIMP_(ULONG) IXXXEnumBatchSummary::AddRef(void)
{
	// Addref this object and also the 'parent' if any
	//
	++m_cRef;
	if(m_pUnkRef != NULL) 
		m_pUnkRef->AddRef();
	return m_cRef;
}

STDMETHODIMP_(ULONG) IXXXEnumBatchSummary::Release(void)
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
 * IXXXEnumBatchSummary::Next
 *
 * Purpose:
 *  Returns the next element in the enumeration.
 *
 * Parameters:
 *  cBS			ULONG max number of OPCBATCHSUMMARYs to return.
 *  pBS			OPCBATCHSUMMARY(s) in which to store the returned
 *				  structures.
 *  pulstr		  ULONG * in which to return how many we
 *				  actually returned.
 *
 * Return Value:
 *  HRESULT		 S_OK if successful, S_FALSE otherwise,
 */

STDMETHODIMP IXXXEnumBatchSummary::Next(ULONG cBS, OPCBATCHSUMMARY **pBS
	, ULONG *pActual)
{
	ULONG		cReturn = 0L;
	ULONG		maxcount = cBS;
	OPCBATCHSUMMARY * temp;

	*pActual = 0L;	// default return count
	*pBS = NULL;

	// If this enumerator is empty - return FALSE (should never happen)
	// 
	if (NULL == m_pBS)
		return S_FALSE;

	// If user passed null for count of items returned
	// Then he is only allowed to ask for 1 item
	//
	if (NULL == pActual)
	{
		if (1L != cBS)
			return E_POINTER;
	}

	// If we are at end of list return FALSE
	//
	if (m_iCur >= m_cBS)
		return S_FALSE;

	// Allocate the items to be returned
	//
	temp = (OPCBATCHSUMMARY*)pIMalloc->Alloc(cBS * sizeof(OPCBATCHSUMMARY));
	if(temp == NULL) return S_FALSE;
	
	// Return as many as we have left in list up to request count
	//
	while (m_iCur < m_cBS && cBS > 0)
	{
		// Fill in an BS to return to caller
		BSClone(&temp[cReturn], &m_pBS[m_iCur], m_pmem);	// Use global memory for strings

		// And move on to the next one
		//
		m_iCur++;
		cReturn++;
		cBS--;
	}

	// Shorten the block to the size actually used
	// and return the final pointer to the caller
	//
	*pBS = (OPCBATCHSUMMARY*)pIMalloc->Realloc(temp, cReturn * sizeof(OPCBATCHSUMMARY));

	if (NULL != pActual)
		*pActual = cReturn;

	if (cReturn == maxcount) return S_OK;
	return S_FALSE;
}







/*
 * IXXXEnumBatchSummary::Skip
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

STDMETHODIMP IXXXEnumBatchSummary::Skip(ULONG cSkip)
{
	if (((m_iCur+cSkip) >= m_cBS) || NULL==m_pBS)
		return S_FALSE;

	m_iCur+=cSkip;
	return S_OK;
}






/*
 * IXXXEnumBatchSummary::Reset
 *
 * Purpose:
 *  Resets the current element index in the enumeration to zero.
 *
 * Parameters:
 *  None
 */

STDMETHODIMP IXXXEnumBatchSummary::Reset(void)
{
	m_iCur=0;
	return S_OK;
}






/*
 * IXXXEnumBatchSummary::Clone
 *
 * Purpose:
 *  Returns another IEnumBS with the same state as ourselves.
 *
 * Parameters:
 *  ppEnum		  IXXXEnumBatchSummary * in which to return the
 *				  new object.
 */

STDMETHODIMP IXXXEnumBatchSummary::Clone(IEnumOPCBatchSummary **ppEnum)
{
	IXXXEnumBatchSummary	*pNew;

	*ppEnum = NULL;

	//Create the clone
	//
	pNew = new IXXXEnumBatchSummary(m_pUnkRef, m_pmem);

	if (NULL == pNew)
		return E_OUTOFMEMORY;

	pNew->AddRef();

	// Set the 'state' of the clone to match the state if this
	//
	pNew->m_iCur = m_iCur;

	*ppEnum = pNew;
	return S_OK;
}

/*
 * IXXXEnumBatchSummary::Count
 *
 * Purpose:
 *  Returns the count of elements in the collection
 *  This is the number of batches in the batch list
 *  at the time the enumerator was created
 *
 * Parameters
 *  pcelt   ULONG*   in which to return the count
 *
 */

STDMETHODIMP IXXXEnumBatchSummary::Count(ULONG *pcelt)
{
  *pcelt = m_cBS;
  return S_OK;
}



///////////////////////////////////////
/// Create an OPCBATCHSUMMARY from the
/// OPCBBatchObj that we use in the
/// sample server to represent a batch
///
void IXXXEnumBatchSummary::CreateBatchSummary(OPCBATCHSUMMARY* pBS, OPCBBatchObj* pBatch)
{
  // In the OPCBBatchObj, all the data is in the form of 
  // Properties which are held as VARIANTs.  We need to convert
  // these to the format used by the fields of OPCBATCHSUMMARY.
  VARIANT v;
  VariantInit(&v);
  HRESULT hr;

  pBS->szID = WSTRClone(pBatch->GetName(), NULL);

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_DESC, v);
  if (SUCCEEDED(hr))
  {
    pBS->szDescription = WSTRClone(V_BSTR(&v), NULL);
  }

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_ID, v);
  if (SUCCEEDED(hr))
  {
    pBS->szOPCItemID = WSTRClone(V_BSTR(&v), NULL);
  }

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_MASTER_RECIPE_ID, v);
  if (SUCCEEDED(hr))
  {
    pBS->szMasterRecipeID = WSTRClone(V_BSTR(&v), NULL);
  }

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_BATCH_SIZE, v);
  if (SUCCEEDED(hr))
  {
    // The OPCBATCHSUMMARY fBatchSize member is a float.  In this
    // sample server we hold the batch size property as a double.
    // So it will get truncated by this assignment.
    pBS->fBatchSize = (float)V_R8(&v);
  }

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_EU, v);
  if (SUCCEEDED(hr))
  {
    pBS->szEU = WSTRClone(V_BSTR(&v), NULL);
  }

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_EXECUTION_STATE, v);
  if (SUCCEEDED(hr))
  {
    pBS->szExecutionState = WSTRClone(V_BSTR(&v), NULL);
  }

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_EXECUTION_MODE, v);
  if (SUCCEEDED(hr))
  {
    pBS->szExecutionMode = WSTRClone(V_BSTR(&v), NULL);
  }

  // The start time and end time properties are held as V_DATE
  // We need to convert them into FILETIME format.
  hr = pBatch->GetPropertyAtID(OPC_PROP_B_ACTUAL_START_TIME, v);
  if (SUCCEEDED(hr))
  {
    DATE date;
    SYSTEMTIME st;
    date = V_DATE(&v);
    VariantTimeToSystemTime(date, &st);
    SystemTimeToFileTime(&st, &pBS->ftActualStartTime);
  }

  hr = pBatch->GetPropertyAtID(OPC_PROP_B_ACTUAL_END_TIME, v);
  if (SUCCEEDED(hr))
  {
    DATE date;
    SYSTEMTIME st;
    date = V_DATE(&v);
    VariantTimeToSystemTime(date, &st);
    SystemTimeToFileTime(&st, &pBS->ftActualEndTime);
  }
}