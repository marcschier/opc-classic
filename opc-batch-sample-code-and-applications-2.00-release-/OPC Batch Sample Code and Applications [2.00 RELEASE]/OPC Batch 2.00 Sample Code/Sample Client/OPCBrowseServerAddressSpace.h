// OPCBrowseServerAddressSpace.h: interface for the COPCBrowseServerAddressSpace class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCBROWSESERVERADDRESSSPACE_H__D0366BC3_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCBROWSESERVERADDRESSSPACE_H__D0366BC3_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"


class COPCBrowseServerAddressSpace  
{
public:
	BOOL IsAvailable();
	COPCBrowseServerAddressSpace();
	virtual ~COPCBrowseServerAddressSpace();

   HRESULT Initialize(IUnknown* pOPCBatchServer);

   HRESULT Try_QueryOrganization();
   HRESULT Try_ChangeBrowsePosition();
   HRESULT Try_BrowseOPCItemIDs();
   HRESULT Try_GetItemID();
   HRESULT Try_BrowseAccessPaths();

   HRESULT Try_ShowTree();

   void BrowseTags(OPCBROWSETYPE type);
   void ShowCurrentBrowsePos();

private:
	CString m_csItemID;
	OPCBROWSETYPE m_BrowseType;
	OPCBROWSEDIRECTION m_dwDir;
	CStringArray m_ItemList;
	OPCNAMESPACETYPE m_NameSpace;
   IOPCBrowseServerAddressSpace*    m_pIOPCBrowseServerAddressSpace;  
	TCHAR	m_wstrItemName[128];
};

#endif // !defined(AFX_OPCBROWSESERVERADDRESSSPACE_H__D0366BC3_4F03_11D3_8764_00104BF10496__INCLUDED_)
