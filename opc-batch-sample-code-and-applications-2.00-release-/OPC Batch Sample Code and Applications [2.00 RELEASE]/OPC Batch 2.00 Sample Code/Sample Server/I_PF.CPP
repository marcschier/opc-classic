// I_PF.cpp
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
//  This file contains a skeleton of 
//  the IPersistFile interface for the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	0.00  11/18/96 ACC
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXPF()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXPF::IXXXPF( LPUNKNOWN parent )
{
	m_Parent = (XXXServer *)parent;
}



///////////////////////////////////////
// ~IXXXPF()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXPF::~IXXXPF( void)
{
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXPF::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXPF::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXPF::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXPF (IPersisFile) interface functions
//


///////////////////////////////////////
// IXXXPF::GetClassID()
///////////////////////////////////////
STDMETHODIMP	IXXXPF::GetClassID( 
		CLSID *pClassID
		)
{
	*pClassID = CLSID_OPCBatchSampleServer;
	return S_OK;
}


///////////////////////////////////////
// IXXXPF::
///////////////////////////////////////
STDMETHODIMP	IXXXPF::IsDirty( void)
{
	return S_OK;
}

        
///////////////////////////////////////
// IXXXPF::
///////////////////////////////////////
STDMETHODIMP	IXXXPF::Load(
		LPCOLESTR pszFileName,
		DWORD dwMode
		)
{
	return S_OK;
}

        
///////////////////////////////////////
// IXXXPF::
///////////////////////////////////////
STDMETHODIMP	IXXXPF::Save( 
		LPCOLESTR pszFileName,
		BOOL fRemember
		)
{
	return S_OK;
}

        
///////////////////////////////////////
// IXXXPF::
///////////////////////////////////////
STDMETHODIMP	IXXXPF::SaveCompleted( 
		LPCOLESTR pszFileName
		)
{
	return S_OK;
}

        
///////////////////////////////////////
// IXXXPF::
///////////////////////////////////////
STDMETHODIMP	IXXXPF::GetCurFile( 
		LPOLESTR *ppszFileName
		)
{
	return S_OK;
}


