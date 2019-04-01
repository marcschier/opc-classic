// DiagBrowser.cpp : implementation file
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
#include "DiagBrowser.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CDiagBrowser dialog


CDiagBrowser::CDiagBrowser(CWnd* pParent /*=NULL*/)
	: CDialog(CDiagBrowser::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDiagBrowser)
	m_csItemID = _T("");
	m_nOption = -1;
	//}}AFX_DATA_INIT
}


void CDiagBrowser::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDiagBrowser)
	DDX_CBString(pDX, IDC_ITEMID, m_csItemID);
	DDX_Radio(pDX, IDC_OPTION1, m_nOption);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CDiagBrowser, CDialog)
	//{{AFX_MSG_MAP(CDiagBrowser)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CDiagBrowser message handlers

BOOL CDiagBrowser::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	CComboBox* pCB = (CComboBox*) GetDlgItem( IDC_ITEMID );
	if( m_pDataList->GetSize() > 0 )
	{
		for( int i = 0; i < m_pDataList->GetSize(); i++ )
		{
			pCB->AddString( m_pDataList->GetAt(i) );
		}
	}
	((CButton*) GetDlgItem(IDC_OPTION1))->SetWindowText(m_csOption[0]);
	((CButton*) GetDlgItem(IDC_OPTION2))->SetWindowText(m_csOption[1]);
	((CButton*) GetDlgItem(IDC_OPTION3))->SetWindowText(m_csOption[2]);
	
	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}
