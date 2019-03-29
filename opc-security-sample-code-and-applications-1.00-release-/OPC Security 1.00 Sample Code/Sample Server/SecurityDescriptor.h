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

#if !defined(AFX_SECURITYDESCRIPTOR_H__0CD42551_2180_11D3_84B6_000000000000__INCLUDED_)
#define AFX_SECURITYDESCRIPTOR_H__0CD42551_2180_11D3_84B6_000000000000__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#define SEC_ACCESS_READ  1
#define SEC_ACCESS_WRITE 2


///////////////////////////////////////////////////////////////////////////////
//  the following code is taken from the ATL's CSecurityDescriptor
//  implementation; if the sample OPC server would utilize ATL, the SID
//  would simply use the ATL implementation
class CSecurityDescriptor
{
public:
                            CSecurityDescriptor();
    virtual                 ~CSecurityDescriptor();

private:    // provoke link error if unwanted copy ctor or assignment is compiled (have no deep copy!)
                            CSecurityDescriptor(CSecurityDescriptor const & other);
    CSecurityDescriptor const & operator = (CSecurityDescriptor const & other);

// Access
public:
    PSECURITY_DESCRIPTOR    GetSD(void)
                            {
                                return m_pSD;
                            }

// Operations
public:
    HRESULT                 Initialize();
    HRESULT                 DenyAccess(LPCTSTR pszPrincipal, DWORD dwAccessMask);
    HRESULT                 AllowAccess(LPCTSTR pszPrincipal, DWORD dwAccessMask);

// Helpers
private:
    HRESULT                 GetPrincipalSID(LPCTSTR pszPrincipal, PSID *ppSid);
    HRESULT                 CopyACL(PACL pDest, PACL pSrc);
    HRESULT                 AddAccessDeniedACEToACL(PACL *ppAcl, LPCTSTR pszPrincipal, DWORD dwAccessMask);
    HRESULT                 AddAccessAllowedACEToACL(PACL *ppAcl, LPCTSTR pszPrincipal, DWORD dwAccessMask);
    HRESULT                 GetTokenSids(HANDLE hToken, PSID* ppUserSid, PSID* ppGroupSid);

// Attributes:
protected:
private:
    PACL                    m_pACL;
    PSECURITY_DESCRIPTOR    m_pSD;
};


#endif // !defined(AFX_SECURITYDESCRIPTOR_H__0CD42551_2180_11D3_84B6_000000000000__INCLUDED_)
