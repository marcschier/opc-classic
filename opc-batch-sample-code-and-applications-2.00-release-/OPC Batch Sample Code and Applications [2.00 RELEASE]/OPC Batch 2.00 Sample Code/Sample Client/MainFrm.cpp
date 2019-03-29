// MainFrm.cpp : implementation of the CMainFrame class
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
//

#include "stdafx.h"
#include "OPCBClient.h"

#include "MainFrm.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif


const int PATH_PANE = 1;



/////////////////////////////////////////////////////////////////////////////
// CMainFrame

IMPLEMENT_DYNCREATE(CMainFrame, CFrameWnd)


BEGIN_MESSAGE_MAP(CMainFrame, CFrameWnd)
	//{{AFX_MSG_MAP(CMainFrame)
	ON_WM_CREATE()
	ON_COMMAND(ID_OPTIONS_SHOWPATH, OnOptionsShowPath)
	ON_UPDATE_COMMAND_UI(ID_OPTIONS_SHOWPATH, OnUpdateOptionsShowPath)
   ON_UPDATE_COMMAND_UI ( PATH_PANE, OnUpdatePane )
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

static UINT indicators[] =
{
	ID_SEPARATOR,//,           // status line indicator
   ID_SEPARATOR
//	ID_INDICATOR_CAPS,
//	ID_INDICATOR_NUM,
//	ID_INDICATOR_SCRL,
};


void CMainFrame::OnUpdatePane (CCmdUI *pCmdUI)  {     pCmdUI->Enable ();  }



/////////////////////////////////////////////////////////////////////////////
// CMainFrame construction/destruction

CMainFrame::CMainFrame()
{
   m_bShowPath = FALSE;
	
}

CMainFrame::~CMainFrame()
{
}

int CMainFrame::OnCreate(LPCREATESTRUCT lpCreateStruct)
{
	if (CFrameWnd::OnCreate(lpCreateStruct) == -1)
		return -1;

	if (!m_wndStatusBar.Create(this) ||
		!m_wndStatusBar.SetIndicators(indicators,
		  sizeof(indicators)/sizeof(UINT)))
	{
		TRACE0("Failed to create status bar\n");
		return -1;      // fail to create
	}


	return 0;
}

BOOL CMainFrame::PreCreateWindow(CREATESTRUCT& cs)
{
	if( !CFrameWnd::PreCreateWindow(cs) )
		return FALSE;

   cs.style &= ~FWS_ADDTOTITLE; 

	return TRUE;
}

/////////////////////////////////////////////////////////////////////////////
// CMainFrame diagnostics

#ifdef _DEBUG
void CMainFrame::AssertValid() const
{
	CFrameWnd::AssertValid();
}

void CMainFrame::Dump(CDumpContext& dc) const
{
	CFrameWnd::Dump(dc);
}

#endif //_DEBUG

/////////////////////////////////////////////////////////////////////////////
// CMainFrame message handlers


void CMainFrame::OnOptionsShowPath() 
{
   m_bShowPath = !m_bShowPath;
}

void CMainFrame::OnUpdateOptionsShowPath(CCmdUI* pCmdUI) 
{
	pCmdUI->SetCheck(m_bShowPath);
}

void CMainFrame::SetPath(LPCTSTR szPath)
{
   m_strPath = szPath;

   m_wndStatusBar.SetPaneText(PATH_PANE, m_strPath);
}

