// OPCEnumerationSets.h: interface for the COPCEnumerationSets class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCENUMERATIONSETS_H__D0366BC7_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCENUMERATIONSETS_H__D0366BC7_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"
#include "opcbc.h"

class COPCEnumerationSets  
{
public:
	COPCEnumerationSets();
	virtual ~COPCEnumerationSets();
   virtual void CleanUp();

   HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_QueryEnumerationSets();
   HRESULT Try_QueryEnumeration();
   HRESULT Try_QueryEnumerationList();

   BOOL IsAvailable() {return ((m_pIOPCEnumerationSets != NULL && m_pIMalloc != NULL) ? TRUE : FALSE); };

private:
   IOPCEnumerationSets* m_pIOPCEnumerationSets;   
   IMalloc*             m_pIMalloc;
};

#endif // !defined(AFX_OPCENUMERATIONSETS_H__D0366BC7_4F03_11D3_8764_00104BF10496__INCLUDED_)
