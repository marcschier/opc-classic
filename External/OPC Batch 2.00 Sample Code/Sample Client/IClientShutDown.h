// IClientShutdown.h: interface for the IClientShutdown class.
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

#if !defined(AFX_ICLIENTShutdown_H__6650DA98_5642_11D3_807E_00A024CE4FA0__INCLUDED_)
#define AFX_ICLIENTShutdown_H__6650DA98_5642_11D3_807E_00A024CE4FA0__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "OPCComn.h"
#include "utility.h"
#include "Unknown.h"

class IClientShutdown : public IOPCShutdown  
{
public:
	HRESULT Initialize(IUnknown *pOPCBatchServer);
	IClientShutdown();
	virtual ~IClientShutdown();
	// the IUnknown Functions
	STDMETHODIMP         QueryInterface( REFIID iid, LPVOID* ppInterface );
	STDMETHODIMP_(ULONG) AddRef( void );
	STDMETHODIMP_(ULONG) Release( void );

	// Member Functions 
	STDMETHODIMP ShutdownRequest( LPCWSTR szReason );

private:
	IUnknown* m_pIUnkown;
	DWORD mRefCount;
};

#endif // !defined(AFX_ICLIENTShutdown_H__6650DA98_5642_11D3_807E_00A024CE4FA0__INCLUDED_)
