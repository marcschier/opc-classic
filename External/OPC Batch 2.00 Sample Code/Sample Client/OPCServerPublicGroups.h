// COPCServerPublicGroups.h: interface for the COPCServerPublicGroups class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_COPCServerPublicGroups_H__D0366BC2_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_COPCServerPublicGroups_H__D0366BC2_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"


class COPCServerPublicGroups  
{
public:
	COPCServerPublicGroups();
	virtual ~COPCServerPublicGroups();
   virtual void CleanUp();

   HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_GetPublicGroupByName();
   HRESULT Try_RemovePublicGroup();

private:
   IOPCServerPublicGroups*    m_pIOPCServerPublicGroups;   
};

#endif // !defined(AFX_COPCServerPublicGroups_H__D0366BC2_4F03_11D3_8764_00104BF10496__INCLUDED_)
