// OPCCommon.h: interface for the COPCCommon class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCCOMMON_H__5FC2DD68_4E51_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCCOMMON_H__5FC2DD68_4E51_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCComn.h"


class COPCCommon  
{
public:
	BOOL IsAvailable();
	COPCCommon();
	virtual ~COPCCommon();

	HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_SetLocaleID();
   HRESULT Try_GetLocaleID();
   HRESULT Try_QueryAvailableLocaleIDs();
	HRESULT Try_GetErrorString();
   HRESULT Try_SetClientName();

private:
   IOPCCommon*    m_pIOPCCommon;


};

#endif // !defined(AFX_OPCCOMMON_H__5FC2DD68_4E51_11D3_8764_00104BF10496__INCLUDED_)
