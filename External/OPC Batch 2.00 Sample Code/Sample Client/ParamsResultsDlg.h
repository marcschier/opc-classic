#if !defined(AFX_PARAMSRESULTSDLG_H__9A726DA1_C847_11D3_876A_00104BF10496__INCLUDED_)
#define AFX_PARAMSRESULTSDLG_H__9A726DA1_C847_11D3_876A_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// ParamsResultsDlg.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CParamsResultsDlg dialog

class CParamsResultsDlg : public CDialog
{
// Construction
public:
	CParamsResultsDlg(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CParamsResultsDlg)
	enum { IDD = IDD_DIALOG_PARAM_RESULTS };
	BOOL	m_bShowParameters;
	BOOL	m_bShowResults;
	CString	m_strItemID;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CParamsResultsDlg)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CParamsResultsDlg)
	virtual BOOL OnInitDialog();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_PARAMSRESULTSDLG_H__9A726DA1_C847_11D3_876A_00104BF10496__INCLUDED_)
