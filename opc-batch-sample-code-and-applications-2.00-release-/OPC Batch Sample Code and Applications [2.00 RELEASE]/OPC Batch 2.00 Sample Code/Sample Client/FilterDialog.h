#if !defined(AFX_FILTERDIALOG_H__EA1D48CA_B409_11D4_A0BA_000102A980B1__INCLUDED_)
#define AFX_FILTERDIALOG_H__EA1D48CA_B409_11D4_A0BA_000102A980B1__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// FilterDialog.h : header file
//

/////////////////////////////////////////////////////////////////////////////
// CFilterDialog dialog

class CFilterDialog : public CDialog
{
// Construction
public:
	CFilterDialog(CWnd* pParent = NULL);   // standard constructor

// Dialog Data
	//{{AFX_DATA(CFilterDialog)
	enum { IDD = IDD_DIALOG_FILTER };
	CString	m_sBatchIDFilter;
	CString	m_sDescrFilter;
	float	m_fMaxBatchSizeFilter;
	float	m_fMinBatchSizeFilter;
	CString	m_sModel;
	BOOL	m_bUnfiltered;
	//}}AFX_DATA


// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(CFilterDialog)
	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support
	//}}AFX_VIRTUAL

// Implementation
protected:

	// Generated message map functions
	//{{AFX_MSG(CFilterDialog)
	afx_msg void OnCheckUnfiltered();
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
};

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_FILTERDIALOG_H__EA1D48CA_B409_11D4_A0BA_000102A980B1__INCLUDED_)
