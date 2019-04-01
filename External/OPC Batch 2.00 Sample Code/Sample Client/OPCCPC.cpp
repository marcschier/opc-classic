// OPCCPC.cpp: implementation of the COPCCPC class.
//
//////////////////////////////////////////////////////////////////////
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
// .42   01/20/00 jb    release interface pointer in destructor

#include "stdafx.h"
#include "OPCBClient.h"
#include "OPCCPC.h"
#include "utility.h"
#include "IClientShutdown.h"

#ifdef _DEBUG
#undef THIS_FILE
static char THIS_FILE[]=__FILE__;
#define new DEBUG_NEW
#endif

//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

COPCCPC::COPCCPC()
{
	m_pICPC = NULL;
	m_MyCookie = 0L;
	m_pMyCallback = NULL;
}

COPCCPC::~COPCCPC()
{
	if( m_pMyCallback ) 
   {
      m_pMyCallback->Release();
   }

   if (m_pICPC != NULL)
		m_pICPC->Release();
}

BOOL COPCCPC::IsAvailable()
{
	return (m_pICPC != NULL );
}

HRESULT COPCCPC::Initialize(IUnknown *pOPCBatchServer)
{
   // initialize member pointer to IOPCServer interface.
   HRESULT hr = pOPCBatchServer->QueryInterface(IID_IConnectionPointContainer, 
		(void**)&m_pICPC);
   if (FAILED(hr))
	{
      m_pICPC = NULL;   
	   DisplayOutput(ERROR_OUT, _T("Unable to get IConnectionPointContainer interface. Error = %s"), FormatHresult(hr));
	}
	else
	{
		// Create a callback instance
		//
		m_pMyCallback = new IClientShutdown;
		m_pMyCallback->Initialize( pOPCBatchServer );
		m_pMyCallback -> AddRef();
	}
   return hr;
}

HRESULT COPCCPC::Try_EnumConnectionPoints()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCPC::Try_EnumConnectionPoints."));

   HRESULT hr;
	IEnumConnectionPoints* pEnumCPs;
	hr = m_pICPC->EnumConnectionPoints( &pEnumCPs );
	if (FAILED(hr))
	{
		DisplayOutput(ERROR_OUT, _T("Error for EnumConnectionPoints:() %s"), FormatHresult(hr));
	}
	else
		DisplayOutput(NORMAL_OUT, _T("Success from EnumConnectionPoints()"));
	return hr;
}

HRESULT COPCCPC::Try_FindConnectionPoint()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCPC::Try_FindConnectionPoint."));

   HRESULT hr;
	//IConnectionPoint *pCallbackCP = 0;
	
	// Establish a connection
	//
	CWaitCursor	myCursor;

	hr = m_pICPC->FindConnectionPoint(IID_IOPCShutdown, &m_pCallbackCP);
	if (FAILED(hr))
	{
		DisplayOutput(ERROR_OUT, _T("No ConnectionPoint for OPCShhutdown:() %s"), FormatHresult(hr));
		return hr;
	}
	DisplayOutput(NORMAL_OUT, _T("Successfully found an IConnectionPoint for IOPCShhutdown."));
	DisplayOutput(NORMAL_OUT, _T("Shutdown Callback is Connected - monitoring for activity."));

	ASSERT( m_pMyCallback != NULL );
	hr = m_pCallbackCP->Advise(m_pMyCallback, &m_MyCookie);
	if (FAILED(hr))
		DisplayOutput(ERROR_OUT, _T("Advise Failed:() %s"), FormatHresult(hr));
	else
		DisplayOutput(NORMAL_OUT, _T("Call Advise() succeed and wait for Unadvise()"));
	// Run test until terminated by user.
	return hr;
}

HRESULT COPCCPC::Try_UnAdvise()
{
   DisplayOutput(VERBOSE_OUT, _T("COPCCPC::Try_UnAdvise."));

   HRESULT hr;
	//IConnectionPoint *pCallbackCP = 0;
	
	// Establish a connection
	//
	CWaitCursor	myCursor;
	/***
	hr = m_pICPC->FindConnectionPoint(IID_IOPCShutdown, &pCallbackCP);
	if (FAILED(hr))
	{
		DisplayOutput(ERROR_OUT, _T("No ConnectionPoint for OPCShhutdown:(%lx)"), hr);
		return hr;
	}
	DisplayOutput(NORMAL_OUT, _T("Successfully found an IConnectionPoint for IOPCShhutdown."));
	***/
	DisplayOutput(NORMAL_OUT, _T("Disconnecting advise()."));
	hr = m_pCallbackCP->Unadvise(m_MyCookie);

	if (FAILED(hr))
		DisplayOutput(ERROR_OUT, _T("UnAdvise Failed:() %s"), FormatHresult(hr));
	else
		DisplayOutput(NORMAL_OUT, _T("Successfull to UnAdvise()."));


	// Release the interfaces
	//
	m_pCallbackCP->Release();
   m_pCallbackCP = NULL;
	m_MyCookie = 0L;
	return hr;
}

BOOL COPCCPC::IsAdvised()
{
	return (m_MyCookie != 0L);
}
