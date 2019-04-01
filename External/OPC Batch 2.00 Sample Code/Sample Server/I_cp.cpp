// I_CP.cpp
//
// (c) Copyright 1997 The OPC Foundation
// ALL RIGHTS RESERVED.
//
// DISCLAIMER:
//  This sample code is provided by the OPC Foundation solely to assist 
//  in understanding the OPC Data Access Specification and may be used
//  as set forth in the License Grant section of the OPC Specification.
//  This code is provided as-is and without warranty or support of any sort
//  and is subject to the Warranty and Liability Disclaimers which appear
//  in the printed OPC Specification.
//
// CREDITS:
//  This code was generously provided to the OPC Foundation by
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//  This file contains the implementation of 
//  the IConnectionPoint interface for OPC
//  for the XXX server.
//
//  NOTE that this ConnectionPoint is NOT tear off interfaces from the group,
//  It is a 'real' object separate from the group.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	      03/17/97 ACC   proposed OPC 2.0 implementation
//       01/12/98 acc   made it generic and reusable
//       06/12/98 acc   cosmetic cleanup
//       01/22/99 acc   cosmetic cleanup - remove opcxxx.h include
//       02/22/99 acc   change 'cookie' value to be 1 rather than 0
//       10/13/99 acc	fix up initialization problem - add SetParent
//
//

#define WIN32_LEAN_AND_MEAN

#include "CPCHELP.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXCP()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXCP::IXXXCP(  REFIID iid)
{
	m_iid = iid;
	m_Callback = 0;
	mRefCount = 0;
}



///////////////////////////////////////
// ~IXXXCP()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXCP::~IXXXCP( void)
{
	// The caller should unadvise before detaching from the server
	// but just in case he does not...
	//
	if(m_Callback) m_Callback->Release();
	m_Callback = NULL;
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXCP::AddRef( void)
{
	return ++mRefCount;
}

///////////////////////////////////////
STDMETHODIMP_(ULONG) IXXXCP::Release( void)
{
	ULONG currentCount = --mRefCount;

	// If no references left for this group
	if ( currentCount == 0)
	{
		// Then delete this group.
		delete this;
	}
	return currentCount;
}

///////////////////////////////////////
STDMETHODIMP IXXXCP::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	if ( ppInterface == NULL)
		return E_INVALIDARG;

	if ( iid == IID_IUnknown )
	{
		*ppInterface = (IUnknown*) this;
	}
	else if ( iid == IID_IConnectionPoint)
	{
		*ppInterface = (IUnknown*) this;
	}
	else
	{
		*ppInterface = NULL;
	}
	
	if ( *ppInterface == NULL)
		return E_NOINTERFACE;

	AddRef();	// Caller must call Release()
	return S_OK;
}



/////////////////////////////////////////////////////////////////////////////
// IXXXCP (IConnectionPoint ) interface functions
//


        
///////////////////////////////////////
// IXXXCP:: GetConnectionInterface
///////////////////////////////////////
STDMETHODIMP	IXXXCP:: GetConnectionInterface( 
		IID *pIID		//out
		)
{
	*pIID = m_iid;

	return S_OK;
}

///////////////////////////////////////
// IXXXCP::GetConnectionPointContainer
///////////////////////////////////////
STDMETHODIMP	IXXXCP:: GetConnectionPointContainer( 
		IConnectionPointContainer **ppCPC	//out
		)
{
	*ppCPC = m_pCPC;
	m_pCPC->AddRef();	// Caller must do Release when he finishes

	return S_OK;
}

///////////////////////////////////////
// IXXXCP::Advise
//
// Store the Callback Pointer for later use
//
///////////////////////////////////////
STDMETHODIMP	IXXXCP:: Advise( 
		IUnknown *pUnkSink,			//in
		DWORD *pdwCookie				//out
		)
{
	HRESULT hr;

	// Basic Sanity Checks
	//
	if(!pdwCookie) return E_POINTER;
	if(!pUnkSink) return E_POINTER;

	// if already in use, return error
	//
	if(m_Callback) return CONNECT_E_ADVISELIMIT;

	// Make sure the passed i/f supports the right callback
	//
	hr = pUnkSink->QueryInterface( m_iid, (LPVOID*) &m_Callback);
	if(FAILED(hr)) return CONNECT_E_CANNOTCONNECT;	// (this is slightly oversimplified)

	// Since only one connection is supported, the cookie is always 1
	// (by MS convention 0 is illegal)
	//
	*pdwCookie = 1;

	return S_OK;
}

///////////////////////////////////////
// IXXXCP::UnAdvise
///////////////////////////////////////
STDMETHODIMP	IXXXCP:: Unadvise( 
		DWORD dwCookie		//in
		)
{
	// Since only one connection is supported, the cookie is always 1
	//
	if(dwCookie != 1) return CONNECT_E_NOCONNECTION;

	// release the client's callback
	//
	if(m_Callback) m_Callback->Release();
	m_Callback = NULL;
	
	return S_OK;
}
        

///////////////////////////////////////
// IXXXCP::EnumConnections
///////////////////////////////////////
STDMETHODIMP	IXXXCP:: EnumConnections( 
		IEnumConnections **ppEnum		//out
		)
{
	// This simple example supports only one connection of this type per group
	//
	return E_NOTIMPL;
}

///////////////////////////////////////
// IXXXCP::SetParent
///////////////////////////////////////
STDMETHODIMP	IXXXCP:: SetParent( 
		IConnectionPointContainer * parent
		)
{
	// Record Parent (CPContainer) for use later by GetConnectionPointContainer
	//
	// Note that although we are storing the CPC i/f we do NOT addref it.
	// If we did, we would end up with circular reference counts.
	// To make a long story short...
	//
	// It is basically the CALLERS (i.e. the QI) responsibility to insure that 
	// he does not release the Parent until after he has released 
	// the ConnectionPoints and that he does not release the connection points
	// until after he has done his UnAdvise.
	//
	m_pCPC = parent;

	return S_OK;
}
