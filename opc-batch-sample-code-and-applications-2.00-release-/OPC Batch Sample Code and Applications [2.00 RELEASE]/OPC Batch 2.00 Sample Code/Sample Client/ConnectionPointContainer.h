// ConnectionPointContainer.h: interface for the CConnectionPointContainer class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_CONNECTIONPOINTCONTAINER_H__D0366BC5_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_CONNECTIONPOINTCONTAINER_H__D0366BC5_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"


class CConnectionPointContainer  
{
public:
	CConnectionPointContainer();
	virtual ~CConnectionPointContainer();

   HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_EnumConnectionPoints();
   HRESULT Try_FindConnectionPoint();

private:
   IConnectionPointContainer*    m_pIConnectionPointContainer;    
};

#endif // !defined(AFX_CONNECTIONPOINTCONTAINER_H__D0366BC5_4F03_11D3_8764_00104BF10496__INCLUDED_)
