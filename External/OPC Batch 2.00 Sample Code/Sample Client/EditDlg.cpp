// EditDlg.cpp : implementation file
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
#include "EditDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CEditDlg dialog


CEditDlg::CEditDlg(CWnd* pParent /*=NULL*/)
	: CDialog(CEditDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CEditDlg)
	m_strEdit = _T("");
	//}}AFX_DATA_INIT
   Init(_T("Enter Text"), _T("Please enter text in edit box:"));
}

CEditDlg::CEditDlg(PCSTR szCaption, PCSTR szLabel, CWnd* pParent)
	: CDialog(CEditDlg::IDD, pParent)
{
	//{{AFX_DATA_INIT(CEditDlg)
	m_strEdit = _T("");
	//}}AFX_DATA_INIT

   Init(szCaption, szLabel);
}


void CEditDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CEditDlg)
	DDX_Text(pDX, IDC_EDIT_GENERAL, m_strEdit);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CEditDlg, CDialog)
	//{{AFX_MSG_MAP(CEditDlg)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CEditDlg message handlers

BOOL CEditDlg::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
   SetWindowText(m_strCaption);
   GetDlgItem(IDC_EDIT_LABEL)->SetWindowText(m_strLabel);
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CEditDlg::Init(PCSTR szCaption, PCSTR szLabel)
{

   m_strCaption = szCaption;
   m_strLabel = szLabel;

}
