// EnumOPCBatchSummary.h: interface for the CEnumOPCBatchSummary class.
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

#if !defined(AFX_ENUMOPCBATCHSUMMARY_H__D0366BC8_4F03_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_ENUMOPCBATCHSUMMARY_H__D0366BC8_4F03_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"
#include "opcbc.h"


class CEnumOPCBatchSummary  
{
public:
	CEnumOPCBatchSummary();
	virtual ~CEnumOPCBatchSummary();
   virtual void CleanUp();

   HRESULT Initialize();

   HRESULT Try_Next(IEnumOPCBatchSummary* pIEnumOPCBatchSummary);
   HRESULT Try_Skip(IEnumOPCBatchSummary* pIEnumOPCBatchSummary);
   HRESULT Try_Reset(IEnumOPCBatchSummary* pIEnumOPCBatchSummary);
   HRESULT Try_Clone(IEnumOPCBatchSummary* pIEnumOPCBatchSummary);

   BOOL IsAvailable() {return ((m_pIMalloc != NULL) ? TRUE : FALSE); };

private:
   IMalloc*             m_pIMalloc;
};

#endif // !defined(AFX_ENUMOPCBATCHSUMMARY_H__D0366BC8_4F03_11D3_8764_00104BF10496__INCLUDED_)
