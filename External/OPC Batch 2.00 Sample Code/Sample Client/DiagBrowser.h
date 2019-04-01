// DiagBrowser.h : 
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

#if !defined(AFX_DIAGBROWSER_H__B1DA2E73_5988_11D3_807E_00A024CE4FA0__INCLUDED_)
#define AFX_DIAGBROWSER_H__B1DA2E73_5988_11D3_807E_00A024CE4FA0__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// DiagBrowser.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CDiagBrowser dialog

class CDiagBrowser : public CDialog
{
// Construction
public:
	CString m_csOption[3];
	CStringArray* m_pDataList;
	CDiagBrowser(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDiagBrowser)
	enum { IDD = IDD_DIAG_BROWSER };
	CString	m_csItemID;
	int		m_nOption;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDiagBrowser)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CDiagBrowser)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_DIAGBROWSER_H__B1DA2E73_5988_11D3_807E_00A024CE4FA0__INCLUDED_)
