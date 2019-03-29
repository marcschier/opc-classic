// I_COMN.cpp
//
// (c) Copyright 1999,2000 The OPC Foundation
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
//  This code was provided to the OPC Foundation by
//  David JENNI, Softing GmbH 4CONTROL
//
// CONTENTS:
//
//  This file contains an implementation of IOPCSecurity
//  for the XXX server.
//
//
// Modification Log:
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  0.91  11/05/99 dj    spec prototype
//  1.00  08/03/00 dj    spec release
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"


/////////////////////////////////////////////////////////////////////////////
// Constructor /Destructor functions
//

///////////////////////////////////////
// IXXXSecurityNT()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXSecurityNT::IXXXSecurityNT(LPUNKNOWN parent)
{
	m_Parent = (XXXServer *)parent;
}



///////////////////////////////////////
// ~IXXXSecurityNT()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXSecurityNT::~IXXXSecurityNT(void)
{
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXSecurityNT::AddRef(void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXSecurityNT::Release(void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXSecurityNT::QueryInterface(REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXSecurityNT interface functions
//
// the IOPCSecurityNT Functions

STDMETHODIMP IXXXSecurityNT::IsAvailableNT(BOOL *pbAvailable)
{
    if (pbAvailable == NULL) {
        return E_POINTER;
    }
    *pbAvailable = TRUE;
    return S_OK;
}


STDMETHODIMP IXXXSecurityNT::QueryMinImpersonationLevel(DWORD *pdwMinImpLevel)
{
    if (pdwMinImpLevel == NULL) {
        return E_INVALIDARG;
    }
    *pdwMinImpLevel = RPC_C_IMP_LEVEL_IMPERSONATE;
    return S_OK;
}


STDMETHODIMP IXXXSecurityNT::ChangeUser(void)
{
    // this prototype implements NT credential only
    if (m_Parent == NULL) {
        return E_FAIL;
    }
    return m_Parent->ChangeUser();
}







///////////////////////////////////////
// IXXXSecurityPrivate()
//   Constructor for this Interface
//
///////////////////////////////////////
IXXXSecurityPrivate::IXXXSecurityPrivate(LPUNKNOWN parent)
{
	m_Parent = (XXXServer *)parent;
}


///////////////////////////////////////
// ~IXXXSecurityPrivate()
//   Destructor for this Interface
//
///////////////////////////////////////
IXXXSecurityPrivate::~IXXXSecurityPrivate(void)
{
}


/////////////////////////////////////////////////////////////////////////////
// IUnknown functions Delegate to Parent
//

STDMETHODIMP_(ULONG) IXXXSecurityPrivate::AddRef(void)
{
	return m_Parent->AddRef();
}

STDMETHODIMP_(ULONG) IXXXSecurityPrivate::Release(void)
{
	return m_Parent->Release();
}

STDMETHODIMP IXXXSecurityPrivate::QueryInterface(REFIID iid, LPVOID* ppInterface)
{
	return m_Parent->QueryInterface(iid, ppInterface);
}



/////////////////////////////////////////////////////////////////////////////
// IXXXSecurityPrivate IOPCCommon interface functions
//
// the IOPCSecurity Functions

STDMETHODIMP IXXXSecurityPrivate::IsAvailablePriv(BOOL *pbAvailable)
{
    if (pbAvailable == NULL) {
        return E_POINTER;
    }
    *pbAvailable = m_Parent->SecPrivIsAvailable ();
    return S_OK;
}

STDMETHODIMP IXXXSecurityPrivate::Logon(LPCWSTR szUserID, LPCWSTR szPassword)
{
    return m_Parent->SecPrivLogon (szUserID, szPassword);
}

STDMETHODIMP IXXXSecurityPrivate::Logoff(void)
{
    return m_Parent->SecPrivLogoff ();
}


