// OPCCPC.h: interface for the COPCCPC class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCCPC_H__6650DA97_5642_11D3_807E_00A024CE4FA0__INCLUDED_)
#define AFX_OPCCPC_H__6650DA97_5642_11D3_807E_00A024CE4FA0__INCLUDED_

#include "IClientShutDown.h"	// Added by ClassView
#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"

class COPCCPC {
public:
	BOOL IsAdvised();
	HRESULT Try_UnAdvise();
	HRESULT Try_FindConnectionPoint();
	HRESULT Try_EnumConnectionPoints();
	HRESULT Initialize(IUnknown* pOPCBatchServer);
	BOOL IsAvailable();
	COPCCPC();
	virtual ~COPCCPC();

private:
	IConnectionPoint* m_pCallbackCP;
	DWORD m_MyCookie;
	IClientShutdown* m_pMyCallback;
	IConnectionPointContainer* m_pICPC;
};

#endif // !defined(AFX_OPCCPC_H__6650DA97_5642_11D3_807E_00A024CE4FA0__INCLUDED_)
