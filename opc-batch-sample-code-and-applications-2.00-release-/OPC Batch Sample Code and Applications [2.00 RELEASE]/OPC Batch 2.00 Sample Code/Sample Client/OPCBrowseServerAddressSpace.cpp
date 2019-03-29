// OPCBrowseServerAddressSpace.cpp: implementation of the COPCBrowseServerAddressSpace class.
//
/////////////////////////////////////////////////////////////////////////////
// (c) Copyright 2000 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist
//  in understanding the OPC Specifications and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Dave Emerson and Jack Beaudry, Yokogawa Electric Company.
//  This application is based on the code for the sample DA client
//  generously provided by Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
// .4    01/12/00 jb    preliminary release for testing.
// .42   01/20/00 jb    release interface pointer in destructor
//                      remove eroneous error message from GetItemID

#include "stdafx.h"
#include "AFXPRIV.H"          // conversions
#include "OPCBClient.h"
#include "OPCBrowseServerAddressSpace.h"
#include "DiagBrowser.h"
#include "utility.h"
#include "EditDlg.h"          // simple edit dialog to get user input

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCBrowseServerAddressSpace::COPCBrowseServerAddressSpace()
{
	m_pIOPCBrowseServerAddressSpace = NULL;
}

COPCBrowseServerAddressSpace::~COPCBrowseServerAddressSpace()
{
	if (m_pIOPCBrowseServerAddressSpace)
		m_pIOPCBrowseServerAddressSpace->Release();

}
HRESULT COPCBrowseServerAddressSpace::Initialize(IUnknown* pOPCBatchServer)
{
   // initialize member pointer to IOPCBrowseServerAddressSpace interface.
   HRESULT hr = pOPCBatchServer->QueryInterface(IID_IOPCBrowseServerAddressSpace, 
		(void**)&m_pIOPCBrowseServerAddressSpace);
   if (FAILED(hr))
	{
      m_pIOPCBrowseServerAddressSpace = NULL;   
	   DisplayOutput(ERROR_OUT, _T("Unable to get IOPCBrowseServerAddressSpace interface. %s"), FormatHresult(hr));
	}
	else
	{
		m_ItemList.RemoveAll();
		m_csItemID = "Root";
	}
   return hr;
}




//////////////////////////////////////////////////////////////////////
// Method Test Functions
//////////////////////////////////////////////////////////////////////

HRESULT COPCBrowseServerAddressSpace::Try_ChangeBrowsePosition()
{

   DisplayOutput(VERBOSE_OUT, _T("COPCBrowseServerAddressSpace::Try_ChangeBrowsePosition."));

   HRESULT hr = S_OK;
  
	USES_CONVERSION;
	CDiagBrowser	dlg;
	
	//Set up options.
	if( m_ItemList.GetSize() < 0 ) //$TODO when would it ever be less than 0?
		return S_FALSE;
	else 
      dlg.m_csItemID = m_csItemID;

	dlg.m_pDataList = &m_ItemList;
	dlg.m_csOption[0] = "Browse Down";
	dlg.m_csOption[1] = "Browse Up";
	dlg.m_csOption[2] = "Browse To";
	dlg.m_nOption = 0;

	if( dlg.DoModal() == IDOK )
	{	
      m_csItemID = dlg.m_csItemID;
		CString	csDir;
		switch( dlg.m_nOption )
		{
		case 0:
			m_dwDir = OPC_BROWSE_DOWN;
			csDir = "OPC_BROWSE_DOWN";
			break;
		case 1:
			m_dwDir = OPC_BROWSE_UP;
			csDir = "OPC_BROWSE_UP";
			break;
		case 2:
			m_dwDir = OPC_BROWSE_TO;
			csDir = "OPC_BROWSE_TO";
			break;
		}

      DisplayOutput(VERBOSE_OUT, _T("User Entered %s, %s"), m_csItemID, csDir);	

		hr = m_pIOPCBrowseServerAddressSpace->ChangeBrowsePosition(m_dwDir, A2CW(dlg.m_csItemID));
		if (FAILED(hr)) 
			DisplayOutput(ERROR_OUT, _T("IOPCBrowseServerAddressSpace::ChangeBrowsePosition[%s] Error = %s"), dlg.m_csItemID, FormatHresult(hr));
		else 
		{
/*
			switch( dlg.m_nOption )
			{
			case 1:
				m_csItemID.Format( "Up Branch of %s",  dlg.m_csItemID);
				break;
			case 0:
				m_csItemID.Format( "Down Branch of %s",  dlg.m_csItemID);
				break;
			case 2:
				break;
			}
*/
			DisplayOutput(NORMAL_OUT, _T("IOPCBrowseServerAddressSpace::ChangeBrowsePosition[%s] %s - OK"), csDir, dlg.m_csItemID);	

         ShowCurrentBrowsePos();      
      }
	}
	return hr;
}


HRESULT COPCBrowseServerAddressSpace::Try_GetItemID()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCBrowseServerAddressSpace::Try_GetItemID."));

   HRESULT	hr;
	LPWSTR	lpItemID;

   
   // Get number of items to fetch
   CEditDlg dlg(_T("Enter ItemID"), _T("Please enter the item ID:"));
   dlg.m_strEdit = m_csItemID;

   if (dlg.DoModal() != IDOK)
      return S_OK;

   DisplayOutput(VERBOSE_OUT, _T("User entered: %s"), dlg.m_strEdit);

	USES_CONVERSION;	

	hr = m_pIOPCBrowseServerAddressSpace->GetItemID(A2W(dlg.m_strEdit), &lpItemID);
	if (FAILED(hr)) 
		DisplayOutput(ERROR_OUT, _T("Error from IOPCBrowseServerAddressSpace::GetItemID of %s Error = %s"), dlg.m_strEdit, FormatHresult(hr));
	else
	{
		DisplayOutput(NORMAL_OUT, _T("IOPCBrowseServerAddressSpace::GetItemID for %s = %ls"), dlg.m_strEdit, lpItemID );
		((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(lpItemID);
	}
   return hr;
}


HRESULT COPCBrowseServerAddressSpace::Try_QueryOrganization()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCBrowseServerAddressSpace::Try_QueryOrganization."));

   HRESULT hr;

	hr = m_pIOPCBrowseServerAddressSpace->QueryOrganization( &m_NameSpace );
	if (FAILED(hr)) 
		DisplayOutput(ERROR_OUT, _T("IOPCBrowseServerAddressSpace::QueryOrganization() Error = %s"), FormatHresult(hr));
	else 
	{
		CString csTmp;
		if( m_NameSpace == OPC_NS_HIERARCHIAL )
			csTmp.Format( "IOPCBrowseServerAddressSpace::QueryOrganization() = OPC_NS_HIERARCHIAL [%ld]",
			m_NameSpace );
		else if( m_NameSpace == OPC_NS_FLAT )
			csTmp.Format( "IOPCBrowseServerAddressSpace::QueryOrganization() = OPC_NS_FLAT [%ld]",
			m_NameSpace );
		else
			csTmp.Format( "IOPCBrowseServerAddressSpace::QueryOrganization() = [%ld]",	m_NameSpace );
		DisplayOutput(NORMAL_OUT, _T( csTmp ) );
	}
   return hr;
}

HRESULT COPCBrowseServerAddressSpace::Try_BrowseOPCItemIDs()
{

   DisplayOutput(VERBOSE_OUT, _T("COPCBrowseServerAddressSpace::Try_BrowseOPCItemIDs."));
   
   HRESULT hr = S_OK;
	IEnumString* pIEnumString;
	//Set up options.
	CDiagBrowser	dlg;
	CStringArray csTmpList;

	if( m_ItemList.GetSize() < 0 )
		return S_FALSE;
	else 
		dlg.m_csItemID = m_csItemID;

	csTmpList.Add( dlg.m_csItemID );
	dlg.m_pDataList = &csTmpList;
	dlg.m_csOption[0] = "Browse Branch";
	dlg.m_csOption[1] = "Browse Leaf";
	dlg.m_csOption[2] = "Browse Flat";
	dlg.m_nOption = 0;

	if( dlg.DoModal() == IDOK )
	{
		switch( dlg.m_nOption )
		{
		case 0:
			m_BrowseType = OPC_BRANCH;
         DisplayOutput(VERBOSE_OUT, _T("User Entered %s, OPC_BRANCH"), dlg.m_csItemID);	
			break;
		case 1:
			m_BrowseType = OPC_LEAF;
         DisplayOutput(VERBOSE_OUT, _T("User Entered %s, OPC_LEAF"), dlg.m_csItemID);	
			break;
		case 2:
			m_BrowseType = OPC_FLAT;
         DisplayOutput(VERBOSE_OUT, _T("User Entered %s, OPC_FLAT"), dlg.m_csItemID);	
			break;
		}

		hr = m_pIOPCBrowseServerAddressSpace->BrowseOPCItemIDs(m_BrowseType, L"*", VT_EMPTY, 0, &pIEnumString);
      if (FAILED(hr)) 
			DisplayOutput(ERROR_OUT, _T("IOPCBrowseServerAddressSpace::BrowseOPCItemIDs() Error = %s"), FormatHresult(hr));
      else {
			int nMemberCount = 0;
			BOOL bCheckOnce = TRUE;
			LPWSTR pString;
			ULONG	nSize;
			while( pIEnumString->Next(1, &pString, &nSize) == S_OK)
			{
				DisplayOutput(NORMAL_OUT, _T("IOPCBrowseServerAddressSpace::BrowseOPCItemIDs() returned \"%ls\""), pString);
				if( m_BrowseType == OPC_BRANCH )
				{
					if( bCheckOnce )
					{
						bCheckOnce = FALSE;
						m_ItemList.RemoveAll();
					}
					m_ItemList.Add( pString );
				}
				((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(pString);
				nMemberCount++;
			}
			pIEnumString->Release();
			DisplayOutput(NORMAL_OUT, _T("IOPCBrowseServerAddressSpace::BrowseOPCItemIDs() Found %d ItemIDs"), nMemberCount);
			if( nMemberCount && m_ItemList.GetSize())
				m_csItemID = m_ItemList[0];
		}
	}
   return hr;
}

HRESULT COPCBrowseServerAddressSpace::Try_BrowseAccessPaths()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCBrowseServerAddressSpace::Try_BrowseAccessPaths."));

   HRESULT	hr;
	IEnumString* pIEnumString;


   // Get number of items to fetch
   CEditDlg dlg(_T("Enter ItemID"), _T("Please enter the item ID to browse:"));
   if (dlg.DoModal() != IDOK)
      return S_OK;
   
   DisplayOutput(VERBOSE_OUT, _T("User Entered %s"), dlg.m_strEdit);	


	USES_CONVERSION;
	hr = m_pIOPCBrowseServerAddressSpace->BrowseAccessPaths(A2W(dlg.m_strEdit), &pIEnumString);
   if (FAILED(hr)) {
		DisplayOutput(ERROR_OUT, _T("IOPCBrowseServerAddressSpace::BrowseAccessPaths() for ID = %ls Error= %s"),
		A2W(dlg.m_strEdit), FormatHresult(hr));
   }
	else 	{
		DisplayOutput(NORMAL_OUT, _T("IOPCBrowseServerAddressSpace::BrowseAccessPaths() OK"));	
		
		LPWSTR pString;
		ULONG	nSize;
		while( pIEnumString->Next(1, &pString, &nSize) == S_OK)
		{
	      DisplayOutput(NORMAL_OUT, _T("IOPCBrowseServerAddressSpace::BrowseAccessPaths() returned \"%ls\""), W2CA(pString));
		   ((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(pString);
		}
		pIEnumString->Release();
	}
   return hr;
}


BOOL COPCBrowseServerAddressSpace::IsAvailable()
{
	return( m_pIOPCBrowseServerAddressSpace != NULL );
}



HRESULT COPCBrowseServerAddressSpace::Try_ShowTree()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCBrowseServerAddressSpace::Try_ShowTree."));

	HRESULT r1;
	OPCNAMESPACETYPE org;

	r1 = m_pIOPCBrowseServerAddressSpace->QueryOrganization(&org);

	if (FAILED(r1))
	{
		DisplayOutput(ERROR_OUT, _T("Error from QueryOrganization: %s"), FormatHresult(r1));
		return E_FAIL;
	}
	DisplayOutput(NORMAL_OUT, _T("QueryOrganization returns: %d"), org);

	// Start to browse at ROOT
	//
	r1 = m_pIOPCBrowseServerAddressSpace->ChangeBrowsePosition(OPC_BROWSE_TO, L"");
	if (FAILED(r1))
	{
		DisplayOutput(ERROR_OUT, _T("Error from ChangeBrowsePosition: %s"), FormatHresult(r1));
		DisplayOutput(ERROR_OUT, _T("Continuing with Browse."));
	}


	// And Browse the Tags
	//
	DisplayOutput(NORMAL_OUT, _T("Leafs:"));
	BrowseTags(OPC_LEAF);

	// And Browse the Branches
	//
	DisplayOutput(NORMAL_OUT, _T("Branchs:"));
	BrowseTags(OPC_BRANCH);

   return S_OK;
}




void COPCBrowseServerAddressSpace::BrowseTags(OPCBROWSETYPE type)
{
	HRESULT r1;
	IEnumString * pEnum;

	r1 = m_pIOPCBrowseServerAddressSpace->BrowseOPCItemIDs(type, L"*", VT_EMPTY, 
		OPC_READABLE|OPC_WRITEABLE, &pEnum);
	if (FAILED(r1))
	{
		DisplayOutput(ERROR_OUT, _T("Error from BrowseItemIDs: %s"), FormatHresult(r1));
		return;
	}

	HRESULT r2 = S_OK;
	LPOLESTR pStr;
	LPOLESTR pStr2;
	ULONG actual;

	// It would be nice to test with more than '1' later
	//
	while((r2 = pEnum->Next(1, &pStr, &actual)) == S_OK)
	{
		HRESULT r3;

		// Print the results incnluding GetItemID Test
		//
		DisplayOutput(NORMAL_OUT, _T("EnumString returns: %ls /"), pStr);
		r3 = m_pIOPCBrowseServerAddressSpace->GetItemID(pStr, &pStr2);
		DisplayOutput(NORMAL_OUT, _T("GetITEMID returns: %ls"), pStr2);


		// If this is a branch then browse it as well...
		//
		if(type == OPC_BRANCH)
		{
			r1 = m_pIOPCBrowseServerAddressSpace->ChangeBrowsePosition(OPC_BROWSE_DOWN, pStr);
			if (FAILED(r1))
			{
				DisplayOutput(ERROR_OUT, _T("Error from ChangeBrowsePosition: %s"), FormatHresult(r1));
				DisplayOutput(ERROR_OUT, _T("Continuing with Browse."));
			} else
			{
				// And Browse the Tags and Branches
				//
				DisplayOutput(NORMAL_OUT, _T("Leafs:"));
				BrowseTags(OPC_LEAF);

				// And Browse the Branches
				//
				DisplayOutput(NORMAL_OUT, _T("Branchs:"));
				BrowseTags(OPC_BRANCH);

				// And restore Browse Position
				//
				r1 = m_pIOPCBrowseServerAddressSpace->ChangeBrowsePosition(OPC_BROWSE_UP, L"");
			}
		}
		// Free the string(s) that Next Returned
		//
		((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(pStr);
		if (pStr2) ((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(pStr2);
	}
	if (FAILED(r2))
	{
		DisplayOutput(ERROR_OUT, _T("Error from Next() %s"), FormatHresult(r2));
	}
		
	// Release the interface
	//
	pEnum->Release();

}


void COPCBrowseServerAddressSpace::ShowCurrentBrowsePos()
{
   HRESULT	hr;
	LPWSTR	lpItemID;
   CString  strItem;

	USES_CONVERSION;	

	hr = m_pIOPCBrowseServerAddressSpace->GetItemID(A2W(strItem), &lpItemID);
	if (FAILED(hr)) 
		DisplayOutput(ERROR_OUT, _T("Error from IOPCBrowseServerAddressSpace::GetItemID  Error = %s"), FormatHresult(hr));
	else
	{
		_tcscpy( m_wstrItemName, W2A(lpItemID) );
      UpdatePath(m_wstrItemName);
		((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(lpItemID);

      DisplayOutput(NORMAL_OUT, _T("Current browse position: %s"), m_wstrItemName);
	}

}