// OPCItemProperties.h: interface for the COPCItemProperties class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCITEMPROPERTIES_H__D0366BC6_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCITEMPROPERTIES_H__D0366BC6_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"


class COPCItemProperties  
{
public:
	COPCItemProperties();
	virtual ~COPCItemProperties();
   virtual void CleanUp();

   HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_QueryAvailableProperties();
   HRESULT Try_GetItemProperties();
   HRESULT Try_LookupItemIDs();

   HRESULT Try_Parameters();
   HRESULT Try_Results();


   BOOL IsAvailable() {return ((m_pIOPCItemProperties != NULL && m_pIMalloc != NULL) ? TRUE : FALSE); };

private:
   IOPCItemProperties*  m_pIOPCItemProperties;    
   IMalloc*             m_pIMalloc;
};

#endif // !defined(AFX_OPCITEMPROPERTIES_H__D0366BC6_4F03_11D3_8764_00104BF10496__INCLUDED_)
