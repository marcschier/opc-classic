// OPCBClientView.h : interface of the COPCBClientView class
//
/////////////////////////////////////////////////////////////////////////////

#if !defined(AFX_OPCBCLIENTVIEW_H__5FC2DD4D_4E51_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCBCLIENTVIEW_H__5FC2DD4D_4E51_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000


#include "OPCBatchServer.h"
#include "OPCBatchServer2.h"
#include "OPCServer.h"
#include "OPCCommon.h"
#include "OPCBrowseServerAddressSpace.h"
#include "OPCEnumerationSets.h"
#include "OPCItemProperties.h"
#include "OPCServerPublicGroups.h"
#include "OPCCPC.h"
#include "PersistFile.h"
#include "Unknown.h"
#include "EnumOPCBatchSummary.h"

class COPCBClientView : public CListView
{
protected: // create from serialization only
	COPCBClientView();
	DECLARE_DYNCREATE(COPCBClientView)

// Attributes
public:
	COPCBClientDoc* GetDocument();

//   void DisplayOutput(DWORD dwFlags, LPCTSTR pFormat, ...);

// Operations
public:

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(COPCBClientView)
	public:
	virtual void OnDraw(CDC* pDC);  // overridden to draw this view
	virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
	protected:
	virtual void OnInitialUpdate(); // called first time after construct
	//}}AFX_VIRTUAL

// Implementation
public:
	virtual ~COPCBClientView();
#ifdef _DEBUG
	virtual void AssertValid() const;
	virtual void Dump(CDumpContext& dc) const;
#endif

protected:

// Generated message map functions
protected:
	//{{AFX_MSG(COPCBClientView)
	afx_msg void OnOptionsVerboseOutput();
	afx_msg void OnUpdateOptionsVerboseOutput(CCmdUI* pCmdUI);
	afx_msg void OnFileConnectToServer();
	afx_msg void OnTest_IOPCServer_GetStatus();
	afx_msg void OnTest_IOPCCommon_SetLocaleID();
	afx_msg void OnTest_IOPCommon_SetClientName();
	afx_msg void OnTest_IOPCCommon_QueryAvailableLocaleIDs();
	afx_msg void OnTest_IOPCCommon_GetLocaleID();
	afx_msg void OnTest_IOPCCommon_GetErrorString();
	afx_msg void OnTest_IOPCServer_AddGroup();
	afx_msg void OnTest_IOPCServer_CreateEnumerator();
	afx_msg void OnTest_IOPCServer_CreateGroupEnumerator();
	afx_msg void OnTest_IOPCServer_GetErrorString();
	afx_msg void OnTest_IOPCServer_GetGroupByName();
	afx_msg void OnTest_IOPCServer_RemoveGroup();
	afx_msg void OnTest_IConnectionpointContainer_EnumConnectionPoints();
	afx_msg void OnTest_IConnectionpointContainer_FindConnectionPoint();
	afx_msg void OnTest_IEnumOPCBatchSummary_Clone();
	afx_msg void OnTest_IEnumOPCBatchSummary_Next();
	afx_msg void OnTest_IEnumOPCBatchSummary_Reset();
	afx_msg void OnTest_IEnumOPCBatchSummary_Skip();
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_BrowseAccessPaths();
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_BrowseOPCItemIDs();
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_ChangeBrowsePosition();
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_GetItemID();
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_QueryOrganization();
	afx_msg void OnTest_IOPCEnumerationSets_QueryEnumeration();
	afx_msg void OnTest_IOPCEnumerationSets_QueryEnumerationList();
	afx_msg void OnTest_IOPCEnumerationSets_QueryEnumerationSets();
	afx_msg void OnTest_IOPCItemProperties_GetItemProperties();
	afx_msg void OnTest_IOPCItemProperties_LookupItemIDs();
	afx_msg void OnTest_IOPCItemProperties_QueryAvailableProperties();
	afx_msg void OnTest_IPersistFile_GetCurFilename();
	afx_msg void OnTest_IPersistFile_IsDirty();
	afx_msg void OnTest_IPersistFile_Load();
	afx_msg void OnTest_IPersistFile_Save();
	afx_msg void OnTest_IPersistFile_SaveComplete();
	afx_msg void OnTest_IUnknown_AddRef();
	afx_msg void OnTest_IUnknown_QueryInterface();
	afx_msg void OnTest_IUnknown_Release();
	afx_msg void OnUpdate_Test_IOPCCommon(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IOPCBrowseServerAddressSpace(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IConnectionPointContainer(CCmdUI* pCmdUI);
	afx_msg void OnTest_Unadvise();
	afx_msg void OnUpdate_Test_Unadvise(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IEnumOPCBatchSummary(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IOPCEnumerationSets(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IOPCItemProperties(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IOPCServer(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IUnknown(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IPersistFile(CCmdUI* pCmdUI);
	afx_msg void OnTest_IOPCBatchServer_CreateEnumerator();
	afx_msg void OnTest_IOPCBatchServer_GetDelimiter();
  afx_msg void OnTest_IOPCBatchServer2_CreateFilteredEnumerator();
	afx_msg void OnUpdate_Test_IOPCBatchServer(CCmdUI* pCmdUI);
	afx_msg void OnUpdate_Test_IOPCBatchServer2(CCmdUI* pCmdUI);
	afx_msg void OnViewClear();
	afx_msg void OnViewScrollDown();
	afx_msg void OnUpdateViewScrollDown(CCmdUI* pCmdUI);
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_ShowTree();
	afx_msg void OnOptionsLogToFile();
	afx_msg void OnUpdateOptionsLogToFile(CCmdUI* pCmdUI);
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_Parameters();
	afx_msg void OnTest_IOPCBrowseServerAddressSpace_Results();
	afx_msg void OnTest_ParametersResults();
	afx_msg void OnUpdate_TestParametersResults(CCmdUI* pCmdUI);
	afx_msg void OnEditCopy();
	afx_msg void OnUpdateEditCopy(CCmdUI* pCmdUI);
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

public:
	CString  m_csStationName;
	int      m_nServerType;
   BOOL     m_bVerboseOutput;
   BOOL     m_bScrollDown;
   BOOL     m_bLogToFile;

private:
	CString  m_csServerProgID;

   IUnknown*                     m_pOPC;

   // OPC Interface class wrappers used here for testing
   COPCBatchServer               m_OPCBatchServer;
   COPCServer                    m_OPCServer;
   COPCCommon                    m_OPCCommon;
   COPCBrowseServerAddressSpace  m_OPCBrowseServerAddressSpace;
   COPCEnumerationSets           m_OPCEnumerationSets;
   COPCItemProperties            m_OPCItemProperties;
   COPCServerPublicGroups        m_OPCServerPublicGroups;
   CPersistFile                  m_PersistFile;
   CUnknown                      m_Unknown;
	COPCCPC								m_OPCCPC; //Class for IConnectPointContainer
   CEnumOPCBatchSummary          m_EnumOPCBatchSummary;
   COPCBatchServer2              m_OPCBatchServer2;

};

#ifndef _DEBUG  // debug version in OPCBClientView.cpp
inline COPCBClientDoc* COPCBClientView::GetDocument()
   { return (COPCBClientDoc*)m_pDocument; }
#endif

/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_OPCBCLIENTVIEW_H__5FC2DD4D_4E51_11D3_8764_00104BF10496__INCLUDED_)
