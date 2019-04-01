// ParamsResultsDlg.cpp : implementation file
//

#include "stdafx.h"
#include "OPCBClient.h"
#include "ParamsResultsDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CParamsResultsDlg dialog


CParamsResultsDlg::CParamsResultsDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CParamsResultsDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CParamsResultsDlg)
	m_bShowParameters = FALSE;
	m_bShowResults = FALSE;
	m_strItemID = _T("");
	//}}AFX_DATA_INIT
}


void CParamsResultsDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CParamsResultsDlg)
	DDX_Check(pDX, IDC_CHECK_PARAMETERS, m_bShowParameters);
	DDX_Check(pDX, IDC_CHECK_RESULTS, m_bShowResults);
	DDX_Text(pDX, IDC_EDIT_ITEMID, m_strItemID);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CParamsResultsDlg, CDialog)
	//{{AFX_MSG_MAP(CParamsResultsDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CParamsResultsDlg message handlers

BOOL CParamsResultsDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
   m_bShowParameters = TRUE;
   m_bShowResults = TRUE;

   UpdateData(FALSE);
   

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
