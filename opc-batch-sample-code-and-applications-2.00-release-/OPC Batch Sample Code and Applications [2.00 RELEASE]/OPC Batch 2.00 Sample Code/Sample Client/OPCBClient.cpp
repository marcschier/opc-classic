// OPCBClient.cpp : Defines the class behaviors for the application.
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

#include "MainFrm.h"
#include "OPCBClientDoc.h"
#include "OPCBClientView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// COPCBClientApp

BEGIN_MESSAGE_MAP(COPCBClientApp, CWinApp)
	//{{AFX_MSG_MAP(COPCBClientApp)
	ON_COMMAND(ID_APP_ABOUT, OnAppAbout)
		// NOTE - the ClassWizard will add and remove mapping macros here.
		//    DO NOT EDIT what you see in these blocks of generated code!
	//}}AFX_MSG_MAP
	// Standard file based document commands
	ON_COMMAND(ID_FILE_NEW, CWinApp::OnFileNew)
	ON_COMMAND(ID_FILE_OPEN, CWinApp::OnFileOpen)
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// COPCBClientApp construction

COPCBClientApp::COPCBClientApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
   m_pIMalloc = NULL;

}

COPCBClientApp::~COPCBClientApp()
{
   if (m_pIMalloc) m_pIMalloc->Release();

}

/////////////////////////////////////////////////////////////////////////////
// The one and only COPCBClientApp object

COPCBClientApp theApp;

/////////////////////////////////////////////////////////////////////////////
// COPCBClientApp initialization

BOOL COPCBClientApp::InitInstance()
{
	if( LocalInit() == -1 )
		return FALSE;

	AfxEnableControlContainer();

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	//  of your final executable, you should remove from the following
	//  the specific initialization routines you do not need.

#ifdef _AFXDLL
	Enable3dControls();			// Call this when using MFC in a shared DLL
#else
	Enable3dControlsStatic();	// Call this when linking to MFC statically
#endif

	// Change the registry key under which our settings are stored.
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization.
	SetRegistryKey(_T("Local AppWizard-Generated Applications"));

	LoadStdProfileSettings(0);  // Load standard INI file options (including MRU)

	// Register the application's document templates.  Document templates
	//  serve as the connection between documents, frame windows and views.

	CSingleDocTemplate* pDocTemplate;
	pDocTemplate = new CSingleDocTemplate(
		IDR_MAINFRAME,
		RUNTIME_CLASS(COPCBClientDoc),
		RUNTIME_CLASS(CMainFrame),       // main SDI frame window
		RUNTIME_CLASS(COPCBClientView));
	AddDocTemplate(pDocTemplate);

	// Parse command line for standard shell commands, DDE, file open
	CCommandLineInfo cmdInfo;
	ParseCommandLine(cmdInfo);

	// Dispatch commands specified on the command line
	if (!ProcessShellCommand(cmdInfo))
		return FALSE;

	// The one and only window has been initialized, so show and update it.
	m_pMainWnd->ShowWindow(SW_SHOW);
	m_pMainWnd->UpdateWindow();

	return TRUE;
}


/////////////////////////////////////////////////////////////////////////////
// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog
{
public:
	CAboutDlg();

// Dialog Data
	//{{AFX_DATA(CAboutDlg)
	enum { IDD = IDD_ABOUTBOX };
	//}}AFX_DATA

	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CAboutDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:
	//{{AFX_MSG(CAboutDlg)
		// No message handlers
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{
	//{{AFX_DATA_INIT(CAboutDlg)
	//}}AFX_DATA_INIT
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CAboutDlg)
	//}}AFX_DATA_MAP
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)
	//{{AFX_MSG_MAP(CAboutDlg)
		// No message handlers
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

// App command to run the dialog
void COPCBClientApp::OnAppAbout()
{
	CAboutDlg aboutDlg;
	aboutDlg.DoModal();
}

/////////////////////////////////////////////////////////////////////////////
// COPCBClientApp message handlers
int COPCBClientApp::LocalInit()
{

	HRESULT	r1;
	// General COM initialization...
	//
	r1 = AfxOleInit();

	if (FAILED(r1))
	{
		AfxMessageBox("Error from CoInitialize\n");
		return -1;
	}

	// This is for DCOM
	//
	r1 = CoInitializeSecurity(
			NULL,   //Points to security descriptor 
			-1,     //Count of entries in asAuthSvc 
			NULL,   //Array of names to register 
			NULL,   //Reserved for future use 
			RPC_C_AUTHN_LEVEL_NONE,    //The default authentication level for proxies
			RPC_C_IMP_LEVEL_IMPERSONATE,//The default impersonation level for proxies
			NULL,                      //Reserved; must be set to  NULL 
			EOAC_NONE,                 //Additional client or server-side capabilities
			NULL                       //Reserved for future use 
			);
	if (FAILED(r1))
	{
		CoUninitialize();
		AfxMessageBox("Error from CoInitializeSecurity: %lx\n", r1);
		return -1;
	}

	// Also get access to the COM memory manager
	//
	r1 = CoGetMalloc(MEMCTX_TASK, &m_pIMalloc);

	if (FAILED(r1))
	{
		AfxMessageBox("GetMalloc failed\n");
		CoUninitialize();
		return -1;
	}
	return 0;
}
