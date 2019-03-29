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
#include "SecurityDescriptor.h"

/////////////////////////////////////////////////////////////////////
//  the following code is taken from the ATL's CSecurityDescriptor
//  implementation; if the sample OPC server would utilize ATL, the SID
//  would simply use the CSecurityDescriptor implementation
/////////////////////////////////////////////////////////////////////

//------------------------------------------------------------------*
/**
 * constructor
 *
 * @param           void
 * @return          void
 * @exception       -
*/
CSecurityDescriptor::CSecurityDescriptor()
  : m_pSD(NULL),
    m_pACL(NULL)
{
}

//------------------------------------------------------------------*
/**
 * destructor
 *
 * @param           void
 * @return          void
 * @exception       -
*/
CSecurityDescriptor::~CSecurityDescriptor()
{
    if (m_pSD != NULL) {
        delete m_pSD;
        m_pSD = NULL;
    }
    if (m_pACL != NULL) {
        free(m_pACL);
        m_pACL = NULL;
    }
}

//------------------------------------------------------------------*
HRESULT
    CSecurityDescriptor::Initialize()
{
    HRESULT hr = S_FALSE;
	if (m_pSD)
	{
		delete m_pSD;
		m_pSD = NULL;
	}
	if (m_pACL)
	{
		free(m_pACL);
		m_pACL = NULL;
	}

    m_pSD = new SECURITY_DESCRIPTOR;
	if (m_pSD == NULL)
		return E_OUTOFMEMORY;

	if (!InitializeSecurityDescriptor(m_pSD, SECURITY_DESCRIPTOR_REVISION))
	{
		HRESULT hr = HRESULT_FROM_WIN32(GetLastError());
		delete m_pSD;
		m_pSD = NULL;
		return hr;
	}

    // Set the ACL to allow EVERYONE
	SetSecurityDescriptorDacl(m_pSD, TRUE, NULL, FALSE);

	HANDLE  hToken;
	if(!OpenProcessToken(GetCurrentProcess(), TOKEN_QUERY, &hToken)) {
		return HRESULT_FROM_WIN32(GetLastError());
	}
    PSID    pUserSID = NULL, pGroupSID = NULL;
    hr = GetTokenSids(hToken, &pUserSID, &pGroupSID);
    CloseHandle(hToken);
    if (FAILED(hr))     return hr;

	if(!SetSecurityDescriptorGroup(m_pSD, pGroupSID, FALSE))    return HRESULT_FROM_WIN32(GetLastError());
	if(!SetSecurityDescriptorOwner(m_pSD, pUserSID, FALSE))     return HRESULT_FROM_WIN32(GetLastError());

	if(!IsValidSecurityDescriptor(m_pSD)) {
		return HRESULT_FROM_WIN32(GetLastError());
	}
    
    return S_OK;
}

//------------------------------------------------------------------*
// for correct canonical order under NT4,
//  add deny entries *BEFORE* all accept entries!
HRESULT
    CSecurityDescriptor::DenyAccess(LPCTSTR pszPrincipal, DWORD dwAccessMask)
{
	HRESULT hr = AddAccessDeniedACEToACL(&m_pACL, pszPrincipal, dwAccessMask);
	if (SUCCEEDED(hr))
		SetSecurityDescriptorDacl(m_pSD, TRUE, m_pACL, FALSE);
	return hr;
}

//------------------------------------------------------------------*
// for correct canonical order under NT4,
//  add deny entries *BEFORE* all accept entries!
HRESULT
    CSecurityDescriptor::AllowAccess(LPCTSTR pszPrincipal, DWORD dwAccessMask)
{
	HRESULT hr = AddAccessAllowedACEToACL(&m_pACL, pszPrincipal, dwAccessMask);
	if (SUCCEEDED(hr))
		SetSecurityDescriptorDacl(m_pSD, TRUE, m_pACL, FALSE);
	return hr;
}

//------------------------------------------------------------------*
HRESULT
    CSecurityDescriptor::GetPrincipalSID(LPCTSTR pszPrincipal, PSID *ppSid)
{
	HRESULT hr;
	LPTSTR pszRefDomain = NULL;
	DWORD dwDomainSize = 0;
	DWORD dwSidSize = 0;
	SID_NAME_USE snu;

	// Call to get size info for alloc
	LookupAccountName(NULL, pszPrincipal, *ppSid, &dwSidSize, pszRefDomain, &dwDomainSize, &snu);

	hr = GetLastError();
	if (hr != ERROR_INSUFFICIENT_BUFFER)
		return HRESULT_FROM_WIN32(hr);

	pszRefDomain = new TCHAR[dwDomainSize];
	if (pszRefDomain == NULL)
		return E_OUTOFMEMORY;

	*ppSid = (PSID) malloc(dwSidSize);
	if (*ppSid != NULL)
	{
		if (!LookupAccountName(NULL, pszPrincipal, *ppSid, &dwSidSize, pszRefDomain, &dwDomainSize, &snu))
		{
			free(*ppSid);
			*ppSid = NULL;
			delete[] pszRefDomain;
			return HRESULT_FROM_WIN32(GetLastError());
		}
		delete[] pszRefDomain;
		return S_OK;
	}
	delete[] pszRefDomain;
	return E_OUTOFMEMORY;
}

//------------------------------------------------------------------*
HRESULT
    CSecurityDescriptor::CopyACL(PACL pDest, PACL pSrc)
{
	ACL_SIZE_INFORMATION aclSizeInfo;
	LPVOID pAce;
	ACE_HEADER *aceHeader;

	if (pSrc == NULL)
		return S_OK;

	if (!GetAclInformation(pSrc, (LPVOID) &aclSizeInfo, sizeof(ACL_SIZE_INFORMATION), AclSizeInformation))
		return HRESULT_FROM_WIN32(GetLastError());

	// Copy all of the ACEs to the new ACL
	for (UINT i = 0; i < aclSizeInfo.AceCount; i++)
	{
		if (!GetAce(pSrc, i, &pAce))
			return HRESULT_FROM_WIN32(GetLastError());

		aceHeader = (ACE_HEADER *) pAce;

		if (!AddAce(pDest, ACL_REVISION2, 0xffffffff, pAce, aceHeader->AceSize))
			return HRESULT_FROM_WIN32(GetLastError());
	}

	return S_OK;
}

//------------------------------------------------------------------*
// for correct canonical order under NT4,
//  add deny entries *BEFORE* all accept entries!
HRESULT
    CSecurityDescriptor::AddAccessDeniedACEToACL(PACL *ppAcl, LPCTSTR pszPrincipal, DWORD dwAccessMask)
{
	ACL_SIZE_INFORMATION aclSizeInfo;
	int aclSize;
	DWORD returnValue;
	PSID principalSID;
	PACL oldACL, newACL = NULL;

	oldACL = *ppAcl;

	returnValue = GetPrincipalSID(pszPrincipal, &principalSID);
	if (FAILED(returnValue))
		return returnValue;

	aclSizeInfo.AclBytesInUse = 0;
	if (*ppAcl != NULL)
		GetAclInformation(oldACL, (LPVOID) &aclSizeInfo, sizeof(ACL_SIZE_INFORMATION), AclSizeInformation);

	aclSize = aclSizeInfo.AclBytesInUse + sizeof(ACL) + sizeof(ACCESS_DENIED_ACE) + GetLengthSid(principalSID) - sizeof(DWORD);

	newACL = (PACL) new BYTE[aclSize];
	if (newACL == NULL)
		return E_OUTOFMEMORY;

	if (!InitializeAcl(newACL, aclSize, ACL_REVISION2))
	{
		free(principalSID);
		return HRESULT_FROM_WIN32(GetLastError());
	}

	if (!AddAccessDeniedAce(newACL, ACL_REVISION2, dwAccessMask, principalSID))
	{
		free(principalSID);
		return HRESULT_FROM_WIN32(GetLastError());
	}

	returnValue = CopyACL(newACL, oldACL);
	if (FAILED(returnValue))
	{
		free(principalSID);
		return returnValue;
	}

	*ppAcl = newACL;

	if (oldACL != NULL)
		free(oldACL);
	free(principalSID);
	return S_OK;
}

//------------------------------------------------------------------*
// for correct canonical order under NT4,
//  add deny entries *BEFORE* all accept entries!
HRESULT
    CSecurityDescriptor::AddAccessAllowedACEToACL(PACL *ppAcl, LPCTSTR pszPrincipal, DWORD dwAccessMask)
{
	ACL_SIZE_INFORMATION aclSizeInfo;
	int aclSize;
	DWORD returnValue;
	PSID principalSID;
	PACL oldACL, newACL = NULL;

	oldACL = *ppAcl;

	returnValue = GetPrincipalSID(pszPrincipal, &principalSID);
	if (FAILED(returnValue))
		return returnValue;

	aclSizeInfo.AclBytesInUse = 0;
	if (*ppAcl != NULL)
		GetAclInformation(oldACL, (LPVOID) &aclSizeInfo, (DWORD) sizeof(ACL_SIZE_INFORMATION), AclSizeInformation);

	aclSize = aclSizeInfo.AclBytesInUse + sizeof(ACL) + sizeof(ACCESS_ALLOWED_ACE) + GetLengthSid(principalSID) - sizeof(DWORD);

	newACL = (PACL) new BYTE[aclSize];
	if (newACL == NULL)
		return E_OUTOFMEMORY;

	if (!InitializeAcl(newACL, aclSize, ACL_REVISION2))
	{
		free(principalSID);
		return HRESULT_FROM_WIN32(GetLastError());
	}

	returnValue = CopyACL(newACL, oldACL);
	if (FAILED(returnValue))
	{
		free(principalSID);
		return returnValue;
	}

	if (!AddAccessAllowedAce(newACL, ACL_REVISION2, dwAccessMask, principalSID))
	{
		free(principalSID);
		return HRESULT_FROM_WIN32(GetLastError());
	}

	*ppAcl = newACL;

	if (oldACL != NULL)
		free(oldACL);
	free(principalSID);
	return S_OK;
}

//------------------------------------------------------------------*
HRESULT
    CSecurityDescriptor::GetTokenSids(HANDLE hToken, PSID* ppUserSid, PSID* ppGroupSid)
{
	DWORD dwSize;
	HRESULT hr;
	PTOKEN_USER ptkUser = NULL;
	PTOKEN_PRIMARY_GROUP ptkGroup = NULL;

	if (ppUserSid)
		*ppUserSid = NULL;
	if (ppGroupSid)
		*ppGroupSid = NULL;

	if (ppUserSid)
	{
		// Get length required for TokenUser by specifying buffer length of 0
		GetTokenInformation(hToken, TokenUser, NULL, 0, &dwSize);
		hr = GetLastError();
		if (hr != ERROR_INSUFFICIENT_BUFFER)
		{
			// Expected ERROR_INSUFFICIENT_BUFFER
//			assert(FALSE);
			hr = HRESULT_FROM_WIN32(hr);
			goto failed;
		}

		ptkUser = (TOKEN_USER*) malloc(dwSize);
		if (ptkUser == NULL)
		{
			hr = E_OUTOFMEMORY;
			goto failed;
		}
		// Get Sid of process token.
		if (!GetTokenInformation(hToken, TokenUser, ptkUser, dwSize, &dwSize))
		{
			// Couldn't get user info
			hr = HRESULT_FROM_WIN32(GetLastError());
//			assert(FALSE);
			goto failed;
		}

		// Make a copy of the Sid for the return value
		dwSize = GetLengthSid(ptkUser->User.Sid);

		PSID pSid;
		pSid = (PSID) malloc(dwSize);
		if (pSid == NULL)
		{
			hr = E_OUTOFMEMORY;
			goto failed;
		}
		if (!CopySid(dwSize, pSid, ptkUser->User.Sid))
		{
			hr = HRESULT_FROM_WIN32(GetLastError());
//			assert(FALSE);
			goto failed;
		}

//		assert(IsValidSid(pSid));
		*ppUserSid = pSid;
		free(ptkUser);
	}
	if (ppGroupSid)
	{
		// Get length required for TokenPrimaryGroup by specifying buffer length of 0
		GetTokenInformation(hToken, TokenPrimaryGroup, NULL, 0, &dwSize);
		hr = GetLastError();
		if (hr != ERROR_INSUFFICIENT_BUFFER)
		{
			// Expected ERROR_INSUFFICIENT_BUFFER
//			assert(FALSE);
			hr = HRESULT_FROM_WIN32(hr);
			goto failed;
		}

		ptkGroup = (TOKEN_PRIMARY_GROUP*) malloc(dwSize);
		if (ptkGroup == NULL)
		{
			hr = E_OUTOFMEMORY;
			goto failed;
		}
		// Get Sid of process token.
		if (!GetTokenInformation(hToken, TokenPrimaryGroup, ptkGroup, dwSize, &dwSize))
		{
			// Couldn't get user info
			hr = HRESULT_FROM_WIN32(GetLastError());
//			assert(FALSE);
			goto failed;
		}

		// Make a copy of the Sid for the return value
		dwSize = GetLengthSid(ptkGroup->PrimaryGroup);

		PSID pSid;
		pSid = (PSID) malloc(dwSize);
		if (pSid == NULL)
		{
			hr = E_OUTOFMEMORY;
			goto failed;
		}
		if (!CopySid(dwSize, pSid, ptkGroup->PrimaryGroup))
		{
			hr = HRESULT_FROM_WIN32(GetLastError());
//			assert(FALSE);
			goto failed;
		}

//		assert(IsValidSid(pSid));

		*ppGroupSid = pSid;
		free(ptkGroup);
	}

	return S_OK;

failed:
	if (ptkUser)
		free(ptkUser);
	if (ptkGroup)
		free (ptkGroup);
	return hr;
}


