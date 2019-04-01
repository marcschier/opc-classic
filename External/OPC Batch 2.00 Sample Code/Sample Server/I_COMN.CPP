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
//        07/27/00 acc/md  fix SetLocaleID, QueryAvailableLocaleIDs, GetErrorString
//                       to pass compliance test
//        08/11/00 ACC/MD implement GetOPCError()
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "OPCERROR.h"

extern	HRESULT GetOPCError( HRESULT hr, LPWSTR *ppString); //opc_err.cpp

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

	if (dwLcid == MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), SORT_DEFAULT))
	{
		s.m_LCID = dwLcid;
		return S_OK;
	}
	if ((dwLcid == 0) || (dwLcid == LOCALE_SYSTEM_DEFAULT))
	{
		s.m_LCID = LOCALE_SYSTEM_DEFAULT;
		return S_OK;
	}
	return E_INVALIDARG;

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
	
	// Return english
	//
	* pdwCount = 0;
	* pdwLcid = NULL;

	* pdwLcid = (DWORD *) CoTaskMemAlloc (sizeof (DWORD) * 1);
	if (* pdwLcid == NULL)
		return E_OUTOFMEMORY;

	(* pdwLcid) [0] = MAKELCID(MAKELANGID(LANG_ENGLISH, SUBLANG_ENGLISH_US), SORT_DEFAULT);
	* pdwCount = 1;

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
	// validate the pointer
	//
	if (ppString == NULL)
		return E_INVALIDARG;

	// Then get the string from a common function (shared with i_comn.cpp)
	//
	return GetOPCError( dwError, ppString);
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


