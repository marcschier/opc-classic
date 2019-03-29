// AccessCheck.h
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
//	Vers    Date   By    Notes
//	----  -------- ---   -----
//  0.91  05/25/99 dj    spec prototype
//  1.00  08/03/00 dj    spec release
//
//

#if !defined(AFX_ACCESSCHECK_H__6BAADA26_1DFF_11D3_84A9_00008630A38B__INCLUDED_)
#define AFX_ACCESSCHECK_H__6BAADA26_1DFF_11D3_84A9_00008630A38B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "SecurityDescriptor.h"


///////////////////////////////////////////////////////////////////////////////
class CAccessCheck  
{
public:
                            CAccessCheck();
    virtual                 ~CAccessCheck();

private:    // provoke link error if unwanted copy ctor or assignment is compiled (have no deep copy!)
                            CAccessCheck(CAccessCheck const & other);
    CAccessCheck const &    operator = (CAccessCheck const & other);

// Access
public:

// Operations
public:
    HRESULT                 Initialize(void);
    HRESULT                 IsAccessOK(DWORD dwRequestedAccess, OUT BOOL &bAccessOK);
    HRESULT                 ChangeUser();
    HRESULT                 DenyAccess(LPCTSTR pszPrincipal, DWORD dwAccessMask);
    HRESULT                 AllowAccess(LPCTSTR pszPrincipal, DWORD dwAccessMask);
    HANDLE                  GetCachedThreadHandle()
                            {
                                return m_hCachedToken;
                            }

// Helpers
private:
    void                    Cleanup(void);
    static HRESULT          DebugShowClientBlanket();
    static LPCTSTR          GetAuthenticationLevelName(DWORD authnLevel);
    static LPCTSTR          GetAuthorizationSvcName(DWORD authzSvc);
    static LPCTSTR          GetAuthenticationSvcName(DWORD authnSvc);

// Attributes:
protected:
private:
    CSecurityDescriptor     m_sdAccess;     // accept/deny SD for secured OPC object
    BOOL                    m_bInitialUserSet;  
    DWORD                   m_dwAccessGranted;
    HANDLE                  m_hCachedToken;     // cached thread token
};

#endif // !defined(AFX_ACCESSCHECK_H__6BAADA26_1DFF_11D3_84A9_00008630A38B__INCLUDED_)
