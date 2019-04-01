// CUnknown.h: interface for the CUnknown class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_CUnknown_H__D0366BC1_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_CUnknown_H__D0366BC1_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CUnknown  
{
public:
	CUnknown();
	virtual ~CUnknown();
   virtual void CleanUp();

   HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_QueryInterface();
   HRESULT Try_AddRef();
   HRESULT Try_Release();

   BOOL IsAvailable() {return ((m_pIUnknown != NULL) ? TRUE : FALSE); };

private:
   IUnknown*    m_pIUnknown;   
};

#endif // !defined(AFX_CUnknown_H__D0366BC1_4F03_11D3_8764_00104BF10496__INCLUDED_)
