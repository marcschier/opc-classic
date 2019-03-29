// OPCServer.h: interface for the COPCServer class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCSERVER_H__5FC2DD59_4E51_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCSERVER_H__5FC2DD59_4E51_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"


class COPCServer  
{
public:
	COPCServer();
	virtual ~COPCServer();
   virtual void CleanUp();

   HRESULT Initialize(IUnknown* pOPCBatchServer);
   HRESULT Try_CreateGroupEnumerator();
   HRESULT Try_AddGroup();
   HRESULT Try_GetErrorString();
   HRESULT Try_GetGroupByName();
   HRESULT Try_RemoveGroup();
	HRESULT Try_GetStatus();
	HRESULT Try_CreateEnumerator();


   OPCHANDLE Try_GetGroupState(IOPCGroupStateMgt * pGRP);

   BOOL IsAvailable() {return ((m_pIOPCServer != NULL && m_pIMalloc != NULL) ? TRUE : FALSE); };

private:
   IOPCServer*    m_pIOPCServer;    

   IMalloc*       m_pIMalloc;

   IUnknown*      m_pGroup;

   DWORD	         m_hServerGroup1;
   DWORD	         m_hServerGroup2;
   DWORD	         m_hServerGroup3;

   IOPCGroupStateMgt*         m_pGRP1GSM;
   IOPCPublicGroupStateMgt*   m_pGRP1PGSM;
   IOPCSyncIO*                m_pGRP1SIO;
   IOPCAsyncIO*               m_pGRP1ASIO;
   IOPCAsyncIO2*              m_pGRP1ASIO2;
   IOPCItemMgt*               m_pGRP1IM;
   IDataObject*               m_pGRP1DO;
   IConnectionPointContainer* m_pGRP1CPC;

   IOPCGroupStateMgt*         m_pGRP2;


};

#endif // !defined(AFX_OPCSERVER_H__5FC2DD59_4E51_11D3_8764_00104BF10496__INCLUDED_)
