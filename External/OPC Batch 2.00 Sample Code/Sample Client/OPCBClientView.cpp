// OPCBClientView.cpp : implementation of the COPCBClientView class
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
//
// CONTENTS:
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
// .4    01/12/00 jb    preliminary release for testing.
//

#include "stdafx.h"
#include "OPCBClient.h"

#include "utility.h"
#include "server.h"
#include "DiagSelectServer.h"
#include "OPCBClientDoc.h"
#include "OPCBClientView.h"

#include "ParamsResults.h"

#include "AFXPRIV.H"          // conversions

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

extern IEnumOPCBatchSummary * G_pIEnumOPCBatchSummary;

/////////////////////////////////////////////////////////////////////////////
// COPCBClientView

IMPLEMENT_DYNCREATE(COPCBClientView, CListView)

BEGIN_MESSAGE_MAP(COPCBClientView, CListView)
	//{{AFX_MSG_MAP(COPCBClientView)
	ON_COMMAND(ID_OPTIONS_VERBOSEOUTPUT, OnOptionsVerboseOutput)
	ON_UPDATE_COMMAND_UI(ID_OPTIONS_VERBOSEOUTPUT, OnUpdateOptionsVerboseOutput)
	ON_COMMAND(ID_FILE_CONNECTTOSERVER, OnFileConnectToServer)
	ON_COMMAND(ID_TEST_IOPCSERVER_GETSTATUS, OnTest_IOPCServer_GetStatus)
	ON_COMMAND(ID_TEST_IOPCCOMMON_SETLOCALEID, OnTest_IOPCCommon_SetLocaleID)
	ON_COMMAND(ID_TEST_IOPCCOMMON_SETCLIENTNAME, OnTest_IOPCommon_SetClientName)
	ON_COMMAND(ID_TEST_IOPCCOMMON_QUERYAVAILABLELOCALEIDS, OnTest_IOPCCommon_QueryAvailableLocaleIDs)
	ON_COMMAND(ID_TEST_IOPCCOMMON_GETLOCALEID, OnTest_IOPCCommon_GetLocaleID)
	ON_COMMAND(ID_TEST_IOPCCOMMON_GETERRORSTRING, OnTest_IOPCCommon_GetErrorString)
	ON_COMMAND(ID_TEST_IOPCSERVER_ADDGROUP, OnTest_IOPCServer_AddGroup)
	ON_COMMAND(ID_TEST_IOPCSERVER_CREATEENUMERATOR, OnTest_IOPCServer_CreateEnumerator)
	ON_COMMAND(ID_TEST_IOPCSERVER_CREATEGROUPENUMERATOR, OnTest_IOPCServer_CreateGroupEnumerator)
	ON_COMMAND(ID_TEST_IOPCSERVER_GETERRORSTRING, OnTest_IOPCServer_GetErrorString)
	ON_COMMAND(ID_TEST_IOPCSERVER_GETGROUPBYNAME, OnTest_IOPCServer_GetGroupByName)
	ON_COMMAND(ID_TEST_IOPCSERVER_REMOVEGROUP, OnTest_IOPCServer_RemoveGroup)
	ON_COMMAND(ID_TEST_ICONNECTIONPOINTCONTAINER_ENUMCONNECTIONPOINTS, OnTest_IConnectionpointContainer_EnumConnectionPoints)
	ON_COMMAND(ID_TEST_ICONNECTIONPOINTCONTAINER_FINDCONNECTIONPOINT, OnTest_IConnectionpointContainer_FindConnectionPoint)
	ON_COMMAND(ID_TEST_IENUMOPCBATCHSUMMARY_CLONE, OnTest_IEnumOPCBatchSummary_Clone)
	ON_COMMAND(ID_TEST_IENUMOPCBATCHSUMMARY_NEXT, OnTest_IEnumOPCBatchSummary_Next)
	ON_COMMAND(ID_TEST_IENUMOPCBATCHSUMMARY_RESET, OnTest_IEnumOPCBatchSummary_Reset)
	ON_COMMAND(ID_TEST_IENUMOPCBATCHSUMMARY_SKIP, OnTest_IEnumOPCBatchSummary_Skip)
	ON_COMMAND(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_BROWSEACCESSPATHS, OnTest_IOPCBrowseServerAddressSpace_BrowseAccessPaths)
	ON_COMMAND(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_BROWSEOPCITEMIDS, OnTest_IOPCBrowseServerAddressSpace_BrowseOPCItemIDs)
	ON_COMMAND(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_CHANGEBROWSEPOSITION, OnTest_IOPCBrowseServerAddressSpace_ChangeBrowsePosition)
	ON_COMMAND(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_GETITEMID, OnTest_IOPCBrowseServerAddressSpace_GetItemID)
	ON_COMMAND(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_QUERYORGANIZATION, OnTest_IOPCBrowseServerAddressSpace_QueryOrganization)
	ON_COMMAND(ID_TEST_IOPCENUMERATIONSETS_QUERYENUMERATION, OnTest_IOPCEnumerationSets_QueryEnumeration)
	ON_COMMAND(ID_TEST_IOPCENUMERATIONSETS_QUERYENUMERATIONLIST, OnTest_IOPCEnumerationSets_QueryEnumerationList)
	ON_COMMAND(ID_TEST_IOPCENUMERATIONSETS_QUERYENUMERATIONSETS, OnTest_IOPCEnumerationSets_QueryEnumerationSets)
	ON_COMMAND(ID_TEST_IOPCITEMPROPERTIES_GETITEMPROPERTIES, OnTest_IOPCItemProperties_GetItemProperties)
	ON_COMMAND(ID_TEST_IOPCITEMPROPERTIES_LOOKUPITEMIDS, OnTest_IOPCItemProperties_LookupItemIDs)
	ON_COMMAND(ID_TEST_IOPCITEMPROPERTIES_QUERYAVAILABLEPROPERTIES, OnTest_IOPCItemProperties_QueryAvailableProperties)
	ON_COMMAND(ID_TEST_IPERSISTFILE_GETCURFILENAME, OnTest_IPersistFile_GetCurFilename)
	ON_COMMAND(ID_TEST_IPERSISTFILE_ISDIRTY, OnTest_IPersistFile_IsDirty)
	ON_COMMAND(ID_TEST_IPERSISTFILE_LOAD, OnTest_IPersistFile_Load)
	ON_COMMAND(ID_TEST_IPERSISTFILE_SAVE, OnTest_IPersistFile_Save)
	ON_COMMAND(ID_TEST_IPERSISTFILE_SAVECOMPLETED, OnTest_IPersistFile_SaveComplete)
	ON_COMMAND(ID_TEST_IUNKNOWN_ADDREF, OnTest_IUnknown_AddRef)
	ON_COMMAND(ID_TEST_IUNKNOWN_QUERYINTERFACE, OnTest_IUnknown_QueryInterface)
	ON_COMMAND(ID_TEST_IUNKNOWN_RELEASE, OnTest_IUnknown_Release)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCCOMMON_SETLOCALEID, OnUpdate_Test_IOPCCommon)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_CHANGEBROWSEPOSITION, OnUpdate_Test_IOPCBrowseServerAddressSpace)
	ON_UPDATE_COMMAND_UI(ID_TEST_ICONNECTIONPOINTCONTAINER_ENUMCONNECTIONPOINTS, OnUpdate_Test_IConnectionPointContainer)
   ON_COMMAND(ID_TEST_UNADVISE, OnTest_Unadvise)
	ON_UPDATE_COMMAND_UI(ID_TEST_UNADVISE, OnUpdate_Test_Unadvise)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCENUMERATIONSETS_QUERYENUMERATION, OnUpdate_Test_IOPCEnumerationSets)
	ON_COMMAND(ID_TEST_IOPCBATCHSERVER_CREATEENUMERATOR, OnTest_IOPCBatchServer_CreateEnumerator)
	ON_COMMAND(ID_TEST_IOPCBATCHSERVER_GETDELIMITER, OnTest_IOPCBatchServer_GetDelimiter)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBATCHSERVER_CREATEENUMERATOR, OnUpdate_Test_IOPCBatchServer)
	ON_COMMAND(ID_TEST_IOPCBATCHSERV2_CREATEFILTENUM, OnTest_IOPCBatchServer2_CreateFilteredEnumerator)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBATCHSERV2_CREATEFILTENUM, OnUpdate_Test_IOPCBatchServer2)
	ON_COMMAND(ID_VIEW_CLEAR, OnViewClear)
	ON_COMMAND(ID_VIEW_SCROLLDOWN, OnViewScrollDown)
	ON_UPDATE_COMMAND_UI(ID_VIEW_SCROLLDOWN, OnUpdateViewScrollDown)
	ON_COMMAND(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_SHOWTREE, OnTest_IOPCBrowseServerAddressSpace_ShowTree)
	ON_COMMAND(ID_OPTIONS_LOGTOFILE, OnOptionsLogToFile)
	ON_UPDATE_COMMAND_UI(ID_OPTIONS_LOGTOFILE, OnUpdateOptionsLogToFile)
	ON_COMMAND(ID_TEST_PARAMETERS_RESULTS, OnTest_ParametersResults)
	ON_UPDATE_COMMAND_UI(ID_TEST_PARAMETERS_RESULTS, OnUpdate_TestParametersResults)
	ON_COMMAND(ID_EDIT_COPY, OnEditCopy)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCCOMMON_SETCLIENTNAME, OnUpdate_Test_IOPCCommon)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCCOMMON_QUERYAVAILABLELOCALEIDS, OnUpdate_Test_IOPCCommon)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCCOMMON_GETLOCALEID, OnUpdate_Test_IOPCCommon)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCCOMMON_GETERRORSTRING, OnUpdate_Test_IOPCCommon)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_BROWSEOPCITEMIDS, OnUpdate_Test_IOPCBrowseServerAddressSpace)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_BROWSEACCESSPATHS, OnUpdate_Test_IOPCBrowseServerAddressSpace)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_GETITEMID, OnUpdate_Test_IOPCBrowseServerAddressSpace)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_QUERYORGANIZATION, OnUpdate_Test_IOPCBrowseServerAddressSpace)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_SHOWTREE, OnUpdate_Test_IOPCBrowseServerAddressSpace)
   ON_UPDATE_COMMAND_UI(ID_TEST_ICONNECTIONPOINTCONTAINER_FINDCONNECTIONPOINT, OnUpdate_Test_IConnectionPointContainer)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBROWSESERVERADDRESSSPACE_QUERYORGANIZATION, OnUpdate_Test_IOPCBrowseServerAddressSpace)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCENUMERATIONSETS_QUERYENUMERATIONLIST, OnUpdate_Test_IOPCEnumerationSets)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCENUMERATIONSETS_QUERYENUMERATIONSETS, OnUpdate_Test_IOPCEnumerationSets)
	ON_UPDATE_COMMAND_UI(ID_TEST_IOPCBATCHSERVER_GETDELIMITER, OnUpdate_Test_IOPCBatchServer)
	ON_UPDATE_COMMAND_UI(ID_EDIT_COPY, OnUpdateEditCopy)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// COPCBClientView construction/destruction

COPCBClientView::COPCBClientView()
{
   m_pOPC            = NULL;
   m_bVerboseOutput  = AfxGetApp()->GetProfileInt("Options","Verbose Output", TRUE);
   m_bScrollDown     = AfxGetApp()->GetProfileInt("Options","Scroll Down", TRUE);
   m_bLogToFile      = AfxGetApp()->GetProfileInt("Options","Log To File", FALSE);
}

COPCBClientView::~COPCBClientView()
{
   if (m_pOPC) m_pOPC->Release(); //**
}

BOOL COPCBClientView::PreCreateWindow(CREATESTRUCT& cs)
{
	cs.style |= LVS_SHOWSELALWAYS | LVS_REPORT | LVS_EDITLABELS;

	return CListView::PreCreateWindow(cs);
}

/////////////////////////////////////////////////////////////////////////////
// COPCBClientView drawing

void COPCBClientView::OnDraw(CDC* pDC)
{
	COPCBClientDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	// TODO: add draw code for native data here
}

void COPCBClientView::OnInitialUpdate()
{
	CListView::OnInitialUpdate();

   // this code only works for a report-mode list view
   ASSERT(GetStyle() & LVS_REPORT);

   // Gain a reference to the list control itself
   CListCtrl& theCtrl = GetListCtrl();

   // Insert a column. This override is the most convenient.
   theCtrl.InsertColumn(0, _T("Debug"), LVCFMT_LEFT);

   // Set reasonable widths for our columns
   theCtrl.SetColumnWidth(0, LVSCW_AUTOSIZE_USEHEADER);


   PostMessage(WM_COMMAND, ID_FILE_CONNECTTOSERVER, 0); // Connect to the server.

}

/////////////////////////////////////////////////////////////////////////////
// COPCBClientView diagnostics

#ifdef _DEBUG
void COPCBClientView::AssertValid() const
{
	CListView::AssertValid();
}

void COPCBClientView::Dump(CDumpContext& dc) const
{
	CListView::Dump(dc);
}

COPCBClientDoc* COPCBClientView::GetDocument() // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(COPCBClientDoc)));
	return (COPCBClientDoc*)m_pDocument;
}
#endif //_DEBUG




/////////////////////////////////////////////////////////////////////////////
// COPCBClientView message handlers

void COPCBClientView::OnOptionsVerboseOutput() 
{
   m_bVerboseOutput = !m_bVerboseOutput;	   
   AfxGetApp()->WriteProfileInt("Options","Verbose Output", m_bVerboseOutput);

}

void COPCBClientView::OnUpdateOptionsVerboseOutput(CCmdUI* pCmdUI) 
{
	pCmdUI->SetCheck(m_bVerboseOutput);
}


void COPCBClientView::OnFileConnectToServer() 
{

   // are we already connected?  if so verify closing current connection
      
      //close connection and references

  // Clean up in case the global pointer to the IEnumOPCBatchSummary
  // was set up in a previous connection to a server
  G_pIEnumOPCBatchSummary = NULL;

   // connect to server dialog
	CDiagSelectServer	diag;

	if( diag.DoModal() == IDOK )
	{
		m_csServerProgID = diag.GetServerID();
		m_csStationName = diag.m_csStationName;
		m_nServerType = diag.m_nServerType;

		CWaitCursor	cw;

      USES_CONVERSION;  
		switch( m_nServerType )
		{
      case 0:
      // connect to specified server
			m_pOPC = CreateInprocOPCServer(T2COLE(m_csServerProgID)); 
			break;

		case 1:
			m_pOPC = CreateLocalOPCServer(T2COLE(m_csServerProgID));
			break;

		case 2:	
			m_pOPC = CreateRemoteOPCServer(T2COLE(m_csServerProgID),A2W(m_csStationName));
			break;
		}
      // initialize interface wrapper classes
		if(m_pOPC)
		{
			DisplayOutput(NORMAL_OUT, _T("Got a pointer to server's IUnknown."));

         m_OPCBatchServer.Initialize(m_pOPC);
         m_OPCBatchServer2.Initialize(m_pOPC);
         m_OPCServer.Initialize(m_pOPC);
         m_OPCBrowseServerAddressSpace.Initialize(m_pOPC);
         m_OPCCommon.Initialize(m_pOPC);
         m_OPCEnumerationSets.Initialize(m_pOPC);
         m_OPCItemProperties.Initialize(m_pOPC);
         m_Unknown.Initialize(m_pOPC);
         m_OPCCPC.Initialize(m_pOPC);

// these interfaces currently aren't supported by this test client.
//         m_OPCServerPublicGroups.Initialize(m_pOPC);
//         m_PersistFile.Initialize(m_pOPC);


		}
      else
      {
			DisplayOutput(NORMAL_OUT, _T("Couldn't create server."));
      }
   
	}	

}



/////////////////////////////////////////////////////////////////////
//  View menu handlers

void COPCBClientView::OnViewClear() 
{
   GetListCtrl().DeleteAllItems();
}

void COPCBClientView::OnViewScrollDown() 
{
   m_bScrollDown = !m_bScrollDown;
   
   AfxGetApp()->WriteProfileInt("Options","Scroll Down", m_bScrollDown);
}

void COPCBClientView::OnUpdateViewScrollDown(CCmdUI* pCmdUI) 
{
	pCmdUI->SetCheck(m_bScrollDown);	
}




/////////////////////////////////////////////////////////////////////////////
// IOPCBatchServer Method Test


void COPCBClientView::OnTest_IOPCBatchServer_CreateEnumerator() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCBatchServer::CreateEnumerator()."));

   m_OPCBatchServer.Try_CreateEnumerator();	
	
}

void COPCBClientView::OnTest_IOPCBatchServer_GetDelimiter() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCBatchServer::GetDelimiter()."));

   m_OPCBatchServer.Try_GetDelimiter();	
	
}


/////////////////////////////////////////////////////////////////////////////
// IOPCBatchServer2 Method Test


void COPCBClientView::OnTest_IOPCBatchServer2_CreateFilteredEnumerator() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCBatchServer2::CreateFilteredEnumerator()."));

   m_OPCBatchServer2.Try_CreateFilteredEnumerator();	
	
}

/////////////////////////////////////////////////////////////////////////////
// IOPCServer Method Tests

void COPCBClientView::OnTest_IOPCServer_GetStatus() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCServer::GetStatus()."));

   m_OPCServer.Try_GetStatus();
	
}


void COPCBClientView::OnTest_IOPCServer_AddGroup() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCServer::AddGroup()."));

   m_OPCServer.Try_AddGroup();
	
}

void COPCBClientView::OnTest_IOPCServer_CreateEnumerator() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCServer::CreateEnumerator()."));

   m_OPCServer.Try_CreateEnumerator();
	
	
}

void COPCBClientView::OnTest_IOPCServer_CreateGroupEnumerator() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCServer::CreateGroupEnumerator()."));

   m_OPCServer.Try_CreateGroupEnumerator();
	
	
}

void COPCBClientView::OnTest_IOPCServer_GetErrorString() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCServer::GetErrorString()."));

   m_OPCServer.Try_GetErrorString();
	
	
}

void COPCBClientView::OnTest_IOPCServer_GetGroupByName() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCServer::GetGroupByName()."));

   m_OPCServer.Try_GetGroupByName();
	
	
}

void COPCBClientView::OnTest_IOPCServer_RemoveGroup() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCServer::RemoveGroup()."));

   m_OPCServer.Try_RemoveGroup();
	
	
}

/////////////////////////////////////////////////////////////////////////////




/////////////////////////////////////////////////////////////////////////////
// IOPCCommon Method Test

void COPCBClientView::OnTest_IOPCCommon_SetLocaleID() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCCommon::SetLocaleID()."));

   m_OPCCommon.Try_SetLocaleID();
	
}

void COPCBClientView::OnTest_IOPCommon_SetClientName() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCCommon::SetClientName()."));

   m_OPCCommon.Try_SetClientName();
	
}

void COPCBClientView::OnTest_IOPCCommon_QueryAvailableLocaleIDs() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCCommon::QueryAvailableLocaleIDs()."));

   m_OPCCommon.Try_QueryAvailableLocaleIDs();
	
}

void COPCBClientView::OnTest_IOPCCommon_GetLocaleID() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCCommon::GetLocaleID()."));

   m_OPCCommon.Try_GetLocaleID();
	
}

void COPCBClientView::OnTest_IOPCCommon_GetErrorString() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCCommon::GetErrorString()."));

   m_OPCCommon.Try_GetErrorString();
	
}


/////////////////////////////////////////////////////////////////////////////
// IConnectionPointContainer Method Test

void COPCBClientView::OnTest_IConnectionpointContainer_EnumConnectionPoints() 
{
	m_OPCCPC.Try_EnumConnectionPoints();
	
}

void COPCBClientView::OnTest_IConnectionpointContainer_FindConnectionPoint() 
{
	m_OPCCPC.Try_FindConnectionPoint();
	
}

void COPCBClientView::OnTest_Unadvise() 
{
	m_OPCCPC.Try_UnAdvise();
	
}


// reference the global declared in OPCBatchServer.cpp
// Note the comment there.....
extern IEnumOPCBatchSummary * G_pIEnumOPCBatchSummary;

/////////////////////////////////////////////////////////////////////////////
// IEnumBatchSummary Method Test

void COPCBClientView::OnTest_IEnumOPCBatchSummary_Clone() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IEnumOPCBatchSummary::Clone()."));

   m_EnumOPCBatchSummary.Try_Clone(G_pIEnumOPCBatchSummary);
}

void COPCBClientView::OnTest_IEnumOPCBatchSummary_Next() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IEnumOPCBatchSummary::Next()."));
	
   m_EnumOPCBatchSummary.Try_Next(G_pIEnumOPCBatchSummary); 
}

void COPCBClientView::OnTest_IEnumOPCBatchSummary_Reset() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IEnumOPCBatchSummary::Reset()."));
	
   m_EnumOPCBatchSummary.Try_Reset(G_pIEnumOPCBatchSummary); 
}

void COPCBClientView::OnTest_IEnumOPCBatchSummary_Skip() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IEnumOPCBatchSummary::Skip()."));
	
   m_EnumOPCBatchSummary.Try_Skip(G_pIEnumOPCBatchSummary);
}

/////////////////////////////////////////////////////////////////////////////
// IOPCBrowseServerAddressSpace Method Tests

void COPCBClientView::OnTest_IOPCBrowseServerAddressSpace_BrowseAccessPaths() 
{
   m_OPCBrowseServerAddressSpace.Try_BrowseAccessPaths();
	
}

void COPCBClientView::OnTest_IOPCBrowseServerAddressSpace_BrowseOPCItemIDs() 
{
   m_OPCBrowseServerAddressSpace.Try_BrowseOPCItemIDs();
	
}

void COPCBClientView::OnTest_IOPCBrowseServerAddressSpace_ChangeBrowsePosition() 
{
   m_OPCBrowseServerAddressSpace.Try_ChangeBrowsePosition();
	
}

void COPCBClientView::OnTest_IOPCBrowseServerAddressSpace_GetItemID() 
{
   m_OPCBrowseServerAddressSpace.Try_GetItemID();
	
}

void COPCBClientView::OnTest_IOPCBrowseServerAddressSpace_QueryOrganization() 
{
   m_OPCBrowseServerAddressSpace.Try_QueryOrganization();
	
}



void COPCBClientView::OnTest_IOPCBrowseServerAddressSpace_ShowTree() 
{
   m_OPCBrowseServerAddressSpace.Try_ShowTree();
	
}




/////////////////////////////////////////////////////////////////////////////
// IOPCEnumerationSets Method Tests

void COPCBClientView::OnTest_IOPCEnumerationSets_QueryEnumeration() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCEnumerationSets::QueryEnumeration()."));

   m_OPCEnumerationSets.Try_QueryEnumeration();	
}

void COPCBClientView::OnTest_IOPCEnumerationSets_QueryEnumerationList() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCEnumerationSets::QueryEnumerationList()."));

	m_OPCEnumerationSets.Try_QueryEnumerationList();
}

void COPCBClientView::OnTest_IOPCEnumerationSets_QueryEnumerationSets() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCEnumerationSets::QueryEnumerationSets()."));

	m_OPCEnumerationSets.Try_QueryEnumerationSets();
}

/////////////////////////////////////////////////////////////////////////////
//  IOPCItemProperties Method Tests


void COPCBClientView::OnTest_IOPCItemProperties_GetItemProperties() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCItemProperties::GetItemProperties()."));

   m_OPCItemProperties.Try_GetItemProperties();
	
}

void COPCBClientView::OnTest_IOPCItemProperties_LookupItemIDs() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCItemProperties::LookupItemIDs()."));

   m_OPCItemProperties.Try_LookupItemIDs();
	
}

void COPCBClientView::OnTest_IOPCItemProperties_QueryAvailableProperties() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IOPCItemProperties::QueryAvailableProperties()."));

   m_OPCItemProperties.Try_QueryAvailableProperties();
	
}




/////////////////////////////////////////////////////////////////////////////
//  IPersistFile Method Tests


void COPCBClientView::OnTest_IPersistFile_GetCurFilename() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IPersistFile::GetCurFilename()."));

	
}

void COPCBClientView::OnTest_IPersistFile_IsDirty() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IPersistFile::IsDirty()."));

	
}

void COPCBClientView::OnTest_IPersistFile_Load() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IPersistFile::Load()."));

	
}

void COPCBClientView::OnTest_IPersistFile_Save() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IPersistFile::Save()."));

	
}

void COPCBClientView::OnTest_IPersistFile_SaveComplete() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IPersistFile::SaveComplete()."));

	
}

/////////////////////////////////////////////////////////////////////////////
// IUnknown Method Test


void COPCBClientView::OnTest_IUnknown_AddRef() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IUnknown::AddRef()."));

   m_Unknown.Try_AddRef();	
}

void COPCBClientView::OnTest_IUnknown_QueryInterface() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IUnknown::QueryInterface()."));

   m_Unknown.Try_QueryInterface();	
}

void COPCBClientView::OnTest_IUnknown_Release() 
{
   DisplayOutput(VERBOSE_OUT, _T("***COMMAND*** Trying IUnknown::Release()."));

   m_Unknown.Try_Release();	
}





/////////////////////////////////////////////////////////////////////////////
// Menu enable/disable handlers

void COPCBClientView::OnUpdate_Test_IOPCCommon(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCCommon.IsAvailable() );	
}


void COPCBClientView::OnUpdate_Test_IOPCBrowseServerAddressSpace(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCBrowseServerAddressSpace.IsAvailable() );	
	
}

void COPCBClientView::OnUpdate_Test_IConnectionPointContainer(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCCPC.IsAvailable() );	
	
}


void COPCBClientView::OnUpdate_Test_Unadvise(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCCPC.IsAdvised() );	
	
}
void COPCBClientView::OnUpdate_Test_IEnumOPCBatchSummary(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_EnumOPCBatchSummary.IsAvailable() );	
	
}

void COPCBClientView::OnUpdate_Test_IOPCEnumerationSets(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCEnumerationSets.IsAvailable() );	
	
}

void COPCBClientView::OnUpdate_Test_IOPCItemProperties(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCItemProperties.IsAvailable() );	
	
}

void COPCBClientView::OnUpdate_Test_IOPCServer(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCServer.IsAvailable() );	
	
}

void COPCBClientView::OnUpdate_Test_IUnknown(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCServer.IsAvailable() );	
	
}

void COPCBClientView::OnUpdate_Test_IPersistFile(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_PersistFile.IsAvailable() );	
	
}


void COPCBClientView::OnUpdate_Test_IOPCBatchServer(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCBatchServer.IsAvailable() );	
}

void COPCBClientView::OnUpdate_Test_IOPCBatchServer2(CCmdUI* pCmdUI) 
{
	pCmdUI->Enable( m_OPCBatchServer2.IsAvailable() );	
}



void COPCBClientView::OnOptionsLogToFile() 
{
   m_bLogToFile = !m_bLogToFile;
   AfxGetApp()->WriteProfileInt("Options","Log To File", m_bLogToFile);	
}

void COPCBClientView::OnUpdateOptionsLogToFile(CCmdUI* pCmdUI) 
{
	pCmdUI->SetCheck(m_bLogToFile);	

}




/////////////////////////////////////////////////////////////////////////////
// Automated example anonymous access of parameters and results

void COPCBClientView::OnTest_ParametersResults() 
{

   Try_ParametersResults(m_pOPC);

}

void COPCBClientView::OnUpdate_TestParametersResults(CCmdUI* pCmdUI) 
{
   pCmdUI->Enable( m_OPCBrowseServerAddressSpace.IsAvailable() &&
                   m_OPCItemProperties.IsAvailable() );	
}

void COPCBClientView::OnEditCopy() 
{
   CEdit* pEdit = GetListCtrl().GetEditControl();
   if (pEdit != NULL)
   {
      CString strEdit;
      pEdit->GetWindowText(strEdit);

      int nStart, nEnd;
      pEdit->GetSel(nStart, nEnd);
      strEdit = strEdit.Mid(nStart, nEnd - nStart);

	   // copy the string to the clipboard
	   if (!OpenClipboard())
	   {
		   AfxMessageBox("Unable to open clipboard.");
		   return;
	   }

	   int nTextLen = (strEdit.GetLength()+1)*sizeof(TCHAR);
	   HGLOBAL hGlobal = GlobalAlloc(GMEM_MOVEABLE, nTextLen);
	   if (hGlobal != NULL)
	   {
		   LPVOID lpText = GlobalLock(hGlobal);
		   memcpy(lpText, strEdit, nTextLen);

		   EmptyClipboard();
		   GlobalUnlock(hGlobal);
#ifdef _UNICODE
		   SetClipboardData(CF_UNICODETEXT, hGlobal);
#else
		   SetClipboardData(CF_TEXT, hGlobal);
#endif
	   }
	   CloseClipboard();

   }


}

void COPCBClientView::OnUpdateEditCopy(CCmdUI* pCmdUI) 
{
   // allow a copy if the list item is being edited.
   pCmdUI->Enable(GetListCtrl().GetEditControl() != NULL);	
}

