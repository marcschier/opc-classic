// OPCBatchServer.h: interface for the COPCBatchServer class.
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

#if !defined(AFX_OPCBatchServer_H__5FC2DD59_4E51_11D3_8764_00104BF10496__INCLUDED_)
#define AFX_OPCBatchServer_H__5FC2DD59_4E51_11D3_8764_00104BF10496__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCDA.h"
#include "opcbc.h"

class COPCBatchServer  
{
public:
	COPCBatchServer();
	virtual ~COPCBatchServer();
   virtual void CleanUp();

   HRESULT Initialize(IUnknown* pOPCBatchServer);
	HRESULT Try_GetDelimiter();
	HRESULT Try_CreateEnumerator();


   OPCHANDLE Try_GetGroupState(IOPCGroupStateMgt * pGRP);

   BOOL IsAvailable() {return ((m_pIOPCBatchServer != NULL && m_pIMalloc != NULL) ? TRUE : FALSE); };

private:

   IOPCBatchServer*        m_pIOPCBatchServer;    

   IMalloc*       m_pIMalloc;

   IUnknown*      m_pGroup;


};

#endif // !defined(AFX_OPCBatchServer_H__5FC2DD59_4E51_11D3_8764_00104BF10496__INCLUDED_)
