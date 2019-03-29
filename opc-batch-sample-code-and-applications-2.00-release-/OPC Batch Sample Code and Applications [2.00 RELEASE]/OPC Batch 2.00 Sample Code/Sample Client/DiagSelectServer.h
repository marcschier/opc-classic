// DialogSelectServer.h : 
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

#if !defined(AFX_CDiagSelectServer_H__3A34103C_4B75_11D3_94DB_00104B35B11C__INCLUDED_)
#define AFX_CDiagSelectServer_H__3A34103C_4B75_11D3_94DB_00104B35B11C__INCLUDED_

#if _MSC_VER >= 1000
#pragma once
#endif // _MSC_VER >= 1000
// CDiagSelectServer.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CDiagSelectServer dialog

class CDiagSelectServer : public CDialog
{
// Construction
public:
	CString GetServerID();
	CDiagSelectServer(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CDiagSelectServer)
	enum { IDD = IDD_DIAG_SERVERS };
	CListBox	m_LBServer;
	CString	m_csStationName;
	int		m_nServerType;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CDiagSelectServer)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CDiagSelectServer)
	virtual BOOL OnInitDialog();
	virtual void OnOK();
	afx_msg void OnSelchangeListServer();
	afx_msg void OnRadio();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
private:
	CString m_csServerID;
	void FindSeverID(CATID cid);
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Developer Studio will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_CDiagSelectServer_H__3A34103C_4B75_11D3_94DB_00104B35B11C__INCLUDED_)
