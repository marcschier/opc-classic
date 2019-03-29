// OPCBClient.h : main header file for the OPCBCLIENT application
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

#if !defined(AFX_OPCBCLIENT_H__5FC2DD45_4E51_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCBCLIENT_H__5FC2DD45_4E51_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"       // main symbols

/////////////////////////////////////////////////////////////////////////////
// COPCBClientApp:
// See OPCBClient.cpp for the implementation of this class
//

class COPCBClientApp : public CWinApp
{
public:
	IMalloc* m_pIMalloc;

	COPCBClientApp();
	~COPCBClientApp();

// Overrides
	// ClassWizard generated virtual function overrides
	//{{AFX_VIRTUAL(COPCBClientApp)
	public:
	virtual BOOL InitInstance();
	//}}AFX_VIRTUAL

// Implementation
	//{{AFX_MSG(COPCBClientApp)
	afx_msg void OnAppAbout();
		// NOTE - the ClassWizard will add and remove member functions here.
		//    DO NOT EDIT what you see in these blocks of generated code !
	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
private:
	int LocalInit();
};


/////////////////////////////////////////////////////////////////////////////

//{{AFX_INSERT_LOCATION}}
// Microsoft Visual C++ will insert additional declarations immediately before the previous line.

#endif // !defined(AFX_OPCBCLIENT_H__5FC2DD45_4E51_11D3_8764_00104BF10496__INCLUDED_)
