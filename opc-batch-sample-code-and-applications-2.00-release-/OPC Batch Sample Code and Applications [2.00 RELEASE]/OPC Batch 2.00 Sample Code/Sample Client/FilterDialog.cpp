// FilterDialog.cpp : implementation file
//

#include "stdafx.h"
#include "OPCBClient.h"
#include "FilterDialog.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CFilterDialog dialog


CFilterDialog::CFilterDialog(CWnd* pParent /*=NULL*/)
	: CDialog(CFilterDialog::IDD, pParent)
{
	//{{AFX_DATA_INIT(CFilterDialog)
	m_sBatchIDFilter = _T("");
	m_sDescrFilter = _T("");
	m_fMaxBatchSizeFilter = 2000.0f;
	m_fMinBatchSizeFilter = 0.0f;
	m_sModel = _T("OPCBBatchModel");
	m_bUnfiltered = FALSE;
	//}}AFX_DATA_INIT
}


void CFilterDialog::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CFilterDialog)
	DDX_Text(pDX, IDC_BATCHID, m_sBatchIDFilter);
	DDX_Text(pDX, IDC_DESCR, m_sDescrFilter);
	DDX_Text(pDX, IDC_MAXSIZE, m_fMaxBatchSizeFilter);
	DDX_Text(pDX, IDC_MINSIZE, m_fMinBatchSizeFilter);
	DDX_CBString(pDX, IDC_MODEL_SELECT, m_sModel);
	DDX_Check(pDX, IDC_CHECK_UNFILTERED, m_bUnfiltered);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CFilterDialog, CDialog)
	//{{AFX_MSG_MAP(CFilterDialog)
	ON_BN_CLICKED(IDC_CHECK_UNFILTERED, OnCheckUnfiltered)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CFilterDialog message handlers

void CFilterDialog::OnCheckUnfiltered() 
{
  if (IsDlgButtonChecked(IDC_CHECK_UNFILTERED))
  {
    GetDlgItem(IDC_BATCHID)->EnableWindow(FALSE);
    GetDlgItem(IDC_DESCR)->EnableWindow(FALSE);
    GetDlgItem(IDC_MAXSIZE)->EnableWindow(FALSE);
    GetDlgItem(IDC_MINSIZE)->EnableWindow(FALSE);
  }
  else
  {
    GetDlgItem(IDC_BATCHID)->EnableWindow(TRUE);
    GetDlgItem(IDC_DESCR)->EnableWindow(TRUE);
    GetDlgItem(IDC_MAXSIZE)->EnableWindow(TRUE);
    GetDlgItem(IDC_MINSIZE)->EnableWindow(TRUE);
  }
}
