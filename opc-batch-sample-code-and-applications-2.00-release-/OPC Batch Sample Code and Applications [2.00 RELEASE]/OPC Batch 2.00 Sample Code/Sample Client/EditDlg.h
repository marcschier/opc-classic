// EditDlg.h : 
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

#if !defined(AFX_EDITDLG_H__F269F8A1_5567_11D3_8767_00104BF10496__INCLUDED_)
#define AFX_EDITDLG_H__F269F8A1_5567_11D3_8767_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// EditDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CEditDlg dialog

class CEditDlg : public CDialog
{
// Construction
public:
	void Init(PCSTR szCaption, PCSTR szLabel);
	CEditDlg(CWnd* pParent = NULL);   // standard constructor
	CEditDlg(PCSTR szCaption, PCSTR szLabel, CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CEditDlg)
	enum { IDD = IDD_DIALOG_EDIT };
	CString	m_strEdit;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CEditDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CEditDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()

private:
   CString m_strCaption;
   CString m_strLabel;
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_EDITDLG_H__F269F8A1_5567_11D3_8767_00104BF10496__INCLUDED_)
