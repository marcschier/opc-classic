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
//  1.00  09/16/99 mfr    Spec prototype
//
//

#if !defined(AFX_PRIVSEC_H__6BAADA26_1DFF_11D3_84A9_00008630A38B__INCLUDED_)
#define AFX_PRIVSEC_H__6BAADA26_1DFF_11D3_84A9_00008630A38B__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include "securitydescriptor.h"

///////////////////////////////////////////////////////////////////////////////
class CPrivateSecurity
{
public:
       							 CPrivateSecurity();
    virtual                 ~CPrivateSecurity();

public:

  ULONG GetUserSecurity(void) 
  {
		return m_SecurityUserAccess;
  }

  void SetUserSecurity(ULONG sec) 
  {
		m_SecurityUserAccess = sec;
		return;
  }

  BOOL GetSecurityEnable(void) 
  {
  		return m_SecurityEnable;
  }

  void SetSecurityEnable(BOOL sec) 
  {
		m_SecurityEnable = sec;
		return;
  }

  ULONG GetAccessLevel(void); 
  BOOL  LogOnUser ( LPCWSTR user, LPCWSTR pword); 
  void  LogOffUser ( void ); 
  HRESULT IsAccessOK(DWORD dwRequestedAccess, BOOL &bAccessOK);

private:  
	BOOL			m_SecurityEnable;			// flag to indicate if security is enabled
	BOOL			m_SecurityUserLoggedIn;	// flag to indicate if a user is logged in
	ULONG			m_SecurityUserAccess;		// the access level of the user logged in
	ULONG			m_SecurityDefaultAccess;	// the default level of security if no one is logged in

};


#endif 
