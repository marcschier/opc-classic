// AccessCheck.cpp
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
//  implements check against NT access token and also caches
//  last user identification and access rights
//
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  0.91  05/25/99 dj    spec prototype
//  1.00  08/03/00 dj    spec release
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "AccessCheck.h"


#define MAX_USER_NAME   500


//////////////////////////////////////////////////////////////////////
// Construction/Destruction
//////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------*
/**
 * constructor
 *
 * @param           void
 * @return          void
 * @exception       -
*/
CAccessCheck::CAccessCheck()
{
    m_hCachedToken = NULL;
    Cleanup();
}

//------------------------------------------------------------------*
/**
 * destructor
 *
 * @param           void
 * @return          void
 * @exception       -
*/
CAccessCheck::~CAccessCheck()
{
    Cleanup();
}

//------------------------------------------------------------------*
/**
 * Initialize
 *
 * @param           void
 * @return          S_OK if ok; HRESULT on error
 * @exception       -
*/
HRESULT
    CAccessCheck::Initialize(void)
{
    HRESULT hr;
    hr = m_sdAccess.Initialize();
    m_bInitialUserSet = FALSE;

    return hr;
}

//------------------------------------------------------------------*
/**
 * IsAccessOK  check if access for cached thread token is ok
 *
 * @param           dwRequestedAccess       requested access (READ and/or write)
 * @param           bAccessOK       OUT: TRUE if access granted
 * @return          S_OK if ok; HRESULT on error
 * @exception       -
*/
HRESULT
    CAccessCheck::IsAccessOK(DWORD dwRequestedAccess, OUT BOOL &bAccessOK)
{
#ifdef _DEBUG
    OutputDebugString(_T("CAccessCheck::IsAccessOK()"));
    DebugShowClientBlanket();
#endif

    HRESULT hr = S_OK;
    // to have a smooth migration with client OPC clients (non-security aware),
    // implicitly call change user to cache access rights of current calling user
    bAccessOK = FALSE;
    if (!m_bInitialUserSet) {
        hr = ChangeUser();
        if (FAILED(hr)) return hr;
    }

    GENERIC_MAPPING GenericMapping;
    DWORD           dwAccessDesired = dwRequestedAccess;
    GenericMapping.GenericRead = SEC_ACCESS_READ;
    GenericMapping.GenericWrite = SEC_ACCESS_WRITE;
    GenericMapping.GenericExecute = 0;
    GenericMapping.GenericAll = SEC_ACCESS_READ | SEC_ACCESS_WRITE;
    ::MapGenericMask(&dwAccessDesired, &GenericMapping);

#ifndef USE_CACHED_THREAD_TOKEN
    // lookup in cached access results
    bAccessOK = ((m_dwAccessGranted & dwAccessDesired) != 0);
#else   // USE_CACHED_THREAD_TOKEN
	PRIVILEGE_SET   PrivilegeSet;
    DWORD           lenPrivSet = sizeof(PRIVILEGE_SET);
    // determine access rights by checking cached thread token!!
    if (!::AccessCheck(m_sdAccess.GetSD(), m_hCachedToken, dwAccessDesired,
                     &GenericMapping, &PrivilegeSet, &lenPrivSet, &m_dwAccessGranted, &bAccessOK))
	{
		return HRESULT_FROM_WIN32(GetLastError());
	}
#endif 
    return S_OK;
}

//------------------------------------------------------------------*
/**
 * ChangeUser   reread client blanket and update cache of calling user
 *              check if cached access approves
 *
 * @return          S_OK if ok; HRESULT on error
 * @exception       -
*/
HRESULT
    CAccessCheck::ChangeUser()
{
	HRESULT hr = E_FAIL;	
	//impersonate the calling client
    hr = ::CoImpersonateClient();
	if (FAILED(hr)) return hr;

#ifdef _DEBUG
    OutputDebugString(_T("CAccessCheck::ChangeUser()"));
    DebugShowClientBlanket();
#endif

    // rebuild cache for new user with both R/W access requests:

    DWORD   dwAuthLevel = 0;
    hr = CoQueryClientBlanket(NULL, NULL, NULL, &dwAuthLevel, NULL, NULL, NULL);
    if (FAILED(hr)) return hr;
    if (   (dwAuthLevel != RPC_C_AUTHN_LEVEL_DEFAULT)
        && (dwAuthLevel <= RPC_C_AUTHN_LEVEL_CONNECT))
    {
//        return E_ACCESSDENIED;
    }
    if(!::IsValidSecurityDescriptor(m_sdAccess.GetSD()))	{
		return E_FAIL;  // IsValidSecurityDescriptor does not set global error code!
	}

	//retrieve the token handler for this thread
	HANDLE  hToken;
    if(!::OpenThreadToken(GetCurrentThread(),
                          TOKEN_QUERY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE,
                          TRUE, &hToken)) {
        ::CoRevertToSelf();
		return HRESULT_FROM_WIN32(GetLastError());
	}

    if (!::DuplicateToken(hToken, SecurityImpersonation, &m_hCachedToken)) {
        ::CoRevertToSelf();
		return HRESULT_FROM_WIN32(GetLastError());
    }

	PRIVILEGE_SET   PrivilegeSet;
    DWORD           lenPrivSet;
    GENERIC_MAPPING GenericMapping;
    BOOL            bAccessOK;
    lenPrivSet = sizeof(PRIVILEGE_SET);

    // check thread token against ACL in SD and cache result
    GenericMapping.GenericRead = SEC_ACCESS_READ;
    GenericMapping.GenericWrite = SEC_ACCESS_WRITE;
    GenericMapping.GenericExecute = 0;
    GenericMapping.GenericAll = SEC_ACCESS_READ | SEC_ACCESS_WRITE;
    
    // simply update m_dwAccessGranted flags, don't care if access is ok
    if (!::AccessCheck(m_sdAccess.GetSD(), m_hCachedToken, MAXIMUM_ALLOWED,
                     &GenericMapping, &PrivilegeSet, &lenPrivSet, &m_dwAccessGranted, &bAccessOK))
	{
        ::CoRevertToSelf();
		return HRESULT_FROM_WIN32(GetLastError());
	}

	//return to the server's identity
    ::CoRevertToSelf();

    m_bInitialUserSet = TRUE;
	return S_OK;
}

//------------------------------------------------------------------*
/**
 * DenyAccess       deny access to secured object for this user
 *          for correct canonical order under NT4,
 *          add deny entries *BEFORE* all accept entries!
 *
 * @param           pszUser         string with domain and user name
 * @param           dwAccessMask    denied access to object
 * @return          S_OK if ok; HRESULT on error
 * @exception       -
*/
HRESULT
    CAccessCheck::DenyAccess(LPCTSTR pszUser, DWORD dwAccessMask)
{
    return m_sdAccess.DenyAccess(pszUser, dwAccessMask);
}

//------------------------------------------------------------------*
/**
 * AllowAccess       allow access to secured object for this user
 *          for correct canonical order under NT4,
 *          add deny entries *BEFORE* all accept entries!
 *
 * @param           pszUser         string with domain and user name
 * @param           dwAccessMask    accepted access to object
 * @return          S_OK if ok; HRESULT on error
 * @exception       -
*/
HRESULT
    CAccessCheck::AllowAccess(LPCTSTR pszUser, DWORD dwAccessMask)
{
    return m_sdAccess.AllowAccess(pszUser, dwAccessMask);
}

//------------------------------------------------------------------*
/**
 * Cleanup  cleanup any cached info and free all resources
 *
 * @param           void
 * @return          void
 * @exception       -
*/
void
    CAccessCheck::Cleanup()
{
    m_bInitialUserSet = FALSE;
    m_dwAccessGranted = 0;
    if (m_hCachedToken != NULL) {
        ::CloseHandle(m_hCachedToken);
        m_hCachedToken = NULL;
    }
}


HRESULT
    CAccessCheck::DebugShowClientBlanket()
{
    // who is calling anyhow?
    HRESULT hr;
    DWORD   authnSvc = 0;               // current authentication service
    DWORD   authzSvc = 0;               // current authorization service
    OLECHAR *pServerPrincName = NULL;   // current principal name
    DWORD   authnLevel = 0;             // current authentication level
    RPC_AUTHZ_HANDLE privs = 0;         // handle to privilege information
    DWORD   capabilities = 0;           // flags indicating further capabilities of the proxy

    hr = CoQueryClientBlanket(&authnSvc, &authzSvc, &pServerPrincName, &authnLevel, NULL, &privs, &capabilities);
    if (FAILED(hr)) return hr;

    TCHAR   szLine[1000];

    TCHAR   szPrincipal[100];
    if (pServerPrincName)
    {
#ifndef _UNICODE
	    WideCharToMultiByte(CP_ACP, 0, pServerPrincName, -1, szPrincipal, sizeof(szPrincipal), NULL, NULL);
#else
        _tcscpy(szPrincipal, pServerPrincName);
#endif
    }
    else {
        _tcscpy(szPrincipal, _T("-None-"));
    }

    wsprintf(szLine, _T("Caller was '%s', level is %s, authenticated using %s, %s\n"),
        szPrincipal, GetAuthenticationLevelName(authnLevel),
        GetAuthenticationSvcName(authnSvc), GetAuthorizationSvcName(authzSvc));
    OutputDebugString(szLine);

    if (pServerPrincName) CoTaskMemFree(pServerPrincName);
    return S_OK;
}

// trace helpers::
LPCTSTR
    CAccessCheck::GetAuthenticationSvcName(DWORD authnSvc)
{
    LPCTSTR pszName = NULL;

    // under NT4.0, only RPC_C_AUTHN_WINNT is realized, but more to come for NT5/W2K
    switch (authnSvc) {
    case RPC_C_AUTHN_NONE:          pszName = _T("RPC_C_AUTHN_NONE"); break;
    case RPC_C_AUTHN_DCE_PRIVATE:   pszName = _T("RPC_C_AUTHN_DCE_PRIVATE"); break;
    case RPC_C_AUTHN_DCE_PUBLIC:    pszName = _T("RPC_C_AUTHN_DCE_PUBLIC"); break;
    case RPC_C_AUTHN_DEC_PUBLIC:    pszName = _T("RPC_C_AUTHN_DEC_PUBLIC"); break;
//    case RPC_C_AUTHN_GSS_NEGOTIATE: pszName = _T("RPC_C_AUTHN_GSS_NEGOTIATE"); break;
    case RPC_C_AUTHN_WINNT:         pszName = _T("RPC_C_AUTHN_WINNT"); break;
//    case RPC_C_AUTHN_GSS_SCHANNEL:  pszName = _T("RPC_C_AUTHN_GSS_SCHANNEL"); break;
    case RPC_C_AUTHN_DPA:           pszName = _T("RPC_C_AUTHN_DPA"); break;
    case RPC_C_AUTHN_MSN:           pszName = _T("RPC_C_AUTHN_MSN"); break;
    case RPC_C_AUTHN_GSS_KERBEROS:  pszName = _T("RPC_C_AUTHN_GSS_KERBEROS"); break;
    case RPC_C_AUTHN_MQ:            pszName = _T("RPC_C_AUTHN_MQ"); break;
    case RPC_C_AUTHN_DEFAULT:       pszName = _T("RPC_C_AUTHN_DEFAULT"); break;
    default:                        pszName = _T("<<Unknown RPC_C_AUTHN_XXX>>"); break;
    }

    return pszName;
}

LPCTSTR
    CAccessCheck::GetAuthorizationSvcName(DWORD authzSvc)
{
    LPCTSTR pszName = NULL;

    switch (authzSvc) {
    case RPC_C_AUTHZ_NONE:          pszName = _T("RPC_C_AUTHZ_NONE"); break;
    case RPC_C_AUTHZ_NAME:          pszName = _T("RPC_C_AUTHZ_NAME"); break;
    case RPC_C_AUTHZ_DCE:           pszName = _T("RPC_C_AUTHZ_DCE"); break;
//    case RPC_C_AUTHZ_DEFAULT:       pszName = _T("RPC_C_AUTHZ_DEFAULT"); break;
    default:                        pszName = _T("<<Unknown RPC_C_AUTHZ_XXX>>"); break;
    }

    return pszName;
}

LPCTSTR
    CAccessCheck::GetAuthenticationLevelName(DWORD authnLevel)
{
    LPCTSTR pszName = NULL;

    switch (authnLevel) {
    case RPC_C_AUTHN_LEVEL_DEFAULT:         pszName = _T("RPC_C_AUTHN_LEVEL_DEFAULT"); break;
    case RPC_C_AUTHN_LEVEL_NONE:            pszName = _T("RPC_C_AUTHN_LEVEL_NONE"); break;
    case RPC_C_AUTHN_LEVEL_CONNECT:         pszName = _T("RPC_C_AUTHN_LEVEL_CONNECT"); break;
    case RPC_C_AUTHN_LEVEL_CALL:            pszName = _T("RPC_C_AUTHN_LEVEL_CALL"); break;
    case RPC_C_AUTHN_LEVEL_PKT:             pszName = _T("RPC_C_AUTHN_LEVEL_PKT"); break;
    case RPC_C_AUTHN_LEVEL_PKT_INTEGRITY:   pszName = _T("RPC_C_AUTHN_LEVEL_PKT_INTEGRITY"); break;
    case RPC_C_AUTHN_LEVEL_PKT_PRIVACY:     pszName = _T("RPC_C_AUTHN_LEVEL_PKT_PRIVACY"); break;
    default:                        pszName = _T("<<Unknown RPC_C_AUTHN_LEVEL_XXX>>"); break;
    }

    return pszName;
}



