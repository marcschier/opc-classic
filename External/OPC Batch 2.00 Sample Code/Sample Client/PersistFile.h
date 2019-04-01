// PersistFile.h: interface for the CPersistFile class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_PERSISTFILE_H__D0366BC4_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_PERSISTFILE_H__D0366BC4_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"


class CPersistFile  
{
public:
	CPersistFile();
	virtual ~CPersistFile();
   virtual void CleanUp();

   HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_IsDirty();
   HRESULT Try_Load();
   HRESULT Try_Save();
   HRESULT Try_SaveCompleted();
   HRESULT Try_GetCurFileName();

   BOOL IsAvailable() {return ((m_pIPersistFile != NULL && m_pIMalloc != NULL) ? TRUE : FALSE); };

private:
   IPersistFile*  m_pIPersistFile;    
   IMalloc*       m_pIMalloc;
};

#endif // !defined(AFX_PERSISTFILE_H__D0366BC4_4F03_11D3_8764_00104BF10496__INCLUDED_)
