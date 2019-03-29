// I_COMN.cpp
//
// (c) Copyright 1998 The OPC Foundation
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
//  Al Chisholm, Intellution Inc.
//
// CONTENTS:
//
//  This file contains an implementation of IOPCCommon
//  for the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//	2.00  01/16/98 ACC
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXCOMN()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXCOMN::IXXXCOMN( LPUNKNOWN parent )
{
	m_Parent = (XXXServer *)parent;
}



///////////////////////////////////////
// ~IXXXCOMN()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXCOMN::~IXXXCOMN( void)
{
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXCOMN::AddRef( void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXCOMN::Release( void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXCOMN::QueryInterface( REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXCOMN IOPCCommon interface functions
//


///////////////////////////////////////
// IXXXCOMN::SetLocaleID
//
///////////////////////////////////////
STDMETHODIMP IXXXCOMN::SetLocaleID( 
            LCID dwLcid	)
{
	XXXServer &s = *m_Parent;
	// Set server default LCID
	//
	s.m_LCID = dwLcid;
	return S_OK;
}
        
///////////////////////////////////////
// IXXXCOMN::GetLocaleID
//
///////////////////////////////////////
STDMETHODIMP IXXXCOMN::GetLocaleID( 
            LCID *pdwLcid )
{
	XXXServer &s = *m_Parent;
	// Get server default LCID
	//
	*pdwLcid = s.m_LCID;
	return S_OK;
}
        
///////////////////////////////////////
// IXXXCOMN::QueryAvailableLocaleIDs
//
///////////////////////////////////////
STDMETHODIMP IXXXCOMN::QueryAvailableLocaleIDs( 
            DWORD  *pdwCount,
            LCID  **pdwLcid )
{
	XXXServer &s = *m_Parent;
	// This sample code does not currently return any LCIDs
	//
	*pdwCount = 0;
	*pdwLcid = NULL;
	return S_OK;
}
        
///////////////////////////////////////
// IXXXCOMN::GetErrorString
//
///////////////////////////////////////
STDMETHODIMP IXXXCOMN::GetErrorString( 
            HRESULT dwError,
            LPWSTR  *ppString )
{
	static WCHAR * test = L"Test Error String...";
	XXXServer &s = *m_Parent;

	// A real server would obtain the string from a resource file
	//
	*ppString = WSTRClone( test, pIMalloc);
	return S_OK;
}
        
///////////////////////////////////////
// IXXXCOMN::SetClientName
//
///////////////////////////////////////
STDMETHODIMP IXXXCOMN::SetClientName( 
            LPCWSTR szName )
{
	XXXServer &s = *m_Parent;
	// The string could be stored in a member variable in the Server
	// for diagnostic purposes. (This sample does not do anything with it)
	//
	return S_OK;
}


