// PrivateSecurity.cpp
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
//  Michael Ryan, Intellution, Inc.
//
// CONTENTS:
//
//  implements of private security 
//
//
// Modification Log:
//  Vers    Date   By    Notes
//  ----  -------- ---   -----
//  1.00  09/16/99 mfr   Spec prototype
//  1.00  08/03/00 dj    spec release
//
//

#define WIN32_LEAN_AND_MEAN

#include "OPCXXX.h"
#include "PrivateSecurity.h"



typedef struct securityitem
{
	char user[32];
	char pass[32];
	ULONG	access;
}	SECURITYITEM;

#define NUM_USER 3

SECURITYITEM sec_list[] =
{
	"USER1", "opc1",   SEC_ACCESS_READ,
	"USER2", "opc2", SEC_ACCESS_WRITE,
	"USER3", "opc3", SEC_ACCESS_WRITE,
	"","",0xffff
};



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
CPrivateSecurity::CPrivateSecurity()
{
	// initialize security information
	m_SecurityEnable = TRUE;		  			// flag to indicate if security is enabled
	m_SecurityUserLoggedIn = FALSE;			   	// flag to indicate if a user is logged in
	m_SecurityUserAccess = SEC_ACCESS_READ;	// the access level of the user logged in
	m_SecurityDefaultAccess = SEC_ACCESS_READ;	// the default level of security if no one is logged in
}

//------------------------------------------------------------------*
/**
 * destructor
 *
 * @param           void
 * @return          void
 * @exception       -
*/
CPrivateSecurity::~CPrivateSecurity()
{
}


//------------------------------------------------------------------*
/**
 * GetAccessLevel
 *
 * @param           void
 * @return          ULONG
 * @exception       -
*/
ULONG CPrivateSecurity::GetAccessLevel(void) 
{
	// if security not configured use MAX_SECURITY_LEVEL
	if ( !GetSecurityEnable() )			
	{
 		return SEC_ACCESS_WRITE;
 	}

	// if no user logged in use default access
	if ( !m_SecurityUserLoggedIn )	
	{
		return m_SecurityDefaultAccess;
	}

	// return the level of logged in user
	return GetUserSecurity(); 
}
 

BOOL  CPrivateSecurity::LogOnUser ( LPCWSTR user, LPCWSTR pword) 
{
    TCHAR   UserName[100];
    TCHAR   PassWord[100];
	int i;

    if (user)
    {
#ifndef _UNICODE
	    WideCharToMultiByte(CP_ACP, 0, user, -1, UserName, sizeof(UserName), NULL, NULL);
#else
        _tcscpy(UserName, user);
#endif
    }
	else
	{
		return FALSE;	// we require a username
	}

    if (pword)
    {
#ifndef _UNICODE
	    WideCharToMultiByte(CP_ACP, 0, pword, -1, PassWord, sizeof(PassWord), NULL, NULL);
#else
        _tcscpy(Password, pword);
#endif
    }
	else
	{
		PassWord[0]='\0';
	}


	for ( i = 0; i < NUM_USER; i++ )
	{

		if ( strcmp ( sec_list[i].user, UserName ) == 0 )
		{
			if ( strcmp ( sec_list[i].pass, PassWord ) == 0 )
			{
				m_SecurityUserLoggedIn = TRUE; 
				m_SecurityUserAccess = sec_list[i].access;
				return TRUE;
			}
		}
	}

	return FALSE;
}

void  CPrivateSecurity::LogOffUser () 
{
	m_SecurityUserLoggedIn = FALSE;
}


HRESULT CPrivateSecurity::IsAccessOK(DWORD dwRequestedAccess, BOOL &bAccessOK)
{
	ULONG ulevel;

	if ( dwRequestedAccess == SEC_ACCESS_READ ) // everyone can read
	{
	    bAccessOK = TRUE;
	}
	else
	{
		ulevel = GetAccessLevel(); 
		if ( ulevel == SEC_ACCESS_WRITE )
		{
		    bAccessOK = TRUE;
		}
		else
		{
		    bAccessOK = FALSE;
		}
	}

	return S_OK;
}
