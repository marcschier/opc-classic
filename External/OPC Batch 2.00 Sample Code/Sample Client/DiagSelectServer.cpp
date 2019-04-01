// CDiagSelectServer.cpp : implementation file
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
#include "OPCBCats.h"
#include "DiagSelectServer.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CDiagSelectServer dialog


CDiagSelectServer::CDiagSelectServer(CWnd* pParent /*=NULL*/)
	: CDialog(CDiagSelectServer::IDD, pParent)
{
	//{{AFX_DATA_INIT(CDiagSelectServer)
	m_csStationName = _T("");
	m_nServerType = 0;
	//}}AFX_DATA_INIT
}


void CDiagSelectServer::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
	//{{AFX_DATA_MAP(CDiagSelectServer)
	DDX_Control(pDX, IDC_LIST_SERVER, m_LBServer);
	DDX_Text(pDX, IDC_EDIT_STATION, m_csStationName);
	DDX_Radio(pDX, IDC_RADIO1, m_nServerType);
	//}}AFX_DATA_MAP
}


BEGIN_MESSAGE_MAP(CDiagSelectServer, CDialog)
	//{{AFX_MSG_MAP(CDiagSelectServer)
	ON_LBN_SELCHANGE(IDC_LIST_SERVER, OnSelchangeListServer)
	ON_BN_CLICKED(IDC_RADIO1, OnRadio)
	ON_BN_CLICKED(IDC_RADIO2, OnRadio)
	ON_BN_CLICKED(IDC_RADIO3, OnRadio)
	//}}AFX_MSG_MAP
END_MESSAGE_MAP()

/////////////////////////////////////////////////////////////////////////////
// CDiagSelectServer message handlers

BOOL CDiagSelectServer::OnInitDialog() 
{
	CDialog::OnInitDialog();
	
	m_csServerID = "";
	m_csStationName = "";
	m_nServerType = 0;
	
	FindSeverID( CATID_OPCDAServer20);

	return TRUE;  // return TRUE unless you set the focus to a control
	              // EXCEPTION: OCX Property Pages should return FALSE
}

void CDiagSelectServer::FindSeverID(CATID cid)
{
	HRESULT hr, hr2;
	ICatInformation *pCat;
	IEnumCLSID * pEnum;
	CATID Implist[1];
	
	Implist[0] = cid;
	
	hr = CoCreateInstance (CLSID_StdComponentCategoriesMgr, NULL,
		CLSCTX_INPROC_SERVER, IID_ICatInformation, (void **)&pCat);
	if (SUCCEEDED(hr)) 
	{
		hr2 = pCat->EnumClassesOfCategories(
			1, Implist,	0, NULL,	&pEnum);
		
		if (SUCCEEDED(hr2)) 
		{
			GUID glist;
			ULONG actual;
			
			// Print the results of the ENum
			//	
			while((hr2 = pEnum->Next(1, &glist, &actual)) == S_OK)
			{
				// If a GUID came back then get the associated ProgID
				//
				WCHAR *result;
				hr2 = ProgIDFromCLSID(glist, &result);
				if(FAILED(hr2)) AfxMessageBox("ProgIDFromCLSID failed.");
				else 
				{
					CString csTmp;
					csTmp.Format( "%ls", result );
					m_LBServer.AddString( csTmp );
					((COPCBClientApp*)AfxGetApp())->m_pIMalloc->Free(result);
				}
			}
			
			pEnum->Release();
		}
		pCat->Release();
	} else
	{
		AfxMessageBox("CoCreate Failed\n");
	}
}

CString CDiagSelectServer::GetServerID()
{
	return m_csServerID;
}

void CDiagSelectServer::OnOK() 
{
	
	CDialog::OnOK();
}

void CDiagSelectServer::OnSelchangeListServer() 
{
	m_LBServer.GetText( m_LBServer.GetCurSel(), m_csServerID );
	
}

void CDiagSelectServer::OnRadio() 
{
	UpdateData();	
	CEdit* pEd = (CEdit*) GetDlgItem(IDC_EDIT_STATION);
	pEd->EnableWindow( m_nServerType == 2 );
}
