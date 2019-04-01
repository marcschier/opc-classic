// OPCBatchServer2.h: interface for the COPCBatchServer2 class.
//
/////////////////////////////////////////////////////////////////////////////
// (c) Copyright 2000-2001 The OPC Foundation
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
//  0.1   11/6/00  WGI   Created with class wizard, pasted method & member
//                       defns in from OPCBatchServer.h
//

#if !defined(AFX_OPCBATCHSERVER2_H__601BD910_B1DD_11D4_A0BA_000102A980B1__INCLUDED_)
#define AFX_OPCBATCHSERVER2_H__601BD910_B1DD_11D4_A0BA_000102A980B1__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000
// OPCBatchServer2.h : header file
//

#include "OPCDA.h"
#include "opcbc.h"

/////////////////////////////////////////////////////////////////////////////
// COPCBatchServer2 

class COPCBatchServer2
{
// Construction
public:
	COPCBatchServer2();

  virtual void CleanUp();
  HRESULT Initialize(IUnknown* pOPCBatchServer);

  HRESULT Try_CreateFilteredEnumerator();

   BOOL IsAvailable() {return ((m_pIOPCBatchServer2 != NULL && m_pIMalloc != NULL) ? TRUE : FALSE); };

   IEnumOPCBatchSummary *  m_pIEnumOPCBatchSummary;

private:

   IOPCBatchServer2*        m_pIOPCBatchServer2;    

   IMalloc*       m_pIMalloc;

   IUnknown*      m_pGroup;

// Implementation
public:
	virtual ~COPCBatchServer2();

};

/////////////////////////////////////////////////////////////////////////////

#endif // !defined(AFX_OPCBATCHSERVER2_H__601BD910_B1DD_11D4_A0BA_000102A980B1__INCLUDED_)
