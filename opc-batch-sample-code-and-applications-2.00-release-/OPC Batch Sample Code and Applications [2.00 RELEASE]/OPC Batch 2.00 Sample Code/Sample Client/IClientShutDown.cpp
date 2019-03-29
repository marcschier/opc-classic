// IClientShutdown.cpp: implementation of the IClientShutdown class.
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

#include "stdafx.h"
#include "OPCBClient.h"
#include "IClientShutdown.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

IClientShutdown::IClientShutdown()
{
	mRefCount = 0;

}

IClientShutdown::~IClientShutdown()
{

}
///////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IClientShutdown:: AddRef( void)
{
	return ++mRefCount;
}



///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP_(ULONG) IClientShutdown:: Release( void)
{
	ULONG currentCount = --mRefCount; 

	// If no references left for this object
	if ( currentCount == 0)
	{
		// Then delete this group.
		delete this;
	}
	return currentCount;
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IClientShutdown:: QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown )
		*ppInterface = (IUnknown*) this;

	else if ( iid == IID_IOPCShutdown)
	{
		*ppInterface = (IUnknown*) this;
	}
	else
	{
		*ppInterface = NULL;
	}
	
	if ( *ppInterface == NULL)
		return E_NOINTERFACE;

	AddRef();
	return S_OK;
}


///////////////////////////////////////
//
///////////////////////////////////////
STDMETHODIMP IClientShutdown:: ShutdownRequest(
	LPCWSTR szReason
)
{
	TRACE("Shutdown Request Called\n");
	if(szReason) 
	{
		m_pIUnkown->Release();
		AfxMessageBox( "Shutdown from OPCB Server is issued!" );
		DisplayOutput( NORMAL_OUT, _T("IOPCShutdown:: Reason is:%ls"), szReason );
	}
	return S_OK;
}

HRESULT IClientShutdown::Initialize(IUnknown *pOPCBatchServer)
{
	m_pIUnkown = pOPCBatchServer;
	return S_OK;
}
